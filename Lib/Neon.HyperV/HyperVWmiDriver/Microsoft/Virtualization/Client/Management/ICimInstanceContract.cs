using System;
using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class ICimInstanceContract : ICimInstance, IDisposable
{
    public ICimClass CimClass => null;

    public CimInstance Instance => null;

    public CimKeyedCollection<CimProperty> CimInstanceProperties => null;

    public CimSystemProperties CimSystemProperties => null;

    public Guid GetCimSessionInstanceId()
    {
        return default(Guid);
    }

    public string GetCimSessionComputerName()
    {
        return null;
    }

    public abstract void Dispose();

    public abstract object Clone();
}
