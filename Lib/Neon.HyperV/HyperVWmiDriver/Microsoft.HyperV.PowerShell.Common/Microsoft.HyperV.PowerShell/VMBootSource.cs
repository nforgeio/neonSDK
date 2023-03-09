using System;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMBootSource : VMComponentObject, IEquatable<VMBootSource>, IBootableDevice
{
	private readonly DataUpdater<IVMBootEntry> m_BootEntry;

	public VMBootSourceType BootType => (VMBootSourceType)m_BootEntry.GetData(UpdatePolicy.EnsureUpdated).SourceType;

	[VirtualizationObjectIdentifier(IdentifierFlags.FriendlyName)]
	public string Description => m_BootEntry.GetData(UpdatePolicy.EnsureUpdated).Description;

	[VirtualizationObjectIdentifier(IdentifierFlags.UniqueIdentifier)]
	public string FirmwarePath => m_BootEntry.GetData(UpdatePolicy.EnsureUpdated).DevicePath;

	public VMDevice Device
	{
		get
		{
			IVMDeviceSetting bootDeviceSetting = m_BootEntry.GetData(UpdatePolicy.EnsureAssociatorsUpdated).GetBootDeviceSetting();
			if (bootDeviceSetting == null)
			{
				return null;
			}
			if (bootDeviceSetting is ISyntheticEthernetPortSetting syntheticEthernetPortSetting)
			{
				return new VMNetworkAdapter(syntheticEthernetPortSetting, syntheticEthernetPortSetting.GetConnectionConfiguration(), GetParentAs<VirtualMachineBase>());
			}
			IVMDriveSetting obj = bootDeviceSetting as IVMDriveSetting;
			IVMDriveControllerSetting controllerSetting = obj.ControllerSetting;
			VMDriveController driveController = GetParentAs<VirtualMachineBase>().FindScsiControllerById(controllerSetting.DeviceId);
			return Drive.CreateForExistingDrive(obj, driveController, GetParentAs<VirtualMachineBase>());
		}
	}

	VMBootSource IBootableDevice.BootSource => this;

	internal IVMBootEntry BootEntry => m_BootEntry.GetData(UpdatePolicy.None);

	internal VMBootSource(IVMBootEntry bootEntry, VirtualMachineBase parentVirtualMachineObject)
		: base(bootEntry, parentVirtualMachineObject)
	{
		m_BootEntry = InitializePrimaryDataUpdater(bootEntry);
	}

	internal bool IsBootDeviceType(BootDevice deviceType)
	{
		VMDevice device = Device;
		if (device == null)
		{
			return false;
		}
		if (deviceType == BootDevice.NetworkAdapter && device is VMNetworkAdapter)
		{
			return true;
		}
		if (deviceType == BootDevice.CD && device is DvdDrive)
		{
			return true;
		}
		if (deviceType == BootDevice.VHD && device is HardDiskDrive)
		{
			return true;
		}
		return false;
	}

	public static bool operator ==(VMBootSource one, VMBootSource two)
	{
		if ((object)one == two)
		{
			return true;
		}
		if ((object)one == null || (object)two == null)
		{
			return false;
		}
		return one.Equals(two);
	}

	public static bool operator !=(VMBootSource one, VMBootSource two)
	{
		return !(one == two);
	}

	public static bool Equals(VMBootSource one, VMBootSource two)
	{
		return one == two;
	}

	public override bool Equals(object other)
	{
		VMBootSource vMBootSource = other as VMBootSource;
		if (vMBootSource == null)
		{
			return false;
		}
		return Equals(vMBootSource);
	}

	public bool Equals(VMBootSource other)
	{
		if (other != null)
		{
			return BootEntry.ManagementPath.Equals(other.BootEntry.ManagementPath);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return BootEntry.GetHashCode();
	}
}
