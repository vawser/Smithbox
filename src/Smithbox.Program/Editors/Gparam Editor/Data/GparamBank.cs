using Andre.IO.VFS;
using Org.BouncyCastle.Crypto;
using SoulsFormats;
using System.Reflection;

namespace StudioCore.Editors.GparamEditor;

public class GparamBank : IDisposable
{
    public ProjectEntry Project;

    public VirtualFileSystem TargetFS = EmptyVirtualFileSystem.Instance;

    public string Name;

    public Dictionary<FileDictionaryEntry, GparamWrapper> Entries = new();

    public GparamBank(string name, ProjectEntry project, VirtualFileSystem targetFs)
    {
        Project = project;
        Name = name;
        TargetFS = targetFs;
    }

    public async Task<bool> Setup()
    {
        await Task.Yield();

        Task<bool> gparamTask = SetupGraphicsParams();
        bool gparamTaskResult = await gparamTask;

        return true;
    }

    public async Task<bool> SetupGraphicsParams()
    {
        await Task.Yield();

        Entries = new();

        foreach (var entry in Project.Locator.GparamFiles.Entries)
        {
            Entries.Add(entry, new GparamWrapper(entry));
        }

        return true;
    }

    /// <summary>
    /// Mark the wrapper that holds the passed GPARAM as modified
    /// </summary>
    public void MarkAsDirty(GPARAM gparam)
    {
        if (gparam == null)
            return;

        var wrapper = Entries.Values.FirstOrDefault(e => e.Gparam == gparam);

        if (wrapper != null)
        {
            wrapper.IsDirty = true;
        }
    }

    public async Task<bool> LoadGraphicsParam(FileDictionaryEntry fileEntry)
    {
        await Task.Yield();

        // If already loaded, just ignore
        if (Entries.Any(e => e.Key.Filename == fileEntry.Filename && e.Key.Extension == fileEntry.Extension && e.Value.IsLoaded()))
        {
            return true;
        }

        if (Entries.Any(e => e.Key.Filename == fileEntry.Filename && e.Key.Extension == fileEntry.Extension))
        {
            var scriptEntry = Entries.FirstOrDefault(e => e.Key.Filename == fileEntry.Filename && e.Key.Extension == fileEntry.Extension);

            if (scriptEntry.Key != null)
            {
                var key = scriptEntry.Key;

                if (scriptEntry.Key.Extension == "gparambnd")
                {
                    if (TargetFS.FileExists(key.Path))
                    {
                        var bnd = TargetFS.ReadFile(key.Path);

                        var binder = new BND4Reader(bnd.Value);

                        var file = binder.Files.FirstOrDefault(e => Path.GetFileNameWithoutExtension(e.Name) == scriptEntry.Key.Filename);

                        if (file != null)
                        {
                            try
                            {
                                var gparamData = binder.ReadFile(file);

                                try
                                {
                                    var gparam = GPARAM.Read(gparamData);

                                    Entries[key].Gparam = gparam;
                                    Entries[key].IsDirty = false;
                                }
                                catch (Exception e)
                                {
                                    Smithbox.LogError(this, 
                                        LOC.Get("GPARAM_Data_Failed_Read_GPARAM", file.Name), e);

                                    return false;
                                }
                            }
                            catch (Exception e)
                            {
                                Smithbox.LogError(this,
                                    LOC.Get("GPARAM_Data_Failed_Read_VFS", file.Name), e);
                                return false;
                            }
                        }
                    }
                    else
                    {
                        Smithbox.LogError(this, 
                            LOC.Get("GPARAM_Data_Failed_Find_VFS", key.Path));

                        return false;
                    }
                }
                else
                {
                    if (TargetFS.FileExists(key.Path))
                    {
                        try
                        {
                            var gparamData = TargetFS.ReadFile(key.Path);

                            try
                            {
                                var gparam = GPARAM.Read(gparamData.Value);

                                Entries[key].Gparam = gparam;
                                Entries[key].IsDirty = false;
                            }
                            catch (Exception e)
                            {
                                Smithbox.LogError(this,
                                    LOC.Get("GPARAM_Data_Failed_Read_GPARAM", key.Path), e);

                                return false;
                            }
                        }
                        catch (Exception e)
                        {
                            Smithbox.LogError(this,
                                LOC.Get("GPARAM_Data_Failed_Read_VFS", key.Path), e);

                            return false;
                        }
                    }
                    else
                    {
                        Smithbox.LogError(this,
                            LOC.Get("GPARAM_Data_Failed_Find_VFS", key.Path));

                        return false;
                    }
                }
            }
        }
        else
        {
            return false;
        }

        return true;
    }

    public async Task<bool> SaveAllGraphicsParams()
    {
        await Task.Yield();

        foreach (var entry in Entries)
        {
            // Only save the files that have been modified
            if (!entry.Value.IsLoaded() || !entry.Value.IsDirty)
                continue;

            await SaveGraphicsParam(entry.Key, entry.Value);
        }

        return true;
    }

    public async Task<bool> SaveGraphicsParam(FileDictionaryEntry fileEntry, GparamWrapper wrapper)
    {
        await Task.Yield();

        if (!wrapper.IsLoaded())
            return false;

        if (!CFG.Current.GparamEditor_Allow_Unmodified_Save)
        {
            if (!wrapper.IsDirty)
            {
                Smithbox.Log(this, LOC.Get("EDITOR_Data_Skip_File_Save", fileEntry.Filename));
                return false;
            }
        }

        if (fileEntry.Extension == "gparambnd")
        {
            return HandleContainerFile(fileEntry, wrapper);
        }
        else
        {
            return HandleExternalFile(fileEntry, wrapper);
        }
    }

    private bool HandleContainerFile(FileDictionaryEntry fileEntry, GparamWrapper wrapper)
    {
        if (!TargetFS.FileExists(fileEntry.Path))
        {
            Smithbox.LogError(this, LOC.Get("EDITOR_Data_Failed_Locate_Container", fileEntry.Path));
            return false;

        }

        // Container
        var containerFileData = TargetFS.ReadFile(fileEntry.Path);

        if(containerFileData == null)
        {
            Smithbox.LogError(this, LOC.Get("EDITOR_Data_Failed_Read_Container", fileEntry.Path));
            return false;
        }

        // Binder
        var binder = BND4.Read(containerFileData.Value);

        if (binder == null)
        {
            Smithbox.LogError(this, LOC.Get("EDITOR_Data_Failed_Read_Container", fileEntry.Path));
            return false;
        }

        // Binder Files
        foreach (var curFile in binder.Files)
        {
            HandleBinderFile(fileEntry, wrapper, binder, curFile);
        }

        // Save Binder
        try
        {
            var writeFile = binder.Write();

            Project.VFS.ProjectFS.WriteFile(fileEntry.Path, writeFile);

            wrapper.IsDirty = false;

            Smithbox.Log(this, LOC.Get("EDITOR_Data_Save_File", fileEntry.Filename));
        }
        catch (Exception ex)
        {
            Smithbox.LogError(this, LOC.Get("EDITOR_Data_Failed_Write_Container", fileEntry.Path), ex);
            return false;
        }

        return true;
    }

    private void HandleBinderFile(FileDictionaryEntry fileEntry, GparamWrapper wrapper, BND4 binder, BinderFile file)
    {
        var filename = Path.GetFileNameWithoutExtension(file.Name);

        if (filename != fileEntry.Filename)
            return;

        // Internal File
        try
        {
            file.Bytes = wrapper.Gparam.Write();
        }
        catch (Exception ex)
        {
            Smithbox.LogError(this, LOC.Get("EDITOR_Data_Failed_Write_File", file.Name), ex);
            return;
        }
    }

    private bool HandleExternalFile(FileDictionaryEntry fileEntry, GparamWrapper wrapper)
    {
        if (!TargetFS.FileExists(fileEntry.Path))
        {
            Smithbox.LogError(this, LOC.Get("EDITOR_Data_Failed_Locate_External_File", fileEntry.Path));
            return false;
        }

        // External File
        try
        {
            var fileData = wrapper.Gparam.Write();

            Project.VFS.ProjectFS.WriteFile(fileEntry.Path, fileData);

            wrapper.IsDirty = false;

            Smithbox.Log(this, LOC.Get("EDITOR_Data_Save_File", fileEntry.Filename));
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("EDITOR_Data_Failed_Write_File", fileEntry.Path), e);
            return false;
        }

        return true;
    }

    #region Dispose
    public void Dispose()
    {
        Entries.Clear();

        Entries = null;
    }
    #endregion
}
