//-----------------------------------------------------------------------------
// FILE:        XenClient.TemplateOperations.cs
// CONTRIBUTOR: Jeff Lill
// COPYRIGHT:   Copyright © 2005-2024 by NEONFORGE LLC.  All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Neon.Common;

namespace Neon.XenServer
{
    public partial class XenClient
    {
        /// <summary>
        /// Implements the <see cref="XenClient"/> virtual machine template operations.
        /// </summary>
        public class TemplateOperations
        {
            private XenClient client;

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="client">The XenServer client instance.</param>
            internal TemplateOperations(XenClient client)
            {
                this.client = client;
            }

            /// <summary>
            /// Lists the XenServer virtual machine templates.
            /// </summary>
            /// <returns>The list of templates.</returns>
            /// <exception cref="XenException">Thrown if the operation failed.</exception>
            public List<XenTemplate> List()
            {
                var response  = client.SafeInvokeItems("template-list", "params=all");
                var templates = new List<XenTemplate>();

                foreach (var result in response.Items)
                {
                    templates.Add(new XenTemplate(result));
                }

                return templates;
            }

            /// <summary>
            /// Finds a specific virtual machine template by name or unique ID.
            /// </summary>
            /// <param name="name">Specifies the target name.</param>
            /// <param name="uuid">Specifies the target unique ID.</param>
            /// <returns>The named item or <c>null</c> if it doesn't exist.</returns>
            /// <exception cref="XenException">Thrown if the operation failed.</exception>
            /// <remarks>
            /// <note>
            /// One of <paramref name="name"/> or <paramref name="uuid"/> must be specified.
            /// </note>
            /// </remarks>
            public XenTemplate Find(string name = null, string uuid = null)
            {
                Covenant.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(name) || !string.IsNullOrWhiteSpace(uuid));

                if (!string.IsNullOrWhiteSpace(name))
                {
                    return List().FirstOrDefault(item => item.NameLabel == name);
                }
                else if (!string.IsNullOrWhiteSpace(uuid))
                {
                    return List().FirstOrDefault(item => item.Uuid == uuid);
                }
                else
                {
                    throw new ArgumentException($"One of [{nameof(name)}] or [{nameof(uuid)}] must be specified.");
                }
            }

            /// <summary>
            /// Installs from an XVA or OVA virtual machine template file on the local workstation to the 
            /// remote XenServer host an XVA or OVA virtual machine template, optionally renaming it.
            /// </summary>
            /// <param name="path">The path to the template file on the local workstation.</param>
            /// <param name="name">The optional template name.</param>
            /// <param name="repositoryNameOrUuid">
            /// Optionally specifies the target storage repository by name or UUID.  
            /// This defaults to <b>Local storage</b>.
            /// </param>
            /// <param name="description">
            /// Optionally specifies the template description.
            /// </param>
            /// <returns>The installed template.</returns>
            /// <exception cref="XenException">Thrown if the operation failed.</exception>
            public XenTemplate ImportVmTemplate(string path, string name = null, string repositoryNameOrUuid = XenClient.LocalStorageName, string description = null)
            {
                Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(path), nameof(path));
                Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(repositoryNameOrUuid), nameof(repositoryNameOrUuid));

                var sr = client.Storage.GetTargetStorageRepository(repositoryNameOrUuid);

                var response = client.SafeInvoke("vm-import", $"filename={path}", $"sr-uuid={sr.Uuid}");
                var uuid     = response.OutputText.Trim();
                var template = Find(uuid: uuid);

                if (!string.IsNullOrEmpty(name))
                {
                    template = Rename(template, name);
                }

                if (!string.IsNullOrEmpty(description))
                {
                    client.SafeInvoke("template-param-set", $"uuid={template.Uuid}", $"name-description={description}");
                }

                return template;
            }

            /// <summary>
            /// Renames a virtual machine template.
            /// </summary>
            /// <param name="template">The target template.</param>
            /// <param name="newName">The new template name.</param>
            /// <returns>The modified template.</returns>
            /// <exception cref="XenException">Thrown if the operation failed.</exception>
            public XenTemplate Rename(XenTemplate template, string newName)
            {
                Covenant.Requires<ArgumentNullException>(template != null, nameof(template));
                Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(newName), nameof(newName));

                client.SafeInvoke("template-param-set", $"uuid={template.Uuid}", $"name-label={newName}");

                return Find(uuid: template.Uuid);
            }

            /// <summary>
            /// Removes a virtual machine template.
            /// </summary>
            /// <param name="template">The target template.</param>
            /// <exception cref="XenException">Thrown if the operation failed.</exception>
            public void Destroy(XenTemplate template)
            {
                Covenant.Requires<ArgumentNullException>(template != null, nameof(template));

                // First, we need to destroy any disks associated with the template because
                // the XE [vm-destroy] command doesn't delete these and [vm-uninstall] doesn't
                // work for templates.
                //
                // Note that the templates [VDBs] property seems to include two (perhaps more)
                // VDB UUIDs (separated by semicolons) but only of them actually exists in the
                // storage repo.  We're just going to delete the VBD that actually exisits.
                //
                // Also note that UUIDS for disks returned by [vdi-list] do not match any of
                // the UUIDs listed in template (or VM) [VDBs] property.  These seem to be
                // included in the disk [vdb-uuids] property and strangly, both the template's
                // disk and the disk for any VMs created from the template will have [vdb-uuids]
                // matching the UUIDs from the template.
                //
                // We don't want to delete VM disks here so we need to distinguish between VM
                // disks and the template disk.  It appears that disks with [sm-config=vdi_type: vhd]
                // indicate VHDs attached to a VM, so we won't delete these.

                var vdbUUids = template.Properties["VBDs"].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(item => item.Trim())
                    .ToDictionary(item => item);

                var sr = client.Storage.Find(XenClient.LocalStorageName);

                Covenant.Assert(sr != null);

                // Destroy the template's disk(s).

                var response = client.SafeInvokeItems("vdi-list", $"sr-uuid={sr.Uuid}", "managed=true", "params=all");

                foreach (var disk in response.Items
                    .Where(item => item["sm-config"] != "vdi_type: vhd"))
                {
                    var diskUuid     = disk["uuid"];
                    var diskVdbUuids = disk["vbd-uuids"].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(item => item.Trim());

                    foreach (var diskVdbUuid in diskVdbUuids)
                    {
                        if (vdbUUids.ContainsKey(diskVdbUuid))
                        {
                            client.SafeInvoke("vdi-destroy", $"uuid={diskUuid}");
                            break;
                        }
                    }
                }

                // Destroy the template.

                client.SafeInvoke("vm-destroy", $"uuid={template.Uuid}");
            }
        }
    }
}
