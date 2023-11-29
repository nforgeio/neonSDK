using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Virtualization.Client.Management;

internal class SettingsDefineCapabilities
{
    private readonly string m_Description;

    internal SettingsDefineCapabilities(CapabilitiesValueRole role, CapabilitiesValueRange range, string description)
    {
        m_Description = description;
    }

    internal bool MatchByDescription(string instanceId)
    {
        return instanceId.IndexOf(m_Description, StringComparison.OrdinalIgnoreCase) != -1;
    }

    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is called via contracts.")]
    private void ObjectInvariant()
    {
    }
}
