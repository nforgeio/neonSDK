using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;

namespace Microsoft.HyperV.PowerShell;

internal static class NetworkUtilities
{
    public static void ParseSubnetString(string subnetAddress, out string address, out byte prefixLength)
    {
        subnetAddress = subnetAddress.Trim();
        int num = subnetAddress.IndexOf('/');
        IPAddress iPAddress;
        if (num >= 0)
        {
            string s = subnetAddress.Substring(num + 1);
            address = subnetAddress.Substring(0, num);
            prefixLength = byte.Parse(s, CultureInfo.CurrentCulture);
            iPAddress = IPAddress.Parse(address);
            if (prefixLength > 128 || (prefixLength > 32 && iPAddress.AddressFamily == AddressFamily.InterNetwork))
            {
                throw new FormatException();
            }
        }
        else
        {
            address = subnetAddress;
            iPAddress = IPAddress.Parse(address);
            prefixLength = (byte)((iPAddress.AddressFamily == AddressFamily.InterNetwork) ? 32 : 128);
        }
        byte[] addressBytes = iPAddress.GetAddressBytes();
        int num2 = 0;
        bool flag = false;
        int num3 = addressBytes.Length - 1;
        while (!flag && num3 >= 0)
        {
            byte b = addressBytes[num3];
            if (b == 0)
            {
                num2 += 8;
            }
            else
            {
                while ((int)b % 2 == 0)
                {
                    b = (byte)((int)b / 2);
                    num2++;
                }
                flag = true;
            }
            num3--;
        }
        int num4 = addressBytes.Length * 8 - num2;
        if (prefixLength < num4)
        {
            throw new FormatException();
        }
    }
}
