using ImGuiNET;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Interface;
using StudioCore.Platform;
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
            if (Smithbox.ProjectType == ProjectType.AC6 || Smithbox.ProjectType == ProjectType.ER || Smithbox.ProjectType == ProjectType.SDT || Smithbox.ProjectType == ProjectType.DS3)
            {
                if (ImGui.RadioButton("Mass Entity Group ID Assignment##tool_Selection_Assign_Entity_Group_ID", MapEditorState.SelectedAction == MapEditorAction.Selection_Assign_Entity_Group_ID))
                {
                    MapEditorState.SelectedAction = MapEditorAction.Selection_Assign_Entity_Group_ID;
                }

                if (!CFG.Current.Interface_MapEditor_Toolbar_ActionList_TopToBottom)
                {
                    ImGui.SameLine();
                }
            }
        }

        public static void Configure(ViewportSelection _selection)
        {
            if (MapEditorState.SelectedAction == MapEditorAction.Selection_Assign_Entity_Group_ID)
            {
                ImguiUtils.WrappedText("Assign an Entity Group ID to all entities across all maps,\noptionally filtering by specific attributes.");
                ImguiUtils.WrappedText("");

                ImguiUtils.WrappedText("Entity Group ID");
                ImGui.InputInt("##entityGroupInput", ref CFG.Current.Toolbar_EntityGroupID);
                ImguiUtils.WrappedText("");

                ImguiUtils.WrappedText("Filter");

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
                ImguiUtils.WrappedText("");

                ImguiUtils.WrappedText("Filter Input");
                ImGui.InputText("##entityGroupAttribute", ref CFG.Current.Toolbar_EntityGroup_Attribute, 255);
                ImguiUtils.WrappedText("");

                ImguiUtils.WrappedText("Target Map");
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
                ImguiUtils.WrappedText("");

                if (SelectedMapFilter == "All")
                {
                    ImguiUtils.WrappedText("WARNING: applying this to all maps will take a few minutes,\nexpect Smithbox to hang until it finishes.");
                    ImguiUtils.WrappedText("");
                }
            }
        }

        public static void Act(ViewportSelection _selection)
        {
            if (MapEditorState.SelectedAction == MapEditorAction.Selection_Assign_Entity_Group_ID)
            {
                if (ImGui.Button("Apply##action_Selection_Assign_Entity_Group_ID", new Vector2(200, 32)))
                {
                    ApplyEntityGroupIdAssignment(_selection);
                }
            }
        }
        public static void Shortcuts()
        {
            if (MapEditorState.SelectedAction == MapEditorAction.Selection_Assign_Entity_Group_ID)
            {
                
            }
        }

        public static void ApplyEntityGroupIdAssignment(ViewportSelection _selection)
        {
            bool proceed = false;

            if (CFG.Current.Interface_MapEditor_PromptUser)
            {
                DialogResult result = DialogResult.None;

                if (SelectedMapFilter == "All")
                {
                    result = PlatformUtils.Instance.MessageBox($"You are about to assign an Entity Group ID to entities across all maps. This action cannot be undone. Are you sure?", $"Smithbox", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                }
                else
                {
                    result = PlatformUtils.Instance.MessageBox($"You are about to assign an Entity Group ID to entities across {SelectedMapFilter}. This action cannot be undone. Are you sure?", $"Smithbox", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                }

                if (result == DialogResult.Yes)
                {
                    proceed = true;
                }
            }
            else
            {
                proceed = true;
            }

            if (proceed)
            {
                // Save current and then unload
                Smithbox.EditorHandler.MapEditor.Save();
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
            var filepath = $"{Smithbox.ProjectRoot}\\map\\MapStudio\\{mapid}.msb.dcx";

            // Armored Core
            if (Smithbox.ProjectType == ProjectType.AC6)
            {
                MSB_AC6 map = MSB_AC6.Read(filepath);

                // Enemies
                foreach (var part in map.Parts.Enemies)
                {
                    MSB_AC6.Part.Enemy enemy = part;

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
            if (Smithbox.ProjectType == ProjectType.ER)
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
            if (Smithbox.ProjectType == ProjectType.SDT)
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
            if (Smithbox.ProjectType == ProjectType.DS3)
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

