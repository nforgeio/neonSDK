#define TRACE
using System;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.Management.Infrastructure;

namespace Microsoft.Virtualization.Client.Management;

internal static class ThrowHelper
{
	private enum FilterFailureReason
	{
		CallFailed,
		ConnectionIssue,
		ServerObjectDeleted,
		VirtualizationManagementException
	}

	public static VirtualizationOperationFailedException CreateVirtualizationOperationFailedException(string errorMsg, string errorDescriptionMsg, VirtualizationOperation operation, long errorCode, bool operationCanceled, ErrorCodeMapper mapper, Exception innerException)
	{
		if (mapper == null)
		{
			mapper = new ErrorCodeMapper();
		}
		errorMsg = mapper.MapError(operation, errorCode, errorMsg);
		VirtualizationOperationFailedException ex = new VirtualizationOperationFailedException(errorMsg, innerException);
		ex.Description = errorDescriptionMsg;
		ex.Operation = operation;
		ex.ErrorCode = errorCode;
		ex.Canceled = operationCanceled;
		if (operationCanceled)
		{
			VMTrace.TraceWarning(string.Format(CultureInfo.InvariantCulture, "Operation '{0}' was canceled.", operation));
		}
		else
		{
			VMTrace.TraceError(string.Format(CultureInfo.InvariantCulture, "Operation '{0}' failed with message '{1}'.", operation, errorMsg), string.Format(CultureInfo.InvariantCulture, "Error code '{0}'.", errorCode), string.Format(CultureInfo.InvariantCulture, "Detailed message '{0}'.", errorDescriptionMsg));
		}
		return ex;
	}

	public static VirtualizationOperationFailedException CreateVirtualizationOperationFailedException(string errorMsg, VirtualizationOperation operation, long errorCode, ErrorCodeMapper mapper, Exception innerException)
	{
		return CreateVirtualizationOperationFailedException(errorMsg, null, operation, errorCode, operationCanceled: false, mapper, innerException);
	}

	public static VirtualizationOperationFailedException CreateVirtualizationOperationFailedException(string errorMsg, VirtualizationOperation operation, long errorCode)
	{
		return CreateVirtualizationOperationFailedException(errorMsg, null, operation, errorCode, operationCanceled: false, null, null);
	}

	public static ValidateVirtualHardDiskException CreateValidateVirtualHardDiskException(string errorMsg, string errorDescriptionMsg, long errorCode, string parentPath, string childPath, ErrorCodeMapper errorCodeMapper, Exception innerException)
	{
		if (errorCodeMapper == null)
		{
			errorCodeMapper = new ErrorCodeMapper();
		}
		errorMsg = errorCodeMapper.MapError(VirtualizationOperation.ValidateVirtualHardDisk, errorCode, errorMsg);
		return new ValidateVirtualHardDiskException(errorMsg, innerException)
		{
			Description = errorDescriptionMsg,
			Operation = VirtualizationOperation.ValidateVirtualHardDisk,
			ErrorCode = errorCode,
			ParentPath = parentPath,
			ChildPath = childPath
		};
	}

	public static ObjectNotFoundException CreateRelatedObjectNotFoundException(Server server, Type objectTypeNotFound, string nameSearchedFor = null, Exception innerException = null)
	{
		ObjectNotFoundException result = new ObjectNotFoundException(string.Format(CultureInfo.CurrentCulture, ErrorMessages.RemoteObjectNotFound, server), innerException);
		VMTrace.TraceError(string.Format(CultureInfo.InvariantCulture, "Unable to find related object of type '{0}' with name '{1}'.", objectTypeNotFound.Name, nameSearchedFor ?? string.Empty));
		return result;
	}

	public static ObjectNotFoundException CreateRemoteObjectNotFoundException(Server server, ObjectKey keyNotFound, Exception inner)
	{
		ObjectNotFoundException result = new ObjectNotFoundException(string.Format(CultureInfo.CurrentCulture, ErrorMessages.RemoteObjectNotFound, server), inner);
		VMTrace.TraceError(string.Format(CultureInfo.InvariantCulture, "Unable to get object with path '{0}'.", keyNotFound.ManagementPath));
		return result;
	}

	public static ObjectNotFoundException CreateRemoteObjectNotFoundException(Server server, WmiObjectPath path, Exception inner)
	{
		ObjectNotFoundException result = new ObjectNotFoundException(string.Format(CultureInfo.CurrentCulture, ErrorMessages.RemoteObjectNotFound, server), inner);
		VMTrace.TraceError(string.Format(CultureInfo.InvariantCulture, "Unable to get object with path '{0}'.", path.ToString()));
		return result;
	}

	public static ObjectNotFoundException CreateRemoteObjectNotFoundException(Server server, string className, Exception inner)
	{
		ObjectNotFoundException result = new ObjectNotFoundException(string.Format(CultureInfo.CurrentCulture, ErrorMessages.RemoteObjectNotFound, server), inner);
		VMTrace.TraceError(string.Format(CultureInfo.InvariantCulture, "Unable to get objects of class '{0}'.", className));
		return result;
	}

	public static ServerConnectionException CreateServerConnectionException(string serverName, ServerConnectionIssue connectionIssue, bool callback, Exception inner)
	{
		if (serverName == null)
		{
			throw new ArgumentNullException("serverName");
		}
		string text = MapServerConnectionIssueToString(serverName, connectionIssue, callback, inner);
		ServerConnectionException result = new ServerConnectionException(text, connectionIssue, inner);
		VMTrace.TraceError(string.Format(CultureInfo.InvariantCulture, "Unable to connect to server '{0}'. Error message: '{1}'", serverName, text));
		return result;
	}

	public static InvalidWmiValueException CreateInvalidPropertyValueException(string propertyName, Type propertyType, object propertyValue, Exception inner)
	{
		string text = string.Format(CultureInfo.CurrentCulture, ErrorMessages.InvalidPropertyValue, propertyName);
		InvalidWmiValueException result = new InvalidWmiValueException(propertyName, propertyType, propertyValue, text, inner);
		VMTrace.TraceError(text);
		return result;
	}

	public static InvalidWmiValueException CreateInvalidMethodReturnValueException(string methodName, Type returnType, object returnValue, Exception inner)
	{
		string text = string.Format(CultureInfo.CurrentCulture, ErrorMessages.InvalidMethodReturnValue, methodName, returnValue ?? "null");
		InvalidWmiValueException result = new InvalidWmiValueException(methodName, returnType, returnValue, text, inner);
		VMTrace.TraceError(text);
		return result;
	}

	public static Exception CreateServerException(Server server, Exception serverException)
	{
		return CreateServerException(server, callback: false, serverException);
	}

	public static Exception CreateServerException(Server server, bool callback, Exception serverException)
	{
		if (server == null)
		{
			throw new ArgumentNullException("server");
		}
		if (serverException == null)
		{
			throw new ArgumentNullException("serverException");
		}
		FilterFailureReason filterFailureReason = FilterFailureReason.CallFailed;
		ServerCallFailedReason reason = ServerCallFailedReason.Unknown;
		ServerConnectionIssue connectionIssue = ServerConnectionIssue.Unknown;
		if (serverException is COMException)
		{
			uint hResult = (uint)((COMException)serverException).HResult;
			switch (hResult)
			{
			case 2147944122u:
				filterFailureReason = FilterFailureReason.ConnectionIssue;
				connectionIssue = ServerConnectionIssue.RpcServerUnavailable;
				break;
			case 2147549186u:
			case 2147944126u:
				reason = ServerCallFailedReason.RpcCallFailed;
				break;
			default:
				VMTrace.TraceWarning("CreateServerException: Unknown server COMException hresult: " + hResult.ToString(CultureInfo.InvariantCulture) + " We should check to see if we would like to add a new error message for this case rather than the generic error message.");
				break;
			}
		}
		else if (serverException is CimException)
		{
			CimException ex = (CimException)serverException;
			switch (ex.NativeErrorCode)
			{
			case NativeErrorCode.InvalidClass:
				filterFailureReason = FilterFailureReason.ConnectionIssue;
				connectionIssue = ServerConnectionIssue.InvalidClass;
				break;
			case NativeErrorCode.InvalidNamespace:
				filterFailureReason = FilterFailureReason.ConnectionIssue;
				connectionIssue = ServerConnectionIssue.InvalidNamespace;
				break;
			case NativeErrorCode.AccessDenied:
				filterFailureReason = FilterFailureReason.ConnectionIssue;
				connectionIssue = ServerConnectionIssue.AccessDenied;
				break;
			case NativeErrorCode.NotFound:
				filterFailureReason = FilterFailureReason.ServerObjectDeleted;
				break;
			case NativeErrorCode.Ok:
				reason = ServerCallFailedReason.Unknown;
				VMTrace.TraceWarning("CreateServerException: Why is WMI returning an exception with ErrorCode \"NoError\"?");
				break;
			case NativeErrorCode.NotSupported:
				reason = ServerCallFailedReason.NotSupported;
				break;
			case NativeErrorCode.Failed:
			{
				CimInstance errorData = ex.ErrorData;
				if (errorData != null && string.Equals((string)errorData.CimInstanceProperties["error_Type"].Value, "HRESULT", StringComparison.OrdinalIgnoreCase))
				{
					switch ((uint)errorData.CimInstanceProperties["error_Code"].Value)
					{
					case 2150859170u:
						filterFailureReason = FilterFailureReason.ConnectionIssue;
						connectionIssue = ServerConnectionIssue.CredSspNotEnabledOnClient;
						break;
					case 2150859171u:
						filterFailureReason = FilterFailureReason.ConnectionIssue;
						connectionIssue = ServerConnectionIssue.ClientCannotDelegateCredentials;
						break;
					case 2147942405u:
						filterFailureReason = FilterFailureReason.ConnectionIssue;
						connectionIssue = ServerConnectionIssue.AccessDenied;
						break;
					}
				}
				break;
			}
			default:
				reason = ServerCallFailedReason.Unknown;
				break;
			}
		}
		else if (serverException is UnauthorizedAccessException)
		{
			filterFailureReason = FilterFailureReason.ConnectionIssue;
			connectionIssue = ServerConnectionIssue.AccessDenied;
		}
		else if (serverException is VirtualizationManagementException)
		{
			filterFailureReason = FilterFailureReason.VirtualizationManagementException;
		}
		else
		{
			reason = ServerCallFailedReason.Unknown;
		}
		switch (filterFailureReason)
		{
		case FilterFailureReason.CallFailed:
			return CreateServerCallFailedException(server, reason, serverException);
		case FilterFailureReason.ConnectionIssue:
			return CreateServerConnectionException(server.UserSpecifiedName, connectionIssue, callback, serverException);
		case FilterFailureReason.ServerObjectDeleted:
			return CreateServerObjectDeletedException(server, serverException);
		case FilterFailureReason.VirtualizationManagementException:
			VMTrace.TraceWarning("Filtering a VirtualizationManagementException. These exceptions should generally not be caught by the CreateServerException filter. Re-throwing.");
			return serverException;
		default:
			throw new Exception("TODO: Um we should not get here.");
		}
	}

	public static ServerCallFailedException CreateServerCallFailedException(Server server, ServerCallFailedReason reason, Exception inner)
	{
		if (server == null)
		{
			throw new ArgumentNullException("server");
		}
		string text = MapServerCallFailedReasonToString(reason, server, inner);
		ServerCallFailedException result = new ServerCallFailedException(text, reason, inner);
		VMTrace.TraceError("Server call failed! Reason of failure: " + text);
		return result;
	}

	public static ServerObjectDeletedException CreateServerObjectDeletedException(Server server, Exception innerException)
	{
		if (server == null)
		{
			throw new ArgumentNullException("server");
		}
		return new ServerObjectDeletedException(string.Format(CultureInfo.CurrentCulture, ErrorMessages.ObjectDeleted, server), innerException);
	}

	public static CancelTaskFailedException CreateCancelTaskFailedException(long errorCode, ErrorCodeMapper mapper)
	{
		if (mapper == null)
		{
			mapper = new ErrorCodeMapper();
		}
		string cancelTaskFailed = ErrorMessages.CancelTaskFailed;
		cancelTaskFailed = mapper.MapError(VirtualizationOperation.CancelTask, errorCode, cancelTaskFailed);
		CancelTaskFailedException result = new CancelTaskFailedException(cancelTaskFailed)
		{
			Operation = VirtualizationOperation.CancelTask,
			ErrorCode = errorCode
		};
		VMTrace.TraceError(string.Format(CultureInfo.InvariantCulture, "Failed to cancel a task. Error code: '{0}'.", errorCode));
		return result;
	}

	public static ClassDefinitionMismatchException CreateClassDefinitionMismatchException(ClassDefinitionMismatchReason reason, string className, string propertyOrMethodName, Exception innerException)
	{
		string text = string.Empty;
		switch (reason)
		{
		case ClassDefinitionMismatchReason.Property:
			text = string.Format(CultureInfo.CurrentCulture, ErrorMessages.InvalidProperty, className, propertyOrMethodName);
			break;
		case ClassDefinitionMismatchReason.Method:
			text = string.Format(CultureInfo.CurrentCulture, ErrorMessages.InvalidMethod, className, propertyOrMethodName);
			break;
		}
		ClassDefinitionMismatchException result = new ClassDefinitionMismatchException(text, innerException);
		VMTrace.TraceError(text);
		return result;
	}

	private static string MapServerConnectionIssueToString(string serverName, ServerConnectionIssue connectionIssue, bool callback, Exception exception)
	{
		string text = null;
		return string.Format(format: connectionIssue switch
		{
			ServerConnectionIssue.ServerResolution => ErrorMessages.ConnectionIssue_ServerResolutionException, 
			ServerConnectionIssue.AccessDenied => callback ? ErrorMessages.ConnectionIssue_AccessDeniedOnCallback : ErrorMessages.ConnectionIssue_AccessDenied, 
			ServerConnectionIssue.InvalidNamespace => ErrorMessages.ConnectionIssue_NotInstalled, 
			ServerConnectionIssue.InvalidClass => ErrorMessages.ConnectionIssue_InvalidClass, 
			ServerConnectionIssue.RpcServerUnavailable => callback ? ErrorMessages.ConnectionIssue_RpcServerUnavailableOnCallback : ErrorMessages.ConnectionIssue_RpcServerUnavailable, 
			ServerConnectionIssue.ConnectedWithDifferentCredentials => ErrorMessages.ConnectionIssue_ConnectedWithDifferentCredentials, 
			ServerConnectionIssue.CredentialsNotSupportedOnLocalHost => ErrorMessages.ConnectionIssue_UserCredentialsNotSupportedOnLocalhost, 
			ServerConnectionIssue.CredSspNotEnabledOnClient => ErrorMessages.ConnectionIssue_CredSspNotConfiguredOnClient, 
			_ => (exception == null) ? ErrorMessages.ConnectionIssue_ReallyUnknown : ErrorMessages.ConnectionIssue_Unknown, 
		}, provider: CultureInfo.CurrentCulture, arg0: serverName, arg1: Environment.GetEnvironmentVariable("COMPUTERNAME"), arg2: (exception != null) ? exception.Message : "");
	}

	private static string MapServerCallFailedReasonToString(ServerCallFailedReason reason, Server server, Exception exception)
	{
		return string.Format(format: reason switch
		{
			ServerCallFailedReason.RpcCallFailed => ErrorMessages.ServerCallFailed_RpcCallFailed, 
			ServerCallFailedReason.ServerOutOfMemoryOrDiskSpace => ErrorMessages.ServerCallFailed_OutOfMemoryOrDiskSpace, 
			ServerCallFailedReason.TimedOut => ErrorMessages.ServerCallFailed_TimedOut, 
			ServerCallFailedReason.UnknownProviderError => ErrorMessages.ServerCallFailed_UnknownProviderError, 
			ServerCallFailedReason.NotSupported => ErrorMessages.OperationFailed_NotSupported, 
			_ => (exception != null) ? ErrorMessages.ServerCallFailed_Unknown : ErrorMessages.ServerCallFailed_ReallyUnknown, 
		}, provider: CultureInfo.CurrentCulture, arg0: server, arg1: (exception != null) ? exception.Message : "");
	}
}
