# Smithbox
Smithbox is a modding tool for Elden Ring, Armored Core VI, Sekiro, Dark Souls 3, Dark Souls 2, Dark Souls 1, Bloodborne and Demon's Souls.

[![GitHub release](https://img.shields.io/github/release/vawser/Smithbox.svg)](https://github.com/vawser/Smithbox/releases/latest)
[![Github All Releases](https://img.shields.io/github/downloads/vawser/Smithbox/total.svg)](https://github.com/vawser/Smithbox/releases/latest)
[![Smithbox Discord](https://img.shields.io/badge/Discord%20-%237289DA.svg?&logo=discord&logoColor=white)](https://discord.gg/5p9bRKkK4J)

## Key Features
- Map Editor: a visual editor for editing maps.
- Param Editor: a table editor for editing parameters, which contain most of the data that defines each game.
- Model Editor: a model editor for viewing and editing models. Similar to the FLVER Editor.
- Text Editor: an editor for editing text localization.
- Gparam Editor: an editor for quickly creating and editing GPARAM files.
- Time Act Editor: an editor for editing TAE files, which control the events that occur during animations.
- Texture Viewer: a simple to use viewer for looking and extracting textures.
- Behavior Editor: a editor for behaviors (ER and NR projects only).
- Material Editor: a editor for MTD and MATBIN files.

## Requirements
* Windows 7/8/8.1/10/11 (64-bit only)
* [Visual C++ Redistributable x64](https://aka.ms/vs/16/release/vc_redist.x64.exe)
* For the error message "You must install or update .NET to run this application", use these exact download links. It is not enough to install the default .NET runtime.
  * [Microsoft .NET Core 7.0 Desktop Runtime](https://aka.ms/dotnet/7.0/windowsdesktop-runtime-win-x64.exe)
  * [Microsoft .NET Core 7.0 ASP.NET Core Runtime](https://aka.ms/dotnet/7.0/aspnetcore-runtime-win-x64.exe)
* A Vulkan 1.3 compatible graphics card with up to date graphics drivers: NVIDIA Maxwell (900 series) and newer or AMD Polaris (Radeon 400 series) and newer
* Intel GPUs currently don't seem to be working properly. At the moment you will need a dedicated NVIDIA or AMD GPU
* A 4GB (8GB recommended) of VRAM if modding DS3/BB/Sekiro/ER maps due to huge map sizes

## Links
Smithbox is a fork of the [DSMapStudio repository](https://github.com/soulsmods/DSMapStudio)

# Support
If you enjoy Smithbox, feel free to support me via [Ko-fi](https://ko-fi.com/vawser).

## Credits (Smithbox)
* Vawser (Developer)
* ivi (Contributor)
* nex3 (Contributor)
* gixxpunk (Contributor)
* Strackeror (Contributor)
* FireWolf700 (Contributor)
* GoogleBen (Contributor)
* LordExelot (Contributor)
* Pear0533 (Contributor)
* Metito (Contributor)
* WarpZehpyr (Contributor)
* twistedgwazi (Contributor)
* FeeeeK (Contributor)
* colaaaaaa123 (Contributor)
* alson041 (Contributor)
* gracenotes (Contributor)

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

# Libraries
* [SoulsFormats](https://github.com/JKAnderson/SoulsFormats) - Credit to TKGP
* [SoapstoneLib](https://github.com/soulsmods/SoapstoneLib) - Credit to gracenotes
* [HKLib](https://github.com/The12thAvenger/HKLib) - Credit to The12thAvenger
* [Hexa.NET.ImGui](https://github.com/HexaEngine/Hexa.NET.ImGui) - Credit to JunaMeinhold
* [Veldrid](https://github.com/veldrid/veldrid)
  
## Usage Instructions
You no longer need to unpack your game unless you are using the Map or Model Editor.

#### Dark Souls: Prepare to Die Edition
* Game must be unpacked with [UDSFM](https://www.nexusmods.com/darksouls/mods/1304) before usage with Smithbox.

#### Dark Souls: Remastered
* Game is unpacked by default and requires no other tools.

#### Dark Souls II: Scholar of the First Sin
* Use [UXM](https://www.nexusmods.com/sekiro/mods/26) to unpack the game. Vanilla Dark Souls 2 is not supported.

#### Dark Souls III
* Use [UXM](https://www.nexusmods.com/sekiro/mods/26) to unpack the game.

#### Sekiro: Shadows Die Twice
* Use [UXM](https://www.nexusmods.com/sekiro/mods/26) to unpack the game.

#### Elden Ring
* Use [UXM Selective Unpack](https://github.com/Nordgaren/UXM-Selective-Unpack) to extract the game files.

#### Armored Core VI
* Use [UXM Selective Unpack](https://github.com/Nordgaren/UXM-Selective-Unpack) to extract the game files.

