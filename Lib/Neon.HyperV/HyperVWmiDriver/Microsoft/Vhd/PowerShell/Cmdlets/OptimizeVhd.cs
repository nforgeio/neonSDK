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

[Cmdlet("Optimize", "VHD", ConfirmImpact = ConfirmImpact.Medium, SupportsShouldProcess = true)]
[OutputType(new Type[] { typeof(VirtualHardDisk) })]
internal sealed class OptimizeVhd : VirtualizationCmdlet<Tuple<Server, string>>, ISupportsAsJob, ISupportsPassthrough
{
	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec. Also arrary is more user friendly.")]
	[ValidateNotNullOrEmpty]
	[Alias(new string[] { "FullName" })]
	[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
	public string[] Path { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter]
	public VhdCompactMode Mode { get; set; }

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
		VhdCompactMode mode = Mode;
		if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_OptimizeVHD, item2)))
		{
			if (!IsParameterSpecified("Mode"))
			{
				string extension = global::System.IO.Path.GetExtension(item2);
				mode = ((!extension.Equals(".VHD", StringComparison.OrdinalIgnoreCase) && !extension.Equals(".AVHD", StringComparison.OrdinalIgnoreCase)) ? VhdCompactMode.Quick : VhdCompactMode.Full);
			}
			VhdUtilities.CompactVirtualHardDisk(item, item2, mode, operationWatcher);
			if (Passthru.IsPresent)
			{
				VirtualHardDisk virtualHardDisk = VhdUtilities.GetVirtualHardDisk(item, item2);
				operationWatcher.WriteObject(virtualHardDisk);
			}
		}
	}
}
