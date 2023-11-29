using System.Collections.ObjectModel;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_VHDSetInformation")]
internal class VHDSetInformation : EmbeddedInstance
{
    internal static class WmiPropertyNames
    {
        public const string Path = "Path";

        public const string SnapshotIdList = "SnapshotIdList";

        public const string AllPaths = "AllPaths";
    }

    public string Path => GetProperty<string>("Path");

    public ReadOnlyCollection<string> SnapshotIdList => new ReadOnlyCollection<string>(GetProperty("SnapshotIdList", new string[0]));

    public ReadOnlyCollection<string> AllPaths => new ReadOnlyCollection<string>(GetProperty("AllPaths", new string[0]));
}
