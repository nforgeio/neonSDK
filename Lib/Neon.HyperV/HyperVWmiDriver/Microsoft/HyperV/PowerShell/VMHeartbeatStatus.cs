namespace Microsoft.HyperV.PowerShell;

internal enum VMHeartbeatStatus
{
    Unknown,
    Disabled,
    NoContact,
    Error,
    LostCommunication,
    OkApplicationsUnknown,
    OkApplicationsHealthy,
    OkApplicationsCritical,
    Paused
}
