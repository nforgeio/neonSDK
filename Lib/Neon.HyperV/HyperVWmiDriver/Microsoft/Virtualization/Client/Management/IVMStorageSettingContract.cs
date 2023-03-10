using System;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class IVMStorageSettingContract : IVMStorageSetting, IVirtualizationManagementObject, IPutableAsync, IPutable
{
	public ushort ThreadCountPerChannel { get; set; }

	public ushort VirtualProcessorsPerChannel { get; set; }

	public bool DisableInterruptBatching { get; set; }

	public abstract Server Server { get; }

	public abstract WmiObjectPath ManagementPath { get; }

	public event EventHandler Deleted;

	public event EventHandler CacheUpdated;

	public abstract IVMTask BeginPut();

	public abstract void EndPut(IVMTask putTask);

	public abstract void Put();

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
