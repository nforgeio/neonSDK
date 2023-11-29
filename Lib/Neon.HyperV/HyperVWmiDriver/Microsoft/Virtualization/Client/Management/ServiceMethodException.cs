using System;

namespace Microsoft.Virtualization.Client.Management;

internal class ServiceMethodException : VirtualizationManagementException
{
    private ServiceMethodError m_ErrorCode;

    public ServiceMethodError ErrorCode
    {
        get
        {
            return m_ErrorCode;
        }
        set
        {
            m_ErrorCode = value;
        }
    }

    public ServiceMethodException()
    {
    }

    public ServiceMethodException(string message)
        : base(message)
    {
    }

    public ServiceMethodException(string message, Exception inner)
        : base(message, inner)
    {
    }

    public ServiceMethodException(string message, ServiceMethodError errorCode)
        : base(message)
    {
        m_ErrorCode = errorCode;
    }
}
