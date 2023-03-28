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

[Cmdlet("Set", "VMKeyStorageDrive", SupportsShouldProcess = true, DefaultParameterSetName = "VMName")]
[OutputType(new Type[] { typeof(KeyStorageDrive) })]
internal sealed class SetVMKeyStorageDrive : VirtualizationCmdlet<KeyStorageDrive>, ISupportsPassthrough, IVmBySingularVMNameCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet
{
	internal static class AdditionalParameterSetNames
	{
		public const string Object = "Object";
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
	[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "Object")]
	public KeyStorageDrive[] VMKeyStorageDrive { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "VMName", Position = 0, Mandatory = true)]
	public string VMName { get; set; }

	[ValidateNotNull]
	[Parameter(ParameterSetName = "VMName", Position = 1)]
	public int? ControllerNumber { get; set; }

	[ValidateNotNull]
	[Parameter(ParameterSetName = "VMName", Position = 2)]
	public int? ControllerLocation { get; set; }

	[ValidateNotNull]
	[Parameter]
	public int? ToControllerNumber { get; set; }

	[ValidateNotNull]
	[Parameter]
	public int? ToControllerLocation { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter]
	public string ResourcePoolName { get; set; }

	[Parameter]
	public SwitchParameter AllowUnverifiedPaths { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
	[Parameter]
	public SwitchParameter Passthru { get; set; }

	internal override IList<KeyStorageDrive> EnumerateOperands(IOperationWatcher operationWatcher)
	{
		operationWatcher.WriteWarning(string.Format(CultureInfo.CurrentCulture, CmdletResources.KeyStorageDriveDeprecatedWarning));
		if (VMKeyStorageDrive != null)
		{
			return VMKeyStorageDrive;
		}
		return ParameterResolvers.ResolveVirtualMachines(this, operationWatcher).SelectManyWithLogging((VirtualMachine vm) => vm.FindDrives(ControllerType.IDE, ControllerNumber, ControllerLocation), operationWatcher).OfType<KeyStorageDrive>()
			.ToList();
	}

	internal override void ProcessOneOperand(KeyStorageDrive keyStorageDrive, IOperationWatcher operationWatcher)
	{
		VirtualMachine parentAs = keyStorageDrive.GetParentAs<VirtualMachine>();
		if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_SetVMKeyStorageDrive, keyStorageDrive, parentAs.Name)))
		{
			KeyStorageDriveConfigurationData originalConfiguration = (KeyStorageDriveConfigurationData)keyStorageDrive.GetCurrentConfiguration();
			KeyStorageDriveConfigurationData requestedConfiguration = GetRequestedConfiguration(keyStorageDrive, parentAs, originalConfiguration);
			keyStorageDrive.Configure(requestedConfiguration, originalConfiguration, operationWatcher);
			if (parentAs.IsClustered)
			{
				ClusterUtilities.UpdateClusterVMConfiguration(parentAs, base.InvokeCommand, operationWatcher);
			}
			if (Passthru.IsPresent)
			{
				operationWatcher.WriteObject(keyStorageDrive);
			}
		}
	}

	private KeyStorageDriveConfigurationData GetRequestedConfiguration(KeyStorageDrive keyStorageDrive, VirtualMachine parent, KeyStorageDriveConfigurationData originalConfiguration)
	{
		KeyStorageDriveConfigurationData keyStorageDriveConfigurationData = new KeyStorageDriveConfigurationData(parent);
		Tuple<VMDriveController, int> tuple = VMDriveController.FindControllerVacancyForMove(keyStorageDrive, ControllerType.IDE, ToControllerNumber, ToControllerLocation);
		if (tuple != null)
		{
			keyStorageDriveConfigurationData.Controller = tuple.Item1;
			keyStorageDriveConfigurationData.ControllerLocation = tuple.Item2;
		}
		else
		{
			keyStorageDriveConfigurationData.Controller = originalConfiguration.Controller;
			keyStorageDriveConfigurationData.ControllerLocation = originalConfiguration.ControllerLocation;
		}
		keyStorageDriveConfigurationData.ResourcePoolName = ResourcePoolName ?? originalConfiguration.ResourcePoolName;
		return keyStorageDriveConfigurationData;
	}
}
