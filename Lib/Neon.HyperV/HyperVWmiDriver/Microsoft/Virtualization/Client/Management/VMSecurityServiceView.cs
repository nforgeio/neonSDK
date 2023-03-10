#define TRACE
using System.Globalization;

namespace Microsoft.Virtualization.Client.Management;

internal class VMSecurityServiceView : View, IVMSecurityService, IVirtualizationManagementObject
{
	internal static class WmiMethodNames
	{
		public const string ModifySecuritySettings = "ModifySecuritySettings";

		public const string SetKeyProtector = "SetKeyProtector";

		public const string GetKeyProtector = "GetKeyProtector";

		public const string RestoreLastKnownGoodKeyProtector = "RestoreLastKnownGoodKeyProtector";
	}

	public IVMTask BeginSetKeyProtector(IVMSecuritySetting securitySettingData, byte[] rawKeyProtector)
	{
		string embeddedInstance = securitySettingData.GetEmbeddedInstance();
		object[] array = new object[3] { embeddedInstance, rawKeyProtector, null };
		uint result = InvokeMethod("SetKeyProtector", array);
		IVMTask iVMTask = BeginMethodTaskReturn(result, null, array[2]);
		iVMTask.ClientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.SetVMKeyProtectorFailed);
		return iVMTask;
	}

	public void EndSetKeyProtector(IVMTask task)
	{
		EndMethod(task, VirtualizationOperation.SetKeyProtector);
		VMTrace.TraceUserActionCompleted("Setting a key protector completed successfully.");
	}

	public byte[] GetKeyProtector(IVMSecuritySetting securitySettingData)
	{
		string embeddedInstance = securitySettingData.GetEmbeddedInstance();
		object[] array = new object[2] { embeddedInstance, null };
		uint num = InvokeMethod("GetKeyProtector", array);
		BeginMethodTaskReturn(num, null, null).ClientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.GetVMKeyProtectorFailed);
		if (num != View.ErrorCodeSuccess)
		{
			throw ThrowHelper.CreateVirtualizationOperationFailedException(string.Format(CultureInfo.CurrentCulture, ErrorMessages.GetVMKeyProtectorFailed), VirtualizationOperation.GetKeyProtector, num);
		}
		return array[1] as byte[];
	}

	public IVMTask BeginRestoreLastKnownGoodKeyProtector(IVMSecuritySetting securitySettingData)
	{
		string embeddedInstance = securitySettingData.GetEmbeddedInstance();
		object[] array = new object[2] { embeddedInstance, null };
		uint result = InvokeMethod("RestoreLastKnownGoodKeyProtector", array);
		IVMTask iVMTask = BeginMethodTaskReturn(result, null, array[1]);
		iVMTask.ClientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.RestoreLKGVMKeyProtectorFailed);
		return iVMTask;
	}

	public void EndRestoreLastKnownGoodKeyProtector(IVMTask task)
	{
		EndMethod(task, VirtualizationOperation.RestoreLastKnownGoodKeyProtector);
		VMTrace.TraceUserActionCompleted("Restoring the last known good key protector completed successfully.");
	}
}
