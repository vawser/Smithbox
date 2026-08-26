using DotNext.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace StudioCore.Developer;

public class DataProjectFileLocator
{
    public DataProjectEntry Project;

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

    public FileDictionary EntryFileListFiles = new();

    public FileDictionary GparamFiles = new();
    public FileDictionary TextFiles = new();

    public FileDictionary MTD_Files = new();
    public FileDictionary MATBIN_Files = new();

    public FileDictionary TextureFiles = new();
    public FileDictionary TexturePackedFiles = new();
    public FileDictionary ShoeboxFiles = new();

    public FileDictionary TimeActFiles = new();
    public FileDictionary BehaviorFiles = new();

    public FileDictionary HavokCollisionFiles = new();

    public DataProjectFileLocator(DataProjectEntry project)
    {
        Project = project;
    }

    #region Init
    public async Task Initialize()
    {
        var folder = Path.Join(AppContext.BaseDirectory, "Assets", "File Dictionaries");
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
                    Smithbox.LogError(this, LOC.Get("PROJECT_Setup_File_Directory_Derserialize_FAIL", filepath), e);
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("PROJECT_Setup_File_Directory_Read_FAIL", filepath), e);
            }
        }

        FileDictionary = jsonFileDictionary;

        CompileDictionaries();

        return;
    }

    public static string NormalizePath(string path)
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
        var mapFiles = new HashSet<FileDictionaryEntry>();
        var chrFiles = new HashSet<FileDictionaryEntry>();
        var assetFiles = new HashSet<FileDictionaryEntry>();
        var partFiles = new HashSet<FileDictionaryEntry>();
        var collisionFiles = new HashSet<FileDictionaryEntry>();
        var mapPieceFiles = new HashSet<FileDictionaryEntry>();
        var lightFiles = new HashSet<FileDictionaryEntry>();
        var ds2LightFiles = new HashSet<FileDictionaryEntry>();
        var navmeshFiles = new HashSet<FileDictionaryEntry>();
        var autoInvadeFiles = new HashSet<FileDictionaryEntry>();
        var lightAtlasFiles = new HashSet<FileDictionaryEntry>();
        var lightProbeFiles = new HashSet<FileDictionaryEntry>();
        var gparamFiles = new HashSet<FileDictionaryEntry>();
        var textFiles = new HashSet<FileDictionaryEntry>();
        var mtdFiles = new HashSet<FileDictionaryEntry>();
        var matbinFiles = new HashSet<FileDictionaryEntry>();
        var textureFiles = new HashSet<FileDictionaryEntry>();
        var texturePackedFiles = new HashSet<FileDictionaryEntry>();
        var shoeboxFiles = new HashSet<FileDictionaryEntry>();
        var timeActFiles = new HashSet<FileDictionaryEntry>();
        var behaviorFiles = new HashSet<FileDictionaryEntry>();
        var entryFileListFiles = new HashSet<FileDictionaryEntry>();
        var havokCollisionFiles = new HashSet<FileDictionaryEntry>();

        // Single pass - check each entry once
        foreach (var entry in allEntries)
        {
            var ext = entry.Extension;
            var folder = entry.Folder;
            var archive = entry.Archive;
            var isMap = folder.StartsWith("/map");
            var isSd = archive.Contains("sd");
            var isDs2Map = folder.StartsWith("/model/map");

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
            {
                collisionFiles.Add(entry);
                havokCollisionFiles.Add(entry);
            }

            // Map piece files
            if (ShouldAddToMapPieceFiles(entry, projectType, isMap, isSd))
                mapPieceFiles.Add(entry);

            // Light files
            if (isMap && ext == "btl" && !isSd)
                lightFiles.Add(entry);

            // Navmesh
            if (isMap && ext == "nva" && !isSd)
                navmeshFiles.Add(entry);

            // Auto invade
            if (folder.StartsWith("/other") && ext == "aipbnd" && !isSd)
                autoInvadeFiles.Add(entry);

            // Light atlas/probe
            if (isMap && ext == "btab" && !isSd)
                lightAtlasFiles.Add(entry);
            if (isMap && ext == "btpb" && !isSd)
                lightProbeFiles.Add(entry);

            // DS2 Light/Light Atlas/Light Probe
            if (isDs2Map && ext == "gibhd" && !isSd)
            {
                ds2LightFiles.Add(entry);
                lightAtlasFiles.Add(entry);
                lightProbeFiles.Add(entry);
            }

            // Entry File List
            if (projectType is ProjectType.DS3 or ProjectType.BB or ProjectType.SDT or ProjectType.ER or ProjectType.AC6)
            {
                if (ext == "entryfilelist" && !isSd)
                {
                    entryFileListFiles.Add(entry);
                }
            }

            // Gparam
            if (folder.StartsWith("/param") && ext == "gparam" && !isSd)
                gparamFiles.Add(entry);

            if (projectType is ProjectType.BB)
            {
                if (folder.StartsWith("/param") && ext == "gparambnd" && !isSd)
                    gparamFiles.Add(entry);
            }

            if (projectType is ProjectType.DS2 or ProjectType.DS2S)
            {
                if (folder.StartsWith("/filter") && ext == "fltparam" && !isSd)
                    gparamFiles.Add(entry);
            }

            // Text files
            if (folder.StartsWith("/msg") && ext == "msgbnd" && !isSd)
                textFiles.Add(entry);

            if (projectType is ProjectType.DS2 or ProjectType.DS2S && folder.StartsWith("/menu") && ext == "fmg" && !isSd)
                textFiles.Add(entry);

            // Materials
            if (ShouldAddToMtdFiles(entry, projectType) && !isSd)
                mtdFiles.Add(entry);

            if (folder.StartsWith("/material") && ext == "matbinbnd" && !isSd)
                matbinFiles.Add(entry);

            // Textures
            if (ShouldAddToTextureFiles(entry, projectType) && !isSd)
                textureFiles.Add(entry);

            if (ext == "tpfbhd" && !isSd)
                texturePackedFiles.Add(entry);
            if (ext == "sblytbnd" && !isSd)
                shoeboxFiles.Add(entry);

            // Animation
            if (ext == "anibnd" && !isSd)
                timeActFiles.Add(entry);
            if (ext == "behbnd" && !isSd)
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
        EntryFileListFiles.Entries = entryFileListFiles;
        HavokCollisionFiles.Entries = havokCollisionFiles;

        // Special handling for text files
        if (projectType == ProjectType.ER && textFiles.Count > 0)
        {
            TextFiles.Entries = textFiles
                .OrderBy(e => e.Folder)
                .ThenBy(e => e.Filename.Contains("dlc02"))
                .ThenBy(e => e.Filename.Contains("dlc01"))
                .ThenBy(e => e.Filename)
                .ToHashSet();
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
