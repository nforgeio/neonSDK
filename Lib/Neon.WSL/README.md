Neon.WSL
========

**INTERNAL USE ONLY:** This library includes some WSL2 related helper classes intended for internal use and is not generally supported at this time.

### Build an Ubuntu-20.04 WSL2 image

We require an Ubuntu-20.04 WSL2 image for builds and unit testing to be located at:

    https://neon-public.s3.us-west-2.amazonaws.com/build-assets/wsl/neon-ubuntu-20.04.tar

This file will be gzipped and will have [Content-Encoding=gzip] and will have 
SUDO password prompting disabled.

**Steps:** Note that you must be a maintainer to upload the image

1. Install Ubuntu 20.04 WSL from the Windows Store, setting the user 
   credentials to: **sysadmin/sysadmin0000**

   https://www.microsoft.com/store/productId/9MTTCL66CPXJ

2. Make the distro support version 2 and be the default.  In a Windows command window:

   ```
   wsl -d Ubuntu-20.04 --set-version 2
   wsl -d Ubuntu-20.04 --set-default
   ```

3. Enable **root** login for Ubuntu 20.04 WSL distros:

   ```
   ubuntu2004 config --default-user root`
   wsl -d Ubuntu-20.04   ```
   ```

4. Export the WSL2 image as a TAR file, gzip it, and then upload to S3 (in a **pwsh** comnmand window):

    ```
    . $env:NF_ROOT\Powershell\includes.ps1

    mkdir C:\Temp
    wsl --terminate Ubuntu-20.04
    del C:\temp\ubuntu-20.04.tar.gz
    del C:\Temp\ubuntu-20.04.tar
    wsl --export Ubuntu-20.04 C:\temp\ubuntu-20.04.tar
    pigz --best --blocksize 512 C:\Temp\ubuntu-20.04.tar
    ren C:\Temp\ubuntu-20.04.tar.gz ubuntu-20.04.tar

    save-tos3 C:\Temp\ubuntu-20.04.tar https://neon-public.s3.us-west-2.amazonaws.com/build-assets/wsl/neon-ubuntu-20.04.tar -gzip -publicReadAccess

    del C:\Temp\ubuntu-20.04.tar.gz
    del C:\temp\ubuntu-20.04.tar
    ```
