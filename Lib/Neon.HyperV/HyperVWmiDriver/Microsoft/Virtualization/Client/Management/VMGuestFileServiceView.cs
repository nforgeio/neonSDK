#define TRACE
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.Virtualization.Client.Management;

internal class VMGuestFileServiceView : VMIntegrationComponentView, IVMGuestFileService, IVMIntegrationComponent, IVMDevice, IVirtualizationManagementObject
{
    private static class WmiNames
    {
        public const string CopyFilesToGuest = "CopyFilesToGuest";
    }

    public IVMTask BeginCopyFilesToGuest(string sourcePath, string destinationPath, bool overwriteExisting, bool createFullPath)
    {
        if (string.IsNullOrWhiteSpace(sourcePath))
        {
            throw new ArgumentNullException("sourcePath");
        }
        if (string.IsNullOrWhiteSpace(sourcePath))
        {
            throw new ArgumentNullException("sourcePath");
        }
        VMTrace.TraceUserActionInitiated("Starting to copy files to guest", "Source: " + sourcePath, "Destination: " + destinationPath, "Overwrite: " + overwriteExisting, "CreateFullPath: " + createFullPath);
        Dictionary<string, object> propertyValues = new Dictionary<string, object>
        {
            { "SourcePath", sourcePath },
            { "DestinationPath", destinationPath },
            { "OverwriteExisting", overwriteExisting },
            { "CreateFullPath", createFullPath }
        };
        string newEmbeddedInstance = base.Server.GetNewEmbeddedInstance("Msvm_CopyFileToGuestSettingData", propertyValues);
        object[] array = new object[2]
        {
            new string[1] { newEmbeddedInstance },
            null
        };
        uint result = InvokeMethod("CopyFilesToGuest", array);
        IVMTask iVMTask = BeginMethodTaskReturn(result, null, array[1]);
        iVMTask.ClientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.CopyFileToGuestFailed, sourcePath, destinationPath);
        return iVMTask;
    }

    public void EndCopyFilesToGuest(IVMTask task)
    {
        if (task == null)
        {
            throw new ArgumentNullException("task");
        }
        EndMethod(task, VirtualizationOperation.CopyDataFile);
        VMTrace.TraceUserActionCompleted("Copy VM files completed successfully.");
    }
}
