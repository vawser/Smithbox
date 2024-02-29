using ImGuiNET;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Toolbar
{
    public static class MapAction_AssignEntityGroupID
    {
        public static bool AffectAllMaps = false;

        public static bool FilterByChr = true;
        public static bool FilterByNpcParam = false;

        public static void Select(ViewportSelection _selection)
        {
            if(CFG.Current.Toolbar_Show_Assign_Entity_Group_ID)
            {
                if (ImGui.Selectable("Assign Entity Group ID##tool_Selection_Assign_Entity_Group_ID", false, ImGuiSelectableFlags.AllowDoubleClick))
                {
                    MapEditorState.CurrentTool = SelectedTool.Selection_Assign_Entity_Group_ID;

                    if (ImGui.IsMouseDoubleClicked(0) && _selection.IsSelection())
                    {
                        if (MapEditorState.LoadedMaps.Any())
                        {
                            Act(_selection);
                        }
                    }
                }
            }
        }

        public static void Configure(ViewportSelection _selection)
        {
            ImGui.Text("Assign an Entity Group ID to all entities, optionally filtering by specific attributes.");
            ImGui.Text("WARNING: this action cannot be undone.");
            ImGui.Separator();

            if (MapEditorState.CurrentTool == SelectedTool.Selection_Assign_Entity_Group_ID)
            {
                ImGui.Text("Type");
                ImGui.Separator();
                if(ImGui.BeginCombo("targetType", "Loaded Maps"))
                {
                    if (ImGui.Selectable("All Maps"))
                    {
                        AffectAllMaps = true;
                    }

                    if (ImGui.Selectable("Loaded Maps"))
                    {
                        AffectAllMaps = false;
                    }
                    ImGui.EndCombo();
                }
                ImguiUtils.ShowHoverTooltip("Determines whether this action affects ALL maps or loaded maps only.");

                ImGui.Separator();
                ImGui.Text("Entity Group ID");
                ImGui.Separator();
                ImGui.InputInt("##entityGroupInput", ref CFG.Current.Toolbar_EntityGroupID);

                ImGui.Separator();
                ImGui.Text("Filter");
                ImGui.Separator();

                if (ImGui.BeginCombo("filterAttribute", "ModelName"))
                {
                    if (ImGui.Selectable("ModelName"))
                    {
                        FilterByChr = true;
                        FilterByNpcParam = false;
                    }

                    if (ImGui.Selectable("NpcParam"))
                    {
                        FilterByChr = false;
                        FilterByNpcParam = true;
                    }
                    ImGui.EndCombo();
                }
                ImguiUtils.ShowHoverTooltip("When assigning the Entity Group ID, the action will only assign it to entities that match this attribute.");
                ImGui.InputText("##entityGroupAttribute", ref CFG.Current.Toolbar_EntityGroup_Attribute, 255);
            }
        }

        public static void Act(ViewportSelection _selection)
        {

        }
    }
}
