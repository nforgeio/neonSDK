#Requires -Version 7.1.3 -RunAsAdministrator
#------------------------------------------------------------------------------
# FILE:         build.ps1
# CONTRIBUTOR:  Jeff Lill
# COPYRIGHT:    Copyright (c) 2005-2022 by neonFORGE LLC.  All rights reserved.
#
# Builds the Neon [test-cadence] image.
#
# USAGE: pwsh -f build.ps1 REGISTRY VERSION TAG

param 
(
	[parameter(Mandatory=$True,Position=1)][string] $registry,
	[parameter(Mandatory=$True,Position=2)][string] $tag
)

$appname           = "test-cadence"
$organization      = SdkRegistryOrg
$base_organization = KubeBaseRegistryOrg
$branch            = GitBranch $env:NF_ROOT

# Build and publish the app to a local [bin] folder.

DeleteFolder bin

$result = mkdir bin
ThrowOnExitCode

dotnet publish "$nfServices\$appname\$appname.csproj" -c Release -o "$pwd\bin" 
ThrowOnExitCode

# Split the build binaries into [__app] (application) and [__dep] dependency subfolders
# so we can tune the image layers.

core-layers $appname "$pwd\bin" 
ThrowOnExitCode

# Build the image.

$result = Invoke-CaptureStreams "docker build -t ${registry}:${tag} --build-arg `"APPNAME=$appname`" --build-arg `"ORGANIZATION=$organization`" --build-arg `"BASE_ORGANIZATION=$base_organization`" --build-arg `"CLUSTER_VERSION=neonsdk-$neonSDK_Version`" --build-arg `"BRANCH=$branch`" ." -interleave

# Clean up

DeleteFolder bin
