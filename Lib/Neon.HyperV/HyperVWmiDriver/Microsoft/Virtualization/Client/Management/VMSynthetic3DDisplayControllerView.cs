namespace Microsoft.Virtualization.Client.Management;

internal class VMSynthetic3DDisplayControllerView : VMDeviceView, IVMSynthetic3DDisplayController, IVMDevice, IVirtualizationManagementObject
{
	internal static class WmiPropertyNames
	{
		public const string AllocatedGPU = "AllocatedGPU";
	}

	public string AllocatedGPU => GetProperty<string>("AllocatedGPU");

	public IVM3dVideoPhysical3dGPU GetPhysical3dGraphicsProcessor()
	{
		return GetRelatedObject<IVM3dVideoPhysical3dGPU>(base.Associations.Synth3dDisplayControllerToPhysical3dGraphicsProcessor, throwIfNotFound: false);
	}
}
