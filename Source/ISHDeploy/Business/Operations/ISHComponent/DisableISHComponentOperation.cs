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
﻿using ISHDeploy.Common.Interfaces;
using ISHDeploy.Data.Actions.Asserts;
using ISHDeploy.Data.Actions.COMPlus;
using ISHDeploy.Data.Actions.ISHProject;
using ISHDeploy.Data.Actions.WebAdministration;
using ISHDeploy.Data.Actions.WindowsServices;
using ISHDeploy.Data.Managers.Interfaces;
using Models = ISHDeploy.Common.Models;

namespace ISHDeploy.Business.Operations.ISHComponent
{
    /// <summary>
    /// Disable components of deployment.
    /// </summary>
    /// <seealso cref="IOperation" />
    public class DisableISHComponentOperation : BaseOperationPaths, IOperation
    {
        /// <summary>
        /// The actions invoker
        /// </summary>
        private readonly IActionInvoker _invoker;

        /// <summary>
        /// Initializes a new instance of the <see cref="DisableISHComponentOperation"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="ishDeployment">The instance of the deployment.</param>
        /// <param name="components">Names of components to be Disabled.</param>
        public DisableISHComponentOperation(ILogger logger, Models.ISHDeployment ishDeployment, params ISHComponentName[] components) :
            base(logger, ishDeployment)
        {
            _invoker = new ActionInvoker(logger, "Enabling of components");
            var serviceManager = ObjectFactory.GetInstance<IWindowsServiceManager>();
            IEnumerable<Models.ISHWindowsService> services;

            foreach (var component in components)
            {
                switch (component)
                {
                    case ISHComponentName.CM :
                        _invoker.AddAction(new StopApplicationPoolAction(logger, InputParameters.CMAppPoolName));
                        break;
                    case ISHComponentName.WS:
                        _invoker.AddAction(new StopApplicationPoolAction(logger, InputParameters.WSAppPoolName));
                        break;
                    case ISHComponentName.STS:
                        _invoker.AddAction(new StopApplicationPoolAction(logger, InputParameters.STSAppPoolName));
                        break;
                    case ISHComponentName.TranslationBuilder:
                        services = serviceManager.GetServices(ishDeployment.Name, ISHWindowsServiceType.TranslationBuilder);
                        foreach (var service in services)
                        {
                            _invoker.AddAction(new StopWindowsServiceAction(Logger, service));
                        }
                        break;
                    case ISHComponentName.TranslationOrganizer:
                        services = serviceManager.GetServices(ishDeployment.Name, ISHWindowsServiceType.TranslationOrganizer);
                        foreach (var service in services)
                        {
                            _invoker.AddAction(
                                new StopWindowsServiceAction(Logger, service));
                        }
                        break;
                    case ISHComponentName.COMPlus:
                        // Check if this operation has implications for several Deployments
                        IEnumerable<Models.ISHDeployment> ishDeployments = null;
                        new GetISHDeploymentsAction(logger, string.Empty, result => ishDeployments = result).Execute();

                        var comPlusComponentManager = ObjectFactory.GetInstance<ICOMPlusComponentManager>();
                        var comPlusComponents = comPlusComponentManager.GetCOMPlusComponents();
                        foreach (var comPlusComponent in comPlusComponents)
                        {
                            if (comPlusComponent.Status == ISHCOMPlusComponentStatus.Enabled)
                            {
                                _invoker.AddAction(new WriteWarningAction(Logger, () => (ishDeployments.Count() > 1),
                                    $"The disabling of COM+ component `{comPlusComponent.Name}` has implications across all deployments."));

                                _invoker.AddAction(
                                    new DisableCOMPlusComponentAction(Logger, comPlusComponent.Name));
                            }
                        }
                        break;
                    default:
                        throw new ArgumentException($"Unsupported component type: {component}");
                }
                _invoker.AddAction(new SaveISHComponentAction(Logger, CurrentISHComponentStatesFilePath, component, false));
            }
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
