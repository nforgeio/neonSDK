using System;
using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal static class InstanceEventManager
{
    private static readonly Dictionary<EventObjectKey, InstanceEventBulkMonitor> gm_InstanceEventMonitorTable = new Dictionary<EventObjectKey, InstanceEventBulkMonitor>();

    internal static readonly TimeSpan gm_DefaultTimeInterval = TimeSpan.FromSeconds(2.0);

    internal static InstanceEventBulkMonitor GetInstanceEventMonitor(EventObjectKey key)
    {
        InstanceEventBulkMonitor value = null;
        lock (gm_InstanceEventMonitorTable)
        {
            if (!gm_InstanceEventMonitorTable.TryGetValue(key, out value))
            {
                value = new InstanceEventBulkMonitor(key);
                gm_InstanceEventMonitorTable[key] = value;
                return value;
            }
            return value;
        }
    }

    public static void TeardownInstanceEventMonitors(Server server)
    {
        List<InstanceEventBulkMonitor> list = new List<InstanceEventBulkMonitor>();
        lock (gm_InstanceEventMonitorTable)
        {
            List<EventObjectKey> list2 = new List<EventObjectKey>(gm_InstanceEventMonitorTable.Count);
            list.Capacity = gm_InstanceEventMonitorTable.Count;
            foreach (KeyValuePair<EventObjectKey, InstanceEventBulkMonitor> item in gm_InstanceEventMonitorTable)
            {
                if (server == null || server.Equals(item.Key.Server))
                {
                    list2.Add(item.Key);
                    list.Add(item.Value);
                }
            }
            foreach (EventObjectKey item2 in list2)
            {
                gm_InstanceEventMonitorTable.Remove(item2);
            }
        }
        foreach (InstanceEventBulkMonitor item3 in list)
        {
            item3.TearDown();
        }
    }
}
