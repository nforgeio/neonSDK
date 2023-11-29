using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell;
using Microsoft.HyperV.PowerShell.Commands;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.Vhd.PowerShell.Cmdlets;

[Cmdlet("Get", "VHDSnapshot", ConfirmImpact = ConfirmImpact.Medium)]
[OutputType(new Type[] { typeof(VHDSnapshotInfo) })]
internal sealed class GetVHDSnapshot : VirtualizationCmdlet<VHDSnapshotInfo>
{
    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec. Also arrary is more user friendly.")]
    [ValidateNotNullOrEmpty]
    [Alias(new string[] { "FullName" })]
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
    public string[] Path { get; set; }

    [Parameter]
    public SwitchParameter GetParentPaths { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec. Also arrary is more user friendly.")]
    [Parameter]
    public Guid[] SnapshotId { get; set; }

    internal override IList<VHDSnapshotInfo> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        List<VHDSnapshotInfo> list = new List<VHDSnapshotInfo>();
        List<string> snapshotIdStrings = new List<string>();
        if (!SnapshotId.IsNullOrEmpty())
        {
            snapshotIdStrings.AddRange(SnapshotId.SelectWithLogging((Guid id) => id.ToString(), operationWatcher).ToList());
        }
        foreach (Server server in ParameterResolvers.GetServers(this, operationWatcher))
        {
            Server currentServer = server;
            IEnumerable<string> inputs = Path.SelectManyWithLogging((string path) => VhdPathResolver.GetVirtualHardDiskFullPath(currentServer, path, base.CurrentFileSystemLocation, base.InvokeProvider), operationWatcher);
            list.AddRange(inputs.SelectManyWithLogging((string path) => VhdUtilities.GetVHDSnapshotInfo(currentServer, path, snapshotIdStrings, GetParentPaths), operationWatcher).ToList());
        }
        return list;
    }

    internal override void ProcessOneOperand(VHDSnapshotInfo operand, IOperationWatcher operationWatcher)
    {
        operationWatcher.WriteObject(operand);
    }
}
