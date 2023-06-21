#define TRACE
using System;
using System.Reflection;
using System.Text;
using Microsoft.Management.Infrastructure;
using Microsoft.Virtualization.Client.Management.Clustering;

namespace Microsoft.Virtualization.Client.Management;

internal class ViewFactory : IViewFactory
{
    private static readonly IViewFactory s_Instance = new ViewFactory();

    internal static IViewFactory Instance => s_Instance;

    private ViewFactory()
    {
    }

    T IViewFactory.CreateView<T>(IProxy proxy, ObjectKey key)
    {
        View obj = (View)Activator.CreateInstance(MapProxyToWrappingViewType(proxy));
        obj.Initialize(proxy, key);
        return (T)(object)obj;
    }

    private static Type MapProxyToWrappingViewType(IProxy proxy)
    {
        string text = (string)proxy.GetProperty("__CLASS");
        Type type = null;
        Type type2 = null;
        Exception innerException = null;
        try
        {
            if (string.Equals(text, "Msvm_ResourceAllocationSettingData", StringComparison.OrdinalIgnoreCase) || string.Equals(text, "Msvm_FcPortAllocationSettingData", StringComparison.OrdinalIgnoreCase) || string.Equals(text, "Msvm_StorageAllocationSettingData", StringComparison.OrdinalIgnoreCase))
            {
                int resourceType = 0;
                object property = proxy.GetProperty("ResourceType");
                if (property != null)
                {
                    resourceType = NumberConverter.UInt16ToInt32((ushort)property);
                }
                string resourceSubType = (string)proxy.GetProperty("ResourceSubType");
                string otherResourceType = (string)proxy.GetProperty("OtherResourceType");
                VMDeviceSettingType deviceType = VMDeviceSettingTypeMapper.MapResourceSubTypeToVMDeviceSettingType(resourceType, resourceSubType, otherResourceType);
                if (type == null)
                {
                    type = VMDeviceSettingTypeMapper.MapVMDeviceSettingTypeToObjectModelType(deviceType);
                }
            }
            else if (string.Equals(text, "Msvm_EthernetPortAllocationSettingData", StringComparison.OrdinalIgnoreCase))
            {
                string[] array = (string[])proxy.GetProperty("HostResource");
                if (array == null || array.Length == 0 || string.IsNullOrEmpty(array[0]))
                {
                    type = typeof(IEthernetConnectionAllocationRequest);
                }
                else
                {
                    try
                    {
                        type = ((!string.Equals(new WmiObjectPath(array[0]).ClassName, "Msvm_VirtualEthernetSwitch", StringComparison.OrdinalIgnoreCase)) ? typeof(IVirtualEthernetSwitchPortSetting) : typeof(IEthernetConnectionAllocationRequest));
                    }
                    catch (CimException ex)
                    {
                        VMTrace.TraceError("Connection RASD HostResource property not formatted as a ManagmentPath as expected.", ex);
                        throw new ArgumentException(null, "proxy");
                    }
                }
            }
            else if (string.Equals(text, "Msvm_ResourcePool", StringComparison.OrdinalIgnoreCase))
            {
                int resourceType2 = 0;
                object property2 = proxy.GetProperty("ResourceType");
                if (property2 != null)
                {
                    resourceType2 = NumberConverter.UInt16ToInt32((ushort)property2);
                }
                string resourceSubType2 = (string)proxy.GetProperty("ResourceSubType");
                type = VMDeviceSettingTypeMapper.MapResourcePoolTypeToObjectModelType(VMDeviceSettingTypeMapper.MapResourceSubTypeToVMDeviceSettingType(resourceType2, resourceSubType2, null));
            }
            else if (string.Equals(text, "Msvm_ComputerSystem", StringComparison.OrdinalIgnoreCase))
            {
                string b = (string)proxy.Path.KeyValues["Name"];
                type = ((!string.Equals(proxy.Key.Server.Name, b, StringComparison.OrdinalIgnoreCase)) ? WmiNameMapper.MapWmiClassNameToType(text, proxy.CimClass) : (HostComputerSystemBaseView.IsHostClusterComputerSystem(proxy) ? typeof(IHostClusterComputerSystem) : typeof(IHostComputerSystem)));
            }
            else if (string.Equals(text, "Msvm_ConcreteJob", StringComparison.OrdinalIgnoreCase))
            {
                int num = NumberConverter.UInt16ToInt32((ushort)proxy.GetProperty("JobType"));
                type = ((num < 130 || num > 139) ? typeof(IVMVirtualizationTask) : typeof(IVMNetworkTask));
            }
            else if (string.Equals(text, "MSCluster_Resource", StringComparison.OrdinalIgnoreCase))
            {
                string a = (string)proxy.GetProperty("Type");
                if (string.Equals(a, "Virtual Machine", StringComparison.OrdinalIgnoreCase))
                {
                    type = typeof(IMSClusterVMResource);
                }
                else if (string.Equals(a, "Virtual Machine Configuration", StringComparison.OrdinalIgnoreCase))
                {
                    type = typeof(IMSClusterVMConfigurationResource);
                }
                else if (string.Equals(a, "Virtual Machine Cluster WMI", StringComparison.OrdinalIgnoreCase))
                {
                    type = typeof(IMSClusterWmiProviderResource);
                }
                else if (string.Equals(a, "Virtual Machine Replication Broker", StringComparison.OrdinalIgnoreCase))
                {
                    type = typeof(IMSClusterReplicaBrokerResource);
                }
                else if (string.Equals(a, "Network Name", StringComparison.OrdinalIgnoreCase))
                {
                    type = typeof(IMSClusterCapResource);
                }
            }
            else
            {
                type = WmiNameMapper.MapWmiClassNameToType(text, proxy.CimClass);
            }
            if (type != null)
            {
                type2 = MapInterfaceToImplementingViewType(type);
            }
        }
        catch (NoWmiMappingException ex2)
        {
            innerException = ex2;
        }
        catch (TypeLoadException ex3)
        {
            innerException = ex3;
        }
        catch (InvalidCastException ex4)
        {
            innerException = ex4;
        }
        if (type2 == null || type2.GetTypeInfo().IsAbstract)
        {
            throw new ArgumentException(null, "proxy", innerException);
        }
        return type2;
    }

    private static Type MapInterfaceToImplementingViewType(Type interfaceType)
    {
        StringBuilder stringBuilder = new StringBuilder(interfaceType.FullName.Length + 5);
        stringBuilder.Append(interfaceType.Namespace);
        stringBuilder.Append('.');
        stringBuilder.Append(interfaceType.Name.Substring(1));
        stringBuilder.Append("View");
        return Type.GetType(stringBuilder.ToString(), throwOnError: true);
    }
}
