namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_Keyboard")]
internal interface IVMKeyboard : IVMDevice, IVirtualizationManagementObject
{
	bool UnicodeSupported { get; }

	void ReleaseKey(int key);

	void TypeText(string text);

	void TypeScancodes(byte[] scancodes);

	void TypeCtrlAltDel();
}
