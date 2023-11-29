using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell;
using Microsoft.HyperV.PowerShell.Commands;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.Vhd.PowerShell.Cmdlets;

[Cmdlet("Convert", "VHD", ConfirmImpact = ConfirmImpact.Medium, SupportsShouldProcess = true)]
[OutputType(new Type[] { typeof(VirtualHardDisk) })]
internal sealed class ConvertVhd : VirtualizationCmdlet<Tuple<Server, string>>, ISupportsAsJob, ISupportsPassthrough
{
    [ValidateNotNullOrEmpty]
    [Alias(new string[] { "FullName" })]
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
    public string Path { get; set; }

    [ValidateNotNullOrEmpty]
    [Parameter(Mandatory = true, Position = 1)]
    public string DestinationPath { get; set; }

    [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "VHD", Justification = "This is per spec.")]
    [ValidateNotNullOrEmpty]
    [Parameter]
    public VhdType VHDType { get; set; }

    [ValidateNotNullOrEmpty]
    [Parameter]
    public string ParentPath { get; set; }

    [ValidateNotNullOrEmpty]
    [Parameter]
    public uint BlockSizeBytes { get; set; }

    [Parameter]
    public SwitchParameter DeleteSource { get; set; }

    [Parameter]
    public SwitchParameter AsJob { get; set; }

    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
    [Parameter]
    public SwitchParameter Passthru { get; set; }

    [Parameter]
    public VirtualHardDiskPmemAddressAbstractionType AddressAbstractionType { get; set; }

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
        string text = null;
        Server item = operand.Item1;
        string item2 = operand.Item2;
        string fullPath = PathUtility.GetFullPath(DestinationPath, base.CurrentFileSystemLocation);
        bool isPmemCompatible = false;
        if (!string.IsNullOrEmpty(ParentPath))
        {
            text = VhdPathResolver.GetSingleVirtualHardDiskFullPath(item, ParentPath, base.CurrentFileSystemLocation, base.InvokeProvider);
        }
        VhdType vhdType = ((VHDType != 0) ? VHDType : VhdUtilities.GetVirtualHardDisk(item, item2).VhdType);
        if (!string.IsNullOrEmpty(text) && vhdType != VhdType.Differencing)
        {
            throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.ConvertVHD_NonDifferencingDiskCannotHaveParentPath);
        }
        if (string.IsNullOrEmpty(text) && vhdType == VhdType.Differencing)
        {
            throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.ConvertVHD_DifferencingDiskMustHaveParentPath);
        }
        string extension = global::System.IO.Path.GetExtension(fullPath);
        VhdFormat destinationFormat;
        if (string.Equals(extension, ".VHD", StringComparison.OrdinalIgnoreCase) || string.Equals(extension, ".AVHD", StringComparison.OrdinalIgnoreCase))
        {
            destinationFormat = VhdFormat.VHD;
        }
        else if (string.Equals(extension, ".VHDS", StringComparison.OrdinalIgnoreCase))
        {
            destinationFormat = VhdFormat.VHDSet;
        }
        else if (string.Equals(extension, ".VHDPMEM", StringComparison.OrdinalIgnoreCase))
        {
            destinationFormat = VhdFormat.VHDX;
            isPmemCompatible = true;
        }
        else
        {
            destinationFormat = VhdFormat.VHDX;
        }
        if (!operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_ConvertVHD, item2)))
        {
            return;
        }
        VhdUtilities.ConvertVirtualHardDisk(item, item2, fullPath, text, vhdType, destinationFormat, BlockSizeBytes, isPmemCompatible, AddressAbstractionType, operationWatcher);
        if (Passthru.IsPresent)
        {
            VirtualHardDisk virtualHardDisk = VhdUtilities.GetVirtualHardDisk(item, fullPath, null);
            operationWatcher.WriteObject(virtualHardDisk);
        }
        if (!DeleteSource.IsPresent)
        {
            return;
        }
        try
        {
            if (!item2.StartsWith("\\\\", StringComparison.OrdinalIgnoreCase))
            {
                Utilities.DeleteDataFiles(item, item2);
            }
            else
            {
                File.Delete(item2);
            }
        }
        catch (Exception ex)
        {
            string message = CmdletErrorMessages.Warning_CannotDeleteSourceDisk + ex.Message;
            operationWatcher.WriteWarning(message);
        }
    }
}
