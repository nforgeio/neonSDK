using System;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class IMetricValueContract : IMetricValue, IVirtualizationManagementObject
{
	public TimeSpan Duration => default(TimeSpan);

	public ulong IntegerValue => 0uL;

	public string RawValue => null;

	public string BreakdownValue => null;

	public MetricType MetricType => MetricType.Unknown;

	public abstract Server Server { get; }

	public abstract WmiObjectPath ManagementPath { get; }

	public event EventHandler Deleted;

	public event EventHandler CacheUpdated;

	public abstract void InvalidatePropertyCache();

	public abstract void UpdatePropertyCache();

	public abstract void UpdatePropertyCache(TimeSpan threshold);

	public abstract void RegisterForInstanceModificationEvents(InstanceModificationEventStrategy strategy);

	public abstract void UnregisterForInstanceModificationEvents();

	public abstract void InvalidateAssociationCache();

	public abstract void UpdateAssociationCache();

	public abstract void UpdateAssociationCache(TimeSpan threshold);

	public abstract string GetEmbeddedInstance();

	public abstract void DiscardPendingPropertyChanges();
}
