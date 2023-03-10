using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.Management.Infrastructure;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Get", "VMHostCluster", DefaultParameterSetName = "ClusterName")]
[OutputType(new Type[] { typeof(VMHostCluster) })]
internal sealed class GetVMHostCluster : VirtualizationCmdlet<VMHostCluster>
{
	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ValueFromPipelineByPropertyName = true, Position = 0, Mandatory = true, ParameterSetName = "CimSession")]
	[ValidateNotNullOrEmpty]
	public override CimSession[] CimSession { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ValueFromPipeline = true, Position = 0, Mandatory = true, ParameterSetName = "ClusterName")]
	[ValidateNotNullOrEmpty]
	public string[] ClusterName { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ValueFromPipeline = true, Position = 1, ParameterSetName = "ClusterName")]
	[ValidateNotNullOrEmpty]
	[CredentialArray]
	public override PSCredential[] Credential { get; set; }

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

	internal override IList<VMHostCluster> EnumerateOperands(IOperationWatcher operationWatcher)
	{
		return VirtualizationObjectLocator.GetVMHostClusters(ParameterResolvers.GetServers(this, operationWatcher), operationWatcher);
	}

	internal override void ProcessOneOperand(VMHostCluster hostCluster, IOperationWatcher operationWatcher)
	{
		operationWatcher.WriteObject(hostCluster);
	}
}
