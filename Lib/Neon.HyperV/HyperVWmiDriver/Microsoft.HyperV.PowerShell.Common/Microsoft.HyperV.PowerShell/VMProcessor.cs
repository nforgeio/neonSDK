using System.Collections.Generic;
using System.Linq;
using Microsoft.HyperV.PowerShell.Common;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMProcessor : VMDevice
{
	private const int gm_ProcessorUsageMaximumValue = 100000;

	private readonly DataUpdater<IVMProcessorSetting> m_ProcessorSetting;

	public string ResourcePoolName
	{
		get
		{
			string poolId = m_ProcessorSetting.GetData(UpdatePolicy.EnsureUpdated).PoolId;
			if (!string.IsNullOrEmpty(poolId))
			{
				return poolId;
			}
			return "Primordial";
		}
		internal set
		{
			string text2 = (m_ProcessorSetting.GetData(UpdatePolicy.None).PoolId = (VMResourcePool.IsPrimordialPoolName(value) ? string.Empty : value));
		}
	}

	public long Count
	{
		get
		{
			return m_ProcessorSetting.GetData(UpdatePolicy.EnsureUpdated).VirtualQuantity;
		}
		internal set
		{
			m_ProcessorSetting.GetData(UpdatePolicy.None).VirtualQuantity = (int)value;
		}
	}

	public bool CompatibilityForMigrationEnabled
	{
		get
		{
			return m_ProcessorSetting.GetData(UpdatePolicy.EnsureUpdated).LimitProcessorFeatures;
		}
		internal set
		{
			m_ProcessorSetting.GetData(UpdatePolicy.None).LimitProcessorFeatures = value;
		}
	}

	public bool CompatibilityForOlderOperatingSystemsEnabled
	{
		get
		{
			return m_ProcessorSetting.GetData(UpdatePolicy.EnsureUpdated).LimitCpuId;
		}
		internal set
		{
			m_ProcessorSetting.GetData(UpdatePolicy.None).LimitCpuId = value;
		}
	}

	public long HwThreadCountPerCore
	{
		get
		{
			return m_ProcessorSetting.GetData(UpdatePolicy.EnsureUpdated).HwThreadsPerCore.Value;
		}
		internal set
		{
			m_ProcessorSetting.GetData(UpdatePolicy.None).HwThreadsPerCore = value;
		}
	}

	public bool ExposeVirtualizationExtensions
	{
		get
		{
			return m_ProcessorSetting.GetData(UpdatePolicy.EnsureUpdated).ExposeVirtualizationExtensions;
		}
		internal set
		{
			m_ProcessorSetting.GetData(UpdatePolicy.None).ExposeVirtualizationExtensions = value;
		}
	}

	public bool EnablePerfmonPmu
	{
		get
		{
			return m_ProcessorSetting.GetData(UpdatePolicy.EnsureUpdated).EnablePerfmonPmu;
		}
		internal set
		{
			m_ProcessorSetting.GetData(UpdatePolicy.None).EnablePerfmonPmu = value;
		}
	}

	public bool EnablePerfmonLbr
	{
		get
		{
			return m_ProcessorSetting.GetData(UpdatePolicy.EnsureUpdated).EnablePerfmonLbr;
		}
		internal set
		{
			m_ProcessorSetting.GetData(UpdatePolicy.None).EnablePerfmonLbr = value;
		}
	}

	public bool EnablePerfmonPebs
	{
		get
		{
			return m_ProcessorSetting.GetData(UpdatePolicy.EnsureUpdated).EnablePerfmonPebs;
		}
		internal set
		{
			m_ProcessorSetting.GetData(UpdatePolicy.None).EnablePerfmonPebs = value;
		}
	}

	public bool EnablePerfmonIpt
	{
		get
		{
			return m_ProcessorSetting.GetData(UpdatePolicy.EnsureUpdated).EnablePerfmonIpt;
		}
		internal set
		{
			m_ProcessorSetting.GetData(UpdatePolicy.None).EnablePerfmonIpt = value;
		}
	}

	public bool EnableLegacyApicMode
	{
		get
		{
			return m_ProcessorSetting.GetData(UpdatePolicy.EnsureUpdated).EnableLegacyApicMode;
		}
		internal set
		{
			m_ProcessorSetting.GetData(UpdatePolicy.None).EnableLegacyApicMode = value;
		}
	}

	public bool AllowACountMCount
	{
		get
		{
			return m_ProcessorSetting.GetData(UpdatePolicy.EnsureUpdated).AllowACountMCount;
		}
		internal set
		{
			m_ProcessorSetting.GetData(UpdatePolicy.None).AllowACountMCount = value;
		}
	}

	public long Maximum
	{
		get
		{
			return m_ProcessorSetting.GetData(UpdatePolicy.EnsureUpdated).Limit * 100 / 100000;
		}
		internal set
		{
			m_ProcessorSetting.GetData(UpdatePolicy.None).Limit = (int)(value * 100000 / 100);
		}
	}

	public long Reserve
	{
		get
		{
			return m_ProcessorSetting.GetData(UpdatePolicy.EnsureUpdated).Reservation * 100 / 100000;
		}
		internal set
		{
			m_ProcessorSetting.GetData(UpdatePolicy.None).Reservation = (int)(value * 100000 / 100);
		}
	}

	public int RelativeWeight
	{
		get
		{
			return m_ProcessorSetting.GetData(UpdatePolicy.EnsureUpdated).Weight;
		}
		internal set
		{
			m_ProcessorSetting.GetData(UpdatePolicy.None).Weight = value;
		}
	}

	public long MaximumCountPerNumaNode
	{
		get
		{
			return m_ProcessorSetting.GetData(UpdatePolicy.EnsureUpdated).MaxProcessorsPerNumaNode;
		}
		internal set
		{
			m_ProcessorSetting.GetData(UpdatePolicy.None).MaxProcessorsPerNumaNode = value;
		}
	}

	public long MaximumCountPerNumaSocket
	{
		get
		{
			return m_ProcessorSetting.GetData(UpdatePolicy.EnsureUpdated).MaxNumaNodesPerSocket;
		}
		internal set
		{
			m_ProcessorSetting.GetData(UpdatePolicy.None).MaxNumaNodesPerSocket = value;
		}
	}

	public bool EnableHostResourceProtection
	{
		get
		{
			return m_ProcessorSetting.GetData(UpdatePolicy.EnsureUpdated).EnableHostResourceProtection;
		}
		internal set
		{
			m_ProcessorSetting.GetData(UpdatePolicy.None).EnableHostResourceProtection = value;
		}
	}

	public IReadOnlyList<VMProcessorOperationalStatus> OperationalStatus
	{
		get
		{
			if (m_ProcessorSetting.GetData(UpdatePolicy.EnsureAssociatorsUpdated).VirtualDevice is IVMProcessor iVMProcessor)
			{
				return (from VMProcessorOperationalStatus status in iVMProcessor.GetOperationalStatus()
					select (status)).ToArray();
			}
			return new VMProcessorOperationalStatus[0];
		}
	}

	public IReadOnlyList<string> StatusDescription
	{
		get
		{
			if (m_ProcessorSetting.GetData(UpdatePolicy.EnsureAssociatorsUpdated).VirtualDevice is IVMProcessor iVMProcessor)
			{
				return iVMProcessor.GetOperationalStatusDescriptions();
			}
			return new string[0];
		}
	}

	internal override string PutDescription => TaskDescriptions.SetVMProcessor;

	internal VMProcessor(IVMProcessorSetting setting, ComputeResource parentComputeResource)
		: base(setting, parentComputeResource)
	{
		m_ProcessorSetting = InitializePrimaryDataUpdater(setting);
	}

	internal override IDataUpdater<IVMDeviceSetting> GetDeviceDataUpdater()
	{
		return m_ProcessorSetting;
	}
}
