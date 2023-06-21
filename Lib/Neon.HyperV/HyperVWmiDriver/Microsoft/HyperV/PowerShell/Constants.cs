using System;

namespace Microsoft.HyperV.PowerShell;

internal static class Constants
{
    internal static class ReplicationConstants
    {
        public const string AllowAllServer = "*";

        public const string DefaultTrustGroup = "DEFAULT";

        public const string DefaultReplicationProviderId = "22391CDC-272C-4DDF-BA88-9BEFB1A0975C";

        public const int MinPortNumber = 1;

        public const int MaxPortNumber = 65535;

        public const int DefaultHttpPort = 80;

        public const int DefaultHttpsPort = 443;

        public const int MinVSSSnapshotFrequencyHours = 1;

        public const int MaxVSSSnapshotFrequencyHours = 12;

        public const int DefaultVSSSnapshotFrequencyHours = 4;

        public const int MinRecoveryHistoryHours = 0;

        public const int MaxRecoveryHistoryHours = 24;

        public const int MinReplicationFrequencySeconds = 30;

        public const int MaxReplicationFrequencySeconds = 900;

        public const int MaxInitialReplicationStartDays = 7;
    }

    internal static readonly TimeSpan UpdateThreshold = TimeSpan.FromSeconds(5.0);

    internal static readonly TimeSpan TaskUpdateProgressThreshold = TimeSpan.FromSeconds(1.0);

    internal const short DefaultVMGeneration = 1;

    internal const int PercentageMinimum = 0;

    internal const int PercentageFull = 100;

    internal const int NoError = 0;

    internal const int UnknownControllerNumber = -1;

    internal const string SnapshotsDirectory = "Snapshots";

    internal const string VirtualMachinesDirectory = "Virtual Machines";

    internal const string VirtualHardDisksDirectory = "Virtual Hard Disks";

    internal static readonly DateTime Win32FileTimeEpoch = new DateTime(1601, 1, 1).ToLocalTime();

    public const string DisableIdeVhdTagFormat = "DisableVhdMoveIDE_{0}_Location_{1}";

    public const string DisableScsiVhdTagFormat = "DisableVhdMoveSCSI_{0}_Location_{1}";

    public const string DisablePmemVhdTagFormat = "DisableVhdMovePMEM_{0}_Location_{1}";

    internal static int Mega = 1048576;
}
