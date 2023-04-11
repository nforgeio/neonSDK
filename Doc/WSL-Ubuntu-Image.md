# Maintainer: Create Ubuntu 20.04 WSL image

These are the instructions for creating and publishing the Ubuntu 20.04 TAR 
image used by NEONFORGE related repos for building components, etc.  This may
need to be recreated from time-to-time.

1. Be sure that WSL2 is installed and configured
2. Install Ubuntu 20.04 WSL from the Windows Store: https://www.microsoft.com/store/productId/9MTTCL66CPXJ
3. Configure root login by default and then login:
   ```
   ubuntu2004 config --default-user root
   wsl -d Ubuntu-20.04
   ```
4. Update the distro via:
   ```
   apt-get update
   apt-get dist-upgrade -y
   exit
   ```
5. Run this in PowerShell (pwsh.exe) to export the distro TAR file and upload it to S3:
   ```
   . $env:NF_ROOT\Powershell\includes.ps1
   Import-AwsCliCredentials
   wsl --export Ubuntu-20.04 neon-ubuntu-20.04.tar
   Save-ToS3 neon-ubuntu-20.04.tar s3://neon-public/download/neon-ubuntu-20.04.tar -publicReadAccess $true
   Remove-Item neon-ubuntu-20.04.tar
   ```
