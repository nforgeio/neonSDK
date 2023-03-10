using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Management.Infrastructure;
using Microsoft.Virtualization.Client.Common;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell.Commands;

internal static class ParameterResolvers
{
	internal static class ReplicationCmdlets
	{
		internal static bool MatchReplicationSet(VMReplication replication, [Optional] string replicaServerName, [Optional] string primaryServerName, [Optional] VMReplicationState? replicationState, [Optional] VMReplicationHealthState? replicationHealth, [Optional] VMReplicationMode? replicationMode, [Optional] string trustGroup)
		{
			if (!string.IsNullOrEmpty(replicaServerName) && !IsFQDNOrNameMatching(replicaServerName, replication.ReplicaServerName))
			{
				return false;
			}
			if (!string.IsNullOrEmpty(primaryServerName) && !IsFQDNOrNameMatching(primaryServerName, replication.PrimaryServerName))
			{
				return false;
			}
			if (replicationState.HasValue && !replication.ReplicationState.Equals(replicationState.Value))
			{
				return false;
			}
			if (replicationHealth.HasValue && !replication.ReplicationHealth.Equals(replicationHealth.Value))
			{
				return false;
			}
			if (replicationMode.HasValue && !replication.ReplicationMode.Equals(replicationMode.Value))
			{
				return false;
			}
			if (!string.IsNullOrEmpty(trustGroup))
			{
				VMReplicationAuthorizationEntry authorizationEntry = replication.AuthorizationEntry;
				if (authorizationEntry == null || !WildcardPatternMatcher.IsPatternMatching(trustGroup, authorizationEntry.TrustGroup))
				{
					return false;
				}
			}
			return true;
		}

		private static bool IsFQDNOrNameMatching(string pattern, string value)
		{
			if (value == null)
			{
				return false;
			}
			if (WildcardPatternMatcher.IsPatternMatching(pattern, value))
			{
				return true;
			}
			int num = value.IndexOf('.');
			if (num >= 0)
			{
				value = value.Remove(num);
				if (WildcardPatternMatcher.IsPatternMatching(pattern, value))
				{
					return true;
				}
			}
			return false;
		}
	}

	private const string gm_MacAddressPatternString = "^([0-9A-F]{2}[:-]?){5}([0-9A-F]{2})$";

	internal static VirtualMachineParameterType GetVirtualMachineParameterType(IMoveOrCompareVMCmdlet moveOrCompareVmCmdlet)
	{
		VirtualMachineParameterType result = VirtualMachineParameterType.None;
		if (moveOrCompareVmCmdlet.CurrentParameterSetIs("NameSingleDestination") || moveOrCompareVmCmdlet.CurrentParameterSetIs("NameSingleDestinationAndCimSession") || moveOrCompareVmCmdlet.CurrentParameterSetIs("NameMultipleDestinations") || moveOrCompareVmCmdlet.CurrentParameterSetIs("NameMultipleDestinationsAndCimSession"))
		{
			result = VirtualMachineParameterType.SingularName;
		}
		else if (moveOrCompareVmCmdlet.CurrentParameterSetIs("VMSingleDestination") || moveOrCompareVmCmdlet.CurrentParameterSetIs("VMSingleDestinationAndCimSession") || moveOrCompareVmCmdlet.CurrentParameterSetIs("VMMultipleDestinations") || moveOrCompareVmCmdlet.CurrentParameterSetIs("VMMultipleDestinationsAndCimSession"))
		{
			result = VirtualMachineParameterType.SingularVMObject;
		}
		return result;
	}

	internal static bool IsMovingToSingleLocation(IMoveOrCompareVMCmdlet cmdlet)
	{
		if (!cmdlet.CurrentParameterSetIs("NameSingleDestination") && !cmdlet.CurrentParameterSetIs("NameSingleDestinationAndCimSession") && !cmdlet.CurrentParameterSetIs("VMSingleDestination"))
		{
			return cmdlet.CurrentParameterSetIs("VMSingleDestinationAndCimSession");
		}
		return true;
	}

	internal static IEnumerable<Server> GetServers(IServerParameters cmdletParameters, IOperationWatcher operationWatcher)
	{
		bool num = !cmdletParameters.CimSession.IsNullOrEmpty();
		bool flag = !cmdletParameters.ComputerName.IsNullOrEmpty();
		bool flag2 = !cmdletParameters.Credential.IsNullOrEmpty();
		IEnumerable<Server> source;
		if (num)
		{
			source = cmdletParameters.CimSession.SelectWithLogging(VirtualizationObjectLocator.GetServer, operationWatcher);
		}
		else if (flag)
		{
			if (!flag2)
			{
				source = cmdletParameters.ComputerName.SelectWithLogging(VirtualizationObjectLocator.GetServer, operationWatcher);
			}
			else if (cmdletParameters.Credential.Length == 1)
			{
				PSCredential pSCredential = cmdletParameters.Credential[0];
				IUserPassCredential credential = UserPassCredential.Create(pSCredential.UserName, pSCredential.Password);
				source = cmdletParameters.ComputerName.SelectWithLogging((string computerName) => VirtualizationObjectLocator.GetServer(computerName, credential), operationWatcher);
			}
			else
			{
				source = cmdletParameters.ComputerName.ZipWithLogging(cmdletParameters.Credential, VirtualizationObjectLocator.GetServer, operationWatcher);
			}
		}
		else
		{
			source = new Server[1] { VirtualizationObjectLocator.GetServer(Environment.GetEnvironmentVariable("COMPUTERNAME")) };
		}
		return source.Distinct();
	}

	internal static Server GetDestinationServer(IDestinationServerParameters cmdletParameters)
	{
		bool num = cmdletParameters.DestinationCimSession != null;
		bool flag = !cmdletParameters.DestinationHost.IsNullOrEmpty();
		Server result = null;
		if (num)
		{
			result = VirtualizationObjectLocator.GetDestinationServer(cmdletParameters.DestinationCimSession);
		}
		else if (flag)
		{
			result = VirtualizationObjectLocator.GetDestinationServer(cmdletParameters.DestinationHost, cmdletParameters.DestinationCredential);
		}
		return result;
	}

	internal static IList<VirtualMachine> ResolveVirtualMachines(IVirtualMachineCmdlet vmCmdlet, IOperationWatcher operationWatcher)
	{
		return ResolveVirtualMachines(vmCmdlet, operationWatcher, ErrorDisplayMode.WriteError);
	}

	internal static IList<VirtualMachine> ResolveVirtualMachines(IVirtualMachineCmdlet vmCmdlet, IOperationWatcher operationWatcher, ErrorDisplayMode errorDisplayMode)
	{
		string[] vmNames;
		switch (vmCmdlet.VirtualMachineParameterType)
		{
		case VirtualMachineParameterType.VMObject:
			return (vmCmdlet as IVmByObjectCmdlet).VM;
		case VirtualMachineParameterType.SingularVMObject:
		{
			IVmBySingularObjectCmdlet vmBySingularObjectCmdlet = vmCmdlet as IVmBySingularObjectCmdlet;
			return new VirtualMachine[1] { vmBySingularObjectCmdlet.VM };
		}
		case VirtualMachineParameterType.VMId:
		{
			IVmByVMIdCmdlet vmByVMIdCmdlet = vmCmdlet as IVmByVMIdCmdlet;
			return VirtualizationObjectLocator.GetVirtualMachinesByIdsAndServers(GetServers(vmByVMIdCmdlet, operationWatcher).ToList(), vmByVMIdCmdlet.VMId, errorDisplayMode, operationWatcher);
		}
		case VirtualMachineParameterType.Name:
			vmNames = (vmCmdlet as IVmByNameCmdlet).Name;
			break;
		case VirtualMachineParameterType.SingularName:
		{
			IVmBySingularNameCmdlet vmBySingularNameCmdlet = vmCmdlet as IVmBySingularNameCmdlet;
			vmNames = new string[1] { vmBySingularNameCmdlet.Name };
			break;
		}
		case VirtualMachineParameterType.VMName:
			vmNames = (vmCmdlet as IVmByVMNameCmdlet).VMName;
			break;
		default:
		{
			IVmBySingularVMNameCmdlet vmBySingularVMNameCmdlet = vmCmdlet as IVmBySingularVMNameCmdlet;
			vmNames = new string[1] { vmBySingularVMNameCmdlet.VMName };
			break;
		}
		}
		return VirtualizationObjectLocator.GetVirtualMachinesByNamesAndServers(GetServers(vmCmdlet, operationWatcher).ToList(), vmNames, allowWildcards: true, errorDisplayMode, operationWatcher).ToList();
	}

	internal static VMCompatibilityReport GenerateCompatibilityReport(IImportOrCompareVMCmdlet importOrCompareVmCmdlet, Server server, IOperationWatcher operationWatcher)
	{
		if (importOrCompareVmCmdlet.CurrentParameterSetIs("Register"))
		{
			return VMCompatibilityReport.CreateReportForRegisterImport(server, importOrCompareVmCmdlet.Path, operationWatcher);
		}
		return VMCompatibilityReport.CreateReportForCopyImport(server, importOrCompareVmCmdlet.Path, importOrCompareVmCmdlet.GenerateNewId.ToBool(), importOrCompareVmCmdlet.VirtualMachinePath, importOrCompareVmCmdlet.SnapshotFilePath, importOrCompareVmCmdlet.SmartPagingFilePath, importOrCompareVmCmdlet.VhdDestinationPath, importOrCompareVmCmdlet.VhdSourcePath, operationWatcher);
	}

	internal static IEnumerable<VMCompatibilityReport> GenerateCompatibilityReports(IImportOrCompareVMCmdlet importOrCompareVmCmdlet, IOperationWatcher operationWatcher)
	{
		return GetServers(importOrCompareVmCmdlet, operationWatcher).SelectWithLogging((Server server) => GenerateCompatibilityReport(importOrCompareVmCmdlet, server, operationWatcher), operationWatcher);
	}

	internal static void GetHbaNames(IVMSanCmdlet sanCmdlet, out IEnumerable<string> nodeWwns, out IEnumerable<string> portWwns)
	{
		if (sanCmdlet.CurrentParameterSetIs("HbaObject"))
		{
			if (sanCmdlet.HostBusAdapter == null)
			{
				nodeWwns = VMSan.EmptyWwns;
				portWwns = VMSan.EmptyWwns;
				return;
			}
			List<string> list = new List<string>();
			List<string> list2 = new List<string>();
			CimInstance[] hostBusAdapter = sanCmdlet.HostBusAdapter;
			foreach (CimInstance cimInstance in hostBusAdapter)
			{
				list.Add((string)cimInstance.CimInstanceProperties["NodeAddress"].Value);
				list2.Add((string)cimInstance.CimInstanceProperties["PortAddress"].Value);
			}
			nodeWwns = list.AsReadOnly();
			portWwns = list2.AsReadOnly();
		}
		else
		{
			nodeWwns = sanCmdlet.WorldWideNodeName;
			portWwns = sanCmdlet.WorldWidePortName;
		}
	}

	internal static IList<VMNetworkAdapterBase> ResolveNetworkAdapters(IVMNetworkAdapterBaseCmdlet cmdlet, string nameFilter, IOperationWatcher operationWatcher)
	{
		if (cmdlet.CurrentParameterSetIs("ResourceObject"))
		{
			return (cmdlet as IVMNetworkAdapterBaseByObjectCmdlet).VMNetworkAdapter;
		}
		IEnumerable<VMNetworkAdapterBase> enumerable;
		if (cmdlet.CurrentParameterSetIs("VMSnapshot"))
		{
			enumerable = (cmdlet as IVMSnapshotCmdlet).VMSnapshot.GetNetworkAdapters();
		}
		else if (cmdlet.CurrentParameterSetIs("ManagementOS"))
		{
			if (cmdlet is IVMInternalNetworkAdapterCmdlet cmdletParameters)
			{
				enumerable = VirtualizationObjectLocator.GetVMHosts(GetServers(cmdletParameters, operationWatcher), operationWatcher).SelectManyWithLogging((VMHost host) => host.InternalNetworkAdapters, operationWatcher);
			}
			else
			{
				IVMInternalNetworkAdapterBySwitchNameCmdlet iVMInternalNetworkAdapterBySwitchNameCmdlet = cmdlet as IVMInternalNetworkAdapterBySwitchNameCmdlet;
				IEnumerable<Server> servers = GetServers(iVMInternalNetworkAdapterBySwitchNameCmdlet, operationWatcher);
				string[] requestedSwitchNames = (string.IsNullOrEmpty(iVMInternalNetworkAdapterBySwitchNameCmdlet.SwitchName) ? null : new string[1] { iVMInternalNetworkAdapterBySwitchNameCmdlet.SwitchName });
				enumerable = VMSwitch.GetSwitchesByNamesAndServers(servers, requestedSwitchNames, allowWildcards: true, ErrorDisplayMode.WriteWarning, operationWatcher).SelectManyWithLogging((VMSwitch virtualSwitch) => virtualSwitch.GetInternalPorts(), operationWatcher);
			}
		}
		else if (cmdlet.CurrentParameterSetIs("ExternalPort"))
		{
			IVMExternalSwitchPortCmdlet iVMExternalSwitchPortCmdlet = cmdlet as IVMExternalSwitchPortCmdlet;
			IEnumerable<Server> servers2 = GetServers(iVMExternalSwitchPortCmdlet, operationWatcher);
			string[] requestedSwitchNames2 = (string.IsNullOrEmpty(iVMExternalSwitchPortCmdlet.SwitchName) ? null : new string[1] { iVMExternalSwitchPortCmdlet.SwitchName });
			enumerable = VMSwitch.GetSwitchesByNamesAndServers(servers2, requestedSwitchNames2, allowWildcards: true, ErrorDisplayMode.WriteWarning, operationWatcher).SelectWithLogging((VMSwitch virtualSwitch) => virtualSwitch.GetExternalPort(), operationWatcher);
		}
		else
		{
			enumerable = ResolveVirtualMachines(cmdlet as IVirtualMachineCmdlet, operationWatcher).SelectManyWithLogging((VirtualMachine vm) => vm.GetNetworkAdapters(), operationWatcher);
		}
		if (!string.IsNullOrEmpty(nameFilter))
		{
			enumerable = FilterNetworkAdaptersByName(enumerable, nameFilter);
		}
		return enumerable.ToList();
	}

	internal static IList<VMNetworkAdapterBase> FilterNetworkAdaptersByName(IEnumerable<VMNetworkAdapterBase> networkAdapters, string nameFilter)
	{
		WildcardPattern namePattern = new WildcardPattern(nameFilter, WildcardOptions.Compiled | WildcardOptions.IgnoreCase);
		List<VMNetworkAdapterBase> list = networkAdapters.Where((VMNetworkAdapterBase adapter) => namePattern.IsMatch(adapter.Name)).ToList();
		if (list.Count == 0)
		{
			throw ExceptionHelper.CreateObjectNotFoundException(CmdletErrorMessages.VMNetworkAdapter_NoneFound, null);
		}
		return list;
	}

	internal static string ValidateAndNormalizeMacAddress(string macAddress, string parameterName)
	{
		string text = null;
		if (new Regex("^([0-9A-F]{2}[:-]?){5}([0-9A-F]{2})$", RegexOptions.IgnoreCase).IsMatch(macAddress))
		{
			return macAddress.ToUpperInvariant().Replace(":", string.Empty).Replace("-", string.Empty);
		}
		throw ParameterValidator.CreateInvalidParameterFormatException(parameterName);
	}
}
