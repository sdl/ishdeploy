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
﻿using ISHDeploy.Business.Invokers;
using ISHDeploy.Data.Actions.Directory;
using ISHDeploy.Common.Interfaces;
using Models = ISHDeploy.Common.Models;

namespace ISHDeploy.Business.Operations.ISHPackage
{
    /// <summary>
    /// Gets the path to the packages folder
    /// </summary>
    /// <seealso cref="BaseOperationPaths" />
    /// <seealso cref="IOperation" />
    public class GetISHPackageFolderPathOperation : BaseOperationPaths, IOperation<string>
    {
        /// <summary>
        /// The actions invoker
        /// </summary>
        public IActionInvoker Invoker { get; }

        /// <summary>
        /// Return path in UNC format
        /// </summary>
        private readonly bool _isUncFormat;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetISHPackageFolderPathOperation"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="ishDeployment">The instance of the deployment.</param>
        /// <param name="isUncFormat">If true return path in UNC format</param>
        public GetISHPackageFolderPathOperation(ILogger logger, Models.ISHDeployment ishDeployment, bool isUncFormat = false) :
            base(logger, ishDeployment)
        {
            _isUncFormat = isUncFormat;

            Invoker = new ActionInvoker(logger, "Getting the path to the packages folder");

            Invoker.AddAction(new DirectoryEnsureExistsAction(logger, PackagesFolderPath));
        }

        /// <summary>
        /// Runs current operation.
        /// </summary>
        public string Run()
        {
            Invoker.Invoke();

            return _isUncFormat ? PackagesFolderUNCPath : PackagesFolderPath; ;
        }
    }
}
