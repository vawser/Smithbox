using ImGuiNET;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Gui;
using StudioCore.Scene;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Veldrid;
using StudioCore.MsbEditor;
using StudioCore.Editors.MapEditor;
using StudioCore.Editor;
using StudioCore.Core;
using StudioCore.Interface;

namespace StudioCore.Editors.ModelEditor;

public class ModelHierarchyView
{
    private ModelEditorScreen Screen;

    private string _searchInput = "";

    public bool SuspendView = false;
    public bool FocusSelection = false;

    public ModelHierarchyView(ModelEditorScreen editor)
    {
        Screen = editor;
    }

    public void OnGui()
    {
        var scale = Smithbox.GetUIScale();

        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        if (!CFG.Current.Interface_ModelEditor_ModelHierarchy)
            return;

        ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * scale, ImGuiCond.FirstUseEver);

        if (ImGui.Begin($@"Model Hierarchy##ModelEditorModelHierarchy"))
        {
            ImGui.InputText($"Search", ref _searchInput, 255);
            ImguiUtils.ShowHoverTooltip("Separate terms are split via the + character.");

            if (Screen.ResourceHandler.CurrentFLVER != null && !SuspendView)
            {
                DisplaySection_Header();
                DisplaySection_Dummies();
                DisplaySection_Materials();
                DisplaySection_GXLists();
                DisplaySection_Nodes();
                DisplaySection_Meshes();
                DisplaySection_BufferLayouts();
                DisplaySection_Skeletons();
            }
        }

        ImGui.End();
        ImGui.PopStyleColor(1);
    }

    public void OnProjectChanged()
    {
        if (Smithbox.ProjectType != ProjectType.Undefined)
        {
        }
    }

    public ModelEntrySelectionType _lastSelectedEntry = ModelEntrySelectionType.None;
    private string _selectedEntry = "";
    public int _selectedDummy = -1;
    public int _selectedMaterial = -1;
    public int _selectedGXList = -1;
    public int _selectedNode = -1;
    public int _selectedMesh = -1;
    public int _selectedBufferLayout = -1;
    public int _selectedBaseSkeleton = -1;
    public int _selectedAllSkeleton = -1;

    public void ResetSelection()
    {
        _selectedEntry = "";
        _lastSelectedEntry = ModelEntrySelectionType.None;
        _selectedDummy = -1;
        _selectedMaterial = -1;
        _selectedGXList = -1;
        _selectedNode = -1;
        _selectedMesh = -1;
        _selectedBufferLayout = -1;
        _selectedBaseSkeleton = -1;
        _selectedAllSkeleton = -1;
    }


    private void DisplaySection_Header()
    {
        if(ImGui.Selectable("Header", _selectedEntry == "Header"))
        {
            ResetSelection();
            _selectedEntry = "Header";
            _lastSelectedEntry = ModelEntrySelectionType.Header;
        }
    }

    private void DisplaySection_Dummies()
    {
        if (ImGui.CollapsingHeader("Dummies"))
        {
            for(int i = 0; i < Screen.ResourceHandler.CurrentFLVER.Dummies.Count; i++)
            {
                var curDummy = Screen.ResourceHandler.CurrentFLVER.Dummies[i];

                if (ImGui.Selectable($"Dummy {i} - [{curDummy.ReferenceID}]", _selectedDummy == i, ImGuiSelectableFlags.AllowDoubleClick))
                {
                    ResetSelection();
                    _selectedDummy = i;
                    _lastSelectedEntry = ModelEntrySelectionType.Dummy;

                    Screen.ModelPropertyEditor._trackedDummyPosition = new Vector3();

                    if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                    {
                        Screen.ViewportHandler.SelectRepresentativeDummy(_selectedDummy);
                    }
                }

                Screen.ViewportHandler.DisplayRepresentativeDummyState(i);

                if (FocusSelection && _selectedDummy == i)
                {
                    FocusSelection = false;
                    ImGui.SetScrollHereY();
                }
            }
        }
    }

    public bool ForceOpenMaterialSection = false;

    private void DisplaySection_Materials()
    {
        if (ForceOpenMaterialSection)
        {
            ForceOpenMaterialSection = false;
            ImGui.SetNextItemOpen(true);
        }

        if (ImGui.CollapsingHeader("Materials"))
        {
            for (int i = 0; i < Screen.ResourceHandler.CurrentFLVER.Materials.Count; i++)
            {
                var materialName = Screen.ResourceHandler.CurrentFLVER.Materials[i].Name;

                if (ImGui.Selectable($"{materialName}##material{i}", _selectedMaterial == i))
                {
                    ResetSelection();
                    _selectedMaterial = i;
                    _lastSelectedEntry = ModelEntrySelectionType.Material;
                }

                if (FocusSelection && _selectedMaterial == i)
                {
                    FocusSelection = false;
                    ImGui.SetScrollHereY();
                }
            }
        }
    }

    public bool ForceOpenGXListSection = false;

    private void DisplaySection_GXLists()
    {
        if (ForceOpenGXListSection)
        {
            ForceOpenGXListSection = false;
            ImGui.SetNextItemOpen(true);
        }

        if (ImGui.CollapsingHeader("GX List"))
        {
            for (int i = 0; i < Screen.ResourceHandler.CurrentFLVER.GXLists.Count; i++)
            {
                if (ImGui.Selectable($"GX List {i}", _selectedGXList == i))
                {
                    ResetSelection();
                    _selectedGXList = i;
                    _lastSelectedEntry = ModelEntrySelectionType.GXList;
                }

                if (FocusSelection && _selectedGXList == i)
                {
                    FocusSelection = false;
                    ImGui.SetScrollHereY();
                }
            }
        }
    }

    public bool ForceOpenNodeSection = false;

    private void DisplaySection_Nodes()
    {
        if (ForceOpenNodeSection)
        {
            ForceOpenNodeSection = false;
            ImGui.SetNextItemOpen(true);
        }

        if (ImGui.CollapsingHeader("Nodes"))
        {
            for (int i = 0; i < Screen.ResourceHandler.CurrentFLVER.Nodes.Count; i++)
            {
                var curNode = Screen.ResourceHandler.CurrentFLVER.Nodes[i];

                if (ImGui.Selectable($"Node {i} - {curNode.Name}", _selectedNode == i, ImGuiSelectableFlags.AllowDoubleClick))
                {
                    ResetSelection();
                    _selectedNode = i;
                    _lastSelectedEntry = ModelEntrySelectionType.Node;

                    Screen.ModelPropertyEditor._trackedNodePosition = new Vector3();

                    if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                    {
                        Screen.ViewportHandler.SelectRepresentativeNode(_selectedNode);
                    }
                }

                Screen.ViewportHandler.DisplayRepresentativeNodeState(i);

                if (FocusSelection && _selectedNode == i)
                {
                    FocusSelection = false;
                    ImGui.SetScrollHereY();
                }
            }
        }
    }

    private void DisplaySection_Meshes()
    {
        if (ImGui.CollapsingHeader("Meshes"))
        {
            for (int i = 0; i < Screen.ResourceHandler.CurrentFLVER.Meshes.Count; i++)
            {
                var materialIndex = Screen.ResourceHandler.CurrentFLVER.Meshes[i].MaterialIndex;
                var nodeIndex = Screen.ResourceHandler.CurrentFLVER.Meshes[i].NodeIndex;

                var material = "";
                if (materialIndex < Screen.ResourceHandler.CurrentFLVER.Materials.Count)
                    material = Screen.ResourceHandler.CurrentFLVER.Materials[materialIndex].Name;

                var node = "";
                if (nodeIndex < Screen.ResourceHandler.CurrentFLVER.Nodes.Count)
                    node = Screen.ResourceHandler.CurrentFLVER.Nodes[nodeIndex].Name;

                if (ImGui.Selectable($"Mesh {i} - {material} : {node}", _selectedMesh == i, ImGuiSelectableFlags.AllowDoubleClick))
                {
                    ResetSelection();
                    _selectedMesh = i;
                    _lastSelectedEntry = ModelEntrySelectionType.Mesh;
                }

                Screen.ViewportHandler.DisplayRepresentativeMeshState(i);

                if (FocusSelection && _selectedMesh == i)
                {
                    FocusSelection = false;
                    ImGui.SetScrollHereY();
                }
            }
        }
    }

    public bool ForceOpenBufferLayoutSection = false;

    private void DisplaySection_BufferLayouts()
    {
        if (ForceOpenBufferLayoutSection)
        {
            ForceOpenBufferLayoutSection = false;
            ImGui.SetNextItemOpen(true);
        }

        if (ImGui.CollapsingHeader("Buffer Layout"))
        {
            for (int i = 0; i < Screen.ResourceHandler.CurrentFLVER.BufferLayouts.Count; i++)
            {
                if (ImGui.Selectable($"Buffer Layout {i}", _selectedBufferLayout == i))
                {
                    ResetSelection();
                    _selectedBufferLayout = i;
                    _lastSelectedEntry = ModelEntrySelectionType.BufferLayout;
                }

                if (FocusSelection && _selectedBufferLayout == i)
                {
                    FocusSelection = false;
                    ImGui.SetScrollHereY();
                }
            }
        }
    }

    private void DisplaySection_Skeletons()
    {
        if (ImGui.CollapsingHeader("Base Skeleton"))
        {
            for (int i = 0; i < Screen.ResourceHandler.CurrentFLVER.Skeletons.BaseSkeleton.Count; i++)
            {
                var nodeIndex = Screen.ResourceHandler.CurrentFLVER.Skeletons.BaseSkeleton[i].NodeIndex;

                var node = "";
                if (nodeIndex < Screen.ResourceHandler.CurrentFLVER.Nodes.Count)
                    node = Screen.ResourceHandler.CurrentFLVER.Nodes[nodeIndex].Name;

                if (ImGui.Selectable($"Base Skeleton {i} - {node}", _selectedBaseSkeleton == i))
                {
                    ResetSelection();
                    _selectedBaseSkeleton = i;
                    _lastSelectedEntry = ModelEntrySelectionType.BaseSkeleton;
                }

                if (FocusSelection && _selectedBaseSkeleton == i)
                {
                    FocusSelection = false;
                    ImGui.SetScrollHereY();
                }
            }
        }

        if (ImGui.CollapsingHeader("All Skeleton"))
        {
            for (int i = 0; i < Screen.ResourceHandler.CurrentFLVER.Skeletons.AllSkeletons.Count; i++)
            {
                var nodeIndex = Screen.ResourceHandler.CurrentFLVER.Skeletons.AllSkeletons[i].NodeIndex;

                var node = "";
                if (nodeIndex < Screen.ResourceHandler.CurrentFLVER.Nodes.Count)
                    node = Screen.ResourceHandler.CurrentFLVER.Nodes[nodeIndex].Name;

                if (ImGui.Selectable($"All Skeleton {i} - {node}", _selectedAllSkeleton == i))
                {
                    ResetSelection();
                    _selectedAllSkeleton = i;
                    _lastSelectedEntry = ModelEntrySelectionType.AllSkeleton;
                }

                if (FocusSelection && _selectedAllSkeleton == i)
                {
                    FocusSelection = false;
                    ImGui.SetScrollHereY();
                }
            }
        }
    }
}
