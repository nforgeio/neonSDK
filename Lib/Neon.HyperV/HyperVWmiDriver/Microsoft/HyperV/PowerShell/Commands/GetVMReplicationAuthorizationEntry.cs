using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.HyperV.PowerShell.ExtensionMethods;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Get", "VMReplicationAuthorizationEntry")]
[OutputType(new Type[] { typeof(VMReplicationAuthorizationEntry) })]
internal sealed class GetVMReplicationAuthorizationEntry : VirtualizationCmdlet<VMReplicationAuthorizationEntry>
{
    [Alias(new string[] { "AllowedPS" })]
    [ValidateNotNullOrEmpty]
    [Parameter(Position = 0, ValueFromPipeline = true)]
    public string AllowedPrimaryServer { get; set; }

    [Alias(new string[] { "StorageLoc" })]
    [ValidateNotNullOrEmpty]
    [Parameter]
    public string ReplicaStorageLocation { get; set; }

    [ValidateNotNullOrEmpty]
    [Parameter]
    public string TrustGroup { get; set; }

    protected override void NormalizeParameters()
    {
        base.NormalizeParameters();
        if (!string.IsNullOrEmpty(ReplicaStorageLocation))
        {
            ReplicaStorageLocation = PathUtility.GetFullPath(ReplicaStorageLocation, base.CurrentFileSystemLocation);
        }
    }

    internal override IList<VMReplicationAuthorizationEntry> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        IEnumerable<VMReplicationAuthorizationEntry> source = ParameterResolvers.GetServers(this, operationWatcher).SelectWithLogging(VMReplicationServer.GetReplicationServer, operationWatcher).SelectMany((VMReplicationServer server) => server.AuthorizationEntries);
        if (!string.IsNullOrEmpty(AllowedPrimaryServer))
        {
            source = source.Where((VMReplicationAuthorizationEntry entry) => WildcardPatternMatcher.IsPatternMatching(AllowedPrimaryServer, entry.AllowedPrimaryServer));
        }
        if (!string.IsNullOrEmpty(ReplicaStorageLocation))
        {
            source = source.Where((VMReplicationAuthorizationEntry entry) => WildcardPatternMatcher.IsPatternMatching(ReplicaStorageLocation, entry.ReplicaStorageLocation));
        }
        if (!string.IsNullOrEmpty(TrustGroup))
        {
            source = source.Where((VMReplicationAuthorizationEntry entry) => WildcardPatternMatcher.IsPatternMatching(TrustGroup, entry.TrustGroup));
        }
        List<VMReplicationAuthorizationEntry> list = source.ToList();
        if (list.Count == 0)
        {
            if (!string.IsNullOrEmpty(AllowedPrimaryServer))
            {
                throw ExceptionHelper.CreateOperationFailedException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.VMReplicationAuthorizationEntry_NotFoundByServerName, AllowedPrimaryServer));
            }
            if (!string.IsNullOrEmpty(TrustGroup))
            {
                throw ExceptionHelper.CreateOperationFailedException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.VMReplicationAuthorizationEntry_NotFoundByTrustGroup, TrustGroup));
            }
        }
        return list;
    }

    internal override void ProcessOneOperand(VMReplicationAuthorizationEntry operand, IOperationWatcher operationWatcher)
    {
        operationWatcher.WriteObject(operand);
    }
}
