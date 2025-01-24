﻿using ImGuiNET;
using StudioCore.Configuration;
using StudioCore.Core.Project;
using StudioCore.Editor;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.MapEditor.Enums;
using StudioCore.Editors.MapEditor.Framework;
using StudioCore.Editors.ModelEditor.Enums;
using StudioCore.Interface;
using StudioCore.MsbEditor;
using StudioCore.Platform;
using StudioCore.Resource.Locators;
using StudioCore.Scene;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace StudioCore.Editors.MapEditor.Core;

public class MapContentView
{
    public MapEditorScreen Screen;
    private IViewport Viewport;

    private ViewportActionManager EditorActionManager;
    private Universe Universe;
    private ViewportSelection Selection;
    private EditorFocusManager FocusManager;

    public string MapID;
    public ObjectContainer Container;

    private string ImguiID = "";
    private int treeImGuiId = 0;

    public MapContentViewType ContentViewType = MapContentViewType.ObjectType;
    public MapContentLoadState ContentLoadState = MapContentLoadState.Unloaded;

    private string SearchBarText = "";

    private bool _setNextFocus;
    private ISelectable _pendingClick;
    private HashSet<Entity> _treeOpenEntities = new();

    public MapContentView(MapEditorScreen screen, string mapID, ObjectContainer container)
    {
        Screen = screen;
        EditorActionManager = screen.EditorActionManager;
        Universe = screen.Universe;
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
        Universe.LoadMap(MapID, selected);

        if(Universe.LoadedObjectContainers.ContainsKey(MapID))
        {
            Container = Universe.LoadedObjectContainers[MapID];
        }
    }

    public void Unload()
    {
        ContentLoadState = MapContentLoadState.Unloaded;

        Screen.EntityTypeCache.RemoveMapFromCache(this);

        Selection.ClearSelection();
        EditorActionManager.Clear();

        if(Container != null)
            Universe.UnloadContainer(Container);

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }

    /// <summary>
    /// Handles the update for each frame
    /// </summary>
    public void OnGui()
    {
        if (ContentLoadState is MapContentLoadState.Unloaded)
            return;

        if (CFG.Current.MapEditor_MapObjectList_ShowMapContentSearch)
        {
            var mapId = MapID;
            var mapName = AliasUtils.GetMapNameAlias(MapID);

            ImGui.SetNextItemWidth(-1);
            ImGui.InputText($"##contentTreeSearch_{ImguiID}", ref SearchBarText, 255);
            UIHelper.ShowHoverTooltip($"Filter the content tree entries for {mapId}: {mapName}");
        }

        ImGui.Separator();

        DisplayContentTree();
    }

    /// <summary>
    /// Handles the display of the MSB contents
    /// </summary>
    public void DisplayContentTree()
    {
        ImGui.BeginChild($"mapContentsTree_{ImguiID}");

        Entity mapRoot = Container?.RootObject;
        ObjectContainerReference mapRef = new(MapID, Universe);
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

        //DisplayContextMenu(selected);
        //HandleSelectionClick(selectTarget, mapRoot, mapRef, nodeopen);

        if (nodeopen)
        {
            var scale = DPI.GetUIScale();
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(8.0f, 3.0f) * scale);

            TypeView((MapContainer)Container);

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
    /// Handles the right-click context menu
    /// </summary>
    private void DisplayContextMenu(bool selected)
    {
        if (ImGui.BeginPopupContextItem($@"mapcontext_{MapID}"))
        {

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
                                    MapObjectSelectable(obj, true);
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

                                        foreach (Entity obj in parent.Children)
                                        {
                                            MapObjectSelectable(obj, true);
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
                                    MapObjectSelectable(obj, true);
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
            if (ImGui.IsItemVisible())
            {
                var alias = AliasUtils.GetEntityAliasName(e);
                UIHelper.DisplayAlias(alias);
            }
        }

        if (ImGui.IsItemClicked(0))
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

}