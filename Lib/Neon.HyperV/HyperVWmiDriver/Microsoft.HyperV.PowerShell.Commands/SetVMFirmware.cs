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

[Cmdlet("Set", "VMFirmware", DefaultParameterSetName = "VMName", SupportsShouldProcess = true)]
[OutputType(new Type[] { typeof(VMFirmware) })]
internal sealed class SetVMFirmware : VirtualizationCmdlet<VMFirmware>, IVMObjectOrVMNameCmdlet, IVmByObjectCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmByVMNameCmdlet, ISupportsPassthrough
{
	private static class ParameterSetNames
	{
		public const string FirmwareObject = "VMFirmware";
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
	[Parameter(ParameterSetName = "VMFirmware", ValueFromPipeline = true, Position = 0, Mandatory = true)]
	public VMFirmware[] VMFirmware { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter]
	public VMComponentObject[] BootOrder { get; set; }

	[Parameter]
	public VMComponentObject FirstBootDevice { get; set; }

	[Parameter]
	public OnOffState? EnableSecureBoot { get; set; }

	[Parameter]
	public string SecureBootTemplate { get; set; }

	[Parameter]
	public Guid? SecureBootTemplateId { get; set; }

	[Parameter]
	public IPProtocolPreference? PreferredNetworkBootProtocol { get; set; }

	[Parameter]
	public ConsoleModeType? ConsoleMode { get; set; }

	[Parameter]
	public OnOffState? PauseAfterBootFailure { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
	[Parameter]
	public SwitchParameter Passthru { get; set; }

	protected override void ValidateParameters()
	{
		base.ValidateParameters();
		if (IsParameterSpecified("BootOrder") && IsParameterSpecified("FirstBootDevice"))
		{
			throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.SetVMFirmware_BootOrderAndFirstBootEntryBothProvided);
		}
		if (IsParameterSpecified("SecureBootTemplate") && IsParameterSpecified("SecureBootTemplateId"))
		{
			throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.SetVMFirmware_SecureBootTemplateAndSecureBootTemplateIdBothProvided);
		}
	}

	internal override IList<VMFirmware> EnumerateOperands(IOperationWatcher operationWatcher)
	{
		IEnumerable<VMFirmware> source = ((!CurrentParameterSetIs("VMFirmware")) ? ParameterResolvers.ResolveVirtualMachines(this, operationWatcher).SelectWithLogging((VirtualMachine vm) => vm.GetFirmware(), operationWatcher) : VMFirmware);
		return source.ToList();
	}

	internal override void ProcessOneOperand(VMFirmware firmware, IOperationWatcher operationWatcher)
	{
		if (!operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_SetVMFirmware, firmware.VMName)))
		{
			return;
		}
		if (FirstBootDevice != null)
		{
			VMBootSource firstDevice = ConvertComponentToBootSource(FirstBootDevice);
			VMBootSource[] bootOrder = firmware.BootOrder;
			List<VMBootSource> list = new List<VMBootSource>(bootOrder.Length) { firstDevice };
			list.AddRange(bootOrder.Where((VMBootSource bootSource) => !bootSource.Equals(firstDevice)));
			firmware.BootOrder = list.ToArray();
		}
		if (BootOrder != null && BootOrder.Length != 0)
		{
			VMBootSource[] array2 = (firmware.BootOrder = BootOrder.Select(ConvertComponentToBootSource).ToArray());
		}
		if (EnableSecureBoot.HasValue)
		{
			firmware.SecureBoot = EnableSecureBoot.Value;
		}
		if (SecureBootTemplate != null)
		{
			firmware.SecureBootTemplate = SecureBootTemplate;
		}
		if (SecureBootTemplateId.HasValue)
		{
			firmware.SecureBootTemplateId = SecureBootTemplateId;
		}
		if (PreferredNetworkBootProtocol.HasValue)
		{
			firmware.PreferredNetworkBootProtocol = PreferredNetworkBootProtocol.Value;
		}
		if (ConsoleMode.HasValue)
		{
			firmware.ConsoleMode = ConsoleMode.Value;
		}
		if (PauseAfterBootFailure.HasValue)
		{
			firmware.PauseAfterBootFailure = PauseAfterBootFailure.Value;
		}
		((IUpdatable)firmware).Put(operationWatcher);
		if (Passthru.IsPresent)
		{
			operationWatcher.WriteObject(firmware);
		}
	}

	private static VMBootSource ConvertComponentToBootSource(VMComponentObject component)
	{
		if (component is IBootableDevice bootableDevice)
		{
			return bootableDevice.BootSource;
		}
		throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.InvalidParameter_DeviceNotBootable);
	}
}
