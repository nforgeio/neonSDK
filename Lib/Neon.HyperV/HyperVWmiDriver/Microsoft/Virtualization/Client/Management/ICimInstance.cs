using System;
using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal interface ICimInstance : IDisposable
{
	ICimClass CimClass { get; }

	CimInstance Instance { get; }

	CimKeyedCollection<CimProperty> CimInstanceProperties { get; }

	CimSystemProperties CimSystemProperties { get; }

	Guid GetCimSessionInstanceId();

	string GetCimSessionComputerName();
}
