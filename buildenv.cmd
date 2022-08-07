@echo off
REM Configures the environment variables required to build neonSDK projects.
REM 
REM 	buildenv [ <source folder> ]
REM
REM Note that <source folder> defaults to the folder holding this
REM batch file.
REM
REM This must be [RUN AS ADMINISTRATOR].

echo ==========================================
echo * neonSDK Build Environment Configurator *
echo ==========================================

REM Default NF_ROOT to the folder holding this batch file after stripping
REM off the trailing backslash.

set NF_ROOT=%~dp0 
set NF_ROOT=%NF_ROOT:~0,-2%

if not [%1]==[] set NF_ROOT=%1

if exist %NF_ROOT%\neonSDK.sln goto goodPath
echo The [%NF_ROOT%\neonSDK.sln] file does not exist.  Please pass the path
echo to the neonSDK solution folder.
goto done

:goodPath 

REM Set NF_REPOS to the parent directory holding the neonFORGE repositories.

pushd "%NF_ROOT%\.."
set NF_REPOS=%cd%
popd 

REM Some scripts need to know the developer's GitHub username:

echo.
set /p NEON_GITHUB_USER="Enter your GitHub username: "

REM Ask the developer if they're a maintainer and set NF_MAINTAINER if they say yes.

:maintainerPrompt

set /P "IS_MAINTAINER=Are you a neonFORGE maintainer (y/n): "

if "%IS_MAINTAINER%"=="y" (
    set NF_MAINTAINER=1
    setx NF_MAINTAINER 1 /M
) else if "%IS_MAINTAINER%"=="Y" (
    set NF_MAINTAINER=1
    setx NF_MAINTAINER 1 /M
) else if "%IS_MAINTAINER%"=="n" (
    set NF_MAINTAINER=
    setx NF_MAINTAINER "" /M
) else if "%IS_MAINTAINER%"=="N" (
    set NF_MAINTAINER=
    setx NF_MAINTAINER "" /M
) else (
    echo.
    echo "*** ERROR: You must answer with Y or N."
    echo.
    goto maintainerPrompt
)

echo.
echo Configuring...
echo.

REM Configure the environment variables.

set NF_TOOLBIN=%NF_ROOT%\ToolBin
set NF_BUILD=%NF_ROOT%\Build
set NF_CACHE=%NF_ROOT%\Build-cache
set NF_SNIPPETS=%NF_ROOT%\Snippets
set NF_TEST=%NF_ROOT%\Test
set NF_TEMP=C:\Temp
set NF_SAMPLES_CADENCE=%NF_ROOT%\..\cadence-samples
set DOTNETPATH=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319
set MSBUILDPATH=C:\Program Files\Microsoft Visual Studio\2022\Community\Msbuild\Current\Bin\MSBuild.exe
set NEON_CLUSTER_TESTING=1

REM Persist the environment variables.

setx NEON_GITHUB_USER "%NEON_GITHUB_USER%" /M                 > nul
setx NF_REPOS "%NF_REPOS%" /M                                 > nul
setx NF_ROOT "%NF_ROOT%" /M                                   > nul
setx NF_TOOLBIN "%NF_TOOLBIN%" /M                             > nul
setx NF_BUILD "%NF_BUILD%" /M                                 > nul
setx NF_CACHE "%NF_CACHE%" /M                                 > nul
setx NF_SNIPPETS "%NF_SNIPPETS%" /M                           > nul
setx NF_TEST "%NF_TEST%" /M                                   > nul
setx NF_TEMP "%NF_TEMP%" /M                                   > nul
setx NF_SAMPLES_CADENCE "%NF_SAMPLES_CADENCE%" /M             > nul

setx DOTNETPATH "%DOTNETPATH%" /M                             > nul
setx MSBUILDPATH "%MSBUILDPATH%" /M                           > nul
setx DOTNET_CLI_TELEMETRY_OPTOUT 1 /M                         > nul
setx DEV_WORKSTATION 1 /M                                     > nul
setx OPENSSL_CONF "%NF_ROOT%\External\OpenSSL\openssl.cnf" /M > nul

REM Make sure required folders exist.

if not exist "%NF_TEMP%" mkdir "%NF_TEMP%"
if not exist "%NF_TOOLBIN%" mkdir "%NF_TOOLBIN%"
if not exist "%NF_BUILD%" mkdir "%NF_BUILD%"
if not exist "%NF_BUILD%\neon" mkdir "%NF_BUILD%\neon"

REM Configure the PATH.
REM
REM Note that some tools like PuTTY and 7-Zip may be installed as
REM x86 or x64 to different directories.  We'll include commands that
REM attempt to add both locations to the path and [pathtool] is
REM smart enough to only add directories that actually exist.

%NF_TOOLBIN%\pathtool -dedup -system -add "%NF_BUILD%"
%NF_TOOLBIN%\pathtool -dedup -system -add "%NF_BUILD%\neon"
%NF_TOOLBIN%\pathtool -dedup -system -add "%NF_TOOLBIN%"
%NF_TOOLBIN%\pathtool -dedup -system -add "%NF_ROOT%\External\OpenSSL"
%NF_TOOLBIN%\pathtool -dedup -system -add "%DOTNETPATH%"
%NF_TOOLBIN%\pathtool -dedup -system -add "C:\cygwin64\bin"
%NF_TOOLBIN%\pathtool -dedup -system -add "%ProgramFiles%\7-Zip"
%NF_TOOLBIN%\pathtool -dedup -system -add "%ProgramFiles(x86)%\7-Zip"
%NF_TOOLBIN%\pathtool -dedup -system -add "%ProgramFiles%\PuTTY"
%NF_TOOLBIN%\pathtool -dedup -system -add "%ProgramFiles(x86)%\PuTTY"
%NF_TOOLBIN%\pathtool -dedup -system -add "%ProgramFiles%\WinSCP"
%NF_TOOLBIN%\pathtool -dedup -system -add "%ProgramFiles(x86)%\WinSCP"

REM Perform additional implementation in via Powershell.

pwsh -File "%NF_ROOT%\buildenv.ps1"

:done
echo.
echo ============================================================================================
echo * Be sure to close and reopen Visual Studio and any command windows to pick up the changes *
echo ============================================================================================
pause
