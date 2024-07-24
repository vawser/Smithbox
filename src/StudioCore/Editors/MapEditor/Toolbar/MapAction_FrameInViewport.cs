﻿using ImGuiNET;
using StudioCore.Interface;
using StudioCore.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Veldrid.Utilities;

namespace StudioCore.Editors.MapEditor.Toolbar
{
    public static class MapAction_FrameInViewport
    {
        public static void Select(ViewportSelection _selection)
        {
            if (ImGui.RadioButton("视图帧率 Frame in Viewport##tool_Selection_FrameInViewport", MapEditorState.SelectedAction == MapEditorAction.Selection_Frame_in_Viewport))
            {
                MapEditorState.SelectedAction = MapEditorAction.Selection_Frame_in_Viewport;
            }

            if (!CFG.Current.Interface_MapEditor_Toolbar_ActionList_TopToBottom)
            {
                ImGui.SameLine();
            }
        }

        public static void Configure(ViewportSelection _selection)
        {
            if (MapEditorState.SelectedAction == MapEditorAction.Selection_Frame_in_Viewport)
            {
                ImguiUtils.WrappedText("视图的帧率(多选无效) Frame the current selection in the viewport (first if multiple are selected).");
                ImguiUtils.WrappedText("");
            }
        }

        public static void Act(ViewportSelection _selection)
        {
            if (MapEditorState.SelectedAction == MapEditorAction.Selection_Frame_in_Viewport)
            {
                if (ImGui.Button("应用 Apply##action_Selection_Frame_in_Viewport", new Vector2(200, 32)))
                {
                    if (_selection.IsSelection())
                    {
                        ApplyFrameInViewport(_selection);
                    }
                    else
                    {
                        PlatformUtils.Instance.MessageBox("无对象选中 No object selected.", "Smithbox", MessageBoxButtons.OK);
                    }
                }
            }
        }
        public static void Shortcuts()
        {
            if (MapEditorState.SelectedAction == MapEditorAction.Selection_Frame_in_Viewport)
            {
                ImguiUtils.WrappedText($"快捷方式 Shortcut: {ImguiUtils.GetKeybindHint(KeyBindings.Current.Toolbar_Frame_Selection_in_Viewport.HintText)}");
            }
        }

        public static void ApplyFrameInViewport(ViewportSelection _selection)
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
                MapEditorState.Viewport.FrameBox(box);
            }
        }
    }
}
