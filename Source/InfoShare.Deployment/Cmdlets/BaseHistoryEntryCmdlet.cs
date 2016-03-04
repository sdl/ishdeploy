﻿using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Text;
using System.Text.RegularExpressions;
using InfoShare.Deployment.Business;
using InfoShare.Deployment.Data.Managers.Interfaces;

namespace InfoShare.Deployment.Cmdlets
{
    /// <summary>
    /// Base cmdlet class that writes cmdlet usage into history info
    /// </summary>
    public abstract class BaseHistoryEntryCmdlet : BaseCmdlet
    {
        private string CurrentDateTime => DateTime.Now.ToString("yyyyMMdd");

        /// <summary>
        /// Deployment Name
        /// </summary>
        protected abstract ISHPaths IshPaths { get; }
        
        /// <summary>
        /// Overrides ProcessRecord from Cmdlet class
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            AddHistoryEntry();
        }

        /// <summary>
        /// Appends new record to history file
        /// </summary>
        private void AddHistoryEntry()
        {
            if (IshPaths == null)
            {
                throw new ArgumentException($"{nameof(IshPaths)} in {nameof(BaseHistoryEntryCmdlet)} cannot be null.");
            }

            // don't log if cmdlet was executed with WhatIf parameter
            if (MyInvocation.BoundParameters.ContainsKey("WhatIf"))
            {
                return;
            }

            var fileManager = ObjectFactory.GetInstance<IFileManager>();
            var historyEntry = new StringBuilder();
            
            if (!fileManager.Exists(IshPaths.HistoryFilePath)) // create history file with initial record
            {
                historyEntry.AppendLine($"# {CurrentDateTime}");
                historyEntry.AppendLine($"$deployment = Get-ISHDeployment -Deployment '{IshPaths.DeploymentSuffix}'");
            }
            else if (IsNewDate(fileManager.ReadAllText(IshPaths.HistoryFilePath), CurrentDateTime)) // group history records by date inside the file
            {
                historyEntry.AppendLine($"{Environment.NewLine}# {CurrentDateTime}");
            }
            
            historyEntry.AppendLine(InvocationLine);

            fileManager.Append(IshPaths.HistoryFilePath, historyEntry.ToString());
        }

        /// <summary>
        /// Describes which cmdlet was executed with which parameters
        /// </summary>
        private string InvocationLine
        {
            get
            {
                var strBldr = new StringBuilder(MyInvocation.MyCommand.Name);

                foreach (var boundParameter in MyInvocation.BoundParameters)
                {
                    var historyParameter = ToHistoryParameter(boundParameter);

                    if (historyParameter.HasValue)
                    {
                        strBldr.Append($" -{historyParameter.Value.Key} {historyParameter.Value.Value}");
                    }
                }

                return strBldr.ToString();
            }
        }

        private KeyValuePair<string, object>? ToHistoryParameter(KeyValuePair<string, object> boundParameter)
        {
            if (string.Compare(boundParameter.Key, "ISHDeployment", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return new KeyValuePair<string, object>(boundParameter.Key, "$deployment");
            }

            if (boundParameter.Value is SwitchParameter)
            {
                return ((SwitchParameter) boundParameter.Value).IsPresent
                    ? new KeyValuePair<string, object>(boundParameter.Key, string.Empty)
                    : (KeyValuePair<string, object>?)null;
            }

            if (boundParameter.Value is string)
            {
                var stringValue = boundParameter.Value.ToString().Replace("\"", "\"\"");
                return new KeyValuePair<string, object>(boundParameter.Key, $"\"{stringValue}\"");
            }
            
            return boundParameter;
        }

        private bool IsNewDate(string historyContent, string currentDate)
        {
            var lastDate = Regex.Match(historyContent, @"[#]\s\d{8}", RegexOptions.RightToLeft);

            return !lastDate.Value.EndsWith(currentDate);
        }
    }
}
