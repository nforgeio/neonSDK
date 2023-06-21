using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell;
using Microsoft.HyperV.PowerShell.Commands;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.Vhd.PowerShell.Cmdlets;

[Cmdlet("Merge", "VHD", ConfirmImpact = ConfirmImpact.Medium, SupportsShouldProcess = true)]
[OutputType(new Type[] { typeof(VirtualHardDisk) })]
internal sealed class MergeVhd : VirtualizationCmdlet<Tuple<Server, string>>, ISupportsAsJob, ISupportsForce, ISupportsPassthrough
{
    [ValidateNotNullOrEmpty]
    [Alias(new string[] { "FullName" })]
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
    public string Path { get; set; }

    [ValidateNotNullOrEmpty]
    [Parameter(Position = 1)]
    public string DestinationPath { get; set; }

    [Parameter]
    public SwitchParameter Force { get; set; }

    [Parameter]
    public SwitchParameter AsJob { get; set; }

    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
    [Parameter]
    public SwitchParameter Passthru { get; set; }

    internal override IList<Tuple<Server, string>> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        List<Tuple<Server, string>> list = new List<Tuple<Server, string>>();
        foreach (Server server in ParameterResolvers.GetServers(this, operationWatcher))
        {
            try
            {
                string singleVirtualHardDiskFullPath = VhdPathResolver.GetSingleVirtualHardDiskFullPath(server, Path, base.CurrentFileSystemLocation, base.InvokeProvider);
                list.Add(Tuple.Create(server, singleVirtualHardDiskFullPath));
            }
            catch (Exception e)
            {
                ExceptionHelper.DisplayErrorOnException(e, operationWatcher);
            }
        }
        return list;
    }

    internal override void ProcessOneOperand(Tuple<Server, string> operand, IOperationWatcher operationWatcher)
    {
        Server item = operand.Item1;
        string item2 = operand.Item2;
        string text;
        if (IsParameterSpecified("DestinationPath"))
        {
            text = VhdPathResolver.GetSingleVirtualHardDiskFullPath(item, DestinationPath, base.CurrentFileSystemLocation, base.InvokeProvider);
        }
        else
        {
            VirtualHardDisk virtualHardDisk = VhdUtilities.GetVirtualHardDisk(item, item2);
            if (string.IsNullOrEmpty(virtualHardDisk.ParentPath))
            {
                throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.VHD_CannotMergeFixedOrDynamicDisk, item2));
            }
            text = virtualHardDisk.ParentPath;
        }
        if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_MergeVHD, item2, text)) && (IsParameterSpecified("DestinationPath") || operationWatcher.ShouldContinue(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldContinue_MergeToImmediateParent, item2))))
        {
            VhdUtilities.MergeVirtualHardDisk(item, item2, text, operationWatcher);
            if (Passthru.IsPresent)
            {
                VirtualHardDisk virtualHardDisk2 = VhdUtilities.GetVirtualHardDisk(item, text);
                operationWatcher.WriteObject(virtualHardDisk2);
            }
        }
    }
}
