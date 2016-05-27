﻿using System.Collections.Generic;
using ISHDeploy.Data.Actions.ISHProject;
using ISHDeploy.Interfaces;
using ISHDeploy.Business.Invokers;

namespace ISHDeploy.Business.Operations.ISHDeployment
{
    // <summary>
    /// Gets a list of installed Content Manager deployments found on the current system.
    /// </summary>
    /// <seealso cref="IOperation{TResult}" />
    public class GetISHDeploymentsOperations : IOperation<IEnumerable<Models.ISHDeploymentExtended>>
    {
        /// <summary>
        /// The actions invoker
        /// </summary>
        private readonly IActionInvoker _invoker;

        /// <summary>
        /// The list of installed Content Manager deployments found on the current system.
        /// </summary>
        private IEnumerable<Models.ISHDeploymentExtended> _ishProjects;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetISHDeploymentOperation"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="projectSuffix">The deployment suffix.</param>
        public GetISHDeploymentsOperations(ILogger logger, string projectSuffix)
        {
            _invoker = new ActionInvoker(logger, "Getting a list of installed Content Manager deployments");
            _invoker.AddAction(new GetISHDeploymentsAction(logger, projectSuffix, result => _ishProjects = result));
        }

        /// <summary>
        /// Runs current operation.
        /// </summary>
        /// <returns>List of installed Content Manager deployments.</returns>
        public IEnumerable<Models.ISHDeploymentExtended> Run()
        {
            _invoker.Invoke();

            return _ishProjects;
        }
    }
}
