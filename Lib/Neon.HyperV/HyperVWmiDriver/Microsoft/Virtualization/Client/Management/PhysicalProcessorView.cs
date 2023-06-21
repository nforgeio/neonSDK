namespace Microsoft.Virtualization.Client.Management;

internal class PhysicalProcessorView : View, IPhysicalProcessor, IVirtualizationManagementObject
{
    internal static class WmiPropertyNames
    {
        public const string NumberOfThreadsOfExecution = "NumberOfLogicalProcessors";
    }

    public int NumberOfThreadsOfExecution => NumberConverter.UInt32ToInt32(GetProperty<uint>("NumberOfLogicalProcessors"));
}
