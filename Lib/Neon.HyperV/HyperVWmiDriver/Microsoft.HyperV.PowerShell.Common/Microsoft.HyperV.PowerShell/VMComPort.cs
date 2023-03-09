using System.Diagnostics.CodeAnalysis;
using Microsoft.HyperV.PowerShell.Common;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

[SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ComPort", Justification = "This is by spec.")]
internal sealed class VMComPort : VMDevice
{
	private readonly DataUpdater<IVMSerialPortSetting> m_ComPortSetting;

	public string Path
	{
		get
		{
			return m_ComPortSetting.GetData(UpdatePolicy.EnsureUpdated).AttachedResourcePath;
		}
		internal set
		{
			m_ComPortSetting.GetData(UpdatePolicy.None).AttachedResourcePath = value;
		}
	}

	public OnOffState DebuggerMode
	{
		get
		{
			return m_ComPortSetting.GetData(UpdatePolicy.EnsureUpdated).DebuggerMode.ToOnOffState();
		}
		internal set
		{
			m_ComPortSetting.GetData(UpdatePolicy.None).DebuggerMode = value.ToBool();
		}
	}

	internal override string PutDescription => TaskDescriptions.SetVMComPort;

	internal VMComPort(IVMSerialPortSetting setting, VirtualMachineBase parentVirtualMachineObject)
		: base(setting, parentVirtualMachineObject)
	{
		m_ComPortSetting = InitializePrimaryDataUpdater(setting);
	}

	internal override IDataUpdater<IVMDeviceSetting> GetDeviceDataUpdater()
	{
		return m_ComPortSetting;
	}
}
