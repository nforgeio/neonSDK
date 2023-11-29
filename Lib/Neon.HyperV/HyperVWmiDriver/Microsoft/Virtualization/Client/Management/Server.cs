#define TRACE
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Generic;
using Microsoft.Management.Infrastructure.Options;
using Microsoft.Management.Infrastructure.Serialization;
using Microsoft.Virtualization.Client.Common;

namespace Microsoft.Virtualization.Client.Management;

internal sealed class Server : IServer
{
    private const int gm_ProductTypeWorkStation = 1;

    private const string gm_VirtualizationNamespacePathV2 = "root/virtualization/v2";

    private const string gm_ClusterVirtualizationNamespacePath = "root/HyperVCluster/v2";

    private const string gm_CimV2NamespacePath = "root/cimv2";

    private const string gm_MSClusterNamespacePath = "root/mscluster";

    private const string gm_StandardCimV2NamespacePath = "root/standardcimv2";

    private const string gm_InteropNamespacePath = "root/interop";

    private readonly object m_SyncRoot = new object();

    private ServerNames m_ServerName;

    private readonly IUserPassCredential m_Credential;

    private readonly int m_HashCode;

    private ICimSession m_Session;

    private WmiNamespaceVersion m_VirtualizationNamespaceVersion = WmiNamespaceVersion.V2;

    private bool m_IsHostCluster;

    private bool m_OSInfoLoaded;

    private bool m_IsClientSku;

    private HyperVOSVersion m_OSVersion;

    private ulong m_OSBuildNumber;

    private static readonly ConcurrentDictionary<Tuple<string, IUserPassCredential>, WeakReference<Server>> gm_ServersByUser = new ConcurrentDictionary<Tuple<string, IUserPassCredential>, WeakReference<Server>>();

    private static readonly ConcurrentDictionary<Guid, WeakReference<Server>> gm_ServersByCimSession = new ConcurrentDictionary<Guid, WeakReference<Server>>();

    [SuppressMessage("Microsoft.Naming", "CA1702")]
    public static Server LocalHost => GetServer(Environment.GetEnvironmentVariable("COMPUTERNAME").ToUpperInvariant());

    public string Name => m_ServerName.NetBiosName;

    public string FullName => m_ServerName.FullName;

    public string UserSpecifiedName => m_ServerName.UserSpecifiedName;

    public IPAddress IPAddress => m_ServerName.IPAddress;

    public bool IsLocalhost => m_ServerName.IsLocalhost;

    public bool IsHostCluster
    {
        get
        {
            LoadOSInfo();
            return m_IsHostCluster;
        }
    }

    public bool IsClientSku
    {
        get
        {
            LoadOSInfo();
            return m_IsClientSku;
        }
    }

    public HyperVOSVersion OSVersion
    {
        get
        {
            LoadOSInfo();
            return m_OSVersion;
        }
    }

    public ulong OSBuildNumber
    {
        get
        {
            LoadOSInfo();
            return m_OSBuildNumber;
        }
    }

    public bool FailoverReplicationFeatureEnabled => !IsClientSku;

    public string VirtualizationNamespace
    {
        get
        {
            if (!IsHostCluster)
            {
                return StandaloneHostVirtualizationNamespace;
            }
            return ClusterHostVirtualizationNamespace;
        }
    }

    public WmiNamespaceVersion VirtualizationNamespaceVersion => m_VirtualizationNamespaceVersion;

    public string StandaloneHostVirtualizationNamespace
    {
        get
        {
            WmiNamespaceVersion virtualizationNamespaceVersion = VirtualizationNamespaceVersion;
            _ = 2;
            return "root/virtualization/v2";
        }
    }

    public string ClusterHostVirtualizationNamespace => "root/HyperVCluster/v2";

    public static string CimV2Namespace => "root/cimv2";

    public static string InteropNamespace => "root/interop";

    public static string MSClusterNamespace => "root/mscluster";

    public static string StandardCimV2Namespace => "root/standardcimv2";

    public IWindowsCredential Credential => SimpleCredential as IWindowsCredential;

    public IUserPassCredential SimpleCredential => m_Credential;

    public ICimSession Session
    {
        get
        {
            if (m_Session == null)
            {
                lock (m_SyncRoot)
                {
                    if (m_Session == null)
                    {
                        CreateSession();
                    }
                }
            }
            return m_Session;
        }
    }

    public ServerNames ServerNames
    {
        get
        {
            return m_ServerName;
        }
        internal set
        {
            m_ServerName = value;
        }
    }

    public DateTime LastCacheFlushTime { get; internal set; } = DateTime.MinValue;


    [DllImport("api-ms-win-core-file-l1-2-1.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool GetDiskFreeSpaceExW(string lpDirectoryName, out ulong lpFreeBytesAvailable, out ulong lpTotalNumberOfBytes, out ulong lpTotalNumberOfFreeBytes);

    private Server(ServerNames serverNames, IUserPassCredential credential)
    {
        m_ServerName = serverNames;
        m_Credential = credential;
        if (m_ServerName.IsLocalhost && credential != null)
        {
            throw ThrowHelper.CreateServerConnectionException(m_ServerName.UserSpecifiedName, ServerConnectionIssue.CredentialsNotSupportedOnLocalHost, callback: false, null);
        }
        m_HashCode = FullName.ToUpperInvariant().GetHashCode();
        if (m_Credential != null)
        {
            m_HashCode ^= m_Credential.GetHashCode();
        }
        CreateSession();
        VMTrace.TraceInformation(string.Format(CultureInfo.InvariantCulture, "Server '{0}' constructed. NetBIOSName: '{1}'. Full name: '{2}'. User: '{3}.", m_ServerName.UserSpecifiedName, m_ServerName.NetBiosName, m_ServerName.FullName, (m_Credential != null) ? (m_Credential.DomainName + "\\" + m_Credential.UserName) : "<interactive>"));
    }

    private Server(ICimSession cimSession)
    {
        m_ServerName = ServerNames.Resolve(cimSession);
        m_Session = cimSession;
        m_HashCode = FullName.ToUpperInvariant().GetHashCode();
        VMTrace.TraceInformation(string.Format(CultureInfo.InvariantCulture, "Server '{0}' constructed. NetBIOSName: '{1}'. Full name: '{2}'. CimSession: '{3}.", m_ServerName.UserSpecifiedName, m_ServerName.NetBiosName, m_ServerName.FullName, m_Session.InstanceId));
    }

    public override bool Equals(object obj)
    {
        bool flag = false;
        Server server = obj as Server;
        if (server != null)
        {
            flag = string.Equals(server.FullName, FullName, StringComparison.OrdinalIgnoreCase);
            if (flag)
            {
                flag = server.SimpleCredential == m_Credential;
            }
        }
        return flag;
    }

    public override int GetHashCode()
    {
        return m_HashCode;
    }

    public override string ToString()
    {
        return UserSpecifiedName;
    }

    public static bool IsLocalhostName(string name)
    {
        return ServerNames.IsLocalhostName(name);
    }

    public static Server GetServer(string name)
    {
        return GetServer(name, null);
    }

    public static Server GetServer(string name, IUserPassCredential credential)
    {
        if (name == null)
        {
            throw new ArgumentNullException("name");
        }
        name = name.Trim();
        return GetServer(ServerNames.Resolve(name, credential), credential);
    }

    public static Server GetServer(ServerNames serverNames, IUserPassCredential credential)
    {
        if (serverNames == null)
        {
            throw new ArgumentNullException("serverNames");
        }
        Tuple<string, IUserPassCredential> lookupKey = Tuple.Create(serverNames.FullName.ToUpperInvariant(), credential);
        return GetServerFromDictionary(gm_ServersByUser, lookupKey, () => new Server(serverNames, credential), serverNames.UserSpecifiedName);
    }

    public static Server GetServer(CimSession cimSession)
    {
        return GetServer(cimSession.ToICimSession());
    }

    public static Server GetServer(ICimSession cimSession)
    {
        if (cimSession == null)
        {
            throw new ArgumentNullException("cimSession");
        }
        return GetServerFromDictionary(gm_ServersByCimSession, cimSession.InstanceId, () => new Server(cimSession), cimSession.ComputerName ?? Environment.GetEnvironmentVariable("COMPUTERNAME"));
    }

    public static void RemoveServer(Server server)
    {
        if (server == null)
        {
            throw new ArgumentNullException("server");
        }
        WeakReference<Server> value = null;
        gm_ServersByUser.TryRemove(Tuple.Create(server.FullName.ToUpperInvariant(), server.SimpleCredential), out value);
        gm_ServersByCimSession.TryRemove(server.Session.InstanceId, out value);
    }

    public void TeardownConnections()
    {
        ICimSession session;
        lock (m_SyncRoot)
        {
            session = m_Session;
            m_Session = null;
        }
        session?.Close();
    }

    public void LoadOSInfo()
    {
        if (m_OSInfoLoaded)
        {
            return;
        }
        bool flag = false;
        bool flag2 = false;
        m_OSVersion = HyperVOSVersion.WindowsThreshold;
        bool flag3 = false;
        using (IEnumerator<ICimInstance> enumerator = EnumerateInstances(CimV2Namespace, "Win32_OperatingSystem").GetEnumerator())
        {
            if (enumerator.MoveNext())
            {
                ICimInstance current = enumerator.Current;
                using (current)
                {
                    object value = current.CimInstanceProperties["ProductType"].Value;
                    int num = Convert.ToInt32(value, CultureInfo.CurrentCulture);
                    m_IsClientSku = num == 1;
                    value = current.CimInstanceProperties["BuildNumber"].Value;
                    m_OSBuildNumber = Convert.ToUInt32(value, CultureInfo.InvariantCulture);
                    value = current.CimInstanceProperties["Version"].Value;
                    string input = value.ToString();
                    m_OSVersion = (Version.TryParse(input, out var result) ? OSVersionLoader.GetHyperVOSVersion(result) : HyperVOSVersion.Unsupported);
                    flag = true;
                    flag3 = true;
                }
            }
        }
        if (!flag3)
        {
            throw ThrowHelper.CreateRelatedObjectNotFoundException(this, typeof(CimInstance));
        }
        try
        {
            WmiObjectPath wmiObjectPath = new WmiObjectPath(this, InteropNamespace, "Msvm_RegisteredProfile", new KeyValueDictionary { { "InstanceID", "DMTF|System Virtualization|1.0.0" } });
            foreach (ICimInstance item in from reference in EnumerateReferencingInstances(wmiObjectPath, "Msvm_ElementConformsToProfile", "ConformantStandard", null)
                select ((CimInstance)reference.CimInstanceProperties["ManagedElement"].Value).ToICimInstance())
            {
                using (item)
                {
                    string b = (string)item.CimInstanceProperties["Name"].Value;
                    if (string.Equals(Name, b, StringComparison.OrdinalIgnoreCase) && string.Equals(item.CimSystemProperties.Namespace, "root/HyperVCluster/v2", StringComparison.OrdinalIgnoreCase))
                    {
                        m_IsHostCluster = true;
                    }
                }
            }
            WmiObjectPath wmiObjectPath2 = ObjectKeyCreator.CreateServiceObjectPath(this, m_IsHostCluster ? ClusterHostVirtualizationNamespace : StandaloneHostVirtualizationNamespace, "Msvm_VirtualSystemManagementService", "vmms", "Msvm_ComputerSystem");
            using (ICimInstance cimInstance2 = GetInstance(wmiObjectPath2))
            {
                if (cimInstance2 == null)
                {
                    throw ThrowHelper.CreateServerConnectionException(UserSpecifiedName, ServerConnectionIssue.AccessDenied, callback: false, null);
                }
            }
            flag2 = true;
        }
        catch (InvalidOperationException inner)
        {
            throw ThrowHelper.CreateRemoteObjectNotFoundException(this, "Msvm_ElementConformsToProfile", inner);
        }
        catch (CimException serverException)
        {
            throw ThrowHelper.CreateServerException(this, serverException);
        }
        m_OSInfoLoaded = flag && flag2;
    }

    public static bool operator ==(Server left, Server right)
    {
        if ((object)left == right)
        {
            return true;
        }
        if ((object)left == null || (object)right == null)
        {
            return false;
        }
        return left.Equals(right);
    }

    public static bool operator !=(Server left, Server right)
    {
        return !(left == right);
    }

    public long GetFreeSpaceInBytes(string path)
    {
        if (IsLocalhost)
        {
            if (!GetDiskFreeSpaceExW(path, out var lpFreeBytesAvailable, out var _, out var _))
            {
                return 0L;
            }
            return (long)lpFreeBytesAvailable;
        }
        return 0L;
    }

    public bool IsHyperVComponentSupported(HyperVComponent component, out string notSupportedReason)
    {
        bool result = true;
        notSupportedReason = null;
        if ((uint)(component - 1) <= 1u)
        {
            if (OSVersion != HyperVOSVersion.WindowsThreshold)
            {
                result = false;
                notSupportedReason = ((component == HyperVComponent.VmConnect) ? ErrorMessages.IncompatibleVersionVmConnect : ErrorMessages.IncompatibleVersionInspectVhdDialog);
            }
        }
        else if (OSVersion == HyperVOSVersion.Unsupported)
        {
            result = false;
            notSupportedReason = ErrorMessages.IncompatibleVersionVmBrowser;
        }
        return result;
    }

    public ICimClass GetClass(string wmiNamespace, string className)
    {
        return Session.GetClass(wmiNamespace, className);
    }

    public ICimInstance GetNewInstance(string wmiNamespace, string className, IDictionary<string, object> propertyValues)
    {
        ICimInstance cimInstance = new CimInstance(GetClass(wmiNamespace, className).Class).ToICimInstance();
        if (propertyValues != null)
        {
            SetInstanceProperties(cimInstance, propertyValues);
        }
        return cimInstance;
    }

    public string GetNewEmbeddedInstance(string className, IDictionary<string, object> propertyValues)
    {
        return GetNewEmbeddedInstance(VirtualizationNamespace, className, propertyValues);
    }

    public string GetNewEmbeddedInstance(string wmiNamespace, string className, IDictionary<string, object> propertyValues)
    {
        string text = null;
        using ICimInstance cimInstance = GetNewInstance(wmiNamespace, className, propertyValues);
        byte[] bytes = null;
        using (CimSerializer cimSerializer = CimSerializer.Create("MI_XML", 0u))
        {
            bytes = cimSerializer.Serialize(cimInstance.Instance, InstanceSerializationOptions.None);
        }
        return Encoding.Unicode.GetString(bytes);
    }

    public ICimInstance GetInstance(WmiObjectPath wmiObjectPath)
    {
        return GetInstance(wmiObjectPath, null);
    }

    public ICimInstance GetInstance(WmiObjectPath wmiObjectPath, CimOperationOptions options)
    {
        if (wmiObjectPath == null)
        {
            throw new ArgumentNullException("wmiObjectPath");
        }
        using ICimInstance instanceId = wmiObjectPath.ToCimInstanceId();
        return Session.GetInstance(wmiObjectPath.NamespaceName, instanceId, options);
    }

    public bool TryGetInstance(WmiObjectPath wmiObjectPath, CimOperationOptions options, out ICimInstance cimInstance)
    {
        cimInstance = null;
        try
        {
            cimInstance = GetInstance(wmiObjectPath, options);
            return true;
        }
        catch (CimException ex)
        {
            if (ex.NativeErrorCode == NativeErrorCode.NotFound || ex.NativeErrorCode == NativeErrorCode.InvalidClass || ex.NativeErrorCode == NativeErrorCode.InvalidNamespace)
            {
                return false;
            }
            throw;
        }
    }

    public string GetEmbeddedInstance(WmiObjectPath wmiObjectPath, IDictionary<string, object> propertyValues)
    {
        string text = null;
        using ICimInstance cimInstance = GetInstance(wmiObjectPath);
        if (propertyValues != null)
        {
            SetInstanceProperties(cimInstance, propertyValues);
        }
        return GetEmbeddedInstance(cimInstance);
    }

    public string GetEmbeddedInstance(ICimInstance cimInstance)
    {
        byte[] bytes = null;
        using (CimSerializer cimSerializer = CimSerializer.Create("MI_XML", 0u))
        {
            bytes = cimSerializer.Serialize(cimInstance.Instance, InstanceSerializationOptions.None);
        }
        return Encoding.Unicode.GetString(bytes);
    }

    public ICimInstance GetInstanceFromEmbeddedInstance(string wmiNamespace, string className, string embeddedInstance)
    {
        CimInstance instance = null;
        using (CimDeserializer cimDeserializer = CimDeserializer.Create("MI_XML", 0u))
        {
            using ICimClass cimClass = GetClass(wmiNamespace, className);
            uint offset = 0u;
            instance = cimDeserializer.DeserializeInstance(Encoding.Unicode.GetBytes(embeddedInstance), ref offset, new CimClass[1] { cimClass.Class });
        }
        return instance.ToICimInstance();
    }

    public void ModifyInstance(WmiObjectPath wmiObjectPath, IDictionary<string, object> modifiedValues)
    {
        if (wmiObjectPath == null)
        {
            throw new ArgumentNullException("wmiObjectPath");
        }
        if (modifiedValues == null)
        {
            throw new ArgumentNullException("modifiedValues");
        }
        using ICimInstance cimInstance = GetInstance(wmiObjectPath);
        foreach (KeyValuePair<string, object> modifiedValue in modifiedValues)
        {
            cimInstance.CimInstanceProperties[modifiedValue.Key].Value = modifiedValue.Value;
        }
        Session.ModifyInstance(cimInstance).Dispose();
    }

    public object InvokeMethod(WmiObjectPath wmiObjectPath, string methodName, object[] args)
    {
        if (wmiObjectPath == null)
        {
            throw new ArgumentNullException("wmiObjectPath");
        }
        object obj = null;
        using ICimClass cimClass = GetClass(wmiObjectPath.NamespaceName, wmiObjectPath.ClassName);
        CimMethodParametersCollection cimMethodParametersCollection = null;
        if (args != null)
        {
            cimMethodParametersCollection = new CimMethodParametersCollection();
            foreach (CimMethodParameterDeclaration parameter in cimClass.CimClassMethods[methodName].Parameters)
            {
                CimQualifier cimQualifier = parameter.Qualifiers["In"];
                if (cimQualifier != null && (bool)cimQualifier.Value)
                {
                    int num = (int)parameter.Qualifiers["ID"].Value;
                    object arg = args[num];
                    ConvertOneMethodArgument(ref arg);
                    CimFlags cimFlags = CimFlags.Parameter;
                    if (arg == null)
                    {
                        cimFlags |= CimFlags.NullValue;
                    }
                    cimMethodParametersCollection.Add(CimMethodParameter.Create(parameter.Name, arg, parameter.CimType, cimFlags));
                }
            }
        }
        CimMethodResult cimMethodResult;
        using (ICimInstance instance = wmiObjectPath.ToCimInstanceId())
        {
            cimMethodResult = Session.InvokeMethod(instance, methodName, cimMethodParametersCollection);
        }
        using (cimMethodResult)
        {
            if (args != null)
            {
                foreach (CimMethodParameterDeclaration parameter2 in cimClass.CimClassMethods[methodName].Parameters)
                {
                    CimQualifier cimQualifier2 = parameter2.Qualifiers["Out"];
                    if (cimQualifier2 == null || !(bool)cimQualifier2.Value)
                    {
                        continue;
                    }
                    object obj2 = cimMethodResult.OutParameters[parameter2.Name].Value;
                    if (obj2 != null)
                    {
                        switch (parameter2.CimType)
                        {
                        case CimType.Reference:
                            obj2 = new WmiObjectPath(this, wmiObjectPath.NamespaceName, ((CimInstance)obj2).ToICimInstance());
                            break;
                        case CimType.ReferenceArray:
                        {
                            CimInstance[] array = (CimInstance[])obj2;
                            WmiObjectPath[] array2 = new WmiObjectPath[array.Length];
                            for (int i = 0; i < array.Length; i++)
                            {
                                CimInstance instance2 = array[i];
                                array2[i] = new WmiObjectPath(this, wmiObjectPath.NamespaceName, instance2.ToICimInstance());
                            }
                            obj2 = array2;
                            break;
                        }
                        }
                    }
                    int num2 = (int)parameter2.Qualifiers["ID"].Value;
                    args[num2] = obj2;
                }
            }
            return cimMethodResult.ReturnValue.Value;
        }
    }

    public IEnumerable<ICimInstance> EnumerateInstances(string wmiNamespace, string className)
    {
        return Session.EnumerateInstances(wmiNamespace, className);
    }

    public IEnumerable<ICimInstance> EnumerateInstances(string wmiNamespace, string className, CimOperationOptions options)
    {
        return Session.EnumerateInstances(wmiNamespace, className, options);
    }

    public IEnumerable<ICimInstance> QueryInstances(string wmiNamespace, string query)
    {
        return Session.QueryInstances(wmiNamespace, "WQL", query);
    }

    public IEnumerable<ICimInstance> QueryInstances(string wmiNamespace, string query, CimOperationOptions options)
    {
        return Session.QueryInstances(wmiNamespace, "WQL", query, options);
    }

    public IEnumerable<ICimInstance> EnumerateAssociatedInstances(WmiObjectPath wmiObjectPath, string associationClass, string resultClass, string sourceRole, string resultRole, CimOperationOptions options)
    {
        if (wmiObjectPath == null)
        {
            throw new ArgumentNullException("wmiObjectPath");
        }
        ICimInstance sourceInstance = wmiObjectPath.ToCimInstanceId();
        return Session.EnumerateAssociatedInstances(wmiObjectPath.NamespaceName, sourceInstance, associationClass, resultClass, sourceRole, resultRole, options);
    }

    public IEnumerable<ICimInstance> EnumerateReferencingInstances(WmiObjectPath wmiObjectPath, string associationClass, string sourceRole, CimOperationOptions options)
    {
        if (wmiObjectPath == null)
        {
            throw new ArgumentNullException("wmiObjectPath");
        }
        ICimInstance sourceInstance = wmiObjectPath.ToCimInstanceId();
        return Session.EnumerateReferencingInstances(wmiObjectPath.NamespaceName, sourceInstance, associationClass, sourceRole, options);
    }

    public CimAsyncMultipleResults<CimSubscriptionResult> SubscribeAsync(string wmiNamespace, string eventQuery)
    {
        return Session.SubscribeAsync(wmiNamespace, "WQL", eventQuery);
    }

    public IDisposable SubscribeAsync(string wmiNamespace, string eventQuery, IObserver<CimSubscriptionResult> subscriber)
    {
        return SubscribeAsync(wmiNamespace, eventQuery).Subscribe(subscriber);
    }

    public void FlushCache()
    {
        LastCacheFlushTime = DateTime.Now;
    }

    private static Server GetServerFromDictionary<TKey>(ConcurrentDictionary<TKey, WeakReference<Server>> servers, TKey lookupKey, Func<Server> createServer, string serverName)
    {
        Server target = null;
        int num = 0;
        do
        {
            WeakReference<Server> orAdd = servers.GetOrAdd(lookupKey, (TKey ignored) => new WeakReference<Server>(createServer()));
            if (orAdd.TryGetTarget(out target))
            {
                break;
            }
            Server server = createServer();
            WeakReference<Server> newValue = new WeakReference<Server>(server);
            if (servers.TryUpdate(lookupKey, newValue, orAdd))
            {
                target = server;
                break;
            }
        }
        while (++num < 2);
        if (target == null)
        {
            throw ThrowHelper.CreateServerConnectionException(serverName, ServerConnectionIssue.Unknown, callback: false, null);
        }
        return target;
    }

    private void CreateSession()
    {
        string serverName = null;
        if (!m_ServerName.IsLocalhost)
        {
            serverName = ((m_ServerName.IPAddress != null) ? m_ServerName.IPAddress.ToString() : m_ServerName.FullName);
        }
        ICimSession cimSession = CreateSession(serverName, SimpleCredential);
        ICimInstance instance = null;
        CimException exception = null;
        if (!cimSession.TestConnection(out instance, out exception))
        {
            throw ThrowHelper.CreateServerException(this, exception);
        }
        m_Session = cimSession;
    }

    internal static ICimSession CreateSession(string serverName, IUserPassCredential credential)
    {
        CimSession cimSession = null;
        if (serverName == null)
        {
            cimSession = CimSession.Create(null);
        }
        else
        {
            CimSessionOptions cimSessionOptions = null;
            if (credential != null)
            {
                CimCredential credential2 = new CimCredential(PasswordAuthenticationMechanism.CredSsp, credential.DomainName, credential.UserName, credential.Password);
                cimSessionOptions = new CimSessionOptions();
                cimSessionOptions.AddDestinationCredentials(credential2);
            }
            cimSession = CimSession.Create(serverName, cimSessionOptions);
        }
        return cimSession.ToICimSession();
    }

    private static void ConvertOneMethodArgument(ref object arg)
    {
        if (arg == null)
        {
            return;
        }
        Type type = arg.GetType();
        if (!type.IsArray)
        {
            arg = ConvertOneMethodArgumentNonArray(arg);
            return;
        }
        IEnumerable<object> source = ((IEnumerable)arg).Cast<object>().Select(ConvertOneMethodArgumentNonArray);
        if (type == typeof(WmiObjectPath[]) || typeof(IVirtualizationManagementObject[]).IsAssignableFrom(type))
        {
            arg = source.Cast<CimInstance>().ToArray();
        }
        else if (typeof(EmbeddedInstance[]).IsAssignableFrom(type))
        {
            arg = source.Cast<string>().ToArray();
        }
        else
        {
            arg = source.ToArray();
        }
    }

    private static object ConvertOneMethodArgumentNonArray(object arg)
    {
        if (arg is WmiObjectPath)
        {
            return ((WmiObjectPath)arg).ToCimInstanceId().Instance;
        }
        if (arg is IVirtualizationManagementObject)
        {
            return ((IVirtualizationManagementObject)arg).ManagementPath.ToCimInstanceId().Instance;
        }
        if (arg is EmbeddedInstance)
        {
            return ((EmbeddedInstance)arg).ToString();
        }
        return arg;
    }

    private static void SetInstanceProperties(ICimInstance cimInstance, IDictionary<string, object> propertyValues)
    {
        foreach (CimProperty cimInstanceProperty in cimInstance.CimInstanceProperties)
        {
            if (propertyValues.TryGetValue(cimInstanceProperty.Name, out var value))
            {
                cimInstanceProperty.Value = value;
            }
        }
    }

    private void ObjectInvariant()
    {
    }
}
