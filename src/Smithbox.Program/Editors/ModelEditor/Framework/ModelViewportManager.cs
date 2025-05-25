using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Editor.Multiselection;
using StudioCore.Editors.MapEditor.Framework;
using StudioCore.Editors.ModelEditor.Actions.Viewport;
using StudioCore.Editors.ModelEditor.Enums;
using StudioCore.Interface;
using StudioCore.ViewportNS;
using System.Numerics;

namespace StudioCore.Editors.ModelEditor;

public class ModelViewportManager
{
    public ModelEditorScreen Editor;

    public IViewport Viewport;

    public bool IsUpdatingViewportModel = false;
    public bool IgnoreHierarchyFocus = false;

    public ModelViewportManager(ModelEditorScreen screen, IViewport viewport)
    {
        Editor = screen;
        Viewport = viewport;
    }

    public bool HasValidLoadedContainer()
    {
        if (Editor._universe.LoadedModelContainer == null)
            return false;

        return true;
    }

    public void UpdateRepresentativeModel(int selectionIndex)
    {
        IsUpdatingViewportModel = true;

        Editor._selection.ClearSelection(Editor);

        UpdateRepresentativeModel();

        if (Editor.Selection._selectedFlverGroupType == GroupSelectionType.Dummy)
        {
            SelectViewportDummy(selectionIndex, Editor._universe.LoadedModelContainer.DummyPoly_RootNode);
        }
        if (Editor.Selection._selectedFlverGroupType == GroupSelectionType.Node)
        {
            SelectViewportDummy(selectionIndex, Editor._universe.LoadedModelContainer.Bone_RootNode);
        }
        if (Editor.Selection._selectedFlverGroupType == GroupSelectionType.Mesh)
        {
            SelectViewportDummy(selectionIndex, Editor._universe.LoadedModelContainer.Mesh_RootNode);
        }

        IsUpdatingViewportModel = false;
    }

    private void SelectViewportDummy(int selectIndex, Entity rootNode)
    {
        if (selectIndex != -1)
        {
            int idx = 0;
            foreach (var entry in rootNode.Children)
            {
                if (idx == selectIndex)
                {
                    Editor._selection.AddSelection(Editor, entry);
                }
                idx++;
            }
        }
    }

    public void UpdateRepresentativeModel()
    {
        var currentInfo = Editor.ResManager.LoadedFlverContainer;

        var containerId = currentInfo.ContainerName;
        var modelId = currentInfo.ContainerName;
        var modelType = currentInfo.Type;
        var mapId = currentInfo.MapID;

        Editor.ResManager.LoadRepresentativeModel(containerId, modelId, modelType, mapId);
    }

    public void UpdateRepresentativeDummy(int index, Vector3 position)
    {
        // Required to stop the LowRequirements build from failing
        if (Smithbox.LowRequirementsMode)
            return;

        if (!HasValidLoadedContainer())
            return;

        var container = Editor._universe.LoadedModelContainer;

        if (index > container.DummyPoly_RootNode.Children.Count - 1)
            return;

        var curNode = container.DummyPoly_RootNode.Children[index];
        ChangeVisualDummyTransform act = new(Editor, curNode, position);
        Editor.EditorActionManager.ExecuteAction(act);
    }

    public void UpdateRepresentativeNode(int index, Vector3 position, Vector3 rotation, Vector3 scale)
    {
        // Required to stop the LowRequirements build from failing
        if (Smithbox.LowRequirementsMode)
            return;

        if (!HasValidLoadedContainer())
            return;

        var container = Editor._universe.LoadedModelContainer;

        if (index > container.Bone_RootNode.Children.Count - 1)
            return;

        var curNode = container.Bone_RootNode.Children[index];
        ChangeVisualNodeTransform act = new(Editor, curNode, position, rotation, scale);
        Editor.EditorActionManager.ExecuteAction(act);
    }

    public void SelectRepresentativeDummy(int index, Multiselection multiSelect)
    {
        // Required to stop the LowRequirements build from failing
        if (Smithbox.LowRequirementsMode)
            return;

        if (!HasValidLoadedContainer())
            return;

        var container = Editor._universe.LoadedModelContainer;

        if (index > container.DummyPoly_RootNode.Children.Count - 1)
            return;

        if (multiSelect.HasValidMultiselection())
        {
            Editor._selection.ClearSelection(Editor);

            foreach (var entry in multiSelect.StoredIndices)
            {
                var curNode = container.DummyPoly_RootNode.Children[entry];
                IgnoreHierarchyFocus = true;
                Editor._selection.AddSelection(Editor, curNode);
            }
        }
        else
        {

            var curNode = container.DummyPoly_RootNode.Children[index];
            IgnoreHierarchyFocus = true;
            Editor._selection.ClearSelection(Editor);
            Editor._selection.AddSelection(Editor, curNode);
        }
    }

    public void SelectRepresentativeNode(int index)
    {
        // Required to stop the LowRequirements build from failing
        if (Smithbox.LowRequirementsMode)
            return;

        if (!HasValidLoadedContainer())
            return;

        var container = Editor._universe.LoadedModelContainer;

        if (index > container.Bone_RootNode.Children.Count - 1)
            return;

        var curNode = container.Bone_RootNode.Children[index];
        IgnoreHierarchyFocus = true;
        Editor._selection.ClearSelection(Editor);
        Editor._selection.AddSelection(Editor, curNode);
    }
    public void SelectRepresentativeMesh(int index)
    {
        // Required to stop the LowRequirements build from failing
        if (Smithbox.LowRequirementsMode)
            return;

        if (!HasValidLoadedContainer())
            return;

        var container = Editor._universe.LoadedModelContainer;

        if (index > container.Mesh_RootNode.Children.Count - 1)
            return;

        var curMesh = container.Mesh_RootNode.Children[index];
        IgnoreHierarchyFocus = true;
        Editor._selection.ClearSelection(Editor);
        Editor._selection.AddSelection(Editor, curMesh);
    }

    public void DisplayRepresentativeDummyState(int index)
    {
        // Required to stop the LowRequirements build from failing
        if (Smithbox.LowRequirementsMode)
            return;

        if (!HasValidLoadedContainer())
            return;

        var container = Editor._universe.LoadedModelContainer;

        if (index > container.DummyPoly_RootNode.Children.Count - 1)
            return;

        Entity curEntity = null;

        var curNode = container.DummyPoly_RootNode.Children[index];
        curEntity = curNode;

        if (curEntity != null)
        {
            ImGui.SetNextItemAllowOverlap();
            var isVisible = curEntity.EditorVisible;
            ImGui.SameLine();
            ImGui.SetCursorPosX(ImGui.GetContentRegionAvail().X - 18.0f * DPI.GetUIScale());
            ImGui.PushStyleColor(ImGuiCol.Text, isVisible
                ? new Vector4(1.0f, 1.0f, 1.0f, 1.0f)
                : new Vector4(0.6f, 0.6f, 0.6f, 1.0f));
            ImGui.TextWrapped(isVisible ? Icons.Eye : Icons.EyeSlash);
            ImGui.PopStyleColor();

            if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
            {
                // Quick-tool all if this key is down
                if (InputTracker.GetKey(KeyBindings.Current.MODEL_ToggleVisibility))
                {
                    for (int i = 0; i < container.DummyPoly_RootNode.Children.Count; i++)
                    {
                        ToggleRepresentativeDummy(i);
                    }
                }
                // Otherwise just toggle this row
                else
                {
                    ToggleRepresentativeDummy(index);
                }
            }
        }
    }

    public void ToggleRepresentativeDummy(int index)
    {
        // Required to stop the LowRequirements build from failing
        if (Smithbox.LowRequirementsMode)
            return;

        if (!HasValidLoadedContainer())
            return;

        var container = Editor._universe.LoadedModelContainer;

        if (index > container.DummyPoly_RootNode.Children.Count - 1)
            return;

        var curNode = container.DummyPoly_RootNode.Children[index];
        curNode.EditorVisible = !curNode.EditorVisible;
    }

    public void DisplayRepresentativeNodeState(int index)
    {
        // Required to stop the LowRequirements build from failing
        if (Smithbox.LowRequirementsMode)
            return;

        if (!HasValidLoadedContainer())
            return;

        var container = Editor._universe.LoadedModelContainer;

        if (index > container.Bone_RootNode.Children.Count - 1)
            return;

        Entity curEntity = null;

        var curNode = container.Bone_RootNode.Children[index];
        curEntity = curNode;

        if (curEntity != null)
        {
            ImGui.SetNextItemAllowOverlap();
            var isVisible = curEntity.EditorVisible;
            ImGui.SameLine();
            ImGui.SetCursorPosX(ImGui.GetContentRegionAvail().X - 18.0f * DPI.GetUIScale());
            ImGui.PushStyleColor(ImGuiCol.Text, isVisible
                ? new Vector4(1.0f, 1.0f, 1.0f, 1.0f)
                : new Vector4(0.6f, 0.6f, 0.6f, 1.0f));
            ImGui.TextWrapped(isVisible ? Icons.Eye : Icons.EyeSlash);
            ImGui.PopStyleColor();

            if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
            {
                // Quick-tool all if this key is down
                if (InputTracker.GetKey(KeyBindings.Current.MODEL_ToggleVisibility))
                {
                    for (int i = 0; i < container.Bone_RootNode.Children.Count; i++)
                    {
                        ToggleRepresentativeNode(i);
                    }
                }
                // Otherwise just toggle this row
                else
                {
                    ToggleRepresentativeNode(index);
                }
            }
        }
    }

    public void ToggleRepresentativeNode(int index)
    {
        // Required to stop the LowRequirements build from failing
        if (Smithbox.LowRequirementsMode)
            return;

        if (!HasValidLoadedContainer())
            return;

        var container = Editor._universe.LoadedModelContainer;

        if (index > container.Bone_RootNode.Children.Count - 1)
            return;

        var curNode = container.Bone_RootNode.Children[index];
        curNode.EditorVisible = !curNode.EditorVisible;
    }
    public void DisplayRepresentativeMeshState(int index)
    {
        // Required to stop the LowRequirements build from failing
        if (Smithbox.LowRequirementsMode)
            return;

        if (!HasValidLoadedContainer())
            return;

        var container = Editor._universe.LoadedModelContainer;

        if (index > container.Mesh_RootNode.Children.Count - 1)
            return;

        Entity curEntity = null;

        var curMesh = container.Mesh_RootNode.Children[index];
        curEntity = curMesh;

        if (curEntity != null)
        {
            ImGui.SetNextItemAllowOverlap();
            var isVisible = curEntity.EditorVisible;
            ImGui.SameLine();
            ImGui.SetCursorPosX(ImGui.GetContentRegionAvail().X - 18.0f * DPI.GetUIScale());
            ImGui.PushStyleColor(ImGuiCol.Text, isVisible
                ? new Vector4(1.0f, 1.0f, 1.0f, 1.0f)
                : new Vector4(0.6f, 0.6f, 0.6f, 1.0f));
            ImGui.TextWrapped(isVisible ? Icons.Eye : Icons.EyeSlash);
            ImGui.PopStyleColor();

            if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
            {
                // Quick-tool all if this key is down
                if (InputTracker.GetKey(KeyBindings.Current.MODEL_ToggleVisibility))
                {
                    for (int i = 0; i < container.Mesh_RootNode.Children.Count; i++)
                    {
                        ToggleRepresentativeMesh(i);
                    }
                }
                // Otherwise just toggle this row
                else
                {
                    ToggleRepresentativeMesh(index);
                }
            }
        }
    }

    public void ToggleRepresentativeMesh(int index)
    {
        // Required to stop the LowRequirements build from failing
        if (Smithbox.LowRequirementsMode)
            return;

        if (!HasValidLoadedContainer())
            return;

        var container = Editor._universe.LoadedModelContainer;

        if (index > container.Mesh_RootNode.Children.Count - 1)
            return;

        var curMesh = container.Mesh_RootNode.Children[index];
        curMesh.EditorVisible = !curMesh.EditorVisible;
    }

    public void OnRepresentativeEntitySelected(Entity ent)
    {
        // Required to stop the LowRequirements build from failing
        if (Smithbox.LowRequirementsMode)
            return;

        if (!HasValidLoadedContainer())
            return;

        if (!IsSelectableNode(ent))
            return;

        if (IsUpdatingViewportModel)
            return;

        // Dummies
        if (ent.WrappedObject is FLVER.Dummy)
        {
            TransformableNamedEntity transformEnt = (TransformableNamedEntity)ent;

            Editor.Selection._selectedFlverGroupType = GroupSelectionType.Dummy;
            Editor.Selection._selectedDummy = transformEnt.Index;

            if (IgnoreHierarchyFocus)
            {
                IgnoreHierarchyFocus = false;
            }
            else
            {
                Editor.Selection.FocusSelection = true;
            }
        }
        // Bones
        if (ent.WrappedObject is FLVER.Node)
        {
            TransformableNamedEntity transformEnt = (TransformableNamedEntity)ent;

            Editor.Selection._selectedFlverGroupType = GroupSelectionType.Node;
            Editor.Selection._selectedNode = transformEnt.Index;

            if (IgnoreHierarchyFocus)
            {
                IgnoreHierarchyFocus = false;
            }
            else
            {
                Editor.Selection.FocusSelection = true;
            }
        }
        // Mesh
        if (ent.WrappedObject is FLVER2.Mesh)
        {
            NamedEntity namedEnt = (NamedEntity)ent;

            Editor.Selection._selectedFlverGroupType = GroupSelectionType.Mesh;
            Editor.Selection._selectedMesh = namedEnt.Index;

            if (IgnoreHierarchyFocus)
            {
                IgnoreHierarchyFocus = false;
            }
            else
            {
                Editor.Selection.FocusSelection = true;
            }
        }
    }

    public void OnRepresentativeEntityDeselected(Entity ent)
    {
        // Required to stop the LowRequirements build from failing
        if (Smithbox.LowRequirementsMode)
            return;

        if (!HasValidLoadedContainer())
            return;

        if (!IsSelectableNode(ent))
            return;

        if (IsUpdatingViewportModel)
            return;
    }

    public void OnRepresentativeEntityUpdate(Entity ent)
    {
        // Required to stop the LowRequirements build from failing
        if (Smithbox.LowRequirementsMode)
            return;

        if (!HasValidLoadedContainer())
            return;

        if (!IsSelectableNode(ent))
            return;

        if (IsUpdatingViewportModel)
            return;

        if (ent.WrappedObject is FLVER.Dummy or FLVER.Node)
        {
            TransformableNamedEntity transformEnt = (TransformableNamedEntity)ent;

            // Dummies
            if (transformEnt.WrappedObject is FLVER.Dummy)
            {
                UpdateStoredDummyPosition(transformEnt);
            }
            // Bones
            if (transformEnt.WrappedObject is FLVER.Node)
            {
                UpdateStoredNodeTransform(transformEnt);
            }
        }
    }

    private void UpdateStoredDummyPosition(TransformableNamedEntity transformEnt)
    {
        if (!HasValidLoadedContainer())
            return;

        if (Editor.Selection._selectedDummy == -1)
            return;

        var dummy = Editor.ResManager.GetCurrentFLVER().Dummies[Editor.Selection._selectedDummy];
        var entDummy = (FLVER.Dummy)transformEnt.WrappedObject;

        if (dummy.Position != entDummy.Position)
        {
            dummy.Position = entDummy.Position;
        }
    }

    private void UpdateStoredNodeTransform(TransformableNamedEntity transformEnt)
    {
        if (!HasValidLoadedContainer())
            return;

        if (Editor.Selection._selectedNode == -1)
            return;

        var bone = Editor.ResManager.GetCurrentFLVER().Nodes[Editor.Selection._selectedNode];
        var entBone = (FLVER.Node)transformEnt.WrappedObject;

        if (bone.Translation != entBone.Translation)
        {
            bone.Translation = entBone.Translation;
        }
    }

    public bool IsSelectableNode(Entity ent)
    {
        if (ent is TransformableNamedEntity or NamedEntity)
        {
            return true;
        }

        return false;
    }
}
