using ImGuiNET;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Core.Project;
using StudioCore.Editor;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.MapEditor.Enums;
using StudioCore.Editors.MapEditor.Framework;
using StudioCore.Editors.MapEditor.Helpers;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.Resource.Locators;
using StudioCore.Scene.Interfaces;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Veldrid;

namespace StudioCore.Editors.MapEditor.Core;

public class MapContentView
{
    public MapEditorScreen Screen;
    private IViewport Viewport;

    private ViewportActionManager EditorActionManager;
    private ViewportSelection Selection;
    private EditorFocusManager FocusManager;

    public string MapID;
    public ObjectContainer Container;

    public string ImguiID = "";
    private int treeImGuiId = 0;

    public MapContentViewType ContentViewType = MapContentViewType.ObjectType;
    public MapContentLoadState ContentLoadState = MapContentLoadState.Unloaded;

    private bool _setNextFocus;
    private ISelectable _pendingClick;
    private HashSet<Entity> _treeOpenEntities = new();

    public MapContentView(MapEditorScreen screen, string mapID, ObjectContainer container)
    {
        Screen = screen;
        EditorActionManager = screen.EditorActionManager;
        Selection = screen.Selection;
        Viewport = screen.MapViewportView.Viewport;
        FocusManager = screen.FocusManager;

        MapID = mapID;
        Container = container;

        ImguiID = mapID;
    }

    public void Load(bool selected)
    {
        ContentLoadState = MapContentLoadState.Loaded;

        Selection.ClearSelection();
        Screen.Universe.LoadMap(MapID, selected);
        Container = Screen.Universe.GetObjectContainerForMap(MapID);

        // Reveal hidden entities if "Allow map unload" is false 
        if (!CFG.Current.MapEditor_EnableMapUnload)
        {
            foreach (var entry in Container.Objects)
            {
                entry.EditorVisible = true;
            }
        }
    }

    public void Unload()
    {
        ContentLoadState = MapContentLoadState.Unloaded;

        // Unload
        if (CFG.Current.MapEditor_EnableMapUnload)
        {
            Screen.EntityTypeCache.RemoveMapFromCache(this);

            Selection.ClearSelection();
            EditorActionManager.Clear();

            if (Container != null)
            {
                Screen.Universe.UnloadContainer(Container, true);
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
        // Option to ignore unloading and just keep map in memory
        else
        {
            // Hide entities if "Allow map unload" is false
            foreach (var entry in Container.Objects)
            {
                entry.EditorVisible = false;
            }
        }
    }

    /// <summary>
    /// Handles the update for each frame
    /// </summary>
    public void OnGui()
    {
        if (ContentLoadState is MapContentLoadState.Unloaded)
            return;

        Screen.MapContentFilter.DisplaySearch(this);

        DisplayQuickActionButtons();

        // Reset this every frame, otherwise the map object selectables won't work correctly
        treeImGuiId = 0;

        DisplayContentTree();
    }

    /// <summary>
    /// Handles the show all button
    /// </summary>
    private void DisplayQuickActionButtons()
    {
        ImGui.SameLine();

        // Show All
        if (ImGui.Button($"{ForkAwesome.Eye}"))
        {
            foreach (var entry in Container.Objects)
            {
                entry.EditorVisible = true;
            }
        }
        UIHelper.ShowHoverTooltip("Force all map objects within this map to be shown.");

        // Hide All
        ImGui.SameLine();
        if (ImGui.Button($"{ForkAwesome.EyeSlash}"))
        {
            foreach (var entry in Container.Objects)
            {
                entry.EditorVisible = false;
            }
        }
        UIHelper.ShowHoverTooltip("Force all map objects within this map to be hidden.");

        // Switch View Type
        ImGui.SameLine();
        if (ImGui.Button($"{ForkAwesome.Sort}"))
        {
            if (ContentViewType is MapContentViewType.ObjectType)
            {
                ContentViewType = MapContentViewType.Flat;
            }
            else if (ContentViewType is MapContentViewType.Flat)
            {
                ContentViewType = MapContentViewType.ObjectType;
            }
        }
        UIHelper.ShowHoverTooltip("Switch the map content list style.");
    }

    /// <summary>
    /// Handles the display of the MSB contents
    /// </summary>
    public void DisplayContentTree()
    {
        ImGui.BeginChild($"mapContentsTree_{ImguiID}");

        Entity mapRoot = Container?.RootObject;
        ObjectContainerReference mapRef = new(MapID);
        ISelectable selectTarget = (ISelectable)mapRoot ?? mapRef;

        ImGuiTreeNodeFlags treeflags = ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.SpanAvailWidth;

        var selected = Selection.GetSelection().Contains(mapRoot) || Selection.GetSelection().Contains(mapRef);
        if (selected)
        {
            treeflags |= ImGuiTreeNodeFlags.Selected;
        }

        var nodeopen = false;
        var unsaved = Container != null && Container.HasUnsavedChanges ? "*" : "";

        ImGui.BeginGroup();

        string treeNodeName = $@"{ForkAwesome.Cube} {MapID}";
        string treeNodeNameFormat = $@"{ForkAwesome.Cube} {MapID}{unsaved}";

        if (Container != null && ContentLoadState is MapContentLoadState.Loaded)
        {
            nodeopen = ImGui.TreeNodeEx(treeNodeName, treeflags, treeNodeNameFormat);

            var mapName = AliasUtils.GetMapNameAlias(MapID);
            UIHelper.DisplayAlias(mapName);
        }

        ImGui.EndGroup();

        if (Selection.ShouldGoto(mapRoot) || Selection.ShouldGoto(mapRef))
        {
            ImGui.SetScrollHereY();
            Selection.ClearGotoTarget();
        }

        if (nodeopen)
        {
            ImGui.Indent(); //TreeNodeEx fails to indent as it is inside a group / indentation is reset
        }

        DisplayRootContextMenu(selected);
        HandleSelectionClick(selectTarget, mapRoot, mapRef, nodeopen);

        if (nodeopen)
        {
            var scale = DPI.GetUIScale();
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(8.0f, 3.0f) * scale);

            if (ContentViewType is MapContentViewType.ObjectType)
            {
                TypeView((MapContainer)Container);
            }
            else if (ContentViewType is MapContentViewType.Flat)
            {
                FlatView((MapContainer)Container);
            }

            ImGui.PopStyleVar();
            ImGui.TreePop();
        }

        ImGui.EndChild();
    }

    /// <summary>
    /// Handle the pending click stuff
    /// </summary>
    private void HandleSelectionClick(ISelectable selectTarget, Entity mapRoot, ObjectContainerReference mapRef, bool nodeopen)
    {
        if (ImGui.IsItemClicked())
        {
            _pendingClick = selectTarget;
        }

        if (ImGui.IsMouseDoubleClicked(0) && _pendingClick != null && mapRoot == _pendingClick)
        {
            Viewport.FramePosition(mapRoot.GetLocalTransform().Position, 10f);
        }

        if ((_pendingClick == mapRoot || mapRef.Equals(_pendingClick)) && ImGui.IsMouseReleased(ImGuiMouseButton.Left))
        {
            if (ImGui.IsItemHovered())
            {
                // Only select if a node is not currently being opened/closed
                if (mapRoot == null || nodeopen && _treeOpenEntities.Contains(mapRoot) || !nodeopen && !_treeOpenEntities.Contains(mapRoot))
                {
                    if (InputTracker.GetKey(Key.ControlLeft) || InputTracker.GetKey(Key.ControlRight))
                    {
                        // Toggle Selection
                        if (Selection.GetSelection().Contains(selectTarget))
                        {
                            Selection.RemoveSelection(selectTarget);
                        }
                        else
                        {
                            Selection.AddSelection(selectTarget);
                        }
                    }
                    else
                    {
                        Selection.ClearSelection();
                        Selection.AddSelection(selectTarget);
                    }
                }

                // Update the open/closed state
                if (mapRoot != null)
                {
                    if (nodeopen && !_treeOpenEntities.Contains(mapRoot))
                    {
                        _treeOpenEntities.Add(mapRoot);
                    }
                    else if (!nodeopen && _treeOpenEntities.Contains(mapRoot))
                    {
                        _treeOpenEntities.Remove(mapRoot);
                    }
                }
            }

            _pendingClick = null;
        }
    }

    /// <summary>
    /// Handles the right-click context menu for map root
    /// </summary>
    private void DisplayRootContextMenu(bool selected)
    {
        if (ImGui.BeginPopupContextItem($@"mapcontext_{MapID}"))
        {
            if (ImGui.Selectable("Copy Map ID"))
            {
                PlatformUtils.Instance.SetClipboardText(MapID);
            }
            if (ImGui.Selectable("Copy Map Name"))
            {
                var mapName = AliasUtils.GetMapNameAlias(MapID);
                PlatformUtils.Instance.SetClipboardText(mapName);
            }
            if (Screen.MapQueryView.IsOpen)
            {
                if (ImGui.Selectable("Add to Map Filter"))
                {
                    Screen.MapQueryView.AddMapFilterInput(MapID);
                }
            }

            ImGui.EndPopup();
        }
    }


    /// <summary>
    /// Handles the right-click context menu for map object
    /// </summary>
    private void DisplayMapObjectContextMenu(Entity ent, int imguiID)
    {
        if (ImGui.BeginPopupContextItem($@"mapobjectcontext_{MapID}_{imguiID}"))
        {
            // Only supported for these types
            if (ent.WrappedObject is IMsbPart or IMsbRegion or IMsbEvent)
            {
                // Move Up
                if (ImGui.Selectable("Move Up"))
                {
                    Screen.ActionHandler.ApplyMapObjectOrderChange(OrderMoveDir.Up);
                }
                UIHelper.ShowHoverTooltip($"Move the currently selected map objects up by one in the map object list  for this object type.\n\nShortcut: {KeyBindings.Current.MAP_MoveObjectUp.HintText}");

                // Move Down
                if (ImGui.Selectable("Move Down"))
                {
                    Screen.ActionHandler.ApplyMapObjectOrderChange(OrderMoveDir.Down);
                }
                UIHelper.ShowHoverTooltip($"Move the currently selected map objects down by one in the map object list  for this object type.\n\nShortcut: {KeyBindings.Current.MAP_MoveObjectDown.HintText}");

                // Move Top
                if (ImGui.Selectable("Move to Top"))
                {
                    Screen.ActionHandler.ApplyMapObjectOrderChange(OrderMoveDir.Top);
                }
                UIHelper.ShowHoverTooltip($"Move the currently selected map objects to the top of the map object list for this object type.\n\nShortcut: {KeyBindings.Current.MAP_MoveObjectTop.HintText}");

                // Move Bottom
                if (ImGui.Selectable("Move to Bottom"))
                {
                    Screen.ActionHandler.ApplyMapObjectOrderChange(OrderMoveDir.Bottom);
                }
                UIHelper.ShowHoverTooltip($"Move the currently selected map objects to the bottom of the map object list for this object type.\n\nShortcut: {KeyBindings.Current.MAP_MoveObjectBottom.HintText}");

                ImGui.Separator();
            }

            if (ImGui.Selectable("Duplicate"))
            {
                Screen.ActionHandler.ApplyDuplicate();
            }
            UIHelper.ShowHoverTooltip($"Duplicate the currently selected map objects.\n\nShortcut: {KeyBindings.Current.CORE_DuplicateSelectedEntry.HintText}");

            if (ImGui.Selectable("Duplicate to Map"))
            {
                Screen.ActionHandler.OpenDuplicateToMapPopup = true;
            }
            UIHelper.ShowHoverTooltip($"Duplicate the selected map objects into another map.\n\nShortcut: {KeyBindings.Current.MAP_DuplicateToMap.HintText}");

            if (ImGui.Selectable("Delete"))
            {
                Screen.ActionHandler.ApplyDelete();
            }
            UIHelper.ShowHoverTooltip($"Delete the currently selected map objects.\n\nShortcut: {KeyBindings.Current.CORE_DeleteSelectedEntry.HintText}");

            // Only supported for these types
            if (ent.WrappedObject is IMsbPart or IMsbRegion or BTL.Light)
            {
                if (ImGui.Selectable("Scramble"))
                {
                    Screen.ActionHandler.ApplyScramble();
                }
                UIHelper.ShowHoverTooltip($"Apply the scramble configuration to the currently selected map objects.\n\nShortcut: {KeyBindings.Current.MAP_ScrambleSelection.HintText}");
            }

            // Only supported for these types
            if (ent.WrappedObject is IMsbPart or IMsbRegion)
            {
                if (ImGui.Selectable("Replicate"))
                {
                    Screen.ActionHandler.ApplyReplicate();
                }
                UIHelper.ShowHoverTooltip($"Apply the replicate configuration to the currently selected map objects.\n\nShortcut: {KeyBindings.Current.MAP_ReplicateSelection.HintText}");
            }

            ImGui.Separator();

            // Only supported for these types
            if (ent.WrappedObject is IMsbPart or IMsbRegion)
            {
                if (ImGui.Selectable("Frame in Viewport"))
                {
                    Screen.ActionHandler.ApplyFrameInViewport();
                }
                UIHelper.ShowHoverTooltip($"Frames the current selection in the viewport.\n\nShortcut: {KeyBindings.Current.MAP_FrameSelection.HintText}");

                if (ImGui.Selectable("Move to Grid"))
                {
                    Screen.ActionHandler.ApplyMovetoGrid();
                }
                UIHelper.ShowHoverTooltip($"Move the current selection to the nearest grid point.\n\nShortcut: {KeyBindings.Current.MAP_SetSelectionToGrid.HintText}");

                if (ImGui.Selectable("Move to Camera"))
                {
                    Screen.ActionHandler.ApplyMoveToCamera();
                }
                UIHelper.ShowHoverTooltip($"Move the current selection to the camera position.\n\nShortcut: {KeyBindings.Current.MAP_MoveToCamera.HintText}");

                if (ent.WrappedObject is IMsbRegion or BTL.Light)
                {
                    if (ImGui.Selectable("Toggle Render Type"))
                    {
                        VisualizationHelper.ToggleRenderType(Selection);
                    }
                    UIHelper.ShowHoverTooltip($"Toggles the rendering style for the current selection.\n\nShortcut: {KeyBindings.Current.VIEWPORT_ToggleRenderType.HintText}");
                }

                ImGui.Separator();
            }

            if (ImGui.Selectable("Copy Name"))
            {
                if (Screen.Selection.IsMultiSelection())
                {
                    var fullStr = "";

                    foreach (var entry in Screen.Selection.GetSelection())
                    {
                        var curEnt = (MsbEntity)entry;

                        if (fullStr != "")
                            fullStr = $"{fullStr}, {curEnt.Name}";
                        else
                            fullStr = $"{curEnt.Name}";
                    }

                    PlatformUtils.Instance.SetClipboardText(fullStr);
                }
                else
                {

                    PlatformUtils.Instance.SetClipboardText(ent.Name);
                }
            }
            UIHelper.ShowHoverTooltip($"Copy the current selection's name to the clipboard. For multi-selections, each name is separated by a comma and space.");

            ImGui.EndPopup();
        }
    }

    /// <summary>
    /// Handles the setup for the object type content selectables
    /// </summary>
    private void TypeView(MapContainer map)
    {
        Screen.EntityTypeCache.AddMapToCache(map);

        foreach (KeyValuePair<MsbEntityType, Dictionary<Type, List<MsbEntity>>> cats in
                 Screen.EntityTypeCache._cachedTypeView[map.Name].OrderBy(q => q.Key.ToString()))
        {
            if (cats.Value.Count > 0)
            {
                ImGuiTreeNodeFlags treeflags = ImGuiTreeNodeFlags.OpenOnArrow;

                if (ImGui.TreeNodeEx(cats.Key.ToString(), treeflags))
                {
                    foreach (KeyValuePair<Type, List<MsbEntity>> typ in cats.Value.OrderBy(q => q.Key.Name))
                    {
                        if (typ.Value.Count > 0)
                        {
                            // Regions don't have multiple types in certain games
                            if (cats.Key == MsbEntityType.Region &&
                                Smithbox.ProjectType is ProjectType.DES
                                    or ProjectType.DS1
                                    or ProjectType.DS1R
                                    or ProjectType.BB)
                            {
                                foreach (MsbEntity obj in typ.Value)
                                {
                                    AliasUtils.UpdateEntityAliasName(obj);

                                    if (Screen.MapContentFilter.ContentFilter(this, obj))
                                    {
                                        MapObjectSelectable(obj, true);
                                    }
                                }
                            }
                            else if (cats.Key == MsbEntityType.Light)
                            {
                                foreach (Entity parent in map.BTLParents)
                                {
                                    var parentAD = (ResourceDescriptor)parent.WrappedObject;
                                    if (ImGui.TreeNodeEx($"{typ.Key.Name} {parentAD.AssetName}",
                                            treeflags))
                                    {
                                        ImGui.SetItemAllowOverlap();
                                        var visible = parent.EditorVisible;
                                        ImGui.SameLine();
                                        ImGui.SetCursorPosX(ImGui.GetWindowContentRegionMax().X -
                                                            18.0f * DPI.GetUIScale());
                                        ImGui.PushStyleColor(ImGuiCol.Text, visible
                                            ? new Vector4(1.0f, 1.0f, 1.0f, 1.0f)
                                            : new Vector4(0.6f, 0.6f, 0.6f, 1.0f));
                                        ImGui.TextWrapped(visible ? ForkAwesome.Eye : ForkAwesome.EyeSlash);
                                        ImGui.PopStyleColor();
                                        if (ImGui.IsItemClicked(0))
                                        {
                                            // Hide/Unhide all lights within this BTL.
                                            parent.EditorVisible = !parent.EditorVisible;
                                        }

                                        for (int i = 0; i < parent.Children.Count; i++)
                                        {
                                            var curObj = parent.Children[i];

                                            AliasUtils.UpdateEntityAliasName(curObj);

                                            if (Screen.MapContentFilter.ContentFilter(this, curObj))
                                            {
                                                MapObjectSelectable(curObj, true);
                                            }
                                        }

                                        ImGui.TreePop();
                                    }
                                    else
                                    {
                                        ImGui.SetItemAllowOverlap();
                                        var visible = parent.EditorVisible;
                                        ImGui.SameLine();
                                        ImGui.SetCursorPosX(ImGui.GetWindowContentRegionMax().X -
                                                            18.0f * DPI.GetUIScale());
                                        ImGui.PushStyleColor(ImGuiCol.Text, visible
                                            ? new Vector4(1.0f, 1.0f, 1.0f, 1.0f)
                                            : new Vector4(0.6f, 0.6f, 0.6f, 1.0f));
                                        ImGui.TextWrapped(visible ? ForkAwesome.Eye : ForkAwesome.EyeSlash);
                                        ImGui.PopStyleColor();
                                        if (ImGui.IsItemClicked(0))
                                        {
                                            // Hide/Unhide all lights within this BTL.
                                            parent.EditorVisible = !parent.EditorVisible;
                                        }
                                    }
                                }
                            }
                            else if (ImGui.TreeNodeEx(typ.Key.Name, treeflags))
                            {
                                foreach (MsbEntity obj in typ.Value)
                                {
                                    AliasUtils.UpdateEntityAliasName(obj);

                                    if (Screen.MapContentFilter.ContentFilter(this, obj))
                                    {
                                        MapObjectSelectable(obj, true);
                                    }
                                }

                                ImGui.TreePop();
                            }
                        }
                        else
                        {
                            ImGui.Text($@"   {typ.Key}");
                        }
                    }

                    ImGui.TreePop();
                }
            }
            else
            {
                ImGui.Text($@"   {cats.Key.ToString()}");
            }
        }
    }

    /// <summary>
    /// Handles the basic selectable entry
    /// </summary>
    private unsafe void MapObjectSelectable(Entity e, bool visicon, bool hierarchial = false)
    {
        var scale = DPI.GetUIScale();

        // Main selectable
        if (e is MsbEntity me)
        {
            ImGui.PushID(me.Type + e.Name);
        }
        else
        {
            ImGui.PushID(e.Name);
        }

        var doSelect = false;
        if (_setNextFocus)
        {
            ImGui.SetItemDefaultFocus();
            _setNextFocus = false;
            doSelect = true;
        }

        var nodeopen = false;
        var padding = hierarchial ? "   " : "    ";

        var arrowKeySelect = false;

        if (hierarchial && e.Children.Count > 0)
        {
            ImGuiTreeNodeFlags treeflags = ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.SpanAvailWidth;
            if (Selection.GetSelection().Contains(e))
            {
                treeflags |= ImGuiTreeNodeFlags.Selected;
            }

            nodeopen = ImGui.TreeNodeEx(e.PrettyName, treeflags);
            if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(0))
            {
                if (e.RenderSceneMesh != null)
                {
                    Viewport.FrameBox(e.RenderSceneMesh.GetBounds());
                }
            }
            if (ImGui.IsItemFocused() && (InputTracker.GetKey(Key.Up) || InputTracker.GetKey(Key.Down)))
            {
                doSelect = true;
                arrowKeySelect = true;
            }
        }
        else
        {
            treeImGuiId++;
            var selectableFlags = ImGuiSelectableFlags.AllowDoubleClick | ImGuiSelectableFlags.AllowItemOverlap;

            if (ImGui.Selectable($"{padding}{e.PrettyName}##{treeImGuiId}", Selection.GetSelection().Contains(e), selectableFlags))
            {
                doSelect = true;

                // If double clicked frame the selection in the viewport
                if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                {
                    if (e.RenderSceneMesh != null)
                    {
                        Viewport.FrameBox(e.RenderSceneMesh.GetBounds());
                    }
                }
            }
            if (ImGui.IsItemFocused() && (InputTracker.GetKey(Key.Up) || InputTracker.GetKey(Key.Down)))
            {
                doSelect = true;
                arrowKeySelect = true;
            }

            var alias = AliasUtils.GetEntityAliasName(e);
            if (ImGui.IsItemVisible())
            {
                UIHelper.DisplayAlias(alias);
            }

            DisplayMapObjectContextMenu(e, treeImGuiId);

        }

        if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
        {
            _pendingClick = e;
        }

        if (_pendingClick == e && ImGui.IsMouseReleased(ImGuiMouseButton.Left))
        {
            if (ImGui.IsItemHovered())
            {
                doSelect = true;
            }

            _pendingClick = null;
        }

        if (hierarchial && doSelect)
        {
            if (nodeopen && !_treeOpenEntities.Contains(e) ||
                !nodeopen && _treeOpenEntities.Contains(e))
            {
                doSelect = false;
            }

            if (nodeopen && !_treeOpenEntities.Contains(e))
            {
                _treeOpenEntities.Add(e);

            }
            else if (!nodeopen && _treeOpenEntities.Contains(e))
            {
                _treeOpenEntities.Remove(e);
            }
        }

        if (Selection.ShouldGoto(e))
        {
            // By default, this places the item at 50% in the frame. Use 0 to place it on top.
            ImGui.SetScrollHereY();
            Selection.ClearGotoTarget();
        }

        // Visibility icon
        if (visicon)
        {
            ImGui.SetItemAllowOverlap();
            var visible = e.EditorVisible;
            ImGui.SameLine();
            ImGui.SetCursorPosX(ImGui.GetWindowContentRegionMax().X - 18.0f * DPI.GetUIScale());
            ImGui.PushStyleColor(ImGuiCol.Text, visible
                ? new Vector4(1.0f, 1.0f, 1.0f, 1.0f)
                : new Vector4(0.6f, 0.6f, 0.6f, 1.0f));
            ImGui.TextWrapped(visible ? ForkAwesome.Eye : ForkAwesome.EyeSlash);
            ImGui.PopStyleColor();
            if (ImGui.IsItemClicked(0))
            {
                e.EditorVisible = !e.EditorVisible;
                doSelect = false;
            }
        }

        // If the visibility icon wasn't clicked, perform the selection
        Utils.EntitySelectionHandler(Selection, e, doSelect, arrowKeySelect);

        // If there's children then draw them
        if (nodeopen)
        {
            HierarchyView(e);
            ImGui.TreePop();
        }

        ImGui.PopID();
    }

    /// <summary>
    /// Handles the setup for the heiarchical content selectables
    /// </summary>
    private void HierarchyView(Entity entity)
    {
        foreach (Entity obj in entity.Children)
        {
            if (obj is Entity e)
            {
                MapObjectSelectable(e, true, true);
            }
        }
    }

    private void FlatView(MapContainer map)
    {
        foreach (Entity obj in map.Objects)
        {
            if (obj is MsbEntity e)
            {
                if (Screen.MapContentFilter.ContentFilter(this, obj))
                {
                    MapObjectSelectable(e, true);
                }
            }
        }
    }
}
