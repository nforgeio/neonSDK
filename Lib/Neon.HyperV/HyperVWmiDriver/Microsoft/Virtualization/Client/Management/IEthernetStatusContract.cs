using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class IEthernetStatusContract : IEthernetStatus
{
    public string Name => null;

    public string ExtensionId => null;

    public string FeatureId => null;

    public IReadOnlyDictionary<string, object> Properties => null;
}
