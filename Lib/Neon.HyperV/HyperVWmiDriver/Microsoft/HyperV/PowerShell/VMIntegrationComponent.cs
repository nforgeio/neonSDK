using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.HyperV.PowerShell.Common;
using Microsoft.Virtualization.Client.Management;

using ErrorMessages = Microsoft.HyperV.PowerShell.Common.ErrorMessages;

namespace Microsoft.HyperV.PowerShell;

internal class VMIntegrationComponent : VMDevice
{
    internal readonly IDataUpdater<IVMIntegrationComponentSetting> m_IntegrationComponentSetting;

    public bool Enabled
    {
        get
        {
            return m_IntegrationComponentSetting.GetData(UpdatePolicy.EnsureUpdated).Enabled;
        }
        internal set
        {
            m_IntegrationComponentSetting.GetData(UpdatePolicy.None).Enabled = value;
        }
    }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
    public VMIntegrationComponentOperationalStatus[] OperationalStatus
    {
        get
        {
            IVMIntegrationComponent integrationComponent = m_IntegrationComponentSetting.GetData(UpdatePolicy.EnsureAssociatorsUpdated).GetIntegrationComponent();
            if (integrationComponent != null)
            {
                return (from status in integrationComponent.GetOperationalStatus()
                    select (VMIntegrationComponentOperationalStatus)status).ToArray();
            }
            return new VMIntegrationComponentOperationalStatus[0];
        }
    }

    public VMIntegrationComponentOperationalStatus? PrimaryOperationalStatus
    {
        get
        {
            VMIntegrationComponentOperationalStatus[] operationalStatus = OperationalStatus;
            if (operationalStatus.Length != 0)
            {
                return operationalStatus[0];
            }
            return null;
        }
    }

    public string PrimaryStatusDescription
    {
        get
        {
            string[] statusDescription = StatusDescription;
            if (statusDescription.Length != 0)
            {
                return statusDescription[0];
            }
            return null;
        }
    }

    public VMIntegrationComponentOperationalStatus? SecondaryOperationalStatus
    {
        get
        {
            VMIntegrationComponentOperationalStatus[] operationalStatus = OperationalStatus;
            if (operationalStatus.Length > 1)
            {
                return operationalStatus[1];
            }
            return null;
        }
    }

    public string SecondaryStatusDescription
    {
        get
        {
            string[] statusDescription = StatusDescription;
            if (statusDescription.Length > 1)
            {
                return statusDescription[1];
            }
            return null;
        }
    }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
    public string[] StatusDescription
    {
        get
        {
            IVMIntegrationComponent integrationComponent = m_IntegrationComponentSetting.GetData(UpdatePolicy.EnsureAssociatorsUpdated).GetIntegrationComponent();
            if (integrationComponent != null)
            {
                return integrationComponent.GetOperationalStatusDescriptions();
            }
            return new string[0];
        }
    }

    internal override string PutDescription => TaskDescriptions.SetVMIntegrationComponent;

    internal VMIntegrationComponent(IVMIntegrationComponentSetting setting, VirtualMachineBase parentVirtualMachineObject)
        : base(setting, parentVirtualMachineObject)
    {
        m_IntegrationComponentSetting = InitializePrimaryDataUpdater(setting);
    }

    internal override IDataUpdater<IVMDeviceSetting> GetDeviceDataUpdater()
    {
        return m_IntegrationComponentSetting;
    }

    internal static VMIntegrationComponent CreateIntegrationComponent(IVMIntegrationComponentSetting setting, VirtualMachineBase parentSystem)
    {
        VMIntegrationComponent vMIntegrationComponent = null;
        if (setting is IVMDataExchangeComponentSetting)
        {
            return new DataExchangeComponent(setting as IVMDataExchangeComponentSetting, parentSystem);
        }
        if (setting is IVMGuestServiceInterfaceComponentSetting)
        {
            return new GuestServiceInterfaceComponent(setting as IVMGuestServiceInterfaceComponentSetting, parentSystem);
        }
        if (setting is IVMShutdownComponentSetting)
        {
            return new ShutdownComponent(setting as IVMShutdownComponentSetting, parentSystem);
        }
        if (setting is IVMTimeSyncComponentSetting || setting is IVMHeartbeatComponentSetting || setting is IVMVssIntegrationComponentSetting)
        {
            return new VMIntegrationComponent(setting, parentSystem);
        }
        return null;
    }

    internal static IEnumerable<VMIntegrationComponent> GetIntegrationComponents(VirtualMachineBase vmOrSnapshot, WildcardPatternMatcher wildcardPatternMatcher)
    {
        IReadOnlyList<VMIntegrationComponent> readOnlyList = vmOrSnapshot.GetVMIntegrationComponents();
        if (wildcardPatternMatcher != null)
        {
            readOnlyList = readOnlyList.Where((VMIntegrationComponent ic) => wildcardPatternMatcher.MatchesAny(ic.Name)).ToList();
        }
        if (readOnlyList.Count <= 0)
        {
            throw ExceptionHelper.CreateInvalidArgumentException(ErrorMessages.VMIntegrationService_IntegrationComponentNotFound);
        }
        return readOnlyList;
    }

    internal VMIntegrationComponentOperationalStatus? GetCurrentPrimaryOperationalStatus()
    {
        IVMIntegrationComponentSetting data = m_IntegrationComponentSetting.GetData(UpdatePolicy.None);
        data.UpdateAssociationCache(TimeSpan.Zero);
        IVMIntegrationComponent integrationComponent = data.GetIntegrationComponent();
        if (integrationComponent != null)
        {
            global::Microsoft.Virtualization.Client.Management.VMIntegrationComponentOperationalStatus[] operationalStatus = integrationComponent.GetOperationalStatus();
            if (operationalStatus.Length != 0)
            {
                return (VMIntegrationComponentOperationalStatus)operationalStatus[0];
            }
        }
        return null;
    }
}
