using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Management.Infrastructure;

namespace Microsoft.Virtualization.Client.Management;

internal class WmiObjectPath : IEquatable<WmiObjectPath>
{
    private static readonly Regex gm_WmiObjectPathParser = new Regex("(?:\\\\\\\\(?<server>[^\\\\]+)\\\\)?(?:(?<namespace>[^:]+):)?(?:(?<class>[^:.]+))(?:$|\\.(?:(?<keys>[^=]+)=(?<values>\"[^\"]*\"|'[^\"]*'|[^=]+),?)+)", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private readonly int m_HashCode;

    private readonly string m_ServerName;

    private readonly string m_NamespaceName;

    private readonly string m_ClassName;

    private readonly KeyValueDictionary m_KeyValues = new KeyValueDictionary();

    public string ServerName => m_ServerName;

    public string NamespaceName => m_NamespaceName;

    public string ClassName => m_ClassName;

    public IReadOnlyDictionary<string, object> KeyValues => m_KeyValues;

    public bool IsInstance => m_KeyValues.Count != 0;

    public ICimClass CimClass { get; private set; }

    public WmiObjectPath(string wmiObjectPath)
    {
        ParseWmiObjectPath(wmiObjectPath, out m_ServerName, out m_NamespaceName, out m_ClassName, out m_KeyValues);
        m_HashCode = CalculateHashCode();
    }

    public WmiObjectPath(Server server, string wmiNamespace, ICimInstance cimInstance)
        : this(server.Name, wmiNamespace, cimInstance)
    {
    }

    [Obsolete("This constructor is obsolete. Use the Server constructor instead.", false)]
    public WmiObjectPath(string serverName, string wmiNamespace, ICimInstance cimInstance)
    {
        if (serverName == null)
        {
            throw new ArgumentNullException("serverName");
        }
        if (wmiNamespace == null)
        {
            throw new ArgumentNullException("wmiNamespace");
        }
        if (cimInstance == null)
        {
            throw new ArgumentNullException("cimInstance");
        }
        m_ServerName = serverName;
        m_NamespaceName = NormalizeNamespaceDelimiters(wmiNamespace);
        m_ClassName = cimInstance.CimSystemProperties.ClassName;
        foreach (CimProperty cimInstanceProperty in cimInstance.CimInstanceProperties)
        {
            if ((cimInstanceProperty.Flags & CimFlags.Key) != CimFlags.None)
            {
                m_KeyValues.Add(cimInstanceProperty.Name, cimInstanceProperty.Value);
            }
        }
        m_HashCode = CalculateHashCode();
        CimClass = cimInstance.CimClass;
    }

    public WmiObjectPath(Server server, string wmiNamespace, string className, IDictionary<string, object> keyValues)
        : this(server.Name, wmiNamespace, className, keyValues)
    {
    }

    [Obsolete("This constructor is obsolete. Use the Server constructor instead.", false)]
    private WmiObjectPath(string serverName, string wmiNamespace, string className, IDictionary<string, object> keyValues)
    {
        if (serverName == null)
        {
            throw new ArgumentNullException("serverName");
        }
        if (wmiNamespace == null)
        {
            throw new ArgumentNullException("wmiNamespace");
        }
        if (className == null)
        {
            throw new ArgumentNullException("className");
        }
        m_ServerName = serverName;
        m_NamespaceName = NormalizeNamespaceDelimiters(wmiNamespace);
        m_ClassName = className;
        if (keyValues != null)
        {
            if (keyValues is KeyValueDictionary)
            {
                m_KeyValues = (KeyValueDictionary)keyValues;
            }
            else
            {
                foreach (KeyValuePair<string, object> keyValue in keyValues)
                {
                    m_KeyValues.Add(keyValue.Key, keyValue.Value);
                }
                if (m_KeyValues.Count != keyValues.Count)
                {
                    throw new ArgumentException("keyValues contained one or more duplicate key values.", "keyValues");
                }
            }
        }
        m_HashCode = CalculateHashCode();
    }

    public WmiObjectPath(Server server, string wmiNamespace, string className)
        : this(server, wmiNamespace, className, null)
    {
    }

    public bool Equals(WmiObjectPath other)
    {
        bool flag = other != null && m_ServerName.Equals(other.m_ServerName, StringComparison.OrdinalIgnoreCase) && m_NamespaceName.Equals(other.m_NamespaceName, StringComparison.OrdinalIgnoreCase) && m_ClassName.Equals(other.m_ClassName, StringComparison.OrdinalIgnoreCase) && m_KeyValues.Count == other.m_KeyValues.Count;
        if (flag)
        {
            foreach (KeyValuePair<string, object> keyValue in m_KeyValues)
            {
                if (!other.KeyValues.ContainsKey(keyValue.Key))
                {
                    return false;
                }
                object obj = other.KeyValues[keyValue.Key];
                flag = ((!(obj is string)) ? (obj is long && keyValue.Value is long && (long)obj == (long)keyValue.Value) : (keyValue.Value is string && ((string)obj).Equals((string)keyValue.Value, StringComparison.OrdinalIgnoreCase)));
                if (!flag)
                {
                    return flag;
                }
            }
            return flag;
        }
        return flag;
    }

    public override int GetHashCode()
    {
        return m_HashCode;
    }

    public override bool Equals(object other)
    {
        if (this == other)
        {
            return true;
        }
        WmiObjectPath wmiObjectPath = other as WmiObjectPath;
        if (wmiObjectPath != null)
        {
            return Equals(wmiObjectPath);
        }
        return false;
    }

    public static bool operator ==(WmiObjectPath thing1, WmiObjectPath thing2)
    {
        if ((object)thing1 != thing2)
        {
            return thing1?.Equals(thing2) ?? false;
        }
        return true;
    }

    public static bool operator !=(WmiObjectPath thing1, WmiObjectPath thing2)
    {
        return !(thing1 == thing2);
    }

    public ICimInstance ToCimInstanceId()
    {
        if (!IsInstance)
        {
            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, ErrorMessages.WmiObjectPath_CannotConvertClassRefToInstanceId, ToString()));
        }
        CimInstance cimInstance = new CimInstance(ClassName, NamespaceName);
        foreach (KeyValuePair<string, object> keyValue in KeyValues)
        {
            cimInstance.CimInstanceProperties.Add(CimProperty.Create(keyValue.Key, keyValue.Value, CimFlags.Property | CimFlags.Key));
        }
        return cimInstance.ToICimInstance();
    }

    public override string ToString()
    {
        return string.Format(CultureInfo.InvariantCulture, "\\\\{0}\\{1}:{2}", ServerName, NamespaceName, GetRelativePath());
    }

    public static WmiObjectPath[] FromStringArray(string[] wmiObjectPathStrings)
    {
        if (wmiObjectPathStrings == null)
        {
            throw new ArgumentNullException("wmiObjectPathStrings");
        }
        WmiObjectPath[] array = new WmiObjectPath[wmiObjectPathStrings.Length];
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = new WmiObjectPath(wmiObjectPathStrings[i]);
        }
        return array;
    }

    public static string[] ToStringArray(WmiObjectPath[] wmiObjectPaths)
    {
        if (wmiObjectPaths == null)
        {
            throw new ArgumentNullException("wmiObjectPaths");
        }
        string[] array = new string[wmiObjectPaths.Length];
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = wmiObjectPaths[i].ToString();
        }
        return array;
    }

    public static WmiObjectPath FromRelativePath(Server server, string namespaceName, string relativePath)
    {
        ParseWmiObjectPath(relativePath, out var serverName, out var namespaceName2, out var className, out var keyValues);
        if (string.IsNullOrEmpty(serverName))
        {
            serverName = server.Name;
        }
        if (string.IsNullOrEmpty(namespaceName2))
        {
            namespaceName2 = namespaceName;
        }
        return new WmiObjectPath(serverName, namespaceName2, className, keyValues);
    }

    private static string NormalizeNamespaceDelimiters(string namespaceName)
    {
        if (!string.IsNullOrEmpty(namespaceName))
        {
            namespaceName = namespaceName.Replace('/', '\\');
        }
        return namespaceName;
    }

    private static void ParseWmiObjectPath(string wmiObjectPath, out string serverName, out string namespaceName, out string className, out KeyValueDictionary keyValues)
    {
        serverName = null;
        namespaceName = null;
        className = null;
        keyValues = null;
        Match match = gm_WmiObjectPathParser.Match(wmiObjectPath);
        if (!match.Success)
        {
            throw CreateInvalidWmiObjectPathException(wmiObjectPath, "wmiObjectPath");
        }
        Group group = match.Groups["server"];
        if (group.Success && !group.Value.Equals(".", StringComparison.OrdinalIgnoreCase))
        {
            serverName = group.Value;
        }
        else
        {
            serverName = string.Empty;
        }
        Group group2 = match.Groups["namespace"];
        namespaceName = (group2.Success ? NormalizeNamespaceDelimiters(group2.Value) : string.Empty);
        Group group3 = match.Groups["class"];
        if (group3.Success)
        {
            className = group3.Value;
            Group group4 = match.Groups["keys"];
            Group group5 = match.Groups["values"];
            if (group4.Captures.Count != group5.Captures.Count)
            {
                throw CreateInvalidWmiObjectPathException(wmiObjectPath, "wmiObjectPath");
            }
            keyValues = new KeyValueDictionary();
            for (int i = 0; i < group4.Captures.Count; i++)
            {
                string value = group4.Captures[i].Value;
                string value2 = group5.Captures[i].Value;
                long result = 0L;
                if (long.TryParse(value2, out result))
                {
                    keyValues.Add(value, result);
                    continue;
                }
                keyValues.Add(value, UnescapeKeyValue(value2.Trim('"')));
            }
            return;
        }
        className = null;
        throw CreateInvalidWmiObjectPathException(wmiObjectPath, "wmiObjectPath");
    }

    private int CalculateHashCode()
    {
        int hashCode = m_ServerName.ToUpperInvariant().GetHashCode();
        hashCode ^= m_NamespaceName.ToUpperInvariant().GetHashCode();
        hashCode ^= m_ClassName.ToUpperInvariant().GetHashCode();
        foreach (KeyValuePair<string, object> keyValue in KeyValues)
        {
            hashCode ^= keyValue.Key.ToUpperInvariant().GetHashCode();
            hashCode = ((!(keyValue.Value is string)) ? (hashCode ^ keyValue.Value.GetHashCode()) : (hashCode ^ ((string)keyValue.Value).ToUpperInvariant().GetHashCode()));
        }
        return hashCode;
    }

    private string GetRelativePath()
    {
        StringBuilder stringBuilder = new StringBuilder(ClassName);
        char value = '.';
        foreach (KeyValuePair<string, object> keyValue in KeyValues)
        {
            stringBuilder.Append(value);
            if (keyValue.Value is string)
            {
                stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}=\"{1}\"", keyValue.Key, EscapeKeyValue((string)keyValue.Value));
            }
            else
            {
                stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}={1}", keyValue.Key, keyValue.Value);
            }
            value = ',';
        }
        return stringBuilder.ToString();
    }

    private void ObjectInvariant()
    {
    }

    private static string UnescapeKeyValue(string keyValue)
    {
        if (keyValue == null)
        {
            throw new ArgumentNullException("keyValue");
        }
        StringBuilder stringBuilder = new StringBuilder(keyValue);
        stringBuilder.Replace("\\\"", "\"");
        stringBuilder.Replace("\\\\", "\\");
        return stringBuilder.ToString();
    }

    private static string EscapeKeyValue(string keyValue)
    {
        if (keyValue == null)
        {
            throw new ArgumentNullException("keyValue");
        }
        StringBuilder stringBuilder = new StringBuilder(keyValue);
        stringBuilder.Replace("\\", "\\\\");
        stringBuilder.Replace("\"", "\\\"");
        return stringBuilder.ToString();
    }

    private static ArgumentException CreateInvalidWmiObjectPathException(string invalidWmiObjectPath, string paramName)
    {
        return new ArgumentException(string.Format(CultureInfo.InvariantCulture, ErrorMessages.InvalidParameter_WmiObjectPathInvalid, invalidWmiObjectPath), paramName);
    }
}
