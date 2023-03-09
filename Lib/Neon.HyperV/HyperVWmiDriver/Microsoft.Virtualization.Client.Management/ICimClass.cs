using System;
using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal interface ICimClass : IDisposable
{
	CimReadOnlyKeyedCollection<CimMethodDeclaration> CimClassMethods { get; }

	CimReadOnlyKeyedCollection<CimPropertyDeclaration> CimClassProperties { get; }

	CimReadOnlyKeyedCollection<CimQualifier> CimClassQualifiers { get; }

	ICimClass CimSuperClass { get; }

	CimClass Class { get; }

	string CimSuperClassName { get; }

	CimSystemProperties CimSystemProperties { get; }
}
