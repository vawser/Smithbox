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
        if (Project.Descriptor.ProjectType is ProjectType.DS3 or ProjectType.BB)
            return true;

        return false;
    }

    public void Setup()
    {
        if (!CanUse())
            return;

        foreach (var entry in Project.Locator.LightProbeFiles.Entries)
        {
            var fileData = Project.VFS.FS.ReadFile(entry.Path);

            if (fileData != null)
            {
                try
                {
                    var btpbData = BTPB.Read(fileData.Value);

                    Files.Add(entry.Filename, btpbData);
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[Map Editor] Failed to read {entry.Path} as BTPB", LogLevel.Error, LogPriority.High, e);
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

        foreach (var entry in Project.Locator.LightProbeFiles.Entries)
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
                    foreach (var parent in map.LightProbeParents)
                    {
                        // Match the root object name to the filename
                        if (parent.WrappedObject.ToString() == entry.Filename)
                        {
                            var btpbData = BTPB.Read(fileData.Value);

                            // Clear groups and then re-fill from the map container hierarchy
                            btpbData.Groups.Clear();

                            foreach (var btpbEntry in parent.Children)
                            {
                                var group = (BTPB.Group)btpbEntry.WrappedObject;

                                btpbData.Groups.Add(group);
                            }

                            var fileOutput = btpbData.Write();

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
                    TaskLogs.AddLog($"[Map Editor] Failed to write {entry.Path} as BTPB", LogLevel.Error, LogPriority.High, e);
                }
            }
        }
    }
}
