using System;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal sealed class DependentObjectDataUpdater<T> : DataUpdaterBase<T> where T : class, IVirtualizationManagementObject
{
    private readonly Func<TimeSpan, T> m_ReloadValueMethod;

    public override bool IsTemplate => false;

    internal DependentObjectDataUpdater(T dataItem, Func<TimeSpan, T> reloadMethod)
        : base(dataItem)
    {
        m_ReloadValueMethod = reloadMethod;
    }

    protected override void UpdateAssociators(TimeSpan threshold)
    {
        UpdateCacheInternal(threshold, UpdatePolicy.EnsureAssociatorsUpdated);
    }

    protected override void UpdateProperties(TimeSpan threshold)
    {
        UpdateCacheInternal(threshold, UpdatePolicy.EnsureUpdated);
    }

    private void UpdateCacheInternal(TimeSpan threshold, UpdatePolicy updatePolicy)
    {
        bool flag = m_Value == null || base.IsDeleted;
        Exception ex = null;
        if (!flag)
        {
            try
            {
                if (updatePolicy == UpdatePolicy.EnsureUpdated)
                {
                    m_Value.UpdatePropertyCache(threshold);
                }
                else
                {
                    m_Value.UpdateAssociationCache(threshold);
                }
            }
            catch (ServerObjectDeletedException ex2)
            {
                ex = ex2;
                flag = true;
            }
        }
        if (!flag)
        {
            return;
        }
        m_Value = m_ReloadValueMethod(threshold);
        if (m_Value != null)
        {
            if (updatePolicy == UpdatePolicy.EnsureUpdated)
            {
                m_Value.UpdatePropertyCache(threshold);
            }
            else
            {
                m_Value.UpdateAssociationCache(threshold);
            }
        }
        else if (ex != null)
        {
            throw ex;
        }
    }
}
