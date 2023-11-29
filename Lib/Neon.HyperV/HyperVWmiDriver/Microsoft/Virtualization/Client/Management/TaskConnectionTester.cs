#define TRACE
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading;

namespace Microsoft.Virtualization.Client.Management;

internal class TaskConnectionTester
{
    private static readonly Dictionary<Server, TaskConnectionTester> gm_Instances = new Dictionary<Server, TaskConnectionTester>();

    private readonly List<VMTaskView> m_RegisteredTasks;

    private readonly Server m_Server;

    private Timer m_Timer;

    private bool m_Disposed;

    private const int TIMER_INTERVAL = 5000;

    public static void RegisterTask(Server server, VMTaskView task)
    {
        lock (gm_Instances)
        {
            if (!gm_Instances.TryGetValue(server, out var value))
            {
                value = new TaskConnectionTester(server);
                gm_Instances.Add(server, value);
            }
            try
            {
                value.RegisterTask(task);
            }
            catch (ObjectDisposedException)
            {
                value = new TaskConnectionTester(server);
                gm_Instances[server] = value;
                value.RegisterTask(task);
            }
        }
    }

    public static void UnregisterTask(Server server, VMTaskView task)
    {
        lock (gm_Instances)
        {
            if (gm_Instances.TryGetValue(server, out var value))
            {
                value.UnregisterTask(task);
            }
        }
    }

    private static void RemoveTesterInstance(TaskConnectionTester instance)
    {
        lock (gm_Instances)
        {
            if (gm_Instances.TryGetValue(instance.m_Server, out var value) && value == instance)
            {
                gm_Instances.Remove(instance.m_Server);
            }
        }
    }

    private TaskConnectionTester(Server server)
    {
        m_Server = server;
        m_RegisteredTasks = new List<VMTaskView>();
    }

    private void Dispose()
    {
        m_Disposed = true;
        m_Timer.Dispose();
    }

    private void RegisterTask(VMTaskView task)
    {
        lock (m_RegisteredTasks)
        {
            if (m_Disposed)
            {
                throw new ObjectDisposedException("TestConnectionTester");
            }
            m_RegisteredTasks.Add(task);
            if (m_RegisteredTasks.Count == 1)
            {
                m_Timer = new Timer(TestConnectedness, null, 5000, -1);
            }
        }
    }

    private void UnregisterTask(VMTaskView task)
    {
        lock (m_RegisteredTasks)
        {
            if (m_Disposed)
            {
                throw new ObjectDisposedException("TestConnectionTester");
            }
            m_RegisteredTasks.Remove(task);
            if (m_RegisteredTasks.Count == 0)
            {
                Dispose();
            }
        }
        if (m_Disposed)
        {
            RemoveTesterInstance(this);
        }
    }

    private void TestConnectedness(object state)
    {
        VMTrace.TraceInformation(string.Format(CultureInfo.InvariantCulture, "Testing the connectedness of server '{0}' for tasks.", m_Server));
        string text = null;
        lock (m_RegisteredTasks)
        {
            m_RegisteredTasks.RemoveAll((VMTaskView task) => task.IsCompleted);
            if (m_RegisteredTasks.Count == 0)
            {
                Dispose();
            }
        }
        if (m_Disposed)
        {
            RemoveTesterInstance(this);
            return;
        }
        try
        {
            IService win32VirtualizationService = ObjectLocator.GetWin32VirtualizationService(m_Server);
            win32VirtualizationService.UpdatePropertyCache(TimeSpan.FromMilliseconds(5000.0));
            if (win32VirtualizationService.State != ServiceState.Running)
            {
                text = ErrorMessages.TaskFailed_ServiceNotRunning;
            }
            else
            {
                try
                {
                    m_Timer.Change(5000, -1);
                }
                catch (ObjectDisposedException)
                {
                    return;
                }
            }
        }
        catch (ServerConnectionException)
        {
            text = ErrorMessages.TaskFailed_NotConnected;
        }
        catch (ServerCallFailedException)
        {
            text = ErrorMessages.TaskFailed_NotConnected;
        }
        catch (Exception ex4)
        {
            VMTrace.TraceError("Unknown error testing for task connectedness!", ex4);
            text = ErrorMessages.TaskFailed_NotConnected;
        }
        if (text == null)
        {
            return;
        }
        VMTrace.TraceWarning("The VMMS service has either stopped or we cannot reach it. Explicitly failing all tasks on this server to prevent an infinite wait.");
        lock (m_RegisteredTasks)
        {
            foreach (VMTaskView registeredTask in m_RegisteredTasks)
            {
                try
                {
                    registeredTask.InformServerDisconnected(text);
                }
                catch (Exception ex5)
                {
                    VMTrace.TraceError("Error updating task for server disconnected.", ex5);
                }
            }
            m_RegisteredTasks.Clear();
            Dispose();
        }
        RemoveTesterInstance(this);
    }

    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is called via contracts.")]
    private void ObjectInvariant()
    {
    }
}
