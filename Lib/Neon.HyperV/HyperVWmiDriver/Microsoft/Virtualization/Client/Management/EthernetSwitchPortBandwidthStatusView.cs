namespace Microsoft.Virtualization.Client.Management;

internal sealed class EthernetSwitchPortBandwidthStatusView : EthernetPortStatusView, IEthernetSwitchPortBandwidthStatus, IEthernetPortStatus, IEthernetStatus, IVirtualizationManagementObject
{
    internal static class WmiMemberNames
    {
        public const string CurrentBandwidthReservationPercentage = "CurrentBandwidthReservationPercentage";
    }

    public uint CurrentBandwidthReservationPercentage => GetProperty<uint>("CurrentBandwidthReservationPercentage");
}
