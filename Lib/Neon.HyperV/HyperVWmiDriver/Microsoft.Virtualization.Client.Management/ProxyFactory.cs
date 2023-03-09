using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Virtualization.Client.Management;

internal class ProxyFactory : IProxyFactory
{
	private static readonly IProxyFactory s_Instance = new ProxyFactory();

	private readonly ProxyRepository _proxyRepository = new ProxyRepository();

	internal static IProxyFactory Instance => s_Instance;

	ProxyRepository IProxyFactory.Repository => _proxyRepository;

	private ProxyFactory()
	{
	}

	IProxy IProxyFactory.GetProxy(ObjectKey key, bool delayInitializePropertyCache, ICimInstance cimInstance, WmiOperationOptions options)
	{
		IProxy proxy = null;
		if (_proxyRepository.TryGetProxy(key, out proxy))
		{
			if (cimInstance != null)
			{
				proxy.UpdatePropertyCache(cimInstance, options);
			}
		}
		else
		{
			proxy = new Proxy(key, delayInitializePropertyCache, cimInstance);
			_proxyRepository.RegisterProxy(ref proxy);
		}
		return proxy;
	}

	bool IProxyFactory.TryGetProxy(ObjectKey key, out IProxy proxy, WmiOperationOptions options)
	{
		bool flag = _proxyRepository.TryGetProxy(key, out proxy);
		if (!flag)
		{
			flag = key.Server.TryGetInstance(key.ManagementPath, options?.CimOperationOptions, out var cimInstance);
			if (flag)
			{
				using (cimInstance)
				{
					proxy = new Proxy(key, delayInitializePropertyCache: false, cimInstance);
					return flag;
				}
			}
		}
		return flag;
	}

	[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is called via contracts.")]
	private void ObjectInvariant()
	{
	}
}
