using System;
using System.Security.Principal;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMConnectAce
{
    public string UserName { get; private set; }

    public SecurityIdentifier UserId { get; private set; }

    public string VMName { get; private set; }

    public Guid VMId { get; private set; }

    public string ComputerName { get; private set; }

    public VMConnectAceAccessType Access { get; private set; }

    internal VMConnectAce(VirtualMachineBase vm, IInteractiveSessionAccess access)
    {
        SetUserNameAndSid(this, access.Trustee);
        VMName = vm.Name;
        VMId = vm.Id;
        ComputerName = vm.Server.UserSpecifiedName;
        Access = (VMConnectAceAccessType)access.AccessType;
    }

    internal VMConnectAce(VirtualMachineBase vm, string userName)
    {
        SetUserNameAndSid(this, userName);
        VMName = vm.Name;
        VMId = vm.Id;
        ComputerName = vm.Server.UserSpecifiedName;
        Access = VMConnectAceAccessType.Allowed;
    }

    private static void SetUserNameAndSid(VMConnectAce ace, string userNameOrSid)
    {
        try
        {
            SecurityIdentifier securityIdentifier2 = (ace.UserId = new SecurityIdentifier(userNameOrSid));
            try
            {
                ace.UserName = securityIdentifier2.Translate(typeof(NTAccount)).Value;
            }
            catch (IdentityNotMappedException)
            {
                ace.UserName = userNameOrSid;
            }
            catch (SystemException)
            {
                ace.UserName = userNameOrSid;
            }
        }
        catch (ArgumentException)
        {
            ace.UserName = userNameOrSid;
            try
            {
                NTAccount nTAccount = new NTAccount(userNameOrSid);
                ace.UserId = (SecurityIdentifier)nTAccount.Translate(typeof(SecurityIdentifier));
            }
            catch (IdentityNotMappedException)
            {
            }
        }
    }
}
