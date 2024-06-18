using ImGuiNET;
using StudioCore.Core;
using StudioCore.Interface;
using StudioCore.MsbEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Toolbar
{
    public static class MapAction_TogglePatrolRoutes
    {
        public static void Select(ViewportSelection _selection)
        {
            if (Smithbox.ProjectType != ProjectType.DS2S && Smithbox.ProjectType != ProjectType.DS2)
            {
                if (ImGui.RadioButton("Toggle Patrol Route Visibility##tool_Selection_Render_Patrol_Routes", MapEditorState.SelectedAction == MapEditorAction.Selection_Render_Patrol_Routes))
                {
                    MapEditorState.SelectedAction = MapEditorAction.Selection_Render_Patrol_Routes;
                }

                if (!CFG.Current.Interface_MapEditor_Toolbar_ActionList_TopToBottom)
                {
                    ImGui.SameLine();
                }
            }
        }

        public static void Configure(ViewportSelection _selection)
        {
            if (MapEditorState.SelectedAction == MapEditorAction.Selection_Render_Patrol_Routes)
            {
                ImguiUtils.WrappedText("Toggle the rendering of patrol route connections.");
                ImguiUtils.WrappedText("");
            }
        }

        public static void Act(ViewportSelection _selection)
        {
            if (MapEditorState.SelectedAction == MapEditorAction.Selection_Render_Patrol_Routes)
            {
                if (Smithbox.ProjectType != ProjectType.DS2S && Smithbox.ProjectType != ProjectType.DS2)
                {
                    if (ImGui.Button("Apply##action_Selection_Render_Patrol_Routes", new Vector2(200, 32)))
                    {
                        PatrolDrawManager.Generate(MapEditorState.Universe);
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("Clear##action_Selection_Clear_Patrol_Routes", new Vector2(200, 32)))
                    {
                        PatrolDrawManager.Clear();
                    }
                }
            }
        }

        public static void Shortcuts()
        {
            if (MapEditorState.SelectedAction == MapEditorAction.Selection_Render_Patrol_Routes)
            {
                ImguiUtils.WrappedText($"Shortcut: {ImguiUtils.GetKeybindHint(KeyBindings.Current.Toolbar_RenderEnemyPatrolRoutes.HintText)}");
            }
        }
    }
}
