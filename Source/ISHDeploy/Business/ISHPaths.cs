﻿using System;
using System.IO;
using ISHDeploy.Models;
using ISHDeploy.Extensions;

namespace ISHDeploy.Business
{
    /// <summary>
    /// Provides absolute paths to all ISH files that are going to be used
    /// </summary>
    public class ISHPaths
    {
        /// <summary>
        /// Specifies Content Manager main folders
        /// </summary>
		public enum IshDeploymentType
		{
            /// <summary>
            /// Content Manager Web folder
            /// </summary>
			Web,
            /// <summary>
            /// Content Manager Data folder
            /// </summary>
			Data,
            /// <summary>
            /// Content Manager App folder
            /// </summary>
			App
        }

        /// <summary>
        /// The instance of the deployment.
        /// </summary>
        private readonly ISHDeployment _ishDeployment;

        /// <summary>
        /// Provides absolute paths to all InfoShare files that are going to be used.
        /// </summary>
        /// <param name="ishDeployment">Instance of the current <see cref="ISHDeployment"/>.</param>
        public ISHPaths(ISHDeployment ishDeployment)
        {
            _ishDeployment = ishDeployment;
        }

        /// <summary>
        /// Gets the file path wrapper.
        /// </summary>
        /// <param name="deploymentType">Type of the deployment.</param>
        /// <param name="filePath">The file path.</param>
        /// <returns>Wrapper around file path.</returns>
        private ISHFilePath GetIshFilePath(IshDeploymentType deploymentType, string filePath)
        {
            return new ISHFilePath(_ishDeployment, deploymentType, filePath);
        }
        
        #region Enable/Disable Content Editor (XOPUS)

        /// <summary>
        /// Path to FolderButtonbar.xml file
        /// </summary>
        public ISHFilePath FolderButtonbar => GetIshFilePath(IshDeploymentType.Web, @"Author\ASP\XSL\FolderButtonbar.xml");

        /// <summary>
        /// Path to InboxButtonBar.xml file
        /// </summary>
		public ISHFilePath InboxButtonBar => GetIshFilePath(IshDeploymentType.Web, @"Author\ASP\XSL\InboxButtonBar.xml");

        /// <summary>
        /// Path to LanguageDocumentButtonbar.xml file
        /// </summary>
		public ISHFilePath LanguageDocumentButtonBar => GetIshFilePath(IshDeploymentType.Web, @"Author\ASP\XSL\LanguageDocumentButtonbar.xml");
        
        /// <summary>
        /// Path to license folder
        /// </summary>
		public ISHFilePath LicenceFolderPath => GetIshFilePath(IshDeploymentType.Web, @"Author\ASP\Editors\Xopus\license\");

        #endregion

        #region Enable/Disable Enrich

        /// <summary>
        /// Path to bluelion-config.xml file
        /// </summary>
		public ISHFilePath EnrichConfig => GetIshFilePath(IshDeploymentType.Web, @"Author\ASP\Editors\Xopus\config\bluelion-config.xml");

        /// <summary>
        /// Path to Xopus config.xml file
        /// </summary>
		public ISHFilePath XopusConfig => GetIshFilePath(IshDeploymentType.Web, @"Author\ASP\Editors\Xopus\config\config.xml");

        /// <summary>
        /// Path to BlueLion-Plugin\web.config file
        /// </summary>
        public ISHFilePath EnrichWebConfig => GetIshFilePath(IshDeploymentType.Web, @"Author\ASP\Editors\Xopus\BlueLion-Plugin\web.config");

        #endregion

        #region Enable/Disable Translation Job
        
        /// <summary>
        /// Path to TopDocumentButtonbar.xml file
        /// </summary>
        public ISHFilePath TopDocumentButtonbar => GetIshFilePath(IshDeploymentType.Web, @"Author\ASP\XSL\TopDocumentButtonbar.xml");

        /// <summary>
        /// Path to Tree.htm file
        /// </summary>
        public ISHFilePath TreeHtm => GetIshFilePath(IshDeploymentType.Web, @"Author\ASP\Tree.htm");

        #endregion

        #region Event Monitor

        /// <summary>
        /// Path to EventMonitorMenuBar.xml file
        /// </summary>
        public ISHFilePath EventMonitorMenuBar => GetIshFilePath(IshDeploymentType.Web, @"Author\ASP\XSL\EventMonitorMenuBar.xml");

        #endregion

        #region InfoShareDeployment folders

        /// <summary>
        /// Path to generated History.ps1 file
        /// </summary>
        public string HistoryFilePath => Path.Combine(_ishDeployment.GetDeploymentAppDataFolder(), "History.ps1");

        /// <summary>
        /// Path to packages folder
        /// </summary>
        public string PackagesFolderPath => _ishDeployment.GetDeploymenPackagesFolderPath();

        /// <summary>
        /// UNC path to packages folder
        /// </summary>
        public string PackagesFolderUNCPath => ConvertLocalFolderPathToUNCPath(_ishDeployment.GetDeploymenPackagesFolderPath());

        /// <summary>
        /// Deployment name
        /// </summary>
        public string DeploymentName => _ishDeployment.Name;

        /// <summary>
        /// Gets the name of the CM web application.
        /// </summary>
        public string CMWebAppName => _ishDeployment.GetCMWebAppName();

        /// <summary>
        /// Gets the name of the WS web application.
        /// </summary>
        public string WSWebAppName => _ishDeployment.GetWSWebAppName();

        /// <summary>
        /// Gets the name of the STS web application.
        /// </summary>
        public string STSWebAppName => _ishDeployment.GetSTSWebAppName();

        /// <summary>
        /// Gets the name of the access host.
        /// </summary>
        public string AccessHostName => _ishDeployment.AccessHostName;

        #endregion
        
        /// <summary>
        /// Converts the local folder path to UNC path.
        /// </summary>
        /// <param name="localPath">The local path.</param>
        /// <returns>Path to folder in UTC format</returns>
        private static string ConvertLocalFolderPathToUNCPath(string localPath)
        {
            return $@"\\{Environment.MachineName}\{localPath.Replace(":", "$")}";
        }
    }
}
