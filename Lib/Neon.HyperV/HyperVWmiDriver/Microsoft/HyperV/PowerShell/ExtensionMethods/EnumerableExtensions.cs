using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.HyperV.PowerShell.ExtensionMethods;

internal static class EnumerableExtensions
{
	internal static IEnumerable<TObject> Apply<TObject>(this IEnumerable<TObject> inputs, Action<TObject> updater)
	{
		return inputs.Select((TObject input) => ApplyOne(input, updater));
	}

	private static TObject ApplyOne<TObject>(TObject input, Action<TObject> updater)
	{
		updater(input);
		return input;
	}

	internal static IEnumerable<TResult> ZipWithLogging<TInput1, TInput2, TResult>(this IEnumerable<TInput1> firstInputs, IEnumerable<TInput2> secondInputs, Func<TInput1, TInput2, TResult> resultSelector, IOperationWatcher operationWatcher) where TResult : class
	{
		return from output in firstInputs.Zip(secondInputs, (TInput1 first, TInput2 second) => ExceptionHelper.TryWithLogging(resultSelector, first, second, operationWatcher))
			where output != null
			select output;
	}
}
