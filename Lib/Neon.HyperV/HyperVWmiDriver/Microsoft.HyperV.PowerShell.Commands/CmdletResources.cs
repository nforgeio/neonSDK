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
internal class CmdletResources
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
				resourceMan = new ResourceManager("Microsoft.HyperV.PowerShell.Cmdlets.Resources.CmdletResources", typeof(CmdletResources).GetTypeInfo().Assembly);
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

	internal static string ConfirmCaption => ResourceManager.GetString("ConfirmCaption", resourceCulture);

	internal static string DefaultVMName => ResourceManager.GetString("DefaultVMName", resourceCulture);

	internal static string ParameterNotImplemented => ResourceManager.GetString("ParameterNotImplemented", resourceCulture);

	internal static string RebootReason => ResourceManager.GetString("RebootReason", resourceCulture);

	internal static string ResourcePoolFullName => ResourceManager.GetString("ResourcePoolFullName", resourceCulture);

	internal static string ShouldContinue_AddVMHostAssignableDevice_ClientSKU => ResourceManager.GetString("ShouldContinue_AddVMHostAssignableDevice_ClientSKU", resourceCulture);

	internal static string ShouldContinue_DebugVM_InjectNMI => ResourceManager.GetString("ShouldContinue_DebugVM_InjectNMI", resourceCulture);

	internal static string ShouldContinue_DisableVMEventing => ResourceManager.GetString("ShouldContinue_DisableVMEventing", resourceCulture);

	internal static string ShouldContinue_EnableVMEventing => ResourceManager.GetString("ShouldContinue_EnableVMEventing", resourceCulture);

	internal static string ShouldContinue_MergeToImmediateParent => ResourceManager.GetString("ShouldContinue_MergeToImmediateParent", resourceCulture);

	internal static string ShouldContinue_NewPrereleaseVM => ResourceManager.GetString("ShouldContinue_NewPrereleaseVM", resourceCulture);

	internal static string ShouldContinue_RemoveVM => ResourceManager.GetString("ShouldContinue_RemoveVM", resourceCulture);

	internal static string ShouldContinue_RemoveVMGroup => ResourceManager.GetString("ShouldContinue_RemoveVMGroup", resourceCulture);

	internal static string ShouldContinue_RemoveVMHostAssignableDevice => ResourceManager.GetString("ShouldContinue_RemoveVMHostAssignableDevice", resourceCulture);

	internal static string ShouldContinue_RemoveVMResourcePool => ResourceManager.GetString("ShouldContinue_RemoveVMResourcePool", resourceCulture);

	internal static string ShouldContinue_RemoveVMSwitch => ResourceManager.GetString("ShouldContinue_RemoveVMSwitch", resourceCulture);

	internal static string ShouldContinue_RemoveVMSwitch_FromPool => ResourceManager.GetString("ShouldContinue_RemoveVMSwitch_FromPool", resourceCulture);

	internal static string ShouldContinue_RestartVM => ResourceManager.GetString("ShouldContinue_RestartVM", resourceCulture);

	internal static string ShouldContinue_SetVhdDiskId => ResourceManager.GetString("ShouldContinue_SetVhdDiskId", resourceCulture);

	internal static string ShouldContinue_SetVMReplication_RemoveAllAuthEntries => ResourceManager.GetString("ShouldContinue_SetVMReplication_RemoveAllAuthEntries", resourceCulture);

	internal static string ShouldContinue_StopVM_TurnOffOnShutDown => ResourceManager.GetString("ShouldContinue_StopVM_TurnOffOnShutDown", resourceCulture);

	internal static string ShouldProcess_AddVMAssignableDevice => ResourceManager.GetString("ShouldProcess_AddVMAssignableDevice", resourceCulture);

	internal static string ShouldProcess_AddVMDvdDrive => ResourceManager.GetString("ShouldProcess_AddVMDvdDrive", resourceCulture);

	internal static string ShouldProcess_AddVMFibreChannelHba => ResourceManager.GetString("ShouldProcess_AddVMFibreChannelHba", resourceCulture);

	internal static string ShouldProcess_AddVMGpuPartitionAdapter => ResourceManager.GetString("ShouldProcess_AddVMGpuPartitionAdapter", resourceCulture);

	internal static string ShouldProcess_AddVMGroupMember => ResourceManager.GetString("ShouldProcess_AddVMGroupMember", resourceCulture);

	internal static string ShouldProcess_AddVMHardDiskDrive => ResourceManager.GetString("ShouldProcess_AddVMHardDiskDrive", resourceCulture);

	internal static string ShouldProcess_AddVMHIDDevices => ResourceManager.GetString("ShouldProcess_AddVMHIDDevices", resourceCulture);

	internal static string ShouldProcess_AddVMHostAssignableDevice => ResourceManager.GetString("ShouldProcess_AddVMHostAssignableDevice", resourceCulture);

	internal static string ShouldProcess_AddVMKeyStorageDrive => ResourceManager.GetString("ShouldProcess_AddVMKeyStorageDrive", resourceCulture);

	internal static string ShouldProcess_AddVMMigrationNetwork => ResourceManager.GetString("ShouldProcess_AddVMMigrationNetwork", resourceCulture);

	internal static string ShouldProcess_AddVMNetworkAdapter_VM => ResourceManager.GetString("ShouldProcess_AddVMNetworkAdapter_VM", resourceCulture);

	internal static string ShouldProcess_AddVMNetworkAdapter_VMSwitch => ResourceManager.GetString("ShouldProcess_AddVMNetworkAdapter_VMSwitch", resourceCulture);

	internal static string ShouldProcess_AddVMNetworkAdapterAcl => ResourceManager.GetString("ShouldProcess_AddVMNetworkAdapterAcl", resourceCulture);

	internal static string ShouldProcess_AddVMNetworkAdapterExtendedAcl => ResourceManager.GetString("ShouldProcess_AddVMNetworkAdapterExtendedAcl", resourceCulture);

	internal static string ShouldProcess_AddVMNetworkAdapterRoutingDomainMapping => ResourceManager.GetString("ShouldProcess_AddVMNetworkAdapterRoutingDomainMapping", resourceCulture);

	internal static string RemoteFX_CmdletWarning => ResourceManager.GetString("RemoteFX_Cmdlet_Warning", resourceCulture);

	internal static string ShouldProcess_AddVMPmemController => ResourceManager.GetString("ShouldProcess_AddVMPmemController", resourceCulture);

	internal static string ShouldProcess_AddVMRemoteFx3dVideoAdapter => ResourceManager.GetString("ShouldProcess_AddVMRemoteFx3dVideoAdapter", resourceCulture);

	internal static string ShouldProcess_AddVMScsiController => ResourceManager.GetString("ShouldProcess_AddVMScsiController", resourceCulture);

	internal static string ShouldProcess_AddVMStoragePath => ResourceManager.GetString("ShouldProcess_AddVMStoragePath", resourceCulture);

	internal static string ShouldProcess_AddVMSwitchExtensionPortFeature => ResourceManager.GetString("ShouldProcess_AddVMSwitchExtensionPortFeature", resourceCulture);

	internal static string ShouldProcess_AddVMSwitchExtensionSwitchFeature => ResourceManager.GetString("ShouldProcess_AddVMSwitchExtensionSwitchFeature", resourceCulture);

	internal static string ShouldProcess_CheckpointVM => ResourceManager.GetString("ShouldProcess_CheckpointVM", resourceCulture);

	internal static string ShouldProcess_CompareVM => ResourceManager.GetString("ShouldProcess_CompareVM", resourceCulture);

	internal static string ShouldProcess_CompleteVMFailover => ResourceManager.GetString("ShouldProcess_CompleteVMFailover", resourceCulture);

	internal static string ShouldProcess_ConnectVMNetworkAdapter_ToSwitch => ResourceManager.GetString("ShouldProcess_ConnectVMNetworkAdapter_ToSwitch", resourceCulture);

	internal static string ShouldProcess_ConnectVMNetworkAdapter_UseAutomaticConnection => ResourceManager.GetString("ShouldProcess_ConnectVMNetworkAdapter_UseAutomaticConnection", resourceCulture);

	internal static string ShouldProcess_ConnectVMSan => ResourceManager.GetString("ShouldProcess_ConnectVMSan", resourceCulture);

	internal static string ShouldProcess_ContinueVMReplication => ResourceManager.GetString("ShouldProcess_ContinueVMReplication", resourceCulture);

	internal static string ShouldProcess_ConvertVHD => ResourceManager.GetString("ShouldProcess_ConvertVHD", resourceCulture);

	internal static string ShouldProcess_CopyVMFile => ResourceManager.GetString("ShouldProcess_CopyVMFile", resourceCulture);

	internal static string ShouldProcess_DebugVM_InjectNMI => ResourceManager.GetString("ShouldProcess_DebugVM_InjectNMI", resourceCulture);

	internal static string ShouldProcess_DisableVMEventing => ResourceManager.GetString("ShouldProcess_DisableVMEventing", resourceCulture);

	internal static string ShouldProcess_DisableVMIntegrationService => ResourceManager.GetString("ShouldProcess_DisableVMIntegrationService", resourceCulture);

	internal static string ShouldProcess_DisableVMMigration => ResourceManager.GetString("ShouldProcess_DisableVMMigration", resourceCulture);

	internal static string ShouldProcess_DisableVMRemoteFXPhysicalVideoAdapter => ResourceManager.GetString("ShouldProcess_DisableVMRemoteFXPhysicalVideoAdapter", resourceCulture);

	internal static string ShouldProcess_DisableVMSwitchExtension => ResourceManager.GetString("ShouldProcess_DisableVMSwitchExtension", resourceCulture);

	internal static string ShouldProcess_DisableVMTPM => ResourceManager.GetString("ShouldProcess_DisableVMTPM", resourceCulture);

	internal static string ShouldProcess_DisconnectVMNetworkAdapter => ResourceManager.GetString("ShouldProcess_DisconnectVMNetworkAdapter", resourceCulture);

	internal static string ShouldProcess_DisconnectVMSan => ResourceManager.GetString("ShouldProcess_DisconnectVMSan", resourceCulture);

	internal static string ShouldProcess_DismountHostAssignableDevice => ResourceManager.GetString("ShouldProcess_DismountHostAssignableDevice", resourceCulture);

	internal static string ShouldProcess_DismountVHD => ResourceManager.GetString("ShouldProcess_DismountVHD", resourceCulture);

	internal static string ShouldProcess_EnableVMEventing => ResourceManager.GetString("ShouldProcess_EnableVMEventing", resourceCulture);

	internal static string ShouldProcess_EnableVMIntegrationService => ResourceManager.GetString("ShouldProcess_EnableVMIntegrationService", resourceCulture);

	internal static string ShouldProcess_EnableVMMigration => ResourceManager.GetString("ShouldProcess_EnableVMMigration", resourceCulture);

	internal static string ShouldProcess_EnableVMRemoteFXPhysicalVideoAdapter => ResourceManager.GetString("ShouldProcess_EnableVMRemoteFXPhysicalVideoAdapter", resourceCulture);

	internal static string ShouldProcess_EnableVMReplication => ResourceManager.GetString("ShouldProcess_EnableVMReplication", resourceCulture);

	internal static string ShouldProcess_EnableVMReplication_AsReplica => ResourceManager.GetString("ShouldProcess_EnableVMReplication_AsReplica", resourceCulture);

	internal static string ShouldProcess_EnableVMSwitchExtension => ResourceManager.GetString("ShouldProcess_EnableVMSwitchExtension", resourceCulture);

	internal static string ShouldProcess_EnableVMTPM => ResourceManager.GetString("ShouldProcess_EnableVMTPM", resourceCulture);

	internal static string ShouldProcess_ExportVM => ResourceManager.GetString("ShouldProcess_ExportVM", resourceCulture);

	internal static string ShouldProcess_ExportVMSnapshot => ResourceManager.GetString("ShouldProcess_ExportVMSnapshot", resourceCulture);

	internal static string ShouldProcess_GetVMKeyProtector => ResourceManager.GetString("ShouldProcess_GetVMKeyProtector", resourceCulture);

	internal static string ShouldProcess_GrantVMConnectAccess => ResourceManager.GetString("ShouldProcess_GrantVMConnectAccess", resourceCulture);

	internal static string ShouldProcess_ImportVM => ResourceManager.GetString("ShouldProcess_ImportVM", resourceCulture);

	internal static string ShouldProcess_ImportVMInitialReplication => ResourceManager.GetString("ShouldProcess_ImportVMInitialReplication", resourceCulture);

	internal static string ShouldProcess_MergeVHD => ResourceManager.GetString("ShouldProcess_MergeVHD", resourceCulture);

	internal static string ShouldProcess_MountHostAssignableDevice => ResourceManager.GetString("ShouldProcess_MountHostAssignableDevice", resourceCulture);

	internal static string ShouldProcess_MountVHD => ResourceManager.GetString("ShouldProcess_MountVHD", resourceCulture);

	internal static string ShouldProcess_MoveVM => ResourceManager.GetString("ShouldProcess_MoveVM", resourceCulture);

	internal static string ShouldProcess_MoveVMStorage => ResourceManager.GetString("ShouldProcess_MoveVMStorage", resourceCulture);

	internal static string ShouldProcess_NewVFD => ResourceManager.GetString("ShouldProcess_NewVFD", resourceCulture);

	internal static string ShouldProcess_NewVHD => ResourceManager.GetString("ShouldProcess_NewVHD", resourceCulture);

	internal static string ShouldProcess_NewVM => ResourceManager.GetString("ShouldProcess_NewVM", resourceCulture);

	internal static string ShouldProcess_NewVMGroup => ResourceManager.GetString("ShouldProcess_NewVMGroup", resourceCulture);

	internal static string ShouldProcess_NewVMReplicationAuthorizationEntry => ResourceManager.GetString("ShouldProcess_NewVMReplicationAuthorizationEntry", resourceCulture);

	internal static string ShouldProcess_NewVMResourcePool => ResourceManager.GetString("ShouldProcess_NewVMResourcePool", resourceCulture);

	internal static string ShouldProcess_NewVMSan => ResourceManager.GetString("ShouldProcess_NewVMSan", resourceCulture);

	internal static string ShouldProcess_NewVMSwitch => ResourceManager.GetString("ShouldProcess_NewVMSwitch", resourceCulture);

	internal static string ShouldProcess_OptimizeVHD => ResourceManager.GetString("ShouldProcess_OptimizeVHD", resourceCulture);

	internal static string ShouldProcess_OptimizeVHDSet => ResourceManager.GetString("ShouldProcess_OptimizeVHDSet", resourceCulture);

	internal static string ShouldProcess_RemoveVHDSnapshot => ResourceManager.GetString("ShouldProcess_RemoveVHDSnapshot", resourceCulture);

	internal static string ShouldProcess_RemoveVM => ResourceManager.GetString("ShouldProcess_RemoveVM", resourceCulture);

	internal static string ShouldProcess_RemoveVMAssignableDevice => ResourceManager.GetString("ShouldProcess_RemoveVMAssignableDevice", resourceCulture);

	internal static string ShouldProcess_RemoveVMDvdDrive => ResourceManager.GetString("ShouldProcess_RemoveVMDvdDrive", resourceCulture);

	internal static string ShouldProcess_RemoveVMFibreChannelHba => ResourceManager.GetString("ShouldProcess_RemoveVMFibreChannelHba", resourceCulture);

	internal static string ShouldProcess_RemoveVMGpuPartitionAdapter => ResourceManager.GetString("ShouldProcess_RemoveVMGpuPartitionAdapter", resourceCulture);

	internal static string ShouldProcess_RemoveVMGroup => ResourceManager.GetString("ShouldProcess_RemoveVMGroup", resourceCulture);

	internal static string ShouldProcess_RemoveVMGroupMember => ResourceManager.GetString("ShouldProcess_RemoveVMGroupMember", resourceCulture);

	internal static string ShouldProcess_RemoveVMHardDiskDrive => ResourceManager.GetString("ShouldProcess_RemoveVMHardDiskDrive", resourceCulture);

	internal static string ShouldProcess_RemoveVMHIDDevices => ResourceManager.GetString("ShouldProcess_RemoveVMHIDDevices", resourceCulture);

	internal static string ShouldProcess_RemoveVMHostAssignableDevice => ResourceManager.GetString("ShouldProcess_RemoveVMHostAssignableDevice", resourceCulture);

	internal static string ShouldProcess_RemoveVMKeyStorageDrive => ResourceManager.GetString("ShouldProcess_RemoveVMKeyStorageDrive", resourceCulture);

	internal static string ShouldProcess_RemoveVMMigrationNetwork => ResourceManager.GetString("ShouldProcess_RemoveVMMigrationNetwork", resourceCulture);

	internal static string ShouldProcess_RemoveVMNetworkAdapter => ResourceManager.GetString("ShouldProcess_RemoveVMNetworkAdapter", resourceCulture);

	internal static string ShouldProcess_RemoveVMNetworkAdapterAcl => ResourceManager.GetString("ShouldProcess_RemoveVMNetworkAdapterAcl", resourceCulture);

	internal static string ShouldProcess_RemoveVMNetworkAdapterExtendedAcl => ResourceManager.GetString("ShouldProcess_RemoveVMNetworkAdapterExtendedAcl", resourceCulture);

	internal static string ShouldProcess_RemoveVMNetworkAdapterRoutingDomainMapping => ResourceManager.GetString("ShouldProcess_RemoveVMNetworkAdapterRoutingDomainMapping", resourceCulture);

	internal static string ShouldProcess_RemoveVMNetworkAdapterTeamMapping => ResourceManager.GetString("ShouldProcess_RemoveVMNetworkAdapterTeamMapping", resourceCulture);

	internal static string ShouldProcess_RemoveVMPmemController => ResourceManager.GetString("ShouldProcess_RemoveVMPmemController", resourceCulture);

	internal static string ShouldProcess_RemoveVMRemoteFx3dVideoAdapter => ResourceManager.GetString("ShouldProcess_RemoveVMRemoteFx3dVideoAdapter", resourceCulture);

	internal static string ShouldProcess_RemoveVMReplication => ResourceManager.GetString("ShouldProcess_RemoveVMReplication", resourceCulture);

	internal static string ShouldProcess_RemoveVMReplicationAuthorizationEntry => ResourceManager.GetString("ShouldProcess_RemoveVMReplicationAuthorizationEntry", resourceCulture);

	internal static string ShouldProcess_RemoveVMResourcePool => ResourceManager.GetString("ShouldProcess_RemoveVMResourcePool", resourceCulture);

	internal static string ShouldProcess_RemoveVMSan => ResourceManager.GetString("ShouldProcess_RemoveVMSan", resourceCulture);

	internal static string ShouldProcess_RemoveVMSavedState_Snapshot => ResourceManager.GetString("ShouldProcess_RemoveVMSavedState_Snapshot", resourceCulture);

	internal static string ShouldProcess_RemoveVMSavedState_VM => ResourceManager.GetString("ShouldProcess_RemoveVMSavedState_VM", resourceCulture);

	internal static string ShouldProcess_RemoveVMScsiController => ResourceManager.GetString("ShouldProcess_RemoveVMScsiController", resourceCulture);

	internal static string ShouldProcess_RemoveVMSnapshot_IncludeAllChildSnapshots => ResourceManager.GetString("ShouldProcess_RemoveVMSnapshot_IncludeAllChildSnapshots", resourceCulture);

	internal static string ShouldProcess_RemoveVMSnapshot_SnapshotOnly => ResourceManager.GetString("ShouldProcess_RemoveVMSnapshot_SnapshotOnly", resourceCulture);

	internal static string ShouldProcess_RemoveVMStoragePath => ResourceManager.GetString("ShouldProcess_RemoveVMStoragePath", resourceCulture);

	internal static string ShouldProcess_RemoveVMSwitch => ResourceManager.GetString("ShouldProcess_RemoveVMSwitch", resourceCulture);

	internal static string ShouldProcess_RemoveVMSwitch_FromPool => ResourceManager.GetString("ShouldProcess_RemoveVMSwitch_FromPool", resourceCulture);

	internal static string ShouldProcess_RemoveVMSwitchExtensionPortFeature => ResourceManager.GetString("ShouldProcess_RemoveVMSwitchExtensionPortFeature", resourceCulture);

	internal static string ShouldProcess_RemoveVMSwitchExtensionSwitchFeature => ResourceManager.GetString("ShouldProcess_RemoveVMSwitchExtensionSwitchFeature", resourceCulture);

	internal static string ShouldProcess_RenameVM => ResourceManager.GetString("ShouldProcess_RenameVM", resourceCulture);

	internal static string ShouldProcess_RenameVMNetworkAdapter => ResourceManager.GetString("ShouldProcess_RenameVMNetworkAdapter", resourceCulture);

	internal static string ShouldProcess_RenameVMResourcePool => ResourceManager.GetString("ShouldProcess_RenameVMResourcePool", resourceCulture);

	internal static string ShouldProcess_RenameVMSan => ResourceManager.GetString("ShouldProcess_RenameVMSan", resourceCulture);

	internal static string ShouldProcess_RenameVMSnapshot => ResourceManager.GetString("ShouldProcess_RenameVMSnapshot", resourceCulture);

	internal static string ShouldProcess_RenameVMSwitch => ResourceManager.GetString("ShouldProcess_RenameVMSwitch", resourceCulture);

	internal static string ShouldProcess_RepairVM => ResourceManager.GetString("ShouldProcess_RepairVM", resourceCulture);

	internal static string ShouldProcess_ResetVMReplicationStatistics => ResourceManager.GetString("ShouldProcess_ResetVMReplicationStatistics", resourceCulture);

	internal static string ShouldProcess_ResizeVHD => ResourceManager.GetString("ShouldProcess_ResizeVHD", resourceCulture);

	internal static string ShouldProcess_RestartVM => ResourceManager.GetString("ShouldProcess_RestartVM", resourceCulture);

	internal static string ShouldProcess_RestoreVMSnapshot => ResourceManager.GetString("ShouldProcess_RestoreVMSnapshot", resourceCulture);

	internal static string ShouldProcess_ResumeVM => ResourceManager.GetString("ShouldProcess_ResumeVM", resourceCulture);

	internal static string ShouldProcess_ResumeVMReplication => ResourceManager.GetString("ShouldProcess_ResumeVMReplication", resourceCulture);

	internal static string ShouldProcess_RevokeVMConnectAccess => ResourceManager.GetString("ShouldProcess_RevokeVMConnectAccess", resourceCulture);

	internal static string ShouldProcess_SaveVM => ResourceManager.GetString("ShouldProcess_SaveVM", resourceCulture);

	internal static string ShouldProcess_SetVHD => ResourceManager.GetString("ShouldProcess_SetVHD", resourceCulture);

	internal static string ShouldProcess_SetVM => ResourceManager.GetString("ShouldProcess_SetVM", resourceCulture);

	internal static string ShouldProcess_SetVMBios => ResourceManager.GetString("ShouldProcess_SetVMBios", resourceCulture);

	internal static string ShouldProcess_SetVMComPort => ResourceManager.GetString("ShouldProcess_SetVMComPort", resourceCulture);

	internal static string ShouldProcess_SetVMDvdDrive => ResourceManager.GetString("ShouldProcess_SetVMDvdDrive", resourceCulture);

	internal static string ShouldProcess_SetVMFibreChannelHba => ResourceManager.GetString("ShouldProcess_SetVMFibreChannelHba", resourceCulture);

	internal static string ShouldProcess_SetVMFirmware => ResourceManager.GetString("ShouldProcess_SetVMFirmware", resourceCulture);

	internal static string ShouldProcess_SetVMFloppyDiskDrive => ResourceManager.GetString("ShouldProcess_SetVMFloppyDiskDrive", resourceCulture);

	internal static string ShouldProcess_SetVMGpuPartitionAdapter => ResourceManager.GetString("ShouldProcess_SetVMGpuPartitionAdapter", resourceCulture);

	internal static string ShouldProcess_SetVMHardDiskDrive => ResourceManager.GetString("ShouldProcess_SetVMHardDiskDrive", resourceCulture);

	internal static string ShouldProcess_SetVMHost => ResourceManager.GetString("ShouldProcess_SetVMHost", resourceCulture);

	internal static string ShouldProcess_SetVMKeyProtector => ResourceManager.GetString("ShouldProcess_SetVMKeyProtector", resourceCulture);

	internal static string ShouldProcess_SetVMKeyStorageDrive => ResourceManager.GetString("ShouldProcess_SetVMKeyStorageDrive", resourceCulture);

	internal static string ShouldProcess_SetVMMemory => ResourceManager.GetString("ShouldProcess_SetVMMemory", resourceCulture);

	internal static string ShouldProcess_SetVMMigrationNetwork => ResourceManager.GetString("ShouldProcess_SetVMMigrationNetwork", resourceCulture);

	internal static string ShouldProcess_SetVMNetworkAdapter => ResourceManager.GetString("ShouldProcess_SetVMNetworkAdapter", resourceCulture);

	internal static string ShouldProcess_SetVMNetworkAdapterIsolation => ResourceManager.GetString("ShouldProcess_SetVMNetworkAdapterIsolation", resourceCulture);

	internal static string ShouldProcess_SetVMNetworkAdapterRdma => ResourceManager.GetString("ShouldProcess_SetVMNetworkAdapterRdma", resourceCulture);

	internal static string ShouldProcess_SetVMNetworkAdapterRoutingDomainMapping => ResourceManager.GetString("ShouldProcess_SetVMNetworkAdapterRoutingDomainMapping", resourceCulture);

	internal static string ShouldProcess_SetVMNetworkAdapterTeamMapping => ResourceManager.GetString("ShouldProcess_SetVMNetworkAdapterTeamMapping", resourceCulture);

	internal static string ShouldProcess_SetVMNetworkAdapterVlan => ResourceManager.GetString("ShouldProcess_SetVMNetworkAdapterVlan", resourceCulture);

	internal static string ShouldProcess_SetVMPartitionableGpu => ResourceManager.GetString("ShouldProcess_SetVMPartitionableGpu", resourceCulture);

	internal static string ShouldProcess_SetVMProcessor => ResourceManager.GetString("ShouldProcess_SetVMProcessor", resourceCulture);

	internal static string ShouldProcess_SetVMRemoteFx3dVideoAdapter => ResourceManager.GetString("ShouldProcess_SetVMRemoteFx3dVideoAdapter", resourceCulture);

	internal static string ShouldProcess_SetVMReplication => ResourceManager.GetString("ShouldProcess_SetVMReplication", resourceCulture);

	internal static string ShouldProcess_SetVMReplication_AsReplica => ResourceManager.GetString("ShouldProcess_SetVMReplication_AsReplica", resourceCulture);

	internal static string ShouldProcess_SetVMReplication_Reverse => ResourceManager.GetString("ShouldProcess_SetVMReplication_Reverse", resourceCulture);

	internal static string ShouldProcess_SetVMReplicationAuthorizationEntry => ResourceManager.GetString("ShouldProcess_SetVMReplicationAuthorizationEntry", resourceCulture);

	internal static string ShouldProcess_SetVMReplicationServer => ResourceManager.GetString("ShouldProcess_SetVMReplicationServer", resourceCulture);

	internal static string ShouldProcess_SetVMResourcePool => ResourceManager.GetString("ShouldProcess_SetVMResourcePool", resourceCulture);

	internal static string ShouldProcess_SetVMSan => ResourceManager.GetString("ShouldProcess_SetVMSan", resourceCulture);

	internal static string ShouldProcess_SetVMSecurity => ResourceManager.GetString("ShouldProcess_SetVMSecurity", resourceCulture);

	internal static string ShouldProcess_SetVMSecurityPolicy => ResourceManager.GetString("ShouldProcess_SetVMSecurityPolicy", resourceCulture);

	internal static string ShouldProcess_SetVMStorageSetting => ResourceManager.GetString("ShouldProcess_SetVMStorageSetting", resourceCulture);

	internal static string ShouldProcess_SetVMSwitch => ResourceManager.GetString("ShouldProcess_SetVMSwitch", resourceCulture);

	internal static string ShouldProcess_SetVMSwitchExtensionPortFeature => ResourceManager.GetString("ShouldProcess_SetVMSwitchExtensionPortFeature", resourceCulture);

	internal static string ShouldProcess_SetVMSwitchExtensionSwitchFeature => ResourceManager.GetString("ShouldProcess_SetVMSwitchExtensionSwitchFeature", resourceCulture);

	internal static string ShouldProcess_SetVMSwitchTeam => ResourceManager.GetString("ShouldProcess_SetVMSwitchTeam", resourceCulture);

	internal static string ShouldProcess_SetVMTPM => ResourceManager.GetString("ShouldProcess_SetVMTPM", resourceCulture);

	internal static string ShouldProcess_SetVMVideo => ResourceManager.GetString("ShouldProcess_SetVMVideo", resourceCulture);

	internal static string ShouldProcess_StartVM => ResourceManager.GetString("ShouldProcess_StartVM", resourceCulture);

	internal static string ShouldProcess_StartVMFailover => ResourceManager.GetString("ShouldProcess_StartVMFailover", resourceCulture);

	internal static string ShouldProcess_StartVMFailover_Prepare => ResourceManager.GetString("ShouldProcess_StartVMFailover_Prepare", resourceCulture);

	internal static string ShouldProcess_StartVMFailover_Test => ResourceManager.GetString("ShouldProcess_StartVMFailover_Test", resourceCulture);

	internal static string ShouldProcess_StartVMInitialReplication => ResourceManager.GetString("ShouldProcess_StartVMInitialReplication", resourceCulture);

	internal static string ShouldProcess_StopVM_Save => ResourceManager.GetString("ShouldProcess_StopVM_Save", resourceCulture);

	internal static string ShouldProcess_StopVM_ShutDown => ResourceManager.GetString("ShouldProcess_StopVM_ShutDown", resourceCulture);

	internal static string ShouldProcess_StopVM_TurnOff => ResourceManager.GetString("ShouldProcess_StopVM_TurnOff", resourceCulture);

	internal static string ShouldProcess_StopVMFailover => ResourceManager.GetString("ShouldProcess_StopVMFailover", resourceCulture);

	internal static string ShouldProcess_StopVMFailover_Test => ResourceManager.GetString("ShouldProcess_StopVMFailover_Test", resourceCulture);

	internal static string ShouldProcess_StopVMInitialReplication => ResourceManager.GetString("ShouldProcess_StopVMInitialReplication", resourceCulture);

	internal static string ShouldProcess_StopVMReplication => ResourceManager.GetString("ShouldProcess_StopVMReplication", resourceCulture);

	internal static string ShouldProcess_SuspendVM => ResourceManager.GetString("ShouldProcess_SuspendVM", resourceCulture);

	internal static string ShouldProcess_SuspendVMReplication => ResourceManager.GetString("ShouldProcess_SuspendVMReplication", resourceCulture);

	internal static string ShouldProcess_TestSharedVHD => ResourceManager.GetString("ShouldProcess_TestSharedVHD", resourceCulture);

	internal static string ShouldProcess_TestVHD => ResourceManager.GetString("ShouldProcess_TestVHD", resourceCulture);

	internal static string ShouldProcess_TestVMNetworkAdapter => ResourceManager.GetString("ShouldProcess_TestVMNetworkAdapter", resourceCulture);

	internal static string ShouldProcess_UpdateVMConfigurationVersion => ResourceManager.GetString("ShouldProcess_UpdateVMConfigurationVersion", resourceCulture);

	internal static string ShouldProcess_UpdateVMOnlineCheckpoints => ResourceManager.GetString("ShouldProcess_UpdateVMOnlineCheckpoints", resourceCulture);

	internal static string ShouldProcess_UpdateVMSavedVM => ResourceManager.GetString("ShouldProcess_UpdateVMSavedVM", resourceCulture);

	internal static string ShouldProcess_UpdateVMWithSavedState => ResourceManager.GetString("ShouldProcess_UpdateVMWithSavedState", resourceCulture);

	internal static string ShouldProcessWarning => ResourceManager.GetString("ShouldProcessWarning", resourceCulture);

	internal static string StartPausedVMWarning => ResourceManager.GetString("StartPausedVMWarning", resourceCulture);

	internal static string StopVMTrace_TraceFileWrittenTo => ResourceManager.GetString("StopVMTrace_TraceFileWrittenTo", resourceCulture);

	internal static string Task_SendingInitialReplication => ResourceManager.GetString("Task_SendingInitialReplication", resourceCulture);

	internal static string TaskNotFound => ResourceManager.GetString("TaskNotFound", resourceCulture);

	internal static string KeyStorageDriveDeprecatedWarning => ResourceManager.GetString("KeyStorageDriveDeprecatedWarning", resourceCulture);

	internal static string TestVMReplicationConnectionSuccessful => ResourceManager.GetString("TestVMReplicationConnectionSuccessful", resourceCulture);

	internal static string VMJob_DefaultCaption => ResourceManager.GetString("VMJob_DefaultCaption", resourceCulture);

	[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
	internal CmdletResources()
	{
	}
}
