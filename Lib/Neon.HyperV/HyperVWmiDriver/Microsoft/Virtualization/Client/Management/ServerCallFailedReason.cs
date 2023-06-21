namespace Microsoft.Virtualization.Client.Management;

internal enum ServerCallFailedReason
{
    Unknown,
    UnknownProviderError,
    ServerOutOfMemoryOrDiskSpace,
    RpcCallFailed,
    TimedOut,
    NotSupported
}
