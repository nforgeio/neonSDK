using System;
using System.Linq;

namespace Microsoft.Virtualization.Client.Management;

internal static class SummaryInformation
{
	internal static class WmiPropertyNames
	{
		public const string InstanceID = "InstanceID";

		public const string Name = "Name";

		public const string ElementName = "ElementName";

		public const string EnabledState = "EnabledState";

		public const string OtherEnabledState = "OtherEnabledState";

		public const string Uptime = "UpTime";

		public const string HealthState = "HealthState";

		public const string HostComputerSystemName = "HostComputerSystemName";

		public const string OperationalStatus = "OperationalStatus";

		public const string StatusDescriptions = "StatusDescriptions";

		public const string CreationTime = "CreationTime";

		public const string Notes = "Notes";

		public const string Version = "Version";

		public const string EnhancedSessionModeState = "EnhancedSessionModeState";

		public const string VirtualSystemSubType = "VirtualSystemSubType";

		public const string Shielded = "Shielded";

		public const string ProcessorLoad = "ProcessorLoad";

		public const string MemoryUsage = "MemoryUsage";

		public const string MemoryAvailable = "MemoryAvailable";

		public const string AvailableMemoryBuffer = "AvailableMemoryBuffer";

		public const string SwapFilesInUse = "SwapFilesInUse";

		public const string MemorySpansPhysicalNumaNodes = "MemorySpansPhysicalNumaNodes";

		public const string ReplicationMode = "ReplicationMode";

		public const string TestReplicaSystem = "TestReplicaSystem";

		public const string ReplicationStateEx = "ReplicationStateEx";

		public const string ReplicationHealthEx = "ReplicationHealthEx";

		public const string ReplicationProviderId = "ReplicationProviderId";

		public const string Heartbeat = "Heartbeat";

		public const string ThumbnailImage = "ThumbnailImage";

		public const string ThumbnailImageWidth = "ThumbnailImageWidth";

		public const string ThumbnailImageHeight = "ThumbnailImageHeight";

		public const string ApplicationHealth = "ApplicationHealth";
	}

	private static readonly string[] gm_RequestedInformationNameOnlyProperties;

	private static readonly WmiOperationOptions gm_RequestedInformationNameOnlyOptions;

	private static readonly string[] gm_RequestedInformationNameAndHostProperties;

	private static readonly WmiOperationOptions gm_RequestedInformationNameAndHostOptions;

	private static readonly string[] gm_RequestedInformationStateOnlyProperties;

	private static readonly WmiOperationOptions gm_RequestedInformationStateOnlyOptions;

	private static readonly string[] gm_RequestedInformationUpdateProperties;

	private static readonly WmiOperationOptions gm_RequestedInformationUpdateOptions;

	private static readonly string[] gm_RequestedInformationDetailDeltaProperties;

	private static readonly WmiOperationOptions gm_RequestedInformationDetailOptions;

	private static readonly string[] gm_RequestedInformationScriptingProperties;

	private static readonly WmiOperationOptions gm_RequestedInformationScriptingOptions;

	private static readonly string[] gm_RequestedInformationUptimeOnlyProperties;

	private static readonly WmiOperationOptions gm_RequestedInformationUptimeOnlyOptions;

	private static readonly string[] gm_RequestedInformationHeartbeatOnlyProperties;

	private static readonly string[] gm_RequestedInformationMemoryDemandOnlyProperties;

	private static readonly WmiOperationOptions gm_RequestedInformationHeartbeatOnlyOptions;

	private static readonly WmiOperationOptions gm_RequestedInformationMemoryDemandOnlyOptions;

	private const int gm_DefaultThumbnailImageWidth = 80;

	private const int gm_DefaultThumbnailImageHeight = 60;

	private const int gm_InvalidMemory = 0;

	private const int gm_InvalidMemoryAvailable = int.MaxValue;

	private const int gm_InvalidAvailableMemoryBuffer = int.MaxValue;

	private const string gm_DefaultReplicationProviderId = "22391CDC-272C-4DDF-BA88-9BEFB1A0975C";

	public static int DefaultThumbnailImageWidth => 80;

	public static int DefaultThumbnailImageHeight => 60;

	public static int InvalidMemory => 0;

	public static int InvalidMemoryAvailable => int.MaxValue;

	public static int InvalidAvailableMemoryBuffer => int.MaxValue;

	static SummaryInformation()
	{
		gm_RequestedInformationNameOnlyProperties = new string[3] { "InstanceID", "Name", "ElementName" };
		gm_RequestedInformationNameAndHostProperties = new string[4] { "InstanceID", "Name", "ElementName", "HostComputerSystemName" };
		gm_RequestedInformationStateOnlyProperties = new string[6] { "InstanceID", "Name", "ElementName", "EnabledState", "OtherEnabledState", "HealthState" };
		gm_RequestedInformationUpdateProperties = new string[25]
		{
			"InstanceID", "Name", "ElementName", "EnabledState", "OtherEnabledState", "ProcessorLoad", "UpTime", "Version", "Shielded", "HealthState",
			"MemoryUsage", "MemoryAvailable", "AvailableMemoryBuffer", "SwapFilesInUse", "MemorySpansPhysicalNumaNodes", "OperationalStatus", "StatusDescriptions", "ReplicationMode", "TestReplicaSystem", "ReplicationStateEx",
			"ReplicationHealthEx", "ReplicationProviderId", "EnhancedSessionModeState", "VirtualSystemSubType", "HostComputerSystemName"
		};
		gm_RequestedInformationDetailDeltaProperties = new string[7] { "CreationTime", "Notes", "Heartbeat", "ApplicationHealth", "ThumbnailImage", "ThumbnailImageWidth", "ThumbnailImageHeight" };
		gm_RequestedInformationScriptingProperties = new string[14]
		{
			"InstanceID", "ApplicationHealth", "AvailableMemoryBuffer", "HealthState", "Heartbeat", "MemoryAvailable", "MemorySpansPhysicalNumaNodes", "MemoryUsage", "ProcessorLoad", "ReplicationHealthEx",
			"ReplicationMode", "ReplicationStateEx", "SwapFilesInUse", "UpTime"
		};
		gm_RequestedInformationUptimeOnlyProperties = new string[2] { "InstanceID", "UpTime" };
		gm_RequestedInformationHeartbeatOnlyProperties = new string[3] { "InstanceID", "ApplicationHealth", "Heartbeat" };
		gm_RequestedInformationMemoryDemandOnlyProperties = new string[3] { "InstanceID", "MemoryUsage", "MemoryAvailable" };
		gm_RequestedInformationNameOnlyOptions = CreateOperationOptions(gm_RequestedInformationNameOnlyProperties);
		gm_RequestedInformationNameAndHostOptions = CreateOperationOptions(gm_RequestedInformationNameAndHostProperties);
		gm_RequestedInformationStateOnlyOptions = CreateOperationOptions(gm_RequestedInformationStateOnlyProperties);
		gm_RequestedInformationUpdateOptions = CreateOperationOptions(gm_RequestedInformationUpdateProperties);
		gm_RequestedInformationScriptingOptions = CreateOperationOptions(gm_RequestedInformationScriptingProperties);
		gm_RequestedInformationUptimeOnlyOptions = CreateOperationOptions(gm_RequestedInformationUptimeOnlyProperties);
		gm_RequestedInformationHeartbeatOnlyOptions = CreateOperationOptions(gm_RequestedInformationHeartbeatOnlyProperties);
		gm_RequestedInformationMemoryDemandOnlyOptions = CreateOperationOptions(gm_RequestedInformationMemoryDemandOnlyProperties);
		gm_RequestedInformationDetailOptions = CreateOperationOptions(gm_RequestedInformationUpdateProperties.Concat(gm_RequestedInformationDetailDeltaProperties).ToArray());
	}

	private static WmiOperationOptions CreateOperationOptions(string[] propertyNames)
	{
		return new WmiOperationOptions
		{
			PartialObjectProperties = propertyNames
		};
	}

	internal static WmiOperationOptions GetRequestedInformationOptions(SummaryInformationRequest requestedInformation)
	{
		WmiOperationOptions wmiOperationOptions = null;
		return requestedInformation switch
		{
			SummaryInformationRequest.NameOnly => gm_RequestedInformationNameOnlyOptions, 
			SummaryInformationRequest.NameAndHost => gm_RequestedInformationNameAndHostOptions, 
			SummaryInformationRequest.StateOnly => gm_RequestedInformationStateOnlyOptions, 
			SummaryInformationRequest.Update => gm_RequestedInformationUpdateOptions, 
			SummaryInformationRequest.Detail => gm_RequestedInformationDetailOptions, 
			SummaryInformationRequest.Scripting => gm_RequestedInformationScriptingOptions, 
			SummaryInformationRequest.UptimeOnly => gm_RequestedInformationUptimeOnlyOptions, 
			SummaryInformationRequest.HeartbeatOnly => gm_RequestedInformationHeartbeatOnlyOptions, 
			SummaryInformationRequest.MemoryDemandOnly => gm_RequestedInformationMemoryDemandOnlyOptions, 
			_ => throw new ArgumentOutOfRangeException("requestedInformation"), 
		};
	}

	public static bool IsDefaultReplicationProviderId(string replicationProviderId)
	{
		return string.Equals(replicationProviderId, "22391CDC-272C-4DDF-BA88-9BEFB1A0975C", StringComparison.OrdinalIgnoreCase);
	}
}
