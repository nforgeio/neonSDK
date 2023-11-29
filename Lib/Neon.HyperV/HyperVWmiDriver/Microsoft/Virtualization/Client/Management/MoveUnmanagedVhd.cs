using System;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_MoveUnmanagedVhd")]
internal sealed class MoveUnmanagedVhd : EmbeddedInstance
{
    private static class WmiPropertyNames
    {
        public const string VhdSourcePath = "VhdSourcePath";

        public const string VhdDestinationPath = "VhdDestinationPath";
    }

    public MoveUnmanagedVhd()
    {
    }

    public MoveUnmanagedVhd(Server server, string sourcePath, string destinationPath)
        : base(server, server.VirtualizationNamespace, "Msvm_MoveUnmanagedVhd")
    {
        if (string.IsNullOrEmpty(sourcePath))
        {
            throw new ArgumentNullException("sourcePath");
        }
        if (string.IsNullOrEmpty(destinationPath))
        {
            throw new ArgumentNullException("destinationPath");
        }
        AddProperty("VhdSourcePath", sourcePath);
        AddProperty("VhdDestinationPath", destinationPath);
    }
}
