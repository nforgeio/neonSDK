using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Management.Infrastructure;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Disconnect", "VMNetworkAdapter", SupportsShouldProcess = true, DefaultParameterSetName = "Name")]
[OutputType(new Type[] { typeof(VMNetworkAdapter) })]
internal sealed class DisconnectVMNetworkAdapter : VirtualizationCmdlet<VMNetworkAdapter>, IVmByVMNameCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, ISupportsPassthrough
{
    internal static class ParameterSetNames
    {
        public const string Name = "Name";

        public const string Object = "Obj";
    }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is byte spec.")]
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "Obj")]
    public VMNetworkAdapter[] VMNetworkAdapter { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is byte spec.")]
    [ValidateNotNullOrEmpty]
    [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Name")]
    public string[] VMName { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is byte spec.")]
    [ValidateNotNullOrEmpty]
    [Parameter(Position = 1, ParameterSetName = "Name")]
    [Alias(new string[] { "VMNetworkAdapterName" })]
    public string[] Name { get; set; }

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

    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
    [Parameter]
    public SwitchParameter Passthru { get; set; }

    internal override IList<VMNetworkAdapter> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        IList<VMNetworkAdapter> list;
        if (IsParameterSpecified("VMNetworkAdapter"))
        {
            list = VMNetworkAdapter;
        }
        else
        {
            IEnumerable<VMNetworkAdapter> source = ParameterResolvers.ResolveVirtualMachines(this, operationWatcher).SelectManyWithLogging((VirtualMachine vm) => vm.GetNetworkAdapters(), operationWatcher);
            if (Name.IsNullOrEmpty())
            {
                list = source.ToList();
            }
            else
            {
                WildcardPatternMatcher matcher = new WildcardPatternMatcher(Name);
                list = source.Where((VMNetworkAdapter adapter) => matcher.MatchesAny(adapter.Name)).ToList();
                if (list.Count == 0)
                {
                    throw ExceptionHelper.CreateObjectNotFoundException(CmdletErrorMessages.VMNetworkAdapter_NoneFound, null);
                }
            }
        }
        return list;
    }

    internal override void ProcessOneOperand(VMNetworkAdapter operand, IOperationWatcher operationWatcher)
    {
        if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_DisconnectVMNetworkAdapter, operand.Name)))
        {
            operand.PrepareForModify(operationWatcher);
            operand.IsEnabled = false;
            ((IUpdatable)operand).Put(operationWatcher);
            if (Passthru.IsPresent)
            {
                operationWatcher.WriteObject(operand);
            }
        }
    }
}
