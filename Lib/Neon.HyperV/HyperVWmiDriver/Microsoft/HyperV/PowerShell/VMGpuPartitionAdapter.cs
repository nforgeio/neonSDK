using Microsoft.HyperV.PowerShell.Common;
using Microsoft.Virtualization.Client.Management;

using ErrorMessages = Microsoft.HyperV.PowerShell.Common.ErrorMessages;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMGpuPartitionAdapter : VMDevice, IRemovable
{
	private readonly DataUpdater<IVMGpuPartitionAdapterSetting> m_AdapterSetting;

	public ulong? MinPartitionVRAM
	{
		get
		{
			return m_AdapterSetting.GetData(UpdatePolicy.EnsureUpdated).MinPartitionVRAM;
		}
		internal set
		{
			m_AdapterSetting.GetData(UpdatePolicy.None).MinPartitionVRAM = value;
		}
	}

	public ulong? MaxPartitionVRAM
	{
		get
		{
			return m_AdapterSetting.GetData(UpdatePolicy.EnsureUpdated).MaxPartitionVRAM;
		}
		internal set
		{
			m_AdapterSetting.GetData(UpdatePolicy.None).MaxPartitionVRAM = value;
		}
	}

	public ulong? OptimalPartitionVRAM
	{
		get
		{
			return m_AdapterSetting.GetData(UpdatePolicy.EnsureUpdated).OptimalPartitionVRAM;
		}
		internal set
		{
			m_AdapterSetting.GetData(UpdatePolicy.None).OptimalPartitionVRAM = value;
		}
	}

	public ulong? MinPartitionEncode
	{
		get
		{
			return m_AdapterSetting.GetData(UpdatePolicy.EnsureUpdated).MinPartitionEncode;
		}
		internal set
		{
			m_AdapterSetting.GetData(UpdatePolicy.None).MinPartitionEncode = value;
		}
	}

	public ulong? MaxPartitionEncode
	{
		get
		{
			return m_AdapterSetting.GetData(UpdatePolicy.EnsureUpdated).MaxPartitionEncode;
		}
		internal set
		{
			m_AdapterSetting.GetData(UpdatePolicy.None).MaxPartitionEncode = value;
		}
	}

	public ulong? OptimalPartitionEncode
	{
		get
		{
			return m_AdapterSetting.GetData(UpdatePolicy.EnsureUpdated).OptimalPartitionEncode;
		}
		internal set
		{
			m_AdapterSetting.GetData(UpdatePolicy.None).OptimalPartitionEncode = value;
		}
	}

	public ulong? MinPartitionDecode
	{
		get
		{
			return m_AdapterSetting.GetData(UpdatePolicy.EnsureUpdated).MinPartitionDecode;
		}
		internal set
		{
			m_AdapterSetting.GetData(UpdatePolicy.None).MinPartitionDecode = value;
		}
	}

	public ulong? MaxPartitionDecode
	{
		get
		{
			return m_AdapterSetting.GetData(UpdatePolicy.EnsureUpdated).MaxPartitionDecode;
		}
		internal set
		{
			m_AdapterSetting.GetData(UpdatePolicy.None).MaxPartitionDecode = value;
		}
	}

	public ulong? OptimalPartitionDecode
	{
		get
		{
			return m_AdapterSetting.GetData(UpdatePolicy.EnsureUpdated).OptimalPartitionDecode;
		}
		internal set
		{
			m_AdapterSetting.GetData(UpdatePolicy.None).OptimalPartitionDecode = value;
		}
	}

	public ulong? MinPartitionCompute
	{
		get
		{
			return m_AdapterSetting.GetData(UpdatePolicy.EnsureUpdated).MinPartitionCompute;
		}
		internal set
		{
			m_AdapterSetting.GetData(UpdatePolicy.None).MinPartitionCompute = value;
		}
	}

	public ulong? MaxPartitionCompute
	{
		get
		{
			return m_AdapterSetting.GetData(UpdatePolicy.EnsureUpdated).MaxPartitionCompute;
		}
		internal set
		{
			m_AdapterSetting.GetData(UpdatePolicy.None).MaxPartitionCompute = value;
		}
	}

	public ulong? OptimalPartitionCompute
	{
		get
		{
			return m_AdapterSetting.GetData(UpdatePolicy.EnsureUpdated).OptimalPartitionCompute;
		}
		internal set
		{
			m_AdapterSetting.GetData(UpdatePolicy.None).OptimalPartitionCompute = value;
		}
	}

	internal override string PutDescription => TaskDescriptions.SetVMGpuPartitionAdapter;

	internal VMGpuPartitionAdapter(IVMGpuPartitionAdapterSetting setting, VirtualMachineBase parentVirtualMachineObject)
		: base(setting, parentVirtualMachineObject)
	{
		m_AdapterSetting = InitializePrimaryDataUpdater(setting);
	}

	void IRemovable.Remove(IOperationWatcher operationWatcher)
	{
		IVMGpuPartitionAdapterSetting data = m_AdapterSetting.GetData(UpdatePolicy.None);
		RemoveInternal(data, TaskDescriptions.RemoveVMGpuPartitionAdapter, operationWatcher);
	}

	internal override IDataUpdater<IVMDeviceSetting> GetDeviceDataUpdater()
	{
		return m_AdapterSetting;
	}

	internal static VMGpuPartitionAdapter AddGpuPartitionAdapter(VirtualMachine vm, VMGpuPartitionAdapter gpuPartitionAdapter, IOperationWatcher operationWatcher)
	{
		IVMGpuPartitionAdapterSetting dataAs = gpuPartitionAdapter.m_AdapterSetting.GetDataAs<IVMGpuPartitionAdapterSetting>(UpdatePolicy.None);
		return new VMGpuPartitionAdapter(vm.AddDeviceSetting(dataAs, TaskDescriptions.AddVMGpuPartitionAdapter, operationWatcher), vm);
	}

	internal static VMGpuPartitionAdapter CreateTemplateGpuPartitionAdapter(VirtualMachine vm)
	{
		return new VMGpuPartitionAdapter((IVMGpuPartitionAdapterSetting)GetGpuPartitionResourcePool(vm.Server).GetCapabilities(Capabilities.DefaultCapability), vm);
	}

	private static IGpuPartitionResourcePool GetGpuPartitionResourcePool(Server host)
	{
		try
		{
			return (IGpuPartitionResourcePool)ObjectLocator.GetHostComputerSystem(host).GetPrimordialResourcePool(VMDeviceSettingType.GpuPartition);
		}
		catch (ObjectNotFoundException innerException)
		{
			throw ExceptionHelper.CreateObjectNotFoundException(ErrorMessages.VMGpuPartitionAdapter_GpuPartitionPoolNotFound, innerException);
		}
	}
}
