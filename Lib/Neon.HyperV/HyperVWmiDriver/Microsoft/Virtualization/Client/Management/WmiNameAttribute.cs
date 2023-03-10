using System;

namespace Microsoft.Virtualization.Client.Management;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
internal sealed class WmiNameAttribute : Attribute
{
	public string Name { get; private set; }

	public bool PrimaryMapping { get; set; }

	public WmiNameAttribute(string name)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			throw new ArgumentNullException("name");
		}
		PrimaryMapping = true;
		Name = name;
	}
}
