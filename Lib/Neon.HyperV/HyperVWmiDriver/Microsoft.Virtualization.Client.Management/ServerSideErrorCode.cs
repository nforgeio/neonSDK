namespace Microsoft.Virtualization.Client.Management;

internal static class ServerSideErrorCode
{
	public const long Failed = 32768L;

	public const long AccessDenied = 32769L;

	public const long NotSupported = 32770L;

	public const long Unknown = 32771L;

	public const long StatusTimeout = 32772L;

	public const long InvalidParameter = 32773L;

	public const long StatusInUse = 32774L;

	public const long StatusInvalidState = 32775L;

	public const long StatusIncorrectType = 32776L;

	public const long StatusUnavailable = 32777L;

	public const long StatusOutOfMemory = 32778L;

	public const long FileNotFound = 32779L;

	public const long RebootRequired = 32783L;

	public const long NicBindingPrevented = 32786L;

	public const long VhdDifferencingChainCycleDetected = 32787L;

	public const long ObjectNotFound = 32789L;

	public const long VhdChildParentIdMismatch = 32791L;
}
