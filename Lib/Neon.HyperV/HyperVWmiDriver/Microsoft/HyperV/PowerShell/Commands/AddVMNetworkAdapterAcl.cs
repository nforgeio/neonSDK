using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.Management.Infrastructure;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Add", "VMNetworkAdapterAcl", DefaultParameterSetName = "VMName", SupportsShouldProcess = true)]
[OutputType(new Type[] { typeof(VMNetworkAdapterAclSetting) })]
internal sealed class AddVMNetworkAdapterAcl : VirtualizationCmdlet<VMNetworkAdapterBase>, IVMObjectOrVMNameCmdlet, IVmByObjectCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmByVMNameCmdlet, IVMInternalNetworkAdapterCmdlet, IVMNetworkAdapterBaseCmdlet, IVMNetworkAdapterBaseByObjectCmdlet, IVMNetworkAdapterAclCmdlet, ISupportsPassthrough
{
	private VMNetworkAdapterAclDirection[] m_Directions;

	private VMNetworkAdapterAclAddresses m_Addresses;

	[Parameter(Mandatory = true)]
	public VMNetworkAdapterAclAction Action { get; set; }

	[Parameter(Mandatory = true)]
	public VMNetworkAdapterAclDirection Direction { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter]
	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
	public string[] LocalIPAddress { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
	[ValidateNotNullOrEmpty]
	[Parameter]
	public string[] LocalMacAddress { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
	[ValidateNotNullOrEmpty]
	[Parameter]
	public string[] RemoteIPAddress { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
	[ValidateNotNullOrEmpty]
	[Parameter]
	public string[] RemoteMacAddress { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
	[Parameter]
	public SwitchParameter Passthru { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec and array is easier for users to use.")]
	[Parameter(ParameterSetName = "ResourceObject", ValueFromPipeline = true, Position = 0, Mandatory = true)]
	public VMNetworkAdapterBase[] VMNetworkAdapter { get; set; }

	[Parameter(ParameterSetName = "ManagementOS", Mandatory = true)]
	public SwitchParameter ManagementOS { get; set; }

	[Parameter(ParameterSetName = "VMObject")]
	[Parameter(ParameterSetName = "VMName")]
	[Parameter(ParameterSetName = "ManagementOS")]
	public string VMNetworkAdapterName { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "VMName")]
	[Parameter(ParameterSetName = "ManagementOS")]
	[ValidateNotNullOrEmpty]
	public override CimSession[] CimSession { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "VMName")]
	[Parameter(ParameterSetName = "ManagementOS")]
	[ValidateNotNullOrEmpty]
	public override string[] ComputerName { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "VMName")]
	[Parameter(ParameterSetName = "ManagementOS")]
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

	protected override void NormalizeParameters()
	{
		base.NormalizeParameters();
		m_Addresses = VMNetworkAdapterAclAddresses.OrganizeAddressList(this);
		if (Direction == VMNetworkAdapterAclDirection.Both)
		{
			m_Directions = new VMNetworkAdapterAclDirection[2]
			{
				VMNetworkAdapterAclDirection.Inbound,
				VMNetworkAdapterAclDirection.Outbound
			};
		}
		else
		{
			m_Directions = new VMNetworkAdapterAclDirection[1] { Direction };
		}
	}

	protected override void ValidateParameters()
	{
		base.ValidateParameters();
		VMNetworkAdapterAclAddresses.ValidateParameterCount(this);
	}

	internal override IList<VMNetworkAdapterBase> EnumerateOperands(IOperationWatcher operationWatcher)
	{
		return ParameterResolvers.ResolveNetworkAdapters(this, VMNetworkAdapterName, operationWatcher);
	}

	internal override void ProcessOneOperand(VMNetworkAdapterBase operand, IOperationWatcher operationWatcher)
	{
		if (!operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_AddVMNetworkAdapterAcl, operand.Name)))
		{
			return;
		}
		VMNetworkAdapterAclSetting[] features = m_Addresses.AddressList.SelectMany((string address) => InitializeTemplateAcls(address, operand)).ToArray();
		VMNetworkAdapterAclSetting[] array = operand.AddFeatureSettings(features, operationWatcher).ToArray();
		if (Action == VMNetworkAdapterAclAction.Meter)
		{
			MetricUtilities.EnablePortAclMetrics(array);
		}
		if (Passthru.IsPresent)
		{
			VMNetworkAdapterAclSetting[] array2 = array;
			foreach (VMNetworkAdapterAclSetting output in array2)
			{
				operationWatcher.WriteObject(output);
			}
		}
	}

	private IEnumerable<VMNetworkAdapterAclSetting> InitializeTemplateAcls(string address, VMNetworkAdapterBase parentAdapter)
	{
		List<VMNetworkAdapterAclSetting> list = new List<VMNetworkAdapterAclSetting>(m_Directions.Length);
		VMNetworkAdapterAclDirection[] directions = m_Directions;
		foreach (VMNetworkAdapterAclDirection direction in directions)
		{
			VMNetworkAdapterAclSetting vMNetworkAdapterAclSetting = VMNetworkAdapterAclSetting.CreateTemplateAclSetting(parentAdapter);
			vMNetworkAdapterAclSetting.Action = Action;
			vMNetworkAdapterAclSetting.Direction = direction;
			vMNetworkAdapterAclSetting.SetAddress(address, m_Addresses.IsRemote, m_Addresses.IsMacAddress);
			list.Add(vMNetworkAdapterAclSetting);
		}
		return list;
	}
}
