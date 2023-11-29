#define TRACE
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.Virtualization.Client.Management;

internal class VMDeviceSettingView : View, IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable
{
    internal static class WmiDeviceSettingPropertyNames
    {
        public const string FriendlyName = "ElementName";

        public const string DeviceId = "InstanceID";

        public const string ResourceSubType = "ResourceSubType";

        public const string VMBusChannelInstanceGuid = "VirtualSystemIdentifiers";

        public const string ResourceType = "ResourceType";

        public const string OtherResourceType = "OtherResourceType";

        public const string PoolId = "PoolID";

        public const string Delete = "RemoveResourceSettings";

        public const string Put = "ModifyResourceSettings";
    }

    public string FriendlyName
    {
        get
        {
            return GetProperty<string>("ElementName");
        }
        set
        {
            if (value == null)
            {
                value = string.Empty;
            }
            SetProperty("ElementName", value);
        }
    }

    public string DeviceTypeName
    {
        get
        {
            int num = 0;
            try
            {
                num = NumberConverter.UInt16ToInt32(GetProperty<ushort>("ResourceType"));
            }
            catch (InvalidWmiValueException ex)
            {
                VMTrace.TraceError("Error getting device resource type!", ex);
            }
            return GetProperty<string>((num == 1) ? "OtherResourceType" : "ResourceSubType");
        }
    }

    public string DeviceId => GetProperty<string>("InstanceID");

    public string PoolId
    {
        get
        {
            return GetProperty<string>("PoolID");
        }
        set
        {
            if (value == null)
            {
                value = string.Empty;
            }
            SetProperty("PoolID", value);
        }
    }

    public Guid VMBusChannelInstanceGuid
    {
        get
        {
            string[] property = GetProperty<string[]>("VirtualSystemIdentifiers");
            if (property != null && property.Length != 0)
            {
                try
                {
                    return new Guid(property[0]);
                }
                catch (FormatException)
                {
                    VMTrace.TraceWarning("The format of the virtual system identifier is not valid. VirtualSystemIdentifiers[0]: " + property[0] + " Returning Guid.Empty in its place.");
                    return Guid.Empty;
                }
                catch (OverflowException)
                {
                    VMTrace.TraceWarning("Overflow formatting the virtual system identifier. VirtualSystemIdentifiers[0]: " + property[0] + "Returning Guid.Empty in its place.");
                    return Guid.Empty;
                }
            }
            return Guid.Empty;
        }
        set
        {
            SetProperty("VirtualSystemIdentifiers", new string[1] { value.ToString("B") });
        }
    }

    public virtual VMDeviceSettingType VMDeviceSettingType
    {
        get
        {
            int resourceType = 0;
            try
            {
                resourceType = NumberConverter.UInt16ToInt32(GetProperty<ushort>("ResourceType"));
            }
            catch (InvalidWmiValueException ex)
            {
                VMTrace.TraceError("Error getting device resource type!", ex);
            }
            string property = GetProperty<string>("ResourceSubType");
            string property2 = GetProperty<string>("OtherResourceType");
            return VMDeviceSettingTypeMapper.MapResourceSubTypeToVMDeviceSettingType(resourceType, property, property2);
        }
    }

    public IVMDevice VirtualDevice => GetRelatedObject<IVMDevice>(base.Associations.LogicalDeviceToSetting, throwIfNotFound: false);

    public IVMComputerSystemSetting VirtualComputerSystemSetting => GetRelatedObject<IVMComputerSystemSetting>(base.Associations.SystemSettingSettingData);

    protected override IVMTask BeginPutInternal(IDictionary<string, object> properties)
    {
        string text = base.Proxy.GetProperty("ElementName") as string;
        text = text ?? string.Empty;
        string embeddedInstance = GetEmbeddedInstance(properties);
        IProxy serviceProxy = GetServiceProxy();
        object[] array = new object[3]
        {
            new string[1] { embeddedInstance },
            null,
            null
        };
        string clientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.ModifyDeviceSettingFailed, text);
        VMTrace.TraceUserActionInitiatedWithProperties(string.Format(CultureInfo.InvariantCulture, "Modifying device '{0}' ('{1}') of type '{2}'.", DeviceId, FriendlyName, DeviceTypeName), properties);
        uint result = serviceProxy.InvokeMethod("ModifyResourceSettings", array);
        IVMTask iVMTask = BeginMethodTaskReturn(result, array[1], array[2]);
        iVMTask.PutProperties = properties;
        iVMTask.ClientSideFailedMessage = clientSideFailedMessage;
        return iVMTask;
    }

    public void Delete()
    {
        using IVMTask iVMTask = BeginDelete();
        iVMTask.WaitForCompletion();
        EndDelete(iVMTask);
    }

    public virtual IVMTask BeginDelete()
    {
        IProxy serviceProxy = GetServiceProxy();
        object[] array = new object[2]
        {
            new VMDeviceSettingView[1] { this },
            null
        };
        string clientSideFailedMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessages.DeleteDeviceFailed, FriendlyName);
        VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Deleting device '{0}' ('{1}') of type '{2}'.", DeviceId, FriendlyName, DeviceTypeName));
        uint result = serviceProxy.InvokeMethod("RemoveResourceSettings", array);
        IVMTask iVMTask = BeginMethodTaskReturn(result, null, array[1]);
        iVMTask.ClientSideFailedMessage = clientSideFailedMessage;
        return iVMTask;
    }

    public virtual void EndDelete(IVMTask deleteTask)
    {
        EndMethod(deleteTask, VirtualizationOperation.Delete);
        VMTrace.TraceUserActionCompleted("Device deleted successfully.");
    }
}
