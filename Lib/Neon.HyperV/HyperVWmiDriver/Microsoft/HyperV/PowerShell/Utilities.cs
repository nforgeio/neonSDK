using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text.RegularExpressions;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal static class Utilities
{
    private const string gm_VirtualFloppyDiskFileExtension = "vfd";

    private const string gm_IsoFileExtension = "iso";

    private const string gm_DiskVolumePatternString = "[a-zA-Z]:$";

    private const string gm_GetVmAssignableDeviceCmdletName = "Get-VmAssignableDevice";

    private static readonly Regex VolumeRegex = new Regex("[a-zA-Z]:$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static readonly object gm_CmdletLock = new object();

    internal static IEnumerable<IDataFile> GetDataFiles(Server server, string path, string[] extensions)
    {
        if (string.IsNullOrEmpty(path))
        {
            throw new ArgumentNullException("path");
        }
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
        string directoryName = Path.GetDirectoryName(path);
        if (extensions == null)
        {
            extensions = new string[1] { Path.GetExtension(path) };
            if (string.IsNullOrEmpty(extensions[0]))
            {
                extensions = null;
            }
            else
            {
                extensions[0] = extensions[0].TrimStart('.');
            }
        }
        return ObjectLocator.GetDataFiles(server, directoryName, fileNameWithoutExtension, extensions);
    }

    internal static IEnumerable<string> GetDataFilePaths(Server server, string path, string[] extensions)
    {
        return from datafile in GetDataFiles(server, path, extensions)
            select datafile.Path;
    }

    internal static void DeleteDataFiles(Server server, string path)
    {
        foreach (IDataFile dataFile in GetDataFiles(server, path, null))
        {
            dataFile.Delete();
        }
    }

    internal static bool DirectoryExists(Server server, string path)
    {
        if (server.IsLocalhost)
        {
            return Directory.Exists(path);
        }
        IWin32Directory win32Directory;
        return ObjectLocator.TryGetWin32Directory(server, path, out win32Directory);
    }

    internal static bool FileExists(Server server, string path)
    {
        if (server.IsLocalhost)
        {
            return File.Exists(path);
        }
        IDataFile dataFile;
        return ObjectLocator.TryGetDataFile(server, path, out dataFile);
    }

    internal static bool IsIsoFilePath(string path)
    {
        return HasExtension(path, "iso");
    }

    internal static bool IsDriveVolumeString(string path)
    {
        return VolumeRegex.IsMatch(NormalizeFolderPath(path));
    }

    internal static bool IsVfdFilePath(string path)
    {
        return HasExtension(path, "vfd");
    }

    internal static string NormalizeFolderPath(string folderPath)
    {
        if (string.IsNullOrEmpty(folderPath))
        {
            throw new ArgumentException(null, "folderPath");
        }
        string path = ((folderPath[folderPath.Length - 1] == Path.DirectorySeparatorChar) ? folderPath : (folderPath + Path.DirectorySeparatorChar));
        path = Path.GetDirectoryName(path);
        if (string.IsNullOrEmpty(path))
        {
            return folderPath;
        }
        return path;
    }

    private static bool HasExtension(string filePath, string extension)
    {
        if (string.IsNullOrEmpty(extension))
        {
            throw new ArgumentNullException("extension");
        }
        if (string.IsNullOrEmpty(filePath))
        {
            return false;
        }
        string empty = string.Empty;
        try
        {
            empty = Path.GetExtension(filePath.Trim());
        }
        catch (ArgumentException)
        {
            return false;
        }
        if (empty != string.Empty)
        {
            empty = empty.Substring(1);
        }
        return string.Equals(empty, extension, StringComparison.OrdinalIgnoreCase);
    }

    internal static CommandInfo GetCmdletInfo(string cmdletName, CommandInvocationIntrinsics commandInvocationIntrinsics)
    {
        lock (gm_CmdletLock)
        {
            return commandInvocationIntrinsics.GetCommand(cmdletName, CommandTypes.Cmdlet);
        }
    }
}
