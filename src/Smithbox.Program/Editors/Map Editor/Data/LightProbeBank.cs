using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace StudioCore.Editors.MapEditor;

public class LightProbeBank
{
    public MapEditorScreen Editor;
    public ProjectEntry Project;

    public Dictionary<string, BTPB> Files = new();

    public LightProbeBank(MapEditorScreen editor, ProjectEntry project)
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

        foreach (var entry in Project.MapData.LightProbeFiles.Entries)
        {
            var fileData = Project.FS.ReadFile(entry.Path);

            if (fileData != null)
            {
                try
                {
                    var btpbData = BTPB.Read(fileData.Value);

                    Files.Add(entry.Filename, btpbData);
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[{Project.ProjectName}:Map Editor] Failed to read {entry.Path} as BTPB", LogLevel.Error, LogPriority.High, e);
                }
            }
        }
    }

    public void LoadBTPB(MapContainer map)
    {
        if (!CanUse())
            return;

        foreach (var entry in Files)
        {
            // File will be: m30_00_00_00_0001, so we match loosely
            if (entry.Key.Contains(map.Name))
            {
                var btpb = entry.Value;

                if (btpb != null)
                {
                    map.LoadBTPB(entry.Key, btpb);
                }
            }
        }
    }

    public void SaveBTPB(MapEditorScreen editor, MapContainer map)
    {
        if (!CanUse())
            return;

        foreach (var entry in Project.MapData.LightProbeFiles.Entries)
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
                    var btpbData = BTPB.Read(fileData.Value);

                    //btpbData.Groups = new();

                    foreach (var ent in map.LightProbeParents)
                    {
                        if (ent.Name == entry.Filename)
                        {
                            foreach (var btabEntry in ent.Children)
                            {

                            }
                        }
                    }

                    var fileOutput = btpbData.Write();

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
                    TaskLogs.AddLog($"[{Project.ProjectName}:Map Editor] Failed to write {entry.Path} as BTPB", LogLevel.Error, LogPriority.High, e);
                }
            }
        }
    }
}
