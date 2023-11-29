using System.Collections.Generic;
using Microsoft.Management.Infrastructure;

namespace Microsoft.Virtualization.Client.Management;

internal abstract class EthernetFeatureView : View, IEthernetFeature, IVirtualizationManagementObject
{
    internal static class WmiQualifierNames
    {
        public const string FeatureID = "UUID";

        public const string ExtensionID = "ExtensionID";
    }

    public string InstanceId => GetProperty<string>("InstanceID");

    public abstract EthernetFeatureType FeatureType { get; }

    public string Name => GetProperty<string>("ElementName");

    public string ExtensionId => GetExtensionId(base.Proxy.CimClass);

    public string FeatureId => GetFeatureId(base.Proxy.CimClass);

    public IVMTask BeginModifySingleFeature(IEthernetSwitchFeatureService service)
    {
        IDictionary<string, object> changedProperties = GetChangedProperties();
        if (changedProperties == null)
        {
            return new CompletedTask(base.Server);
        }
        IEthernetFeature[] features = new EthernetFeatureView[1] { this };
        IVMTask iVMTask = service.BeginModifyFeatures(features);
        iVMTask.PutProperties = changedProperties;
        return iVMTask;
    }

    public void EndModifySingleFeature(IVMTask modifyTask)
    {
        EndPut(modifyTask);
    }

    internal static string GetFeatureId(ICimClass cimClass)
    {
        return ExtractQualifierValue(cimClass, "UUID");
    }

    internal static string GetExtensionId(ICimClass cimClass)
    {
        return ExtractQualifierValue(cimClass, "ExtensionID");
    }

    private static string ExtractQualifierValue(ICimClass cimClass, string propertyName)
    {
        WmiNameMapper.TryResolveCimClassToExtensibleType(cimClass, out var _);
        CimQualifier cimQualifier = cimClass.CimClassQualifiers[propertyName];
        if (cimQualifier != null)
        {
            return cimQualifier.Value as string;
        }
        return null;
    }
}
