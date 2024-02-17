using ImGuiNET;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Banks.AliasBank;
using StudioCore.Gui;
using StudioCore.AssetLocator;
using StudioCore.Platform;
using StudioCore.UserProject;
using StudioCore.Scene;
using StudioCore.Settings;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;
using StudioCore.Banks;
using StudioCore.Editors.ParamEditor;
using StudioCore.MsbEditor;
using StudioCore.Editors.MapEditor;
using Microsoft.AspNetCore.Components.Forms;

namespace StudioCore.Editors.ModelEditor;

public struct DragDropPayload
{
    public Entity Entity;
}

public struct DragDropPayloadReference
{
    public int Index;
}

public class ModelSceneTree : IActionEventHandler
{
    private readonly List<Entity> _dragDropDestObjects = new();
    private readonly List<int> _dragDropDests = new();
    private readonly Dictionary<int, DragDropPayload> _dragDropPayloads = new();

    private readonly List<Entity> _dragDropSources = new();
    private readonly ViewportActionManager _editorActionManager;

    private readonly string _id;
    private readonly ViewportSelection _selection;

    // Keep track of open tree nodes for selection management purposes
    private readonly HashSet<Entity> _treeOpenEntities = new();
    private readonly Universe _universe;

    private readonly IViewport _viewport;

    private bool _chaliceLoadError;

    private string _chaliceMapID = "m29_";
    private int _dragDropPayloadCounter;

    private bool _initiatedDragDrop;

    private ulong
        _mapEnt_ImGuiID; // Needed to avoid issue with identical IDs during keyboard navigation. May be unecessary when ImGUI is updated.

    private string _mapNameSearchStr = "";

    private ISelectable _pendingClick;
    private bool _pendingDragDrop;

    private bool _setNextFocus;

    private Dictionary<string, string> _chrAliasCache;
    private Dictionary<string, string> _objAliasCache;
    private Dictionary<string, string> _mapPieceAliasCache;

    private ModelEditorScreen _editor;


    public ModelSceneTree(ModelEditorScreen editor, string id, Universe universe, ViewportSelection sel, ViewportActionManager aman, IViewport vp)
    {
        _editor = editor;
        _id = id;
        _universe = universe;
        _selection = sel;
        _editorActionManager = aman;
        _viewport = vp;

        _chrAliasCache = null;
        _objAliasCache = null;
        _mapPieceAliasCache = null;
    }

    public void OnActionEvent(ActionEvent evt)
    {

    }
    private unsafe void ModelSelectable(Entity e, bool visicon, bool hierarchial = false)
    {
        var scale = Smithbox.GetUIScale();

        if (e.Name == null)
            return;

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
        }
        else
        {
            _mapEnt_ImGuiID++;

            string name = e.PrettyName;
            string aliasedName = name;
            var modelName = e.GetPropertyValue<string>("ModelName");

            if (modelName == null)
                modelName = "";

            if (CFG.Current.MapEditor_Show_Character_Names_in_Scene_Tree)
            {
                if (e.IsPartEnemy())
                {
                    if (_chrAliasCache != null && _chrAliasCache.ContainsKey(modelName))
                    {
                        aliasedName = $"{name} {_chrAliasCache[modelName]}";
                    }
                    else
                    {
                        foreach (var entry in ModelAliasBank.Bank.AliasNames.GetEntries("Characters"))
                        {
                            if (modelName == entry.id)
                            {
                                aliasedName = $" {{ {entry.name} }}";

                                if (_chrAliasCache == null)
                                {
                                    _chrAliasCache = new Dictionary<string, string>();
                                }

                                if (!_chrAliasCache.ContainsKey(entry.id))
                                {
                                    _chrAliasCache.Add(modelName, aliasedName);
                                }
                            }
                        }
                    }
                }
            }

            if (ImGui.Selectable(padding + aliasedName + "##" + _mapEnt_ImGuiID,
                    _selection.GetSelection().Contains(e),
                    ImGuiSelectableFlags.AllowDoubleClick | ImGuiSelectableFlags.AllowItemOverlap))
            {
                // If double clicked frame the selection in the viewport
                if (ImGui.IsMouseDoubleClicked(0))
                {
                    if (e.RenderSceneMesh != null)
                    {
                        _viewport.FrameBox(e.RenderSceneMesh.GetBounds());
                    }
                }
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

        // Up/Down arrow mass selection
        var arrowKeySelect = false;
        if (ImGui.IsItemFocused()
            && (InputTracker.GetKey(Key.Up) || InputTracker.GetKey(Key.Down)))
        {
            doSelect = true;
            arrowKeySelect = true;
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

        if (ImGui.BeginPopupContextItem())
        {
            _editor.OnEntityContextMenu(e);
            ImGui.EndPopup();
        }

        if (ImGui.BeginDragDropSource())
        {
            ImGui.Text(e.PrettyName);
            // Kinda meme
            DragDropPayload p = new();
            p.Entity = e;
            _dragDropPayloads.Add(_dragDropPayloadCounter, p);
            DragDropPayloadReference r = new();
            r.Index = _dragDropPayloadCounter;
            _dragDropPayloadCounter++;
            GCHandle handle = GCHandle.Alloc(r, GCHandleType.Pinned);
            ImGui.SetDragDropPayload("entity", handle.AddrOfPinnedObject(), (uint)sizeof(DragDropPayloadReference));
            ImGui.EndDragDropSource();
            handle.Free();
            _initiatedDragDrop = true;
        }

        if (hierarchial && ImGui.BeginDragDropTarget())
        {
            ImGuiPayloadPtr payload = ImGui.AcceptDragDropPayload("entity");
            if (payload.NativePtr != null)
            {
                var h = (DragDropPayloadReference*)payload.Data;
                DragDropPayload pload = _dragDropPayloads[h->Index];
                _dragDropPayloads.Remove(h->Index);
                _dragDropSources.Add(pload.Entity);
                _dragDropDestObjects.Add(e);
                _dragDropDests.Add(e.Children.Count);
            }

            ImGui.EndDragDropTarget();
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

        // Invisible item to be a drag drop target between nodes
        if (_pendingDragDrop)
        {
            if (e is MsbEntity me2)
            {
                ImGui.SetItemAllowOverlap();
                ImGui.InvisibleButton(me2.Type + e.Name, new Vector2(-1, 3.0f) * scale);
            }
            else
            {
                ImGui.SetItemAllowOverlap();
                ImGui.InvisibleButton(e.Name, new Vector2(-1, 3.0f) * scale);
            }

            if (ImGui.IsItemFocused())
            {
                _setNextFocus = true;
            }

            if (ImGui.BeginDragDropTarget())
            {
                ImGuiPayloadPtr payload = ImGui.AcceptDragDropPayload("entity");
                if (payload.NativePtr != null) //todo: never passes
                {
                    var h = (DragDropPayloadReference*)payload.Data;
                    DragDropPayload pload = _dragDropPayloads[h->Index];
                    _dragDropPayloads.Remove(h->Index);
                    if (hierarchial)
                    {
                        _dragDropSources.Add(pload.Entity);
                        _dragDropDestObjects.Add(e.Parent);
                        _dragDropDests.Add(e.Parent.ChildIndex(e) + 1);
                    }
                    else
                    {
                        _dragDropSources.Add(pload.Entity);
                        _dragDropDests.Add(pload.Entity.Container.Objects.IndexOf(e) + 1);
                    }
                }

                ImGui.EndDragDropTarget();
            }
        }

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
                ModelSelectable(e, true, true);
            }
        }
    }

    public void OnGui()
    {
        var scale = Smithbox.GetUIScale();

        ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(0.145f, 0.145f, 0.149f, 1.0f));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0.0f, 2.0f) * scale);

        var titleString = $@"Model Hierarchy##{_id}";

        if (ImGui.Begin(titleString))
        {
            if (_initiatedDragDrop)
            {
                _initiatedDragDrop = false;
                _pendingDragDrop = true;
            }

            if (_pendingDragDrop && ImGui.IsMouseReleased(ImGuiMouseButton.Left))
            {
                _pendingDragDrop = false;
            }

            ImGui.PopStyleVar();

            if (Smithbox.LowRequirementsMode)
            {
                ImGui.NewLine();
                ImGui.Text("  This editor is not available in low requirements mode.");
                ImGui.End();
                ImGui.PopStyleColor();
                return;
            }

            ImGui.BeginChild("listtree");
            
            IOrderedEnumerable<KeyValuePair<string, MapObjectContainer>> orderedMaps =
                _universe.LoadedObjectContainers.OrderBy(k => k.Key);

            _mapEnt_ImGuiID = 0;
            foreach (KeyValuePair<string, MapObjectContainer> lm in orderedMaps)
            {
                var metaName = "";
                MapObjectContainer map = lm.Value;
                var mapid = lm.Key;
                if (mapid == null)
                {
                    continue;
                }

                if (MapAliasBank.Bank.MapNames != null)
                {
                    if (MapAliasBank.Bank.MapNames.ContainsKey(mapid))
                    {
                        metaName = MapAliasBank.Bank.MapNames[mapid];
                    }
                }

                // Map name search filter
                if (_mapNameSearchStr != ""
                    && (!CFG.Current.MapEditor_Always_List_Loaded_Maps || map == null)
                    && !lm.Key.Contains(_mapNameSearchStr, StringComparison.CurrentCultureIgnoreCase)
                    && !metaName.Contains(_mapNameSearchStr, StringComparison.CurrentCultureIgnoreCase))
                {
                    continue;
                }

                Entity mapRoot = map?.RootObject;
                MapObjectContainerReference mapRef = new(mapid, _universe);
                ISelectable selectTarget = (ISelectable)mapRoot ?? mapRef;

                ImGuiTreeNodeFlags treeflags = ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.SpanAvailWidth;
                var selected = _selection.GetSelection().Contains(mapRoot) ||
                               _selection.GetSelection().Contains(mapRef);
                if (selected)
                {
                    treeflags |= ImGuiTreeNodeFlags.Selected;
                }

                var nodeopen = false;
                var unsaved = map != null && map.HasUnsavedChanges ? "*" : "";
                ImGui.BeginGroup();
                if (map != null)
                {
                    nodeopen = ImGui.TreeNodeEx($@"{ForkAwesome.Cube} {mapid}", treeflags,
                        $@"{ForkAwesome.Cube} {mapid}{unsaved}");
                }
                else
                {
                    ImGui.Selectable($@"   {ForkAwesome.Cube} {mapid}", selected);
                }

                if (metaName != "")
                {
                    ImGui.SameLine();
                    ImGui.PushTextWrapPos();
                    if (metaName.StartsWith("--")) // Marked as normally unused (use red text)
                    {
                        ImGui.TextColored(new Vector4(1.0f, 0.0f, 0.0f, 1.0f), @$"<{metaName.Replace("--", "")}>");
                    }
                    else
                    {
                        ImGui.TextColored(new Vector4(1.0f, 1.0f, 0.0f, 1.0f), @$"<{metaName}>");
                    }

                    ImGui.PopTextWrapPos();
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

                if (ImGui.IsItemClicked())
                {
                    _pendingClick = selectTarget;
                }

                if (ImGui.IsMouseDoubleClicked(0) && _pendingClick != null && mapRoot == _pendingClick)
                {
                    _viewport.FramePosition(mapRoot.GetLocalTransform().Position, 10f);
                }

                if ((_pendingClick == mapRoot || mapRef.Equals(_pendingClick)) &&
                    ImGui.IsMouseReleased(ImGuiMouseButton.Left))
                {
                    if (ImGui.IsItemHovered())
                    {
                        // Only select if a node is not currently being opened/closed
                        if (mapRoot == null ||
                            nodeopen && _treeOpenEntities.Contains(mapRoot) ||
                            !nodeopen && !_treeOpenEntities.Contains(mapRoot))
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
                    if (_pendingDragDrop)
                    {
                        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(8.0f, 0.0f) * scale);
                    }
                    else
                    {
                        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(8.0f, 3.0f) * scale);
                    }

                    HierarchyView(map.RootObject);

                    ImGui.PopStyleVar();
                    ImGui.TreePop();
                }
            }

            ImGui.EndChild();

            if (_dragDropSources.Count > 0)
            {
                if (_dragDropDestObjects.Count > 0)
                {
                    ChangeEntityHierarchyAction action = new(_universe, _dragDropSources, _dragDropDestObjects,
                        _dragDropDests, false);
                    _editorActionManager.ExecuteAction(action);
                    _dragDropSources.Clear();
                    _dragDropDests.Clear();
                    _dragDropDestObjects.Clear();
                }
                else
                {
                    ReorderContainerObjectsAction action = new(_universe, _dragDropSources, _dragDropDests, false);
                    _editorActionManager.ExecuteAction(action);
                    _dragDropSources.Clear();
                    _dragDropDests.Clear();
                }
            }
        }
        else
        {
            ImGui.PopStyleVar();
        }

        ImGui.End();
        ImGui.PopStyleColor();
        _selection.ClearGotoTarget();
    }
}
