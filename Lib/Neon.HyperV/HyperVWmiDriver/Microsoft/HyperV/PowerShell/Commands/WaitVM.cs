using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Management.Automation;
using System.Threading;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.Management.Infrastructure;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Wait", "VM", DefaultParameterSetName = "Name")]
[OutputType(new Type[] { typeof(VirtualMachine) })]
internal sealed class WaitVM : VirtualizationCmdlet<VirtualMachine>, IVMObjectOrNameCmdlet, IVmByObjectCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmByNameCmdlet, ISupportsAsJob, ISupportsPassthrough, IDisposable
{
    private ManualResetEventSlim waitHandle = new ManualResetEventSlim(initialState: false);

    private bool disposed;

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "Name")]
    [ValidateNotNullOrEmpty]
    public override CimSession[] CimSession { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "Name")]
    [ValidateNotNullOrEmpty]
    public override string[] ComputerName { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "Name")]
    [ValidateNotNullOrEmpty]
    [CredentialArray]
    public override PSCredential[] Credential { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
    [ValidateNotNullOrEmpty]
    [Parameter(ParameterSetName = "VMObject", ValueFromPipeline = true, Position = 0, Mandatory = true)]
    public VirtualMachine[] VM { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [ValidateNotNullOrEmpty]
    [Alias(new string[] { "VMName" })]
    [Parameter(ParameterSetName = "Name", ValueFromPipeline = true, Position = 0, Mandatory = true)]
    public string[] Name { get; set; }

    [Parameter]
    public SwitchParameter AsJob { get; set; }

    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
    [Parameter]
    public SwitchParameter Passthru { get; set; }

    [Parameter]
    public WaitVMTypes For { get; set; }

    [ValidateRange(1, ushort.MaxValue)]
    [Parameter]
    public ushort Delay { get; set; } = 5;


    [Alias(new string[] { "TimeoutSec" })]
    [ValidateRange(-1, int.MaxValue)]
    [Parameter]
    public int Timeout { get; set; } = -1;


    public void Dispose()
    {
        if (!disposed)
        {
            waitHandle.Dispose();
            disposed = true;
        }
    }

    internal override IList<VirtualMachine> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        return ParameterResolvers.ResolveVirtualMachines(this, operationWatcher);
    }

    internal override void ProcessOneOperand(VirtualMachine operand, IOperationWatcher operationWatcher)
    {
        if (!operand.TryWaitCondition(For, TimeSpan.FromSeconds(Timeout), Delay, waitHandle))
        {
            throw ExceptionHelper.CreateOperationFailedException(CmdletErrorMessages.OperationFailed_ConditionNotSatisfied);
        }
        if (Passthru.IsPresent)
        {
            operationWatcher.WriteObject(operand);
        }
    }

    protected override void StopProcessing()
    {
        waitHandle.Set();
        base.StopProcessing();
    }

    protected override void EndProcessing()
    {
        Dispose();
        base.EndProcessing();
    }
}
