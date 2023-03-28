using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Microsoft.HyperV.PowerShell.Common;

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
				resourceMan = new ResourceManager(WmiHelper.GetResourceName("Microsoft.HyperV.PowerShell.Objects.Common.ErrorMessages"), typeof(ErrorMessages).GetTypeInfo().Assembly);
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

	internal static string ArgumentOutOfRange_InvalidEnumValue => ResourceManager.GetString("ArgumentOutOfRange_InvalidEnumValue", resourceCulture);

	internal static string CannotFindDismountedDevice => ResourceManager.GetString("CannotFindDismountedDevice", resourceCulture);

	internal static string CannotFindVacantControllerLocation => ResourceManager.GetString("CannotFindVacantControllerLocation", resourceCulture);

	internal static string ClusterPath_AvailableStorage => ResourceManager.GetString("ClusterPath_AvailableStorage", resourceCulture);

	internal static string ClusterPath_ConfigurationUpdateCmdletNotInstalledOrLoaded => ResourceManager.GetString("ClusterPath_ConfigurationUpdateCmdletNotInstalledOrLoaded", resourceCulture);

	internal static string ClusterPath_FailedToRefreshConfiguration => ResourceManager.GetString("ClusterPath_FailedToRefreshConfiguration", resourceCulture);

	internal static string ClusterPath_FailedToVerify => ResourceManager.GetString("ClusterPath_FailedToVerify", resourceCulture);

	internal static string ClusterPath_NoClusterGroup => ResourceManager.GetString("ClusterPath_NoClusterGroup", resourceCulture);

	internal static string ClusterPath_NonCluster => ResourceManager.GetString("ClusterPath_NonCluster", resourceCulture);

	internal static string ClusterPath_NotInGroup => ResourceManager.GetString("ClusterPath_NotInGroup", resourceCulture);

	internal static string ClusterPath_NotValid => ResourceManager.GetString("ClusterPath_NotValid", resourceCulture);

	internal static string DeviceNaming_NotApply => ResourceManager.GetString("DeviceNaming_NotApply", resourceCulture);

	internal static string Import_InvalidFileName => ResourceManager.GetString("Import_InvalidFileName", resourceCulture);

	internal static string InvalidArgument_ReplicaBrokerInvalidPort => ResourceManager.GetString("InvalidArgument_ReplicaBrokerInvalidPort", resourceCulture);

	internal static string InvalidArgument_ReplicaNodeExpectingFqdn => ResourceManager.GetString("InvalidArgument_ReplicaNodeExpectingFqdn", resourceCulture);

	internal static string InvalidOperation_DestinationMigrationsAreDisabled => ResourceManager.GetString("InvalidOperation_DestinationMigrationsAreDisabled", resourceCulture);

	internal static string InvalidOperation_DestinationNetworksAreNotConfigured => ResourceManager.GetString("InvalidOperation_DestinationNetworksAreNotConfigured", resourceCulture);

	internal static string InvalidOperation_DownlevelManagementNotSupported => ResourceManager.GetString("InvalidOperation_DownlevelManagementNotSupported", resourceCulture);

	internal static string InvalidOperation_MetricsOnPlannedVMNotSupported => ResourceManager.GetString("InvalidOperation_MetricsOnPlannedVMNotSupported", resourceCulture);

	internal static string InvalidOperation_SetVMHostNoParameterProvided => ResourceManager.GetString("InvalidOperation_SetVMHostNoParameterProvided", resourceCulture);

	internal static string InvalidOperation_SourceMigrationsAreDisabled => ResourceManager.GetString("InvalidOperation_SourceMigrationsAreDisabled", resourceCulture);

	internal static string InvalidParameter_FileNameNotGuid => ResourceManager.GetString("InvalidParameter_FileNameNotGuid", resourceCulture);

	internal static string InvalidParameter_HashContainsInvalidKeys => ResourceManager.GetString("InvalidParameter_HashContainsInvalidKeys", resourceCulture);

	internal static string InvalidParameter_HashDoesNotContainDestination => ResourceManager.GetString("InvalidParameter_HashDoesNotContainDestination", resourceCulture);

	internal static string InvalidParameter_HashDoesNotContainSource => ResourceManager.GetString("InvalidParameter_HashDoesNotContainSource", resourceCulture);

	internal static string InvalidParameter_HashtableKeyIsNotString => ResourceManager.GetString("InvalidParameter_HashtableKeyIsNotString", resourceCulture);

	internal static string InvalidParameter_InvalidClusterObjectType => ResourceManager.GetString("InvalidParameter_InvalidClusterObjectType", resourceCulture);

	internal static string InvalidParameter_MatchesMultipleTemplates => ResourceManager.GetString("InvalidParameter_MatchesMultipleTemplates", resourceCulture);

	internal static string InvalidParameter_MatchesNoTemplates => ResourceManager.GetString("InvalidParameter_MatchesNoTemplates", resourceCulture);

	internal static string InvalidParameter_NoPropertyFound => ResourceManager.GetString("InvalidParameter_NoPropertyFound", resourceCulture);

	internal static string InvalidParameter_UnknownFormat => ResourceManager.GetString("InvalidParameter_UnknownFormat", resourceCulture);

	internal static string InvalidParameter_VHDIsRDSUserProfileDisk => ResourceManager.GetString("InvalidParameter_VHDIsRDSUserProfileDisk", resourceCulture);

	internal static string InvalidParameter_VHDIsSharable => ResourceManager.GetString("InvalidParameter_VHDIsSharable", resourceCulture);

	internal static string MoreThanOneSnapshotFound => ResourceManager.GetString("MoreThanOneSnapshotFound", resourceCulture);

	internal static string OperationFailed_InvalidState => ResourceManager.GetString("OperationFailed_InvalidState", resourceCulture);

	internal static string OperationFailed_NotSupportedInClusteredVM => ResourceManager.GetString("OperationFailed_NotSupportedInClusteredVM", resourceCulture);

	internal static string OperationFailed_RollbackFailed => ResourceManager.GetString("OperationFailed_RollbackFailed", resourceCulture);

	internal static string PacketDirect_DoesNotApply => ResourceManager.GetString("PacketDirect_DoesNotApply", resourceCulture);

	internal static string NumaAwarePlacement_DoesNotApply => ResourceManager.GetString("NumaAwarePlacement_DoesNotApply", resourceCulture);

	internal static string PhysicalDvdDrive_NotFound => ResourceManager.GetString("PhysicalDvdDrive_NotFound", resourceCulture);

	internal static string PhysicalHardDrive_NotFound => ResourceManager.GetString("PhysicalHardDrive_NotFound", resourceCulture);

	internal static string SnapshotNotFound => ResourceManager.GetString("SnapshotNotFound", resourceCulture);

	internal static string VHD_InvalidDiskNumberForVirtualHardDisk => ResourceManager.GetString("VHD_InvalidDiskNumberForVirtualHardDisk", resourceCulture);

	internal static string VHD_MountedDiskImageNotFound => ResourceManager.GetString("VHD_MountedDiskImageNotFound", resourceCulture);

	internal static string VMAssignableDevice_NotFound => ResourceManager.GetString("VMAssignableDevice_NotFound", resourceCulture);

	internal static string VMBios_Generation2NotSupported => ResourceManager.GetString("VMBios_Generation2NotSupported", resourceCulture);

	internal static string VMEthernetPool_MissingResource => ResourceManager.GetString("VMEthernetPool_MissingResource", resourceCulture);

	internal static string VMFibreChannelHba_WorldWideNameFormatError => ResourceManager.GetString("VMFibreChannelHba_WorldWideNameFormatError", resourceCulture);

	internal static string VMFirmware_Generation1NotSupported => ResourceManager.GetString("VMFirmware_Generation1NotSupported", resourceCulture);

	internal static string VMFloppyDiskDrive_Generation2NotSupported => ResourceManager.GetString("VMFloppyDiskDrive_Generation2NotSupported", resourceCulture);

	internal static string VMGpuPartitionAdapter_GpuPartitionPoolNotFound => ResourceManager.GetString("VMGpuPartitionAdapter_GpuPartitionPoolNotFound", resourceCulture);

	internal static string VMGpuPartitionAdapter_NotFound => ResourceManager.GetString("VMGpuPartitionAdapter_NotFound", resourceCulture);

	internal static string VMGroup_VMGroupType_IncorrectType => ResourceManager.GetString("VMGroup_VMGroupType_IncorrectType", resourceCulture);

	internal static string VMGroup_VMType_IncorrectType => ResourceManager.GetString("VMGroup_VMType_IncorrectType", resourceCulture);

	internal static string VMIdeController_Generation2NotSupported => ResourceManager.GetString("VMIdeController_Generation2NotSupported", resourceCulture);

	internal static string VMIntegrationService_IntegrationComponentNotFound => ResourceManager.GetString("VMIntegrationService_IntegrationComponentNotFound", resourceCulture);

	internal static string VMNetworkAdapter_ConnectionNotFound => ResourceManager.GetString("VMNetworkAdapter_ConnectionNotFound", resourceCulture);

	internal static string VMNotFoundById => ResourceManager.GetString("VMNotFoundById", resourceCulture);

	internal static string VMNotFoundByName => ResourceManager.GetString("VMNotFoundByName", resourceCulture);

	internal static string VMPciExpressResourcePool_MissingResource => ResourceManager.GetString("VMPciExpressResourcePool_MissingResource", resourceCulture);

	internal static string VMPmemController_NotFound => ResourceManager.GetString("VMPmemController_NotFound", resourceCulture);

	internal static string VMRemoteFx3DVideoAdapter_AdapterAlreadyExists => ResourceManager.GetString("VMRemoteFx3DVideoAdapter_AdapterAlreadyExists", resourceCulture);

	internal static string VMRemoteFx3DVideoAdapter_DeviceNotFound => ResourceManager.GetString("VMRemoteFx3DVideoAdapter_DeviceNotFound", resourceCulture);

	internal static string VMRemoteFx3DVideoAdapter_DevNotInstalledError => ResourceManager.GetString("VMRemoteFx3DVideoAdapter_DevNotInstalledError", resourceCulture);

	internal static string VMRemoteFx3DVideoAdapter_NotGpuCapable => ResourceManager.GetString("VMRemoteFx3DVideoAdapter_NotGpuCapable", resourceCulture);

	internal static string VMRemoteFx3DVideoAdapter_Synth3dVideoPoolNotFound => ResourceManager.GetString("VMRemoteFx3DVideoAdapter_Synth3dVideoPoolNotFound", resourceCulture);

	internal static string VMReplication_ActionNotApplicable_None => ResourceManager.GetString("VMReplication_ActionNotApplicable_None", resourceCulture);

	internal static string VMReplication_ActionNotApplicableOnPrimary => ResourceManager.GetString("VMReplication_ActionNotApplicableOnPrimary", resourceCulture);

	internal static string VMReplication_ActionNotApplicableOnReplica => ResourceManager.GetString("VMReplication_ActionNotApplicableOnReplica", resourceCulture);

	internal static string VMReplication_ActionNotApplicableOnTestReplica => ResourceManager.GetString("VMReplication_ActionNotApplicableOnTestReplica", resourceCulture);

	internal static string VMReplication_AuthorizationEntryNotFound => ResourceManager.GetString("VMReplication_AuthorizationEntryNotFound", resourceCulture);

	internal static string VMReplication_BrokerResourceNotFound => ResourceManager.GetString("VMReplication_BrokerResourceNotFound", resourceCulture);

	internal static string VMReplication_BrokerResourcePortMappingNotSpecified => ResourceManager.GetString("VMReplication_BrokerResourcePortMappingNotSpecified", resourceCulture);

	internal static string VMReplication_CapNameNotSpecified => ResourceManager.GetString("VMReplication_CapNameNotSpecified", resourceCulture);

	internal static string VMReplication_ClusterNameSpecified => ResourceManager.GetString("VMReplication_ClusterNameSpecified", resourceCulture);

	internal static string VMReplication_ClusterStorageError => ResourceManager.GetString("VMReplication_ClusterStorageError", resourceCulture);

	internal static string VMReplication_DomainShouldBeSpecified => ResourceManager.GetString("VMReplication_DomainShouldBeSpecified", resourceCulture);

	internal static string VMReplication_ExpectingFqdn => ResourceManager.GetString("VMReplication_ExpectingFqdn", resourceCulture);

	internal static string VMReplication_InvalidPortValue => ResourceManager.GetString("VMReplication_InvalidPortValue", resourceCulture);

	internal static string VMReplication_KerberosAndCertificatePortsShouldBeDifferent => ResourceManager.GetString("VMReplication_KerberosAndCertificatePortsShouldBeDifferent", resourceCulture);

	internal static string VMReplication_NodePortMappingNotSpecified => ResourceManager.GetString("VMReplication_NodePortMappingNotSpecified", resourceCulture);

	internal static string VMReplication_NoReplicatedDisk => ResourceManager.GetString("VMReplication_NoReplicatedDisk", resourceCulture);

	internal static string VMReplication_PortValueNotSpecified => ResourceManager.GetString("VMReplication_PortValueNotSpecified", resourceCulture);

	internal static string VMReplication_PrimaryBrokerNotEnabled => ResourceManager.GetString("VMReplication_PrimaryBrokerNotEnabled", resourceCulture);

	internal static string VMReplication_ReplicaBrokerNotEnabled => ResourceManager.GetString("VMReplication_ReplicaBrokerNotEnabled", resourceCulture);

	internal static string VMReplication_SameClusterError => ResourceManager.GetString("VMReplication_SameClusterError", resourceCulture);

	internal static string VMReplication_SameHostError => ResourceManager.GetString("VMReplication_SameHostError", resourceCulture);

	internal static string VMReplication_UniquePortsNotSpecified => ResourceManager.GetString("VMReplication_UniquePortsNotSpecified", resourceCulture);

	internal static string VMResourcePool_MissingResource => ResourceManager.GetString("VMResourcePool_MissingResource", resourceCulture);

	internal static string VMResourcePool_ParentPoolNotFound => ResourceManager.GetString("VMResourcePool_ParentPoolNotFound", resourceCulture);

	internal static string VMSan_InitiatorPortNotFound => ResourceManager.GetString("VMSan_InitiatorPortNotFound", resourceCulture);

	internal static string VMSan_MissingResource => ResourceManager.GetString("VMSan_MissingResource", resourceCulture);

	internal static string VMScsiController_NotFound => ResourceManager.GetString("VMScsiController_NotFound", resourceCulture);

	internal static string VMStorageResourcePool_MissingResource => ResourceManager.GetString("VMStorageResourcePool_MissingResource", resourceCulture);

	internal static string VMSwitch_ExternalPortNotFound => ResourceManager.GetString("VMSwitch_ExternalPortNotFound", resourceCulture);

	internal static string VMSwitch_IovSwitchCannotChangeSwitchType => ResourceManager.GetString("VMSwitch_IovSwitchCannotChangeSwitchType", resourceCulture);

	internal static string VMSwitch_NotFoundByName => ResourceManager.GetString("VMSwitch_NotFoundByName", resourceCulture);

	internal static string VMSwitch_NotFoundInPool => ResourceManager.GetString("VMSwitch_NotFoundInPool", resourceCulture);

	internal static string VMSwitchExtension_NotFound => ResourceManager.GetString("VMSwitchExtension_NotFound", resourceCulture);

	internal static string VMTPM_NotFound => ResourceManager.GetString("VMTPM_NotFound", resourceCulture);

	internal static string Warning_VMAlreadyInDesiredState => ResourceManager.GetString("Warning_VMAlreadyInDesiredState", resourceCulture);

	[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
	internal ErrorMessages()
	{
	}
}
