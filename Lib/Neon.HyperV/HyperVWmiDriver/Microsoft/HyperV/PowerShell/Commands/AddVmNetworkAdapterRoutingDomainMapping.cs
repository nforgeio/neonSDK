using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.Management.Infrastructure;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Add", "VMNetworkAdapterRoutingDomainMapping", DefaultParameterSetName = "VMName", SupportsShouldProcess = true)]
[OutputType(new Type[] { typeof(VMNetworkAdapterRoutingDomainSetting) })]
internal sealed class AddVmNetworkAdapterRoutingDomainMapping : VirtualizationCmdlet<VMNetworkAdapterBase>, IVMObjectOrVMNameCmdlet, IVmByObjectCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmByVMNameCmdlet, IVMInternalNetworkAdapterCmdlet, IVMNetworkAdapterBaseCmdlet, IVMNetworkAdapterBaseByObjectCmdlet, ISupportsPassthrough
{
	[ValidateNotNullOrEmpty]
	[Parameter(Position = 1, Mandatory = true)]
	public Guid RoutingDomainID { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter(Position = 2, Mandatory = true)]
	public string RoutingDomainName { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter(Position = 3, Mandatory = true)]
	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
	public int[] IsolationID { get; set; }

	[Parameter(Position = 4, Mandatory = false)]
	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
	public string[] IsolationName { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
	[Parameter]
	public SwitchParameter Passthru { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec and array is easier for users to use.")]
	[Parameter(ParameterSetName = "ResourceObject", ValueFromPipeline = true, Position = 0, Mandatory = true)]
	public VMNetworkAdapterBase[] VMNetworkAdapter { get; set; }

	[Parameter(ParameterSetName = "ManagementOS", Mandatory = true)]
	public SwitchParameter ManagementOS { get; set; }

	[Parameter(ParameterSetName = "VMObject")]
	[Parameter(ParameterSetName = "VMName")]
	[Parameter(ParameterSetName = "ManagementOS")]
	public string VMNetworkAdapterName { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "VMName")]
	[Parameter(ParameterSetName = "ManagementOS")]
	[ValidateNotNullOrEmpty]
	public override CimSession[] CimSession { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "VMName")]
	[Parameter(ParameterSetName = "ManagementOS")]
	[ValidateNotNullOrEmpty]
	public override string[] ComputerName { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "VMName")]
	[Parameter(ParameterSetName = "ManagementOS")]
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

	protected override void NormalizeParameters()
	{
		base.NormalizeParameters();
		if (IsolationName == null)
		{
			IsolationName = new string[IsolationID.Length];
		}
	}

	protected override void ValidateParameters()
	{
		base.ValidateParameters();
	}

	internal override IList<VMNetworkAdapterBase> EnumerateOperands(IOperationWatcher operationWatcher)
	{
		return ParameterResolvers.ResolveNetworkAdapters(this, VMNetworkAdapterName, operationWatcher);
	}

	internal override void ProcessOneOperand(VMNetworkAdapterBase operand, IOperationWatcher operationWatcher)
	{
		if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_AddVMNetworkAdapterRoutingDomainMapping, operand.Name)))
		{
			if (operand.CurrentIsolationMode == VMNetworkAdapterIsolationMode.None)
			{
				throw ExceptionHelper.CreateInvalidStateException(CmdletErrorMessages.VMNetworkAdapterRoutingDomainMapping_IsolationModeDoesNotSupportRoutingDomain, null, operand);
			}
			VMNetworkAdapterRoutingDomainSetting vMNetworkAdapterRoutingDomainSetting = VMNetworkAdapterRoutingDomainSetting.CreateTemplateRoutingDomainSetting(operand);
			vMNetworkAdapterRoutingDomainSetting.RoutingDomainID = RoutingDomainID;
			vMNetworkAdapterRoutingDomainSetting.RoutingDomainName = RoutingDomainName;
			vMNetworkAdapterRoutingDomainSetting.IsolationID = IsolationID;
			vMNetworkAdapterRoutingDomainSetting.IsolationName = IsolationName;
			IEnumerable<VMNetworkAdapterRoutingDomainSetting> output = operand.AddFeatureSettings(new VMNetworkAdapterRoutingDomainSetting[1] { vMNetworkAdapterRoutingDomainSetting }, operationWatcher);
			if (Passthru.IsPresent)
			{
				operationWatcher.WriteObject(output);
			}
		}
	}
}
