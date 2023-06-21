using System.Management.Automation;

namespace Microsoft.HyperV.PowerShell;

internal interface IOperationWatcher
{
    void WriteObject(object output);

    void WriteVerbose(string message);

    void WriteWarning(string message);

    void WriteError(ErrorRecord record);

    void Watch(WatchableTask task);

    bool ShouldProcess(string description);

    bool ShouldContinue(string description);
}
