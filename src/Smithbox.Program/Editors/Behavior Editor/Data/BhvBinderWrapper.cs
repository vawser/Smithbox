using Andre.IO.VFS;
using SoulsFormats;
using StudioCore.Application;

namespace StudioCore.Editors.BehaviorEditor;

/// <summary>
/// A behbnd and its .bhv files. Saving reads the project's copy of the binder again and replaces only the bytes of
/// changed .bhv entries, so the other entries (tae_action .tae files), IDs, flags, order and compression stay as
/// they are; P0 found every AC6 behbnd rewrites byte-identical (Claude/Documentation/BHV Editor Analysis.md).
/// </summary>
public class BhvBinderWrapper
{
    public ProjectEntry Project;
    public VirtualFileSystem TargetFS;

    /// <summary>The behbnd's name without extensions, e.g. c0200</summary>
    public string Name { get; }

    public string FilePath { get; }

    /// <summary>The .bhv files, in binder order</summary>
    public List<BhvFileWrapper> Files { get; } = new();

    /// <summary>.bhv entries that failed to read; shown greyed, and saved as they were</summary>
    public List<string> UnreadableFiles { get; } = new();

    /// <summary>How many entries of other types (the tae_action .tae files) the binder holds</summary>
    public int OtherEntryCount { get; private set; }

    public BhvBinderWrapper(ProjectEntry project, FileDictionaryEntry dictEntry, VirtualFileSystem targetFS)
    {
        Project = project;
        TargetFS = targetFS;
        Name = dictEntry.Filename;
        FilePath = dictEntry.Path;
    }

    public bool IsDirty() => Files.Any(f => f.IsDirty);

    public async Task<bool> Load()
    {
        await Task.Yield();

        if (!TargetFS.FileExists(FilePath))
        {
            Smithbox.LogError(this, LOC.Get("BHV_Data_Log_Failed_to_Find_File", FilePath));
            return false;
        }

        try
        {
            var binder = BND4.Read(TargetFS.ReadFile(FilePath).Value);

            foreach (var entry in binder.Files)
            {
                if (!entry.Name.EndsWith(".bhv", StringComparison.OrdinalIgnoreCase))
                {
                    OtherEntryCount++;
                    continue;
                }

                try
                {
                    Files.Add(new BhvFileWrapper(this, entry.Name, entry.ID, BHV.Read(entry.Bytes)));
                }
                catch (Exception e)
                {
                    UnreadableFiles.Add(entry.Name);
                    Smithbox.LogError(this, LOC.Get("BHV_Data_Log_Failed_to_Read_BHV", entry.Name, Name), e);
                }
            }

            return true;
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("BHV_Data_Log_Failed_to_Read_File", FilePath), e);
            return false;
        }
    }

    /// <summary>
    /// Writes the changed .bhv files into the project's copy of the binder, then clears their dirty marks.
    /// Doesn't need a view, so the unsaved changes prompt can save every binder.
    /// </summary>
    public bool SaveDirtyFiles()
    {
        if (!IsDirty())
            return true;

        var containerFileData = TargetFS.ReadFile(FilePath);

        if (containerFileData == null)
        {
            Smithbox.LogError(this, LOC.Get("EDITOR_Data_Failed_Read_Container", FilePath));
            return false;
        }

        try
        {
            var binder = BND4.Read(containerFileData.Value);

            foreach (var file in Files.Where(f => f.IsDirty))
            {
                var entry = binder.Files.FirstOrDefault(e => e.Name == file.Key);

                if (entry == null)
                {
                    Smithbox.LogError(this, LOC.Get("BHV_Data_Log_Entry_Missing", file.Key, FilePath));
                    return false;
                }

                entry.Bytes = file.BHV.Write();
            }

            // Keeps the binder's own compression (DCX_KRAK_MAX for AC6), read with it
            Project.VFS.ProjectFS.WriteFile(FilePath, binder.Write());
        }
        catch (Exception ex)
        {
            Smithbox.LogError(this, LOC.Get("EDITOR_Data_Failed_Write_Container", FilePath), ex);
            return false;
        }

        foreach (var file in Files)
            file.IsDirty = false;

        Smithbox.Log(this, LOC.Get("EDITOR_Data_Save_File", FilePath));

        return true;
    }
}
