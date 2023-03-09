#define TRACE
using System;
using System.Globalization;

namespace Microsoft.Virtualization.Client.Management;

internal class VMDriveSettingView : VMDeviceSettingView, IVMDriveSetting, IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable, IDeleteableAsync, IDeleteable
{
	internal static class WmiMemberNames
	{
		public const string PhysicalPassThroughDrivePath = "HostResource";

		public const string ParentController = "Parent";

		public const string ControllerAddress = "AddressOnParent";

		public const string KsdConnectionPath = "Connection";

		public const string StorageSubsystemType = "StorageSubsystemType";
	}

	public WmiObjectPath PhysicalPassThroughDrivePath
	{
		get
		{
			object[] property = GetProperty<object[]>("HostResource");
			WmiObjectPath result = null;
			if (property != null && property.Length != 0)
			{
				string text = (string)property[0];
				try
				{
					return GetWmiObjectPathFromPath(text);
				}
				catch (ArgumentException inner)
				{
					throw ThrowHelper.CreateInvalidPropertyValueException("HostResource", typeof(WmiObjectPath), text, inner);
				}
			}
			return result;
		}
		set
		{
			SetProperty("HostResource", new string[1] { (value != null) ? value.ToString() : string.Empty });
		}
	}

	public string KSDConnectionPath
	{
		get
		{
			object[] property = GetProperty<object[]>("Connection");
			string result = null;
			if (property != null && property.Length != 0)
			{
				result = (string)property[0];
			}
			return result;
		}
		set
		{
			if (VMDeviceSettingType == VMDeviceSettingType.KeyStorageDrive)
			{
				SetProperty("Connection", new string[1] { string.IsNullOrEmpty(value) ? string.Empty : value });
				SetProperty("StorageSubsystemType", "ksd");
			}
		}
	}

	public IVMDriveControllerSetting ControllerSetting
	{
		get
		{
			IVMDriveControllerSetting iVMDriveControllerSetting = null;
			string property = GetProperty<string>("Parent");
			if (!string.IsNullOrEmpty(property))
			{
				return (IVMDriveControllerSetting)GetViewFromPath(property);
			}
			throw ThrowHelper.CreateRelatedObjectNotFoundException(base.Server, typeof(IVMDriveControllerSetting));
		}
		set
		{
			if (value != null)
			{
				SetProperty("Parent", value.ManagementPath);
			}
			else
			{
				SetProperty("Parent", string.Empty);
			}
		}
	}

	public int ControllerAddress
	{
		get
		{
			string property = GetProperty<string>("AddressOnParent");
			int result = -1;
			try
			{
				return int.Parse(property, NumberStyles.None, CultureInfo.InvariantCulture);
			}
			catch (ArgumentNullException)
			{
				return result;
			}
			catch (FormatException)
			{
				return result;
			}
			catch (OverflowException)
			{
				return result;
			}
		}
		set
		{
			string value2 = value.ToString(CultureInfo.InvariantCulture);
			SetProperty("AddressOnParent", value2);
		}
	}

	private bool IsSyntheticDrive
	{
		get
		{
			VMDeviceSettingType vMDeviceSettingType = VMDeviceSettingType;
			if (vMDeviceSettingType != VMDeviceSettingType.HardDiskSyntheticDrive && vMDeviceSettingType != VMDeviceSettingType.DvdSyntheticDrive && vMDeviceSettingType != VMDeviceSettingType.DisketteSyntheticDrive)
			{
				return vMDeviceSettingType == VMDeviceSettingType.KeyStorageDrive;
			}
			return true;
		}
	}

	public IVMBootEntry BootEntry => GetRelatedObject<IVMBootEntry>(base.Associations.LogicalIdentity);

	public override IVMTask BeginDelete()
	{
		try
		{
			UpdateAssociationCache();
			IVirtualDiskSetting insertedDisk = GetInsertedDisk();
			if (insertedDisk != null)
			{
				try
				{
					insertedDisk.Delete();
				}
				catch (Exception wrappedException)
				{
					return new CompletedTask(base.Server, wrappedException);
				}
			}
		}
		catch (Exception ex)
		{
			VMTrace.TraceError("Failed to find the drive's inserted disk. Assume it doesn't have one and continue to attempt to delete it. If it does have one then the delete is expected to fail.", ex);
		}
		return base.BeginDelete();
	}

	public IVirtualDiskSetting GetInsertedDisk()
	{
		if (IsSyntheticDrive)
		{
			return GetRelatedObject<IVirtualDiskSetting>(base.Associations.DriveSettingToDiskSetting, throwIfNotFound: false);
		}
		return null;
	}

	public IVMHardDiskDrive GetPhysicalDrive()
	{
		IVMHardDiskDrive result = null;
		WmiObjectPath physicalPassThroughDrivePath = PhysicalPassThroughDrivePath;
		if (physicalPassThroughDrivePath != null)
		{
			result = (IVMHardDiskDrive)GetViewFromPath(physicalPassThroughDrivePath);
		}
		return result;
	}
}
