using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.HyperV.PowerShell.Common;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMBios : VMComponentObject, IUpdatable
{
	private readonly DataUpdater<IVMComputerSystemSetting> m_VMSetting;

	public bool NumLockEnabled
	{
		get
		{
			return m_VMSetting.GetData(UpdatePolicy.EnsureUpdated).BiosNumLock;
		}
		internal set
		{
			m_VMSetting.GetData(UpdatePolicy.None).BiosNumLock = value;
		}
	}

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
	public BootDevice[] StartupOrder
	{
		get
		{
			return (from bootEntry in m_VMSetting.GetData(UpdatePolicy.EnsureUpdated).GetBootOrder()
				select (BootDevice)bootEntry).ToArray();
		}
		internal set
		{
			m_VMSetting.GetData(UpdatePolicy.None).SetBootOrder(value.Select((BootDevice bootEntry) => (Microsoft.Virtualization.Client.Management.BootDevice)bootEntry).ToArray());
		}
	}

	internal VMBios(IVMComputerSystemSetting vmSetting, VirtualMachineBase parentVirtualMachineObject)
		: base(vmSetting, parentVirtualMachineObject)
	{
		m_VMSetting = InitializePrimaryDataUpdater(vmSetting);
	}

	public static BootDevice TranslateBootDeviceValue(BootDevice newBootDevice)
	{
		return newBootDevice switch
		{
			BootDevice.NetworkAdapter => BootDevice.LegacyNetworkAdapter, 
			BootDevice.VHD => BootDevice.IDE, 
			_ => newBootDevice, 
		};
	}

	void IUpdatable.Put(IOperationWatcher operationWatcher)
	{
		operationWatcher.PerformPut(m_VMSetting.GetData(UpdatePolicy.None), TaskDescriptions.SetVMBios, this);
	}
}
