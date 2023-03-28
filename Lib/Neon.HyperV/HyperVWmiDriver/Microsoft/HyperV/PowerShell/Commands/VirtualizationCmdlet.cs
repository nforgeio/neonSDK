using System;
using System.Collections.Generic;

namespace Microsoft.HyperV.PowerShell.Commands;

internal abstract class VirtualizationCmdlet<T> : VirtualizationCmdletBase
{
	internal abstract IList<T> EnumerateOperands(IOperationWatcher operationWatcher);

	internal virtual void ValidateOperandList(IList<T> operands, IOperationWatcher operationWatcher)
	{
	}

	internal void ProcessOperands(IList<T> operands, IOperationWatcher operationWatcher)
	{
		foreach (T operand in operands)
		{
			try
			{
				ProcessOneOperand(operand, operationWatcher);
			}
			catch (Exception e)
			{
				ExceptionHelper.DisplayErrorOnException(e, operationWatcher);
			}
		}
	}

	internal abstract void ProcessOneOperand(T operand, IOperationWatcher operationWatcher);

	internal override void PerformOperation(IOperationWatcher operationWatcher)
	{
		IList<T> operands = EnumerateOperands(operationWatcher);
		ValidateOperandList(operands, operationWatcher);
		ProcessOperands(operands, operationWatcher);
	}
}
