//-----------------------------------------------------------------------------
// FILE:	    HyperVWmi.Utility.cs
// CONTRIBUTOR: Jeff Lill
// COPYRIGHT:   NONE
//
// This code was obtained from a Microsoft code sample which did not include a
// license or copyright statement.  We're going to apply the Apache License to
// this to be compatible with the rest of the neon-sdk. 
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#pragma warning disable CA1416  // Validate platform compatibility

using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace Neon.HyperV
{
    internal sealed partial class HyperVWmi
    {
        public class Utility
        {
            static class JobState
            {
                public const UInt16 New = 2;
                public const UInt16 Starting = 3;
                public const UInt16 Running = 4;
                public const UInt16 Suspended = 5;
                public const UInt16 ShuttingDown = 6;
                public const UInt16 Completed = 7;
                public const UInt16 Terminated = 8;
                public const UInt16 Killed = 9;
                public const UInt16 Exception = 10;
                public const UInt16 Service = 11;
            }
            /// <summary>
            /// Common utility function to get a service object
            /// </summary>
            /// <param name="scope"></param>
            /// <param name="serviceName"></param>
            /// <returns></returns>
            public static ManagementObject GetServiceObject(ManagementScope scope, string serviceName)
            {
                ManagementPath wmiPath = new ManagementPath(serviceName);
                ManagementClass serviceClass = new ManagementClass(scope, wmiPath, null);
                ManagementObjectCollection services = serviceClass.GetInstances();

                ManagementObject serviceObject = null;

                foreach (ManagementObject service in services)
                {
                    serviceObject = service;
                }
                return serviceObject;
            }

            public static ManagementObject GetHostSystemDevice(string deviceClassName, string deviceObjectElementName, ManagementScope scope)
            {
                string hostName = System.Environment.MachineName;
                ManagementObject systemDevice = GetSystemDevice(deviceClassName, deviceObjectElementName, hostName, scope);
                return systemDevice;
            }


            public static ManagementObject GetSystemDevice
            (
                string deviceClassName,
                string deviceObjectElementName,
                string vmName,
                ManagementScope scope)
            {
                ManagementObject systemDevice = null;
                ManagementObject computerSystem = Utility.GetTargetComputer(vmName, scope);

                ManagementObjectCollection systemDevices = computerSystem.GetRelated
                (
                    deviceClassName,
                    "Msvm_SystemDevice",
                    null,
                    null,
                    "PartComponent",
                    "GroupComponent",
                    false,
                    null
                );

                foreach (ManagementObject device in systemDevices)
                {
                    if (device["ElementName"].ToString().ToLower() == deviceObjectElementName.ToLower())
                    {
                        systemDevice = device;
                        break;
                    }
                }

                return systemDevice;
            }

            public static bool JobCompleted(ManagementBaseObject outParams, ManagementScope scope)
            {
                bool jobCompleted = true;

                //Retrieve msvc_StorageJob path. This is a full wmi path
                string JobPath = (string)outParams["Job"];
                ManagementObject Job = new ManagementObject(scope, new ManagementPath(JobPath), null);
                //Try to get storage job information
                Job.Get();
                while ((UInt16)Job["JobState"] == JobState.Starting
                    || (UInt16)Job["JobState"] == JobState.Running)
                {
                    Console.WriteLine("In progress... {0}% completed.", Job["PercentComplete"]);
                    System.Threading.Thread.Sleep(1000);
                    Job.Get();
                }

                //Figure out if job failed
                UInt16 jobState = (UInt16)Job["JobState"];
                if (jobState != JobState.Completed)
                {
                    UInt16 jobErrorCode = (UInt16)Job["ErrorCode"];
                    Console.WriteLine("Error Code:{0}", jobErrorCode);
                    Console.WriteLine("ErrorDescription: {0}", (string)Job["ErrorDescription"]);
                    jobCompleted = false;
                }
                return jobCompleted;
            }


            public static ManagementObject GetTargetComputer(string vmElementName, ManagementScope scope)
            {
                string query = string.Format("select * from Msvm_ComputerSystem Where ElementName = '{0}'", vmElementName);

                ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, new ObjectQuery(query));

                ManagementObjectCollection computers = searcher.Get();

                ManagementObject computer = null;

                foreach (ManagementObject instance in computers)
                {
                    computer = instance;
                    break;
                }
                return computer;
            }

            public static ManagementObject GetVirtualSystemSettingData(ManagementObject vm)
            {
                ManagementObject vmSetting = null;
                ManagementObjectCollection vmSettings = vm.GetRelated
                (
                    "Msvm_VirtualSystemSettingData",
                    "Msvm_SettingsDefineState",
                    null,
                    null,
                    "SettingData",
                    "ManagedElement",
                    false,
                    null
                );

                if (vmSettings.Count != 1)
                {
                    throw new Exception(String.Format("{0} instance of Msvm_VirtualSystemSettingData was found", vmSettings.Count));
                }

                foreach (ManagementObject instance in vmSettings)
                {
                    vmSetting = instance;
                    break;
                }

                return vmSetting;
            }

            enum ValueRole
            {
                Default = 0,
                Minimum = 1,
                Maximum = 2,
                Increment = 3
            }
            enum ValueRange
            {
                Default = 0,
                Minimum = 1,
                Maximum = 2,
                Increment = 3
            }

            //
            // Get RASD definitions
            //
            public static ManagementObject GetResourceAllocationsettingDataDefault
            (
                ManagementScope scope,
                UInt16 resourceType,
                string resourceSubType,
                string otherResourceType
                )
            {
                ManagementObject RASD = null;

                string query = String.Format("select * from Msvm_ResourcePool where ResourceType = '{0}' and ResourceSubType ='{1}' and OtherResourceType = '{2}'",
                                 resourceType, resourceSubType, otherResourceType);

                if (resourceType == ResourceType.Other)
                {
                    query = String.Format("select * from Msvm_ResourcePool where ResourceType = '{0}' and ResourceSubType = null and OtherResourceType = {1}",
                                                 resourceType, otherResourceType);
                }
                else
                {
                    query = String.Format("select * from Msvm_ResourcePool where ResourceType = '{0}' and ResourceSubType ='{1}' and OtherResourceType = null",
                                                 resourceType, resourceSubType);
                }

                ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, new ObjectQuery(query));

                ManagementObjectCollection poolResources = searcher.Get();

                //Get pool resource allocation ability
                if (poolResources.Count == 1)
                {
                    foreach (ManagementObject poolResource in poolResources)
                    {
                        ManagementObjectCollection allocationCapabilities = poolResource.GetRelated("Msvm_AllocationCapabilities");
                        foreach (ManagementObject allocationCapability in allocationCapabilities)
                        {
                            ManagementObjectCollection settingDatas = allocationCapability.GetRelationships("Msvm_SettingsDefineCapabilities");
                            foreach (ManagementObject settingData in settingDatas)
                            {

                                if (Convert.ToInt16(settingData["ValueRole"]) == (UInt16)ValueRole.Default)
                                {
                                    RASD = new ManagementObject(settingData["PartComponent"].ToString());
                                    break;
                                }
                            }
                        }
                    }
                }

                return RASD;
            }

            public static ManagementObject GetResourceAllocationsettingData
            (
                ManagementObject vm,
                UInt16 resourceType,
                string resourceSubType,
                string otherResourceType
                )
            {
                //vm->vmsettings->RASD for IDE controller
                ManagementObject RASD = null;
                ManagementObjectCollection settingDatas = vm.GetRelated("Msvm_VirtualSystemsettingData");
                foreach (ManagementObject settingData in settingDatas)
                {
                    //retrieve the rasd
                    ManagementObjectCollection RASDs = settingData.GetRelated("Msvm_ResourceAllocationsettingData");
                    foreach (ManagementObject rasdInstance in RASDs)
                    {
                        if (Convert.ToUInt16(rasdInstance["ResourceType"]) == resourceType)
                        {
                            //found the matching type
                            if (resourceType == ResourceType.Other)
                            {
                                if (rasdInstance["OtherResourceType"].ToString() == otherResourceType)
                                {
                                    RASD = rasdInstance;
                                    break;
                                }
                            }
                            else
                            {
                                if (rasdInstance["ResourceSubType"].ToString() == resourceSubType)
                                {
                                    RASD = rasdInstance;
                                    break;
                                }
                            }
                        }
                    }

                }
                return RASD;
            }
        }
    }
}
