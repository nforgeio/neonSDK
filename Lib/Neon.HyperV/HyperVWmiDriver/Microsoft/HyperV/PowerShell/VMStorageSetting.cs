using Microsoft.HyperV.PowerShell.Common;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMStorageSetting : VMComponentObject, IUpdatable
{
	private readonly DataUpdater<IVMStorageSetting> m_StorageSetting;

	public ThreadCount ThreadCountPerChannel
	{
		get
		{
			return (ThreadCount)m_StorageSetting.GetData(UpdatePolicy.EnsureUpdated).ThreadCountPerChannel;
		}
		internal set
		{
			m_StorageSetting.GetData(UpdatePolicy.None).ThreadCountPerChannel = (ushort)value;
		}
	}

	public ushort VirtualProcessorsPerChannel
	{
		get
		{
			return m_StorageSetting.GetData(UpdatePolicy.EnsureUpdated).VirtualProcessorsPerChannel;
		}
		internal set
		{
			m_StorageSetting.GetData(UpdatePolicy.None).VirtualProcessorsPerChannel = value;
		}
	}

	public bool DisableInterruptBatching
	{
		get
		{
			return m_StorageSetting.GetData(UpdatePolicy.EnsureUpdated).DisableInterruptBatching;
		}
		internal set
		{
			m_StorageSetting.GetData(UpdatePolicy.None).DisableInterruptBatching = value;
		}
	}

	internal VMStorageSetting(IVMStorageSetting storageSetting, VirtualMachineBase parentVirtualMachineObject)
		: base(storageSetting, parentVirtualMachineObject)
	{
		m_StorageSetting = InitializePrimaryDataUpdater(storageSetting);
	}

	void IUpdatable.Put(IOperationWatcher operationWatcher)
	{
		IVMStorageSetting data = m_StorageSetting.GetData(UpdatePolicy.None);
		operationWatcher.PerformPut(data, TaskDescriptions.SetVMStorageSettings, this);
	}
}
