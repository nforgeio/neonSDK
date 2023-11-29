using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal interface IEthernetStatus
{
    string Name { get; }

    string ExtensionId { get; }

    string FeatureId { get; }

    IReadOnlyDictionary<string, object> Properties { get; }
}
