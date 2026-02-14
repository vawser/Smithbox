using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
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

    public async Task Sort(Action<ProjectLoadProgress> reportProgress, bool silent = false)
    {
        await Task.Yield();

        MapFiles.Entries.Sort();
        MapPieceFiles.Entries.Sort();
        ChrFiles.Entries.Sort();
        AssetFiles.Entries.Sort();
        PartFiles.Entries.Sort();
        CollisionFiles.Entries.Sort();
        LightFiles.Entries.Sort();
        DS2_LightFiles.Entries.Sort();
        NavmeshFiles.Entries.Sort();
        AutoInvadeFiles.Entries.Sort();
        LightAtlasFiles.Entries.Sort();
        LightProbeFiles.Entries.Sort();

        GparamFiles.Entries.Sort();
        TextFiles.Entries.Sort();

        MTD_Files.Entries.Sort();
        MATBIN_Files.Entries.Sort();

        TextureFiles.Entries.Sort();
        TexturePackedFiles.Entries.Sort();
        ShoeboxFiles.Entries.Sort();

        TimeActFiles.Entries.Sort();
        BehaviorFiles.Entries.Sort();

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
    private MultiIndex BuildMultiIndex()
    {
        var allEntries = Project.Locator.FileDictionary.Entries;
        var count = allEntries.Count;

        // Pre-allocate arrays with exact size
        var allArray = new FileDictionaryEntry[count];
        var nonSdList = new ConcurrentBag<FileDictionaryEntry>();
        var mapList = new ConcurrentBag<FileDictionaryEntry>();

        var byExtension = new ConcurrentDictionary<string, ConcurrentBag<FileDictionaryEntry>>(Environment.ProcessorCount, 50);
        var byFolderStart = new ConcurrentDictionary<string, ConcurrentBag<FileDictionaryEntry>>(Environment.ProcessorCount, 20);
        var extAndFolder = new ConcurrentDictionary<string, ConcurrentDictionary<string, ConcurrentBag<FileDictionaryEntry>>>(Environment.ProcessorCount, 50);

        int index = -1;

        Parallel.ForEach(allEntries, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, entry =>
        {
            int localIndex = Interlocked.Increment(ref index);

            allArray[localIndex] = entry;

            // Build indexes in thread-local storage first
            var isNonSd = entry.Archive != "sd";
            var isMap = entry.Folder.StartsWith("/map");
            var extension = entry.Extension;
            var folderStart = GetFolderStart(entry.Folder);

            if (isNonSd)
            {
                nonSdList.Add(entry);
            }

            if (isMap)
            {
                mapList.Add(entry);
            }

            byExtension.GetOrAdd(extension, _ => new ConcurrentBag<FileDictionaryEntry>()).Add(entry);
            byFolderStart.GetOrAdd(folderStart, _ => new ConcurrentBag<FileDictionaryEntry>()).Add(entry);

            if (isNonSd)
            {
                var folderDict = extAndFolder.GetOrAdd(extension, _ => new ConcurrentDictionary<string, ConcurrentBag<FileDictionaryEntry>>());
                folderDict.GetOrAdd(folderStart, _ => new ConcurrentBag<FileDictionaryEntry>()).Add(entry);
            }
        });

        return new MultiIndex
        {
            AllArray = allArray,
            NonSdArray = nonSdList.ToArray(),
            MapArray = mapList.ToArray(),
            ByExtension = byExtension.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToList()),
            ByFolderStart = byFolderStart.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToList()),
            ExtensionAndFolder = extAndFolder.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.ToDictionary(inner => inner.Key, inner => inner.Value.ToList())),
            MapIds = new HashSet<string>(mapList.Select(m => m.Filename))
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string GetFolderStart(string folder)
    {
        var slashIndex = folder.IndexOf('/');
        return slashIndex > 0 ? folder.Substring(0, slashIndex) : folder;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static List<FileDictionaryEntry> FilterEntries(
        FileDictionaryEntry[] entries,
        string folderPrefix = null,
        string extension = null,
        bool excludeAutoroute = false,
        bool excludeSd = false)
    {
        var result = new List<FileDictionaryEntry>(entries.Length / 10);

        for (int i = 0; i < entries.Length; i++)
        {
            ref readonly var entry = ref entries[i];

            if (folderPrefix != null && !entry.Folder.StartsWith(folderPrefix))
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
    private static List<FileDictionaryEntry> GetEntriesByExtension(
        MultiIndex index,
        string extension,
        string folderStart = null)
    {
        if (folderStart != null &&
            index.ExtensionAndFolder.TryGetValue(extension, out var folderDict) &&
            folderDict.TryGetValue(folderStart, out var entries))
        {
            return new List<FileDictionaryEntry>(entries);
        }

        if (index.ByExtension.TryGetValue(extension, out var extEntries))
        {
            if (folderStart != null)
            {
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
            Task.Run(() => CompileTextureDictionaries(index)),
            Task.Run(() => CompileAnimDictionaries(index))
        };

        Task.WaitAll(tasks);
    }

    public void CompileMapDictionaries(MultiIndex index)
    {
        var mapArray = index.MapArray;

        Parallel.Invoke(
            () => MapFiles.Entries = FilterEntries(mapArray, extension: "msb", excludeAutoroute: true),
            () => LightFiles.Entries = GetEntriesByExtension(index, "btl", "/map"),
            () => DS2_LightFiles.Entries = GetEntriesByExtension(index, "gibhd", "/map"),
            () => NavmeshFiles.Entries = GetEntriesByExtension(index, "nva", "/map"),
            () => CollisionFiles.Entries = GetEntriesByExtension(index, "hkxbhd", "/map"),
            () => LightAtlasFiles.Entries = GetEntriesByExtension(index, "btab", "/map"),
            () => LightProbeFiles.Entries = GetEntriesByExtension(index, "btpb", "/map"),
            () => AutoInvadeFiles.Entries = GetEntriesByExtension(index, "aipbnd", "/other")
        );
    }

    public void CompileModelDictionaries(MultiIndex index)
    {
        var projectType = Project.Descriptor.ProjectType;

        var mapTask = Task.Run(() =>
        {
            MapFiles.Entries = FilterEntries(
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
                ChrFiles.Entries = GetEntriesByExtension(index, "bnd", "/model");
                var filtered = new List<FileDictionaryEntry>(ChrFiles.Entries.Count / 2);
                foreach (var entry in ChrFiles.Entries)
                {
                    if (entry.Folder.StartsWith("/model/chr"))
                        filtered.Add(entry);
                }
                ChrFiles.Entries = filtered;
            }
            else
            {
                ChrFiles.Entries = GetEntriesByExtension(index, "chrbnd");
                var filtered = new List<FileDictionaryEntry>(ChrFiles.Entries.Count);
                foreach (var entry in ChrFiles.Entries)
                {
                    if (!entry.Archive.Contains("sd"))
                        filtered.Add(entry);
                }
                ChrFiles.Entries = filtered;
            }
        });

        var assetTask = Task.Run(() => AssetFiles.Entries = GetAssetFiles(index, projectType));
        var partTask = Task.Run(() => PartFiles.Entries = GetPartFiles(index, projectType));

        Task.WaitAll(mapTask, chrTask, assetTask, partTask);

        CompileCollisionFiles(index, projectType);
        CompileMapPieceFiles(index, projectType);
    }

    private List<FileDictionaryEntry> GetAssetFiles(MultiIndex index, ProjectType projectType)
    {
        return projectType switch
        {
            ProjectType.DS1 => GetEntriesByExtension(index, "objbnd", "/obj"),
            ProjectType.DS2S or ProjectType.DS2 => FilterByFolderPrefix(GetEntriesByExtension(index, "bnd", "/model"), "/model/obj"),
            ProjectType.DS3 or ProjectType.BB or ProjectType.SDT => GetEntriesByExtension(index, "objbnd", "/obj"),
            ProjectType.ER or ProjectType.AC6 or ProjectType.NR => FilterExcludeArchive(GetEntriesByExtension(index, "geombnd", "/asset"), "sd"),
            _ => new List<FileDictionaryEntry>()
        };
    }

    private List<FileDictionaryEntry> GetPartFiles(MultiIndex index, ProjectType projectType)
    {
        return projectType switch
        {
            ProjectType.DS1 => GetEntriesByExtension(index, "partsbnd", "/parts"),
            ProjectType.DS2S or ProjectType.DS2 => FilterByFolderPrefix(GetEntriesByExtension(index, "bnd", "/model"), "/model/parts"),
            ProjectType.DS3 or ProjectType.BB or ProjectType.SDT => GetEntriesByExtension(index, "partsbnd", "/parts"),
            ProjectType.ER or ProjectType.AC6 or ProjectType.NR => FilterExcludeArchive(GetEntriesByExtension(index, "partsbnd", "/parts"), "sd"),
            _ => new List<FileDictionaryEntry>()
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static List<FileDictionaryEntry> FilterByFolderPrefix(List<FileDictionaryEntry> entries, string prefix)
    {
        var result = new List<FileDictionaryEntry>(entries.Count / 2);
        foreach (var entry in entries)
        {
            if (entry.Folder.StartsWith(prefix))
                result.Add(entry);
        }
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static List<FileDictionaryEntry> FilterExcludeArchive(List<FileDictionaryEntry> entries, string exclude)
    {
        var result = new List<FileDictionaryEntry>(entries.Count);
        foreach (var entry in entries)
        {
            if (!entry.Archive.Contains(exclude))
                result.Add(entry);
        }
        return result;
    }

    private void CompileCollisionFiles(MultiIndex index, ProjectType projectType)
    {
        var collisions = new ConcurrentBag<FileDictionaryEntry>();

        if (projectType is ProjectType.DS2S or ProjectType.DS2)
        {
            var ds2Collisions = GetEntriesByExtension(index, "hkxbhd", "/model");
            foreach (var entry in ds2Collisions)
            {
                if (entry.Folder.StartsWith("/model/map"))
                    collisions.Add(entry);
            }
        }

        var mapEntries = MapFiles.Entries;

        Parallel.ForEach(mapEntries, map =>
        {
            var mapid = map.Filename;
            var entries = GetCollisionEntriesForMap(index, mapid, projectType);

            foreach (var entry in entries)
            {
                collisions.Add(entry);
            }
        });

        CollisionFiles.Entries = collisions.ToList();
    }

    private List<FileDictionaryEntry> GetCollisionEntriesForMap(
        MultiIndex index,
        string mapid,
        ProjectType projectType)
    {
        return projectType switch
        {
            ProjectType.DS1 or ProjectType.DES =>
                FilterByFolderPrefix(GetEntriesByExtension(index, "hkx", "/map"), $"/map/{mapid}"),

            ProjectType.DS1R or ProjectType.DS3 or ProjectType.BB or ProjectType.SDT =>
                FilterByFolderPrefix(GetEntriesByExtension(index, "hkxbhd", "/map"), $"/map/{mapid}"),

            ProjectType.ER or ProjectType.AC6 or ProjectType.NR =>
                FilterByFolderPrefixAndExcludeArchive(
                    GetEntriesByExtension(index, "hkxbhd", "/map"),
                    $"/map/{mapid.Substring(0, 3)}/{mapid}",
                    "sd"),

            _ => new List<FileDictionaryEntry>()
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static List<FileDictionaryEntry> FilterByFolderPrefixAndExcludeArchive(
        List<FileDictionaryEntry> entries, string prefix, string excludeArchive)
    {
        var result = new List<FileDictionaryEntry>(entries.Count / 4);
        foreach (var entry in entries)
        {
            if (entry.Folder.StartsWith(prefix) && !entry.Archive.Contains(excludeArchive))
                result.Add(entry);
        }
        return result;
    }

    private void CompileMapPieceFiles(MultiIndex index, ProjectType projectType)
    {
        var mapPieces = new ConcurrentBag<FileDictionaryEntry>();

        if (projectType is ProjectType.DS2S or ProjectType.DS2)
        {
            var ds2Pieces = GetEntriesByExtension(index, "mapbhd", "/model");
            foreach (var entry in ds2Pieces)
            {
                if (entry.Folder.StartsWith("/model/map"))
                    mapPieces.Add(entry);
            }
        }

        var mapEntries = MapFiles.Entries;

        Parallel.ForEach(mapEntries, map =>
        {
            var mapid = map.Filename;
            var entries = GetMapPieceEntriesForMap(index, mapid, projectType);

            foreach (var entry in entries)
            {
                mapPieces.Add(entry);
            }
        });

        MapPieceFiles.Entries = mapPieces.ToList();
    }

    private List<FileDictionaryEntry> GetMapPieceEntriesForMap(
        MultiIndex index,
        string mapid,
        ProjectType projectType)
    {
        return projectType switch
        {
            ProjectType.DS1 or ProjectType.DS1R or ProjectType.BB or ProjectType.DES =>
                FilterByFolderPrefix(GetEntriesByExtension(index, "flver", "/map"), $"/map/{mapid}"),

            ProjectType.ER or ProjectType.AC6 or ProjectType.NR =>
                FilterByFolderPrefixAndExcludeArchive(
                    GetEntriesByExtension(index, "mapbnd", "/map"),
                    $"/map/{mapid.Substring(0, 3)}/{mapid}",
                    "sd"),

            _ =>
                FilterByFolderPrefix(GetEntriesByExtension(index, "mapbnd", "/map"), $"/map/{mapid}")
        };
    }

    public void CompileTextDictionaries(MultiIndex index)
    {
        var msgbndEntries = GetEntriesByExtension(index, "msgbnd", "/msg");

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
            fmgDictionary.Entries = GetEntriesByExtension(index, "fmg", "/menu");
        }

        TextFiles = ProjectUtils.MergeFileDictionaries(msgbndDictionary, fmgDictionary);
    }

    public void CompileGparamDictionaries(MultiIndex index)
    {
        GparamFiles.Entries = GetEntriesByExtension(index, "gparam", "/param");
    }

    public void CompileMaterialDictionaries(MultiIndex index)
    {
        var projectType = Project.Descriptor.ProjectType;

        if (projectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            var materialEntries = GetEntriesByExtension(index, "bnd", "/material");
            var filtered = new List<FileDictionaryEntry>(1);
            foreach (var entry in materialEntries)
            {
                if (entry.Filename == "allmaterialbnd")
                    filtered.Add(entry);
            }
            MTD_Files.Entries = filtered;
        }
        else
        {
            MTD_Files.Entries = GetEntriesByExtension(index, "mtdbnd", "/mtd");
        }

        MATBIN_Files.Entries = GetEntriesByExtension(index, "matbinbnd", "/material");
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
            () => baseDict.Entries = GetEntriesByExtension(index, "tpf"),
            () =>
            {
                if (projectType is ProjectType.DS2S or ProjectType.DS2)
                {
                    var entries = GetEntriesByExtension(index, "bnd", "/model");
                    var filtered = new List<FileDictionaryEntry>(entries.Count / 4);
                    foreach (var entry in entries)
                    {
                        if (entry.Folder == "/model/obj")
                            filtered.Add(entry);
                    }
                    objDict.Entries = filtered;
                }
                else
                    objDict.Entries = GetEntriesByExtension(index, "objbnd");
            },
            () => chrDict.Entries = GetEntriesByExtension(index, "texbnd"),
            () =>
            {
                if (projectType is ProjectType.DS2S or ProjectType.DS2)
                {
                    commonPartDict.Entries = GetEntriesByExtension(index, "commonbnd");
                    partDict.Entries = FilterByFolderPrefix(GetEntriesByExtension(index, "bnd", "/model"), "/model/parts");
                }
                else
                {
                    partDict.Entries = GetEntriesByExtension(index, "partsbnd");
                }
            },
            () => sfxDict.Entries = GetEntriesByExtension(index, "ffxbnd"),
            () => TexturePackedFiles.Entries = GetEntriesByExtension(index, "tpfbhd"),
            () => ShoeboxFiles.Entries = GetEntriesByExtension(index, "sblytbnd")
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
    private void CompileAnimDictionaries(MultiIndex index)
    {
        var anims = new ConcurrentBag<FileDictionaryEntry>();

        Parallel.Invoke(
            () => TimeActFiles.Entries = GetEntriesByExtension(index, "anibnd"),
            () => BehaviorFiles.Entries = GetEntriesByExtension(index, "behbnd")
        );
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
