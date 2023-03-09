namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_InteractiveSessionACE")]
internal interface IInteractiveSessionAccess
{
	ushort AccessType { get; }

	string Trustee { get; }
}
