namespace Microsoft.Virtualization.Client.Management.Clustering;

internal interface IMSClusterResourceBase : IVirtualizationManagementObject
{
    string Name { get; }

    string Owner { get; }
}
