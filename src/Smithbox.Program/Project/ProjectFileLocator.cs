using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
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

    public FileDictionary TimeActFiles = new();
    public FileDictionary BehaviorFiles = new();

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

            var add = false;

            // Skip any new base dir files
            if (relativeFolder == "/")
                continue;

            if (CFG.Current.Project_Scan_Directory_Strict_Mode)
            {
                // If project relative path already exists in vanilla directory, ignore it as we don't need to include it
                if (existingPaths.Contains(relativePath))
                    continue;

                // Add if it is a file in a new map/aeg/aet folder
                if (relativeFolder.Contains("/map") || relativeFolder.Contains("/aeg") || relativeFolder.Contains("/aet"))
                    add = true;

                // Add if it is a new file in any of the vanilla directories
                foreach (var entry in existingFolders)
                {
                    if (entry == relativeFolder)
                    {
                        add = true;
                    }
                }
            }
            else
            {
                add = true;
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
        var allEntries = FileDictionary.Entries;
        var projectType = Project.Descriptor.ProjectType;

        // Initialize all lists
        var mapFiles = new List<FileDictionaryEntry>();
        var chrFiles = new List<FileDictionaryEntry>();
        var assetFiles = new List<FileDictionaryEntry>();
        var partFiles = new List<FileDictionaryEntry>();
        var collisionFiles = new List<FileDictionaryEntry>();
        var mapPieceFiles = new List<FileDictionaryEntry>();
        var lightFiles = new List<FileDictionaryEntry>();
        var ds2LightFiles = new List<FileDictionaryEntry>();
        var navmeshFiles = new List<FileDictionaryEntry>();
        var autoInvadeFiles = new List<FileDictionaryEntry>();
        var lightAtlasFiles = new List<FileDictionaryEntry>();
        var lightProbeFiles = new List<FileDictionaryEntry>();
        var gparamFiles = new List<FileDictionaryEntry>();
        var textFiles = new List<FileDictionaryEntry>();
        var mtdFiles = new List<FileDictionaryEntry>();
        var matbinFiles = new List<FileDictionaryEntry>();
        var textureFiles = new List<FileDictionaryEntry>();
        var texturePackedFiles = new List<FileDictionaryEntry>();
        var shoeboxFiles = new List<FileDictionaryEntry>();
        var timeActFiles = new List<FileDictionaryEntry>();
        var behaviorFiles = new List<FileDictionaryEntry>();

        // Single pass - check each entry once
        foreach (var entry in allEntries)
        {
            var ext = entry.Extension;
            var folder = entry.Folder;
            var archive = entry.Archive;
            var isMap = folder.StartsWith("/map");
            var isSd = archive.Contains("sd");

            // Map files
            if (ShouldAddToMapFiles(entry, isMap, isSd))
                mapFiles.Add(entry);

            // Character files
            if (ShouldAddToChrFiles(entry, projectType, isSd))
                chrFiles.Add(entry);

            // Asset files
            if (ShouldAddToAssetFiles(entry, projectType, isSd))
                assetFiles.Add(entry);

            // Part files
            if (ShouldAddToPartFiles(entry, projectType, isSd))
                partFiles.Add(entry);

            // Collision files
            if (ShouldAddToCollisionFiles(entry, projectType, isMap, isSd))
                collisionFiles.Add(entry);

            // Map piece files
            if (ShouldAddToMapPieceFiles(entry, projectType, isMap, isSd))
                mapPieceFiles.Add(entry);

            // Light files
            if (isMap && ext == "btl")
                lightFiles.Add(entry);
            if (isMap && ext == "gibhd")
                ds2LightFiles.Add(entry);

            // Navmesh
            if (isMap && ext == "nva")
                navmeshFiles.Add(entry);

            // Auto invade
            if (folder.StartsWith("/other") && ext == "aipbnd")
                autoInvadeFiles.Add(entry);

            // Light atlas/probe
            if (isMap && ext == "btab")
                lightAtlasFiles.Add(entry);
            if (isMap && ext == "btpb")
                lightProbeFiles.Add(entry);

            // Gparam
            if (folder.StartsWith("/param") && ext == "gparam")
                gparamFiles.Add(entry);

            // Text files
            if (folder.StartsWith("/msg") && ext == "msgbnd")
                textFiles.Add(entry);
            if (projectType is ProjectType.DS2 or ProjectType.DS2S && folder.StartsWith("/menu") && ext == "fmg")
                textFiles.Add(entry);

            // Materials
            if (ShouldAddToMtdFiles(entry, projectType))
                mtdFiles.Add(entry);

            if (folder.StartsWith("/material") && ext == "matbinbnd")
                matbinFiles.Add(entry);

            // Textures
            if (ShouldAddToTextureFiles(entry, projectType))
                textureFiles.Add(entry);

            if (ext == "tpfbhd")
                texturePackedFiles.Add(entry);
            if (ext == "sblytbnd")
                shoeboxFiles.Add(entry);

            // Animation
            if (ext == "anibnd")
                timeActFiles.Add(entry);
            if (ext == "behbnd")
                behaviorFiles.Add(entry);
        }

        // Assign to public properties
        MapFiles.Entries = mapFiles;
        ChrFiles.Entries = chrFiles;
        AssetFiles.Entries = assetFiles;
        PartFiles.Entries = partFiles;
        CollisionFiles.Entries = collisionFiles;
        MapPieceFiles.Entries = mapPieceFiles;
        LightFiles.Entries = lightFiles;
        DS2_LightFiles.Entries = ds2LightFiles;
        NavmeshFiles.Entries = navmeshFiles;
        AutoInvadeFiles.Entries = autoInvadeFiles;
        LightAtlasFiles.Entries = lightAtlasFiles;
        LightProbeFiles.Entries = lightProbeFiles;
        GparamFiles.Entries = gparamFiles;
        MTD_Files.Entries = mtdFiles;
        MATBIN_Files.Entries = matbinFiles;
        TextureFiles.Entries = textureFiles;
        TexturePackedFiles.Entries = texturePackedFiles;
        ShoeboxFiles.Entries = shoeboxFiles;
        TimeActFiles.Entries = timeActFiles;
        BehaviorFiles.Entries = behaviorFiles;

        // Special handling for text files
        if (projectType == ProjectType.ER && textFiles.Count > 0)
        {
            TextFiles.Entries = textFiles
                .OrderBy(e => e.Folder)
                .ThenBy(e => e.Filename.Contains("dlc02"))
                .ThenBy(e => e.Filename.Contains("dlc01"))
                .ThenBy(e => e.Filename)
                .ToList();
        }
        else
        {
            TextFiles.Entries = textFiles;
        }
    }

    // Helper methods to check if entries should be added to specific categories

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool ShouldAddToMapFiles(FileDictionaryEntry entry, bool isMap, bool isSd)
    {
        return isMap && entry.Extension == "msb" && !entry.Folder.Contains("autoroute") && !isSd;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool ShouldAddToChrFiles(FileDictionaryEntry entry, ProjectType projectType, bool isSd)
    {
        if (projectType is ProjectType.DS2S or ProjectType.DS2)
            return entry.Extension == "bnd" && entry.Folder.StartsWith("/model/chr");

        return entry.Extension == "chrbnd" && !isSd;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool ShouldAddToAssetFiles(FileDictionaryEntry entry, ProjectType projectType, bool isSd)
    {
        return projectType switch
        {
            ProjectType.DS1 => entry.Extension == "objbnd" && entry.Folder.StartsWith("/obj"),
            ProjectType.DS2S or ProjectType.DS2 => entry.Extension == "bnd" && entry.Folder.StartsWith("/model/obj"),
            ProjectType.DS3 or ProjectType.BB or ProjectType.SDT => entry.Extension == "objbnd" && entry.Folder.StartsWith("/obj"),
            ProjectType.ER or ProjectType.AC6 or ProjectType.NR => entry.Extension == "geombnd" && entry.Folder.StartsWith("/asset") && !isSd,
            _ => false
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool ShouldAddToPartFiles(FileDictionaryEntry entry, ProjectType projectType, bool isSd)
    {
        return projectType switch
        {
            ProjectType.DS1 => entry.Extension == "partsbnd" && entry.Folder.StartsWith("/parts"),
            ProjectType.DS2S or ProjectType.DS2 => entry.Extension == "bnd" && entry.Folder.StartsWith("/model/parts"),
            ProjectType.DS3 or ProjectType.BB or ProjectType.SDT => entry.Extension == "partsbnd" && entry.Folder.StartsWith("/parts"),
            ProjectType.ER or ProjectType.AC6 or ProjectType.NR => entry.Extension == "partsbnd" && entry.Folder.StartsWith("/parts") && !isSd,
            _ => false
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool ShouldAddToCollisionFiles(FileDictionaryEntry entry, ProjectType projectType, bool isMap, bool isSd)
    {
        return projectType switch
        {
            ProjectType.DS2S or ProjectType.DS2 => entry.Extension == "hkxbhd" && entry.Folder.StartsWith("/model/map"),
            ProjectType.DS1 or ProjectType.DES => isMap && entry.Extension == "hkx",
            ProjectType.DS1R or ProjectType.DS3 or ProjectType.BB or ProjectType.SDT => isMap && entry.Extension == "hkxbhd",
            ProjectType.ER or ProjectType.AC6 or ProjectType.NR => isMap && entry.Extension == "hkxbhd" && !isSd,
            _ => false
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool ShouldAddToMapPieceFiles(FileDictionaryEntry entry, ProjectType projectType, bool isMap, bool isSd)
    {
        return projectType switch
        {
            ProjectType.DS2S or ProjectType.DS2 => entry.Extension == "mapbhd" && entry.Folder.StartsWith("/model/map"),
            ProjectType.DS1 or ProjectType.DS1R or ProjectType.BB or ProjectType.DES => isMap && entry.Extension == "flver",
            ProjectType.ER or ProjectType.AC6 or ProjectType.NR => isMap && entry.Extension == "mapbnd" && !isSd,
            _ => isMap && entry.Extension == "mapbnd"
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool ShouldAddToMtdFiles(FileDictionaryEntry entry, ProjectType projectType)
    {
        if (projectType is ProjectType.DS2 or ProjectType.DS2S)
            return entry.Extension == "bnd" && entry.Folder.StartsWith("/material") && entry.Filename == "allmaterialbnd";

        return entry.Extension == "mtdbnd" && entry.Folder.StartsWith("/mtd");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool ShouldAddToTextureFiles(FileDictionaryEntry entry, ProjectType projectType)
    {
        var ext = entry.Extension;
        var folder = entry.Folder;

        // Standalone texture files
        if (ext == "tpf")
            return true;

        // Texture container files
        if (ext is "texbnd" or "ffxbnd" or "commonbnd")
            return true;

        // Model bundles that contain textures
        if (ext == "objbnd")
            return true;

        if (ext == "partsbnd")
            return true;

        // DS2/DS2S special cases - bnd files in specific folders
        if (projectType is ProjectType.DS2S or ProjectType.DS2)
        {
            if (ext == "bnd" && (folder == "/model/obj" || folder.Contains("/model/parts")))
                return true;
        }

        return false;
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
