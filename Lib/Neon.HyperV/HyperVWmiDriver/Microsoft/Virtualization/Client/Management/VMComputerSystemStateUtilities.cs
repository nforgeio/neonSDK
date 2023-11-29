using System;

namespace Microsoft.Virtualization.Client.Management;

internal static class VMComputerSystemStateUtilities
{
    internal static class OtherEnabledState
    {
        public const string Saving = "Saving";

        public const string Pausing = "Quiescing";

        public const string Resuming = "Resuming";

        public const string FastSaving = "FastSaving";

        public const string ComponentServicing = "ComponentServicing";
    }

    internal static bool IsVMComputerSystemStateValid(VMComputerSystemState state)
    {
        if (Enum.IsDefined(typeof(VMComputerSystemState), state))
        {
            return state != VMComputerSystemState.Unknown;
        }
        return false;
    }

    internal static VMComputerSystemState ConvertVMComputerSystemOtherState(string otherState)
    {
        if (string.Equals(otherState, "Quiescing", StringComparison.OrdinalIgnoreCase))
        {
            return VMComputerSystemState.Pausing;
        }
        if (string.Equals(otherState, "Resuming", StringComparison.OrdinalIgnoreCase))
        {
            return VMComputerSystemState.Resuming;
        }
        if (string.Equals(otherState, "Saving", StringComparison.OrdinalIgnoreCase))
        {
            return VMComputerSystemState.Saving;
        }
        if (string.Equals(otherState, "FastSaving", StringComparison.OrdinalIgnoreCase))
        {
            return VMComputerSystemState.FastSaving;
        }
        if (string.Equals(otherState, "ComponentServicing", StringComparison.OrdinalIgnoreCase))
        {
            return VMComputerSystemState.ComponentServicing;
        }
        return VMComputerSystemState.Unknown;
    }

    internal static bool IsVMComputerSystemHealthStateValid(VMComputerSystemHealthState healthState)
    {
        if (Enum.IsDefined(typeof(VMComputerSystemHealthState), healthState))
        {
            return healthState != VMComputerSystemHealthState.Unknown;
        }
        return false;
    }

    internal static VMComputerSystemOperationalStatus[] ConvertOperationalStatus(ushort[] operationalStatusValues)
    {
        if (operationalStatusValues == null)
        {
            return null;
        }
        VMComputerSystemOperationalStatus[] array = new VMComputerSystemOperationalStatus[operationalStatusValues.Length];
        for (int i = 0; i < operationalStatusValues.Length; i++)
        {
            if (operationalStatusValues[i] == 10 || operationalStatusValues[i] == 15)
            {
                array[i] = VMComputerSystemOperationalStatus.Ok;
            }
            else
            {
                array[i] = (VMComputerSystemOperationalStatus)operationalStatusValues[i];
            }
        }
        return array;
    }

    internal static FailoverReplicationState[] ConvertReplicationState(ushort[] replicationStateValues)
    {
        if (replicationStateValues == null)
        {
            return null;
        }
        FailoverReplicationState[] array = new FailoverReplicationState[replicationStateValues.Length];
        for (int i = 0; i < replicationStateValues.Length; i++)
        {
            array[i] = (FailoverReplicationState)replicationStateValues[i];
        }
        return array;
    }

    internal static FailoverReplicationHealth[] ConvertReplicationHealth(ushort[] replicationHealthValues)
    {
        if (replicationHealthValues == null)
        {
            return null;
        }
        FailoverReplicationHealth[] array = new FailoverReplicationHealth[replicationHealthValues.Length];
        for (int i = 0; i < replicationHealthValues.Length; i++)
        {
            array[i] = (FailoverReplicationHealth)replicationHealthValues[i];
        }
        return array;
    }

    public static string GetFailureStatusDescription(VMComputerSystemHealthState healthState, VMComputerSystemOperationalStatus[] operationalStatus, string[] statusDescriptions)
    {
        string result = string.Empty;
        if (healthState != VMComputerSystemHealthState.Ok && IsValidOperationalStatus(operationalStatus) && IsValidStatusDescriptions(statusDescriptions) && operationalStatus[0] != VMComputerSystemOperationalStatus.Ok)
        {
            result = statusDescriptions[0];
            if (statusDescriptions.Length > 1 && !string.IsNullOrEmpty(statusDescriptions[1]))
            {
                result = statusDescriptions[1];
            }
        }
        return result;
    }

    private static bool IsValidStatusDescriptions(string[] statusDescriptions)
    {
        if (statusDescriptions == null || statusDescriptions.Length == 0 || string.IsNullOrEmpty(statusDescriptions[0]))
        {
            return false;
        }
        return true;
    }

    private static bool IsValidOperationalStatus(VMComputerSystemOperationalStatus[] operationalStatus)
    {
        if (operationalStatus == null || operationalStatus.Length == 0)
        {
            return false;
        }
        return true;
    }
}
