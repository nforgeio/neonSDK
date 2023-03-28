using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.Management.Infrastructure;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Copy", "VMFile", DefaultParameterSetName = "Name", SupportsShouldProcess = true)]
internal sealed class CopyVMFile : VirtualizationCmdlet<VirtualMachine>, ISupportsForce, ISupportsAsJob, IVMObjectOrNameCmdlet, IVmByObjectCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmByNameCmdlet
{
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

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[ValidateNotNullOrEmpty]
	[Alias(new string[] { "VMName" })]
	[Parameter(ParameterSetName = "Name", Mandatory = true, Position = 0)]
	public string[] Name { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "VMObject", ValueFromPipeline = true, Position = 0, Mandatory = true)]
	public VirtualMachine[] VM { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter(Mandatory = true, Position = 1, ValueFromPipeline = true)]
	public string SourcePath { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter(Mandatory = true, Position = 2)]
	public string DestinationPath { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter(Mandatory = true)]
	public CopyFileSourceType FileSource { get; set; }

	[Parameter]
	public SwitchParameter CreateFullPath { get; set; }

	[Parameter]
	public SwitchParameter Force { get; set; }

	[Parameter]
	public SwitchParameter AsJob { get; set; }

	internal override IList<VirtualMachine> EnumerateOperands(IOperationWatcher operationWatcher)
	{
		return ParameterResolvers.ResolveVirtualMachines(this, operationWatcher);
	}

	internal override void ProcessOneOperand(VirtualMachine operand, IOperationWatcher operationWatcher)
	{
		if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_CopyVMFile, operand.Name)))
		{
			operand.GetVMIntegrationComponents().OfType<GuestServiceInterfaceComponent>().First()
				.CopyFileToGuest(PathUtility.GetFullPath(SourcePath, base.CurrentFileSystemLocation), DestinationPath, Force.IsPresent, CreateFullPath.IsPresent, operationWatcher);
		}
	}
}
