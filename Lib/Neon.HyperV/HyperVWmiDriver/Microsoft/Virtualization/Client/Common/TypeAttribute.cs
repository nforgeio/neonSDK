using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Virtualization.Client.Common;

[AttributeUsage(AttributeTargets.Assembly)]
internal class TypeAttribute : Attribute
{
    private readonly Type _implementingType;

    internal Type ImplementingType => _implementingType;

    internal TypeAttribute(Type implementingType)
    {
        _implementingType = implementingType;
    }

    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is called via contracts.")]
    private void ObjectInvariant()
    {
    }
}
