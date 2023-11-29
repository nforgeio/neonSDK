using System;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal class TaskUpdateEventArgs : EventArgs
{
    public string Caption { get; internal set; }

    public int PercentComplete { get; internal set; }

    public bool IsCompleted { get; internal set; }

    public static TaskUpdateEventArgs CreateForTask(IVMTask task, string caption)
    {
        return new TaskUpdateEventArgs
        {
            Caption = caption,
            PercentComplete = task.PercentComplete,
            IsCompleted = task.IsCompleted
        };
    }
}
