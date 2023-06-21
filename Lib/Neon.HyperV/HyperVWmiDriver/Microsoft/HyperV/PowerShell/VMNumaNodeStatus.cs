using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Numa", Justification = "This is by spec.")]
internal sealed class VMNumaNodeStatus : VirtualizationObject
{
    private readonly DataUpdater<IHostNumaNode> m_NumaNodeSetting;

    private readonly DataUpdater<IVMMemory> m_VirtualMachineMemory;

    private readonly DataUpdater<IVMComputerSystem> m_VirtualMachineSettings;

    [VirtualizationObjectIdentifier(IdentifierFlags.UniqueIdentifier | IdentifierFlags.FriendlyName)]
    public int NodeId
    {
        get
        {
            string nodeId = m_NumaNodeSetting.GetData(UpdatePolicy.None).NodeId;
            return int.Parse(nodeId.Substring(nodeId.LastIndexOf("\\", StringComparison.Ordinal) + 1), CultureInfo.InvariantCulture);
        }
    }

    [VirtualizationObjectIdentifier(IdentifierFlags.UniqueIdentifier)]
    public Guid VMId => Guid.Parse(m_VirtualMachineSettings.GetData(UpdatePolicy.EnsureUpdated).InstanceId);

    [VirtualizationObjectIdentifier(IdentifierFlags.FriendlyName)]
    public string VMName => m_VirtualMachineSettings.GetData(UpdatePolicy.EnsureUpdated).Name;

    public int MemoryUsed => NumberConverter.UInt64ToInt32(m_VirtualMachineMemory.GetData(UpdatePolicy.EnsureUpdated).AllocatedRam);

    private VMNumaNodeStatus(IVMComputerSystem virtualMachine, IVMMemory memory, IHostNumaNode node)
        : base(virtualMachine)
    {
        m_VirtualMachineSettings = InitializePrimaryDataUpdater(virtualMachine);
        m_VirtualMachineMemory = new DataUpdater<IVMMemory>(memory);
        m_NumaNodeSetting = new DataUpdater<IHostNumaNode>(node);
    }

    internal static VMNumaNodeStatus[] GetVMNumaNodeStatus(VMHost host)
    {
        return GetVMNumaNodeStatus(host.Server);
    }

    internal static VMNumaNodeStatus[] GetVMNumaNodeStatus(Server server)
    {
        List<VMNumaNodeStatus> list = new List<VMNumaNodeStatus>();
        foreach (IVMComputerSystem vMComputerSystem in ObjectLocator.GetVMComputerSystems(server))
        {
            List<IVMMemory> list2 = vMComputerSystem.Memory.ToList();
            if (list2.IsNullOrEmpty())
            {
                continue;
            }
            foreach (IVMMemory item in list2)
            {
                IHostNumaNode numaNode = item.GetNumaNode();
                if (numaNode != null)
                {
                    list.Add(new VMNumaNodeStatus(vMComputerSystem, item, numaNode));
                }
            }
        }
        return list.ToArray();
    }
}
