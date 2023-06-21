using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Management.Infrastructure;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Get", "VMSwitchExtension", DefaultParameterSetName = "SwitchName")]
[OutputType(new Type[] { typeof(VMSwitchExtension) })]
internal sealed class GetVMSwitchExtension : VirtualizationCmdlet<VMSwitchExtension>
{
    internal static class ParameterSetNames
    {
        public const string SwitchName = "SwitchName";

        public const string SwitchObject = "SwitchObject";
    }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Array is more user friendly, and this is a user input parameter.")]
    [ValidateNotNullOrEmpty]
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "SwitchName")]
    public string[] VMSwitchName { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec, and array is more user friendly as this is an user input parameter.")]
    [ValidateNotNullOrEmpty]
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "SwitchObject")]
    public VMSwitch[] VMSwitch { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Array is more user friendly, and this is a user input parameter.")]
    [ValidateNotNullOrEmpty]
    [Parameter(Mandatory = false, Position = 1, ValueFromPipeline = false)]
    public string[] Name { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "SwitchName", ValueFromPipelineByPropertyName = true)]
    [ValidateNotNullOrEmpty]
    public override CimSession[] CimSession { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "SwitchName")]
    [Alias(new string[] { "PSComputerName" })]
    [ValidateNotNullOrEmpty]
    public override string[] ComputerName { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "SwitchName")]
    [ValidateNotNullOrEmpty]
    [CredentialArray]
    public override PSCredential[] Credential { get; set; }

    protected override void ValidateParameters()
    {
        base.ValidateParameters();
        CurrentParameterSetIs("SwitchName");
    }

    internal override IList<VMSwitchExtension> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        IEnumerable<VMSwitch> inputs = ((!CurrentParameterSetIs("SwitchName")) ? VMSwitch : global::Microsoft.HyperV.PowerShell.VMSwitch.GetSwitchesByNamesAndServers(ParameterResolvers.GetServers(this, operationWatcher), VMSwitchName, allowWildcards: true, ErrorDisplayMode.WriteWarning, operationWatcher));
        IEnumerable<VMSwitchExtension> inputs2 = inputs.SelectManyWithLogging((VMSwitch virtualSwitch) => virtualSwitch.Extensions, operationWatcher);
        inputs2 = inputs2.SelectManyWithLogging((VMSwitchExtension extension) => extension.ExpandExtensionTree(), operationWatcher);
        if (!Name.IsNullOrEmpty())
        {
            WildcardPatternMatcher matcher = new WildcardPatternMatcher(Name);
            inputs2 = inputs2.Where((VMSwitchExtension extension) => matcher.MatchesAny(extension.Name));
        }
        return inputs2.ToList();
    }

    internal override void ProcessOneOperand(VMSwitchExtension operand, IOperationWatcher operationWatcher)
    {
        operationWatcher.WriteObject(operand);
    }
}
