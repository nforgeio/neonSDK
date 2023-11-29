using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.HyperV.PowerShell.Common;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMSnapshot : VirtualMachineBase, IRemovable, IUpdatable
{
    private readonly VirtualMachine m_VM;

    public SnapshotType SnapshotType => (SnapshotType)GetSettings(UpdatePolicy.EnsureUpdated).VirtualSystemType;

    public bool IsAutomaticCheckpoint => GetSettings(UpdatePolicy.EnsureUpdated).IsAutomaticCheckpoint;

    public Guid VMId
    {
        get
        {
            if (!Guid.TryParse(GetSettings(UpdatePolicy.EnsureUpdated).SystemName, out var result))
            {
                return Guid.Empty;
            }
            return result;
        }
    }

    public string VMName => m_VM.Name;

    public override VMState State
    {
        get
        {
            if (!GetSettings(UpdatePolicy.EnsureUpdated).IsSaved)
            {
                return VMState.Off;
            }
            return VMState.Saved;
        }
    }

    internal VirtualMachine VM => m_VM;

    internal VMSnapshot(IVMComputerSystemSetting vmSetting, VirtualMachine vm)
        : base(null, vmSetting)
    {
        m_VM = vm;
    }

    void IUpdatable.Put(IOperationWatcher operationWatcher)
    {
        operationWatcher.PerformPut(GetSettings(UpdatePolicy.None), TaskDescriptions.SetVMSnapshot, this);
    }

    void IRemovable.Remove(IOperationWatcher operationWatcher)
    {
        operationWatcher.PerformDelete(GetSettings(UpdatePolicy.EnsureUpdated), TaskDescriptions.RemoveVMSnapshot, this);
        VM.InvalidateAssociations();
    }

    internal void Apply(IOperationWatcher operationWatcher)
    {
        IVMComputerSystemSetting settings = GetSettings(UpdatePolicy.EnsureUpdated);
        operationWatcher.PerformOperation(settings.BeginApply, settings.EndApply, TaskDescriptions.RestoreVMSnapshot, this);
    }

    internal void DeleteSavedState(IOperationWatcher operationWatcher)
    {
        IVMComputerSystemSetting settings = GetSettings(UpdatePolicy.EnsureUpdated);
        operationWatcher.PerformOperation(settings.BeginClearSnapshotState, settings.EndClearSnapshotState, TaskDescriptions.RemoveVMSnapshotSavedState, this);
    }

    internal void DeleteTree(IOperationWatcher operationWatcher)
    {
        IVMComputerSystemSetting settings = GetSettings(UpdatePolicy.EnsureUpdated);
        operationWatcher.PerformOperation(settings.BeginDeleteTree, settings.EndDeleteTree, TaskDescriptions.RemoveVMSnapshot_IncludeAllChildSnapshots, this);
        VM.InvalidateAssociations();
    }

    internal override void Export(IOperationWatcher operationWatcher, string path, CaptureLiveState? captureLiveState)
    {
        IVMComputerSystemSetting settings = GetSettings(UpdatePolicy.None);
        ExportInternal(operationWatcher, captureLiveState, path, settings, TaskDescriptions.ExportVMSnapshot);
    }

    internal IList<VMSnapshot> GetChildSnapshots()
    {
        return (from child in GetSettings(UpdatePolicy.EnsureAssociatorsUpdated).ChildSettings
            where child.IsSnapshot
            select new VMSnapshot(child, VM)).ToList();
    }
}
