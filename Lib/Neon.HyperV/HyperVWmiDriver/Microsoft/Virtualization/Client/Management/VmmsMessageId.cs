namespace Microsoft.Virtualization.Client.Management;

internal enum VmmsMessageId
{
    Unknown = 0,
    ResyncSuspended = 33400,
    ReplicationCritical = 33402,
    ReplicationSuspended = 33404,
    InitialReplicationNotStarted = 33406,
    InitialReplicationNotFinished = 33408,
    ResyncPendingCompletion = 33410,
    FailedOver = 33412,
    RpoMissLimitCrossed = 33416,
    BrokerNotInstalled = 33418,
    NotInCluster = 33420,
    ResyncRequired = 33422,
    CorruptConfiguration = 33424,
    ErrorLimit30SecCrossed = 33426,
    ErrorLimit5MinCrossed = 33428,
    ErrorLimit15MinCrossed = 33430,
    WarningLimit30SecCrossed = 33432,
    WarningLimit5MinCrossed = 33434,
    WarningLimit15MinCrossed = 33436,
    WarningLimitPercentageCrossed = 33438,
    WarningLastAppliedTime30SecCrossed = 33440,
    WarningLastAppliedTime5MinCrossed = 33442,
    WarningLastAppliedTime15MinCrossed = 33444,
    WarningLastAppliedTimeDayCrossed = 33446
}
