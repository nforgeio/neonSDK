namespace Microsoft.Virtualization.Client.Management;

internal interface IViewFactory
{
	T CreateView<T>(IProxy proxy, ObjectKey key) where T : IVirtualizationManagementObject;
}
