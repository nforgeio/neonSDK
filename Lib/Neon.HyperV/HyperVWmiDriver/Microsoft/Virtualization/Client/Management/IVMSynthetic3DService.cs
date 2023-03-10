namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_Synthetic3DService")]
internal interface IVMSynthetic3DService : IVirtualizationManagementObject
{
	IVMTask BeginEnableGPUForVirtualization(IVM3dVideoPhysical3dGPU physicalGPU);

	void EndEnableGPUForVirtualization(IVMTask task);

	IVMTask BeginDisableGPUForVirtualization(IVM3dVideoPhysical3dGPU physicalGPU);

	void EndDisableGPUForVirtualization(IVMTask task);
}
