using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Management.Automation;
using Microsoft.Management.Infrastructure;

namespace Microsoft.HyperV.PowerShell.Commands;

[SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ComPort", Justification = "This is by spec.")]
[Cmdlet("Get", "VMComPort", DefaultParameterSetName = "VMName")]
[OutputType(new Type[] { typeof(VMComPort) })]
internal sealed class GetVMComPort : VirtualizationCmdlet<VMComPort>, IVMObjectOrVMNameCmdlet, IVmByObjectCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmByVMNameCmdlet, IVMSnapshotCmdlet
{
    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
    [ValidateNotNullOrEmpty]
    [Parameter(ParameterSetName = "VMObject", ValueFromPipeline = true, Position = 0, Mandatory = true)]
    public VirtualMachine[] VM { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [ValidateNotNullOrEmpty]
    [Parameter(ParameterSetName = "VMName", ValueFromPipeline = true, Position = 0, Mandatory = true)]
    public string[] VMName { get; set; }

    [ValidateNotNullOrEmpty]
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "VMSnapshot")]
    [Alias(new string[] { "VMCheckpoint" })]
    public VMSnapshot VMSnapshot { get; set; }

    [Parameter(Position = 1)]
    [ValidateRange(1, 2)]
    public int Number { get; set; }

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

    internal override IList<VMComPort> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        IEnumerable<VirtualMachineBase> enumerable = (IEnumerable<VirtualMachineBase>)((!IsParameterSpecified("VMSnapshot")) ? ((IEnumerable)ParameterResolvers.ResolveVirtualMachines(this, operationWatcher)) : ((IEnumerable)new VMSnapshot[1] { VMSnapshot }));
        bool num = IsParameterSpecified("Number");
        bool flag = !num || Number == 1;
        bool flag2 = !num || Number == 2;
        List<VMComPort> list = new List<VMComPort>();
        foreach (VirtualMachineBase item in enumerable)
        {
            if (flag)
            {
                list.Add(item.ComPort1);
            }
            if (flag2)
            {
                list.Add(item.ComPort2);
            }
        }
        return list;
    }

    internal override void ProcessOneOperand(VMComPort operand, IOperationWatcher operationWatcher)
    {
        operationWatcher.WriteObject(operand);
    }
}
