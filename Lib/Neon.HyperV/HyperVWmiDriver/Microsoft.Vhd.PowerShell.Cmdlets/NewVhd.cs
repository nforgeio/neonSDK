using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell;
using Microsoft.HyperV.PowerShell.Commands;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.Vhd.PowerShell.Cmdlets;

[Cmdlet("New", "VHD", ConfirmImpact = ConfirmImpact.Medium, SupportsShouldProcess = true, DefaultParameterSetName = "DynamicWithoutSource")]
[OutputType(new Type[] { typeof(VirtualHardDisk) })]
internal sealed class NewVhd : VirtualizationCreationCmdlet<VirtualHardDisk>, ISupportsAsJob
{
	private static class ParameterSetNames
	{
		public const string FixedWithSource = "FixedWithSource";

		public const string FixedWithoutSource = "FixedWithoutSource";

		public const string DynamicWithSource = "DynamicWithSource";

		public const string DynamicWithoutSource = "DynamicWithoutSource";

		public const string Differencing = "Differencing";
	}

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec. Also arrary is more user friendly.")]
	[ValidateNotNullOrEmpty]
	[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
	public string[] Path { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter(Mandatory = true, Position = 1, ParameterSetName = "Differencing")]
	public string ParentPath { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter(Mandatory = true, Position = 1, ParameterSetName = "FixedWithoutSource")]
	[Parameter(Mandatory = true, Position = 1, ParameterSetName = "DynamicWithoutSource")]
	[Parameter(Position = 2, ParameterSetName = "Differencing")]
	public ulong SizeBytes { get; set; }

	[ValidateNotNullOrEmpty]
	[Alias(new string[] { "Number" })]
	[Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "FixedWithSource")]
	[Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "DynamicWithSource")]
	public uint SourceDisk { get; set; }

	[Parameter(Mandatory = true, ParameterSetName = "DynamicWithSource")]
	[Parameter(ParameterSetName = "DynamicWithoutSource")]
	public SwitchParameter Dynamic { get; set; }

	[Parameter(Mandatory = true, ParameterSetName = "FixedWithSource")]
	[Parameter(Mandatory = true, ParameterSetName = "FixedWithoutSource")]
	public SwitchParameter Fixed { get; set; }

	[Parameter(ParameterSetName = "Differencing")]
	public SwitchParameter Differencing { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter]
	public uint BlockSizeBytes { get; set; }

	[ValidateNotNullOrEmpty]
	[ValidateSet(new string[] { "512", "4096" })]
	[Parameter(ParameterSetName = "FixedWithoutSource")]
	[Parameter(ParameterSetName = "DynamicWithoutSource")]
	public uint LogicalSectorSizeBytes { get; set; }

	[ValidateNotNullOrEmpty]
	[ValidateSet(new string[] { "512", "4096" })]
	[Parameter(ParameterSetName = "FixedWithoutSource")]
	[Parameter(ParameterSetName = "DynamicWithoutSource")]
	[Parameter(ParameterSetName = "Differencing")]
	public uint PhysicalSectorSizeBytes { get; set; }

	[ValidateSet(new string[] { "None", "BTT" })]
	[Parameter(Mandatory = false, ParameterSetName = "FixedWithoutSource")]
	public VirtualHardDiskPmemAddressAbstractionType AddressAbstractionType { get; set; }

	[ValidateSet(new string[] { "0", "1GB", "1073741824" })]
	[Parameter(Mandatory = false, ParameterSetName = "FixedWithoutSource")]
	public ulong DataAlignmentBytes { get; set; }

	[Parameter]
	public SwitchParameter AsJob { get; set; }

	internal override IList<VirtualHardDisk> CreateObjects(IOperationWatcher operationWatcher)
	{
		return ParameterResolvers.GetServers(this, operationWatcher).SelectManyWithLogging((Server server) => CreateVHDs(server, Path, operationWatcher), operationWatcher).ToList();
	}

	private IEnumerable<VirtualHardDisk> CreateVHDs(Server server, IEnumerable<string> paths, IOperationWatcher operationWatcher)
	{
		string parentPath = null;
		if (!string.IsNullOrEmpty(ParentPath))
		{
			parentPath = VhdPathResolver.GetSingleVirtualHardDiskFullPath(server, ParentPath, base.CurrentFileSystemLocation, base.InvokeProvider);
		}
		return paths.SelectWithLogging((string path) => CreateSingleVHD(server, path, parentPath, operationWatcher), operationWatcher);
	}

	private VirtualHardDisk CreateSingleVHD(Server server, string path, string parentPath, IOperationWatcher operationWatcher)
	{
		VirtualHardDisk result = null;
		bool pmemCompatible = false;
		if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_NewVHD, path)))
		{
			path = PathUtility.GetFullPath(path, base.CurrentFileSystemLocation);
			string extension = System.IO.Path.GetExtension(path);
			VhdFormat vhdFormat;
			if (extension.Equals(".VHD", StringComparison.OrdinalIgnoreCase))
			{
				vhdFormat = VhdFormat.VHD;
			}
			else if (extension.Equals(".VHDS", StringComparison.OrdinalIgnoreCase))
			{
				vhdFormat = VhdFormat.VHDSet;
			}
			else if (extension.Equals(".VHDPMEM", StringComparison.OrdinalIgnoreCase))
			{
				pmemCompatible = true;
				vhdFormat = VhdFormat.VHDX;
			}
			else
			{
				vhdFormat = VhdFormat.VHDX;
			}
			if (CurrentParameterSetIs("FixedWithSource") || CurrentParameterSetIs("DynamicWithSource"))
			{
				VhdType destinationType = (CurrentParameterSetIs("FixedWithSource") ? VhdType.Fixed : VhdType.Dynamic);
				string deviceIdByDiskNumber = VhdUtilities.GetDeviceIdByDiskNumber(server, SourceDisk);
				if (string.IsNullOrEmpty(deviceIdByDiskNumber))
				{
					throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.InvalidParameter_InvalidDiskNumberForDiskObject, path));
				}
				result = VhdUtilities.ConvertVirtualHardDisk(server, deviceIdByDiskNumber, path, null, destinationType, vhdFormat, BlockSizeBytes, isPmemCompatible: false, VirtualHardDiskPmemAddressAbstractionType.None, operationWatcher);
			}
			else if (CurrentParameterSetIs("FixedWithoutSource") || CurrentParameterSetIs("DynamicWithoutSource"))
			{
				VhdType destinationType = (CurrentParameterSetIs("FixedWithoutSource") ? VhdType.Fixed : VhdType.Dynamic);
				result = VhdUtilities.CreateVirtualHardDisk(server, destinationType, vhdFormat, path, null, (long)SizeBytes, BlockSizeBytes, LogicalSectorSizeBytes, PhysicalSectorSizeBytes, pmemCompatible, AddressAbstractionType, (long)DataAlignmentBytes, operationWatcher);
			}
			else
			{
				VhdType destinationType = VhdType.Differencing;
				result = VhdUtilities.CreateVirtualHardDisk(server, destinationType, vhdFormat, path, parentPath, (long)SizeBytes, BlockSizeBytes, 0L, PhysicalSectorSizeBytes, pmemCompatible: false, VirtualHardDiskPmemAddressAbstractionType.None, 0L, operationWatcher);
			}
		}
		return result;
	}
}
