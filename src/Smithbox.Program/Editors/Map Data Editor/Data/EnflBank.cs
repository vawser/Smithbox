using Andre.IO.VFS;
using SoulsFormats;
using SoulsFormats.KF4;
using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapDataEditor;

public class EnflBank : IDisposable
{
    public ProjectEntry Project;

    public VirtualFileSystem TargetFS = EmptyVirtualFileSystem.Instance;

    public Dictionary<FileDictionaryEntry, ENFL> EntryFileLists = new();

    public string Name;

    public EnflBank(string name, ProjectEntry project, VirtualFileSystem targetFs)
    {
        Project = project;
        Name = name;
        TargetFS = targetFs;
    }

    public bool Setup()
    {
        foreach (var entry in Project.Locator.EntryFileListFiles.Entries)
        {
            if (!EntryFileLists.ContainsKey(entry))
            {
                EntryFileLists.Add(entry, null);
            }
        }

        return true;
    }


    public bool LoadEntryFileList(FileDictionaryEntry fileEntry)
    {
        ENFL entryFileList;

        try
        {
            var fileData = TargetFS.ReadFileOrThrow(fileEntry.Path);

            try
            {
                entryFileList = ENFL.Read(fileData);

                if (EntryFileLists.ContainsKey(fileEntry))
                {
                    EntryFileLists[fileEntry] = entryFileList;
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, $"[Map Data Editor] Failed to read {fileEntry.Path} as ENFL", e);
                return false;
            }
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, $"[Map Data Editor] Failed to read {fileEntry.Path} from VFS", e);
            return false;
        }

        return true;
    }

    public bool SaveEntryFileList(MapDataEditorView view, FileDictionaryEntry fileEntry)
    {
        if (EntryFileLists.ContainsKey(fileEntry))
        {
            var entryFileList = EntryFileLists[fileEntry];

            try
            {
                var fileData = entryFileList.Write();

                Project.VFS.ProjectFS.WriteFile(fileEntry.Path, fileData);
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, $"[Map Data Editor] Failed to write {fileEntry.Path}", e);

                return false;
            }
        }

        return true;
    }

    #region Dispose
    public void Dispose()
    {

    }
    #endregion
}
