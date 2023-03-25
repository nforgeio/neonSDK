using System;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class IVMSecurityServiceContract : IVMSecurityService, IVirtualizationManagementObject
{
	public abstract Server Server { get; }

	public abstract WmiObjectPath ManagementPath { get; }

	public event EventHandler Deleted;

	public event EventHandler CacheUpdated;

	public IVMTask BeginSetKeyProtector(IVMSecuritySetting securitySettingData, byte[] rawKeyProtector)
	{
		return null;
	}

	public void EndSetKeyProtector(IVMTask task)
	{
	}

	public byte[] GetKeyProtector(IVMSecuritySetting securitySettingData)
	{
		return null;
	}

	public IVMTask BeginRestoreLastKnownGoodKeyProtector(IVMSecuritySetting securitySettingData)
	{
		return null;
	}

	public void EndRestoreLastKnownGoodKeyProtector(IVMTask task)
	{
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
