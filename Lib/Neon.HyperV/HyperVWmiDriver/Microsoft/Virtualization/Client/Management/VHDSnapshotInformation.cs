using System;
using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_VHDSnapshotInformation")]
internal sealed class VHDSnapshotInformation : EmbeddedInstance
{
    internal static class WmiPropertyNames
    {
        public const string FilePath = "FilePath";

        public const string SnapshotId = "SnapshotId";

        public const string SnapshotPath = "SnapshotPath";

        public const string ParentPathsList = "ParentPathsList";

        public const string CreationTime = "CreationTime";

        public const string RCTId = "ResilientChangeTrackingId";
    }

    public string FilePath => GetProperty<string>("FilePath");

    public string SnapshotId => GetProperty<string>("SnapshotId");

    public string SnapshotPath => GetProperty<string>("SnapshotPath");

    public DateTime CreationTime => WmiTypeConverters.DateTimeConverter.ConvertFromWmiType(GetProperty<string>("CreationTime"));

    public string RCTId => GetProperty<string>("ResilientChangeTrackingId");

    public IReadOnlyCollection<string> ParentPathsList => (IReadOnlyCollection<string>)(object)GetProperty<string[]>("ParentPathsList");
}
