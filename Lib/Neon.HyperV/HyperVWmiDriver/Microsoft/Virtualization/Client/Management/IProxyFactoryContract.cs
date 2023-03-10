namespace Microsoft.Virtualization.Client.Management;

internal abstract class IProxyFactoryContract : IProxyFactory
{
	ProxyRepository IProxyFactory.Repository => null;

	IProxy IProxyFactory.GetProxy(ObjectKey key, bool delayInitializePropertyCache, ICimInstance cimInstance, WmiOperationOptions options)
	{
		return null;
	}

	bool IProxyFactory.TryGetProxy(ObjectKey key, out IProxy proxy, WmiOperationOptions options)
	{
		proxy = null;
		return false;
	}
}
