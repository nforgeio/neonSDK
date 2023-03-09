using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Common;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Virtualization.Client.Management;

using ErrorMessages = Microsoft.HyperV.PowerShell.Common.ErrorMessages;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMFirmware : VMComponentObject, IUpdatable
{
	private const ushort gm_ProtocolIPv4 = 4096;

	private const ushort gm_ProtocolIPv6 = 4097;

	private readonly DataUpdater<IVMComputerSystemSetting> m_VMSetting;

	private readonly IVMService m_Service;

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
	public VMBootSource[] BootOrder
	{
		get
		{
			return (from bootEntry in m_VMSetting.GetData(UpdatePolicy.EnsureUpdated).GetFirmwareBootOrder()
				select new VMBootSource(bootEntry, GetParentAs<VirtualMachineBase>())).ToArray();
		}
		internal set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			m_VMSetting.GetData(UpdatePolicy.None).SetFirmwareBootOrder(value.Select((VMBootSource source) => source.BootEntry));
		}
	}

	public IPProtocolPreference PreferredNetworkBootProtocol
	{
		get
		{
			return m_VMSetting.GetData(UpdatePolicy.None).NetworkBootPreferredProtocol switch
			{
				4096 => IPProtocolPreference.IPv4, 
				4097 => IPProtocolPreference.IPv6, 
				_ => IPProtocolPreference.IPv4, 
			};
		}
		internal set
		{
			m_VMSetting.GetData(UpdatePolicy.None).NetworkBootPreferredProtocol = value switch
			{
				IPProtocolPreference.IPv4 => 4096, 
				IPProtocolPreference.IPv6 => 4097, 
				_ => 4096, 
			};
		}
	}

	public OnOffState SecureBoot
	{
		get
		{
			return m_VMSetting.GetData(UpdatePolicy.EnsureUpdated).SecureBootEnabled.ToOnOffState();
		}
		internal set
		{
			m_VMSetting.GetData(UpdatePolicy.None).SecureBootEnabled = value.ToBool();
		}
	}

	public Guid? SecureBootTemplateId
	{
		get
		{
			return m_VMSetting.GetData(UpdatePolicy.EnsureUpdated).SecureBootTemplateId;
		}
		internal set
		{
			m_VMSetting.GetData(UpdatePolicy.None).SecureBootTemplateId = value;
		}
	}

	public string SecureBootTemplate
	{
		get
		{
			Guid? secureBootTemplateId = m_VMSetting.GetData(UpdatePolicy.EnsureUpdated).SecureBootTemplateId;
			if (secureBootTemplateId.HasValue && m_Service.TryGetSecureBootTemplate(secureBootTemplateId.Value, out var template))
			{
				return template.Name;
			}
			return null;
		}
		internal set
		{
			IVMComputerSystemSetting data = m_VMSetting.GetData(UpdatePolicy.None);
			IEnumerable<SecureBootTemplate> secureBootTemplates = m_Service.GetSecureBootTemplates();
			WildcardPattern pattern = new WildcardPattern(value, WildcardOptions.Compiled | WildcardOptions.IgnoreCase);
			IList<SecureBootTemplate> source = secureBootTemplates.Where((SecureBootTemplate template) => pattern.IsMatch(template.Name)).ToList();
			int num = source.Count();
			if (num == 0)
			{
				throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, ErrorMessages.InvalidParameter_MatchesNoTemplates, value));
			}
			if (num > 1)
			{
				throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, ErrorMessages.InvalidParameter_MatchesMultipleTemplates, value));
			}
			data.SecureBootTemplateId = source.Single().Id;
		}
	}

	public ConsoleModeType ConsoleMode
	{
		get
		{
			m_VMSetting.GetData(UpdatePolicy.EnsureUpdated);
			return (ConsoleModeType)m_VMSetting.GetData(UpdatePolicy.None).ConsoleMode;
		}
		internal set
		{
			m_VMSetting.GetData(UpdatePolicy.None).ConsoleMode = (Microsoft.Virtualization.Client.Management.ConsoleModeType)value;
		}
	}

	public OnOffState PauseAfterBootFailure
	{
		get
		{
			return m_VMSetting.GetData(UpdatePolicy.EnsureUpdated).PauseAfterBootFailure.ToOnOffState();
		}
		internal set
		{
			m_VMSetting.GetData(UpdatePolicy.None).PauseAfterBootFailure = value.ToBool();
		}
	}

	internal VMFirmware(IVMComputerSystemSetting vmSetting, VirtualMachineBase parentVirtualMachineObject)
		: base(vmSetting, parentVirtualMachineObject)
	{
		m_VMSetting = InitializePrimaryDataUpdater(vmSetting);
		m_Service = ObjectLocator.GetVirtualizationService(base.Server);
	}

	void IUpdatable.Put(IOperationWatcher operationWatcher)
	{
		operationWatcher.PerformPut(m_VMSetting.GetData(UpdatePolicy.None), TaskDescriptions.SetVMFirmware, this);
	}
}
