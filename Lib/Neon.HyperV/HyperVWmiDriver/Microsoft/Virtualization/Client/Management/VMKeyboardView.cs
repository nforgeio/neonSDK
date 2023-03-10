#define TRACE
using System.Globalization;

namespace Microsoft.Virtualization.Client.Management;

internal class VMKeyboardView : VMDeviceView, IVMKeyboard, IVMDevice, IVirtualizationManagementObject
{
	internal static class WmiMethodNames
	{
		public const string ReleaseKey = "ReleaseKey";

		public const string TypeText = "TypeText";

		public const string TypeScancodes = "TypeScancodes";

		public const string TypeCtrlAltDel = "TypeCtrlAltDel";
	}

	internal static class WmiPropertyNames
	{
		public const string UnicodeSupported = "UnicodeSupported";
	}

	private class VMKeyboardErrorCodeMapper : ErrorCodeMapper
	{
		public override string MapError(VirtualizationOperation operation, long errorCode, string operationFailedMsg)
		{
			if ((operation == VirtualizationOperation.TypeScancodes || operation == VirtualizationOperation.TypeText) && errorCode == 32773)
			{
				return ErrorMessages.KeyboardTypeScanCodesFailed_TooManyCharacters;
			}
			return base.MapError(operation, errorCode, operationFailedMsg);
		}
	}

	public bool UnicodeSupported => GetProperty<bool>("UnicodeSupported");

	protected override ErrorCodeMapper GetErrorCodeMapper()
	{
		return new VMKeyboardErrorCodeMapper();
	}

	public void ReleaseKey(int key)
	{
		object[] args = new object[1] { key };
		VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Simulating releasing key (keycode = '{0}').", key));
		uint num = InvokeMethod("ReleaseKey", args);
		if (num != View.ErrorCodeSuccess)
		{
			throw ThrowHelper.CreateVirtualizationOperationFailedException(ErrorMessages.KeyboardDeviceFailed, VirtualizationOperation.TypeText, num);
		}
		VMTrace.TraceUserActionCompleted("Simulating key release completed successfully.");
	}

	public void TypeText(string text)
	{
		object[] args = new object[1] { text };
		VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Simulating typing ASCII characters '{0}'.", text));
		uint num = InvokeMethod("TypeText", args);
		if (num != View.ErrorCodeSuccess)
		{
			throw ThrowHelper.CreateVirtualizationOperationFailedException(ErrorMessages.KeyboardDeviceFailed, VirtualizationOperation.TypeText, num, GetErrorCodeMapper(), null);
		}
		VMTrace.TraceUserActionCompleted("Simulating typing ASCII characters completed successfully.");
	}

	public void TypeScancodes(byte[] scancodes)
	{
		object[] args = new object[1] { scancodes };
		VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Simulating typing scancodes '{0}'.", HexEncoding.ToString(scancodes)));
		uint num = InvokeMethod("TypeScancodes", args);
		if (num != View.ErrorCodeSuccess)
		{
			throw ThrowHelper.CreateVirtualizationOperationFailedException(ErrorMessages.KeyboardDeviceFailed, VirtualizationOperation.TypeScancodes, num, GetErrorCodeMapper(), null);
		}
		VMTrace.TraceUserActionCompleted("Simulating typing scancodes completed successfully.");
	}

	public void TypeCtrlAltDel()
	{
		VMTrace.TraceUserActionInitiated("Simulating typing Ctrl-Alt-Del.");
		uint num = InvokeMethod("TypeCtrlAltDel", null);
		if (num != View.ErrorCodeSuccess)
		{
			throw ThrowHelper.CreateVirtualizationOperationFailedException(ErrorMessages.KeyboardDeviceFailed, VirtualizationOperation.TypeText, num);
		}
		VMTrace.TraceUserActionCompleted("Simulating typing Ctrl-Alt-Del completed successfully.");
	}
}
