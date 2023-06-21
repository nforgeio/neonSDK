using System;

namespace Microsoft.Virtualization.Client.Management;

internal interface ISummaryInformationBase : ISummaryInformationPropertiesBase, IVirtualizationManagementObject
{
    void UpdatePropertyCache(SummaryInformationRequest requestedInformation);

    void UpdatePropertyCache(TimeSpan threshold, SummaryInformationRequest requestedInformation);
}
