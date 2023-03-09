using System;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class IVMSecuritySettingContract : IVMSecuritySetting, IVirtualizationManagementObject, IPutableAsync, IPutable
{
	public bool TpmEnabled { get; set; }

	public bool KsdEnabled { get; set; }

	public bool ShieldingRequested { get; set; }

	public bool EncryptStateAndVmMigrationTraffic { get; set; }

	public bool DataProtectionRequested { get; set; }

	public bool VirtualizationBasedSecurityOptOut { get; set; }

	public bool BindToHostTpm { get; set; }

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
