#define TRACE
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.Management.Infrastructure;
using Microsoft.Virtualization.Client;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Stop", "VM", DefaultParameterSetName = "Name", SupportsShouldProcess = true)]
[OutputType(new Type[] { typeof(VirtualMachine) })]
internal sealed class StopVM : VirtualizationCmdlet<VirtualMachine>, IVMObjectOrNameCmdlet, IVmByObjectCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmByNameCmdlet, ISupportsAsJob, ISupportsPassthrough, ISupportsForce
{
	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "Name")]
	[ValidateNotNullOrEmpty]
	public override CimSession[] CimSession { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "Name")]
	[ValidateNotNullOrEmpty]
	public override string[] ComputerName { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "Name")]
	[ValidateNotNullOrEmpty]
	[CredentialArray]
	public override PSCredential[] Credential { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "VMObject", ValueFromPipeline = true, Position = 0, Mandatory = true)]
	public VirtualMachine[] VM { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[ValidateNotNullOrEmpty]
	[Alias(new string[] { "VMName" })]
	[Parameter(ParameterSetName = "Name", ValueFromPipeline = true, Position = 0, Mandatory = true)]
	public string[] Name { get; set; }

	[Parameter]
	public SwitchParameter Save { get; set; }

	[Parameter]
	[SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "TurnOff", Justification = "Spec'd to use TurnOff to avoid negative meaning.")]
	public SwitchParameter TurnOff { get; set; }

	[Parameter]
	public SwitchParameter Force { get; set; }

	[Parameter]
	public SwitchParameter AsJob { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
	[Parameter]
	public SwitchParameter Passthru { get; set; }

	protected override void ValidateParameters()
	{
		base.ValidateParameters();
		if (Save.IsPresent && TurnOff.IsPresent)
		{
			throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.StopVM_SaveAndTurnOffBothSpecified);
		}
	}

	internal override IList<VirtualMachine> EnumerateOperands(IOperationWatcher operationWatcher)
	{
		return ParameterResolvers.ResolveVirtualMachines(this, operationWatcher);
	}

	internal override void ProcessOneOperand(VirtualMachine vm, IOperationWatcher operationWatcher)
	{
		string name = vm.Name;
		bool flag = false;
		if (Save.IsPresent)
		{
			if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_StopVM_Save, name)))
			{
				vm.ChangeState(VirtualMachineAction.Save, operationWatcher);
			}
		}
		else if (TurnOff.IsPresent)
		{
			if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_StopVM_TurnOff, name)))
			{
				vm.ChangeState(VirtualMachineAction.Stop, operationWatcher);
				flag = true;
			}
		}
		else if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_StopVM_ShutDown, name)))
		{
			if (vm.GetCurrentState() == VMState.Off)
			{
				VirtualMachine.WriteAlreadyInDesiredStateWarning(operationWatcher);
			}
			else if (vm.IsShutdownComponentAvailable())
			{
				ShutDown(vm, Force.IsPresent, operationWatcher);
				flag = true;
			}
			else if (operationWatcher.ShouldContinue(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldContinue_StopVM_TurnOffOnShutDown, name)))
			{
				vm.ChangeState(VirtualMachineAction.Stop, operationWatcher);
				flag = true;
			}
		}
		if (flag)
		{
			try
			{
				DeleteAutomaticCheckpoint(vm, operationWatcher);
			}
			catch (Exception ex)
			{
				VMTrace.TraceWarning("Unable to remove automatic checkpoint", ex);
			}
		}
		if (Passthru.IsPresent)
		{
			operationWatcher.WriteObject(vm);
		}
	}

	private void ShutDown(VirtualMachine vm, bool force, IOperationWatcher operationWatcher)
	{
		if (force)
		{
			try
			{
				vm.ChangeState(VirtualMachineAction.ForceShutdown, operationWatcher);
				return;
			}
			catch (VirtualizationException e)
			{
				ExceptionHelper.DisplayErrorOnException(e, operationWatcher);
				vm.ChangeState(VirtualMachineAction.Stop, operationWatcher);
				return;
			}
		}
		vm.ChangeState(VirtualMachineAction.ShutDown, operationWatcher);
	}

	private void DeleteAutomaticCheckpoint(VirtualMachine vm, IOperationWatcher operationWatcher)
	{
		VMSnapshot parentSnapshot = vm.GetParentSnapshot();
		if (parentSnapshot.IsAutomaticCheckpoint)
		{
			((IRemovable)parentSnapshot).Remove(operationWatcher);
		}
	}
}
