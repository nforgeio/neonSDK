using System.Diagnostics.CodeAnalysis;
using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Options;

namespace Microsoft.Virtualization.Client.Management;

internal class WmiOperationOptions
{
    private readonly CimOperationOptions m_CimOperationOptions = new CimOperationOptions();

    private string[] m_PartialUpdateProperties;

    internal CimOperationOptions CimOperationOptions => m_CimOperationOptions;

    internal bool PartialObject
    {
        get
        {
            if (!m_CimOperationOptions.KeysOnly)
            {
                return m_PartialUpdateProperties != null;
            }
            return true;
        }
    }

    internal string[] PartialObjectProperties
    {
        get
        {
            return m_PartialUpdateProperties;
        }
        set
        {
            m_CimOperationOptions.SetCustomOption("__GET_EXTENSIONS", optionValue: true, mustComply: false);
            m_CimOperationOptions.SetCustomOption("__GET_EXT_CLIENT_REQUEST", optionValue: true, mustComply: false);
            m_CimOperationOptions.SetCustomOption("__GET_EXT_PROPERTIES", value, CimType.StringArray, mustComply: false);
            m_PartialUpdateProperties = value;
        }
    }

    internal bool KeysOnly
    {
        get
        {
            return m_CimOperationOptions.KeysOnly;
        }
        set
        {
            m_CimOperationOptions.KeysOnly = value;
        }
    }

    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is called via contracts.")]
    private void ObjectInvariant()
    {
    }
}
