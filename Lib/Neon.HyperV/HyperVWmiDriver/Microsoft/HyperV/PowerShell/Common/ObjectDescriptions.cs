using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Microsoft.HyperV.PowerShell.Common;

[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
[DebuggerNonUserCode]
[CompilerGenerated]
internal class ObjectDescriptions
{
    private static ResourceManager resourceMan;

    private static CultureInfo resourceCulture;

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    internal static ResourceManager ResourceManager
    {
        get
        {
            if (resourceMan == null)
            {
                resourceMan = new ResourceManager(WmiHelper.GetResourceName("Microsoft.HyperV.PowerShell.Objects.Common.ObjectDescriptions"), typeof(ObjectDescriptions).GetTypeInfo().Assembly);
            }
            return resourceMan;
        }
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    internal static CultureInfo Culture
    {
        get
        {
            return resourceCulture;
        }
        set
        {
            resourceCulture = value;
        }
    }

    internal static string VMNetworkAdapter_DefaultName_Legacy => ResourceManager.GetString("VMNetworkAdapter_DefaultName_Legacy", resourceCulture);

    internal static string VMNetworkAdapter_DefaultName_Synthetic => ResourceManager.GetString("VMNetworkAdapter_DefaultName_Synthetic", resourceCulture);

    internal static string VMNetworkAdapter_SwitchName_SoftAffinity => ResourceManager.GetString("VMNetworkAdapter_SwitchName_SoftAffinity", resourceCulture);

    internal static string VMSwitch_SETInterfaceDescription => ResourceManager.GetString("VMSwitch_SETInterfaceDescription", resourceCulture);

    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
    internal ObjectDescriptions()
    {
    }
}
