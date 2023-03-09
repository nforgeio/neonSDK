using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.HyperV.PowerShell.Common;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMRemoteFXPhysicalVideoAdapter : VirtualizationObject
{
	private readonly DataUpdater<IVM3dVideoPhysical3dGPU> m_PhysicalAdapter;

	[VirtualizationObjectIdentifier(IdentifierFlags.UniqueIdentifier)]
	public string Id => m_PhysicalAdapter.GetData(UpdatePolicy.EnsureUpdated).InstanceID;

	[VirtualizationObjectIdentifier(IdentifierFlags.FriendlyName)]
	public string Name => m_PhysicalAdapter.GetData(UpdatePolicy.EnsureUpdated).Name;

	public string GPUID => m_PhysicalAdapter.GetData(UpdatePolicy.EnsureUpdated).GPUID;

	public long TotalVideoMemory => m_PhysicalAdapter.GetData(UpdatePolicy.EnsureUpdated).TotalVideoMemory;

	public long AvailableVideoMemory => m_PhysicalAdapter.GetData(UpdatePolicy.EnsureUpdated).AvailableVideoMemory;

	public long DedicatedSystemMemory => m_PhysicalAdapter.GetData(UpdatePolicy.EnsureUpdated).DedicatedSystemMemory;

	public long DedicatedVideoMemory => m_PhysicalAdapter.GetData(UpdatePolicy.EnsureUpdated).DedicatedVideoMemory;

	public long SharedSystemMemory => m_PhysicalAdapter.GetData(UpdatePolicy.EnsureUpdated).SharedSystemMemory;

	public bool Enabled => m_PhysicalAdapter.GetData(UpdatePolicy.EnsureUpdated).EnabledForVirtualization;

	public bool CompatibleForVirtualization => m_PhysicalAdapter.GetData(UpdatePolicy.EnsureUpdated).CompatibleForVirtualization;

	public string DirectXVersion => m_PhysicalAdapter.GetData(UpdatePolicy.EnsureUpdated).DirectXVersion;

	public string PixelShaderVersion => m_PhysicalAdapter.GetData(UpdatePolicy.EnsureUpdated).PixelShaderVersion;

	public string DriverProvider => m_PhysicalAdapter.GetData(UpdatePolicy.EnsureUpdated).DriverProvider;

	public string DriverDate => m_PhysicalAdapter.GetData(UpdatePolicy.EnsureUpdated).DriverDate.ToString("u", CultureInfo.CurrentCulture);

	public string DriverInstalledDate => m_PhysicalAdapter.GetData(UpdatePolicy.EnsureUpdated).DriverDate.ToString("u", CultureInfo.CurrentCulture);

	public string DriverVersion => m_PhysicalAdapter.GetData(UpdatePolicy.EnsureUpdated).DriverVersion;

	public string DriverModelVersion => m_PhysicalAdapter.GetData(UpdatePolicy.EnsureUpdated).DriverModelVersion;

	private VMRemoteFXPhysicalVideoAdapter(IVM3dVideoPhysical3dGPU physicalAdapter)
		: base(physicalAdapter)
	{
		m_PhysicalAdapter = InitializePrimaryDataUpdater(physicalAdapter);
	}

	internal void SetGPUEnabledForVirtualizationState(bool enabled, IOperationWatcher operationWatcher)
	{
		IVMSynthetic3DService synthetic3DService = ObjectLocator.GetSynthetic3DService(base.Server);
		Func<IVM3dVideoPhysical3dGPU, IVMTask> beginMethod;
		Action<IVMTask> endTaskMethod;
		if (enabled)
		{
			beginMethod = synthetic3DService.BeginEnableGPUForVirtualization;
			endTaskMethod = synthetic3DService.EndEnableGPUForVirtualization;
		}
		else
		{
			beginMethod = synthetic3DService.BeginDisableGPUForVirtualization;
			endTaskMethod = synthetic3DService.EndDisableGPUForVirtualization;
		}
		operationWatcher.PerformOperation(() => beginMethod(m_PhysicalAdapter.GetData(UpdatePolicy.None)), endTaskMethod, TaskDescriptions.SetVMRemoteFXPhysicalVideoAdapter, this);
		m_PhysicalAdapter.GetData(UpdatePolicy.None).InvalidatePropertyCache();
	}

	private static IEnumerable<VMRemoteFXPhysicalVideoAdapter> GetVmRemoteFxPhysicalVideoAdapters(IEnumerable<Server> servers)
	{
		List<VMRemoteFXPhysicalVideoAdapter> list = new List<VMRemoteFXPhysicalVideoAdapter>();
		foreach (Server server in servers)
		{
			IList<IVM3dVideoPhysical3dGPU> vm3DVideoPhysical3dGpus = ObjectLocator.GetVm3DVideoPhysical3dGpus(server);
			list.AddRange(vm3DVideoPhysical3dGpus.Select((IVM3dVideoPhysical3dGPU gpu) => new VMRemoteFXPhysicalVideoAdapter(gpu)));
		}
		return list;
	}

	internal static IEnumerable<VMRemoteFXPhysicalVideoAdapter> GetVmRemoteFxPhysicalVideoAdapters(IEnumerable<Server> servers, string[] gpuNames)
	{
		IEnumerable<VMRemoteFXPhysicalVideoAdapter> enumerable = GetVmRemoteFxPhysicalVideoAdapters(servers);
		if (!gpuNames.IsNullOrEmpty())
		{
			WildcardPatternMatcher matcher = new WildcardPatternMatcher(gpuNames);
			enumerable = enumerable.Where((VMRemoteFXPhysicalVideoAdapter gpu) => matcher.MatchesAny(gpu.Name));
		}
		return enumerable;
	}
}
