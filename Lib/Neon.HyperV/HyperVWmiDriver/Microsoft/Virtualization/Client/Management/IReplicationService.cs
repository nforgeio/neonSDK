using System;
using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_ReplicationService")]
internal interface IReplicationService : IVirtualizationManagementObject
{
	ushort[] OperationalStatus { get; }

	string[] StatusDescriptions { get; }

	IFailoverReplicationServiceSetting Setting { get; }

	IEnumerable<IFailoverReplicationAuthorizationSetting> AuthorizationSettings { get; }

	IVMTask BeginStartReplication(IVMComputerSystem computerSystem, InitialReplicationType initialReplicationType, string initialReplicationShare, DateTime scheduledDateTime);

	void EndStartReplication(IVMTask task);

	IVMTask BeginImportInitialReplica(IVMComputerSystem computerSystem, string initialReplicationImportLocation);

	void EndImportInitialReplica(IVMTask task);

	IVMTask BeginRemoveReplicationRelationshipEx(IVMComputerSystem computerSystem, IVMReplicationRelationship replicationRelationship);

	void EndRemoveReplicationRelationshipEx(IVMTask task);

	IVMTask BeginCreateReplicationRelationship(IVMComputerSystem computerSystem, IVMReplicationSettingData replicationSettingData);

	void EndCreateReplicationRelationship(IVMTask task);

	IVMTask BeginReverseReplicationRelationship(IVMComputerSystem computerSystem, IVMReplicationSettingData replicationSettingData);

	void EndReverseReplicationRelationship(IVMTask task);

	IVMTask BeginChangeReplicationModeToPrimary(IVMComputerSystem computerSystem, IVMReplicationRelationship replicationRelationship);

	void EndChangeReplicationModeToPrimary(IVMTask task);

	IVMTask BeginResynchronizeReplication(IVMComputerSystem computerSystem, DateTime scheduledDateTime);

	void EndResynchronizeReplication(IVMTask task);

	IVMTask BeginResetReplicationStatisticsEx(IVMComputerSystem computerSystem, IVMReplicationRelationship replicationRelationship);

	void ResetReplicationStatisticsEx(IVMComputerSystem computerSystem, IVMReplicationRelationship replicationRelationship);

	void EndResetReplicationStatisticsEx(IVMTask task);

	string[] GetSystemCertificates();

	IVMTask BeginInitiateFailover(IVMComputerSystem computerSystem, IVMComputerSystemSetting snapshot);

	void EndInitiateFailover(IVMTask task);

	IVMTask BeginRevertFailover(IVMComputerSystem computerSystem);

	void EndRevertFailover(IVMTask task);

	IVMTask BeginCommitFailover(IVMComputerSystem computerSystem);

	void EndCommitFailover(IVMTask task);

	IVMTask BeginCreateTestVirtualSystem(IVMComputerSystem computerSystem, IVMComputerSystemSetting snapshot);

	IVMComputerSystem EndCreateTestVirtualSystem(IVMTask task, string instanceId);

	IVMTask BeginTestReplicationConnection(string recoveryConnectionPoint, ushort recoveryServerPortNumber, RecoveryAuthenticationType authenticationType, string certificateThumbPrint, bool bypassProxyServer);

	void EndTestReplicationConnection(IVMTask task);

	IVMTask BeginAddAuthorizationEntry(ReplicationAuthorizationEntry replicationAuthEntry);

	void AddAuthorizationEntry(ReplicationAuthorizationEntry replicationAuthEntry);

	void EndAddAuthorizationEntry(IVMTask task);

	void ModifyAuthorizationEntry(ReplicationAuthorizationEntry replicationAuthEntry);

	IVMTask BeginSetAuthorizationEntry(IVMComputerSystem computerSystem, ReplicationAuthorizationEntry replicationAuthEntry);

	void SetAuthorizationEntry(IVMComputerSystem computerSystem, ReplicationAuthorizationEntry replicationAuthEntry);

	void EndSetAuthorizationEntry(IVMTask task);

	void RemoveAuthorizationEntry(string allowedPrimaryHostSystem);
}
