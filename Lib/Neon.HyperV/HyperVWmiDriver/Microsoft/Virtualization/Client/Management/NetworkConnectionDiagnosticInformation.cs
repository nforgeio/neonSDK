namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_NetworkConnectionDiagnosticInformation")]
internal class NetworkConnectionDiagnosticInformation : EmbeddedInstance
{
	internal static class WmiPropertyNames
	{
		public const string RoundTripTime = "RoundTripTime";
	}

	public uint RoundTripTime => GetProperty("RoundTripTime", 0u);
}
