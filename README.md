# Smithbox
Smithbox is a modding tool for Elden Ring, Elden Ring: Nightreign, Armored Core VI, Sekiro, Dark Souls 3, Dark Souls 2, Dark Souls 1, Bloodborne, and Demon's Souls.

[![GitHub release](https://img.shields.io/github/release/vawser/Smithbox.svg)](https://github.com/vawser/Smithbox/releases/latest)
[![Github All Releases](https://img.shields.io/github/downloads/vawser/Smithbox/total.svg)](https://github.com/vawser/Smithbox/releases/latest)
[![Smithbox Discord](https://img.shields.io/badge/Discord%20-%237289DA.svg?&logo=discord&logoColor=white)](https://discord.gg/5p9bRKkK4J)

## Key Features
- **Map Editor**: a visual editor for editing maps.
- **Model Editor**: a model editor for viewing and editing models. Similar to the FLVER Editor.
- **Param Editor**: a table editor for editing parameters, which contain most of the data that defines each game.
- **Text Editor**: an editor for editing text localization.
- **Graphics Param Editor**: an editor for quickly creating and editing GPARAM files.
- **Material Editor**: an editor for editing MTD and MATBIN materials.
- **Texture Viewer**: a simple to use viewer for looking and extracting textures.
- **File Browser**: a browser of the files contained within the data directory, plus the project directory. Allows easy extraction.

You no longer need to unpack a game for any of the editors.

## Links
Smithbox is a fork of the [DSMapStudio repository](https://github.com/soulsmods/DSMapStudio).

### Smithbox Tutorials
You can find helpful information and guides/tutorials about Smithbox at [soulsmodding.com](https://www.soulsmodding.com/doku.php?id=smithbox).

### Unpacking Files
If you want to unpack games files, you should use [Nuxe](https://github.com/JKAnderson/Nuxe/releases).

### Scripts
If you want to edit EMEVD scripts, you should use [DarkScript3](https://github.com/AinTunez/DarkScript3).

If you want to edit ESD scripts, you should use [ESDLang](https://github.com/thefifthmatt/ESDLang) or [ESDStudio](https://github.com/GompDS/ESDStudio) if you want a GUI interface.

## Support
If you enjoy Smithbox, feel free to support me (Vawser) via [Ko-fi](https://ko-fi.com/vawser)!

## Troubleshooting
### My game is telling me there is a 'Save Corruption' error when I launch my mod.

This is an issue with your mod launch setup. For most modern games, it is recommended that you use [ME3](https://github.com/garyttierney/me3) to launch your mods. Smithbox has support for creating a ME3 profile built-in.

For DS1, DS2 and pre 1.15.2 DS3, you will still need to use ModEngine2 or other mod launching solutions. In this case, you typically need to run the game with vanilla once when creating a new character. And then enable any mods. 

### Smithbox starts and then immediately closes

This is likely due to your graphics card lacking support for Vulkan. By default Smithbox will launch and try and use the Vulkan backend. 

If this occurs, you can go to %AppData%\Local\Smithbox\Configuration, open the Configuration.json file with a text editor, and then change the System_RenderingBackend variable to 0. This will make Smithbox use the OpenGL backend, which is supported by almost all graphics cards (however it will disable viewport functionality).

### Smithbox displays an error message box when I do X action

If you experience a crash, please report it to the [Smithbox Discord](https://discord.gg/5p9bRKkK4J) or post the error message in the Issues tab in this repository. This will help me find and fix the causes of such crashes.

### How do I use the latest build of Smithbox

To download the latest build of Smithbox, outside of the official releases, click on the green tick next to the latest commit when viewing the repository. Click on Details, and then on Summary. Scroll down, and you will see a link starting with: Smithbox-SHA, click on it to download the latest  build. 

Note: you will need a Github account to be able to navigate into the Github Actions page and download the build.

The latest build may be incomplete, as it is the in-development build of Smithbox, but it does allow you to immediately benefit from new additions and changes, whereas the official builds are only created when a substantial amount of changes has been finished.

## Credits (Smithbox)
* Vawser 
* ivi 
* nex3 
* gixxpunk 
* Strackeror 
* FireWolf700 
* GoogleBen 
* LordExelot 
* Pear0533 
* Metito 
* WarpZehpyr 
* twistedgwazi 
* FeeeeK 
* colaaaaaa123 
* alson041 
* gracenotes 

## Credits (DSMapStudio)
* Katalash
* philiquaz
* george
* thefifthmatt
* TKGP
* Nordgaren
* [Pav](https://github.com/JohrnaJohrna)
* [Meowmaritus](https://github.com/meowmaritus)
* [PredatorCZ](https://github.com/PredatorCZ)
* [Horkrux](https://github.com/horkrux)
* Shadowth117

## Libraries
Smithbox utilises the following libraries and projects:
* [SoulsFormats](https://github.com/JKAnderson/SoulsFormats) - Credit to TKGP
* [SoapstoneLib](https://github.com/soulsmods/SoapstoneLib) - Credit to gracenotes
* [HKLib](https://github.com/The12thAvenger/HKLib) - Credit to The12thAvenger
* [Hexa.NET.ImGui](https://github.com/HexaEngine/Hexa.NET.ImGui) - Credit to JunaMeinhold
* [Tracy Profiler](https://github.com/wolfpld/tracy)
* [Veldrid](https://github.com/veldrid/veldrid)

## Requirements
* Windows 7/8/8.1/10/11 (64-bit only)
* [Visual C++ Redistributable x64](https://aka.ms/vs/16/release/vc_redist.x64.exe)
* For the error message "You must install or update .NET to run this application", use these exact download links. It is not enough to install the default .NET runtime.
  * [Microsoft .NET Core 7.0 Desktop Runtime](https://aka.ms/dotnet/7.0/windowsdesktop-runtime-win-x64.exe)
  * [Microsoft .NET Core 7.0 ASP.NET Core Runtime](https://aka.ms/dotnet/7.0/aspnetcore-runtime-win-x64.exe)
* A Vulkan 1.3 compatible graphics card with up to date graphics drivers: NVIDIA Maxwell (900 series) and newer or AMD Polaris (Radeon 400 series) and newer.
* Intel GPUs currently don't seem to be working properly. At the moment you will need a dedicated NVIDIA or AMD GPU.
* A 4GB (8GB recommended) of VRAM if modding DS3/BB/Sekiro/ER maps due to huge map sizes.

## AI Disclaimer
Elements of the code and metadata for this project have been produced with AI tools.
