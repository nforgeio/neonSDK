#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.HyperV.PowerShell.Common;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Virtualization.Client;
using Microsoft.Virtualization.Client.Management;

using ErrorMessages = Microsoft.HyperV.PowerShell.Common.ErrorMessages;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VirtualMachine : VirtualMachineBase, IMeasurable, IMeasurableInternal, IRemovable, IUpdatable, IVMNetworkAdapterOwner
{
	private class StateTransition
	{
		internal VMState[] AllowedCurrentStates { get; set; }

		internal string Description { get; set; }

		internal VMState TargetState { get; set; }
	}

	private static readonly Dictionary<VirtualMachineAction, StateTransition> gm_StateTransitionMap;

	private readonly SummaryInformationDataUpdater m_SummaryInformation;

	public string ConfigurationLocation => GetSettings(UpdatePolicy.EnsureUpdated).ConfigurationDataRoot;

	public bool SmartPagingFileInUse
	{
		get
		{
			bool result = false;
			if (m_SummaryInformation != null)
			{
				result = m_SummaryInformation.GetData(UpdatePolicy.EnsureUpdated).SwapFilesInUse;
			}
			return result;
		}
	}

	public string SmartPagingFilePath
	{
		get
		{
			return GetSettings(UpdatePolicy.EnsureUpdated).SwapFileDataRoot;
		}
		internal set
		{
			GetSettings(UpdatePolicy.None).SwapFileDataRoot = value;
		}
	}

	public string SnapshotFileLocation
	{
		get
		{
			return GetSettings(UpdatePolicy.EnsureUpdated).SnapshotDataRoot;
		}
		internal set
		{
			GetSettings(UpdatePolicy.None).SnapshotDataRoot = value;
		}
	}

	public StartAction AutomaticStartAction
	{
		get
		{
			return (StartAction)GetSettings(UpdatePolicy.EnsureUpdated).AutomaticStartupAction;
		}
		internal set
		{
			GetSettings(UpdatePolicy.None).AutomaticStartupAction = (ServiceStartOperation)value;
		}
	}

	public int AutomaticStartDelay
	{
		get
		{
			return (int)GetSettings(UpdatePolicy.EnsureUpdated).AutomaticStartupActionDelay.TotalSeconds;
		}
		internal set
		{
			GetSettings(UpdatePolicy.None).AutomaticStartupActionDelay = TimeSpan.FromSeconds(value);
		}
	}

	public StopAction AutomaticStopAction
	{
		get
		{
			return (StopAction)GetSettings(UpdatePolicy.EnsureUpdated).AutomaticShutdownAction;
		}
		internal set
		{
			GetSettings(UpdatePolicy.None).AutomaticShutdownAction = (ServiceStopOperation)value;
		}
	}

	public CriticalErrorAction AutomaticCriticalErrorAction
	{
		get
		{
			return (CriticalErrorAction)GetSettings(UpdatePolicy.EnsureUpdated).AutomaticCriticalErrorAction;
		}
		internal set
		{
			GetSettings(UpdatePolicy.None).AutomaticCriticalErrorAction = (global::Microsoft.Virtualization.Client.Management.CriticalErrorAction)value;
		}
	}

	public int AutomaticCriticalErrorActionTimeout
	{
		get
		{
			return (int)GetSettings(UpdatePolicy.EnsureUpdated).AutomaticCriticalErrorActionTimeout.TotalMinutes;
		}
		internal set
		{
			GetSettings(UpdatePolicy.None).AutomaticCriticalErrorActionTimeout = TimeSpan.FromMinutes(value);
		}
	}

	public bool AutomaticCheckpointsEnabled
	{
		get
		{
			return GetSettings(UpdatePolicy.EnsureUpdated).EnableAutomaticCheckpoints;
		}
		internal set
		{
			GetSettings(UpdatePolicy.None).EnableAutomaticCheckpoints = value;
		}
	}

	public int CPUUsage
	{
		get
		{
			int result = 0;
			if (m_SummaryInformation != null)
			{
				result = m_SummaryInformation.GetData(UpdatePolicy.EnsureUpdated).ProcessorLoad;
			}
			return result;
		}
	}

	public long MemoryAssigned
	{
		get
		{
			long result = 0L;
			if (m_SummaryInformation != null)
			{
				result = m_SummaryInformation.GetData(UpdatePolicy.EnsureUpdated).AssignedMemory * Constants.Mega;
			}
			return result;
		}
	}

	public long MemoryDemand
	{
		get
		{
			long result = 0L;
			if (m_SummaryInformation != null)
			{
				result = m_SummaryInformation.GetData(UpdatePolicy.EnsureUpdated).MemoryDemand * Constants.Mega;
			}
			return result;
		}
	}

	public string MemoryStatus
	{
		get
		{
			VMMemoryStatus vMMemoryStatus = VMMemoryStatus.None;
			if (base.DynamicMemoryEnabled)
			{
				if (SmartPagingFileInUse)
				{
					vMMemoryStatus = VMMemoryStatus.Paging;
				}
				else if (m_SummaryInformation != null)
				{
					int availableMemoryBuffer = m_SummaryInformation.GetData(UpdatePolicy.EnsureUpdated).AvailableMemoryBuffer;
					if (availableMemoryBuffer != int.MaxValue)
					{
						vMMemoryStatus = ((availableMemoryBuffer < 0) ? VMMemoryStatus.Warning : ((availableMemoryBuffer > 80) ? VMMemoryStatus.Ok : VMMemoryStatus.Low));
					}
				}
			}
			else if (m_SummaryInformation != null && m_SummaryInformation.GetData(UpdatePolicy.EnsureUpdated).MemorySpansPhysicalNumaNodes)
			{
				vMMemoryStatus = VMMemoryStatus.Spanning;
			}
			return (string)TypeDescriptor.GetConverter(typeof(VMMemoryStatus)).ConvertTo(vMMemoryStatus, typeof(string));
		}
	}

	public bool? NumaAligned
	{
		get
		{
			bool? result = null;
			List<IVMMemory> source = GetComputerSystem(UpdatePolicy.EnsureAssociatorsUpdated).Memory.ToList();
			if (source.Any())
			{
				result = source.All((IVMMemory memory) => memory.GetVirtualNumaNodes().Any() || memory.GetPhysicalMemory().Count() == 1);
			}
			return result;
		}
	}

	public int NumaNodesCount => GetComputerSystem(UpdatePolicy.EnsureUpdated).NumberOfNumaNodes;

	public int NumaSocketCount
	{
		get
		{
			int num = (int)GetProcessor().MaximumCountPerNumaSocket;
			if (num <= 1)
			{
				return NumaNodesCount;
			}
			return (NumaNodesCount - 1) / num + 1;
		}
	}

	public VMHeartbeatStatus? Heartbeat
	{
		get
		{
			if (State == VMState.Off)
			{
				return null;
			}
			if (m_SummaryInformation != null)
			{
				return (VMHeartbeatStatus)m_SummaryInformation.GetData(UpdatePolicy.EnsureUpdated).Heartbeat;
			}
			return null;
		}
	}

	public string IntegrationServicesState => (string)TypeDescriptor.GetConverter(typeof(ICStatus)).ConvertTo(ICStatus.None, typeof(string));

	public Version IntegrationServicesVersion => new Version("0.0");

	public TimeSpan Uptime
	{
		get
		{
			TimeSpan result = TimeSpan.Zero;
			if (m_SummaryInformation != null)
			{
				result = m_SummaryInformation.GetData(UpdatePolicy.EnsureUpdated).Uptime;
			}
			return result;
		}
	}

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
	public VMOperationalStatus[] OperationalStatus => (from status in GetComputerSystem(UpdatePolicy.EnsureUpdated).GetOperationalStatus()
		select (VMOperationalStatus)status).ToArray();

	public VMOperationalStatus? PrimaryOperationalStatus
	{
		get
		{
			VMOperationalStatus[] operationalStatus = OperationalStatus;
			if (operationalStatus != null && operationalStatus.Length != 0)
			{
				return operationalStatus[0];
			}
			return null;
		}
	}

	public VMOperationalStatus? SecondaryOperationalStatus
	{
		get
		{
			VMOperationalStatus[] operationalStatus = OperationalStatus;
			if (operationalStatus != null && operationalStatus.Length > 1)
			{
				return operationalStatus[1];
			}
			return null;
		}
	}

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
	public string[] StatusDescriptions => GetComputerSystem(UpdatePolicy.EnsureUpdated).GetStatusDescriptions();

	public string PrimaryStatusDescription
	{
		get
		{
			string[] statusDescriptions = StatusDescriptions;
			if (statusDescriptions != null && statusDescriptions.Length != 0)
			{
				return statusDescriptions[0];
			}
			return null;
		}
	}

	public string SecondaryStatusDescription
	{
		get
		{
			string[] statusDescriptions = StatusDescriptions;
			if (statusDescriptions != null && statusDescriptions.Length > 1)
			{
				return statusDescriptions[1];
			}
			return null;
		}
	}

	public string Status
	{
		get
		{
			string[] statusDescriptions = StatusDescriptions;
			if (statusDescriptions.Length > 1 && !string.IsNullOrEmpty(statusDescriptions[1]))
			{
				return statusDescriptions[1];
			}
			if (statusDescriptions.Length != 0)
			{
				return statusDescriptions[0];
			}
			return null;
		}
	}

	public VMReplicationHealthState ReplicationHealth
	{
		get
		{
			VMReplicationHealthState result = VMReplicationHealthState.Normal;
			if (m_SummaryInformation != null)
			{
				result = (VMReplicationHealthState)m_SummaryInformation.GetData(UpdatePolicy.EnsureUpdated).GetReplicationHealthEx()[0];
			}
			return result;
		}
	}

	public VMReplicationMode ReplicationMode
	{
		get
		{
			VMReplicationMode result = VMReplicationMode.None;
			if (m_SummaryInformation != null)
			{
				result = (VMReplicationMode)m_SummaryInformation.GetData(UpdatePolicy.EnsureUpdated).ReplicationMode;
			}
			return result;
		}
	}

	public VMReplicationState ReplicationState
	{
		get
		{
			VMReplicationState result = VMReplicationState.Disabled;
			if (m_SummaryInformation != null)
			{
				ISummaryInformation data = m_SummaryInformation.GetData(UpdatePolicy.EnsureUpdated);
				result = VMReplicationBase.ConvertFailoverReplicationStateToVMReplicationState((VMReplicationMode)data.ReplicationMode, (ReplicationWmiState)data.GetReplicationStateEx()[0], isExtended: false);
			}
			return result;
		}
	}

	public bool ResourceMeteringEnabled
	{
		get
		{
			IVMComputerSystem computerSystemAs = GetComputerSystemAs<IVMComputerSystem>(UpdatePolicy.EnsureAssociatorsUpdated);
			if (computerSystemAs == null)
			{
				return false;
			}
			return computerSystemAs.AggregateMetricEnabledState != MetricEnabledState.Disabled;
		}
	}

	public CheckpointType CheckpointType
	{
		get
		{
			return (CheckpointType)GetSettings(UpdatePolicy.EnsureUpdated).UserSnapshotType;
		}
		internal set
		{
			GetSettings(UpdatePolicy.None).UserSnapshotType = (UserSnapshotType)value;
		}
	}

	public EnhancedSessionTransportType EnhancedSessionTransportType
	{
		get
		{
			return (EnhancedSessionTransportType)GetSettings(UpdatePolicy.EnsureUpdated).EnhancedSessionTransportType;
		}
		internal set
		{
			GetSettings(UpdatePolicy.None).EnhancedSessionTransportType = (global::Microsoft.Virtualization.Client.Management.EnhancedSessionTransportType)value;
		}
	}

	public IReadOnlyList<VMGroup> Groups
	{
		get
		{
			IReadOnlyList<VMGroup> result = null;
			IVMComputerSystem computerSystemAs = GetComputerSystemAs<IVMComputerSystem>(UpdatePolicy.EnsureAssociatorsUpdated);
			if (computerSystemAs != null)
			{
				result = computerSystemAs.CollectingCollections.Select((IVMCollection group) => new VMGroup(group)).ToList().AsReadOnly();
			}
			return result;
		}
	}

	internal bool IsPlanned => GetComputerSystemAs<IVMPlannedComputerSystem>(UpdatePolicy.None) != null;

	public override string Version => GetSettings(UpdatePolicy.EnsureUpdated).Version;

	public VirtualMachineType VirtualMachineType => (VirtualMachineType)GetSettings(UpdatePolicy.EnsureUpdated).VirtualSystemType;

	public VirtualMachineSubType VirtualMachineSubType => (VirtualMachineSubType)GetSettings(UpdatePolicy.EnsureUpdated).VirtualSystemSubType;

	public override string Notes
	{
		get
		{
			return base.Notes;
		}
		internal set
		{
			GetSettings(UpdatePolicy.None).Notes = value;
		}
	}

	public override VMState State
	{
		get
		{
			VMState vMState = (VMState)GetComputerSystem(UpdatePolicy.EnsureUpdated).State;
			if (m_SummaryInformation != null && !IsPlanned && m_SummaryInformation.GetData(UpdatePolicy.EnsureUpdated).HealthState != VMComputerSystemHealthState.Ok)
			{
				vMState = CriticalStateConverter.GetCriticalState(vMState);
			}
			return vMState;
		}
	}

	static VirtualMachine()
	{
		gm_StateTransitionMap = new Dictionary<VirtualMachineAction, StateTransition>();
		gm_StateTransitionMap.Add(VirtualMachineAction.Start, new StateTransition
		{
			AllowedCurrentStates = new VMState[5]
			{
				VMState.Off,
				VMState.Saved,
				VMState.Paused,
				VMState.FastSaved,
				VMState.Hibernated
			},
			TargetState = VMState.Running,
			Description = TaskDescriptions.StartVM
		});
		gm_StateTransitionMap.Add(VirtualMachineAction.Stop, new StateTransition
		{
			AllowedCurrentStates = new VMState[4]
			{
				VMState.Running,
				VMState.Paused,
				VMState.Saved,
				VMState.FastSaved
			},
			TargetState = VMState.Off,
			Description = TaskDescriptions.StopVM
		});
		gm_StateTransitionMap.Add(VirtualMachineAction.ShutDown, new StateTransition
		{
			AllowedCurrentStates = new VMState[1] { VMState.Running },
			TargetState = VMState.Stopping,
			Description = TaskDescriptions.ShutdownVM
		});
		gm_StateTransitionMap.Add(VirtualMachineAction.ForceShutdown, new StateTransition
		{
			AllowedCurrentStates = new VMState[1] { VMState.Running },
			TargetState = VMState.ForceShutdown,
			Description = TaskDescriptions.ShutdownVM
		});
		gm_StateTransitionMap.Add(VirtualMachineAction.Save, new StateTransition
		{
			AllowedCurrentStates = new VMState[2]
			{
				VMState.Running,
				VMState.Paused
			},
			TargetState = VMState.Saved,
			Description = TaskDescriptions.SaveVM
		});
		gm_StateTransitionMap.Add(VirtualMachineAction.Restart, new StateTransition
		{
			AllowedCurrentStates = new VMState[1] { VMState.Running },
			TargetState = VMState.Reset,
			Description = TaskDescriptions.RestartVM
		});
		gm_StateTransitionMap.Add(VirtualMachineAction.Suspend, new StateTransition
		{
			AllowedCurrentStates = new VMState[1] { VMState.Running },
			TargetState = VMState.Paused,
			Description = TaskDescriptions.SuspendVM
		});
		gm_StateTransitionMap.Add(VirtualMachineAction.Resume, new StateTransition
		{
			AllowedCurrentStates = new VMState[1] { VMState.Paused },
			TargetState = VMState.Running,
			Description = TaskDescriptions.ResumeVM
		});
		gm_StateTransitionMap.Add(VirtualMachineAction.DeleteSavedState, new StateTransition
		{
			AllowedCurrentStates = new VMState[2]
			{
				VMState.Saved,
				VMState.FastSaved
			},
			TargetState = VMState.Off,
			Description = TaskDescriptions.RemoveVMSavedState
		});
	}

	internal static VirtualMachine Create(Server server, string name, string location, int generation, Version version, IOperationWatcher operationWatcher)
	{
		IVMService service = ObjectLocator.GetVirtualizationService(server);
		VirtualSystemSubType type = (VirtualSystemSubType)generation;
		return new VirtualMachine(operationWatcher.PerformOperationWithReturn(() => service.BeginCreateVirtualSystem(name, location, type, version), service.EndCreateVirtualSystem<IVMComputerSystem>, TaskDescriptions.NewVM, null));
	}

	internal static VirtualMachine Import(Server server, string definitionFilePath, string snapshotFolder, bool generateNewId, IOperationWatcher operationWatcher)
	{
		IVMService service = ObjectLocator.GetVirtualizationService(server);
		return new VirtualMachine(operationWatcher.PerformOperationWithReturn(() => service.BeginImportSystemDefinition(definitionFilePath, snapshotFolder, generateNewId), (IVMTask task) => service.EndImportSystemDefinition(task, returnImportedVM: true), TaskDescriptions.Task_ImportVM, null));
	}

	internal static VirtualMachine Realize(VirtualMachine pvm, IOperationWatcher operationWatcher)
	{
		IVMService service = ObjectLocator.GetVirtualizationService(pvm.Server);
		IVMPlannedComputerSystem pvmObject = pvm.GetComputerSystemAs<IVMPlannedComputerSystem>(UpdatePolicy.None);
		return new VirtualMachine(operationWatcher.PerformOperationWithReturn(() => service.BeginRealizePlannedVirtualSystem(pvmObject), (IVMTask task) => service.EndRealizePlannedVirtualSystem(task, returnRealizedVm: true), TaskDescriptions.Task_RealizeVM, pvm));
	}

	internal static void WriteAlreadyInDesiredStateWarning(IOperationWatcher operationWatcher)
	{
		operationWatcher.WriteWarning(ErrorMessages.Warning_VMAlreadyInDesiredState);
	}

	internal VirtualMachine(IVMComputerSystemBase computerSystem)
		: base(computerSystem, computerSystem.Setting)
	{
		if (computerSystem is IVMComputerSystem virtualMachine)
		{
			m_SummaryInformation = new SummaryInformationDataUpdater(virtualMachine);
		}
	}

	void IRemovable.Remove(IOperationWatcher operationWatcher)
	{
		IVMComputerSystemBase computerSystem = GetComputerSystem(UpdatePolicy.EnsureUpdated);
		operationWatcher.PerformDelete(computerSystem, TaskDescriptions.RemoveVM, this);
	}

	void IUpdatable.Put(IOperationWatcher operationWatcher)
	{
		IVMComputerSystemSetting settings = GetSettings(UpdatePolicy.None);
		operationWatcher.PerformPut(settings, TaskDescriptions.SetVM, this);
	}

	IMetricMeasurableElement IMeasurableInternal.GetMeasurableElement(UpdatePolicy policy)
	{
		return GetComputerSystemAs<IVMComputerSystem>(policy) ?? throw ExceptionHelper.CreateInvalidOperationException(ErrorMessages.InvalidOperation_MetricsOnPlannedVMNotSupported, null, this);
	}

	IEnumerable<IMetricValue> IMeasurableInternal.GetMetricValues()
	{
		return ((IMeasurableInternal)this).GetMeasurableElement(UpdatePolicy.EnsureAssociatorsUpdated).GetMetricValues();
	}

	internal void ChangeState(VirtualMachineAction action, IOperationWatcher operationWatcher)
	{
		StateTransition stateTransition = gm_StateTransitionMap[action];
		VMState currentState = GetCurrentState();
		VMState targetState = stateTransition.TargetState;
		if (currentState == targetState)
		{
			WriteAlreadyInDesiredStateWarning(operationWatcher);
			return;
		}
		if (!stateTransition.AllowedCurrentStates.Contains(currentState))
		{
			throw ExceptionHelper.CreateInvalidStateException(ErrorMessages.OperationFailed_InvalidState, null, this);
		}
		IVMComputerSystemBase vm = GetComputerSystem(UpdatePolicy.EnsureUpdated);
		operationWatcher.PerformOperation(() => vm.BeginSetState((VMComputerSystemState)targetState), vm.EndSetState, stateTransition.Description, this);
		vm.InvalidatePropertyCache();
		vm.InvalidateAssociationCache();
	}

	internal bool IsShutdownComponentAvailable()
	{
		bool result = false;
		ShutdownComponent shutdownComponent = GetVMIntegrationComponents().OfType<ShutdownComponent>().FirstOrDefault();
		if (shutdownComponent != null && shutdownComponent.Enabled)
		{
			result = shutdownComponent.GetCurrentPrimaryOperationalStatus() == VMIntegrationComponentOperationalStatus.Ok;
		}
		return result;
	}

	internal VMSnapshot TakeSnapshot(IOperationWatcher operationWatcher)
	{
		IVMComputerSystem computerSystemAs = GetComputerSystemAs<IVMComputerSystem>(UpdatePolicy.EnsureUpdated);
		return new VMSnapshot(operationWatcher.PerformOperationWithReturn(computerSystemAs.BeginTakeSnapshot, computerSystemAs.EndTakeSnapshot, TaskDescriptions.CheckpointVM, this), this);
	}

	internal VMSnapshot TakeAutomaticCheckpoint(IOperationWatcher operationWatcher)
	{
		try
		{
			IVMComputerSystem vm = GetComputerSystemAs<IVMComputerSystem>(UpdatePolicy.EnsureUpdated);
			return new VMSnapshot(operationWatcher.PerformOperationWithReturn(() => vm.BeginTakeSnapshot(takeAutomaticSnapshot: true), vm.EndTakeSnapshot, TaskDescriptions.CheckpointVM, this), this);
		}
		catch (Exception ex)
		{
			VMTrace.TraceWarning("Unable to take automatic checkpoint", ex);
			return null;
		}
	}

	internal void InjectNonMaskableInterrupt(IOperationWatcher operationWatcher)
	{
		IVMComputerSystem computerSystemAs = GetComputerSystemAs<IVMComputerSystem>(UpdatePolicy.EnsureUpdated);
		operationWatcher.PerformOperation(computerSystemAs.BeginInjectNonMaskableInterrupt, computerSystemAs.EndInjectNonMaskableInterrupt, TaskDescriptions.DebugVM_InjectNMI, this);
	}

	internal void Upgrade(IOperationWatcher operationWatcher)
	{
		IVMComputerSystem computerSystemAs = GetComputerSystemAs<IVMComputerSystem>(UpdatePolicy.EnsureUpdated);
		operationWatcher.PerformOperation(computerSystemAs.BeginUpgrade, computerSystemAs.EndUpgrade, TaskDescriptions.Task_UpdateVMConfigurationVersion, this);
		computerSystemAs.InvalidatePropertyCache();
		computerSystemAs.InvalidateAssociationCache();
		GetSettings(UpdatePolicy.None).InvalidatePropertyCache();
		GetSettings(UpdatePolicy.None).InvalidateAssociationCache();
	}

	internal override void Export(IOperationWatcher operationWatcher, string path, CaptureLiveState? captureLiveState)
	{
		ExportInternal(operationWatcher, captureLiveState, path, null, TaskDescriptions.Task_ExportingVM);
	}

	internal void SetReplicationStateEx(VMReplication replicationRelationship, ReplicationWmiState state, IOperationWatcher watcher)
	{
		IVMComputerSystem vm = GetComputerSystemAs<IVMComputerSystem>(UpdatePolicy.None);
		IVMReplicationRelationship relationshipObj = replicationRelationship.GetReplicationRelationship(UpdatePolicy.None);
		watcher.PerformOperation(() => vm.BeginSetReplicationStateEx(relationshipObj, (FailoverReplicationState)state), vm.EndSetReplicationState, TaskDescriptions.Task_SetVMReplicationState, null);
		vm.InvalidatePropertyCache();
		vm.InvalidateAssociationCache();
		relationshipObj.InvalidatePropertyCache();
		relationshipObj.InvalidateAssociationCache();
	}

	internal IList<VMSnapshot> GetVMSnapshots()
	{
		return GetComputerSystem(UpdatePolicy.EnsureAssociatorsUpdated).Snapshots.Select((IVMComputerSystemSetting snapshotSetting) => new VMSnapshot(snapshotSetting, this)).ToList();
	}

	internal void PerformStorageCopyForImport(string sourceFolder, string destinationFolder)
	{
		if (string.Equals(sourceFolder, destinationFolder, StringComparison.OrdinalIgnoreCase))
		{
			return;
		}
		List<IVirtualDiskSetting> vhdsForVM = ImportFileUtilities.GetVhdsForVM(GetComputerSystem(UpdatePolicy.EnsureUpdated));
		Dictionary<string, string> dictionary = ImportFileUtilities.FindStorageFiles(base.Server, vhdsForVM, sourceFolder, fallbackToOriginalPath: false);
		List<string> list = new List<string>();
		try
		{
			foreach (KeyValuePair<string, string> item in dictionary)
			{
				string destinationFile = global::System.IO.Path.Combine(destinationFolder, item.Key);
				FileUtilities.CopyFile(base.Server, item.Value, destinationFile, overwrite: true);
				list.Add(item.Value);
			}
		}
		catch (Exception)
		{
			foreach (string item2 in list)
			{
				FileUtilities.DeleteFile(base.Server, item2);
			}
			throw;
		}
	}

	internal void ImportSnapshotsFromFolder(IOperationWatcher operationWatcher, string snapshotFolder)
	{
		IVMService service = ObjectLocator.GetVirtualizationService(base.Server);
		IVMPlannedComputerSystem pvm = GetComputerSystemAs<IVMPlannedComputerSystem>(UpdatePolicy.None);
		operationWatcher.PerformOperation(() => service.BeginImportSnapshotDefinitions(pvm, snapshotFolder), delegate(IVMTask task)
		{
			service.EndImportSnapshotDefinitions(task, returnSnapshots: false);
		}, TaskDescriptions.Task_ImportSnapshots, this);
	}

	internal IEnumerable<VMConnectAce> GetVMConnectAccess()
	{
		return from ace in ObjectLocator.GetTerminalService(base.Server).GetVMConnectAccess(GetComputerSystemAs<IVMComputerSystem>(UpdatePolicy.None))
			select new VMConnectAce(this, ace);
	}

	internal void GrantVMConnectAccess(ICollection<string> trustees, IOperationWatcher operationWatcher)
	{
		ITerminalService service = ObjectLocator.GetTerminalService(base.Server);
		operationWatcher.PerformOperation(() => service.BeginGrantVMConnectAccess(GetComputerSystemAs<IVMComputerSystem>(UpdatePolicy.None), trustees), service.EndGrantVMConnectAccess, TaskDescriptions.GrantVMConnectAccess, this);
	}

	internal void RevokeVMConnectAccess(ICollection<string> trustees, IOperationWatcher operationWatcher)
	{
		ITerminalService service = ObjectLocator.GetTerminalService(base.Server);
		operationWatcher.PerformOperation(() => service.BeginRevokeVMConnectAccess(GetComputerSystemAs<IVMComputerSystem>(UpdatePolicy.None), trustees), service.EndRevokeVMConnectAccess, TaskDescriptions.RevokeVMConnectAccess, this);
	}

	internal void InvalidateAssociations()
	{
		GetComputerSystemAs<IVMComputerSystem>(UpdatePolicy.None).InvalidateAssociationCache();
	}

	internal VMState GetCurrentState()
	{
		VMState result = VMState.Off;
		IVMComputerSystem computerSystemAs = GetComputerSystemAs<IVMComputerSystem>(UpdatePolicy.EnsureAssociatorsUpdated);
		if (computerSystemAs != null)
		{
			ISummaryInformation vMSummaryInformation = computerSystemAs.GetVMSummaryInformation(SummaryInformationRequest.StateOnly);
			if (vMSummaryInformation != null)
			{
				result = (VMState)vMSummaryInformation.State;
			}
		}
		return result;
	}

	internal TimeSpan GetCurrentUptime()
	{
		IVMComputerSystem computerSystemAs = GetComputerSystemAs<IVMComputerSystem>(UpdatePolicy.EnsureAssociatorsUpdated);
		if (computerSystemAs != null)
		{
			ISummaryInformation vMSummaryInformation = computerSystemAs.GetVMSummaryInformation(SummaryInformationRequest.UptimeOnly);
			if (vMSummaryInformation != null)
			{
				return vMSummaryInformation.Uptime;
			}
		}
		return Uptime;
	}

	internal VMHeartbeatStatus? GetCurrentHeartbeat()
	{
		IVMComputerSystem computerSystemAs = GetComputerSystemAs<IVMComputerSystem>(UpdatePolicy.EnsureAssociatorsUpdated);
		if (computerSystemAs != null)
		{
			ISummaryInformation vMSummaryInformation = computerSystemAs.GetVMSummaryInformation(SummaryInformationRequest.HeartbeatOnly);
			if (vMSummaryInformation != null)
			{
				return (VMHeartbeatStatus)vMSummaryInformation.Heartbeat;
			}
		}
		return Heartbeat;
	}

	internal long GetCurrentMemoryDemand()
	{
		long result = 0L;
		IVMComputerSystem computerSystemAs = GetComputerSystemAs<IVMComputerSystem>(UpdatePolicy.EnsureAssociatorsUpdated);
		if (computerSystemAs != null)
		{
			ISummaryInformation vMSummaryInformation = computerSystemAs.GetVMSummaryInformation(SummaryInformationRequest.MemoryDemandOnly);
			if (vMSummaryInformation != null)
			{
				result = vMSummaryInformation.MemoryDemand * Constants.Mega;
			}
		}
		return result;
	}

	private bool IsHeartbeatRunning()
	{
		VMHeartbeatStatus? currentHeartbeat = GetCurrentHeartbeat();
		if (currentHeartbeat != VMHeartbeatStatus.OkApplicationsCritical && currentHeartbeat != VMHeartbeatStatus.OkApplicationsHealthy)
		{
			return currentHeartbeat == VMHeartbeatStatus.OkApplicationsUnknown;
		}
		return true;
	}

	private bool IsIPAddressAvailable()
	{
		return base.NetworkAdapters.Any((VMNetworkAdapter a) => a.IPAddresses.Any());
	}

	private bool IsVMRebooted(ref TimeSpan lastUptime)
	{
		TimeSpan currentUptime = GetCurrentUptime();
		bool num = currentUptime < lastUptime;
		if (!num)
		{
			lastUptime = currentUptime;
		}
		return num;
	}

	private bool IsVMReadyForMemoryOperations()
	{
		return GetCurrentMemoryDemand() != 0;
	}

	internal bool IsUpgradable()
	{
		return GetComputerSystemAs<IVMComputerSystem>(UpdatePolicy.EnsureUpdated).IsUpgradable();
	}

	internal bool TryWaitCondition(WaitVMTypes waitType, TimeSpan timeout, ushort delay, ManualResetEventSlim waitHandle)
	{
		DateTime now = DateTime.Now;
		TimeSpan lastUptime = GetCurrentUptime();
		bool flag = false;
		do
		{
			flag = waitType switch
			{
				WaitVMTypes.Heartbeat => IsHeartbeatRunning(), 
				WaitVMTypes.IPAddress => IsIPAddressAvailable(), 
				WaitVMTypes.Reboot => IsVMRebooted(ref lastUptime), 
				WaitVMTypes.MemoryOperations => IsVMReadyForMemoryOperations(), 
				_ => throw new NotSupportedException(), 
			};
			if (!flag)
			{
				if (timeout >= TimeSpan.Zero && DateTime.Now - now >= timeout)
				{
					break;
				}
				waitHandle.Wait(delay * 1000);
			}
		}
		while (!flag && !waitHandle.IsSet);
		return flag;
	}

	internal IVMTask GetReplicationSendingInitialTask()
	{
		Predicate<IVMTask> selector = (IVMTask task) => task.JobType == 116;
		return GetReplicationTask(selector);
	}

	internal IVMTask GetReplicationStartTask()
	{
		Predicate<IVMTask> selector = (IVMTask task) => task.JobType == 94 || task.JobType == 117 || task.JobType == 118 || task.JobType == 95;
		return GetReplicationTask(selector);
	}

	internal IVMTask GetReplicationResyncTask()
	{
		Predicate<IVMTask> selector = (IVMTask task) => task.JobType == 107;
		return GetReplicationTask(selector);
	}

	internal IEnumerable<IVMTask> GetReplicationTasks()
	{
		return GetComputerSystemAs<IVMComputerSystem>(UpdatePolicy.None).Tasks?.Where((IVMTask task) => task.JobType >= 90 && task.JobType <= 129);
	}

	internal IVMTask GetReplicationUpdateTask()
	{
		Predicate<IVMTask> selector = (IVMTask task) => task.JobType == 125;
		return GetReplicationTask(selector);
	}

	private IVMTask GetReplicationTask(Predicate<IVMTask> selector)
	{
		return GetReplicationTasks()?.FirstOrDefault((IVMTask task) => selector(task));
	}

	internal static void RemoveDeviceSetting<TDeviceSetting>(TDeviceSetting deviceSetting, string taskDescription, VirtualizationObject targetObjectForOperation, IOperationWatcher operationWatcher) where TDeviceSetting : class, IVMDeviceSetting, IDeleteableAsync
	{
		operationWatcher.PerformDelete(deviceSetting, taskDescription, targetObjectForOperation);
	}

	private TDeviceSetting AddVMDevice<TDeviceSetting>(IAddableVMDevice<TDeviceSetting> templateDevice, IOperationWatcher operationWatcher) where TDeviceSetting : class, IVMDeviceSetting, IDeleteableAsync
	{
		TDeviceSetting val = null;
		TDeviceSetting val2 = null;
		val = templateDevice.GetDeviceSetting(UpdatePolicy.None);
		val2 = AddDeviceSetting(val, templateDevice.DescriptionForAdd, operationWatcher);
		templateDevice.FinishAddingDeviceSetting(val2);
		return val2;
	}

	private TComponentSetting AddComponent<TComponentSetting>(IHasAttachableComponent<TComponentSetting> templateDevice, IOperationWatcher operationWatcher) where TComponentSetting : class, IVMDeviceSetting, IDeleteableAsync
	{
		TComponentSetting componentSetting = templateDevice.GetComponentSetting(UpdatePolicy.None);
		TComponentSetting val = AddDeviceSetting(componentSetting, templateDevice.DescriptionForAttach, operationWatcher);
		templateDevice.FinishAttachingComponentSetting(val);
		return val;
	}

	private Tuple<TPrimary, TComponent> AddDeviceWithComponent<TPrimary, TComponent>(IAddableVMDevice<TPrimary, TComponent> templateDevice, IOperationWatcher operationWatcher) where TPrimary : class, IVMDeviceSetting, IDeleteableAsync where TComponent : class, IVMDeviceSetting, IDeleteableAsync
	{
		TPrimary val = AddVMDevice(templateDevice, operationWatcher);
		TComponent item = null;
		if (templateDevice.HasComponent())
		{
			try
			{
				item = AddComponent(templateDevice, operationWatcher);
			}
			catch
			{
				RemoveDeviceSetting(val, templateDevice.DescriptionForAddRollback, this, operationWatcher);
				throw;
			}
		}
		return Tuple.Create(val, item);
	}

	internal VMPmemController AddPmemController(VMPmemController templatePmemController, IOperationWatcher operationWatcher)
	{
		IVMPmemControllerSetting iVMPmemControllerSetting = AddVMDevice(templatePmemController, operationWatcher);
		return FindPmemControllerById(iVMPmemControllerSetting.DeviceId);
	}

	internal VMScsiController AddScsiController(VMScsiController templateScsiController, IOperationWatcher operationWatcher)
	{
		IVMScsiControllerSetting iVMScsiControllerSetting = AddVMDevice(templateScsiController, operationWatcher);
		return FindScsiControllerById(iVMScsiControllerSetting.DeviceId);
	}

	internal VMFibreChannelHba AddFibreChannelAdapter(VMFibreChannelHba templateAdapter, IOperationWatcher operationWatcher)
	{
		Tuple<IFibreChannelPortSetting, IFcPoolAllocationSetting> tuple = AddDeviceWithComponent(templateAdapter, operationWatcher);
		return new VMFibreChannelHba(tuple.Item1, tuple.Item2, this);
	}

	internal void MoveStorage(string destinationStoragePath, string vhdResourcePoolName, bool retainVhdCopiesOnSource, bool removeSourceUnmanagedVhds, bool allowUnverifiedPathsInCluster, IOperationWatcher operationWatcher)
	{
		IVMMigrationService vMMigrationService = ObjectLocator.GetVMMigrationService(base.Server);
		IVMMigrationSetting migrationSettings = null;
		IVMComputerSystemSetting virtualMachineSettings = null;
		List<IVirtualDiskSetting> newVhdSettings = null;
		PrepareMigrationParametersForSingleDestination(vMMigrationService, null, VMMigrationType.Storage, includeStorage: false, destinationStoragePath, vhdResourcePoolName, retainVhdCopiesOnSource, removeSourceUnmanagedVhds, allowUnverifiedPathsInCluster, operationWatcher, out migrationSettings, out virtualMachineSettings, out newVhdSettings);
		Migrate(vMMigrationService, null, migrationSettings, virtualMachineSettings, newVhdSettings, operationWatcher);
	}

	internal void MoveStorage(string virtualMachinePath, string snapshotFilePath, string smartPagingFilePath, IReadOnlyList<VhdMigrationMapping> vhdMigrationMappings, string vhdResourcePoolName, bool retainVhdCopiesOnSource, bool removeSourceUnmanagedVhds, bool allowUnverifiedPathsInCluster, IOperationWatcher operationWatcher)
	{
		IVMMigrationService vMMigrationService = ObjectLocator.GetVMMigrationService(base.Server);
		IVMMigrationSetting migrationSettings = null;
		IVMComputerSystemSetting virtualMachineSettings = null;
		List<IVirtualDiskSetting> newVhdSettings = null;
		PrepareMigrationParametersForMultipleDestinations(vMMigrationService, null, VMMigrationType.Storage, virtualMachinePath, snapshotFilePath, smartPagingFilePath, vhdMigrationMappings, vhdResourcePoolName, retainVhdCopiesOnSource, removeSourceUnmanagedVhds, allowUnverifiedPathsInCluster, out migrationSettings, out virtualMachineSettings, out newVhdSettings);
		Migrate(vMMigrationService, null, migrationSettings, virtualMachineSettings, newVhdSettings, operationWatcher);
	}

	internal void MoveTo(Server destinationServer, bool includeStorage, string destinationStoragePath, string vhdResourcePoolName, bool retainVhdCopiesOnSource, bool removeSourceUnmanagedVhds, IOperationWatcher operationWatcher)
	{
		includeStorage |= !string.IsNullOrEmpty(destinationStoragePath);
		IVMMigrationService vMMigrationService = ObjectLocator.GetVMMigrationService(base.Server);
		IVMMigrationSetting migrationSettings = null;
		IVMComputerSystemSetting virtualMachineSettings = null;
		List<IVirtualDiskSetting> newVhdSettings = null;
		PrepareMigrationParametersForSingleDestination(vMMigrationService, destinationServer, includeStorage ? VMMigrationType.VirtualSystemAndStorage : VMMigrationType.VirtualSystem, includeStorage, destinationStoragePath, vhdResourcePoolName, retainVhdCopiesOnSource, removeSourceUnmanagedVhds, allowUnverifiedPathsInCluster: false, operationWatcher, out migrationSettings, out virtualMachineSettings, out newVhdSettings);
		Migrate(vMMigrationService, destinationServer, migrationSettings, virtualMachineSettings, newVhdSettings, operationWatcher);
	}

	internal void MoveTo(Server destinationServer, string virtualMachinePath, string snapshotFilePath, string smartPagingFilePath, IReadOnlyList<VhdMigrationMapping> vhdMigrationMappings, string vhdResourcePoolName, bool retainVhdCopiesOnSource, bool removeSourceUnmanagedVhds, IOperationWatcher operationWatcher)
	{
		bool flag = !string.IsNullOrEmpty(virtualMachinePath) || !string.IsNullOrEmpty(snapshotFilePath) || !string.IsNullOrEmpty(smartPagingFilePath) || !vhdMigrationMappings.IsNullOrEmpty();
		if (flag && base.IsClustered)
		{
			throw ExceptionHelper.CreateOperationFailedException(ErrorMessages.OperationFailed_NotSupportedInClusteredVM);
		}
		IVMMigrationService vMMigrationService = ObjectLocator.GetVMMigrationService(base.Server);
		IVMMigrationSetting migrationSettings = null;
		IVMComputerSystemSetting virtualMachineSettings = null;
		List<IVirtualDiskSetting> newVhdSettings = null;
		PrepareMigrationParametersForMultipleDestinations(vMMigrationService, destinationServer, flag ? VMMigrationType.VirtualSystemAndStorage : VMMigrationType.VirtualSystem, virtualMachinePath, snapshotFilePath, smartPagingFilePath, vhdMigrationMappings, vhdResourcePoolName, retainVhdCopiesOnSource, removeSourceUnmanagedVhds, allowUnverifiedPathsInCluster: false, out migrationSettings, out virtualMachineSettings, out newVhdSettings);
		Migrate(vMMigrationService, destinationServer, migrationSettings, virtualMachineSettings, newVhdSettings, operationWatcher);
	}

	internal void MoveTo(VMCompatibilityReport compatibilityReport, bool retainVhdCopiesOnSource, bool removeSourceUnmanagedVhds, IOperationWatcher operationWatcher)
	{
		IVMMigrationService vMMigrationService = ObjectLocator.GetVMMigrationService(base.Server);
		VirtualMachine vM = compatibilityReport.VM;
		Server server = vM.Server;
		IVMMigrationSetting migrationSettings = null;
		IVMComputerSystemSetting virtualMachineSettings = null;
		List<IVirtualDiskSetting> newVhdSettings = null;
		List<MoveUnmanagedVhd> unmanagedVhdMappings = null;
		PrepareMigrationParametersForPlannedVm(vMMigrationService, server, vM, compatibilityReport.OperationType, compatibilityReport.VhdMigrationMappings, compatibilityReport.VhdResourcePoolName, retainVhdCopiesOnSource, removeSourceUnmanagedVhds, out migrationSettings, out virtualMachineSettings, out newVhdSettings, out unmanagedVhdMappings);
		Migrate(vMMigrationService, server, migrationSettings, virtualMachineSettings, newVhdSettings, operationWatcher);
	}

	internal VMCompatibilityReport CheckCompatibility(Server destinationServer, bool includeStorage, string destinationStoragePath, string vhdResourcePoolName, bool retainVhdCopiesOnSource, IOperationWatcher operationWatcher)
	{
		includeStorage |= !string.IsNullOrEmpty(destinationStoragePath);
		if (base.IsClustered && includeStorage)
		{
			throw ExceptionHelper.CreateOperationFailedException(ErrorMessages.OperationFailed_NotSupportedInClusteredVM);
		}
		IVMMigrationService vMMigrationService = ObjectLocator.GetVMMigrationService(base.Server);
		IVMMigrationSetting migrationSettings = null;
		IVMComputerSystemSetting virtualMachineSettings = null;
		List<IVirtualDiskSetting> newVhdSettings = null;
		PrepareMigrationParametersForSingleDestination(vMMigrationService, destinationServer, VMMigrationType.PlannedVirtualSystem, includeStorage, destinationStoragePath, vhdResourcePoolName, retainVhdCopiesOnSource, removeSourceUnmanagedVhds: false, allowUnverifiedPathsInCluster: false, operationWatcher, out migrationSettings, out virtualMachineSettings, out newVhdSettings);
		Migrate(vMMigrationService, destinationServer, migrationSettings, virtualMachineSettings, null, operationWatcher);
		return new VMCompatibilityReport(PrepareForCheckMigratabilityAndGetPlannedVM(destinationServer, migrationSettings, includeStorage), errors: CheckMigratability(vMMigrationService, destinationServer, migrationSettings, virtualMachineSettings, newVhdSettings, operationWatcher), operation: includeStorage ? OperationType.MoveVirtualMachineAndStorage : OperationType.MoveVirtualMachine, destination: destinationServer.UserSpecifiedName, path: null, snapshotPath: null, vhdDestinationPath: null, vhdSourcePath: null, vhdMigrationMappings: null, resourcePoolName: null);
	}

	internal VMCompatibilityReport CheckCompatibility(Server destinationServer, string virtualMachinePath, string snapshotFilePath, string smartPagingFilePath, IReadOnlyList<VhdMigrationMapping> vhdMigrationMappings, string vhdResourcePoolName, bool retainVhdCopiesOnSource, IOperationWatcher operationWatcher)
	{
		bool flag = !string.IsNullOrEmpty(virtualMachinePath) || !string.IsNullOrEmpty(snapshotFilePath) || !string.IsNullOrEmpty(smartPagingFilePath) || !vhdMigrationMappings.IsNullOrEmpty();
		if (flag && base.IsClustered)
		{
			throw ExceptionHelper.CreateOperationFailedException(ErrorMessages.OperationFailed_NotSupportedInClusteredVM);
		}
		IVMMigrationService vMMigrationService = ObjectLocator.GetVMMigrationService(base.Server);
		IVMMigrationSetting migrationSettings = null;
		IVMComputerSystemSetting virtualMachineSettings = null;
		List<IVirtualDiskSetting> newVhdSettings = null;
		PrepareMigrationParametersForMultipleDestinations(vMMigrationService, destinationServer, VMMigrationType.PlannedVirtualSystem, virtualMachinePath, snapshotFilePath, smartPagingFilePath, vhdMigrationMappings, vhdResourcePoolName, retainVhdCopiesOnSource, removeSourceUnmanagedVhds: false, allowUnverifiedPathsInCluster: false, out migrationSettings, out virtualMachineSettings, out newVhdSettings);
		Migrate(vMMigrationService, destinationServer, migrationSettings, virtualMachineSettings, null, operationWatcher);
		return new VMCompatibilityReport(PrepareForCheckMigratabilityAndGetPlannedVM(destinationServer, migrationSettings, flag), errors: CheckMigratability(vMMigrationService, destinationServer, migrationSettings, virtualMachineSettings, newVhdSettings, operationWatcher), operation: flag ? OperationType.MoveVirtualMachineAndStorage : OperationType.MoveVirtualMachine, destination: destinationServer.UserSpecifiedName, path: null, snapshotPath: null, vhdDestinationPath: null, vhdSourcePath: null, vhdMigrationMappings: vhdMigrationMappings, resourcePoolName: vhdResourcePoolName);
	}

	internal VMCompatibilityReport CheckCompatibility(VMCompatibilityReport compatibilityReport, bool retainVhdCopiesOnSource, bool removeSourceUnmanagedVhds, IOperationWatcher operationWatcher)
	{
		IVMMigrationService vMMigrationService = ObjectLocator.GetVMMigrationService(base.Server);
		VirtualMachine vM = compatibilityReport.VM;
		Server server = vM.Server;
		IVMMigrationSetting migrationSettings = null;
		IVMComputerSystemSetting virtualMachineSettings = null;
		List<IVirtualDiskSetting> newVhdSettings = null;
		List<MoveUnmanagedVhd> unmanagedVhdMappings = null;
		PrepareMigrationParametersForPlannedVm(vMMigrationService, server, vM, compatibilityReport.OperationType, compatibilityReport.VhdMigrationMappings, compatibilityReport.VhdResourcePoolName, retainVhdCopiesOnSource, removeSourceUnmanagedVhds, out migrationSettings, out virtualMachineSettings, out newVhdSettings, out unmanagedVhdMappings);
		IEnumerable<MsvmError> errors = CheckMigratability(vMMigrationService, server, migrationSettings, virtualMachineSettings, newVhdSettings, operationWatcher);
		return new VMCompatibilityReport(vM, compatibilityReport.OperationType, compatibilityReport.Destination, null, null, null, null, compatibilityReport.VhdMigrationMappings, compatibilityReport.VhdResourcePoolName, errors);
	}

	private static string GetTaskDescription(VMMigrationType migrationType)
	{
		switch (migrationType)
		{
		case VMMigrationType.VirtualSystem:
		case VMMigrationType.VirtualSystemAndStorage:
			return TaskDescriptions.MoveVM;
		case VMMigrationType.Storage:
			return TaskDescriptions.MoveVMStorage;
		case VMMigrationType.PlannedVirtualSystem:
			return TaskDescriptions.StageVM;
		default:
			throw new NotSupportedException();
		}
	}

	private void DeletePlannedVMAtDestination(Server destinationServer)
	{
		if (ObjectLocator.TryGetVMPlannedComputerSystem(destinationServer, base.Id.ToString(), out var plannedVM))
		{
			try
			{
				plannedVM.Delete();
			}
			catch (ServerObjectDeletedException)
			{
			}
			plannedVM.RemoveFromCache();
		}
	}

	private void Migrate(IVMMigrationService migrationService, Server destinationServer, IVMMigrationSetting migrationSettings, IVMComputerSystemSetting virtualMachineSettings, List<IVirtualDiskSetting> newVhdSettings, IOperationWatcher operationWatcher)
	{
		if (destinationServer != null && string.IsNullOrEmpty(migrationSettings.DestinationPlannedVirtualSystemId))
		{
			DeletePlannedVMAtDestination(destinationServer);
		}
		operationWatcher.PerformOperation(() => migrationService.BeginMigration(GetComputerSystemAs<IVMComputerSystem>(UpdatePolicy.None), (destinationServer != null) ? destinationServer.FullName : null, migrationSettings, virtualMachineSettings, newVhdSettings), migrationService.EndMigration, GetTaskDescription(migrationSettings.MigrationType), this);
		if (virtualMachineSettings != null)
		{
			virtualMachineSettings.InvalidatePropertyCache();
		}
		if (!newVhdSettings.IsNullOrEmpty())
		{
			foreach (IVirtualDiskSetting newVhdSetting in newVhdSettings)
			{
				newVhdSetting.InvalidatePropertyCache();
			}
		}
		if (destinationServer != null && !string.IsNullOrEmpty(migrationSettings.DestinationPlannedVirtualSystemId) && ObjectLocator.TryGetVMPlannedComputerSystem(destinationServer, base.Id.ToString(), out var plannedVM))
		{
			plannedVM.RemoveFromCache();
		}
	}

	private IEnumerable<MsvmError> CheckMigratability(IVMMigrationService migrationService, Server destinationServer, IVMMigrationSetting migrationSettings, IVMComputerSystemSetting virtualMachineSettings, List<IVirtualDiskSetting> newVhdSettings, IOperationWatcher operationWatcher)
	{
		if (string.IsNullOrEmpty(migrationSettings.DestinationPlannedVirtualSystemId))
		{
			DeletePlannedVMAtDestination(destinationServer);
		}
		IEnumerable<MsvmError> result = Enumerable.Empty<MsvmError>();
		using IVMTask iVMTask = migrationService.BeginCheckMigratability(GetComputerSystemAs<IVMComputerSystem>(UpdatePolicy.None), destinationServer.FullName, migrationSettings, virtualMachineSettings, newVhdSettings);
		WatchableTask.MonitorTask(iVMTask, TaskDescriptions.CompareVM, operationWatcher, this);
		try
		{
			migrationService.EndCheckMigratability(iVMTask);
			return result;
		}
		catch (VirtualizationOperationFailedException)
		{
			return iVMTask.GetErrors();
		}
	}

	private void PrepareMigrationParametersForSingleDestination(IVMMigrationService migrationService, Server destinationServer, VMMigrationType migrationType, bool includeStorage, string destinationStoragePath, string vhdResourcePoolName, bool retainVhdCopiesOnSource, bool removeSourceUnmanagedVhds, bool allowUnverifiedPathsInCluster, IOperationWatcher operationWatcher, out IVMMigrationSetting migrationSettings, out IVMComputerSystemSetting virtualMachineSettings, out List<IVirtualDiskSetting> newVhdSettings)
	{
		if (base.IsClustered)
		{
			switch (migrationType)
			{
			case VMMigrationType.Storage:
				if (!string.IsNullOrEmpty(destinationStoragePath))
				{
					ClusterUtilities.EnsureClusterPathValid(this, destinationStoragePath, allowUnverifiedPathsInCluster);
				}
				break;
			case VMMigrationType.VirtualSystem:
			case VMMigrationType.PlannedVirtualSystem:
			case VMMigrationType.VirtualSystemAndStorage:
				if (includeStorage || !string.IsNullOrEmpty(destinationStoragePath))
				{
					throw ExceptionHelper.CreateOperationFailedException(ErrorMessages.OperationFailed_NotSupportedInClusteredVM);
				}
				break;
			}
		}
		newVhdSettings = PrepareMigrationVirtualDiskSettingsForSingleLocation(includeStorage, destinationStoragePath, vhdResourcePoolName, operationWatcher);
		migrationSettings = PrepareMigrationSettings(migrationService, destinationServer, migrationType, retainVhdCopiesOnSource, removeSourceUnmanagedVhds, null);
		PrepareMigrationVMSettings(destinationStoragePath, destinationStoragePath, destinationStoragePath, out virtualMachineSettings);
	}

	private void PrepareMigrationParametersForMultipleDestinations(IVMMigrationService migrationService, Server destinationServer, VMMigrationType migrationType, string virtualMachinePath, string snapshotFilePath, string smartPagingFilePath, IReadOnlyList<VhdMigrationMapping> vhdMigrationMappings, string vhdResourcePoolName, bool retainVhdCopiesOnSource, bool removeSourceUnmanagedVhds, bool allowUnverifiedPathsInCluster, out IVMMigrationSetting migrationSettings, out IVMComputerSystemSetting virtualMachineSettings, out List<IVirtualDiskSetting> newVhdSettings)
	{
		if (base.IsClustered)
		{
			switch (migrationType)
			{
			case VMMigrationType.Storage:
				if (!string.IsNullOrEmpty(virtualMachinePath))
				{
					ClusterUtilities.EnsureClusterPathValid(this, virtualMachinePath, allowUnverifiedPathsInCluster);
				}
				if (!string.IsNullOrEmpty(snapshotFilePath))
				{
					ClusterUtilities.EnsureClusterPathValid(this, snapshotFilePath, allowUnverifiedPathsInCluster);
				}
				if (!string.IsNullOrEmpty(smartPagingFilePath))
				{
					ClusterUtilities.EnsureClusterPathValid(this, smartPagingFilePath, allowUnverifiedPathsInCluster);
				}
				if (vhdMigrationMappings.IsNullOrEmpty())
				{
					break;
				}
				foreach (VhdMigrationMapping vhdMigrationMapping in vhdMigrationMappings)
				{
					ClusterUtilities.EnsureClusterPathValid(this, vhdMigrationMapping.DestinationPath, allowUnverifiedPathsInCluster);
				}
				break;
			case VMMigrationType.PlannedVirtualSystem:
			case VMMigrationType.VirtualSystemAndStorage:
				if (!string.IsNullOrEmpty(virtualMachinePath) || !string.IsNullOrEmpty(snapshotFilePath) || !string.IsNullOrEmpty(smartPagingFilePath) || !vhdMigrationMappings.IsNullOrEmpty())
				{
					throw ExceptionHelper.CreateOperationFailedException(ErrorMessages.OperationFailed_NotSupportedInClusteredVM);
				}
				break;
			}
		}
		List<MoveUnmanagedVhd> unmanagedVhdMappings = null;
		PrepareMigrationVirtualDiskSettingsForMultipleDestinations(vhdMigrationMappings, vhdResourcePoolName, out newVhdSettings, out unmanagedVhdMappings);
		migrationSettings = PrepareMigrationSettings(migrationService, destinationServer, migrationType, retainVhdCopiesOnSource, removeSourceUnmanagedVhds, unmanagedVhdMappings);
		PrepareMigrationVMSettings(virtualMachinePath, snapshotFilePath, smartPagingFilePath, out virtualMachineSettings);
	}

	private static void PrepareMigrationParametersForPlannedVm(IVMMigrationService migrationService, Server destinationServer, VirtualMachine plannedVm, OperationType operationType, IReadOnlyList<VhdMigrationMapping> vhdMigrationMappings, string vhdResourcePoolName, bool retainVhdCopiesOnSource, bool removeSourceUnmanagedVhds, out IVMMigrationSetting migrationSettings, out IVMComputerSystemSetting virtualMachineSettings, out List<IVirtualDiskSetting> newVhdSettings, out List<MoveUnmanagedVhd> unmanagedVhdMappings)
	{
		VMMigrationType migrationType;
		if (operationType == OperationType.MoveVirtualMachine)
		{
			migrationType = VMMigrationType.VirtualSystem;
			newVhdSettings = null;
			unmanagedVhdMappings = null;
		}
		else
		{
			migrationType = VMMigrationType.VirtualSystemAndStorage;
			if (!vhdMigrationMappings.IsNullOrEmpty())
			{
				plannedVm.PrepareMigrationVirtualDiskSettingsForMultipleDestinations(vhdMigrationMappings, vhdResourcePoolName, out newVhdSettings, out unmanagedVhdMappings);
			}
			else
			{
				newVhdSettings = (from hardDiskDrive in plannedVm.GetVirtualHardDiskDrives()
					select hardDiskDrive.GetVirtualDiskSetting(UpdatePolicy.None)).ToList();
				unmanagedVhdMappings = null;
			}
		}
		migrationSettings = PrepareMigrationSettings(migrationService, destinationServer, migrationType, retainVhdCopiesOnSource, removeSourceUnmanagedVhds, unmanagedVhdMappings);
		migrationSettings.DestinationPlannedVirtualSystemId = plannedVm.Id.ToString();
		migrationSettings.MigrationType = migrationType;
		virtualMachineSettings = null;
	}

	private IEnumerable<HardDiskDrive> GetAllVirtualHardDiskDrives()
	{
		return GetVirtualHardDiskDrives().Concat(GetVMSnapshots().SelectMany((VMSnapshot snapshot) => snapshot.GetVirtualHardDiskDrives()));
	}

	private IDictionary<string, HardDiskDrive> GetAllVirtualHardDiskDrivesByPath()
	{
		return GetAllVirtualHardDiskDrives().ToDictionary((HardDiskDrive hardDiskDrive) => hardDiskDrive.Path, StringComparer.OrdinalIgnoreCase);
	}

	private bool IsHardDiskDriveMovable(HardDiskDrive drive, out string reason)
	{
		bool result = false;
		if (drive.SupportPersistentReservations.HasValue && drive.SupportPersistentReservations.Value)
		{
			reason = string.Format(CultureInfo.CurrentCulture, ErrorMessages.InvalidParameter_VHDIsSharable, drive.Path);
		}
		else if (!drive.IsMovable())
		{
			reason = string.Format(CultureInfo.CurrentCulture, ErrorMessages.InvalidParameter_VHDIsRDSUserProfileDisk, drive.Path);
		}
		else
		{
			reason = null;
			result = true;
		}
		return result;
	}

	private void EnsureHardDiskDriveIsMovable(HardDiskDrive drive)
	{
		if (!IsHardDiskDriveMovable(drive, out var reason))
		{
			throw ExceptionHelper.CreateInvalidArgumentException(reason);
		}
	}

	private void PrepareMigrationVirtualDiskSettingsForMultipleDestinations(IReadOnlyList<VhdMigrationMapping> vhdMigrationMappings, string vhdResourcePoolName, out List<IVirtualDiskSetting> newVhdSettings, out List<MoveUnmanagedVhd> unmanagedVhdMappings)
	{
		List<IVirtualDiskSetting> list = null;
		List<MoveUnmanagedVhd> list2 = null;
		if (!vhdMigrationMappings.IsNullOrEmpty())
		{
			IDictionary<string, HardDiskDrive> allVirtualHardDiskDrivesByPath = GetAllVirtualHardDiskDrivesByPath();
			list = new List<IVirtualDiskSetting>();
			list2 = new List<MoveUnmanagedVhd>();
			foreach (VhdMigrationMapping vhdMigrationMapping in vhdMigrationMappings)
			{
				HardDiskDrive value = null;
				if (!allVirtualHardDiskDrivesByPath.TryGetValue(vhdMigrationMapping.SourcePath, out value))
				{
					list2.Add(new MoveUnmanagedVhd(base.Server, vhdMigrationMapping.SourcePath, vhdMigrationMapping.DestinationPath));
					continue;
				}
				EnsureHardDiskDriveIsMovable(value);
				IVirtualDiskSetting virtualDiskSetting = value.GetVirtualDiskSetting(UpdatePolicy.None);
				virtualDiskSetting.Path = vhdMigrationMapping.DestinationPath;
				if (!string.IsNullOrEmpty(vhdResourcePoolName))
				{
					virtualDiskSetting.PoolId = ((!VMResourcePool.IsPrimordialPoolName(vhdResourcePoolName)) ? vhdResourcePoolName : string.Empty);
				}
				list.Add(virtualDiskSetting);
			}
		}
		newVhdSettings = list;
		unmanagedVhdMappings = list2;
	}

	private List<IVirtualDiskSetting> PrepareMigrationVirtualDiskSettingsForSingleLocation(bool includeStorage, string destinationStoragePath, string vhdResourcePoolName, IOperationWatcher operationWatcher)
	{
		string text = null;
		List<IVirtualDiskSetting> list = new List<IVirtualDiskSetting>();
		if (!string.IsNullOrEmpty(destinationStoragePath))
		{
			text = global::System.IO.Path.Combine(destinationStoragePath, "Virtual Hard Disks");
			if (string.IsNullOrEmpty(vhdResourcePoolName))
			{
				vhdResourcePoolName = "Primordial";
			}
		}
		else
		{
			if (!includeStorage)
			{
				return list;
			}
			vhdResourcePoolName = null;
		}
		foreach (HardDiskDrive allVirtualHardDiskDrife in GetAllVirtualHardDiskDrives())
		{
			if (!IsHardDiskDriveMovable(allVirtualHardDiskDrife, out var reason))
			{
				operationWatcher.WriteWarning(reason);
				continue;
			}
			IVirtualDiskSetting virtualDiskSetting = allVirtualHardDiskDrife.GetVirtualDiskSetting(UpdatePolicy.None);
			if (string.IsNullOrEmpty(text))
			{
				if (!string.IsNullOrEmpty(vhdResourcePoolName))
				{
					virtualDiskSetting.Path = string.Empty;
				}
			}
			else
			{
				string fileName = global::System.IO.Path.GetFileName(virtualDiskSetting.Path);
				if (fileName != null)
				{
					virtualDiskSetting.Path = global::System.IO.Path.Combine(text, fileName);
				}
			}
			if (vhdResourcePoolName != null)
			{
				virtualDiskSetting.PoolId = ((!VMResourcePool.IsPrimordialPoolName(vhdResourcePoolName)) ? vhdResourcePoolName : string.Empty);
			}
			list.Add(virtualDiskSetting);
		}
		return list;
	}

	private static IVMMigrationSetting PrepareMigrationSettings(IVMMigrationService migrationService, Server destinationServer, VMMigrationType migrationType, bool retainVhdCopiesOnSource, bool removeSourceUnmanagedVhds, List<MoveUnmanagedVhd> unmanagedVhdMappings)
	{
		IVMMigrationSetting migrationSetting = migrationService.GetMigrationSetting(migrationType);
		if (destinationServer != null)
		{
			IVMMigrationServiceSetting setting = migrationService.Setting;
			setting.UpdatePropertyCache();
			if (!setting.EnableVirtualSystemMigration)
			{
				throw ExceptionHelper.CreateOperationFailedException(ErrorMessages.InvalidOperation_SourceMigrationsAreDisabled);
			}
			IVMMigrationService vMMigrationService = ObjectLocator.GetVMMigrationService(destinationServer);
			IVMMigrationServiceSetting setting2 = vMMigrationService.Setting;
			setting2.UpdatePropertyCache();
			if (!setting2.EnableVirtualSystemMigration)
			{
				throw ExceptionHelper.CreateOperationFailedException(ErrorMessages.InvalidOperation_DestinationMigrationsAreDisabled);
			}
			vMMigrationService.UpdatePropertyCache();
			string[] migrationServiceListenerIPAddressList = vMMigrationService.GetMigrationServiceListenerIPAddressList();
			if (migrationServiceListenerIPAddressList.IsNullOrEmpty())
			{
				throw ExceptionHelper.CreateOperationFailedException(ErrorMessages.InvalidOperation_DestinationNetworksAreNotConfigured);
			}
			migrationSetting.DestinationIPAddressList = migrationServiceListenerIPAddressList;
			if (setting2.EnableSmbTransport && setting.EnableSmbTransport)
			{
				migrationSetting.TransportType = VMMigrationTransportType.SMB;
			}
		}
		migrationSetting.RetainVhdCopiesOnSource = retainVhdCopiesOnSource;
		migrationSetting.RemoveSourceUnmanagedVhds = removeSourceUnmanagedVhds;
		if (!unmanagedVhdMappings.IsNullOrEmpty())
		{
			migrationSetting.UnmanagedVhds = unmanagedVhdMappings.ToArray();
		}
		return migrationSetting;
	}

	private VirtualMachine PrepareForCheckMigratabilityAndGetPlannedVM(Server destinationServer, IVMMigrationSetting migrationSettings, bool includeStorage)
	{
		string text = base.Id.ToString();
		VirtualMachine result = new VirtualMachine(ObjectLocator.GetVMPlannedComputerSystem(destinationServer, text));
		migrationSettings.DestinationPlannedVirtualSystemId = text;
		migrationSettings.MigrationType = (includeStorage ? VMMigrationType.VirtualSystemAndStorage : VMMigrationType.VirtualSystem);
		return result;
	}

	private void PrepareMigrationVMSettings(string virtualMachinePath, string snapshotFilePath, string smartPagingFilePath, out IVMComputerSystemSetting virtualMachineSettings)
	{
		IVMComputerSystemSetting setting = GetComputerSystemAs<IVMComputerSystem>(UpdatePolicy.None).Setting;
		bool flag = false;
		if (!string.IsNullOrEmpty(virtualMachinePath) && !string.Equals(setting.ConfigurationDataRoot, virtualMachinePath, StringComparison.OrdinalIgnoreCase))
		{
			setting.ConfigurationDataRoot = virtualMachinePath;
			flag = true;
		}
		if (!string.IsNullOrEmpty(snapshotFilePath) && !string.Equals(setting.SnapshotDataRoot, snapshotFilePath, StringComparison.OrdinalIgnoreCase))
		{
			setting.SnapshotDataRoot = snapshotFilePath;
			flag = true;
		}
		if (!string.IsNullOrEmpty(smartPagingFilePath) && !string.Equals(setting.SwapFileDataRoot, smartPagingFilePath, StringComparison.OrdinalIgnoreCase))
		{
			setting.SwapFileDataRoot = smartPagingFilePath;
			flag = true;
		}
		virtualMachineSettings = (flag ? setting : null);
	}
}
