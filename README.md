# Smithbox

A personal version of DSMapStudio, which is a standalone integrated modding tool for modern FromSoft games, which include Demon's Souls (PS3), the Dark Souls series, Bloodborne, Sekiro, and Elden Ring. It currently includes a map editor, a game param editor, and a text editor for editing in game text.

## New Features
- Asset Browser: allows you to quickly view and categorize Chr, Obj/Asset, Part and MapPiece assets. There is a base name and tag list supplied by Smithbox, and support for local mod overrides.
- Map Grid: allows you to see a segmented grid throughout the MSB viewport. The size, segments and height of the grid can be adjusted.

## Requirements
* Windows 7/8/8.1/10/11 (64-bit only)
* Visual C++ Redistributable x64 - INSTALL THIS IF THE PROGRAM CRASHES ON STARTUP (https://aka.ms/vs/16/release/vc_redist.x64.exe)
* For the error message "You must install or update .NET to run this application", use these exact download links. It is not enough to install the default .NET runtime.
  * Microsoft .NET Core 7.0 **Desktop** Runtime (https://aka.ms/dotnet/7.0/windowsdesktop-runtime-win-x64.exe)
  * Microsoft .NET Core 7.0 ASP.NET Core Runtime (https://aka.ms/dotnet/7.0/aspnetcore-runtime-win-x64.exe)
* A Vulkan 1.3 compatible graphics card with up to date graphics drivers: NVIDIA Maxwell (900 series) and newer or AMD Polaris (Radeon 400 series) and newer
* Intel GPUs currently don't seem to be working properly. At the moment you will need a dedicated NVIDIA or AMD GPU
* A 4GB (8GB recommended) of VRAM if modding DS3/BB/Sekiro/ER maps due to huge map sizes

## Basic usage instructions
### Game instructions
* **Dark Souls Prepare to die Edition**: Game must be unpacked with UDSFM before usage with Map Studio (https://www.nexusmods.com/darksouls/mods/1304).
* **Dark Souls Remastered**: Game is unpacked by default and requires no other tools.
* **Dark Souls 2 SOTFS**: Use UXM (https://www.nexusmods.com/sekiro/mods/26) to unpack the game. Vanilla Dark Souls 2 is not supported.
* **Dark Souls 3 and Sekiro**: Use UXM to extract the game files.
* **Demon's Souls**: Make sure to disable the RPCS3 file cache to test changes if using an emulator.
* **Bloodborne**: Any valid full game dump should work out of the box. Note that some dumps will have the base game (1.0) and the patch as separate, so the patch should be merged on top of the base game before use with map studio. You're on your own for installing mods to console at the moment.
* **Sekiro**: Use UXM to extract game files.
* **Elden Ring**: Use UXM Selective Unpack (https://github.com/Nordgaren/UXM-Selective-Unpack) to extract the game files. It's recommended to unpack everything, but at least the `map`, `asset`, `chr`, and `msg` directories are needed for basic editor usage.

### Mod projects
Map studio operates on top of something I call mod projects. These are typically stored in a separate directory from the base game, and all modifies files will be saved there instead of overwriting the base game files. The intended workflow is to install mod engine for your respective game and set the modoverridedirectory in modengine.ini to your mod project directory. This way you don't have to modify base game files (and work on multiple mod projects at a time) and you can easily distribute a mod by zipping up the project directory and uploading it.

## Game Limitations
* **Dark Souls Remastered**: Cannot render map collision in the viewport at this time.
* **Sekiro**: Cannot render map collision and navmesh in the viewport at this time.
* **Elden Ring**: Cannot render map collision and navmesh in the viewport at this time.

## Libraries Utilized
* Soulsformats
* [Newtonsoft Json.NET](https://www.newtonsoft.com/json)
* Heavily modified version of Veldrid for rendering backend
* Vortice.Vulkan bindings for Vulkan
* ImGui.NET for UI
* A small portion of [HavokLib](https://github.com/PredatorCZ/HavokLib), specifically the spline-compressed animation decompressor, adapted for C#
* Recast for navigation mesh generation
* Fork Awesome font for icons

## Original Credits for DSMapStudio
* Katalash - Project lead and original author
* philiquaz - Primary maintainer of integrated param editor
* george - Core maintainer and contributor
* thefifthmatt - Author of SoapstoneLib which allows cross-tool features

### Special Thanks
* TKGP - Made Soulsformats
* [Pav](https://github.com/JohrnaJohrna)
* [Meowmaritus](https://github.com/meowmaritus) - Made DSAnimStudio, which DSMapStudio is loosely based on
* [PredatorCZ](https://github.com/PredatorCZ) - Reverse engineered Spline-Compressed Animation entirely.
* [Horkrux](https://github.com/horkrux) - Reverse engineered the header and swizzling used on non-PC platform textures.
* [Vawser](https://github.com/vawser) - DS2/3 Documentation

## Credits for Smithbox
* Vawser
  
