namespace Microsoft.Virtualization.Client.Management;

internal class TerminalServiceSettingView : View, ITerminalServiceSetting, IVirtualizationManagementObject
{
    private static class WmiPropertyNames
    {
        internal const string ListernerPort = "ListenerPort";
    }

    public int ListenerPort => NumberConverter.UInt32ToInt32(GetProperty<uint>("ListenerPort"));
}
