#define TRACE
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.Virtualization.Client.Management;

internal class Win32DirectoryView : View, IWin32Directory, IVirtualizationManagementObject, IDeleteable
{
    public string Name => GetProperty<string>("Name");

    public bool IsSystem => GetProperty<bool>("System");

    public bool IsHidden => GetProperty<bool>("Hidden");

    public IEnumerable<IDataFile> GetFiles()
    {
        return GetRelatedObjects<IDataFile>(base.Associations.Win32DirectoryContainsFile);
    }

    public IEnumerable<IWin32Directory> GetSubdirectories()
    {
        List<IWin32Directory> list = new List<IWin32Directory>(GetRelatedObjects<IWin32Directory>(base.Associations.Win32SubDirectories));
        int num = -1;
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].Name.Length < Name.Length)
            {
                num = i;
                break;
            }
        }
        if (num != -1)
        {
            list.RemoveAt(num);
        }
        return list;
    }

    public void Delete()
    {
        VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Deleting directory {0}", Name));
        uint num = InvokeMethod("Delete");
        if (num != View.ErrorCodeSuccess)
        {
            throw ThrowHelper.CreateVirtualizationOperationFailedException(string.Format(CultureInfo.CurrentCulture, ErrorMessages.DeleteDirectoryFailed, Name), VirtualizationOperation.DeleteDataFile, num, GetErrorCodeMapper(), null);
        }
        VMTrace.TraceUserActionCompleted("Deleting data file completed successfully.");
        base.ProxyFactory.Repository.UnregisterProxy(base.Proxy);
    }
}
