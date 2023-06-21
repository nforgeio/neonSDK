using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Management.Automation;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Get", "VMSystemSwitchExtension")]
[OutputType(new Type[] { typeof(VMSystemSwitchExtension) })]
internal sealed class GetVMSystemSwitchExtension : VirtualizationCmdlet<VMSystemSwitchExtension>
{
    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Array is more user friendly, and this is a user input parameter.")]
    [ValidateNotNullOrEmpty]
    [Parameter(Mandatory = false)]
    public string[] Name { get; set; }

    internal override IList<VMSystemSwitchExtension> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        return VMSystemSwitchExtension.GetExtensionsByNamesAndServers(ParameterResolvers.GetServers(this, operationWatcher), Name, allowWildcards: true, ErrorDisplayMode.WriteWarning, operationWatcher);
    }

    internal override void ProcessOneOperand(VMSystemSwitchExtension operand, IOperationWatcher operationWatcher)
    {
        operationWatcher.WriteObject(operand);
    }
}
