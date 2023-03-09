using System;

namespace Microsoft.Virtualization.Client.Management;

internal static class VMIntegrationComponentStatusUtilities
{
	internal static bool IsVMIntegrationComponentOperationalStatusValid(VMIntegrationComponentOperationalStatus operationalStatus)
	{
		if (Enum.IsDefined(typeof(VMIntegrationComponentOperationalStatus), operationalStatus))
		{
			return operationalStatus != VMIntegrationComponentOperationalStatus.Unknown;
		}
		return false;
	}

	internal static VMIntegrationComponentOperationalStatus[] ConvertOperationalStatus(ushort[] operationalStatusValues)
	{
		if (operationalStatusValues == null)
		{
			return null;
		}
		VMIntegrationComponentOperationalStatus[] array = new VMIntegrationComponentOperationalStatus[operationalStatusValues.Length];
		for (int i = 0; i < operationalStatusValues.Length; i++)
		{
			array[i] = (VMIntegrationComponentOperationalStatus)operationalStatusValues[i];
			if (!IsVMIntegrationComponentOperationalStatusValid(array[i]))
			{
				throw ThrowHelper.CreateInvalidPropertyValueException("OperationalStatus", typeof(VMIntegrationComponentOperationalStatus), array[i], null);
			}
		}
		return array;
	}
}
