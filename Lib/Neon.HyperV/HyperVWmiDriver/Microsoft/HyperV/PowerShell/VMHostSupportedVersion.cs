using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMHostSupportedVersion : VirtualizationObject
{
	private readonly DataUpdater<IVMComputerSystemSetting> m_SupportedVersionSetting;

	private readonly bool m_IsDefault;

	public string Name => m_SupportedVersionSetting.GetData(UpdatePolicy.None).Name;

	public Version Version => new Version(m_SupportedVersionSetting.GetData(UpdatePolicy.None).Version);

	public bool IsDefault => m_IsDefault;

	private VMHostSupportedVersion(IVMComputerSystemSetting supportedVersionSetting, IVMComputerSystemSetting defaultSetting)
		: base(supportedVersionSetting)
	{
		m_SupportedVersionSetting = new DataUpdater<IVMComputerSystemSetting>(supportedVersionSetting);
		m_IsDefault = string.Equals(supportedVersionSetting.Version, defaultSetting.Version, StringComparison.OrdinalIgnoreCase);
	}

	internal static IEnumerable<VMHostSupportedVersion> GetVmHostSupportedVersions(Server server)
	{
		try
		{
			IVMService virtualizationService = ObjectLocator.GetVirtualizationService(server);
			IVMComputerSystemSetting defaultSetting = virtualizationService.AllCapabilities.DefaultComputerSystemSetting;
			return virtualizationService.AllCapabilities.SupportedVersionSettings.Select((IVMComputerSystemSetting s) => new VMHostSupportedVersion(s, defaultSetting)).ToList();
		}
		catch (VirtualizationManagementException exception)
		{
			throw ExceptionHelper.ConvertToVirtualizationException(exception, null);
		}
	}

	internal static IEnumerable<VMHostSupportedVersion> GetVmHostSupportedVersions(IEnumerable<Server> servers, IOperationWatcher operationWatcher)
	{
		return servers.SelectManyWithLogging(GetVmHostSupportedVersions, operationWatcher).ToList();
	}
}
