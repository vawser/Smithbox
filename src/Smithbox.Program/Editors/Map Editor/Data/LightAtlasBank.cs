using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks.Dataflow;

namespace StudioCore.Editors.MapEditor;

public class LightAtlasBank
{
    public MapEditorScreen Editor;
    public ProjectEntry Project;

    public Dictionary<string, BTAB> Files = new();

    public LightAtlasBank(MapEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;

        Setup();
    }

    public bool CanUse()
    {
        if (Project.Descriptor.ProjectType is ProjectType.DS3 or ProjectType.BB)
            return true;

        return false;
    }

    public void Setup()
    {
        if (!CanUse())
            return;

        foreach (var entry in Project.Handler.MapData.LightAtlasFiles.Entries)
        {
            var fileData = Project.VFS.FS.ReadFile(entry.Path);

            if (fileData != null)
            {
                try
                {
                    var btabData = BTAB.Read(fileData.Value);

                    Files.Add(entry.Filename, btabData);
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[Map Editor] Failed to read {entry.Path} as BTAB", LogLevel.Error, LogPriority.High, e);
                }
            }
        }
    }

    public void LoadBTAB(MapContainer map)
    {
        if (!CanUse())
            return;

        foreach (var entry in Files)
        {
            // File will be: m30_00_00_00_0001, so we match loosely
            if (entry.Key.Contains(map.Name))
            {
                var btab = entry.Value;

                if (btab != null)
                {
                    map.LoadBTAB(entry.Key, btab);
                }
            }
        }
    }

    public void SaveBTAB(MapEditorScreen editor, MapContainer map)
    {
        if (!CanUse())
            return;

        foreach (var entry in Project.Handler.MapData.LightAtlasFiles.Entries)
        {
            // File will be: m30_00_00_00_0001, so we match loosely
            if (!entry.Filename.Contains(map.Name))
                continue;

            var fileData = Project.VFS.FS.ReadFile(entry.Path);

            if (fileData != null)
            {
                var applyEdit = false;

                try
                {
                    foreach(var parent in map.LightAtlasParents)
                    {
                        if (parent.WrappedObject.ToString() == entry.Filename)
                        {
                            var btabData = BTAB.Read(fileData.Value);

                            btabData.Entries.Clear();

                            foreach (var btabEntry in parent.Children)
                            {
                                var curEntry = (BTAB.Entry)btabEntry.WrappedObject;

                                btabData.Entries.Add(curEntry);
                            }

                            var fileOutput = btabData.Write();

                            if (!BytePerfectHelper.Md5Equal(fileData.Value.Span, fileOutput))
                            {
                                applyEdit = true;
                            }

                            if (applyEdit)
                            {
                                Project.VFS.ProjectFS.WriteFile(entry.Path, fileOutput);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[Map Editor] Failed to write {entry.Path} as BTAB", LogLevel.Error, LogPriority.High, e);
                }
            }
        }
    }
}
