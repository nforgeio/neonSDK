using System.Globalization;

namespace Microsoft.Virtualization.Client.Management;

internal class ErrorCodeMapper
{
	public virtual string MapError(VirtualizationOperation operation, long errorCode, string operationFailedMsg)
	{
		string result = operationFailedMsg;
		long num = errorCode - -3;
		if ((ulong)num <= 3uL)
		{
			switch (num)
			{
			case 1L:
				result = Concatenate(operationFailedMsg, ErrorMessages.OperationFailed_ExpectedAffectedElementNotFound);
				goto IL_0196;
			case 0L:
				result = Concatenate(operationFailedMsg, ErrorMessages.OperationFailed_TaskDeleted);
				goto IL_0196;
			case 2L:
			case 3L:
				goto IL_0196;
			}
		}
		if (errorCode != 4096)
		{
			long num2 = errorCode - 32768;
			if ((ulong)num2 <= 21uL)
			{
				switch (num2)
				{
				case 1L:
					result = Concatenate(operationFailedMsg, ErrorMessages.OperationFailed_AccessDenied);
					goto IL_0196;
				case 2L:
					result = Concatenate(operationFailedMsg, ErrorMessages.OperationFailed_NotSupported);
					goto IL_0196;
				case 4L:
					result = Concatenate(operationFailedMsg, ErrorMessages.OperationFailed_TimedOut);
					goto IL_0196;
				case 5L:
					result = Concatenate(operationFailedMsg, ErrorMessages.OperationFailed_InvalidParameter);
					goto IL_0196;
				case 6L:
					result = Concatenate(operationFailedMsg, ErrorMessages.OperationFailed_StatusInUse);
					goto IL_0196;
				case 7L:
					result = Concatenate(operationFailedMsg, ErrorMessages.OperationFailed_InvalidState);
					goto IL_0196;
				case 8L:
					result = Concatenate(operationFailedMsg, ErrorMessages.OperationFailed_IncorrectType);
					goto IL_0196;
				case 9L:
					result = Concatenate(operationFailedMsg, ErrorMessages.OperationFailed_Unavailable);
					goto IL_0196;
				case 10L:
					result = Concatenate(operationFailedMsg, ErrorMessages.OperationFailed_OutOfMemory);
					goto IL_0196;
				case 11L:
					result = Concatenate(operationFailedMsg, ErrorMessages.OperationFailed_FileNotFound);
					goto IL_0196;
				case 15L:
					result = ErrorMessages.RebootRequired;
					goto IL_0196;
				case 21L:
					result = Concatenate(operationFailedMsg, ErrorMessages.OperationFailed_ObjectNotFound);
					goto IL_0196;
				case 0L:
				case 3L:
					goto IL_0196;
				}
			}
			string furtherErrorSummary = string.Format(CultureInfo.CurrentCulture, ErrorMessages.OperationFailed_UnknownErrorCode, errorCode);
			result = Concatenate(operationFailedMsg, furtherErrorSummary);
		}
		goto IL_0196;
		IL_0196:
		return result;
	}

	public static string Concatenate(string initialErrorSummary, string furtherErrorSummary)
	{
		if (string.IsNullOrEmpty(initialErrorSummary))
		{
			return furtherErrorSummary;
		}
		if (string.IsNullOrEmpty(furtherErrorSummary))
		{
			return initialErrorSummary;
		}
		return initialErrorSummary + "\r\n\r\n" + furtherErrorSummary;
	}
}
