using System;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_ReplicationSettingData")]
internal interface IVMReplicationSettingData : IVirtualizationManagementObject, IPutableAsync, IPutable
{
    [Key]
    string InstanceId { get; }

    FailoverReplicationAuthenticationType AuthenticationType { get; set; }

    bool BypassProxyServer { get; set; }

    bool CompressionEnabled { get; set; }

    string RecoveryConnectionPoint { get; set; }

    string RecoveryHostSystem { get; }

    string PrimaryConnectionPoint { get; }

    string PrimaryHostSystem { get; }

    int RecoveryServerPort { get; set; }

    string CertificateThumbPrint { get; set; }

    int ApplicationConsistentSnapshotInterval { get; set; }

    FailoverReplicationInterval ReplicationInterval { get; set; }

    int RecoveryHistory { get; set; }

    bool AutoResynchronizeEnabled { get; set; }

    TimeSpan AutoResynchronizeIntervalStart { get; set; }

    TimeSpan AutoResynchronizeIntervalEnd { get; set; }

    WmiObjectPath[] IncludedDisks { get; set; }

    bool EnableWriteOrderPreservationAcrossDisks { get; set; }

    string ReplicationProvider { get; set; }

    bool ReplicateHostKvpItems { get; set; }

    IVMComputerSystemBase VMComputerSystem { get; }

    IVMReplicationRelationship Relationship { get; }
}
