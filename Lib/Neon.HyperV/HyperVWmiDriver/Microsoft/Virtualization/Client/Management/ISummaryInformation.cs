namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_SummaryInformation", PrimaryMapping = true)]
internal interface ISummaryInformation : ISummaryInformationProperties, ISummaryInformationPropertiesBase, ISummaryInformationBase, IVirtualizationManagementObject
{
    ISummaryInformationSnapshot CreateSnapshot();
}
