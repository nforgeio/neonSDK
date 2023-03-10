using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal class VMProcessorView : VMDeviceView, IVMProcessor, IVMDevice, IVirtualizationManagementObject
{
	internal static class WmiPropertyNames
	{
		public const string LoadPercentage = "LoadPercentage";

		public const string Status = "OperationalStatus";

		public const string StatusDescriptions = "StatusDescriptions";
	}

	public int LoadPercentage => NumberConverter.UInt16ToInt32(GetProperty<ushort>("LoadPercentage"));

	public IReadOnlyList<VMProcessorOperationalStatus> GetOperationalStatus()
	{
		return VMProcessorStatusUtilities.ConvertOperationalStatus(GetProperty<ushort[]>("OperationalStatus"));
	}

	public IReadOnlyList<string> GetOperationalStatusDescriptions()
	{
		return GetProperty<string[]>("StatusDescriptions");
	}
}
