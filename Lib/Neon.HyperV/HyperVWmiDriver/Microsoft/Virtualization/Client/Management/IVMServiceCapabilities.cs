using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_VirtualSystemManagementCapabilities")]
internal interface IVMServiceCapabilities : IVirtualizationManagementObject
{
	IEnumerable<IVMComputerSystemSetting> ComputerSystemSettings { get; }

	IVMComputerSystemSetting DefaultComputerSystemSetting { get; }

	IEnumerable<IVMComputerSystemSetting> SupportedVersionSettings { get; }

	IEnumerable<IVMComputerSystemSetting> SupportedSecureBootTemplateSettings { get; }
}
