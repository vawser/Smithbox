using ImGuiNET;
using StudioCore.Interface;
using StudioCore.MsbEditor;
using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Toolbar
{
    public static class Action_TogglePatrolRoutes
    {
        public static void Select(ViewportSelection _selection)
        {
            if (CFG.Current.Toolbar_Show_Render_Patrol_Routes)
            {
                if (Project.Type is not ProjectType.DS2S)
                {
                    if (ImGui.Selectable("Patrol Routes##tool_Selection_Render_Patrol_Routes", false, ImGuiSelectableFlags.AllowDoubleClick))
                    {
                        MapToolbar.CurrentTool = SelectedTool.Selection_Render_Patrol_Routes;

                        if (ImGui.IsMouseDoubleClicked(0) && _selection.IsSelection())
                        {
                            Act(_selection);
                        }
                    }
                }
            }
        }

        public static void Act(ViewportSelection _selection)
        {
            if (Project.Type is not ProjectType.DS2S)
            {
                PatrolDrawManager.Generate(MapToolbar.Universe);
            }
        }

        public static void Configure(ViewportSelection _selection)
        {
            if (MapToolbar.CurrentTool == SelectedTool.Selection_Render_Patrol_Routes)
            {
                ImGui.Text("Toggle the rendering of patrol route connections.");
                ImGui.Separator();
                ImGui.Text($"Shortcut: {ImguiUtils.GetKeybindHint(KeyBindings.Current.Toolbar_RenderEnemyPatrolRoutes.HintText)}");
                ImGui.Separator();

                if (ImGui.Button("Clear"))
                {
                    PatrolDrawManager.Clear();
                }
            }
        }
    }
}
