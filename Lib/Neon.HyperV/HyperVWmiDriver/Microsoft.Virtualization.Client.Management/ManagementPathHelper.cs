using System;
using System.Globalization;

namespace Microsoft.Virtualization.Client.Management;

internal static class ManagementPathHelper
{
	internal enum QuoteType
	{
		Single,
		Double
	}

	public static string EscapePropertyValue(string propertyValue, QuoteType quoteType)
	{
		if (propertyValue == null)
		{
			throw new ArgumentNullException("propertyValue");
		}
		if (quoteType != 0 && quoteType != QuoteType.Double)
		{
			throw new ArgumentOutOfRangeException("quoteType", string.Format(CultureInfo.CurrentCulture, ErrorMessages.ArgumentOutOfRange_InvalidEnumValue, quoteType.ToString(), typeof(QuoteType).Name));
		}
		string text = propertyValue.Replace("\\", "\\\\");
		if (quoteType != 0)
		{
			return text.Replace("\"", "\\\"");
		}
		return text.Replace("'", "\\'");
	}
}
