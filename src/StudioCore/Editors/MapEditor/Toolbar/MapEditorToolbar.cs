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

            MapToolbar.ActionManager = _actionManager;
            MapToolbar.Scene = _scene;
            MapToolbar.Universe = _universe;
            MapToolbar.Viewport = _viewport;
            MapToolbar.Toolbar = this;
        }

        public void OnGui()
        {
            var scale = Smithbox.GetUIScale();

            if (Project.Type == ProjectType.Undefined)
                return;

            Action_GenerateNavigationData.OnTextReset();
            MapToolbar.LoadedMaps = _universe.LoadedObjectContainers.Values.Where(x => x != null);

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
            Action_GoToInObjectList.Select(_selection);
            Action_FrameInViewport.Select(_selection);
            Action_MoveToCamera.Select(_selection);
            Action_MoveToGrid.Select(_selection);

            Action_TogglePresence.Select(_selection);
            Action_ToggleVisibility.Select(_selection);

            Action_Create.Select(_selection);
            Action_Duplicate.Select(_selection);
            Action_Rotate.Select(_selection);
            Action_Scramble.Select(_selection);
            Action_Replicate.Select(_selection);

            ImGui.Separator();
            ImGui.Text("Global actions");
            ImguiUtils.ShowHoverTooltip("Double-click to use. These actions are done in the global context.");
            ImGui.Separator();

            // Global
            Action_ToggleObjectVisibilityByTag.Select(_selection);
            Action_TogglePatrolRoutes.Select(_selection);
            Action_CheckDuplicateEntityID.Select(_selection);
            Action_GenerateNavigationData.Select(_selection);

            ImGui.EndChild();
        }

        public void DisplayToolConfiguration()
        {
            ImGui.BeginChild("toolconfiguration");

            // Contextual
            Action_GoToInObjectList.Configure(_selection);
            Action_FrameInViewport.Configure(_selection);
            Action_MoveToCamera.Configure(_selection);
            Action_MoveToGrid.Configure(_selection);

            Action_TogglePresence.Configure(_selection);
            Action_ToggleVisibility.Configure(_selection);

            Action_Create.Configure(_selection);
            Action_Duplicate.Configure(_selection);
            Action_Rotate.Configure(_selection);
            Action_Scramble.Configure(_selection);
            Action_Replicate.Configure(_selection);

            // Global
            Action_ToggleObjectVisibilityByTag.Configure(_selection);
            Action_TogglePatrolRoutes.Configure(_selection);
            Action_CheckDuplicateEntityID.Configure(_selection);
            Action_GenerateNavigationData.Configure(_selection);

            ImGui.EndChild();
        }


        /// <summary>
        /// Save selected object's position to Position clipboard
        /// </summary>
        public void CopyCurrentPosition(PropertyInfo prop, object obj)
        {
            CFG.Current.SavedPosition = (Vector3)prop.GetValue(obj, null);
        }

        /// <summary>
        /// Paste saved position to current selection Position property
        /// </summary>
        public void PasteSavedPosition()
        {
            List<ViewportAction> actlist = new();
            foreach (Entity sel in _selection.GetFilteredSelection<Entity>())
            {
                actlist.Add(sel.ApplySavedPosition());
            }

            CompoundAction action = new(actlist);
            _actionManager.ExecuteAction(action);
        }

        /// <summary>
        /// Save selected object's position to Rotation clipboard
        /// </summary>
        public void CopyCurrentRotation(PropertyInfo prop, object obj)
        {
            CFG.Current.SavedRotation = (Vector3)prop.GetValue(obj, null);
        }

        /// <summary>
        /// Paste saved rotation to current selection Rotation property
        /// </summary>
        public void PasteSavedRotation()
        {
            List<ViewportAction> actlist = new();
            foreach (Entity sel in _selection.GetFilteredSelection<Entity>())
            {
                actlist.Add(sel.ApplySavedRotation());
            }

            CompoundAction action = new(actlist);
            _actionManager.ExecuteAction(action);
        }

        /// <summary>
        /// Save selected object's scale to Scale clipboard
        /// </summary>
        public void CopyCurrentScale(PropertyInfo prop, object obj)
        {
            CFG.Current.SavedScale = (Vector3)prop.GetValue(obj, null);
        }

        /// <summary>
        /// Paste saved scale to current selection Scale property
        /// </summary>
        public void PasteSavedScale()
        {
            List<ViewportAction> actlist = new();
            foreach (Entity sel in _selection.GetFilteredSelection<Entity>())
            {
                actlist.Add(sel.ApplySavedScale());
            }

            CompoundAction action = new(actlist);
            _actionManager.ExecuteAction(action);
        }
    }
}
