#Requires -Version 7.1.3 -RunAsAdministrator
#------------------------------------------------------------------------------
# FILE:         build.ps1
# CONTRIBUTOR:  Jeff Lill
# COPYRIGHT:    Copyright © 2005-2024 by NEONFORGE LLC.  All rights reserved.
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

# Builds the container image.

param 
(
	[parameter(Mandatory=$true, Position=1)][string] $registry,
	[parameter(Mandatory=$true, Position=2)][string] $tag,
	[parameter(Mandatory=$true, Position=3)][string] $config
)

$appname           = "test-api"
$organization      = SdkRegistryOrg
$base_organization = KubeBaseRegistryOrg
$branch            = GitBranch $env:NF_ROOT

try
{
    # Build and publish the app to a local [bin] folder.

    DeleteFolder bin

    mkdir bin | Out-Null
    ThrowOnExitCode

    dotnet publish "$nfServices\$appname\$appname.csproj" -c $config -o "$pwd\bin" 
    ThrowOnExitCode

    # Split the build binaries into [__app] (application) and [__dep] dependency subfolders
    # so we can tune the image layers.

    core-layers $appname "$pwd\bin" 
    ThrowOnExitCode

    # Build the image.

    $baseImage = Get-DotnetBaseImage "$nfRoot\global.json"

    Invoke-CaptureStreams "docker build -t ${registry}:${tag} --build-arg `"APPNAME=$appname`" --build-arg `"ORGANIZATION=$organization`" --build-arg `"BASE_ORGANIZATION=$base_organization`" --build-arg `"CLUSTER_VERSION=neonsdk-$neonSDK_Version`" --build-arg `"BASE_IMAGE=$baseImage`" --build-arg `"BRANCH=$branch`" ." -interleave | Out-Null
}
finally
{
    # Clean up

    DeleteFolder bin
}
