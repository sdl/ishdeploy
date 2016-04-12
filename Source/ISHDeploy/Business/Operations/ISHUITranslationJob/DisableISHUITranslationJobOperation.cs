﻿using ISHDeploy.Business.Invokers;
using ISHDeploy.Data.Actions.TextFile;
using ISHDeploy.Data.Actions.XmlFile;
using ISHDeploy.Interfaces;

namespace ISHDeploy.Business.Operations.ISHUITranslationJob
{
    /// <summary>
    /// Disables translation job functionality for Content Manager deployment.
    /// </summary>
    /// <seealso cref="IOperation" />
    public class DisableISHUITranslationJobOperation : IOperation
    {
        /// <summary>
        /// The actions invoker.
        /// </summary>
        private readonly IActionInvoker _invoker;

        /// <summary>
        /// Initializes a new instance of the <see cref="DisableISHUITranslationJobOperation"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="paths">Reference for all files paths.</param>
        public DisableISHUITranslationJobOperation(ILogger logger, ISHPaths paths)
        {
            _invoker = new ActionInvoker(logger, "Disabling of InfoShare translation job");
            
            _invoker.AddAction(new CommentNodesByPrecedingPatternAction(logger, paths.EventMonitorMenuBar, CommentPatterns.EventMonitorTranslationJobs));
            _invoker.AddAction(new CommentNodeByXPathAction(logger, paths.TopDocumentButtonbar, CommentPatterns.TopDocumentTranslationJobXPath, true));
            _invoker.AddAction(new CommentBlockAction(logger, paths.TreeHtm, CommentPatterns.TranslationJobHack));
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
