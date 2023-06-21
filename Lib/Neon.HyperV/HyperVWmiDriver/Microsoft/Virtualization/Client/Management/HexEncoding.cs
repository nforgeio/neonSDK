using System;
using System.Globalization;
using System.Text;

namespace Microsoft.Virtualization.Client.Management;

internal static class HexEncoding
{
    internal static string ToString(byte[] bytes)
    {
        if (bytes == null)
        {
            throw new ArgumentNullException("bytes");
        }
        if (bytes.Length == 0)
        {
            return string.Empty;
        }
        int num = bytes.Length;
        StringBuilder stringBuilder = new StringBuilder(num * 3 - 1);
        stringBuilder.Append(bytes[0].ToString("X2", CultureInfo.InvariantCulture));
        for (int i = 1; i < num; i++)
        {
            string value = bytes[i].ToString("X2", CultureInfo.InvariantCulture);
            stringBuilder.Append(" ");
            stringBuilder.Append(value);
        }
        return stringBuilder.ToString();
    }
}
