using Andre.IO.VFS;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextureViewer;

public class TextureBank : IDisposable
{
    public ProjectEntry Project;

    public VirtualFileSystem TargetFS = EmptyVirtualFileSystem.Instance;

    public string Name;

    public Dictionary<FileDictionaryEntry, BinderContents> Entries = new();
    public Dictionary<FileDictionaryEntry, BinderContents> PackedEntries = new();

    public Dictionary<FileDictionaryEntry, ShoeboxLayoutContainer> ShoeboxEntries = new();

    public TextureBank(string name,  ProjectEntry project, VirtualFileSystem targetFs)
    {
        Project = project;
        Name = name;
        TargetFS = targetFs;
    }

    public async Task<bool> Setup()
    {
        await Task.Yield();

        Task<bool> textureTask = SetupTextures();
        bool textureTaskResult = await textureTask;

        Task<bool> packedTextureTask = SetupPackedTextures();
        bool packedTextureTaskResult = await packedTextureTask;

        SetupProjectFiles();

        Task<bool> shoeboxTask = SetupShoeboxLayouts();
        bool shoeboxTaskResult = await shoeboxTask;

        return true;
    }

    public async Task<bool> SetupTextures()
    {
        await Task.Yield();

        Entries = new();

        if (Project.Locator.TextureFiles.Entries != null)
        {
            foreach (var entry in Project.Locator.TextureFiles.Entries)
            {
                Entries.Add(entry, null);
            }
        }

        return true;
    }

    public async Task<bool> SetupPackedTextures()
    {
        await Task.Yield();

        PackedEntries = new();

        if (Project.Locator.TexturePackedFiles.Entries != null)
        {
            foreach (var entry in Project.Locator.TexturePackedFiles.Entries)
            {
                PackedEntries.Add(entry, null);
            }
        }

        return true;
    }

    public async Task<bool> SetupShoeboxLayouts()
    {
        await Task.Yield();

        ShoeboxEntries = new();

        foreach (var entry in Project.Locator.ShoeboxFiles.Entries)
        {
            var newShoeboxContainer = new ShoeboxLayoutContainer(Project, entry);

            await newShoeboxContainer.Setup();

            ShoeboxEntries.Add(entry, newShoeboxContainer);
        }

        if (Project.Descriptor.ProjectType is ProjectType.DS3 or ProjectType.BB)
        {
            var newFileEntry = new FileDictionaryEntry();
            newFileEntry.Archive = "";
            newFileEntry.Path = "";
            newFileEntry.Folder = "";
            newFileEntry.Filename = "icon_preview";
            newFileEntry.Extension = "";

            var newShoeboxContainer = new ShoeboxLayoutContainer(Project, newFileEntry);
            await newShoeboxContainer.SetupLayoutsDirectly();

            ShoeboxEntries.Add(newFileEntry, newShoeboxContainer);
        }

        return true;
    }

    // For TPF and the others
    public async Task<bool> LoadTextureBinder(FileDictionaryEntry fileEntry)
    {
        await Task.Yield();

        // For the binders with TPF within
        if (fileEntry.Extension != "tpf")
        {
            // If already loaded, just ignore
            if (Entries.Any(e => e.Key.Filename == fileEntry.Filename && e.Value != null))
            {
                return true;
            }

            if (Entries.Any(e => e.Key.Filename == fileEntry.Filename))
            {
                var binderEntry = Entries.FirstOrDefault(e => e.Key.Filename == fileEntry.Filename);

                if (binderEntry.Key != null)
                {
                    var key = binderEntry.Key;

                    if (TargetFS.FileExists(key.Path))
                    {
                        try
                        {
                            var taeBinderData = TargetFS.ReadFile(key.Path);

                            if (Project.Descriptor.ProjectType is ProjectType.DES or ProjectType.DS1 or ProjectType.DS1R)
                            {
                                try
                                {
                                    var taeBinder = BND3.Read(taeBinderData.Value);

                                    var files = new Dictionary<BinderFile, TPF>();

                                    foreach (var file in taeBinder.Files)
                                    {
                                        if (!file.Name.Contains(".tpf"))
                                            continue;

                                        var data = file.Bytes;

                                        // Some files are empty, ignore them
                                        if (data.Length == 0)
                                            continue;

                                        try
                                        {
                                            var tpfData = TPF.Read(data);

                                            files.Add(file, tpfData);
                                        }
                                        catch (Exception e)
                                        {
                                            Smithbox.LogError(this,
                                                LOC.Get("TEXVIEW_DataSetup_Log_Failed_TPF_Read", file.Name, Name), e);
                                            return false;
                                        }
                                    }

                                    var newBinderContents = new BinderContents
                                    {
                                        Name = fileEntry.Filename,
                                        Binder = taeBinder,
                                        Files = files
                                    };

                                    Entries[key] = newBinderContents;
                                }
                                catch (Exception e)
                                {
                                    Smithbox.LogError(this,
                                        LOC.Get("TEXVIEW_DataSetup_Log_Failed_BND4_Read", key.Filename, Name), e);
                                    return false;
                                }
                            }
                            else
                            {
                                try
                                {
                                    var taeBinder = BND4.Read(taeBinderData.Value);

                                    var files = new Dictionary<BinderFile, TPF>();

                                    foreach (var file in taeBinder.Files)
                                    {
                                        if (!file.Name.Contains(".tpf"))
                                            continue;

                                        var data = file.Bytes;

                                        // Some files are empty, ignore them
                                        if (data.Length == 0)
                                            continue;

                                        try
                                        {
                                            var tpfData = TPF.Read(data);

                                            files.Add(file, tpfData);
                                        }
                                        catch (Exception e)
                                        {
                                            Smithbox.LogError(this,
                                                LOC.Get("TEXVIEW_DataSetup_Log_Failed_TPF_Read", file.Name, Name), e);
                                            return false;
                                        }
                                    }

                                    var newBinderContents = new BinderContents
                                    {
                                        Name = fileEntry.Filename,
                                        Binder = taeBinder,
                                        Files = files
                                    };

                                    Entries[key] = newBinderContents;
                                }
                                catch (Exception e)
                                {
                                    Smithbox.LogError(this,
                                        LOC.Get("TEXVIEW_DataSetup_Log_Failed_BND4_Read", key.Filename, Name), e);
                                    return false;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Smithbox.LogError(this,
                                LOC.Get("TEXVIEW_DataSetup_Log_Failed_VFS_Read", key.Filename, Name), e);
                            return false;
                        }
                    }
                    else
                    {
                        Smithbox.LogError(this,
                            LOC.Get("TEXVIEW_DataSetup_Log_Failed_VFS_Find", key.Path, Name));
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }
        }
        // For tpf.dcx
        else if (fileEntry.Extension == "tpf")
        {
            // If already loaded, just ignore
            if (Entries.Any(e => e.Key.Filename == fileEntry.Filename && e.Value != null))
            {
                return true;
            }

            // Basically creates a fake binder to store the loose TPF in so it fits the standard system.
            if (Entries.Any(e => e.Key.Filename == fileEntry.Filename))
            {
                var tpfEntry = Entries.FirstOrDefault(e => e.Key.Filename == fileEntry.Filename);

                if (tpfEntry.Key != null)
                {
                    var key = tpfEntry.Key;

                    if (Project.Descriptor.ProjectType is ProjectType.DES or ProjectType.DS1 or ProjectType.DS1R)
                    {
                        var fakeBinder = new BND3();

                        if (TargetFS.FileExists(key.Path))
                        {
                            try
                            {
                                var tpfFileData = TargetFS.ReadFile(key.Path);

                                // Create binder file
                                var binderFile = new BinderFile
                                {
                                    ID = 0,
                                    Name = fileEntry.Filename,
                                    Bytes = tpfFileData.Value
                                };
                                fakeBinder.Files.Add(binderFile);

                                // Load actual file
                                var files = new Dictionary<BinderFile, TPF>();
                                var data = binderFile.Bytes;

                                // Some files are empty, ignore them
                                if (data.Length != 0)
                                {
                                    try
                                    {
                                        var tpfData = TPF.Read(data);
                                        files.Add(binderFile, tpfData);

                                        var newBinderContents = new BinderContents
                                        {
                                            Name = fileEntry.Filename,
                                            Binder = fakeBinder,
                                            Files = files,
                                            Loose = true
                                        };

                                        Entries[key] = newBinderContents;
                                    }
                                    catch (Exception e)
                                    {
                                        Smithbox.LogError(this,
                                            LOC.Get("TEXVIEW_DataSetup_Log_Failed_VFS_Read", fileEntry.Filename, Name), e);

                                        return false;
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                Smithbox.LogError(this,
                                    LOC.Get("TEXVIEW_DataSetup_Log_Failed_VFS_Read", fileEntry.Filename, Name), e);

                                return false;
                            }
                        }
                        else
                        {
                            Smithbox.LogError(this,
                                LOC.Get("TEXVIEW_DataSetup_Log_Failed_VFS_Find", fileEntry.Path, Name));
                            return false;
                        }
                    }
                    else
                    {
                        var fakeBinder = new BND4();

                        if (TargetFS.FileExists(key.Path))
                        {
                            try
                            {
                                var tpfFileData = TargetFS.ReadFile(key.Path);

                                // Create binder file
                                var binderFile = new BinderFile
                                {
                                    ID = 0,
                                    Name = fileEntry.Filename,
                                    Bytes = tpfFileData.Value
                                };
                                fakeBinder.Files.Add(binderFile);

                                // Load actual file
                                var files = new Dictionary<BinderFile, TPF>();
                                var data = binderFile.Bytes;

                                // Some files are empty, ignore them
                                if (data.Length != 0)
                                {
                                    try
                                    {
                                        var tpfData = TPF.Read(data);
                                        files.Add(binderFile, tpfData);

                                        var newBinderContents = new BinderContents
                                        {
                                            Name = fileEntry.Filename,
                                            Binder = fakeBinder,
                                            Files = files,
                                            Loose = true
                                        };

                                        Entries[key] = newBinderContents;
                                    }
                                    catch (Exception e)
                                    {
                                        Smithbox.LogError(this,
                                            LOC.Get("TEXVIEW_DataSetup_Log_Failed_TPF_Read", fileEntry.Filename, Name), e);

                                        return false;
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                Smithbox.LogError(this,
                                    LOC.Get("TEXVIEW_DataSetup_Log_Failed_VFS_Read", key.Filename, Name), e);

                                return false;
                            }
                        }
                        else
                        {
                            Smithbox.LogError(this,
                                LOC.Get("TEXVIEW_DataSetup_Log_Failed_VFS_Find", fileEntry.Path, Name));
                            return false;
                        }
                    }
                }
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    // For TPFBHD
    public async Task<bool> LoadPackedTextureBinder(FileDictionaryEntry fileEntry)
    {
        await Task.Yield();

        // If already loaded, just ignore
        if (PackedEntries.Any(e => e.Key.Path == fileEntry.Path && e.Value != null))
        {
            return true;
        }

        if (PackedEntries.Any(e => e.Key.Path == fileEntry.Path))
        {
            var binderEntry = PackedEntries.FirstOrDefault(e => e.Key.Path == fileEntry.Path);

            if (binderEntry.Key != null)
            {
                var key = binderEntry.Key;

                var bhdPath = key.Path;
                var bdtPath = key.Path.Replace(".tpfbhd", ".tpfbdt");

                if (TargetFS.FileExists(bhdPath) && TargetFS.FileExists(bdtPath))
                {
                    try
                    {
                        var bhdData = TargetFS.ReadFile(key.Path);
                        var bdtData = TargetFS.ReadFile(bdtPath);

                        if (Project.Descriptor.ProjectType is ProjectType.DES or ProjectType.DS1 or ProjectType.DS1R)
                        {
                            try
                            {
                                var packedBinder = BXF3.Read(bhdData.Value, bdtData.Value);

                                var files = new Dictionary<BinderFile, TPF>();

                                foreach (var file in packedBinder.Files)
                                {
                                    if (!file.Name.Contains(".tpf"))
                                        continue;

                                    var data = file.Bytes;

                                    // Some files are empty, ignore them
                                    if (data.Length == 0)
                                        continue;

                                    try
                                    {
                                        var tpfData = TPF.Read(data);

                                        files.Add(file, tpfData);
                                    }
                                    catch (Exception e)
                                    {
                                        Smithbox.LogError(this,
                                            LOC.Get("TEXVIEW_DataSetup_Log_Failed_TPF_Read", file.Name, Name), e);

                                        return false;
                                    }
                                }

                                var newBinderContents = new BinderContents
                                {
                                    Name = fileEntry.Filename,
                                    Binder = packedBinder,
                                    Files = files
                                };

                                PackedEntries[key] = newBinderContents;
                            }
                            catch (Exception e)
                            {
                                Smithbox.LogError(this,
                                    LOC.Get("TEXVIEW_DataSetup_Log_Failed_BND4_Read", Name, Name), e);

                                return false;
                            }
                        }
                        else
                        {
                            try
                            {
                                var packedBinder = BXF4.Read(bhdData.Value, bdtData.Value);

                                var files = new Dictionary<BinderFile, TPF>();

                                foreach (var file in packedBinder.Files)
                                {
                                    if (!file.Name.Contains(".tpf"))
                                        continue;

                                    var data = file.Bytes;

                                    // Some files are empty, ignore them
                                    if (data.Length == 0)
                                        continue;

                                    try
                                    {
                                        var tpfData = TPF.Read(data);

                                        files.Add(file, tpfData);
                                    }
                                    catch (Exception e)
                                    {
                                        Smithbox.LogError(this,
                                            LOC.Get("TEXVIEW_DataSetup_Log_Failed_TPF_Read", file.Name, Name), e);

                                        return false;
                                    }
                                }

                                var newBinderContents = new BinderContents
                                {
                                    Name = fileEntry.Filename,
                                    Binder = packedBinder,
                                    Files = files
                                };

                                PackedEntries[key] = newBinderContents;
                            }
                            catch (Exception e)
                            {
                                Smithbox.LogError(this,
                                    LOC.Get("TEXVIEW_DataSetup_Log_Failed_BND4_Read", key.Filename, Name), e);

                                return false;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Smithbox.LogError(this,
                            LOC.Get("TEXVIEW_DataSetup_Log_Failed_VFS_Read", key.Filename, Name), e);

                        return false;
                    }
                }
                else
                {
                    Smithbox.LogError(this,
                        LOC.Get("TEXVIEW_DataSetup_Log_Failed_VFS_Find", key.Filename, Name));
                }
            }
        }
        else
        {
            return false;
        }

        return true;
    }

    #region Save
    /// <summary>
    /// Every loaded container with unsaved edits, loose/binder and packed alike.
    /// </summary>
    public IEnumerable<KeyValuePair<FileDictionaryEntry, BinderContents>> ModifiedContainers()
    {
        foreach (var dict in new[] { Entries, PackedEntries })
        {
            if (dict == null)
                continue;

            foreach (var pair in dict)
            {
                if (pair.Value != null && pair.Value.IsModified)
                    yield return pair;
            }
        }
    }

    public bool HasUnsavedChanges => ModifiedContainers().Any();

    /// <summary>
    /// The project files the next save writes, for the unsaved changes prompt.
    /// </summary>
    public List<string> GetUnsavedFiles()
    {
        return ModifiedContainers()
            .SelectMany(e => e.Value.GetSavePaths(e.Key))
            .Distinct()
            .OrderBy(e => e)
            .ToList();
    }

    /// <summary>
    /// Saves every container with unsaved edits. True if all of them were written.
    /// </summary>
    public bool SaveAll()
    {
        var success = true;

        // Copied: a save clears the container's modified set
        foreach (var pair in ModifiedContainers().ToList())
        {
            if (pair.Value.Save(Project, pair.Key))
            {
                foreach (var path in pair.Value.GetSavePaths(pair.Key))
                    ProjectFiles[path] = true;
            }
            else
            {
                success = false;
            }
        }

        return success;
    }
    #endregion

    #region Copy
    /// <summary>
    /// Writes a game data container into the project as it is (a container with unsaved edits is saved instead).
    /// Afterwards it is a project file, which the game loads instead of its own (T3, §6 Q5).
    /// </summary>
    public bool CopyToProject(FileDictionaryEntry entry)
    {
        var contents = Entries.TryGetValue(entry, out var c) ? c : PackedEntries.TryGetValue(entry, out var p) ? p : null;

        if (contents != null && contents.IsModified)
        {
            var saved = contents.Save(Project, entry);
            if (saved)
                foreach (var path in contents.GetSavePaths(entry))
                    ProjectFiles[path] = true;
            return saved;
        }

        try
        {
            var paths = entry.Extension == "tpfbhd" ? new List<string> { entry.Path, BinderContents.GetBdtPath(entry.Path) } : [entry.Path];

            foreach (var path in paths)
            {
                var data = Project.VFS.FS.ReadFile(path);
                if (data == null)
                {
                    Smithbox.LogError(this, LOC.Get("TEXVIEW_DataSetup_Log_Failed_VFS_Find", path, Name));
                    return false;
                }

                Project.VFS.ProjectFS.WriteFile(path, data.Value.ToArray());
                ProjectFiles[path] = true;
            }

            Smithbox.Log(this, LOC.Get("TEXVIEW_Copy_Log_Copied_To_Project", entry.Path));
            return true;
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("TEXVIEW_Copy_Log_Failed", entry.Path), e);
            return false;
        }
    }

    /// <summary>The path a Copy As with this stem writes: the source's folder and extensions, the new name.</summary>
    public static string GetCopyAsPath(FileDictionaryEntry source, string newStem)
    {
        var slash = source.Path.LastIndexOf('/');
        var folder = slash >= 0 ? source.Path[..(slash + 1)] : "";
        var fileName = slash >= 0 ? source.Path[(slash + 1)..] : source.Path;
        var dot = fileName.IndexOf('.');
        return folder + newStem + (dot >= 0 ? fileName[dot..] : "");
    }

    /// <summary>Why a Copy As name can't be used, or null.</summary>
    public string ValidateCopyAs(FileDictionaryEntry source, string newStem)
    {
        if (string.IsNullOrWhiteSpace(newStem))
            return LOC.Get("TEXVIEW_TpfCommands_Name_Empty");

        if (newStem.IndexOfAny(['\\', '/', ':', '*', '?', '"', '<', '>', '|', '.']) >= 0)
            return LOC.Get("TEXVIEW_TpfCommands_Name_Invalid");

        var path = GetCopyAsPath(source, newStem);

        if (Entries.Keys.Concat(PackedEntries.Keys).Any(e => string.Equals(e.Path, path, StringComparison.OrdinalIgnoreCase))
            || Project.VFS.FS.FileExists(path))
            return LOC.Get("TEXVIEW_Copy_Exists", path);

        // The bank finds texture binders by file name
        if (Entries.Keys.Any(e => string.Equals(e.Filename, newStem, StringComparison.OrdinalIgnoreCase)))
            return LOC.Get("TEXVIEW_Copy_Name_Taken", newStem);

        return null;
    }

    /// <summary>
    /// Writes a copy of a container (with its unsaved edits) into the project under a new name, renaming the files
    /// inside it that carry the old name (c2010_h.texbnd → c2011_h.texbnd holds c2011_h.tpf), and adds it to the
    /// file lists so it can be edited at once. Packed tpfbhd containers (map textures) can't be copied.
    /// </summary>
    public FileDictionaryEntry CopyAs(FileDictionaryEntry source, string newStem)
    {
        if (source.Extension == "tpfbhd" || ValidateCopyAs(source, newStem) != null)
            return null;

        var newPath = GetCopyAsPath(source, newStem);

        try
        {
            if (!Entries.TryGetValue(source, out var contents) || contents == null)
            {
                LoadTextureBinder(source).Wait();
                contents = Entries.FirstOrDefault(e => e.Key.Filename == source.Filename).Value;
            }

            if (contents == null)
            {
                Smithbox.LogError(this, LOC.Get("TEXVIEW_DataSetup_Log_Failed_VFS_Find", source.Path, Name));
                return null;
            }

            var output = contents.BuildOutput(source);
            if (output == null || output.Count != 1)
                return null;

            byte[] bytes = output[0].Bytes;

            if (!contents.Loose)
            {
                var oldStem = source.Filename;
                IBinder copy = contents.Binder is BND3 ? BND3.Read(bytes) : BND4.Read(bytes);

                // The stem can be anywhere in the name: c2010_h.tpf (ER), AM_c1270_d.tpf (DS2)
                foreach (var file in copy.Files)
                {
                    var (folder, stem, extensions) = TpfCommands.SplitName(file.Name);
                    var at = stem.IndexOf(oldStem, StringComparison.OrdinalIgnoreCase);
                    if (at >= 0)
                        file.Name = folder + stem[..at] + newStem + stem[(at + oldStem.Length)..] + extensions;
                }

                bytes = copy is BND3 b3 ? b3.Write() : ((BND4)copy).Write();
            }

            Project.VFS.ProjectFS.WriteFile(newPath, bytes);
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("TEXVIEW_Copy_Log_Failed", newPath), e);
            return null;
        }

        // Only now the file exists: add it so it is editable at once
        var newEntry = source.Clone();
        newEntry.Path = newPath;
        newEntry.Filename = newStem;

        Project.Locator.TextureFiles.Entries.Add(newEntry);
        Entries[newEntry] = null;
        ProjectFiles[newPath] = true;

        Smithbox.Log(this, LOC.Get("TEXVIEW_Copy_Log_Copied_As", source.Path, newPath));
        return newEntry;
    }
    #endregion

    #region Project Files
    /// <summary>
    /// Whether a file comes from the project rather than the game data, by path. Filled on a background thread at
    /// setup (a stat per file), and updated when the editor saves a file.
    /// </summary>
    public Dictionary<string, bool> ProjectFiles = new(StringComparer.OrdinalIgnoreCase);

    public bool IsProjectFile(FileDictionaryEntry entry)
    {
        return entry != null && ProjectFiles.TryGetValue(entry.Path, out var isProject) && isProject;
    }

    private void SetupProjectFiles()
    {
        var result = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        var projectFs = Project.VFS.ProjectFS;

        foreach (var dict in new[] { Entries, PackedEntries })
        {
            foreach (var entry in dict.Keys)
            {
                try
                {
                    result[entry.Path] = projectFs.FileExists(entry.Path);
                }
                catch
                {
                    result[entry.Path] = false;
                }
            }
        }

        ProjectFiles = result;
    }
    #endregion

    #region Dispose
    public void Dispose()
    {
        Entries = null;
        PackedEntries = null;
        ShoeboxEntries = null;
    }
    #endregion
}

public class BinderContents
{
    public string Name { get; set; }
    public IBinder Binder { get; set; }
    public Dictionary<BinderFile, TPF> Files { get; set; }

    /// <summary>
    /// This is to mark a 'fake' binder used for the loose TPF files
    /// </summary>
    public bool Loose { get; set; } = false;

    /// <summary>
    /// The binder files whose TPF has unsaved edits. An unedited TPF is never rewritten, so its bytes stay as read.
    /// </summary>
    public HashSet<BinderFile> ModifiedFiles { get; } = new();

    /// <summary>
    /// TPFs were added to, removed from or renamed in the binder (T3), so the binder is rewritten even if no TPF is.
    /// </summary>
    public bool StructureModified { get; set; }

    public bool IsModified => ModifiedFiles.Count > 0 || StructureModified;

    /// <summary>
    /// The TPF binder files in binder order (the dictionary's order isn't kept across removes and re-adds).
    /// </summary>
    public IEnumerable<BinderFile> OrderedFiles()
    {
        return Binder?.Files.Where(f => Files.ContainsKey(f)) ?? Files.Keys;
    }

    /// <summary>
    /// Marks the TPF held by this binder file as edited, so the next save writes it.
    /// </summary>
    public void MarkModified(BinderFile file)
    {
        if (file != null && Files.ContainsKey(file))
        {
            ModifiedFiles.Add(file);
        }
    }

    /// <summary>
    /// The project paths a save of this container writes (the bdt beside a tpfbhd too).
    /// </summary>
    public List<string> GetSavePaths(FileDictionaryEntry entry)
    {
        if (Binder is BXF3 or BXF4)
        {
            return [entry.Path, GetBdtPath(entry.Path)];
        }

        return [entry.Path];
    }

    public static string GetBdtPath(string bhdPath) => bhdPath.Replace(".tpfbhd", ".tpfbdt");

    /// <summary>
    /// Writes the edited TPFs back into their binder files, then the container to the project, each with the
    /// compression it was read with. TPF and binder writers keep the layout they were read with, so the untouched
    /// parts stay byte-identical (Texture Editor plan, T0/T1).
    /// </summary>
    public bool Save(ProjectEntry project, FileDictionaryEntry entry)
    {
        if (!IsModified)
            return true;

        try
        {
            var output = BuildOutput(entry);

            if (output == null)
            {
                Smithbox.LogError(this, LOC.Get("TEXVIEW_Save_Log_Unsupported_Container", entry.Path));
                return false;
            }

            foreach (var (path, bytes) in output)
            {
                project.VFS.ProjectFS.WriteFile(path, bytes);
            }

            ModifiedFiles.Clear();
            StructureModified = false;
            Smithbox.Log(this, LOC.Get("TEXVIEW_Save_Log_Saved_Container", entry.Path));
            return true;
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("TEXVIEW_Save_Log_Failed_Container", entry.Path), e);
            return false;
        }
    }

    /// <summary>
    /// The files this container is written as, with its current (possibly unsaved) TPFs: the edited TPFs are written
    /// into their binder files first. One file, or a bhd and a bdt. Null for an unknown container type.
    /// </summary>
    public List<(string Path, byte[] Bytes)> BuildOutput(FileDictionaryEntry entry)
    {
        if (Loose)
        {
            // The fake binder holds the loose TPF as its only file; the TPF itself is the file on disk
            return [(entry.Path, Files.Values.First().Write())];
        }

        // A TPF removed after it was edited is no longer in the binder
        foreach (var file in ModifiedFiles.Where(f => Files.ContainsKey(f)))
        {
            file.Bytes = Files[file].Write();
        }

        switch (Binder)
        {
            case BND3 bnd3:
                return [(entry.Path, bnd3.Write())];
            case BND4 bnd4:
                return [(entry.Path, bnd4.Write())];
            case BXF3 bxf3:
                {
                    bxf3.Write(out var bhd, out var bdt);
                    return [(entry.Path, bhd), (GetBdtPath(entry.Path), bdt)];
                }
            case BXF4 bxf4:
                {
                    bxf4.Write(out var bhd, out var bdt);
                    return [(entry.Path, bhd), (GetBdtPath(entry.Path), bdt)];
                }
            default:
                return null;
        }
    }

    public void WriteBinder(string outputPath)
    {
        if (Binder is BND3 bnd3)
        {
            bnd3.Write(outputPath);
        }
        else if (Binder is BND4 bnd4)
        {
            bnd4.Write(outputPath);
        }
        else if (Binder is BXF3 bxf3)
        {
            var bdtPath = outputPath.Replace(".tpfbhd", ".tpfbdt");
            bxf3.Write(bdtPath, outputPath);
        }
        else if (Binder is BXF4 bxf4)
        {
            var bdtPath = outputPath.Replace(".tpfbhd", ".tpfbdt");
            bxf4.Write(bdtPath, outputPath);
        }
    }
}

