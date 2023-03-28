using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.HyperV.PowerShell.Common;
using Microsoft.Virtualization.Client.Management;

using ErrorMessages = Microsoft.HyperV.PowerShell.Common.ErrorMessages;

namespace Microsoft.HyperV.PowerShell;

[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Fibre", Justification = "This is per spec.")]
internal sealed class VMFibreChannelHba : VMDevice, IAddableVMFibreChannelHba, IAddableVMDevice<IFibreChannelPortSetting, IFcPoolAllocationSetting>, IAddableVMDevice<IFibreChannelPortSetting>, IAddable, IHasAttachableComponent<IFcPoolAllocationSetting>, IRemovable
{
	private static readonly Regex gm_WorldWideNameRegex = new Regex("^[A-Fa-f0-9]{16}$");

	private IDataUpdater<IFibreChannelPortSetting> m_FibreChannelPortSetting;

	private IDataUpdater<IFcPoolAllocationSetting> m_ConnectionSetting;

	public string SanName
	{
		get
		{
			string result = null;
			IFcPoolAllocationSetting data = m_ConnectionSetting.GetData(UpdatePolicy.EnsureUpdated);
			if (data != null)
			{
				result = data.PoolId;
			}
			return result;
		}
		internal set
		{
			m_ConnectionSetting.GetData(UpdatePolicy.None).PoolId = value;
		}
	}

	[SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "WorldWide", Justification = "This is per spec.")]
	public string WorldWideNodeNameSetA
	{
		get
		{
			return m_FibreChannelPortSetting.GetData(UpdatePolicy.EnsureUpdated).WorldWideNodeName;
		}
		internal set
		{
			m_FibreChannelPortSetting.GetData(UpdatePolicy.None).WorldWideNodeName = value;
		}
	}

	[SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "WorldWide", Justification = "This is per spec.")]
	public string WorldWidePortNameSetA
	{
		get
		{
			return m_FibreChannelPortSetting.GetData(UpdatePolicy.EnsureUpdated).WorldWidePortName;
		}
		internal set
		{
			m_FibreChannelPortSetting.GetData(UpdatePolicy.None).WorldWidePortName = value;
		}
	}

	[SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "WorldWide", Justification = "This is per spec.")]
	public string WorldWideNodeNameSetB
	{
		get
		{
			return m_FibreChannelPortSetting.GetData(UpdatePolicy.EnsureUpdated).SecondaryWorldWideNodeName;
		}
		internal set
		{
			m_FibreChannelPortSetting.GetData(UpdatePolicy.None).SecondaryWorldWideNodeName = value;
		}
	}

	[SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "WorldWide", Justification = "This is per spec.")]
	public string WorldWidePortNameSetB
	{
		get
		{
			return m_FibreChannelPortSetting.GetData(UpdatePolicy.EnsureUpdated).SecondaryWorldWidePortName;
		}
		internal set
		{
			m_FibreChannelPortSetting.GetData(UpdatePolicy.None).SecondaryWorldWidePortName = value;
		}
	}

	internal override string PutDescription => TaskDescriptions.SetVMFibreChannelHba;

	public bool IsTemplate { get; private set; }

	string IAddableVMDevice<IFibreChannelPortSetting>.DescriptionForAdd => TaskDescriptions.AddVMFibreChannelHba;

	string IHasAttachableComponent<IFcPoolAllocationSetting>.DescriptionForAttach => TaskDescriptions.SetVMFibreChannelHba_AttachConnection;

	string IAddableVMDevice<IFibreChannelPortSetting, IFcPoolAllocationSetting>.DescriptionForAddRollback => TaskDescriptions.AddVMFibreChannelHba_RollbackPartialAdd;

	internal VMFibreChannelHba(IFibreChannelPortSetting setting, IFcPoolAllocationSetting connectionSetting, VirtualMachineBase parentVirtualMachineObject)
		: this(setting, connectionSetting, parentVirtualMachineObject, isTemplate: false)
	{
	}

	private VMFibreChannelHba(IFibreChannelPortSetting setting, IFcPoolAllocationSetting connectionSetting, VirtualMachineBase parentVirtualMachineObject, bool isTemplate)
		: base(setting, parentVirtualMachineObject)
	{
		if (!isTemplate)
		{
			m_FibreChannelPortSetting = InitializeExistingPortUpdater(setting);
			m_ConnectionSetting = InitializeExistingConnectionUpdater(connectionSetting);
		}
		else
		{
			m_FibreChannelPortSetting = InitializeTemplatePortUpdater(setting);
			m_ConnectionSetting = InitializeTemplateConnectionUpdater(connectionSetting);
		}
		IsTemplate = isTemplate;
	}

	private IDataUpdater<IFibreChannelPortSetting> InitializeExistingPortUpdater(IFibreChannelPortSetting portSetting)
	{
		return InitializePrimaryDataUpdater(portSetting);
	}

	private IDataUpdater<IFcPoolAllocationSetting> InitializeExistingConnectionUpdater(IFcPoolAllocationSetting connectionSetting)
	{
		return new DependentObjectDataUpdater<IFcPoolAllocationSetting>(connectionSetting, (TimeSpan time) => m_FibreChannelPortSetting.GetData(UpdatePolicy.EnsureAssociatorsUpdated).GetConnectionConfiguration());
	}

	private IDataUpdater<IFibreChannelPortSetting> InitializeTemplatePortUpdater(IFibreChannelPortSetting portSetting)
	{
		return new TemplateObjectDataUpdater<IFibreChannelPortSetting>(portSetting);
	}

	private IDataUpdater<IFcPoolAllocationSetting> InitializeTemplateConnectionUpdater(IFcPoolAllocationSetting connectionSetting)
	{
		return new TemplateObjectDataUpdater<IFcPoolAllocationSetting>(connectionSetting);
	}

	internal override IDataUpdater<IVMDeviceSetting> GetDeviceDataUpdater()
	{
		return m_FibreChannelPortSetting;
	}

	internal override void PutSelf(IOperationWatcher operationWatcher)
	{
		base.PutSelf(operationWatcher);
		IFcPoolAllocationSetting data = m_ConnectionSetting.GetData(UpdatePolicy.None);
		if (data != null)
		{
			PutOneDeviceSetting(data, operationWatcher);
		}
	}

	void IAddableVMDevice<IFibreChannelPortSetting>.FinishAddingDeviceSetting(IFibreChannelPortSetting deviceSetting)
	{
		((IHasAttachableComponent<IFcPoolAllocationSetting>)this).GetComponentSetting(UpdatePolicy.None).Parent = deviceSetting.ManagementPath;
		m_FibreChannelPortSetting = InitializeExistingPortUpdater(deviceSetting);
	}

	IFibreChannelPortSetting IAddableVMDevice<IFibreChannelPortSetting>.GetDeviceSetting(UpdatePolicy policy)
	{
		return m_FibreChannelPortSetting.GetData(policy);
	}

	void IHasAttachableComponent<IFcPoolAllocationSetting>.FinishAttachingComponentSetting(IFcPoolAllocationSetting componentSetting)
	{
		m_ConnectionSetting = InitializeExistingConnectionUpdater(componentSetting);
	}

	IFcPoolAllocationSetting IHasAttachableComponent<IFcPoolAllocationSetting>.GetComponentSetting(UpdatePolicy policy)
	{
		return m_ConnectionSetting.GetData(policy);
	}

	bool IHasAttachableComponent<IFcPoolAllocationSetting>.HasComponent()
	{
		return m_ConnectionSetting.GetData(UpdatePolicy.None) != null;
	}

	void IRemovable.Remove(IOperationWatcher operationWatcher)
	{
		IFibreChannelPortSetting data = m_FibreChannelPortSetting.GetData(UpdatePolicy.None);
		RemoveInternal(data, TaskDescriptions.RemoveVMFibreChannelHba, operationWatcher);
	}

	internal bool Matches(string worldWideNodeNameSetA, string worldWidePortNameSetA, string worldWideNodeNameSetB, string worldWidePortNameSetB)
	{
		if (string.Equals(WorldWideNodeNameSetA, worldWideNodeNameSetA, StringComparison.OrdinalIgnoreCase) && string.Equals(WorldWidePortNameSetA, worldWidePortNameSetA, StringComparison.OrdinalIgnoreCase) && string.Equals(WorldWideNodeNameSetB, worldWideNodeNameSetB, StringComparison.OrdinalIgnoreCase))
		{
			return string.Equals(WorldWidePortNameSetB, worldWidePortNameSetB, StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}

	internal static void GenerateWorldWideNames(Server server, out string nodeName, out string portNameA, out string portNameB)
	{
		IVMService virtualizationService = ObjectLocator.GetVirtualizationService(server);
		string[] array = virtualizationService.GenerateWorldWidePortNames(2);
		IVMServiceSetting setting = virtualizationService.Setting;
		nodeName = setting.AssignedWorldWideNodeName;
		portNameA = array[0];
		portNameB = array[1];
	}

	internal static void ValidateWorldWideName(string name)
	{
		if (!gm_WorldWideNameRegex.Match(name).Success)
		{
			throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, ErrorMessages.VMFibreChannelHba_WorldWideNameFormatError, name));
		}
	}

	internal static VMFibreChannelHba CreateTemplateFibreChannelHba(VirtualMachine parentVirtualMachine)
	{
		IHostComputerSystem hostComputerSystem = ObjectLocator.GetHostComputerSystem(parentVirtualMachine.Server);
		IFibreChannelPortSetting setting = (IFibreChannelPortSetting)hostComputerSystem.GetSettingCapabilities(VMDeviceSettingType.FibreChannelPort, Capabilities.DefaultCapability);
		IFcPoolAllocationSetting connectionSetting = (IFcPoolAllocationSetting)hostComputerSystem.GetSettingCapabilities(VMDeviceSettingType.FibreChannelConnection, Capabilities.DefaultCapability);
		return new VMFibreChannelHba(setting, connectionSetting, parentVirtualMachine, isTemplate: true);
	}
}
