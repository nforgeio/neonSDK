using System;

namespace Microsoft.Virtualization.Client.Management;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
internal sealed class FrequentlyChangingAttribute : Attribute
{
}
