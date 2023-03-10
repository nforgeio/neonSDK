using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_CDROMDrive")]
[SuppressMessage("Microsoft.Naming", "CA1705", Justification = "VMCD is not one acronym, it is two acronyms of two letters next to each other.")]
internal interface IVMCDRomDrive : IVMDrive, IVMDevice, IVirtualizationManagementObject
{
}
