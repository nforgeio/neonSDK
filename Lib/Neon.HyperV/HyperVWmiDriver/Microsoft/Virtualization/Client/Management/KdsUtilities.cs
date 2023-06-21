#define TRACE
using System;
using System.Globalization;
using System.Linq;
using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Options;

namespace Microsoft.Virtualization.Client.Management;

internal static class KdsUtilities
{
    private class KdsUtilitiesErrorCodeMapper : ErrorCodeMapper
    {
        public override string MapError(VirtualizationOperation operation, long errorCode, string operationFailedMsg)
        {
            return ErrorMessages.SetVMKeyProtectorFailed;
        }
    }

    private const string gm_KdsNamespace = "root\\microsoft\\windows\\hgs";

    private const string gm_KdsGuardianName = "MSFT_HgsGuardian";

    private const string gm_KdsKeyProtectorName = "MSFT_HgsKeyProtector";

    private const string gm_DefaultGuardianName = "UntrustedGuardian";

    private const ushort gm_UnknownMode = 0;

    private const ushort gm_LocalMode = 1;

    private static CimMethodResult InvokeCimMethod(CimSession session, string methodNamespace, string className, string methodName, CimMethodParametersCollection parameters)
    {
        CimException invokeError = null;
        CimOperationOptions cimOperationOptions = new CimOperationOptions();
        cimOperationOptions.WriteError = delegate(CimInstance errorInstance)
        {
            if (errorInstance != null)
            {
                invokeError = new CimException(errorInstance);
            }
            return CimResponseType.YesToAll;
        };
        cimOperationOptions.WriteMessage = delegate(uint channel, string message)
        {
            VMTrace.TraceWarning(string.Format(CultureInfo.InvariantCulture, "Non - terminating error received when invoking CIM method '{0}'.", methodName), message);
        };
        try
        {
            return session.InvokeMethod(methodNamespace, className, methodName, parameters, cimOperationOptions);
        }
        catch (CimException ex)
        {
            throw ThrowHelper.CreateVirtualizationOperationFailedException(null, ex.Message, VirtualizationOperation.SetKeyProtector, ex.HResult, operationCanceled: false, new KdsUtilitiesErrorCodeMapper(), ex);
        }
        finally
        {
            if (invokeError != null)
            {
                throw ThrowHelper.CreateVirtualizationOperationFailedException(null, invokeError.Message, VirtualizationOperation.SetKeyProtector, invokeError.HResult, operationCanceled: false, new KdsUtilitiesErrorCodeMapper(), invokeError);
            }
        }
    }

    private static CimInstance GetDefaultGuardian(CimSession session)
    {
        CimInstance cimInstance = (from g in session.EnumerateInstances("root\\microsoft\\windows\\hgs", "MSFT_HgsGuardian")
            where g.CimInstanceProperties["Name"].Value as string == "UntrustedGuardian"
            select g).FirstOrDefault();
        if (cimInstance == null)
        {
            using (CimMethodParametersCollection cimMethodParametersCollection = new CimMethodParametersCollection())
            {
                cimMethodParametersCollection.Add(CimMethodParameter.Create("Name", "UntrustedGuardian", CimType.String, CimFlags.None));
                cimMethodParametersCollection.Add(CimMethodParameter.Create("GenerateCertificates", true, CimFlags.None));
                return InvokeCimMethod(session, "root\\microsoft\\windows\\hgs", "MSFT_HgsGuardian", "NewByGenerateCertificates", cimMethodParametersCollection).OutParameters["cmdletOutput"].Value as CimInstance;
            }
        }
        return cimInstance;
    }

    public static bool IsDataProtectionPropertyUsedToEncrypt(Version vmVersion)
    {
        return vmVersion < VMConfigurationVersion.WinServer2016_TP5;
    }

    public static bool IsKeyStorageDriveAvailable(Version vmVersion)
    {
        return vmVersion >= VMConfigurationVersion.Redstone_1;
    }

    public static bool IsLocalModeSupported(Server server)
    {
        try
        {
            using CimInstance cimInstance = server.Session.InvokeMethod("root\\microsoft\\windows\\hgs", "MSFT_HgsClientConfiguration", "Get", null).OutParameters["cmdletOutput"].Value as CimInstance;
            return ((cimInstance.CimInstanceProperties["Mode"].Value as ushort?) ?? 0) == 1;
        }
        catch (NullReferenceException)
        {
            return false;
        }
    }

    public static byte[] NewLocalKeyProtector(Server server)
    {
        if (!IsLocalModeSupported(server))
        {
            throw new InvalidOperationException(ErrorMessages.KDSUtilities_NewLocalKPNotSupportedInSHSMode);
        }
        CimSession session = server.Session.Session;
        CimInstance defaultGuardian = GetDefaultGuardian(session);
        using CimMethodParametersCollection cimMethodParametersCollection = new CimMethodParametersCollection();
        cimMethodParametersCollection.Add(CimMethodParameter.Create("Owner", defaultGuardian, CimFlags.None));
        cimMethodParametersCollection.Add(CimMethodParameter.Create("AllowUntrustedRoot", true, CimFlags.None));
        using CimMethodResult cimMethodResult = InvokeCimMethod(session, "root\\microsoft\\windows\\hgs", "MSFT_HgsKeyProtector", "NewByGuardians", cimMethodParametersCollection);
        using CimInstance cimInstance = (CimInstance)cimMethodResult.OutParameters["cmdletOutput"].Value;
        return cimInstance.CimInstanceProperties["RawData"].Value as byte[];
    }
}
