using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Management.Infrastructure;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("New", "VMSan", SupportsShouldProcess = true, DefaultParameterSetName = "HbaObject")]
[OutputType(new Type[] { typeof(VMSan) })]
internal sealed class NewVMSan : VirtualizationCreationCmdlet<VMSan>, IVMSanCmdlet, IParameterSet
{
	[Parameter]
	[ValidateNotNullOrEmpty]
	public new CimSession CimSession { get; set; }

	[Parameter]
	[ValidateNotNullOrEmpty]
	public new string ComputerName { get; set; }

	[Parameter]
	[ValidateNotNullOrEmpty]
	[Credential]
	public new PSCredential Credential { get; set; }

	[Parameter(Mandatory = true, Position = 0)]
	[ValidateNotNullOrEmpty]
	[Alias(new string[] { "SanName" })]
	public string Name { get; set; }

	[Parameter]
	[ValidateNotNull]
	public string Note { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
	[Parameter(ParameterSetName = "HbaObject")]
	[ValidateNotNullOrEmpty]
	public CimInstance[] HostBusAdapter { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "WorldWide", Justification = "This is per spec.")]
	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
	[Parameter(Mandatory = true, ParameterSetName = "StringWwn")]
	[ValidateNotNullOrEmpty]
	[Alias(new string[] { "Wwnn", "NodeName", "Wwnns", "NodeNames", "WorldWideNodeNames", "NodeAddress" })]
	public string[] WorldWideNodeName { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "WorldWide", Justification = "This is per spec.")]
	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
	[Parameter(Mandatory = true, ParameterSetName = "StringWwn")]
	[ValidateNotNullOrEmpty]
	[Alias(new string[] { "Wwpn", "PortName", "Wwpns", "PortNames", "WorldWidePortNames", "PortAddress" })]
	public string[] WorldWidePortName { get; set; }

	protected override void NormalizeParameters()
	{
		base.NormalizeParameters();
		if (CimSession != null)
		{
			base.CimSession = new CimSession[1] { CimSession };
		}
		if (!ComputerName.IsNullOrEmpty())
		{
			base.ComputerName = new string[1] { ComputerName };
		}
		if (Credential != null)
		{
			base.Credential = new PSCredential[1] { Credential };
		}
	}

	protected override void ValidateParameters()
	{
		base.ValidateParameters();
		ParameterValidator.ValidateHbaParameters(this);
	}

	internal override IList<VMSan> CreateObjects(IOperationWatcher operationWatcher)
	{
		return (from server in ParameterResolvers.GetServers(this, operationWatcher)
			select CreateVMSan(server, operationWatcher)).ToList();
	}

	private VMSan CreateVMSan(Server server, IOperationWatcher operationWatcher)
	{
		if (!operationWatcher.ShouldProcess(CmdletResources.ShouldProcess_NewVMSan))
		{
			return null;
		}
		ParameterResolvers.GetHbaNames(this, out var nodeWwns, out var portWwns);
		return VMSan.CreateVMSan(server, Name, Note, nodeWwns, portWwns, operationWatcher);
	}
}
