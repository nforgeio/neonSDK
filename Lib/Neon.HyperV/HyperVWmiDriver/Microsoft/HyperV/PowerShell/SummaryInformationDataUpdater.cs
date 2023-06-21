using System;
using System.Globalization;
using Microsoft.HyperV.PowerShell.Common;
using Microsoft.Virtualization.Client.Management;

using ErrorMessages = Microsoft.HyperV.PowerShell.Common.ErrorMessages;

namespace Microsoft.HyperV.PowerShell;

internal sealed class SummaryInformationDataUpdater : IDataUpdater<ISummaryInformation>
{
    private ISummaryInformation m_SummaryInformation;

    private readonly IVMComputerSystem m_VirtualMachine;

    private DateTime m_TimeOfLastUpdate;

    private readonly object m_SyncObject = new object();

    private bool m_Initialized;

    private bool m_IsDeleted;

    public bool IsDeleted
    {
        get
        {
            return m_IsDeleted;
        }
        private set
        {
            if (!m_IsDeleted && value)
            {
                this.Deleted?.Invoke(this, EventArgs.Empty);
            }
            m_IsDeleted = value;
        }
    }

    public bool IsTemplate => false;

    public event EventHandler Deleted;

    public SummaryInformationDataUpdater(IVMComputerSystem virtualMachine)
    {
        m_VirtualMachine = virtualMachine;
        EnsureUpdated(Constants.UpdateThreshold);
    }

    public ISummaryInformation GetData(UpdatePolicy policy)
    {
        switch (policy)
        {
        case UpdatePolicy.EnsureUpdated:
            EnsureUpdated(Constants.UpdateThreshold);
            break;
        case UpdatePolicy.EnsureAssociatorsUpdated:
            throw new NotImplementedException("The summary information has no related objects.");
        default:
            throw new ArgumentOutOfRangeException("policy", string.Format(CultureInfo.CurrentCulture, ErrorMessages.ArgumentOutOfRange_InvalidEnumValue, policy.ToString(), typeof(UpdatePolicy).Name));
        case UpdatePolicy.None:
            break;
        }
        return m_SummaryInformation;
    }

    private void EnsureUpdated(TimeSpan threshold)
    {
        lock (m_SyncObject)
        {
            if (!m_Initialized || DateTime.Now - m_TimeOfLastUpdate >= threshold || m_TimeOfLastUpdate < m_VirtualMachine.Server.LastCacheFlushTime)
            {
                m_TimeOfLastUpdate = DateTime.Now;
                ISummaryInformation vMSummaryInformation = m_VirtualMachine.GetVMSummaryInformation(SummaryInformationRequest.Scripting);
                if (vMSummaryInformation == null)
                {
                    IsDeleted = true;
                    this.Deleted?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    m_SummaryInformation = vMSummaryInformation;
                }
                m_Initialized = true;
            }
        }
    }
}
