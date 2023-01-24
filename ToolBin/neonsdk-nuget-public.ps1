#Requires -Version 7.1.3 -RunAsAdministrator
#------------------------------------------------------------------------------
# FILE:         neon-nuget-public.ps1
# CONTRIBUTOR:  Jeff Lill
# COPYRIGHT:    Copyright © 2005-2023 by NEONFORGE LLC.  All rights reserved.
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

# Publishes RELEASE builds of the NeonForge Nuget packages to the
# local file system and public Nuget.org repositories.
#
# USAGE: pwsh -f neonsdk-nuget-public.ps1 [OPTIONS]
#
# OPTIONS:
#
#       -dirty  - Use GitHub sources for SourceLink even if local repo is dirty
#
# REMARKS:
#
# NOTE: The script writes the package publication version to:
#
#           $/build/nuget/version.txt
#

param 
(
    [switch]$dirty = $false     # use GitHub sources for SourceLink even if local repo is dirty
)

# Import the global solution include file.

. $env:NF_ROOT/Powershell/includes.ps1

# Abort if Visual Studio is running because that can lead to 
# build configuration conflicts because this script builds the
# RELEASE configuration and we normally have VS in DEBUG mode.

Ensure-VisualStudioNotRunning

# Verify that the user has the required environment variables.  These will
# be available only for maintainers and are intialized by the neonCLOUD
# [buildenv.cmd] script.

if (!(Test-Path env:NC_ROOT))
{
    "*** ERROR: This script is intended for maintainers only:"
    "           [NC_ROOT] environment variable is not defined."
    ""
    "           Maintainers should re-run the neonCLOUD [buildenv.cmd] script."

    return 1
}

# Retrieve any necessary credentials.

$nugetApiKey = Get-SecretPassword "NUGET_PUBLIC_KEY"

#------------------------------------------------------------------------------
# Sets the package version in the specified project file.

function SetVersion
{
    [CmdletBinding()]
    param (
        [Parameter(Position=0, Mandatory=$true)]
        [string]$project,
        [Parameter(Position=1, Mandatory=$true)]
        [string]$version
    )

    "$project"
	neon-build pack-version "$env:NF_ROOT\Lib\Neon.Common\Build.cs" NeonSdkVersion "$env:NF_ROOT\Lib\$project\$project.csproj"
    ThrowOnExitCode
}

#------------------------------------------------------------------------------
# Builds and publishes the project packages.

function Publish
{
    [CmdletBinding()]
    param (
        [Parameter(Position=0, Mandatory=$true)]
        [string]$project,
        [Parameter(Position=1, Mandatory=$true)]
        [string]$version
    )

    $projectPath = [io.path]::combine($env:NF_ROOT, "Lib", "$project", "$project" + ".csproj")

    # Disabling symbol packages now that we're embedding PDB files.
    #
    # dotnet pack $projectPath -c $config -p:IncludeSymbols=true -p:SymbolPackageFormat=snupkg -o "$env:NF_BUILD\nuget"

    dotnet pack $projectPath -c $config -o "$env:NF_BUILD\nuget"
    ThrowOnExitCode

    if (Test-Path "$env:NF_ROOT\Lib\$project\prerelease.txt")
    {
        $prerelease = Get-Content "$env:NF_ROOT\Lib\$project\prerelease.txt" -First 1
        $prerelease = $prerelease.Trim()

        if ($prerelease -ne "")
        {
            $prerelease = "-" + $prerelease
        }
    }
    else
    {
        $prerelease = ""
    }

	nuget push -Source nuget.org -ApiKey $nugetApiKey "$env:NF_BUILD\nuget\$project.$neonSdkVersion$prerelease.nupkg" -SkipDuplicate -Timeout 600
    ThrowOnExitCode
}

try
{
    # We're going to build the RELEASE configuration.

    $config = "Release"

    # Load the library and neonKUBE versions.

    $msbuild        = $env:MSBUILDPATH
    $nfRoot         = "$env:NF_ROOT"
    $nfSolution     = "$nfRoot\neonSDK.sln"
    $nfBuild        = "$env:NF_BUILD"
    $nfLib          = "$nfRoot\Lib"
    $nfTools        = "$nfRoot\Tools"
    $nfToolBin      = "$nfRoot\ToolBin"
    $neonSdkVersion = $(& "$nfToolBin\neon-build" read-version "$nfLib/Neon.Common/Build.cs" NeonSdkVersion)

    #--------------------------------------------------------------------------
    # SourceLink configuration:
	#
	# We're going to fail this when the current git branch is dirty 
	# and [-dirty] wasn't passed. 

    $gitDirty = IsGitDirty

    if ($gitDirty -and -not $dirty)
    {
        throw "Cannot publish nugets because the git branch is dirty.  Use the [-dirty] option to override."
    }

    $env:NEON_PUBLIC_SOURCELINK = "true"

    #------------------------------------------------------------------------------
    # Save the publish version to [$/build/nuget/version.text] so release tools can
    # determine the current release.

    [System.IO.Directory]::CreateDirectory("$nfRoot\build\nuget") | Out-Null
    [System.IO.File]::WriteAllText("$nfRoot\build\nuget\version.txt", $neonSdkVersion)

    #------------------------------------------------------------------------------
    # We need to do a release solution build to ensure that any tools or other
    # dependencies are built before we build and publish the individual packages.

    Write-Info ""
    Write-Info "********************************************************************************"
    Write-Info "***                           RESTORE PACKAGES                               ***"
    Write-Info "********************************************************************************"
    Write-Info ""

    & "$msbuild" "$nfSolution" -t:restore -verbosity:quiet

    if (-not $?)
    {
        throw "ERROR: RESTORE FAILED"
    }

    Write-Info ""
    Write-Info "********************************************************************************"
    Write-Info "***                            CLEAN SOLUTION                                ***"
    Write-Info "********************************************************************************"
    Write-Info ""

    & "$msbuild" "$nfSolution" -p:Configuration=$config -t:Clean -m -verbosity:quiet

    if (-not $?)
    {
        throw "ERROR: CLEAN FAILED"
    }

    Write-Info  ""
    Write-Info  "*******************************************************************************"
    Write-Info  "***                           BUILD SOLUTION                                ***"
    Write-Info  "*******************************************************************************"
    Write-Info  ""

    & "$msbuild" "$nfSolution" -p:Configuration=$config -restore -m -verbosity:quiet

    if (-not $?)
    {
        throw "ERROR: BUILD FAILED"
    }

    #------------------------------------------------------------------------------
    # Update the project versions.

    SetVersion Neon.Blazor              $neonSdkVersion
    SetVersion Neon.BuildInfo           $neonSdkVersion
    SetVersion Neon.Cassandra           $neonSdkVersion
    SetVersion Neon.Common              $neonSdkVersion
    SetVersion Neon.Cryptography        $neonSdkVersion
    SetVersion Neon.CSharp              $neonSdkVersion
    SetVersion Neon.Deployment          $neonSdkVersion
    SetVersion Neon.Docker              $neonSdkVersion
    SetVersion Neon.GitHub              $neonSdkVersion
    SetVersion Neon.JsonConverters      $neonSdkVersion
    SetVersion Neon.HyperV              $neonSdkVersion
    SetVersion Neon.Service             $neonSdkVersion
    SetVersion Neon.ModelGen            $neonSdkVersion
    SetVersion Neon.ModelGenerator      $neonSdkVersion
    SetVersion Neon.Nats                $neonSdkVersion
    SetVersion Neon.Postgres            $neonSdkVersion
    SetVersion Neon.SSH                 $neonSdkVersion
    SetVersion Neon.Tailwind            $neonSdkVersion
    SetVersion Neon.Web                 $neonSdkVersion
    SetVersion Neon.WinTTY              $neonSdkVersion
    SetVersion Neon.WSL                 $neonSdkVersion
    SetVersion Neon.XenServer           $neonSdkVersion
    SetVersion Neon.Xunit               $neonSdkVersion
    SetVersion Neon.Xunit.YugaByte      $neonSdkVersion
    SetVersion Neon.YugaByte            $neonSdkVersion

    #------------------------------------------------------------------------------
    # Build and publish the projects.

    Publish Neon.Blazor                 $neonSdkVersion
    Publish Neon.BuildInfo              $neonSdkVersion
    Publish Neon.Cassandra              $neonSdkVersion
    Publish Neon.Common                 $neonSdkVersion
    Publish Neon.Cryptography           $neonSdkVersion
    Publish Neon.CSharp                 $neonSdkVersion
    Publish Neon.Deployment             $neonSdkVersion
    Publish Neon.Docker                 $neonSdkVersion
    Publish Neon.GitHub                 $neonSdkVersion
    Publish Neon.JsonConverters         $neonSdkVersion
    Publish Neon.HyperV                 $neonSdkVersion
    Publish Neon.Service                $neonSdkVersion
    Publish Neon.ModelGen               $neonSdkVersion
    Publish Neon.ModelGenerator         $neonSdkVersion
    Publish Neon.Nats                   $neonSdkVersion
    Publish Neon.Postgres               $neonSdkVersion
    Publish Neon.SSH                    $neonSdkVersion
    Publish Neon.Tailwind               $neonSdkVersion
    Publish Neon.Web                    $neonSdkVersion
    Publish Neon.WinTTY                 $neonSdkVersion
    Publish Neon.WSL                    $neonSdkVersion
    Publish Neon.XenServer              $neonSdkVersion
    Publish Neon.Xunit                  $neonSdkVersion
    Publish Neon.Xunit.YugaByte         $neonSdkVersion
    Publish Neon.YugaByte               $neonSdkVersion

    #------------------------------------------------------------------------------
    # Remove all of the generated nuget files so these don't accumulate.

    Remove-Item "$env:NF_BUILD\nuget\*.nupkg"

    ""
    "** Package publication completed"
    ""
}
catch
{
    Write-Exception $_
    exit 1
}

