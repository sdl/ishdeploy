﻿using System.Management.Automation;
using ISHDeploy.Business;
using ISHDeploy.Extensions;
using ISHDeploy.Business.Operations.ISHDeployment;

namespace ISHDeploy.Cmdlets.ISHDeployment
{
    /// <summary>
    /// <para type="synopsis">Clears customization history for Content Manager deployment.</para>
    /// <para type="description">The Clear-ISHDeploymentHistory cmdlet clears customization history information for Content Manager deployment that was generated by other cmdlets.</para>
    /// <para type="link">Get-ISHDeployment</para>
    /// <para type="link">Get-ISHDeploymentHistory</para>
    /// <para type="link">Undo-ISHDeployment</para>
    /// </summary>
    /// <example>
    /// <code>PS C:\>Clear-ISHDeploymentHistory -ISHDeployment $deployment</code>
    /// <para>This command clears the history information for Content Manager deployment.
    /// Parameter $deployment is an instance of the Content Manager deployment retrieved from Get-ISHDeployment cmdlet.</para>
    /// </example>
    [Cmdlet(VerbsCommon.Clear, "ISHDeploymentHistory")]
    public class ClearISHDeploymentHistoryCmdlet : BaseCmdlet
    {
        /// <summary>
        /// <para type="description">Specifies the instance of the Content Manager deployment.</para>
        /// </summary>
        [Parameter(Mandatory = true, HelpMessage = "Instance of the installed Content Manager deployment.")]
        public Models.ISHDeployment ISHDeployment { get; set; }
        
        /// <summary>
        /// Executes cmdlet
        /// </summary>
        public override void ExecuteCmdlet()
        {
            var ishPaths = new ISHPaths(ISHDeployment);

            // Remove history file
            var historyFilePath = ishPaths.HistoryFilePath;

			// Clean backup directory
			var backupFolderPath = ISHDeployment.GetDeploymentBackupFolder();

            var operation = new ClearISHDeploymentHistoryOperation(Logger, historyFilePath, backupFolderPath);
            operation.Run();
		}
	}
}
