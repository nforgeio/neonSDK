using Microsoft.HyperV.PowerShell.Common;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMMemory : VMDevice
{
	private readonly DataUpdater<IVMMemorySetting> m_MemorySetting;

	public string ResourcePoolName
	{
		get
		{
			string poolId = m_MemorySetting.GetData(UpdatePolicy.EnsureUpdated).PoolId;
			if (!string.IsNullOrEmpty(poolId))
			{
				return poolId;
			}
			return "Primordial";
		}
		internal set
		{
			string text2 = (m_MemorySetting.GetData(UpdatePolicy.None).PoolId = (VMResourcePool.IsPrimordialPoolName(value) ? string.Empty : value));
		}
	}

	public int Buffer
	{
		get
		{
			return m_MemorySetting.GetData(UpdatePolicy.EnsureUpdated).TargetMemoryBuffer;
		}
		internal set
		{
			m_MemorySetting.GetData(UpdatePolicy.None).TargetMemoryBuffer = value;
		}
	}

	public bool DynamicMemoryEnabled
	{
		get
		{
			return m_MemorySetting.GetData(UpdatePolicy.EnsureUpdated).IsDynamicMemoryEnabled;
		}
		internal set
		{
			m_MemorySetting.GetData(UpdatePolicy.None).IsDynamicMemoryEnabled = value;
		}
	}

	public long Maximum
	{
		get
		{
			return m_MemorySetting.GetData(UpdatePolicy.EnsureUpdated).MaximumMemory * Constants.Mega;
		}
		internal set
		{
			m_MemorySetting.GetData(UpdatePolicy.None).MaximumMemory = value / Constants.Mega;
		}
	}

	public long MaximumPerNumaNode
	{
		get
		{
			return m_MemorySetting.GetData(UpdatePolicy.EnsureUpdated).MaximumMemoryPerNumaNode;
		}
		internal set
		{
			m_MemorySetting.GetData(UpdatePolicy.None).MaximumMemoryPerNumaNode = value;
		}
	}

	public long Minimum
	{
		get
		{
			return m_MemorySetting.GetData(UpdatePolicy.EnsureUpdated).MinimumMemory * Constants.Mega;
		}
		internal set
		{
			m_MemorySetting.GetData(UpdatePolicy.None).MinimumMemory = value / Constants.Mega;
		}
	}

	public int Priority
	{
		get
		{
			return m_MemorySetting.GetData(UpdatePolicy.EnsureUpdated).PriorityLevel / 100;
		}
		internal set
		{
			m_MemorySetting.GetData(UpdatePolicy.None).PriorityLevel = value * 100;
		}
	}

	public long Startup
	{
		get
		{
			return m_MemorySetting.GetData(UpdatePolicy.EnsureUpdated).AllocatedRam * Constants.Mega;
		}
		internal set
		{
			m_MemorySetting.GetData(UpdatePolicy.None).AllocatedRam = value / Constants.Mega;
		}
	}

	internal override string PutDescription => TaskDescriptions.SetVMMemory;

	internal VMMemory(IVMMemorySetting setting, ComputeResource parentComputeResource)
		: base(setting, parentComputeResource)
	{
		m_MemorySetting = InitializePrimaryDataUpdater(setting);
	}

	internal override IDataUpdater<IVMDeviceSetting> GetDeviceDataUpdater()
	{
		return m_MemorySetting;
	}
}
