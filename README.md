# Smithbox
A personal version of DSMapStudio, which is a standalone integrated modding tool for modern FromSoft games, which include Demon's Souls (PS3), the Dark Souls series, Bloodborne, Sekiro, and Elden Ring. It currently includes a map editor, a game param editor, and a text editor for editing in game text.

## New Features
- Asset Browser: allows you to quickly view and categorize Chr, Obj/Asset, Part and MapPiece assets. There is a base name and tag list supplied by Smithbox, and support for local mod overrides.
- Map Grid: allows you to see a segmented grid throughout the MSB viewport. The size, segments and height of the grid can be adjusted.
- Map Editor Toolbar: adds the ability to quickly configure the various map-specific actions, as well as adding new powerful actions such as Scramble and Replicate.

## Requirements
* Windows 7/8/8.1/10/11 (64-bit only)
* [Visual C++ Redistributable x64](https://aka.ms/vs/16/release/vc_redist.x64.exe)
* For the error message "You must install or update .NET to run this application", use these exact download links. It is not enough to install the default .NET runtime.
  * [Microsoft .NET Core 7.0 Desktop Runtime](https://aka.ms/dotnet/7.0/windowsdesktop-runtime-win-x64.exe)
  * [Microsoft .NET Core 7.0 ASP.NET Core Runtime](https://aka.ms/dotnet/7.0/aspnetcore-runtime-win-x64.exe)
* A Vulkan 1.3 compatible graphics card with up to date graphics drivers: NVIDIA Maxwell (900 series) and newer or AMD Polaris (Radeon 400 series) and newer
* Intel GPUs currently don't seem to be working properly. At the moment you will need a dedicated NVIDIA or AMD GPU
* A 4GB (8GB recommended) of VRAM if modding DS3/BB/Sekiro/ER maps due to huge map sizes

## Usage Instructions
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

#### Demon's Souls
* Make sure to disable the RPCS3 file cache to test changes if using an emulator.

#### Bloodborne
* Any valid full game dump should work out of the box. 
* Note that some dumps will have the base game (1.0) and the patch as separate, so the patch should be merged on top of the base game before use with map studio.

#### Elden Ring
* Use [UXM Selective Unpack](https://github.com/Nordgaren/UXM-Selective-Unpack) to extract the game files.

## Limitations
* **Dark Souls Remastered**: Cannot render map collision in the viewport at this time.
* **Sekiro**: Cannot render map collision and navmesh in the viewport at this time.
* **Elden Ring**: Cannot render map collision and navmesh in the viewport at this time.

# Links
[DSMapStudio repository](https://github.com/soulsmods/DSMapStudio)

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

## Credits (Smithbox)
* Vawser
* ivi


  
