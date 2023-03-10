using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal interface IAddableVMFibreChannelHba : IAddableVMDevice<IFibreChannelPortSetting, IFcPoolAllocationSetting>, IAddableVMDevice<IFibreChannelPortSetting>, IAddable, IHasAttachableComponent<IFcPoolAllocationSetting>
{
}
