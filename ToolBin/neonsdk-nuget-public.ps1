#Requires -Version 7.1.3 -RunAsAdministrator
#------------------------------------------------------------------------------
# FILE:         neon-nuget-public.ps1
# CONTRIBUTOR:  Jeff Lill
# COPYRIGHT:    Copyright (c) 2005-2022 by neonFORGE LLC.  All rights reserved.
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

# Import the global solution include file.

. $env:NF_ROOT/Powershell/includes.ps1

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

# This needs to run with elevated privileges.

Request-AdminPermissions

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

	dotnet pack $projectPath -c $config -p:IncludeSymbols=true -p:SymbolPackageFormat=snupkg -o "$env:NF_BUILD\nuget"
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

# We need to do a release solution build to ensure that any tools or other
# dependencies are built before we build and publish the individual packages.

Write-Info ""
Write-Info "********************************************************************************"
Write-Info "***                            CLEAN SOLUTION                                ***"
Write-Info "********************************************************************************"
Write-Info ""

& neon-build clean-generated-cs $nfRoot
& "$msbuild" "$nfSolution" -p:Configuration=$config -t:Clean -m -verbosity:quiet

if (-not $?)
{
    throw "ERROR: CLEAN FAILED"
}

Write-Info ""
Write-Info "********************************************************************************"
Write-Info "***                           RESTORE PACKAGES                               ***"
Write-Info "********************************************************************************"
Write-Info ""

& "$msbuild" "$nfSolution" -t:restore -verbosity:quiet

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

# Update the project versions.

SetVersion Neon.Cadence             $neonSdkVersion
SetVersion Neon.Cassandra           $neonSdkVersion
SetVersion Neon.Common              $neonSdkVersion
SetVersion Neon.Couchbase           $neonSdkVersion
SetVersion Neon.Cryptography        $neonSdkVersion
SetVersion Neon.CSharp              $neonSdkVersion
SetVersion Neon.Deployment          $neonSdkVersion
SetVersion Neon.Docker              $neonSdkVersion
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
SetVersion Neon.Xunit.Cadence       $neonSdkVersion
SetVersion Neon.Xunit.Couchbase     $neonSdkVersion
SetVersion Neon.Xunit.YugaByte      $neonSdkVersion
SetVersion Neon.YugaByte            $neonSdkVersion

# Build and publish the projects.

Publish Neon.Cadence                $neonSdkVersion
Publish Neon.Cassandra              $neonSdkVersion
Publish Neon.Common                 $neonSdkVersion
Publish Neon.Couchbase              $neonSdkVersion
Publish Neon.Cryptography           $neonSdkVersion
Publish Neon.CSharp                 $neonSdkVersion
Publish Neon.Deployment             $neonSdkVersion
Publish Neon.Docker                 $neonSdkVersion
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
Publish Neon.Xunit.Cadence          $neonSdkVersion
Publish Neon.Xunit.Couchbase        $neonSdkVersion
Publish Neon.Xunit.YugaByte         $neonSdkVersion
Publish Neon.YugaByte               $neonSdkVersion

# Remove all of the generated nuget files so these don't accumulate.

Remove-Item "$env:NF_BUILD\nuget\*"

""
"** Package publication completed"
""

