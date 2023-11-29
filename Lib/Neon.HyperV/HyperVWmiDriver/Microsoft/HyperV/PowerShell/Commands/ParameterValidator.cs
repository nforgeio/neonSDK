using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.HyperV.PowerShell.ExtensionMethods;

namespace Microsoft.HyperV.PowerShell.Commands;

internal static class ParameterValidator
{
    internal static void ValidateServerParameters(IServerParameters cmdletParameters)
    {
        bool num = !cmdletParameters.CimSession.IsNullOrEmpty();
        bool flag = !cmdletParameters.ComputerName.IsNullOrEmpty();
        bool flag2 = !cmdletParameters.Credential.IsNullOrEmpty();
        if (num && flag)
        {
            throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.InvariantCulture, CmdletErrorMessages.InvalidParameter_ParametersAreMutuallyExclusive, "CimSession", "ComputerName"));
        }
        if (num && flag2)
        {
            throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.InvariantCulture, CmdletErrorMessages.InvalidParameter_ParametersAreMutuallyExclusive, "CimSession", "Credential"));
        }
        if (flag2 && !flag)
        {
            throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.InvariantCulture, CmdletErrorMessages.InvalidParameter_ParameterRequiresParameter, "CimCredential", "ComputerName"));
        }
        if (flag && flag2 && cmdletParameters.Credential.Length != 1 && cmdletParameters.Credential.Length != cmdletParameters.ComputerName.Length)
        {
            throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.InvalidParameter_MismatchedCredentials);
        }
    }

    internal static void ValidateDestinationServerParameters(IDestinationServerParameters cmdletParameters)
    {
        bool num = cmdletParameters.DestinationCimSession != null;
        bool flag = cmdletParameters.DestinationHost != null;
        bool flag2 = cmdletParameters.DestinationCredential != null;
        if (num && flag)
        {
            throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.InvariantCulture, CmdletErrorMessages.InvalidParameter_ParametersAreMutuallyExclusive, "DestinationCimSession", "DestinationHost"));
        }
        if (num && flag2)
        {
            throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.InvariantCulture, CmdletErrorMessages.InvalidParameter_ParametersAreMutuallyExclusive, "DestinationCimSession", "DestinationCredential"));
        }
        if (!num && !flag)
        {
            throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.InvariantCulture, CmdletErrorMessages.InvalidParameter_OneOfMutuallyExclusiveParametersRequired, "DestinationCimSession", "DestinationHost"));
        }
        if (flag2 && !flag)
        {
            throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.InvariantCulture, CmdletErrorMessages.InvalidParameter_ParameterRequiresParameter, "DestinationCredential", "DestinationHost"));
        }
    }

    internal static void ValidateHbaParameters(IVMSanCmdlet sanCmdlet)
    {
        if (!sanCmdlet.CurrentParameterSetIs("HbaObject"))
        {
            if (sanCmdlet.WorldWideNodeName.Length != sanCmdlet.WorldWidePortName.Length)
            {
                throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.NewVMSan_NamesMismatched);
            }
            if (!sanCmdlet.WorldWideNodeName.All(VMSan.IsValidWwn) || !sanCmdlet.WorldWidePortName.All(VMSan.IsValidWwn))
            {
                throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.NewVMSan_WorldWideNameFormatError);
            }
        }
    }

    internal static void ValidateMoveOrCompareVMParameters(IMoveOrCompareVMCmdlet moveOrCompareVmCmdlet, bool includeStorage)
    {
        bool flag = true;
        bool flag2 = true;
        if (moveOrCompareVmCmdlet.CurrentParameterSetIs("VMSingleDestination") || moveOrCompareVmCmdlet.CurrentParameterSetIs("VMSingleDestinationAndCimSession"))
        {
            flag2 = false;
        }
        else if (moveOrCompareVmCmdlet.CurrentParameterSetIs("NameMultipleDestinations") | moveOrCompareVmCmdlet.CurrentParameterSetIs("NameMultipleDestinationsAndCimSession"))
        {
            flag = false;
        }
        else if (moveOrCompareVmCmdlet.CurrentParameterSetIs("VMMultipleDestinations") || moveOrCompareVmCmdlet.CurrentParameterSetIs("VMMultipleDestinationsAndCimSession"))
        {
            flag = false;
            flag2 = false;
        }
        if (!includeStorage && string.IsNullOrEmpty(moveOrCompareVmCmdlet.DestinationStoragePath))
        {
            return;
        }
        if (flag)
        {
            if (!string.IsNullOrEmpty(moveOrCompareVmCmdlet.DestinationStoragePath) && !PathUtility.IsValidPath(moveOrCompareVmCmdlet.DestinationStoragePath))
            {
                throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.InvalidParameter_InvalidDirectoryPath, moveOrCompareVmCmdlet.DestinationStoragePath));
            }
            return;
        }
        if (!string.IsNullOrEmpty(moveOrCompareVmCmdlet.VirtualMachinePath) && !PathUtility.IsValidPath(moveOrCompareVmCmdlet.VirtualMachinePath))
        {
            throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.InvalidParameter_InvalidDirectoryPath, moveOrCompareVmCmdlet.VirtualMachinePath));
        }
        if (!string.IsNullOrEmpty(moveOrCompareVmCmdlet.SnapshotFilePath) && !PathUtility.IsValidPath(moveOrCompareVmCmdlet.SnapshotFilePath))
        {
            throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.InvalidParameter_InvalidDirectoryPath, moveOrCompareVmCmdlet.SnapshotFilePath));
        }
        if (!string.IsNullOrEmpty(moveOrCompareVmCmdlet.SmartPagingFilePath) && !PathUtility.IsValidPath(moveOrCompareVmCmdlet.SmartPagingFilePath))
        {
            throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.InvalidParameter_InvalidDirectoryPath, moveOrCompareVmCmdlet.SmartPagingFilePath));
        }
        if (!moveOrCompareVmCmdlet.Vhds.IsNullOrEmpty())
        {
            ValidateVhdMigrationMappings(moveOrCompareVmCmdlet.VhdMigrationMappings);
        }
    }

    internal static void ValidateImportOrCompareVMParameters(IImportOrCompareVMCmdlet importOrCompareVmCmdlet)
    {
        importOrCompareVmCmdlet.CurrentParameterSetIs("Copy");
    }

    internal static void ValidateVhdMigrationMappings(IEnumerable<VhdMigrationMapping> vhdMigrationMappings)
    {
        foreach (VhdMigrationMapping vhdMigrationMapping in vhdMigrationMappings)
        {
            if (string.IsNullOrEmpty(vhdMigrationMapping.SourcePath) || !PathUtility.IsValidFilePath(vhdMigrationMapping.SourcePath))
            {
                throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.InvalidParameter_InvalidFullPath, vhdMigrationMapping.SourcePath));
            }
            if (string.IsNullOrEmpty(vhdMigrationMapping.DestinationPath) || !PathUtility.IsValidFilePath(vhdMigrationMapping.DestinationPath))
            {
                throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.InvalidParameter_InvalidFullPath, vhdMigrationMapping.DestinationPath));
            }
            if (!string.Equals(Path.GetFileName(vhdMigrationMapping.SourcePath), Path.GetFileName(vhdMigrationMapping.DestinationPath), StringComparison.OrdinalIgnoreCase))
            {
                throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.InvalidParameter_SourceAndDestinationVhdNameMismatch);
            }
        }
    }

    internal static VirtualizationException CreateInvalidParameterFormatException(string parameterName)
    {
        return ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.InvalidParameter_InvalidFormat, parameterName));
    }
}
