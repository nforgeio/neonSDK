namespace Microsoft.Virtualization.Client.Management;

internal static class Capabilities
{
    private static readonly SettingsDefineCapabilities gm_MaxCapability = new SettingsDefineCapabilities(CapabilitiesValueRole.Supported, CapabilitiesValueRange.Maximums, "Maximum");

    private static readonly SettingsDefineCapabilities gm_MinCapability = new SettingsDefineCapabilities(CapabilitiesValueRole.Supported, CapabilitiesValueRange.Minimums, "Minimum");

    private static readonly SettingsDefineCapabilities gm_DefaultCapability = new SettingsDefineCapabilities(CapabilitiesValueRole.Default, CapabilitiesValueRange.Point, "Default");

    public static SettingsDefineCapabilities MaxCapability => gm_MaxCapability;

    public static SettingsDefineCapabilities MinCapability => gm_MinCapability;

    public static SettingsDefineCapabilities DefaultCapability => gm_DefaultCapability;
}
