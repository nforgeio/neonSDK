#define TRACE
using System;
using System.Globalization;

namespace Microsoft.Virtualization.Client.Management;

internal class VMSynthetic3DServiceView : View, IVMSynthetic3DService, IVirtualizationManagementObject
{
	internal static class WmiMemberNames
	{
		public const string EnableGPUForVirtualization = "EnableGPUForVirtualization";

		public const string DisableGPUForVirtualization = "DisableGPUForVirtualization";
	}

	public IVMTask BeginEnableGPUForVirtualization(IVM3dVideoPhysical3dGPU physicalGPU)
	{
		if (physicalGPU == null)
		{
			throw new ArgumentNullException("physicalGPU");
		}
		string clientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.EnableGPUForVirtualization, physicalGPU.FriendlyName);
		VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Starting enablement of GPU '{0}' for virtualization", physicalGPU.FriendlyName));
		object[] array = new object[2] { physicalGPU, null };
		uint result = InvokeMethod("EnableGPUForVirtualization", array);
		IVMTask iVMTask = BeginMethodTaskReturn(result, null, array[1]);
		iVMTask.ClientSideFailedMessage = clientSideFailedMessage;
		return iVMTask;
	}

	public void EndEnableGPUForVirtualization(IVMTask task)
	{
		EndMethod(task, VirtualizationOperation.EnableGPUForVirtualization);
		VMTrace.TraceUserActionCompleted("Enablement of GPU for virtualization completed successfully.");
	}

	public IVMTask BeginDisableGPUForVirtualization(IVM3dVideoPhysical3dGPU physicalGPU)
	{
		if (physicalGPU == null)
		{
			throw new ArgumentNullException("physicalGPU");
		}
		string clientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.DisableGPUForVirtualization, physicalGPU.FriendlyName);
		VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Starting disablement of GPU '{0}' for virtualization", physicalGPU.FriendlyName));
		object[] array = new object[2] { physicalGPU, null };
		uint result = InvokeMethod("DisableGPUForVirtualization", array);
		IVMTask iVMTask = BeginMethodTaskReturn(result, null, array[1]);
		iVMTask.ClientSideFailedMessage = clientSideFailedMessage;
		return iVMTask;
	}

	public void EndDisableGPUForVirtualization(IVMTask task)
	{
		EndMethod(task, VirtualizationOperation.DisableGPUForVirtualization);
		VMTrace.TraceUserActionCompleted("Disable GPU for virtualization completed successfully.");
	}
}
