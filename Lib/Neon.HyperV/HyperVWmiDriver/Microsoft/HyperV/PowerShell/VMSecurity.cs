using System;
using Microsoft.HyperV.PowerShell.Common;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMSecurity : VirtualizationObject, IUpdatable
{
	private readonly DataUpdater<IVMSecuritySetting> m_SecuritySetting;

	private Version m_VmVersion;

	public bool TpmEnabled
	{
		get
		{
			return m_SecuritySetting.GetData(UpdatePolicy.EnsureUpdated).TpmEnabled;
		}
		internal set
		{
			m_SecuritySetting.GetData(UpdatePolicy.None).TpmEnabled = value;
		}
	}

	public bool KsdEnabled
	{
		get
		{
			return m_SecuritySetting.GetData(UpdatePolicy.EnsureUpdated).KsdEnabled;
		}
		internal set
		{
			m_SecuritySetting.GetData(UpdatePolicy.None).KsdEnabled = value;
		}
	}

	public bool Shielded
	{
		get
		{
			IVMSecuritySetting data = m_SecuritySetting.GetData(UpdatePolicy.EnsureUpdated);
			if (data.TpmEnabled)
			{
				return data.ShieldingRequested;
			}
			return false;
		}
		internal set
		{
			m_SecuritySetting.GetData(UpdatePolicy.None).ShieldingRequested = value;
		}
	}

	public bool EncryptStateAndVmMigrationTraffic
	{
		get
		{
			IVMSecuritySetting data = m_SecuritySetting.GetData(UpdatePolicy.EnsureUpdated);
			bool flag = (KdsUtilities.IsDataProtectionPropertyUsedToEncrypt(m_VmVersion) ? data.DataProtectionRequested : data.EncryptStateAndVmMigrationTraffic);
			if (data.TpmEnabled || data.KsdEnabled)
			{
				return data.ShieldingRequested || flag;
			}
			return false;
		}
		internal set
		{
			IVMSecuritySetting data = m_SecuritySetting.GetData(UpdatePolicy.None);
			if (KdsUtilities.IsDataProtectionPropertyUsedToEncrypt(m_VmVersion))
			{
				data.DataProtectionRequested = value;
			}
			else
			{
				data.EncryptStateAndVmMigrationTraffic = value;
			}
		}
	}

	public bool VirtualizationBasedSecurityOptOut
	{
		get
		{
			return m_SecuritySetting.GetData(UpdatePolicy.EnsureUpdated).VirtualizationBasedSecurityOptOut;
		}
		internal set
		{
			m_SecuritySetting.GetData(UpdatePolicy.None).VirtualizationBasedSecurityOptOut = value;
		}
	}

	public bool BindToHostTpm
	{
		get
		{
			return m_SecuritySetting.GetData(UpdatePolicy.EnsureUpdated).BindToHostTpm;
		}
		internal set
		{
			m_SecuritySetting.GetData(UpdatePolicy.None).BindToHostTpm = value;
		}
	}

	internal VMSecurity(IVMSecuritySetting securitySettings, Version vmVersion)
		: base(securitySettings)
	{
		m_SecuritySetting = InitializePrimaryDataUpdater(securitySettings);
		m_VmVersion = vmVersion;
	}

	void IUpdatable.Put(IOperationWatcher operationWatcher)
	{
		IVMSecuritySetting data = m_SecuritySetting.GetData(UpdatePolicy.None);
		operationWatcher.PerformPut(data, TaskDescriptions.SetVMSecuritySettings, this);
	}

	internal static byte[] GetKeyProtector(VMSecurity securityObject, IOperationWatcher operationWatcher)
	{
		IVMSecurityService vMSecurityService = ObjectLocator.GetVMSecurityService(securityObject.Server);
		IVMSecuritySetting data = securityObject.m_SecuritySetting.GetData(UpdatePolicy.EnsureUpdated);
		return vMSecurityService.GetKeyProtector(data);
	}

	internal static void SetKeyProtector(VMSecurity securityObject, byte[] rawKeyProtector, IOperationWatcher operationWatcher)
	{
		IVMSecurityService service = ObjectLocator.GetVMSecurityService(securityObject.Server);
		IVMSecuritySetting securitySetting = securityObject.m_SecuritySetting.GetData(UpdatePolicy.EnsureUpdated);
		operationWatcher.PerformOperation(() => service.BeginSetKeyProtector(securitySetting, rawKeyProtector), service.EndSetKeyProtector, TaskDescriptions.SetVMKeyProtector, securityObject);
	}

	internal static void SetLocalKeyProtector(VMSecurity securityObject, IOperationWatcher operationWatcher)
	{
		IVMSecurityService service = ObjectLocator.GetVMSecurityService(securityObject.Server);
		IVMSecuritySetting securitySetting = securityObject.m_SecuritySetting.GetData(UpdatePolicy.EnsureUpdated);
		byte[] localKeyProtector = KdsUtilities.NewLocalKeyProtector(securityObject.Server);
		operationWatcher.PerformOperation(() => service.BeginSetKeyProtector(securitySetting, localKeyProtector), service.EndSetKeyProtector, TaskDescriptions.SetVMKeyProtector, securityObject);
	}

	internal static void RestoreLastKnownGoodKeyProtector(VMSecurity securityObject, IOperationWatcher operationWatcher)
	{
		IVMSecurityService service = ObjectLocator.GetVMSecurityService(securityObject.Server);
		IVMSecuritySetting securitySetting = securityObject.m_SecuritySetting.GetData(UpdatePolicy.EnsureUpdated);
		operationWatcher.PerformOperation(() => service.BeginRestoreLastKnownGoodKeyProtector(securitySetting), service.EndRestoreLastKnownGoodKeyProtector, TaskDescriptions.SetVMKeyProtector, securityObject);
	}
}
