using Andre.IO.VFS;
using SoulsFormats;

namespace StudioCore.Editors.GparamEditor;

public class GparamBank : IDisposable
{
    public ProjectEntry Project;

    public VirtualFileSystem TargetFS = EmptyVirtualFileSystem.Instance;

    public string Name;

    public Dictionary<FileDictionaryEntry, GPARAM> Entries = new();

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
            Entries.Add(entry, null);
        }

        return true;
    }

    public async Task<bool> LoadGraphicsParam(FileDictionaryEntry fileEntry)
    {
        await Task.Yield();

        // If already loaded, just ignore
        if (Entries.Any(e => e.Key.Filename == fileEntry.Filename && e.Key.Extension == fileEntry.Extension && e.Value != null))
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

                                    Entries[key] = gparam;
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

                                Entries[key] = gparam;
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
            await SaveGraphicsParam(entry.Key, entry.Value);
        }

        return true;
    }

    public async Task<bool> SaveGraphicsParam(FileDictionaryEntry fileEntry, GPARAM gparamEntry)
    {
        await Task.Yield();

        if (fileEntry.Extension == "gparambnd")
        {
            if (TargetFS.FileExists(fileEntry.Path))
            {
                var bnd = TargetFS.ReadFile(fileEntry.Path);

                var binder = BND4.Read(bnd.Value);

                foreach (var entry in binder.Files)
                {
                    var filename = Path.GetFileNameWithoutExtension(entry.Name);

                    if (filename != fileEntry.Filename)
                        continue;

                    try
                    {
                        try
                        {
                            entry.Bytes = gparamEntry.Write();
                        }
                        catch (Exception ex)
                        {
                            Smithbox.LogError(this,
                                LOC.Get("GPARAM_Data_Failed_Write_GPARAM", entry.Name), ex);
                        }

                        var writeFile = binder.Write();

                        Project.VFS.ProjectFS.WriteFile(fileEntry.Path, writeFile);
                    }
                    catch (Exception ex)
                    {
                        Smithbox.LogError(this,
                            LOC.Get("GPARAM_Data_Failed_Write_Binder", fileEntry.Path), ex);
                    }
                }
            }
            else
            {
                Smithbox.LogError(this,
                    LOC.Get("GPARAM_Data_Failed_Find_VFS", fileEntry.Path));
            }
        }
        else
        {
            try
            {
                var bytes = gparamEntry.Write();

                Project.VFS.ProjectFS.WriteFile(fileEntry.Path, bytes);
            }
            catch (Exception e)
            {
                Smithbox.LogError(this,
                    LOC.Get("GPARAM_Data_Failed_Write_GPARAM", fileEntry.Path), e);

                return false;
            }
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
