# DSMapStudio

Suite of modding tools for Elden Ring, Sekiro, the Dark Souls series, Bloodborne, and Demon's Souls (PS3)

[![GitHub release](https://img.shields.io/github/release/soulsmods/DSMapStudio.svg)](https://github.com/soulsmods/DSMapStudio/releases/latest)
[![Github All Releases](https://img.shields.io/github/downloads/soulsmods/DSMapStudio/total.svg)](https://github.com/soulsmods/DSMapStudio/releases/latest)
[![Discord](https://img.shields.io/badge/Discord%20-%237289DA.svg?&logo=discord&logoColor=white)](https://discord.com/invite/CKDBCUFhB3)

## About DS Map Studio:
DS Map Studio is a standalone integrated modding tool for modern FromSoft games, which include Demon's Souls (PS3), the Dark Souls series, Bloodborne, Sekiro, and Elden Ring. It currently includes a map editor, a game param editor, and a text editor for editing in game text.

![msb-editor-screenshot](https://user-images.githubusercontent.com/44953920/209740902-ab75c7fb-e281-4833-aeab-4c2ea41da815.png)

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

## FAQ

### Q: Can DSMapStudio be used to export models/maps to FBX like you could with DSTools?
A: Unfortunately not at the moment. Currently DSMapStudio doesn't have functionality for importing or exporting any assets, though an asset pipeline is planned to be implemented eventually.

### Q: Why are graphics requirements so steep? Will you ever support DX11 again?
A: Given the high requirements of rendering maps from DS3, Sekiro, and Elden Ring at good performance, DSMapStudio is designed around modern Vulkan and using newer GPU features centered around GPU driven rendering. Unfortunately, this means that DSMapStudio requires a relatively modern GPU that supports newer Vulkan features (NVIDIA Maxwell or AMD Polaris or newer). We realize that this is not ideal for people who want to only use the param editor though, and may look into a fallback renderer to support just the param editor if there is enough demand.

### Q: Will true custom maps be possible?
A: True custom maps are an eventual goal, but there's a lot of hurdles to overcome until we get there. Currently DSMapStudio doesn't have any kind of asset pipeline to import custom assets, but other community tools may be able to create assets that can be used in DSMapStudio and the games.

### Q: Why did you abandon DSTools?
A: DSTools worked well for the creation of many mods, and is still actively used today. However, the bindings of Unity data structures to Souls ones grew very messy and buggy, and led to a very unintuitive user experience (i.e. most users can't intuitively know what Unity operations are actually supported by DSTools for export). Unity also doesn't provide sufficiently low level APIs for many of its useful subsystems like its lightmapper and navmesh generator, so making these subsystems work for Dark Souls range from painful to impossible.

By far the biggest issue though is how heavyweight Unity is and how bad performance is when importing assets. All the Dark Souls assets have to be imported into Unity which takes a large amount of space and imports themselves can take 10s of minutes for a map. All these lead me to decide to make an editor from scratch that is A) heavily focused on the Souls games and have the user interface designed for editing them and B) has super fast load times by loading the game assets directly with no intermediate conversions needing to be stored. Map Studio still lacks some of the more advanced features supported by DSTools + Unity, but currently the core experience is much nicer to use with loading times for maps being measured in seconds rather than minutes.

## Credits
* Katalash - Project lead and original author
* philiquaz - Primary maintainer of integrated param editor
* george - Core maintainer and contributor
* thefifthmatt - Author of SoapstoneLib which allows cross-tool features

## Special Thanks
* TKGP - Made Soulsformats
* [Pav](https://github.com/JohrnaJohrna)
* [Meowmaritus](https://github.com/meowmaritus) - Made DSAnimStudio, which DSMapStudio is loosely based on
* [PredatorCZ](https://github.com/PredatorCZ) - Reverse engineered Spline-Compressed Animation entirely.
* [Horkrux](https://github.com/horkrux) - Reverse engineered the header and swizzling used on non-PC platform textures.
* [Vawser](https://github.com/vawser) - DS2/3 Documentation

## Libraries Utilized
* Soulsformats
* [Newtonsoft Json.NET](https://www.newtonsoft.com/json)
* Heavily modified version of Veldrid for rendering backend
* Vortice.Vulkan bindings for Vulkan
* ImGui.NET for UI
* A small portion of [HavokLib](https://github.com/PredatorCZ/HavokLib), specifically the spline-compressed animation decompressor, adapted for C#
* Recast for navigation mesh generation
* Fork Awesome font for icons
