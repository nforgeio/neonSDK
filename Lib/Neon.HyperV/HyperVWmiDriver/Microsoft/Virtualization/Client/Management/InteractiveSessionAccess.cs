namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_InteractiveSessionACE")]
internal class InteractiveSessionAccess : EmbeddedInstance, IInteractiveSessionAccess
{
    public ushort AccessType => GetProperty("AccessType", (ushort)0);

    public string Trustee => GetProperty<string>("Trustee");
}
