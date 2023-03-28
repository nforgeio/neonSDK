using System;
using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class IHostComputerSystemBaseContract : IHostComputerSystemBase, IVirtualizationManagementObject
{
	public IVMService VirtualizationService => null;

	public abstract Server Server { get; }

	public abstract WmiObjectPath ManagementPath { get; }

	public event VMComputersSystemCreatedEventHandler VMComputerSystemCreated;

	public event VMVirtualizationTaskCreatedEventHandler VMVirtualizationTaskCreated;

	public event EventHandler Deleted;

	public event EventHandler CacheUpdated;

	public IList<ISummaryInformation> GetAllSummaryInformation(SummaryInformationRequest requestedInformation)
	{
		return null;
	}

	public IList<ISummaryInformation> GetSummaryInformation(IList<IVMComputerSystem> vmList, SummaryInformationRequest requestedInformation)
	{
		return null;
	}

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
