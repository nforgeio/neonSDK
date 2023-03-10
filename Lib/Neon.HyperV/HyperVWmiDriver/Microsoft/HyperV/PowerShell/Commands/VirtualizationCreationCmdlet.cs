using System.Collections.Generic;

namespace Microsoft.HyperV.PowerShell.Commands;

internal abstract class VirtualizationCreationCmdlet<T> : VirtualizationCmdletBase
{
	internal abstract IList<T> CreateObjects(IOperationWatcher operationWatcher);

	internal override void PerformOperation(IOperationWatcher operationWatcher)
	{
		foreach (T item in CreateObjects(operationWatcher))
		{
			operationWatcher.WriteObject(item);
		}
	}
}
