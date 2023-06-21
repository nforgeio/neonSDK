using System;
using System.Threading;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell.ExtensionMethods;

internal static class OperationWatcherExtensions
{
    internal static void PerformOperation(this IOperationWatcher watcher, Func<IVMTask> startTaskMethod, Action<IVMTask> endTaskMethod, string taskDescription, VirtualizationObject targetObject)
    {
        watcher.PerformOperationWithReturn(startTaskMethod, ConvertActionToFunction(endTaskMethod), taskDescription, targetObject);
    }

    internal static void PerformPut(this IOperationWatcher watcher, IPutableAsync putableObject, string putTaskDescription, VirtualizationObject targetObject)
    {
        watcher.PerformOperation(putableObject.BeginPut, putableObject.EndPut, putTaskDescription, targetObject);
    }

    internal static void PerformDelete(this IOperationWatcher watcher, IDeleteableAsync deleteableObject, string removeTaskDescription, VirtualizationObject targetObject)
    {
        watcher.PerformOperation(deleteableObject.BeginDelete, deleteableObject.EndDelete, removeTaskDescription, targetObject);
    }

    internal static T PerformOperationWithReturn<T>(this IOperationWatcher watcher, Func<IVMTask> startTaskMethod, Func<IVMTask, T> endTaskMethod, string taskDescription, VirtualizationObject targetObject)
    {
        using IVMTask iVMTask = startTaskMethod();
        WatchableTask.MonitorTask(iVMTask, taskDescription, watcher, targetObject);
        return endTaskMethod(iVMTask);
    }

    private static Func<T, bool> ConvertActionToFunction<T>(Action<T> action)
    {
        return delegate(T x)
        {
            action(x);
            return true;
        };
    }

    internal static bool WaitOnCondition(this IOperationWatcher watcher, Func<bool> predicate, ManualResetEventSlim waitHandle, DateTime expireTime)
    {
        while (!waitHandle.IsSet)
        {
            bool flag = predicate();
            if (flag || DateTime.Now >= expireTime)
            {
                return flag;
            }
            waitHandle.Wait(500);
        }
        return false;
    }
}
