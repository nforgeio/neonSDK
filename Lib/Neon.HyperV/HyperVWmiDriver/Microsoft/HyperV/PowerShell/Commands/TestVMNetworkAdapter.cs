using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Management.Infrastructure;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Test", "VMNetworkAdapter", SupportsShouldProcess = true, DefaultParameterSetName = "VMName")]
[OutputType(new Type[] { typeof(VMNetworkAdapterConnectionTestResult) })]
internal sealed class TestVMNetworkAdapter : VirtualizationCmdlet<VMNetworkAdapter>, IVmBySingularObjectCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmBySingularVMNameCmdlet, ISupportsPassthrough
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

	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "VMName", ValueFromPipeline = true, Position = 0, Mandatory = true)]
	public string VMName { get; set; }

	[Parameter(ParameterSetName = "ResourceObject", ValueFromPipeline = true, Position = 0, Mandatory = true)]
	public VMNetworkAdapter VMNetworkAdapter { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "VMObject", ValueFromPipeline = true, Position = 0, Mandatory = true)]
	public VirtualMachine VM { get; set; }

	[Parameter(ParameterSetName = "VMObject")]
	[Parameter(ParameterSetName = "VMName")]
	[Alias(new string[] { "VMNetworkAdapterName" })]
	public string Name { get; set; }

	[Parameter(ParameterSetName = "VMObject")]
	[Parameter(ParameterSetName = "VMName")]
	[Parameter(ParameterSetName = "ResourceObject")]
	public SwitchParameter Sender { get; set; }

	[Parameter(ParameterSetName = "VMObject")]
	[Parameter(ParameterSetName = "VMName")]
	[Parameter(ParameterSetName = "ResourceObject")]
	public SwitchParameter Receiver { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter(Mandatory = true)]
	public string SenderIPAddress { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter(Mandatory = true)]
	public string ReceiverIPAddress { get; set; }

	[Parameter]
	public string NextHopMacAddress { get; set; }

	[Parameter]
	public int? IsolationId { get; set; }

	[Parameter(Mandatory = true)]
	public int SequenceNumber { get; set; }

	[Parameter]
	public int? PayloadSize { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
	[Parameter]
	public SwitchParameter Passthru { get; set; }

	protected override void NormalizeParameters()
	{
		base.NormalizeParameters();
		if (!string.IsNullOrEmpty(NextHopMacAddress))
		{
			NextHopMacAddress = ParameterResolvers.ValidateAndNormalizeMacAddress(NextHopMacAddress, "NextHopMacAddress");
		}
	}

	protected override void ValidateParameters()
	{
		base.ValidateParameters();
		if (!Receiver.IsPresent && !Sender.IsPresent)
		{
			throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.InvalidParameter_MissingSenderOrReceiver);
		}
		if (Sender.IsPresent && string.IsNullOrEmpty(NextHopMacAddress))
		{
			throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.InvalidParameter_MissingMacAddressParameter);
		}
	}

	internal override IList<VMNetworkAdapter> EnumerateOperands(IOperationWatcher operationWatcher)
	{
		if (IsParameterSpecified("VMNetworkAdapter"))
		{
			return new VMNetworkAdapter[1] { VMNetworkAdapter };
		}
		IEnumerable<VMNetworkAdapter> source = ParameterResolvers.ResolveVirtualMachines(this, operationWatcher).SelectManyWithLogging((VirtualMachine vm) => vm.GetNetworkAdapters(), operationWatcher);
		if (!string.IsNullOrEmpty(Name))
		{
			WildcardPattern wildcardPattern = new WildcardPattern(Name, WildcardOptions.Compiled | WildcardOptions.IgnoreCase);
			source = source.Where((VMNetworkAdapter adapter) => wildcardPattern.IsMatch(adapter.Name));
		}
		return source.ToList();
	}

	internal override void ValidateOperandList(IList<VMNetworkAdapter> operands, IOperationWatcher operationWatcher)
	{
		base.ValidateOperandList(operands, operationWatcher);
		if (operands.Count < 1)
		{
			throw ExceptionHelper.CreateObjectNotFoundException(CmdletErrorMessages.VMNetworkAdapter_NoneFound, null);
		}
		if (operands.Count > 1)
		{
			throw ExceptionHelper.CreateObjectNotFoundException(CmdletErrorMessages.VMNetworkAdapter_MoreThanOneFound, null);
		}
	}

	internal override void ProcessOneOperand(VMNetworkAdapter operand, IOperationWatcher operationWatcher)
	{
		if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_TestVMNetworkAdapter, operand.Name)))
		{
			VMNetworkAdapterConnectionTestResult output = operand.TestNetworkConnectivity(Sender.IsPresent, SenderIPAddress, ReceiverIPAddress, NextHopMacAddress, IsolationId.GetValueOrDefault(0), SequenceNumber, PayloadSize.GetValueOrDefault(0));
			operationWatcher.WriteObject(output);
		}
	}
}
