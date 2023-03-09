using System;
using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class IVMCollectionContract : IVMCollection, IHyperVCollection, IVirtualizationManagementObject, IPutableAsync, IPutable, IDeleteableAsync, IDeleteable
{
	public IEnumerable<IVMComputerSystem> CollectedVirtualMachines => null;

	public abstract Guid InstanceId { get; }

	public abstract string Name { get; set; }

	public abstract CollectionType Type { get; }

	public abstract Server Server { get; }

	public abstract WmiObjectPath ManagementPath { get; }

	public event EventHandler Deleted;

	public event EventHandler CacheUpdated;

	public IVMTask BeginAddVirtualMachine(IVMComputerSystemBase virtualMachine)
	{
		return null;
	}

	public void EndAddVirtualMachine(IVMTask task)
	{
	}

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

	public abstract IVMTask BeginDelete();

	public abstract void EndDelete(IVMTask deleteTask);

	public abstract void Delete();
}
