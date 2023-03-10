using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace Microsoft.HyperV.PowerShell;

internal sealed class WildcardPatternMatcher
{
	internal const WildcardOptions Options = WildcardOptions.Compiled | WildcardOptions.IgnoreCase;

	internal IEnumerable<WildcardPattern> Patterns { get; private set; }

	internal WildcardPatternMatcher(IEnumerable<string> patternStrings)
	{
		Patterns = patternStrings.Select((string pattern) => new WildcardPattern(pattern, WildcardOptions.Compiled | WildcardOptions.IgnoreCase));
	}

	internal static bool IsPatternMatching(string pattern, string value)
	{
		if (string.IsNullOrEmpty(pattern))
		{
			return true;
		}
		if (value == null)
		{
			return false;
		}
		return new WildcardPattern(pattern, WildcardOptions.Compiled | WildcardOptions.IgnoreCase).IsMatch(value);
	}

	internal bool MatchesAny(string target)
	{
		return Patterns.Any((WildcardPattern pattern) => pattern.IsMatch(target));
	}
}
