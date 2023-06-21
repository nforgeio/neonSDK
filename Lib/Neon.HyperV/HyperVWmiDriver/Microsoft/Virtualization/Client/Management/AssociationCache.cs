using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Virtualization.Client.Management;

internal class AssociationCache : CacheBase
{
    private List<ObjectKey> m_RelatedObjectKeys;

    private readonly Association m_Association;

    public List<ObjectKey> RelatedObjectKeys => m_RelatedObjectKeys;

    public AssociationCache(ObjectKey key, Association association)
        : base(key)
    {
        if (association == null)
        {
            throw new ArgumentNullException("association");
        }
        m_Association = association;
        m_RelatedObjectKeys = new List<ObjectKey>();
    }

    protected override bool PerformCacheUpdate(WmiOperationOptions options)
    {
        List<ObjectKey> relatedObjects = m_Association.GetRelatedObjects(m_Key.Server, m_Key.ManagementPath, options);
        lock (m_ObjectLock)
        {
            m_RelatedObjectKeys = relatedObjects;
            m_LastWmiUpdateTime = DateTime.Now;
            m_Initialized = true;
            return true;
        }
    }

    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is called via contracts.")]
    private void ObjectInvariant()
    {
    }
}
