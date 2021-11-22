# LibTool [![License](https://img.shields.io/github/license/xd-Abi/LibTool.svg)](https://github.com/xd-Abi/LibTool/blob/main/LICENSE)
 
![LibTool](Resources/Icon.png?raw=true "LibTool")

***
LibTool is a tool to download libraries from the internet. It also has some Utility commands to manage the libraries.

## Example
```
<?xml version="1.0" encoding="utf-8"?>
<LibTool>

    <Config>
        <Override>False</Override>
        <DefaultInRoot>True</DefaultInRoot>
        <RootPath>../../Libraries/</RootPath>
        <RelativePath>Root</RelativePath>
    </Config>

    <Include>
        <File InRoot="false">TestLib.xml</File>
        <Dir>
            <Path>LibToolFiles/</Path>
            <Filter>**.xml</Filter>
        </Dir>
    </Include>
</LibTool>
```

## Gettings Started
LibTool is build using premake5. Visual Studio 2017 or 2019 is recommended.

### Windows
```
git clone https://github.com/xd-Abi/LibTool
cd LibTool
call Build\Win-GenProjects.bat
```

### Mac and Linux
Mac and linux are not supported yet. 