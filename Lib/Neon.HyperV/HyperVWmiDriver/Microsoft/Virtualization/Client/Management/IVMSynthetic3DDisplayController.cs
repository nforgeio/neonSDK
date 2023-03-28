namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_Synthetic3DDisplayController")]
internal interface IVMSynthetic3DDisplayController : IVMDevice, IVirtualizationManagementObject
{
	string AllocatedGPU { get; }

	IVM3dVideoPhysical3dGPU GetPhysical3dGraphicsProcessor();
}
