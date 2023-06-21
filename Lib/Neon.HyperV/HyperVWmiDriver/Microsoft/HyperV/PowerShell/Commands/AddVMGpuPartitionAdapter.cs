using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.Management.Infrastructure;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Add", "VMGpuPartitionAdapter", SupportsShouldProcess = true, DefaultParameterSetName = "VMName")]
[OutputType(new Type[] { typeof(VMGpuPartitionAdapter) })]
internal sealed class AddVMGpuPartitionAdapter : VirtualizationCmdlet<VirtualMachine>, IVMObjectOrVMNameCmdlet, IVmByObjectCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmByVMNameCmdlet, ISupportsPassthrough
{
    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "VMName")]
    [ValidateNotNullOrEmpty]
    public override CimSession[] CimSession { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "VMName")]
    [ValidateNotNullOrEmpty]
    public override string[] ComputerName { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "VMName")]
    [ValidateNotNullOrEmpty]
    [CredentialArray]
    public override PSCredential[] Credential { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
    [ValidateNotNullOrEmpty]
    [Parameter(ParameterSetName = "VMObject", ValueFromPipeline = true, Position = 0, Mandatory = true)]
    public VirtualMachine[] VM { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [ValidateNotNullOrEmpty]
    [Parameter(ParameterSetName = "VMName", ValueFromPipeline = true, Position = 0, Mandatory = true)]
    public string[] VMName { get; set; }

    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
    [Parameter]
    public SwitchParameter Passthru { get; set; }

    [ValidateNotNull]
    [Parameter]
    public ulong? MinPartitionVRAM { get; set; }

    [ValidateNotNull]
    [Parameter]
    public ulong? MaxPartitionVRAM { get; set; }

    [ValidateNotNull]
    [Parameter]
    public ulong? OptimalPartitionVRAM { get; set; }

    [ValidateNotNull]
    [Parameter]
    public ulong? MinPartitionEncode { get; set; }

    [ValidateNotNull]
    [Parameter]
    public ulong? MaxPartitionEncode { get; set; }

    [ValidateNotNull]
    [Parameter]
    public ulong? OptimalPartitionEncode { get; set; }

    [ValidateNotNull]
    [Parameter]
    public ulong? MinPartitionDecode { get; set; }

    [ValidateNotNull]
    [Parameter]
    public ulong? MaxPartitionDecode { get; set; }

    [ValidateNotNull]
    [Parameter]
    public ulong? OptimalPartitionDecode { get; set; }

    [ValidateNotNull]
    [Parameter]
    public ulong? MinPartitionCompute { get; set; }

    [ValidateNotNull]
    [Parameter]
    public ulong? MaxPartitionCompute { get; set; }

    [ValidateNotNull]
    [Parameter]
    public ulong? OptimalPartitionCompute { get; set; }

    internal override IList<VirtualMachine> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        return ParameterResolvers.ResolveVirtualMachines(this, operationWatcher);
    }

    internal override void ProcessOneOperand(VirtualMachine vm, IOperationWatcher operationWatcher)
    {
        if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_AddVMGpuPartitionAdapter, vm.Name)))
        {
            VMGpuPartitionAdapter gpuPartitionAdapter = VMGpuPartitionAdapter.CreateTemplateGpuPartitionAdapter(vm);
            ConfigureGpuPartitionAdapter(gpuPartitionAdapter);
            VMGpuPartitionAdapter output = VMGpuPartitionAdapter.AddGpuPartitionAdapter(vm, gpuPartitionAdapter, operationWatcher);
            if (Passthru.IsPresent)
            {
                operationWatcher.WriteObject(output);
            }
        }
    }

    private void ConfigureGpuPartitionAdapter(VMGpuPartitionAdapter gpuPartitionAdapter)
    {
        if (MinPartitionVRAM.HasValue)
        {
            gpuPartitionAdapter.MinPartitionVRAM = MinPartitionVRAM.Value;
        }
        if (MaxPartitionVRAM.HasValue)
        {
            gpuPartitionAdapter.MaxPartitionVRAM = MaxPartitionVRAM.Value;
        }
        if (OptimalPartitionVRAM.HasValue)
        {
            gpuPartitionAdapter.OptimalPartitionVRAM = OptimalPartitionVRAM.Value;
        }
        if (MinPartitionEncode.HasValue)
        {
            gpuPartitionAdapter.MinPartitionEncode = MinPartitionEncode.Value;
        }
        if (MaxPartitionEncode.HasValue)
        {
            gpuPartitionAdapter.MaxPartitionEncode = MaxPartitionEncode.Value;
        }
        if (OptimalPartitionEncode.HasValue)
        {
            gpuPartitionAdapter.OptimalPartitionEncode = OptimalPartitionEncode.Value;
        }
        if (MinPartitionDecode.HasValue)
        {
            gpuPartitionAdapter.MinPartitionDecode = MinPartitionDecode.Value;
        }
        if (MaxPartitionDecode.HasValue)
        {
            gpuPartitionAdapter.MaxPartitionDecode = MaxPartitionDecode.Value;
        }
        if (OptimalPartitionDecode.HasValue)
        {
            gpuPartitionAdapter.OptimalPartitionDecode = OptimalPartitionDecode.Value;
        }
        if (MinPartitionCompute.HasValue)
        {
            gpuPartitionAdapter.MinPartitionCompute = MinPartitionCompute.Value;
        }
        if (MaxPartitionCompute.HasValue)
        {
            gpuPartitionAdapter.MaxPartitionCompute = MaxPartitionCompute.Value;
        }
        if (OptimalPartitionCompute.HasValue)
        {
            gpuPartitionAdapter.OptimalPartitionCompute = OptimalPartitionCompute.Value;
        }
    }
}
