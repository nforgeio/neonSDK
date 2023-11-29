using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using Microsoft.HyperV.PowerShell.Common;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Virtualization.Client.Management;

using ErrorMessages = Microsoft.HyperV.PowerShell.Common.ErrorMessages;

namespace Microsoft.HyperV.PowerShell;

internal abstract class VMResourcePool : VirtualizationObject, IVMResourcePool, IMeasurableInternal, IRemovable, IUpdatable
{
    private class PoolTypeInfo
    {
        internal VMResourcePoolType PoolType { get; private set; }

        internal ConstructorInfo Constructor { get; private set; }

        internal PoolTypeInfo(VMResourcePoolType poolType, Type systemType)
        {
            PoolType = poolType;
            ConstructorInfo[] constructors = systemType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic);
            Constructor = constructors.Where((ConstructorInfo c) => c.GetParameters().Count() == 1 && c.GetParameters().Single().ParameterType == typeof(IResourcePool)).FirstOrDefault();
        }
    }

    protected class PathEqualityComparer : IEqualityComparer<object>
    {
        bool IEqualityComparer<object>.Equals(object first, object second)
        {
            string a = (string)first;
            string b = (string)second;
            return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
        }

        int IEqualityComparer<object>.GetHashCode(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            if (!(obj is string text))
            {
                return obj.GetHashCode();
            }
            return text.ToUpperInvariant().GetHashCode();
        }
    }

    protected class VirtualizationManagementObjectEqualityComparer<TVirtManObj> : IEqualityComparer<object> where TVirtManObj : IVirtualizationManagementObject
    {
        private static readonly global::Microsoft.Virtualization.Client.Management.VirtualizationManagementObjectEqualityComparer<TVirtManObj> gm_EqualityComparer = new global::Microsoft.Virtualization.Client.Management.VirtualizationManagementObjectEqualityComparer<TVirtManObj>();

        bool IEqualityComparer<object>.Equals(object firstObj, object secondObj)
        {
            TVirtManObj first = (TVirtManObj)firstObj;
            TVirtManObj second = (TVirtManObj)secondObj;
            return gm_EqualityComparer.Equals(first, second);
        }

        int IEqualityComparer<object>.GetHashCode(object obj)
        {
            TVirtManObj obj2 = (TVirtManObj)obj;
            return gm_EqualityComparer.GetHashCode(obj2);
        }
    }

    internal const string PrimordialPoolDisplayName = "Primordial";

    private static readonly IReadOnlyDictionary<VMDeviceSettingType, PoolTypeInfo> gm_VirtManToCmdletPoolType;

    private static readonly IReadOnlyDictionary<VMResourcePoolType, VMDeviceSettingType> gm_CmdletToVirtManPoolType;

    internal readonly IDataUpdater<IResourcePool> m_ResourcePool;

    internal readonly IDataUpdater<IResourcePoolSetting> m_ResourcePoolSetting;

    [VirtualizationObjectIdentifier(IdentifierFlags.UniqueIdentifier | IdentifierFlags.FriendlyName)]
    public string Name
    {
        get
        {
            return PoolIdToDisplayName(m_ResourcePool.GetData(UpdatePolicy.EnsureUpdated).PoolId);
        }
        internal set
        {
            m_ResourcePoolSetting.GetData(UpdatePolicy.None).PoolId = DisplayNameToPoolId(value);
        }
    }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
    public string[] ParentName
    {
        get
        {
            List<string> list = null;
            foreach (IResourcePool parentPool in m_ResourcePool.GetData(UpdatePolicy.EnsureAssociatorsUpdated).ParentPools)
            {
                if (list == null)
                {
                    list = new List<string>();
                }
                list.Add(PoolIdToDisplayName(parentPool.PoolId));
            }
            return list?.ToArray();
        }
    }

    public bool ResourceMeteringEnabled => m_ResourcePool.GetData(UpdatePolicy.EnsureAssociatorsUpdated).AggregateMetricEnabledState != MetricEnabledState.Disabled;

    public VMResourcePoolType ResourcePoolType => MapVirtManToCmdletPoolTypeInfo(m_ResourcePool.GetData(UpdatePolicy.EnsureUpdated).DeviceSettingType).PoolType;

    protected virtual IEqualityComparer<object> HostResourceComparer => EqualityComparer<object>.Default;

    protected virtual string MissingResourcesError => ErrorMessages.VMResourcePool_MissingResource;

    protected virtual string MissingResourcesFormat => "C";

    static VMResourcePool()
    {
        gm_VirtManToCmdletPoolType = new Dictionary<VMDeviceSettingType, PoolTypeInfo>
        {
            {
                VMDeviceSettingType.Memory,
                new PoolTypeInfo(VMResourcePoolType.Memory, typeof(VMMemoryResourcePool))
            },
            {
                VMDeviceSettingType.Processor,
                new PoolTypeInfo(VMResourcePoolType.Processor, typeof(VMProcessorResourcePool))
            },
            {
                VMDeviceSettingType.EthernetConnection,
                new PoolTypeInfo(VMResourcePoolType.Ethernet, typeof(VMEthernetResourcePool))
            },
            {
                VMDeviceSettingType.HardDisk,
                new PoolTypeInfo(VMResourcePoolType.VHD, typeof(VMVhdResourcePool))
            },
            {
                VMDeviceSettingType.IsoDisk,
                new PoolTypeInfo(VMResourcePoolType.ISO, typeof(VMIsoResourcePool))
            },
            {
                VMDeviceSettingType.FloppyDisk,
                new PoolTypeInfo(VMResourcePoolType.VFD, typeof(VMVfdResourcePool))
            },
            {
                VMDeviceSettingType.FibreChannelPort,
                new PoolTypeInfo(VMResourcePoolType.FibreChannelPort, typeof(VMFcResourcePool))
            },
            {
                VMDeviceSettingType.FibreChannelConnection,
                new PoolTypeInfo(VMResourcePoolType.FibreChannelConnection, typeof(VMSan))
            },
            {
                VMDeviceSettingType.PciExpress,
                new PoolTypeInfo(VMResourcePoolType.PciExpress, typeof(VMPciExpressResourcePool))
            }
        };
        Dictionary<VMResourcePoolType, VMDeviceSettingType> dictionary = new Dictionary<VMResourcePoolType, VMDeviceSettingType>();
        foreach (KeyValuePair<VMDeviceSettingType, PoolTypeInfo> item in gm_VirtManToCmdletPoolType)
        {
            dictionary.Add(item.Value.PoolType, item.Key);
        }
        gm_CmdletToVirtManPoolType = dictionary;
    }

    internal static bool IsPrimordialPoolName(string poolName)
    {
        if (!string.IsNullOrEmpty(poolName))
        {
            return string.Equals(poolName, "Primordial", StringComparison.OrdinalIgnoreCase);
        }
        return true;
    }

    internal static IEnumerable<VMResourcePool> GetVMResourcePools(Server server, IEnumerable<string> poolNames, bool allowWildcards, IEnumerable<VMResourcePoolType> poolTypes)
    {
        List<string> list = poolNames?.Select(DisplayNameToPoolId).ToList();
        IEnumerable<VMDeviceSettingType> types = poolTypes?.Select(MapCmdletToVirtManPoolType);
        IEnumerable<IResourcePool> source = ((allowWildcards && list != null && list.Count != 0 && list.Any(WildcardPattern.ContainsWildcardCharacters)) ? ObjectLocator.GetResourcePoolsByNamesAndTypes(server, list, allowWildcards: true, types) : ObjectLocator.GetResourcePoolsByNamesAndTypes(server, list, allowWildcards: false, types));
        return source.Where(IsSupportedVirtManPoolType).Select(NewVMResourcePool);
    }

    internal static VMResourcePool CreateVMResourcePool<THostResourceType>(Server server, string poolName, VMResourcePoolType poolType, string poolNotes, IEnumerable<string> parentNames, IReadOnlyCollection<THostResourceType> hostResources, IOperationWatcher operationWatcher) where THostResourceType : class
    {
        string poolId = DisplayNameToPoolId(poolName);
        VMDeviceSettingType vMDeviceSettingType = MapCmdletToVirtManPoolType(poolType);
        List<string> names = ((parentNames != null) ? parentNames.Select(DisplayNameToPoolId).ToList() : new List<string> { string.Empty });
        VMResourcePool vMResourcePool = NewVMResourcePool(ObjectLocator.GetHostComputerSystem(server).GetPrimordialResourcePool(vMDeviceSettingType));
        IList<IResourcePool> newParentPools = ObjectLocator.GetResourcePoolsByNamesAndTypes(server, names, allowWildcards: false, new VMDeviceSettingType[1] { vMDeviceSettingType });
        if (newParentPools.Count == 0)
        {
            throw ExceptionHelper.CreateObjectNotFoundException(ErrorMessages.VMResourcePool_ParentPoolNotFound, null);
        }
        IResourcePoolConfigurationService poolService = ObjectLocator.GetResourcePoolConfigurationService(server);
        List<IResourcePoolAllocationSetting> newAllocationSettings = new List<IResourcePoolAllocationSetting>();
        foreach (IResourcePool item in newParentPools)
        {
            IResourcePoolAllocationSetting resourcePoolAllocationSetting = poolService.CreateTemplateAllocationSetting(item.PoolId, item.DeviceSettingType);
            vMResourcePool.SetHostResourceInAllocationFromParentPool(hostResources, item, resourcePoolAllocationSetting);
            newAllocationSettings.Add(resourcePoolAllocationSetting);
        }
        vMResourcePool.EnsureNoMissingResources(hostResources, newAllocationSettings, operationWatcher);
        IResourcePoolSetting poolSettings = poolService.CreateTemplatePoolSetting(poolId, vMDeviceSettingType);
        if (!string.IsNullOrEmpty(poolNotes))
        {
            poolSettings.Notes = poolNotes;
        }
        return NewVMResourcePool(operationWatcher.PerformOperationWithReturn(() => poolService.BeginCreateResourcePool(poolSettings, newParentPools.ToArray(), newAllocationSettings.ToArray()), poolService.EndCreateResourcePool, TaskDescriptions.NewVMResourcePool, null));
    }

    internal VMResourcePool(IResourcePool resourcePool)
        : base(resourcePool)
    {
        m_ResourcePool = InitializePrimaryDataUpdater(resourcePool);
        m_ResourcePoolSetting = new DataUpdater<IResourcePoolSetting>(resourcePool.Setting);
    }

    void IRemovable.Remove(IOperationWatcher operationWatcher)
    {
        operationWatcher.PerformDelete(m_ResourcePool.GetData(UpdatePolicy.None), TaskDescriptions.RemoveVMResourcePool, this);
        base.IsDeleted = true;
    }

    void IUpdatable.Put(IOperationWatcher operationWatcher)
    {
        operationWatcher.PerformPut(m_ResourcePoolSetting.GetData(UpdatePolicy.EnsureUpdated), TaskDescriptions.RenameVMResourcePool, this);
        m_ResourcePool.GetData(UpdatePolicy.None).InvalidatePropertyCache();
    }

    IMetricMeasurableElement IMeasurableInternal.GetMeasurableElement(UpdatePolicy policy)
    {
        return m_ResourcePool.GetData(policy);
    }

    IEnumerable<IMetricValue> IMeasurableInternal.GetMetricValues()
    {
        return m_ResourcePool.GetData(UpdatePolicy.EnsureAssociatorsUpdated).GetMetricValues();
    }

    internal IResourcePool GetResourcePool(UpdatePolicy policy)
    {
        return m_ResourcePool.GetData(policy);
    }

    internal void SetParentNames(string[] parentNames, IOperationWatcher operationWatcher)
    {
        if (parentNames == null)
        {
            throw new ArgumentNullException("parentNames");
        }
        IResourcePoolConfigurationService poolService = ObjectLocator.GetResourcePoolConfigurationService(base.Server);
        List<object> hostResources = GetHostResources().ToList();
        IEnumerable<string> names = parentNames.Select(DisplayNameToPoolId);
        bool flag = parentNames.Any(WildcardPattern.ContainsWildcardCharacters);
        IList<IResourcePool> newParentPools = ObjectLocator.GetResourcePoolsByNamesAndTypes(base.Server, names, flag, new VMDeviceSettingType[1] { MapCmdletToVirtManPoolType(ResourcePoolType) });
        if (newParentPools.Count == 0 || (!flag && newParentPools.Count != parentNames.Length))
        {
            throw ExceptionHelper.CreateObjectNotFoundException(ErrorMessages.VMResourcePool_ParentPoolNotFound, null);
        }
        IResourcePool pool = m_ResourcePool.GetData(UpdatePolicy.EnsureAssociatorsUpdated);
        List<IResourcePoolAllocationSetting> newAllocationSettings = new List<IResourcePoolAllocationSetting>();
        foreach (IResourcePool parentPool in newParentPools)
        {
            IResourcePoolAllocationSetting resourcePoolAllocationSetting = pool.AllocationSettings.SingleOrDefault((IResourcePoolAllocationSetting a) => string.Equals(a.PoolId, parentPool.PoolId, StringComparison.OrdinalIgnoreCase));
            if (resourcePoolAllocationSetting == null)
            {
                resourcePoolAllocationSetting = poolService.CreateTemplateAllocationSetting(parentPool.PoolId, parentPool.DeviceSettingType);
                SetHostResourceInAllocationFromParentPool(hostResources, parentPool, resourcePoolAllocationSetting);
            }
            newAllocationSettings.Add(resourcePoolAllocationSetting);
        }
        EnsureNoMissingResources(hostResources, newAllocationSettings, operationWatcher);
        operationWatcher.PerformOperation(() => poolService.BeginModifyResourcePool(pool, newParentPools.ToArray(), newAllocationSettings.ToArray()), poolService.EndModifyResourcePool, TaskDescriptions.SetVMResourcePool_SetParentNames, this);
        m_ResourcePool.GetData(UpdatePolicy.None).InvalidateAssociationCache();
        m_ResourcePoolSetting.GetData(UpdatePolicy.None).InvalidateAssociationCache();
    }

    protected static VMResourcePool NewVMResourcePool(IResourcePool pool)
    {
        return (VMResourcePool)MapVirtManToCmdletPoolTypeInfo(pool.DeviceSettingType).Constructor.Invoke(new object[1] { pool });
    }

    protected IEnumerable<object> GetHostResources()
    {
        return m_ResourcePool.GetData(UpdatePolicy.EnsureAssociatorsUpdated).AllocationSettings.SelectMany(GetHostResources);
    }

    protected virtual IEnumerable<object> GetHostResources(IResourcePoolAllocationSetting poolAllocationSetting)
    {
        return Enumerable.Empty<object>();
    }

    protected virtual bool HasHostResource(IResourcePoolAllocationSetting poolAllocationSetting, object hostResource)
    {
        return true;
    }

    internal void AddHostResources(IEnumerable<object> hostResourcesToAdd, string taskDescription, IOperationWatcher operationWatcher)
    {
        ReadOnlyCollection<object> hostResources = GetHostResources().Concat(hostResourcesToAdd).ToList().AsReadOnly();
        SetHostResources(hostResources, taskDescription, operationWatcher);
    }

    internal void RemoveHostResources(IReadOnlyList<object> hostResourcesToRemove, string taskDescription, IOperationWatcher operationWatcher)
    {
        List<object> hostResources = (from hostResource in GetHostResources()
            where !hostResourcesToRemove.Contains(hostResource, HostResourceComparer)
            select hostResource).ToList();
        SetHostResources(hostResources, taskDescription, operationWatcher);
    }

    protected virtual void SetHostResourceInAllocationFromParentPool(IEnumerable<object> hostResources, IResourcePool parentPool, IResourcePoolAllocationSetting parentPoolAllocationSetting)
    {
    }

    private static string PoolIdToDisplayName(string poolId)
    {
        if (!IsPrimordialPoolName(poolId))
        {
            return poolId;
        }
        return "Primordial";
    }

    private static string DisplayNameToPoolId(string displayName)
    {
        if (!IsPrimordialPoolName(displayName))
        {
            return displayName;
        }
        return string.Empty;
    }

    private static VMDeviceSettingType MapCmdletToVirtManPoolType(VMResourcePoolType cmdletPoolType)
    {
        return gm_CmdletToVirtManPoolType[cmdletPoolType];
    }

    private static PoolTypeInfo MapVirtManToCmdletPoolTypeInfo(VMDeviceSettingType virtManType)
    {
        return gm_VirtManToCmdletPoolType[virtManType];
    }

    private static bool IsSupportedVirtManPoolType(IResourcePool pool)
    {
        return gm_VirtManToCmdletPoolType.ContainsKey(pool.DeviceSettingType);
    }

    internal void SetHostResources(IReadOnlyCollection<object> hostResources, string taskDescription, IOperationWatcher operationWatcher)
    {
        IResourcePool pool = m_ResourcePool.GetData(UpdatePolicy.EnsureAssociatorsUpdated);
        List<IResourcePoolAllocationSetting> newAllocationSettings = new List<IResourcePoolAllocationSetting>();
        List<IResourcePool> parentPools = new List<IResourcePool>();
        foreach (IResourcePoolAllocationSetting allocationSetting in pool.AllocationSettings)
        {
            IResourcePool parentResourcePool = allocationSetting.ParentResourcePool;
            SetHostResourceInAllocationFromParentPool(hostResources, parentResourcePool, allocationSetting);
            newAllocationSettings.Add(allocationSetting);
            parentPools.Add(parentResourcePool);
        }
        EnsureNoMissingResources(hostResources, newAllocationSettings, operationWatcher);
        IResourcePoolConfigurationService poolService = ObjectLocator.GetResourcePoolConfigurationService(base.Server);
        operationWatcher.PerformOperation(() => poolService.BeginModifyResourcePool(pool, parentPools.ToArray(), newAllocationSettings.ToArray()), poolService.EndModifyResourcePool, taskDescription, this);
        pool.InvalidateAssociationCache();
    }

    private void EnsureNoMissingResources(IEnumerable<object> hostResources, IEnumerable<IResourcePoolAllocationSetting> allocationSettings, IOperationWatcher operationWatcher)
    {
        if (hostResources == null)
        {
            return;
        }
        ReadOnlyCollection<object> readOnlyCollection = hostResources.Where((object r) => !allocationSettings.Any((IResourcePoolAllocationSetting a) => HasHostResource(a, r))).ToList().AsReadOnly();
        int num = readOnlyCollection.Count();
        if (num == 0)
        {
            return;
        }
        foreach (object item in readOnlyCollection)
        {
            string arg = ((item is VirtualizationObject virtualizationObject) ? virtualizationObject.ToString(MissingResourcesFormat) : item.ToString());
            VirtualizationException ex = ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, MissingResourcesError, arg));
            if (--num == 0)
            {
                throw ex;
            }
            operationWatcher.WriteError(ExceptionHelper.GetErrorRecordFromException(ex));
        }
    }
}
