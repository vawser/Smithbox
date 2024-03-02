using ImGuiNET;
using SoulsFormats;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Toolbar
{
    // TODO:
    // Change this to allow the definition of a attribute:entity group ID map, e.g.
    // c0000:10005000
    // c1010:10005010
    // etc
    // This would allow for all of a user's desired assignments to be done in one iteration,
    // which is important since it takes longer than a minute to save all Elden Ring's MSBs

    // TODO:
    // Allow user to select attribute, rather than pre-defined list

    public static class MapAction_AssignEntityGroupID
    {
        public static List<string> FilterType = new List<string>()
        {
            "None",
            "Character ID",
            "NPC Param ID",
            "NPC Think Param ID"
        };

        public static string SelectedFilterType = FilterType[0];

        public static string SelectedMapFilter = "All";

        public static void Select(ViewportSelection _selection)
        {
            if (Project.Type == ProjectType.ER || Project.Type == ProjectType.SDT || Project.Type == ProjectType.DS3)
            {
                if (CFG.Current.Toolbar_Show_Assign_Entity_Group_ID)
                {
                    if (ImGui.Selectable("Mass Entity Group ID Assignment##tool_Selection_Assign_Entity_Group_ID", false, ImGuiSelectableFlags.AllowDoubleClick))
                    {
                        MapEditorState.CurrentTool = SelectedTool.Selection_Assign_Entity_Group_ID;

                        if (ImGui.IsMouseDoubleClicked(0))
                        {
                            Act(_selection);
                        }
                    }
                }
            }
        }

        public static void Configure(ViewportSelection _selection)
        {
            if (MapEditorState.CurrentTool == SelectedTool.Selection_Assign_Entity_Group_ID)
            {
                ImGui.Text("Assign an Entity Group ID to all entities across all maps,\noptionally filtering by specific attributes.");
                ImGui.Text("WARNING: this action cannot be undone.");

                ImGui.Separator();
                ImGui.Text("Entity Group ID");
                ImGui.Separator();
                ImGui.InputInt("##entityGroupInput", ref CFG.Current.Toolbar_EntityGroupID);

                ImGui.Separator();
                ImGui.Text("Filter");
                ImGui.Separator();

                if (ImGui.BeginCombo("##filterAttribute", SelectedFilterType))
                {
                    foreach (var entry in FilterType)
                    {
                        if (ImGui.Selectable($"{entry}"))
                        {
                            SelectedFilterType = entry;
                            break;
                        }
                    }

                    ImGui.EndCombo();
                }
                ImguiUtils.ShowHoverTooltip("When assigning the Entity Group ID, the action will only assign it to entities that match this attribute.");
                ImGui.InputText("##entityGroupAttribute", ref CFG.Current.Toolbar_EntityGroup_Attribute, 255);

                ImGui.Separator();
                ImGui.Text("Target");
                ImGui.Separator();

                if (ImGui.BeginCombo("##mapTargetFilter", SelectedMapFilter))
                {
                    IOrderedEnumerable<KeyValuePair<string, ObjectContainer>> orderedMaps = MapEditorState.Universe.LoadedObjectContainers.OrderBy(k => k.Key);

                    foreach (var entry in orderedMaps)
                    {
                        if (ImGui.Selectable($"{entry.Key}"))
                        {
                            SelectedMapFilter = entry.Key;
                            break;
                        }
                    }

                    if (ImGui.Selectable($"All"))
                    {
                        SelectedMapFilter = "All";
                    }

                    ImGui.EndCombo();
                }
                ImguiUtils.ShowHoverTooltip("When assigning the Entity Group ID, the action will only assign it to entities that match this attribute.");
                
                if (SelectedMapFilter == "All")
                {
                    ImGui.Text("WARNING: applying this to all maps will take a few minutes,\nexpect Smithbox to hang until it finishes.");
                }
            }
        }

        public static void Act(ViewportSelection _selection)
        {
            DialogResult result = DialogResult.None;

            if (SelectedMapFilter == "All")
            {
                result = PlatformUtils.Instance.MessageBox($"You are about to assign an Entity Group ID to entities across all maps. Are you sure?", $"Smithbox", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            }
            else
            {
                 result = PlatformUtils.Instance.MessageBox($"You are about to assign an Entity Group ID to entities across {SelectedMapFilter}. Are you sure?", $"Smithbox", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            }

            if (result == DialogResult.Yes)
            {
                // Save current and then unload
                EditorContainer.MsbEditor.Save();
                MapEditorState.Universe.UnloadAll();

                if (SelectedMapFilter == "All")
                {
                    IOrderedEnumerable<KeyValuePair<string, ObjectContainer>> orderedMaps = MapEditorState.Universe.LoadedObjectContainers.OrderBy(k => k.Key);

                    foreach (KeyValuePair<string, ObjectContainer> lm in orderedMaps)
                    {
                        ApplyEntityGroupIdChange(lm.Key);
                    }
                }
                else
                {
                    ApplyEntityGroupIdChange(SelectedMapFilter);
                }
            }
        }

        public static void ApplyEntityGroupIdChange(string mapid)
        {
            var filepath = $"{Project.GameModDirectory}\\map\\MapStudio\\{mapid}.msb.dcx";

            // Elden Ring
            if (Project.Type == ProjectType.ER)
            {
                MSBE map = MSBE.Read(filepath);

                // Enemies
                foreach (var part in map.Parts.Enemies)
                {
                    MSBE.Part.Enemy enemy = part;

                    bool isApplied = true;

                    if(SelectedFilterType == "Character ID")
                    {
                        isApplied = false;

                        if (enemy.ModelName == CFG.Current.Toolbar_EntityGroup_Attribute)
                        {
                            isApplied = true;
                        }
                    }

                    if (SelectedFilterType == "NPC Param ID")
                    {
                        isApplied = false;

                        if (enemy.NPCParamID.ToString() == CFG.Current.Toolbar_EntityGroup_Attribute)
                        {
                            isApplied = true;
                        }
                    }

                    if (SelectedFilterType == "NPC Think Param ID")
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
            if (Project.Type == ProjectType.ER)
            {
                MSBS map = MSBS.Read(filepath);

                // Enemies
                foreach (var part in map.Parts.Enemies)
                {
                    MSBS.Part.Enemy enemy = part;

                    bool isApplied = true;

                    if (SelectedFilterType == "Character ID")
                    {
                        isApplied = false;

                        if (enemy.ModelName == CFG.Current.Toolbar_EntityGroup_Attribute)
                        {
                            isApplied = true;
                        }
                    }

                    if (SelectedFilterType == "NPC Param ID")
                    {
                        isApplied = false;

                        if (enemy.NPCParamID.ToString() == CFG.Current.Toolbar_EntityGroup_Attribute)
                        {
                            isApplied = true;
                        }
                    }

                    if (SelectedFilterType == "NPC Think Param ID")
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
            if (Project.Type == ProjectType.ER)
            {
                MSB3 map = MSB3.Read(filepath);

                // Enemies
                foreach (var part in map.Parts.Enemies)
                {
                    MSB3.Part.Enemy enemy = part;

                    bool isApplied = true;

                    if (SelectedFilterType == "Character ID")
                    {
                        isApplied = false;

                        if (enemy.ModelName == CFG.Current.Toolbar_EntityGroup_Attribute)
                        {
                            isApplied = true;
                        }
                    }

                    if (SelectedFilterType == "NPC Param ID")
                    {
                        isApplied = false;

                        if (enemy.NPCParamID.ToString() == CFG.Current.Toolbar_EntityGroup_Attribute)
                        {
                            isApplied = true;
                        }
                    }

                    if (SelectedFilterType == "NPC Think Param ID")
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
    }
}

