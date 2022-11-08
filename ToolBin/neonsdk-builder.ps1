#Requires -Version 7.1.3 -RunAsAdministrator
#------------------------------------------------------------------------------
# FILE:         neonsdk-builder.ps1
# CONTRIBUTOR:  Jeff Lill
# COPYRIGHT:    Copyright © 2005-2022 by NEONFORGE LLC.  All rights reserved.
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

# Performs a clean build of the neonSDK solution and publishes binaries
# to the [$/build] folder.
#
# USAGE: pwsh -f neonsdk-builder.ps1 [OPTIONS]
#
# OPTIONS:
#
#       -codedoc      - Builds the code documentation
#       -all          - Builds with all of the options above

param 
(
    [switch]$codedoc = $false,
    [switch]$all     = $false,
    [switch]$debug   = $false   # Optionally specify DEBUG build config
)

#------------------------------------------------------------------------------
# $todo(jefflill):

if ($codedoc)
{
    Write-Error " "
    Write-Error "ERROR: Code documentation builds are temporarily disabled until we"
    Write-Error "       port to DocFX.  SHFB doesn't work for multi-targeted projects."
    Write-Error " "
    Write-Error "       https://github.com/nforgeio/neonKUBE/issues/1206"
    Write-Error " "
    exit 1
}

#------------------------------------------------------------------------------

# Import the global solution include file.

. $env:NF_ROOT/Powershell/includes.ps1

# Abort if Visual Studio is running because that can cause [pubcore] to
# fail due to locked files.

# $note(jefflill): 
#
# We don't currently need this check but I'm leaving it here commented
# out to make it easier to revive in the future, if necessary.

# Ensure-VisualStudioNotRunning

# Initialize

if ($all)
{
    # $codedoc = $true
}

if ($debug)
{
    $config = "Debug"
}
else
{
    $config = "Release"
}

$msbuild     = $env:MSBUILDPATH
$nfRoot      = $env:NF_ROOT
$nfSolution  = "$nfRoot\neonSDK.sln"
$nfBuild     = "$env:NF_BUILD"
$nfLib       = "$nfRoot\Lib"
$nfTools     = "$nfRoot\Tools"
$nfToolBin   = "$nfRoot\ToolBin"
$buildConfig = "-p:Configuration=$config"
$env:PATH   += ";$nfBuild"

$neonSdkVersion = $(& "$nfToolBin\neon-build" read-version "$nfLib\Neon.Common\Build.cs" NeonSdkVersion)
ThrowOnExitCode

#------------------------------------------------------------------------------
# Perform the operation.

Push-Cwd $nfRoot | Out-Null

$verbosity = "minimal"

try
{
    # Build the solution.

    if (-not $nobuild)
    {
        # We see somewhat random build problems when Visual Studio has the solution open,
        # so have the user close Visual Studio instances first.

        # $note(jefflill): 
        #
        # We don't currently need this check but I'm leaving it here commented
        # out to make it easier to revive in the future, if necessary.

        # Ensure-VisualStudioNotRunning

        # Clear the NF_BUILD folder and delete any [bin] or [obj] folders
        # to be really sure we're doing a clean build.  I've run into 
        # situations where I've upgraded SDKs or Visual Studio and Files
        # left over from previous builds that caused build trouble.

        & $nfToolBin\neon-build clean "$nfRoot"
        ThrowOnExitCode

        # Clean and build the solution.

        Write-Info ""
        Write-Info "*******************************************************************************"
        Write-Info "***                           RESTORE PACKAGES                              ***"
        Write-Info "*******************************************************************************"
        Write-Info ""

        & "$msbuild" "$nfSolution" -t:restore -verbosity:minimal

        if (-not $?)
        {
            throw "ERROR: RESTORE FAILED"
        }

        Write-Info ""
        Write-Info "*******************************************************************************"
        Write-Info "***                           CLEAN SOLUTION                                ***"
        Write-Info "*******************************************************************************"
        Write-Info ""

        & "$msbuild" "$nfSolution" $buildConfig -t:Clean -m -verbosity:$verbosity

        if (-not $?)
        {
            throw "ERROR: CLEAN FAILED"
        }

        Write-Info ""
        Write-Info "*******************************************************************************"
        Write-Info "***                           BUILD SOLUTION                                ***"
        Write-Info "*******************************************************************************"
        Write-Info ""

        & "$msbuild" "$nfSolution" $buildConfig -restore -m -verbosity:$verbosity

        if (-not $?)
        {
            throw "ERROR: BUILD FAILED"
        }
    }

    # Build the code documentation if requested.

    if ($codedoc)
    {
        Write-Info ""
        Write-Info "**********************************************************************"
        Write-Info "***                      CODE DOCUMENTATION                        ***"
        Write-Info "**********************************************************************"
        Write-Info ""

        # Remove some pesky aliases:

        del alias:rm
        del alias:cp
        del alias:mv

        if (-not $?)
        {
            throw "ERROR: Cannot remove: $nfBuild\codedoc"
        }

        & "$msbuild" "$nfSolution" -p:Configuration=CodeDoc -restore -m -verbosity:$verbosity

        if (-not $?)
        {
            throw "ERROR: BUILD FAILED"
        }

        # Copy the CHM file to a more convenient place for adding to the GitHub release
        # and generate the SHA256 for it.

        $nfDocOutput = "$nfroot\Websites\CodeDoc\bin\CodeDoc"

        & cp "$nfDocOutput\neon.chm" "$nfbuild"
        ThrowOnExitCode

        ""
        "Generating neon.chm SHA256..."
	    ""

        & cat "$nfBuild\neon.chm" | openssl dgst -sha256 -binary | neon-build hexdump > "$nfBuild\neon.chm.sha256.txt"
        ThrowOnExitCode

        # Move the documentation build output.
	
        & rm -r --force "$nfBuild\codedoc"
        ThrowOnExitCode

        & mv "$nfDocOutput" "$nfBuild\codedoc"
        ThrowOnExitCode

        # Munge the SHFB generated documentation site:
        #
        #   1. Insert the Google Analytics [gtag.js] scripts
        #   2. Munge and relocate HTML files for better site
        #      layout and friendlier permalinks.

        ""
        "Tweaking Layout and Enabling Google Analytics..."
	    ""

        & neon-build shfb --gtag="$nfroot\Websites\CodeDoc\gtag.js" --styles="$nfRoot\WebSites\CodeDoc\styles" "$nfRoot\WebSites\CodeDoc" "$nfBuild\codedoc"
        ThrowOnExitCode
    }
}
catch
{
    Write-Exception $_
    exit 1
}
finally
{
    Pop-Cwd | Out-Null
}
