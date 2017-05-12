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

using ISHDeploy.Common;
using ISHDeploy.Common.Enums;
using ISHDeploy.Data.Managers.Interfaces;
using ISHDeploy.Common.Interfaces;
using ISHDeploy.Common.Interfaces.Actions;

namespace ISHDeploy.Data.Actions.WebAdministration
{
    /// <summary>
    /// Sets property of application pool
    /// </summary>
    /// <seealso cref="SingleFileCreationAction" />
    public class SetApplicationPoolPropertyAction : BaseAction, IRestorableAction
    {
        /// <summary>
        /// The Application Pool name.
        /// </summary>
        private readonly string _appPoolName;

        /// <summary>
        /// The Application Pool property value.
        /// </summary>
        private readonly object _value;

        /// <summary>
        /// The Application Pool property name.
        /// </summary>
        private readonly ApplicationPoolProperty _propertyName;

        /// <summary>
        /// The web Administration manager
        /// </summary>
        private readonly IWebAdministrationManager _webAdminManager;

        /// <summary>
        /// The Application Pool property previous value.
        /// </summary>
        private object _backedUpValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="SetApplicationPoolPropertyAction"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="appPoolName">Name of the application pool.</param>
        /// <param name="propertyName">The name of ApplicationPool property.</param>
        /// <param name="value">The name of user.</param>
        public SetApplicationPoolPropertyAction(ILogger logger, string appPoolName, ApplicationPoolProperty propertyName, object value)
            : base(logger)
        {
            _appPoolName = appPoolName;
            _propertyName = propertyName;
            _value = value;

            _webAdminManager = ObjectFactory.GetInstance<IWebAdministrationManager>();
        }

        /// <summary>
        ///	Gets current value before change.
        /// </summary>
        public void Backup()
        {
            _backedUpValue = _webAdminManager.GetApplicationPoolProperty(_appPoolName, _propertyName);
        }

        /// <summary>
        /// Executes current action.
        /// </summary>
        public override void Execute()
        {
            _webAdminManager.SetApplicationPoolProperty(_appPoolName, _propertyName, _value);
        }

        /// <summary>
        ///	Reverts a value to initial state.
        /// </summary>
        public void Rollback()
        {
            _webAdminManager.SetApplicationPoolProperty(_appPoolName, _propertyName, _backedUpValue);
        }
    }
}
