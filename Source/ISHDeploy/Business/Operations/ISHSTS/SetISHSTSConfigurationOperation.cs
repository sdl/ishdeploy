/*
 * Copyright (c) 2014 All Rights Reserved by the SDL Group.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using ISHDeploy.Business.Invokers;
using ISHDeploy.Business.Operations.ISHComponent;
using ISHDeploy.Common;
using ISHDeploy.Common.Enums;
using ISHDeploy.Data.Actions.Certificate;
using ISHDeploy.Data.Actions.DataBase;
using ISHDeploy.Data.Actions.File;
using ISHDeploy.Data.Actions.WebAdministration;
using ISHDeploy.Data.Actions.XmlFile;
using ISHDeploy.Common.Interfaces;
using ISHDeploy.Data.Managers.Interfaces;
using Microsoft.Web.Administration;
using Models = ISHDeploy.Common.Models;

namespace ISHDeploy.Business.Operations.ISHSTS
{
    /// <summary>
    /// Sets STS token signing certificate and/or changes type of authentication.
    /// </summary>
    /// <seealso cref="IOperation" />
    public class SetISHSTSConfigurationOperation : BaseOperationPaths, IOperation
    {
        /// <summary>
        /// The actions invoker
        /// </summary>
        public IActionInvoker Invoker { get; }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="ishDeployment">The instance of the deployment.</param>
        /// <param name="thumbprint">The Token signing certificate Thumbprint.</param>
        /// <param name="authenticationType">The authentication type.</param>
        public SetISHSTSConfigurationOperation(ILogger logger, Models.ISHDeployment ishDeployment, string thumbprint, AuthenticationType authenticationType) :
            base(logger, ishDeployment)
        {
            CheckPermissions();

            Invoker = new ActionInvoker(logger, "Setting of STS token signing certificate and type of authentication");

            AddActionsToStopSTSApplicationPool(ishDeployment);
            AddActionsToSetTokenSigningCertificate(thumbprint);
            AddActionsToSetAuthenticationType(ishDeployment, authenticationType);
            AddActionsToStartSTSApplicationPool(ishDeployment);
        }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="ishDeployment">The instance of the deployment.</param>
        /// <param name="thumbprint">The Token signing certificate Thumbprint.</param>
        public SetISHSTSConfigurationOperation(ILogger logger, Models.ISHDeployment ishDeployment, string thumbprint) :
            base(logger, ishDeployment)
        {
            Invoker = new ActionInvoker(logger, "Setting of STS token signing certificate");

            AddActionsToStopSTSApplicationPool(ishDeployment);
            AddActionsToSetTokenSigningCertificate(thumbprint);

            string authenticationType = string.Empty;
            (new GetValueAction(Logger, InputParametersFilePath, InputParametersXml.AuthenticationTypeXPath,
                    result => authenticationType = result)).Execute();

            if (authenticationType == AuthenticationType.Windows.ToString())
            {
                var applicationPoolUser = $@"IIS AppPool\{InputParameters.STSAppPoolName}";
                AddActionsToSetCertificateFilePermission(applicationPoolUser, GetNormalizedThumbprint(thumbprint));
            }
            AddActionsToStartSTSApplicationPool(ishDeployment);
        }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="ishDeployment">The instance of the deployment.</param>
        /// <param name="authenticationType">The authentication type.</param>
        public SetISHSTSConfigurationOperation(ILogger logger, Models.ISHDeployment ishDeployment, AuthenticationType authenticationType) :
            base(logger, ishDeployment)
        {
            CheckPermissions();

            Invoker = new ActionInvoker(logger, "Setting of STS authentication type");

            AddActionsToStopSTSApplicationPool(ishDeployment);
            AddActionsToSetAuthenticationType(ishDeployment, authenticationType);
            AddActionsToStartSTSApplicationPool(ishDeployment);
        }

        /// <summary>
        /// User Access Control check
        /// </summary>
        /// <exception cref="System.Exception">Administrator role not found. Starting new process with elevated rights.</exception>
        private void CheckPermissions()
        {
            var windowsId = System.Security.Principal.WindowsIdentity.GetCurrent();
            var windowsPrincipal = new System.Security.Principal.WindowsPrincipal(windowsId);

            if (!windowsPrincipal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator))
            {
                throw new Exception("Administrator role not found. Please start new process with elevated rights.");
            }
        }

        /// <summary>
        /// Adds the stop STS application pool action.
        /// </summary>
        /// <param name="ishDeployment">The instance of the deployment.</param>
        private void AddActionsToStopSTSApplicationPool(Models.ISHDeployment ishDeployment)
        {
            var stoptOperation = new StopISHComponentOperation(Logger, ishDeployment, ISHComponentName.STS);
            Invoker.AddActionsRange(stoptOperation.Invoker.GetActions());
        }

        /// <summary>
        /// Adds actions for setting STS token signing certificate.
        /// </summary>
        /// <param name="thumbprint">The Token signing certificate Thumbprint.</param>
        private void AddActionsToSetTokenSigningCertificate(string thumbprint)
        {
            thumbprint = GetNormalizedThumbprint(thumbprint);

            var subjectThumbprint = string.Empty;
            (new GetCertificateSubjectByThumbprintAction(Logger, thumbprint, result => subjectThumbprint = result)).Execute();

            Invoker.AddAction(new SetAttributeValueAction(Logger, InfoShareSTSConfigPath, InfoShareSTSConfig.CertificateThumbprintAttributeXPath, thumbprint));

            var fileManager = ObjectFactory.GetInstance<IFileManager>();
            bool isDataBaseFileExist = fileManager.FileExists(InfoShareSTSDataBasePath.AbsolutePath);

            if (isDataBaseFileExist)
            {
                Invoker.AddAction(new SqlCompactExecuteAction(Logger,
                    InfoShareSTSDataBaseConnectionString,
                    string.Format(InfoShareSTSDataBase.UpdateCertificateInKeyMaterialConfigurationSQLCommandFormat,
                        subjectThumbprint)));
            }
        }

        /// <summary>
        /// Adds actions for setting STS authentication type.
        /// </summary>
        /// <param name="ishDeployment">The instance of the deployment.</param>
        /// <param name="authenticationType">The authentication type.</param>
        private void AddActionsToSetAuthenticationType(Models.ISHDeployment ishDeployment, AuthenticationType authenticationType)
        {
            string currentEndpoint = string.Empty;
            (new GetValueAction(Logger, InfoShareWSConnectionConfigPath, InfoShareWSConnectionConfig.WSTrustEndpointUrlXPath,
                result => currentEndpoint = result)).Execute();

            if (authenticationType == AuthenticationType.Windows)
            {
                // Enable Windows Authentication for STS web site
                Invoker.AddAction(new WindowsAuthenticationSwitcherAction(Logger, InputParameters.STSWebAppName, true));

                // If current endpoint is STS endpoint (deployment uses STS as server of authorization)
                // then change the reference to the "issue/wstrust/mixed/windows" endpoint and binding type to WindowsMixed type
                if (currentEndpoint.Contains($"{InputParameters.BaseUrl}/{ishDeployment.WebAppNameSTS}"))
                {
                    var windowsEndpoint = currentEndpoint.Replace("issue/wstrust/mixed/username", "issue/wstrust/mixed/windows");

                    AddActionsToChangeEndpointAndBindingTypes(BindingType.WindowsMixed, windowsEndpoint);
                }

                // Assign user permissions
                var applicationPoolUser = $@"IIS AppPool\{InputParameters.STSAppPoolName}";
                AddActionsToSetCertificateFilePermission(applicationPoolUser, InputParameters.ServiceCertificateThumbprint);
                Invoker.AddAction(new FileSystemRightsAssignAction(Logger, ishDeployment.AppPath, applicationPoolUser, FileSystemRightsAssignAction.FileSystemAccessRights.FullControl));
                if (ishDeployment.AppPath != ishDeployment.DataPath)
                {
                    Invoker.AddAction(new FileSystemRightsAssignAction(Logger, ishDeployment.DataPath, applicationPoolUser, FileSystemRightsAssignAction.FileSystemAccessRights.FullControl));

                }
                if (ishDeployment.DataPath != ishDeployment.WebPath)
                {
                    Invoker.AddAction(new FileSystemRightsAssignAction(Logger, ishDeployment.WebPath, applicationPoolUser, FileSystemRightsAssignAction.FileSystemAccessRights.FullControl));
                }

                // Set ApplicationPoolIdentity identityType for STS application pool
                Invoker.AddAction(new SetApplicationPoolPropertyAction(
                    Logger, 
                    InputParameters.STSAppPoolName,
                    ApplicationPoolProperty.identityType, 
                    ProcessModelIdentityType.ApplicationPoolIdentity));

                Invoker.AddAction(new SetElementValueAction(Logger, InputParametersFilePath, InputParametersXml.InfoshareSTSWindowsAuthenticationEnabledXPath, "true"));
            }
            else
            {
                // Disable Windows Authentication for STS web site
                Invoker.AddAction(new WindowsAuthenticationSwitcherAction(Logger, InputParameters.STSWebAppName, false));

                // If current endpoint is STS endpoint (deployment uses STS as server of authorization)
                // then change the reference to the "issue/wstrust/mixed/username" endpoint and binding type to UserNameMixed type
                if (currentEndpoint.Contains($"{InputParameters.BaseUrl}/{ishDeployment.WebAppNameSTS}"))
                {
                    var usernameEndpoint = currentEndpoint.Replace("issue/wstrust/mixed/windows", "issue/wstrust/mixed/username");

                    AddActionsToChangeEndpointAndBindingTypes(BindingType.UserNameMixed, usernameEndpoint);
                }

                // Set SpecificUser identityType for STS application pool
                Invoker.AddAction(new SetApplicationPoolPropertyAction(
                    Logger, 
                    InputParameters.STSAppPoolName,
                    ApplicationPoolProperty.identityType, 
                    ProcessModelIdentityType.SpecificUser));
                Invoker.AddAction(new SetElementValueAction(Logger, InputParametersFilePath, InputParametersXml.InfoshareSTSWindowsAuthenticationEnabledXPath, "false"));
            }
            Invoker.AddAction(new SetAttributeValueAction(Logger, InfoShareSTSConfigPath, InfoShareSTSConfig.AuthenticationTypeAttributeXPath, authenticationType.ToString()));
            Invoker.AddAction(new SetElementValueAction(Logger, InputParametersFilePath, InputParametersXml.AuthenticationTypeXPath, authenticationType.ToString()));
        }

        private void AddActionsToSetCertificateFilePermission(string applicationPoolUser, string certificateThumbprint)
        {
            string pathToCertificate = string.Empty;
            (new GetPathToCertificateByThumbprintAction(Logger, certificateThumbprint, s => pathToCertificate = s)).Execute();

            Invoker.AddAction(new FileSystemRightsAssignAction(Logger, pathToCertificate, applicationPoolUser, FileSystemRightsAssignAction.FileSystemAccessRights.FullControl));
        }

        /// <summary>
        /// Adds actions to change endpoints and binding types
        /// </summary>
        /// <param name="bindingType">The type of binding.</param>
        /// <param name="endpoint">The endpoint.</param>
        private void AddActionsToChangeEndpointAndBindingTypes(BindingType bindingType, string endpoint)
        {
            string bindingTypeAsString = bindingType.ToString();
            // Change ~\Web\InfoShareWS\connectionconfiguration.xml
            Invoker.AddAction(new SetElementValueAction(Logger, InfoShareWSConnectionConfigPath, InfoShareWSConnectionConfig.WSTrustBindingTypeXPath, bindingTypeAsString));
            Invoker.AddAction(new SetElementValueAction(Logger, InfoShareWSConnectionConfigPath, InfoShareWSConnectionConfig.WSTrustEndpointUrlXPath, endpoint));

            // Change ~\Web\Author\ASP\Trisoft.InfoShare.Client.config
            Invoker.AddAction(new SetElementValueAction(Logger, TrisoftInfoShareClientConfigPath, TrisoftInfoShareClientConfig.WSTrustBindingTypeXPath, bindingTypeAsString));
            Invoker.AddAction(new SetElementValueAction(Logger, TrisoftInfoShareClientConfigPath, TrisoftInfoShareClientConfig.WSTrustEndpointUrlXPath, endpoint));

            // Change ~\Data\PublishingService\Tools\FeedSDLLiveContent.ps1.config
            Invoker.AddAction(new SetAttributeValueAction(Logger, FeedSDLLiveContentConfigPath, FeedSDLLiveContentConfig.WSTrustEndpointUrlXPath, FeedSDLLiveContentConfig.WSTrustBindingTypeAttributeName, bindingTypeAsString));
            Invoker.AddAction(new SetAttributeValueAction(Logger, FeedSDLLiveContentConfigPath, FeedSDLLiveContentConfig.WSTrustEndpointUrlXPath, FeedSDLLiveContentConfig.WSTrustEndpointUrlAttributeName, endpoint));

            // Change ~\App\TranslationOrganizer\Bin\TranslationOrganizer.exe.config
            Invoker.AddAction(new SetAttributeValueAction(Logger, TranslationOrganizerConfigFilePath, TranslationOrganizerConfig.WSTrustEndpointUrlXPath, TranslationOrganizerConfig.WSTrustBindingTypeAttributeName, bindingTypeAsString));
            Invoker.AddAction(new SetAttributeValueAction(Logger, TranslationOrganizerConfigFilePath, TranslationOrganizerConfig.WSTrustEndpointUrlXPath, TranslationOrganizerConfig.WSTrustEndpointUrlAttributeName, endpoint));

            // Change ~\App\Utilities\SynchronizeToLiveContent\SynchronizeToLiveContent.ps1.config
            Invoker.AddAction(new SetAttributeValueAction(Logger, SynchronizeToLiveContentConfigPath, SynchronizeToLiveContentConfig.WSTrustEndpointUrlXPath, SynchronizeToLiveContentConfig.WSTrustBindingTypeAttributeName, bindingTypeAsString));
            Invoker.AddAction(new SetAttributeValueAction(Logger, SynchronizeToLiveContentConfigPath, SynchronizeToLiveContentConfig.WSTrustEndpointUrlXPath, SynchronizeToLiveContentConfig.WSTrustEndpointUrlAttributeName, endpoint));

            // InputParameters.xml
            Invoker.AddAction(new SetElementValueAction(Logger, InputParametersFilePath, InputParametersXml.IssuerWSTrustEndpointUrlXPath, endpoint));
            Invoker.AddAction(new SetElementValueAction(Logger, InputParametersFilePath, InputParametersXml.IssuerWSTrustEndpointUrl_NormalizedXPath, endpoint.Replace(InputParameters.BaseHostName, InputParameters.LocalServiceHostName)));
            Invoker.AddAction(new SetElementValueAction(Logger, InputParametersFilePath, InputParametersXml.IssuerWSTrustBindingTypeXPath, bindingType.ToString()));
        }

        /// <summary>
        /// Adds start STS application pool action.
        /// </summary>
        /// <param name="ishDeployment">The instance of the deployment.</param>
        private void AddActionsToStartSTSApplicationPool(Models.ISHDeployment ishDeployment)
        {
            var startOperation = new StartISHComponentOperation(Logger, ishDeployment, ISHComponentName.STS);
            Invoker.AddActionsRange(startOperation.Invoker.GetActions());
        }

        /// <summary>
        /// Runs current operation.
        /// </summary>
        public void Run()
        {
            Invoker.Invoke();
        }
    }
}
