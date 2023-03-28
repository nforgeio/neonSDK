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
internal class EnumValues
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
				resourceMan = new ResourceManager(WmiHelper.GetResourceName("Microsoft.HyperV.PowerShell.Objects.Common.EnumValues"), typeof(EnumValues).GetTypeInfo().Assembly);
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

	internal static string Error_CannotConvertValue => ResourceManager.GetString("Error_CannotConvertValue", resourceCulture);

	internal static string ICStatus_None => ResourceManager.GetString("ICStatus_None", resourceCulture);

	internal static string ICStatus_RequiresUpdate => ResourceManager.GetString("ICStatus_RequiresUpdate", resourceCulture);

	internal static string ICStatus_UpToDate => ResourceManager.GetString("ICStatus_UpToDate", resourceCulture);

	internal static string TaskStatus_Completed => ResourceManager.GetString("TaskStatus_Completed", resourceCulture);

	internal static string TaskStatus_CompletedWithWarning => ResourceManager.GetString("TaskStatus_CompletedWithWarning", resourceCulture);

	internal static string TaskStatus_Deleted => ResourceManager.GetString("TaskStatus_Deleted", resourceCulture);

	internal static string TaskStatus_Exception => ResourceManager.GetString("TaskStatus_Exception", resourceCulture);

	internal static string TaskStatus_Killed => ResourceManager.GetString("TaskStatus_Killed", resourceCulture);

	internal static string TaskStatus_New => ResourceManager.GetString("TaskStatus_New", resourceCulture);

	internal static string TaskStatus_Running => ResourceManager.GetString("TaskStatus_Running", resourceCulture);

	internal static string TaskStatus_ShuttingDown => ResourceManager.GetString("TaskStatus_ShuttingDown", resourceCulture);

	internal static string TaskStatus_Starting => ResourceManager.GetString("TaskStatus_Starting", resourceCulture);

	internal static string TaskStatus_Suspended => ResourceManager.GetString("TaskStatus_Suspended", resourceCulture);

	internal static string TaskStatus_Terminated => ResourceManager.GetString("TaskStatus_Terminated", resourceCulture);

	internal static string ThreadCount_Automatic => ResourceManager.GetString("ThreadCount_Automatic", resourceCulture);

	internal static string ThreadCount_High => ResourceManager.GetString("ThreadCount_High", resourceCulture);

	internal static string ThreadCount_Low => ResourceManager.GetString("ThreadCount_Low", resourceCulture);

	internal static string ThreadCount_Medium => ResourceManager.GetString("ThreadCount_Medium", resourceCulture);

	internal static string VMMemoryStatus_Low => ResourceManager.GetString("VMMemoryStatus_Low", resourceCulture);

	internal static string VMMemoryStatus_None => ResourceManager.GetString("VMMemoryStatus_None", resourceCulture);

	internal static string VMMemoryStatus_Ok => ResourceManager.GetString("VMMemoryStatus_Ok", resourceCulture);

	internal static string VMMemoryStatus_Paging => ResourceManager.GetString("VMMemoryStatus_Paging", resourceCulture);

	internal static string VMMemoryStatus_Spanning => ResourceManager.GetString("VMMemoryStatus_Spanning", resourceCulture);

	internal static string VMMemoryStatus_Warning => ResourceManager.GetString("VMMemoryStatus_Warning", resourceCulture);

	[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
	internal EnumValues()
	{
	}
}
