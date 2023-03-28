using System;
using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class IProxyContract : IProxy
{
	public ObjectKey Key => null;

	public WmiObjectPath Path => null;

	public ICimClass CimClass => null;

	public string[] EventWatchAdditionalConditions
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public event EventHandler PropertyCacheUpdated;

	public event EventHandler Deleted;

	public void NotifyPropertyCacheUpdated()
	{
	}

	public object GetProperty(string propertyName)
	{
		return null;
	}

	public IEnumerable<string> GetPropertyNames()
	{
		return null;
	}

	public bool DoesPropertyExist(string propertyName)
	{
		return false;
	}

	public void PutProperties(IDictionary<string, object> propertyDictionary)
	{
	}

	public bool NeedsUpdate(TimeSpan refreshTime)
	{
		return false;
	}

	public void InvalidatePropertyCache()
	{
	}

	public void InvalidateAssociationCache()
	{
	}

	public void UpdatePropertyCache(IDictionary<string, object> propertyDictionary)
	{
	}

	public void UpdatePropertyCache(TimeSpan threshold)
	{
	}

	public void UpdatePropertyCache(TimeSpan threshold, WmiOperationOptions options)
	{
	}

	public void UpdatePropertyCache(ICimInstance cimInstance, WmiOperationOptions options)
	{
	}

	public void RegisterForInstanceModificationEvents(InstanceModificationEventStrategy strategy)
	{
	}

	public void UnregisterForInstanceModificationEvents()
	{
	}

	public void UpdateAssociationCache(TimeSpan threshold, IReadOnlyList<Association> associationsToExclude)
	{
	}

	public void UpdateOneCachedAssociation(Association association, TimeSpan threshold)
	{
	}

	public TReturnType InvokeMethodWithReturn<TReturnType>(string methodName, object[] args)
	{
		return default(TReturnType);
	}

	public void InvokeMethodWithoutReturn(string methodName, object[] args)
	{
	}

	public uint InvokeMethod(string methodName, object[] args)
	{
		return 0u;
	}

	public IEnumerable<ObjectKey> GetRelatedObjectKeys(Association association)
	{
		return null;
	}

	public IEnumerable<ObjectKey> GetRelatedObjectKeys(Association association, WmiOperationOptions options)
	{
		return null;
	}
}
