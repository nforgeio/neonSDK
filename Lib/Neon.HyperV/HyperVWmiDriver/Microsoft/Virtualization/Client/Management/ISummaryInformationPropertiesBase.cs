using System;

namespace Microsoft.Virtualization.Client.Management;

internal interface ISummaryInformationPropertiesBase
{
    string Name { get; }

    string ElementName { get; }

    VMComputerSystemState State { get; }

    TimeSpan Uptime { get; }

    VMComputerSystemHealthState HealthState { get; }

    string HostComputerSystemName { get; }

    DateTime CreationTime { get; }

    string Notes { get; }

    string Version { get; }

    bool Shielded { get; }

    bool RdpEnhancedModeAvailable { get; }

    VirtualSystemSubType VirtualSystemSubType { get; }

    VMComputerSystemOperationalStatus[] GetOperationalStatus();

    string[] GetStatusDescriptions();
}
