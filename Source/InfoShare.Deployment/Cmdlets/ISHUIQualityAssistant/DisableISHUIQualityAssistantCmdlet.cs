﻿using System.Management.Automation;
using InfoShare.Deployment.Business.CmdSets.ISHUIQualityAssistant;
using InfoShare.Deployment.Providers;

namespace InfoShare.Deployment.Cmdlets.ISHUIQualityAssistant
{
    [Cmdlet(VerbsLifecycle.Disable, "ISHUIQualityAssistant", SupportsShouldProcess = false)]
    public sealed class DisableISHUIQualityAssistantCmdlet : BaseCmdlet
    {
        [Parameter(Mandatory = false, Position = 0)]
        [Alias("proj")]
        [ValidateNotNullOrEmpty]
        public Models.ISHDeployment ISHDeployment { get; set; }

        public override void ExecuteCmdlet()
        {
            var ishDeployment = ISHDeployment ?? ISHProjectProvider.Instance.ISHDeployment;
            var cmdSet = new DisableISHUIQualityAssistantCmdSet(this, ishDeployment.AuthorFolderPath);

            cmdSet.Run();
        }
    }
}
