using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Virtualization.Client.Management;

internal sealed class EthernetSwitchPortVlanFeatureView : EthernetSwitchPortFeatureView, IEthernetSwitchPortVlanFeature, IEthernetSwitchPortFeature, IEthernetFeature, IVirtualizationManagementObject
{
    internal static class WmiMemberNames
    {
        public const string AccessVlan = "AccessVlanId";

        public const string NativeVlan = "NativeVlanId";

        public const string OperationMode = "OperationMode";

        public const string PrivateVlanMode = "PvlanMode";

        public const string TrunkVlanList = "TrunkVlanIdArray";

        public const string PrimaryVlan = "PrimaryVlanId";

        public const string SecondaryVlan = "SecondaryVlanId";

        public const string SecondaryVlanList = "SecondaryVlanIdArray";
    }

    public int AccessVlan
    {
        get
        {
            return NumberConverter.UInt16ToInt32(GetProperty<ushort>("AccessVlanId"));
        }
        set
        {
            ushort num = NumberConverter.Int32ToUInt16(value);
            SetProperty("AccessVlanId", num);
        }
    }

    public int NativeVlan
    {
        get
        {
            return NumberConverter.UInt16ToInt32(GetProperty<ushort>("NativeVlanId"));
        }
        set
        {
            ushort num = NumberConverter.Int32ToUInt16(value);
            SetProperty("NativeVlanId", num);
        }
    }

    public VlanOperationMode OperationMode
    {
        get
        {
            return (VlanOperationMode)GetProperty<uint>("OperationMode");
        }
        set
        {
            SetProperty("OperationMode", (uint)value);
        }
    }

    public override EthernetFeatureType FeatureType => EthernetFeatureType.Vlan;

    public PrivateVlanMode PrivateMode
    {
        get
        {
            return (PrivateVlanMode)GetProperty<uint>("PvlanMode");
        }
        set
        {
            SetProperty("PvlanMode", (uint)value);
        }
    }

    public int PrimaryVlan
    {
        get
        {
            return NumberConverter.UInt16ToInt32(GetProperty<ushort>("PrimaryVlanId"));
        }
        set
        {
            ushort num = NumberConverter.Int32ToUInt16(value);
            SetProperty("PrimaryVlanId", num);
        }
    }

    public int SecondaryVlan
    {
        get
        {
            return NumberConverter.UInt16ToInt32(GetProperty<ushort>("SecondaryVlanId"));
        }
        set
        {
            ushort num = NumberConverter.Int32ToUInt16(value);
            SetProperty("SecondaryVlanId", num);
        }
    }

    public IList<int> GetTrunkVlanList()
    {
        ushort[] property = GetProperty<ushort[]>("TrunkVlanIdArray");
        int[] array = new int[property.Length];
        for (int i = 0; i < property.Length; i++)
        {
            array[i] = NumberConverter.UInt16ToInt32(property[i]);
        }
        return array;
    }

    public void SetTrunkVlanList(IList<int> trunkList)
    {
        ushort[] array;
        if (trunkList != null)
        {
            array = new ushort[trunkList.Count];
            for (int i = 0; i < trunkList.Count; i++)
            {
                array[i] = NumberConverter.Int32ToUInt16(trunkList[i]);
            }
        }
        else
        {
            array = new ushort[0];
        }
        SetProperty("TrunkVlanIdArray", array);
    }

    public IList<int> GetSecondaryVlanList()
    {
        switch (PrivateMode)
        {
        case PrivateVlanMode.Isolated:
        case PrivateVlanMode.Community:
        {
            ushort property2 = GetProperty<ushort>("SecondaryVlanId");
            return new int[1] { NumberConverter.UInt16ToInt32(property2) };
        }
        case PrivateVlanMode.Promiscuous:
        {
            ushort[] property = GetProperty<ushort[]>("SecondaryVlanIdArray");
            int[] array = new int[property.Length];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = NumberConverter.UInt16ToInt32(property[i]);
            }
            return array;
        }
        default:
            return new int[0];
        }
    }

    public void SetSecondaryVlanList(IList<int> secondaryVlanList)
    {
        ushort[] value = ((secondaryVlanList != null) ? secondaryVlanList.Select(NumberConverter.Int32ToUInt16).ToArray() : new ushort[0]);
        SetProperty("SecondaryVlanIdArray", value);
    }
}
