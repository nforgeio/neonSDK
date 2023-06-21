namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_Synth3dVideoPool")]
internal interface ISynth3dVideoResourcePool : IResourcePool, IVirtualizationManagementObject, IDeleteableAsync, IDeleteable, IMetricMeasurableElement
{
    bool Is3dVideoSupported { get; }

    bool IsGPUCapable { get; }

    long CalculateVideoMemoryRequirements(int monitorResolution, int numberOfMonitors);
}
