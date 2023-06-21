#define TRACE
using System;
using System.Collections.Generic;
using System.Threading;

namespace Microsoft.Virtualization.Client.Management;

internal class ServiceView : View, IService, IVirtualizationManagementObject
{
    internal static class MemberNames
    {
        public const string Started = "Started";

        public const string Status = "Status";

        public const string Start = "StartService";

        public const string Stop = "StopService";
    }

    private ServiceState? m_StateWorkaround;

    private DateTime m_StateWorkaroundUpdateTime;

    private static readonly TimeSpan gm_StateChangeTimeout = TimeSpan.FromSeconds(30.0);

    public string Name => GetProperty<string>("Name");

    public bool Started
    {
        get
        {
            if (m_StateWorkaround.HasValue)
            {
                return m_StateWorkaround.Value == ServiceState.Running;
            }
            return GetProperty<bool>("Started");
        }
    }

    public ServiceState State
    {
        get
        {
            if (m_StateWorkaround.HasValue)
            {
                return m_StateWorkaround.Value;
            }
            return GetProperty("State", ServiceEnumTypeConverters.ServiceStateConverter);
        }
    }

    public IEnumerable<IService> DependentServices => GetRelatedObjects<IService>(base.Associations.DependentServices);

    internal override void Initialize(IProxy proxy, ObjectKey key)
    {
        base.Initialize(proxy, key);
        ServiceStateWorkaround();
    }

    public override void UpdatePropertyCache(TimeSpan threshold)
    {
        bool num = m_StateWorkaround.HasValue && DateTime.Now - m_StateWorkaroundUpdateTime > threshold;
        base.UpdatePropertyCache(threshold);
        if (num)
        {
            ServiceStateWorkaround();
        }
    }

    public void Start()
    {
        VMTrace.TraceUserActionInitiated(Name + ": Starting service ...");
        try
        {
            ThrowOnError(InvokeMethod("StartService"));
            WaitForServiceState(ServiceState.Running);
            foreach (IService dependentService in DependentServices)
            {
                dependentService.UpdatePropertyCache();
                if (dependentService.State == ServiceState.Stopped)
                {
                    dependentService.Start();
                }
            }
        }
        catch (ServerCallFailedException exception)
        {
            HandleServerCallFailedException(VirtualizationOperation.StartService, exception);
        }
        VMTrace.TraceUserActionCompleted(Name + ": Service started.");
    }

    public void Stop()
    {
        VMTrace.TraceUserActionInitiated(Name + ": Stopping service ...");
        try
        {
            foreach (IService dependentService in DependentServices)
            {
                dependentService.UpdatePropertyCache();
                if (dependentService.State == ServiceState.Running)
                {
                    dependentService.Stop();
                }
            }
            ThrowOnError(InvokeMethod("StopService"));
            WaitForServiceState(ServiceState.Stopped);
        }
        catch (ServerCallFailedException exception)
        {
            HandleServerCallFailedException(VirtualizationOperation.StopService, exception);
        }
        VMTrace.TraceUserActionCompleted(Name + ": Service stopped.");
    }

    private void WaitForServiceState(ServiceState expectedState)
    {
        if (State == expectedState)
        {
            return;
        }
        DateTime now = DateTime.Now;
        TimeSpan timeSpan;
        do
        {
            UpdatePropertyCache();
            if (State != expectedState)
            {
                Thread.Sleep(500);
                timeSpan = DateTime.Now - now;
                continue;
            }
            break;
        }
        while (timeSpan < gm_StateChangeTimeout);
    }

    private void ServiceStateWorkaround()
    {
        object property = GetProperty<object>("Status");
        string property2 = GetProperty<string>("Name");
        if (property != null)
        {
            return;
        }
        bool flag = false;
        try
        {
            IVirtualizationManagementObject virtualizationManagementObject = null;
            switch (property2)
            {
            case "vmms":
                virtualizationManagementObject = ObjectLocator.GetVirtualizationService(base.Server);
                break;
            case "vhdsvc":
                virtualizationManagementObject = ObjectLocator.GetImageManagementService(base.Server);
                break;
            case "nvspwmi":
                virtualizationManagementObject = ObjectLocator.GetVirtualSwitchManagementService(base.Server);
                break;
            }
            if (virtualizationManagementObject != null)
            {
                virtualizationManagementObject.UpdatePropertyCache(TimeSpan.FromSeconds(1.0));
                flag = true;
            }
        }
        catch (VirtualizationManagementException)
        {
        }
        m_StateWorkaround = (flag ? ServiceState.Running : ServiceState.Stopped);
        m_StateWorkaroundUpdateTime = DateTime.Now;
    }

    private static void ThrowOnError(object result)
    {
        ServiceMethodError serviceMethodError = (ServiceMethodError)(uint)result;
        if (serviceMethodError != 0)
        {
            throw new ServiceMethodException(MapErrorToMessage(serviceMethodError), serviceMethodError);
        }
    }

    private static string MapErrorToMessage(ServiceMethodError errorCode)
    {
        return errorCode switch
        {
            ServiceMethodError.NotSupported => ErrorMessages.ServiceError_NotSupported, 
            ServiceMethodError.AccessDenied => ErrorMessages.ServiceError_AccessDenied, 
            ServiceMethodError.DependentServicesRunning => ErrorMessages.ServiceError_DependentServicesRunning, 
            ServiceMethodError.InvalidServiceControl => ErrorMessages.ServiceError_InvalidServiceControl, 
            ServiceMethodError.ServiceCannotAcceptControl => ErrorMessages.ServiceError_ServiceCannotAcceptControl, 
            ServiceMethodError.ServiceNotActive => ErrorMessages.ServiceError_ServiceNotActive, 
            ServiceMethodError.ServiceRequestTimeout => ErrorMessages.ServiceError_ServiceRequestTimeout, 
            ServiceMethodError.UnknownFailure => ErrorMessages.ServiceError_UnknownFailure, 
            ServiceMethodError.PathNotFound => ErrorMessages.ServiceError_PathNotFound, 
            ServiceMethodError.ServiceAlreadyRunning => ErrorMessages.ServiceError_ServiceAlreadyRunning, 
            ServiceMethodError.ServiceDatabaseLocked => ErrorMessages.ServiceError_ServiceDatabaseLocked, 
            ServiceMethodError.ServiceDependencyDeleted => ErrorMessages.ServiceError_ServiceDependencyDeleted, 
            ServiceMethodError.ServiceDependencyFailure => ErrorMessages.ServiceError_ServiceDependencyFailure, 
            ServiceMethodError.ServiceDisabled => ErrorMessages.ServiceError_ServiceDisabled, 
            ServiceMethodError.ServiceLogOnFailure => ErrorMessages.ServiceError_ServiceLogonFailure, 
            ServiceMethodError.ServiceMarkedForDeletion => ErrorMessages.ServiceError_ServiceMarkedForDeletion, 
            ServiceMethodError.ServiceNoThread => ErrorMessages.ServiceError_ServiceNoThread, 
            ServiceMethodError.StatusCircularDependency => ErrorMessages.ServiceError_StatusCircularDependency, 
            ServiceMethodError.StatusDuplicateName => ErrorMessages.ServiceError_StatusDuplicateName, 
            ServiceMethodError.StatusInvalidName => ErrorMessages.ServiceError_StatusInvalidName, 
            ServiceMethodError.StatusInvalidParameter => ErrorMessages.ServiceError_StatusInvalidParameter, 
            ServiceMethodError.StatusInvalidServiceAccount => ErrorMessages.ServiceError_StatusInvalidServiceAccount, 
            ServiceMethodError.StatusServiceExists => ErrorMessages.ServiceError_StatusServiceExists, 
            ServiceMethodError.ServiceAlreadyPaused => ErrorMessages.ServiceError_ServiceAlreadyPaused, 
            _ => string.Empty, 
        };
    }

    private void HandleServerCallFailedException(VirtualizationOperation operation, ServerCallFailedException exception)
    {
        if (exception.FailureReason == ServerCallFailedReason.UnknownProviderError)
        {
            throw ThrowHelper.CreateVirtualizationOperationFailedException(string.Empty, operation, 32769L, GetErrorCodeMapper(), exception.InnerException);
        }
        throw exception;
    }
}
