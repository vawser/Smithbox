using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using StudioCore.Utilities;
using System.Numerics;

namespace StudioCore.Editors.ModelEditor;

public class ModelContentsView : IActionEventHandler
{
    public ModelEditorView View;
    public ProjectEntry Project;

    public string ImguiID = "ModelContentView";
    private int treeImGuiId = 0;

    private bool _setNextFocus;
    private ISelectable _pendingClick;
    private HashSet<Entity> _treeOpenEntities = new();

    private string ContentTreeFilter = "";
    private bool ExactContentTreeFilter = false;

    // The rows in the order they were drawn, this frame and the last, so a shift-click selects the rows between the
    // two clicks as they appear (categories and the node hierarchy order them differently from the objects)
    private List<Entity> _displayOrder = new();
    private List<Entity> _lastDisplayOrder = new();

    // The row a shift-click range starts from: the last one clicked without shift
    private Entity _rangeAnchor;

    // Drag and drop in the node hierarchy: what is being dragged, and a dummy dropped on a bone waiting for the
    // choice between attaching it and moving it into the bone's space
    private const string HierarchyPayload = "MODEL_HIERARCHY_ENTRY";
    private Entity _dragEntity;
    private (Entity Dummy, Entity Bone)? _pendingDummyDrop;
    private bool _openDummyDropMenu;

    public ModelContentsView(ModelEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        view.ViewportActionManager.AddEventHandler(this);
        view.ActionManager.AddEventHandler(this);
    }

    public void Display(float width, float height)
    {
        GUI.TitleHeader(
            LOC.Get("MODEL_Contents_Header"),
            LOC.Get("MODEL_Contents_Header_TT"));

        DisplayHeader();

        ImGui.BeginChild("ModelContents", new Vector2(0, 0), ImGuiChildFlags.Borders);

        if (View.Selection.SelectedModelWrapper != null)
        {
            if (View.Selection.SelectedModelWrapper.HasCollision)
            {
                ImGui.Text(LOC.Get("MODEL_Contents_Has_Collision_Hint"));
            }
            else
            {
                ImGui.Text(LOC.Get("MODEL_Contents_Lacks_Collision_Hint"));
            }
        }

        if (View.Selection.SelectedModelWrapper != null && 
            View.Selection.SelectedModelWrapper.FLVER != null)
        {
            treeImGuiId = 0;

            var container = View.Selection.SelectedModelWrapper.Container;

            if (container != null)
            {
                DisplayContentTree(container);
            }
        }
        else
        {
            ImGui.Text(LOC.Get("MODEL_Contents_No_Flver_Hint"));
        }

        ImGui.EndChild();
    }

    public void DisplayHeader()
    {
        ImGui.BeginChild($"framedListFilter_modelEditor_ContentTree", EditorFilters.GetHeaderSize(), ImGuiChildFlags.Borders);

        EditorFilters.DisplaySearchbar("modelEditor_ContentTree",
            ref ContentTreeFilter, ref ExactContentTreeFilter);

        var wrapper = View.Selection.SelectedModelWrapper;

        ImGui.SameLine();

        // Show All
        ImGui.SameLine();
        if (ImGui.Button($"{Icons.Eye}", DPI.IconButtonSize))
        {
            if (wrapper != null && wrapper.Container != null)
            {
                foreach (var entry in wrapper.Container.Objects)
                {
                    entry.EditorVisible = true;
                }
            }
        }
        GUI.Tooltip(LOC.Get("MODEL_Contents_Toggle_Show_All_TT"));

        // Hide All
        ImGui.SameLine();
        if (ImGui.Button($"{Icons.EyeSlash}", DPI.IconButtonSize))
        {
            if (wrapper != null && wrapper.Container != null)
            {
                foreach (var entry in wrapper.Container.Objects)
                {
                    entry.EditorVisible = false;
                }
            }
        }
        GUI.Tooltip(LOC.Get("MODEL_Contents_Toggle_Hide_All_TT"));


        // Auto-Open Tree
        ImGui.SameLine();
        if (ImGui.Button($"{Icons.Tree}##toggleAutoOpen"))
        {
            CFG.Current.ModelEditor_Contents_Auto_Open_Tree = !CFG.Current.ModelEditor_Contents_Auto_Open_Tree;
        }

        var autoTreeMode = LOC.Get("MODEL_Contents_TreeState_Open");
        if (CFG.Current.ModelEditor_Contents_Auto_Open_Tree)
            autoTreeMode = LOC.Get("MODEL_Contents_TreeState_Closed");

        GUI.Tooltip(LOC.Get("MODEL_Contents_TreeState_TT", autoTreeMode));

        // Node Hierarchy
        ImGui.SameLine();
        if (ImGui.Button($"{Icons.Sitemap}##toggleNodeHierarchy"))
        {
            CFG.Current.ModelEditor_Contents_Node_Hierarchy = !CFG.Current.ModelEditor_Contents_Node_Hierarchy;
        }

        var hierarchyMode = CFG.Current.ModelEditor_Contents_Node_Hierarchy
            ? LOC.Get("MODEL_Contents_Node_Hierarchy_On")
            : LOC.Get("MODEL_Contents_Node_Hierarchy_Off");

        GUI.Tooltip(LOC.Get("MODEL_Contents_Node_Hierarchy_TT", hierarchyMode));

        // Refresh Viewport: spins while a preview of the edits is on its way
        ImGui.SameLine();

        var isPreviewPending = wrapper?.Container?.IsPreviewPending ?? false;

        ImGui.BeginDisabled(wrapper?.Container == null);
        if (ImGui.Button($"{(isPreviewPending ? Icons.Spinner : Icons.Refresh)}##refreshViewportFromEdits"))
        {
            View.RefreshViewportFromEdits();
        }
        ImGui.EndDisabled();
        GUI.Tooltip(LOC.Get("MODEL_Editor_Viewport_Refresh_TT"));

        ImGui.EndChild();
    }

    public bool CanDisplayModelObject(ModelContainer container, ModelEntity entity)
    {
        return true;
    }

    public void DisplayContentTree(ModelContainer container)
    {
        _lastDisplayOrder = _displayOrder;
        _displayOrder = new();

        ImGui.BeginChild($"modelContentsTree_{ImguiID}");

        Entity modelRoot = container?.RootObject;
        ObjectContainerReference modelRef = new(container.Name);

        ISelectable selectTarget = (ISelectable)modelRoot ?? modelRef;

        ImGuiTreeNodeFlags treeflags = ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.SpanAvailWidth;

        if (CFG.Current.ModelEditor_Contents_Auto_Open_Tree)
        {
            treeflags = treeflags | ImGuiTreeNodeFlags.DefaultOpen;
        }

        var selected = View.ViewportSelection.GetSelection().Contains(modelRoot) ||
            View.ViewportSelection.GetSelection().Contains(modelRef);

        // Go To opens the model so its target can be reached
        if (View.ViewportSelection.GotoTreeTarget is Entity gotoTarget && gotoTarget.Container == container)
        {
            ImGui.SetNextItemOpen(true);
        }

        if (selected)
        {
            treeflags |= ImGuiTreeNodeFlags.Selected;
        }

        var nodeopen = false;
        // The same marker as the File and Source Lists, which every kind of edit sets and saving clears
        var unsaved = View.Selection.SelectedModelWrapper is { IsDirty: true } wrapper && wrapper.Container == container
            ? "*"
            : "";

        ImGui.BeginGroup();

        string treeNodeName = $@"{Icons.Cube} {container.Name}";
        string treeNodeNameFormat = $@"{Icons.Cube} {container.Name}{unsaved}";

        nodeopen = ImGui.TreeNodeEx(treeNodeName, treeflags, treeNodeNameFormat);

        // TODO: alias

        ImGui.EndGroup();

        if (View.ViewportSelection.ShouldGoto(modelRoot) || View.ViewportSelection.ShouldGoto(modelRef))
        {
            ImGui.SetScrollHereY();
            View.ViewportSelection.ClearGotoTarget();
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

        DisplayDummyDropMenu(container);

        ImGui.EndChild();
    }

    #region Drag and Drop
    /// <summary>
    /// Bones and dummies in the node hierarchy can be dragged onto bones. Called right after the row is drawn.
    /// </summary>
    private unsafe void HandleHierarchyDragDrop(ModelContainer container, Entity e, string label)
    {
        if (e.WrappedObject is FLVER.Node or FLVER.Dummy && ImGui.BeginDragDropSource())
        {
            _dragEntity = e;

            byte dummy = 0;
            ImGui.SetDragDropPayload(HierarchyPayload, &dummy, 1);
            ImGui.Text(label);

            ImGui.EndDragDropSource();
        }

        if (e.WrappedObject is FLVER.Node && ImGui.BeginDragDropTarget())
        {
            var payload = ImGui.AcceptDragDropPayload(HierarchyPayload);

            if (!payload.IsNull && _dragEntity is ModelEntity dragged && dragged != e)
            {
                OnDropOnBone(container, dragged, e);
                _dragEntity = null;
            }

            ImGui.EndDragDropTarget();
        }
    }

    /// <summary>
    /// The Node category header takes bones dropped on it to the top of the skeleton
    /// </summary>
    private void HandleRootDropTarget(ModelContainer container)
    {
        if (!ImGui.BeginDragDropTarget())
            return;

        var payload = ImGui.AcceptDragDropPayload(HierarchyPayload);

        if (!payload.IsNull && _dragEntity is ModelEntity { WrappedObject: FLVER.Node } dragged)
        {
            Execute(ReparentAction.ForNode(View, container, dragged, -1));
            _dragEntity = null;
        }

        ImGui.EndDragDropTarget();
    }

    private void OnDropOnBone(ModelContainer container, ModelEntity dragged, Entity bone)
    {
        var boneIndex = container.Nodes.IndexOf(bone);

        if (boneIndex < 0)
            return;

        if (dragged.WrappedObject is FLVER.Node)
        {
            var action = ReparentAction.ForNode(View, container, dragged, boneIndex);

            if (action == null)
            {
                Smithbox.LogError(this, LOC.Get("MODEL_Contents_Drop_Into_Own_Child", dragged.Name, bone.Name));
                return;
            }

            Execute(action);
        }
        else if (dragged.WrappedObject is FLVER.Dummy)
        {
            // A dummy has two links to bones: which one is meant is asked
            _pendingDummyDrop = (dragged, bone);
            _openDummyDropMenu = true;
        }
    }

    private void DisplayDummyDropMenu(ModelContainer container)
    {
        if (_openDummyDropMenu)
        {
            ImGui.OpenPopup("##modelDummyDropMenu");
            _openDummyDropMenu = false;
        }

        if (!ImGui.BeginPopup("##modelDummyDropMenu"))
            return;

        if (_pendingDummyDrop is { } drop && drop.Dummy is ModelEntity dummy)
        {
            var boneIndex = container.Nodes.IndexOf(drop.Bone);
            var boneName = (drop.Bone.WrappedObject as FLVER.Node)?.Name ?? "";

            if (ImGui.Selectable(LOC.Get("MODEL_Contents_Drop_Dummy_Attach", boneName)))
            {
                Execute(ReparentAction.ForDummyAttach(View, dummy, boneIndex));
                _pendingDummyDrop = null;
            }
            GUI.Tooltip(LOC.Get("MODEL_Contents_Drop_Dummy_Attach_TT"));

            if (ImGui.Selectable(LOC.Get("MODEL_Contents_Drop_Dummy_Parent", boneName)))
            {
                Execute(ReparentAction.ForDummyParent(View, container, dummy, boneIndex));
                _pendingDummyDrop = null;
            }
            GUI.Tooltip(LOC.Get("MODEL_Contents_Drop_Dummy_Parent_TT"));
        }

        ImGui.EndPopup();
    }

    private void Execute(ViewportAction action)
    {
        if (action != null)
        {
            View.ViewportActionManager.ExecuteAction(action);
        }
    }
    #endregion

    private void TypeView(ModelContainer container)
    {
        View.EntityTypeCache.AddModelToCache(container);

        var isFiltering = !string.IsNullOrWhiteSpace(ContentTreeFilter);
        var gotoTarget = View.ViewportSelection.GotoTreeTarget as Entity;

        foreach (KeyValuePair<ModelEntityType, Dictionary<Type, List<ModelEntity>>> cats in
                 View.EntityTypeCache._cachedTypeView[container.Name].OrderBy(q => q.Key.ToString()))
        {
            var entries = cats.Value.OrderBy(q => q.Key.Name).SelectMany(q => q.Value).ToList();

            if (entries.Count == 0)
            {
                // Empty categories only get in the way of the results while filtering
                if (!isFiltering)
                {
                    ImGui.Text($@"   {cats.Key.ToString()}");
                }

                continue;
            }

            var matchCount = isFiltering
                ? entries.Select((e, i) => GetEntryLabel(container, e, i))
                    .Count(label => EditorFilters.IsMatch(ContentTreeFilter, label, ExactContentTreeFilter))
                : entries.Count;

            if (isFiltering && matchCount == 0)
                continue;

            var countText = isFiltering ? $"{matchCount}/{entries.Count}" : $"{entries.Count}";

            ImGuiTreeNodeFlags treeflags = ImGuiTreeNodeFlags.OpenOnArrow;

            // Filtering opens the categories, so the matches can be seen
            if (CFG.Current.ModelEditor_Contents_Auto_Open_Tree || isFiltering)
            {
                treeflags = treeflags | ImGuiTreeNodeFlags.DefaultOpen;
            }

            // Go To opens the category holding its target
            if (gotoTarget != null && entries.Any(e => e == gotoTarget || IsAncestorOf(e, gotoTarget)))
            {
                ImGui.SetNextItemOpen(true);
            }

            // The flat list is kept while filtering, as matches can be nested deep within the hierarchy
            var useHierarchy = cats.Key == ModelEntityType.Node
                && CFG.Current.ModelEditor_Contents_Node_Hierarchy
                && !isFiltering;

            var categoryOpen = ImGui.TreeNodeEx($"{cats.Key} ({countText})###category_{cats.Key}", treeflags);

            // Bones dropped on the header go to the top of the skeleton
            if (useHierarchy)
            {
                HandleRootDropTarget(container);
            }

            if (categoryOpen)
            {
                if (useHierarchy)
                {
                    HierarchyView(container, container.RootObject, entries);
                }
                else
                {
                    for (var index = 0; index < entries.Count; index++)
                    {
                        var obj = entries[index];

                        if (CanDisplayModelObject(container, obj))
                        {
                            ModelObjectSelectable(container, obj, index);
                        }
                    }
                }

                ImGui.TreePop();
            }
        }
    }

    private static bool IsAncestorOf(Entity ancestor, Entity entity)
    {
        for (var parent = entity?.Parent; parent != null; parent = parent.Parent)
        {
            if (parent == ancestor)
                return true;
        }

        return false;
    }

    private void DisplayTopContextMenu(ModelContainer map, bool selected)
    {
        if (ImGui.BeginPopupContextItem($@"modelTopContext_{map.Name}"))
        {
            // Add: with the model root selected, new objects go to their defaults
            View.AddAction.OnContext();

            ImGui.Separator();

            // Information
            if (ImGui.BeginMenu($"{LOC.Get("MODEL_Contents_Context_Info_Header")}##infoMenuHeader"))
            {
                // Copy Model Name
                if (ImGui.Selectable($"{LOC.Get("MODEL_Contents_Action_Copy_Model_Name")}##copyModelNameAction"))
                {
                    PlatformUtils.Instance.SetClipboardText(map.Name);
                }
                GUI.Tooltip(LOC.Get("MODEL_Contents_Action_Copy_Model_Name_TT"));

                ImGui.EndMenu();
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
            // Frames the whole model
            View.FrameAction.FrameCurrentEntity(modelRoot);
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
                    if (InputManager.HasCtrlDown())
                    {
                        // Toggle Selection
                        if (View.ViewportSelection.GetSelection().Contains(selectTarget))
                        {
                            View.ViewportSelection.RemoveSelection(selectTarget);
                        }
                        else
                        {
                            View.ViewportSelection.AddSelection(selectTarget);
                        }
                    }
                    else
                    {
                        View.ViewportSelection.ClearSelection();
                        View.ViewportSelection.AddSelection(selectTarget);
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

    /// <summary>
    /// Lists the children of an entity as a tree. At the model root, only the entries passed in are listed, as the
    /// root also holds the meshes, materials and so on.
    /// </summary>
    private void HierarchyView(ModelContainer container, Entity entity, List<ModelEntity> rootEntries = null)
    {
        var allowed = rootEntries != null ? new HashSet<Entity>(rootEntries) : null;

        foreach (Entity obj in entity.Children.ToList())
        {
            if (allowed != null && !allowed.Contains(obj))
                continue;

            var index = obj.WrappedObject is FLVER.Dummy
                ? container.Dummies.IndexOf(obj)
                : container.Nodes.IndexOf(obj);

            ModelObjectSelectable(container, obj, index, true);
        }
    }

    /// <summary>
    /// The text an object is listed under, which the filter also matches against
    /// </summary>
    private string GetEntryLabel(ModelContainer container, Entity e, int index)
    {
        var key = $"{LOC.Get("MODEL_Contents_Entry_Title")} {index}";

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

        // Mesh: Material [Node]
        if (e.WrappedObject is FLVER2.Mesh)
        {
            var mesh = (FLVER2.Mesh)e.WrappedObject;

            var matIndex = mesh.MaterialIndex;
            var nodeIndex = mesh.NodeIndex;

            var meshName = LOC.Get("MODEL_Contents_Mesh_Title");
            var nodeName = "";

            if (matIndex != -1)
            {
                for (int i = 0; i < container.Materials.Count; i++)
                {
                    var curMat = container.Materials[i];

                    if (i == matIndex)
                    {
                        var mat = (FLVER2.Material)curMat.WrappedObject;

                        meshName = mat.Name;
                    }
                }
            }

            if (nodeIndex != -1)
            {
                for (int i = 0; i < container.Nodes.Count; i++)
                {
                    var curNode = container.Nodes[i];

                    if (i == nodeIndex)
                    {
                        var node = (FLVER.Node)curNode.WrappedObject;

                        nodeName = $"{node.Name}";
                    }
                }
            }

            if (CFG.Current.ModelEditor_Contents_NodeNameInMeshEntry)
            {
                key = $"{meshName} [{nodeName}]";
            }
            else
            {
                key = $"{meshName}";
            }
        }

        return key;
    }

    private unsafe void ModelObjectSelectable(ModelContainer container, Entity e, int index = -1, bool hierarchial = false)
    {
        var scale = DPI.UIScale();

        var key = GetEntryLabel(container, e, index);

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

        if (hierarchial && e.Children.Count > 0)
        {
            DisplayVisibilityButton(e, container, key, index);

            ImGuiTreeNodeFlags treeflags = ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.SpanAvailWidth;

            if (CFG.Current.ModelEditor_Contents_Auto_Open_Tree)
            {
                treeflags = treeflags | ImGuiTreeNodeFlags.DefaultOpen;
            }

            if (View.ViewportSelection.GetSelection().Contains(e))
            {
                treeflags |= ImGuiTreeNodeFlags.Selected;
            }

            // Go To opens the nodes above its target
            if (View.ViewportSelection.GotoTreeTarget is Entity gotoTarget && IsAncestorOf(e, gotoTarget))
            {
                ImGui.SetNextItemOpen(true);
            }

            nodeopen = ImGui.TreeNodeEx($"{key}##{e.PrettyName}", treeflags);
            _displayOrder.Add(e);

            HandleHierarchyDragDrop(container, e, key);

            if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(0))
            {
                if (e.RenderSceneMesh != null)
                {
                    View.FrameAction.FrameCurrentEntity(e);
                }
            }

            // Bones with children get the same actions as the rest
            treeImGuiId++;
            DisplayModelObjectContextMenu(container, e, treeImGuiId);

            if (ImGui.IsItemFocused())
            {
                if (InputManager.HasArrowSelection())
                {
                    doSelect = true;
                    arrowKeySelect = true;
                }
            }
        }
        else
        {
            treeImGuiId++;
            var selectableFlags = ImGuiSelectableFlags.AllowDoubleClick | ImGuiSelectableFlags.AllowOverlap;

            var displayName = key;

            var isMatch = EditorFilters.IsMatch(ContentTreeFilter, displayName, ExactContentTreeFilter);

            if (isMatch)
            {
                DisplayVisibilityButton(e, container, key, index);

                var isSelectableClicked = ImGui.Selectable($"{displayName}##{treeImGuiId}", View.ViewportSelection.GetSelection().Contains(e), selectableFlags);
                _displayOrder.Add(e);

                // Childless bones and the dummies under bones, in the node hierarchy
                if (hierarchial)
                {
                    HandleHierarchyDragDrop(container, e, displayName);
                }

                if (isSelectableClicked)
                {
                    doSelect = true;

                    // If double clicked frame the selection in the viewport
                    if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                    {
                        if (e.RenderSceneMesh != null)
                        {
                            View.FrameAction.FrameCurrentEntity(e);
                        }
                    }
                }

                if (ImGui.IsItemFocused())
                {
                    if (InputManager.HasArrowSelection())
                    {
                        doSelect = true;
                        arrowKeySelect = true;
                    }
                }

                DisplayModelObjectContextMenu(container, e, treeImGuiId);
            }
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

        if (View.ViewportSelection.ShouldGoto(e))
        {
            // By default, this places the item at 50% in the frame. Use 0 to place it on top.
            ImGui.SetScrollHereY();
            View.ViewportSelection.ClearGotoTarget();
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
            View.AddAction.OnContext();

            ImGui.Separator();

            View.DuplicateAction.OnContext();
            View.DeleteAction.OnContext();

            ImGui.Separator();

            View.FrameAction.OnContext();
            View.PullToCameraAction.OnContext();

            ImGui.Separator();

            View.ReorderAction.OnContext();

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
            if (InputManager.IsPressed(KeybindID.Apply_to_All))
            {
                // Use one target state for the whole group; inverting each entry individually leaves
                // mixed states whenever the entries didn't all start out the same.
                var newVisibleState = !entity.EditorVisible;

                // All the objects, as bones and dummies are mostly parented to other bones rather than the root
                foreach (var entry in container.Objects)
                {
                    if (entry.WrappedObject?.GetType() == entity.WrappedObject?.GetType())
                    {
                        entry.EditorVisible = newVisibleState;
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

        GUI.Tooltip(LOC.Get("MODEL_Contents_Visibility_Button_TT"));

    }

    public void HandleEntitySelection(Entity entity, bool itemSelected, bool isItemFocused)
    {
        // Up/Down arrow mass selection
        var arrowKeySelect = false;

        if (isItemFocused && InputManager.HasArrowSelection())
        {
            itemSelected = true;
            arrowKeySelect = true;
        }

        if (itemSelected)
        {
            if (arrowKeySelect)
            {
                if (InputManager.HasCtrlDown() || InputManager.HasShiftDown())
                {
                    View.ViewportSelection.AddSelection(entity);
                }
                else
                {
                    View.ViewportSelection.ClearSelection();
                    View.ViewportSelection.AddSelection(entity);
                }

                _rangeAnchor = entity;
            }
            else if (InputManager.HasCtrlDown())
            {
                // Toggle Selection
                if (View.ViewportSelection.GetSelection().Contains(entity))
                {
                    View.ViewportSelection.RemoveSelection(entity);
                }
                else
                {
                    View.ViewportSelection.AddSelection(entity);
                }

                _rangeAnchor = entity;
            }
            else if (View.ViewportSelection.GetSelection().Count > 0
                     && InputManager.HasShiftDown())
            {
                // Select Range: the rows between the anchor and this one, as drawn. The anchor stays put, so
                // further shift-clicks change the end of the range.
                var i1 = _rangeAnchor != null ? _lastDisplayOrder.IndexOf(_rangeAnchor) : -1;
                var i2 = _lastDisplayOrder.IndexOf(entity);

                if (i1 != -1 && i2 != -1)
                {
                    var iStart = Math.Min(i1, i2);
                    var iEnd = Math.Max(i1, i2);

                    View.ViewportSelection.ClearSelection();

                    for (var i = iStart; i <= iEnd; i++)
                    {
                        View.ViewportSelection.AddSelection(_lastDisplayOrder[i]);
                    }
                }
                else
                {
                    View.ViewportSelection.AddSelection(entity);
                }
            }
            else
            {
                // Exclusive Selection
                View.ViewportSelection.ClearSelection();
                View.ViewportSelection.AddSelection(entity);

                _rangeAnchor = entity;
            }
        }
    }

    // This updates the model content tree when actions occur
    public void OnActionEvent(ActionEvent evt)
    {
        if (evt.HasFlag(ActionEvent.ObjectAddedRemoved))
        {
            View.EntityTypeCache.InvalidateCache();
        }
    }
}
