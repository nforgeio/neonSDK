using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using System.Text.RegularExpressions;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell.Commands;

internal static class VhdPathResolver
{
    private static Regex VhdExtensionsRegex = new Regex(".+\\.((vhds$)|(a?vhdx?$)|(vhdpmem$))", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static string[] VhdExtensionsList = new string[6] { "VHD", "AVHD", "VHDX", "AVHDX", "VHDS", "VHDPMEM" };

    internal static List<string> ResolveVirtualHardDiskFullPath(Server server, string path, ProviderIntrinsics provider)
    {
        List<string> list = new List<string>();
        if (!WildcardPattern.ContainsWildcardCharacters(path) && !IsVhdFilePath(path))
        {
            throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.VHD_InvalidVhdFilePath, path));
        }
        if (!string.IsNullOrEmpty(path))
        {
            if (path.StartsWith("\\\\", StringComparison.OrdinalIgnoreCase))
            {
                {
                    foreach (PSObject item in provider.ChildItem.Get(path, recurse: false))
                    {
                        if (item.TypeNames[0] == "System.IO.FileInfo")
                        {
                            FileInfo fileInfo = (FileInfo)LanguagePrimitives.ConvertTo(item, typeof(FileInfo), CultureInfo.CurrentCulture);
                            if (IsVhdFilePath(fileInfo.FullName))
                            {
                                list.Add(fileInfo.FullName);
                            }
                        }
                    }
                    return list;
                }
            }
            string[] extensions = null;
            if (string.IsNullOrEmpty(Path.GetExtension(path)))
            {
                extensions = VhdExtensionsList;
            }
            list.AddRange(Utilities.GetDataFilePaths(server, path, extensions));
        }
        return list;
    }

    internal static List<string> GetVirtualHardDiskFullPath(Server server, string path, string currentFileSystemLocation, ProviderIntrinsics provider)
    {
        string fullPath = PathUtility.GetFullPath(path, currentFileSystemLocation);
        List<string> list = ResolveVirtualHardDiskFullPath(server, fullPath, provider);
        if (list.Count == 0)
        {
            throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.VHD_InvalidVhdFilePath, path));
        }
        return list;
    }

    internal static List<string> GetVHDOrDirectoryFullPath(Server server, string path, string currentFileSystemLocation, ProviderIntrinsics provider)
    {
        string fullPath = PathUtility.GetFullPath(path, currentFileSystemLocation);
        if (!WildcardPattern.ContainsWildcardCharacters(fullPath) && string.IsNullOrEmpty(Path.GetExtension(fullPath)))
        {
            return new List<string> { fullPath };
        }
        return ResolveVirtualHardDiskFullPath(server, fullPath, provider);
    }

    internal static string GetSingleVirtualHardDiskFullPath(Server server, string path, string currentFileSystemLocation, ProviderIntrinsics provider)
    {
        List<string> virtualHardDiskFullPath = GetVirtualHardDiskFullPath(server, path, currentFileSystemLocation, provider);
        if (virtualHardDiskFullPath.Count == 1)
        {
            return virtualHardDiskFullPath[0];
        }
        throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.VHD_InvalidVhdFilePath, path));
    }

    internal static bool IsVhdFilePath(string input)
    {
        return VhdExtensionsRegex.IsMatch(input);
    }
}
