namespace Microsoft.HyperV.PowerShell;

internal sealed class VMNetworkAdapterConnectionTestResult
{
	public int RoundTripTime { get; private set; }

	internal VMNetworkAdapterConnectionTestResult(int roundTripTime)
	{
		RoundTripTime = roundTripTime;
	}
}
