using Andre.IO.VFS;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.MapEditor;
using StudioCore.Logger;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Formats;

/// <summary>
/// Generates the break files of maps from their saved MSBs: BREAKOBJ for DeS, DS1, BB and DS3,
/// BREAKOBJ2 for DS2, and BREAKGEOM for ER and NR. The other games have no break files.
/// The break data of new parts is copied from vanilla instances of their model; see <see cref="BreakDataBuilder"/>.
/// </summary>
public class BreakDataGenerator
{
    private enum BreakFormat
    {
        None,
        BreakObj,
        BreakObj2,
        BreakGeom
    }

    private readonly ProjectEntry Project;

    // Built from the vanilla files on first use, as reading them all takes a while for ER
    private BreakTemplateLibrary Library;
    private DCX.Type Compression;

    public BreakDataGenerator(ProjectEntry project)
    {
        Project = project;
    }

    private BreakFormat Format => Project.Descriptor.ProjectType switch
    {
        ProjectType.DES or ProjectType.DS1 or ProjectType.DS1R or ProjectType.BB or ProjectType.DS3 => BreakFormat.BreakObj,
        ProjectType.DS2 or ProjectType.DS2S => BreakFormat.BreakObj2,
        ProjectType.ER or ProjectType.NR => BreakFormat.BreakGeom,
        _ => BreakFormat.None
    };

    public bool CanUse() => Format != BreakFormat.None;

    /// <summary>
    /// Generates the break files of the maps in a background task.
    /// </summary>
    public void GenerateTask(IReadOnlyList<string> mapIds)
    {
        if (!CanUse())
            return;

        TaskManager.LiveTask task = new(
            "mapEditor_generateBreakData",
            LOC.Get("SYS_Header"),
            LOC.Get("MAP_Break_Data_Task_PASS"),
            LOC.Get("MAP_Break_Data_Task_FAIL"),
            TaskManager.RequeueType.None,
            false,
            LogPriority.Normal,
            () => Generate(mapIds)
        );

        TaskManager.Run(task);
    }

    private void Generate(IReadOnlyList<string> mapIds)
    {
        if (Library == null)
        {
            Smithbox.Log(this, LOC.Get("MAP_Break_Data_Building_Templates"));
            BuildLibrary();
        }

        int written = 0;
        foreach (var mapId in mapIds)
        {
            try
            {
                if (GenerateMap(mapId))
                    written++;
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("MAP_Break_Data_Map_FAIL", mapId), e);
            }
        }

        Smithbox.Log(this, LOC.Get("MAP_Break_Data_Generated", written, mapIds.Count));
    }

    #region Templates
    private void BuildLibrary()
    {
        var library = new BreakTemplateLibrary();
        var vanillaFS = Project.VFS.VanillaFS;
        bool compressionFound = false;

        foreach (var entry in Project.Locator.BreakFiles.Entries.OrderBy(e => e.Path, StringComparer.OrdinalIgnoreCase))
        {
            if (!vanillaFS.FileExists(entry.Path))
                continue;

            try
            {
                var data = DCX.Decompress(vanillaFS.ReadFile(entry.Path).Value, out var compression);
                if (!compressionFound)
                {
                    Compression = compression;
                    compressionFound = true;
                }

                switch (Format)
                {
                    case BreakFormat.BreakObj:
                    {
                        var parts = ReadParts(vanillaFS, entry.Filename);
                        if (parts != null)
                            library.AddBreakObj(parts, BREAKOBJ.Read(data));
                        break;
                    }
                    case BreakFormat.BreakObj2:
                    {
                        var parts = ReadParts(vanillaFS, entry.Filename);
                        if (parts != null)
                            library.AddBreakObj2(parts, BREAKOBJ2.Read(data));
                        break;
                    }
                    case BreakFormat.BreakGeom:
                        library.AddBreakGeom(GetLod(entry), BREAKGEOM.Read(data));
                        break;
                }
            }
            catch (Exception e)
            {
                // The BB chalice dungeon file uses a layout that is not supported, so it is skipped here
                Smithbox.LogWarning(this, LOC.Get("MAP_Break_Data_Template_Skipped", entry.Path, e.Message), LogPriority.Low);
            }
        }

        if (!compressionFound)
        {
            Compression = Format == BreakFormat.BreakGeom ? DCX.Type.DCX_KRAK : DCX.Type.None;
        }

        Library = library;
    }

    private static int GetLod(FileDictionaryEntry entry)
    {
        for (int lod = 1; lod < BreakDataBuilder.LodCount; lod++)
        {
            if (entry.Folder.EndsWith($"lod{lod}", StringComparison.OrdinalIgnoreCase))
                return lod;
        }
        return 0;
    }
    #endregion

    #region Generation
    /// <summary>
    /// Generates the break files of a map, writing those that differ from the current ones.
    /// Returns whether any file was written.
    /// </summary>
    private bool GenerateMap(string mapId)
    {
        var targetFS = Project.Handler.MapData.PrimaryBank.TargetFS;
        var vanillaFS = Project.VFS.VanillaFS;

        if (IsUneditedVanilla(mapId))
            return false;

        var wrapper = Project.Handler.MapData.PrimaryBank.Maps.FirstOrDefault(e => e.Key.Filename == mapId).Value;
        if (wrapper?.MapContainer != null && wrapper.MapContainer.IsDirty(MapFileType.MSB))
        {
            Smithbox.LogWarning(this, LOC.Get("MAP_Break_Data_Unsaved_Map", mapId), LogPriority.High);
        }

        var parts = ReadParts(targetFS, mapId);
        if (parts == null)
        {
            Smithbox.LogWarning(this, LOC.Get("MAP_Break_Data_No_MSB", mapId), LogPriority.Normal);
            return false;
        }

        var vanillaParts = ReadParts(vanillaFS, mapId);
        var stats = new BreakBuildStats();
        bool written = false;

        switch (Format)
        {
            case BreakFormat.BreakObj:
            {
                var path = GetBreakFilePath(mapId, mapId);
                var vanillaFile = ReadVanilla(path, BREAKOBJ.Read);
                var file = BreakDataBuilder.BuildBreakObj(parts, vanillaParts, vanillaFile, Library, stats);
                written = WriteIfChanged(path, file.Write());
                break;
            }
            case BreakFormat.BreakObj2:
            {
                var path = GetBreakFilePath(mapId, mapId);
                var vanillaFile = ReadVanilla(path, BREAKOBJ2.Read);
                var file = BreakDataBuilder.BuildBreakObj2(parts, vanillaParts, vanillaFile, Library, stats);
                written = WriteIfChanged(path, file.Write());
                break;
            }
            case BreakFormat.BreakGeom:
            {
                var paths = new string[BreakDataBuilder.LodCount];
                var vanillaFiles = new BREAKGEOM[BreakDataBuilder.LodCount];
                for (int lod = 0; lod < BreakDataBuilder.LodCount; lod++)
                {
                    paths[lod] = GetBreakFilePath(mapId, $"{mapId}_lod{lod}", lod);
                    vanillaFiles[lod] = ReadVanilla(paths[lod], BREAKGEOM.Read);
                }

                var files = BreakDataBuilder.BuildBreakGeom(mapId, parts, vanillaParts, vanillaFiles, Library, stats);
                for (int lod = 0; lod < BreakDataBuilder.LodCount; lod++)
                {
                    if (files[lod] != null && WriteIfChanged(paths[lod], files[lod].Write()))
                        written = true;
                }
                break;
            }
        }

        if (written)
            Smithbox.Log(this, LOC.Get("MAP_Break_Data_Map_PASS", mapId, stats.Kept, stats.Added));

        return written;
    }

    /// <summary>
    /// Whether neither the MSB nor the break files of a map are in the project. Its vanilla break files then
    /// already match what would be generated, so the MSBs don't need to be read.
    /// </summary>
    private bool IsUneditedVanilla(string mapId)
    {
        var projectFS = Project.VFS.ProjectFS;

        var msbEntry = Project.Locator.MapFiles.Entries.FirstOrDefault(e => e.Filename == mapId);
        if (msbEntry == null || projectFS.FileExists(msbEntry.Path) || !Project.VFS.VanillaFS.FileExists(msbEntry.Path))
            return false;

        int lodCount = Format == BreakFormat.BreakGeom ? BreakDataBuilder.LodCount : 1;
        for (int lod = 0; lod < lodCount; lod++)
        {
            var filename = Format == BreakFormat.BreakGeom ? $"{mapId}_lod{lod}" : mapId;
            if (projectFS.FileExists(GetBreakFilePath(mapId, filename, lod)))
                return false;
        }

        return true;
    }

    /// <summary>
    /// The path of a break file of a map: the one in the file dictionary, or where a new one goes.
    /// </summary>
    private string GetBreakFilePath(string mapId, string filename, int lod = 0)
    {
        var entry = Project.Locator.BreakFiles.Entries.FirstOrDefault(e => e.Filename == filename && GetLod(e) == lod);
        if (entry != null)
            return entry.Path;

        return Project.Descriptor.ProjectType switch
        {
            ProjectType.DS2 or ProjectType.DS2S => $"/breakobj/{mapId}.breakobj",
            ProjectType.ER or ProjectType.NR => $"/map/breakgeom/lod{lod}/{filename}.breakgeom.dcx",
            _ when Compression == DCX.Type.None => $"/map/breakobj/{mapId}.breakobj",
            _ => $"/map/breakobj/{mapId}.breakobj.dcx"
        };
    }

    private T ReadVanilla<T>(string path, Func<Memory<byte>, T> read) where T : class
    {
        var vanillaFS = Project.VFS.VanillaFS;
        if (!vanillaFS.FileExists(path))
            return null;

        // A vanilla file that can't be read must not be replaced by a rebuilt one, so this is left to throw
        return read(DCX.Decompress(vanillaFS.ReadFile(path).Value));
    }

    private bool WriteIfChanged(string path, byte[] data)
    {
        var targetFS = Project.Handler.MapData.PrimaryBank.TargetFS;
        if (targetFS.FileExists(path))
        {
            var current = DCX.Decompress(targetFS.ReadFile(path).Value);
            if (BytePerfectHelper.Md5Equal(current.Span, data))
                return false;
        }

        bool compressed = path.EndsWith(".dcx", StringComparison.OrdinalIgnoreCase);
        Project.VFS.ProjectFS.WriteFile(path, compressed ? DCX.Compress(data, Compression) : data);
        return true;
    }
    #endregion

    #region MSB
    /// <summary>
    /// Reads the parts of a map's MSB that its break files list, or null if the MSB isn't in the file system.
    /// </summary>
    private List<BreakPart> ReadParts(VirtualFileSystem fs, string mapId)
    {
        var entry = Project.Locator.MapFiles.Entries.FirstOrDefault(e => e.Filename == mapId);
        if (entry == null || !fs.FileExists(entry.Path))
            return null;

        var data = fs.ReadFile(entry.Path).Value;

        return Project.Descriptor.ProjectType switch
        {
            ProjectType.DES => MSBD.Read(data).Parts.Objects.Select(p => new BreakPart(p.Name, p.ModelName, 0)).ToList(),
            ProjectType.DS1 or ProjectType.DS1R => MSB1.Read(data).Parts.Objects.Select(p => new BreakPart(p.Name, p.ModelName, 0)).ToList(),
            ProjectType.BB => MSBB.Read(data).Parts.Objects.Select(p => new BreakPart(p.Name, p.ModelName, 0)).ToList(),
            ProjectType.DS3 => MSB3.Read(data).Parts.Objects.Select(p => new BreakPart(p.Name, p.ModelName, 0)).ToList(),
            ProjectType.DS2 or ProjectType.DS2S => MSB2.Read(data).Parts.Objects.Select(p => new BreakPart(p.Name, p.ModelName, (int)p.ObjectInstanceID)).ToList(),
            ProjectType.ER => MSBE.Read(data).Parts.Assets.Select(p => new BreakPart(p.Name, p.ModelName, p.InstanceID)).ToList(),
            ProjectType.NR => MSB_NR.Read(data).Parts.Assets.Select(p => new BreakPart(p.Name, p.ModelName, p.InstanceID)).ToList(),
            _ => null
        };
    }
    #endregion
}
