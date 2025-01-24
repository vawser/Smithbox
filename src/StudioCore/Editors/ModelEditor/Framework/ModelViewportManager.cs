using HKLib.hk2018.hkHashMapDetail;
using ImGuiNET;
using Microsoft.AspNetCore.Mvc.Rendering;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Editor;
using StudioCore.Editor.Multiselection;
using StudioCore.Editors.MapEditor.Framework;
using StudioCore.Editors.ModelEditor.Actions.Viewport;
using StudioCore.Editors.ModelEditor.Enums;
using StudioCore.Interface;
using StudioCore.MsbEditor;
using StudioCore.Resource;
using StudioCore.Resource.Locators;
using StudioCore.Resource.Types;
using StudioCore.Scene;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Veldrid.Utilities;

namespace StudioCore.Editors.ModelEditor;

public class ModelViewportManager
{
    public ModelEditorScreen Screen;
    public Universe Universe;

    public IViewport Viewport;

    public bool IsUpdatingViewportModel = false;
    public bool IgnoreHierarchyFocus = false;

    public ModelViewportManager(ModelEditorScreen screen, IViewport viewport)
    {
        Screen = screen;
        Universe = screen._universe;
        Viewport = viewport;
    }

    public bool HasValidLoadedContainer()
    {
        if (Universe.LoadedModelContainer == null)
            return false;

        return true;
    }

    public void UpdateRepresentativeModel(int selectionIndex)
    {
        IsUpdatingViewportModel = true;

        Screen._selection.ClearSelection();

        UpdateRepresentativeModel();

        if (Screen.Selection._selectedFlverGroupType == GroupSelectionType.Dummy)
        {
            SelectViewportDummy(selectionIndex, Screen._universe.LoadedModelContainer.DummyPoly_RootNode);
        }
        if (Screen.Selection._selectedFlverGroupType == GroupSelectionType.Node)
        {
            SelectViewportDummy(selectionIndex, Screen._universe.LoadedModelContainer.Bone_RootNode);
        }
        if (Screen.Selection._selectedFlverGroupType == GroupSelectionType.Mesh)
        {
            SelectViewportDummy(selectionIndex, Screen._universe.LoadedModelContainer.Mesh_RootNode);
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
                    Screen._selection.AddSelection(entry);
                }
                idx++;
            }
        }
    }

    public void UpdateRepresentativeModel()
    {
        var currentInfo = Screen.ResManager.LoadedFlverContainer;

        var containerId = currentInfo.ContainerName;
        var modelId = currentInfo.ContainerName;
        var modelType = currentInfo.Type;
        var mapId = currentInfo.MapID;

        Screen.ResManager.LoadRepresentativeModel(containerId, modelId, modelType, mapId);
    }

    public void UpdateRepresentativeDummy(int index, Vector3 position)
    {
        // Required to stop the LowRequirements build from failing
        if (Smithbox.LowRequirementsMode)
            return;

        if (!HasValidLoadedContainer())
            return;

        var container = Screen._universe.LoadedModelContainer;

        if (index > container.DummyPoly_RootNode.Children.Count - 1)
            return;

        var curNode = container.DummyPoly_RootNode.Children[index];
        ChangeVisualDummyTransform act = new(curNode, position);
        Screen.EditorActionManager.ExecuteAction(act);
    }

    public void UpdateRepresentativeNode(int index, Vector3 position, Vector3 rotation, Vector3 scale)
    {
        // Required to stop the LowRequirements build from failing
        if (Smithbox.LowRequirementsMode)
            return;

        if (!HasValidLoadedContainer())
            return;

        var container = Screen._universe.LoadedModelContainer;

        if (index > container.Bone_RootNode.Children.Count - 1)
            return;

        var curNode = container.Bone_RootNode.Children[index];
        ChangeVisualNodeTransform act = new(curNode, position, rotation, scale);
        Screen.EditorActionManager.ExecuteAction(act);
    }

    public void SelectRepresentativeDummy(int index, Multiselection multiSelect)
    {
        // Required to stop the LowRequirements build from failing
        if (Smithbox.LowRequirementsMode)
            return;

        if (!HasValidLoadedContainer())
            return;

        var container = Screen._universe.LoadedModelContainer;

        if (index > container.DummyPoly_RootNode.Children.Count - 1)
            return;

        if (multiSelect.HasValidMultiselection())
        {
            Screen._selection.ClearSelection();

            foreach (var entry in multiSelect.StoredIndices)
            {
                var curNode = container.DummyPoly_RootNode.Children[entry];
                IgnoreHierarchyFocus = true;
                Screen._selection.AddSelection(curNode);
            }
        }
        else
        {

            var curNode = container.DummyPoly_RootNode.Children[index];
            IgnoreHierarchyFocus = true;
            Screen._selection.ClearSelection();
            Screen._selection.AddSelection(curNode);
        }
    }

    public void SelectRepresentativeNode(int index)
    {
        // Required to stop the LowRequirements build from failing
        if (Smithbox.LowRequirementsMode)
            return;

        if (!HasValidLoadedContainer())
            return;

        var container = Screen._universe.LoadedModelContainer;

        if (index > container.Bone_RootNode.Children.Count - 1)
            return;

        var curNode = container.Bone_RootNode.Children[index];
        IgnoreHierarchyFocus = true;
        Screen._selection.ClearSelection();
        Screen._selection.AddSelection(curNode);
    }
    public void SelectRepresentativeMesh(int index)
    {
        // Required to stop the LowRequirements build from failing
        if (Smithbox.LowRequirementsMode)
            return;

        if (!HasValidLoadedContainer())
            return;

        var container = Screen._universe.LoadedModelContainer;

        if (index > container.Mesh_RootNode.Children.Count - 1)
            return;

        var curMesh = container.Mesh_RootNode.Children[index];
        IgnoreHierarchyFocus = true;
        Screen._selection.ClearSelection();
        Screen._selection.AddSelection(curMesh);
    }

    public void DisplayRepresentativeDummyState(int index)
    {
        // Required to stop the LowRequirements build from failing
        if (Smithbox.LowRequirementsMode)
            return;

        if (!HasValidLoadedContainer())
            return;

        var container = Screen._universe.LoadedModelContainer;

        if (index > container.DummyPoly_RootNode.Children.Count - 1)
            return;

        Entity curEntity = null;

        var curNode = container.DummyPoly_RootNode.Children[index];
        curEntity = curNode;

        if (curEntity != null)
        {
            ImGui.SetItemAllowOverlap();
            var isVisible = curEntity.EditorVisible;
            ImGui.SameLine();
            ImGui.SetCursorPosX(ImGui.GetWindowContentRegionMax().X - 18.0f * DPI.GetUIScale());
            ImGui.PushStyleColor(ImGuiCol.Text, isVisible
                ? new Vector4(1.0f, 1.0f, 1.0f, 1.0f)
                : new Vector4(0.6f, 0.6f, 0.6f, 1.0f));
            ImGui.TextWrapped(isVisible ? ForkAwesome.Eye : ForkAwesome.EyeSlash);
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

        var container = Screen._universe.LoadedModelContainer;

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

        var container = Screen._universe.LoadedModelContainer;

        if (index > container.Bone_RootNode.Children.Count - 1)
            return;

        Entity curEntity = null;

        var curNode = container.Bone_RootNode.Children[index];
        curEntity = curNode;

        if (curEntity != null)
        {
            ImGui.SetItemAllowOverlap();
            var isVisible = curEntity.EditorVisible;
            ImGui.SameLine();
            ImGui.SetCursorPosX(ImGui.GetWindowContentRegionMax().X - 18.0f * DPI.GetUIScale());
            ImGui.PushStyleColor(ImGuiCol.Text, isVisible
                ? new Vector4(1.0f, 1.0f, 1.0f, 1.0f)
                : new Vector4(0.6f, 0.6f, 0.6f, 1.0f));
            ImGui.TextWrapped(isVisible ? ForkAwesome.Eye : ForkAwesome.EyeSlash);
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

        var container = Screen._universe.LoadedModelContainer;

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

        var container = Screen._universe.LoadedModelContainer;

        if (index > container.Mesh_RootNode.Children.Count - 1)
            return;

        Entity curEntity = null;

        var curMesh = container.Mesh_RootNode.Children[index];
        curEntity = curMesh;

        if (curEntity != null)
        {
            ImGui.SetItemAllowOverlap();
            var isVisible = curEntity.EditorVisible;
            ImGui.SameLine();
            ImGui.SetCursorPosX(ImGui.GetWindowContentRegionMax().X - 18.0f * DPI.GetUIScale());
            ImGui.PushStyleColor(ImGuiCol.Text, isVisible
                ? new Vector4(1.0f, 1.0f, 1.0f, 1.0f)
                : new Vector4(0.6f, 0.6f, 0.6f, 1.0f));
            ImGui.TextWrapped(isVisible ? ForkAwesome.Eye : ForkAwesome.EyeSlash);
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

        var container = Screen._universe.LoadedModelContainer;

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

            Screen.Selection._selectedFlverGroupType = GroupSelectionType.Dummy;
            Screen.Selection._selectedDummy = transformEnt.Index;

            if (IgnoreHierarchyFocus)
            {
                IgnoreHierarchyFocus = false;
            }
            else
            {
                Screen.Selection.FocusSelection = true;
            }
        }
        // Bones
        if (ent.WrappedObject is FLVER.Node)
        {
            TransformableNamedEntity transformEnt = (TransformableNamedEntity)ent;

            Screen.Selection._selectedFlverGroupType = GroupSelectionType.Node;
            Screen.Selection._selectedNode = transformEnt.Index;

            if (IgnoreHierarchyFocus)
            {
                IgnoreHierarchyFocus = false;
            }
            else
            {
                Screen.Selection.FocusSelection = true;
            }
        }
        // Mesh
        if (ent.WrappedObject is FLVER2.Mesh)
        {
            NamedEntity namedEnt = (NamedEntity)ent;

            Screen.Selection._selectedFlverGroupType = GroupSelectionType.Mesh;
            Screen.Selection._selectedMesh = namedEnt.Index;

            if (IgnoreHierarchyFocus)
            {
                IgnoreHierarchyFocus = false;
            }
            else
            {
                Screen.Selection.FocusSelection = true;
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

        if (Screen.Selection._selectedDummy == -1)
            return;

        var dummy = Screen.ResManager.GetCurrentFLVER().Dummies[Screen.Selection._selectedDummy];
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

        if (Screen.Selection._selectedNode == -1)
            return;

        var bone = Screen.ResManager.GetCurrentFLVER().Nodes[Screen.Selection._selectedNode];
        var entBone = (FLVER.Node)transformEnt.WrappedObject;

        if (bone.Position != entBone.Position)
        {
            bone.Position = entBone.Position;
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
