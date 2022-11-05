#Requires -Version 7.1.3 -RunAsAdministrator
#------------------------------------------------------------------------------
# FILE:         build.ps1
# CONTRIBUTOR:  Marcus Bowyer
# COPYRIGHT:    Copyright © 2005-2022 by NEONFORGE LLC.  All rights reserved.
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

# Build the image.

$result = Invoke-CaptureStreams "docker build -t ${registry}:${version} --build-arg `"APPNAME=$appname`" --build-arg VERSION=$version ." -interleave

# Clean up

DeleteFolder bin
