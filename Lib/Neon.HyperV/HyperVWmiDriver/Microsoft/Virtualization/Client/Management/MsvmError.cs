using System;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_Error")]
internal sealed class MsvmError : EmbeddedInstance
{
    private static class WmiMemberNames
    {
        public const string Id = "MessageID";

        public const string ErrorSource = "ErrorSource";

        public const string Message = "Message";

        public const string MessageArguments = "MessageArguments";
    }

    public long Id => Convert.ToInt64(GetProperty<string>("MessageID"), CultureInfo.InvariantCulture);

    public WmiObjectPath ErrorSource
    {
        get
        {
            WmiObjectPath result = null;
            if (TryGetProperty<string>("ErrorSource", out var value) && !string.IsNullOrEmpty(value))
            {
                result = WmiObjectPath.FromRelativePath(base.Server, base.Server.VirtualizationNamespace, value);
            }
            return result;
        }
    }

    public string Message => GetProperty<string>("Message");

    public IReadOnlyList<string> MessageArguments
    {
        get
        {
            string[] value = null;
            if (!TryGetProperty<string[]>("MessageArguments", out value))
            {
                value = new string[0];
            }
            return value;
        }
    }
}
