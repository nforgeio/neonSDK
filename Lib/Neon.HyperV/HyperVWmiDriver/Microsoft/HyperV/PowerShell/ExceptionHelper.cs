#define TRACE
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.HyperV.PowerShell.Common;
using Microsoft.Virtualization.Client;
using Microsoft.Virtualization.Client.Management;

using ErrorMessages = Microsoft.HyperV.PowerShell.Common.ErrorMessages;

namespace Microsoft.HyperV.PowerShell;

internal static class ExceptionHelper
{
    private static void MapExceptionToCategory(Exception exception, out string errorIdentifier, out ErrorCategory errorCategory)
    {
        if (exception is VirtualizationManagementException exception2)
        {
            MapVirtualizationManagementExceptionToCategory(exception2, out errorIdentifier, out errorCategory);
        }
        else if (exception is ArgumentException)
        {
            errorIdentifier = "InvalidParameter";
            errorCategory = ErrorCategory.InvalidArgument;
        }
        else if (exception is InvalidOperationException)
        {
            errorIdentifier = "InvalidOperation";
            errorCategory = ErrorCategory.InvalidOperation;
        }
        else if (exception is UnauthorizedAccessException)
        {
            errorIdentifier = "AccessDenied";
            errorCategory = ErrorCategory.PermissionDenied;
        }
        else if (exception is FileNotFoundException || exception is DirectoryNotFoundException)
        {
            errorIdentifier = "FileNotFound";
            errorCategory = ErrorCategory.ObjectNotFound;
        }
        else if (exception is IOException)
        {
            errorIdentifier = "DeviceNotFound";
            errorCategory = ErrorCategory.ObjectNotFound;
        }
        else
        {
            errorIdentifier = "Unspecified";
            errorCategory = ErrorCategory.NotSpecified;
        }
    }

    private static void MapVirtualizationManagementExceptionToCategory(VirtualizationManagementException exception, out string errorIdentifier, out ErrorCategory errorCategory)
    {
        if (exception is VirtualizationOperationFailedException ex)
        {
            long errorCode = ex.ErrorCode;
            long num = errorCode - 32768;
            if ((ulong)num <= 21uL)
            {
                switch (num)
                {
                case 1L:
                    errorIdentifier = "AccessDenied";
                    errorCategory = ErrorCategory.PermissionDenied;
                    return;
                case 2L:
                    errorIdentifier = "NotSupported";
                    errorCategory = ErrorCategory.InvalidOperation;
                    return;
                case 4L:
                    errorIdentifier = "OperationTimeout";
                    errorCategory = ErrorCategory.OperationTimeout;
                    return;
                case 5L:
                    errorIdentifier = "InvalidParameter";
                    errorCategory = ErrorCategory.InvalidArgument;
                    return;
                case 6L:
                    errorIdentifier = "ObjectInUse";
                    errorCategory = ErrorCategory.ResourceBusy;
                    return;
                case 10L:
                    errorIdentifier = "OutOfMemory";
                    errorCategory = ErrorCategory.FromStdErr;
                    return;
                case 11L:
                    errorIdentifier = "ObjectNotFound";
                    errorCategory = ErrorCategory.ObjectNotFound;
                    return;
                case 15L:
                    errorIdentifier = "RebootRequired";
                    errorCategory = ErrorCategory.FromStdErr;
                    return;
                case 7L:
                    errorIdentifier = "InvalidState";
                    errorCategory = ErrorCategory.InvalidOperation;
                    return;
                case 8L:
                    errorIdentifier = "StatusIncorrectType";
                    errorCategory = ErrorCategory.InvalidOperation;
                    return;
                case 9L:
                    errorIdentifier = "StatusUnavailable";
                    errorCategory = ErrorCategory.ResourceUnavailable;
                    return;
                case 0L:
                    errorIdentifier = "OperationFailed";
                    errorCategory = ErrorCategory.NotSpecified;
                    return;
                case 21L:
                    errorIdentifier = "ObjectNotFound";
                    errorCategory = ErrorCategory.ObjectNotFound;
                    return;
                }
            }
            errorIdentifier = "Unspecified";
            errorCategory = ErrorCategory.NotSpecified;
        }
        else if (exception is ObjectNotFoundException)
        {
            errorIdentifier = "ObjectNotFound";
            errorCategory = ErrorCategory.ObjectNotFound;
        }
        else
        {
            errorIdentifier = "Unspecified";
            errorCategory = ErrorCategory.NotSpecified;
        }
    }

    internal static ErrorRecord GetErrorRecordFromException(Exception exception)
    {
        VirtualizationException ex = exception as VirtualizationException;
        if (ex == null)
        {
            ex = ConvertToVirtualizationException(exception, null);
        }
        return new ErrorRecord(ex, ex.ErrorIdentifier, ex.ErrorCategory, ex.TargetObject);
    }

    internal static VirtualizationException ConvertToVirtualizationException(Exception exception, VirtualizationObject targetObject)
    {
        if (exception is VirtualizationException result)
        {
            return result;
        }
        MapExceptionToCategory(exception, out var errorIdentifier, out var errorCategory);
        return new VirtualizationException(GetErrorMessageFromException(exception), exception, errorIdentifier, errorCategory, targetObject);
    }

    private static string GetErrorMessageFromException(Exception exception)
    {
        if (exception is VirtualizationManagementException ex)
        {
            string message = ex.Message;
            string description = ex.Description;
            if (string.IsNullOrEmpty(description))
            {
                return message;
            }
            if (description.StartsWith(message, StringComparison.CurrentCultureIgnoreCase))
            {
                return description;
            }
            StringBuilder stringBuilder = new StringBuilder(message);
            stringBuilder.AppendLine();
            stringBuilder.AppendLine();
            stringBuilder.Append(description);
            return stringBuilder.ToString();
        }
        return exception.Message;
    }

    internal static VirtualizationException CreateInvalidArgumentException(string message, [Optional] Exception innerException)
    {
        VirtualizationException ex = new VirtualizationException(message, innerException, "InvalidParameter", ErrorCategory.InvalidArgument, null);
        VMTrace.TraceError(message, ex);
        return ex;
    }

    internal static VirtualizationException CreateInvalidStateException(string message, Exception innerException, VirtualizationObject targetObject)
    {
        VirtualizationException ex = new VirtualizationException(message, innerException, "InvalidState", ErrorCategory.InvalidOperation, targetObject);
        VMTrace.TraceError(message, ex);
        return ex;
    }

    internal static VirtualizationException CreateInvalidOperationException(string message, Exception innerException, VirtualizationObject targetObject)
    {
        VirtualizationException ex = new VirtualizationException(message, innerException, "InvalidOperation", ErrorCategory.InvalidOperation, targetObject);
        VMTrace.TraceError(message, ex);
        return ex;
    }

    internal static VirtualizationException CreateObjectNotFoundException(string message, Exception innerException)
    {
        VirtualizationException ex = new VirtualizationException(message, innerException, "ObjectNotFound", ErrorCategory.ObjectNotFound, null);
        VMTrace.TraceError(message, ex);
        return ex;
    }

    internal static VirtualizationException CreateOperationFailedException(string message, Exception innerException = null)
    {
        VirtualizationException ex = new VirtualizationException(message, innerException, "OperationFailed", ErrorCategory.InvalidOperation, null);
        VMTrace.TraceError(message, ex);
        return ex;
    }

    internal static VirtualizationException CreateRollbackFailedException(Exception innerException)
    {
        string operationFailed_RollbackFailed = ErrorMessages.OperationFailed_RollbackFailed;
        VirtualizationException ex = new VirtualizationException(operationFailed_RollbackFailed, innerException, "OperationFailed", ErrorCategory.NotSpecified, null);
        VMTrace.TraceError(operationFailed_RollbackFailed, ex);
        return ex;
    }

    internal static TOutput TryWithLogging<TInput, TOutput>(Func<TInput, TOutput> function, TInput input, IOperationWatcher operationWatcher)
    {
        TOutput result = default(TOutput);
        try
        {
            result = function(input);
            return result;
        }
        catch (Exception e)
        {
            DisplayErrorOnException(e, operationWatcher);
            return result;
        }
    }

    internal static TOutput TryWithLogging<TInput1, TInput2, TOutput>(Func<TInput1, TInput2, TOutput> function, TInput1 first, TInput2 second, IOperationWatcher operationWatcher)
    {
        TOutput result = default(TOutput);
        try
        {
            result = function(first, second);
            return result;
        }
        catch (Exception e)
        {
            DisplayErrorOnException(e, operationWatcher);
            return result;
        }
    }

    internal static IEnumerable<TOutput> TryManyWithLogging<TInput, TOutput>(Func<TInput, IEnumerable<TOutput>> function, TInput input, IOperationWatcher operationWatcher)
    {
        try
        {
            IEnumerable<TOutput> enumerable = function(input);
            if (!(enumerable is ICollection<TOutput>))
            {
                return enumerable.ToList();
            }
            return enumerable;
        }
        catch (Exception e)
        {
            DisplayErrorOnException(e, operationWatcher);
            return Enumerable.Empty<TOutput>();
        }
    }

    internal static void DisplayErrorOnException(Exception e, IOperationWatcher operationWatcher)
    {
        if (e is PipelineStoppedException)
        {
            throw e;
        }
        ErrorRecord errorRecordFromException = GetErrorRecordFromException(e);
        operationWatcher.WriteError(errorRecordFromException);
    }
}
