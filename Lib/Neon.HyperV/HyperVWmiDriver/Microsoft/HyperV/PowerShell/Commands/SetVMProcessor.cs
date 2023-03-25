using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.Management.Infrastructure;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Set", "VMProcessor", DefaultParameterSetName = "VMName", SupportsShouldProcess = true)]
[OutputType(new Type[] { typeof(VMProcessor) })]
internal sealed class SetVMProcessor : VirtualizationCmdlet<VMProcessor>, IVMObjectOrVMNameCmdlet, IVmByObjectCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmByVMNameCmdlet, ISupportsPassthrough
{
	private static class ParameterSetNames
	{
		public const string ProcessorObject = "VMProcessor";
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
	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "VMProcessor", ValueFromPipeline = true, Position = 0, Mandatory = true)]
	public VMProcessor[] VMProcessor { get; set; }

	[ValidateNotNull]
	[Parameter]
	public long? Count { get; set; }

	[ValidateNotNull]
	[Parameter]
	public bool? CompatibilityForMigrationEnabled { get; set; }

	[ValidateNotNull]
	[Parameter]
	public bool? CompatibilityForOlderOperatingSystemsEnabled { get; set; }

	[ValidateNotNull]
	[Parameter]
	public long? HwThreadCountPerCore { get; set; }

	[ValidateNotNull]
	[Parameter]
	public long? Maximum { get; set; }

	[ValidateNotNull]
	[Parameter]
	public long? Reserve { get; set; }

	[ValidateNotNull]
	[Parameter]
	public int? RelativeWeight { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Numa", Justification = "This is by spec.")]
	[ValidateNotNull]
	[Parameter]
	public int? MaximumCountPerNumaNode { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Numa", Justification = "This is by spec.")]
	[ValidateNotNull]
	[Parameter]
	public int? MaximumCountPerNumaSocket { get; set; }

	[ValidateNotNull]
	[Parameter]
	public string ResourcePoolName { get; set; }

	[ValidateNotNull]
	[Parameter]
	public bool? EnableHostResourceProtection { get; set; }

	[ValidateNotNull]
	[Parameter]
	public bool? ExposeVirtualizationExtensions { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter]
	public string[] Perfmon { get; set; }

	[ValidateNotNull]
	[Parameter]
	public bool? EnableLegacyApicMode { get; set; }

	[ValidateNotNull]
	[Parameter]
	public bool? AllowACountMCount { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
	[Parameter]
	public SwitchParameter Passthru { get; set; }

	internal override IList<VMProcessor> EnumerateOperands(IOperationWatcher operationWatcher)
	{
		if (CurrentParameterSetIs("VMProcessor"))
		{
			return VMProcessor;
		}
		return (from vm in ParameterResolvers.ResolveVirtualMachines(this, operationWatcher)
			select vm.GetProcessor()).ToList();
	}

	internal override void ProcessOneOperand(VMProcessor processor, IOperationWatcher operationWatcher)
	{
		if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_SetVMProcessor, processor.VMName)))
		{
			if (Count.HasValue)
			{
				processor.Count = Count.Value;
			}
			if (CompatibilityForOlderOperatingSystemsEnabled.HasValue)
			{
				processor.CompatibilityForOlderOperatingSystemsEnabled = CompatibilityForOlderOperatingSystemsEnabled.Value;
			}
			if (HwThreadCountPerCore.HasValue)
			{
				processor.HwThreadCountPerCore = HwThreadCountPerCore.Value;
			}
			if (CompatibilityForMigrationEnabled.HasValue)
			{
				processor.CompatibilityForMigrationEnabled = CompatibilityForMigrationEnabled.Value;
			}
			if (RelativeWeight.HasValue)
			{
				processor.RelativeWeight = RelativeWeight.Value;
			}
			if (Reserve.HasValue)
			{
				processor.Reserve = Reserve.Value;
			}
			if (Maximum.HasValue)
			{
				processor.Maximum = Maximum.Value;
			}
			if (ResourcePoolName != null)
			{
				processor.ResourcePoolName = ResourcePoolName;
			}
			if (MaximumCountPerNumaNode.HasValue)
			{
				processor.MaximumCountPerNumaNode = MaximumCountPerNumaNode.Value;
			}
			if (MaximumCountPerNumaSocket.HasValue)
			{
				processor.MaximumCountPerNumaSocket = MaximumCountPerNumaSocket.Value;
			}
			if (EnableHostResourceProtection.HasValue)
			{
				processor.EnableHostResourceProtection = EnableHostResourceProtection.Value;
			}
			if (ExposeVirtualizationExtensions.HasValue)
			{
				processor.ExposeVirtualizationExtensions = ExposeVirtualizationExtensions.Value;
			}
			if (Perfmon != null)
			{
				processor.EnablePerfmonPmu = Perfmon.Contains("pmu", StringComparer.OrdinalIgnoreCase);
				processor.EnablePerfmonLbr = Perfmon.Contains("lbr", StringComparer.OrdinalIgnoreCase);
				processor.EnablePerfmonPebs = Perfmon.Contains("pebs", StringComparer.OrdinalIgnoreCase);
				processor.EnablePerfmonIpt = Perfmon.Contains("ipt", StringComparer.OrdinalIgnoreCase);
			}
			if (EnableLegacyApicMode.HasValue)
			{
				processor.EnableLegacyApicMode = EnableLegacyApicMode.Value;
			}
			if (AllowACountMCount.HasValue)
			{
				processor.AllowACountMCount = AllowACountMCount.Value;
			}
			((IUpdatable)processor).Put(operationWatcher);
			if ((bool)Passthru)
			{
				operationWatcher.WriteObject(processor);
			}
		}
	}
}
