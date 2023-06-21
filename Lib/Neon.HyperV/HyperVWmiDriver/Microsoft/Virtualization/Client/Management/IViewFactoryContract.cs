namespace Microsoft.Virtualization.Client.Management;

internal abstract class IViewFactoryContract : IViewFactory
{
    T IViewFactory.CreateView<T>(IProxy proxy, ObjectKey key)
    {
        return default(T);
    }
}
