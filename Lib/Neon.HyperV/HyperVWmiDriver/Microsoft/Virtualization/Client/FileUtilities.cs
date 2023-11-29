#define TRACE
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Virtualization.Client.Management;
using Microsoft.Virtualization.Client.Management.Clustering;
using Microsoft.Virtualization.Client.Management.Utilities;

namespace Microsoft.Virtualization.Client;

internal static class FileUtilities
{
    private const string gm_FileShareRegularExp = "^\\\\\\\\(?<server>[^\\\\?]+)\\\\+(?<share>[^\\\\]+)(?<dir>(\\\\+[^\\\\]+)+)?\\\\*$";

    public static string AppendSeparatorIntoPath(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            throw new ArgumentException(null, "path");
        }
        if (path[path.Length - 1] != Path.DirectorySeparatorChar)
        {
            return path + Path.DirectorySeparatorChar;
        }
        return path;
    }

    public static string NormalizeFolderPath(string folderPath)
    {
        string path = AppendSeparatorIntoPath(folderPath);
        path = Path.GetDirectoryName(path);
        if (string.IsNullOrEmpty(path))
        {
            return folderPath;
        }
        return path;
    }

    public static bool FolderExists(Server server, string folderPath)
    {
        if (string.IsNullOrEmpty(folderPath))
        {
            throw new ArgumentException(null, "folderPath");
        }
        folderPath = NormalizeFolderPath(folderPath);
        if (server.IsLocalhost)
        {
            return Directory.Exists(folderPath);
        }
        try
        {
            ObjectLocator.GetWin32Directory(server, folderPath).UpdateAssociationCache();
            return true;
        }
        catch (ObjectNotFoundException)
        {
        }
        return false;
    }

    public static bool FileExists(Server server, string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            throw new ArgumentException(null, "filePath");
        }
        if (server.IsLocalhost)
        {
            return File.Exists(filePath);
        }
        try
        {
            ObjectLocator.GetDataFile(server, filePath).UpdateAssociationCache();
            return true;
        }
        catch (ObjectNotFoundException)
        {
        }
        return false;
    }

    [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", Justification = "In Folder should not be treated as a compound word.")]
    public static IEnumerable<string> EnumerateFilesInFolder(Server server, string folderPath, string[] extensions)
    {
        if (!FolderExists(server, folderPath))
        {
            throw CreateFolderNotFoundException(folderPath, null);
        }
        if (server.IsLocalhost)
        {
            return extensions.SelectMany((string extension) => Directory.EnumerateFiles(folderPath, string.Format(CultureInfo.InvariantCulture, "*.{0}", extension)));
        }
        return from dataFile in ObjectLocator.GetDataFiles(server, folderPath, extensions)
            select dataFile.Path;
    }

    public static void CopyFile(Server server, string sourceFile, string destinationFile, bool overwrite)
    {
        if (string.IsNullOrEmpty(sourceFile))
        {
            throw new ArgumentException(null, "sourceFile");
        }
        if (string.IsNullOrEmpty(destinationFile))
        {
            throw new ArgumentException(null, "destinationFile");
        }
        if (overwrite)
        {
            DeleteFile(server, destinationFile);
        }
        else if (FileExists(server, destinationFile))
        {
            throw new VirtualizationManagementException(string.Format(CultureInfo.CurrentCulture, FileUtilitiesResources.FileExistsError, destinationFile));
        }
        if (server.IsLocalhost)
        {
            File.Copy(sourceFile, destinationFile);
        }
        else
        {
            ObjectLocator.GetDataFile(server, sourceFile).Copy(destinationFile);
        }
    }

    public static void DeleteFile(Server server, string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            throw new ArgumentException(null, "filePath");
        }
        if (server.IsLocalhost)
        {
            File.Delete(filePath);
            return;
        }
        try
        {
            ObjectLocator.GetDataFile(server, filePath).Delete();
        }
        catch (ObjectNotFoundException)
        {
        }
    }

    public static ObjectNotFoundException CreateFolderNotFoundException(string path, Exception inner)
    {
        ObjectNotFoundException result = new ObjectNotFoundException(string.Format(CultureInfo.CurrentCulture, FileUtilitiesResources.FolderNotFoundError, path), inner);
        VMTrace.TraceError(string.Format(CultureInfo.InvariantCulture, "Unable to find file or folder with path '{0}'.", path));
        return result;
    }

    public static bool IsValidClusterStorage(string storageLocation, Server server)
    {
        bool flag = true;
        flag = IsCsvStorageType(storageLocation, server);
        if (!flag)
        {
            flag = IsFileShareStorageType(storageLocation);
        }
        return flag;
    }

    private static bool IsCsvStorageType(string storageLocation, Server server)
    {
        try
        {
            IMSClusterCluster updatedClusterCluster = ClusterUtilities.GetUpdatedClusterCluster(server);
            if (updatedClusterCluster == null)
            {
                return false;
            }
            if (!updatedClusterCluster.EnableSharedVolumes)
            {
                return false;
            }
            string text = string.Format(CultureInfo.CurrentCulture, "{0}{1}", updatedClusterCluster.SharedVolumesRoot, "\\");
            if (storageLocation.StartsWith(text, StringComparison.OrdinalIgnoreCase) && storageLocation.Length > text.Length)
            {
                return true;
            }
            return false;
        }
        catch (VirtualizationManagementException ex)
        {
            VMTrace.TraceError("Failed to get the cluster wmi object", ex);
            return false;
        }
    }

    private static bool IsFileShareStorageType(string storageLocation)
    {
        if (new Regex("^\\\\\\\\(?<server>[^\\\\?]+)\\\\+(?<share>[^\\\\]+)(?<dir>(\\\\+[^\\\\]+)+)?\\\\*$").IsMatch(storageLocation))
        {
            return true;
        }
        return false;
    }
}
