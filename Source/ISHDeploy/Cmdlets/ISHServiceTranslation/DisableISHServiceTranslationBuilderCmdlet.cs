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
﻿using System.Management.Automation;
﻿using ISHDeploy.Business.Operations.ISHComponent;
﻿using ISHDeploy.Common.Enums;

namespace ISHDeploy.Cmdlets.ISHServiceTranslation
{
    /// <summary>
    /// <para type="synopsis">Disables translation builder windows service.</para>
    /// <para type="description">The Disable-ISHServiceTranslationBuilder cmdlet disables translation builder windows service.</para>
    /// <para type="link">Set-ISHServiceTranslationBuilder</para>
    /// <para type="link">Get-ISHServiceTranslationBuilder</para>
    /// <para type="link">Enable-ISHServiceTranslationBuilder</para>
    /// </summary>
    /// <example>
    /// <code>PS C:\>Disable-ISHServiceTranslationBuilder -ISHDeployment $deployment</code>
    /// <para>This command disables the translation builder windows service.
    /// Parameter $deployment is a deployment name or an instance of the Content Manager deployment retrieved from Get-ISHDeployment cmdlet.</para>
    /// </example>
    [Cmdlet(VerbsLifecycle.Disable, "ISHServiceTranslationBuilder")]
    public sealed class DisableISHServiceTranslationBuilderCmdlet : BaseHistoryEntryCmdlet
    {
        /// <summary>
        /// Executes cmdlet
        /// </summary>
        public override void ExecuteCmdlet()
        {
            var operation = new DisableISHComponentOperation(Logger, ISHDeployment, ISHComponentName.TranslationBuilder);

            operation.Run();
        }
    }
}
