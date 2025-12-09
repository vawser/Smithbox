using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Editor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static SoulsFormats.NVA;

namespace StudioCore.Editors.MapEditor;

public class HavokNavmeshManager
{
    public MapEditorScreen Editor;
    public ProjectEntry Project;

    public Dictionary<string, NVA> Files = new();

    public HavokNavmeshManager(MapEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public bool CanUse()
    {
        if (Project.ProjectType is ProjectType.DS3 or ProjectType.BB or ProjectType.SDT or ProjectType.ER)
            return true;

        // NOTE: SDT doesn't render the meshes as it doesn't have HKX support yet

        return false;
    }

    public void LoadHavokNVA(MapContainer map)
    {
        if (!CanUse())
            return;

        if (Files.ContainsKey(map.Name))
        {
            var nva = Files[map.Name];

            if (nva != null)
            {
                map.LoadHavokNVA(map.Name, nva);
            }
        }
        else
        {
            var entry = Project.MapData.NavmeshFiles.Entries.FirstOrDefault(e => e.Filename == map.Name);
            if (entry != null)
            {
                try
                {
                    var nvaData = Project.MapData.PrimaryBank.TargetFS.ReadFileOrThrow(entry.Path);

                    try
                    {
                        var nva = NVA.Read(nvaData);

                        Files.Add(Path.GetFileNameWithoutExtension(entry.Filename), nva);

                        map.LoadHavokNVA(map.Name, nva);
                    }
                    catch (Exception e)
                    {
                        TaskLogs.AddLog($"[{Project.ProjectName}:Map Editor] Failed to read {entry.Path} as NVA", LogLevel.Error, Tasks.LogPriority.High, e);
                    }
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[{Project.ProjectName}:Map Editor] Failed to read {entry.Path} from VFS", LogLevel.Error, Tasks.LogPriority.High, e);
                }
            }
        }
    }

    public void SaveHavokNVA(MapEditorScreen editor, MapContainer map)
    {
        if (!CanUse())
            return;

        foreach (var entry in Project.MapData.NavmeshFiles.Entries)
        {
            try
            {
                var mapID = Path.GetFileNameWithoutExtension(map.Name);

                if (map.Name == mapID)
                {
                    var fileData = Project.MapData.PrimaryBank.TargetFS.ReadFileOrThrow(entry.Path);
                    var existingNVA = NVA.Read(fileData);

                    var version = existingNVA.NavmeshInfoEntries.Version;

                    // Re-make the navmesh entries from the entities
                    var newNavmeshes = new NavmeshInfoSection((int)version);

                    foreach (var entNavmesh in map.NavmeshParent.Children)
                    {
                        var navmesh = (NavmeshInfo)entNavmesh.WrappedObject;

                        newNavmeshes.Add(navmesh);
                    }

                    existingNVA.NavmeshInfoEntries = newNavmeshes;

                    var newFileData = existingNVA.Write();
                    Project.ProjectFS.WriteFile(entry.Path, newFileData);
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{Project.ProjectName}:Map Editor] Failed to write {entry.Path} as NVA", LogLevel.Error, Tasks.LogPriority.High, e);
            }
        }
    }
}
