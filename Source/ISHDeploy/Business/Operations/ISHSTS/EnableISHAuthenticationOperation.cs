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
using ISHDeploy.Business.Invokers;
using ISHDeploy.Business.Operations.ISHIntegrationSTSWS;
using ISHDeploy.Data.Actions.Directory;
using ISHDeploy.Data.Actions.File;
using ISHDeploy.Data.Actions.StringActions;
using ISHDeploy.Data.Actions.XmlFile;
using ISHDeploy.Interfaces;
using ISHDeploy.Models;
using System.IO;

namespace ISHDeploy.Business.Operations.ISHSTS
{
    /// <summary>
    /// Enable internal STS access
    /// </summary>
    /// <seealso cref="IOperation" />
    public class EnableISHAuthenticationOperation : BaseOperationPaths, IOperation
    {
        /// <summary>
        /// The actions invoker
        /// </summary>
        private readonly IActionInvoker _invoker;

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="ishDeployment">The instance of the deployment.</param>
        /// <param name="lCHost">Host name.</param>
        /// <param name="lCWebAppName">Application name.</param>
        public EnableISHAuthenticationOperation(ILogger logger, Models.ISHDeployment ishDeployment, string lCHost, string lCWebAppName) :
            base(logger, ishDeployment)
        {
            string targetFolderName = "Internal";
            string souceFolder = Path.Combine(AuthorFolderPath, "InfoShareWS");
            string folderToDelete = Path.Combine(souceFolder, targetFolderName);

            _invoker = new ActionInvoker(logger, "Enable internal STS access.");

            // first we need to delete directory and then create it
            _invoker.AddAction(new DirectoryRemoveAction(Logger, folderToDelete));
            //_invoker.AddAction(new DirectoryCreateAction(Logger, souceFolder));

            // Files contents generation 
            var indexContent = string.Empty;
            (new CreateIndexHTMLAction(Logger,
                InputParameters.BaseUrl + "/" + InputParameters.WebAppNameCM + "/",
                InputParameters.BaseUrl + "/" + InputParameters.WebAppNameWS + "/" + targetFolderName,
                InputParameters.BaseUrl + "/" + InputParameters.WebAppNameSTS + "/",
                lCHost,
                lCWebAppName,
                result => indexContent = result)).Execute();

            // Creating index.html file
            var filelPath = new ISHFilePath(souceFolder, BackupWebFolderPath, targetFolderName);
            _invoker.AddAction(new FileCreateAction(Logger, filelPath, "index.html", indexContent));

            string authentication, url, authenticationType = string.Empty;
            (new GetValueAction(Logger, InfoShareWSConnectionConfigPath, InfoShareWSConnectionConfig.WSTrustBindingTypeXPath,
                result => authenticationType = result)).Execute();
            if (authenticationType != AuthenticationTypes.Windows.ToString())
            {
                authentication = BindingType.UserNameMixed.ToString();
                url = InputParameters.BaseUrl + "/" + InputParameters.WebAppNameSTS + "/issue/wstrust/mixed/username";
            }
            else
            {
                authentication = BindingType.WindowsMixed.ToString();;
                url = InputParameters.BaseUrl + "/" + InputParameters.WebAppNameSTS + "/issue/wstrust/mixed/windows";
            }

            // Change ~\Web\InfoShareWS\connectionconfiguration.xml
            _invoker.AddAction(new SetElementValueAction(Logger, InfoShareWSConnectionConfigPath, InfoShareWSConnectionConfig.WSTrustBindingTypeXPath, authentication));
            _invoker.AddAction(new SetElementValueAction(Logger, InfoShareWSConnectionConfigPath, InfoShareWSConnectionConfig.WSTrustEndpointUrlXPath, url));

        }

        /// <summary>
        /// Runs current operation.
        /// </summary>
        public void Run()
        {
            _invoker.Invoke();
        }
    }
}
