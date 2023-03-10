using System;

namespace Microsoft.Virtualization.Client.Management;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
internal sealed class KeyAttribute : Attribute
{
}
