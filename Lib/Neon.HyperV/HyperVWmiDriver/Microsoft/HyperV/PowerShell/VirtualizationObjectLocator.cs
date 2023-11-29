using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Common;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Management.Infrastructure;
using Microsoft.Virtualization.Client.Common;
using Microsoft.Virtualization.Client.Management;

using ErrorMessages = Microsoft.HyperV.PowerShell.Common.ErrorMessages;

namespace Microsoft.HyperV.PowerShell;

internal static class VirtualizationObjectLocator
{
    internal static Server GetServer(CimSession cimSession)
    {
        Server server = Server.GetServer(cimSession);
        PrepareServerConnections(server);
        return server;
    }

    internal static Server GetServer(string computerName)
    {
        return GetServer(computerName, (IUserPassCredential)null);
    }

    internal static Server GetServer(string computerName, IUserPassCredential credential)
    {
        Server server = Server.GetServer(computerName, credential);
        PrepareServerConnections(server);
        return server;
    }

    internal static Server GetServer(string computerName, PSCredential psCredential)
    {
        return GetServer(computerName, CreateUserPassCredential(psCredential));
    }

    internal static Server GetDestinationServer(CimSession cimSession)
    {
        Server server = Server.GetServer(cimSession);
        PrepareServerConnections(server, allowDownlevelServers: true);
        return server;
    }

    internal static Server GetDestinationServer(string computerName, PSCredential psCredential)
    {
        IUserPassCredential credential = CreateUserPassCredential(psCredential);
        Server server = Server.GetServer(computerName, credential);
        PrepareServerConnections(server, allowDownlevelServers: true);
        return server;
    }

    private static IUserPassCredential CreateUserPassCredential(PSCredential psCredential)
    {
        if (psCredential == null)
        {
            return null;
        }
        return UserPassCredential.Create(psCredential.UserName, psCredential.Password);
    }

    private static void PrepareServerConnections(Server server, bool allowDownlevelServers = false)
    {
        if (!allowDownlevelServers && server.OSVersion < HyperVOSVersion.WindowsThreshold)
        {
            throw ExceptionHelper.CreateInvalidOperationException(string.Format(CultureInfo.CurrentCulture, ErrorMessages.InvalidOperation_DownlevelManagementNotSupported, server), null, null);
        }
    }

    internal static VirtualMachine GetVirtualMachineById(Server server, Guid id)
    {
        if (!TryGetVirtualMachineById(server, id, out var virtualMachine))
        {
            throw ExceptionHelper.CreateObjectNotFoundException(string.Format(CultureInfo.CurrentCulture, ErrorMessages.VMNotFoundById, id), null);
        }
        return virtualMachine;
    }

    internal static bool TryGetVirtualMachineById(Server server, Guid id, out VirtualMachine virtualMachine)
    {
        IVMComputerSystemBase computerSystemBase = null;
        try
        {
            string instanceId = id.ToString();
            if (!ObjectLocator.TryGetVMComputerSystemBase(server, instanceId, out computerSystemBase))
            {
                virtualMachine = null;
                return false;
            }
            if (server.IsHostCluster)
            {
                ISummaryInformation vMSummaryInformation = ((IVMComputerSystem)computerSystemBase).GetVMSummaryInformation(SummaryInformationRequest.NameAndHost);
                computerSystemBase = ObjectLocator.GetVMComputerSystemBase(GetServer(vMSummaryInformation.HostComputerSystemName, vMSummaryInformation.Server.SimpleCredential), instanceId);
            }
            virtualMachine = new VirtualMachine(computerSystemBase);
            return true;
        }
        catch (ServerObjectDeletedException)
        {
            computerSystemBase?.RemoveFromCache();
            virtualMachine = null;
            return false;
        }
        catch (VirtualizationManagementException exception)
        {
            throw ExceptionHelper.ConvertToVirtualizationException(exception, null);
        }
    }

    internal static IList<VirtualMachine> GetVirtualMachinesByNamesAndServers(IEnumerable<Server> servers, IList<string> vmNames, bool allowWildcards, ErrorDisplayMode errorDisplayMode, IOperationWatcher operationWatcher)
    {
        try
        {
            WildcardPatternMatcher matcher = null;
            if (vmNames != null && vmNames.Count > 0 && allowWildcards && vmNames.Any(WildcardPattern.ContainsWildcardCharacters))
            {
                matcher = new WildcardPatternMatcher(vmNames);
            }
            IList<VirtualMachine> virtualMachinesByNamesAndServers = GetVirtualMachinesByNamesAndServers(servers, vmNames, matcher, operationWatcher);
            if (vmNames != null && vmNames.Count > 0)
            {
                WriteNonMatchingNameErrors(vmNames, virtualMachinesByNamesAndServers.Select((VirtualMachine vm) => vm.Name), allowWildcards: true, ErrorMessages.VMNotFoundByName, errorDisplayMode, operationWatcher);
            }
            return virtualMachinesByNamesAndServers;
        }
        catch (VirtualizationManagementException exception)
        {
            throw ExceptionHelper.ConvertToVirtualizationException(exception, null);
        }
    }

    private static IList<VirtualMachine> GetVirtualMachinesByNamesAndServers(IEnumerable<Server> servers, IList<string> vmNames, WildcardPatternMatcher matcher, IOperationWatcher operationWatcher)
    {
        List<VirtualMachine> list = new List<VirtualMachine>();
        foreach (Server server in servers)
        {
            Server currentServer = server;
            IEnumerable<ISummaryInformation> allSummaryInformation = ObjectLocator.GetHostOrClusterComputerSystem(server).GetAllSummaryInformation(SummaryInformationRequest.NameAndHost);
            allSummaryInformation = ((vmNames == null || vmNames.Count <= 0) ? allSummaryInformation.OrderBy((ISummaryInformation summary) => summary.ElementName, StringComparer.CurrentCultureIgnoreCase) : ((matcher == null) ? allSummaryInformation.Where((ISummaryInformation vm) => vmNames.Contains(vm.ElementName, StringComparer.OrdinalIgnoreCase)) : allSummaryInformation.Where((ISummaryInformation vm) => matcher.MatchesAny(vm.ElementName))));
            IEnumerable<VirtualMachine> collection = allSummaryInformation.SelectWithLogging((ISummaryInformation summary) => new VirtualMachine(ObjectLocator.GetVMComputerSystem((!currentServer.IsHostCluster) ? currentServer : GetServer(summary.HostComputerSystemName, currentServer.SimpleCredential), summary.Name)), operationWatcher);
            list.AddRange(collection);
        }
        return list;
    }

    internal static IList<VirtualMachine> GetVirtualMachinesByIdsAndServers(IEnumerable<Server> servers, IList<Guid> ids, ErrorDisplayMode errorDisplayMode, IOperationWatcher operationWatcher)
    {
        try
        {
            List<VirtualMachine> list = new List<VirtualMachine>();
            foreach (IHostComputerSystemBase item in servers.Select(ObjectLocator.GetHostOrClusterComputerSystem))
            {
                list.AddRange(from vm in ObjectLocator.GetVMComputerSystems(item.Server, ids)
                    select new VirtualMachine(vm));
            }
            WriteNonMatchingNameErrors(ids.Select((Guid id) => id.ToString()), list.Select((VirtualMachine vm) => vm.Id.ToString()), allowWildcards: true, ErrorMessages.VMNotFoundById, errorDisplayMode, operationWatcher);
            return list;
        }
        catch (VirtualizationManagementException exception)
        {
            throw ExceptionHelper.ConvertToVirtualizationException(exception, null);
        }
    }

    internal static VMSnapshot GetVMSnapshotById(Server server, Guid id)
    {
        try
        {
            IVMComputerSystemSetting vMComputerSystemSetting = ObjectLocator.GetVMComputerSystemSetting(server, string.Format(CultureInfo.InvariantCulture, "Microsoft:{0}", id));
            if (!vMComputerSystemSetting.IsSnapshot)
            {
                throw ExceptionHelper.CreateObjectNotFoundException(ErrorMessages.SnapshotNotFound, null);
            }
            VirtualMachine vm = new VirtualMachine(vMComputerSystemSetting.VMComputerSystem);
            return new VMSnapshot(vMComputerSystemSetting, vm);
        }
        catch (VirtualizationManagementException exception)
        {
            throw ExceptionHelper.ConvertToVirtualizationException(exception, null);
        }
    }

    private static VMHostCluster GetVMHostCluster(Server server)
    {
        try
        {
            return new VMHostCluster(ObjectLocator.GetClusterWmiProviderResource(server));
        }
        catch (VirtualizationManagementException exception)
        {
            throw ExceptionHelper.ConvertToVirtualizationException(exception, null);
        }
    }

    internal static IList<VMHostCluster> GetVMHostClusters(IEnumerable<Server> servers, IOperationWatcher operationWatcher)
    {
        return servers.SelectWithLogging(GetVMHostCluster, operationWatcher).ToList();
    }

    internal static VMHost GetVMHost(Server server)
    {
        try
        {
            return new VMHost(ObjectLocator.GetHostComputerSystem(server));
        }
        catch (VirtualizationManagementException exception)
        {
            throw ExceptionHelper.ConvertToVirtualizationException(exception, null);
        }
    }

    internal static IList<VMHost> GetVMHosts(IEnumerable<Server> servers, IOperationWatcher operationWatcher)
    {
        return servers.SelectWithLogging(GetVMHost, operationWatcher).ToList();
    }

    internal static void WriteNonMatchingNameErrors(IEnumerable<string> requestedNames, IEnumerable<string> actualNames, bool allowWildcards, string errorMessageFormat, ErrorDisplayMode errorDisplayMode, IOperationWatcher operationWatcher)
    {
        if (errorDisplayMode == ErrorDisplayMode.None)
        {
            return;
        }
        foreach (string item in (allowWildcards ? requestedNames.Where((string name) => !WildcardPattern.ContainsWildcardCharacters(name)) : requestedNames).Except(actualNames, StringComparer.OrdinalIgnoreCase))
        {
            string message = string.Format(CultureInfo.CurrentCulture, errorMessageFormat, item);
            if (errorDisplayMode == ErrorDisplayMode.WriteError)
            {
                VirtualizationException exception = ExceptionHelper.CreateObjectNotFoundException(message, null);
                operationWatcher.WriteError(ExceptionHelper.GetErrorRecordFromException(exception));
            }
            else
            {
                VirtualizationException ex = ExceptionHelper.CreateInvalidArgumentException(message);
                ErrorRecord record = new ErrorRecord(ex, ex.ErrorIdentifier, ex.ErrorCategory, item);
                operationWatcher.WriteError(record);
            }
        }
    }
}
