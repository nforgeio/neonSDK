namespace Microsoft.Virtualization.Client.Management;

internal static class WmiTypeConverters
{
	public static DateTimeConverter DateTimeConverter = new DateTimeConverter();

	public static TimeSpanConverter TimeSpanConverter = new TimeSpanConverter();

	public static GuidStringConverter GuidStringConverter = new GuidStringConverter();
}
