using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Virtualization.Client.Management;

internal interface IHostComputerSystemBase : IVirtualizationManagementObject
{
    IVMService VirtualizationService { get; }

    [SuppressMessage("Microsoft.Design", "CA1009")]
    event VMComputersSystemCreatedEventHandler VMComputerSystemCreated;

    [SuppressMessage("Microsoft.Design", "CA1009")]
    event VMVirtualizationTaskCreatedEventHandler VMVirtualizationTaskCreated;

    IList<ISummaryInformation> GetAllSummaryInformation(SummaryInformationRequest requestedInformation);

    IList<ISummaryInformation> GetSummaryInformation(IList<IVMComputerSystem> vmList, SummaryInformationRequest requestedInformation);
}
