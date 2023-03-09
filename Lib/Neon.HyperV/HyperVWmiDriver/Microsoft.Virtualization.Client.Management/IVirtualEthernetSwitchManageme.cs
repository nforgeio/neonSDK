using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class IVirtualEthernetSwitchManagementCapabilitiesContract : IVirtualEthernetSwitchManagementCapabilities, IVirtualizationManagementObject
{
	public bool IOVSupport => false;

	public string[] IOVSupportReasons => null;

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
[WmiName("Msvm_VirtualEthernetSwitchManagementCapabilities")]
internal interface IVirtualEthernetSwitchManagementCapabilities : IVirtualizationManagementObject
{
	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "IOV", Justification = "This is by spec.")]
	bool IOVSupport { get; }

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "IOV", Justification = "This is by spec.")]
	string[] IOVSupportReasons { get; }
}
