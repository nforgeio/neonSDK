using System;
using System.Management.Automation;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VirtualizationException : Exception
{
    public string ErrorIdentifier { get; internal set; }

    public ErrorCategory ErrorCategory { get; internal set; }

    public VirtualizationObject TargetObject { get; internal set; }

    internal VirtualizationException(string message, Exception innerException, string errorIdentifier, ErrorCategory errorCategory, VirtualizationObject targetObject)
        : base(message, innerException)
    {
        ErrorIdentifier = errorIdentifier;
        ErrorCategory = errorCategory;
        TargetObject = targetObject;
    }
}
