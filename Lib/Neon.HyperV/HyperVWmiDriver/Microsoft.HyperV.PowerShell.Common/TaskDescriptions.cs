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
internal class TaskDescriptions
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
				resourceMan = new ResourceManager("Microsoft.HyperV.PowerShell.Objects.Common.TaskDescriptions", typeof(TaskDescriptions).GetTypeInfo().Assembly);
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

	internal static string AddPciExpressDevice_ToResourcePool => ResourceManager.GetString("AddPciExpressDevice_ToResourcePool", resourceCulture);

	internal static string AddVMAssignableDevice => ResourceManager.GetString("AddVMAssignableDevice", resourceCulture);

	internal static string AddVMBattery => ResourceManager.GetString("AddVMBattery", resourceCulture);

	internal static string AddVMDvdDrive => ResourceManager.GetString("AddVMDvdDrive", resourceCulture);

	internal static string AddVMFibreChannelHba => ResourceManager.GetString("AddVMFibreChannelHba", resourceCulture);

	internal static string AddVMFibreChannelHba_RollbackPartialAdd => ResourceManager.GetString("AddVMFibreChannelHba_RollbackPartialAdd", resourceCulture);

	internal static string AddVMGpuPartitionAdapter => ResourceManager.GetString("AddVMGpuPartitionAdapter", resourceCulture);

	internal static string AddVMGroupMember => ResourceManager.GetString("AddVMGroupMember", resourceCulture);

	internal static string AddVMHardDiskDrive => ResourceManager.GetString("AddVMHardDiskDrive", resourceCulture);

	internal static string AddVMHIDDevices => ResourceManager.GetString("AddVMHIDDevices", resourceCulture);

	internal static string AddVMNetworkAdapter_ManagementOS => ResourceManager.GetString("AddVMNetworkAdapter_ManagementOS", resourceCulture);

	internal static string AddVMNetworkAdapter_RollbackPartialAdd => ResourceManager.GetString("AddVMNetworkAdapter_RollbackPartialAdd", resourceCulture);

	internal static string AddVMNetworkAdapter_VirtualMachine => ResourceManager.GetString("AddVMNetworkAdapter_VirtualMachine", resourceCulture);

	internal static string AddVMNetworkAdapterFeature => ResourceManager.GetString("AddVMNetworkAdapterFeature", resourceCulture);

	internal static string AddVMPmemController => ResourceManager.GetString("AddVMPmemController", resourceCulture);

	internal static string AddVMRemoteFx3dVideoAdapter => ResourceManager.GetString("AddVMRemoteFx3dVideoAdapter", resourceCulture);

	internal static string AddVMScsiController => ResourceManager.GetString("AddVMScsiController", resourceCulture);

	internal static string AddVMStoragePath => ResourceManager.GetString("AddVMStoragePath", resourceCulture);

	internal static string AddVMSwitch => ResourceManager.GetString("AddVMSwitch", resourceCulture);

	internal static string AddVMSwitchFeature => ResourceManager.GetString("AddVMSwitchFeature", resourceCulture);

	internal static string AddVMSyntheticDisplayController => ResourceManager.GetString("AddVMSyntheticDisplayController", resourceCulture);

	internal static string AddVMSyntheticKeyboardController => ResourceManager.GetString("AddVMSyntheticKeyboardController", resourceCulture);

	internal static string AddVMSyntheticMouseController => ResourceManager.GetString("AddVMSyntheticMouseController", resourceCulture);

	internal static string CheckpointVM => ResourceManager.GetString("CheckpointVM", resourceCulture);

	internal static string CompareVM => ResourceManager.GetString("CompareVM", resourceCulture);

	internal static string ConnectVMSan => ResourceManager.GetString("ConnectVMSan", resourceCulture);

	internal static string ConvertVHD => ResourceManager.GetString("ConvertVHD", resourceCulture);

	internal static string CopyFileToGuestJob => ResourceManager.GetString("CopyFileToGuestJob", resourceCulture);

	internal static string DebugVM_InjectNMI => ResourceManager.GetString("DebugVM_InjectNMI", resourceCulture);

	internal static string DeleteVHDSnapshot => ResourceManager.GetString("DeleteVHDSnapshot", resourceCulture);

	internal static string DisconnectVMSan => ResourceManager.GetString("DisconnectVMSan", resourceCulture);

	internal static string DismountVHD => ResourceManager.GetString("DismountVHD", resourceCulture);

	internal static string DriveConfigurationRollback => ResourceManager.GetString("DriveConfigurationRollback", resourceCulture);

	internal static string ExportVMSnapshot => ResourceManager.GetString("ExportVMSnapshot", resourceCulture);

	internal static string GetVMKeyProtector => ResourceManager.GetString("GetVMKeyProtector", resourceCulture);

	internal static string GrantVMConnectAccess => ResourceManager.GetString("GrantVMConnectAccess", resourceCulture);

	internal static string MergeVHD => ResourceManager.GetString("MergeVHD", resourceCulture);

	internal static string MountVHD => ResourceManager.GetString("MountVHD", resourceCulture);

	internal static string MoveVM => ResourceManager.GetString("MoveVM", resourceCulture);

	internal static string MoveVMStorage => ResourceManager.GetString("MoveVMStorage", resourceCulture);

	internal static string NewVFD => ResourceManager.GetString("NewVFD", resourceCulture);

	internal static string NewVHD => ResourceManager.GetString("NewVHD", resourceCulture);

	internal static string NewVM => ResourceManager.GetString("NewVM", resourceCulture);

	internal static string NewVMGroup => ResourceManager.GetString("NewVMGroup", resourceCulture);

	internal static string NewVMResourcePool => ResourceManager.GetString("NewVMResourcePool", resourceCulture);

	internal static string NewVMSwitch => ResourceManager.GetString("NewVMSwitch", resourceCulture);

	internal static string OptimizeVHD => ResourceManager.GetString("OptimizeVHD", resourceCulture);

	internal static string OptimizeVHDSet => ResourceManager.GetString("OptimizeVHDSet", resourceCulture);

	internal static string PercentCompleteTemplate => ResourceManager.GetString("PercentCompleteTemplate", resourceCulture);

	internal static string RemovePciExpressDevice_FromResourcePool => ResourceManager.GetString("RemovePciExpressDevice_FromResourcePool", resourceCulture);

	internal static string RemoveVM => ResourceManager.GetString("RemoveVM", resourceCulture);

	internal static string RemoveVMAssignableDevice => ResourceManager.GetString("RemoveVMAssignableDevice", resourceCulture);

	internal static string RemoveVMBattery => ResourceManager.GetString("RemoveVMBattery", resourceCulture);

	internal static string RemoveVMDvdDrive => ResourceManager.GetString("RemoveVMDvdDrive", resourceCulture);

	internal static string RemoveVMFibreChannelHba => ResourceManager.GetString("RemoveVMFibreChannelHba", resourceCulture);

	internal static string RemoveVMGpuPartitionAdapter => ResourceManager.GetString("RemoveVMGpuPartitionAdapter", resourceCulture);

	internal static string RemoveVMGroup => ResourceManager.GetString("RemoveVMGroup", resourceCulture);

	internal static string RemoveVMGroupMember => ResourceManager.GetString("RemoveVMGroupMember", resourceCulture);

	internal static string RemoveVMHardDiskDrive => ResourceManager.GetString("RemoveVMHardDiskDrive", resourceCulture);

	internal static string RemoveVMHIDDevices => ResourceManager.GetString("RemoveVMHIDDevices", resourceCulture);

	internal static string RemoveVMNetworkAdapter_FromSwitch => ResourceManager.GetString("RemoveVMNetworkAdapter_FromSwitch", resourceCulture);

	internal static string RemoveVMNetworkAdapter_FromVM => ResourceManager.GetString("RemoveVMNetworkAdapter_FromVM", resourceCulture);

	internal static string RemoveVMNetworkAdapterFeature => ResourceManager.GetString("RemoveVMNetworkAdapterFeature", resourceCulture);

	internal static string RemoveVMPmemController => ResourceManager.GetString("RemoveVMPmemController", resourceCulture);

	internal static string RemoveVMRemoteFx3DVideoAdapter => ResourceManager.GetString("RemoveVMRemoteFx3DVideoAdapter", resourceCulture);

	internal static string RemoveVMResourcePool => ResourceManager.GetString("RemoveVMResourcePool", resourceCulture);

	internal static string RemoveVMSavedState => ResourceManager.GetString("RemoveVMSavedState", resourceCulture);

	internal static string RemoveVMScsiController => ResourceManager.GetString("RemoveVMScsiController", resourceCulture);

	internal static string RemoveVMSnapshot => ResourceManager.GetString("RemoveVMSnapshot", resourceCulture);

	internal static string RemoveVMSnapshot_IncludeAllChildSnapshots => ResourceManager.GetString("RemoveVMSnapshot_IncludeAllChildSnapshots", resourceCulture);

	internal static string RemoveVMSnapshotSavedState => ResourceManager.GetString("RemoveVMSnapshotSavedState", resourceCulture);

	internal static string RemoveVMStoragePath => ResourceManager.GetString("RemoveVMStoragePath", resourceCulture);

	internal static string RemoveVMSwitch => ResourceManager.GetString("RemoveVMSwitch", resourceCulture);

	internal static string RemoveVMSwitch_FromResourcePool => ResourceManager.GetString("RemoveVMSwitch_FromResourcePool", resourceCulture);

	internal static string RemoveVMSwitchFeature => ResourceManager.GetString("RemoveVMSwitchFeature", resourceCulture);

	internal static string RenameVMResourcePool => ResourceManager.GetString("RenameVMResourcePool", resourceCulture);

	internal static string ResizeVHD => ResourceManager.GetString("ResizeVHD", resourceCulture);

	internal static string RestartVM => ResourceManager.GetString("RestartVM", resourceCulture);

	internal static string RestoreVMSnapshot => ResourceManager.GetString("RestoreVMSnapshot", resourceCulture);

	internal static string ResumeVM => ResourceManager.GetString("ResumeVM", resourceCulture);

	internal static string RevokeVMConnectAccess => ResourceManager.GetString("RevokeVMConnectAccess", resourceCulture);

	internal static string SaveVM => ResourceManager.GetString("SaveVM", resourceCulture);

	internal static string SetReplicationService => ResourceManager.GetString("SetReplicationService", resourceCulture);

	internal static string SetVHD => ResourceManager.GetString("SetVHD", resourceCulture);

	internal static string SetVHDSnapshot => ResourceManager.GetString("SetVHDSnapshot", resourceCulture);

	internal static string SetVM => ResourceManager.GetString("SetVM", resourceCulture);

	internal static string SetVMAssignedDevice => ResourceManager.GetString("SetVMAssignedDevice", resourceCulture);

	internal static string SetVMBattery => ResourceManager.GetString("SetVMBattery", resourceCulture);

	internal static string SetVMBios => ResourceManager.GetString("SetVMBios", resourceCulture);

	internal static string SetVMComPort => ResourceManager.GetString("SetVMComPort", resourceCulture);

	internal static string SetVMDataExchangeComponent => ResourceManager.GetString("SetVMDataExchangeComponent", resourceCulture);

	internal static string SetVMDvdDrive => ResourceManager.GetString("SetVMDvdDrive", resourceCulture);

	internal static string SetVMDvdDrive_EjectDisk => ResourceManager.GetString("SetVMDvdDrive_EjectDisk", resourceCulture);

	internal static string SetVMDvdDrive_InsertDisk => ResourceManager.GetString("SetVMDvdDrive_InsertDisk", resourceCulture);

	internal static string SetVMFibreChannelHba => ResourceManager.GetString("SetVMFibreChannelHba", resourceCulture);

	internal static string SetVMFibreChannelHba_AttachConnection => ResourceManager.GetString("SetVMFibreChannelHba_AttachConnection", resourceCulture);

	internal static string SetVMFirmware => ResourceManager.GetString("SetVMFirmware", resourceCulture);

	internal static string SetVMFloppyDiskDrive => ResourceManager.GetString("SetVMFloppyDiskDrive", resourceCulture);

	internal static string SetVMFloppyDiskDrive_EjectDisk => ResourceManager.GetString("SetVMFloppyDiskDrive_EjectDisk", resourceCulture);

	internal static string SetVMGpuPartitionAdapter => ResourceManager.GetString("SetVMGpuPartitionAdapter", resourceCulture);

	internal static string SetVMGroupName => ResourceManager.GetString("SetVMGroupName", resourceCulture);

	internal static string SetVMGuestServiceIntegrationComponent => ResourceManager.GetString("SetVMGuestServiceIntegrationComponent", resourceCulture);

	internal static string SetVMHardDiskDrive => ResourceManager.GetString("SetVMHardDiskDrive", resourceCulture);

	internal static string SetVMHardDiskDrive_AttachVirtualDisk => ResourceManager.GetString("SetVMHardDiskDrive_AttachVirtualDisk", resourceCulture);

	internal static string SetVMHardDiskDrive_DetachVirtualDisk => ResourceManager.GetString("SetVMHardDiskDrive_DetachVirtualDisk", resourceCulture);

	internal static string SetVMHost => ResourceManager.GetString("SetVMHost", resourceCulture);

	internal static string SetVMHostCluster => ResourceManager.GetString("SetVMHostCluster", resourceCulture);

	internal static string SetVMIdeController => ResourceManager.GetString("SetVMIdeController", resourceCulture);

	internal static string SetVMIntegrationComponent => ResourceManager.GetString("SetVMIntegrationComponent", resourceCulture);

	internal static string SetVMKeyboard => ResourceManager.GetString("SetVMKeyboard", resourceCulture);

	internal static string SetVMKeyProtector => ResourceManager.GetString("SetVMKeyProtector", resourceCulture);

	internal static string SetVMMemory => ResourceManager.GetString("SetVMMemory", resourceCulture);

	internal static string SetVMMetrics => ResourceManager.GetString("SetVMMetrics", resourceCulture);

	internal static string SetVMMigrationNetwork => ResourceManager.GetString("SetVMMigrationNetwork", resourceCulture);

	internal static string SetVMMigrationService => ResourceManager.GetString("SetVMMigrationService", resourceCulture);

	internal static string SetVMMouse => ResourceManager.GetString("SetVMMouse", resourceCulture);

	internal static string SetVMNetworkAdapter => ResourceManager.GetString("SetVMNetworkAdapter", resourceCulture);

	internal static string SetVMNetworkAdapter_InternalOrExternal => ResourceManager.GetString("SetVMNetworkAdapter_InternalOrExternal", resourceCulture);

	internal static string SetVMNetworkAdapterFeature => ResourceManager.GetString("SetVMNetworkAdapterFeature", resourceCulture);

	internal static string SetVMPartitionableGpuPartitionCount => ResourceManager.GetString("SetVMPartitionableGpuPartitionCount", resourceCulture);

	internal static string SetVMPmemController => ResourceManager.GetString("SetVMPmemController", resourceCulture);

	internal static string SetVMProcessor => ResourceManager.GetString("SetVMProcessor", resourceCulture);

	internal static string SetVMRemoteFx3DVideoAdapter => ResourceManager.GetString("SetVMRemoteFx3DVideoAdapter", resourceCulture);

	internal static string SetVMRemoteFXPhysicalVideoAdapter => ResourceManager.GetString("SetVMRemoteFXPhysicalVideoAdapter", resourceCulture);

	internal static string SetVMReplicationSettings => ResourceManager.GetString("SetVMReplicationSettings", resourceCulture);

	internal static string SetVMResourcePool_SetParentNames => ResourceManager.GetString("SetVMResourcePool_SetParentNames", resourceCulture);

	internal static string SetVMS3DisplayController => ResourceManager.GetString("SetVMS3DisplayController", resourceCulture);

	internal static string SetVMSanHbas => ResourceManager.GetString("SetVMSanHbas", resourceCulture);

	internal static string SetVMScsiController => ResourceManager.GetString("SetVMScsiController", resourceCulture);

	internal static string SetVMSecuritySettings => ResourceManager.GetString("SetVMSecuritySettings", resourceCulture);

	internal static string SetVMShutdownComponent => ResourceManager.GetString("SetVMShutdownComponent", resourceCulture);

	internal static string SetVMSnapshot => ResourceManager.GetString("SetVMSnapshot", resourceCulture);

	internal static string SetVMStorageSettings => ResourceManager.GetString("SetVMStorageSettings", resourceCulture);

	internal static string SetVMSwitch => ResourceManager.GetString("SetVMSwitch", resourceCulture);

	internal static string SetVMSwitchExtension => ResourceManager.GetString("SetVMSwitchExtension", resourceCulture);

	internal static string SetVMSwitchFeature => ResourceManager.GetString("SetVMSwitchFeature", resourceCulture);

	internal static string SetVMVideo => ResourceManager.GetString("SetVMVideo", resourceCulture);

	internal static string ShutdownVM => ResourceManager.GetString("ShutdownVM", resourceCulture);

	internal static string StageVM => ResourceManager.GetString("StageVM", resourceCulture);

	internal static string StartVM => ResourceManager.GetString("StartVM", resourceCulture);

	internal static string StopVM => ResourceManager.GetString("StopVM", resourceCulture);

	internal static string SuspendVM => ResourceManager.GetString("SuspendVM", resourceCulture);

	internal static string Task_AddReplicationAuthorizationEntry => ResourceManager.GetString("Task_AddReplicationAuthorizationEntry", resourceCulture);

	internal static string Task_ChangeReplicationModeToPrimary => ResourceManager.GetString("Task_ChangeReplicationModeToPrimary", resourceCulture);

	internal static string Task_CommitFailover => ResourceManager.GetString("Task_CommitFailover", resourceCulture);

	internal static string Task_CreateReplicationRelationship => ResourceManager.GetString("Task_CreateReplicationRelationship", resourceCulture);

	internal static string Task_DeleteVMAuthorizationEntry => ResourceManager.GetString("Task_DeleteVMAuthorizationEntry", resourceCulture);

	internal static string Task_ExportingVM => ResourceManager.GetString("Task_ExportingVM", resourceCulture);

	internal static string Task_ImportReplication => ResourceManager.GetString("Task_ImportReplication", resourceCulture);

	internal static string Task_ImportSnapshots => ResourceManager.GetString("Task_ImportSnapshots", resourceCulture);

	internal static string Task_ImportVM => ResourceManager.GetString("Task_ImportVM", resourceCulture);

	internal static string Task_InitiateFailover => ResourceManager.GetString("Task_InitiateFailover", resourceCulture);

	internal static string Task_InjectNonMaskableInterruptVM => ResourceManager.GetString("Task_InjectNonMaskableInterruptVM", resourceCulture);

	internal static string Task_RealizeVM => ResourceManager.GetString("Task_RealizeVM", resourceCulture);

	internal static string Task_RemoveReplication => ResourceManager.GetString("Task_RemoveReplication", resourceCulture);

	internal static string Task_ResetReplicationStatistics => ResourceManager.GetString("Task_ResetReplicationStatistics", resourceCulture);

	internal static string Task_Resynchronize => ResourceManager.GetString("Task_Resynchronize", resourceCulture);

	internal static string Task_ReverseReplicationRelationship => ResourceManager.GetString("Task_ReverseReplicationRelationship", resourceCulture);

	internal static string Task_RevertVMFailover => ResourceManager.GetString("Task_RevertVMFailover", resourceCulture);

	internal static string Task_SetAuthorizationEntry => ResourceManager.GetString("Task_SetAuthorizationEntry", resourceCulture);

	internal static string Task_SetVMAuthorizationEntry => ResourceManager.GetString("Task_SetVMAuthorizationEntry", resourceCulture);

	internal static string Task_SetVMReplicationState => ResourceManager.GetString("Task_SetVMReplicationState", resourceCulture);

	internal static string Task_StartFailover => ResourceManager.GetString("Task_StartFailover", resourceCulture);

	internal static string Task_StartReplication => ResourceManager.GetString("Task_StartReplication", resourceCulture);

	internal static string Task_StopFailover => ResourceManager.GetString("Task_StopFailover", resourceCulture);

	internal static string Task_TestReplicaSystem => ResourceManager.GetString("Task_TestReplicaSystem", resourceCulture);

	internal static string Task_TestReplicationConnection => ResourceManager.GetString("Task_TestReplicationConnection", resourceCulture);

	internal static string Task_UpdateNetworkAdapterFailoverConfiguration => ResourceManager.GetString("Task_UpdateNetworkAdapterFailoverConfiguration", resourceCulture);

	internal static string Task_UpdateVMConfigurationVersion => ResourceManager.GetString("Task_UpdateVMConfigurationVersion", resourceCulture);

	internal static string Task_ValidateVM => ResourceManager.GetString("Task_ValidateVM", resourceCulture);

	internal static string TestSharedVHD => ResourceManager.GetString("TestSharedVHD", resourceCulture);

	internal static string TestVHD => ResourceManager.GetString("TestVHD", resourceCulture);

	[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
	internal TaskDescriptions()
	{
	}
}
