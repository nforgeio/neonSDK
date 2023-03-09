using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.Management.Infrastructure;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Set", "VM", DefaultParameterSetName = "Name", SupportsShouldProcess = true)]
[OutputType(new Type[] { typeof(VirtualMachine) })]
internal sealed class SetVM : VirtualizationCmdlet<VirtualMachine>, IVMObjectOrNameCmdlet, IVmByObjectCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmByNameCmdlet, ISupportsPassthrough
{
	private const int MaxAutomaticStartDelay = 999999999;

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
	public bool GuestControlledCacheTypes { get; set; }

	[Parameter]
	public uint LowMemoryMappedIoSpace { get; set; }

	[Parameter]
	public ulong HighMemoryMappedIoSpace { get; set; }

	[Parameter]
	public bool BatteryPassthroughEnabled { get; set; }

	[ValidateNotNull]
	[Parameter]
	public long? ProcessorCount { get; set; }

	[Parameter]
	public SwitchParameter DynamicMemory { get; set; }

	[Parameter]
	public SwitchParameter StaticMemory { get; set; }

	[ValidateNotNull]
	[Parameter]
	public long? MemoryMinimumBytes { get; set; }

	[ValidateNotNull]
	[Parameter]
	public long? MemoryMaximumBytes { get; set; }

	[ValidateNotNull]
	[Parameter]
	public long? MemoryStartupBytes { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter]
	public StartAction? AutomaticStartAction { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter]
	public StopAction? AutomaticStopAction { get; set; }

	[ValidateNotNull]
	[Parameter]
	public int? AutomaticStartDelay { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter]
	public CriticalErrorAction? AutomaticCriticalErrorAction { get; set; }

	[ValidateNotNull]
	[Parameter]
	public int? AutomaticCriticalErrorActionTimeout { get; set; }

	[ValidateNotNull]
	[Parameter]
	public bool? AutomaticCheckpointsEnabled { get; set; }

	[ValidateNotNull]
	[Parameter]
	public OnOffState? LockOnDisconnect { get; set; }

	[ValidateNotNull]
	[Parameter]
	public string Notes { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter]
	public string NewVMName { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter]
	public string SnapshotFileLocation { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter]
	public string SmartPagingFilePath { get; set; }

	[Parameter]
	public CheckpointType? CheckpointType { get; set; }

	[Parameter]
	public EnhancedSessionTransportType? EnhancedSessionTransportType { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
	[Parameter]
	public SwitchParameter Passthru { get; set; }

	[Parameter]
	public SwitchParameter AllowUnverifiedPaths { get; set; }

	internal override IList<VirtualMachine> EnumerateOperands(IOperationWatcher operationWatcher)
	{
		return ParameterResolvers.ResolveVirtualMachines(this, operationWatcher);
	}

	internal override void ProcessOneOperand(VirtualMachine operand, IOperationWatcher operationWatcher)
	{
		if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_SetVM, operand.Name)))
		{
			SetMemorySettings(operand, operationWatcher);
			SetProcessorSettings(operand, operationWatcher);
			SetBatterySettings(operand, operationWatcher);
			SetVMProperties(operand, operationWatcher);
			if (Passthru.IsPresent)
			{
				operationWatcher.WriteObject(operand);
			}
		}
	}

	private void SetVMProperties(VirtualMachine virtualMachine, IOperationWatcher operationWatcher)
	{
		if (AutomaticStartAction.HasValue)
		{
			virtualMachine.AutomaticStartAction = AutomaticStartAction.Value;
		}
		if (AutomaticStartDelay.HasValue)
		{
			virtualMachine.AutomaticStartDelay = AutomaticStartDelay.Value;
		}
		if (AutomaticStopAction.HasValue)
		{
			virtualMachine.AutomaticStopAction = AutomaticStopAction.Value;
		}
		if (AutomaticCriticalErrorAction.HasValue)
		{
			virtualMachine.AutomaticCriticalErrorAction = AutomaticCriticalErrorAction.Value;
		}
		if (AutomaticCriticalErrorActionTimeout.HasValue)
		{
			virtualMachine.AutomaticCriticalErrorActionTimeout = AutomaticCriticalErrorActionTimeout.Value;
		}
		if (AutomaticCheckpointsEnabled.HasValue)
		{
			virtualMachine.AutomaticCheckpointsEnabled = AutomaticCheckpointsEnabled.Value;
		}
		if (!string.IsNullOrEmpty(NewVMName))
		{
			virtualMachine.Name = NewVMName;
		}
		if (Notes != null)
		{
			virtualMachine.Notes = Notes;
		}
		if (LockOnDisconnect.HasValue)
		{
			virtualMachine.LockOnDisconnect = LockOnDisconnect.Value;
			if (LockOnDisconnect.Value == OnOffState.On && virtualMachine.RemoteFxAdapter != null)
			{
				operationWatcher.WriteWarning(CmdletErrorMessages.Warning_LockOnDisconnectNotAffectRemotefx);
			}
		}
		bool flag = false;
		if (!string.IsNullOrEmpty(SnapshotFileLocation))
		{
			if (virtualMachine.IsClustered)
			{
				ClusterUtilities.EnsureClusterPathValid(virtualMachine, SnapshotFileLocation, AllowUnverifiedPaths.IsPresent);
				flag = true;
			}
			virtualMachine.SnapshotFileLocation = SnapshotFileLocation;
		}
		if (!string.IsNullOrEmpty(SmartPagingFilePath))
		{
			if (virtualMachine.IsClustered)
			{
				ClusterUtilities.EnsureClusterPathValid(virtualMachine, SmartPagingFilePath, AllowUnverifiedPaths.IsPresent);
				flag = true;
			}
			virtualMachine.SmartPagingFilePath = SmartPagingFilePath;
		}
		if (CheckpointType.HasValue)
		{
			virtualMachine.CheckpointType = CheckpointType.Value;
		}
		if (EnhancedSessionTransportType.HasValue)
		{
			virtualMachine.EnhancedSessionTransportType = EnhancedSessionTransportType.Value;
		}
		if (IsParameterSpecified("GuestControlledCacheTypes"))
		{
			virtualMachine.GuestControlledCacheTypes = GuestControlledCacheTypes;
		}
		if (IsParameterSpecified("LowMemoryMappedIoSpace"))
		{
			virtualMachine.LowMemoryMappedIoSpace = LowMemoryMappedIoSpace;
		}
		if (IsParameterSpecified("HighMemoryMappedIoSpace"))
		{
			virtualMachine.HighMemoryMappedIoSpace = HighMemoryMappedIoSpace;
		}
		((IUpdatable)virtualMachine).Put(operationWatcher);
		if (flag)
		{
			ClusterUtilities.UpdateClusterVMConfiguration(virtualMachine, base.InvokeCommand, operationWatcher);
		}
	}

	private void SetMemorySettings(VirtualMachine virtualMachine, IOperationWatcher operationWatcher)
	{
		bool flag = DynamicMemory.IsPresent || MemoryMinimumBytes.HasValue || MemoryMaximumBytes.HasValue;
		bool isPresent = StaticMemory.IsPresent;
		if ((flag && isPresent) || (flag && !DynamicMemory.IsPresent && !virtualMachine.DynamicMemoryEnabled))
		{
			throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.VMMemoryParameterMixMatching);
		}
		if (DynamicMemory.IsPresent)
		{
			virtualMachine.VirtualNumaEnabled = false;
			((IUpdatable)virtualMachine).Put(operationWatcher);
		}
		VMMemory memory = virtualMachine.GetMemory();
		if (DynamicMemory.IsPresent)
		{
			memory.DynamicMemoryEnabled = true;
		}
		else if (StaticMemory.IsPresent)
		{
			memory.DynamicMemoryEnabled = false;
		}
		if (MemoryStartupBytes.HasValue)
		{
			memory.Startup = MemoryStartupBytes.Value;
		}
		if (MemoryMinimumBytes.HasValue)
		{
			memory.Minimum = MemoryMinimumBytes.Value;
		}
		if (MemoryMaximumBytes.HasValue)
		{
			memory.Maximum = MemoryMaximumBytes.Value;
		}
		((IUpdatable)memory).Put(operationWatcher);
		if (StaticMemory.IsPresent)
		{
			virtualMachine.VirtualNumaEnabled = true;
			((IUpdatable)virtualMachine).Put(operationWatcher);
		}
	}

	private void SetProcessorSettings(VirtualMachine virtualMachine, IOperationWatcher operationWatcher)
	{
		if (ProcessorCount.HasValue)
		{
			VMProcessor processor = virtualMachine.GetProcessor();
			processor.Count = ProcessorCount.Value;
			((IUpdatable)processor).Put(operationWatcher);
		}
	}

	private void SetBatterySettings(VirtualMachine virtualMachine, IOperationWatcher operationWatcher)
	{
		if (IsParameterSpecified("BatteryPassthroughEnabled"))
		{
			VMBattery battery = virtualMachine.GetBattery();
			if (BatteryPassthroughEnabled && battery == null)
			{
				VMBattery.AddBattery(virtualMachine, operationWatcher);
			}
			else if (!BatteryPassthroughEnabled)
			{
				((IRemovable)battery)?.Remove(operationWatcher);
			}
		}
	}
}
