using ImGuiNET;
using StudioCore.Configuration;
using StudioCore.Gui;
using StudioCore.Platform;
using StudioCore.Scene;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Veldrid;
using StudioCore.Editors.ParamEditor;
using StudioCore.MsbEditor;
using StudioCore.Interface;
using StudioCore.Editor;
using StudioCore.Locators;
using StudioCore.Editors.MapEditor.WorldMap;
using StudioCore.Core;

namespace StudioCore.Editors.MapEditor;

public struct DragDropPayload
{
    public Entity Entity;
}

public struct DragDropPayloadReference
{
    public int Index;
}

public interface SceneTreeEventHandler
{
    public void OnEntityContextMenu(Entity ent);
}

public class MapSceneTree : IActionEventHandler
{
    public enum Configuration
    {
        MapEditor,
        ModelEditor
    }

    public enum ViewMode
    {
        Hierarchy,
        Flat,
        ObjectType
    }

    private readonly Configuration _configuration;

    private readonly ViewportActionManager _editorActionManager;

    private readonly SceneTreeEventHandler _handler;

    private readonly string _id;
    private readonly ViewportSelection _selection;

    // Keep track of open tree nodes for selection management purposes
    private readonly HashSet<Entity> _treeOpenEntities = new();
    private readonly Universe _universe;

    private readonly string[] _viewModeStrings = { "Hierarchy View", "Flat View", "Type View" };

    private readonly IViewport _viewport;

    private Dictionary<string, Dictionary<MsbEntity.MsbEntityType, Dictionary<Type, List<MsbEntity>>>>
        _cachedTypeView;

    private bool _chaliceLoadError;

    private string _chaliceMapID = "m29_";

    private ulong
        _mapEnt_ImGuiID; // Needed to avoid issue with identical IDs during keyboard navigation. May be unecessary when ImGUI is updated.

    private string _mapObjectListSearchInput = "";

    private ISelectable _pendingClick;

    private bool _setNextFocus;

    private ViewMode _viewMode = ViewMode.ObjectType;

    private WorldMapScreen _worldMapScreen;

    private MapEditorScreen Screen;

    public MapSceneTree(MapEditorScreen screen, Configuration configuration, SceneTreeEventHandler handler, string id, Universe universe, ViewportSelection sel, ViewportActionManager aman, IViewport vp)
    {
        Screen = screen;

        _handler = handler;
        _id = id;
        _universe = universe;
        _selection = sel;
        _editorActionManager = aman;
        _viewport = vp;
        _configuration = configuration;

        if (_configuration == Configuration.ModelEditor)
        {
            _viewMode = ViewMode.Hierarchy;
        }

        _worldMapScreen = new WorldMapScreen();
    }

    public void OnProjectChanged()
    {
        _worldMapScreen.OnProjectChanged();

        if (Smithbox.ProjectType != ProjectType.Undefined)
        {
            
        }
    }

    public void SetWorldMapSelection()
    {
        _mapObjectListSearchInput = "World Map Selection";
    }

    public void OnGui()
    {
        var scale = Smithbox.GetUIScale();

        _worldMapScreen.Shortcuts();

        if (!CFG.Current.Interface_MapEditor_MapObjectList)
            return;

        ImGui.PushStyleColor(ImGuiCol.ChildBg, CFG.Current.ImGui_ChildBg);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0.0f, 0.0f));

        if (ImGui.Begin($@"Map Object List##{_id}"))
        {
            ImGui.PopStyleVar();

            if (Smithbox.ProjectType is ProjectType.DS2S || Smithbox.ProjectType is ProjectType.DS2)
            {
                if (ParamBank.PrimaryBank.IsLoadingParams)
                {
                    ImGui.NewLine();
                    ImGui.Text("  Please wait for params to finish loading.");
                    ImGui.End();
                    ImGui.PopStyleColor();
                    return;
                }
            }

            ImGui.Spacing();
            ImGui.Indent(5 * scale);

            // World Map
            _worldMapScreen.DisplayWorldMapButton();
            _worldMapScreen.DisplayWorldMap();

            // List Sorting Style
            if (CFG.Current.MapEditor_MapObjectList_ShowListSortingType)
            {
                ImGui.AlignTextToFramePadding();
                ImGui.Text("List Sorting Style:");
                ImGui.SameLine();
                ImGui.SetNextItemWidth(-1);

                var mode = (int)_viewMode;
                if (ImGui.Combo("##typecombo", ref mode, _viewModeStrings, _viewModeStrings.Length))
                {
                    _viewMode = (ViewMode)mode;
                }
            }

            // Map ID Search
            if (CFG.Current.MapEditor_MapObjectList_ShowMapIdSearch)
            {
                var widthUnit = ImGui.GetWindowWidth() / 100;

                ImGui.SetNextItemWidth(widthUnit * 80);
                ImGui.InputText("##treeSearch", ref _mapObjectListSearchInput, 99);
                ImguiUtils.ShowHoverTooltip("Filters the map list by name.\nFuzzy search, so name only needs to contain the string within part of it to appear.");
                ImGui.SameLine();
                if (ImGui.Button($"Clear##ClearMapFilter", new Vector2(widthUnit * 16, 20 * Smithbox.GetUIScale())))
                {
                    _mapObjectListSearchInput = "";
                    _worldMapScreen.MapSelectionActive = false;
                    Smithbox.EditorHandler.MapEditor.WorldMap_ClickedMapZone = null;
                }
                ImguiUtils.ShowHoverTooltip("Clear the map search filter.");
            }

            ImGui.Unindent(5 * scale);

            DisplayMapObjectList();
        }
        else
        {
            ImGui.PopStyleVar();
        }

        ImGui.End();
        ImGui.PopStyleColor();
        _selection.ClearGotoTarget();
    }

    private void DisplayMapObjectList()
    {
        var scale = Smithbox.GetUIScale();

        ImGui.BeginChild("listtree");

        if (_universe.LoadedObjectContainers.Count == 0)
        {
            if (_universe.GameType == ProjectType.Undefined)
            {
                ImGui.Text("No project loaded. File -> New Project");
            }
            else
            {
                ImGui.Text("This Editor requires unpacked game files. Use UXM");
            }
        }


        IEnumerable<string> selectedWorldMaps = null;
        if (Smithbox.ProjectType == ProjectType.ER)
        {
            _worldMapScreen.MapSelectionActive = _mapObjectListSearchInput == "World Map Selection";
            if (_worldMapScreen.MapSelectionActive)
            {
                selectedWorldMaps = Smithbox.EditorHandler.MapEditor.WorldMap_ClickedMapZone;
            }
        }

        var orderedMaps = _universe.LoadedObjectContainers
            .Where(k => k.Key is not null)
            .OrderBy(k => k.Key);

        var loadedMaps = orderedMaps.Where(k => k.Value is not null);
        var unloadedMaps = orderedMaps.Where(k => k.Value is null);
        if (selectedWorldMaps is not null && selectedWorldMaps.Any())
        {
            unloadedMaps = unloadedMaps.Where(m => selectedWorldMaps.Contains(m.Key));
            if (!CFG.Current.MapEditor_Always_List_Loaded_Maps)
                loadedMaps = loadedMaps.Where(m => selectedWorldMaps.Contains(m.Key));
        }
        else
        {
            var matchesMapSearch = (KeyValuePair<string, ObjectContainer> k) =>
                SearchFilters.IsMapSearchMatch(
                    _mapObjectListSearchInput, k.Key,
                    AliasUtils.GetMapNameAlias(k.Key),
                    AliasUtils.GetMapTags(k.Key)
                );
            unloadedMaps = unloadedMaps.Where(matchesMapSearch);
            if (!CFG.Current.MapEditor_Always_List_Loaded_Maps)
                loadedMaps = loadedMaps.Where(matchesMapSearch);
        }

        _mapEnt_ImGuiID = 0;
        var unloadedSeparate = !loadedMaps.Any();
        foreach (KeyValuePair<string, ObjectContainer> lm in loadedMaps.Concat(unloadedMaps))
        {
            var (CurrentMapID, CurrentObjectContainer) = lm;

            if (CurrentObjectContainer is null && !unloadedSeparate)
            {
                ImGui.Separator();
                unloadedSeparate = true;
            }

            Entity mapRoot = CurrentObjectContainer?.RootObject;
            ObjectContainerReference mapRef = new(CurrentMapID, _universe);
            ISelectable selectTarget = (ISelectable)mapRoot ?? mapRef;

            ImGuiTreeNodeFlags treeflags = ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.SpanAvailWidth;

            var selected = _selection.GetSelection().Contains(mapRoot) || _selection.GetSelection().Contains(mapRef);
            if (selected)
            {
                treeflags |= ImGuiTreeNodeFlags.Selected;
            }

            var nodeopen = false;
            var unsaved = CurrentObjectContainer != null && CurrentObjectContainer.HasUnsavedChanges ? "*" : "";

            ImGui.BeginGroup();

            string treeNodeName = $@"{ForkAwesome.Cube} {CurrentMapID}";
            string treeNodeNameFormat = $@"{ForkAwesome.Cube} {CurrentMapID}{unsaved}";

            // Loaded Map
            if (CurrentObjectContainer != null)
            {
                nodeopen = ImGui.TreeNodeEx(treeNodeName, treeflags, treeNodeNameFormat);
            }
            // Unloaded Map
            else
            {
                if (ImGui.Selectable($@"   {treeNodeName}", selected, ImGuiSelectableFlags.AllowDoubleClick))
                {
                    if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                    {
                        if (CFG.Current.MapEditor_Enable_Map_Load_on_Double_Click)
                        {
                            if (selected)
                            {
                                _selection.ClearSelection();
                            }

                            _universe.LoadMap(CurrentMapID, selected);
                        }
                    }
                }
            }

            if (CFG.Current.MapEditor_MapObjectList_ShowMapNames)
            {
                AliasUtils.DisplayAlias(AliasUtils.GetMapNameAlias(CurrentMapID));
            }

            ImGui.EndGroup();

            if (_selection.ShouldGoto(mapRoot) || _selection.ShouldGoto(mapRef))
            {
                ImGui.SetScrollHereY();
                _selection.ClearGotoTarget();
            }

            if (nodeopen)
            {
                ImGui.Indent(); //TreeNodeEx fails to indent as it is inside a group / indentation is reset
            }

            // Right click context menu
            if (ImGui.BeginPopupContextItem($@"mapcontext_{CurrentMapID}"))
            {
                if (CurrentObjectContainer == null)
                {
                    if (ImGui.Selectable("Load Map"))
                    {
                        if (selected)
                        {
                            _selection.ClearSelection();
                        }

                        _universe.LoadMap(CurrentMapID, selected);
                    }
                }
                else if (CurrentObjectContainer is MapContainer m)
                {
                    if (ImGui.Selectable("Save Map"))
                    {
                        try
                        {
                            _universe.SaveMap(m);
                        }
                        catch (SavingFailedException e)
                        {
                            ((MapEditorScreen)_handler).HandleSaveException(e);
                        }
                    }

                    if (ImGui.Selectable("Unload Map"))
                    {
                        _selection.ClearSelection();
                        _editorActionManager.Clear();
                        _universe.UnloadContainer(m);
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        GC.Collect();
                    }
                }

                if (_universe.GameType is ProjectType.ER)
                {
                    if (CurrentMapID.StartsWith("m60") || CurrentMapID.StartsWith("m61"))
                    {
                        if (ImGui.Selectable("Load Related Maps"))
                        {
                            if (selected)
                            {
                                _selection.ClearSelection();
                            }

                            _universe.LoadMap(CurrentMapID);
                            _universe.LoadRelatedMapsER(CurrentMapID, _universe.LoadedObjectContainers);
                        }
                    }
                }

                if (_universe.GetLoadedMapCount() > 1)
                {
                    if (ImGui.Selectable("Unload All Maps"))
                    {
                        DialogResult result = PlatformUtils.Instance.MessageBox("Unload all maps?", "Confirm",
                            MessageBoxButtons.YesNo);
                        if (result == DialogResult.Yes)
                        {
                            _selection.ClearSelection();
                            _editorActionManager.Clear();
                            _universe.UnloadAllMaps();
                            GC.Collect();
                            GC.WaitForPendingFinalizers();
                            GC.Collect();
                        }
                    }
                }

                if (Screen.MapQueryHandler.IsOpen)
                {
                    if (ImGui.Selectable("Add to Map Filter"))
                    {
                        Screen.MapQueryHandler.AddMapFilterInput(CurrentMapID);
                    }
                }
                if (Screen.MapQueryEditHandler.IsOpen)
                {
                    if (ImGui.Selectable("Add to Map Filter"))
                    {
                        Screen.MapQueryEditHandler.AddMapFilterInput(CurrentMapID);
                    }
                }

                ImGui.EndPopup();
            }

            if (ImGui.IsItemClicked())
            {
                _pendingClick = selectTarget;
            }

            if (ImGui.IsMouseDoubleClicked(0) && _pendingClick != null && mapRoot == _pendingClick)
            {
                _viewport.FramePosition(mapRoot.GetLocalTransform().Position, 10f);
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
                            if (_selection.GetSelection().Contains(selectTarget))
                            {
                                _selection.RemoveSelection(selectTarget);
                            }
                            else
                            {
                                _selection.AddSelection(selectTarget);
                            }
                        }
                        else
                        {
                            _selection.ClearSelection();
                            _selection.AddSelection(selectTarget);
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

            if (nodeopen)
            {
                ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(8.0f, 3.0f) * scale);

                if (_viewMode == ViewMode.Hierarchy)
                {
                    HierarchyView(CurrentObjectContainer.RootObject);
                }
                else if (_viewMode == ViewMode.Flat)
                {
                    FlatView((MapContainer)CurrentObjectContainer);
                }
                else if (_viewMode == ViewMode.ObjectType)
                {
                    TypeView((MapContainer)CurrentObjectContainer);
                }

                ImGui.PopStyleVar();
                ImGui.TreePop();
            }

            // Update type cache when a map is no longer loaded
            if (_cachedTypeView != null && CurrentObjectContainer == null && _cachedTypeView.ContainsKey(CurrentMapID))
            {
                _cachedTypeView.Remove(CurrentMapID);
            }
        }

        if (Smithbox.ProjectType == ProjectType.BB && _configuration == Configuration.MapEditor)
        {
            ChaliceDungeonImportButton();
        }

        ImGui.EndChild();
    }

    private unsafe void MapObjectSelectable(Entity e, bool visicon, bool hierarchial = false)
    {
        var scale = Smithbox.GetUIScale();

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
            if (_selection.GetSelection().Contains(e))
            {
                treeflags |= ImGuiTreeNodeFlags.Selected;
            }

            nodeopen = ImGui.TreeNodeEx(e.PrettyName, treeflags);
            if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(0))
            {
                if (e.RenderSceneMesh != null)
                {
                    _viewport.FrameBox(e.RenderSceneMesh.GetBounds());
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
            _mapEnt_ImGuiID++;
            var selectableFlags = ImGuiSelectableFlags.AllowDoubleClick | ImGuiSelectableFlags.AllowItemOverlap;

            if (ImGui.Selectable($"{padding}{e.PrettyName}##{_mapEnt_ImGuiID}", _selection.GetSelection().Contains(e), selectableFlags))
            {
                doSelect = true;

                // If double clicked frame the selection in the viewport
                if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                {
                    if (e.RenderSceneMesh != null)
                    {
                        _viewport.FrameBox(e.RenderSceneMesh.GetBounds());
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
                AliasUtils.DisplayAlias(alias);
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

        if (_selection.ShouldGoto(e))
        {
            // By default, this places the item at 50% in the frame. Use 0 to place it on top.
            ImGui.SetScrollHereY();
            _selection.ClearGotoTarget();
        }

        if (ImGui.BeginPopupContextItem($"EntityContextMenu{_mapEnt_ImGuiID}"))
        {
            _handler.OnEntityContextMenu(e);
            ImGui.EndPopup();
        }

        // Visibility icon
        if (visicon)
        {
            ImGui.SetItemAllowOverlap();
            var visible = e.EditorVisible;
            ImGui.SameLine();
            ImGui.SetCursorPosX(ImGui.GetWindowContentRegionMax().X - 18.0f * Smithbox.GetUIScale());
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
        Utils.EntitySelectionHandler(_selection, e, doSelect, arrowKeySelect);

        // If there's children then draw them
        if (nodeopen)
        {
            HierarchyView(e);
            ImGui.TreePop();
        }

        ImGui.PopID();
    }

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
                MapObjectSelectable(e, true);
            }
        }
    }

    private void TypeView(MapContainer map)
    {
        if (_cachedTypeView == null || !_cachedTypeView.ContainsKey(map.Name))
        {
            RebuildTypeViewCache(map);
        }

        foreach (KeyValuePair<MsbEntity.MsbEntityType, Dictionary<Type, List<MsbEntity>>> cats in
                 _cachedTypeView[map.Name].OrderBy(q => q.Key.ToString()))
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
                            if (cats.Key == MsbEntity.MsbEntityType.Region &&
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
                            else if (cats.Key == MsbEntity.MsbEntityType.Light)
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
                                                            18.0f * Smithbox.GetUIScale());
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
                                                            18.0f * Smithbox.GetUIScale());
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

    public void OnActionEvent(ActionEvent evt)
    {
        if (evt.HasFlag(ActionEvent.ObjectAddedRemoved))
        {
            _cachedTypeView = null;
        }
    }

    private void RebuildTypeViewCache(MapContainer map)
    {
        if (_cachedTypeView == null)
        {
            _cachedTypeView =
                new Dictionary<string, Dictionary<MsbEntity.MsbEntityType, Dictionary<Type, List<MsbEntity>>>>();
        }

        Dictionary<MsbEntity.MsbEntityType, Dictionary<Type, List<MsbEntity>>> mapcache = new();
        mapcache.Add(MsbEntity.MsbEntityType.Part, new Dictionary<Type, List<MsbEntity>>());
        mapcache.Add(MsbEntity.MsbEntityType.Region, new Dictionary<Type, List<MsbEntity>>());
        mapcache.Add(MsbEntity.MsbEntityType.Event, new Dictionary<Type, List<MsbEntity>>());
        if (Smithbox.ProjectType is ProjectType.BB or ProjectType.DS3 or ProjectType.SDT
            or ProjectType.ER or ProjectType.AC6)
        {
            mapcache.Add(MsbEntity.MsbEntityType.Light, new Dictionary<Type, List<MsbEntity>>());
        }
        else if (Smithbox.ProjectType is ProjectType.DS2S || Smithbox.ProjectType is ProjectType.DS2)
        {
            mapcache.Add(MsbEntity.MsbEntityType.Light, new Dictionary<Type, List<MsbEntity>>());
            mapcache.Add(MsbEntity.MsbEntityType.DS2Event, new Dictionary<Type, List<MsbEntity>>());
            mapcache.Add(MsbEntity.MsbEntityType.DS2EventLocation, new Dictionary<Type, List<MsbEntity>>());
            mapcache.Add(MsbEntity.MsbEntityType.DS2Generator, new Dictionary<Type, List<MsbEntity>>());
            mapcache.Add(MsbEntity.MsbEntityType.DS2GeneratorRegist, new Dictionary<Type, List<MsbEntity>>());
        }

        foreach (Entity obj in map.Objects)
        {
            if (obj is MsbEntity e && mapcache.ContainsKey(e.Type))
            {
                Type typ = e.WrappedObject.GetType();
                if (!mapcache[e.Type].ContainsKey(typ))
                {
                    mapcache[e.Type].Add(typ, new List<MsbEntity>());
                }

                mapcache[e.Type][typ].Add(e);
            }
        }

        if (!_cachedTypeView.ContainsKey(map.Name))
        {
            _cachedTypeView.Add(map.Name, mapcache);
        }
        else
        {
            _cachedTypeView[map.Name] = mapcache;
        }
    }

    private bool DisplayChaliceDungeonSection = true;

    private void ChaliceDungeonImportButton()
    {
        ImGui.Separator();

        if(ImGui.Selectable("Toggle Chalice Dungeon Panel"))
        {
            DisplayChaliceDungeonSection = !DisplayChaliceDungeonSection;
        }

        ImGui.Separator();

        if (DisplayChaliceDungeonSection)
        {
            var width = ImGui.GetWindowWidth() * 0.95f;

            ImGui.Indent(5f);
            ImGui.AlignTextToFramePadding();
            ImGui.Text("Chalice ID (m29_xx_xx_xx): ");
            var pname = _chaliceMapID;

            if (_chaliceLoadError)
            {
                ImGui.PushStyleColor(ImGuiCol.FrameBg, CFG.Current.ImGui_ErrorInput_Background);
            }

            ImGui.SetNextItemWidth(width);
            if (ImGui.InputText("##chalicename", ref pname, 12))
            {
                _chaliceMapID = pname;
            }

            if (_chaliceLoadError)
            {
                ImGui.PopStyleColor();
            }

            if (ImGui.Button("Load", new Vector2(width, 24)))
            {
                if (!_universe.LoadMap(_chaliceMapID))
                {
                    _chaliceLoadError = true;
                }
                else
                {
                    ImGui.CloseCurrentPopup();
                    _chaliceLoadError = false;
                    _chaliceMapID = "m29_";
                }
            }
            ImGui.Unindent(5f);
        }
    }
}
