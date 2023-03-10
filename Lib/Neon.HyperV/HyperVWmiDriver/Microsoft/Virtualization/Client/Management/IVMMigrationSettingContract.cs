using System;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class IVMMigrationSettingContract : IVMMigrationSetting, IVirtualizationManagementObject
{
	public VMMigrationType MigrationType
	{
		get
		{
			return VMMigrationType.Unknown;
		}
		set
		{
		}
	}

	public string DestinationPlannedVirtualSystemId
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public string[] DestinationIPAddressList
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public bool RetainVhdCopiesOnSource
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	public bool EnableCompression
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	public VMMigrationTransportType TransportType
	{
		get
		{
			return (VMMigrationTransportType)0;
		}
		set
		{
		}
	}

	public MoveUnmanagedVhd[] UnmanagedVhds
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public bool RemoveSourceUnmanagedVhds
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

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
