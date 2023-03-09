using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.HyperV.PowerShell.Common;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Virtualization.Client.Management;

using ErrorMessages = Microsoft.HyperV.PowerShell.Common.ErrorMessages;


namespace Microsoft.HyperV.PowerShell;

internal abstract class VMReplicationBase : VMComponentObject
{
	protected readonly IDataUpdater<IVMReplicationSettingData> m_ReplicationSetting;

	protected readonly IDataUpdater<IVMReplicationRelationship> m_ReplicationRelationship;

	protected readonly VirtualMachine m_Vm;

	public string AllowedPrimaryServer
	{
		get
		{
			IFailoverReplicationAuthorizationSetting replicationAuthorizationSetting = m_Vm.GetComputerSystemAs<IVMComputerSystem>(UpdatePolicy.EnsureUpdated).ReplicationAuthorizationSetting;
			if (replicationAuthorizationSetting == null)
			{
				return string.Empty;
			}
			return replicationAuthorizationSetting.AllowedPrimaryHostSystem;
		}
	}

	public string CurrentReplicaServerName => m_ReplicationSetting.GetData(UpdatePolicy.EnsureUpdated).RecoveryHostSystem;

	public DateTime? LastAppliedLogTime => m_ReplicationRelationship.GetData(UpdatePolicy.EnsureUpdated).LastApplyTime;

	public DateTime? LastReplicationTime => m_ReplicationRelationship.GetData(UpdatePolicy.EnsureUpdated).LastReplicationTime;

	public string PrimaryServerName
	{
		get
		{
			IVMReplicationSettingData data = m_ReplicationSetting.GetData(UpdatePolicy.EnsureUpdated);
			if (!string.IsNullOrEmpty(data.PrimaryHostSystem))
			{
				return data.PrimaryHostSystem;
			}
			return data.PrimaryConnectionPoint;
		}
	}

	public string ReplicaServerName
	{
		get
		{
			return m_ReplicationSetting.GetData(UpdatePolicy.EnsureUpdated).RecoveryConnectionPoint;
		}
		internal set
		{
			m_ReplicationSetting.GetData(UpdatePolicy.None).RecoveryConnectionPoint = value;
		}
	}

	public IList<HardDiskDrive> ReplicatedDisks
	{
		get
		{
			WmiObjectPath[] includedDisks = m_ReplicationSetting.GetData(UpdatePolicy.EnsureUpdated).IncludedDisks;
			if (!includedDisks.IsNullOrEmpty())
			{
				IEnumerable<string> replicatedDisksIds = includedDisks.Select((WmiObjectPath drivePath) => ((IVirtualDiskSetting)ObjectLocator.GetVirtualizationManagementObject(base.Server, drivePath)).DeviceId);
				return (from disk in m_Vm.GetVirtualHardDiskDrives()
					where replicatedDisksIds.Contains(disk.GetVirtualDiskSetting(UpdatePolicy.None).DeviceId, StringComparer.OrdinalIgnoreCase)
					select disk).ToList();
			}
			return new List<HardDiskDrive>();
		}
		internal set
		{
			IEnumerable<HardDiskDrive> virtualHardDiskDrives = m_Vm.GetVirtualHardDiskDrives();
			IEnumerable<string> passedInDiskIds = value.Select((HardDiskDrive includedDisk) => includedDisk.Id);
			HardDiskDrive[] array = virtualHardDiskDrives.Where((HardDiskDrive disk) => passedInDiskIds.Contains(disk.Id, StringComparer.OrdinalIgnoreCase)).ToArray();
			if (array.Length == 0)
			{
				throw ExceptionHelper.CreateInvalidOperationException(string.Format(CultureInfo.CurrentCulture, ErrorMessages.VMReplication_NoReplicatedDisk, m_Vm.Name), null, this);
			}
			m_ReplicationSetting.GetData(UpdatePolicy.None).IncludedDisks = array.Select((HardDiskDrive disk) => disk.m_AttachedDiskSetting.GetData(UpdatePolicy.None).ManagementPath).ToArray();
		}
	}

	public VMReplicationHealthState ReplicationHealth => (VMReplicationHealthState)m_ReplicationRelationship.GetData(UpdatePolicy.EnsureUpdated).ReplicationHealth;

	public VMReplicationMode ReplicationMode => m_Vm.ReplicationMode;

	public VMReplicationRelationshipType ReplicationRelationshipType { get; private set; }

	public VMReplicationState ReplicationState => ConvertFailoverReplicationStateToVMReplicationState(ReplicationMode, ReplicationWmiState, ReplicationRelationshipType == VMReplicationRelationshipType.Extended);

	public VirtualMachine TestVirtualMachine
	{
		get
		{
			IVMComputerSystem computerSystemAs = m_Vm.GetComputerSystemAs<IVMComputerSystem>(UpdatePolicy.EnsureUpdated);
			VMReplicationMode replicationMode = ReplicationMode;
			if (replicationMode == VMReplicationMode.Replica || replicationMode == VMReplicationMode.ExtendedReplica)
			{
				IVMComputerSystem testReplicaSystem = computerSystemAs.TestReplicaSystem;
				if (testReplicaSystem != null)
				{
					return new VirtualMachine(testReplicaSystem);
				}
			}
			return null;
		}
	}

	internal string PrimaryConnectionPoint => m_ReplicationSetting.GetData(UpdatePolicy.EnsureUpdated).PrimaryConnectionPoint;

	internal ReplicationWmiState ReplicationWmiState => (ReplicationWmiState)m_ReplicationRelationship.GetData(UpdatePolicy.EnsureUpdated).ReplicationState;

	internal VMReplicationBase(IVMReplicationSettingData replicationSetting, IVMReplicationRelationship replicationRelationship, VMReplicationRelationshipType relationshipType, VirtualMachine vm)
		: base(replicationSetting, vm)
	{
		m_ReplicationSetting = InitializePrimaryDataUpdater(replicationSetting);
		m_ReplicationRelationship = new DataUpdater<IVMReplicationRelationship>(replicationRelationship);
		ReplicationRelationshipType = relationshipType;
		m_Vm = vm;
	}

	internal IVMReplicationSettingData GetReplicationSetting(UpdatePolicy policy)
	{
		return m_ReplicationSetting.GetData(policy);
	}

	internal IVMReplicationRelationship GetReplicationRelationship(UpdatePolicy policy)
	{
		return m_ReplicationRelationship.GetData(policy);
	}

	internal static VMReplicationState ConvertFailoverReplicationStateToVMReplicationState(VMReplicationMode mode, ReplicationWmiState state, bool isExtended)
	{
		switch (mode)
		{
		case VMReplicationMode.Primary:
			switch (state)
			{
			case ReplicationWmiState.Ready:
				return VMReplicationState.ReadyForInitialReplication;
			case ReplicationWmiState.WaitingToCompleteInitialReplication:
				return VMReplicationState.InitialReplicationInProgress;
			case ReplicationWmiState.Replicating:
				return VMReplicationState.Replicating;
			case ReplicationWmiState.SyncedReplicationComplete:
				return VMReplicationState.PreparedForFailover;
			case ReplicationWmiState.Suspended:
				return VMReplicationState.Suspended;
			case ReplicationWmiState.Critical:
				return VMReplicationState.Error;
			case ReplicationWmiState.WaitingForStartResynchronize:
				return VMReplicationState.WaitingForStartResynchronize;
			case ReplicationWmiState.Resynchronizing:
				return VMReplicationState.Resynchronizing;
			case ReplicationWmiState.ResynchronizeSuspended:
				return VMReplicationState.ResynchronizeSuspended;
			case ReplicationWmiState.RecoveryInProgress:
				return VMReplicationState.RecoveryInProgress;
			case ReplicationWmiState.WaitingForUpdateCompletion:
				return VMReplicationState.WaitingForUpdateCompletion;
			case ReplicationWmiState.UpdateCritical:
				return VMReplicationState.UpdateError;
			case ReplicationWmiState.WaitingForRepurposeCompletion:
				return VMReplicationState.WaitingForRepurposeCompletion;
			case ReplicationWmiState.PreparedForSyncReplication:
				return VMReplicationState.PreparedForSyncReplication;
			case ReplicationWmiState.PreparedForGroupReverseReplication:
				return VMReplicationState.PreparedForGroupReverseReplication;
			}
			break;
		case VMReplicationMode.Replica:
		case VMReplicationMode.ExtendedReplica:
			switch (state)
			{
			case ReplicationWmiState.Ready:
				if (isExtended)
				{
					return VMReplicationState.ReadyForInitialReplication;
				}
				break;
			case ReplicationWmiState.WaitingToCompleteInitialReplication:
				if (!isExtended)
				{
					return VMReplicationState.WaitingForInitialReplication;
				}
				return VMReplicationState.InitialReplicationInProgress;
			case ReplicationWmiState.Replicating:
				return VMReplicationState.Replicating;
			case ReplicationWmiState.Recovered:
				return VMReplicationState.FailedOverWaitingCompletion;
			case ReplicationWmiState.Committed:
				return VMReplicationState.FailedOver;
			case ReplicationWmiState.Suspended:
				return VMReplicationState.Suspended;
			case ReplicationWmiState.Critical:
				return VMReplicationState.Error;
			case ReplicationWmiState.WaitingForStartResynchronize:
				if (isExtended)
				{
					return VMReplicationState.WaitingForStartResynchronize;
				}
				break;
			case ReplicationWmiState.Resynchronizing:
				return VMReplicationState.Resynchronizing;
			case ReplicationWmiState.ResynchronizeSuspended:
				return VMReplicationState.ResynchronizeSuspended;
			case ReplicationWmiState.RecoveryInProgress:
				return VMReplicationState.RecoveryInProgress;
			case ReplicationWmiState.FailbackInProgress:
				return VMReplicationState.FailbackInProgress;
			case ReplicationWmiState.FailbackComplete:
				return VMReplicationState.FailbackComplete;
			case ReplicationWmiState.WaitingForUpdateCompletion:
				return VMReplicationState.WaitingForUpdateCompletion;
			case ReplicationWmiState.UpdateCritical:
				return VMReplicationState.UpdateError;
			case ReplicationWmiState.WaitingForRepurposeCompletion:
				if (!isExtended)
				{
					return VMReplicationState.WaitingForRepurposeCompletion;
				}
				break;
			case ReplicationWmiState.FiredrillInProgress:
				return VMReplicationState.FiredrillInProgress;
			}
			break;
		}
		return VMReplicationState.Disabled;
	}
}
