using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Virtualization.Client.Management;

internal class PropertyCacheManager
{
	private readonly PropertyCache m_PropertyCache;

	private readonly Proxy m_Proxy;

	private readonly ImePropertyCacheUpdater m_ImeUpdater;

	internal ICimClass CimClass => m_PropertyCache.CimClass;

	public PropertyCacheManager(PropertyCache cache, Proxy proxy)
	{
		if (cache == null)
		{
			throw new ArgumentNullException("cache");
		}
		if (proxy == null)
		{
			throw new ArgumentNullException("proxy");
		}
		m_PropertyCache = cache;
		m_Proxy = proxy;
		m_ImeUpdater = new ImePropertyCacheUpdater(proxy, this);
	}

	public bool TryGetValue(string propertyName, out object value)
	{
		return m_PropertyCache.TryGetValue(propertyName, out value);
	}

	public IEnumerable<string> GetPropertyNames()
	{
		return m_PropertyCache.GetPropertyNames();
	}

	public bool NeedsUpdate(TimeSpan refreshTime)
	{
		return m_PropertyCache.NeedsUpdate(refreshTime);
	}

	public void InvalidateCache()
	{
		m_PropertyCache.InvalidateCache();
	}

	public void UpdateCache(TimeSpan threshold, WmiOperationOptions options)
	{
		if (m_PropertyCache.Update(threshold, options))
		{
			m_Proxy.NotifyPropertyCacheUpdated();
		}
	}

	internal void UpdateCache(ICimInstance cimInstance, WmiOperationOptions options)
	{
		if (m_PropertyCache.Update(cimInstance, options))
		{
			m_Proxy.NotifyPropertyCacheUpdated();
		}
	}

	public void UpdateCache(ICimInstance cimInstance)
	{
		if (m_PropertyCache.Update(cimInstance))
		{
			m_Proxy.NotifyPropertyCacheUpdated();
		}
	}

	public void UpdateCache(IDictionary<string, object> propertyDictionary)
	{
		if (m_PropertyCache.Update(propertyDictionary))
		{
			m_Proxy.NotifyPropertyCacheUpdated();
		}
	}

	public void RegisterForInstanceModificationEvents(InstanceModificationEventStrategy strategy)
	{
		m_ImeUpdater.RegisterForInstanceModificationEvents(strategy);
	}

	public void UnregisterForInstanceModificationEvents()
	{
		m_ImeUpdater.UnregisterForInstanceModificationEvents();
	}

	[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is called via contracts.")]
	private void ObjectInvariant()
	{
	}
}
