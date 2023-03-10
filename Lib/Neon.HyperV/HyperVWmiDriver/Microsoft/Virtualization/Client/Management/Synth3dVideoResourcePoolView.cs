#define TRACE
using System;
using System.Globalization;

namespace Microsoft.Virtualization.Client.Management;

internal class Synth3dVideoResourcePoolView : ResourcePoolView, ISynth3dVideoResourcePool, IResourcePool, IVirtualizationManagementObject, IDeleteableAsync, IDeleteable, IMetricMeasurableElement
{
	internal static class WmiPropertyNames
	{
		public const string Is3dVideoSupported = "Is3dVideoSupported";

		public const string IsGPUCapable = "IsGPUCapable";

		public const string CalculateVideoMemoryRequirements = "CalculateVideoMemoryRequirements";
	}

	public bool Is3dVideoSupported => GetProperty<bool>("Is3dVideoSupported");

	public bool IsGPUCapable => GetProperty<bool>("IsGPUCapable");

	public long CalculateVideoMemoryRequirements(int monitorResolution, int numberOfMonitors)
	{
		VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Starting calculate of video memory requirements"));
		long num = 0L;
		object[] array = new object[3] { monitorResolution, numberOfMonitors, num };
		uint num2 = InvokeMethod("CalculateVideoMemoryRequirements", array);
		if (num2 != View.ErrorCodeSuccess)
		{
			throw ThrowHelper.CreateVirtualizationOperationFailedException(string.Format(CultureInfo.CurrentCulture, ErrorMessages.CalculateVideoMemoryRequirements, monitorResolution, numberOfMonitors), VirtualizationOperation.CalculateVideoMemoryRequirements, num2);
		}
		num = Convert.ToInt64(array[2].ToString(), CultureInfo.InvariantCulture);
		VMTrace.TraceUserActionCompleted("Video memory calculation completed successfully.");
		return num;
	}
}
