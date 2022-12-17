## Implementation Notes (jefflill):

At some point, I'd like to implement Hyper-V support directly via WMI rather than using
PowerShell 7+, which need to be installed on user workstations.  This increases our installer
sizes by about 100MB and also consumes about 300MB on the user disk.

I've started the port, but this is not going to be as easy as I originally thought, so I'm
going to defer this until the future after we've actually released neonKUBE.  I'm going to
go ahead and leave the in-progress WMI code here along with the unit tests but exclude these
files from the build until we have the chance to get it working.
    
Hyper-V related WMI documentation:

    https://learn.microsoft.com/en-us/windows/win32/hyperv_v2/windows-virtualization-portal

This tool is also quite handy for exploring WMI and manual testing (be sure to run this
as administrator):

    https://www.ks-soft.net/hostmon.eng/wmi/index.htm

We're using APIS within the **virtualization/v2** namespaces vs. the older **virtualization**
namespace.  I believe MSFT transition to the v2 namespace for Windows Server 2012.  V2 has 
signficant differences so be sure you're looking at the v2 documentation.

I also believe that the **System.Management** classes are really just wrappers over low-level
COM objects.  Many of these objects implement **IDisposible** so we'll need to take care to
dispose these to avoid memory leaks.  I'm implementing some helper methods and classes to make
this a bit easier to accomplish.

I've also purchased an ebook copy of **Pro Hyper-V**.  This is an older book discussing the
WMI **v1** Hyper-V classes but it's useful because it describes how to accomplish things using
WMI via PowerShell.  The newer edition describes **v2** but was published after Microsoft 
released the Hyper-V Cmdlets and so the new edition doesn't really discuss low-level WMI.

Contact me if you need to borrow a copy of the ebook.  I don't want to add this to our GitHub
repos to avoid any copyright issues.
