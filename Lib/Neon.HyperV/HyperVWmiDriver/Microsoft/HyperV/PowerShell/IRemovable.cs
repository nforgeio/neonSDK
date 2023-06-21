namespace Microsoft.HyperV.PowerShell;

internal interface IRemovable
{
    void Remove(IOperationWatcher operationWatcher);
}
