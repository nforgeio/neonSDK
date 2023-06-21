using Microsoft.HyperV.PowerShell.Common;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Virtualization.Client.Management;

using ErrorMessages = Microsoft.HyperV.PowerShell.Common.ErrorMessages;

namespace Microsoft.HyperV.PowerShell;

internal sealed class GuestServiceInterfaceComponent : VMIntegrationComponent
{
    internal override string PutDescription => TaskDescriptions.SetVMGuestServiceIntegrationComponent;

    internal GuestServiceInterfaceComponent(IVMGuestServiceInterfaceComponentSetting setting, VirtualMachineBase parentVirtualMachineObject)
        : base(setting, parentVirtualMachineObject)
    {
    }

    internal void CopyFileToGuest(string sourcePath, string destinationPath, bool overwriteExisting, bool createFullPath, IOperationWatcher watcher)
    {
        IVMGuestServiceInterfaceComponent iVMGuestServiceInterfaceComponent = (IVMGuestServiceInterfaceComponent)m_IntegrationComponentSetting.GetData(UpdatePolicy.EnsureAssociatorsUpdated).GetIntegrationComponent();
        if (iVMGuestServiceInterfaceComponent == null)
        {
            throw ExceptionHelper.CreateOperationFailedException(ErrorMessages.OperationFailed_InvalidState);
        }
        IVMGuestFileService service = iVMGuestServiceInterfaceComponent.FileService;
        watcher.PerformOperation(() => service.BeginCopyFilesToGuest(sourcePath, destinationPath, overwriteExisting, createFullPath), service.EndCopyFilesToGuest, TaskDescriptions.CopyFileToGuestJob, null);
    }
}
