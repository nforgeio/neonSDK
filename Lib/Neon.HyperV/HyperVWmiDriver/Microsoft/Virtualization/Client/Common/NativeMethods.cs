using System.Runtime.InteropServices;

namespace Microsoft.Virtualization.Client.Common;

internal static class NativeMethods
{
    [DllImport("vmstaging.dll", PreserveSig = false)]
    internal static extern void IsFeatureEnabled(Feature Feature, [MarshalAs(UnmanagedType.Bool)] out bool IsEnabled);
}
