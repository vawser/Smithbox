using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Veldrid;
using static Google.Protobuf.Reflection.FieldOptions.Types;
using static StudioCore.Editors.MapEditor.MsbUtils;

namespace StudioCore.Editors.MapEditor;

public class MapContentView
{
    public MapEditorScreen Editor;
    public ProjectEntry Project;

    public string ImguiID = "MapContentView";
    private int treeImGuiId = 0;

    public MapContentViewType ContentViewType = MapContentViewType.ObjectType;

    private bool _setNextFocus;
    private ISelectable _pendingClick;
    private HashSet<Entity> _treeOpenEntities = new();

    public MapContentView(MapEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    /// <summary>
    /// Handles the update for each frame
    /// </summary>
    public void OnGui()
    {
        var scale = DPI.UIScale();

        if (!CFG.Current.Interface_MapEditor_MapContents)
            return;

        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * scale, ImGuiCond.FirstUseEver);

        // Map Contents
        if (ImGui.Begin($@"Map Contents##mapContentsPanel", ImGuiWindowFlags.MenuBar))
        {
            Editor.FocusManager.SwitchMapEditorContext(MapEditorContext.MapContents);

            DisplayMenubar();

            if (Editor.Selection.SelectedMapContainer != null)
            {
                var map = Editor.Selection.SelectedMapContainer;

                Editor.MapContentFilter.DisplaySearch(map);

                DisplayQuickActionButtons(map);

                // Reset this every frame, otherwise the map object selectables won't work correctly
                treeImGuiId = 0;

                DisplayContentTree(map);
            }
        }

        ImGui.End();
        ImGui.PopStyleColor(1);

        Editor.ViewportSelection.ClearGotoTarget();
    }

    public void DisplayMenubar()
    {
        if (ImGui.BeginMenuBar())
        {
            if (ImGui.BeginMenu("Content Display"))
            {
                if (ImGui.MenuItem("Tree"))
                {
                    ContentViewType = MapContentViewType.ObjectType;
                }
                UIHelper.Tooltip("Display the content in the object type tree form.");
                UIHelper.ShowActiveStatus(ContentViewType == MapContentViewType.ObjectType);

                if (ImGui.MenuItem("Flat"))
                {
                    ContentViewType = MapContentViewType.Flat;
                }
                UIHelper.Tooltip("Display the content in the flat form.");
                UIHelper.ShowActiveStatus(ContentViewType == MapContentViewType.Flat);

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Name Display"))
            {
                var curType = CFG.Current.MapEditor_MapContentList_EntryNameDisplayType;

                if (ImGui.MenuItem("Internal"))
                {
                    CFG.Current.MapEditor_MapContentList_EntryNameDisplayType = EntityNameDisplayType.Internal;
                }
                UIHelper.Tooltip("Display the internal map object name only.");
                UIHelper.ShowActiveStatus(curType == EntityNameDisplayType.Internal);

                if (ImGui.MenuItem("Internal + Text"))
                {
                    CFG.Current.MapEditor_MapContentList_EntryNameDisplayType = EntityNameDisplayType.Internal_FMG;
                }
                UIHelper.Tooltip("Display the internal map object name with the associated FMG name as the alias.");
                UIHelper.ShowActiveStatus(curType == EntityNameDisplayType.Internal_FMG);

                ImGui.EndMenu();
            }

            if(Editor.LightAtlasBank.CanUse())
            {
                if (ImGui.BeginMenu("Light Atlases"))
                {
                    if (ImGui.MenuItem("Automatically adjust entries"))
                    {
                        CFG.Current.MapEditor_LightAtlas_AutomaticAdjust = !CFG.Current.MapEditor_LightAtlas_AutomaticAdjust;
                    }
                    UIHelper.Tooltip("If enabled, when a part is renamed, if a light atlas entry points to it, the name reference within the entry is updated to the new name.");
                    UIHelper.ShowActiveStatus(CFG.Current.MapEditor_LightAtlas_AutomaticAdjust);


                    if (ImGui.MenuItem("Automatically add entries"))
                    {
                        CFG.Current.MapEditor_LightAtlas_AutomaticAdd = !CFG.Current.MapEditor_LightAtlas_AutomaticAdd;
                    }
                    UIHelper.Tooltip("If enabled, when new parts are duplicated, the a new light atlas entry pointing to the newly duplicated part is created (deriving the other properties from the source part).");
                    UIHelper.ShowActiveStatus(CFG.Current.MapEditor_LightAtlas_AutomaticAdd);

                    if (ImGui.MenuItem("Automatically delete entries"))
                    {
                        CFG.Current.MapEditor_LightAtlas_AutomaticDelete = !CFG.Current.MapEditor_LightAtlas_AutomaticDelete;
                    }
                    UIHelper.Tooltip("If enabled, when parts are deleted, if there is a light atlas entry pointing to that part, the entry is deleted.");
                    UIHelper.ShowActiveStatus(CFG.Current.MapEditor_LightAtlas_AutomaticDelete);

                    ImGui.EndMenu();
                }
            }

            ImGui.EndMenuBar();
        }
    }

    /// <summary>
    /// Handles the show all button
    /// </summary>
    private void DisplayQuickActionButtons(MapContainer map)
    {
        ImGui.SameLine();

        // Show All
        ImGui.SameLine();
        if (ImGui.Button($"{Icons.Eye}", DPI.IconButtonSize))
        {
            foreach (var entry in map.Objects)
            {
                entry.EditorVisible = true;
            }
        }
        UIHelper.Tooltip("Force all map objects within this map to be shown.");

        // Hide All
        ImGui.SameLine();
        if (ImGui.Button($"{Icons.EyeSlash}", DPI.IconButtonSize))
        {
            foreach (var entry in map.Objects)
            {
                entry.EditorVisible = false;
            }
        }
        UIHelper.Tooltip("Force all map objects within this map to be hidden.");
    }

    /// <summary>
    /// Handles the display of the MSB contents
    /// </summary>
    public void DisplayContentTree(MapContainer map)
    {
        ImGui.BeginChild($"mapContentsTree_{ImguiID}");

        Entity mapRoot = map?.RootObject;

        ObjectContainerReference mapRef = new(map.Name);

        ISelectable selectTarget = (ISelectable)mapRoot ?? mapRef;

        ImGuiTreeNodeFlags treeflags = ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.SpanAvailWidth;

        var selected = Editor.ViewportSelection.GetSelection().Contains(mapRoot) || Editor.ViewportSelection.GetSelection().Contains(mapRef);
        if (selected)
        {
            treeflags |= ImGuiTreeNodeFlags.Selected;
        }

        var nodeopen = false;
        var unsaved = map != null && map.HasUnsavedChanges ? "*" : "";

        ImGui.BeginGroup();

        string treeNodeName = $@"{Icons.Cube} {map.Name}";
        string treeNodeNameFormat = $@"{Icons.Cube} {map.Name}{unsaved}";

        if (map != null)
        {
            nodeopen = ImGui.TreeNodeEx(treeNodeName, treeflags, treeNodeNameFormat);

            var mapName = AliasHelper.GetMapNameAlias(Editor.Project, map.Name);
            UIHelper.DisplayAlias(mapName, CFG.Current.Interface_MapEditor_WrapAliasDisplay);
        }

        ImGui.EndGroup();

        if (Editor.ViewportSelection.ShouldGoto(mapRoot) || Editor.ViewportSelection.ShouldGoto(mapRef))
        {
            ImGui.SetScrollHereY();
            Editor.ViewportSelection.ClearGotoTarget();
        }

        if (nodeopen)
        {
            ImGui.Indent(); //TreeNodeEx fails to indent as it is inside a group / indentation is reset
        }

        DisplayTopContextMenu(map, selected);
        HandleSelectionClick(selectTarget, mapRoot, mapRef, nodeopen);

        if (nodeopen)
        {
            var scale = DPI.UIScale();
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(8.0f, 3.0f) * scale);

            if (ContentViewType is MapContentViewType.ObjectType)
            {
                TypeView(map);
            }
            else if (ContentViewType is MapContentViewType.Flat)
            {
                FlatView(map);
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
            Editor.MapViewportView.Viewport.FramePosition(mapRoot.GetLocalTransform().Position, 10f);
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
                        if (Editor.ViewportSelection.GetSelection().Contains(selectTarget))
                        {
                            Editor.ViewportSelection.RemoveSelection(Editor, selectTarget);
                        }
                        else
                        {
                            Editor.ViewportSelection.AddSelection(Editor, selectTarget);
                        }
                    }
                    else
                    {
                        Editor.ViewportSelection.ClearSelection(Editor);
                        Editor.ViewportSelection.AddSelection(Editor, selectTarget);
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
    private void DisplayTopContextMenu(MapContainer map, bool selected)
    {
        if (ImGui.BeginPopupContextItem($@"mapcontext_{map.Name}"))
        {
            if (ImGui.Selectable("Copy Map ID"))
            {
                PlatformUtils.Instance.SetClipboardText(map.Name);
            }
            if (ImGui.Selectable("Copy Map Name"))
            {
                var mapName = AliasHelper.GetMapNameAlias(Editor.Project, map.Name);
                PlatformUtils.Instance.SetClipboardText(mapName);
            }
            if (Editor.GlobalSearchTool.IsOpen)
            {
                if (ImGui.Selectable("Add to Map Filter"))
                {
                    Editor.GlobalSearchTool.AddMapFilterInput(map.Name);
                }
            }

            ImGui.EndPopup();
        }
    }


    /// <summary>
    /// Handles the right-click context menu for map object
    /// </summary>
    private void DisplayMapObjectContextMenu(MapContainer map, Entity ent, int imguiID)
    {
        if (ImGui.BeginPopupContextItem($@"mapobjectcontext_{map.Name}_{imguiID}"))
        {
            Editor.ReorderAction.OnContext(ent);

            Editor.DuplicateAction.OnContext();
            Editor.DeleteAction.OnContext();
            Editor.DuplicateToMapAction.OnContext();
            Editor.RotateAction.OnContext();
            Editor.ScrambleAction.OnContext(ent);
            Editor.ReplicateAction.OnContext(ent);
            Editor.RenderTypeAction.OnContext(ent);

            ImGui.Separator();

            Editor.FrameAction.OnContext(ent);
            Editor.PullToCameraAction.OnContext(ent);

            ImGui.Separator();

            Editor.EditorVisibilityAction.OnContext();
            Editor.GameVisibilityAction.OnContext();

            ImGui.Separator();

            Editor.SelectionGroupTool.OnContext();

            ImGui.Separator();

            Editor.SelectAllAction.OnContext(ent);

            ImGui.Separator();

            Editor.AdjustToGridAction.OnContext();

            ImGui.Separator();

            Editor.EntityInfoAction.OnContext(ent);

            ImGui.EndPopup();
        }
    }

    /// <summary>
    /// Handles the setup for the object type content selectables
    /// </summary>
    private void TypeView(MapContainer map)
    {
        Editor.EntityTypeCache.AddMapToCache(map);

        foreach (KeyValuePair<MsbEntityType, Dictionary<Type, List<MsbEntity>>> cats in
                 Editor.EntityTypeCache._cachedTypeView[map.Name].OrderBy(q => q.Key.ToString()))
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
                                Editor.Project.ProjectType is ProjectType.DES
                                    or ProjectType.DS1
                                    or ProjectType.DS1R
                                    or ProjectType.BB)
                            {
                                foreach (MsbEntity obj in typ.Value)
                                {
                                    AliasHelper.UpdateEntityAliasName(Editor.Project, obj);

                                    if (Editor.MapContentFilter.ContentFilter(map, obj))
                                    {
                                        MapObjectSelectable(map, obj, true);
                                    }
                                }
                            }
                            else if (cats.Key == MsbEntityType.Light)
                            {
                                foreach (Entity parent in map.BTLParents)
                                {
                                    var parentName = parent.WrappedObject;

                                    if (ImGui.TreeNodeEx($"{typ.Key.Name} {parentName}",
                                            treeflags))
                                    {
                                        for (int i = 0; i < parent.Children.Count; i++)
                                        {
                                            var curObj = parent.Children[i];

                                            AliasHelper.UpdateEntityAliasName(Editor.Project, curObj);

                                            if (Editor.MapContentFilter.ContentFilter(map, curObj))
                                            {
                                                MapObjectSelectable(map, curObj, true, i);
                                            }
                                        }

                                        ImGui.TreePop();
                                    }
                                }
                            }
                            else if (cats.Key == MsbEntityType.LightAtlas)
                            {
                                foreach (Entity parent in map.LightAtlasParents)
                                {
                                    var parentName = parent.WrappedObject;

                                    if (ImGui.TreeNodeEx($"{typ.Key.Name} {parentName}",
                                            treeflags))
                                    {
                                        for (int i = 0; i < parent.Children.Count; i++)
                                        {
                                            var curObj = parent.Children[i];

                                            AliasHelper.UpdateEntityAliasName(Editor.Project, curObj);

                                            if (Editor.MapContentFilter.ContentFilter(map, curObj))
                                            {
                                                MapObjectSelectable(map, curObj, true, i);
                                            }
                                        }

                                        ImGui.TreePop();
                                    }
                                }
                            }
                            else if (cats.Key == MsbEntityType.LightProbeVolume)
                            {
                                foreach (Entity parent in map.LightProbeParents)
                                {
                                    var parentName = parent.WrappedObject;

                                    if (ImGui.TreeNodeEx($"{typ.Key.Name} {parentName}",
                                            treeflags))
                                    {
                                        for (int i = 0; i < parent.Children.Count; i++)
                                        {
                                            var curObj = parent.Children[i];

                                            AliasHelper.UpdateEntityAliasName(Editor.Project, curObj);

                                            if (Editor.MapContentFilter.ContentFilter(map, curObj))
                                            {
                                                MapObjectSelectable(map, curObj, true, i);
                                            }
                                        }

                                        ImGui.TreePop();
                                    }
                                }
                            }
                            else if (ImGui.TreeNodeEx(typ.Key.Name, treeflags))
                            {
                                int index = 0;
                                foreach (MsbEntity obj in typ.Value)
                                {
                                    AliasHelper.UpdateEntityAliasName(Editor.Project, obj);

                                    if (Editor.MapContentFilter.ContentFilter(map, obj))
                                    {
                                        MapObjectSelectable(map, obj, true, index);
                                    }

                                    index++;
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
    private unsafe void MapObjectSelectable(MapContainer map, Entity e, bool visicon, int index = -1, bool hierarchial = false)
    {
        var scale = DPI.UIScale();

        var key = $"Entry {index}";

        if (e.SupportsName)
        {
            if (e.Name != null && e.Name != "null")
            {
                key = e.Name;
            }
        }

        // Main selectable
        if (e is MsbEntity me)
        {
            ImGui.PushID(me.Type + key);
        }
        else
        {
            ImGui.PushID(key);
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

        // Visibility icon
        if (visicon)
        {
            var icon = e.EditorVisible ? Icons.Eye : Icons.EyeSlash;

            ImGui.PushItemFlag(ImGuiItemFlags.NoNav, true);
            ImGui.PushStyleColor(ImGuiCol.Button, Vector4.Zero);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, Vector4.Zero);
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, Vector4.Zero);
            ImGui.PushStyleColor(ImGuiCol.Border, Vector4.Zero);
            if (ImGui.Button($"{icon}##mapObject{key}", DPI.InlineIconButtonSize))
            {
                if (InputTracker.GetKey(KeyBindings.Current.MAP_ToggleMapObjectGroupVisibility))
                {
                    var targetContainer = Editor.Selection.GetMapContainerFromMapID(map.Name);

                    var mapRoot = targetContainer.RootObject;
                    foreach(var entry in mapRoot.Children)
                    {
                        if (entry.WrappedObject.GetType() == e.WrappedObject.GetType())
                        {
                            entry.EditorVisible = !entry.EditorVisible;
                            doSelect = false;
                        }
                    }
                }
                else
                {
                    e.EditorVisible = !e.EditorVisible;
                    doSelect = false;
                }
            }
            ImGui.PopStyleColor(4);
            ImGui.PopItemFlag();
            ImGui.SameLine();

            UIHelper.Tooltip("Toggle visibility state of this map object.");
        }

        if (hierarchial && e.Children.Count > 0)
        {
            ImGuiTreeNodeFlags treeflags = ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.SpanAvailWidth;
            if (Editor.ViewportSelection.GetSelection().Contains(e))
            {
                treeflags |= ImGuiTreeNodeFlags.Selected;
            }

            nodeopen = ImGui.TreeNodeEx(e.PrettyName, treeflags);
            if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(0))
            {
                if (e.RenderSceneMesh != null)
                {
                    Editor.MapViewportView.Viewport.FrameBox(e.RenderSceneMesh.GetBounds(), new Vector3());
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
            var selectableFlags = ImGuiSelectableFlags.AllowDoubleClick | ImGuiSelectableFlags.AllowOverlap;

            var displayName = key;

            if (e.SupportsName && e.PrettyName != null && e.PrettyName != "null")
            {
                if (CFG.Current.MapEditor_MapContentList_EntryNameDisplayType is EntityNameDisplayType.Internal or EntityNameDisplayType.Internal_FMG or EntityNameDisplayType.Internal_Community)
                {
                    displayName = e.PrettyName;
                }
                else if (CFG.Current.MapEditor_MapContentList_EntryNameDisplayType is EntityNameDisplayType.Community or EntityNameDisplayType.Community_FMG)
                {
                    displayName = e.PrettyName;

                    var nameListEntry = Project.MapData.MapObjectNameLists.FirstOrDefault(entry => entry.Key == Editor.Selection.SelectedMapID);

                    if (nameListEntry.Value != null)
                    {
                        var match = nameListEntry.Value.Entries.FirstOrDefault(entry => entry.ID == e.Name);

                        if (match != null)
                        {
                            displayName = match.Name;
                        }
                    }
                }
            }

            if (ImGui.Selectable($"{displayName}##{treeImGuiId}", Editor.ViewportSelection.GetSelection().Contains(e), selectableFlags))
            {
                doSelect = true;

                // If double clicked frame the selection in the viewport
                if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                {
                    if (e.RenderSceneMesh != null)
                    {
                        Editor.MapViewportView.Viewport.FrameBox(e.RenderSceneMesh.GetBounds(), new Vector3());
                    }
                }
            }
            if (ImGui.IsItemFocused() && (InputTracker.GetKey(Key.Up) || InputTracker.GetKey(Key.Down)))
            {
                doSelect = true;
                arrowKeySelect = true;
            }

            if (CFG.Current.MapEditor_MapContentList_EntryNameDisplayType is EntityNameDisplayType.Internal_FMG or EntityNameDisplayType.Community_FMG)
            {
                var alias = AliasHelper.GetEntityAliasName(Editor.Project, e);
                if (ImGui.IsItemVisible())
                {
                    UIHelper.DisplayAlias(alias);
                }
            }
            else if (CFG.Current.MapEditor_MapContentList_EntryNameDisplayType is EntityNameDisplayType.Internal_Community)
            {
                var nameListEntry = Project.MapData.MapObjectNameLists.FirstOrDefault(entry => entry.Key == Editor.Selection.SelectedMapID);

                if (nameListEntry.Value != null)
                {
                    var match = nameListEntry.Value.Entries.FirstOrDefault(entry => entry.ID == e.Name);

                    if (match != null)
                    {
                        if (ImGui.IsItemVisible())
                        {
                            UIHelper.DisplayAlias(match.Name);
                        }
                    }
                }

            }

            DisplayMapObjectContextMenu(map, e, treeImGuiId);

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

        if (Editor.ViewportSelection.ShouldGoto(e))
        {
            // By default, this places the item at 50% in the frame. Use 0 to place it on top.
            ImGui.SetScrollHereY();
            Editor.ViewportSelection.ClearGotoTarget();
        }

        // If the visibility icon wasn't clicked, perform the selection
        Utils.EntitySelectionHandler(Editor, Editor.ViewportSelection, e, doSelect, arrowKeySelect);

        // If there's children then draw them
        if (nodeopen)
        {
            HierarchyView(map, e);
            ImGui.TreePop();
        }

        ImGui.PopID();
    }

    /// <summary>
    /// Handles the setup for the heiarchical content selectables
    /// </summary>
    private void HierarchyView(MapContainer map, Entity entity)
    {
        foreach (Entity obj in entity.Children)
        {
            if (obj is Entity e)
            {
                MapObjectSelectable(map, e, true, -1, true);
            }
        }
    }

    private void FlatView(MapContainer map)
    {
        foreach (Entity obj in map.Objects)
        {
            if (obj is MsbEntity e)
            {
                if (Editor.MapContentFilter.ContentFilter(map, obj))
                {
                    MapObjectSelectable(map, e, true);
                }
            }
        }
    }
}
