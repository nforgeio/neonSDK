namespace Microsoft.Virtualization.Client.Management;

internal interface IEthernetFeature : IVirtualizationManagementObject
{
    [Key]
    string InstanceId { get; }

    EthernetFeatureType FeatureType { get; }

    string Name { get; }

    string ExtensionId { get; }

    string FeatureId { get; }

    IVMTask BeginModifySingleFeature(IEthernetSwitchFeatureService service);

    void EndModifySingleFeature(IVMTask modifyTask);
}
