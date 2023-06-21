using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.HyperV.PowerShell.Common;
using Microsoft.Management.Infrastructure;
using Microsoft.Virtualization.Client.Management;

using ErrorMessages = Microsoft.HyperV.PowerShell.Common.ErrorMessages;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Add", "VMGroupMember", DefaultParameterSetName = "VM Using Name", SupportsShouldProcess = true)]
[OutputType(new Type[] { typeof(VMGroup) })]
internal sealed class AddVMGroupMember : VirtualizationCmdlet<VMGroup>, ISupportsPassthrough
{
    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "VM Using Name")]
    [Parameter(ParameterSetName = "VMGroup Using Name")]
    [Parameter(ParameterSetName = "VM Using ID")]
    [Parameter(ParameterSetName = "VMGroup Using ID")]
    [ValidateNotNullOrEmpty]
    public override CimSession[] CimSession { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "VM Using Name")]
    [Parameter(ParameterSetName = "VMGroup Using Name")]
    [Parameter(ParameterSetName = "VM Using ID")]
    [Parameter(ParameterSetName = "VMGroup Using ID")]
    [ValidateNotNullOrEmpty]
    public override string[] ComputerName { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "VM Using Name")]
    [Parameter(ParameterSetName = "VMGroup Using Name")]
    [Parameter(ParameterSetName = "VM Using ID")]
    [Parameter(ParameterSetName = "VMGroup Using ID")]
    [ValidateNotNullOrEmpty]
    [CredentialArray]
    public override PSCredential[] Credential { get; set; }

    [ValidateNotNullOrEmpty]
    [Parameter(ParameterSetName = "VM Using Name", Mandatory = true, Position = 0)]
    [Parameter(ParameterSetName = "VMGroup Using Name", Mandatory = true, Position = 0)]
    public string Name { get; set; }

    [Parameter(ParameterSetName = "VM Using ID", Mandatory = true, Position = 0)]
    [Parameter(ParameterSetName = "VMGroup Using ID", Mandatory = true, Position = 0)]
    [ValidateNotNullOrEmpty]
    public Guid Id { get; set; }

    [Parameter(ParameterSetName = "VM Using InputObject", Mandatory = true, Position = 0)]
    [Parameter(ParameterSetName = "VMGroup Using InputObject", Mandatory = true, Position = 0)]
    [ValidateNotNullOrEmpty]
    public VMGroup VMGroup { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "VM Using Name", Mandatory = true, Position = 1)]
    [Parameter(ParameterSetName = "VM Using ID", Mandatory = true, Position = 1)]
    [Parameter(ParameterSetName = "VM Using InputObject", Mandatory = true, Position = 1)]
    [ValidateNotNullOrEmpty]
    public VirtualMachine[] VM { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "VMGroup Using Name", Mandatory = true, Position = 1)]
    [Parameter(ParameterSetName = "VMGroup Using ID", Mandatory = true, Position = 1)]
    [Parameter(ParameterSetName = "VMGroup Using InputObject", Mandatory = true, Position = 1)]
    [ValidateNotNullOrEmpty]
    public VMGroup[] VMGroupMember { get; set; }

    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
    [Parameter]
    public SwitchParameter Passthru { get; set; }

    internal override IList<VMGroup> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        if (CurrentParameterSetIs("VM Using InputObject") || CurrentParameterSetIs("VMGroup Using InputObject"))
        {
            return new VMGroup[1] { VMGroup };
        }
        IEnumerable<Server> servers = ParameterResolvers.GetServers(this, operationWatcher);
        if (CurrentParameterSetIs("VM Using Name") || CurrentParameterSetIs("VMGroup Using Name"))
        {
            IList<string> list = new List<string>();
            list.Add(Name);
            return VMGroup.GetVMGroupsByName(servers, list, operationWatcher);
        }
        return VMGroup.GetVMGroupsById(servers, Id, operationWatcher);
    }

    internal override void ValidateOperandList(IList<VMGroup> operands, IOperationWatcher operationWatcher)
    {
        base.ValidateOperandList(operands, operationWatcher);
        if (!operands.Any())
        {
            throw ExceptionHelper.CreateObjectNotFoundException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.VMGroupMember_NoneFound, Name), null);
        }
        if (operands.Count > 1)
        {
            throw ExceptionHelper.CreateObjectNotFoundException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.VMGroupMember_MoreThanOneFound, Name), null);
        }
    }

    internal override void ProcessOneOperand(VMGroup operand, IOperationWatcher operationWatcher)
    {
        if (!operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_AddVMGroupMember, Name)))
        {
            return;
        }
        if (CurrentParameterSetIs("VM Using Name") || CurrentParameterSetIs("VM Using ID") || CurrentParameterSetIs("VM Using InputObject"))
        {
            if (operand.GroupType == GroupType.ManagementCollectionType)
            {
                throw ExceptionHelper.CreateObjectNotFoundException(ErrorMessages.VMGroup_VMGroupType_IncorrectType, null);
            }
            VirtualMachine[] vM = VM;
            foreach (VirtualMachine vm in vM)
            {
                operand.AddVM(vm, operationWatcher);
            }
        }
        else
        {
            if (operand.GroupType == GroupType.VMCollectionType)
            {
                throw ExceptionHelper.CreateObjectNotFoundException(ErrorMessages.VMGroup_VMType_IncorrectType, null);
            }
            VMGroup[] vMGroupMember = VMGroupMember;
            foreach (VMGroup group in vMGroupMember)
            {
                operand.AddGroup(group, operationWatcher);
            }
        }
        if (Passthru.IsPresent)
        {
            operationWatcher.WriteObject(operand);
        }
    }
}
