namespace Microsoft.Virtualization.Client.Management;

internal interface IProxyFactory
{
    ProxyRepository Repository { get; }

    IProxy GetProxy(ObjectKey key, bool delayInitializePropertyCache = false, ICimInstance cimInstance = null, WmiOperationOptions options = null);

    bool TryGetProxy(ObjectKey key, out IProxy proxy, WmiOperationOptions options = null);
}
