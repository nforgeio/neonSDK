namespace Microsoft.Virtualization.Client.Management;

internal class VMShutdownComponentView : VMIntegrationComponentView, IVMShutdownComponent, IVMIntegrationComponent, IVMDevice, IVirtualizationManagementObject
{
	internal static class WmiMemberNames
	{
		public const string InitiateReboot = "InitiateReboot";

		public const string InitiateShutdown = "InitiateShutdown";
	}

	internal class ShutdownErrorCodeMapper : ErrorCodeMapper
	{
		public const long gm_NotReady = 32780L;

		public const long gm_MachineLocked = 32781L;

		public const long gm_ShutdownInProgress = 32782L;

		public override string MapError(VirtualizationOperation operation, long errorCode, string operationFailedMsg)
		{
			switch (errorCode)
			{
			default:
			{
				long num = errorCode - 32780;
				if ((ulong)num <= 2uL)
				{
					switch (num)
					{
					case 0L:
						return ErrorCodeMapper.Concatenate(operationFailedMsg, ErrorMessages.Shutdown_NotReady);
					case 1L:
						return ErrorCodeMapper.Concatenate(operationFailedMsg, ErrorMessages.Shutdown_MachineLocked);
					case 2L:
						return ErrorCodeMapper.Concatenate(operationFailedMsg, ErrorMessages.Shutdown_ShutdownInProgress);
					}
				}
				break;
			}
			case 32769L:
				return ErrorCodeMapper.Concatenate(operationFailedMsg, ErrorMessages.OperationFailed_AccessDenied);
			case 32768L:
				break;
			}
			return ErrorCodeMapper.Concatenate(operationFailedMsg, ErrorMessages.Shutdown_GenericFailure);
		}
	}

	public long MachineLockedErrorCode => 32781L;

	protected override ErrorCodeMapper GetErrorCodeMapper()
	{
		return new ShutdownErrorCodeMapper();
	}

	public void InitiateReboot(bool force, string reason)
	{
		object[] args = new object[2] { force, reason };
		uint num = InvokeMethod("InitiateReboot", args);
		if (num != View.ErrorCodeSuccess)
		{
			ErrorCodeMapper errorCodeMapper = GetErrorCodeMapper();
			throw ThrowHelper.CreateVirtualizationOperationFailedException(ErrorMessages.IntegrationComponentRebootFailed, VirtualizationOperation.InitiateReboot, num, errorCodeMapper, null);
		}
	}

	public void InitiateShutdown(bool force, string reason)
	{
		object[] args = new object[2] { force, reason };
		uint num = InvokeMethod("InitiateShutdown", args);
		if (num != View.ErrorCodeSuccess)
		{
			ErrorCodeMapper errorCodeMapper = GetErrorCodeMapper();
			throw ThrowHelper.CreateVirtualizationOperationFailedException(ErrorMessages.IntegrationComponentShutdownFailed, VirtualizationOperation.InitiateShutdown, num, errorCodeMapper, null);
		}
	}
}
