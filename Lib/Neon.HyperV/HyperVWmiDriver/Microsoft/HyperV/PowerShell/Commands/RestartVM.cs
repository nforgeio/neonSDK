using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Threading;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Management.Infrastructure;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Restart", "VM", SupportsShouldProcess = true, DefaultParameterSetName = "Name")]
[OutputType(new Type[] { typeof(VirtualMachine) })]
internal sealed class RestartVM : VirtualizationCmdlet<VirtualMachine>, IVMObjectOrNameCmdlet, IVmByObjectCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmByNameCmdlet, ISupportsForce, ISupportsAsJob, ISupportsPassthrough, IDisposable
{
    private static class ParameterSetNames
    {
        public const string NameWait = "NameWait";

        public const string VMObjectWait = "VMObjectWait";
    }

    internal enum RestartType
    {
        Reset,
        Reboot
    }

    internal enum WaitVMTypes
    {
        Heartbeat,
        IPAddress
    }

    private ManualResetEventSlim waitHandle = new ManualResetEventSlim(initialState: false);

    private bool disposed;

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "Name")]
    [Parameter(ParameterSetName = "NameWait")]
    [ValidateNotNullOrEmpty]
    public override CimSession[] CimSession { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "Name")]
    [Parameter(ParameterSetName = "NameWait")]
    [ValidateNotNullOrEmpty]
    public override string[] ComputerName { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [Parameter(ParameterSetName = "Name")]
    [Parameter(ParameterSetName = "NameWait")]
    [ValidateNotNullOrEmpty]
    [CredentialArray]
    public override PSCredential[] Credential { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
    [ValidateNotNullOrEmpty]
    [Parameter(ParameterSetName = "VMObject", ValueFromPipeline = true, Position = 0, Mandatory = true)]
    [Parameter(ParameterSetName = "VMObjectWait", ValueFromPipeline = true, Position = 0, Mandatory = true)]
    public VirtualMachine[] VM { get; set; }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
    [ValidateNotNullOrEmpty]
    [Alias(new string[] { "VMName" })]
    [Parameter(ParameterSetName = "Name", ValueFromPipeline = true, Position = 0, Mandatory = true)]
    [Parameter(ParameterSetName = "NameWait", ValueFromPipeline = true, Position = 0, Mandatory = true)]
    public string[] Name { get; set; }

    [Parameter]
    public SwitchParameter Force { get; set; }

    [Parameter]
    public SwitchParameter AsJob { get; set; }

    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
    [Parameter]
    public SwitchParameter Passthru { get; set; }

    [Parameter(ParameterSetName = "NameWait", Mandatory = true)]
    [Parameter(ParameterSetName = "VMObjectWait", Mandatory = true)]
    public SwitchParameter Wait { get; set; }

    [Parameter(ParameterSetName = "NameWait")]
    [Parameter(ParameterSetName = "VMObjectWait")]
    public WaitVMTypes For { get; set; }

    [ValidateRange(1, ushort.MaxValue)]
    [Parameter(ParameterSetName = "NameWait")]
    [Parameter(ParameterSetName = "VMObjectWait")]
    public ushort Delay { get; set; } = 5;


    [Alias(new string[] { "TimeoutSec" })]
    [ValidateRange(-1, int.MaxValue)]
    [Parameter(ParameterSetName = "NameWait")]
    [Parameter(ParameterSetName = "VMObjectWait")]
    public int? Timeout { get; set; }

    [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
    [Parameter(ParameterSetName = "Name")]
    [Parameter(ParameterSetName = "VMObject")]
    [Parameter(ParameterSetName = "NameWait")]
    [Parameter(ParameterSetName = "VMObjectWait")]
    public RestartType Type { get; set; }

    public void Dispose()
    {
        if (!disposed)
        {
            waitHandle.Dispose();
            disposed = true;
        }
    }

    internal override IList<VirtualMachine> EnumerateOperands(IOperationWatcher operationWatcher)
    {
        return ParameterResolvers.ResolveVirtualMachines(this, operationWatcher);
    }

    internal override void ProcessOneOperand(VirtualMachine operand, IOperationWatcher operationWatcher)
    {
        string name = operand.Name;
        if (!operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_RestartVM, name)) || (Type != RestartType.Reboot && !operationWatcher.ShouldContinue(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldContinue_RestartVM, name))))
        {
            return;
        }
        bool flag = Timeout == -1;
        DateTime now = DateTime.Now;
        TimeSpan timeSpan = TimeSpan.FromSeconds(Timeout ?? 300);
        DateTime expireTime = (flag ? DateTime.MaxValue : (now + timeSpan));
        if (Type == RestartType.Reboot)
        {
            if (!operand.IsShutdownComponentAvailable())
            {
                throw ExceptionHelper.CreateOperationFailedException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.OperationFailed_ShutdownICNotAvailableForReboot, name));
            }
            ((IVMShutdownComponent)operand.GetVMIntegrationComponents().OfType<ShutdownComponent>().First()
                .m_IntegrationComponentSetting.GetData(UpdatePolicy.EnsureAssociatorsUpdated).GetIntegrationComponent()).InitiateReboot(Force.IsPresent, CmdletResources.RebootReason);
            if (!operationWatcher.WaitOnCondition(() => !operand.IsShutdownComponentAvailable(), waitHandle, expireTime))
            {
                throw ExceptionHelper.CreateOperationFailedException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.OperationFailed_RestartTimeout, timeSpan.TotalSeconds));
            }
            if (!operationWatcher.WaitOnCondition(() => operand.IsShutdownComponentAvailable(), waitHandle, expireTime))
            {
                throw ExceptionHelper.CreateOperationFailedException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.OperationFailed_RestartTimeout, timeSpan.TotalSeconds));
            }
        }
        else
        {
            TimeSpan lastUptime = operand.GetCurrentUptime();
            operand.ChangeState(VirtualMachineAction.Restart, operationWatcher);
            TimeSpan currentUptime;
            if (!operationWatcher.WaitOnCondition(delegate
            {
                currentUptime = operand.GetCurrentUptime();
                bool result = currentUptime < lastUptime;
                lastUptime = currentUptime;
                return result;
            }, waitHandle, expireTime))
            {
                throw ExceptionHelper.CreateOperationFailedException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.OperationFailed_RestartTimeout, timeSpan.TotalSeconds));
            }
        }
        if (Wait.IsPresent)
        {
            TimeSpan timeSpan2 = DateTime.Now - now;
            TimeSpan timeSpan3 = (flag ? TimeSpan.FromSeconds(-1.0) : (timeSpan - timeSpan2));
            if (!flag && timeSpan3 < TimeSpan.Zero)
            {
                timeSpan3 = TimeSpan.Zero;
            }
            if (!operand.TryWaitCondition((global::Microsoft.HyperV.PowerShell.WaitVMTypes)For, timeSpan3, Delay, waitHandle))
            {
                throw ExceptionHelper.CreateOperationFailedException(CmdletErrorMessages.OperationFailed_ConditionNotSatisfied);
            }
        }
        if (Passthru.IsPresent)
        {
            operationWatcher.WriteObject(operand);
        }
    }

    protected override void StopProcessing()
    {
        waitHandle.Set();
        base.StopProcessing();
    }

    protected override void EndProcessing()
    {
        Dispose();
        base.EndProcessing();
    }
}
