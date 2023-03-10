using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("New", "VMResourcePool", SupportsShouldProcess = true)]
[OutputType(new Type[] { typeof(VMResourcePool) })]
internal sealed class NewVMResourcePool : VirtualizationCreationCmdlet<VMResourcePool>
{
	[ValidateNotNullOrEmpty]
	[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true)]
	public string Name { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter(Mandatory = true, Position = 1)]
	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Arrays are more user friendly, and this is by spec.")]
	public VMResourcePoolType[] ResourcePoolType { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter(Position = 2)]
	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Arrays are more user friendly, and this is by spec.")]
	public string[] ParentName { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter(Position = 3)]
	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Arrays are more user friendly, and this is by spec.")]
	public string[] Paths { get; set; }

	protected override void NormalizeParameters()
	{
		base.NormalizeParameters();
		if (ParentName == null || ParentName.Length == 0)
		{
			ParentName = new string[1] { "Primordial" };
		}
	}

	protected override void ValidateParameters()
	{
		base.ValidateParameters();
		bool num = ResourcePoolType.Intersect(VMStorageResourcePool.Types).Any();
		bool flag = ResourcePoolType.Except(VMStorageResourcePool.Types).Any();
		if (num)
		{
			if (Paths == null || Paths.Length == 0)
			{
				throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.InvalidParameter_StoragePoolMustBeCreatedWithPath);
			}
			if (flag)
			{
				throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.InvalidParameter_StoragePoolMustBeCreatedAlone);
			}
		}
		if (flag && Paths != null)
		{
			throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.InvalidParameter_OnlyStoragePoolCanBeCreatedWithPaths);
		}
	}

	internal override IList<VMResourcePool> CreateObjects(IOperationWatcher operationWatcher)
	{
		return ParameterResolvers.GetServers(this, operationWatcher).SelectManyWithLogging((Server server) => CreateVMResourcePools(server, operationWatcher), operationWatcher).ToList();
	}

	private IEnumerable<VMResourcePool> CreateVMResourcePools(Server server, IOperationWatcher operationWatcher)
	{
		return ResourcePoolType.SelectWithLogging((VMResourcePoolType poolType) => CreateVMResourcePool(server, poolType, operationWatcher), operationWatcher);
	}

	private VMResourcePool CreateVMResourcePool(Server server, VMResourcePoolType poolType, IOperationWatcher operationWatcher)
	{
		if (!operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_NewVMResourcePool, Name, poolType)))
		{
			return null;
		}
		return VMResourcePool.CreateVMResourcePool(server, Name, poolType, null, ParentName, (IReadOnlyCollection<string>)(object)Paths, operationWatcher);
	}
}
