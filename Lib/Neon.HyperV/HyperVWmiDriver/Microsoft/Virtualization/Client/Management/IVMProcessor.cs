using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_Processor")]
internal interface IVMProcessor : IVMDevice, IVirtualizationManagementObject
{
	int LoadPercentage { get; }

	IReadOnlyList<VMProcessorOperationalStatus> GetOperationalStatus();

	IReadOnlyList<string> GetOperationalStatusDescriptions();
}
