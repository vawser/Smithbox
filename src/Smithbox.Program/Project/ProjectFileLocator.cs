using DotNext.Collections.Generic;
using Microsoft.Extensions.Logging;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StudioCore.Application;

public class ProjectFileLocator : IDisposable
{
    public ProjectEntry Project;

    public FileDictionary FileDictionary;

    public FileDictionary MapFiles = new();
    public FileDictionary MapPieceFiles = new();
    public FileDictionary ChrFiles = new();
    public FileDictionary AssetFiles = new();
    public FileDictionary PartFiles = new();
    public FileDictionary CollisionFiles = new();
    public FileDictionary LightFiles = new();
    public FileDictionary DS2_LightFiles = new();
    public FileDictionary NavmeshFiles = new();
    public FileDictionary AutoInvadeFiles = new();
    public FileDictionary LightAtlasFiles = new();
    public FileDictionary LightProbeFiles = new();

    public FileDictionary GparamFiles = new();
    public FileDictionary TextFiles = new();

    public FileDictionary MTD_Files = new();
    public FileDictionary MATBIN_Files = new();

    public FileDictionary TextureFiles = new();
    public FileDictionary TexturePackedFiles = new();
    public FileDictionary ShoeboxFiles = new();


    public ProjectFileLocator(ProjectEntry project)
    {
        Project = project;
    }

    #region Init
    public async Task Initialize(Action<ProjectLoadProgress> reportProgress, bool silent = false)
    {
        var projectLocalFolder = Path.Combine(Project.Descriptor.ProjectPath, ".smithbox");

        if (!Directory.Exists(projectLocalFolder))
        {
            Directory.CreateDirectory(projectLocalFolder);
        }

        var folder = Path.Join(StudioCore.Common.FileLocations.Assets, "File Dictionaries");
        var file = "";

        // Build the file dictionary JSON objects here
        switch (Project.Descriptor.ProjectType)
        {
            case ProjectType.DES:
                file = "DES-File-Dictionary.json"; break;
            case ProjectType.DS1:
                file = "DS1-File-Dictionary.json"; break;
            case ProjectType.DS1R:
                file = "DS1R-File-Dictionary.json"; break;
            case ProjectType.DS2:
                file = "DS2-File-Dictionary.json"; break;
            case ProjectType.DS2S:
                file = "DS2S-File-Dictionary.json"; break;
            case ProjectType.DS3:
                file = "DS3-File-Dictionary.json"; break;
            case ProjectType.BB:
                file = "BB-File-Dictionary.json"; break;
            case ProjectType.SDT:
                file = "SDT-File-Dictionary.json"; break;
            case ProjectType.ER:
                file = "ER-File-Dictionary.json"; break;
            case ProjectType.AC6:
                file = "AC6-File-Dictionary.json"; break;
            case ProjectType.NR:
                file = "NR-File-Dictionary.json"; break;
            default: break;
        }

        if (!silent)
        {
            reportProgress?.Invoke(new()
            {
                PhaseLabel = "Initializing Project",
                StepLabel = "Compiling root file directory",
                Percent = 0.5f
            });
        }

        var filepath = Path.Join(folder, file);

        var jsonFileDictionary = new FileDictionary();

        if (File.Exists(filepath))
        {
            try
            {
                var filestring = await File.ReadAllTextAsync(filepath);

                try
                {
                    jsonFileDictionary = JsonSerializer.Deserialize(filestring, ProjectJsonSerializerContext.Default.FileDictionary);
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, $"Failed to deserialize the file dictionary: {filepath}", e);
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, $"Failed to read the file dictionary: {filepath}", e);
            }
        }

        if (!silent)
        {
            reportProgress?.Invoke(new()
            {
                PhaseLabel = "Initializing Project",
                StepLabel = "Compiling project file directory",
                Percent = 0.25f
            });
        }

        if (CFG.Current.Project_Scan_Directory_For_Additions)
        {
            var projectFileDictionary = BuildProjectFileDictionary(
                Project.Descriptor.ProjectPath,
                jsonFileDictionary, Project.Descriptor.ProjectType);

            FileDictionary = MergeFileDictionaries(jsonFileDictionary, projectFileDictionary);
        }
        else
        {
            FileDictionary = jsonFileDictionary;
        }

        if (!silent)
        {
            reportProgress?.Invoke(new()
            {
                PhaseLabel = "Initializing Project",
                StepLabel = "Compiling editor file directories",
                Percent = 0.3f
            });
        }

        CompileDictionaries();

        return;
    }

    private FileDictionary BuildProjectFileDictionary(string projectPath, FileDictionary existingDict, ProjectType type)
    {
        var projectFileDictionary = new FileDictionary { Entries = new() };

        if (!Directory.Exists(projectPath))
            return projectFileDictionary;

        var existingPaths = new HashSet<string>(
            existingDict.Entries.Select(e => e.Path),
            StringComparer.OrdinalIgnoreCase);

        var existingFolders = new HashSet<string>(
            existingDict.Entries.Select(e => e.Folder),
            StringComparer.OrdinalIgnoreCase);

        var archiveName = Path.GetFileName(projectPath);

        foreach (var filePath in Directory.EnumerateFiles(projectPath, "*", SearchOption.AllDirectories))
        {
            var dir = Path.GetDirectoryName(filePath);
            var normalizedDir = NormalizePath(dir);

            var relativePath = "/" + Path.GetRelativePath(projectPath, filePath).Replace('\\', '/');
            var relativeFolder = Path.GetDirectoryName(relativePath).Replace('\\', '/');

            // Skip any new base dir files
            if (relativeFolder == "/")
                continue;

            // If project relative path already exists in vanilla directory, ignore it as we don't need to include it
            if (existingPaths.Contains(relativePath))
                continue;

            var add = false;

            // Add if it is a file in a new aeg/aet folder
            if (relativeFolder.Contains("/aeg") || relativeFolder.Contains("/aet"))
                add = true;

            // Add if it is a new file in any of the vanilla directories
            foreach (var entry in existingFolders)
            {
                if(entry == relativeFolder)
                {
                    add = true;
                }
            }

            if (add)
            {
                var ext = Path.GetExtension(filePath).TrimStart('.').ToLowerInvariant();
                var fileName = Path.GetFileNameWithoutExtension(filePath);

                if (ext == "dcx")
                {
                    var prevExt = Path.GetExtension(fileName);
                    if (!string.IsNullOrEmpty(prevExt))
                    {
                        ext = prevExt.TrimStart('.').ToLowerInvariant();
                        fileName = Path.GetFileNameWithoutExtension(fileName);
                    }
                }

                projectFileDictionary.Entries.Add(new FileDictionaryEntry
                {
                    Archive = archiveName,
                    Path = relativePath,
                    Folder = Path.GetDirectoryName(relativePath)?.Replace('\\', '/'),
                    Filename = fileName,
                    Extension = ext
                });
            }
        }

        return projectFileDictionary;
    }

    private FileDictionary MergeFileDictionaries(FileDictionary first, FileDictionary second)
    {
        var combined = new FileDictionary();
        combined.Entries = new();

        // Normalize and track unique paths
        var seenPaths = new HashSet<string>(
            first.Entries
                 .Select(e => NormalizePath(e.Path))
                 .Where(p => p != null),
            StringComparer.OrdinalIgnoreCase);

        combined.Entries.AddRange(first.Entries);

        foreach (var entry in second.Entries)
        {
            var normalizedPath = NormalizePath(entry.Path);
            if (normalizedPath != null && !seenPaths.Contains(normalizedPath))
            {
                combined.Entries.Add(entry);
                seenPaths.Add(normalizedPath);
            }
        }

        combined.Entries = combined.Entries.OrderBy(e => e.Filename).ToList();
        return combined;
    }

    private static string NormalizePath(string path)
    {
        return string.IsNullOrWhiteSpace(path)
            ? null
            : path.Trim().Replace('\\', '/'); // normalize separators and trim
    }
    #endregion

    #region Dictionaries
    public void CompileDictionaries()
    {
        CompileMapDictionaries();
        CompileModelDictionaries();
        CompileTextDictionaries();
        CompileGparamDictionaries();
        CompileMaterialDictionaries();
        CompileTextureDictionaries();
    }

    public void CompileMapDictionaries()
    {
        // MSB
        MapFiles.Entries = Project.Locator.FileDictionary.Entries
            .Where(e => e.Folder.StartsWith("/map") && !e.Folder.Contains("autoroute"))
            .Where(e => e.Extension == "msb")
            .ToList();

        // BTL
        LightFiles.Entries = Project.Locator.FileDictionary.Entries
            .Where(e => e.Folder.StartsWith("/map"))
            .Where(e => e.Extension == "btl")
            .ToList();

        DS2_LightFiles.Entries = Project.Locator.FileDictionary.Entries
            .Where(e => e.Folder.StartsWith("/map"))
            .Where(e => e.Extension == "gibhd")
            .ToList();

        // NVA
        NavmeshFiles.Entries = Project.Locator.FileDictionary.Entries
            .Where(e => e.Folder.StartsWith("/map"))
            .Where(e => e.Extension == "nva")
            .ToList();

        // Collision
        CollisionFiles.Entries = Project.Locator.FileDictionary.Entries
            .Where(e => e.Folder.StartsWith("/map"))
            .Where(e => e.Extension == "hkxbhd")
            .ToList();

        // AutoInvade
        AutoInvadeFiles.Entries = Project.Locator.FileDictionary.Entries
            .Where(e => e.Folder.StartsWith("/other"))
            .Where(e => e.Extension == "aipbnd")
            .ToList();

        // Light Atlases
        LightAtlasFiles.Entries = Project.Locator.FileDictionary.Entries
            .Where(e => e.Folder.StartsWith("/map"))
            .Where(e => e.Extension == "btab")
            .ToList();

        // Light Probes
        LightProbeFiles.Entries = Project.Locator.FileDictionary.Entries
            .Where(e => e.Folder.StartsWith("/map"))
            .Where(e => e.Extension == "btpb")
            .ToList();
    }

    public void CompileModelDictionaries()
    {
        // Maps (for the map pieces)
        MapFiles.Entries = Project.Locator.FileDictionary.Entries
            .Where(e => e.Folder.StartsWith("/map") && !e.Folder.Contains("autoroute"))
            .Where(e => e.Extension == "msb")
            .Where(e => !e.Archive.Contains("sd"))
            .ToList();

        // Characters
        if (Project.Descriptor.ProjectType is ProjectType.DS2S or ProjectType.DS2)
        {
            ChrFiles.Entries = Project.Locator.FileDictionary.Entries
                    .Where(e => e.Folder.StartsWith("/model/chr"))
                    .Where(e => e.Extension == "bnd")
                    .ToList();
        }
        else
        {
            ChrFiles.Entries = Project.Locator.FileDictionary.Entries
                    .Where(e => e.Extension == "chrbnd")
                    .Where(e => !e.Archive.Contains("sd"))
                    .ToList();
        }

        // Assets / Objects
        if (Project.Descriptor.ProjectType is ProjectType.DS1)
        {
            AssetFiles.Entries = Project.Locator.FileDictionary.Entries
                .Where(e => e.Folder.StartsWith($"/obj"))
                .Where(e => e.Extension == "objbnd")
                .ToList();
        }
        else if (Project.Descriptor.ProjectType is ProjectType.DS2S or ProjectType.DS2)
        {
            AssetFiles.Entries = Project.Locator.FileDictionary.Entries
                .Where(e => e.Folder.StartsWith($"/model/obj"))
                .Where(e => e.Extension == "bnd")
                .ToList();
        }
        else if (Project.Descriptor.ProjectType is ProjectType.DS3 or ProjectType.BB or ProjectType.SDT)
        {
            AssetFiles.Entries = Project.Locator.FileDictionary.Entries
                .Where(e => e.Folder.StartsWith($"/obj"))
                .Where(e => e.Extension == "objbnd")
                .ToList();
        }
        else if (Project.Descriptor.ProjectType is ProjectType.ER or ProjectType.AC6 or ProjectType.NR)
        {
            AssetFiles.Entries = Project.Locator.FileDictionary.Entries
                .Where(e => e.Folder.StartsWith($"/asset"))
                .Where(e => e.Extension == "geombnd")
                .Where(e => !e.Archive.Contains("sd"))
                .ToList();
        }

        // Parts
        if (Project.Descriptor.ProjectType is ProjectType.DS1)
        {
            PartFiles.Entries = Project.Locator.FileDictionary.Entries
                .Where(e => e.Folder.StartsWith($"/parts"))
                .Where(e => e.Extension == "partsbnd")
                .ToList();
        }
        else if (Project.Descriptor.ProjectType is ProjectType.DS2S or ProjectType.DS2)
        {
            PartFiles.Entries = Project.Locator.FileDictionary.Entries
                .Where(e => e.Folder.StartsWith($"/model/parts"))
                .Where(e => e.Extension == "bnd")
                .ToList();
        }
        else if (Project.Descriptor.ProjectType is ProjectType.DS3 or ProjectType.BB or ProjectType.SDT)
        {
            PartFiles.Entries = Project.Locator.FileDictionary.Entries
                .Where(e => e.Folder.StartsWith($"/parts"))
                .Where(e => e.Extension == "partsbnd")
                .ToList();
        }
        else if (Project.Descriptor.ProjectType is ProjectType.ER or ProjectType.AC6 or ProjectType.NR)
        {
            PartFiles.Entries = Project.Locator.FileDictionary.Entries
                .Where(e => e.Folder.StartsWith($"/parts"))
                .Where(e => e.Extension == "partsbnd")
                .Where(e => !e.Archive.Contains("sd"))
                .ToList();
        }

        // Collisions
        if (Project.Descriptor.ProjectType is ProjectType.DS2S or ProjectType.DS2)
        {
            CollisionFiles.Entries = Project.Locator.FileDictionary.Entries
                .Where(e => e.Folder.StartsWith($"/model/map"))
                .Where(e => e.Extension == "hkxbhd")
                .ToList();
        }

        foreach (var map in MapFiles.Entries)
        {
            var mapid = map.Filename;
            var entries = new List<FileDictionaryEntry>();

            if (Project.Descriptor.ProjectType is ProjectType.DS1 or ProjectType.DES)
            {
                entries = Project.Locator.FileDictionary.Entries
                    .Where(e => e.Folder.StartsWith($"/map/{mapid}"))
                    .Where(e => e.Extension == "hkx")
                    .ToList();
            }
            else if (Project.Descriptor.ProjectType is ProjectType.DS1R)
            {
                entries = Project.Locator.FileDictionary.Entries
                    .Where(e => e.Folder.StartsWith($"/map/{mapid}"))
                    .Where(e => e.Extension == "hkxbhd")
                    .ToList();
            }
            else if (Project.Descriptor.ProjectType is ProjectType.DS3 or ProjectType.BB or ProjectType.SDT)
            {
                entries = Project.Locator.FileDictionary.Entries
                    .Where(e => e.Folder.StartsWith($"/map/{mapid}"))
                    .Where(e => e.Extension == "hkxbhd")
                    .ToList();
            }
            else if (Project.Descriptor.ProjectType is ProjectType.ER or ProjectType.AC6 or ProjectType.NR)
            {
                entries = Project.Locator.FileDictionary.Entries
                    .Where(e => e.Folder.StartsWith($"/map/{mapid.Substring(0, 3)}/{mapid}"))
                    .Where(e => e.Extension == "hkxbhd")
                    .Where(e => !e.Archive.Contains("sd"))
                    .ToList();
            }

            foreach (var entry in entries)
            {
                CollisionFiles.Entries.Add(entry);
            }
        }

        // Map Parts
        MapPieceFiles.Entries = new();

        if (Project.Descriptor.ProjectType is ProjectType.DS2S or ProjectType.DS2)
        {
            MapPieceFiles.Entries = Project.Locator.FileDictionary.Entries
                .Where(e => e.Folder.StartsWith($"/model/map"))
                .Where(e => e.Extension == "mapbhd")
                .ToList();
        }

        foreach (var map in MapFiles.Entries)
        {
            var mapid = map.Filename;
            var entries = new List<FileDictionaryEntry>();

            if (Project.Descriptor.ProjectType is ProjectType.DS1)
            {
                entries = Project.Locator.FileDictionary.Entries
                    .Where(e => e.Folder.StartsWith($"/map/{mapid}"))
                    .Where(e => e.Extension == "flver")
                    .ToList();
            }
            else if (Project.Descriptor.ProjectType is ProjectType.DS1R)
            {
                entries = Project.Locator.FileDictionary.Entries
                    .Where(e => e.Folder.StartsWith($"/map/{mapid}"))
                    .Where(e => e.Extension == "flver")
                    .ToList();
            }
            else if (Project.Descriptor.ProjectType is ProjectType.BB or ProjectType.DES)
            {
                entries = Project.Locator.FileDictionary.Entries
                    .Where(e => e.Folder.StartsWith($"/map/{mapid}/"))
                    .Where(e => e.Extension == "flver")
                    .ToList();
            }
            else if (Project.Descriptor.ProjectType is ProjectType.ER or ProjectType.AC6 or ProjectType.NR)
            {
                entries = Project.Locator.FileDictionary.Entries
                    .Where(e => e.Folder.StartsWith($"/map/{mapid.Substring(0, 3)}/{mapid}"))
                    .Where(e => e.Extension == "mapbnd")
                    .Where(e => !e.Archive.Contains("sd"))
                    .ToList();
            }
            else
            {
                entries = Project.Locator.FileDictionary.Entries
                    .Where(e => e.Folder.StartsWith($"/map/{mapid}"))
                    .Where(e => e.Extension == "mapbnd")
                    .ToList();
            }

            foreach (var entry in entries)
            {
                MapPieceFiles.Entries.Add(entry);
            }
        }
    }

    public void CompileTextDictionaries()
    {
        // MSGBND
        var msgbndDictionary = new FileDictionary();
        msgbndDictionary.Entries = Project.Locator.FileDictionary.Entries
            .Where(e => e.Archive != "sd")
            .Where(e => e.Folder.StartsWith("/msg"))
            .Where(e => e.Extension == "msgbnd")
            .ToList();


        if (Project.Descriptor.ProjectType == ProjectType.ER)
        {
            msgbndDictionary.Entries = msgbndDictionary.Entries.OrderBy(e => e.Folder).ThenBy(e => e.Filename.Contains("dlc02")).ThenBy(e => e.Filename.Contains("dlc01")).ThenBy(e => e.Filename).ToList();
        }

        // FMG
        var fmgDictionary = new FileDictionary();
        fmgDictionary.Entries = new List<FileDictionaryEntry>();

        if (Project.Descriptor.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            fmgDictionary.Entries = Project.Locator.FileDictionary.Entries
                .Where(e => e.Archive != "sd")
                .Where(e => e.Folder.StartsWith("/menu/text"))
                .Where(e => e.Extension == "fmg").ToList();
        }

        TextFiles = ProjectUtils.MergeFileDictionaries(msgbndDictionary, fmgDictionary);
    }

    public void CompileGparamDictionaries()
    {
        GparamFiles.Entries = Project.Locator.FileDictionary.Entries
            .Where(e => e.Archive != "sd")
            .Where(e => e.Folder.StartsWith("/param/drawparam"))
            .Where(e => e.Extension == "gparam")
            .ToList();
    }

    public void CompileMaterialDictionaries()
    {
        MTD_Files.Entries = Project.Locator.FileDictionary.Entries
            .Where(e => e.Archive != "sd")
            .Where(e => e.Folder.StartsWith("/mtd"))
            .Where(e => e.Extension == "mtdbnd")
            .ToList();

        // DS2 has it as a single .bnd file
        if (Project.Descriptor.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            MTD_Files.Entries = Project.Locator.FileDictionary.Entries
            .Where(e => e.Archive != "sd")
            .Where(e => e.Folder.StartsWith("/material"))
            .Where(e => e.Filename == "allmaterialbnd")
            .ToList();
        }

        MATBIN_Files.Entries = Project.Locator.FileDictionary.Entries
            .Where(e => e.Archive != "sd")
            .Where(e => e.Folder.StartsWith("/material"))
            .Where(e => e.Extension == "matbinbnd")
            .ToList();

    }

    public void CompileTextureDictionaries()
    {
        var secondaryDicts = new List<FileDictionary>();

        // TPF
        var baseDict = new FileDictionary();
        baseDict.Entries = Project.Locator.FileDictionary.Entries
            .Where(e => e.Archive != "sd")
            .Where(e => e.Extension == "tpf")
            .ToList();

        // Object Textures
        var objDict = new FileDictionary();
        objDict.Entries = Project.Locator.FileDictionary.Entries
            .Where(e => e.Archive != "sd")
            .Where(e => e.Extension == "objbnd")
            .ToList();

        if (Project.Descriptor.ProjectType is ProjectType.DS2S or ProjectType.DS2)
        {
            objDict.Entries = Project.Locator.FileDictionary.Entries
                .Where(e => e.Archive != "sd")
                .Where(e => e.Extension == "bnd" && e.Folder == "/model/obj")
                .ToList();
        }

        secondaryDicts.Add(objDict);

        // Chr Textures
        var chrDict = new FileDictionary();
        chrDict.Entries = Project.Locator.FileDictionary.Entries
            .Where(e => e.Archive != "sd")
            .Where(e => e.Extension == "texbnd")
            .ToList();

        secondaryDicts.Add(chrDict);

        // Part Textures
        var partDict = new FileDictionary();
        partDict.Entries = Project.Locator.FileDictionary.Entries
            .Where(e => e.Archive != "sd")
            .Where(e => e.Extension == "partsbnd")
            .ToList();

        if (Project.Descriptor.ProjectType is ProjectType.DS2S or ProjectType.DS2)
        {
            var commonPartDict = new FileDictionary();
            commonPartDict.Entries = Project.Locator.FileDictionary.Entries
                .Where(e => e.Archive != "sd")
                .Where(e => e.Extension == "commonbnd")
                .ToList();

            secondaryDicts.Add(commonPartDict);

            partDict.Entries = Project.Locator.FileDictionary.Entries
                .Where(e => e.Archive != "sd")
                .Where(e => e.Extension == "bnd" && e.Folder.Contains("/model/parts"))
                .ToList();

            secondaryDicts.Add(partDict);
        }
        else
        {
            secondaryDicts.Add(partDict);
        }

        // SFX Textures
        var sfxDict = new FileDictionary();
        sfxDict.Entries = Project.Locator.FileDictionary.Entries
            .Where(e => e.Archive != "sd")
            .Where(e => e.Extension == "ffxbnd")
            .ToList();

        secondaryDicts.Add(sfxDict);

        // Merge all unique entries from the secondary dicts into the base dict to form the final dictionary
        TextureFiles = ProjectUtils.MergeFileDictionaries(baseDict, secondaryDicts);

        TexturePackedFiles.Entries = Project.Locator.FileDictionary.Entries
            .Where(e => e.Archive != "sd")
            .Where(e => e.Extension == "tpfbhd")
            .ToList();

        ShoeboxFiles.Entries = Project.Locator.FileDictionary.Entries
            .Where(e => e.Archive != "sd")
            .Where(e => e.Extension == "sblytbnd")
            .ToList();
    }
    #endregion

    #region Dispose
    public void Dispose()
    {
        FileDictionary = null;
        MapFiles = null;
        LightFiles = null;
        DS2_LightFiles = null;
        NavmeshFiles = null;
        CollisionFiles = null;
        AutoInvadeFiles = null;
        LightAtlasFiles = null;
        LightProbeFiles = null;
        GparamFiles = null;
        TextFiles = null;
        MTD_Files = null;
        MATBIN_Files = null;
        MapPieceFiles = null;
        ChrFiles = null;
        AssetFiles = null;
        PartFiles = null;
        CollisionFiles = null;
        MapFiles = null;
        TextureFiles = null;
        TexturePackedFiles = null;
        ShoeboxFiles = null;

    }
    #endregion
}