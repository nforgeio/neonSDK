using System;
using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal interface IProxy
{
	ObjectKey Key { get; }

	WmiObjectPath Path { get; }

	ICimClass CimClass { get; }

	string[] EventWatchAdditionalConditions { get; set; }

	event EventHandler PropertyCacheUpdated;

	event EventHandler Deleted;

	void NotifyPropertyCacheUpdated();

	object GetProperty(string propertyName);

	IEnumerable<string> GetPropertyNames();

	bool DoesPropertyExist(string propertyName);

	void PutProperties(IDictionary<string, object> propertyDictionary);

	bool NeedsUpdate(TimeSpan refreshTime);

	void InvalidatePropertyCache();

	void InvalidateAssociationCache();

	void UpdatePropertyCache(IDictionary<string, object> propertyDictionary);

	void UpdatePropertyCache(TimeSpan threshold);

	void UpdatePropertyCache(TimeSpan threshold, WmiOperationOptions options);

	void UpdatePropertyCache(ICimInstance cimInstance, WmiOperationOptions options);

	void RegisterForInstanceModificationEvents(InstanceModificationEventStrategy strategy);

	void UnregisterForInstanceModificationEvents();

	void UpdateAssociationCache(TimeSpan threshold, IReadOnlyList<Association> associationsToExclude);

	void UpdateOneCachedAssociation(Association association, TimeSpan threshold);

	TReturnType InvokeMethodWithReturn<TReturnType>(string methodName, object[] args);

	void InvokeMethodWithoutReturn(string methodName, object[] args);

	uint InvokeMethod(string methodName, object[] args);

	IEnumerable<ObjectKey> GetRelatedObjectKeys(Association association, WmiOperationOptions options);
}
