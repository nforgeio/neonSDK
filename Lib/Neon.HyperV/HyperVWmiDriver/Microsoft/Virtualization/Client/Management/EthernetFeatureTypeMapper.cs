using System;
using System.Globalization;

namespace Microsoft.Virtualization.Client.Management;

internal static class EthernetFeatureTypeMapper
{
    internal const string Offload = "C885BFD1-ABB7-418F-8163-9F379C9F7166";

    internal const string Vlan = "952C5004-4465-451C-8CB8-FA9AB382B773";

    internal const string Acl = "998BEF4A-5D55-492A-9C43-8B2F5EAE9F2B";

    internal const string Bandwidth = "24AD3CE1-69BD-4978-B2AC-DAAD389D699C";

    internal const string Security = "776E0BA7-94A1-41C8-8F28-951F524251B5";

    internal const string ExtendedAcl = "CD168FF0-A16D-4514-B7B5-8BBBA791A928";

    internal const string RoutingDomain = "BF874DF0-F729-4312-A0FA-194271B88990";

    internal const string Isolation = "83AF2CCB-72C9-4479-A285-94E58A98CAA6";

    internal const string SwitchBandwidth = "3EB2B8E8-4ABF-4DBF-9071-16DD47481FBE";

    internal const string SwitchNicTeaming = "17AD26F1-DD6F-4ED9-BD2F-3750ADE6B68B";

    internal const string Rdma = "7A498FC4-8572-4FDC-92AB-7A6FC7042D53";

    internal const string TeamMapping = "8D45D4BD-8C18-435C-98A7-D632A7C618FF";

    internal const string SwitchOffload = "1550E863-4337-4917-A040-5A27DBC58B59";

    public static string MapFeatureTypeToFeatureId(EthernetFeatureType featureType)
    {
        return featureType switch
        {
            EthernetFeatureType.Offload => "C885BFD1-ABB7-418F-8163-9F379C9F7166", 
            EthernetFeatureType.Vlan => "952C5004-4465-451C-8CB8-FA9AB382B773", 
            EthernetFeatureType.Acl => "998BEF4A-5D55-492A-9C43-8B2F5EAE9F2B", 
            EthernetFeatureType.Bandwidth => "24AD3CE1-69BD-4978-B2AC-DAAD389D699C", 
            EthernetFeatureType.Security => "776E0BA7-94A1-41C8-8F28-951F524251B5", 
            EthernetFeatureType.ExtendedAcl => "CD168FF0-A16D-4514-B7B5-8BBBA791A928", 
            EthernetFeatureType.RoutingDomain => "BF874DF0-F729-4312-A0FA-194271B88990", 
            EthernetFeatureType.Isolation => "83AF2CCB-72C9-4479-A285-94E58A98CAA6", 
            EthernetFeatureType.SwitchBandwidth => "3EB2B8E8-4ABF-4DBF-9071-16DD47481FBE", 
            EthernetFeatureType.SwitchNicTeaming => "17AD26F1-DD6F-4ED9-BD2F-3750ADE6B68B", 
            EthernetFeatureType.Rdma => "7A498FC4-8572-4FDC-92AB-7A6FC7042D53", 
            EthernetFeatureType.TeamMapping => "8D45D4BD-8C18-435C-98A7-D632A7C618FF", 
            EthernetFeatureType.SwitchOffload => "1550E863-4337-4917-A040-5A27DBC58B59", 
            _ => throw new ArgumentOutOfRangeException("featureType", string.Format(CultureInfo.CurrentCulture, ErrorMessages.ArgumentOutOfRange_InvalidEnumValue, featureType.ToString(), typeof(EthernetFeatureType).Name)), 
        };
    }
}
