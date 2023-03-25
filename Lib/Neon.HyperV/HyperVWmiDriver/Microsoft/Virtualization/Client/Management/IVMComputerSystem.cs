using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_ComputerSystem")]
internal interface IVMComputerSystem : IVMComputerSystemBase, IVirtualizationManagementObject, IDeleteableAsync, IDeleteable, IMetricMeasurableElement
{
	FailoverReplicationMode ReplicationMode { get; }

	EnhancedSessionModeStateType EnhancedSessionModeState { get; }

	IEnumerable<IVMCollection> CollectingCollections { get; }

	IFailoverReplicationAuthorizationSetting ReplicationAuthorizationSetting { get; }

	IVMComputerSystem TestReplicaSystem { get; }

	[SuppressMessage("Microsoft.Design", "CA1009")]
	event SnapshotCreatedEventHandler SnapshotCreated;

	IVMTask BeginSetReplicationStateEx(IVMReplicationRelationship replicationRelationship, FailoverReplicationState state);

	void EndSetReplicationState(IVMTask task);

	IVMTask BeginTakeSnapshot();

	IVMTask BeginTakeSnapshot(bool takeAutomaticSnapshot);

	IVMComputerSystemSetting EndTakeSnapshot(IVMTask snapshotTask);

	IVMTask BeginInjectNonMaskableInterrupt();

	void EndInjectNonMaskableInterrupt(IVMTask injectNonMaskableInterruptTask);

	IVMTask BeginUpgrade();

	void EndUpgrade(IVMTask upgradeTask);

	ISummaryInformation GetVMSummaryInformation(SummaryInformationRequest requestedInformation = SummaryInformationRequest.Update);

	ReplicationHealthInformation GetVMReplicationStatisticsEx(IVMReplicationRelationship replicationRelationship);

	void RemoveKvpItem(string name, KvpItemPool pool);

	bool IsExtendedReplicationEnabled();

	bool IsSnapshotAvailable();

	bool IsProductionSnapshotAvailable();

	bool WasOnlineProductionSnapshot();

	bool IsUpgradable();

	IEnumerable<ITerminalConnection> GetTerminalConnections();

	bool DoesTerminalConnectionExist();

	IEnumerable<IVMMigrationTask> GetMigrationTasks();
}
