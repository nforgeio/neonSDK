using System;

namespace Microsoft.Virtualization.Client.Management;

internal static class VMProcessorStatusUtilities
{
	internal static bool IsVMProcessorOperationalStatusValid(VMProcessorOperationalStatus operationalStatus)
	{
		return Enum.IsDefined(typeof(VMProcessorOperationalStatus), operationalStatus);
	}

	internal static VMProcessorOperationalStatus[] ConvertOperationalStatus(ushort[] operationalStatusValues)
	{
		if (operationalStatusValues == null)
		{
			return null;
		}
		VMProcessorOperationalStatus[] array = new VMProcessorOperationalStatus[operationalStatusValues.Length];
		for (int i = 0; i < operationalStatusValues.Length; i++)
		{
			array[i] = (VMProcessorOperationalStatus)operationalStatusValues[i];
			if (!IsVMProcessorOperationalStatusValid(array[i]))
			{
				throw ThrowHelper.CreateInvalidPropertyValueException("OperationalStatus", typeof(VMProcessorOperationalStatus), array[i], null);
			}
		}
		return array;
	}
}
