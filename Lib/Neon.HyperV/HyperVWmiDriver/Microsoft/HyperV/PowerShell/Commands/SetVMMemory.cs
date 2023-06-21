using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.Management.Infrastructure;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Set", "VMMemory", DefaultParameterSetName = "VMName", SupportsShouldProcess = true)]
[OutputType(new Type[] { typeof(VMMemory) })]
internal sealed class SetVMMemory : VirtualizationCmdlet<VMMemory>, IVMObjectOrVMNameCmdlet, IVmByObjectCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmByVMNameCmdlet, ISupportsPassthrough
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
    [ValidateNotNull]
    [Parameter(ParameterSetName = "ResourceObject", ValueFromPipeline = true, Position = 0, Mandatory = true)]
    public VMMemory[] VMMemory { get; set; }

    [Parameter]
    [ValidateNotNull]
    public int? Buffer { get; set; }

    [Parameter]
    public bool DynamicMemoryEnabled { get; set; }

    [ValidateNotNull]
    [Parameter]
    public long? MaximumBytes { get; set; }

    [ValidateNotNull]
    [Parameter]
    public long? StartupBytes { get; set; }

    [ValidateNotNull]
    [Parameter]
    public long? MinimumBytes { get; set; }

    [ValidateNotNull]
    [Parameter]
    public int? Priority { get; set; }

    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Numa", Justification = "This is by spec.")]
    [ValidateNotNull]
    [Parameter]
    public long? MaximumAmountPerNumaNodeBytes { get; set; }

    [ValidateNotNull]
    [Parameter]
    public string ResourcePoolName { get; set; }

    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
    [Parameter]
    public SwitchParameter Passthru { get; set; }

    internal override IList<VMMemory> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        if (CurrentParameterSetIs("ResourceObject"))
        {
            return VMMemory;
        }
        return (from vm in ParameterResolvers.ResolveVirtualMachines(this, operationWatcher)
            select vm.GetMemory()).ToList();
    }

    internal override void ProcessOneOperand(VMMemory memory, IOperationWatcher operationWatcher)
    {
        bool? flag = null;
        bool? flag2 = null;
        if (IsParameterSpecified("DynamicMemoryEnabled"))
        {
            flag = DynamicMemoryEnabled;
        }
        VirtualMachineBase parentAs = memory.GetParentAs<VirtualMachineBase>();
        if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_SetVMMemory, parentAs.Name)))
        {
            ValidateParametersInternal(memory.DynamicMemoryEnabled, false);
            if (flag.Equals(true) || flag2.Equals(true))
            {
                parentAs.VirtualNumaEnabled = false;
                ((IUpdatable)parentAs).Put(operationWatcher);
            }
            if (flag.HasValue)
            {
                memory.DynamicMemoryEnabled = flag.Value;
            }
            if (Priority.HasValue)
            {
                memory.Priority = Priority.Value;
            }
            if (StartupBytes.HasValue)
            {
                memory.Startup = StartupBytes.Value;
            }
            if (MinimumBytes.HasValue)
            {
                memory.Minimum = MinimumBytes.Value;
            }
            if (MaximumBytes.HasValue)
            {
                memory.Maximum = MaximumBytes.Value;
            }
            if (Buffer.HasValue)
            {
                memory.Buffer = Buffer.Value;
            }
            if (MaximumAmountPerNumaNodeBytes.HasValue)
            {
                memory.MaximumPerNumaNode = MaximumAmountPerNumaNodeBytes.Value / Constants.Mega;
            }
            if (ResourcePoolName != null)
            {
                memory.ResourcePoolName = ResourcePoolName;
            }
            ((IUpdatable)memory).Put(operationWatcher);
            if (!memory.DynamicMemoryEnabled)
            {
                parentAs.VirtualNumaEnabled = true;
                ((IUpdatable)parentAs).Put(operationWatcher);
            }
            if (Passthru.IsPresent)
            {
                operationWatcher.WriteObject(memory);
            }
        }
    }

    protected override void ValidateParameters()
    {
        base.ValidateParameters();
        ValidateParametersInternal(null, null);
        if (MaximumAmountPerNumaNodeBytes.HasValue && MaximumAmountPerNumaNodeBytes.Value % (2 * Constants.Mega) != 0L)
        {
            throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.SetVMMemory_SizeNotAligned);
        }
    }

    private void ValidateParametersInternal(bool? dynamicMemoryEnabledOldValue, bool? consolidationEnabledOldValue)
    {
        if (Buffer.HasValue || MinimumBytes.HasValue || MaximumBytes.HasValue)
        {
            bool? flag = dynamicMemoryEnabledOldValue;
            bool? flag2 = consolidationEnabledOldValue;
            if (IsParameterSpecified("DynamicMemoryEnabled"))
            {
                flag = DynamicMemoryEnabled;
            }
            if (flag.Equals(false) && flag2.Equals(false))
            {
                throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.SetVMMemory_ParameterMismatch);
            }
        }
    }
}
