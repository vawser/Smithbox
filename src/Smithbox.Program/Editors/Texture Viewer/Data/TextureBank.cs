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

        Task<bool> shoeboxTask = SetupShoeboxLayouts();
        bool shoeboxTaskResult = await shoeboxTask;

        return true;
    }

    public async Task<bool> SetupTextures()
    {
        await Task.Yield();

        Entries = new();

        if (Project.Handler.TextureData.TextureFiles.Entries != null)
        {
            foreach (var entry in Project.Handler.TextureData.TextureFiles.Entries)
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

        if (Project.Handler.TextureData.TexturePackedFiles.Entries != null)
        {
            foreach (var entry in Project.Handler.TextureData.TexturePackedFiles.Entries)
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

        foreach (var entry in Project.Handler.TextureData.ShoeboxFiles.Entries)
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

                    try
                    {
                        var taeBinderData = TargetFS.ReadFileOrThrow(key.Path);

                        if (Project.Descriptor.ProjectType is ProjectType.DES or ProjectType.DS1 or ProjectType.DS1R)
                        {
                            try
                            {
                                var taeBinder = BND3.Read(taeBinderData);

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
                                        TaskLogs.AddError($"[Texture Viewer] Failed to read {file.Name} as TPF in {Name}.", e);
                                        return false;
                                    }
                                }

                                var newBinderContents = new BinderContents();
                                newBinderContents.Name = fileEntry.Filename;
                                newBinderContents.Binder = taeBinder;
                                newBinderContents.Files = files;

                                Entries[key] = newBinderContents;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddError($"[Texture Viewer] Failed to read {key.Filename} as BND4 in {Name}.", e);
                                return false;
                            }
                        }
                        else
                        {
                            try
                            {
                                var taeBinder = BND4.Read(taeBinderData);

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
                                        TaskLogs.AddError($"[Texture Viewer] Failed to read {file.Name} as TPF in {Name}.", e);
                                        return false;
                                    }
                                }

                                var newBinderContents = new BinderContents();
                                newBinderContents.Name = fileEntry.Filename;
                                newBinderContents.Binder = taeBinder;
                                newBinderContents.Files = files;

                                Entries[key] = newBinderContents;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddError($"[Texture Viewer] Failed to read {key.Filename} as BND4 in {Name}", e);
                                return false;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        TaskLogs.AddError($"[Texture Viewer] Failed to read {key.Filename} from VFS in {Name}", e);
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

                        try
                        {
                            var tpfFileData = TargetFS.ReadFileOrThrow(key.Path);

                            // Create binder file
                            var binderFile = new BinderFile();
                            binderFile.ID = 0;
                            binderFile.Name = fileEntry.Filename;
                            binderFile.Bytes = tpfFileData;
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

                                    var newBinderContents = new BinderContents();
                                    newBinderContents.Name = fileEntry.Filename;
                                    newBinderContents.Binder = fakeBinder;
                                    newBinderContents.Files = files;
                                    newBinderContents.Loose = true;

                                    Entries[key] = newBinderContents;
                                }
                                catch (Exception e)
                                {
                                    TaskLogs.AddError($"[Texture Viewer] Failed to read {fileEntry.Filename} from VFS in {Name}", e);
                                    return false;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            TaskLogs.AddError($"[Texture Viewer] Failed to read {key.Filename} from VFS in {Name}", e);
                            return false;
                        }
                    }
                    else
                    {
                        var fakeBinder = new BND4();

                        try
                        {
                            var tpfFileData = TargetFS.ReadFileOrThrow(key.Path);

                            // Create binder file
                            var binderFile = new BinderFile();
                            binderFile.ID = 0;
                            binderFile.Name = fileEntry.Filename;
                            binderFile.Bytes = tpfFileData;
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

                                    var newBinderContents = new BinderContents();
                                    newBinderContents.Name = fileEntry.Filename;
                                    newBinderContents.Binder = fakeBinder;
                                    newBinderContents.Files = files;
                                    newBinderContents.Loose = true;

                                    Entries[key] = newBinderContents;
                                }
                                catch (Exception e)
                                {
                                    TaskLogs.AddError($"[Texture Viewer] Failed to read {fileEntry.Filename} as TPF in {Name}", e);
                                    return false;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            TaskLogs.AddError($"[Texture Viewer] Failed to read {key.Filename} from VFS in {Name}", e);

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

                try
                {
                    var bdtPath = key.Path.Replace(".tpfbhd", ".tpfbdt");

                    var bhdData = TargetFS.ReadFileOrThrow(key.Path);
                    var bdtData = TargetFS.ReadFileOrThrow(bdtPath);

                    if (Project.Descriptor.ProjectType is ProjectType.DES or ProjectType.DS1 or ProjectType.DS1R)
                    {
                        try
                        {
                            var packedBinder = BXF3.Read(bhdData, bdtData);

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
                                    TaskLogs.AddError($"[Texture Viewer] Failed to {file.Name} as TPF in {Name}.", e);
                                    return false;
                                }
                            }

                            var newBinderContents = new BinderContents();
                            newBinderContents.Name = fileEntry.Filename;
                            newBinderContents.Binder = packedBinder;
                            newBinderContents.Files = files;

                            PackedEntries[key] = newBinderContents;
                        }
                        catch (Exception e)
                        {
                            TaskLogs.AddError($"[Texture Viewer] Failed to read {Name} as BND4 in {Name}", e);
                            return false;
                        }
                    }
                    else
                    {
                        try
                        {
                            var packedBinder = BXF4.Read(bhdData, bdtData);

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
                                    TaskLogs.AddError($"[Texture Viewer] Failed to {file.Name} as TPF in {Name}.",e);
                                    return false;
                                }
                            }

                            var newBinderContents = new BinderContents();
                            newBinderContents.Name = fileEntry.Filename;
                            newBinderContents.Binder = packedBinder;
                            newBinderContents.Files = files;

                            PackedEntries[key] = newBinderContents;
                        }
                        catch (Exception e)
                        {
                            TaskLogs.AddError($"[Texture Viewer] Failed to {key.Filename} as BXF4 in {Name}.",  e);
                            return false;
                        }
                    }
                }
                catch (Exception e)
                {
                    TaskLogs.AddError($"[Texture Viewer] Failed to {key.Filename} from VFS in {Name}.", e);
                    return false;
                }
            }
        }
        else
        {
            return false;
        }

        return true;
    }

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
}

