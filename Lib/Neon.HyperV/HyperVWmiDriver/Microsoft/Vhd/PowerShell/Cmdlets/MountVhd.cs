using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell;
using Microsoft.HyperV.PowerShell.Commands;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.Vhd.PowerShell.Cmdlets;

[Cmdlet("Mount", "VHD", ConfirmImpact = ConfirmImpact.Medium, SupportsShouldProcess = true)]
[OutputType(new Type[] { typeof(VirtualHardDisk) })]
internal sealed class MountVhd : VirtualizationCmdlet<Tuple<Server, string>>, ISupportsPassthrough
{
    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec. Also arrary is more user friendly.")]
    [ValidateNotNullOrEmpty]
    [Alias(new string[] { "FullName" })]
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
    public string[] Path { get; set; }

    [Parameter]
    public SwitchParameter NoDriveLetter { get; set; }

    [Parameter]
    public SwitchParameter ReadOnly { get; set; }

    [Parameter(ValueFromPipelineByPropertyName = true)]
    public Guid? SnapshotId { get; set; }

    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
    [Parameter]
    public SwitchParameter Passthru { get; set; }

    protected override void ValidateParameters()
    {
        if (!SnapshotId.HasValue)
        {
            return;
        }
        string[] path = Path;
        foreach (string text in path)
        {
            if (!global::System.IO.Path.GetExtension(text).Equals(".VHDS", StringComparison.OrdinalIgnoreCase))
            {
                throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.InvalidParameter_SnapshotIDOnlyValidForVHDS, text));
            }
        }
    }

    internal override IList<Tuple<Server, string>> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        List<Tuple<Server, string>> list = new List<Tuple<Server, string>>();
        List<string> snapshotIdString = new List<string>();
        if (SnapshotId.HasValue)
        {
            snapshotIdString.Add(SnapshotId.ToString());
        }
        foreach (Server server in ParameterResolvers.GetServers(this, operationWatcher))
        {
            Server currentServer = server;
            IEnumerable<string> enumerable = Path.SelectManyWithLogging((string path) => VhdPathResolver.GetVirtualHardDiskFullPath(currentServer, path, base.CurrentFileSystemLocation, base.InvokeProvider), operationWatcher);
            if (SnapshotId.HasValue)
            {
                enumerable = enumerable.SelectManyWithLogging((string path) => VhdUtilities.GetVHDSnapshotInfo(currentServer, path, snapshotIdString, getParentPaths: false), operationWatcher).SelectWithLogging((VHDSnapshotInfo snapshot) => VhdPathResolver.GetSingleVirtualHardDiskFullPath(currentServer, snapshot.SnapshotPath, snapshot.FilePath, base.InvokeProvider), operationWatcher);
            }
            list.AddRange(enumerable.Select((string path) => Tuple.Create(currentServer, path)));
        }
        return list;
    }

    internal override void ProcessOneOperand(Tuple<Server, string> operand, IOperationWatcher operationWatcher)
    {
        Server item = operand.Item1;
        string item2 = operand.Item2;
        if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_MountVHD, item2)))
        {
            VhdUtilities.MountVirtualHardDisk(item, item2, !NoDriveLetter.IsPresent, ReadOnly.IsPresent, operationWatcher);
            if (Passthru.IsPresent)
            {
                VirtualHardDisk virtualHardDisk = VhdUtilities.GetVirtualHardDisk(item, item2);
                operationWatcher.WriteObject(virtualHardDisk);
            }
        }
    }
}
