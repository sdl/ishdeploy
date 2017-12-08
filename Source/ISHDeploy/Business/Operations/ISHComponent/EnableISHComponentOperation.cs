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
using System.Collections.Generic;
using System.Linq;
using ISHDeploy.Business.Invokers;
using ISHDeploy.Common;
using ISHDeploy.Common.Enums;
using ISHDeploy.Common.Interfaces;
using ISHDeploy.Data.Actions.Asserts;
using ISHDeploy.Data.Actions.COMPlus;
using ISHDeploy.Data.Actions.ISHProject;
using ISHDeploy.Data.Actions.WindowsServices;
using ISHDeploy.Data.Managers.Interfaces;
using Models = ISHDeploy.Common.Models;

namespace ISHDeploy.Business.Operations.ISHComponent
{
    /// <summary>
    /// Enable components of deployment.
    /// </summary>
    /// <seealso cref="IOperation" />
    public class EnableISHComponentOperation : BaseOperationPaths, IOperation
    {
        /// <summary>
        /// The actions invoker
        /// </summary>
        public IActionInvoker Invoker { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnableISHComponentOperation"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="ishDeployment">The instance of the deployment.</param>
        /// <param name="componentsNames">Names of components to be enabled.</param>
        /// <remarks>Don't use this method for background task services</remarks>
        public EnableISHComponentOperation(ILogger logger, Models.ISHDeployment ishDeployment, params ISHComponentName[] componentsNames) :
            base(logger, ishDeployment)
        {
            Invoker = new ActionInvoker(logger, "Enabling of components");

            var dataAggregateHelper = ObjectFactory.GetInstance<IDataAggregateHelper>();
            var components =
               dataAggregateHelper.GetExpectedStateOfComponents(CurrentISHComponentStatesFilePath.AbsolutePath).Components.Where(x => componentsNames.Contains(x.Name)).ToList();

            InitializeActions(logger, ishDeployment, components);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnableISHComponentOperation"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="ishDeployment">The instance of the deployment.</param>
        /// <param name="components">List with the components to be enabled.</param>
        public EnableISHComponentOperation(ILogger logger, Models.ISHDeployment ishDeployment, IEnumerable<Models.ISHComponent> components) :
            base(logger, ishDeployment)
        {
            Invoker = new ActionInvoker(logger, "Enabling of components");
            InitializeActions(logger, ishDeployment, components.ToList());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnableISHComponentOperation"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="ishDeployment">The instance of the deployment.</param>
        /// <param name="backgroundTaskRole">The role of BackgroundTask component to be enabled.</param>
        public EnableISHComponentOperation(ILogger logger, Models.ISHDeployment ishDeployment, string backgroundTaskRole) :
            base(logger, ishDeployment)
        {
            Invoker = new ActionInvoker(logger, $"Enabling of BackgroundTask component with role `{backgroundTaskRole}`");

            var serviceManager = ObjectFactory.GetInstance<IWindowsServiceManager>();
            var dataAggregateHelper = ObjectFactory.GetInstance<IDataAggregateHelper>();

            var componentsCollection =
               dataAggregateHelper.GetExpectedStateOfComponents(CurrentISHComponentStatesFilePath.AbsolutePath);

            var component = componentsCollection[ISHComponentName.BackgroundTask, backgroundTaskRole];

            if (component != null)
            {
                if (ishDeployment.Status == ISHDeploymentStatus.Started || ishDeployment.Status == ISHDeploymentStatus.Starting)
                {
                    // [SCTCM-302] Only enable and start the background task service when the deployment is started or in the process of starting
                    Invoker.AddAction(new WindowsServiceVanillaBackUpAction(logger, VanillaPropertiesOfWindowsServicesFilePath, ishDeployment.Name));

                    var services = serviceManager.GetISHBackgroundTaskWindowsServices(ishDeployment.Name);
                    foreach (
                        var service in
                            services.Where(
                                x => string.Equals(x.Role, component.Role, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        Invoker.AddAction(new SetWindowsServiceStartupTypeAction(Logger, service, ISHWindowsServiceStartupType.Automatic));
                    }
                }
                else
                {
                    Logger.WriteWarning($"The background task with role '{backgroundTaskRole}' will only be marked as enabled. The component '{component.Name}' will not be started, because the deployment '{ishDeployment.Name}' is not started");
                }
            }
            else
            {
                throw new ArgumentException($"The BackgroundTask component with role `{backgroundTaskRole}` does not exist");
            }

            bool isRunning = false;
            if (ishDeployment.Status == ISHDeploymentStatus.Started || ishDeployment.Status == ISHDeploymentStatus.Starting)
            {
                // [SCTCM-302] Only enable and start the components when the deployment is started or in the process of starting
                var startOperation = new StartISHComponentOperation(logger, ishDeployment, backgroundTaskRole);

                Invoker.AddActionsRange(startOperation.Invoker.GetActions());
                isRunning = true;
            }

            Invoker.AddAction(
                new SaveISHComponentAction(
                    Logger,
                    CurrentISHComponentStatesFilePath,
                    component.Role,
                    true, 
                    isRunning));
        }

        /// <summary>
        /// Runs current operation.
        /// </summary>
        public void Run()
        {
            Invoker.Invoke();
        }

        #region Private methods
        /// <summary>
        /// Initializes the actions of the <see cref="EnableISHComponentOperation"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="ishDeployment">The instance of the deployment.</param>
        /// <param name="components">List with the components to be enabled.</param>
        private void InitializeActions(ILogger logger, Models.ISHDeployment ishDeployment, List<Models.ISHComponent> components)
        {
            var serviceManager = ObjectFactory.GetInstance<IWindowsServiceManager>();

            if (components.Any(x => x.Name == ISHComponentName.Crawler)  && !components.Any(x => x.Name == ISHComponentName.SolrLucene))
            {
                var dependencies = serviceManager.GetDependencies(ishDeployment.Name, ISHWindowsServiceType.Crawler);
                if (dependencies.Count() > 0)
                {
                    // Check if SolrLucene is already enabled, otherwise add SolrLucene to the components to enable
                    var dataAggregateHelper = ObjectFactory.GetInstance<IDataAggregateHelper>();

                    var solrLuceneComponents = dataAggregateHelper.GetExpectedStateOfComponents(CurrentISHComponentStatesFilePath.AbsolutePath).Components.Where(x => x.Name == ISHComponentName.SolrLucene).ToArray();
                    if (!solrLuceneComponents.Any(y => y.IsEnabled))
                    {
                        components.AddRange(solrLuceneComponents);
                    }
                }
            }

            foreach (var component in components)
            {
                if (ishDeployment.Status == ISHDeploymentStatus.Started || ishDeployment.Status == ISHDeploymentStatus.Starting)
                {
                    // [SCTCM-302] Only enable and start the components when the deployment is started or in the process of starting
                    IEnumerable<Models.ISHWindowsService> services;
                    switch (component.Name)
                    {
                        case ISHComponentName.COMPlus:
                            // Check if this operation has implications for several Deployments
                            IEnumerable<Models.ISHDeployment> ishDeployments = null;
                            new GetISHDeploymentsAction(logger, string.Empty, result => ishDeployments = result).Execute();

                            var comPlusComponentManager = ObjectFactory.GetInstance<ICOMPlusComponentManager>();
                            var comPlusComponents = comPlusComponentManager.GetCOMPlusComponents();
                            foreach (var comPlusComponent in comPlusComponents)
                            {
                                if (comPlusComponent.Status == ISHCOMPlusComponentStatus.Disabled)
                                {
                                    Invoker.AddAction(new WriteWarningAction(Logger, () => (ishDeployments.Count() > 1),
                                        $"The enabling of COM+ component `{comPlusComponent.Name}` has implications across all deployments."));

                                    Invoker.AddAction(
                                        new EnableCOMPlusComponentAction(Logger, comPlusComponent.Name));
                                }
                                else
                                {
                                    Invoker.AddAction(new WriteVerboseAction(Logger, () => (true),
                                        $"COM+ component `{comPlusComponent.Name}` was already enabled"));
                                }
                            }
                            break;
                        case ISHComponentName.TranslationBuilder:
                            Invoker.AddAction(new WindowsServiceVanillaBackUpAction(logger, VanillaPropertiesOfWindowsServicesFilePath, ishDeployment.Name));

                            services = serviceManager.GetServices(ishDeployment.Name, ISHWindowsServiceType.TranslationBuilder);
                            foreach (var service in services)
                            {
                                Invoker.AddAction(new SetWindowsServiceStartupTypeAction(Logger, service, ISHWindowsServiceStartupType.Automatic));
                            }
                            break;
                        case ISHComponentName.TranslationOrganizer:
                            Invoker.AddAction(new WindowsServiceVanillaBackUpAction(logger, VanillaPropertiesOfWindowsServicesFilePath, ishDeployment.Name));

                            services = serviceManager.GetServices(ishDeployment.Name, ISHWindowsServiceType.TranslationOrganizer);
                            foreach (var service in services)
                            {
                                Invoker.AddAction(new SetWindowsServiceStartupTypeAction(Logger, service, ISHWindowsServiceStartupType.Automatic));
                            }
                            break;
                        case ISHComponentName.Crawler:
                            Invoker.AddAction(new WindowsServiceVanillaBackUpAction(logger, VanillaPropertiesOfWindowsServicesFilePath, ishDeployment.Name));

                            services = serviceManager.GetServices(ishDeployment.Name, ISHWindowsServiceType.Crawler);
                            foreach (var service in services)
                            {
                                Invoker.AddAction(new SetWindowsServiceStartupTypeAction(Logger, service, ISHWindowsServiceStartupType.Automatic));
                            }
                            break;
                        case ISHComponentName.SolrLucene:
                            Invoker.AddAction(new WindowsServiceVanillaBackUpAction(logger, VanillaPropertiesOfWindowsServicesFilePath, ishDeployment.Name));

                            services = serviceManager.GetServices(ishDeployment.Name, ISHWindowsServiceType.SolrLucene);
                            foreach (var service in services)
                            {
                                Invoker.AddAction(new SetWindowsServiceStartupTypeAction(Logger, service, ISHWindowsServiceStartupType.Automatic));
                            }
                            break;
                        case ISHComponentName.BackgroundTask:
                            Invoker.AddAction(new WindowsServiceVanillaBackUpAction(logger, VanillaPropertiesOfWindowsServicesFilePath, ishDeployment.Name));

                            var backgroundTaskServices = serviceManager.GetISHBackgroundTaskWindowsServices(ishDeployment.Name);
                            foreach (var backgroundTaskService in backgroundTaskServices)
                            {
                                if (backgroundTaskService.Role.Equals(component.Role, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    Invoker.AddAction(new SetWindowsServiceStartupTypeAction(Logger, backgroundTaskService, ISHWindowsServiceStartupType.Automatic));
                                }
                            }
                            break;
                    }
                }
                else
                {
                    Logger.WriteWarning($"The component '{component.Name}' will only be marked as enabled. The component '{component.Name}' will not be started, because the deployment '{ishDeployment.Name}' is not started");
                }

                bool isRunning = ishDeployment.Status == ISHDeploymentStatus.Started || ishDeployment.Status == ISHDeploymentStatus.Starting;

                if (component.Name == ISHComponentName.BackgroundTask)
                {
                    Invoker.AddAction(
                        new SaveISHComponentAction(
                            Logger,
                            CurrentISHComponentStatesFilePath,
                            component.Role,
                            true,
                            isRunning));
                }
                else
                {
                    Invoker.AddAction(
                        new SaveISHComponentAction(
                            Logger,
                            CurrentISHComponentStatesFilePath,
                            component.Name,
                            true,
                            isRunning));
                }
            }

            var startOperation = new StartISHComponentOperation(logger, ishDeployment, components);
            Invoker.AddActionsRange(startOperation.Invoker.GetActions());
        }
        #endregion
    }
}
