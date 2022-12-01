using System;
using System.Management;

namespace Neon.HyperV
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                using (var hypervWmi = new HyperVWmi())
                {
                    hypervWmi.ValidateDisk(@"C:\Temp\test.vhdx");
                }
            }
            catch (Exception e)
            {
            }
        }
    }
}
