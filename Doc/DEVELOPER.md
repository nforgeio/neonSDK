# neonSDK Developer Setup

This page describes how to get started with neonSDK development.

## Workstation Requirements

* Windows 10 Professional (64-bit) with at least 16GB RAM
* Virtualization capable workstation
* Visual Studio 2019 Edition (or better)
* Visual Studio Code

Note that the build environment currently assumes that only one Windows user will be acting as a developer on any given workstation.  Developers cannot share a machine and neonSDK only builds on Windows at this time.

## Workstation Configuration

Follow the steps below to configure a development or test workstation:

1. Make sure that Windows is **fully updated**.

2. We highly recommend that you configure Windows to display hidden files:

    * Press the **Windows key** and run **File Explorer**
    * Click the **View** tab at the top.
    * Click the **Options** icon on the right and select **Change folder and search options**.
    * Click the **View** tab in the popup dialog.
    * Select the **Show hidden files, folders, and drives** radio button.
    * Uncheck the **Hide extensions for known types** check box.

3. Some versions of Skype listen for inbound connections on ports **80** and **443**.  This will interfere with services we'll want to test locally.  You need to disable this:

    * In Skype, select the **Tools/Options** menu.
    * Select the **Advanced/Connection** tab on the left.
    * **Uncheck**: Use **port 80 and 443** for additional incoming connections.

      ![Skype Connections](Images/Developer/SkypeConnections.png?raw=true)
    * **Restart Skype**

4. Ensure that Hyper-V is installed and enabled:

    * Run the following command in a **cmd** window to verify that your workstation is capable of virtualization and that it's enabled. You're looking for output like the image below:
      ```
      systeminfo
      ```
      ![Virtualization Info](Images/Developer/virtualization.png?raw=true)

      looking for a message saying that: **A hypervisor has been detected.**

    * Press the Windows key and enter: **windows features** and press ENTER.

    * Ensure that the check boxes highlighted in red below are checked:

    ![Hyper-V Features](Images/Developer/hyper-v.png?raw=true) 

    * Reboot your machine as required.

5. Uninstall **Powershell 6x** if installed.

6. Install the latest **64-bit** production release of PowerShell 7.1.3 (or greater) from [here](https://github.com/PowerShell/PowerShell/releases) (`PowerShell-#.#.#-win.x64.msi`)

7. Enable PowerShell script execution via (in a CMD window as administrator):
    ```
    powershell Set-ExecutionPolicy -ExecutionPolicy Unrestricted -Scope CurrentUser
    ```

8. Enable **WSL2**:

    a. Open a **pwsh** console **as administrator** and execute these commands:
    ```
    dism.exe /online /enable-feature /featurename:VirtualMachinePlatform /all /norestart
    dism.exe /online /enable-feature /featurename:Microsoft-Windows-Subsystem-Linux /all /norestart
    ```

    b. Install the WSL2 Kernel update as described [here](https://aka.ms/wsl2kernel)

       NOTE: You may **skip step 6** because we're going to install our own WSL distro below.

    c. Execute these Powershell commands in **pwsh** to install **neon-ubuntu-20.04** WSL2 distro:
       ```
       Invoke-WebRequest https://neon-public.s3.us-west-2.amazonaws.com/download/neon-ubuntu-20.04.tar -OutFile $env:TEMP\neon-ubuntu-20.04.tar
       wsl --import neon-ubuntu-20.04 $env:USERPROFILE\wsl-neon-ubuntu-20.04 $env:TEMP\neon-ubuntu-20.04.tar
       Remove-Item $env:TEMP\neon-ubuntu-20.04.tar
       wsl --set-default-version 2
       ```

    d. Open **Docker Desktop** and goto **Settings/Resources/WSL Integration**, turn on integration for
       the new **neon-ubuntu-20.04** distro and click **Apply & Restart**.

9. Install **Docker for Windows (Stable)** from [here](https://www.docker.com/products/docker-desktop)

    * You'll need to create a DockerHub account if you don't already have one
    * Start Docker and click the Settings **Gear** on the right side of the title bar
    * Click **General** and check **Start Docker Desktop when you log in**
	* Start a command window and use `docker login` to login using your GitHub credentials

10. Install **Visual Studio 2022 Community 17.4+** from [here](https://visualstudio.microsoft.com/thank-you-downloading-visual-studio/?sku=Community&channel=Release&version=VS2022&source=VSLandingPage&cid=2030&passive=false)

  * Check **all workloads** on the first panel
  * Select the **Individual Components** tab, search for **Git for Windows** and check that
  * Click **Install** (and take a coffee break)
  * Apply any pending **Visual Studio updates**
  * **Close** Visual Studio to install any updates
  * **NOTE:** You need sign into Visual Studio using a Windows account (like **sally@neonforge.com** for internal developers)

11. Create a **shortcut** for Visual Studio and configure it to run as **administrator**.  To build and run neonSDK applications and services, **Visual Studio must have with elevated privileges**.

12. Install Visual Studio Code and GO:

    * Install **Visual Studio Code 64-bit** from [here](https://code.visualstudio.com/download)
    * Create a desktop shortcut for Visual Studio Code and configure it to run as **administrator**.
    * Install [Microsoft Remote Development Extension Pack](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.vscode-remote-extensionpack).

13. Download the **SysInternals utiliies** from [here](https://download.sysinternals.com/files/SysinternalsSuite.zip) and extract them to a folder on your PATH, like: **C:\Tools**.

14. Install some SDKs:

   * Install **.NET Framework 4.8 Developer Pack** from [here](https://dotnet.microsoft.com/download/thank-you/net48-developer-pack)
   * Install **.NET 5.0 SDK 5.0.408** from [here](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/sdk-5.0.403-windows-x64-installer) (.NET SDK x64 installer)
   * Install **.NET 6.0 SDK 6.0.301** from [here](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/sdk-6.0.301-windows-x64-installer) (.NET SDK x64 installer)
   * Install **.NET 6.0 SDK 6.0.402 x64** from [here](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/sdk-6.0.402-windows-x64-installer) (.NET SDK x64 installer)
   * Install **.NET 7.0.200 SDK 7.0.2 x64** from [here](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/sdk-7.0.200-windows-x64-installer) (.NET SDK x64 installer)
   
15. **Visual Studio:** Enable preview .NET SDKs:
    * Open Visual Studio
    * Goto: **Tools/Options/Environment/Preview Features** (if present)
    * Check: **Use previews of the .NET SDK (requires restart)
    * Restart all Visual Studio instances

16. **Clone** the related neonSDK repos to the same parent directory as **neonSDK** using the repo names for the folders:

    * https://github.com/nforgeio/neonSDK.git
    * https://github.com/nforgeio/documentation.git

    You can do this manually or use the CMD script below: 

    ```
    cd "%NF_ROOT%\.."
    mkdir neonSDK
    git clone https://github.com/nforgeio/neonSDK.git

    cd "%NF_ROOT%\.."
    mkdir documentation
    git clone https://github.com/nforgeio/documentation.git
    ```

17. **Clone** the https://github.com/nforgeio/neonSDK.git repository to your workstation:

    * **IMPORTANT:** All NEONFORGE related repositories must be cloned within the same parent directory and their folder names must be the same as the repo names.

18. Configure the build **environment variables**:

    * Open **File Explorer**
    * Navigate to the directory holding the cloned repository
    * **Right-click** on **buildenv.cmd** and then **Run as adminstrator**
    * Answer the questions
    * Press ENTER to close the CMD window when the script is finished
  
19. **Close** any running instances of **Visual Studio**

20. Install **7-Zip (32-bit)** (using the Windows *.msi* installer) from [here](http://www.7-zip.org/download.html)

21. Install **Cygwin - setup-x86-64.exe** (all packages and default path) from: [here](https://www.cygwin.com/setup-x86_64.exe).
    You'll need to choose a mirror and then add **C:\cygwin64\bin** to the **PATH**.

22. *Optional:* Many server components are deployed to Linux, so you’ll need terminal and file management programs.  
    Install both programs to their default directories:

    * Install **WinSCP** from [here](http://winscp.net/eng/download.php) (I typically use the "Explorer" interface)
    * Install **PuTTY** from [here](https://www.chiark.greenend.org.uk/~sgtatham/putty/latest.html)
    * *Optional:* The default PuTTY color scheme sucks (dark blue on a black background doesn’t work for me).  You can update 
      the default scheme to Zenburn Light by **right-clicking** on the `$\External\zenburn-ligh-putty.reg` in **Windows Explorer** 
      and selecting **Merge**
    * WinSCP: Enable **hidden files**.  Start **WinSCP**, select **View/Preferences...**, and then click **Panels** on the left 
      and check **Show hidden files**:
    
      ![WinSCP Hidden Files](Images/Developer/WinSCPHiddenFiles.png?raw=true)

23. Confirm that the solution builds:

    * Restart **Visual Studio** as **administrator** (to pick up the new environment variables)
    * Open **$/neonSDK.sln** (where **$** is the repo root directory)
    * You may be asked to login to GitHub.  Enter your GitHub username and GITHUB PAT as the password and check the save password button
    * Click the **Install** link at the top of the solution explorer panel when there's a warning about a missing SDK.
    * Select **Build/Rebuild** Solution

24. *Optional:* Install **Notepad++** from [here](https://notepad-plus-plus.org/download)

25. *Optional:* Install **Postman** REST API tool from [here](https://www.postman.com/downloads/)

26. *Optional:* Install **Cmdr/Mini** command shell:

  * **IMPORTANT: Don't install the Full version** to avoid installing Linux command line tools that might conflict with the Cygwin tools installed earlier.
  * Download the ZIP archive from: [here](https://cmder.app/)
  * Unzip it into a new folder (like C:\Tools\Cmder**) and then ensure that this folder is in your **PATH**.
  * Create a desktop shortcut if you wish and configure it to run as administrator.
  * Consider removing the alias definitions in `$\vendor\user_aliases.cmd.default` file so that commands like `ls` will work properly.  I deleted all lines beneath the first `@echo off`.
  * Run Cmdr to complete the installation.

27. *Optional:* Install the latest version of **XCP-ng Center** from [here](https://github.com/xcp-ng/xenadmin/releases) if you'll need to manage Virtual Machines hosted on XCP-ng.

28. *Optional:* Maintainers who will be publishing releases will need to:
	
    * Install: GitHub CLI (amd64) v1.9.2 or greater from: https://github.com/cli/cli/releases

    * Download nuget.exe from https://www.nuget.org/downloads and copy it into your **C:\Tools** folder.

    * Obtain a nuget API key from a maintainer and install the key on your workstation via:
	
	  `nuget SetApiKey YOUR-KEY`

29. *Optional:* Disable **Visual Studio Complete Line Intellicode**.  I (jefflill) personally find this distracting.  This blog post agrees and describes how to disable this:

    https://dotnetcoretutorials.com/2021/11/27/turning-off-visual-studio-2022-intellicode-complete-line-intellisense/

    a. Open **Tools/Options/Intellicode (**not Intellisense!) and disable:
    b. **Show whole line completions**
    c. **Show whole line completions on new lines**

30. *Optional:* Create the **EDITOR** environment variable and point it to `C:\Program Files\Notepad++\notepad++.exe` or your favorite text editor executable.

31: *Optional:* Maintainers will need to install **AWS client version 2** from: [here](https://docs.aws.amazon.com/cli/latest/userguide/install-cliv2-windows.html)

32: *Optional:* Maintainers authorized to perform releases will need to follow the README.md instructions in the NEONCLOUD repo to configure credentials for the GitHub Releases and the Container Registry.
