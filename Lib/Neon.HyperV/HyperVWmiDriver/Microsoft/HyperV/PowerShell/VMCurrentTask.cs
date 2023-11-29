using System;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMCurrentTask : IDisposable
{
    private readonly IVMTask m_Task;

    private volatile bool m_Disposed;

    public string Name => m_Task.Name;

    public int PercentComplete => m_Task.PercentComplete;

    internal VMCurrentTask(IVMTask task)
    {
        m_Task = task;
    }

    ~VMCurrentTask()
    {
        Dispose(disposing: true);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!m_Disposed && disposing)
        {
            if (m_Task != null)
            {
                m_Task.Dispose();
            }
            m_Disposed = true;
        }
    }
}
