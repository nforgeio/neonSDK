using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Management.Infrastructure;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Measure", "VMResourcePool")]
[OutputType(new Type[] { typeof(VMMeteringReportForResourcePool) })]
internal sealed class MeasureVMResourcePool : VirtualizationCmdlet<VMMeteringReportForResourcePool>
{
    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ValueFromPipelineByPropertyName = true)]
    [ValidateNotNullOrEmpty]
    public override CimSession[] CimSession { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter]
    [ValidateNotNullOrEmpty]
    public override string[] ComputerName { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter]
    [ValidateNotNullOrEmpty]
    [CredentialArray]
    public override PSCredential[] Credential { get; set; }

    [ValidateNotNullOrEmpty]
    [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)]
    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Arrays are more user friendly, and this is by spec.")]
    public string[] Name { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [ValidateSet(new string[] { "Ethernet", "Memory", "Processor", "VHD" }, IgnoreCase = true)]
    [Parameter(Position = 1, ValueFromPipelineByPropertyName = true)]
    public VMResourcePoolType[] ResourcePoolType { get; set; }

    protected override void ValidateParameters()
    {
        base.ValidateParameters();
    }

    internal override IList<VMMeteringReportForResourcePool> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        return ParameterResolvers.GetServers(this, operationWatcher).SelectManyWithLogging((Server server) => EnumerateOperandsByServer(server, operationWatcher), operationWatcher).ToList();
    }

    internal override void ProcessOneOperand(VMMeteringReportForResourcePool operand, IOperationWatcher operationWatcher)
    {
        operationWatcher.WriteObject(operand);
    }

    private IEnumerable<VMMeteringReportForResourcePool> EnumerateOperandsByServer(Server server, IOperationWatcher operationWatcher)
    {
        return from pool in VMResourcePool.GetVMResourcePools(server, Name, allowWildcards: true, ResourcePoolType).OfType<IMeasurableResourcePool>()
            where ResourceMeteringIsEnabled(pool, operationWatcher)
            group pool by pool.Name into @group
            select new VMMeteringReportForResourcePool(server, @group.ToDictionary((IMeasurableResourcePool pool) => pool.ResourcePoolType));
    }

    private bool ResourceMeteringIsEnabled(IMeasurableResourcePool resourcePool, IOperationWatcher operationWatcher)
    {
        bool resourceMeteringEnabled = resourcePool.ResourceMeteringEnabled;
        if (!resourceMeteringEnabled)
        {
            string message = string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.InvalidParameter_MetricsDisabledForResourcePool, resourcePool.Name, resourcePool.ResourcePoolType);
            if (Name.Length != 1 || WildcardPattern.ContainsWildcardCharacters(Name[0]) || !IsParameterSpecified("ResourcePoolType"))
            {
                operationWatcher.WriteWarning(message);
                return resourceMeteringEnabled;
            }
            throw ExceptionHelper.CreateInvalidArgumentException(message);
        }
        return resourceMeteringEnabled;
    }
}
