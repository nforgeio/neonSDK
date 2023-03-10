using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal class CimInstanceWrapper : ICimInstance, IDisposable
{
	private readonly CimInstance instance;

	public ICimClass CimClass => instance.CimClass.ToICimClass();

	public CimInstance Instance => instance;

	public CimKeyedCollection<CimProperty> CimInstanceProperties => instance.CimInstanceProperties;

	public CimSystemProperties CimSystemProperties => instance.CimSystemProperties;

	internal CimInstanceWrapper(CimInstance instance)
	{
		this.instance = instance;
	}

	public string GetCimSessionComputerName()
	{
		return instance.GetCimSessionComputerName();
	}

	public Guid GetCimSessionInstanceId()
	{
		return instance.GetCimSessionInstanceId();
	}

	public void Dispose()
	{
		instance.Dispose();
	}

	public override string ToString()
	{
		return instance.ToString();
	}

	[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is called via contracts.")]
	private void ObjectInvariant()
	{
	}
}
