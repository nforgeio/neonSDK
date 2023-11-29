using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class EthernetStatusView : View, IEthernetStatus
{
    public string Name => GetProperty<string>("ElementName");

    public string ExtensionId => EthernetFeatureView.GetExtensionId(base.Proxy.CimClass);

    public string FeatureId => EthernetFeatureView.GetFeatureId(base.Proxy.CimClass);

    public IReadOnlyDictionary<string, object> Properties => GetPropertyNames().ToDictionary((string name) => name, base.GetProperty<object>);
}
