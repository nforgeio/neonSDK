using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Text.RegularExpressions;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.Management.Infrastructure;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Set", "VMNetworkAdapterVlan", DefaultParameterSetName = "VMName", SupportsShouldProcess = true)]
[OutputType(new Type[] { typeof(VMNetworkAdapterVlanSetting) })]
internal sealed class SetVMNetworkAdapterVlan : VirtualizationCmdlet<VMNetworkAdapterBase>, IVMObjectOrVMNameCmdlet, IVmByObjectCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmByVMNameCmdlet, IVMInternalNetworkAdapterCmdlet, IVMNetworkAdapterBaseCmdlet, IVMNetworkAdapterBaseByObjectCmdlet, ISupportsPassthrough
{
	private VlanParameterSetType m_VlanParameterSet;

	private static Regex gm_VlanIdRangeRegex = new Regex("^(\\d+)-(\\d+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

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

	[Parameter]
	[Alias(new string[] { "u" })]
	public SwitchParameter Untagged { get; set; }

	[Parameter]
	[Alias(new string[] { "a" })]
	public SwitchParameter Access { get; set; }

	[ValidateNotNull]
	[Parameter]
	[Alias(new string[] { "AccessVlanId" })]
	public int? VlanId { get; set; }

	[Parameter]
	[Alias(new string[] { "t" })]
	public SwitchParameter Trunk { get; set; }

	[ValidateNotNull]
	[Parameter]
	public int? NativeVlanId { get; set; }

	[Parameter]
	public string AllowedVlanIdList { get; set; }

	[Parameter]
	[Alias(new string[] { "i" })]
	public SwitchParameter Isolated { get; set; }

	[Parameter]
	[Alias(new string[] { "c" })]
	public SwitchParameter Community { get; set; }

	[Parameter]
	[Alias(new string[] { "p" })]
	public SwitchParameter Promiscuous { get; set; }

	[ValidateNotNull]
	[Parameter]
	public int? PrimaryVlanId { get; set; }

	[ValidateNotNull]
	[Parameter]
	public int? SecondaryVlanId { get; set; }

	[Parameter]
	public string SecondaryVlanIdList { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
	[Parameter]
	public SwitchParameter Passthru { get; set; }

	protected override void NormalizeParameters()
	{
		base.NormalizeParameters();
		m_VlanParameterSet = ComputeVlanParameterSet();
	}

	protected override void ValidateParameters()
	{
		base.ValidateParameters();
		if (m_VlanParameterSet == VlanParameterSetType.Unknown || m_VlanParameterSet == VlanParameterSetType.MissingParameter)
		{
			throw ExceptionHelper.CreateInvalidArgumentException("Missing parameter");
		}
		if (m_VlanParameterSet == VlanParameterSetType.ConflictingParameters)
		{
			throw ExceptionHelper.CreateInvalidArgumentException("Conflicting parameters");
		}
	}

	internal override IList<VMNetworkAdapterBase> EnumerateOperands(IOperationWatcher operationWatcher)
	{
		return ParameterResolvers.ResolveNetworkAdapters(this, VMNetworkAdapterName, operationWatcher);
	}

	internal override void ProcessOneOperand(VMNetworkAdapterBase operand, IOperationWatcher operationWatcher)
	{
		if (!operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_SetVMNetworkAdapterVlan, operand.Name)))
		{
			return;
		}
		VMNetworkAdapterVlanSetting vlanSetting = operand.VlanSetting;
		if (m_VlanParameterSet == VlanParameterSetType.Untagged)
		{
			if (!vlanSetting.IsTemplate)
			{
				((IRemovable)vlanSetting).Remove(operationWatcher);
			}
			return;
		}
		if (!vlanSetting.IsTemplate)
		{
			vlanSetting.ClearSettings();
		}
		switch (m_VlanParameterSet)
		{
		case VlanParameterSetType.Access:
			vlanSetting.OperationMode = VMNetworkAdapterVlanMode.Access;
			vlanSetting.AccessVlanId = VlanId.Value;
			break;
		case VlanParameterSetType.Trunk:
			vlanSetting.OperationMode = VMNetworkAdapterVlanMode.Trunk;
			vlanSetting.NativeVlanId = NativeVlanId.Value;
			vlanSetting.AllowedVlanIdList = ParseVlanIdList(AllowedVlanIdList);
			break;
		case VlanParameterSetType.PrivateIsolated:
			vlanSetting.OperationMode = VMNetworkAdapterVlanMode.Private;
			vlanSetting.PrivateVlanMode = VMNetworkAdapterPrivateVlanMode.Isolated;
			vlanSetting.PrimaryVlanId = PrimaryVlanId.Value;
			vlanSetting.SecondaryVlanId = SecondaryVlanId.Value;
			break;
		case VlanParameterSetType.PrivateCommunity:
			vlanSetting.OperationMode = VMNetworkAdapterVlanMode.Private;
			vlanSetting.PrivateVlanMode = VMNetworkAdapterPrivateVlanMode.Community;
			vlanSetting.PrimaryVlanId = PrimaryVlanId.Value;
			vlanSetting.SecondaryVlanId = SecondaryVlanId.Value;
			break;
		case VlanParameterSetType.PrivatePromiscuous:
			vlanSetting.OperationMode = VMNetworkAdapterVlanMode.Private;
			vlanSetting.PrivateVlanMode = VMNetworkAdapterPrivateVlanMode.Promiscuous;
			vlanSetting.PrimaryVlanId = PrimaryVlanId.Value;
			vlanSetting.SecondaryVlanIdList = ParseVlanIdList(SecondaryVlanIdList);
			break;
		}
		VMNetworkAdapterVlanSetting output = operand.AddOrModifyOneFeatureSetting(vlanSetting, operationWatcher);
		if (Passthru.IsPresent)
		{
			operationWatcher.WriteObject(output);
		}
	}

	private static List<int> ParseVlanIdList(string vlanIdListString)
	{
		List<int> list = new List<int>();
		string[] array = vlanIdListString.Split(',');
		for (int i = 0; i < array.Length; i++)
		{
			string text = array[i].Trim();
			if (gm_VlanIdRangeRegex.IsMatch(text))
			{
				Match match = gm_VlanIdRangeRegex.Match(text);
				int num = int.Parse(match.Groups[1].Value, NumberStyles.Integer, CultureInfo.CurrentCulture);
				int num2 = int.Parse(match.Groups[2].Value, NumberStyles.Integer, CultureInfo.CurrentCulture);
				if (num2 <= num)
				{
					throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.SetVMNetworkAdapterVlan_InvalidVlanIdListFormat);
				}
				list.AddRange(Enumerable.Range(num, num2 + 1 - num));
			}
			else
			{
				if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.CurrentCulture, out var result))
				{
					throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.SetVMNetworkAdapterVlan_InvalidVlanIdListFormat);
				}
				list.Add(result);
			}
		}
		return list.Distinct().ToList();
	}

	private VlanParameterSetType MatchOneParameterSet(bool[] specifiedParameters, VlanParameterSetType parameterSet)
	{
		if (specifiedParameters.All((bool parameterExists) => parameterExists))
		{
			return parameterSet;
		}
		if (specifiedParameters.All((bool parameterExists) => !parameterExists))
		{
			return VlanParameterSetType.Unknown;
		}
		return VlanParameterSetType.MissingParameter;
	}

	private VlanParameterSetType ComputeVlanParameterSet()
	{
		bool[] specifiedParameters = new bool[1] { Untagged.IsPresent };
		bool[] specifiedParameters2 = new bool[2] { Access.IsPresent, VlanId.HasValue };
		bool[] specifiedParameters3 = new bool[3]
		{
			Trunk.IsPresent,
			NativeVlanId.HasValue,
			AllowedVlanIdList != null
		};
		List<VlanParameterSetType> list = new List<VlanParameterSetType>(4)
		{
			MatchOneParameterSet(specifiedParameters, VlanParameterSetType.Untagged),
			MatchOneParameterSet(specifiedParameters2, VlanParameterSetType.Access),
			MatchOneParameterSet(specifiedParameters3, VlanParameterSetType.Trunk),
			ComputePrivateVlanParameterSet()
		};
		list.RemoveAll((VlanParameterSetType parameterType) => parameterType == VlanParameterSetType.Unknown);
		if (list.Count == 0)
		{
			return VlanParameterSetType.Unknown;
		}
		if (list.Count > 1)
		{
			return VlanParameterSetType.ConflictingParameters;
		}
		return list.Single();
	}

	private VlanParameterSetType ComputePrivateVlanParameterSet()
	{
		bool[] specifiedParameters = new bool[3] { Isolated.IsPresent, PrimaryVlanId.HasValue, SecondaryVlanId.HasValue };
		bool[] specifiedParameters2 = new bool[3] { Community.IsPresent, PrimaryVlanId.HasValue, SecondaryVlanId.HasValue };
		bool[] specifiedParameters3 = new bool[3]
		{
			Promiscuous.IsPresent,
			PrimaryVlanId.HasValue,
			SecondaryVlanIdList != null
		};
		List<VlanParameterSetType> list = new List<VlanParameterSetType>(3)
		{
			MatchOneParameterSet(specifiedParameters, VlanParameterSetType.PrivateIsolated),
			MatchOneParameterSet(specifiedParameters2, VlanParameterSetType.PrivateCommunity),
			MatchOneParameterSet(specifiedParameters3, VlanParameterSetType.PrivatePromiscuous)
		};
		list.RemoveAll((VlanParameterSetType parameterType) => parameterType == VlanParameterSetType.Unknown);
		if (list.Count == 0)
		{
			return VlanParameterSetType.Unknown;
		}
		List<VlanParameterSetType> list2 = list.FindAll((VlanParameterSetType t) => t != VlanParameterSetType.MissingParameter);
		if (list2.Count == 1)
		{
			return list2.Single();
		}
		if (list.Count > 1)
		{
			return VlanParameterSetType.ConflictingParameters;
		}
		return VlanParameterSetType.MissingParameter;
	}
}
