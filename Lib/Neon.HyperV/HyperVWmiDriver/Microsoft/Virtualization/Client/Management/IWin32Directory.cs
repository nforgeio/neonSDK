using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Win32_Directory")]
internal interface IWin32Directory : IVirtualizationManagementObject, IDeleteable
{
	[Key]
	string Name { get; }

	bool IsHidden { get; }

	bool IsSystem { get; }

	IEnumerable<IDataFile> GetFiles();

	IEnumerable<IWin32Directory> GetSubdirectories();
}
