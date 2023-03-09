using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.HyperV.PowerShell.Commands.Resources;

namespace Microsoft.HyperV.PowerShell.Commands;

internal sealed class VMNetworkAdapterAclAddresses
{
	internal List<string> AddressList { get; private set; }

	internal bool IsMacAddress { get; private set; }

	internal bool IsRemote { get; private set; }

	private VMNetworkAdapterAclAddresses()
	{
	}

	internal static void ValidateParameterCount(IVMNetworkAdapterAclCmdlet cmdlet)
	{
		int num = 0;
		if (cmdlet.LocalIPAddress != null)
		{
			num++;
		}
		if (cmdlet.LocalMacAddress != null)
		{
			num++;
		}
		if (cmdlet.RemoteIPAddress != null)
		{
			num++;
		}
		if (cmdlet.RemoteMacAddress != null)
		{
			num++;
		}
		if (num == 0)
		{
			throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.VMNetworkAdapterAcl_NoAddressSpecified);
		}
		if (num > 1)
		{
			throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.VMNetworkAdapterAcl_MultipleAddressTypesSpecified);
		}
	}

	internal static VMNetworkAdapterAclAddresses OrganizeAddressList(IVMNetworkAdapterAclCmdlet cmdlet)
	{
		VMNetworkAdapterAclAddresses vMNetworkAdapterAclAddresses = new VMNetworkAdapterAclAddresses();
		if (cmdlet.LocalIPAddress != null)
		{
			vMNetworkAdapterAclAddresses.AddressList = new List<string>(cmdlet.LocalIPAddress);
			vMNetworkAdapterAclAddresses.IsRemote = false;
			vMNetworkAdapterAclAddresses.IsMacAddress = false;
		}
		else if (cmdlet.RemoteIPAddress != null)
		{
			vMNetworkAdapterAclAddresses.AddressList = new List<string>(cmdlet.RemoteIPAddress);
			vMNetworkAdapterAclAddresses.IsRemote = true;
			vMNetworkAdapterAclAddresses.IsMacAddress = false;
		}
		else if (cmdlet.LocalMacAddress != null)
		{
			vMNetworkAdapterAclAddresses.AddressList = new List<string>(cmdlet.LocalMacAddress);
			vMNetworkAdapterAclAddresses.IsRemote = false;
			vMNetworkAdapterAclAddresses.IsMacAddress = true;
		}
		else if (cmdlet.RemoteMacAddress != null)
		{
			vMNetworkAdapterAclAddresses.AddressList = new List<string>(cmdlet.RemoteMacAddress);
			vMNetworkAdapterAclAddresses.IsRemote = true;
			vMNetworkAdapterAclAddresses.IsMacAddress = true;
		}
		if (!vMNetworkAdapterAclAddresses.IsMacAddress)
		{
			int num = vMNetworkAdapterAclAddresses.AddressList.FindIndex((string address) => string.Equals(address, "ANY", StringComparison.OrdinalIgnoreCase));
			if (num >= 0)
			{
				vMNetworkAdapterAclAddresses.AddressList.RemoveAt(num);
				vMNetworkAdapterAclAddresses.AddressList.Add("0.0.0.0/0");
				vMNetworkAdapterAclAddresses.AddressList.Add("::/0");
			}
		}
		vMNetworkAdapterAclAddresses.AddressList = vMNetworkAdapterAclAddresses.AddressList.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
		return vMNetworkAdapterAclAddresses;
	}
}
