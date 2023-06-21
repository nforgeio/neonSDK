using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.HyperV.PowerShell.ExtensionMethods;

internal static class EnumerableValidationExtensions
{
    internal static bool IsNullOrEmpty(this string str)
    {
        return string.IsNullOrEmpty(str);
    }

    internal static bool IsNullOrEmpty(this IEnumerable sequence)
    {
        if (sequence == null)
        {
            return true;
        }
        return !sequence.GetEnumerator().MoveNext();
    }

    internal static bool IsNullOrEmpty<TType>(this IEnumerable<TType> sequence)
    {
        return ((IEnumerable)sequence).IsNullOrEmpty();
    }

    internal static bool ContainsNullEntries<TType>(this IEnumerable<TType> sequence) where TType : class
    {
        return sequence.Any((TType entry) => entry == null);
    }

    internal static bool IsNullOrEmptyIncludingEntries<TType>(this TType[] array) where TType : class
    {
        if (!array.IsNullOrEmpty())
        {
            return array.ContainsNullEntries();
        }
        return true;
    }

    internal static bool ContainsNullOrEmptyEntries(this IEnumerable<string> sequence)
    {
        return sequence.Any(string.IsNullOrEmpty);
    }

    internal static bool IsNullOrEmptyIncludingEntries(this string[] stringArray)
    {
        if (!stringArray.IsNullOrEmpty())
        {
            return stringArray.ContainsNullOrEmptyEntries();
        }
        return true;
    }
}
