namespace Microsoft.Virtualization.Client.Management;

internal static class ValidationErrorId
{
	public static long SavedStateIncompatible = 21016L;

	public static long MissingSnapshot = 40002L;

	public static long SaveStateFileMissing = 40004L;

	public static long MemoryFileMissing = 40006L;

	public static long PassThroughDiskDetected = 40008L;

	public static long VhdFileMissing = 40010L;

	public static long MemoryWeightAboveMax = 40012L;

	public static long MemoryQuantityAboveMax = 40014L;

	public static long MemoryQuantityBelowMin = 40016L;

	public static long MemoryQuantityNotMultipleOf2 = 40018L;

	public static long MemoryQuantityAboveLimit = 40020L;

	public static long MemoryQuantityBelowReservation = 40022L;

	public static long MemoryLimitAboveMax = 40024L;

	public static long MemoryLimitBelowMin = 40026L;

	public static long MemoryLimitNotMultipleOf2 = 40028L;

	public static long MemoryReservationAboveMax = 40030L;

	public static long MemoryReservationBelowMin = 40032L;

	public static long MemoryReservationNotMultipleOf2 = 40034L;

	public static long MemoryBufferAboveMax = 40036L;

	public static long MemoryBufferBelowMin = 40038L;

	public static long DynamicMemoryNumaSpanningConflict = 40040L;

	public static long VDevInvalidPoolId = 12638L;

	public static long SynthFcPoolIdNotFound = 32172L;

	public static long SynthFcPoolIdInvalid = 32173L;

	public static long DeviceNotCompatible = 24008L;

	public static long VmVersionNotSupported = 24006L;

	public static long VmFailedTopologyInit = 25014L;

	public static long ProcessorVendorMismatch = 24002L;

	public static long ProcessorFeaturesNotSupported = 24004L;

	public static long MemoryPoolIdInvalid = 23134L;

	public static long ProcessorLimitOutOfRange = 14390L;

	public static long ProcessorReservationOutOfRange = 14400L;

	public static long ProcessorWeightOutOfRange = 14410L;

	public static long ProcessorVirtualQuantityOutOfRange = 14420L;

	public static long ProcessorPoolIdInvalid = 14424L;

	public static long EthernetPoolNotFound = 33010L;

	public static long EthernetSwitchNotFound = 33012L;

	public static long EthernetSwitchNotFoundInPool = 33014L;

	public static long ConfigurationDataRootCreationFailure = 13000L;

	public static long SnapshotDataRootCreationFailure = 16350L;

	public static long SlpDataRootCreationFailure = 16352L;

	public static long PassThroughDiskNotFound = 27106L;

	public static long StoragePoolAbsolutePathRequired = 32900L;

	public static long StoragePoolAmbiguousRelativePath = 32906L;

	public static long StoragePoolAbsolutePathNotInBaseDirectories = 32908L;

	public static long StoragePoolPathContainingIntegrityStream = 32928L;

	public static long RemoteFxIncompatible = 32605L;

	public static long GroupNotFound = 40046L;
}
