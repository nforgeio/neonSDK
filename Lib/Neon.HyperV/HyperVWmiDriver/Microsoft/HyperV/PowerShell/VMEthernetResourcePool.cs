using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.HyperV.PowerShell.Common;
using Microsoft.Virtualization.Client.Management;

using ErrorMessages = Microsoft.HyperV.PowerShell.Common.ErrorMessages;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMEthernetResourcePool : VMResourcePool, IMeasurableResourcePool, IMeasurable, IMeasurableInternal, IVMResourcePool
{
    private static readonly IEqualityComparer<object> gm_VirtualSwitchComparer = new VirtualizationManagementObjectEqualityComparer<IVirtualEthernetSwitch>();

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
    public VMSwitch[] VMSwitches => (from s in ((IEthernetConnectionResourcePool)m_ResourcePool.GetData(UpdatePolicy.EnsureAssociatorsUpdated)).GetSwitches()
        select new VMSwitch(s.Setting)).ToArray();

    protected override IEqualityComparer<object> HostResourceComparer => gm_VirtualSwitchComparer;

    protected override string MissingResourcesError => ErrorMessages.VMEthernetPool_MissingResource;

    internal VMEthernetResourcePool(IResourcePool resourcePool)
        : base(resourcePool)
    {
    }

    internal IReadOnlyList<VMSwitch> GetSwitchesByNames(string[] requestedSwitchNames, bool allowWildcards, ErrorDisplayMode errorDisplayMode, IOperationWatcher operationWatcher)
    {
        IEnumerable<VMSwitch> source = VMSwitches;
        if (requestedSwitchNames != null)
        {
            if (allowWildcards)
            {
                WildcardPatternMatcher matcher = new WildcardPatternMatcher(requestedSwitchNames);
                source = source.Where((VMSwitch virtualSwitch) => matcher.MatchesAny(virtualSwitch.Name));
            }
            else
            {
                source = source.Where((VMSwitch virtualSwitch) => requestedSwitchNames.Contains(virtualSwitch.Name, StringComparer.OrdinalIgnoreCase));
            }
        }
        IReadOnlyList<VMSwitch> readOnlyList = source.ToList().AsReadOnly();
        if (requestedSwitchNames != null)
        {
            VirtualizationObjectLocator.WriteNonMatchingNameErrors(requestedSwitchNames, readOnlyList.Select((VMSwitch virtualSwitch) => virtualSwitch.Name), allowWildcards, ErrorMessages.VMSwitch_NotFoundInPool, errorDisplayMode, operationWatcher);
        }
        return readOnlyList;
    }

    internal void AddSwitches(IEnumerable<VMSwitch> switchesToAdd, IOperationWatcher operationWatcher)
    {
        AddHostResources(switchesToAdd.Select((VMSwitch sw) => sw.VirtualizationManagementSwitch), TaskDescriptions.AddVMSwitch, operationWatcher);
    }

    internal void RemoveSwitches(IEnumerable<VMSwitch> switchesToRemove, IOperationWatcher operationWatcher)
    {
        RemoveHostResources(switchesToRemove.Select((VMSwitch sw) => sw.VirtualizationManagementSwitch).ToList().AsReadOnly(), TaskDescriptions.RemoveVMSwitch_FromResourcePool, operationWatcher);
    }

    protected override IEnumerable<object> GetHostResources(IResourcePoolAllocationSetting poolAllocationSetting)
    {
        IGsmPoolAllocationSetting obj = (IGsmPoolAllocationSetting)poolAllocationSetting;
        obj.UpdatePropertyCache(Constants.UpdateThreshold);
        return obj.GetSwitches();
    }

    protected override bool HasHostResource(IResourcePoolAllocationSetting poolAllocationSetting, object hostResource)
    {
        IVirtualEthernetSwitch virtualSwitchToCheck = (IVirtualEthernetSwitch)hostResource;
        IGsmPoolAllocationSetting obj = (IGsmPoolAllocationSetting)poolAllocationSetting;
        obj.UpdatePropertyCache(Constants.UpdateThreshold);
        return obj.GetSwitches().Any((IVirtualSwitch virtualSwitch) => virtualSwitch.ManagementPath == virtualSwitchToCheck.ManagementPath);
    }

    protected override void SetHostResourceInAllocationFromParentPool(IEnumerable<object> hostResources, IResourcePool parentPool, IResourcePoolAllocationSetting parentPoolAllocationSetting)
    {
        IEnumerable<IVirtualEthernetSwitch> enumerable2;
        if (hostResources == null)
        {
            IEnumerable<IVirtualEthernetSwitch> enumerable = new List<IVirtualEthernetSwitch>();
            enumerable2 = enumerable;
        }
        else
        {
            enumerable2 = hostResources.Cast<IVirtualEthernetSwitch>();
        }
        IEnumerable<IVirtualEthernetSwitch> source = enumerable2;
        IEthernetConnectionResourcePool ethernetConnectionResourcePool = (IEthernetConnectionResourcePool)parentPool;
        IGsmPoolAllocationSetting gsmPoolAllocationSetting = (IGsmPoolAllocationSetting)parentPoolAllocationSetting;
        if (ethernetConnectionResourcePool.Primordial)
        {
            gsmPoolAllocationSetting.SetSwitches(source.Cast<IVirtualSwitch>().ToList());
        }
        else
        {
            gsmPoolAllocationSetting.SetSwitches(source.Where(ethernetConnectionResourcePool.HasSwitch).Cast<IVirtualSwitch>().ToList());
        }
    }
}
