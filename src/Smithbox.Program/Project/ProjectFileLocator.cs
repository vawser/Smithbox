using System;
using System.Collections.Concurrent;
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
                    if(entry == relativeFolder)
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
    private MultiIndex BuildMultiIndex()
    {
        var allEntries = Project.Locator.FileDictionary.Entries;
        var count = allEntries.Count;

        // Pre-allocate arrays with exact size
        var allArray = new FileDictionaryEntry[count];
        var nonSdList = new List<FileDictionaryEntry>(count);
        var mapList = new List<FileDictionaryEntry>(count / 10); // Estimate ~10% are map files

        var byExtension = new Dictionary<string, List<FileDictionaryEntry>>(50); // ~50 unique extensions
        var byFolderStart = new Dictionary<string, List<FileDictionaryEntry>>(20); // ~20 unique folder starts
        var extAndFolder = new Dictionary<string, Dictionary<string, List<FileDictionaryEntry>>>(50);

        var lockObj = new object();
        int index = 0;

        Parallel.ForEach(allEntries, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, entry =>
        {
            int localIndex;
            lock (lockObj)
            {
                localIndex = index++;
            }

            allArray[localIndex] = entry;

            // Build indexes in thread-local storage first
            var isNonSd = entry.Archive != "sd";
            var isMap = entry.Folder.AsSpan().StartsWith("/map");
            var extension = entry.Extension;
            var folderStart = GetFolderStart(entry.Folder);

            // Collect to thread-safe structures
            if (isNonSd)
            {
                lock (nonSdList) { nonSdList.Add(entry); }
            }

            if (isMap)
            {
                lock (mapList) { mapList.Add(entry); }
            }

            // Build extension index
            lock (byExtension)
            {
                if (!byExtension.TryGetValue(extension, out var extList))
                {
                    extList = new List<FileDictionaryEntry>();
                    byExtension[extension] = extList;
                }
                extList.Add(entry);
            }

            // Build folder start index
            lock (byFolderStart)
            {
                if (!byFolderStart.TryGetValue(folderStart, out var folderList))
                {
                    folderList = new List<FileDictionaryEntry>();
                    byFolderStart[folderStart] = folderList;
                }
                folderList.Add(entry);
            }

            // Build combined index for ultra-fast double-key lookups
            if (isNonSd)
            {
                lock (extAndFolder)
                {
                    if (!extAndFolder.TryGetValue(extension, out var folderDict))
                    {
                        folderDict = new Dictionary<string, List<FileDictionaryEntry>>();
                        extAndFolder[extension] = folderDict;
                    }

                    if (!folderDict.TryGetValue(folderStart, out var entries))
                    {
                        entries = new List<FileDictionaryEntry>();
                        folderDict[folderStart] = entries;
                    }

                    entries.Add(entry);
                }
            }
        });

        return new MultiIndex
        {
            AllArray = allArray,
            NonSdArray = nonSdList.ToArray(),
            MapArray = mapList.ToArray(),
            ByExtension = byExtension,
            ByFolderStart = byFolderStart,
            ExtensionAndFolder = extAndFolder,
            MapIds = new HashSet<string>(mapList.Select(m => m.Filename))
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string GetFolderStart(string folder)
    {
        var span = folder.AsSpan();
        var slashIndex = span.IndexOf("/", StringComparison.CurrentCultureIgnoreCase);
        return slashIndex > 0 ? folder.Substring(0, slashIndex) : folder;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static List<FileDictionaryEntry> UltraFastFilter(
        FileDictionaryEntry[] entries,
        string folderPrefix = null,
        string extension = null,
        bool excludeAutoroute = false,
        bool excludeSd = false)
    {
        var result = new List<FileDictionaryEntry>(entries.Length / 10);

        var folderSpan = folderPrefix.AsSpan();
        var hasFolder = folderPrefix != null;

        for (int i = 0; i < entries.Length; i++)
        {
            ref readonly var entry = ref entries[i];

            if (hasFolder && !entry.Folder.AsSpan().StartsWith(folderSpan))
                continue;

            if (extension != null && entry.Extension != extension)
                continue;

            if (excludeAutoroute && entry.Folder.Contains("autoroute"))
                continue;

            if (excludeSd && entry.Archive.Contains("sd"))
                continue;

            result.Add(entry);
        }

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static List<FileDictionaryEntry> FastLookup(
        MultiIndex index,
        string extension,
        string folderStart = null)
    {
        if (folderStart != null &&
            index.ExtensionAndFolder.TryGetValue(extension, out var folderDict) &&
            folderDict.TryGetValue(folderStart, out var entries))
        {
            return new List<FileDictionaryEntry>(entries); // Return copy
        }

        if (index.ByExtension.TryGetValue(extension, out var extEntries))
        {
            if (folderStart != null)
            {
                // Filter by folder
                var result = new List<FileDictionaryEntry>(extEntries.Count / 4);
                foreach (var entry in extEntries)
                {
                    if (entry.Folder.StartsWith(folderStart))
                        result.Add(entry);
                }
                return result;
            }
            return new List<FileDictionaryEntry>(extEntries);
        }

        return new List<FileDictionaryEntry>();
    }

    public void CompileDictionaries()
    {
        var index = BuildMultiIndex();

        var tasks = new[]
        {
            Task.Run(() => CompileMapDictionaries(index)),
            Task.Run(() => CompileModelDictionaries(index)),
            Task.Run(() => CompileTextDictionaries(index)),
            Task.Run(() => CompileGparamDictionaries(index)),
            Task.Run(() => CompileMaterialDictionaries(index)),
            Task.Run(() => CompileTextureDictionaries(index))
        };

        Task.WaitAll(tasks);
    }

    public void CompileMapDictionaries(MultiIndex index)
    {
        var mapArray = index.MapArray;

        // Parallel processing of independent filters
        Parallel.Invoke(
            () => MapFiles.Entries = UltraFastFilter(mapArray, extension: "msb", excludeAutoroute: true),
            () => LightFiles.Entries = FastLookup(index, "btl", "/map"),
            () => DS2_LightFiles.Entries = FastLookup(index, "gibhd", "/map"),
            () => NavmeshFiles.Entries = FastLookup(index, "nva", "/map"),
            () => CollisionFiles.Entries = FastLookup(index, "hkxbhd", "/map"),
            () => LightAtlasFiles.Entries = FastLookup(index, "btab", "/map"),
            () => LightProbeFiles.Entries = FastLookup(index, "btpb", "/map"),
            () => AutoInvadeFiles.Entries = FastLookup(index, "aipbnd", "/other")
        );
    }

    public void CompileModelDictionaries(MultiIndex index)
    {
        var projectType = Project.Descriptor.ProjectType;

        var mapTask = Task.Run(() =>
        {
            MapFiles.Entries = UltraFastFilter(
                index.AllArray,
                folderPrefix: "/map",
                extension: "msb",
                excludeAutoroute: true,
                excludeSd: true);
        });

        var chrTask = Task.Run(() =>
        {
            if (projectType is ProjectType.DS2S or ProjectType.DS2)
            {
                ChrFiles.Entries = FastLookup(index, "bnd", "/model");
                ChrFiles.Entries = ChrFiles.Entries.Where(e => e.Folder.StartsWith("/model/chr")).ToList();
            }
            else
            {
                ChrFiles.Entries = FastLookup(index, "chrbnd");
                ChrFiles.Entries = ChrFiles.Entries.Where(e => !e.Archive.Contains("sd")).ToList();
            }
        });

        var assetTask = Task.Run(() => AssetFiles.Entries = GetAssetFilesOptimized(index, projectType));
        var partTask = Task.Run(() => PartFiles.Entries = GetPartFilesOptimized(index, projectType));

        Task.WaitAll(mapTask, chrTask, assetTask, partTask);

        CompileCollisionFilesUltraFast(index, projectType);
        CompileMapPieceFilesUltraFast(index, projectType);
    }

    private List<FileDictionaryEntry> GetAssetFilesOptimized(MultiIndex index, ProjectType projectType)
    {
        return projectType switch
        {
            ProjectType.DS1 => FastLookup(index, "objbnd", "/obj"),
            ProjectType.DS2S or ProjectType.DS2 => FastLookup(index, "bnd", "/model").Where(e => e.Folder.StartsWith("/model/obj")).ToList(),
            ProjectType.DS3 or ProjectType.BB or ProjectType.SDT => FastLookup(index, "objbnd", "/obj"),
            ProjectType.ER or ProjectType.AC6 or ProjectType.NR => FastLookup(index, "geombnd", "/asset").Where(e => !e.Archive.Contains("sd")).ToList(),
            _ => new List<FileDictionaryEntry>()
        };
    }

    private List<FileDictionaryEntry> GetPartFilesOptimized(MultiIndex index, ProjectType projectType)
    {
        return projectType switch
        {
            ProjectType.DS1 => FastLookup(index, "partsbnd", "/parts"),
            ProjectType.DS2S or ProjectType.DS2 => FastLookup(index, "bnd", "/model").Where(e => e.Folder.StartsWith("/model/parts")).ToList(),
            ProjectType.DS3 or ProjectType.BB or ProjectType.SDT => FastLookup(index, "partsbnd", "/parts"),
            ProjectType.ER or ProjectType.AC6 or ProjectType.NR => FastLookup(index, "partsbnd", "/parts").Where(e => !e.Archive.Contains("sd")).ToList(),
            _ => new List<FileDictionaryEntry>()
        };
    }

    private void CompileCollisionFilesUltraFast(MultiIndex index, ProjectType projectType)
    {
        var collisions = new ConcurrentBag<FileDictionaryEntry>();

        if (projectType is ProjectType.DS2S or ProjectType.DS2)
        {
            var ds2Collisions = FastLookup(index, "hkxbhd", "/model");
            foreach (var entry in ds2Collisions.Where(e => e.Folder.StartsWith("/model/map")))
            {
                collisions.Add(entry);
            }
        }

        // Parallel map processing
        var mapEntries = MapFiles.Entries;

        Parallel.ForEach(mapEntries, map =>
        {
            var mapid = map.Filename;
            var entries = GetCollisionEntriesForMapOptimized(index, mapid, projectType);

            foreach (var entry in entries)
            {
                collisions.Add(entry);
            }
        });

        CollisionFiles.Entries = collisions.ToList();
    }

    private List<FileDictionaryEntry> GetCollisionEntriesForMapOptimized(
        MultiIndex index,
        string mapid,
        ProjectType projectType)
    {
        return projectType switch
        {
            ProjectType.DS1 or ProjectType.DES =>
                FastLookup(index, "hkx", "/map").Where(e => e.Folder.StartsWith($"/map/{mapid}")).ToList(),

            ProjectType.DS1R or ProjectType.DS3 or ProjectType.BB or ProjectType.SDT =>
                FastLookup(index, "hkxbhd", "/map").Where(e => e.Folder.StartsWith($"/map/{mapid}")).ToList(),

            ProjectType.ER or ProjectType.AC6 or ProjectType.NR =>
                FastLookup(index, "hkxbhd", "/map")
                    .Where(e => e.Folder.StartsWith($"/map/{mapid.Substring(0, 3)}/{mapid}") && !e.Archive.Contains("sd"))
                    .ToList(),

            _ => new List<FileDictionaryEntry>()
        };
    }

    private void CompileMapPieceFilesUltraFast(MultiIndex index, ProjectType projectType)
    {
        var mapPieces = new ConcurrentBag<FileDictionaryEntry>();

        if (projectType is ProjectType.DS2S or ProjectType.DS2)
        {
            var ds2Pieces = FastLookup(index, "mapbhd", "/model");
            foreach (var entry in ds2Pieces.Where(e => e.Folder.StartsWith("/model/map")))
            {
                mapPieces.Add(entry);
            }
        }

        var mapEntries = MapFiles.Entries;

        Parallel.ForEach(mapEntries, map =>
        {
            var mapid = map.Filename;
            var entries = GetMapPieceEntriesForMapOptimized(index, mapid, projectType);

            foreach (var entry in entries)
            {
                mapPieces.Add(entry);
            }
        });

        MapPieceFiles.Entries = mapPieces.ToList();
    }

    private List<FileDictionaryEntry> GetMapPieceEntriesForMapOptimized(
        MultiIndex index,
        string mapid,
        ProjectType projectType)
    {
        return projectType switch
        {
            ProjectType.DS1 or ProjectType.DS1R or ProjectType.BB or ProjectType.DES =>
                FastLookup(index, "flver", "/map").Where(e => e.Folder.StartsWith($"/map/{mapid}")).ToList(),

            ProjectType.ER or ProjectType.AC6 or ProjectType.NR =>
                FastLookup(index, "mapbnd", "/map")
                    .Where(e => e.Folder.StartsWith($"/map/{mapid.Substring(0, 3)}/{mapid}") && !e.Archive.Contains("sd"))
                    .ToList(),

            _ =>
                FastLookup(index, "mapbnd", "/map").Where(e => e.Folder.StartsWith($"/map/{mapid}")).ToList()
        };
    }

    public void CompileTextDictionaries(MultiIndex index)
    {
        var msgbndEntries = FastLookup(index, "msgbnd", "/msg");

        if (Project.Descriptor.ProjectType == ProjectType.ER)
        {
            msgbndEntries = msgbndEntries
                .OrderBy(e => e.Folder)
                .ThenBy(e => e.Filename.Contains("dlc02"))
                .ThenBy(e => e.Filename.Contains("dlc01"))
                .ThenBy(e => e.Filename)
                .ToList();
        }

        var msgbndDictionary = new FileDictionary { Entries = msgbndEntries };
        var fmgDictionary = new FileDictionary { Entries = new List<FileDictionaryEntry>() };

        if (Project.Descriptor.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            fmgDictionary.Entries = FastLookup(index, "fmg", "/menu");
        }

        TextFiles = ProjectUtils.MergeFileDictionaries(msgbndDictionary, fmgDictionary);
    }

    public void CompileGparamDictionaries(MultiIndex index)
    {
        GparamFiles.Entries = FastLookup(index, "gparam", "/param");
    }

    public void CompileMaterialDictionaries(MultiIndex index)
    {
        var projectType = Project.Descriptor.ProjectType;

        if (projectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            var materialEntries = FastLookup(index, "bnd", "/material");
            MTD_Files.Entries = materialEntries.Where(e => e.Filename == "allmaterialbnd").ToList();
        }
        else
        {
            MTD_Files.Entries = FastLookup(index, "mtdbnd", "/mtd");
        }

        MATBIN_Files.Entries = FastLookup(index, "matbinbnd", "/material");
    }

    public void CompileTextureDictionaries(MultiIndex index)
    {
        var projectType = Project.Descriptor.ProjectType;

        var baseDict = new FileDictionary();
        var objDict = new FileDictionary();
        var chrDict = new FileDictionary();
        var partDict = new FileDictionary();
        var commonPartDict = new FileDictionary();
        var sfxDict = new FileDictionary();

        Parallel.Invoke(
            () => baseDict.Entries = FastLookup(index, "tpf"),
            () =>
            {
                if (projectType is ProjectType.DS2S or ProjectType.DS2)
                    objDict.Entries = FastLookup(index, "bnd", "/model").Where(e => e.Folder == "/model/obj").ToList();
                else
                    objDict.Entries = FastLookup(index, "objbnd");
            },
            () => chrDict.Entries = FastLookup(index, "texbnd"),
            () =>
            {
                if (projectType is ProjectType.DS2S or ProjectType.DS2)
                {
                    commonPartDict.Entries = FastLookup(index, "commonbnd");
                    partDict.Entries = FastLookup(index, "bnd", "/model").Where(e => e.Folder.Contains("/model/parts")).ToList();
                }
                else
                {
                    partDict.Entries = FastLookup(index, "partsbnd");
                }
            },
            () => sfxDict.Entries = FastLookup(index, "ffxbnd"),
            () => TexturePackedFiles.Entries = FastLookup(index, "tpfbhd"),
            () => ShoeboxFiles.Entries = FastLookup(index, "sblytbnd")
        );

        var secondaryDicts = new List<FileDictionary> { objDict, chrDict, sfxDict };

        if (projectType is ProjectType.DS2S or ProjectType.DS2)
        {
            secondaryDicts.Add(commonPartDict);
            secondaryDicts.Add(partDict);
        }
        else
        {
            secondaryDicts.Add(partDict);
        }

        TextureFiles = ProjectUtils.MergeFileDictionaries(baseDict, secondaryDicts);
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

public class MultiIndex
{
    public Dictionary<string, Dictionary<string, List<FileDictionaryEntry>>> ExtensionAndFolder { get; set; }

    public Dictionary<string, List<FileDictionaryEntry>> ByExtension { get; set; }
    public Dictionary<string, List<FileDictionaryEntry>> ByFolderStart { get; set; }

    public FileDictionaryEntry[] NonSdArray { get; set; }
    public FileDictionaryEntry[] MapArray { get; set; }
    public FileDictionaryEntry[] AllArray { get; set; }

    public HashSet<string> MapIds { get; set; }
}
