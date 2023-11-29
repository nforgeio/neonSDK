using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell;
using Microsoft.HyperV.PowerShell.Commands;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.Vhd.PowerShell.Cmdlets;

[SuppressMessage("Microsoft.Interoperability", "CA1405:ComVisibleTypeBaseTypesShouldBeComVisible")]
[Cmdlet("Test", "VHD", ConfirmImpact = ConfirmImpact.Medium, DefaultParameterSetName = "ExistingVHD")]
[OutputType(new Type[] { typeof(bool) })]
internal sealed class TestVhd : VirtualizationCmdlet<Tuple<Server, string>>
{
    private static class ParameterSetNames
    {
        public const string ExistingVhd = "ExistingVHD";

        public const string SharedDisk = "SharedDisk";
    }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec. Also array is more user friendly.")]
    [ValidateNotNullOrEmpty]
    [Alias(new string[] { "FullName" })]
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
    public string[] Path { get; set; }

    [Parameter(Mandatory = true, ParameterSetName = "SharedDisk")]
    [Alias(new string[] { "ShareVirtualDisk" })]
    public SwitchParameter SupportPersistentReservations { get; set; }

    protected override void ValidateParameters()
    {
        base.ValidateParameters();
        if (CurrentParameterSetIs("SharedDisk") && !SupportPersistentReservations)
        {
            throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.VHD_SupportPRSwitchMustBeTrue));
        }
    }

    internal override IList<Tuple<Server, string>> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        List<Tuple<Server, string>> list = new List<Tuple<Server, string>>();
        foreach (Server server in ParameterResolvers.GetServers(this, operationWatcher))
        {
            string[] path = Path;
            foreach (string path2 in path)
            {
                IEnumerable<string> enumerable;
                try
                {
                    enumerable = ((!CurrentParameterSetIs("ExistingVHD")) ? VhdPathResolver.GetVHDOrDirectoryFullPath(server, path2, base.CurrentFileSystemLocation, base.InvokeProvider) : VhdPathResolver.GetVirtualHardDiskFullPath(server, path2, base.CurrentFileSystemLocation, base.InvokeProvider));
                }
                catch (Exception e)
                {
                    ExceptionHelper.DisplayErrorOnException(e, operationWatcher);
                    operationWatcher.WriteObject(false);
                    enumerable = null;
                }
                if (enumerable != null)
                {
                    list.AddRange(enumerable.Select((string expandedPath) => Tuple.Create(server, expandedPath)));
                }
            }
        }
        return list;
    }

    internal override void ProcessOneOperand(Tuple<Server, string> operand, IOperationWatcher operationWatcher)
    {
        Server item = operand.Item1;
        string item2 = operand.Item2;
        bool flag;
        try
        {
            if (CurrentParameterSetIs("ExistingVHD"))
            {
                if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_TestVHD, item2)))
                {
                    VhdUtilities.ValidateVirtualHardDisk(item, item2, operationWatcher);
                }
            }
            else if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_TestSharedVHD, item2)))
            {
                VhdUtilities.ValidateSharedVirtualHardDisk(item, item2, operationWatcher);
            }
            flag = true;
        }
        catch (Exception e)
        {
            ExceptionHelper.DisplayErrorOnException(e, operationWatcher);
            flag = false;
        }
        operationWatcher.WriteObject(flag);
    }
}
