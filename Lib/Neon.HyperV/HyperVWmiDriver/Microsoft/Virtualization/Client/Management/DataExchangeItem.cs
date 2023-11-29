namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_KvpExchangeDataItem")]
internal sealed class DataExchangeItem : EmbeddedInstance
{
    private static class WmiPropertyNames
    {
        public const string Name = "Name";
    }

    public string Name => GetProperty<string>("Name");
}
