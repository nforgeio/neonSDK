using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Management.Automation;
using Microsoft.Management.Infrastructure;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Get", "VMNetworkAdapterFailoverConfiguration", DefaultParameterSetName = "VMName")]
[OutputType(new Type[] { typeof(VMNetworkAdapterFailoverSetting) })]
internal sealed class GetVMNetworkAdapterFailoverConfiguration : VirtualizationCmdlet<VMNetworkAdapter>, IVMNetworkAdapterBaseCmdlet, IParameterSet, IVmByVMNameCmdlet, IVirtualMachineCmdlet, IServerParameters, IVmByObjectCmdlet
{
	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "VMName")]
	[ValidateNotNullOrEmpty]
	public override CimSession[] CimSession { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "VMName")]
	[ValidateNotNullOrEmpty]
	public override string[] ComputerName { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "VMName")]
	[ValidateNotNullOrEmpty]
	[CredentialArray]
	public override PSCredential[] Credential { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "VMName", ValueFromPipeline = true, Position = 0, Mandatory = true)]
	public string[] VMName { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "VMObject", ValueFromPipeline = true, Position = 0, Mandatory = true)]
	public VirtualMachine[] VM { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec and array is easier for users to use.")]
	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "ResourceObject", ValueFromPipeline = true, Position = 0, Mandatory = true)]
	public VMNetworkAdapter[] VMNetworkAdapter { get; set; }

	[Parameter(ParameterSetName = "VMObject")]
	[Parameter(ParameterSetName = "VMName")]
	public string VMNetworkAdapterName { get; set; }

	internal override IList<VMNetworkAdapter> EnumerateOperands(IOperationWatcher operationWatcher)
	{
		if (CurrentParameterSetIs("ResourceObject"))
		{
			return VMNetworkAdapter;
		}
		return (from adapter in ParameterResolvers.ResolveNetworkAdapters(this, VMNetworkAdapterName, operationWatcher).OfType<VMNetworkAdapter>()
			where adapter.IsSynthetic
			select adapter).ToList();
	}

	internal override void ProcessOneOperand(VMNetworkAdapter operand, IOperationWatcher operationWatcher)
	{
		VMNetworkAdapterFailoverSetting failoverSetting = operand.FailoverSetting;
		if (failoverSetting != null)
		{
			operationWatcher.WriteObject(failoverSetting);
		}
	}
}
