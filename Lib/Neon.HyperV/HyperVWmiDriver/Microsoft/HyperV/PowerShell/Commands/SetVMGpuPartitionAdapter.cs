using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.HyperV.PowerShell.Common;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Management.Infrastructure;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Set", "VMGpuPartitionAdapter", DefaultParameterSetName = "VMName", SupportsShouldProcess = true)]
[OutputType(new Type[] { typeof(VMGpuPartitionAdapter) })]
internal sealed class SetVMGpuPartitionAdapter : VirtualizationCmdlet<VMGpuPartitionAdapter>, IVMObjectOrVMNameCmdlet, IVmByObjectCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmByVMNameCmdlet, ISupportsPassthrough
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

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [ValidateNotNullOrEmpty]
    [Parameter(ParameterSetName = "VMName", ValueFromPipeline = true, Position = 0, Mandatory = true)]
    public string[] VMName { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
    [ValidateNotNullOrEmpty]
    [Parameter(ParameterSetName = "VMObject", ValueFromPipeline = true, Position = 0, Mandatory = true)]
    public VirtualMachine[] VM { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [ValidateNotNullOrEmpty]
    [Parameter(ParameterSetName = "Object", ValueFromPipeline = true, Position = 0, Mandatory = true)]
    public VMGpuPartitionAdapter[] VMGpuPartitionAdapter { get; set; }

    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
    [Parameter]
    public SwitchParameter Passthru { get; set; }

    [ValidateNotNullOrEmpty]
    [Parameter(ParameterSetName = "VMName")]
    [Parameter(ParameterSetName = "VMObject")]
    public string AdapterId { get; set; }

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

    internal override IList<VMGpuPartitionAdapter> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        if (CurrentParameterSetIs("Object"))
        {
            return VMGpuPartitionAdapter;
        }
        IEnumerable<VMGpuPartitionAdapter> enumerable = ParameterResolvers.ResolveVirtualMachines(this, operationWatcher).SelectManyWithLogging((VirtualMachine vm) => vm.GetGpuPartitionAdapters(AdapterId), operationWatcher);
        if (enumerable == null || !enumerable.Any())
        {
            throw ExceptionHelper.CreateObjectNotFoundException(ErrorMessages.VMGpuPartitionAdapter_NotFound, null);
        }
        return enumerable.ToList();
    }

    internal override void ProcessOneOperand(VMGpuPartitionAdapter operand, IOperationWatcher operationWatcher)
    {
        if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_SetVMGpuPartitionAdapter, operand.VMName)))
        {
            if (ConfigureGpuPartitionAdapter(operand))
            {
                ((IUpdatable)operand).Put(operationWatcher);
            }
            if ((bool)Passthru)
            {
                operationWatcher.WriteObject(operand);
            }
        }
    }

    private bool ConfigureGpuPartitionAdapter(VMGpuPartitionAdapter gpuPartitionAdapter)
    {
        bool result = false;
        if (MinPartitionVRAM.HasValue)
        {
            result = true;
            gpuPartitionAdapter.MinPartitionVRAM = MinPartitionVRAM.Value;
        }
        if (MaxPartitionVRAM.HasValue)
        {
            result = true;
            gpuPartitionAdapter.MaxPartitionVRAM = MaxPartitionVRAM.Value;
        }
        if (OptimalPartitionVRAM.HasValue)
        {
            result = true;
            gpuPartitionAdapter.OptimalPartitionVRAM = OptimalPartitionVRAM.Value;
        }
        if (MinPartitionEncode.HasValue)
        {
            result = true;
            gpuPartitionAdapter.MinPartitionEncode = MinPartitionEncode.Value;
        }
        if (MaxPartitionEncode.HasValue)
        {
            result = true;
            gpuPartitionAdapter.MaxPartitionEncode = MaxPartitionEncode.Value;
        }
        if (OptimalPartitionEncode.HasValue)
        {
            result = true;
            gpuPartitionAdapter.OptimalPartitionEncode = OptimalPartitionEncode.Value;
        }
        if (MinPartitionDecode.HasValue)
        {
            result = true;
            gpuPartitionAdapter.MinPartitionDecode = MinPartitionDecode.Value;
        }
        if (MaxPartitionDecode.HasValue)
        {
            result = true;
            gpuPartitionAdapter.MaxPartitionDecode = MaxPartitionDecode.Value;
        }
        if (OptimalPartitionDecode.HasValue)
        {
            result = true;
            gpuPartitionAdapter.OptimalPartitionDecode = OptimalPartitionDecode.Value;
        }
        if (MinPartitionCompute.HasValue)
        {
            result = true;
            gpuPartitionAdapter.MinPartitionCompute = MinPartitionCompute.Value;
        }
        if (MaxPartitionCompute.HasValue)
        {
            result = true;
            gpuPartitionAdapter.MaxPartitionCompute = MaxPartitionCompute.Value;
        }
        if (OptimalPartitionCompute.HasValue)
        {
            result = true;
            gpuPartitionAdapter.OptimalPartitionCompute = OptimalPartitionCompute.Value;
        }
        return result;
    }
}
