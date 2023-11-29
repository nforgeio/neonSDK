namespace Microsoft.Virtualization.Client.Management;

internal static class JobTypeId
{
    public const int StartReplicationJobTypeRange = 90;

    public const int StartReplicationOverNetwork = 94;

    public const int ImportReplication = 95;

    public const int InitiateFailover = 97;

    public const int InitiateSyncedReplication = 100;

    public const int SendingDelta = 105;

    public const int ReceivingDelta = 106;

    public const int Resynchronization = 107;

    public const int ApplyChangeLog = 108;

    public const int SendingInitialReplica = 116;

    public const int StartReplicationUsingBackup = 117;

    public const int StartReplicationExport = 118;

    public const int ExtendedResynchronization = 121;

    public const int ChangeReplicationModeToPrimary = 123;

    public const int UpdateDiskSet = 125;

    public const int EndReplicationJobTypeRange = 129;

    public const int FirstNetworkJobType = 130;

    public const int LastNetworkJobType = 139;

    public const int MigrateVm = 305;

    public const int MigrateVmAndStorage = 306;
}
