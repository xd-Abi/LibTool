workspace "LibTool"
    architecture "x86_64"
    location "../"
    targetdir "%{wks.location}/Binaries/%{cfg.configuration}/%{cfg.platform}/"
    objdir "%{wks.location}/Binaries/Object/"

    configurations
    {
        "Debug",
        "Release",
    }

    platforms
    {
        "Win64"
    }

project "LibTool"
    kind "ConsoleApp"
    language "C#"
    location "%{wks.location}/Source"
    icon "%{wks.location}/Source/Resources/Icon.ico"

    files 
    {
        "%{wks.location}/Source/**.cs"
    }

    links 
    {
        "System",         
        "System.Core",
        "System.IO.Compression", 
        "System.IO.Compression.FileSystem", 
    }