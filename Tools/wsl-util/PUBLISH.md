# Instructions

You'll need to publish the **wsl-util** project to **$\neonSDK\Toolbin** after making any changes.
Not that publishing within Visual Studio doesn't seem to work any more for some reason, so you'll
need to publish via this script instead:

```
cd "%NF_ROOT%\Tools\wsl-util"
if exist "%NF_ROOT%\ToolBin\wsl-util" rm -rf "%NF_ROOT%\ToolBin\wsl-util\*"
dotnet publish -o "%NF_ROOT%\ToolBin\wsl-util"
cd "%NF_ROOT%\ToolBin\wsl-util"
rm -rf *.xml
```
