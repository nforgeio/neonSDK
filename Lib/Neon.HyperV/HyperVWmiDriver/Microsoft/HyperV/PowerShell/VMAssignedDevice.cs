using System.Linq;
using Microsoft.HyperV.PowerShell.Common;
using Microsoft.Virtualization.Client.Management;

using ErrorMessages = Microsoft.HyperV.PowerShell.Common.ErrorMessages;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMAssignedDevice : VMDevice, IRemovable
{
	private readonly DataUpdater<IVMAssignableDeviceSetting> m_DeviceSetting;

	internal override string PutDescription => TaskDescriptions.SetVMAssignedDevice;

	public string InstanceID
	{
		get
		{
			IVMAssignableDevice physicalDevice = m_DeviceSetting.GetData(UpdatePolicy.EnsureUpdated).GetPhysicalDevice();
			if (physicalDevice == null)
			{
				return string.Empty;
			}
			return physicalDevice.DeviceInstancePath;
		}
	}

	public string LocationPath
	{
		get
		{
			IVMAssignableDevice physicalDevice = m_DeviceSetting.GetData(UpdatePolicy.EnsureUpdated).GetPhysicalDevice();
			if (physicalDevice == null)
			{
				return string.Empty;
			}
			return physicalDevice.LocationPath;
		}
	}

	public string ResourcePoolName
	{
		get
		{
			string poolId = m_DeviceSetting.GetData(UpdatePolicy.EnsureUpdated).PoolId;
			if (!string.IsNullOrEmpty(poolId))
			{
				return poolId;
			}
			return "Primordial";
		}
	}

	public ushort VirtualFunction => m_DeviceSetting.GetData(UpdatePolicy.EnsureUpdated).VirtualFunction;

	internal VMAssignedDevice(IVMAssignableDeviceSetting setting, VirtualMachineBase parentVirtualMachineObject)
		: base(setting, parentVirtualMachineObject)
	{
		m_DeviceSetting = InitializePrimaryDataUpdater(setting);
	}

	internal override IDataUpdater<IVMDeviceSetting> GetDeviceDataUpdater()
	{
		return m_DeviceSetting;
	}

	internal static VMAssignedDevice AddAssignableDevice(VirtualMachine vm, IOperationWatcher operationWatcher, string instancePath, string locationPath, string resourcePoolName, ushort? virtualFunction)
	{
		IVMAssignableDeviceSetting iVMAssignableDeviceSetting = VMDevice.CreateTemplateDeviceSetting<IVMAssignableDeviceSetting>(vm.Server, VMDeviceSettingType.PciExpress);
		if (!string.IsNullOrEmpty(instancePath) || !string.IsNullOrEmpty(locationPath))
		{
			IVMAssignableDevice iVMAssignableDevice = PciExpressUtilities.FilterAssignableDevices(ObjectLocator.GetHostComputerSystem(vm.Server).GetHostAssignableDevices(Constants.UpdateThreshold), instancePath, locationPath).FirstOrDefault();
			if (iVMAssignableDevice == null)
			{
				throw ExceptionHelper.CreateInvalidArgumentException(ErrorMessages.VMAssignableDevice_NotFound);
			}
			iVMAssignableDeviceSetting.PhysicalDevicePath = iVMAssignableDevice.ManagementPath;
		}
		else
		{
			iVMAssignableDeviceSetting.PoolId = (VMResourcePool.IsPrimordialPoolName(resourcePoolName) ? string.Empty : resourcePoolName);
		}
		if (virtualFunction.HasValue)
		{
			iVMAssignableDeviceSetting.VirtualFunction = virtualFunction.Value;
		}
		return new VMAssignedDevice(vm.AddDeviceSetting(iVMAssignableDeviceSetting, TaskDescriptions.AddVMAssignableDevice, operationWatcher), vm);
	}

	void IRemovable.Remove(IOperationWatcher operationWatcher)
	{
		IVMAssignableDeviceSetting data = m_DeviceSetting.GetData(UpdatePolicy.None);
		RemoveInternal(data, TaskDescriptions.RemoveVMAssignableDevice, operationWatcher);
	}
}
