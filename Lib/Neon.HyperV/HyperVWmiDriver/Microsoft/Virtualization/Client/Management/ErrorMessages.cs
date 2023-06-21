using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Microsoft.Virtualization.Client.Management;

[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
[DebuggerNonUserCode]
[CompilerGenerated]
internal class ErrorMessages
{
    private static ResourceManager resourceMan;

    private static CultureInfo resourceCulture;

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    internal static ResourceManager ResourceManager
    {
        get
        {
            if (resourceMan == null)
            {
                resourceMan = new ResourceManager(WmiHelper.GetResourceName("Microsoft.Virtualization.Client.Management.Exceptions.ErrorMessages"), typeof(ErrorMessages).GetTypeInfo().Assembly);
            }
            return resourceMan;
        }
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    internal static CultureInfo Culture
    {
        get
        {
            return resourceCulture;
        }
        set
        {
            resourceCulture = value;
        }
    }

    internal static string AddBootSourceFailed => ResourceManager.GetString("AddBootSourceFailed", resourceCulture);

    internal static string AddDeviceFailed => ResourceManager.GetString("AddDeviceFailed", resourceCulture);

    internal static string AddDeviceSucceededButObjectPathNotFound => ResourceManager.GetString("AddDeviceSucceededButObjectPathNotFound", resourceCulture);

    internal static string AddEthernetFeatureSettingsFailed => ResourceManager.GetString("AddEthernetFeatureSettingsFailed", resourceCulture);

    internal static string AddMigrationNetworkSettingsFailed => ResourceManager.GetString("AddMigrationNetworkSettingsFailed", resourceCulture);

    internal static string AddReplicationAuthSettingsFailed => ResourceManager.GetString("AddReplicationAuthSettingsFailed", resourceCulture);

    internal static string AddReplicationSettingsFailed => ResourceManager.GetString("AddReplicationSettingsFailed", resourceCulture);

    internal static string AddSwitchPortsFailed => ResourceManager.GetString("AddSwitchPortsFailed", resourceCulture);

    internal static string ApplyReplicaFailed => ResourceManager.GetString("ApplyReplicaFailed", resourceCulture);

    internal static string ApplySnapshotFailed => ResourceManager.GetString("ApplySnapshotFailed", resourceCulture);

    internal static string ArgumentOutOfRange_InvalidEnumValue => ResourceManager.GetString("ArgumentOutOfRange_InvalidEnumValue", resourceCulture);

    internal static string ArgumentOutOfRange_NoSupportedAssociations => ResourceManager.GetString("ArgumentOutOfRange_NoSupportedAssociations", resourceCulture);

    internal static string AttachVirtualHardDiskFailed => ResourceManager.GetString("AttachVirtualHardDiskFailed", resourceCulture);

    internal static string CalculateVideoMemoryRequirements => ResourceManager.GetString("CalculateVideoMemoryRequirements", resourceCulture);

    internal static string CancelTaskFailed => ResourceManager.GetString("CancelTaskFailed", resourceCulture);

    internal static string ChangeReplicationModeToPrimaryFailed => ResourceManager.GetString("ChangeReplicationModeToPrimaryFailed", resourceCulture);

    internal static string CommitReplicaFailed => ResourceManager.GetString("CommitReplicaFailed", resourceCulture);

    internal static string CompactVirtualHardDiskFailed => ResourceManager.GetString("CompactVirtualHardDiskFailed", resourceCulture);

    internal static string ConnectionIssue_AccessDenied => ResourceManager.GetString("ConnectionIssue_AccessDenied", resourceCulture);

    internal static string ConnectionIssue_AccessDeniedOnCallback => ResourceManager.GetString("ConnectionIssue_AccessDeniedOnCallback", resourceCulture);

    internal static string ConnectionIssue_ConnectedWithDifferentCredentials => ResourceManager.GetString("ConnectionIssue_ConnectedWithDifferentCredentials", resourceCulture);

    internal static string ConnectionIssue_CredSspNotConfiguredOnClient => ResourceManager.GetString("ConnectionIssue_CredSspNotConfiguredOnClient", resourceCulture);

    internal static string ConnectionIssue_InvalidClass => ResourceManager.GetString("ConnectionIssue_InvalidClass", resourceCulture);

    internal static string ConnectionIssue_NotInstalled => ResourceManager.GetString("ConnectionIssue_NotInstalled", resourceCulture);

    internal static string ConnectionIssue_ReallyUnknown => ResourceManager.GetString("ConnectionIssue_ReallyUnknown", resourceCulture);

    internal static string ConnectionIssue_RpcServerUnavailable => ResourceManager.GetString("ConnectionIssue_RpcServerUnavailable", resourceCulture);

    internal static string ConnectionIssue_RpcServerUnavailableOnCallback => ResourceManager.GetString("ConnectionIssue_RpcServerUnavailableOnCallback", resourceCulture);

    internal static string ConnectionIssue_ServerResolutionException => ResourceManager.GetString("ConnectionIssue_ServerResolutionException", resourceCulture);

    internal static string ConnectionIssue_Unknown => ResourceManager.GetString("ConnectionIssue_Unknown", resourceCulture);

    internal static string ConnectionIssue_UserCredentialsNotSupportedOnLocalhost => ResourceManager.GetString("ConnectionIssue_UserCredentialsNotSupportedOnLocalhost", resourceCulture);

    internal static string ControlMetricsByDefinitionFailed => ResourceManager.GetString("ControlMetricsByDefinitionFailed", resourceCulture);

    internal static string ControlMetricsFailed => ResourceManager.GetString("ControlMetricsFailed", resourceCulture);

    internal static string ConvertVirtualHardDiskFailed => ResourceManager.GetString("ConvertVirtualHardDiskFailed", resourceCulture);

    internal static string CopyDataFileFailed => ResourceManager.GetString("CopyDataFileFailed", resourceCulture);

    internal static string CopyFileToGuestFailed => ResourceManager.GetString("CopyFileToGuestFailed", resourceCulture);

    internal static string CreatePlannedSystemFailed => ResourceManager.GetString("CreatePlannedSystemFailed", resourceCulture);

    internal static string CreatePool => ResourceManager.GetString("CreatePool", resourceCulture);

    internal static string CreateSwitchFailed => ResourceManager.GetString("CreateSwitchFailed", resourceCulture);

    internal static string CreateTestSystemFailed => ResourceManager.GetString("CreateTestSystemFailed", resourceCulture);

    internal static string CreateVirtualDiskFailed => ResourceManager.GetString("CreateVirtualDiskFailed", resourceCulture);

    internal static string CreateVirtualSystemFailed => ResourceManager.GetString("CreateVirtualSystemFailed", resourceCulture);

    internal static string CreateVirtualSystemSuccededButObjectPathNotFound => ResourceManager.GetString("CreateVirtualSystemSuccededButObjectPathNotFound", resourceCulture);

    internal static string DataFileError_AccessDenied => ResourceManager.GetString("DataFileError_AccessDenied", resourceCulture);

    internal static string DataFileError_DirectoryNotEmpty => ResourceManager.GetString("DataFileError_DirectoryNotEmpty", resourceCulture);

    internal static string DataFileError_InvalidObject => ResourceManager.GetString("DataFileError_InvalidObject", resourceCulture);

    internal static string DataFileError_InvalidPath => ResourceManager.GetString("DataFileError_InvalidPath", resourceCulture);

    internal static string DataFileError_SharingViolation => ResourceManager.GetString("DataFileError_SharingViolation", resourceCulture);

    internal static string DeleteDataFileFailed => ResourceManager.GetString("DeleteDataFileFailed", resourceCulture);

    internal static string DeleteDeviceFailed => ResourceManager.GetString("DeleteDeviceFailed", resourceCulture);

    internal static string DeleteDirectoryFailed => ResourceManager.GetString("DeleteDirectoryFailed", resourceCulture);

    internal static string DeleteReplicationAuthSettingsFailed => ResourceManager.GetString("DeleteReplicationAuthSettingsFailed", resourceCulture);

    internal static string DeleteResourcePoolFailed => ResourceManager.GetString("DeleteResourcePoolFailed", resourceCulture);

    internal static string DeleteSwitchFailed => ResourceManager.GetString("DeleteSwitchFailed", resourceCulture);

    internal static string DeleteSwitchPortsFailed => ResourceManager.GetString("DeleteSwitchPortsFailed", resourceCulture);

    internal static string DeleteVHDSnapshotFailed => ResourceManager.GetString("DeleteVHDSnapshotFailed", resourceCulture);

    internal static string DeleteVirtualMachineFailed => ResourceManager.GetString("DeleteVirtualMachineFailed", resourceCulture);

    internal static string DeleteVirtualSystemSettingFailed => ResourceManager.GetString("DeleteVirtualSystemSettingFailed", resourceCulture);

    internal static string DeleteVirtualSystemSettingTreeFailed => ResourceManager.GetString("DeleteVirtualSystemSettingTreeFailed", resourceCulture);

    internal static string DetachVirtualHardDiskFailed => ResourceManager.GetString("DetachVirtualHardDiskFailed", resourceCulture);

    internal static string DisableGPUForVirtualization => ResourceManager.GetString("DisableGPUForVirtualization", resourceCulture);

    internal static string DismountAssignableDeviceFailed => ResourceManager.GetString("DismountAssignableDeviceFailed", resourceCulture);

    internal static string EnableGPUForVirtualization => ResourceManager.GetString("EnableGPUForVirtualization", resourceCulture);

    internal static string ExportComputerSystemFailed => ResourceManager.GetString("ExportComputerSystemFailed", resourceCulture);

    internal static string ExternalFcPortToSwitchConnectionError => ResourceManager.GetString("ExternalFcPortToSwitchConnectionError", resourceCulture);

    internal static string GeneratingWorldWideNamesFailed => ResourceManager.GetString("GeneratingWorldWideNamesFailed", resourceCulture);

    internal static string GetMountedStorageImageFailed => ResourceManager.GetString("GetMountedStorageImageFailed", resourceCulture);

    internal static string GetVHDSetInformationFailed => ResourceManager.GetString("GetVHDSetInformationFailed", resourceCulture);

    internal static string GetVHDSnapshotInformationFailed => ResourceManager.GetString("GetVHDSnapshotInformationFailed", resourceCulture);

    internal static string GetVirtualHardDiskInfoFailed => ResourceManager.GetString("GetVirtualHardDiskInfoFailed", resourceCulture);

    internal static string GetVirtualHardDiskInfoParsingError => ResourceManager.GetString("GetVirtualHardDiskInfoParsingError", resourceCulture);

    internal static string GetVMConnectAccessFailed => ResourceManager.GetString("GetVMConnectAccessFailed", resourceCulture);

    internal static string GetVMKeyProtectorFailed => ResourceManager.GetString("GetVMKeyProtectorFailed", resourceCulture);

    internal static string GrantVMConnectAccessFailed => ResourceManager.GetString("GrantVMConnectAccessFailed", resourceCulture);

    internal static string ImportComputerSystemFailed => ResourceManager.GetString("ImportComputerSystemFailed", resourceCulture);

    internal static string ImportComputerSystemSuccededButObjectPathNotFound => ResourceManager.GetString("ImportComputerSystemSuccededButObjectPathNotFound", resourceCulture);

    internal static string ImportReplicationFailed => ResourceManager.GetString("ImportReplicationFailed", resourceCulture);

    internal static string ImportSnapshotDefinitionsFailed => ResourceManager.GetString("ImportSnapshotDefinitionsFailed", resourceCulture);

    internal static string IncompatibleVersionInspectVhdDialog => ResourceManager.GetString("IncompatibleVersionInspectVhdDialog", resourceCulture);

    internal static string IncompatibleVersionVmBrowser => ResourceManager.GetString("IncompatibleVersionVmBrowser", resourceCulture);

    internal static string IncompatibleVersionVmConnect => ResourceManager.GetString("IncompatibleVersionVmConnect", resourceCulture);

    internal static string InjectNonMaskableInterruptFailed => ResourceManager.GetString("InjectNonMaskableInterruptFailed", resourceCulture);

    internal static string IntegrationComponentRebootFailed => ResourceManager.GetString("IntegrationComponentRebootFailed", resourceCulture);

    internal static string IntegrationComponentShutdownFailed => ResourceManager.GetString("IntegrationComponentShutdownFailed", resourceCulture);

    internal static string InvalidMethod => ResourceManager.GetString("InvalidMethod", resourceCulture);

    internal static string InvalidMethodReturnValue => ResourceManager.GetString("InvalidMethodReturnValue", resourceCulture);

    internal static string InvalidParameter_WmiObjectPathInvalid => ResourceManager.GetString("InvalidParameter_WmiObjectPathInvalid", resourceCulture);

    internal static string InvalidProperty => ResourceManager.GetString("InvalidProperty", resourceCulture);

    internal static string InvalidPropertyValue => ResourceManager.GetString("InvalidPropertyValue", resourceCulture);

    internal static string KDSUtilities_NewLocalKPNotSupportedInSHSMode => ResourceManager.GetString("KDSUtilities_NewLocalKPNotSupportedInSHSMode", resourceCulture);

    internal static string KeyboardDeviceFailed => ResourceManager.GetString("KeyboardDeviceFailed", resourceCulture);

    internal static string KeyboardTypeScanCodesFailed_TooManyCharacters => ResourceManager.GetString("KeyboardTypeScanCodesFailed_TooManyCharacters", resourceCulture);

    internal static string MergeVirtualDiskFailed => ResourceManager.GetString("MergeVirtualDiskFailed", resourceCulture);

    internal static string MigrationOperationFailed => ResourceManager.GetString("MigrationOperationFailed", resourceCulture);

    internal static string ModifyDeviceSettingFailed => ResourceManager.GetString("ModifyDeviceSettingFailed", resourceCulture);

    internal static string ModifyEthernetFeatureSettingsFailed => ResourceManager.GetString("ModifyEthernetFeatureSettingsFailed", resourceCulture);

    internal static string ModifyFailoverReplicationServiceSettingsFailed => ResourceManager.GetString("ModifyFailoverReplicationServiceSettingsFailed", resourceCulture);

    internal static string ModifyMigrationNetworkSettingsFailed => ResourceManager.GetString("ModifyMigrationNetworkSettingsFailed", resourceCulture);

    internal static string ModifyPoolFailed => ResourceManager.GetString("ModifyPoolFailed", resourceCulture);

    internal static string ModifyPoolResourcesFailed => ResourceManager.GetString("ModifyPoolResourcesFailed", resourceCulture);

    internal static string ModifyReplicationAuthSettingsFailed => ResourceManager.GetString("ModifyReplicationAuthSettingsFailed", resourceCulture);

    internal static string ModifyResourcePoolSettingsFailed => ResourceManager.GetString("ModifyResourcePoolSettingsFailed", resourceCulture);

    internal static string ModifySecuritySettingsFailed => ResourceManager.GetString("ModifySecuritySettingsFailed", resourceCulture);

    internal static string ModifyStorageSettingFailed => ResourceManager.GetString("ModifyStorageSettingFailed", resourceCulture);

    internal static string ModifySwitchPortsFailed => ResourceManager.GetString("ModifySwitchPortsFailed", resourceCulture);

    internal static string ModifyVirtualizationSettingsFailed => ResourceManager.GetString("ModifyVirtualizationSettingsFailed", resourceCulture);

    internal static string ModifyVirtualSwitchFailed => ResourceManager.GetString("ModifyVirtualSwitchFailed", resourceCulture);

    internal static string ModifyVirtualSystemSettingFailed => ResourceManager.GetString("ModifyVirtualSystemSettingFailed", resourceCulture);

    internal static string MountAssignableDeviceFailed => ResourceManager.GetString("MountAssignableDeviceFailed", resourceCulture);

    internal static string NetworkUtilities_NicNotFound => ResourceManager.GetString("NetworkUtilities_NicNotFound", resourceCulture);

    internal static string ObjectDeleted => ResourceManager.GetString("ObjectDeleted", resourceCulture);

    internal static string OperationFailed_AccessDenied => ResourceManager.GetString("OperationFailed_AccessDenied", resourceCulture);

    internal static string OperationFailed_ExpectedAffectedElementNotFound => ResourceManager.GetString("OperationFailed_ExpectedAffectedElementNotFound", resourceCulture);

    internal static string OperationFailed_FileNotFound => ResourceManager.GetString("OperationFailed_FileNotFound", resourceCulture);

    internal static string OperationFailed_IncorrectType => ResourceManager.GetString("OperationFailed_IncorrectType", resourceCulture);

    internal static string OperationFailed_InvalidParameter => ResourceManager.GetString("OperationFailed_InvalidParameter", resourceCulture);

    internal static string OperationFailed_InvalidState => ResourceManager.GetString("OperationFailed_InvalidState", resourceCulture);

    internal static string OperationFailed_NotSupported => ResourceManager.GetString("OperationFailed_NotSupported", resourceCulture);

    internal static string OperationFailed_ObjectNotFound => ResourceManager.GetString("OperationFailed_ObjectNotFound", resourceCulture);

    internal static string OperationFailed_OutOfMemory => ResourceManager.GetString("OperationFailed_OutOfMemory", resourceCulture);

    internal static string OperationFailed_StatusInUse => ResourceManager.GetString("OperationFailed_StatusInUse", resourceCulture);

    internal static string OperationFailed_TaskDeleted => ResourceManager.GetString("OperationFailed_TaskDeleted", resourceCulture);

    internal static string OperationFailed_TimedOut => ResourceManager.GetString("OperationFailed_TimedOut", resourceCulture);

    internal static string OperationFailed_Unavailable => ResourceManager.GetString("OperationFailed_Unavailable", resourceCulture);

    internal static string OperationFailed_UnknownErrorCode => ResourceManager.GetString("OperationFailed_UnknownErrorCode", resourceCulture);

    internal static string OptimizeVHDSet => ResourceManager.GetString("OptimizeVHDSet", resourceCulture);

    internal static string RealizePlannedComputerSystemFailed => ResourceManager.GetString("RealizePlannedComputerSystemFailed", resourceCulture);

    internal static string RebootRequired => ResourceManager.GetString("RebootRequired", resourceCulture);

    internal static string ReconnectParentDiskFailed => ResourceManager.GetString("ReconnectParentDiskFailed", resourceCulture);

    internal static string RemoteObjectNotFound => ResourceManager.GetString("RemoteObjectNotFound", resourceCulture);

    internal static string RemoveEthernetFeatureSettingsFailed => ResourceManager.GetString("RemoveEthernetFeatureSettingsFailed", resourceCulture);

    internal static string RemoveKvpItemsFailed => ResourceManager.GetString("RemoveKvpItemsFailed", resourceCulture);

    internal static string RemoveMigrationNetworkSettingsFailed => ResourceManager.GetString("RemoveMigrationNetworkSettingsFailed", resourceCulture);

    internal static string RemoveReplicationAuthSettingFailed => ResourceManager.GetString("RemoveReplicationAuthSettingFailed", resourceCulture);

    internal static string RemoveReplicationSettingsFailed => ResourceManager.GetString("RemoveReplicationSettingsFailed", resourceCulture);

    internal static string RequestReplicationStateChangeFailed => ResourceManager.GetString("RequestReplicationStateChangeFailed", resourceCulture);

    internal static string RequestSwitchExtensionStateChangeFailed => ResourceManager.GetString("RequestSwitchExtensionStateChangeFailed", resourceCulture);

    internal static string ResetReplicationStatisticsFailed => ResourceManager.GetString("ResetReplicationStatisticsFailed", resourceCulture);

    internal static string ResizeVirtualHardDiskFailed => ResourceManager.GetString("ResizeVirtualHardDiskFailed", resourceCulture);

    internal static string RestoreLKGVMKeyProtectorFailed => ResourceManager.GetString("RestoreLKGVMKeyProtectorFailed", resourceCulture);

    internal static string ResynchronizationFailed => ResourceManager.GetString("ResynchronizationFailed", resourceCulture);

    internal static string ReverseReplicationSettingsFailed => ResourceManager.GetString("ReverseReplicationSettingsFailed", resourceCulture);

    internal static string RevertReplicaFailed => ResourceManager.GetString("RevertReplicaFailed", resourceCulture);

    internal static string RevokeVMConnectAccessFailed => ResourceManager.GetString("RevokeVMConnectAccessFailed", resourceCulture);

    internal static string ServerCallFailed_OutOfMemoryOrDiskSpace => ResourceManager.GetString("ServerCallFailed_OutOfMemoryOrDiskSpace", resourceCulture);

    internal static string ServerCallFailed_ReallyUnknown => ResourceManager.GetString("ServerCallFailed_ReallyUnknown", resourceCulture);

    internal static string ServerCallFailed_RpcCallFailed => ResourceManager.GetString("ServerCallFailed_RpcCallFailed", resourceCulture);

    internal static string ServerCallFailed_TimedOut => ResourceManager.GetString("ServerCallFailed_TimedOut", resourceCulture);

    internal static string ServerCallFailed_Unknown => ResourceManager.GetString("ServerCallFailed_Unknown", resourceCulture);

    internal static string ServerCallFailed_UnknownProviderError => ResourceManager.GetString("ServerCallFailed_UnknownProviderError", resourceCulture);

    internal static string ServiceError_AccessDenied => ResourceManager.GetString("ServiceError_AccessDenied", resourceCulture);

    internal static string ServiceError_DependentServicesRunning => ResourceManager.GetString("ServiceError_DependentServicesRunning", resourceCulture);

    internal static string ServiceError_InvalidServiceControl => ResourceManager.GetString("ServiceError_InvalidServiceControl", resourceCulture);

    internal static string ServiceError_NotSupported => ResourceManager.GetString("ServiceError_NotSupported", resourceCulture);

    internal static string ServiceError_PathNotFound => ResourceManager.GetString("ServiceError_PathNotFound", resourceCulture);

    internal static string ServiceError_ServiceAlreadyPaused => ResourceManager.GetString("ServiceError_ServiceAlreadyPaused", resourceCulture);

    internal static string ServiceError_ServiceAlreadyRunning => ResourceManager.GetString("ServiceError_ServiceAlreadyRunning", resourceCulture);

    internal static string ServiceError_ServiceCannotAcceptControl => ResourceManager.GetString("ServiceError_ServiceCannotAcceptControl", resourceCulture);

    internal static string ServiceError_ServiceDatabaseLocked => ResourceManager.GetString("ServiceError_ServiceDatabaseLocked", resourceCulture);

    internal static string ServiceError_ServiceDependencyDeleted => ResourceManager.GetString("ServiceError_ServiceDependencyDeleted", resourceCulture);

    internal static string ServiceError_ServiceDependencyFailure => ResourceManager.GetString("ServiceError_ServiceDependencyFailure", resourceCulture);

    internal static string ServiceError_ServiceDisabled => ResourceManager.GetString("ServiceError_ServiceDisabled", resourceCulture);

    internal static string ServiceError_ServiceLogonFailure => ResourceManager.GetString("ServiceError_ServiceLogonFailure", resourceCulture);

    internal static string ServiceError_ServiceMarkedForDeletion => ResourceManager.GetString("ServiceError_ServiceMarkedForDeletion", resourceCulture);

    internal static string ServiceError_ServiceNotActive => ResourceManager.GetString("ServiceError_ServiceNotActive", resourceCulture);

    internal static string ServiceError_ServiceNoThread => ResourceManager.GetString("ServiceError_ServiceNoThread", resourceCulture);

    internal static string ServiceError_ServiceRequestTimeout => ResourceManager.GetString("ServiceError_ServiceRequestTimeout", resourceCulture);

    internal static string ServiceError_StatusCircularDependency => ResourceManager.GetString("ServiceError_StatusCircularDependency", resourceCulture);

    internal static string ServiceError_StatusDuplicateName => ResourceManager.GetString("ServiceError_StatusDuplicateName", resourceCulture);

    internal static string ServiceError_StatusInvalidName => ResourceManager.GetString("ServiceError_StatusInvalidName", resourceCulture);

    internal static string ServiceError_StatusInvalidParameter => ResourceManager.GetString("ServiceError_StatusInvalidParameter", resourceCulture);

    internal static string ServiceError_StatusInvalidServiceAccount => ResourceManager.GetString("ServiceError_StatusInvalidServiceAccount", resourceCulture);

    internal static string ServiceError_StatusServiceExists => ResourceManager.GetString("ServiceError_StatusServiceExists", resourceCulture);

    internal static string ServiceError_UnknownFailure => ResourceManager.GetString("ServiceError_UnknownFailure", resourceCulture);

    internal static string SetStateFailed => ResourceManager.GetString("SetStateFailed", resourceCulture);

    internal static string SetVHDSnapshotInformationFailed => ResourceManager.GetString("SetVHDSnapshotInformationFailed", resourceCulture);

    internal static string SetVirtualHardDiskSettingDataFailed => ResourceManager.GetString("SetVirtualHardDiskSettingDataFailed", resourceCulture);

    internal static string SetVMKeyProtectorFailed => ResourceManager.GetString("SetVMKeyProtectorFailed", resourceCulture);

    internal static string Shutdown_GenericFailure => ResourceManager.GetString("Shutdown_GenericFailure", resourceCulture);

    internal static string Shutdown_MachineLocked => ResourceManager.GetString("Shutdown_MachineLocked", resourceCulture);

    internal static string Shutdown_NotReady => ResourceManager.GetString("Shutdown_NotReady", resourceCulture);

    internal static string Shutdown_ShutdownInProgress => ResourceManager.GetString("Shutdown_ShutdownInProgress", resourceCulture);

    internal static string StartReplicationFailed => ResourceManager.GetString("StartReplicationFailed", resourceCulture);

    internal static string TakeSnapshotFailed => ResourceManager.GetString("TakeSnapshotFailed", resourceCulture);

    internal static string TaskFailed_NotConnected => ResourceManager.GetString("TaskFailed_NotConnected", resourceCulture);

    internal static string TaskFailed_ServiceNotRunning => ResourceManager.GetString("TaskFailed_ServiceNotRunning", resourceCulture);

    internal static string TestNetworkConnectivityFailed => ResourceManager.GetString("TestNetworkConnectivityFailed", resourceCulture);

    internal static string UpgradeVMConfigurationVersionFailed => ResourceManager.GetString("UpgradeVMConfigurationVersionFailed", resourceCulture);

    internal static string ValidatePersistentReservationSupportFailed => ResourceManager.GetString("ValidatePersistentReservationSupportFailed", resourceCulture);

    internal static string ValidatePlannedComputerSystemFailed => ResourceManager.GetString("ValidatePlannedComputerSystemFailed", resourceCulture);

    internal static string ValidateVirtualHardDiskFailed => ResourceManager.GetString("ValidateVirtualHardDiskFailed", resourceCulture);

    internal static string VirtualHardDiskChainBroken => ResourceManager.GetString("VirtualHardDiskChainBroken", resourceCulture);

    internal static string VirtualHardDiskIdMismatch => ResourceManager.GetString("VirtualHardDiskIdMismatch", resourceCulture);

    internal static string WmiObjectPath_CannotConvertClassRefToInstanceId => ResourceManager.GetString("WmiObjectPath_CannotConvertClassRefToInstanceId", resourceCulture);

    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
    internal ErrorMessages()
    {
    }
}
