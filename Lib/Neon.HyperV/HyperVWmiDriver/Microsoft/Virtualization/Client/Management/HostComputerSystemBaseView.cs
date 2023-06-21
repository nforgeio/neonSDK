#define TRACE
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Microsoft.Virtualization.Client.Management;

internal class HostComputerSystemBaseView : View, IHostComputerSystemBase, IVirtualizationManagementObject
{
    internal static class WmiMemberNames
    {
        public const string Dedicated = "Dedicated";

        public const string OtherDedicatedDescriptions = "OtherDedicatedDescriptions";
    }

    private EventObjectKey m_VMComputerSystemCreationKey;

    private VMComputersSystemCreatedEventHandler m_VMComputerSystemCreated;

    private EventObjectKey m_VMTaskCreationKey;

    private VMVirtualizationTaskCreatedEventHandler m_VMTaskCreated;

    public IVMService VirtualizationService => ObjectLocator.GetVirtualizationService(base.Server);

    private EventObjectKey VMComputerSystemCreationKey => m_VMComputerSystemCreationKey ?? (m_VMComputerSystemCreationKey = ObjectKeyCreator.CreateVMCreationEventObjectKey(base.Server));

    private EventObjectKey VMTaskCreationKey => m_VMTaskCreationKey ?? (m_VMTaskCreationKey = ObjectKeyCreator.CreateVMTaskCreationEventObjectKey(base.Server));

    public event VMComputersSystemCreatedEventHandler VMComputerSystemCreated
    {
        add
        {
            if (m_VMComputerSystemCreated == null)
            {
                RegisterForInstanceCreationEvent(VMComputerSystemCreationKey, OnVmcsCreated);
            }
            m_VMComputerSystemCreated = (VMComputersSystemCreatedEventHandler)Delegate.Combine(m_VMComputerSystemCreated, value);
        }
        remove
        {
            if (m_VMComputerSystemCreated != null)
            {
                m_VMComputerSystemCreated = (VMComputersSystemCreatedEventHandler)Delegate.Remove(m_VMComputerSystemCreated, value);
                if (m_VMComputerSystemCreated == null)
                {
                    UnregisterForInstanceCreationEvent(VMComputerSystemCreationKey, OnVmcsCreated);
                }
            }
        }
    }

    public event VMVirtualizationTaskCreatedEventHandler VMVirtualizationTaskCreated
    {
        add
        {
            if (m_VMTaskCreated == null)
            {
                RegisterForInstanceCreationEvent(VMTaskCreationKey, OnVMTaskCreated);
            }
            m_VMTaskCreated = (VMVirtualizationTaskCreatedEventHandler)Delegate.Combine(m_VMTaskCreated, value);
        }
        remove
        {
            if (m_VMTaskCreated != null)
            {
                UnregisterForInstanceCreationEvent(VMTaskCreationKey, OnVMTaskCreated);
            }
        }
    }

    private void OnVmcsCreated(object sender, InstanceEventArrivedArgs eventArrived)
    {
        if (m_VMComputerSystemCreated != null)
        {
            try
            {
                IVMComputerSystem vMComputerSystem = ObjectLocator.GetVMComputerSystem(base.Server, eventArrived.TargetInstance);
                VMTrace.TraceWmiEvent(string.Format(CultureInfo.InvariantCulture, "ICE virtual machine '{0}' created.", vMComputerSystem.InstanceId));
                m_VMComputerSystemCreated(vMComputerSystem);
            }
            catch (Exception ex)
            {
                VMTrace.TraceError("Error creating Virtman object for new vm!", ex);
            }
        }
    }

    private void OnVMTaskCreated(object sender, InstanceEventArrivedArgs eventArrived)
    {
        if (m_VMTaskCreated != null)
        {
            try
            {
                IVMTask vMTask = ObjectLocator.GetVMTask(base.Server, eventArrived.TargetInstance);
                VMTrace.TraceWmiEvent(string.Format(CultureInfo.InvariantCulture, "ICE task '{0}' created.", vMTask.InstanceId));
                m_VMTaskCreated(vMTask);
            }
            catch (Exception ex)
            {
                VMTrace.TraceError("Error creating Virtman object for new task!", ex);
            }
        }
    }

    internal static bool IsHostClusterComputerSystem(IProxy proxy)
    {
        ushort[] dedicated = (ushort[])proxy.GetProperty("Dedicated");
        string[] otherDedicatedDescriptions = (string[])proxy.GetProperty("OtherDedicatedDescriptions");
        return IsHostClusterComputerSystem(dedicated, otherDedicatedDescriptions);
    }

    private static bool IsHostClusterComputerSystem(ushort[] dedicated, string[] otherDedicatedDescriptions)
    {
        if (dedicated != null && dedicated.FirstOrDefault() == 2 && otherDedicatedDescriptions != null)
        {
            return string.Equals(otherDedicatedDescriptions.FirstOrDefault(), "Hyper-V Cluster", StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }

    public IList<ISummaryInformation> GetAllSummaryInformation(SummaryInformationRequest requestedInformation)
    {
        VMTrace.TraceWmiGetSummaryInformation(base.Server, null, requestedInformation);
        WmiOperationOptions requestedInformationOptions = SummaryInformation.GetRequestedInformationOptions(requestedInformation);
        return ObjectFactory.Instance.GetVirtualizationManagementObjects<ISummaryInformation>(base.Server, base.Server.VirtualizationNamespace, "Msvm_SummaryInformation", requestedInformationOptions);
    }

    public IList<ISummaryInformation> GetSummaryInformation(IList<IVMComputerSystem> vmList, SummaryInformationRequest requestedInformation)
    {
        VMTrace.TraceWmiGetSummaryInformation(base.Server, vmList, requestedInformation);
        WmiOperationOptions requestedInformationOptions = SummaryInformation.GetRequestedInformationOptions(requestedInformation);
        IEnumerable<string> propertyValues = vmList.Select((IVMComputerSystem vm) => vm.InstanceId);
        return ObjectLocator.QueryObjectsByProperty<ISummaryInformation>(base.Server, "Name", propertyValues, allowWildcards: false, requestedInformationOptions);
    }
}
