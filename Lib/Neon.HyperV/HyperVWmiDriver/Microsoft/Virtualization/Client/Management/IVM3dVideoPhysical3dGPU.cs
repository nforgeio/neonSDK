using System;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_Physical3dGraphicsProcessor")]
internal interface IVM3dVideoPhysical3dGPU : IVMDevice, IVirtualizationManagementObject
{
    string Name { get; }

    bool EnabledForVirtualization { get; }

    bool CompatibleForVirtualization { get; }

    string GPUID { get; }

    string InstanceID { get; }

    long AdapterIndexID { get; }

    string DirectXVersion { get; }

    string PixelShaderVersion { get; }

    long DedicatedVideoMemory { get; }

    long DedicatedSystemMemory { get; }

    long SharedSystemMemory { get; }

    long AvailableVideoMemory { get; }

    long TotalVideoMemory { get; }

    string DriverProvider { get; }

    DateTime DriverDate { get; }

    DateTime DriverInstalled { get; }

    string DriverVersion { get; }

    string DriverModelVersion { get; }
}
