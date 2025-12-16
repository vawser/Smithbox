using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Veldrid;

namespace StudioCore.Editors.ModelEditor;

public class ModelContentView : IActionEventHandler
{
    public ModelEditorScreen Editor;
    public ProjectEntry Project;

    public string ImguiID = "ModelContentView";
    private int treeImGuiId = 0;

    private bool _setNextFocus;
    private ISelectable _pendingClick;
    private HashSet<Entity> _treeOpenEntities = new();

    public string SearchInput = "";

    public ModelContentView(ModelEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void OnGui()
    {
        var scale = DPI.UIScale();

        if (!CFG.Current.Interface_ModelEditor_Properties)
            return;

        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * scale, ImGuiCond.FirstUseEver);

        if (ImGui.Begin($@"Model Contents##modelContentsPanel", ImGuiWindowFlags.MenuBar))
        {
            Editor.FocusManager.SwitchModelEditorContext(ModelEditorContext.ModelProperties);

            DisplayMenubar();

            if (Editor.Selection.SelectedModelWrapper != null && 
                Editor.Selection.SelectedModelWrapper.FLVER != null)
            {
                DisplaySearchbar();
                DisplayButtons();

                treeImGuiId = 0;

                var container = Editor.Selection.SelectedModelWrapper.Container;

                if (container != null)
                {
                    DisplayContentTree(container);
                }
            }
            else
            {
                ImGui.Text("No FLVER has been loaded yet.");
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

            ImGui.EndMenuBar();
        }
    }

    public void DisplaySearchbar()
    {
        var wrapper = Editor.Selection.SelectedModelWrapper;

        var windowWidth = ImGui.GetWindowWidth();

        DPI.ApplyInputWidth(windowWidth * 0.6f);
        ImGui.InputText($"##contentFilterSearch_{wrapper.Name}", ref SearchInput, 255);
        UIHelper.Tooltip($"Filter the content tree.");
    }

    public bool CanDisplayModelObject(ModelContainer container, ModelEntity entity)
    {
        return true;
    }

    public void DisplayButtons()
    {
        var wrapper = Editor.Selection.SelectedModelWrapper;

        if (wrapper.Container == null)
            return;

        ImGui.SameLine();

        // Show All
        ImGui.SameLine();
        if (ImGui.Button($"{Icons.Eye}", DPI.IconButtonSize))
        {
            foreach (var entry in wrapper.Container.Objects)
            {
                entry.EditorVisible = true;
            }
        }
        UIHelper.Tooltip("Force all model objects to be shown.");

        // Hide All
        ImGui.SameLine();
        if (ImGui.Button($"{Icons.EyeSlash}", DPI.IconButtonSize))
        {
            foreach (var entry in wrapper.Container.Objects)
            {
                entry.EditorVisible = false;
            }
        }
        UIHelper.Tooltip("Force all model objects to be hidden.");
    }

    public void DisplayContentTree(ModelContainer container)
    {
        ImGui.BeginChild($"modelContentsTree_{ImguiID}");

        Entity modelRoot = container?.RootObject;
        ObjectContainerReference modelRef = new(container.Name);

        ISelectable selectTarget = (ISelectable)modelRoot ?? modelRef;

        ImGuiTreeNodeFlags treeflags = ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.SpanAvailWidth;

        var selected = Editor.ViewportSelection.GetSelection().Contains(modelRoot) || 
            Editor.ViewportSelection.GetSelection().Contains(modelRef);

        if (selected)
        {
            treeflags |= ImGuiTreeNodeFlags.Selected;
        }

        var nodeopen = false;
        var unsaved = container != null && container.HasUnsavedChanges ? "*" : "";


        ImGui.BeginGroup();

        string treeNodeName = $@"{Icons.Cube} {container.Name}";
        string treeNodeNameFormat = $@"{Icons.Cube} {container.Name}{unsaved}";

        nodeopen = ImGui.TreeNodeEx(treeNodeName, treeflags, treeNodeNameFormat);

        // TODO: alias

        ImGui.EndGroup();

        if (Editor.ViewportSelection.ShouldGoto(modelRoot) || Editor.ViewportSelection.ShouldGoto(modelRef))
        {
            ImGui.SetScrollHereY();
            Editor.ViewportSelection.ClearGotoTarget();
        }

        if (nodeopen)
        {
            ImGui.Indent(); //TreeNodeEx fails to indent as it is inside a group / indentation is reset
        }

        DisplayTopContextMenu(container, selected);
        HandleSelectionClick(selectTarget, modelRoot, modelRef, nodeopen);

        if (nodeopen)
        {
            var scale = DPI.UIScale();
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(8.0f, 3.0f) * scale);

            TypeView(container);

            ImGui.PopStyleVar();
            ImGui.TreePop();
        }

        ImGui.EndChild();
    }

    private void DisplayTopContextMenu(ModelContainer map, bool selected)
    {
        if (ImGui.BeginPopupContextItem($@"modelTopContext_{map.Name}"))
        {
            if (ImGui.Selectable("Copy Model Name"))
            {
                PlatformUtils.Instance.SetClipboardText(map.Name);
            }

            ImGui.EndPopup();
        }
    }

    private void HandleSelectionClick(ISelectable selectTarget, Entity modelRoot, ObjectContainerReference modelRef, bool nodeopen)
    {
        if (ImGui.IsItemClicked())
        {
            _pendingClick = selectTarget;
        }

        if (ImGui.IsMouseDoubleClicked(0) && _pendingClick != null && modelRoot == _pendingClick)
        {
            Editor.ModelViewportView.Viewport.FramePosition(modelRoot.GetLocalTransform().Position, 10f);
        }

        if ((_pendingClick == modelRoot || modelRef.Equals(_pendingClick)) && ImGui.IsMouseReleased(ImGuiMouseButton.Left))
        {
            if (ImGui.IsItemHovered())
            {
                // Only select if a node is not currently being opened/closed
                if (modelRoot == null || 
                    nodeopen && _treeOpenEntities.Contains(modelRoot) || 
                    !nodeopen && !_treeOpenEntities.Contains(modelRoot))
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
                if (modelRoot != null)
                {
                    if (nodeopen && !_treeOpenEntities.Contains(modelRoot))
                    {
                        _treeOpenEntities.Add(modelRoot);
                    }
                    else if (!nodeopen && _treeOpenEntities.Contains(modelRoot))
                    {
                        _treeOpenEntities.Remove(modelRoot);
                    }
                }
            }

            _pendingClick = null;
        }
    }

    private void TypeView(ModelContainer container)
    {
        Editor.EntityTypeCache.AddModelToCache(container);

        var dict = Editor.EntityTypeCache._cachedTypeView[container.Name].OrderBy(q => q.Key.ToString());

        foreach (var cats in dict)
        {
            if (cats.Value.Count > 0)
            {
                ImGuiTreeNodeFlags treeflags = ImGuiTreeNodeFlags.OpenOnArrow;

                if (ImGui.TreeNodeEx(cats.Key.ToString(), treeflags))
                {
                    foreach (var typ in cats.Value.OrderBy(q => q.Key.Name))
                    {
                        if (typ.Value.Count > 0)
                        {
                            if (ImGui.TreeNodeEx(typ.Key.Name, treeflags))
                            {
                                int index = 0;

                                foreach (var obj in typ.Value)
                                {
                                    if (CanDisplayModelObject(container, obj))
                                    {
                                        ModelObjectSelectable(container, obj, index);
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

    private void HierarchyView(ModelContainer container, Entity entity)
    {
        foreach (Entity obj in entity.Children)
        {
            if (obj is Entity e)
            {
                ModelObjectSelectable(container, e, -1, true);
            }
        }
    }

    private unsafe void ModelObjectSelectable(ModelContainer container, Entity e, int index = -1, bool hierarchial = false)
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

        // Dummy: Ref ID
        if(e.WrappedObject is FLVER.Dummy)
        {
            var dummy = (FLVER.Dummy)e.WrappedObject;

            var parentBone = dummy.ParentBoneIndex;

            if (parentBone != -1)
            {
                for (int i = 0; i < container.Nodes.Count; i++)
                {
                    var curNode = container.Nodes[i];

                    if (i == parentBone)
                    {
                        var node = (FLVER.Node)curNode.WrappedObject;

                        key = $"{node.Name} [{dummy.ReferenceID}]";
                    }
                }

            }
            else
            {
                key = $"{key} [{dummy.ReferenceID}]";
            }
        }

        // Main selectable
        if (e is ModelEntity me)
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

        DisplayVisibilityButton(e, container, key, index);

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
                Editor.FrameAction.FrameCurrentEntity(e);
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

            if (ImGui.Selectable($"{displayName}##{treeImGuiId}", Editor.ViewportSelection.GetSelection().Contains(e), selectableFlags))
            {
                doSelect = true;

                // If double clicked frame the selection in the viewport
                if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                {
                    if (e.RenderSceneMesh != null)
                    {
                        Editor.FrameAction.FrameCurrentEntity(e);
                    }
                }
            }
            if (ImGui.IsItemFocused() && (InputTracker.GetKey(Key.Up) || InputTracker.GetKey(Key.Down)))
            {
                doSelect = true;
                arrowKeySelect = true;
            }

            DisplayModelObjectContextMenu(container, e, treeImGuiId);

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
        HandleEntitySelection(e, doSelect, arrowKeySelect);

        // If there's children then draw them
        if (nodeopen)
        {
            HierarchyView(container, e);
            ImGui.TreePop();
        }

        ImGui.PopID();
    }

    private void DisplayModelObjectContextMenu(ModelContainer container, Entity ent, int imguiID)
    {
        if (ImGui.BeginPopupContextItem($@"modelObjectContext_{container.Name}_{imguiID}"))
        {
            Editor.DuplicateAction.OnContext();
            Editor.DeleteAction.OnContext();

            ImGui.Separator();

            Editor.FrameAction.OnContext();
            Editor.PullToCameraAction.OnContext();

            ImGui.Separator();

            Editor.ReorderAction.OnContext();

            ImGui.EndPopup();
        }
    }

    public void DisplayVisibilityButton(Entity entity, ModelContainer container, string key, int index)
    {
        // Visibility icon
        var icon = entity.EditorVisible ? Icons.Eye : Icons.EyeSlash;

        ImGui.PushItemFlag(ImGuiItemFlags.NoNav, true);
        ImGui.PushStyleColor(ImGuiCol.Button, Vector4.Zero);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, Vector4.Zero);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, Vector4.Zero);
        ImGui.PushStyleColor(ImGuiCol.Border, Vector4.Zero);

        if (ImGui.Button($"{icon}##modelObjectVisibility{key}{index}", DPI.InlineIconButtonSize))
        {
            if (InputTracker.GetKey(KeyBindings.Current.MAP_ToggleMapObjectGroupVisibility))
            {
                foreach (var entry in container.RootObject.Children)
                {
                    if (entry.WrappedObject.GetType() == entity.WrappedObject.GetType())
                    {
                        entry.EditorVisible = !entry.EditorVisible;
                    }
                }
            }
            else
            {
                entity.EditorVisible = !entity.EditorVisible;
            }
        }
        ImGui.PopStyleColor(4);
        ImGui.PopItemFlag();
        ImGui.SameLine();

        UIHelper.Tooltip("Toggle visibility state of this model object.");

    }

    public void HandleEntitySelection(Entity entity, bool itemSelected, bool isItemFocused, List<WeakReference<Entity>> filteredEntityList = null)
    {
        // Up/Down arrow mass selection
        var arrowKeySelect = false;
        if (isItemFocused && (InputTracker.GetKey(Key.Up) || InputTracker.GetKey(Key.Down)))
        {
            itemSelected = true;
            arrowKeySelect = true;
        }

        if (itemSelected)
        {
            if (arrowKeySelect)
            {
                if (InputTracker.GetKey(Key.ControlLeft)
                    || InputTracker.GetKey(Key.ControlRight)
                    || InputTracker.GetKey(Key.ShiftLeft)
                    || InputTracker.GetKey(Key.ShiftRight))
                {
                    Editor.ViewportSelection.AddSelection(Editor, entity);
                }
                else
                {
                    Editor.ViewportSelection.ClearSelection(Editor);
                    Editor.ViewportSelection.AddSelection(Editor, entity);
                }
            }
            else if (InputTracker.GetKey(Key.ControlLeft) || InputTracker.GetKey(Key.ControlRight))
            {
                // Toggle Selection
                if (Editor.ViewportSelection.GetSelection().Contains(entity))
                {
                    Editor.ViewportSelection.RemoveSelection(Editor, entity);
                }
                else
                {
                    Editor.ViewportSelection.AddSelection(Editor, entity);
                }
            }
            else if (Editor.ViewportSelection.GetSelection().Count > 0
                     && (InputTracker.GetKey(Key.ShiftLeft) || InputTracker.GetKey(Key.ShiftRight)))
            {
                // Select Range
                List<Entity> entList;
                if (filteredEntityList != null)
                {
                    entList = new();
                    foreach (WeakReference<Entity> ent in filteredEntityList)
                    {
                        if (ent.TryGetTarget(out Entity e))
                        {
                            entList.Add(e);
                        }
                    }
                }
                else
                {
                    entList = entity.Container.Objects;
                }

                var i1 = -1;

                if (entity.GetType() == typeof(ModelEntity))
                {
                    i1 = entList.IndexOf(Editor.ViewportSelection.GetFilteredSelection<ModelEntity>()
                        .FirstOrDefault(fe => fe.Container == entity.Container && fe != entity.Container.RootObject));
                }

                var i2 = -1;

                if (entity.GetType() == typeof(ModelEntity))
                {
                    i2 = entList.IndexOf((ModelEntity)entity);
                }

                if (i1 != -1 && i2 != -1)
                {
                    var iStart = i1;
                    var iEnd = i2;
                    if (i2 < i1)
                    {
                        iStart = i2;
                        iEnd = i1;
                    }

                    for (var i = iStart; i <= iEnd; i++)
                    {
                        Editor.ViewportSelection.AddSelection(Editor, entList[i]);
                    }
                }
                else
                {
                    Editor.ViewportSelection.AddSelection(Editor, entity);
                }
            }
            else
            {
                // Exclusive Selection
                Editor.ViewportSelection.ClearSelection(Editor);
                Editor.ViewportSelection.AddSelection(Editor, entity);
            }
        }
    }

    // This updates the model content tree when actions occur
    public void OnActionEvent(ActionEvent evt)
    {
        if (evt.HasFlag(ActionEvent.ObjectAddedRemoved))
        {
            Editor.EntityTypeCache.InvalidateCache();
        }
    }
}
