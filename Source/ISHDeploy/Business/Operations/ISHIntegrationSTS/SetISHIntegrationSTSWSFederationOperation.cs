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
﻿using System;
using ISHDeploy.Business.Invokers;
using ISHDeploy.Data.Actions.XmlFile;
using ISHDeploy.Common.Interfaces;
using Models = ISHDeploy.Common.Models;

namespace ISHDeploy.Business.Operations.ISHIntegrationSTS
{
    /// <summary>
    /// Sets WSFederation configuration.
    /// </summary>
    public class SetISHIntegrationSTSWSFederationOperation : BaseOperationPaths, IOperation
    {
        /// <summary>
        /// The actions invoker
        /// </summary>
        public IActionInvoker Invoker { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SetISHIntegrationSTSWSFederationOperation"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="ishDeployment">The instance of the deployment.</param>
        /// <param name="endpoint">The URL to issuer endpoint.</param>
        public SetISHIntegrationSTSWSFederationOperation(ILogger logger, Models.ISHDeployment ishDeployment, Uri endpoint) :
            base(logger, ishDeployment)
        {
            Invoker = new ActionInvoker(logger, "Setting of WSFederation configuration");

            Invoker.AddAction(new SetAttributeValueAction(logger, InfoShareAuthorWebConfigPath, InfoShareAuthorWebConfig.FederationConfigurationXPath, InfoShareAuthorWebConfig.FederationConfigurationAttributeName, endpoint.ToString()));
            Invoker.AddAction(new SetElementValueAction(logger, InputParametersFilePath, InputParametersXml.IssuerWSFederationEndpointUrlXPath, endpoint.ToString()));
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
