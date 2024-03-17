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
using StudioCore.Platform;

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

            if (!CFG.Current.Interface_MapEditor_Toolbar)
                return;

            MapAction_GenerateNavigationData.OnTextReset();
            MapEditorState.LoadedMaps = _universe.LoadedObjectContainers.Values.Where(x => x != null);

            ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
            ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * scale, ImGuiCond.FirstUseEver);

            if (ImGui.Begin($"Toolbar##MapEditorToolbar"))
            {
                if (CFG.Current.Interface_MapEditor_Toolbar_HorizontalOrientation)
                {
                    ImGui.Columns(2);

                    ImGui.BeginChild("##MapEditorToolbar_Selection");

                    ShowActionList();

                    ImGui.EndChild();

                    ImGui.NextColumn();

                    ImGui.BeginChild("##MapEditorToolbar_Configuration");

                    ShowSelectedConfiguration();

                    ImGui.EndChild();
                }
                else
                {
                    ShowActionList();

                    ImGui.BeginChild("##MapEditorToolbar_Configuration");

                    ShowSelectedConfiguration();

                    ImGui.EndChild();
                }
            }

            ImGui.End();
            ImGui.PopStyleColor(1);
        }

        public void ShowActionList()
        {
            ImGui.Separator();
            ImGui.AlignTextToFramePadding();
            ImGui.Text("Actions");
            ImguiUtils.ShowHoverTooltip("Click to select a toolbar action.");
            ImGui.SameLine();

            if (ImGui.Button($"{ForkAwesome.Refresh}##SwitchOrientation"))
            {
                CFG.Current.Interface_MapEditor_Toolbar_HorizontalOrientation = !CFG.Current.Interface_MapEditor_Toolbar_HorizontalOrientation;
            }
            ImguiUtils.ShowHoverTooltip("Toggle the orientation of the toolbar.");
            ImGui.SameLine();

            if (ImGui.Button($"{ForkAwesome.ExclamationTriangle}##PromptUser"))
            {
                if(CFG.Current.Interface_MapEditor_PromptUser)
                {
                    CFG.Current.Interface_MapEditor_PromptUser = false;
                    PlatformUtils.Instance.MessageBox("Map Editor Toolbar will no longer prompt the user.", "Smithbox", MessageBoxButtons.OK);
                }
                else
                {
                    CFG.Current.Interface_MapEditor_PromptUser = true;
                    PlatformUtils.Instance.MessageBox("Map Editor Toolbar will prompt user before applying certain toolbar actions.", "Smithbox", MessageBoxButtons.OK);
                }
            }
            ImguiUtils.ShowHoverTooltip("Toggle whether certain toolbar actions prompt the user before applying.");
            ImGui.Separator();

            // Contextual
            MapAction_GoToInObjectList.Select(_selection);
            MapAction_FrameInViewport.Select(_selection);
            MapAction_MoveToCamera.Select(_selection);
            MapAction_MoveToGrid.Select(_selection);

            MapAction_TogglePresence.Select(_selection);
            MapAction_ToggleVisibility.Select(_selection);

            MapAction_Duplicate.Select(_selection);
            MapAction_Rotate.Select(_selection);
            MapAction_Scramble.Select(_selection);
            MapAction_Replicate.Select(_selection);

            // Global
            MapAction_Create.Select(_selection);
            MapAction_AssignEntityGroupID.Select(_selection);
            MapAction_ToggleObjectVisibilityByTag.Select(_selection);
            MapAction_TogglePatrolRoutes.Select(_selection);
            MapAction_CheckForErrors.Select(_selection);
            MapAction_GenerateNavigationData.Select(_selection);
        }

        public void ShowSelectedConfiguration()
        {
            ImGui.Indent(10.0f);
            ImGui.Separator();
            ImGui.Text("Configuration");
            ImGui.Separator();

            // Shortcut: Contextual
            MapAction_GoToInObjectList.Shortcuts();
            MapAction_FrameInViewport.Shortcuts();
            MapAction_MoveToCamera.Shortcuts();
            MapAction_MoveToGrid.Shortcuts();

            MapAction_TogglePresence.Shortcuts();
            MapAction_ToggleVisibility.Shortcuts();

            MapAction_Duplicate.Shortcuts();
            MapAction_Rotate.Shortcuts();
            MapAction_Scramble.Shortcuts();
            MapAction_Replicate.Shortcuts();

            // Shortcut: Global
            MapAction_Create.Shortcuts();
            MapAction_AssignEntityGroupID.Shortcuts();
            MapAction_ToggleObjectVisibilityByTag.Shortcuts();
            MapAction_TogglePatrolRoutes.Shortcuts();
            MapAction_CheckForErrors.Shortcuts();
            MapAction_GenerateNavigationData.Shortcuts();

            // Configure: Contextual
            MapAction_GoToInObjectList.Configure(_selection);
            MapAction_FrameInViewport.Configure(_selection);
            MapAction_MoveToCamera.Configure(_selection);
            MapAction_MoveToGrid.Configure(_selection);

            MapAction_TogglePresence.Configure(_selection);
            MapAction_ToggleVisibility.Configure(_selection);

            MapAction_Duplicate.Configure(_selection);
            MapAction_Rotate.Configure(_selection);
            MapAction_Scramble.Configure(_selection);
            MapAction_Replicate.Configure(_selection);

            // Configure: Global
            MapAction_Create.Configure(_selection);
            MapAction_AssignEntityGroupID.Configure(_selection);
            MapAction_ToggleObjectVisibilityByTag.Configure(_selection);
            MapAction_TogglePatrolRoutes.Configure(_selection);
            MapAction_CheckForErrors.Configure(_selection);
            MapAction_GenerateNavigationData.Configure(_selection);

            // Act: Contextual
            MapAction_GoToInObjectList.Act(_selection);
            MapAction_FrameInViewport.Act(_selection);
            MapAction_MoveToCamera.Act(_selection);
            MapAction_MoveToGrid.Act(_selection);

            MapAction_TogglePresence.Act(_selection);
            MapAction_ToggleVisibility.Act(_selection);

            MapAction_Duplicate.Act(_selection);
            MapAction_Rotate.Act(_selection);
            MapAction_Scramble.Act(_selection);
            MapAction_Replicate.Act(_selection);

            // Act: Global
            MapAction_Create.Act(_selection);
            MapAction_AssignEntityGroupID.Act(_selection);
            MapAction_ToggleObjectVisibilityByTag.Act(_selection);
            MapAction_TogglePatrolRoutes.Act(_selection);
            MapAction_CheckForErrors.Act(_selection);
            MapAction_GenerateNavigationData.Act(_selection);
        }
    }
}
