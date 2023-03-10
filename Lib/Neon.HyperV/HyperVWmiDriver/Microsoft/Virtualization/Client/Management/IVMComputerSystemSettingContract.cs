using System;
using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class IVMComputerSystemSettingContract : IVMComputerSystemSetting, IVirtualizationManagementObject, IPutableAsync, IPutable, IDeleteableAsync, IDeleteable
{
	public string InstanceId => null;

	public string Name
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public string Version
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public string SystemName => null;

	public string ConfigurationId => null;

	public VirtualSystemType VirtualSystemType => VirtualSystemType.RealizedSnapshot;

	public VirtualSystemSubType VirtualSystemSubType
	{
		get
		{
			return VirtualSystemSubType.Unknown;
		}
		set
		{
		}
	}

	public bool IsSaved => false;

	public string Notes
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public string Description
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public DateTime CreationTime => default(DateTime);

	public bool VirtualNumaEnabled
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	public bool BiosNumLock
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	public bool GuestControlledCacheTypes
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	public ushort NetworkBootPreferredProtocol
	{
		get
		{
			return 0;
		}
		set
		{
		}
	}

	public IVMComputerSystemSetting ParentSnapshot => null;

	public IEnumerable<IVMComputerSystemSetting> ChildSettings => null;

	public bool SecureBootEnabled
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	public Guid? SecureBootTemplateId
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public ConsoleModeType ConsoleMode
	{
		get
		{
			return ConsoleModeType.Default;
		}
		set
		{
		}
	}

	public EnhancedSessionTransportType EnhancedSessionTransportType
	{
		get
		{
			return EnhancedSessionTransportType.VMBus;
		}
		set
		{
		}
	}

	public bool LockOnDisconnect
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	public bool PauseAfterBootFailure
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	public uint LowMemoryMappedIoSpace
	{
		get
		{
			return 0u;
		}
		set
		{
		}
	}

	public ulong HighMemoryMappedIoSpace
	{
		get
		{
			return 0uL;
		}
		set
		{
		}
	}

	public string ConfigurationDataRoot
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public string SnapshotDataRoot
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public UserSnapshotType UserSnapshotType
	{
		get
		{
			return (UserSnapshotType)0;
		}
		set
		{
		}
	}

	public string SwapFileDataRoot
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public ServiceStartOperation AutomaticStartupAction
	{
		get
		{
			return (ServiceStartOperation)0;
		}
		set
		{
		}
	}

	public ServiceStopOperation AutomaticShutdownAction
	{
		get
		{
			return (ServiceStopOperation)0;
		}
		set
		{
		}
	}

	public TimeSpan AutomaticStartupActionDelay
	{
		get
		{
			return default(TimeSpan);
		}
		set
		{
		}
	}

	public CriticalErrorAction AutomaticCriticalErrorAction
	{
		get
		{
			return CriticalErrorAction.None;
		}
		set
		{
		}
	}

	public TimeSpan AutomaticCriticalErrorActionTimeout
	{
		get
		{
			return default(TimeSpan);
		}
		set
		{
		}
	}

	public bool IsSnapshot => false;

	public bool IsAutomaticCheckpoint => false;

	public bool EnableAutomaticCheckpoints
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	public IVMComputerSystemBase VMComputerSystem => null;

	public abstract Server Server { get; }

	public abstract WmiObjectPath ManagementPath { get; }

	public event EventHandler Deleted;

	public event EventHandler CacheUpdated;

	public BootDevice[] GetBootOrder()
	{
		return null;
	}

	public void SetBootOrder(BootDevice[] bootOrder)
	{
	}

	public IEnumerable<IVMBootEntry> GetFirmwareBootOrder()
	{
		return null;
	}

	public void SetFirmwareBootOrder(IEnumerable<IVMBootEntry> bootEntries)
	{
	}

	public void Apply()
	{
	}

	public IVMTask BeginApply()
	{
		return null;
	}

	public void EndApply(IVMTask applyTask)
	{
	}

	public IVMTask BeginDeleteTree()
	{
		return null;
	}

	public void EndDeleteTree(IVMTask deleteTask)
	{
	}

	public IVMTask BeginClearSnapshotState()
	{
		return null;
	}

	public void EndClearSnapshotState(IVMTask clearTask)
	{
	}

	public byte[] GetThumbnailImage(int widthPixels, int heightPixels)
	{
		return null;
	}

	public long GetSizeOfSystemFiles()
	{
		return 0L;
	}

	public IEnumerable<IVMDeviceSetting> GetDeviceSettings()
	{
		return null;
	}

	public IEnumerable<IVMDeviceSetting> GetDeviceSettingsLimited(bool update, TimeSpan threshold)
	{
		return null;
	}

	public IVMMemorySetting GetMemorySetting()
	{
		return null;
	}

	public IVMSecuritySetting GetSecuritySetting()
	{
		return null;
	}

	public IVMStorageSetting GetStorageSetting()
	{
		return null;
	}

	public IVMProcessorSetting GetProcessorSetting()
	{
		return null;
	}

	public IVMSyntheticDisplayControllerSetting GetSyntheticDisplayControllerSetting()
	{
		return null;
	}

	public IVMSyntheticKeyboardControllerSetting GetSyntheticKeyboardControllerSetting()
	{
		return null;
	}

	public IVMSyntheticMouseControllerSetting GetSyntheticMouseControllerSetting()
	{
		return null;
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
