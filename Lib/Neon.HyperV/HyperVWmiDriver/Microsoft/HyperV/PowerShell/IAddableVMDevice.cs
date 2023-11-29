using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal interface IAddableVMDevice<TVirtManDeviceSetting> : IAddable where TVirtManDeviceSetting : IVMDeviceSetting, IDeleteableAsync
{
    string DescriptionForAdd { get; }

    TVirtManDeviceSetting GetDeviceSetting(UpdatePolicy policy);

    void FinishAddingDeviceSetting(TVirtManDeviceSetting deviceSetting);
}
internal interface IAddableVMDevice<TPrimary, TComponent> : IAddableVMDevice<TPrimary>, IAddable, IHasAttachableComponent<TComponent> where TPrimary : IVMDeviceSetting, IDeleteableAsync where TComponent : IVMDeviceSetting, IDeleteableAsync
{
    string DescriptionForAddRollback { get; }
}
