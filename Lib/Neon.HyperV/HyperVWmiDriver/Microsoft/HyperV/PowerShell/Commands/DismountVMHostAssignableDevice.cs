using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.Management.Infrastructure;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Dismount", "VMHostAssignableDevice", SupportsShouldProcess = true, DefaultParameterSetName = "ComputerName")]
[OutputType(new Type[] { typeof(VMHostAssignableDevice) })]
internal sealed class DismountVMHostAssignableDevice : VirtualizationCmdlet<Server>, ISupportsPassthrough, ISupportsForce
{
    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [ValidateNotNullOrEmpty]
    public override CimSession[] CimSession { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [ValidateNotNullOrEmpty]
    public override string[] ComputerName { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [ValidateNotNullOrEmpty]
    [CredentialArray]
    public override PSCredential[] Credential { get; set; }

    [Parameter(Position = 2)]
    public string InstancePath { get; set; }

    [Parameter(Position = 3)]
    public string LocationPath { get; set; }

    [Parameter(Position = 4)]
    public SwitchParameter Force { get; set; }

    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
    [Parameter]
    public SwitchParameter Passthru { get; set; }

    internal override IList<Server> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        return ParameterResolvers.GetServers(this, operationWatcher).ToList();
    }

    internal override void ProcessOneOperand(Server operand, IOperationWatcher operationWatcher)
    {
        bool isPresent = Force.IsPresent;
        if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_DismountHostAssignableDevice)))
        {
            VMHostAssignableDevice output = VMHostAssignableDevice.Dismount(operand, InstancePath, LocationPath, !isPresent, !isPresent);
            if (Passthru.IsPresent)
            {
                operationWatcher.WriteObject(output);
            }
        }
    }
}
