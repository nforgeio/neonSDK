#define TRACE
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Microsoft.Virtualization.Client.Management;

internal class ImageManagementServiceView : View, IImageManagementService, IVirtualizationManagementObject
{
	internal static class WmiMemberNames
	{
		public const string CreateVirtualHardDisk = "CreateVirtualHardDisk";

		public const string SetParentVirtualHardDisk = "SetParentVirtualHardDisk";

		public const string CreateVirtualFloppyDisk = "CreateVirtualFloppyDisk";

		public const string MergeVirtualHardDisk = "MergeVirtualHardDisk";

		public const string CompactVirtualHardDisk = "CompactVirtualHardDisk";

		public const string ResizeVirtualHardDisk = "ResizeVirtualHardDisk";

		public const string ConvertVirtualHardDisk = "ConvertVirtualHardDisk";

		public const string GetVirtualHardDiskSettingData = "GetVirtualHardDiskSettingData";

		public const string GetVirtualHardDiskState = "GetVirtualHardDiskState";

		public const string SetVirtualHardDiskSettingData = "SetVirtualHardDiskSettingData";

		public const string ValidateVirtualHardDisk = "ValidateVirtualHardDisk";

		public const string ValidatePersistentReservationSupport = "ValidatePersistentReservationSupport";

		public const string AttachVirtualHardDisk = "AttachVirtualHardDisk";

		public const string GetMountedStorageImage = "FindMountedStorageImageInstance";

		public const string GetVHDSetInformation = "GetVHDSetInformation";

		public const string GetVHDSnapshotInformation = "GetVHDSnapshotInformation";

		public const string SetVHDSnapshotInformation = "SetVHDSnapshotInformation";

		public const string DeleteVHDSnapshot = "DeleteVHDSnapshot";

		public const string OptimizeVHDSet = "OptimizeVHDSet";
	}

	private class ImageManagementServiceErrorCodeMapper : ErrorCodeMapper
	{
		public string m_BrokenLinkParentPath;

		public string m_BrokenLinkChildPath;

		public override string MapError(VirtualizationOperation operation, long errorCode, string operationFailedMsg)
		{
			if ((operation == VirtualizationOperation.GetVirtualHardDiskSettingData || operation == VirtualizationOperation.GetVirtualHardDiskState) && errorCode == -5)
			{
				return ErrorCodeMapper.Concatenate(operationFailedMsg, ErrorMessages.GetVirtualHardDiskInfoParsingError);
			}
			if (operation == VirtualizationOperation.ValidateVirtualHardDisk && !string.IsNullOrEmpty(m_BrokenLinkParentPath) && !string.IsNullOrEmpty(m_BrokenLinkChildPath))
			{
				return ErrorCodeMapper.Concatenate(operationFailedMsg, string.Format(CultureInfo.CurrentCulture, ErrorMessages.VirtualHardDiskChainBroken, m_BrokenLinkParentPath, m_BrokenLinkChildPath));
			}
			if (operation == VirtualizationOperation.SetParentVirtualHardDisk && errorCode == 32791)
			{
				return ErrorCodeMapper.Concatenate(operationFailedMsg, string.Format(CultureInfo.CurrentCulture, ErrorMessages.VirtualHardDiskIdMismatch));
			}
			return base.MapError(operation, errorCode, operationFailedMsg);
		}
	}

	public IVMTask BeginCreateVirtualHardDisk(VirtualHardDiskType type, VirtualHardDiskFormat format, string path, string parentPath, long maxInternalSize)
	{
		return BeginCreateVirtualHardDisk(type, format, path, parentPath, maxInternalSize, 0L, 0L, 0L, pmemCompatible: false, VirtualHardDiskPmemAddressAbstractionType.None, 0L);
	}

	public IVMTask BeginCreateVirtualHardDisk(VirtualHardDiskType type, VirtualHardDiskFormat format, string path, string parentPath, long maxInternalSize, long blockSize, long logicalSectorSize, long physicalSectorSize, bool pmemCompatible, VirtualHardDiskPmemAddressAbstractionType addressAbstractionType, long dataAlignment)
	{
		if (type != VirtualHardDiskType.FixedSize && type != VirtualHardDiskType.DynamicallyExpanding && type != VirtualHardDiskType.Differencing)
		{
			throw new ArgumentException("Invalid disk type", "type");
		}
		if (format != VirtualHardDiskFormat.Vhd && format != VirtualHardDiskFormat.Vhdx && format != VirtualHardDiskFormat.VHDSet)
		{
			throw new ArgumentException("Invalid disk format", "format");
		}
		if (pmemCompatible)
		{
			if (type != VirtualHardDiskType.FixedSize)
			{
				throw new ArgumentException("Invalid disk type for PMEM-compatible VHD", "type");
			}
			if (format != VirtualHardDiskFormat.Vhdx)
			{
				throw new ArgumentException("Invalid disk format for PMEM-compatible VHD", "format");
			}
		}
		else if (addressAbstractionType != 0)
		{
			throw new ArgumentException("Address abstraction type can be specified only for PMEM-compatible VHDs", "addressAbstractionType");
		}
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		if (type == VirtualHardDiskType.Differencing && parentPath == null)
		{
			throw new ArgumentNullException("parentPath");
		}
		if (dataAlignment != 0L)
		{
			if (!pmemCompatible)
			{
				throw new ArgumentException("DataAlignment can be specified only for PMEM-compatible VHDs", "format");
			}
			if (addressAbstractionType != 0)
			{
				throw new ArgumentException("The specified DataAlignment value is not compatible with the specified AddressAbstractionType", "format");
			}
		}
		string clientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.CreateVirtualDiskFailed, path);
		switch (type)
		{
		case VirtualHardDiskType.FixedSize:
			VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Starting creating fixed virtual hard disk '{0}' (size = '{1}')", path, maxInternalSize));
			break;
		case VirtualHardDiskType.DynamicallyExpanding:
			VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Starting creating dynamic virtual hard disk '{0}' (size = '{1}')", path, maxInternalSize));
			break;
		default:
			VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Starting creating differencing virtual hard disk '{0}' (parent = '{1}')", path, parentPath));
			break;
		}
		VirtualHardDiskSettingData virtualHardDiskSettingData = new VirtualHardDiskSettingData(base.Server, type, format, path, parentPath)
		{
			MaxInternalSize = maxInternalSize,
			BlockSize = blockSize,
			LogicalSectorSize = logicalSectorSize,
			PhysicalSectorSize = physicalSectorSize,
			IsPmemCompatible = pmemCompatible,
			PmemAddressAbstractionType = addressAbstractionType,
			DataAlignment = dataAlignment
		};
		object[] array = new object[2] { virtualHardDiskSettingData, null };
		uint result = InvokeMethod("CreateVirtualHardDisk", array);
		IVMTask iVMTask = BeginMethodTaskReturn(result, null, array[1]);
		iVMTask.ClientSideFailedMessage = clientSideFailedMessage;
		return iVMTask;
	}

	public void EndCreateVirtualHardDisk(IVMTask createVhdTask)
	{
		EndMethod(createVhdTask, VirtualizationOperation.CreateVirtualHardDisk);
		VMTrace.TraceUserActionCompleted("Creating virtual hard disk completed successfully.");
	}

	public IVMTask BeginCreateVirtualFloppyDisk(string path)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		string clientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.CreateVirtualDiskFailed, path);
		object[] array = new object[2] { path, null };
		VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Starting creating virtual floppy disk '{0}'", path));
		uint result = InvokeMethod("CreateVirtualFloppyDisk", array);
		IVMTask iVMTask = BeginMethodTaskReturn(result, null, array[1]);
		iVMTask.ClientSideFailedMessage = clientSideFailedMessage;
		return iVMTask;
	}

	public void EndCreateVirtualFloppyDisk(IVMTask createVfdTask)
	{
		EndMethod(createVfdTask, VirtualizationOperation.CreateVirtualFloppyDisk);
		VMTrace.TraceUserActionCompleted("Creating virtual floppy disk completed successfully.");
	}

	public IVMTask BeginReconnectParentVirtualHardDisk(string childPath, string parentPath, string leafPath, bool reconnectWithForce)
	{
		if (childPath == null)
		{
			throw new ArgumentNullException("childPath");
		}
		if (parentPath == null)
		{
			throw new ArgumentNullException("parentPath");
		}
		string clientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.ReconnectParentDiskFailed, childPath, parentPath);
		object[] array = new object[5] { childPath, parentPath, leafPath, reconnectWithForce, null };
		VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Starting reconnect parent virtual hard disk for child disk '{0}' to parent disk '{1}'", childPath, parentPath));
		uint result = InvokeMethod("SetParentVirtualHardDisk", array);
		IVMTask iVMTask = BeginMethodTaskReturn(result, null, array[4]);
		iVMTask.ClientSideFailedMessage = clientSideFailedMessage;
		return iVMTask;
	}

	public void EndReconnectParentVirtualHardDisk(IVMTask reconnectTask)
	{
		EndMethod(reconnectTask, VirtualizationOperation.SetParentVirtualHardDisk);
		VMTrace.TraceUserActionCompleted("Reconnect parent virtual hard disk completed successfully.");
	}

	public IVMTask BeginMergeVirtualHardDisk(string sourcePath, string destinationPath)
	{
		if (sourcePath == null)
		{
			throw new ArgumentNullException("sourcePath");
		}
		if (destinationPath == null)
		{
			throw new ArgumentNullException("destinationPath");
		}
		string clientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.MergeVirtualDiskFailed, sourcePath, destinationPath);
		object[] array = new object[3] { sourcePath, destinationPath, null };
		VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Starting merging virtual hard disk '{0}' into '{1}'", sourcePath, destinationPath));
		uint result = InvokeMethod("MergeVirtualHardDisk", array);
		IVMTask iVMTask = BeginMethodTaskReturn(result, null, array[2]);
		iVMTask.ClientSideFailedMessage = clientSideFailedMessage;
		return iVMTask;
	}

	public void EndMergeVirtualHardDisk(IVMTask mergeVhdTask)
	{
		EndMethod(mergeVhdTask, VirtualizationOperation.MergeVirtualHardDisk);
		VMTrace.TraceUserActionCompleted("Merging virtual hard disk completed successfully.");
	}

	public IVMTask BeginCompactVirtualHardDisk(string path, VirtualHardDiskFormat format)
	{
		return BeginCompactVirtualHardDisk(path, (format == VirtualHardDiskFormat.Vhdx) ? 1u : 0u);
	}

	public IVMTask BeginCompactVirtualHardDisk(string path, uint mode)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		string clientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.CompactVirtualHardDiskFailed, path);
		object[] array = new object[3] { path, mode, null };
		VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Starting compacting virtual hard disk '{0}'", path));
		uint result = InvokeMethod("CompactVirtualHardDisk", array);
		IVMTask iVMTask = BeginMethodTaskReturn(result, null, array[2]);
		iVMTask.ClientSideFailedMessage = clientSideFailedMessage;
		return iVMTask;
	}

	public void EndCompactVirtualHardDisk(IVMTask compactVhdTask)
	{
		EndMethod(compactVhdTask, VirtualizationOperation.CompactVirtualHardDisk);
		VMTrace.TraceUserActionCompleted("Compacting virtual hard disk completed successfully.");
	}

	public IVMTask BeginResizeVirtualHardDisk(string path, ulong? maxInternalSize)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		string clientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.ResizeVirtualHardDiskFailed, path);
		object[] array = new object[3] { path, maxInternalSize, null };
		VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Resizing virtual hard disk '{0}'", path));
		uint result = InvokeMethod("ResizeVirtualHardDisk", array);
		IVMTask iVMTask = BeginMethodTaskReturn(result, null, array[2]);
		iVMTask.ClientSideFailedMessage = clientSideFailedMessage;
		return iVMTask;
	}

	public void EndResizeVirtualHardDisk(IVMTask resizeVhdTask)
	{
		EndMethod(resizeVhdTask, VirtualizationOperation.ResizeVirtualHardDisk);
		VMTrace.TraceUserActionCompleted("Resizing virtual hard disk completed successfully.");
	}

	public IVMTask BeginConvertVirtualHardDisk(string sourcePath, string destinationPath, string destinationParentPath, VirtualHardDiskType destinationType, VirtualHardDiskFormat destinationFormat, long blockSize)
	{
		return BeginConvertVirtualHardDisk(sourcePath, destinationPath, destinationParentPath, destinationType, destinationFormat, blockSize, pmemCompatible: false, VirtualHardDiskPmemAddressAbstractionType.None);
	}

	public IVMTask BeginConvertVirtualHardDisk(string sourcePath, string destinationPath, string destinationParentPath, VirtualHardDiskType destinationType, VirtualHardDiskFormat destinationFormat, long blockSize, bool pmemCompatible, VirtualHardDiskPmemAddressAbstractionType addressAbstractionType)
	{
		if (sourcePath == null)
		{
			throw new ArgumentNullException("sourcePath");
		}
		if (destinationPath == null)
		{
			throw new ArgumentNullException("destinationPath");
		}
		if (destinationType != VirtualHardDiskType.FixedSize && destinationType != VirtualHardDiskType.DynamicallyExpanding && destinationType != VirtualHardDiskType.Differencing)
		{
			throw new ArgumentException("Invalid disk type", "destinationType");
		}
		if (destinationType != VirtualHardDiskType.Differencing && !string.IsNullOrEmpty(destinationParentPath))
		{
			throw new ArgumentException("Invalid parent path", "destinationParentPath");
		}
		if (pmemCompatible)
		{
			if (destinationType != VirtualHardDiskType.FixedSize)
			{
				throw new ArgumentException("Invalid disk type for PMEM-compatible VHD", "destinationType");
			}
			if (destinationFormat != VirtualHardDiskFormat.Vhdx)
			{
				throw new ArgumentException("Invalid disk format for PMEM-compatible VHD", "destinationFormat");
			}
		}
		else if (addressAbstractionType != 0)
		{
			throw new ArgumentException("Address abstraction type can be specified only for PMEM-compatible VHDs", "addressAbstractionType");
		}
		if (destinationType == VirtualHardDiskType.Differencing && string.IsNullOrEmpty(destinationParentPath))
		{
			throw new ArgumentNullException("destinationParentPath");
		}
		if (string.IsNullOrEmpty(destinationParentPath))
		{
			destinationParentPath = null;
		}
		string clientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.ConvertVirtualHardDiskFailed, sourcePath, destinationPath);
		VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Starting copying virtual hard disk '{0}' to '{1}', type = '{2}', format = '{3}'", sourcePath, destinationPath, destinationType, destinationFormat));
		VirtualHardDiskSettingData virtualHardDiskSettingData = new VirtualHardDiskSettingData(base.Server, destinationType, destinationFormat, destinationPath, destinationParentPath)
		{
			BlockSize = blockSize,
			IsPmemCompatible = pmemCompatible,
			PmemAddressAbstractionType = addressAbstractionType
		};
		object[] array = new object[3] { sourcePath, virtualHardDiskSettingData, null };
		uint result = InvokeMethod("ConvertVirtualHardDisk", array);
		IVMTask iVMTask = BeginMethodTaskReturn(result, null, array[2]);
		iVMTask.ClientSideFailedMessage = clientSideFailedMessage;
		return iVMTask;
	}

	public void EndConvertVirtualHardDisk(IVMTask convertVhdTask)
	{
		EndMethod(convertVhdTask, VirtualizationOperation.ConvertVirtualHardDisk);
		VMTrace.TraceUserActionCompleted("Converting virtual hard disk completed successfully.");
	}

	public VirtualHardDiskSettingData GetVirtualHardDiskSettingData(string path)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		string text = string.Format(CultureInfo.CurrentCulture, ErrorMessages.GetVirtualHardDiskInfoFailed, path);
		VirtualHardDiskSettingData result = null;
		object[] array = new object[3] { path, null, null };
		VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Starting getting setting data for virtual hard disk '{0}'", path));
		uint num = InvokeMethod("GetVirtualHardDiskSettingData", array);
		using IVMTask iVMTask = BeginMethodTaskReturn(num, null, array[2]);
		iVMTask.ClientSideFailedMessage = text;
		iVMTask.WaitForCompletion();
		if (num == View.ErrorCodeSuccess)
		{
			string text2 = array[1] as string;
			VMTrace.TraceUserActionCompleted("Getting setting data of virtual hard disk completed successfully.", text2);
			try
			{
				return EmbeddedInstance.ConvertTo<VirtualHardDiskSettingData>(base.Server, text2);
			}
			catch (Exception ex)
			{
				if (ex is ArgumentNullException || ex is FormatException)
				{
					long errorCode = -5L;
					VirtualizationOperation operation = VirtualizationOperation.GetVirtualHardDiskSettingData;
					ErrorCodeMapper errorCodeMapper = GetErrorCodeMapper();
					throw ThrowHelper.CreateVirtualizationOperationFailedException(text, operation, errorCode, errorCodeMapper, ex);
				}
				throw;
			}
		}
		EndMethod(iVMTask, VirtualizationOperation.GetVirtualHardDiskSettingData);
		return result;
	}

	public VirtualHardDiskState GetVirtualHardDiskState(string path)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		string text = string.Format(CultureInfo.CurrentCulture, ErrorMessages.GetVirtualHardDiskInfoFailed, path);
		VirtualHardDiskState result = null;
		object[] array = new object[3] { path, null, null };
		VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Starting getting state for virtual hard disk '{0}'", path));
		uint num = InvokeMethod("GetVirtualHardDiskState", array);
		using IVMTask iVMTask = BeginMethodTaskReturn(num, null, array[2]);
		iVMTask.ClientSideFailedMessage = text;
		iVMTask.WaitForCompletion();
		if (num == View.ErrorCodeSuccess)
		{
			string text2 = array[1] as string;
			VMTrace.TraceUserActionCompleted("Getting state of virtual hard disk completed successfully.", text2);
			try
			{
				return EmbeddedInstance.ConvertTo<VirtualHardDiskState>(base.Server, text2);
			}
			catch (Exception ex)
			{
				if (ex is ArgumentNullException || ex is FormatException)
				{
					long errorCode = -5L;
					VirtualizationOperation operation = VirtualizationOperation.GetVirtualHardDiskState;
					ErrorCodeMapper errorCodeMapper = GetErrorCodeMapper();
					throw ThrowHelper.CreateVirtualizationOperationFailedException(text, operation, errorCode, errorCodeMapper, ex);
				}
				throw;
			}
		}
		EndMethod(iVMTask, VirtualizationOperation.GetVirtualHardDiskState);
		return result;
	}

	protected override ErrorCodeMapper GetErrorCodeMapper()
	{
		return new ImageManagementServiceErrorCodeMapper();
	}

	public IVMTask BeginValidateVirtualHardDisk(string path)
	{
		if (string.IsNullOrEmpty(path))
		{
			throw new ArgumentException(null, "path");
		}
		string clientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.ValidateVirtualHardDiskFailed, path);
		object[] array = new object[2] { path, null };
		VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Starting validation of disk '{0}'", path));
		uint result = InvokeMethod("ValidateVirtualHardDisk", array);
		IVMTask iVMTask = BeginMethodTaskReturn(result, null, array[1]);
		iVMTask.ClientSideFailedMessage = clientSideFailedMessage;
		return iVMTask;
	}

	public void EndValidateVirtualHardDisk(IVMTask validateTask)
	{
		if (validateTask.Status != VMTaskStatus.CompletedSuccessfully)
		{
			string text = null;
			string text2 = null;
			if (validateTask is IVMStorageTask iVMStorageTask)
			{
				text = iVMStorageTask.Parent;
				text2 = iVMStorageTask.Child;
			}
			GetErrorInformationFromTask(validateTask, out var errorSummaryDescription, out var errorDetailsDescription, out var errorCode, out var errorCodeMapper);
			if (errorCodeMapper is ImageManagementServiceErrorCodeMapper imageManagementServiceErrorCodeMapper)
			{
				imageManagementServiceErrorCodeMapper.m_BrokenLinkParentPath = text;
				imageManagementServiceErrorCodeMapper.m_BrokenLinkChildPath = text2;
			}
			throw ThrowHelper.CreateValidateVirtualHardDiskException(errorSummaryDescription, errorDetailsDescription, errorCode, text, text2, errorCodeMapper, null);
		}
		VMTrace.TraceUserActionCompleted("Validation of disk completed successfully.");
	}

	public IVMTask BeginAttachVirtualHardDisk(string path, bool assignDriveLetter, bool readOnly)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		string clientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.AttachVirtualHardDiskFailed, path);
		object[] array = new object[4] { path, assignDriveLetter, readOnly, null };
		VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Starting attaching virtual hard disk '{0}'", path));
		uint result = InvokeMethod("AttachVirtualHardDisk", array);
		IVMTask iVMTask = BeginMethodTaskReturn(result, null, array[3]);
		iVMTask.ClientSideFailedMessage = clientSideFailedMessage;
		return iVMTask;
	}

	public void EndAttachVirtualHardDisk(IVMTask attachTask)
	{
		EndMethod(attachTask, VirtualizationOperation.AttachVirtualHardDisk);
		VMTrace.TraceUserActionCompleted("Attaching virtual hard disk completed successfully.");
	}

	public IVMTask BeginValidatePersistentReservationSupport(string path)
	{
		if (string.IsNullOrEmpty(path))
		{
			throw new ArgumentException(null, "path");
		}
		string clientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.ValidatePersistentReservationSupportFailed, path);
		object[] array = new object[2] { path, null };
		VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Starting validation of Persistent Reservations support for file system '{0}'", path));
		uint result = InvokeMethod("ValidatePersistentReservationSupport", array);
		IVMTask iVMTask = BeginMethodTaskReturn(result, null, array[1]);
		iVMTask.ClientSideFailedMessage = clientSideFailedMessage;
		return iVMTask;
	}

	public void EndValidatePersistentReservationSupport(IVMTask validateTask)
	{
		EndMethod(validateTask, VirtualizationOperation.ValidatePersistentReservationSupport);
		VMTrace.TraceUserActionCompleted("Validating persistent reservation support completed successfully.");
	}

	public IMountedStorageImage GetMountedStorageImage(string path)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		string errorMsg = string.Format(CultureInfo.CurrentCulture, ErrorMessages.GetMountedStorageImageFailed, path);
		IMountedStorageImage result = null;
		object[] array = new object[3]
		{
			path,
			(ushort)2,
			null
		};
		VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Starting getting mounted storage image for virtual hard disk '{0}'", path));
		uint num = InvokeMethod("FindMountedStorageImageInstance", array);
		if (num == View.ErrorCodeSuccess)
		{
			WmiObjectPath path2 = (WmiObjectPath)array[2];
			result = (IMountedStorageImage)GetViewFromPath(path2);
		}
		else if ((ulong)num != 32789)
		{
			throw ThrowHelper.CreateVirtualizationOperationFailedException(errorMsg, VirtualizationOperation.FindMountedStorageImage, num);
		}
		return result;
	}

	public IVMTask BeginSetVirtualHardDiskSettingData(VirtualHardDiskSettingData settings)
	{
		if (settings == null)
		{
			throw new ArgumentNullException("settings");
		}
		string clientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.SetVirtualHardDiskSettingDataFailed, settings.Path);
		object[] array = new object[2] { settings, null };
		VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Starting setting properties for virtual hard disk '{0}'", settings.Path));
		uint result = InvokeMethod("SetVirtualHardDiskSettingData", array);
		IVMTask iVMTask = BeginMethodTaskReturn(result, null, array[1]);
		iVMTask.ClientSideFailedMessage = clientSideFailedMessage;
		return iVMTask;
	}

	public void EndSetVirtualHardDiskSettingData(IVMTask setVhdSettingDataTask)
	{
		EndMethod(setVhdSettingDataTask, VirtualizationOperation.SetVirtualHardDiskSettingData);
		VMTrace.TraceUserActionCompleted("Setting the virtual hard disk setting data completed successfully.");
	}

	public IWin32DiskDrive GetWin32DiskDrive(uint diskNumber)
	{
		string format = "SELECT * FROM {0} WHERE {1} = {2}";
		format = string.Format(CultureInfo.InvariantCulture, format, "Win32_DiskDrive", "Index", diskNumber);
		QueryAssociation association = QueryAssociation.CreateFromQuery(Server.CimV2Namespace, format);
		return GetRelatedObject<IWin32DiskDrive>(association, throwIfNotFound: false);
	}

	public VHDSetInformation GetVHDSetInformation(string vhdSetPath, bool getAllPaths)
	{
		if (vhdSetPath == null)
		{
			throw new ArgumentNullException("vhdSetPath");
		}
		string text = string.Format(CultureInfo.CurrentCulture, ErrorMessages.GetVHDSetInformationFailed, vhdSetPath);
		VHDSetInformation result = null;
		uint[] array = new uint[1] { getAllPaths ? 2u : 0u };
		object[] array2 = new object[4] { vhdSetPath, array, null, null };
		VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Starting getting information for VHD Set file '{0}'", vhdSetPath));
		uint num = InvokeMethod("GetVHDSetInformation", array2);
		using IVMTask iVMTask = BeginMethodTaskReturn(num, null, array2[3]);
		iVMTask.ClientSideFailedMessage = text;
		iVMTask.WaitForCompletion();
		if (num == View.ErrorCodeSuccess)
		{
			string text2 = array2[2] as string;
			VMTrace.TraceUserActionCompleted("Getting information about VHD Set file completed successfully.", text2);
			try
			{
				return EmbeddedInstance.ConvertTo<VHDSetInformation>(base.Server, text2);
			}
			catch (Exception ex)
			{
				if (ex is ArgumentNullException || ex is FormatException)
				{
					long errorCode = -5L;
					VirtualizationOperation operation = VirtualizationOperation.GetVHDSetInformation;
					ErrorCodeMapper errorCodeMapper = GetErrorCodeMapper();
					throw ThrowHelper.CreateVirtualizationOperationFailedException(text, operation, errorCode, errorCodeMapper, ex);
				}
				throw;
			}
		}
		EndMethod(iVMTask, VirtualizationOperation.GetVHDSetInformation);
		return result;
	}

	public List<VHDSnapshotInformation> GetVHDSnapshotInformation(string vhdSetPath, List<string> snapshotIDs, bool getParentPathsList)
	{
		if (vhdSetPath == null)
		{
			throw new ArgumentNullException("vhdSetPath");
		}
		string text = string.Format(CultureInfo.CurrentCulture, ErrorMessages.GetVHDSnapshotInformationFailed, vhdSetPath);
		List<VHDSnapshotInformation> result = null;
		string[] array = snapshotIDs?.ToArray();
		uint[] array2 = new uint[1] { getParentPathsList ? 2u : 0u };
		object[] array3 = new object[5] { vhdSetPath, array, array2, null, null };
		VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Starting getting information about Snapshots in VHD Set file '{0}'", vhdSetPath));
		uint num = InvokeMethod("GetVHDSnapshotInformation", array3);
		using IVMTask iVMTask = BeginMethodTaskReturn(num, null, array3[4]);
		iVMTask.ClientSideFailedMessage = text;
		iVMTask.WaitForCompletion();
		if (num == View.ErrorCodeSuccess)
		{
			string[] array4 = array3[3] as string[];
			VMTrace.TraceUserActionCompleted("Getting information about snapshot within VHD Set file completed successfully.", array4);
			try
			{
				return array4.Select((string vhdSetInfoEmbeddedInstance) => EmbeddedInstance.ConvertTo<VHDSnapshotInformation>(base.Server, vhdSetInfoEmbeddedInstance)).ToList();
			}
			catch (Exception ex)
			{
				if (ex is ArgumentNullException || ex is FormatException)
				{
					long errorCode = -5L;
					VirtualizationOperation operation = VirtualizationOperation.GetVHDSnapshotInformation;
					ErrorCodeMapper errorCodeMapper = GetErrorCodeMapper();
					throw ThrowHelper.CreateVirtualizationOperationFailedException(text, operation, errorCode, errorCodeMapper, ex);
				}
				throw;
			}
		}
		EndMethod(iVMTask, VirtualizationOperation.GetVHDSnapshotInformation);
		return result;
	}

	public IVMTask BeginSetVHDSnapshotInformation(VHDSnapshotInformation snapshot)
	{
		if (snapshot == null)
		{
			throw new ArgumentNullException("snapshot");
		}
		string clientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.SetVHDSnapshotInformationFailed, snapshot.FilePath, snapshot.SnapshotId);
		string text = snapshot.ToString();
		object[] array = new object[2] { text, null };
		VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Starting setting snapshot '{1}' in VHD Set file '{0}'", snapshot.FilePath, snapshot.SnapshotId));
		uint result = InvokeMethod("SetVHDSnapshotInformation", array);
		IVMTask iVMTask = BeginMethodTaskReturn(result, null, array[1]);
		iVMTask.ClientSideFailedMessage = clientSideFailedMessage;
		return iVMTask;
	}

	public void EndSetVHDSnapshotInformation(IVMTask setVHDSnapshotInformationTask)
	{
		EndMethod(setVHDSnapshotInformationTask, VirtualizationOperation.SetVHDSnapshotInformation);
		VMTrace.TraceUserActionCompleted("Setting the VHD Snapshot completed successfully.");
	}

	public IVMTask BeginDeleteVHDSnapshot(string vhdSetPath, string snapshotId, bool persistReferenceSnapshot)
	{
		if (vhdSetPath == null)
		{
			throw new ArgumentNullException("vhdSetPath");
		}
		if (snapshotId == null)
		{
			throw new ArgumentNullException("snapshotId");
		}
		string clientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.DeleteVHDSnapshotFailed, vhdSetPath, snapshotId);
		object[] array = new object[4] { vhdSetPath, snapshotId, persistReferenceSnapshot, null };
		VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Starting deleting snapshot '{1}' in VHD Set file '{0}'", vhdSetPath, snapshotId));
		uint result = InvokeMethod("DeleteVHDSnapshot", array);
		IVMTask iVMTask = BeginMethodTaskReturn(result, null, array[3]);
		iVMTask.ClientSideFailedMessage = clientSideFailedMessage;
		return iVMTask;
	}

	public void EndDeleteVHDSnapshot(IVMTask deleteVHDSnapshotTask)
	{
		EndMethod(deleteVHDSnapshotTask, VirtualizationOperation.DeleteVHDSnapshot);
		VMTrace.TraceUserActionCompleted("Deleting the VHD Snapshot completed successfully.");
	}

	public IVMTask BeginOptimizeVHDSet(string path)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		string clientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.OptimizeVHDSet, path);
		object[] array = new object[2] { path, null };
		VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Starting optimizing VHD Set file '{0}.", path));
		uint result = InvokeMethod("OptimizeVHDSet", array);
		IVMTask iVMTask = BeginMethodTaskReturn(result, null, array[1]);
		iVMTask.ClientSideFailedMessage = clientSideFailedMessage;
		return iVMTask;
	}

	public void EndOptimizeVHDSet(IVMTask optimizeVHDSetTask)
	{
		EndMethod(optimizeVHDSetTask, VirtualizationOperation.OptimizeVHDSet);
		VMTrace.TraceUserActionCompleted("Optimizing the VHD Set file completed successfully.");
	}
}
