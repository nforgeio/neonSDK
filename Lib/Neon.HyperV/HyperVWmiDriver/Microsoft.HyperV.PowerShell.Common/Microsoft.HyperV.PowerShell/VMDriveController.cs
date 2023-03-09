using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.HyperV.PowerShell.Common;
using Microsoft.Virtualization.Client.Management;

using ErrorMessages = Microsoft.HyperV.PowerShell.Common.ErrorMessages;

namespace Microsoft.HyperV.PowerShell;

internal abstract class VMDriveController : VMDevice
{
	internal readonly DataUpdater<IVMDriveControllerSetting> m_ControllerSetting;

	internal abstract int ControllerLocationCount { get; }

	internal abstract ControllerType ControllerType { get; }

	public abstract int ControllerNumber { get; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
	public Drive[] Drives => GetDrives().ToArray();

	internal string DeviceID => m_ControllerSetting.GetData(UpdatePolicy.None)?.DeviceId;

	internal VMDriveController(IVMDriveControllerSetting setting, VirtualMachineBase parentVirtualMachineObject)
		: base(setting, parentVirtualMachineObject)
	{
		m_ControllerSetting = InitializePrimaryDataUpdater(setting);
	}

	internal IVMDriveControllerSetting GetControllerSetting()
	{
		return m_ControllerSetting.GetData(UpdatePolicy.None);
	}

	internal override IDataUpdater<IVMDeviceSetting> GetDeviceDataUpdater()
	{
		return m_ControllerSetting;
	}

	private IEnumerable<Drive> GetDrives()
	{
		return from driveSetting in m_ControllerSetting.GetData(UpdatePolicy.EnsureAssociatorsUpdated).GetDriveSettings()
			select Drive.CreateForExistingDrive(driveSetting, this, GetParentAs<VirtualMachineBase>());
	}

	private SortedSet<int> GetDriveLocations()
	{
		return new SortedSet<int>(from drive in GetDrives()
			select drive.ControllerLocation);
	}

	internal int FindFirstVacantLocation()
	{
		if (!TryFindFirstVacantLocation(out var vacantLocation))
		{
			throw ExceptionHelper.CreateObjectNotFoundException(ErrorMessages.CannotFindVacantControllerLocation, null);
		}
		return vacantLocation;
	}

	private bool TryFindFirstVacantLocation(out int vacantLocation)
	{
		bool result = false;
		vacantLocation = -1;
		SortedSet<int> driveLocations = GetDriveLocations();
		for (int i = 0; i < ControllerLocationCount; i++)
		{
			if (!driveLocations.Contains(i))
			{
				vacantLocation = i;
				result = true;
				break;
			}
		}
		return result;
	}

	internal static Tuple<VMDriveController, int> FindControllerVacancy(VirtualMachineBase vmOrSnapshot, ControllerType? controllerType, int? controllerNumber, int? controllerLocation)
	{
		IEnumerable<VMDriveController> enumerable = vmOrSnapshot.GetDriveControllers(controllerType);
		if (controllerNumber.HasValue)
		{
			enumerable = enumerable.Where((VMDriveController controller) => controller.ControllerNumber == controllerNumber.Value);
		}
		VMDriveController vMDriveController = null;
		int item = -1;
		foreach (VMDriveController item2 in enumerable)
		{
			int vacantLocation;
			if (controllerLocation.HasValue)
			{
				int value = controllerLocation.Value;
				if (value < item2.ControllerLocationCount && !item2.GetDriveLocations().Contains(value))
				{
					vMDriveController = item2;
					item = value;
					break;
				}
			}
			else if (item2.TryFindFirstVacantLocation(out vacantLocation))
			{
				vMDriveController = item2;
				item = vacantLocation;
				break;
			}
		}
		if (vMDriveController == null)
		{
			throw ExceptionHelper.CreateObjectNotFoundException(ErrorMessages.CannotFindVacantControllerLocation, null);
		}
		return Tuple.Create(vMDriveController, item);
	}

	internal static Tuple<VMDriveController, int> FindControllerVacancyForMove(Drive drive, ControllerType? newControllerType, int? newControllerNumber, int? newControllerLocation)
	{
		VirtualMachineBase parentAs = drive.GetParentAs<VirtualMachineBase>();
		ControllerType controllerType = drive.ControllerType;
		int controllerNumber = drive.ControllerNumber;
		int controllerLocation = drive.ControllerLocation;
		if (newControllerType.HasValue && newControllerType.Value != controllerType)
		{
			return FindControllerVacancy(parentAs, newControllerType, newControllerNumber, newControllerLocation);
		}
		if (newControllerNumber.HasValue && newControllerNumber.Value != controllerNumber)
		{
			return FindControllerVacancy(parentAs, controllerType, newControllerNumber, newControllerLocation);
		}
		if (newControllerLocation.HasValue && newControllerLocation.Value != controllerLocation)
		{
			return FindControllerVacancy(parentAs, controllerType, controllerNumber, newControllerLocation);
		}
		return null;
	}
}
