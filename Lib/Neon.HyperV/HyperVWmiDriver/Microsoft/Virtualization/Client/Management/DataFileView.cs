#define TRACE
using System.Globalization;

namespace Microsoft.Virtualization.Client.Management;

internal class DataFileView : View, IDataFile, IVirtualizationManagementObject, IDeleteable
{
	internal class DataFileErrorCodeMapper : ErrorCodeMapper
	{
		private const long gm_AccessDenied = 8L;

		private const long gm_FileNotFound = 9L;

		private const long gm_DirectoryNotEmpty = 14L;

		private const long gm_SharingViolation = 15L;

		private const long gm_PrivilegeNotHold = 17L;

		public override string MapError(VirtualizationOperation operation, long errorCode, string operationFailedMsg)
		{
			string text = operationFailedMsg;
			switch (errorCode)
			{
			default:
			{
				long num = errorCode - 14;
				if ((ulong)num <= 3uL)
				{
					switch (num)
					{
					case 0L:
						return ErrorCodeMapper.Concatenate(operationFailedMsg, ErrorMessages.DataFileError_DirectoryNotEmpty);
					case 1L:
						return ErrorCodeMapper.Concatenate(operationFailedMsg, ErrorMessages.DataFileError_SharingViolation);
					case 3L:
						return ErrorCodeMapper.Concatenate(operationFailedMsg, ErrorMessages.DataFileError_SharingViolation);
					}
				}
				string furtherErrorSummary = string.Format(CultureInfo.CurrentCulture, ErrorMessages.OperationFailed_UnknownErrorCode, errorCode);
				return ErrorCodeMapper.Concatenate(operationFailedMsg, furtherErrorSummary);
			}
			case 8L:
				return ErrorCodeMapper.Concatenate(operationFailedMsg, ErrorMessages.DataFileError_AccessDenied);
			case 9L:
				return ErrorCodeMapper.Concatenate(operationFailedMsg, ErrorMessages.DataFileError_InvalidObject);
			}
		}
	}

	internal static class WmiMemberNames
	{
		public const string System = "System";

		public const string Hidden = "Hidden";

		public const string Delete = "Delete";

		public const string Copy = "Copy";
	}

	public string Path => GetProperty<string>("Name");

	public bool IsSystem => GetProperty<bool>("System");

	public bool IsHidden => GetProperty<bool>("Hidden");

	public void Delete()
	{
		VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Deleting data file {0}", Path));
		uint num = InvokeMethod("Delete");
		if (num != View.ErrorCodeSuccess)
		{
			throw ThrowHelper.CreateVirtualizationOperationFailedException(string.Format(CultureInfo.CurrentCulture, ErrorMessages.DeleteDataFileFailed, Path), VirtualizationOperation.DeleteDataFile, num, GetErrorCodeMapper(), null);
		}
		VMTrace.TraceUserActionCompleted("Deleting data file completed successfully.");
		base.ProxyFactory.Repository.UnregisterProxy(base.Proxy);
	}

	public void Copy(string destinationFile)
	{
		VMTrace.TraceUserActionInitiated(string.Format(CultureInfo.InvariantCulture, "Copying data file {0} to {1}", Path, destinationFile));
		uint num = InvokeMethod("Copy", destinationFile);
		if (num != View.ErrorCodeSuccess)
		{
			throw ThrowHelper.CreateVirtualizationOperationFailedException(string.Format(CultureInfo.CurrentCulture, ErrorMessages.CopyDataFileFailed, Path), VirtualizationOperation.CopyDataFile, num, GetErrorCodeMapper(), null);
		}
		VMTrace.TraceUserActionCompleted("Copying data file completed successfully.");
	}

	protected override ErrorCodeMapper GetErrorCodeMapper()
	{
		return new DataFileErrorCodeMapper();
	}
}
