using System.Collections.Generic;
using System.Linq;
using Microsoft.HyperV.PowerShell.Common;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMSwitchExtension : VirtualizationObject, IUpdatable
{
	private readonly DataUpdater<IEthernetSwitchExtension> m_Extension;

	private readonly VMSwitch m_VirtualSwitch;

	[VirtualizationObjectIdentifier(IdentifierFlags.UniqueIdentifier)]
	public string Id => m_Extension.GetData(UpdatePolicy.EnsureUpdated).ExtensionId;

	[VirtualizationObjectIdentifier(IdentifierFlags.FriendlyName)]
	public string Name => m_Extension.GetData(UpdatePolicy.EnsureUpdated).FriendlyName;

	public string Vendor => m_Extension.GetData(UpdatePolicy.EnsureUpdated).Company;

	public string Version => m_Extension.GetData(UpdatePolicy.EnsureUpdated).Version;

	public VMSwitchExtensionType ExtensionType => (VMSwitchExtensionType)m_Extension.GetData(UpdatePolicy.EnsureUpdated).ExtensionType;

	public string ParentExtensionId => m_Extension.GetData(UpdatePolicy.EnsureAssociatorsUpdated).Parent?.ExtensionId;

	public string ParentExtensionName => m_Extension.GetData(UpdatePolicy.EnsureAssociatorsUpdated).Parent?.FriendlyName;

	public string SwitchId => m_VirtualSwitch.Id.ToString();

	public string SwitchName => m_VirtualSwitch.Name;

	public bool Enabled
	{
		get
		{
			return m_Extension.GetData(UpdatePolicy.EnsureUpdated).IsEnabled;
		}
		internal set
		{
			m_Extension.GetData(UpdatePolicy.None).IsEnabled = value;
		}
	}

	public bool Running => m_Extension.GetData(UpdatePolicy.EnsureUpdated).IsRunning;

	internal IEthernetSwitchExtension VirtualizationManagementExtension => m_Extension.GetData(UpdatePolicy.None);

	internal VMSwitchExtension(IEthernetSwitchExtension switchExtension, VMSwitch virtualSwitch)
		: base(switchExtension)
	{
		m_Extension = InitializePrimaryDataUpdater(switchExtension);
		m_VirtualSwitch = virtualSwitch;
	}

	void IUpdatable.Put(IOperationWatcher operationWatcher)
	{
		operationWatcher.PerformPut(m_Extension.GetData(UpdatePolicy.None), TaskDescriptions.SetVMSwitchExtension, this);
	}

	internal IEnumerable<VMSwitchExtension> ExpandExtensionTree()
	{
		VMSwitchExtension[] first = new VMSwitchExtension[1] { this };
		IEnumerable<VMSwitchExtension> second = m_Extension.GetData(UpdatePolicy.EnsureAssociatorsUpdated).Children.Select((IEthernetSwitchExtension child) => new VMSwitchExtension(child, m_VirtualSwitch));
		return first.Concat(second);
	}
}
