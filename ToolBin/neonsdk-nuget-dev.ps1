#Requires -Version 7.1.3 -RunAsAdministrator
#------------------------------------------------------------------------------
# FILE:         neonsdk-nuget-dev.ps1
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

# NOTE: This is script works only for maintainers with proper credentials.

# Publishes DEBUG builds of the NeonForge Nuget packages to the repo
# at https://nuget-dev.neoncloud.io so intermediate builds can be shared 
# by maintainers.
#
# USAGE: pwsh -f neonsdk-nuget-dev.ps1 [OPTIONS]
#
# OPTIONS:
#
#       -local          - Publishes to C:\nc-nuget-local
#       -localversion   - Use the local version number (emergeny only)
#       -drty           - Use GitHub sources for SourceLink even if local repo is dirty
#       -release        - Do a RELEASE build instead of DEBUG (the default)
#       -restore        - Just restore the CSPROJ files after cancelling publish
#
# Generally, you'll use this script without any options to publish to the private
# feed in the neonCLOUD headend using the atomic counter there to update VERSION
# numbers, especially for shared branches and especially the master branch.
#
# During development on private branches, you may wish to use a local feed
# instead, which is simply the C:\nc-nuget-local folder.  This will be much 
# faster and will reduce the accumulation of packages in our private feed.
# Use the [-local] option for this.
#
# NOTE: The script writes the package publication version to:
#
#           $/build/nuget/version.txt
#
# EMERGENCY USE:
# 
# By default, [-local] will still use the atomic versioner service in the
# headend to increment counters so that these versions monotonically increase
# across all packages published by all developers.  In an emergency such as
# when the headend services are down or when you're trying to work offline
# or on a poor connection, you can combine [-local-version] with [-local].
#
# This indicates that version numbers will be obtained from local counter files
# within the local feed folder:
#
#   C:\nc-nuget-local\neonKUBE.version.txt
#   C:\nc-nuget-local\neonSDK.version.txt
#
# These simply hold the next version as an integer on the first line for 
# each set of packages.  You'll need to manually initialize these files
# with reasonable version numbers greater than any previously published
# packages.
#
# Once the emergency is over, you must to manually update the versions
# on the headend to be greater than any version published locally by any
# developer on the team and then republish all packages using the new
# version.

# $todo(jefflill):
#
# We should update the versioner to manage the entire version, not just
# incrementing the PATCH part of the version.  This would make it easier
# to recover from emergency use of the [-local-version] switch by simply
# needing to increment the MINOR component of the versions without needing
# to coordinate with developers to determine the maximum version published.
#
#   https://app.zenhub.com/workspaces/neonforge-6042ead6ec0efa0012c5facf/issues/nforgeio/neoncloud/173

param 
(
    [switch]$local        = $false,     # publish to local file system
    [switch]$localVersion = $false,     # use a local version counter (emergency only)
    [switch]$dirty        = $false,     # use GitHub sources for SourceLink even if local repo is dirty
    [switch]$release      = $false,     # RELEASE build instead of DEBUG (the default)
    [switch]$restore      = $false      # Just restore the CSPROJ files after cancelling publish
)

# Import the global solution include file.

. $env:NF_ROOT/Powershell/includes.ps1

# Abort if Visual Studio is running when we're building release
# nuget packages because that can lead to build configuration 
# conflicts because this script builds the RELEASE configuration 
# and we normally have VS in DEBUG mode.

if ($release)
{
    Ensure-VisualStudioNotRunning
}

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

# We're going to build the DEBUG configuration by default for DEV packages so debugging will be easier.

if ($release)
{
    $config = "Release"
}
else
{
    $config = "Debug"
}

#------------------------------------------------------------------------------
# Sets the package version in the specified project file and makes a backup
# of the original project file named [$project.bak].

function SetVersion
{
    [CmdletBinding()]
    param (
        [Parameter(Position=0, Mandatory=$true)]
        [string]$project,
        [Parameter(Position=1, Mandatory=$true)]
        [string]$version
    )

    "* SetVersion: ${project}:${version}"

    $projectPath    = [io.path]::combine($env:NF_ROOT, "Lib", "$project", "$project" + ".csproj")
    $orgProjectFile = Get-Content "$projectPath" -Encoding utf8
    $regex          = [regex]'<Version>(.*)</Version>'
    $match          = $regex.Match($orgProjectFile)
    $orgVersion     = $match.Groups[1].Value
    $tmpProjectFile = $orgProjectFile.Replace("<Version>$orgVersion</Version>", "<Version>$version</Version>")

    if (!(Test-Path "$projectPath.bak"))
    {
        Copy-Item "$projectPath" "$projectPath.bak"
    }
    
    $tmpProjectFile | Out-File -FilePath "$projectPath" -Encoding utf8
}

#------------------------------------------------------------------------------
# Restores the original project version for a project.

function RestoreVersion
{
    [CmdletBinding()]
    param (
        [Parameter(Position=0, Mandatory=$true)]
        [string]$project
    )

    "* Restore: ${project}"

    $projectPath = [io.path]::combine($env:NF_ROOT, "Lib", "$project", "$project" + ".csproj")

    Copy-Item "$projectPath.bak" "$projectPath"
    Remove-Item "$projectPath.bak"
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

    $localIndicator = ""

    if ($local)
    {
        $localIndicator = " (local)"
    }

    ""
    "==============================================================================="
    "* Publishing: ${project}:${version}${localIndicator}"
    "==============================================================================="

    $projectPath = [io.path]::combine($env:NF_ROOT, "Lib", "$project", "$project" + ".csproj")

    dotnet pack $projectPath -c $config -o "$env:NF_BUILD\nuget"
    ThrowOnExitCode

    $nugetPath = "$env:NF_BUILD\nuget\$project.$version.nupkg"

    if ($local)
    {
        dotnet nuget push $nugetPath --source $env:NC_NUGET_LOCAL
        ThrowOnExitCode
    }
    else
    {
        dotnet nuget push $nugetPath --source $nugetFeedSource --api-key $nugetFeedApiKey --skip-duplicate --timeout 600
        ThrowOnExitCode
    }
}

try
{
    $msbuild     = $env:MSBUILDPATH
    $nfRoot      = "$env:NF_ROOT"
    $nfSolution  = "$nfRoot\neonSDK.sln"
    $branch      = GitBranch $nfRoot

    if ($localVersion)
    {
        $local = $true
    }

    if ($localVersion)
    {
        # EMERGENCY MODE: Use local counters.

        $nfVersionPath = [System.IO.Path]::Combine($env:NC_NUGET_LOCAL, "neonSDK.version.txt")
        $nkVersionPath = [System.IO.Path]::Combine($env:NC_NUGET_LOCAL, "neonKUBE.version.txt")

        if (![System.IO.File]::Exists("$nfVersionPath") -or ![System.IO.File]::Exists("$nfVersionPath"))
        {
            Write-Error "You'll need to manually initialize the local version files at:" -ErrorAction continue
            Write-Error ""                   -ErrorAction continue
            Write-Error "    $nfVersionPath" -ErrorAction continue
            Write-Error "    $nfVersionPath" -ErrorAction continue
            Write-Error "" -ErrorAction continue
            Write-Error "Create these files with the minor version number currently referenced" -ErrorAction continue
            Write-Error "by your local neonCLOUD solution:" -ErrorAction continue
            Write-Error "" -ErrorAction continue
            Write-Error "The easiest way to do this is to open the [neonCLOUD/Tools/neon-cli/neon-cli.csproj]" -ErrorAction continue
            Write-Error "file extract the minor version for the package references as described below:" -ErrorAction continue
            Write-Error "" -ErrorAction continue
            Write-Error "    neonSDK.version.txt:  from Neon.Common" -ErrorAction continue
            Write-Error "    neonKUBE.version.txt: from Neon.Kube" -ErrorAction continue
            Write-Error "" -ErrorAction continue
            Write-Error "NOTE: These two version numbers are currently the same (Jan 2022), but they" -ErrorAction continue
            Write-Error "      may diverge at any time and will definitely diverge after we separate " -ErrorAction continue
            Write-Error "      neonSDK and neonKUBE." -ErrorAction continue
            exit 1
        }

        $version = [int](Get-Content -TotalCount 1 $nfVersionPath).Trim()
        $version++
        [System.IO.File]::WriteAllText($nfVersionPath, $version)
        $neonSdkVersion = "10000.0.$version-dev-$branch"
    }
    else
    {
        # We're going to call the neonCLOUD nuget versioner service to atomically increment the 
        # dev package version counters for the solution and then generate the full version for
        # the packages we'll be publishing.  We'll use separate counters for the neonSDK
        # and neonKUBE packages.
        #
        # The package versions will also include the current branch appended to the preview tag
        # so a typical package version will look like:
        #
        #       10000.0.VERSION-dev-master
        #
        # where we use major version 10000 as a value that will never be exceeded by a real
        # release, VERSION is automatically incremented for every package published, [master]
        # in this case is the current branch at the time of publishing and [-dev] indicates
        # that this is a non-production release.

        # Retrieve any necessary credentials.

        $versionerKey    = Get-SecretValue "NUGET_VERSIONER_KEY" "group-devops"
        $nugetFeedName   = "nc-nuget-devfeed"
        $nugetFeedSource = "https://nuget.pkg.github.com/nforgeio/index.json"
        $nugetFeedApiKey = Get-SecretPassword "GITHUB[accesstoken]" user-$env:NC_USER

        # Get the nuget versioner API key from the environment and convert it into a base-64 string.

        $versionerKeyBase64 = [Convert]::ToBase64String(([System.Text.Encoding]::UTF8.GetBytes($versionerKey)))

        # Submit PUT requests to the versioner service, specifying the counter name.  The service will
        # atomically increment the counter and return the next value.

        $reply           = Invoke-WebRequest -Uri "$env:NC_NUGET_VERSIONER/counter/neonSDK-dev" -Method 'PUT' -Headers @{ 'Authorization' = "Bearer $versionerKeyBase64" } 
        $neonSdkVersion  = "10000.0.$reply-dev-$branch"
    }

    #------------------------------------------------------------------------------
    # Save the publish version to [$/build/nuget/version.text] so release tools can
    # determine the current release.

    [System.IO.File]::WriteAllText("$nfRoot\build\nuget\version.txt", $neonSdkVersion)

    #------------------------------------------------------------------------------
    # SourceLink configuration: We need to decide whether to set the environment variable 
    # [NEON_PUBLIC_SOURCELINK=true] to enable SourceLink references to our GitHub repos.

    $gitDirty = IsGitDirty

    if (-not $local -and $gitDirty -and -not $dirty -and -not $restore)
    {
        throw "Cannot publish nugets because the git branch is dirty.  Use the -dirty option to override."
    }

    $env:NEON_PUBLIC_SOURCELINK = "true"

    #------------------------------------------------------------------------------
    # We need to do a solution build to ensure that any tools or other dependencies 
    # are built before we build and publish the individual packages.

    if (-not $restore)
    {
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

        #----------------------------------------------------------------------
        # We need to set the version in all of the project files so that implicit 
        # package dependencies will work for external projects importing these 
        # packages.

        SetVersion Neon.Blazor                      $neonSdkVersion
        SetVersion Neon.BuildInfo                   $neonSdkVersion
        SetVersion Neon.Cassandra                   $neonSdkVersion
        SetVersion Neon.Common                      $neonSdkVersion
        SetVersion Neon.Cryptography                $neonSdkVersion
        SetVersion Neon.CSharp                      $neonSdkVersion
        SetVersion Neon.Deployment                  $neonSdkVersion
        SetVersion Neon.Docker                      $neonSdkVersion
        SetVersion Neon.GitHub                      $neonSdkVersion
        SetVersion Neon.JsonConverters              $neonSdkVersion
        SetVersion Neon.HyperV                      $neonSdkVersion
        SetVersion Neon.Service                     $neonSdkVersion
        SetVersion Neon.ModelGen                    $neonSdkVersion
        SetVersion Neon.ModelGenerator              $neonSdkVersion
        SetVersion Neon.Nats                        $neonSdkVersion
        SetVersion Neon.Postgres                    $neonSdkVersion
        SetVersion Neon.SSH                         $neonSdkVersion
        SetVersion Neon.Tailwind                    $neonSdkVersion
        SetVersion Neon.Web                         $neonSdkVersion
        SetVersion Neon.WinTTY                      $neonSdkVersion
        SetVersion Neon.WSL                         $neonSdkVersion
        SetVersion Neon.XenServer                   $neonSdkVersion
        SetVersion Neon.Xunit                       $neonSdkVersion
        SetVersion Neon.Xunit.YugaByte              $neonSdkVersion
        SetVersion Neon.YugaByte                    $neonSdkVersion

        # Build and publish the projects.

        Publish Neon.Blazor                         $neonSdkVersion
        Publish Neon.BuildInfo                      $neonSdkVersion
        Publish Neon.Cassandra                      $neonSdkVersion
        Publish Neon.Common                         $neonSdkVersion
        Publish Neon.Cryptography                   $neonSdkVersion
        Publish Neon.CSharp                         $neonSdkVersion
        Publish Neon.Deployment                     $neonSdkVersion
        Publish Neon.Docker                         $neonSdkVersion
        Publish Neon.GitHUb                         $neonSdkVersion
        Publish Neon.JsonConverters                 $neonSdkVersion
        Publish Neon.HyperV                         $neonSdkVersion
        Publish Neon.Service                        $neonSdkVersion
        Publish Neon.ModelGen                       $neonSdkVersion
        Publish Neon.ModelGenerator                 $neonSdkVersion
        Publish Neon.Nats                           $neonSdkVersion
        Publish Neon.Postgres                       $neonSdkVersion
        Publish Neon.SSH                            $neonSdkVersion
        Publish Neon.Tailwind                       $neonSdkVersion
        Publish Neon.Web                            $neonSdkVersion
        Publish Neon.WinTTY                         $neonSdkVersion
        Publish Neon.WSL                            $neonSdkVersion
        Publish Neon.XenServer                      $neonSdkVersion
        Publish Neon.Xunit                          $neonSdkVersion
        Publish Neon.Xunit.YugaByte                 $neonSdkVersion
        Publish Neon.YugaByte                       $neonSdkVersion
    }

    #--------------------------------------------------------------------------
    # Restore the project versions

    RestoreVersion Neon.Blazor
    RestoreVersion Neon.BuildInfo
    RestoreVersion Neon.Cassandra
    RestoreVersion Neon.Common
    RestoreVersion Neon.Cryptography
    RestoreVersion Neon.CSharp
    RestoreVersion Neon.Deployment
    RestoreVersion Neon.Docker
    RestoreVersion Neon.GitHub
    RestoreVersion Neon.JsonConverters
    RestoreVersion Neon.HyperV
    RestoreVersion Neon.Service
    RestoreVersion Neon.ModelGen
    RestoreVersion Neon.ModelGenerator
    RestoreVersion Neon.Nats
    RestoreVersion Neon.Postgres
    RestoreVersion Neon.SSH
    RestoreVersion Neon.Tailwind
    RestoreVersion Neon.Web
    RestoreVersion Neon.WinTTY
    RestoreVersion Neon.WSL
    RestoreVersion Neon.XenServer
    RestoreVersion Neon.Xunit
    RestoreVersion Neon.Xunit.YugaByte
    RestoreVersion Neon.YugaByte

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

