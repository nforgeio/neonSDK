using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal class ObjectFactory : IObjectFactory
{
	private static readonly IObjectFactory s_Instance = new ObjectFactory();

	internal static IObjectFactory Instance => s_Instance;

	private ObjectFactory()
	{
	}

	T IObjectFactory.GetVirtualizationManagementObject<T>(ObjectKey key, ICimInstance cimInstance, WmiOperationOptions options)
	{
		IProxy proxy = ProxyFactory.Instance.GetProxy(key, delayInitializePropertyCache: false, cimInstance, options);
		return ViewFactory.Instance.CreateView<T>(proxy, key);
	}

	IList<T> IObjectFactory.GetVirtualizationManagementObjects<T>(Server server, string wmiNamespace, string wmiClassName, WmiOperationOptions options)
	{
		return GetVirtualizationManagementObjects<T>(server, wmiNamespace, server.EnumerateInstances(wmiNamespace, wmiClassName, options?.CimOperationOptions), options);
	}

	IList<T> IObjectFactory.QueryVirtualizationManagementObjects<T>(Server server, string wmiNamespace, string wqlQuery, WmiOperationOptions options)
	{
		return GetVirtualizationManagementObjects<T>(server, wmiNamespace, server.QueryInstances(wmiNamespace, wqlQuery, options?.CimOperationOptions), options);
	}

	bool IObjectFactory.TryGetVirtualizationManagementObject<T>(ObjectKey key, out T virtManObj)
	{
		IProxy proxy = null;
		virtManObj = default(T);
		if (ProxyFactory.Instance.TryGetProxy(key, out proxy))
		{
			virtManObj = ViewFactory.Instance.CreateView<T>(proxy, key);
			return true;
		}
		return false;
	}

	private IList<T> GetVirtualizationManagementObjects<T>(Server server, string wmiNamespace, IEnumerable<ICimInstance> cimInstances, WmiOperationOptions options) where T : IVirtualizationManagementObject
	{
		List<T> list = new List<T>();
		foreach (ICimInstance cimInstance2 in cimInstances)
		{
			using (cimInstance2)
			{
				try
				{
					WmiObjectPath path = new WmiObjectPath(server, wmiNamespace, cimInstance2);
					ObjectKey key = new ObjectKey(server, path);
					list.Add(((IObjectFactory)this).GetVirtualizationManagementObject<T>(key, cimInstance2, options));
				}
				catch (NoWmiMappingException)
				{
				}
			}
		}
		return list;
	}
}
