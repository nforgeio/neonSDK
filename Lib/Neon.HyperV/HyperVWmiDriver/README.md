Copyright © Microsoft Corporation.

This folder holds the decompiled source files from the Microsoft HyperV PowerShell 
cmdlets.  We're embedding the Hyper-V cmdlet code in this library so we can call 
these cmdlets directly rather than having to to crank up PowerShell for each call.
We're assuming that doing this is kosher, since Microsoft details how to obtain
PowerShell cmdlet source code here:

    https://learn.microsoft.com/en-us/archive/blogs/luisdem/get-the-source-code-of-the-powershell-cmdlets

Here's how we did this:

**IMPORTANT"** 

**Use ILSpy v8.0 Preview or later**.  Earlier versions clip output file names to 
30 characters and some of the decompiled file names will be longer than this,
and to make things worse, some files start with the same 30 characters, so
some files will be overwritten and be effectively lost.

1. Use **ILSpy** to save the DLL source and project files.

   **IMPORTANT:** Goto **ViewOptions...** and at the bottom, check
                  **Use nested directories for namespaces**.

   Decompile these assemblies from the GAC (right-click and **Save Code...**:

   * Microsoft.HyperV.PowerShell.Cmdlets
   * Microsoft.HyperV.PowerShell.Objects
   * Microsoft.Virtualization.Client.Common.Types
   * Microsoft.Virtualization.Client.Management

   **DO NOT** decompile this assembly (to avoid duplicate files):

   * Microsoft.HyperV.PowerShell

2. Fix ambigous **ErrorMessages** references.

3. Change any **public** types to **internal**.

4. Add the ***.resx** files.

5. Add **using** statements to resolve ambiguous type references (as required):

   ```
   using AllowNull     = System.Management.Automation.AllowNullAttribute;
   using ErrorMessages = Microsoft.HyperV.PowerShell.Common.ErrorMessages;
   ```

6. Rename the broken `StackFrame(fNeedFileInfo: true)` parameter to `needFileInfo`.

7. Modify any `new ResourceManager()` calls to prefix the resource name with
   `WmiPortHelper.ResourceRoot` so these can be loaded from the new assembly:

   Change:

   ```
   new ResourceManager("...");
   ```

   To:

   ```
   new ResourceManager(WmiHelper.GetResourceName("..."));
   ```

8. Edit **VirtualizationCmdletBase.cs** to have `CurrentFileSystemLocation` return the
   current directory:

   ```
   //protected string CurrentFileSystemLocation => base.SessionState.Path.CurrentFileSystemLocation.Path;
   protected string CurrentFileSystemLocation => Environment.CurrentDirectory;
  ```
