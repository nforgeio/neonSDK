namespace Microsoft.Virtualization.Client.Management;

internal sealed class EthernetSwitchBandwidthStatusView : EthernetSwitchStatusView, IEthernetSwitchBandwidthStatus, IEthernetSwitchStatus, IEthernetStatus, IVirtualizationManagementObject
{
	internal static class WmiMemberNames
	{
		public const string DefaultFlowReservation = "DefaultFlowReservation";

		public const string DefaultFlowReservationPercentage = "DefaultFlowReservationPercentage";

		public const string DefaultFlowWeight = "DefaultFlowWeight";
	}

	public ulong DefaultFlowReservation => GetProperty<ulong>("DefaultFlowReservation");

	public uint DefaultFlowReservationPercentage => GetProperty<uint>("DefaultFlowReservationPercentage");

	public ulong DefaultFlowWeight => GetProperty<ulong>("DefaultFlowWeight");
}
