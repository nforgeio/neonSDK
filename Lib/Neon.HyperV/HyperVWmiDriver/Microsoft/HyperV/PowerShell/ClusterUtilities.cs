using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using Microsoft.HyperV.PowerShell.Common;
using Microsoft.Virtualization.Client.Management;
using Microsoft.Virtualization.Client.Management.Clustering;

using ErrorMessages = Microsoft.HyperV.PowerShell.Common.ErrorMessages;

namespace Microsoft.HyperV.PowerShell;

internal static class ClusterUtilities
{
    private const string gm_UpdateClusterVMConfigurationCmdletName = "FailoverClusters\\Update-ClusterVirtualMachineConfiguration";

    private const string gm_ClusterGroupTypeName = "Microsoft.FailoverClusters.PowerShell.ClusterGroup";

    private const string gm_ClusterResourceTypeName = "Microsoft.FailoverClusters.PowerShell.ClusterResource";

    private static CommandInfo GetUpdateVMClusterConfigurationCmdlet(CommandInvocationIntrinsics commandInvocationIntrinsics)
    {
        return Utilities.GetCmdletInfo("FailoverClusters\\Update-ClusterVirtualMachineConfiguration", commandInvocationIntrinsics);
    }

    internal static void UpdateClusterVMConfiguration(VirtualMachineBase vmOrSnapshot, CommandInvocationIntrinsics commandInvocationIntrinsics, IOperationWatcher operationWatcher)
    {
        try
        {
            CommandInfo updateVMClusterConfigurationCmdlet = GetUpdateVMClusterConfigurationCmdlet(commandInvocationIntrinsics);
            IMSClusterCluster clusterObject = ObjectLocator.GetClusterObject(vmOrSnapshot.Server);
            if (updateVMClusterConfigurationCmdlet == null)
            {
                operationWatcher.WriteWarning(string.Format(CultureInfo.CurrentCulture, ErrorMessages.ClusterPath_ConfigurationUpdateCmdletNotInstalledOrLoaded, vmOrSnapshot.GetVirtualMachine().Name, clusterObject.Name));
                return;
            }
            global::System.Management.Automation.PowerShell powerShell = global::System.Management.Automation.PowerShell.Create(RunspaceMode.CurrentRunspace);
            powerShell.AddCommand(updateVMClusterConfigurationCmdlet);
            powerShell.AddParameter("VMId", vmOrSnapshot.GetVirtualMachine().Id);
            powerShell.AddParameter("Cluster", clusterObject.Name);
            powerShell.AddParameter("ErrorAction", "Stop");
            powerShell.Invoke();
        }
        catch (Exception innerException)
        {
            throw ExceptionHelper.CreateInvalidArgumentException(ErrorMessages.ClusterPath_FailedToRefreshConfiguration, innerException);
        }
    }

    internal static void EnsureClusterPathValid(VirtualMachine virtualMachine, string path, bool allowUnverifiedPaths)
    {
        if (string.IsNullOrEmpty(path) || IsNetworkPath(path))
        {
            return;
        }
        IMSClusterCluster clusterObject = ObjectLocator.GetClusterObject(virtualMachine.Server);
        IMSClusterResourceGroup group = clusterObject.GetVirtualMachineResource(virtualMachine.Id.ToString()).GetGroup();
        ClusterVerifyPathResult clusterVerifyPathResult = clusterObject.VerifyPath(path, group.Name);
        if (clusterVerifyPathResult != 0 && clusterVerifyPathResult != ClusterVerifyPathResult.Network && !(clusterVerifyPathResult == ClusterVerifyPathResult.NonCluster && allowUnverifiedPaths))
        {
            string text = MapClusterVerifyPathResultToErrorString(clusterVerifyPathResult);
            if (!string.IsNullOrEmpty(text))
            {
                throw ExceptionHelper.CreateInvalidArgumentException(text);
            }
        }
    }

    private static bool IsNetworkPath(string path)
    {
        if (Uri.TryCreate(path, UriKind.Absolute, out var result) && result.IsUnc)
        {
            return true;
        }
        try
        {
            if (new DriveInfo(path).DriveType == DriveType.Network)
            {
                return true;
            }
        }
        catch
        {
        }
        return false;
    }

    private static string MapClusterVerifyPathResultToErrorString(ClusterVerifyPathResult result)
    {
        switch (result)
        {
        case ClusterVerifyPathResult.NotValid:
            return ErrorMessages.ClusterPath_NotValid;
        case ClusterVerifyPathResult.AvailableStorage:
            return ErrorMessages.ClusterPath_AvailableStorage;
        case ClusterVerifyPathResult.NonCluster:
            return ErrorMessages.ClusterPath_NonCluster;
        case ClusterVerifyPathResult.NotInGroup:
        case ClusterVerifyPathResult.OtherGroup:
            return ErrorMessages.ClusterPath_NotInGroup;
        default:
            throw new ArgumentOutOfRangeException("result", string.Format(CultureInfo.CurrentCulture, ErrorMessages.ArgumentOutOfRange_InvalidEnumValue, result.ToString(), typeof(ClusterVerifyPathResult).Name));
        }
    }

    internal static IEnumerable<VirtualMachine> GetVirtualMachinesFromClusterObject(PSObject inputObject, IOperationWatcher operationWatcher)
    {
        if (inputObject == null)
        {
            throw new ArgumentNullException("inputObject");
        }
        if (!((inputObject.Properties["Name"] ?? throw CreateClusterObjectInvalidException(inputObject)).Value is string text))
        {
            throw CreateClusterObjectInvalidException(inputObject);
        }
        Server server = ResolveServerFromClusterObject(inputObject);
        string fullName = inputObject.BaseObject.GetType().FullName;
        if (string.Equals(fullName, "Microsoft.FailoverClusters.PowerShell.ClusterGroup", StringComparison.Ordinal))
        {
            return GetVirtualMachinesByClusterGroup(server, text, operationWatcher);
        }
        if (string.Equals(fullName, "Microsoft.FailoverClusters.PowerShell.ClusterResource", StringComparison.Ordinal))
        {
            return new VirtualMachine[1] { GetVirtualMachineFromClusterResource(server, text) };
        }
        throw CreateClusterObjectInvalidException(inputObject);
    }

    private static VirtualMachine GetVirtualMachineFromClusterResource(Server server, string resourceName)
    {
        if (!(ObjectLocator.GetClusterResource(server, resourceName) is IMSClusterVMOrConfigurationResource iMSClusterVMOrConfigurationResource) || !Guid.TryParse(iMSClusterVMOrConfigurationResource.VMComputerSystemInstanceId, out var result))
        {
            throw CreateClusterObjectInvalidException(resourceName);
        }
        return VirtualizationObjectLocator.GetVirtualMachineById(server, result);
    }

    private static IEnumerable<VirtualMachine> GetVirtualMachinesByClusterGroup(Server server, string groupName, IOperationWatcher operationWatcher)
    {
        Guid[] ids = (from resource in ObjectLocator.GetClusterResourceGroup(server, groupName).GetResources().OfType<IMSClusterVMResource>()
            select Guid.Parse(resource.VMComputerSystemInstanceId)).ToArray();
        return VirtualizationObjectLocator.GetVirtualMachinesByIdsAndServers(new Server[1] { server }, ids, ErrorDisplayMode.WriteError, operationWatcher);
    }

    private static Server ResolveServerFromClusterObject(PSObject inputObject)
    {
        object inputObj = (inputObject.Properties["OwnerNode"] ?? throw CreateClusterObjectInvalidException(inputObject)).Value ?? throw CreateClusterObjectInvalidException(inputObject);
        string reflectedPropertyValue = GetReflectedPropertyValue<string>(inputObj, "Name");
        if (reflectedPropertyValue == null)
        {
            throw CreateClusterObjectInvalidException(inputObject);
        }
        string reflectedPropertyValue2 = GetReflectedPropertyValue<string>(GetReflectedPropertyValue<object>(inputObj, "Cluster") ?? throw CreateClusterObjectInvalidException(inputObject), "Domain");
        string computerName = reflectedPropertyValue;
        if (!string.IsNullOrEmpty(reflectedPropertyValue2))
        {
            computerName = string.Format(CultureInfo.InvariantCulture, "{0}.{1}", reflectedPropertyValue, reflectedPropertyValue2);
        }
        return VirtualizationObjectLocator.GetServer(computerName);
    }

    private static TType GetReflectedPropertyValue<TType>(object inputObj, string propertyName)
    {
        Type type = inputObj.GetType();
        PropertyInfo property = type.GetProperty(propertyName);
        if (property == null || !typeof(TType).IsAssignableFrom(property.PropertyType))
        {
            throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, ErrorMessages.InvalidParameter_NoPropertyFound, type.FullName, propertyName));
        }
        return (TType)property.GetValue(inputObj, null);
    }

    private static Exception CreateClusterObjectInvalidException(object inputObject)
    {
        return ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, ErrorMessages.InvalidParameter_InvalidClusterObjectType, inputObject));
    }
}
