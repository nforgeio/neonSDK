using System.Collections.Generic;
using System.Linq;
using Microsoft.HyperV.PowerShell.Common;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMPartitionableGpu : VirtualizationObject, IUpdatable
{
    private readonly DataUpdater<IPartitionableGpu> m_PartitionableGpu;

    [VirtualizationObjectIdentifier(IdentifierFlags.UniqueIdentifier)]
    public string Name => m_PartitionableGpu.GetData(UpdatePolicy.EnsureUpdated).Name;

    public IList<ushort> ValidPartitionCounts => m_PartitionableGpu.GetData(UpdatePolicy.EnsureUpdated).ValidPartitionCounts;

    public ushort PartitionCount
    {
        get
        {
            return m_PartitionableGpu.GetData(UpdatePolicy.EnsureUpdated).PartitionCount;
        }
        internal set
        {
            m_PartitionableGpu.GetData(UpdatePolicy.None).PartitionCount = value;
        }
    }

    public ulong TotalVRAM => m_PartitionableGpu.GetData(UpdatePolicy.EnsureUpdated).TotalVRAM;

    public ulong AvailableVRAM => m_PartitionableGpu.GetData(UpdatePolicy.EnsureUpdated).AvailableVRAM;

    public ulong MinPartitionVRAM => m_PartitionableGpu.GetData(UpdatePolicy.EnsureUpdated).MinPartitionVRAM;

    public ulong MaxPartitionVRAM => m_PartitionableGpu.GetData(UpdatePolicy.EnsureUpdated).MaxPartitionVRAM;

    public ulong OptimalPartitionVRAM => m_PartitionableGpu.GetData(UpdatePolicy.EnsureUpdated).OptimalPartitionVRAM;

    public ulong TotalEncode => m_PartitionableGpu.GetData(UpdatePolicy.EnsureUpdated).TotalEncode;

    public ulong AvailableEncode => m_PartitionableGpu.GetData(UpdatePolicy.EnsureUpdated).AvailableEncode;

    public ulong MinPartitionEncode => m_PartitionableGpu.GetData(UpdatePolicy.EnsureUpdated).MinPartitionEncode;

    public ulong MaxPartitionEncode => m_PartitionableGpu.GetData(UpdatePolicy.EnsureUpdated).MaxPartitionEncode;

    public ulong OptimalPartitionEncode => m_PartitionableGpu.GetData(UpdatePolicy.EnsureUpdated).OptimalPartitionEncode;

    public ulong TotalDecode => m_PartitionableGpu.GetData(UpdatePolicy.EnsureUpdated).TotalDecode;

    public ulong AvailableDecode => m_PartitionableGpu.GetData(UpdatePolicy.EnsureUpdated).AvailableDecode;

    public ulong MinPartitionDecode => m_PartitionableGpu.GetData(UpdatePolicy.EnsureUpdated).MinPartitionDecode;

    public ulong MaxPartitionDecode => m_PartitionableGpu.GetData(UpdatePolicy.EnsureUpdated).MaxPartitionDecode;

    public ulong OptimalPartitionDecode => m_PartitionableGpu.GetData(UpdatePolicy.EnsureUpdated).OptimalPartitionDecode;

    public ulong TotalCompute => m_PartitionableGpu.GetData(UpdatePolicy.EnsureUpdated).TotalCompute;

    public ulong AvailableCompute => m_PartitionableGpu.GetData(UpdatePolicy.EnsureUpdated).AvailableCompute;

    public ulong MinPartitionCompute => m_PartitionableGpu.GetData(UpdatePolicy.EnsureUpdated).MinPartitionCompute;

    public ulong MaxPartitionCompute => m_PartitionableGpu.GetData(UpdatePolicy.EnsureUpdated).MaxPartitionCompute;

    public ulong OptimalPartitionCompute => m_PartitionableGpu.GetData(UpdatePolicy.EnsureUpdated).OptimalPartitionCompute;

    private VMPartitionableGpu(IPartitionableGpu partitionableGpu)
        : base(partitionableGpu)
    {
        m_PartitionableGpu = InitializePrimaryDataUpdater(partitionableGpu);
    }

    void IUpdatable.Put(IOperationWatcher operationWatcher)
    {
        operationWatcher.PerformPut(m_PartitionableGpu.GetData(UpdatePolicy.None), TaskDescriptions.SetVMPartitionableGpuPartitionCount, this);
    }

    internal static VMPartitionableGpu[] GetVMPartitionableGpus(Server server)
    {
        return (from gpu in ObjectLocator.GetPartitionableGpus(server)
            select new VMPartitionableGpu(gpu)).ToArray();
    }
}
