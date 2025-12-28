using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

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
        if (Project.ProjectType is ProjectType.DS3 or ProjectType.BB)
            return true;

        return false;
    }

    public void Setup()
    {
        if (!CanUse())
            return;

        foreach (var entry in Project.MapData.LightAtlasFiles.Entries)
        {
            var fileData = Project.FS.ReadFile(entry.Path);

            if (fileData != null)
            {
                try
                {
                    var btabData = BTAB.Read(fileData.Value);

                    Files.Add(entry.Filename, btabData);
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[{Project.ProjectName}:Map Editor] Failed to read {entry.Path} as BTAB", LogLevel.Error, LogPriority.High, e);
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

        foreach (var entry in Project.MapData.LightAtlasFiles.Entries)
        {
            // File will be: m30_00_00_00_0001, so we match loosely
            if (!entry.Filename.Contains(map.Name))
                continue;

            var fileData = Project.FS.ReadFile(entry.Path);

            if (fileData != null)
            {
                var applyEdit = false;

                try
                {
                    var btabData = BTAB.Read(fileData.Value);

                    //btabData.Entries = new();

                    foreach(var ent in map.LightAtlasParents)
                    {
                        if (ent.Name == entry.Filename)
                        {
                            foreach (var btabEntry in ent.Children)
                            {

                            }
                        }
                    }

                    var fileOutput = btabData.Write();

                    if (!BytePerfectHelper.Md5Equal(fileData.Value.Span, fileOutput))
                    {
                        applyEdit = true;
                    }

                    if (applyEdit)
                    {
                        Project.ProjectFS.WriteFile(entry.Path, fileOutput);
                    }
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[{Project.ProjectName}:Map Editor] Failed to write {entry.Path} as BTAB", LogLevel.Error, LogPriority.High, e);
                }
            }
        }
    }
}
