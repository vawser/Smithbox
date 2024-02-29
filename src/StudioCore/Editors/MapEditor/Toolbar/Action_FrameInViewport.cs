using ImGuiNET;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Veldrid.Utilities;

namespace StudioCore.Editors.MapEditor.Toolbar
{
    public static class Action_FrameInViewport
    {
        public static void Select(ViewportSelection _selection)
        {
            if (CFG.Current.Toolbar_Show_Frame_in_Viewport)
            {
                if (ImGui.Selectable("Frame in Viewport##tool_Selection_FrameInViewport", false, ImGuiSelectableFlags.AllowDoubleClick))
                {
                    MapToolbar.CurrentTool = SelectedTool.Selection_Frame_in_Viewport;

                    if (ImGui.IsMouseDoubleClicked(0) && _selection.IsSelection())
                    {
                        Act(_selection);
                    }
                }
            }
        }

        public static void Configure(ViewportSelection _selection)
        {
            if (MapToolbar.CurrentTool == SelectedTool.Selection_Frame_in_Viewport)
            {
                ImGui.Text("Frame the current selection in the viewport (first if multiple are selected).");
                ImGui.Separator();
                ImGui.Text($"Shortcut: {ImguiUtils.GetKeybindHint(KeyBindings.Current.Toolbar_Frame_Selection_in_Viewport.HintText)}");
                ImGui.Separator();
            }
        }

        public static void Act(ViewportSelection _selection)
        {
            HashSet<Entity> selected = _selection.GetFilteredSelection<Entity>();
            var first = false;
            BoundingBox box = new();
            foreach (Entity s in selected)
            {
                if (s.RenderSceneMesh != null)
                {
                    if (!first)
                    {
                        box = s.RenderSceneMesh.GetBounds();
                        first = true;
                    }
                    else
                    {
                        box = BoundingBox.Combine(box, s.RenderSceneMesh.GetBounds());
                    }
                }
                else if (s.Container.RootObject == s)
                {
                    // Selection is transform node
                    Vector3 nodeOffset = new(10.0f, 10.0f, 10.0f);
                    Vector3 pos = s.GetLocalTransform().Position;
                    BoundingBox nodeBox = new(pos - nodeOffset, pos + nodeOffset);
                    if (!first)
                    {
                        first = true;
                        box = nodeBox;
                    }
                    else
                    {
                        box = BoundingBox.Combine(box, nodeBox);
                    }
                }
            }

            if (first)
            {
                MapToolbar.Viewport.FrameBox(box);
            }
        }
    }
}
