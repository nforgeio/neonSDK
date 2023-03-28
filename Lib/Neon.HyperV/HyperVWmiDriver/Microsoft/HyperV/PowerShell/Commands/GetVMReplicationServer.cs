using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Management.Infrastructure;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Get", "VMReplicationServer", DefaultParameterSetName = "ComputerName")]
[OutputType(new Type[] { typeof(VMReplicationServer) })]
internal sealed class GetVMReplicationServer : VirtualizationCmdlet<VMReplicationServer>
{
	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "CimSession")]
	[ValidateNotNullOrEmpty]
	public override CimSession[] CimSession { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(Position = 0, ValueFromPipeline = true, ParameterSetName = "ComputerName")]
	[ValidateNotNullOrEmpty]
	public override string[] ComputerName { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(Position = 1, ParameterSetName = "ComputerName")]
	[ValidateNotNullOrEmpty]
	[CredentialArray]
	public override PSCredential[] Credential { get; set; }

	internal override IList<VMReplicationServer> EnumerateOperands(IOperationWatcher operationWatcher)
	{
		return ParameterResolvers.GetServers(this, operationWatcher).SelectWithLogging(VMReplicationServer.GetReplicationServer, operationWatcher).ToList();
	}

	internal override void ProcessOneOperand(VMReplicationServer operand, IOperationWatcher operationWatcher)
	{
		operationWatcher.WriteObject(operand);
	}
}
