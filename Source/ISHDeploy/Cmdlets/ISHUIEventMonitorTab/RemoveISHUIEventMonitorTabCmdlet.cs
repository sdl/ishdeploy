/**
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
﻿using System.Management.Automation;
using ISHDeploy.Business.Operations.ISHUIEventMonitorTab;

namespace ISHDeploy.Cmdlets.ISHUIEventMonitorTab
{
	/// <summary>
	///		<para type="synopsis">Removes tab from EventMonitorTab.</para>
	///		<para type="description">The Removes-ISHUIEventMonitorTab cmdlet removes Tabs definitions from Content Manager deployment.</para>
	///		<para type="link">Set-ISHUIEventMonitorTab</para>
	///		<para type="link">Move-ISHUIEventMonitorTab</para>
	/// </summary>
	/// <example>
	///		<code>PS C:\>Remove-ISHUIEventMonitorTab -ISHDeployment $deployment -Label "Translation"</code>
	///		<para>Removes definition of the tab with label "Translation".</para>
	///		<para>This command removes XML definitions from EventMonitor.
	///			Parameter $deployment is a deployment name or an instance of the Content Manager deployment retrieved from Get-ISHDeployment cmdlet.
	///		</para>
	/// </example>
	[Cmdlet(VerbsCommon.Remove, "ISHUIEventMonitorTab")]
    public class RemoveISHUIEventMonitorTabCmdlet : BaseHistoryEntryCmdlet
    {
		/// <summary>
		/// <para type="description">Label of menu item.</para>
		/// </summary>
		[Parameter(Mandatory = true, HelpMessage = "Label of menu item")]
		[ValidateNotNullOrEmpty]
		public string Label { get; set; }

        /// <summary>
        /// Executes cmdlet
        /// </summary>
        public override void ExecuteCmdlet()
        {
            var operation = new RemoveISHUIEventMonitorTabOperation(Logger, ISHDeployment, Label);

			operation.Run();
        }
    }
}
