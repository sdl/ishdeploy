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

using System;
using System.Collections.Generic;
using System.Management.Automation;
﻿using ISHDeploy.Business.Operations.ISHServiceTranslation;
using ISHDeploy.Common.Enums;

namespace ISHDeploy.Cmdlets.ISHServiceTranslation
{
    /// <summary>
    /// <para type="synopsis">Sets translation organizer windows service.</para>
    /// <para type="description">The Set-ISHServiceTranslationOrganizer cmdlet sets translation organizer windows service.</para>
    /// <para type="link">Enable-ISHExternalPreview</para>
    /// </summary>
    /// <example>
    /// <code>PS C:\>Set-ISHServiceTranslationOrganizer -ISHDeployment $deployment -MaxTranslationJobItemsUpdatedInOneCall 1000</code>
    /// <para>This command enables the translation organizer windows service.
    /// Parameter $deployment is a deployment name or an instance of the Content Manager deployment retrieved from Get-ISHDeployment cmdlet.</para>
    /// </example>
    /// <example>
    /// <code>PS C:\>Set-ISHServiceTranslationOrganizer -ISHDeployment $deployment -SynchronizeTemplates $true</code>
    /// <para>This command enables synchronizing the templates.
    /// Parameter $deployment is a deployment name or an instance of the Content Manager deployment retrieved from Get-ISHDeployment cmdlet.</para>
    /// </example>
    /// <example>
    /// <code>PS C:\>Set-ISHServiceTranslationOrganizer -ISHDeployment $deployment -Count 2</code>
    /// <para>This command changes the amount of instances of services.
    /// Parameter $deployment is a deployment name or an instance of the Content Manager deployment retrieved from Get-ISHDeployment cmdlet.</para>
    /// </example>
    [Cmdlet(VerbsCommon.Set, "ISHServiceTranslationOrganizer")]
    public sealed class SetISHServiceTranslationOrganizerCmdlet : BaseHistoryEntryCmdlet
    {
        /// <summary>
        /// <para type="description">The path to dump folder.</para>
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "The path to dump folder", ParameterSetName = "TranslationOrganizerSettings")]
        [ValidateNotNullOrEmpty]
        public string DumpFolder { get; set; }

        /// <summary>
        /// <para type="description">The maximum number of job items updated in one call.</para>
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "The maximum number of job items updated in one call", ParameterSetName = "TranslationOrganizerSettings")]
        [ValidateNotNullOrEmpty]
        [ValidateRange(1, 1000)]
        public int MaxTranslationJobItemsUpdatedInOneCall { get; set; }

        /// <summary>
        /// <para type="description">The interval of job polling.</para>
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "The interval of job polling", ParameterSetName = "TranslationOrganizerSettings")]
        [ValidateNotNullOrEmpty]
        public TimeSpan JobPollingInterval { get; set; }

        /// <summary>
        /// <para type="description">The interval of polling of pending job.</para>
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "The interval of polling of pending job", ParameterSetName = "TranslationOrganizerSettings")]
        [ValidateNotNullOrEmpty]
        public TimeSpan PendingJobPollingInterval { get; set; }

        /// <summary>
        /// <para type="description">The interval of system task.</para>
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "The interval of system task", ParameterSetName = "TranslationOrganizerSettings")]
        [ValidateNotNullOrEmpty]
        public TimeSpan SystemTaskInterval { get; set; }

        /// <summary>
        /// <para type="description">The number of attempts before fail on retrieval.</para>
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "The number of attempts before fail on retrieval", ParameterSetName = "TranslationOrganizerSettings")]
        [ValidateNotNullOrEmpty]
        [ValidateRange(1, 30)]
        public int AttemptsBeforeFailOnRetrieval { get; set; }

        /// <summary>
        /// <para type="description">TUpdate leased by per number of items.</para>
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "Update leased by per number of items", ParameterSetName = "TranslationOrganizerSettings")]
        [ValidateNotNullOrEmpty]
        [ValidateRange(1, 1000)]
        public int UpdateLeasedByPerNumberOfItems { get; set; }

        /// <summary>
        /// <para type="description">Synchronizing the templates.</para>
        /// </summary>
        [Parameter(Mandatory = true, HelpMessage = "Synchronizing the templates", ParameterSetName = "TranslationOrganizerSynchronization")]
        [ValidateNotNullOrEmpty]
        public bool SynchronizeTemplates { get; set; }

        /// <summary>
        /// <para type="description">The number of retries on timeout.</para>
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "The number of retries on timeout", ParameterSetName = "TranslationOrganizerSettings")]
        [ValidateNotNullOrEmpty]
        [ValidateRange(1, 30)]
        public int RetriesOnTimeout { get; set; }

        /// <summary>
        /// <para type="description">The interval of polling of pending job.</para>
        /// </summary>
        [Parameter(Mandatory = true, HelpMessage = "The number of TranslationOrganizer services in the system", ParameterSetName = "TranslationOrganizerCount")]
        [ValidateNotNullOrEmpty]
        [ValidateRange(1, 10)]
        public int Count { get; set; }

        /// <summary>
        /// Executes cmdlet
        /// </summary>
        public override void ExecuteCmdlet()
        {

            if (ParameterSetName == "TranslationOrganizerSettings")
            {
                var parameters = new Dictionary<TranslationOrganizerSetting, object>();
                foreach (var cmdletParameter in MyInvocation.BoundParameters)
                {
                    switch (cmdletParameter.Key)
                    {
                        case "DumpFolder":
                            parameters.Add(TranslationOrganizerSetting.dumpFolder, DumpFolder);
                            break;
                        case "MaxTranslationJobItemsUpdatedInOneCall":
                            parameters.Add(TranslationOrganizerSetting.maxTranslationJobItemsUpdatedInOneCall, MaxTranslationJobItemsUpdatedInOneCall);
                            break;
                        case "JobPollingInterval":
                            parameters.Add(TranslationOrganizerSetting.jobPollingInterval, JobPollingInterval);
                            break;
                        case "PendingJobPollingInterval":
                            parameters.Add(TranslationOrganizerSetting.pendingJobPollingInterval, PendingJobPollingInterval);
                            break;
                        case "SystemTaskInterval":
                            parameters.Add(TranslationOrganizerSetting.systemTaskInterval, SystemTaskInterval);
                            break;
                        case "AttemptsBeforeFailOnRetrieval":
                            parameters.Add(TranslationOrganizerSetting.attemptsBeforeFailOnRetrieval, AttemptsBeforeFailOnRetrieval);
                            break;
                        case "UpdateLeasedByPerNumberOfItems":
                            parameters.Add(TranslationOrganizerSetting.updateLeasedByPerNumberOfItems, UpdateLeasedByPerNumberOfItems);
                            break;
                        case "RetriesOnTimeout":
                            parameters.Add(TranslationOrganizerSetting.retriesOnTimeout, RetriesOnTimeout);
                            break;

                    }
                }

                if (parameters.Count > 0)
                {
                    var operation = new SetISHServiceTranslationOrganizerOperation(Logger, ISHDeployment, parameters);

                    operation.Run();
                }
                else
                {
                    Logger.WriteWarning("The input parameter are not specified");
                }
            }
            else if (ParameterSetName == "TranslationOrganizerSynchronization")
            {
                var operation = new SetISHServiceTranslationOrganizerOperation(Logger, ISHDeployment, new Dictionary<TranslationOrganizerSetting, object> { { TranslationOrganizerSetting.synchronizeTemplates, SynchronizeTemplates } });

                operation.Run();
            }
            else if (ParameterSetName == "TranslationOrganizerCount")
            {
                var operation = new SetISHServiceTranslationOrganizerOperation(Logger, ISHDeployment, Count);

                operation.Run();
            }
        }
    }
}
