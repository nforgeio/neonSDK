using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal class CimClassWrapper : ICimClass, IDisposable
{
	private readonly CimClass klass;

	public CimReadOnlyKeyedCollection<CimMethodDeclaration> CimClassMethods => klass.CimClassMethods;

	public CimReadOnlyKeyedCollection<CimPropertyDeclaration> CimClassProperties => klass.CimClassProperties;

	public CimReadOnlyKeyedCollection<CimQualifier> CimClassQualifiers => klass.CimClassQualifiers;

	public ICimClass CimSuperClass => klass.CimSuperClass.ToICimClass();

	public CimClass Class => klass;

	public string CimSuperClassName => klass.CimSuperClassName;

	public CimSystemProperties CimSystemProperties => klass.CimSystemProperties;

	internal CimClassWrapper(CimClass klass)
	{
		this.klass = klass;
	}

	public void Dispose()
	{
		klass.Dispose();
	}

	public override int GetHashCode()
	{
		return klass.GetHashCode();
	}

	public override bool Equals(object o)
	{
		return klass.Equals(o);
	}

	public override string ToString()
	{
		return klass.ToString();
	}

	[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is called via contracts.")]
	private void ObjectInvariant()
	{
	}
}
