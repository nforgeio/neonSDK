using System;
using System.Globalization;

namespace Microsoft.Virtualization.Client.Management;

internal class Synth3DResolution : IComparable<Synth3DResolution>, IFormattable
{
	public uint Horizontal { get; private set; }

	public uint Vertical { get; private set; }

	public Synth3DResolution(uint horizontal, uint vertical)
	{
		Horizontal = horizontal;
		Vertical = vertical;
	}

	public static bool TryParse(string resolutionString, out Synth3DResolution resolutionObj)
	{
		resolutionObj = null;
		if (string.IsNullOrWhiteSpace(resolutionString))
		{
			return false;
		}
		string[] array = resolutionString.ToLowerInvariant().Split('x');
		if (array.Length != 2)
		{
			return false;
		}
		if (uint.TryParse(array[0], out var result) && uint.TryParse(array[1], out var result2))
		{
			resolutionObj = new Synth3DResolution(result, result2);
		}
		return resolutionObj != null;
	}

	public string ToString(string formatSpecifier, IFormatProvider provider)
	{
		if (provider == null)
		{
			provider = CultureInfo.CurrentCulture;
		}
		if (formatSpecifier == null)
		{
			return string.Format(provider, "{0}x{1}", Horizontal, Vertical);
		}
		if (formatSpecifier.Equals("D", StringComparison.CurrentCultureIgnoreCase))
		{
			return string.Format(provider, "{0} x {1}", Horizontal, Vertical);
		}
		throw new FormatException();
	}

	public string ToString(string formatSpecifier)
	{
		return ToString(formatSpecifier, CultureInfo.CurrentCulture);
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.CurrentCulture, "{0}x{1}", Horizontal, Vertical);
	}

	public int CompareTo(Synth3DResolution other)
	{
		if (other == null)
		{
			return -1;
		}
		if (Horizontal == other.Horizontal)
		{
			return Vertical.CompareTo(other.Vertical);
		}
		return Horizontal.CompareTo(other.Horizontal);
	}
}
