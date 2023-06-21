using System.Diagnostics.CodeAnalysis;

namespace Microsoft.HyperV.PowerShell;

[SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue", Justification = "Doesn't make sense to have a zero here as it's defined by DMTF standard.")]
internal enum ReplicationAuthenticationType
{
    Kerberos = 1,
    Certificate
}
