namespace Microsoft.Virtualization.Client.Management;

internal enum ServiceMethodError
{
    Success,
    NotSupported,
    AccessDenied,
    DependentServicesRunning,
    InvalidServiceControl,
    ServiceCannotAcceptControl,
    ServiceNotActive,
    ServiceRequestTimeout,
    UnknownFailure,
    PathNotFound,
    ServiceAlreadyRunning,
    ServiceDatabaseLocked,
    ServiceDependencyDeleted,
    ServiceDependencyFailure,
    ServiceDisabled,
    ServiceLogOnFailure,
    ServiceMarkedForDeletion,
    ServiceNoThread,
    StatusCircularDependency,
    StatusDuplicateName,
    StatusInvalidName,
    StatusInvalidParameter,
    StatusInvalidServiceAccount,
    StatusServiceExists,
    ServiceAlreadyPaused
}
