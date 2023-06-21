#define TRACE
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Microsoft.Virtualization.Client.Management;

internal class TerminalServiceView : View, ITerminalService, IVirtualizationManagementObject
{
    private static class WmiMemberNames
    {
        internal const string GetInteractiveSessionAcl = "GetInteractiveSessionACL";

        internal const string GrantInteractiveSessionAccess = "GrantInteractiveSessionAccess";

        internal const string RevokeInteractiveSessionAccess = "RevokeInteractiveSessionAccess";
    }

    public IVMTask BeginGrantVMConnectAccess(IVMComputerSystem virtualMachine, ICollection<string> trustees)
    {
        if (virtualMachine == null)
        {
            throw new ArgumentNullException("virtualMachine");
        }
        if (trustees == null || trustees.Count == 0)
        {
            throw new ArgumentNullException("trustees");
        }
        string text = string.Format(CultureInfo.InvariantCulture, ErrorMessages.GrantVMConnectAccessFailed, virtualMachine.ManagementPath);
        VMTrace.TraceUserActionInitiated(text);
        object[] array = new object[3]
        {
            virtualMachine,
            trustees.ToArray(),
            null
        };
        uint result = InvokeMethod("GrantInteractiveSessionAccess", array);
        IVMTask iVMTask = BeginMethodTaskReturn(result, null, array[2]);
        iVMTask.ClientSideFailedMessage = text;
        return iVMTask;
    }

    public IVMTask BeginRevokeVMConnectAccess(IVMComputerSystem virtualMachine, ICollection<string> trustees)
    {
        if (virtualMachine == null)
        {
            throw new ArgumentNullException("virtualMachine");
        }
        if (trustees == null || trustees.Count == 0)
        {
            throw new ArgumentNullException("trustees");
        }
        string text = string.Format(CultureInfo.InvariantCulture, ErrorMessages.RevokeVMConnectAccessFailed, virtualMachine.ManagementPath);
        VMTrace.TraceUserActionInitiated(text);
        object[] array = new object[3]
        {
            virtualMachine,
            trustees.ToArray(),
            null
        };
        uint result = InvokeMethod("RevokeInteractiveSessionAccess", array);
        IVMTask iVMTask = BeginMethodTaskReturn(result, null, array[2]);
        iVMTask.ClientSideFailedMessage = text;
        return iVMTask;
    }

    public void EndGrantVMConnectAccess(IVMTask task)
    {
        if (task == null)
        {
            throw new ArgumentNullException("task");
        }
        EndMethod(task, VirtualizationOperation.GrantVMConnectAccess);
        VMTrace.TraceUserActionCompleted("Grant VM connect access completed.");
    }

    public void EndRevokeVMConnectAccess(IVMTask task)
    {
        if (task == null)
        {
            throw new ArgumentNullException("task");
        }
        EndMethod(task, VirtualizationOperation.RevokeVMConnectAccess);
        VMTrace.TraceUserActionCompleted("Revoke VM connect access completed.");
    }

    public IEnumerable<IInteractiveSessionAccess> GetVMConnectAccess(IVMComputerSystem virtualMachine)
    {
        if (virtualMachine == null)
        {
            throw new ArgumentNullException("virtualMachine");
        }
        object[] array = new object[2] { virtualMachine, null };
        uint num = InvokeMethod("GetInteractiveSessionACL", array);
        if (num == View.ErrorCodeSuccess)
        {
            return ((IEnumerable<string>)array[1]).Select((string instance) => EmbeddedInstance.ConvertTo<InteractiveSessionAccess>(base.Server, instance));
        }
        throw ThrowHelper.CreateVirtualizationOperationFailedException(string.Format(CultureInfo.CurrentCulture, ErrorMessages.GetVMConnectAccessFailed, virtualMachine.ManagementPath), VirtualizationOperation.GetVMConnectAccess, num);
    }
}
