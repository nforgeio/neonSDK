using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal abstract class ComputeResource : VirtualizationObject
{
	private readonly DataUpdater<IVMComputerSystemBase> _computerSystem;

	private readonly DataUpdater<IVMComputerSystemSetting> _settings;

	public DateTime CreationTime => _settings.GetData(UpdatePolicy.EnsureUpdated).CreationTime;

	[VirtualizationObjectIdentifier(IdentifierFlags.UniqueIdentifier)]
	public Guid Id
	{
		get
		{
			if (!Guid.TryParse(_settings.GetData(UpdatePolicy.EnsureUpdated).ConfigurationId, out var result))
			{
				return Guid.Empty;
			}
			return result;
		}
	}

	[VirtualizationObjectIdentifier(IdentifierFlags.FriendlyName)]
	public string Name
	{
		get
		{
			return _settings.GetData(UpdatePolicy.EnsureUpdated).Name;
		}
		internal set
		{
			_settings.GetData(UpdatePolicy.None).Name = value;
		}
	}

	public List<VMNetworkAdapter> NetworkAdapters => GetNetworkAdapters().ToList();

	internal ComputeResource(IVMComputerSystemBase computerSystem, IVMComputerSystemSetting settings)
		: base(settings)
	{
		if (computerSystem != null)
		{
			_computerSystem = new DataUpdater<IVMComputerSystemBase>(computerSystem);
		}
		_settings = InitializePrimaryDataUpdater(settings);
	}

	internal IVMComputerSystemBase GetComputerSystem(UpdatePolicy policy)
	{
		if (_computerSystem != null)
		{
			return _computerSystem.GetData(policy);
		}
		return null;
	}

	internal TChildType GetComputerSystemAs<TChildType>(UpdatePolicy policy) where TChildType : IVMComputerSystemBase
	{
		if (_computerSystem != null)
		{
			return _computerSystem.GetDataAs<TChildType>(policy);
		}
		return default(TChildType);
	}

	internal IVMComputerSystemSetting GetSettings(UpdatePolicy policy)
	{
		return _settings.GetData(policy);
	}

	protected IEnumerable<IVMDeviceSetting> GetDeviceSettings()
	{
		return _settings.GetData(UpdatePolicy.EnsureAssociatorsUpdated).GetDeviceSettings();
	}

	internal TDeviceSetting AddDeviceSetting<TDeviceSetting>(TDeviceSetting templateSetting, string taskDescription, IOperationWatcher operationWatcher) where TDeviceSetting : class, IVMDeviceSetting
	{
		IVMComputerSystemBase computerSystem = GetComputerSystem(UpdatePolicy.EnsureUpdated);
		TDeviceSetting result = (TDeviceSetting)operationWatcher.PerformOperationWithReturn(() => computerSystem.BeginAddDevice(templateSetting), computerSystem.EndAddDevice, taskDescription, this);
		InvalidateDeviceSettingsList();
		return result;
	}

	internal VMMemory GetMemory()
	{
		return new VMMemory(_settings.GetData(UpdatePolicy.EnsureAssociatorsUpdated).GetMemorySetting(), this);
	}

	internal IEnumerable<VMNetworkAdapter> GetNetworkAdapters()
	{
		return from nic in GetDeviceSettings().OfType<IEthernetPortSetting>()
			select new VMNetworkAdapter(nic, nic.GetConnectionConfiguration(), this);
	}

	internal VMProcessor GetProcessor()
	{
		return new VMProcessor(_settings.GetData(UpdatePolicy.EnsureAssociatorsUpdated).GetProcessorSetting(), this);
	}

	internal virtual void InvalidateDeviceSettingsList()
	{
		GetSettings(UpdatePolicy.None).InvalidateAssociationCache();
	}
}
