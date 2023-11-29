namespace Microsoft.Virtualization.Client.Management;

[WmiName("CIM_DataFile")]
internal interface IDataFile : IVirtualizationManagementObject, IDeleteable
{
    [Key]
    string Path { get; }

    bool IsSystem { get; }

    bool IsHidden { get; }

    void Copy(string destinationFile);
}
