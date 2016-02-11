﻿using System.IO;
using InfoShare.Deployment.Business.Invokers;
using InfoShare.Deployment.Data;
using InfoShare.Deployment.Data.Commands.XmlFileCommands;
using InfoShare.Deployment.Interfaces;
using InfoShare.Deployment.Models;

namespace InfoShare.Deployment.Business.CmdSets.ISHExternalPreview
{
    public class DisableISHExternalPreviewCmdSet : ICmdSet
    {
        private readonly CommandInvoker _invoker;

        private readonly string[] _commentPatterns = { CommentPatterns.TrisoftExternalPreviewModuleXPath, CommentPatterns.SectionTrisoftInfoshareWebExternalPreviewModuleXPath, CommentPatterns.TrisoftInfoshareWebExternalPreviewModuleXPath };

        public DisableISHExternalPreviewCmdSet(ILogger logger, ISHProject ishProject)
        {
            _invoker = new CommandInvoker(logger, "InfoShare ExternalPreview deactivation");

            _invoker.AddCommand(new XmlSetAttributeValueCommand(logger, Path.Combine(ishProject.AuthorFolderPath, ISHPaths.WebConfig), CommentPatterns.TrisoftInfoshareWebExternalXPath, CommentPatterns.TrisoftInfoshareWebExternalAttributeName, "THE_FISHEXTERNALID_TO_USE"));
            _invoker.AddCommand(new XmlNodeCommentCommand(logger, Path.Combine(ishProject.AuthorFolderPath, ISHPaths.WebConfig), _commentPatterns));
        }

        public void Run()
        {
            _invoker.Invoke();
        }
    }
}
