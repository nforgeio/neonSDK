using System;
using System.Globalization;

namespace Microsoft.Virtualization.Client.Management;

internal sealed class ManagementDateTimeConverter
{
    private const int SIZEOFDMTFDATETIME = 25;

    private const int MAXSIZE_UTC_DMTF = 999;

    private const long MAXDATE_INTIMESPAN = 99999999L;

    private ManagementDateTimeConverter()
    {
    }

    public static DateTime ToDateTime(string dmtfDate)
    {
        int num = DateTime.MinValue.Year;
        int num2 = DateTime.MinValue.Month;
        int num3 = DateTime.MinValue.Day;
        int num4 = DateTime.MinValue.Hour;
        int num5 = DateTime.MinValue.Minute;
        int num6 = DateTime.MinValue.Second;
        int millisecond = 0;
        DateTime minValue = DateTime.MinValue;
        if (dmtfDate == null)
        {
            throw new ArgumentOutOfRangeException("dmtfDate");
        }
        if (dmtfDate.Length == 0)
        {
            throw new ArgumentOutOfRangeException("dmtfDate");
        }
        if (dmtfDate.Length != 25)
        {
            throw new ArgumentOutOfRangeException("dmtfDate");
        }
        IFormatProvider provider = (IFormatProvider)CultureInfo.InvariantCulture.GetFormat(typeof(int));
        long num7 = 0L;
        try
        {
            string empty = string.Empty;
            empty = dmtfDate.Substring(0, 4);
            if ("****" != empty)
            {
                num = int.Parse(empty, provider);
            }
            empty = dmtfDate.Substring(4, 2);
            if ("**" != empty)
            {
                num2 = int.Parse(empty, provider);
            }
            empty = dmtfDate.Substring(6, 2);
            if ("**" != empty)
            {
                num3 = int.Parse(empty, provider);
            }
            empty = dmtfDate.Substring(8, 2);
            if ("**" != empty)
            {
                num4 = int.Parse(empty, provider);
            }
            empty = dmtfDate.Substring(10, 2);
            if ("**" != empty)
            {
                num5 = int.Parse(empty, provider);
            }
            empty = dmtfDate.Substring(12, 2);
            if ("**" != empty)
            {
                num6 = int.Parse(empty, provider);
            }
            empty = dmtfDate.Substring(15, 6);
            if ("******" != empty)
            {
                num7 = long.Parse(empty, (IFormatProvider)CultureInfo.InvariantCulture.GetFormat(typeof(long))) * 10;
            }
            if (num < 0 || num2 < 0 || num3 < 0 || num4 < 0 || num5 < 0 || num6 < 0 || num7 < 0)
            {
                throw new ArgumentOutOfRangeException("dmtfDate");
            }
        }
        catch
        {
            throw new ArgumentOutOfRangeException("dmtfDate");
        }
        minValue = new DateTime(num, num2, num3, num4, num5, num6, millisecond).AddTicks(num7);
        long num8 = TimeZoneInfo.Local.GetUtcOffset(minValue).Ticks / 600000000;
        int num9 = 0;
        string text = dmtfDate.Substring(22, 3);
        long num10 = 0L;
        if ("***" != text)
        {
            text = dmtfDate.Substring(21, 4);
            try
            {
                num9 = int.Parse(text, provider);
            }
            catch
            {
                throw new ArgumentOutOfRangeException("dmtfDate");
            }
            num10 = num9 - num8;
            minValue = minValue.AddMinutes(num10 * -1);
        }
        return minValue;
    }

    public static string ToDmtfDateTime(DateTime date)
    {
        string empty = string.Empty;
        TimeSpan utcOffset = TimeZoneInfo.Local.GetUtcOffset(date);
        long value = utcOffset.Ticks / 600000000;
        IFormatProvider formatProvider = (IFormatProvider)CultureInfo.InvariantCulture.GetFormat(typeof(int));
        if (Math.Abs(value) > 999)
        {
            date = date.ToUniversalTime();
            empty = "+000";
        }
        else if (utcOffset.Ticks >= 0)
        {
            empty = "+" + (utcOffset.Ticks / 600000000).ToString(formatProvider).PadLeft(3, '0');
        }
        else
        {
            string text = value.ToString(formatProvider);
            empty = "-" + text.Substring(1, text.Length - 1).PadLeft(3, '0');
        }
        string text2 = string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(date.Year.ToString(formatProvider).PadLeft(4, '0') + date.Month.ToString(formatProvider).PadLeft(2, '0'), date.Day.ToString(formatProvider).PadLeft(2, '0')), date.Hour.ToString(formatProvider).PadLeft(2, '0')), date.Minute.ToString(formatProvider).PadLeft(2, '0')), date.Second.ToString(formatProvider).PadLeft(2, '0')), ".");
        DateTime dateTime = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, 0);
        string text3 = ((date.Ticks - dateTime.Ticks) * 1000 / 10000).ToString((IFormatProvider)CultureInfo.InvariantCulture.GetFormat(typeof(long)));
        if (text3.Length > 6)
        {
            text3 = text3.Substring(0, 6);
        }
        return string.Concat(text2 + text3.PadLeft(6, '0'), empty);
    }

    public static TimeSpan ToTimeSpan(string dmtfTimespan)
    {
        int num = 0;
        int num2 = 0;
        int num3 = 0;
        int num4 = 0;
        IFormatProvider provider = (IFormatProvider)CultureInfo.InvariantCulture.GetFormat(typeof(int));
        _ = TimeSpan.MinValue;
        if (dmtfTimespan == null)
        {
            throw new ArgumentOutOfRangeException("dmtfTimespan");
        }
        if (dmtfTimespan.Length == 0)
        {
            throw new ArgumentOutOfRangeException("dmtfTimespan");
        }
        if (dmtfTimespan.Length != 25)
        {
            throw new ArgumentOutOfRangeException("dmtfTimespan");
        }
        if (dmtfTimespan.Substring(21, 4) != ":000")
        {
            throw new ArgumentOutOfRangeException("dmtfTimespan");
        }
        long num5 = 0L;
        try
        {
            _ = string.Empty;
            num = int.Parse(dmtfTimespan.Substring(0, 8), provider);
            num2 = int.Parse(dmtfTimespan.Substring(8, 2), provider);
            num3 = int.Parse(dmtfTimespan.Substring(10, 2), provider);
            num4 = int.Parse(dmtfTimespan.Substring(12, 2), provider);
            num5 = long.Parse(dmtfTimespan.Substring(15, 6), (IFormatProvider)CultureInfo.InvariantCulture.GetFormat(typeof(long))) * 10;
        }
        catch
        {
            throw new ArgumentOutOfRangeException("dmtfTimespan");
        }
        if (num < 0 || num2 < 0 || num3 < 0 || num4 < 0 || num5 < 0)
        {
            throw new ArgumentOutOfRangeException("dmtfTimespan");
        }
        TimeSpan timeSpan = new TimeSpan(num, num2, num3, num4, 0);
        TimeSpan timeSpan2 = TimeSpan.FromTicks(num5);
        return timeSpan + timeSpan2;
    }

    public static string ToDmtfTimeInterval(TimeSpan timespan)
    {
        string text = timespan.Days.ToString((IFormatProvider)CultureInfo.InvariantCulture.GetFormat(typeof(int))).PadLeft(8, '0');
        IFormatProvider formatProvider = (IFormatProvider)CultureInfo.InvariantCulture.GetFormat(typeof(int));
        if ((long)timespan.Days > 99999999L || timespan < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException("timespan");
        }
        string text2 = string.Concat(string.Concat(string.Concat(text + timespan.Hours.ToString(formatProvider).PadLeft(2, '0'), timespan.Minutes.ToString(formatProvider).PadLeft(2, '0')), timespan.Seconds.ToString(formatProvider).PadLeft(2, '0')), ".");
        TimeSpan timeSpan = new TimeSpan(timespan.Days, timespan.Hours, timespan.Minutes, timespan.Seconds, 0);
        string text3 = ((timespan.Ticks - timeSpan.Ticks) * 1000 / 10000).ToString((IFormatProvider)CultureInfo.InvariantCulture.GetFormat(typeof(long)));
        if (text3.Length > 6)
        {
            text3 = text3.Substring(0, 6);
        }
        return string.Concat(text2 + text3.PadLeft(6, '0'), ":000");
    }
}
