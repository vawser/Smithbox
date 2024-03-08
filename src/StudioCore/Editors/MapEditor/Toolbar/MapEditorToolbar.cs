using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using Microsoft.Toolkit.HighPerformance;
using StudioCore.Gui;
using StudioCore.Scene;
using StudioCore.Utilities;
using System.IO;
using System;
using DotNext;
using Veldrid.Utilities;
using SoulsFormats;
using StudioCore.Interface;
using System.Reflection;
using StudioCore.UserProject;
using StudioCore.Banks;
using StudioCore.MsbEditor;
using StudioCore.BanksMain;

namespace StudioCore.Editors.MapEditor.Toolbar
{
    public class MapEditorToolbar
    {
        private readonly ViewportActionManager _actionManager;

        private readonly RenderScene _scene;
        private readonly ViewportSelection _selection;

        private Universe _universe;

        private IViewport _viewport;

        public MapEditorToolbar(RenderScene scene, ViewportSelection sel, ViewportActionManager manager, Universe universe, IViewport viewport)
        {
            _scene = scene;
            _selection = sel;
            _actionManager = manager;
            _universe = universe;

            _viewport = viewport;

            MapEditorState.ActionManager = _actionManager;
            MapEditorState.Scene = _scene;
            MapEditorState.Universe = _universe;
            MapEditorState.Viewport = _viewport;
            MapEditorState.Toolbar = this;
        }

        public void OnGui()
        {
            var scale = Smithbox.GetUIScale();

            if (Project.Type == ProjectType.Undefined)
                return;

            MapAction_GenerateNavigationData.OnTextReset();
            MapEditorState.LoadedMaps = _universe.LoadedObjectContainers.Values.Where(x => x != null);

            ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
            ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * scale, ImGuiCond.FirstUseEver);

            if (ImGui.Begin($@"Toolbar##MsbMenubar"))
            {
                ImGui.Columns(2);

                DisplayToolSelectionList();

                ImGui.NextColumn();

                DisplayToolConfiguration();
            }

            ImGui.End();
            ImGui.PopStyleColor(1);
        }

        public void DisplayToolSelectionList()
        {
            // Selection List
            ImGui.BeginChild("toolselection");

            ImGui.Separator();
            ImGui.Text("Selection actions");
            ImguiUtils.ShowHoverTooltip("Double-click to use. These actions are done in the context of a selection.");
            ImGui.Separator();

            // Contextual
            MapAction_GoToInObjectList.Select(_selection);
            MapAction_FrameInViewport.Select(_selection);
            MapAction_MoveToCamera.Select(_selection);
            MapAction_MoveToGrid.Select(_selection);

            MapAction_TogglePresence.Select(_selection);
            MapAction_ToggleVisibility.Select(_selection);

            MapAction_Create.Select(_selection);
            MapAction_Duplicate.Select(_selection);
            MapAction_Rotate.Select(_selection);
            MapAction_Scramble.Select(_selection);
            MapAction_Replicate.Select(_selection);

            ImGui.Separator();
            ImGui.Text("Global actions");
            ImguiUtils.ShowHoverTooltip("Double-click to use. These actions are done in the global context.");
            ImGui.Separator();

            // Global
            MapAction_AssignEntityGroupID.Select(_selection);
            MapAction_ToggleObjectVisibilityByTag.Select(_selection);
            MapAction_TogglePatrolRoutes.Select(_selection);
            MapAction_CheckForErrors.Select(_selection);
            MapAction_GenerateNavigationData.Select(_selection);

            ImGui.EndChild();
        }

        public void DisplayToolConfiguration()
        {
            ImGui.BeginChild("toolconfiguration");

            // Contextual
            MapAction_GoToInObjectList.Configure(_selection);
            MapAction_FrameInViewport.Configure(_selection);
            MapAction_MoveToCamera.Configure(_selection);
            MapAction_MoveToGrid.Configure(_selection);

            MapAction_TogglePresence.Configure(_selection);
            MapAction_ToggleVisibility.Configure(_selection);

            MapAction_Create.Configure(_selection);
            MapAction_Duplicate.Configure(_selection);
            MapAction_Rotate.Configure(_selection);
            MapAction_Scramble.Configure(_selection);
            MapAction_Replicate.Configure(_selection);

            // Global
            MapAction_AssignEntityGroupID.Configure(_selection);
            MapAction_ToggleObjectVisibilityByTag.Configure(_selection);
            MapAction_TogglePatrolRoutes.Configure(_selection);
            MapAction_CheckForErrors.Configure(_selection);
            MapAction_GenerateNavigationData.Configure(_selection);

            ImGui.EndChild();
        }
    }
}
