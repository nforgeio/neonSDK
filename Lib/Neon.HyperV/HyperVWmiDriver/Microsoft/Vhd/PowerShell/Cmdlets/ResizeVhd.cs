using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell;
using Microsoft.HyperV.PowerShell.Commands;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.Vhd.PowerShell.Cmdlets;

[Cmdlet("Resize", "VHD", DefaultParameterSetName = "Size", ConfirmImpact = ConfirmImpact.Medium, SupportsShouldProcess = true)]
[OutputType(new Type[] { typeof(VirtualHardDisk) })]
internal sealed class ResizeVhd : VirtualizationCmdlet<Tuple<Server, string>>, ISupportsAsJob, ISupportsPassthrough
{
    private static class ParameterSetNames
    {
        public const string MinimumSize = "MinimumSize";

        public const string Size = "Size";
    }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec. Also arrary is more user friendly.")]
    [ValidateNotNullOrEmpty]
    [Alias(new string[] { "FullName" })]
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
    public string[] Path { get; set; }

    [ValidateNotNullOrEmpty]
    [Parameter(Mandatory = true, Position = 1, ParameterSetName = "Size")]
    public ulong SizeBytes { get; set; }

    [Parameter(Mandatory = true, ParameterSetName = "MinimumSize")]
    public SwitchParameter ToMinimumSize { get; set; }

    [Parameter]
    public SwitchParameter AsJob { get; set; }

    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
    [Parameter]
    public SwitchParameter Passthru { get; set; }

    internal override IList<Tuple<Server, string>> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        List<Tuple<Server, string>> list = new List<Tuple<Server, string>>();
        foreach (Server server in ParameterResolvers.GetServers(this, operationWatcher))
        {
            Server currentServer = server;
            IEnumerable<string> source = Path.SelectManyWithLogging((string path) => VhdPathResolver.GetVirtualHardDiskFullPath(currentServer, path, base.CurrentFileSystemLocation, base.InvokeProvider), operationWatcher);
            list.AddRange(source.Select((string path) => Tuple.Create(currentServer, path)));
        }
        return list;
    }

    internal override void ProcessOneOperand(Tuple<Server, string> operand, IOperationWatcher operationWatcher)
    {
        Server item = operand.Item1;
        string item2 = operand.Item2;
        ulong? size = null;
        if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_ResizeVHD, item2)))
        {
            if (CurrentParameterSetIs("Size"))
            {
                size = SizeBytes;
            }
            VhdUtilities.ResizeVirtualHardDisk(item, item2, size, operationWatcher);
            if (Passthru.IsPresent)
            {
                VirtualHardDisk virtualHardDisk = VhdUtilities.GetVirtualHardDisk(item, item2);
                operationWatcher.WriteObject(virtualHardDisk);
            }
        }
    }
}
