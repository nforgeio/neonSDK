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
internal class EnumValues
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
				resourceMan = new ResourceManager(WmiHelper.GetResourceName("Microsoft.Virtualization.Client.Management.ObjectModel.EnumValues"), typeof(EnumValues).GetTypeInfo().Assembly);
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

	internal static string BandwidthReservationMode_Absolute => ResourceManager.GetString("BandwidthReservationMode_Absolute", resourceCulture);

	internal static string BandwidthReservationMode_Default => ResourceManager.GetString("BandwidthReservationMode_Default", resourceCulture);

	internal static string BandwidthReservationMode_None => ResourceManager.GetString("BandwidthReservationMode_None", resourceCulture);

	internal static string BandwidthReservationMode_Weight => ResourceManager.GetString("BandwidthReservationMode_Weight", resourceCulture);

	internal static string CriticalErrorAction_None => ResourceManager.GetString("CriticalErrorAction_None", resourceCulture);

	internal static string CriticalErrorAction_Pause => ResourceManager.GetString("CriticalErrorAction_Pause", resourceCulture);

	internal static string EnhancedModeStateType_Available => ResourceManager.GetString("EnhancedModeStateType_Available", resourceCulture);

	internal static string EnhancedModeStateType_Disabled => ResourceManager.GetString("EnhancedModeStateType_Disabled", resourceCulture);

	internal static string EnhancedModeStateType_Enabled => ResourceManager.GetString("EnhancedModeStateType_Enabled", resourceCulture);

	internal static string EnhancedSessionTransportType_HvSocket => ResourceManager.GetString("EnhancedSessionTransportType_HvSocket", resourceCulture);

	internal static string EnhancedSessionTransportType_VMBus => ResourceManager.GetString("EnhancedSessionTransportType_VMBus", resourceCulture);

	internal static string EthernetSwitchExtensionType_Capture => ResourceManager.GetString("EthernetSwitchExtensionType_Capture", resourceCulture);

	internal static string EthernetSwitchExtensionType_Filter => ResourceManager.GetString("EthernetSwitchExtensionType_Filter", resourceCulture);

	internal static string EthernetSwitchExtensionType_Forward => ResourceManager.GetString("EthernetSwitchExtensionType_Forward", resourceCulture);

	internal static string EthernetSwitchExtensionType_Monitoring => ResourceManager.GetString("EthernetSwitchExtensionType_Monitoring", resourceCulture);

	internal static string EthernetSwitchExtensionType_Native => ResourceManager.GetString("EthernetSwitchExtensionType_Native", resourceCulture);

	internal static string EthernetSwitchExtensionType_Unknown => ResourceManager.GetString("EthernetSwitchExtensionType_Unknown", resourceCulture);

	internal static string FailoverReplicationHealth_Critical => ResourceManager.GetString("FailoverReplicationHealth_Critical", resourceCulture);

	internal static string FailoverReplicationHealth_Normal => ResourceManager.GetString("FailoverReplicationHealth_Normal", resourceCulture);

	internal static string FailoverReplicationHealth_NotApplicable => ResourceManager.GetString("FailoverReplicationHealth_NotApplicable", resourceCulture);

	internal static string FailoverReplicationHealth_Warning => ResourceManager.GetString("FailoverReplicationHealth_Warning", resourceCulture);

	internal static string FailoverReplicationMode_ExtendedReplica => ResourceManager.GetString("FailoverReplicationMode_ExtendedReplica", resourceCulture);

	internal static string FailoverReplicationMode_None => ResourceManager.GetString("FailoverReplicationMode_None", resourceCulture);

	internal static string FailoverReplicationMode_Primary => ResourceManager.GetString("FailoverReplicationMode_Primary", resourceCulture);

	internal static string FailoverReplicationMode_Recovery => ResourceManager.GetString("FailoverReplicationMode_Recovery", resourceCulture);

	internal static string FailoverReplicationMode_TestReplica => ResourceManager.GetString("FailoverReplicationMode_TestReplica", resourceCulture);

	internal static string FailoverReplicationState_Committed => ResourceManager.GetString("FailoverReplicationState_Committed", resourceCulture);

	internal static string FailoverReplicationState_Critical => ResourceManager.GetString("FailoverReplicationState_Critical", resourceCulture);

	internal static string FailoverReplicationState_Disabled => ResourceManager.GetString("FailoverReplicationState_Disabled", resourceCulture);

	internal static string FailoverReplicationState_FailbackComplete => ResourceManager.GetString("FailoverReplicationState_FailbackComplete", resourceCulture);

	internal static string FailoverReplicationState_FailbackInProgress => ResourceManager.GetString("FailoverReplicationState_FailbackInProgress", resourceCulture);

	internal static string FailoverReplicationState_FiredrillInProgress => ResourceManager.GetString("FailoverReplicationState_FiredrillInProgress", resourceCulture);

	internal static string FailoverReplicationState_PreparedForGroupReverseReplication => ResourceManager.GetString("FailoverReplicationState_PreparedForGroupReverseReplication", resourceCulture);

	internal static string FailoverReplicationState_PreparedForSyncReplication => ResourceManager.GetString("FailoverReplicationState_PreparedForSyncReplication", resourceCulture);

	internal static string FailoverReplicationState_Ready => ResourceManager.GetString("FailoverReplicationState_Ready", resourceCulture);

	internal static string FailoverReplicationState_Recovered => ResourceManager.GetString("FailoverReplicationState_Recovered", resourceCulture);

	internal static string FailoverReplicationState_RecoveryInProgress => ResourceManager.GetString("FailoverReplicationState_RecoveryInProgress", resourceCulture);

	internal static string FailoverReplicationState_Replicating => ResourceManager.GetString("FailoverReplicationState_Replicating", resourceCulture);

	internal static string FailoverReplicationState_ResynchronizeSuspended => ResourceManager.GetString("FailoverReplicationState_ResynchronizeSuspended", resourceCulture);

	internal static string FailoverReplicationState_Resynchronizing => ResourceManager.GetString("FailoverReplicationState_Resynchronizing", resourceCulture);

	internal static string FailoverReplicationState_Suspended => ResourceManager.GetString("FailoverReplicationState_Suspended", resourceCulture);

	internal static string FailoverReplicationState_SyncedReplicationComplete => ResourceManager.GetString("FailoverReplicationState_SyncedReplicationComplete", resourceCulture);

	internal static string FailoverReplicationState_Unknown => ResourceManager.GetString("FailoverReplicationState_Unknown", resourceCulture);

	internal static string FailoverReplicationState_UpdateCritical => ResourceManager.GetString("FailoverReplicationState_UpdateCritical", resourceCulture);

	internal static string FailoverReplicationState_WaitingForRepurposeCompletion => ResourceManager.GetString("FailoverReplicationState_WaitingForRepurposeCompletion", resourceCulture);

	internal static string FailoverReplicationState_WaitingForStartResynchronize => ResourceManager.GetString("FailoverReplicationState_WaitingForStartResynchronize", resourceCulture);

	internal static string FailoverReplicationState_WaitingForUpdateCompletion => ResourceManager.GetString("FailoverReplicationState_WaitingForUpdateCompletion", resourceCulture);

	internal static string FailoverReplicationState_WaitingToCompleteInitialReplication => ResourceManager.GetString("FailoverReplicationState_WaitingToCompleteInitialReplication", resourceCulture);

	internal static string ServiceStartOperation_AlwaysStartup => ResourceManager.GetString("ServiceStartOperation_AlwaysStartup", resourceCulture);

	internal static string ServiceStartOperation_None => ResourceManager.GetString("ServiceStartOperation_None", resourceCulture);

	internal static string ServiceStartOperation_RestartIfPreviouslyRunning => ResourceManager.GetString("ServiceStartOperation_RestartIfPreviouslyRunning", resourceCulture);

	internal static string ServiceStopOperation_PowerOff => ResourceManager.GetString("ServiceStopOperation_PowerOff", resourceCulture);

	internal static string ServiceStopOperation_SaveState => ResourceManager.GetString("ServiceStopOperation_SaveState", resourceCulture);

	internal static string ServiceStopOperation_Shutdown => ResourceManager.GetString("ServiceStopOperation_Shutdown", resourceCulture);

	internal static string VHDSetAdditionalInformationType_Other => ResourceManager.GetString("VHDSetAdditionalInformationType_Other", resourceCulture);

	internal static string VHDSetAdditionalInformationType_Paths => ResourceManager.GetString("VHDSetAdditionalInformationType_Paths", resourceCulture);

	internal static string VHDSetAdditionalInformationType_Unknown => ResourceManager.GetString("VHDSetAdditionalInformationType_Unknown", resourceCulture);

	internal static string VHDSetSnapshotType_Other => ResourceManager.GetString("VHDSetSnapshotType_Other", resourceCulture);

	internal static string VHDSetSnapshotType_ResilientChangeTracking => ResourceManager.GetString("VHDSetSnapshotType_ResilientChangeTracking", resourceCulture);

	internal static string VHDSetSnapshotType_Unknown => ResourceManager.GetString("VHDSetSnapshotType_Unknown", resourceCulture);

	internal static string VHDSetSnapshotType_VirtualMachine => ResourceManager.GetString("VHDSetSnapshotType_VirtualMachine", resourceCulture);

	internal static string VHDSnapshotAdditionalInformationType_Other => ResourceManager.GetString("VHDSnapshotAdditionalInformationType_Other", resourceCulture);

	internal static string VHDSnapshotAdditionalInformationType_ParentPaths => ResourceManager.GetString("VHDSnapshotAdditionalInformationType_ParentPaths", resourceCulture);

	internal static string VHDSnapshotAdditionalInformationType_Unknown => ResourceManager.GetString("VHDSnapshotAdditionalInformationType_Unknown", resourceCulture);

	internal static string VirtualHardDiskFormat_Unknown => ResourceManager.GetString("VirtualHardDiskFormat_Unknown", resourceCulture);

	internal static string VirtualHardDiskFormat_Vhd => ResourceManager.GetString("VirtualHardDiskFormat_Vhd", resourceCulture);

	internal static string VirtualHardDiskFormat_VHDSet => ResourceManager.GetString("VirtualHardDiskFormat_VHDSet", resourceCulture);

	internal static string VirtualHardDiskFormat_Vhdx => ResourceManager.GetString("VirtualHardDiskFormat_Vhdx", resourceCulture);

	internal static string VirtualHardDiskType_Differencing => ResourceManager.GetString("VirtualHardDiskType_Differencing", resourceCulture);

	internal static string VirtualHardDiskType_DynamicallyExpanding => ResourceManager.GetString("VirtualHardDiskType_DynamicallyExpanding", resourceCulture);

	internal static string VirtualHardDiskType_FixedSize => ResourceManager.GetString("VirtualHardDiskType_FixedSize", resourceCulture);

	internal static string VirtualHardDiskType_Unknown => ResourceManager.GetString("VirtualHardDiskType_Unknown", resourceCulture);

	internal static string VirtualSystemSubType_Type1 => ResourceManager.GetString("VirtualSystemSubType_Type1", resourceCulture);

	internal static string VirtualSystemSubType_Type2 => ResourceManager.GetString("VirtualSystemSubType_Type2", resourceCulture);

	internal static string VMComputerSystemState_FastSaved => ResourceManager.GetString("VMComputerSystemState_FastSaved", resourceCulture);

	internal static string VMComputerSystemState_FastSaved_VMSettings => ResourceManager.GetString("VMComputerSystemState_FastSaved_VMSettings", resourceCulture);

	internal static string VMComputerSystemState_Hibernated => ResourceManager.GetString("VMComputerSystemState_Hibernated", resourceCulture);

	internal static string VMComputerSystemState_Hibernated_VMSettings => ResourceManager.GetString("VMComputerSystemState_Hibernated_VMSettings", resourceCulture);

	internal static string VMComputerSystemState_ComponentServicing => ResourceManager.GetString("VMComputerSystemState_ComponentServicing", resourceCulture);

	internal static string VMComputerSystemState_ComponentServicing_VMSettings => ResourceManager.GetString("VMComputerSystemState_ComponentServicing_VMSettings", resourceCulture);

	internal static string VMComputerSystemState_Paused => ResourceManager.GetString("VMComputerSystemState_Paused", resourceCulture);

	internal static string VMComputerSystemState_Paused_VMSettings => ResourceManager.GetString("VMComputerSystemState_Paused_VMSettings", resourceCulture);

	internal static string VMComputerSystemState_Pausing => ResourceManager.GetString("VMComputerSystemState_Pausing", resourceCulture);

	internal static string VMComputerSystemState_Pausing_VMSettings => ResourceManager.GetString("VMComputerSystemState_Pausing_VMSettings", resourceCulture);

	internal static string VMComputerSystemState_PowerOff => ResourceManager.GetString("VMComputerSystemState_PowerOff", resourceCulture);

	internal static string VMComputerSystemState_PowerOff_VMSettings => ResourceManager.GetString("VMComputerSystemState_PowerOff_VMSettings", resourceCulture);

	internal static string VMComputerSystemState_Reset_VMSettings => ResourceManager.GetString("VMComputerSystemState_Reset_VMSettings", resourceCulture);

	internal static string VMComputerSystemState_Resuming => ResourceManager.GetString("VMComputerSystemState_Resuming", resourceCulture);

	internal static string VMComputerSystemState_Resuming_VMSettings => ResourceManager.GetString("VMComputerSystemState_Resuming_VMSettings", resourceCulture);

	internal static string VMComputerSystemState_Running => ResourceManager.GetString("VMComputerSystemState_Running", resourceCulture);

	internal static string VMComputerSystemState_Running_VMSettings => ResourceManager.GetString("VMComputerSystemState_Running_VMSettings", resourceCulture);

	internal static string VMComputerSystemState_Saved => ResourceManager.GetString("VMComputerSystemState_Saved", resourceCulture);

	internal static string VMComputerSystemState_Saved_VMSettings => ResourceManager.GetString("VMComputerSystemState_Saved_VMSettings", resourceCulture);

	internal static string VMComputerSystemState_Saving => ResourceManager.GetString("VMComputerSystemState_Saving", resourceCulture);

	internal static string VMComputerSystemState_Saving_VMSettings => ResourceManager.GetString("VMComputerSystemState_Saving_VMSettings", resourceCulture);

	internal static string VMComputerSystemState_Starting => ResourceManager.GetString("VMComputerSystemState_Starting", resourceCulture);

	internal static string VMComputerSystemState_Starting_VMSettings => ResourceManager.GetString("VMComputerSystemState_Starting_VMSettings", resourceCulture);

	internal static string VMComputerSystemState_Stopping => ResourceManager.GetString("VMComputerSystemState_Stopping", resourceCulture);

	internal static string VMComputerSystemState_Stopping_VMSettings => ResourceManager.GetString("VMComputerSystemState_Stopping_VMSettings", resourceCulture);

	internal static string VMComputerSystemState_Unknown => ResourceManager.GetString("VMComputerSystemState_Unknown", resourceCulture);

	internal static string VMComputerSystemState_Unknown_VMSettings => ResourceManager.GetString("VMComputerSystemState_Unknown_VMSettings", resourceCulture);

	internal static string VMHeartbeatStatus_Disabled => ResourceManager.GetString("VMHeartbeatStatus_Disabled", resourceCulture);

	internal static string VMHeartbeatStatus_Error => ResourceManager.GetString("VMHeartbeatStatus_Error", resourceCulture);

	internal static string VMHeartbeatStatus_LostCommunication => ResourceManager.GetString("VMHeartbeatStatus_LostCommunication", resourceCulture);

	internal static string VMHeartbeatStatus_NoContact => ResourceManager.GetString("VMHeartbeatStatus_NoContact", resourceCulture);

	internal static string VMHeartbeatStatus_OkApplicationsCritical => ResourceManager.GetString("VMHeartbeatStatus_OkApplicationsCritical", resourceCulture);

	internal static string VMHeartbeatStatus_OkApplicationsHealthy => ResourceManager.GetString("VMHeartbeatStatus_OkApplicationsHealthy", resourceCulture);

	internal static string VMHeartbeatStatus_OkApplicationsUnknown => ResourceManager.GetString("VMHeartbeatStatus_OkApplicationsUnknown", resourceCulture);

	internal static string VMHeartbeatStatus_Paused => ResourceManager.GetString("VMHeartbeatStatus_Paused", resourceCulture);

	internal static string VMHeartbeatStatus_Unknown => ResourceManager.GetString("VMHeartbeatStatus_Unknown", resourceCulture);

	internal static string VMIntegrationComponentOperationalStatus_Disabled => ResourceManager.GetString("VMIntegrationComponentOperationalStatus_Disabled", resourceCulture);

	internal static string VMIntegrationComponentOperationalStatus_Dormant => ResourceManager.GetString("VMIntegrationComponentOperationalStatus_Dormant", resourceCulture);

	internal static string VMIntegrationComponentOperationalStatus_Error => ResourceManager.GetString("VMIntegrationComponentOperationalStatus_Error", resourceCulture);

	internal static string VMIntegrationComponentOperationalStatus_LostCommunication => ResourceManager.GetString("VMIntegrationComponentOperationalStatus_LostCommunication", resourceCulture);

	internal static string VMIntegrationComponentOperationalStatus_NoContact => ResourceManager.GetString("VMIntegrationComponentOperationalStatus_NoContact", resourceCulture);

	internal static string VMIntegrationComponentOperationalStatus_Ok => ResourceManager.GetString("VMIntegrationComponentOperationalStatus_Ok", resourceCulture);

	internal static string VMIntegrationComponentOperationalStatus_Unknown => ResourceManager.GetString("VMIntegrationComponentOperationalStatus_Unknown", resourceCulture);

	internal static string VMMigrationType_PlannedVirtualSystem => ResourceManager.GetString("VMMigrationType_PlannedVirtualSystem", resourceCulture);

	internal static string VMMigrationType_Storage => ResourceManager.GetString("VMMigrationType_Storage", resourceCulture);

	internal static string VMMigrationType_Unknown => ResourceManager.GetString("VMMigrationType_Unknown", resourceCulture);

	internal static string VMMigrationType_VirtualSystem => ResourceManager.GetString("VMMigrationType_VirtualSystem", resourceCulture);

	internal static string VMMigrationType_VirtualSystemAndStorage => ResourceManager.GetString("VMMigrationType_VirtualSystemAndStorage", resourceCulture);

	internal static string VMProcessorOperationalStatus_DegradedDebugPort => ResourceManager.GetString("VMProcessorOperationalStatus_DegradedDebugPort", resourceCulture);

	internal static string VMProcessorOperationalStatus_DegradedDvdMedia => ResourceManager.GetString("VMProcessorOperationalStatus_DegradedDvdMedia", resourceCulture);

	internal static string VMProcessorOperationalStatus_DegradedFloppyMedia => ResourceManager.GetString("VMProcessorOperationalStatus_DegradedFloppyMedia", resourceCulture);

	internal static string VMProcessorOperationalStatus_DegradedLegacyNic => ResourceManager.GetString("VMProcessorOperationalStatus_DegradedLegacyNic", resourceCulture);

	internal static string VMProcessorOperationalStatus_DegradedSerialPort => ResourceManager.GetString("VMProcessorOperationalStatus_DegradedSerialPort", resourceCulture);

	internal static string VMProcessorOperationalStatus_DegradedSuspended => ResourceManager.GetString("VMProcessorOperationalStatus_DegradedSuspended", resourceCulture);

	internal static string VMProcessorOperationalStatus_Disabled => ResourceManager.GetString("VMProcessorOperationalStatus_Disabled", resourceCulture);

	internal static string VMProcessorOperationalStatus_Enabled => ResourceManager.GetString("VMProcessorOperationalStatus_Enabled", resourceCulture);

	internal static string VMProcessorOperationalStatus_OK => ResourceManager.GetString("VMProcessorOperationalStatus_OK", resourceCulture);

	internal static string VMProcessorOperationalStatus_Unknown => ResourceManager.GetString("VMProcessorOperationalStatus_Unknown", resourceCulture);

	[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
	internal EnumValues()
	{
	}
}
