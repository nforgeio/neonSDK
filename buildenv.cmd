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

REM Set NF_REPOS to the parent directory holding the NEONFORGE repositories.

pushd "%NF_ROOT%\.."
set NF_REPOS=%cd%
popd 

REM We need to capture the user's GitHub username and email address:

echo.
set /p GITHUB_USERNAME="Enter your GitHub username: "

echo.
set /p GITHUB_EMAIL="Enter the email to be included in GitHub commits: "

REM Ask the developer if they're a maintainer and set NF_MAINTAINER if they say yes.

:maintainerPrompt

echo.
set /P "IS_MAINTAINER=Are you a NEONFORGE maintainer? (y/n): "

if "%IS_MAINTAINER%"=="y" (
    set NF_MAINTAINER=1
) else if "%IS_MAINTAINER%"=="Y" (
    set NF_MAINTAINER=1
) else if "%IS_MAINTAINER%"=="n" (
    set NF_MAINTAINER=
) else if "%IS_MAINTAINER%"=="N" (
    set NF_MAINTAINER=
) else (
    echo.
    echo "*** ERROR: You must answer with: Y or N."
    echo.
    goto maintainerPrompt
)

REM Ask maintainers for their NEONFORGE Office 365 username.

if "%NF_MAINTAINER%"=="1" (
    echo.
    set /p NC_USER="Enter your NEONFORGE Office 365 username: "
    setx NC_USER "%NC_USER%" /M > nul
)

REM Ask the developer if they're using preview Visual Studio.

:previewVSPrompt

echo.
set /P "IS_VS_PREVIEW=Are you a using a PREVIEW version of Visual Studio? (y/n): "

if "%IS_VS_PREVIEW%"=="y" (
    set IS_VS_PREVIEW=1
) else if "%IS_VS_PREVIEW%"=="Y" (
    set IS_VS_PREVIEW=1
) else if "%IS_VS_PREVIEW%"=="n" (
    set IS_VS_PREVIEW=0
) else if "%IS_VS_PREVIEW%"=="N" (
    set IS_VS_PREVIEW=0
) else (
    echo.
    echo "*** ERROR: You must answer with: Y or N."
    echo.
    goto previewVSPrompt
)

if "%IS_VS_PREVIEW%"=="1" (
    set VS_EDITION=Preview
) else (
    set VS_EDITION=Community
)

REM Ask the developer if they have Telerik JustMock installed and
REM set the JUSTMOCK_ENABLED environment variable if they do.

:justMockPrompt

echo.
set /P "HAS_JUSTMOCK=Do you have Telerik JustMock installed? (y/n): "

if "%HAS_JUSTMOCK%"=="y" (
    set HAS_JUSTMOCK=1
) else if "%HAS_JUSTMOCK%"=="Y" (
    set HAS_JUSTMOCK=1
) else if "%HAS_JUSTMOCK%"=="n" (
    set HAS_JUSTMOCK=0
) else if "%HAS_JUSTMOCK%"=="N" (
    set HAS_JUSTMOCK=0
) else (
    echo.
    echo "*** ERROR: You must answer with: Y or N."
    echo.
    goto justMockPrompt
)

if "%HAS_JUSTMOCK%"=="1" (
    setx JUSTMOCK_ENABLED "%HAS_JUSTMOCK%" /M > nul
)

REM Get on with configuration.

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
set MSBUILDPATH=C:\Program Files\Microsoft Visual Studio\2022\%VS_EDITION%\Msbuild\Current\Bin\MSBuild.exe
set NEON_CLUSTER_TESTING=1

REM Persist the environment variables.

setx GITHUB_USERNAME "%GITHUB_USERNAME%" /M       > nul
setx GITHUB_EMAIL "%GITHUB_EMAIL%" /M             > nul
setx NF_MAINTAINER "%NF_MAINTAINER%" /M           > nul              
setx NF_REPOS "%NF_REPOS%" /M                     > nul
setx NF_ROOT "%NF_ROOT%" /M                       > nul
setx NF_TOOLBIN "%NF_TOOLBIN%" /M                 > nul
setx NF_BUILD "%NF_BUILD%" /M                     > nul
setx NF_CACHE "%NF_CACHE%" /M                     > nul
setx NF_SNIPPETS "%NF_SNIPPETS%" /M               > nul
setx NF_TEST "%NF_TEST%" /M                       > nul
setx NF_TEMP "%NF_TEMP%" /M                       > nul
setx NF_SAMPLES_CADENCE "%NF_SAMPLES_CADENCE%" /M > nul

if "%NF_MAINTAINER%"=="1" (
    setx NC_USER "%NC_USER%" /M > nul
)

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
REM attempt to add both locations to the path and [%NF_TOOLBIN%\pathtool] is
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

REM Perform additional implementation via Powershell.

pwsh -File "%NF_ROOT%\buildenv.ps1"

:done
echo.
echo ============================================================================================
echo * Be sure to close and reopen Visual Studio and any command windows to pick up the changes *
echo ============================================================================================
pause
