using System;

namespace Microsoft.Virtualization.Client.Management;

internal static class NumberConverter
{
    public static int UInt16ToInt32(ushort value)
    {
        return value;
    }

    public static long UInt16ToInt64(ushort value)
    {
        return value;
    }

    public static ushort Int32ToUInt16(int value)
    {
        if (value < 0 || value > 65535)
        {
            throw new ArgumentOutOfRangeException("value");
        }
        return (ushort)value;
    }

    public static int UInt32ToInt32(uint value)
    {
        return (int)value;
    }

    public static uint Int32ToUInt32(int value)
    {
        return (uint)value;
    }

    public static long UInt64ToInt64(ulong value)
    {
        return (long)value;
    }

    public static ulong Int64ToUInt64(long value)
    {
        return (ulong)value;
    }

    public static int UInt64ToInt32(ulong value)
    {
        return (int)value;
    }

    public static ulong Int32ToUInt64(int value)
    {
        return (ulong)value;
    }

    public static byte Int32ToByte(int value)
    {
        return (byte)value;
    }

    public static long UInt32ToInt64(uint value)
    {
        return value;
    }

    public static uint Int64ToUInt32(long value)
    {
        return (uint)value;
    }
}
