#define TRACE
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Management.Infrastructure;
using Microsoft.Virtualization.Client.Management.Clustering;

namespace Microsoft.Virtualization.Client.Management;

internal static class ObjectLocator
{
    private static IEnumerable<string> CreateFiltersForWqlQuery(string propertyName, IEnumerable<string> propertyValues)
    {
        return propertyValues.Select((string propertyValue) => CreateFilterForWqlQuery(propertyName, propertyValue));
    }

    private static IEnumerable<string> CreateFiltersForWqlQuery(string propertyName, IEnumerable<string> propertyValues, bool allowWildcards)
    {
        return propertyValues.Select((string propertyValue) => CreateFilterForWqlQuery(propertyName, propertyValue, allowWildcards));
    }

    private static string CreateFilterForWqlQuery(string propertyName, string propertyValue)
    {
        return CreateFilterForWqlQuery(propertyName, propertyValue, WildcardPattern.ContainsWildcardCharacters(propertyValue));
    }

    private static string CreateFilterForWqlQuery(string propertyName, string propertyValue, bool allowWildcards)
    {
        string format = (allowWildcards ? "{0} LIKE '{1}'" : "{0} = '{1}'");
        return string.Format(CultureInfo.InvariantCulture, format, propertyName, allowWildcards ? new WildcardPattern(propertyValue).ToWql() : propertyValue);
    }

    internal static IList<string> JoinWithMaxLength(string separator, IEnumerable<string> values, int maxLength)
    {
        List<string> list = new List<string>();
        StringBuilder stringBuilder = new StringBuilder(maxLength, maxLength);
        foreach (string value in values)
        {
            int num = value.Length;
            if (stringBuilder.Length > 0)
            {
                num += separator.Length;
            }
            if (stringBuilder.Length + num > maxLength)
            {
                list.Add(stringBuilder.ToString());
                stringBuilder.Clear();
            }
            if (stringBuilder.Length == 0)
            {
                stringBuilder.Append(value);
                continue;
            }
            stringBuilder.Append(separator);
            stringBuilder.Append(value);
        }
        if (stringBuilder.Length > 0)
        {
            list.Add(stringBuilder.ToString());
        }
        return list;
    }

    public static IList<TVirtualizationManagementObject> QueryObjectsByNames<TVirtualizationManagementObject>(Server server, IEnumerable<string> names, bool allowWildcards) where TVirtualizationManagementObject : IVirtualizationManagementObject
    {
        return QueryObjectsByProperty<TVirtualizationManagementObject>(server, "ElementName", names, allowWildcards);
    }

    public static IList<TVirtualizationManagementObject> QueryObjectsByProperty<TVirtualizationManagementObject>(Server server, string propertyName, IEnumerable<string> propertyValues, bool allowWildcards) where TVirtualizationManagementObject : IVirtualizationManagementObject
    {
        return QueryObjectsByProperty<TVirtualizationManagementObject>(server, propertyName, propertyValues, allowWildcards, null);
    }

    internal static IList<TVirtualizationManagementObject> QueryObjectsByProperty<TVirtualizationManagementObject>(Server server, string propertyName, IEnumerable<string> propertyValues, bool allowWildcards, WmiOperationOptions options) where TVirtualizationManagementObject : IVirtualizationManagementObject
    {
        string wmiClassName = WmiNameMapper.MapTypeToWmiClassName(typeof(TVirtualizationManagementObject));
        ICollection<string> collection = ((propertyValues == null) ? new List<string>() : propertyValues.ToList());
        if (collection.Count == 0)
        {
            return ObjectFactory.Instance.GetVirtualizationManagementObjects<TVirtualizationManagementObject>(server, server.VirtualizationNamespace, wmiClassName, options);
        }
        IEnumerable<string> values = CreateFiltersForWqlQuery(propertyName, collection, allowWildcards);
        IEnumerable<string> source;
        if (allowWildcards)
        {
            string text = string.Format(CultureInfo.InvariantCulture, "SELECT * FROM {0} WHERE ({1})", wmiClassName, string.Join(" OR ", values));
            source = new string[1] { text };
        }
        else
        {
            int length = string.Format(CultureInfo.InvariantCulture, "SELECT * FROM {0} WHERE ({1})", wmiClassName, string.Empty).Length;
            int maxLength = 16384 - length;
            source = from queryClauses in JoinWithMaxLength(" OR ", values, maxLength)
                select string.Format(CultureInfo.InvariantCulture, "SELECT * FROM {0} WHERE ({1})", wmiClassName, queryClauses);
        }
        return source.SelectMany((string query) => ObjectFactory.Instance.QueryVirtualizationManagementObjects<TVirtualizationManagementObject>(server, server.VirtualizationNamespace, query, options)).Distinct(new VirtualizationManagementObjectEqualityComparer<TVirtualizationManagementObject>()).ToList();
    }

    public static Task<IHostComputerSystem> GetHostComputerSystemAsync(Server server)
    {
        return Task.Run(() => GetHostComputerSystem(server));
    }

    public static Task<IVMService> GetVirtualizationServiceAsync(Server server)
    {
        return Task.Run(() => GetVirtualizationService(server));
    }

    public static Task<IImageManagementService> GetImageManagementServiceAsync(Server server)
    {
        return Task.Run(() => GetImageManagementService(server));
    }

    public static Task<IVirtualSwitchManagementService> GetVirtualSwitchManagementServiceAsync(Server server)
    {
        return Task.Run(() => GetVirtualSwitchManagementService(server));
    }

    public static IService GetWin32VirtualizationService(Server server)
    {
        ObjectKey key = ObjectKeyCreator.CreateWin32VirtualizationServiceObjectKey(server);
        return ObjectFactory.Instance.GetVirtualizationManagementObject<IService>(key);
    }

    public static IService GetWin32RDVirtualizationHostService(Server server)
    {
        ObjectKey key = ObjectKeyCreator.CreateWin32RDVirtualizationHostObjectKey(server);
        return ObjectFactory.Instance.GetVirtualizationManagementObject<IService>(key);
    }

    public static bool TryGetDataFile(Server server, string path, out IDataFile dataFile)
    {
        ObjectKey key = ObjectKeyCreator.CreateCimDataFileObjectKey(server, path);
        return ObjectFactory.Instance.TryGetVirtualizationManagementObject<IDataFile>(key, out dataFile);
    }

    public static IDataFile GetDataFile(Server server, string path)
    {
        ObjectKey key = ObjectKeyCreator.CreateCimDataFileObjectKey(server, path);
        return ObjectFactory.Instance.GetVirtualizationManagementObject<IDataFile>(key);
    }

    private static IList<IDataFile> GetDataFiles(Server server, string drive, string path, string fileName, string[] extensions)
    {
        if (drive == null)
        {
            throw new ArgumentNullException("drive");
        }
        if (path == null)
        {
            throw new ArgumentNullException("path");
        }
        string text = Path.DirectorySeparatorChar.ToString();
        if (drive.EndsWith(text, StringComparison.OrdinalIgnoreCase))
        {
            drive = drive.TrimEnd(Path.DirectorySeparatorChar);
        }
        if (!path.StartsWith(text, StringComparison.OrdinalIgnoreCase))
        {
            path = text + path;
        }
        if (!path.EndsWith(text, StringComparison.OrdinalIgnoreCase))
        {
            path += text;
        }
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "SELECT * FROM {0} WHERE {1} AND {2}", "CIM_DataFile", CreateFilterForWqlQuery("Drive", drive, allowWildcards: false), CreateFilterForWqlQuery("Path", ManagementPathHelper.EscapePropertyValue(path, ManagementPathHelper.QuoteType.Single)));
        if (fileName != null && !string.Equals(fileName, "*", StringComparison.OrdinalIgnoreCase))
        {
            stringBuilder.AppendFormat(CultureInfo.InvariantCulture, " AND {0}", CreateFilterForWqlQuery("FileName", fileName));
        }
        if (extensions != null && extensions.Length != 0)
        {
            stringBuilder.AppendFormat(CultureInfo.InvariantCulture, " AND ({0})", string.Join(" OR ", CreateFiltersForWqlQuery("Extension", extensions)));
        }
        return ObjectFactory.Instance.QueryVirtualizationManagementObjects<IDataFile>(server, Server.CimV2Namespace, stringBuilder.ToString());
    }

    public static IList<IDataFile> GetDataFiles(Server server, string directory, string fileName, string[] extensions)
    {
        if (directory == null)
        {
            throw new ArgumentNullException("directory");
        }
        if (directory.StartsWith("\\\\", StringComparison.OrdinalIgnoreCase) || directory.Length < 2 || directory[1] != ':')
        {
            throw new ArgumentException(ErrorMessages.DataFileError_InvalidPath, "directory");
        }
        string[] array = directory.Split(new char[1] { Path.DirectorySeparatorChar }, 2);
        string text = null;
        string path = null;
        if (array.Length == 0)
        {
            throw new ArgumentException(ErrorMessages.DataFileError_InvalidPath, "directory");
        }
        if (array.Length == 1)
        {
            path = "\\";
        }
        else if (array.Length == 2)
        {
            path = array[1];
        }
        text = array[0];
        return GetDataFiles(server, text, path, fileName, extensions);
    }

    public static IList<IDataFile> GetDataFiles(Server server, string directory, string[] extensions)
    {
        return GetDataFiles(server, directory, null, extensions);
    }

    public static IList<IDataFile> GetDataFiles(Server server, string directory)
    {
        return GetDataFiles(server, directory, null, null);
    }

    public static bool TryGetWin32Directory(Server server, string directory, out IWin32Directory win32Directory)
    {
        ObjectKey key = ObjectKeyCreator.CreateWin32DirectoryObjectKey(server, directory);
        return ObjectFactory.Instance.TryGetVirtualizationManagementObject<IWin32Directory>(key, out win32Directory);
    }

    public static IWin32Directory GetWin32Directory(Server server, string directory)
    {
        ObjectKey key = ObjectKeyCreator.CreateWin32DirectoryObjectKey(server, directory);
        return ObjectFactory.Instance.GetVirtualizationManagementObject<IWin32Directory>(key);
    }

    public static IVMService GetVirtualizationService(Server server)
    {
        ObjectKey key = ObjectKeyCreator.CreateVMServiceObjectKey(server);
        return ObjectFactory.Instance.GetVirtualizationManagementObject<IVMService>(key);
    }

    public static IReplicationService GetFailoverReplicationService(Server server)
    {
        ObjectKey key = ObjectKeyCreator.CreateFailoverReplicationServiceObjectKey(server);
        return ObjectFactory.Instance.GetVirtualizationManagementObject<IReplicationService>(key);
    }

    public static IHostComputerSystem GetHostComputerSystem(Server server)
    {
        ObjectKey key = ObjectKeyCreator.CreateHostComputerSystemObjectKey(server);
        return ObjectFactory.Instance.GetVirtualizationManagementObject<IHostComputerSystem>(key);
    }

    public static IHostClusterComputerSystem GetHostClusterComputerSystem(Server server)
    {
        ObjectKey key = ObjectKeyCreator.CreateHostClusterComputerSystemObjectKey(server);
        return ObjectFactory.Instance.GetVirtualizationManagementObject<IHostClusterComputerSystem>(key);
    }

    public static IHostComputerSystemBase GetHostOrClusterComputerSystem(Server server)
    {
        if (server.IsHostCluster)
        {
            return GetHostClusterComputerSystem(server);
        }
        return GetHostComputerSystem(server);
    }

    public static IWin32ComputerSystem GetWin32ComputerSystem(Server server)
    {
        ObjectKey key = ObjectKeyCreator.CreateWin32ComputerSystemObjectKey(server);
        return ObjectFactory.Instance.GetVirtualizationManagementObject<IWin32ComputerSystem>(key);
    }

    public static IImageManagementService GetImageManagementService(Server server)
    {
        ObjectKey key = ObjectKeyCreator.CreateImageManagementServiceObjectKey(server);
        return ObjectFactory.Instance.GetVirtualizationManagementObject<IImageManagementService>(key);
    }

    public static IVirtualSwitchManagementService GetVirtualSwitchManagementService(Server server)
    {
        ObjectKey key = ObjectKeyCreator.CreateSwitchManagementServiceObjectKey(server);
        return ObjectFactory.Instance.GetVirtualizationManagementObject<IVirtualSwitchManagementService>(key);
    }

    public static IAssignableDeviceService GetAssignableDeviceService(Server server)
    {
        ObjectKey key = ObjectKeyCreator.CreateAssignableDeviceService(server);
        return ObjectFactory.Instance.GetVirtualizationManagementObject<IAssignableDeviceService>(key);
    }

    public static IResourcePoolConfigurationService GetResourcePoolConfigurationService(Server server)
    {
        ObjectKey key = ObjectKeyCreator.CreateResourcePoolConfigServiceObjectKey(server);
        return ObjectFactory.Instance.GetVirtualizationManagementObject<IResourcePoolConfigurationService>(key);
    }

    public static IList<IResourcePool> GetResourcePoolsByNamesAndTypes(Server server, IEnumerable<string> names, bool allowWildcards, IEnumerable<VMDeviceSettingType> types)
    {
        bool flag = true;
        StringBuilder stringBuilder = new StringBuilder();
        if (names != null)
        {
            bool flag2 = false;
            foreach (string name in names)
            {
                if (flag)
                {
                    stringBuilder.Append(" WHERE ");
                    flag = false;
                }
                if (!flag2)
                {
                    stringBuilder.Append("(");
                    flag2 = true;
                }
                else
                {
                    stringBuilder.Append(" OR ");
                }
                if (string.IsNullOrEmpty(name))
                {
                    stringBuilder.Append("Primordial = TRUE");
                }
                else
                {
                    stringBuilder.AppendFormat(CultureInfo.InvariantCulture, allowWildcards ? "PoolID LIKE '{0}'" : "PoolID = '{0}'", name);
                }
            }
            if (flag2)
            {
                stringBuilder.Append(")");
            }
        }
        HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (types != null)
        {
            bool flag3 = !flag;
            bool flag4 = false;
            foreach (VMDeviceSettingType type2 in types)
            {
                VMDeviceSettingTypeMapper.MapVMDeviceSettingTypeToResourceType(type2, out var resourceType, out var resourceSubType);
                if (flag)
                {
                    stringBuilder.Append(" WHERE ");
                    flag = false;
                    flag3 = false;
                }
                else if (flag3)
                {
                    stringBuilder.Append(" AND ");
                    flag3 = false;
                }
                if (!flag4)
                {
                    stringBuilder.Append("(");
                    flag4 = true;
                }
                else
                {
                    stringBuilder.Append(" OR ");
                }
                stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "(ResourceType = {0} AND ResourceSubType = '{1}')", resourceType, resourceSubType);
                Type type = VMDeviceSettingTypeMapper.MapResourcePoolTypeToObjectModelType(type2);
                hashSet.Add(WmiNameMapper.MapTypeToWmiClassName(type));
            }
            if (flag4)
            {
                stringBuilder.Append(")");
            }
        }
        string wqlQuery = string.Format(CultureInfo.InvariantCulture, "SELECT * FROM {0}{1}", (hashSet.Count == 1) ? hashSet.First() : "CIM_ResourcePool", stringBuilder.ToString());
        return ObjectFactory.Instance.QueryVirtualizationManagementObjects<IResourcePool>(server, server.VirtualizationNamespace, wqlQuery);
    }

    public static IList<IHostNumaNode> GetHostNumaNodes(Server server)
    {
        if (server == null)
        {
            throw new ArgumentNullException("server");
        }
        return ObjectFactory.Instance.GetVirtualizationManagementObjects<IHostNumaNode>(server, server.VirtualizationNamespace, "Msvm_NumaNode");
    }

    public static ITerminalService GetTerminalService(Server server)
    {
        ObjectKey key = ObjectKeyCreator.CreateTerminalServiceObjectKey(server);
        return ObjectFactory.Instance.GetVirtualizationManagementObject<ITerminalService>(key);
    }

    public static ITerminalServiceSetting GetTerminalServiceSetting(Server server)
    {
        ObjectKey key = ObjectKeyCreator.CreateTerminalServiceSettingObjectKey(server);
        return ObjectFactory.Instance.GetVirtualizationManagementObject<ITerminalServiceSetting>(key);
    }

    public static IVMMigrationService GetVMMigrationService(Server server)
    {
        ObjectKey key = ObjectKeyCreator.CreateMigrationServiceObjectKey(server);
        return ObjectFactory.Instance.GetVirtualizationManagementObject<IVMMigrationService>(key);
    }

    public static IVMSecurityService GetVMSecurityService(Server server)
    {
        ObjectKey key = ObjectKeyCreator.CreateSecurityServiceObjectKey(server);
        return ObjectFactory.Instance.GetVirtualizationManagementObject<IVMSecurityService>(key);
    }

    public static IMetricService GetMetricService(Server server)
    {
        ObjectKey key = ObjectKeyCreator.CreateMetricServiceObjectKey(server);
        return ObjectFactory.Instance.GetVirtualizationManagementObject<IMetricService>(key);
    }

    public static ICollectionManagementService GetCollectionManagementService(Server server)
    {
        ObjectKey key = ObjectKeyCreator.CreateCollectionManagementServiceObjectKey(server);
        return ObjectFactory.Instance.GetVirtualizationManagementObject<ICollectionManagementService>(key);
    }

    public static IVMComputerSystemBase GetVMComputerSystemBase(Server server, string instanceId)
    {
        if (!TryGetVMComputerSystemBase(server, instanceId, out var computerSystemBase))
        {
            WmiObjectPath path = new WmiObjectPath(server, server.VirtualizationNamespace, "CIM_ComputerSystem", new KeyValueDictionary { { "Name", instanceId } });
            throw ThrowHelper.CreateRemoteObjectNotFoundException(server, path, null);
        }
        return computerSystemBase;
    }

    public static bool TryGetVMComputerSystemBase(Server server, string instanceId, out IVMComputerSystemBase computerSystemBase)
    {
        ObjectKey key = ObjectKeyCreator.CreateVMComputerSystemObjectKey(server, instanceId);
        if (ProxyFactory.Instance.Repository.TryGetProxy(key, out var proxy))
        {
            computerSystemBase = ViewFactory.Instance.CreateView<IVMComputerSystem>(proxy, key);
            return true;
        }
        key = ObjectKeyCreator.CreateVMPlannedComputerSystemObjectKey(server, instanceId);
        if (ProxyFactory.Instance.Repository.TryGetProxy(key, out proxy))
        {
            computerSystemBase = ViewFactory.Instance.CreateView<IVMPlannedComputerSystem>(proxy, key);
            return true;
        }
        string wqlQuery = string.Format(CultureInfo.InvariantCulture, "SELECT * FROM CIM_ComputerSystem WHERE Name = '{0}'", instanceId);
        computerSystemBase = ObjectFactory.Instance.QueryVirtualizationManagementObjects<IVMComputerSystemBase>(server, server.VirtualizationNamespace, wqlQuery).FirstOrDefault();
        return computerSystemBase != null;
    }

    public static IVMComputerSystem GetVMComputerSystem(Server server, string instanceId)
    {
        return GetVMComputerSystem(server, instanceId, delayInitializePropertyCache: false);
    }

    public static IVMComputerSystem GetVMComputerSystem(Server server, string instanceId, bool delayInitializePropertyCache)
    {
        ObjectKey key = ObjectKeyCreator.CreateVMComputerSystemObjectKey(server, instanceId);
        IProxy proxy = ProxyFactory.Instance.GetProxy(key, delayInitializePropertyCache);
        return ViewFactory.Instance.CreateView<IVMComputerSystem>(proxy, key);
    }

    public static IList<IVMComputerSystem> GetVMComputerSystems(Server server)
    {
        if (server == null)
        {
            throw new ArgumentNullException("server");
        }
        return ObjectFactory.Instance.QueryVirtualizationManagementObjects<IVMComputerSystem>(server, server.VirtualizationNamespace, string.Format(CultureInfo.InvariantCulture, "SELECT * FROM Msvm_ComputerSystem WHERE Name <> '{0}'", server.Name));
    }

    public static IList<IVMComputerSystem> GetVMComputerSystems(Server server, ICollection<Guid> ids)
    {
        if (server == null)
        {
            throw new ArgumentNullException("server");
        }
        if (ids == null || ids.Count == 0)
        {
            throw new ArgumentNullException("ids");
        }
        return QueryObjectsByProperty<IVMComputerSystem>(server, "Name", ids.Select((Guid id) => id.ToString()), allowWildcards: false);
    }

    public static IVMPlannedComputerSystem GetVMPlannedComputerSystem(Server server, string instanceId)
    {
        ObjectKey key = ObjectKeyCreator.CreateVMPlannedComputerSystemObjectKey(server, instanceId);
        return ObjectFactory.Instance.GetVirtualizationManagementObject<IVMPlannedComputerSystem>(key);
    }

    public static bool TryGetVMPlannedComputerSystem(Server server, string instanceId, out IVMPlannedComputerSystem plannedVM)
    {
        ObjectKey key = ObjectKeyCreator.CreateVMPlannedComputerSystemObjectKey(server, instanceId);
        return ObjectFactory.Instance.TryGetVirtualizationManagementObject<IVMPlannedComputerSystem>(key, out plannedVM);
    }

    internal static IVMComputerSystem GetVMComputerSystem(Server server, ICimInstance cimInstance)
    {
        string instanceId = (string)cimInstance.CimInstanceProperties["Name"].Value;
        ObjectKey key = ObjectKeyCreator.CreateVMComputerSystemObjectKey(server, instanceId);
        return ObjectFactory.Instance.GetVirtualizationManagementObject<IVMComputerSystem>(key, cimInstance);
    }

    public static ISummaryInformation GetVMSummaryInformation(Server server, string instanceId)
    {
        ObjectKey key = ObjectKeyCreator.CreateVMSummaryInformationObjectKey(server, instanceId);
        return ObjectFactory.Instance.GetVirtualizationManagementObject<ISummaryInformation>(key);
    }

    public static IVMComputerSystemSetting GetVMComputerSystemSetting(Server server, string instanceId)
    {
        ObjectKey key = ObjectKeyCreator.CreateVMComputerSystemSettingObjectKey(server, instanceId);
        return ObjectFactory.Instance.GetVirtualizationManagementObject<IVMComputerSystemSetting>(key);
    }

    internal static IVMComputerSystemSetting GetVMComputerSystemSetting(Server server, string instanceId, ICimInstance cimInstance)
    {
        ObjectKey key = ObjectKeyCreator.CreateVMComputerSystemSettingObjectKey(server, instanceId);
        return ObjectFactory.Instance.GetVirtualizationManagementObject<IVMComputerSystemSetting>(key, cimInstance);
    }

    public static IMSClusterWmiProviderResource GetClusterWmiProviderResource(Server server)
    {
        ObjectKey key = ObjectKeyCreator.CreateClusterResourceObjectKey(server, "Virtual Machine Cluster WMI");
        return ObjectFactory.Instance.GetVirtualizationManagementObject<IMSClusterWmiProviderResource>(key);
    }

    public static IMSClusterNode GetClusterNode(Server server, string nodeName)
    {
        ObjectKey key = ObjectKeyCreator.CreateClusterNodeObjectKey(server, nodeName);
        return ObjectFactory.Instance.GetVirtualizationManagementObject<IMSClusterNode>(key);
    }

    public static IMSClusterCluster GetClusterObject(Server server)
    {
        if (TryGetClusterObject(server, out var clusterObject))
        {
            return clusterObject;
        }
        throw new ObjectNotFoundException();
    }

    public static bool TryGetClusterObject(Server server, out IMSClusterCluster clusterObject)
    {
        if (server == null)
        {
            throw new ArgumentNullException("server");
        }
        ObjectKey key = ObjectKeyCreator.CreateClusterObjectKey(server, server.Name);
        try
        {
            return ObjectFactory.Instance.TryGetVirtualizationManagementObject<IMSClusterCluster>(key, out clusterObject);
        }
        catch (CimException ex)
        {
            if (ex.NativeErrorCode == NativeErrorCode.Failed)
            {
                clusterObject = null;
                return false;
            }
            throw;
        }
    }

    public static IMSClusterReplicaBrokerResource GetClusterReplicaBrokerResource(Server server, string replicaBrokerName)
    {
        ObjectKey key = ObjectKeyCreator.CreateClusterResourceObjectKey(server, replicaBrokerName);
        return ObjectFactory.Instance.GetVirtualizationManagementObject<IMSClusterReplicaBrokerResource>(key);
    }

    public static IMSClusterCapResource GetClusterCapResource(Server server, string capName)
    {
        ObjectKey key = ObjectKeyCreator.CreateClusterResourceObjectKey(server, capName);
        return ObjectFactory.Instance.GetVirtualizationManagementObject<IMSClusterCapResource>(key);
    }

    public static IMSClusterResource GetClusterResource(Server server, string resourceName)
    {
        ObjectKey key = ObjectKeyCreator.CreateClusterResourceObjectKey(server, resourceName);
        return ObjectFactory.Instance.GetVirtualizationManagementObject<IMSClusterResource>(key);
    }

    public static IMSClusterResourceGroup GetClusterResourceGroup(Server server, string groupName)
    {
        ObjectKey key = ObjectKeyCreator.CreateClusterResourceGroupKey(server, groupName);
        return ObjectFactory.Instance.GetVirtualizationManagementObject<IMSClusterResourceGroup>(key);
    }

    public static bool TryGetVirtualizationManagementObject<TType>(Server server, WmiObjectPath path, out TType virtManObj) where TType : IVirtualizationManagementObject
    {
        ObjectKey key = new ObjectKey(server, path);
        return ObjectFactory.Instance.TryGetVirtualizationManagementObject<TType>(key, out virtManObj);
    }

    public static IVirtualizationManagementObject GetVirtualizationManagementObject(Server server, WmiObjectPath path)
    {
        return GetVirtualizationManagementObject(server, path, null);
    }

    public static IVirtualizationManagementObject GetVirtualizationManagementObject(Server server, WmiObjectPath path, ICimInstance cimInstance)
    {
        return GetVirtualizationManagementObject(server, path, cimInstance, null);
    }

    internal static IVirtualizationManagementObject GetVirtualizationManagementObject(Server server, WmiObjectPath path, ICimInstance cimInstance, WmiOperationOptions options)
    {
        IVirtualizationManagementObject virtualizationManagementObject = null;
        try
        {
            ObjectKey key = new ObjectKey(server, path);
            return ObjectFactory.Instance.GetVirtualizationManagementObject<IVirtualizationManagementObject>(key, cimInstance, options);
        }
        catch (NoWmiMappingException inner)
        {
            throw ThrowHelper.CreateRemoteObjectNotFoundException(server, path, inner);
        }
    }

    public static IVMSynthetic3DService GetSynthetic3DService(Server server)
    {
        ObjectKey key = ObjectKeyCreator.CreateSynthetic3DServiceObjectKey(server);
        return ObjectFactory.Instance.GetVirtualizationManagementObject<IVMSynthetic3DService>(key);
    }

    public static IList<IVMSynthetic3DDisplayController> Get3DVideoDevices(Server server)
    {
        return ObjectFactory.Instance.GetVirtualizationManagementObjects<IVMSynthetic3DDisplayController>(server, server.VirtualizationNamespace, "Msvm_Synthetic3DDisplayController");
    }

    public static IList<IHyperVCollection> GetHyperVCollections(Server server)
    {
        return ObjectFactory.Instance.GetVirtualizationManagementObjects<IHyperVCollection>(server, server.VirtualizationNamespace, "CIM_CollectionOfMSEs");
    }

    public static IInstalledEthernetSwitchExtension GetInstalledEthernetSwitchExtension(Server server, string extensionId)
    {
        ObjectKey key = ObjectKeyCreator.CreateInstalledEthernetSwitchExtensionObjectKey(server, extensionId);
        return ObjectFactory.Instance.GetVirtualizationManagementObject<IInstalledEthernetSwitchExtension>(key);
    }

    public static IList<IVMTaskAssociationObject> GetAllVMTaskAssociationObjects(Server server)
    {
        if (server == null)
        {
            throw new ArgumentNullException("server");
        }
        List<IVMTaskAssociationObject> list = new List<IVMTaskAssociationObject>();
        try
        {
            foreach (ICimInstance item in server.EnumerateInstances(server.VirtualizationNamespace, "Msvm_AffectedJobElement"))
            {
                using (item)
                {
                    list.Add(new VMTaskAssociationObject(server, item));
                }
            }
            return list;
        }
        catch (Exception serverException)
        {
            throw ThrowHelper.CreateServerException(server, serverException);
        }
    }

    internal static IVMTask GetVMTask(Server server, ICimInstance cimInstance)
    {
        string className = cimInstance.CimSystemProperties.ClassName;
        string instanceId = (string)cimInstance.CimInstanceProperties["InstanceID"].Value;
        return GetVMTask(server, className, instanceId, cimInstance);
    }

    private static IVMTask GetVMTask(Server server, string jobClassName, string instanceId, ICimInstance cimInstance)
    {
        if (server == null)
        {
            throw new ArgumentNullException("server");
        }
        ObjectKey key = ObjectKeyCreator.CreateVMTaskObjectKey(server, jobClassName, instanceId);
        return ObjectFactory.Instance.GetVirtualizationManagementObject<IVMTask>(key, cimInstance);
    }

    public static IList<IVM3dVideoPhysical3dGPU> GetVm3DVideoPhysical3dGpus(Server server)
    {
        if (server == null)
        {
            throw new ArgumentNullException("server");
        }
        return ObjectFactory.Instance.GetVirtualizationManagementObjects<IVM3dVideoPhysical3dGPU>(server, server.VirtualizationNamespace, "Msvm_Physical3dGraphicsProcessor");
    }

    public static IList<IPartitionableGpu> GetPartitionableGpus(Server server)
    {
        if (server == null)
        {
            throw new ArgumentNullException("server");
        }
        return ObjectFactory.Instance.GetVirtualizationManagementObjects<IPartitionableGpu>(server, server.VirtualizationNamespace, "Msvm_PartitionableGpu");
    }

    internal static void RemoveVirtualMachineFromCache(IProxy virtualMachineProxy)
    {
        string b = (string)virtualMachineProxy.Path.KeyValues["Name"];
        ProxyFactory.Instance.Repository.UnregisterProxy(virtualMachineProxy);
        foreach (IProxy proxy in ProxyFactory.Instance.Repository.GetProxies(virtualMachineProxy.Key.Server, "Msvm_VirtualSystemSettingData"))
        {
            try
            {
                string text = (string)proxy.GetProperty("VirtualSystemIdentifier");
                if (text != null && string.Equals(text, b, StringComparison.OrdinalIgnoreCase))
                {
                    ProxyFactory.Instance.Repository.UnregisterProxy(proxy);
                }
            }
            catch (Exception ex)
            {
                VMTrace.TraceError("Could not find this SystemName property in the vssd object. This is unexpected as these objects should be fully initialized in the cache.", ex);
            }
        }
    }
}
