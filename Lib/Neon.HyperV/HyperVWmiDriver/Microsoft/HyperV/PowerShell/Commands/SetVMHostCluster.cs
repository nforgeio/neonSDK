using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.Management.Infrastructure;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Set", "VMHostCluster", DefaultParameterSetName = "ClusterName")]
[OutputType(new Type[] { typeof(VMHostCluster) })]
internal sealed class SetVMHostCluster : VirtualizationCmdlet<VMHostCluster>, ISupportsPassthrough
{
	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ValueFromPipeline = true, ParameterSetName = "CimSession", Position = 0, Mandatory = true)]
	[ValidateNotNullOrEmpty]
	public override CimSession[] CimSession { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ValueFromPipeline = true, ParameterSetName = "ClusterName", Position = 0, Mandatory = true)]
	[ValidateNotNullOrEmpty]
	public string[] ClusterName { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ValueFromPipeline = true, ParameterSetName = "ClusterName", Position = 1, Mandatory = false)]
	[ValidateNotNullOrEmpty]
	[CredentialArray]
	public override PSCredential[] Credential { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ValueFromPipeline = true, ParameterSetName = "InputObject", Position = 0, Mandatory = true)]
	[ValidateNotNullOrEmpty]
	public VMHostCluster[] InputObject { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	public override string[] ComputerName
	{
		get
		{
			return ClusterName;
		}
		set
		{
			throw new PSNotImplementedException(string.Format(CultureInfo.CurrentCulture, CmdletResources.ParameterNotImplemented, "ComputerName"));
		}
	}

	[Parameter]
	[ValidateNotNullOrEmpty]
	public string SharedStoragePath { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
	[Parameter]
	public SwitchParameter Passthru { get; set; }

	internal override IList<VMHostCluster> EnumerateOperands(IOperationWatcher operationWatcher)
	{
		if (CurrentParameterSetIs("ClusterName"))
		{
			return VirtualizationObjectLocator.GetVMHostClusters(ParameterResolvers.GetServers(this, operationWatcher), operationWatcher);
		}
		return InputObject.ToList();
	}

	internal override void ProcessOneOperand(VMHostCluster hostCluster, IOperationWatcher operationWatcher)
	{
		if (!string.IsNullOrEmpty(SharedStoragePath))
		{
			hostCluster.SharedStoragePath = SharedStoragePath;
		}
		((IUpdatable)hostCluster).Put(operationWatcher);
		if (Passthru.IsPresent)
		{
			operationWatcher.WriteObject(hostCluster);
		}
	}
}
