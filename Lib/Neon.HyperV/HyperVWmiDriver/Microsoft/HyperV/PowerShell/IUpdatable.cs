namespace Microsoft.HyperV.PowerShell;

internal interface IUpdatable
{
	void Put(IOperationWatcher operationWatcher);
}
