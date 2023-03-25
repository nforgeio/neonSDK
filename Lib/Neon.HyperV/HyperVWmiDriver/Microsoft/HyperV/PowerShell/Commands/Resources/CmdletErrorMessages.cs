using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Microsoft.HyperV.PowerShell.Commands.Resources;

[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
[DebuggerNonUserCode]
[CompilerGenerated]
internal class CmdletErrorMessages
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
				resourceMan = new ResourceManager(WmiHelper.GetResourceName("Microsoft.HyperV.PowerShell.Cmdlets.Resources.CmdletErrorMessages"), typeof(CmdletErrorMessages).GetTypeInfo().Assembly);
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

	internal static string AsJobAndPassthruBothPresent => ResourceManager.GetString("AsJobAndPassthruBothPresent", resourceCulture);

	internal static string ConvertVHD_DifferencingDiskMustHaveParentPath => ResourceManager.GetString("ConvertVHD_DifferencingDiskMustHaveParentPath", resourceCulture);

	internal static string ConvertVHD_NonDifferencingDiskCannotHaveParentPath => ResourceManager.GetString("ConvertVHD_NonDifferencingDiskCannotHaveParentPath", resourceCulture);

	internal static string Generation2_NoFloppySupport => ResourceManager.GetString("Generation2_NoFloppySupport", resourceCulture);

	internal static string Generation2_NoIDESupport => ResourceManager.GetString("Generation2_NoIDESupport", resourceCulture);

	internal static string Generation2_NoLegacyNetworkAdapterSupport => ResourceManager.GetString("Generation2_NoLegacyNetworkAdapterSupport", resourceCulture);

	internal static string Generation2_NoVhdFormatSupport => ResourceManager.GetString("Generation2_NoVhdFormatSupport", resourceCulture);

	internal static string GetVMIntegrationService_IntegrationComponentNotFound => ResourceManager.GetString("GetVMIntegrationService_IntegrationComponentNotFound", resourceCulture);

	internal static string GetVMMigrationNetwork_NoMigrationNetworksFound => ResourceManager.GetString("GetVMMigrationNetwork_NoMigrationNetworksFound", resourceCulture);

	internal static string GetVMResourcePool_NoneFound => ResourceManager.GetString("GetVMResourcePool_NoneFound", resourceCulture);

	internal static string GetVMSan_NoneFound => ResourceManager.GetString("GetVMSan_NoneFound", resourceCulture);

	internal static string ImportVM_FailedValidate => ResourceManager.GetString("ImportVM_FailedValidate", resourceCulture);

	internal static string InvalidArgument_ParameterNotProvided => ResourceManager.GetString("InvalidArgument_ParameterNotProvided", resourceCulture);

	internal static string InvalidArgument_ParameterNotRequired => ResourceManager.GetString("InvalidArgument_ParameterNotRequired", resourceCulture);

	internal static string InvalidArgument_ReplicationFrequencyNotValid => ResourceManager.GetString("InvalidArgument_ReplicationFrequencyNotValid", resourceCulture);

	internal static string InvalidArgument_SetVMMigrationNetwork => ResourceManager.GetString("InvalidArgument_SetVMMigrationNetwork", resourceCulture);

	internal static string InvalidOperation_NoSettingsChanged => ResourceManager.GetString("InvalidOperation_NoSettingsChanged", resourceCulture);

	internal static string InvalidOperation_NumaSpanningIsEnabled => ResourceManager.GetString("InvalidOperation_NumaSpanningIsEnabled", resourceCulture);

	internal static string InvalidOperation_ShieldingIsEnabled => ResourceManager.GetString("InvalidOperation_ShieldingIsEnabled", resourceCulture);

	internal static string InvalidParameter_AddExistingTeamMember => ResourceManager.GetString("InvalidParameter_AddExistingTeamMember", resourceCulture);

	internal static string InvalidParameter_AddNicToMultipleVMSwitches => ResourceManager.GetString("InvalidParameter_AddNicToMultipleVMSwitches", resourceCulture);

	internal static string InvalidParameter_AuthEntryAllServerNotAllowedWithExistingEntries => ResourceManager.GetString("InvalidParameter_AuthEntryAllServerNotAllowedWithExistingEntries", resourceCulture);

	internal static string InvalidParameter_AuthEntryCustomNotAllowedWithAllServer => ResourceManager.GetString("InvalidParameter_AuthEntryCustomNotAllowedWithAllServer", resourceCulture);

	internal static string InvalidParameter_CompatibilityReportMustBeForImport => ResourceManager.GetString("InvalidParameter_CompatibilityReportMustBeForImport", resourceCulture);

	internal static string InvalidParameter_ConflictingParameters => ResourceManager.GetString("InvalidParameter_ConflictingParameters", resourceCulture);

	internal static string InvalidParameter_DeviceNotBootable => ResourceManager.GetString("InvalidParameter_DeviceNotBootable", resourceCulture);

	internal static string InvalidParameter_EnableSoftwareRsc => ResourceManager.GetString("InvalidParameter_EnableSoftwareRsc", resourceCulture);

	internal static string InvalidParameter_HostvNic => ResourceManager.GetString("InvalidParameter_HostvNic", resourceCulture);

	internal static string InvalidParameter_IncompatibleWithPmem => ResourceManager.GetString("InvalidParameter_IncompatibleWithPmem", resourceCulture);

	internal static string InvalidParameter_InvalidBandwidthReservationMode => ResourceManager.GetString("InvalidParameter_InvalidBandwidthReservationMode", resourceCulture);

	internal static string InvalidParameter_InvalidBandwidthReservationRange => ResourceManager.GetString("InvalidParameter_InvalidBandwidthReservationRange", resourceCulture);

	internal static string InvalidParameter_InvalidDirectoryPath => ResourceManager.GetString("InvalidParameter_InvalidDirectoryPath", resourceCulture);

	internal static string InvalidParameter_InvalidDiskNumberForDiskObject => ResourceManager.GetString("InvalidParameter_InvalidDiskNumberForDiskObject", resourceCulture);

	internal static string InvalidParameter_InvalidFormat => ResourceManager.GetString("InvalidParameter_InvalidFormat", resourceCulture);

	internal static string InvalidParameter_InvalidFullPath => ResourceManager.GetString("InvalidParameter_InvalidFullPath", resourceCulture);

	internal static string InvalidParameter_InvalidIovWeightRange => ResourceManager.GetString("InvalidParameter_InvalidIovWeightRange", resourceCulture);

	internal static string InvalidParameter_InvalidRdmaWeightRange => ResourceManager.GetString("InvalidParameter_InvalidRdmaWeightRange", resourceCulture);

	internal static string InvalidParameter_InvalidUncPath => ResourceManager.GetString("InvalidParameter_InvalidUncPath", resourceCulture);

	internal static string InvalidParameter_InvalidUri => ResourceManager.GetString("InvalidParameter_InvalidUri", resourceCulture);

	internal static string InvalidParameter_InvalidVirtualAdapterName => ResourceManager.GetString("InvalidParameter_InvalidVirtualAdapterName", resourceCulture);

	internal static string InvalidParameter_IovQueuePairCountValue => ResourceManager.GetString("InvalidParameter_IovQueuePairCountValue", resourceCulture);

	internal static string InvalidParameter_MandatoryFeatureIdAndManagementOS => ResourceManager.GetString("InvalidParameter_MandatoryFeatureIdAndManagementOS", resourceCulture);

	internal static string InvalidParameter_MetricsDisabledForResourcePool => ResourceManager.GetString("InvalidParameter_MetricsDisabledForResourcePool", resourceCulture);

	internal static string InvalidParameter_MismatchedCredentials => ResourceManager.GetString("InvalidParameter_MismatchedCredentials", resourceCulture);

	internal static string InvalidParameter_MissingMacAddressParameter => ResourceManager.GetString("InvalidParameter_MissingMacAddressParameter", resourceCulture);

	internal static string InvalidParameter_MissingSenderOrReceiver => ResourceManager.GetString("InvalidParameter_MissingSenderOrReceiver", resourceCulture);

	internal static string InvalidParameter_NoPciExpressDeviceSpecified => ResourceManager.GetString("InvalidParameter_NoPciExpressDeviceSpecified", resourceCulture);

	internal static string InvalidParameter_OneOfMutuallyExclusiveParametersRequired => ResourceManager.GetString("InvalidParameter_OneOfMutuallyExclusiveParametersRequired", resourceCulture);

	internal static string InvalidParameter_OnlyStoragePoolCanBeCreatedWithPaths => ResourceManager.GetString("InvalidParameter_OnlyStoragePoolCanBeCreatedWithPaths", resourceCulture);

	internal static string InvalidParameter_OnlyValidParameterForThisParameter => ResourceManager.GetString("InvalidParameter_OnlyValidParameterForThisParameter", resourceCulture);

	internal static string InvalidParameter_ParameterOutOfRange => ResourceManager.GetString("InvalidParameter_ParameterOutOfRange", resourceCulture);

	internal static string InvalidParameter_ParameterRequiresParameter => ResourceManager.GetString("InvalidParameter_ParameterRequiresParameter", resourceCulture);

	internal static string InvalidParameter_ParametersAreMutuallyExclusive => ResourceManager.GetString("InvalidParameter_ParametersAreMutuallyExclusive", resourceCulture);

	internal static string InvalidParameter_ParametersUnequalLength => ResourceManager.GetString("InvalidParameter_ParametersUnequalLength", resourceCulture);

	internal static string InvalidParameter_PathCannotBeNull => ResourceManager.GetString("InvalidParameter_PathCannotBeNull", resourceCulture);

	internal static string InvalidParameter_PhysicalAndVirtualDiskSettingsBothProvided => ResourceManager.GetString("InvalidParameter_PhysicalAndVirtualDiskSettingsBothProvided", resourceCulture);

	internal static string InvalidParameter_RemoveAllTeamMember => ResourceManager.GetString("InvalidParameter_RemoveAllTeamMember", resourceCulture);

	internal static string InvalidParameter_RemoveNonExistingTeamMember => ResourceManager.GetString("InvalidParameter_RemoveNonExistingTeamMember", resourceCulture);

	internal static string InvalidParameter_RequiresVhdxExtension => ResourceManager.GetString("InvalidParameter_RequiresVhdxExtension", resourceCulture);

	internal static string InvalidParameter_ServerDoesntSupportV3 => ResourceManager.GetString("InvalidParameter_ServerDoesntSupportV3", resourceCulture);

	internal static string InvalidParameter_SetRoutingDomainCriteriaMissing => ResourceManager.GetString("InvalidParameter_SetRoutingDomainCriteriaMissing", resourceCulture);

	internal static string InvalidParameter_SnapshotIDOnlyValidForVHDS => ResourceManager.GetString("InvalidParameter_SnapshotIDOnlyValidForVHDS", resourceCulture);

	internal static string InvalidParameter_SourceAndDestinationVhdNameMismatch => ResourceManager.GetString("InvalidParameter_SourceAndDestinationVhdNameMismatch", resourceCulture);

	internal static string InvalidParameter_StaticAndDynamicMacAddressBothPresent => ResourceManager.GetString("InvalidParameter_StaticAndDynamicMacAddressBothPresent", resourceCulture);

	internal static string InvalidParameter_StoragePoolMustBeCreatedAlone => ResourceManager.GetString("InvalidParameter_StoragePoolMustBeCreatedAlone", resourceCulture);

	internal static string InvalidParameter_StoragePoolMustBeCreatedWithPath => ResourceManager.GetString("InvalidParameter_StoragePoolMustBeCreatedWithPath", resourceCulture);

	internal static string InvalidParameter_UnableToResolveToSingleFile => ResourceManager.GetString("InvalidParameter_UnableToResolveToSingleFile", resourceCulture);

	internal static string InvalidParameter_VMReplicationServer_ListenerPortMappingShouldBeUsed => ResourceManager.GetString("InvalidParameter_VMReplicationServer_ListenerPortMappingShouldBeUsed", resourceCulture);

	internal static string InvalidParameter_VMSwitchIsNotTeamingEnabled => ResourceManager.GetString("InvalidParameter_VMSwitchIsNotTeamingEnabled", resourceCulture);

	internal static string InvalidParameter_VrssMaxQueuePairsCountValue => ResourceManager.GetString("InvalidParameter_VrssMaxQueuePairsCountValue", resourceCulture);

	internal static string InvalidParameter_VrssMinQueuePairsCountValue => ResourceManager.GetString("InvalidParameter_VrssMinQueuePairsCountValue", resourceCulture);

	internal static string InvalidParameter_VrssQueueSchedulingModeValue => ResourceManager.GetString("InvalidParameter_VrssQueueSchedulingModeValue", resourceCulture);

	internal static string InvalidParameter_VrssVmbusChannelAffinityPolicyValue => ResourceManager.GetString("InvalidParameter_VrssVmbusChannelAffinityPolicyValue", resourceCulture);

	internal static string MetricsDisabledForVM => ResourceManager.GetString("MetricsDisabledForVM", resourceCulture);

	internal static string MigrationNetworkUnacceptableLinkLocal => ResourceManager.GetString("MigrationNetworkUnacceptableLinkLocal", resourceCulture);

	internal static string MigrationNetworkUnacceptableLoopback => ResourceManager.GetString("MigrationNetworkUnacceptableLoopback", resourceCulture);

	internal static string NewVM_FailedToGetController => ResourceManager.GetString("NewVM_FailedToGetController", resourceCulture);

	internal static string NewVM_FailedToRemoveNewVhd => ResourceManager.GetString("NewVM_FailedToRemoveNewVhd", resourceCulture);

	internal static string NewVM_FailedToRollBack => ResourceManager.GetString("NewVM_FailedToRollBack", resourceCulture);

	internal static string NewVM_NoMatchingBootEntryFoundWarning => ResourceManager.GetString("NewVM_NoMatchingBootEntryFoundWarning", resourceCulture);

	internal static string NewVMKeyStorageDrive_CannotAddMultiple => ResourceManager.GetString("NewVMKeyStorageDrive_CannotAddMultiple", resourceCulture);

	internal static string NewVMSan_NamesMismatched => ResourceManager.GetString("NewVMSan_NamesMismatched", resourceCulture);

	internal static string NewVMSan_WorldWideNameFormatError => ResourceManager.GetString("NewVMSan_WorldWideNameFormatError", resourceCulture);

	internal static string NewVMSwitch_FailedToRollBack => ResourceManager.GetString("NewVMSwitch_FailedToRollBack", resourceCulture);

	internal static string NewVMSwitch_InvalidSwitchType => ResourceManager.GetString("NewVMSwitch_InvalidSwitchType", resourceCulture);

	internal static string NewVMSwitch_NonTeamingWithMultipleAdapters => ResourceManager.GetString("NewVMSwitch_NonTeamingWithMultipleAdapters", resourceCulture);

	internal static string NoDeviceFound => ResourceManager.GetString("NoDeviceFound", resourceCulture);

	internal static string NoParametersSpecified => ResourceManager.GetString("NoParametersSpecified", resourceCulture);

	internal static string NumaSpanningChangeNeedsVmmsReboot => ResourceManager.GetString("NumaSpanningChangeNeedsVmmsReboot", resourceCulture);

	internal static string OperationFailed_CannotEditMultipleSubnets => ResourceManager.GetString("OperationFailed_CannotEditMultipleSubnets", resourceCulture);

	internal static string OperationFailed_ConditionNotSatisfied => ResourceManager.GetString("OperationFailed_ConditionNotSatisfied", resourceCulture);

	internal static string OperationFailed_HostDoesNotSupportLM => ResourceManager.GetString("OperationFailed_HostDoesNotSupportLM", resourceCulture);

	internal static string OperationFailed_InitialReplicationStartAndAsJobTogether => ResourceManager.GetString("OperationFailed_InitialReplicationStartAndAsJobTogether", resourceCulture);

	internal static string OperationFailed_InvalidReplicationState => ResourceManager.GetString("OperationFailed_InvalidReplicationState", resourceCulture);

	internal static string OperationFailed_InvalidState => ResourceManager.GetString("OperationFailed_InvalidState", resourceCulture);

	internal static string OperationFailed_NotSupported => ResourceManager.GetString("OperationFailed_NotSupported", resourceCulture);

	internal static string OperationFailed_NotSupportedForExternalReplicationProvider => ResourceManager.GetString("OperationFailed_NotSupportedForExternalReplicationProvider", resourceCulture);

	internal static string OperationFailed_RestartTimeout => ResourceManager.GetString("OperationFailed_RestartTimeout", resourceCulture);

	internal static string OperationFailed_ShutdownICNotAvailableForReboot => ResourceManager.GetString("OperationFailed_ShutdownICNotAvailableForReboot", resourceCulture);

	internal static string OperationFailed_UnexpectedConnectionError => ResourceManager.GetString("OperationFailed_UnexpectedConnectionError", resourceCulture);

	internal static string RouteDomainMapping_NoneFound => ResourceManager.GetString("RouteDomainMapping_NoneFound", resourceCulture);

	internal static string SetVMFirmware_BootOrderAndFirstBootEntryBothProvided => ResourceManager.GetString("SetVMFirmware_BootOrderAndFirstBootEntryBothProvided", resourceCulture);

	internal static string SetVMFirmware_SecureBootTemplateAndSecureBootTemplateIdBothProvided => ResourceManager.GetString("SetVMFirmware_SecureBootTemplateAndSecureBootTemplateIdBothProvided", resourceCulture);

	internal static string SetVMFloppyDiskDrive_InvalidPath => ResourceManager.GetString("SetVMFloppyDiskDrive_InvalidPath", resourceCulture);

	internal static string SetVMMemory_ParameterMismatch => ResourceManager.GetString("SetVMMemory_ParameterMismatch", resourceCulture);

	internal static string SetVMMemory_SizeNotAligned => ResourceManager.GetString("SetVMMemory_SizeNotAligned", resourceCulture);

	internal static string SetVMNetworkAdapter_ParameterInvalidForManagementOS => ResourceManager.GetString("SetVMNetworkAdapter_ParameterInvalidForManagementOS", resourceCulture);

	internal static string SetVMNetworkAdapterVlan_InvalidVlanIdListFormat => ResourceManager.GetString("SetVMNetworkAdapterVlan_InvalidVlanIdListFormat", resourceCulture);

	internal static string SetVMSwitch_AllowManagementOSForExternalSwitchOnly => ResourceManager.GetString("SetVMSwitch_AllowManagementOSForExternalSwitchOnly", resourceCulture);

	internal static string SetVMSwitch_NoNetworkAdapterSpecified => ResourceManager.GetString("SetVMSwitch_NoNetworkAdapterSpecified", resourceCulture);

	internal static string SetVMSwitch_TeamingSwitchCannotChangeTypeAndExtNics => ResourceManager.GetString("SetVMSwitch_TeamingSwitchCannotChangeTypeAndExtNics", resourceCulture);

	internal static string StartVMTrace_CannotSetTraceFile => ResourceManager.GetString("StartVMTrace_CannotSetTraceFile", resourceCulture);

	internal static string StartVMTrace_DumpObjectsRequiresVerboseLevel => ResourceManager.GetString("StartVMTrace_DumpObjectsRequiresVerboseLevel", resourceCulture);

	internal static string StopVM_SaveAndTurnOffBothSpecified => ResourceManager.GetString("StopVM_SaveAndTurnOffBothSpecified", resourceCulture);

	internal static string StopVM_ShutdownComponentNotAvailable => ResourceManager.GetString("StopVM_ShutdownComponentNotAvailable", resourceCulture);

	internal static string SubnetAlreadyExists => ResourceManager.GetString("SubnetAlreadyExists", resourceCulture);

	internal static string UpdateVMConfigurationVersion_VMVersionMaximum_Warning => ResourceManager.GetString("UpdateVMConfigurationVersion_VMVersionMaximum_Warning", resourceCulture);

	internal static string VHD_CannotMergeFixedOrDynamicDisk => ResourceManager.GetString("VHD_CannotMergeFixedOrDynamicDisk", resourceCulture);

	internal static string VHD_InvalidVhdFilePath => ResourceManager.GetString("VHD_InvalidVhdFilePath", resourceCulture);

	internal static string VHD_SupportPRSwitchMustBeTrue => ResourceManager.GetString("VHD_SupportPRSwitchMustBeTrue", resourceCulture);

	internal static string VMDrive_NoPathSpecifiedWithPool => ResourceManager.GetString("VMDrive_NoPathSpecifiedWithPool", resourceCulture);

	internal static string VMDvdDrive_InvalidDvdDrivePath => ResourceManager.GetString("VMDvdDrive_InvalidDvdDrivePath", resourceCulture);

	internal static string VMFailover_InvalidSnapshot => ResourceManager.GetString("VMFailover_InvalidSnapshot", resourceCulture);

	internal static string VMFailover_OnlyValidParameterOnPrimary => ResourceManager.GetString("VMFailover_OnlyValidParameterOnPrimary", resourceCulture);

	internal static string VMFailover_TestVMNotFound => ResourceManager.GetString("VMFailover_TestVMNotFound", resourceCulture);

	internal static string VMFibreChannelHba_MultipleHbasFoundMatchingCriteria => ResourceManager.GetString("VMFibreChannelHba_MultipleHbasFoundMatchingCriteria", resourceCulture);

	internal static string VMFibreChannelHba_NoVirtualHbaFound => ResourceManager.GetString("VMFibreChannelHba_NoVirtualHbaFound", resourceCulture);

	internal static string VMFibreChannelHba_WorldWideNameDuplicateSetsError => ResourceManager.GetString("VMFibreChannelHba_WorldWideNameDuplicateSetsError", resourceCulture);

	internal static string VMGroupMember_MoreThanOneFound => ResourceManager.GetString("VMGroupMember_MoreThanOneFound", resourceCulture);

	internal static string VMGroupMember_NoneFound => ResourceManager.GetString("VMGroupMember_NoneFound", resourceCulture);

	internal static string VMHardDiskDrive_IncompatibleWithPersistentReservations => ResourceManager.GetString("VMHardDiskDrive_IncompatibleWithPersistentReservations", resourceCulture);

	internal static string VMHardDiskDrive_NoPathSpecified => ResourceManager.GetString("VMHardDiskDrive_NoPathSpecified", resourceCulture);

	internal static string VMHardDiskDrive_StorageQos_InvalidPolicyID => ResourceManager.GetString("VMHardDiskDrive_StorageQos_InvalidPolicyID", resourceCulture);

	internal static string VMHardDiskDrive_StorageQoS_InvalidPolicyInstance => ResourceManager.GetString("VMHardDiskDrive_StorageQoS_InvalidPolicyInstance", resourceCulture);

	internal static string VMHardDiskDrive_StorageQoS_MultiplePolicyParameters => ResourceManager.GetString("VMHardDiskDrive_StorageQoS_MultiplePolicyParameters", resourceCulture);

	internal static string VMHardDiskDrive_StorageQos_PassthroughDisksNotSupported => ResourceManager.GetString("VMHardDiskDrive_StorageQos_PassthroughDisksNotSupported", resourceCulture);

	internal static string VMHardDiskDrive_StorageQos_VirtualPMEMNotSupported => ResourceManager.GetString("VMHardDiskDrive_StorageQos_VirtualPMEMNotSupported", resourceCulture);

	internal static string VMHardDiskDrive_SupportsPersistentReservations_VirtualPMEMNotSupported => ResourceManager.GetString("VMHardDiskDrive_SupportsPersistentReservations_VirtualPMEMNotSupported", resourceCulture);

	internal static string VMHardDiskDrive_VirutalPMEM_CacheOverrideNotSupported => ResourceManager.GetString("VMHardDiskDrive_VirutalPMEM_CacheOverrideNotSupported", resourceCulture);

	internal static string VMHardDiskDrive_VirutalPMEM_PassthroughDisksNotSupported => ResourceManager.GetString("VMHardDiskDrive_VirutalPMEM_PassthroughDisksNotSupported", resourceCulture);

	internal static string VMMemoryParameterMixMatching => ResourceManager.GetString("VMMemoryParameterMixMatching", resourceCulture);

	internal static string VMNetworkAdapter_MoreThanOneFound => ResourceManager.GetString("VMNetworkAdapter_MoreThanOneFound", resourceCulture);

	internal static string VMNetworkAdapter_NoneFound => ResourceManager.GetString("VMNetworkAdapter_NoneFound", resourceCulture);

	internal static string VMNetworkAdapterAcl_MultipleAddressTypesSpecified => ResourceManager.GetString("VMNetworkAdapterAcl_MultipleAddressTypesSpecified", resourceCulture);

	internal static string VMNetworkAdapterAcl_NoAddressSpecified => ResourceManager.GetString("VMNetworkAdapterAcl_NoAddressSpecified", resourceCulture);

	internal static string VMNetworkAdapterFailoverConfiguration_IPSettingsSpecifiedWithoutAddress => ResourceManager.GetString("VMNetworkAdapterFailoverConfiguration_IPSettingsSpecifiedWithoutAddress", resourceCulture);

	internal static string VMNetworkAdapterFailoverConfiguration_NotSupportedAdapter => ResourceManager.GetString("VMNetworkAdapterFailoverConfiguration_NotSupportedAdapter", resourceCulture);

	internal static string VMNetworkAdapterRoutingDomainMapping_IsolationModeDoesNotSupportRoutingDomain => ResourceManager.GetString("VMNetworkAdapterRoutingDomainMapping_IsolationModeDoesNotSupportRoutingDomain", resourceCulture);

	internal static string VMReplication_ActionNotApplicableOnReplica => ResourceManager.GetString("VMReplication_ActionNotApplicableOnReplica", resourceCulture);

	internal static string VMReplication_ActionOnlyApplicableOnReplica => ResourceManager.GetString("VMReplication_ActionOnlyApplicableOnReplica", resourceCulture);

	internal static string VMReplication_AlreadyEnabled => ResourceManager.GetString("VMReplication_AlreadyEnabled", resourceCulture);

	internal static string VMReplication_AppConsistentWithoutRecoveryHistory => ResourceManager.GetString("VMReplication_AppConsistentWithoutRecoveryHistory", resourceCulture);

	internal static string VMReplication_AuthorizationEntryNotFound => ResourceManager.GetString("VMReplication_AuthorizationEntryNotFound", resourceCulture);

	internal static string VMReplication_CannotModifyIncludedVhdsOnReplica => ResourceManager.GetString("VMReplication_CannotModifyIncludedVhdsOnReplica", resourceCulture);

	internal static string VMReplication_CannotModifySettingsOnReplica => ResourceManager.GetString("VMReplication_CannotModifySettingsOnReplica", resourceCulture);

	internal static string VMReplication_HardDiskNotAttachedToVM => ResourceManager.GetString("VMReplication_HardDiskNotAttachedToVM", resourceCulture);

	internal static string VMReplication_InvalidExtendedReplicationFrequency => ResourceManager.GetString("VMReplication_InvalidExtendedReplicationFrequency", resourceCulture);

	internal static string VMReplication_InvalidValueForRequiredDependentParameter => ResourceManager.GetString("VMReplication_InvalidValueForRequiredDependentParameter", resourceCulture);

	internal static string VMReplication_NoRecord_If_State_Disabled => ResourceManager.GetString("VMReplication_NoRecord_If_State_Disabled", resourceCulture);

	internal static string VMReplication_NotEnabled => ResourceManager.GetString("VMReplication_NotEnabled", resourceCulture);

	internal static string VMReplication_Reverse_CertificateThumbprintNotProvided => ResourceManager.GetString("VMReplication_Reverse_CertificateThumbprintNotProvided", resourceCulture);

	internal static string VMReplication_StartTimeOccursInPast => ResourceManager.GetString("VMReplication_StartTimeOccursInPast", resourceCulture);

	internal static string VMReplication_StartTimeOccursTooMuchInFuture => ResourceManager.GetString("VMReplication_StartTimeOccursTooMuchInFuture", resourceCulture);

	internal static string VMReplication_UseBackup_ScheduledTime => ResourceManager.GetString("VMReplication_UseBackup_ScheduledTime", resourceCulture);

	internal static string VMReplicationAuthorizationEntry_NotFoundByServerName => ResourceManager.GetString("VMReplicationAuthorizationEntry_NotFoundByServerName", resourceCulture);

	internal static string VMReplicationAuthorizationEntry_NotFoundByTrustGroup => ResourceManager.GetString("VMReplicationAuthorizationEntry_NotFoundByTrustGroup", resourceCulture);

	internal static string VMSwitch_MoreThanOneFound => ResourceManager.GetString("VMSwitch_MoreThanOneFound", resourceCulture);

	internal static string VMSwitch_NoSwitchFound => ResourceManager.GetString("VMSwitch_NoSwitchFound", resourceCulture);

	internal static string VMSwitchExtensionFeature_CannotModifyOrRemoveTemplateFeature => ResourceManager.GetString("VMSwitchExtensionFeature_CannotModifyOrRemoveTemplateFeature", resourceCulture);

	internal static string VMSynth3dVideoAdapter_InvalidMonitorCount => ResourceManager.GetString("VMSynth3dVideoAdapter_InvalidMonitorCount", resourceCulture);

	internal static string VMSynth3dVideoAdapter_InvalidResolution => ResourceManager.GetString("VMSynth3dVideoAdapter_InvalidResolution", resourceCulture);

	internal static string VMSynth3dVideoAdapter_InvalidResolutionBasedOnMaping => ResourceManager.GetString("VMSynth3dVideoAdapter_InvalidResolutionBasedOnMaping", resourceCulture);

	internal static string VMSynth3dVideoAdapter_InvalidVramSize => ResourceManager.GetString("VMSynth3dVideoAdapter_InvalidVramSize", resourceCulture);

	internal static string VMSynth3dVideoAdapter_NotEnoughVramSize => ResourceManager.GetString("VMSynth3dVideoAdapter_NotEnoughVramSize", resourceCulture);

	internal static string VramSynth3dVideoAdapter_InvalidOperationOnVramSize => ResourceManager.GetString("VramSynth3dVideoAdapter_InvalidOperationOnVramSize", resourceCulture);

	internal static string Warning_CannotDeleteRootDiskForVHDSet => ResourceManager.GetString("Warning_CannotDeleteRootDiskForVHDSet", resourceCulture);

	internal static string Warning_CannotDeleteSourceDisk => ResourceManager.GetString("Warning_CannotDeleteSourceDisk", resourceCulture);

	internal static string Warning_CredSSP => ResourceManager.GetString("Warning_CredSSP", resourceCulture);

	internal static string Warning_LockOnDisconnectNotAffectRemotefx => ResourceManager.GetString("Warning_LockOnDisconnectNotAffectRemotefx", resourceCulture);

	internal static string Warning_MustSpecifyPathOrDebuggerMode => ResourceManager.GetString("Warning_MustSpecifyPathOrDebuggerMode", resourceCulture);

	internal static string Warning_VMPmemIsPrerelease => ResourceManager.GetString("Warning_VMPmemIsPrerelease", resourceCulture);

	[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
	internal CmdletErrorMessages()
	{
	}
}
