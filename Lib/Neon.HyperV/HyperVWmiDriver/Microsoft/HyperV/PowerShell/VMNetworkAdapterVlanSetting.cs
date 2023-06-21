using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMNetworkAdapterVlanSetting : VMNetworkAdapterFeatureBase
{
    public VMNetworkAdapterVlanMode OperationMode
    {
        get
        {
            return (VMNetworkAdapterVlanMode)((IEthernetSwitchPortVlanFeature)m_FeatureSetting).OperationMode;
        }
        internal set
        {
            ((IEthernetSwitchPortVlanFeature)m_FeatureSetting).OperationMode = (VlanOperationMode)value;
        }
    }

    public int AccessVlanId
    {
        get
        {
            return ((IEthernetSwitchPortVlanFeature)m_FeatureSetting).AccessVlan;
        }
        internal set
        {
            ((IEthernetSwitchPortVlanFeature)m_FeatureSetting).AccessVlan = value;
        }
    }

    public int NativeVlanId
    {
        get
        {
            return ((IEthernetSwitchPortVlanFeature)m_FeatureSetting).NativeVlan;
        }
        internal set
        {
            ((IEthernetSwitchPortVlanFeature)m_FeatureSetting).NativeVlan = value;
        }
    }

    [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Need to accept input for the parameter.")]
    [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "This is per spec.")]
    public List<int> AllowedVlanIdList
    {
        get
        {
            return ((IEthernetSwitchPortVlanFeature)m_FeatureSetting).GetTrunkVlanList().ToList();
        }
        internal set
        {
            ((IEthernetSwitchPortVlanFeature)m_FeatureSetting).SetTrunkVlanList(value);
        }
    }

    public string AllowedVlanIdListString => FormatVlanIdList(AllowedVlanIdList);

    public VMNetworkAdapterPrivateVlanMode PrivateVlanMode
    {
        get
        {
            return (VMNetworkAdapterPrivateVlanMode)((IEthernetSwitchPortVlanFeature)m_FeatureSetting).PrivateMode;
        }
        internal set
        {
            ((IEthernetSwitchPortVlanFeature)m_FeatureSetting).PrivateMode = (PrivateVlanMode)value;
        }
    }

    public int PrimaryVlanId
    {
        get
        {
            return ((IEthernetSwitchPortVlanFeature)m_FeatureSetting).PrimaryVlan;
        }
        internal set
        {
            ((IEthernetSwitchPortVlanFeature)m_FeatureSetting).PrimaryVlan = value;
        }
    }

    public int SecondaryVlanId
    {
        get
        {
            VMNetworkAdapterPrivateVlanMode privateVlanMode = PrivateVlanMode;
            if (privateVlanMode == VMNetworkAdapterPrivateVlanMode.Isolated || privateVlanMode == VMNetworkAdapterPrivateVlanMode.Community)
            {
                return ((IEthernetSwitchPortVlanFeature)m_FeatureSetting).SecondaryVlan;
            }
            return 0;
        }
        internal set
        {
            ((IEthernetSwitchPortVlanFeature)m_FeatureSetting).SecondaryVlan = value;
        }
    }

    [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Need to accept input for the parameter.")]
    [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "This is per spec.")]
    public List<int> SecondaryVlanIdList
    {
        get
        {
            if (PrivateVlanMode == VMNetworkAdapterPrivateVlanMode.Promiscuous)
            {
                return ((IEthernetSwitchPortVlanFeature)m_FeatureSetting).GetSecondaryVlanList().ToList();
            }
            return null;
        }
        internal set
        {
            ((IEthernetSwitchPortVlanFeature)m_FeatureSetting).SetSecondaryVlanList(value);
        }
    }

    public string SecondaryVlanIdListString => FormatVlanIdList(SecondaryVlanIdList);

    internal VMNetworkAdapterVlanSetting(IEthernetSwitchPortVlanFeature vlanFeature, VMNetworkAdapterBase parentAdapter)
        : base(vlanFeature, parentAdapter, isTemplate: false)
    {
    }

    private VMNetworkAdapterVlanSetting(VMNetworkAdapterBase parentAdapter)
        : base(parentAdapter, EthernetFeatureType.Vlan)
    {
    }

    internal static VMNetworkAdapterVlanSetting CreateTemplateVlanSetting(VMNetworkAdapterBase parentAdapter)
    {
        return new VMNetworkAdapterVlanSetting(parentAdapter);
    }

    internal void ClearSettings()
    {
        IEthernetSwitchPortVlanFeature ethernetSwitchPortVlanFeature = (IEthernetSwitchPortVlanFeature)VMNetworkingFeatureBase.GetDefaultFeatureSettingInstance(base.Server, EthernetFeatureType.Vlan);
        IEthernetSwitchPortVlanFeature obj = (IEthernetSwitchPortVlanFeature)m_FeatureSetting;
        obj.AccessVlan = ethernetSwitchPortVlanFeature.AccessVlan;
        obj.NativeVlan = ethernetSwitchPortVlanFeature.NativeVlan;
        obj.OperationMode = ethernetSwitchPortVlanFeature.OperationMode;
        obj.PrivateMode = ethernetSwitchPortVlanFeature.PrivateMode;
        obj.PrimaryVlan = ethernetSwitchPortVlanFeature.PrimaryVlan;
        obj.SecondaryVlan = ethernetSwitchPortVlanFeature.SecondaryVlan;
    }

    private static string FormatVlanIdList(IList<int> vlanIds)
    {
        if (vlanIds == null)
        {
            return null;
        }
        if (vlanIds.Count < 1)
        {
            return string.Empty;
        }
        List<int> list = vlanIds.ToList();
        list.Sort();
        List<Tuple<int, int>> list2 = new List<Tuple<int, int>>();
        int num = list[0];
        int num2 = num;
        foreach (int item in list.Skip(1))
        {
            if (item > num2 + 1)
            {
                list2.Add(Tuple.Create(num, num2));
                num = item;
                num2 = item;
            }
            else
            {
                num2 = item;
            }
        }
        list2.Add(Tuple.Create(num, num2));
        string[] value = list2.Select(FormatVlanRange).ToArray();
        return string.Join(",", value);
    }

    private static string FormatVlanRange(Tuple<int, int> range)
    {
        int item = range.Item1;
        int item2 = range.Item2;
        if (item == item2)
        {
            return item.ToString(CultureInfo.InvariantCulture);
        }
        return string.Format(CultureInfo.InvariantCulture, "{0}-{1}", item, item2);
    }
}
