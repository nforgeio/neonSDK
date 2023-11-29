using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.Management.Infrastructure;

using AllowNull = System.Management.Automation.AllowNullAttribute;

namespace Microsoft.HyperV.PowerShell.Commands;

[SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ComPort", Justification = "This is by spec.")]
[Cmdlet("Set", "VMComPort", DefaultParameterSetName = "VMName", SupportsShouldProcess = true)]
[OutputType(new Type[] { typeof(VMComPort) })]
internal sealed class SetVMComPort : VirtualizationCmdlet<VMComPort>, IVMObjectOrVMNameCmdlet, IVmByObjectCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmByVMNameCmdlet, ISupportsPassthrough
{
    private static class ParameterSetNames
    {
        public const string ComportObject = "VMComPort";
    }

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
    [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ComPort", Justification = "This is by spec.")]
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "VMComPort")]
    public VMComPort[] VMComPort { get; set; }

    [Parameter(Mandatory = true, ParameterSetName = "VMObject")]
    [Parameter(Mandatory = true, Position = 1, ParameterSetName = "VMName")]
    [ValidateRange(1, 2)]
    public int Number { get; set; }

    [Parameter(Position = 2)]
    [AllowEmptyString]
    [AllowNull]
    public string Path { get; set; }

    [Parameter]
    public OnOffState DebuggerMode { get; set; }

    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
    [Parameter]
    public SwitchParameter Passthru { get; set; }

    protected override void ValidateParameters()
    {
        if (!IsParameterSpecified("Path") && !IsParameterSpecified("DebuggerMode"))
        {
            WriteWarning(CmdletErrorMessages.Warning_MustSpecifyPathOrDebuggerMode);
        }
        base.ValidateParameters();
    }

    internal override IList<VMComPort> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        if (CurrentParameterSetIs("VMComPort"))
        {
            return VMComPort;
        }
        return (from vm in ParameterResolvers.ResolveVirtualMachines(this, operationWatcher)
            select (Number != 1) ? vm.ComPort2 : vm.ComPort1).ToList();
    }

    internal override void ProcessOneOperand(VMComPort comPort, IOperationWatcher operationWatcher)
    {
        if (!operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_SetVMComPort, comPort.Name)))
        {
            return;
        }
        bool flag = false;
        if (IsParameterSpecified("Path"))
        {
            if (Path == null)
            {
                Path = string.Empty;
            }
            comPort.Path = Path;
            flag = true;
        }
        if (IsParameterSpecified("DebuggerMode"))
        {
            comPort.DebuggerMode = DebuggerMode;
            flag = true;
        }
        if (flag)
        {
            ((IUpdatable)comPort).Put(operationWatcher);
        }
        if (Passthru.IsPresent)
        {
            operationWatcher.WriteObject(comPort);
        }
    }
}
