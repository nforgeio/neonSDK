using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.HyperV.PowerShell.ExtensionMethods;

internal static class EnumerableSelectExtensions
{
	internal static IEnumerable<TOutput> SelectWithLogging<TInput, TOutput>(this IEnumerable<TInput> inputs, Func<TInput, TOutput> action, IOperationWatcher operationWatcher) where TOutput : class
	{
		return from input in inputs
			select ExceptionHelper.TryWithLogging(action, input, operationWatcher) into output
			where output != null
			select output;
	}

	internal static IEnumerable<TOutput> SelectManyWithLogging<TInput, TOutput>(this IEnumerable<TInput> inputs, Func<TInput, IEnumerable<TOutput>> action, IOperationWatcher operationWatcher) where TOutput : class
	{
		return from output in inputs.SelectMany((TInput input) => ExceptionHelper.TryManyWithLogging(action, input, operationWatcher))
			where output != null
			select output;
	}
}
