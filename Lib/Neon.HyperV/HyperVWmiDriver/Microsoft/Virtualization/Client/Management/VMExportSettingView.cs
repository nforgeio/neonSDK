namespace Microsoft.Virtualization.Client.Management;

internal class VMExportSettingView : View, IVMExportSetting, IVirtualizationManagementObject
{
	internal static class WmiMemberNames
	{
		public const string CopyVmStorage = "CopyVmStorage";

		public const string CopyVmRuntimeInformation = "CopyVmRuntimeInformation";

		public const string CreateVmExportSubdirectory = "CreateVmExportSubdirectory";

		public const string CopySnapshotConfiguration = "CopySnapshotConfiguration";

		public const string SnapshotVirtualSystem = "SnapshotVirtualSystem";

		public const string CaptureLiveState = "CaptureLiveState";
	}

	public bool CopyVmStorage
	{
		get
		{
			return GetProperty<bool>("CopyVmStorage");
		}
		set
		{
			SetProperty("CopyVmStorage", value);
		}
	}

	public bool CopyVmRuntimeInformation
	{
		get
		{
			return GetProperty<bool>("CopyVmRuntimeInformation");
		}
		set
		{
			SetProperty("CopyVmRuntimeInformation", value);
		}
	}

	public bool CreateVmExportSubdirectory
	{
		get
		{
			return GetProperty<bool>("CreateVmExportSubdirectory");
		}
		set
		{
			SetProperty("CreateVmExportSubdirectory", value);
		}
	}

	public SnapshotExportMode CopySnapshotConfiguration
	{
		get
		{
			return GetProperty<SnapshotExportMode>("CopySnapshotConfiguration");
		}
		set
		{
			SetProperty("CopySnapshotConfiguration", (int)value);
		}
	}

	public IVMComputerSystemSetting SnapshotVirtualSystem
	{
		get
		{
			IVMComputerSystemSetting iVMComputerSystemSetting = null;
			string property = GetProperty<string>("SnapshotVirtualSystem");
			if (!string.IsNullOrEmpty(property))
			{
				return (IVMComputerSystemSetting)GetViewFromPath(property);
			}
			throw ThrowHelper.CreateRelatedObjectNotFoundException(base.Server, typeof(IVMComputerSystemSetting));
		}
		set
		{
			string value2 = ((value == null) ? string.Empty : value.ManagementPath.ToString());
			SetProperty("SnapshotVirtualSystem", value2);
		}
	}

	public CaptureLiveStateMode CaptureLiveState
	{
		get
		{
			return GetProperty<CaptureLiveStateMode>("CaptureLiveState");
		}
		set
		{
			SetProperty("CaptureLiveState", (int)value);
		}
	}
}
