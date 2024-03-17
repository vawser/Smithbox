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
using Silk.NET.OpenGL;
using StudioCore.BanksMain;
using StudioCore.Editor;

namespace StudioCore.Editors.ModelEditor;

public struct DragDropPayload
{
    public Entity Entity;
}

public struct DragDropPayloadReference
{
    public int Index;
}

public class ModelSceneTree : MapEditor.IActionEventHandler
{
    private readonly ViewportActionManager _editorActionManager;

    private readonly string _id;
    private readonly ViewportSelection _selection;

    // Keep track of open tree nodes for selection management purposes
    private readonly HashSet<Entity> _treeOpenEntities = new();
    private readonly Universe _universe;

    private readonly IViewport _viewport;

    private ulong _mapEnt_ImGuiID; // Needed to avoid issue with identical IDs during keyboard navigation. May be unecessary when ImGUI is updated.

    private ISelectable _pendingClick;

    private bool _setNextFocus;

    public static ModelContainer Model { get;  set; }

    private ModelEditorScreen _editor;

    public ModelSceneTree(ModelEditorScreen editor, string id, Universe universe, ViewportSelection sel, ViewportActionManager aman, IViewport vp)
    {
        _editor = editor;
        _id = id;
        _universe = universe;
        _selection = sel;
        _editorActionManager = aman;
        _viewport = vp;
    }

    public ViewportSelection GetCurrentSelection()
    {
        return _selection;
    }

    public void OnGui()
    {
        var scale = Smithbox.GetUIScale();

        if (!CFG.Current.Interface_ModelEditor_ModelHierarchy)
            return;

        ImGui.PushStyleColor(ImGuiCol.ChildBg, CFG.Current.ImGui_ChildBg);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0.0f, 2.0f) * scale);

        DisplaySceneTree();
    }

    public void DisplaySceneTree()
    {
        var scale = Smithbox.GetUIScale();

        // Scene Tree
        if (ImGui.Begin($@"Model Hierarchy##{_id}"))
        {
            ImGui.PopStyleVar();

            if (Smithbox.LowRequirementsMode)
            {
                ImGui.NewLine();
                ImGui.Text("  This editor is not available in low requirements mode.");
                ImGui.End();
                ImGui.PopStyleColor();
                return;
            }

            // Tree List
            ImGui.BeginChild("listtree");

            IOrderedEnumerable<KeyValuePair<string, ModelContainer>> loadedModels =
                _universe.LoadedModelContainers.OrderBy(k => k.Key);

            _mapEnt_ImGuiID = 0;

            foreach (KeyValuePair<string, ModelContainer> lm in loadedModels)
            {
                ModelContainer loadedModel = lm.Value;
                var assetName = lm.Key;

                if (assetName == null)
                {
                    continue;
                }

                Model = loadedModel;

                Entity rootEntity = loadedModel?.RootObject;
                ObjectContainerReference mapRef = new(assetName, _universe);
                ISelectable selectTarget = (ISelectable)rootEntity ?? mapRef;

                ImGuiTreeNodeFlags treeflags = ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.DefaultOpen;

                var selected = _selection.GetSelection().Contains(rootEntity) ||
                               _selection.GetSelection().Contains(mapRef);
                if (selected)
                {
                    treeflags |= ImGuiTreeNodeFlags.Selected;
                }

                var nodeopen = false;
                var unsaved = loadedModel != null && loadedModel.HasUnsavedChanges ? "*" : "";

                // Model ID and Name
                ImGui.BeginGroup();

                if (loadedModel != null)
                {
                    nodeopen = ImGui.TreeNodeEx($@"{ForkAwesome.Cube} {assetName}", treeflags,
                        $@"{ForkAwesome.Cube} {assetName}{unsaved}");
                }
                else
                {
                    ImGui.Selectable($@"   {ForkAwesome.Cube} {assetName}", selected);
                }

                var metaName = GetMetaName(assetName);
                if (metaName != "")
                {
                    ImGui.SameLine();
                    ImGui.PushTextWrapPos();
                    if (metaName.StartsWith("--")) // Marked as normally unused (use red text)
                    {
                        ImGui.TextColored(CFG.Current.ImGui_AliasName_Text, @$"<{metaName.Replace("--", "")}>");
                    }
                    else
                    {
                        ImGui.TextColored(CFG.Current.ImGui_AliasName_Text, @$"<{metaName}>");
                    }

                    ImGui.PopTextWrapPos();
                }

                ImGui.EndGroup();

                if (_selection.ShouldGoto(rootEntity) || _selection.ShouldGoto(mapRef))
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

                if (ImGui.IsMouseDoubleClicked(0) && _pendingClick != null && rootEntity == _pendingClick)
                {
                    _viewport.FramePosition(rootEntity.GetLocalTransform().Position, 10f);
                }

                if ((_pendingClick == rootEntity || mapRef.Equals(_pendingClick)) &&
                    ImGui.IsMouseReleased(ImGuiMouseButton.Left))
                {
                    if (ImGui.IsItemHovered())
                    {
                        // Only select if a node is not currently being opened/closed
                        if (rootEntity == null || nodeopen && _treeOpenEntities.Contains(rootEntity) || !nodeopen && !_treeOpenEntities.Contains(rootEntity))
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
                        if (rootEntity != null)
                        {
                            if (nodeopen && !_treeOpenEntities.Contains(rootEntity))
                            {
                                _treeOpenEntities.Add(rootEntity);
                            }
                            else if (!nodeopen && _treeOpenEntities.Contains(rootEntity))
                            {
                                _treeOpenEntities.Remove(rootEntity);
                            }
                        }
                    }

                    _pendingClick = null;
                }

                if (nodeopen)
                {
                    ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(8.0f, 3.0f) * scale);

                    HierarchyView(loadedModel.RootObject);

                    ImGui.PopStyleVar();
                    ImGui.TreePop();
                }
            }

            ImGui.EndChild();
        }
        else
        {
            ImGui.PopStyleVar();
        }

        ImGui.End();
        ImGui.PopStyleColor();
        _selection.ClearGotoTarget();
    }

    private void HierarchyView(Entity ent)
    {
        foreach (Entity obj in ent.Children)
        {
            if (obj is Entity e)
            {
                ModelSelectable(e, true);
            }
        }
    }

    public bool TreeSelectable(Entity e)
    {
        var nodeopen = false;

        ImGuiTreeNodeFlags treeflags = ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.DefaultOpen;

        // Don't auto-open these objects
        if (e.WrappedObject is ModelRootNode rootNode && rootNode.Name == "Bones")
        {
            treeflags = ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.SpanAvailWidth;
        }

        // Show selected tree row
        if (_selection.GetSelection().Contains(e))
        {
            treeflags |= ImGuiTreeNodeFlags.Selected;
        }

        // Tree top
        nodeopen = ImGui.TreeNodeEx(e.PrettyName, treeflags);

        if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(0))
        {
            // Frame model if double-clicked
            if (e.RenderSceneMesh != null)
            {
                _viewport.FrameBox(e.RenderSceneMesh.GetBounds());
            }
        }

        return nodeopen;
    }
    
    public void RowSelectable(Entity e)
    {
        string name = e.PrettyName;
        string aliasedName = name;

        if (Model != null)
        {
            if (CFG.Current.ModelEditor_DisplayDmyPolyReferenceID)
            {
                if (e.WrappedObject is FLVER.Dummy)
                {
                    var refId = e.GetPropertyValue<short>("ReferenceID");

                    if (refId != -1)
                    {
                        aliasedName = $"{aliasedName} {{ ID {refId} }}";
                    }
                }
            }

            if (CFG.Current.ModelEditor_DisplayMatNameOnMesh)
            {
                if (e.WrappedObject is FLVER2.Mesh)
                {
                    var mesh = e.WrappedObject as FLVER2.Mesh;

                    if (mesh.MaterialIndex != -1)
                    {
                        if (Model.MaterialDictionary.ContainsKey(mesh.MaterialIndex))
                        {
                            aliasedName = $"{aliasedName} {{ {Model.MaterialDictionary[mesh.MaterialIndex]} }}";
                        }
                    }
                }
            }
        }

        if (ImGui.Selectable("   " + aliasedName + "##" + _mapEnt_ImGuiID, _selection.GetSelection().Contains(e), ImGuiSelectableFlags.AllowDoubleClick | ImGuiSelectableFlags.AllowItemOverlap))
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

    private unsafe void ModelSelectable(Entity e, bool visicon)
    {
        if (e.Name == null)
            return;

        ImGui.PushID(e.Name);

        var doSelect = false;

        if (_setNextFocus)
        {
            ImGui.SetItemDefaultFocus();
            _setNextFocus = false;
            doSelect = true;
        }

        var nodeopen = false;

        // Tree List class top
        if (e.Children.Count > 0)
        {
            nodeopen = TreeSelectable(e);
        }
        // Entries
        else
        {
            _mapEnt_ImGuiID++;

            RowSelectable(e);
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
        if (ImGui.IsItemFocused() && (InputTracker.GetKey(Key.Up) || InputTracker.GetKey(Key.Down)))
        {
            doSelect = true;
            arrowKeySelect = true;
        }

        if (doSelect)
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

    public string GetMetaName(string assetName)
    {
        var metaName = "";

        if (!ModelAliasBank.Bank.IsLoadingAliases && ModelAliasBank.Bank.AliasNames != null)
        {
            var list = ModelAliasBank.Bank.AliasNames.GetEntries("Characters");
            foreach (var entry in list)
            {
                if (entry.id == assetName)
                {
                    metaName = entry.name;
                }
            }
        }
        if (!ModelAliasBank.Bank.IsLoadingAliases && ModelAliasBank.Bank.AliasNames != null)
        {
            var list = ModelAliasBank.Bank.AliasNames.GetEntries("Objects");
            foreach (var entry in list)
            {
                if (entry.id == assetName)
                {
                    metaName = entry.name;
                }
            }
        }
        if (!ModelAliasBank.Bank.IsLoadingAliases && ModelAliasBank.Bank.AliasNames != null)
        {
            var list = ModelAliasBank.Bank.AliasNames.GetEntries("Parts");
            foreach (var entry in list)
            {
                if (entry.id == assetName)
                {
                    metaName = entry.name;
                }
            }
        }
        if (!ModelAliasBank.Bank.IsLoadingAliases && ModelAliasBank.Bank.AliasNames != null)
        {
            var list = ModelAliasBank.Bank.AliasNames.GetEntries("MapPieces");
            foreach (var entry in list)
            {
                if (entry.id == assetName)
                {
                    metaName = entry.name;
                }
            }
        }

        return metaName;
    }
    public void OnActionEvent(MapEditor.ActionEvent evt)
    {

    }
}
