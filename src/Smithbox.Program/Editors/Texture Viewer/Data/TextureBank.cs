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

    public bool IsModified => ModifiedFiles.Count > 0;

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
            if (Loose)
            {
                // The fake binder holds the loose TPF as its only file; the TPF itself is the file on disk
                var tpf = Files.Values.First();
                project.VFS.ProjectFS.WriteFile(entry.Path, tpf.Write());
            }
            else
            {
                foreach (var file in ModifiedFiles)
                {
                    file.Bytes = Files[file].Write();
                }

                switch (Binder)
                {
                    case BND3 bnd3:
                        project.VFS.ProjectFS.WriteFile(entry.Path, bnd3.Write());
                        break;
                    case BND4 bnd4:
                        project.VFS.ProjectFS.WriteFile(entry.Path, bnd4.Write());
                        break;
                    case BXF3 bxf3:
                        {
                            bxf3.Write(out var bhd, out var bdt);
                            project.VFS.ProjectFS.WriteFile(entry.Path, bhd);
                            project.VFS.ProjectFS.WriteFile(GetBdtPath(entry.Path), bdt);
                            break;
                        }
                    case BXF4 bxf4:
                        {
                            bxf4.Write(out var bhd, out var bdt);
                            project.VFS.ProjectFS.WriteFile(entry.Path, bhd);
                            project.VFS.ProjectFS.WriteFile(GetBdtPath(entry.Path), bdt);
                            break;
                        }
                    default:
                        Smithbox.LogError(this, LOC.Get("TEXVIEW_Save_Log_Unsupported_Container", entry.Path));
                        return false;
                }
            }

            ModifiedFiles.Clear();
            Smithbox.Log(this, LOC.Get("TEXVIEW_Save_Log_Saved_Container", entry.Path));
            return true;
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("TEXVIEW_Save_Log_Failed_Container", entry.Path), e);
            return false;
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

