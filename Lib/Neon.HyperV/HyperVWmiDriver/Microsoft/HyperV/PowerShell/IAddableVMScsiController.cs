using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal interface IAddableVMScsiController : IAddableVMDevice<IVMScsiControllerSetting>, IAddable
{
}
