using System;
using System.Globalization;

namespace Microsoft.Virtualization.Client.Management;

internal class Synth3DVramSize : IComparable<Synth3DVramSize>, IComparable<ulong>, IFormattable
{
	public ulong VramSize { get; private set; }

	public Synth3DVramSize(ulong vramSizeInBytes)
	{
		VramSize = vramSizeInBytes;
	}

	public string ToString(string formatSpecifier, IFormatProvider provider)
	{
		if (provider == null)
		{
			provider = CultureInfo.CurrentCulture;
		}
		if (formatSpecifier == null)
		{
			return string.Format(provider, "{0}", VramSize);
		}
		if (formatSpecifier.Equals("MB", StringComparison.CurrentCultureIgnoreCase))
		{
			return string.Format(provider, "{0}", VramSize / 1048576uL);
		}
		throw new FormatException();
	}

	public string ToString(string formatSpecifier)
	{
		return ToString(formatSpecifier, CultureInfo.CurrentCulture);
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.CurrentCulture, "{0}", VramSize);
	}

	public int CompareTo(Synth3DVramSize other)
	{
		if (other == null)
		{
			return -1;
		}
		return VramSize.CompareTo(other.VramSize);
	}

	public int CompareTo(ulong other)
	{
		return VramSize.CompareTo(other);
	}
}
