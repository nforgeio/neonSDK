using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal interface IAddableVMNetworkAdapter : IAddableVMDevice<IEthernetPortSetting, IEthernetConnectionAllocationRequest>, IAddableVMDevice<IEthernetPortSetting>, IAddable, IHasAttachableComponent<IEthernetConnectionAllocationRequest>
{
}
