﻿using System.Management.Automation;
using ISHDeploy.Business.Operations;
using ISHDeploy.Business.Operations.ISHContentEditor;

namespace ISHDeploy.Cmdlets.ISHContentEditor
{
    /// <summary>
    /// <para type="synopsis">Sets new license for Content Editor.</para>
    /// <para type="description">The Set-ISHContentEditor cmdlet sets new license for Content Editor using domain name and license key parameters.</para>
    /// <para type="link">Test-ISHContentEditor</para>
    /// </summary>
    /// <example>
    /// <para></para>
    /// <code>PS C:\>Set-ISHContentEditor -ISHDeployment $deployment -Domain "localhost" -LicenseKey "123445345342331313432423"</code>
    /// <para>This command sets new license for Content Editor using domain name and license key.
    /// Parameter $deployment is an instance of the Content Manager deployment retrieved from Get-ISHDeployment cmdlet.</para>
    /// </example>
	[Cmdlet(VerbsCommon.Set, "ISHContentEditor")]
	public class SetISHContentEditorCmdlet : BaseHistoryEntryCmdlet
	{
        /// <summary>
        /// Hardcoded value of the license file type
        /// </summary>
		const string LicenseFileExtension = ".txt";
        
		/// <summary>
        /// <para type="description">Specifies the domain name for license.</para>
        /// </summary>
		[Parameter(Mandatory = true, HelpMessage = "Domain name for license")]
		[ValidateNotNullOrEmpty]
		public string Domain { get; set; }

        /// <summary>
        /// <para type="description">Specifies the license key value.</para>
        /// </summary>
		[Parameter(Mandatory = true, HelpMessage = "License key value")]
		[ValidateNotNullOrEmpty]
		public string LicenseKey { get; set; }
        
        /// <summary>
        /// Executes cmdlet
        /// </summary>
        public override void ExecuteCmdlet()
        {
            var operation = new SetISHContentEditorOperation(Logger, OperationPaths.FoldersPaths.LicenceFolderPath, string.Concat(Domain, LicenseFileExtension), LicenseKey);

            operation.Run();
		}
	}
}