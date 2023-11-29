using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text.RegularExpressions;

namespace Microsoft.HyperV.PowerShell.Commands;

internal static class PathUtility
{
    internal static readonly string DirectorySeparator = Path.DirectorySeparatorChar.ToString();

    internal const int MaxPath = 260;

    private static readonly Regex gm_PathValidator = new Regex(string.Format(CultureInfo.InvariantCulture, "([a-z]:(\\\\[{0}]*)?)|(\\\\\\\\[^\\]+?\\\\[{0}]+)|(\\\\\\\\\\?\\\\Volume\\{{[0-9a-f]{{8}}-[0-9a-f]{{4}}-[0-9a-f]{{4}}-[0-9a-f]{{4}}-[0-9a-f]{{12}}\\}}(\\\\[{0}]*)?)", new string(Path.GetInvalidFileNameChars().Concat(new char[2] { '/', '*' }).ToArray())), RegexOptions.IgnoreCase | RegexOptions.Compiled);

    internal static bool IsValidPath(string path)
    {
        return gm_PathValidator.IsMatch(path);
    }

    internal static bool IsValidFilePath(string filePath)
    {
        if (IsValidPath(filePath))
        {
            return !filePath.EndsWith(DirectorySeparator, StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }

    internal static string GetFullPath(string path, string currentFileSystemLocation)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return currentFileSystemLocation;
        }
        string text = path.Trim();
        if (!Path.IsPathRooted(text))
        {
            text = Path.Combine(currentFileSystemLocation, text);
        }
        else if (text[0] == '\\' && (text.Length == 1 || text[1] != '\\'))
        {
            string text2 = Directory.GetDirectoryRoot(currentFileSystemLocation);
            if (!text2.EndsWith("\\", StringComparison.OrdinalIgnoreCase))
            {
                text2 += "\\";
            }
            text = Path.Combine(text2, text.Remove(0, 1));
        }
        if (!WildcardPattern.ContainsWildcardCharacters(text))
        {
            text = Path.GetFullPath(text);
        }
        else
        {
            int startIndex = text.IndexOfAny(new char[3] { '*', '[', '?' });
            int num = text.LastIndexOf('\\', startIndex);
            text = Path.GetFullPath(text.Substring(0, num)) + text.Substring(num);
        }
        if (text.EndsWith("\\", StringComparison.OrdinalIgnoreCase) && text.IndexOf('\\') != text.Length - 1)
        {
            text = text.TrimEnd('\\');
        }
        return text;
    }
}
