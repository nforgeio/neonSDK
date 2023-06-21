using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Virtualization.Client.Management;

[SuppressMessage("Microsoft.Naming", "CA1704")]
internal interface IPutable
{
    void Put();
}
