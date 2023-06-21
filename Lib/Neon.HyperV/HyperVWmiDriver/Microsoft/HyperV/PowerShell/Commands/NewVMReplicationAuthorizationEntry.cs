using System;
using System.Collections.Generic;
using System.Globalization;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("New", "VMReplicationAuthorizationEntry", SupportsShouldProcess = true)]
[OutputType(new Type[] { typeof(VMReplicationAuthorizationEntry) })]
internal sealed class NewVMReplicationAuthorizationEntry : VirtualizationCreationCmdlet<VMReplicationAuthorizationEntry>
{
    [Alias(new string[] { "AllowedPS" })]
    [ValidateNotNullOrEmpty]
    [Parameter(Mandatory = true, ValueFromPipeline = true, Position = 0)]
    public string AllowedPrimaryServer { get; set; }

    [Alias(new string[] { "StorageLoc" })]
    [ValidateNotNullOrEmpty]
    [Parameter(Mandatory = true, Position = 1)]
    public string ReplicaStorageLocation { get; set; }

    [ValidateNotNullOrEmpty]
    [Parameter(Mandatory = true, Position = 2)]
    public string TrustGroup { get; set; }

    protected override void ValidateParameters()
    {
        base.ValidateParameters();
        if (!string.IsNullOrEmpty(ReplicaStorageLocation))
        {
            ReplicaStorageLocation = PathUtility.GetFullPath(ReplicaStorageLocation, base.CurrentFileSystemLocation);
        }
        if (!string.Equals(AllowedPrimaryServer, "*", StringComparison.OrdinalIgnoreCase))
        {
            string text = AllowedPrimaryServer;
            if (text.StartsWith("*.", StringComparison.CurrentCultureIgnoreCase))
            {
                text = text.Substring(2);
            }
            if (Uri.CheckHostName(text) != UriHostNameType.Dns)
            {
                throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.InvalidParameter_InvalidUri, "AllowedPrimaryServer"), new UriFormatException("AllowedPrimaryServer"));
            }
        }
    }

    internal override IList<VMReplicationAuthorizationEntry> CreateObjects(IOperationWatcher operationWatcher)
    {
        List<VMReplicationAuthorizationEntry> list = new List<VMReplicationAuthorizationEntry>();
        foreach (Server server in ParameterResolvers.GetServers(this, operationWatcher))
        {
            if (!operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_NewVMReplicationAuthorizationEntry, server, AllowedPrimaryServer)))
            {
                continue;
            }
            VMReplicationServer replicationServer = VMReplicationServer.GetReplicationServer(server);
            VMReplicationAuthorizationEntry foundEntry;
            if (string.Equals(AllowedPrimaryServer, "*", StringComparison.OrdinalIgnoreCase))
            {
                if (replicationServer.AuthorizationEntries.Length != 0)
                {
                    throw ExceptionHelper.CreateInvalidOperationException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.InvalidParameter_AuthEntryAllServerNotAllowedWithExistingEntries, "AllowedPrimaryServer"), null, null);
                }
            }
            else if (replicationServer.TryFindAuthorizationEntry("*", out foundEntry))
            {
                throw ExceptionHelper.CreateInvalidOperationException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.InvalidParameter_AuthEntryCustomNotAllowedWithAllServer, "AllowedPrimaryServer"), null, null);
            }
            replicationServer.AddAuthorizationEntry(AllowedPrimaryServer, ReplicaStorageLocation, TrustGroup, operationWatcher);
            replicationServer.TryFindAuthorizationEntry(AllowedPrimaryServer, out foundEntry);
            list.Add(foundEntry);
        }
        return list;
    }
}
