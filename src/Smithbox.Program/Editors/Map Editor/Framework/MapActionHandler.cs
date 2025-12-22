using SoulsFormats;
using StudioCore.Application;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using static SoulsFormats.MSBE.Part;

namespace StudioCore.Editors.MapEditor;

public class MapActionHandler
{
    private MapEditorScreen Editor;
    private ProjectEntry Project;

    public MapActionHandler(MapEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    /// <summary>
    /// Generate Navigation Data
    /// </summary>
    public void GenerateNavigationData()
    {
        HashSet<string> idCache = new();

        foreach (var entry in Project.MapData.PrimaryBank.Maps)
        {
            string mapid = entry.Key.Filename;

            if (Editor.Project.ProjectType is ProjectType.DES)
            {
                if (mapid != "m03_01_00_99" && !mapid.StartsWith("m99"))
                {
                    var areaId = mapid.Substring(0, 3);
                    if (idCache.Contains(areaId))
                        continue;
                    idCache.Add(areaId);

                    var areaDirectories = new List<string>();
                    foreach (var tEntry in Project.MapData.PrimaryBank.Maps)
                    {
                        if (tEntry.Key.Filename.StartsWith(areaId) && tEntry.Key.Filename != "m03_01_00_99")
                        {
                            areaDirectories.Add(Path.Combine(Editor.Project.DataPath, "map", tEntry.Key.Filename));
                        }
                    }
                    SoulsMapMetadataGenerator.GenerateMCGMCP(Editor, areaDirectories, toBigEndian: true);
                }
                else
                {
                    var areaDirectories = new List<string> { Path.Combine(Editor.Project.DataPath, "map", mapid) };
                    SoulsMapMetadataGenerator.GenerateMCGMCP(Editor, areaDirectories, toBigEndian: true);
                }
            }
            else if (Editor.Project.ProjectType is ProjectType.DS1 or ProjectType.DS1R)
            {
                var areaDirectories = new List<string> { Path.Combine(Editor.Project.DataPath, "map", mapid) };

                SoulsMapMetadataGenerator.GenerateMCGMCP(Editor, areaDirectories, toBigEndian: false);
            }
        }

        TaskLogs.AddLog("Navigation Data generated.");
    }

    public EntityFilterType SelectedFilter = EntityFilterType.None;

    public string SelectedMapFilter = "All";

    public void ApplyEntityAssigner()
    {
        // Save current and then unload
        Editor.Save();
        Editor.Universe.UnloadAll();

        if (SelectedMapFilter == "All")
        {
            foreach (var entry in Project.MapData.PrimaryBank.Maps)
            {
                ApplyEntityGroupIdChange(entry.Key.Filename);
            }
        }
        else
        {
            ApplyEntityGroupIdChange(SelectedMapFilter);
        }
    }

    public void ApplyEntityGroupIdChange(string mapid)
    {
        var filepath = Path.Join(Editor.Project.ProjectPath, "map", "MapStudio", $"{mapid}.msb.dcx");

        // Armored Core
        if (Editor.Project.ProjectType == ProjectType.AC6)
        {
            MSB_AC6 map = MSB_AC6.Read(filepath);

            // Enemies
            foreach (var part in map.Parts.Enemies)
            {
                MSB_AC6.Part.Enemy enemy = part;

                bool isApplied = true;

                if (SelectedFilter is EntityFilterType.ChrID)
                {
                    isApplied = false;

                    if (enemy.ModelName == CFG.Current.Toolbar_EntityGroup_Attribute)
                    {
                        isApplied = true;
                    }
                }

                if (SelectedFilter is EntityFilterType.NpcParamID)
                {
                    isApplied = false;

                    if (enemy.NPCParamID.ToString() == CFG.Current.Toolbar_EntityGroup_Attribute)
                    {
                        isApplied = true;
                    }
                }

                if (SelectedFilter is EntityFilterType.NpcThinkParamID)
                {
                    isApplied = false;

                    if (enemy.NPCParamID.ToString() == CFG.Current.Toolbar_EntityGroup_Attribute)
                    {
                        isApplied = true;
                    }
                }

                if (isApplied)
                {
                    for (int i = 0; i < enemy.EntityGroupIDs.Length; i++)
                    {
                        if (enemy.EntityGroupIDs[i] == 0)
                        {
                            enemy.EntityGroupIDs[i] = (uint)CFG.Current.Toolbar_EntityGroupID;

                            TaskLogs.AddLog($"Added new Entity Group ID {CFG.Current.Toolbar_EntityGroupID} to {enemy.Name}.");
                            break;
                        }
                    }
                }
            }

            map.Write(filepath);
        }

        // Elden Ring
        if (Editor.Project.ProjectType == ProjectType.ER)
        {
            MSBE map = MSBE.Read(filepath);

            // Enemies
            foreach (var part in map.Parts.Enemies)
            {
                Enemy enemy = part;

                bool isApplied = true;

                if (SelectedFilter is EntityFilterType.ChrID)
                {
                    isApplied = false;

                    if (enemy.ModelName == CFG.Current.Toolbar_EntityGroup_Attribute)
                    {
                        isApplied = true;
                    }
                }

                if (SelectedFilter is EntityFilterType.NpcParamID)
                {
                    isApplied = false;

                    if (enemy.NPCParamID.ToString() == CFG.Current.Toolbar_EntityGroup_Attribute)
                    {
                        isApplied = true;
                    }
                }

                if (SelectedFilter is EntityFilterType.NpcThinkParamID)
                {
                    isApplied = false;

                    if (enemy.NPCParamID.ToString() == CFG.Current.Toolbar_EntityGroup_Attribute)
                    {
                        isApplied = true;
                    }
                }

                if (isApplied)
                {
                    for (int i = 0; i < enemy.EntityGroupIDs.Length; i++)
                    {
                        if (enemy.EntityGroupIDs[i] == 0)
                        {
                            enemy.EntityGroupIDs[i] = (uint)CFG.Current.Toolbar_EntityGroupID;

                            TaskLogs.AddLog($"Added new Entity Group ID {CFG.Current.Toolbar_EntityGroupID} to {enemy.Name}.");
                            break;
                        }
                    }
                }
            }

            map.Write(filepath);
        }

        // Sekiro
        if (Editor.Project.ProjectType == ProjectType.SDT)
        {
            MSBS map = MSBS.Read(filepath);

            // Enemies
            foreach (var part in map.Parts.Enemies)
            {
                MSBS.Part.Enemy enemy = part;

                bool isApplied = true;

                if (SelectedFilter is EntityFilterType.ChrID)
                {
                    isApplied = false;

                    if (enemy.ModelName == CFG.Current.Toolbar_EntityGroup_Attribute)
                    {
                        isApplied = true;
                    }
                }

                if (SelectedFilter is EntityFilterType.NpcParamID)
                {
                    isApplied = false;

                    if (enemy.NPCParamID.ToString() == CFG.Current.Toolbar_EntityGroup_Attribute)
                    {
                        isApplied = true;
                    }
                }

                if (SelectedFilter is EntityFilterType.NpcThinkParamID)
                {
                    isApplied = false;

                    if (enemy.NPCParamID.ToString() == CFG.Current.Toolbar_EntityGroup_Attribute)
                    {
                        isApplied = true;
                    }
                }

                if (isApplied)
                {
                    for (int i = 0; i < enemy.EntityGroupIDs.Length; i++)
                    {
                        if (enemy.EntityGroupIDs[i] == 0)
                        {
                            enemy.EntityGroupIDs[i] = CFG.Current.Toolbar_EntityGroupID;

                            TaskLogs.AddLog($"Added new Entity Group ID {CFG.Current.Toolbar_EntityGroupID} to {enemy.Name}.");
                            break;
                        }
                    }
                }
            }

            map.Write(filepath);
        }

        // DS3
        if (Editor.Project.ProjectType == ProjectType.DS3)
        {
            MSB3 map = MSB3.Read(filepath);

            // Enemies
            foreach (var part in map.Parts.Enemies)
            {
                MSB3.Part.Enemy enemy = part;

                bool isApplied = true;

                if (SelectedFilter is EntityFilterType.ChrID)
                {
                    isApplied = false;

                    if (enemy.ModelName == CFG.Current.Toolbar_EntityGroup_Attribute)
                    {
                        isApplied = true;
                    }
                }

                if (SelectedFilter is EntityFilterType.NpcParamID)
                {
                    isApplied = false;

                    if (enemy.NPCParamID.ToString() == CFG.Current.Toolbar_EntityGroup_Attribute)
                    {
                        isApplied = true;
                    }
                }

                if (SelectedFilter is EntityFilterType.NpcThinkParamID)
                {
                    isApplied = false;

                    if (enemy.NPCParamID.ToString() == CFG.Current.Toolbar_EntityGroup_Attribute)
                    {
                        isApplied = true;
                    }
                }

                if (isApplied)
                {
                    for (int i = 0; i < enemy.EntityGroups.Length; i++)
                    {
                        if (enemy.EntityGroups[i] == 0)
                        {
                            enemy.EntityGroups[i] = CFG.Current.Toolbar_EntityGroupID;

                            TaskLogs.AddLog($"Added new Entity Group ID {CFG.Current.Toolbar_EntityGroupID} to {enemy.Name}.");
                            break;
                        }
                    }
                }
            }

            map.Write(filepath);
        }
    }

    /// <summary>
    /// Map Object Namer
    /// </summary>
    
}
