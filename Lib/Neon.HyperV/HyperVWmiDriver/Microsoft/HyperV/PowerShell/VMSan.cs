using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.HyperV.PowerShell.Common;
using Microsoft.Management.Infrastructure;
using Microsoft.Virtualization.Client.Management;

using ErrorMessages = Microsoft.HyperV.PowerShell.Common.ErrorMessages;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMSan : VMResourcePool
{
	private const string gm_FibreChannelHbaNamespace = "root/Microsoft/Windows/Storage";

	private const string gm_FibreChannelHbaQueryFormat = "SELECT * FROM MSFT_InitiatorPort WHERE ConnectionType = 1 AND NodeAddress = '{0}' AND PortAddress = '{1}'";

	private static IReadOnlyCollection<string> gm_EmptyWwn = new ReadOnlyCollection<string>(new Collection<string>());

	private static readonly IEqualityComparer<object> gm_VirtualSwitchComparer = new VirtualizationManagementObjectEqualityComparer<IVirtualFcSwitch>();

	private static readonly Regex gm_WwnValidator = new Regex("^[a-f0-9]{16}$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

	internal static IReadOnlyCollection<string> EmptyWwns => gm_EmptyWwn;

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
	public CimInstance[] HBAs
	{
		get
		{
			CimInstance[] array = (from fcSwitch in ((IFibreChannelResourcePool)m_ResourcePool.GetData(UpdatePolicy.EnsureAssociatorsUpdated)).GetVirtualFcSwitches(updateRasdPropertyCache: true)
				select fcSwitch.GetExternalFcPort()).Select(GetInitiatorPort).ToArray();
			if (array.Length == 0)
			{
				return null;
			}
			return array;
		}
	}

	public string Note
	{
		get
		{
			return m_ResourcePoolSetting.GetData(UpdatePolicy.EnsureUpdated).Notes;
		}
		internal set
		{
			m_ResourcePoolSetting.GetData(UpdatePolicy.None).Notes = value;
		}
	}

	protected override IEqualityComparer<object> HostResourceComparer => gm_VirtualSwitchComparer;

	protected override string MissingResourcesError => ErrorMessages.VMSan_MissingResource;

	protected override string MissingResourcesFormat => "n";

	internal VMSan(IResourcePool resourcePool)
		: base(resourcePool)
	{
	}

	internal static bool IsValidWwn(string wwn)
	{
		return gm_WwnValidator.IsMatch(wwn);
	}

	internal static IEnumerable<VMSan> GetVMSans(Server server, IEnumerable<string> sanNames, bool allowWildcards)
	{
		VMResourcePoolType[] poolTypes = new VMResourcePoolType[1] { VMResourcePoolType.FibreChannelConnection };
		return (from pool in VMResourcePool.GetVMResourcePools(server, sanNames, allowWildcards, poolTypes)
			where !VMResourcePool.IsPrimordialPoolName(pool.Name)
			select pool).Cast<VMSan>();
	}

	internal static VMSan CreateVMSan(Server server, string sanName, string sanNotes, IEnumerable<string> nodeWwns, IEnumerable<string> portWwns, IOperationWatcher operationWatcher)
	{
		List<IVirtualFcSwitch> hostResources = GetVirtualFcSwitches(server, nodeWwns, portWwns, operationWatcher).ToList();
		return (VMSan)VMResourcePool.CreateVMResourcePool(server, sanName, VMResourcePoolType.FibreChannelConnection, sanNotes, null, hostResources, operationWatcher);
	}

	internal void ConnectHbas(IEnumerable<string> nodeWwns, IEnumerable<string> portWwns, IOperationWatcher operationWatcher)
	{
		IEnumerable<IVirtualFcSwitch> virtualFcSwitches = GetVirtualFcSwitches(base.Server, nodeWwns, portWwns, operationWatcher);
		AddHostResources(virtualFcSwitches, TaskDescriptions.ConnectVMSan, operationWatcher);
	}

	internal void DisconnectHbas(IEnumerable<string> nodeWwns, IEnumerable<string> portWwns, IOperationWatcher operationWatcher)
	{
		List<IVirtualFcSwitch> hostResourcesToRemove = GetVirtualFcSwitches(base.Server, nodeWwns, portWwns, operationWatcher).ToList();
		RemoveHostResources(hostResourcesToRemove, TaskDescriptions.DisconnectVMSan, operationWatcher);
	}

	internal void SetHbas(IEnumerable<string> nodeWwns, IEnumerable<string> portWwns, IOperationWatcher operationWatcher)
	{
		List<IVirtualFcSwitch> hostResources = GetVirtualFcSwitches(base.Server, nodeWwns, portWwns, operationWatcher).ToList();
		SetHostResources(hostResources, TaskDescriptions.SetVMSanHbas, operationWatcher);
	}

	protected override IEnumerable<object> GetHostResources(IResourcePoolAllocationSetting poolAllocationSetting)
	{
		IFcPoolAllocationSetting obj = (IFcPoolAllocationSetting)poolAllocationSetting;
		obj.UpdatePropertyCache(Constants.UpdateThreshold);
		return obj.GetSwitches();
	}

	protected override bool HasHostResource(IResourcePoolAllocationSetting poolAllocationSetting, object hostResource)
	{
		IVirtualFcSwitch virtualSwitchToCheck = (IVirtualFcSwitch)hostResource;
		IFcPoolAllocationSetting obj = (IFcPoolAllocationSetting)poolAllocationSetting;
		obj.UpdatePropertyCache(Constants.UpdateThreshold);
		return obj.GetSwitches().Any((IVirtualSwitch virtualSwitch) => virtualSwitch.ManagementPath == virtualSwitchToCheck.ManagementPath);
	}

	protected override void SetHostResourceInAllocationFromParentPool(IEnumerable<object> hostResources, IResourcePool parentPool, IResourcePoolAllocationSetting parentPoolAllocationSetting)
	{
		IEnumerable<IVirtualFcSwitch> enumerable2;
		if (hostResources == null)
		{
			IEnumerable<IVirtualFcSwitch> enumerable = new List<IVirtualFcSwitch>();
			enumerable2 = enumerable;
		}
		else
		{
			enumerable2 = hostResources.Cast<IVirtualFcSwitch>();
		}
		IEnumerable<IVirtualFcSwitch> source = enumerable2;
		IFibreChannelResourcePool fibreChannelResourcePool = (IFibreChannelResourcePool)parentPool;
		IFcPoolAllocationSetting fcPoolAllocationSetting = (IFcPoolAllocationSetting)parentPoolAllocationSetting;
		if (fibreChannelResourcePool.Primordial)
		{
			fcPoolAllocationSetting.SetSwitches(source.Cast<IVirtualSwitch>().ToList());
		}
		else
		{
			fcPoolAllocationSetting.SetSwitches(source.Where(fibreChannelResourcePool.HasSwitch).Cast<IVirtualSwitch>().ToList());
		}
	}

	private static IEnumerable<IVirtualFcSwitch> GetVirtualFcSwitches(Server server, IEnumerable<string> nodeWwns, IEnumerable<string> portWwns, IOperationWatcher operationWatcher)
	{
		IHostComputerSystem hostComputerSystem = ObjectLocator.GetHostComputerSystem(server);
		hostComputerSystem.UpdateAssociationCache(Constants.UpdateThreshold);
		Collection<Tuple<string, string>> missingWwNames = new Collection<Tuple<string, string>>();
		Dictionary<Tuple<string, string>, IExternalFcPort> externalPortsByWwNames = hostComputerSystem.ExternalFcPorts.ToDictionary((IExternalFcPort externalFcPort) => Tuple.Create(externalFcPort.WorldWideNodeName.ToUpperInvariant(), externalFcPort.WorldWidePortName.ToUpperInvariant()));
		ReadOnlyCollection<IVirtualFcSwitch> result = nodeWwns.Zip(portWwns, (string nodeName, string portName) => LookupVirtualFcSwitch(externalPortsByWwNames, nodeName, portName, missingWwNames)).ToList().AsReadOnly();
		int num = missingWwNames.Count;
		if (num != 0)
		{
			foreach (Tuple<string, string> item in missingWwNames)
			{
				VirtualizationException ex = ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, ErrorMessages.VMSan_InitiatorPortNotFound, item.Item1, item.Item2));
				if (--num == 0)
				{
					throw ex;
				}
				operationWatcher.WriteError(ExceptionHelper.GetErrorRecordFromException(ex));
			}
			return result;
		}
		return result;
	}

	private static IVirtualFcSwitch LookupVirtualFcSwitch(IReadOnlyDictionary<Tuple<string, string>, IExternalFcPort> externalPortsByWwNames, string nodeName, string portName, ICollection<Tuple<string, string>> missingWwNames)
	{
		IExternalFcPort value = null;
		Tuple<string, string> key = Tuple.Create(nodeName.ToUpperInvariant(), portName.ToUpperInvariant());
		if (!externalPortsByWwNames.TryGetValue(key, out value))
		{
			missingWwNames.Add(Tuple.Create(nodeName, portName));
		}
		return value?.GetVirtualFcSwitch();
	}

	private CimInstance GetInitiatorPort(IExternalFcPort externalFcPort)
	{
		string query = string.Format(CultureInfo.InvariantCulture, "SELECT * FROM MSFT_InitiatorPort WHERE ConnectionType = 1 AND NodeAddress = '{0}' AND PortAddress = '{1}'", externalFcPort.WorldWideNodeName, externalFcPort.WorldWidePortName);
		return base.Server.QueryInstances("root/Microsoft/Windows/Storage", query).SingleOrDefault()?.Instance;
	}
}
