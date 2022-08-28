#Requires -Version 7.1.3 -RunAsAdministrator
#------------------------------------------------------------------------------
# FILE:         build.ps1
# CONTRIBUTOR:  Marcus Bowyer
# COPYRIGHT:    Copyright (c) 2005-2022 by neonFORGE LLC.  All rights reserved.
#
# Builds the Neon [aspnet] image.
#
# USAGE: pwsh -f build.ps1 REGISTRY VERSION TAG

param 
(
	[parameter(Mandatory=$True,Position=1)][string] $registry,
	[parameter(Mandatory=$True,Position=2)][string] $version
)

$appname      = "aspnet"
$organization = SdkRegistryOrg

# Copy the common scripts.

DeleteFolder _common

mkdir _common
copy ..\_common\*.* .\_common

# Build the image.

$result = Invoke-CaptureStreams "docker build -t ${registry}:${version} --build-arg `"APPNAME=$appname`" --build-arg VERSION=$version ." -interleave

# Clean up

DeleteFolder bin
DeleteFolder _common
