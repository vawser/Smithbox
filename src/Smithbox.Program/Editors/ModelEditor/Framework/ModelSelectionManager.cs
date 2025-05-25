using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editor.Multiselection;
using StudioCore.Editors.ModelEditor.Enums;
using System.Numerics;

namespace StudioCore.Editors.ModelEditor;

public class ModelSelectionManager
{
    private ModelEditorScreen Editor;
    private ModelViewportManager ViewportManager;

    public string _selectedFileName = "";
    public string _selectedAssociatedMapID = "";
    public FileSelectionType _selectedFileModelType = FileSelectionType.None;

    public GroupSelectionType _selectedFlverGroupType = GroupSelectionType.None;
    public string _selectedEntry = "";
    public int _selectedDummy = -1;
    public int _selectedMaterial = -1;
    public int _selectedGXList = -1;
    public int _selectedNode = -1;
    public int _selectedMesh = -1;
    public int _selectedBufferLayout = -1;
    public int _selectedBaseSkeletonBone = -1;
    public int _selectedAllSkeletonBone = -1;
    public int _selectedLowCollision = -1;
    public int _selectedHighCollision = -1;

    public int _subSelectedTextureRow = -1;
    public int _subSelectedGXItemRow = -1;
    public int _subSelectedFaceSetRow = -1;
    public int _subSelectedVertexBufferRow = -1;
    public int _subSelectedBufferLayoutMember = -1;

    public Vector3 _trackedDummyPosition = new Vector3();
    public Vector3 _trackedNodeTranslation = new Vector3();

    public bool FocusSelection = false;
    public bool SelectDummy = false;
    public bool SelectMaterial = false;
    public bool SelectGxList = false;
    public bool SelectNode = false;
    public bool SelectMesh = false;
    public bool SelectBuffer = false;
    public bool SelectBaseSkeleton = false;
    public bool SelectAllSkeleton = false;
    public bool SelectLowCollision = false;
    public bool SelectHighCollision = false;

    public bool ForceOpenMaterialSection = false;
    public bool ForceOpenGXListSection = false;
    public bool ForceOpenNodeSection = false;
    public bool ForceOpenBufferLayoutSection = false;

    public Multiselection DummyMultiselect;
    public Multiselection MaterialMultiselect;
    public Multiselection GxListMultiselect;
    public Multiselection NodeMultiselect;
    public Multiselection MeshMultiselect;
    public Multiselection BufferLayoutMultiselect;
    public Multiselection BaseSkeletonMultiselect;
    public Multiselection AllSkeletonMultiselect;

    private KeyBind MultiSelectKey = KeyBindings.Current.MODEL_Multiselect;

    public ModelSelectionManager(ModelEditorScreen screen)
    {
        Editor = screen;
        ViewportManager = screen.ViewportManager;

        DummyMultiselect = new Multiselection(MultiSelectKey);
        MaterialMultiselect = new Multiselection(MultiSelectKey);
        GxListMultiselect = new Multiselection(MultiSelectKey);
        NodeMultiselect = new Multiselection(MultiSelectKey);
        MeshMultiselect = new Multiselection(MultiSelectKey);
        BufferLayoutMultiselect = new Multiselection(MultiSelectKey);
        BaseSkeletonMultiselect = new Multiselection(MultiSelectKey);
        AllSkeletonMultiselect = new Multiselection(MultiSelectKey);
    }

    /// <summary>
    /// Reset selection state on project change
    /// </summary>
    public void OnProjectChanged()
    {
        ResetSelection();
        ResetMultiSelection();
    }

    /// <summary>
    /// Reset the current individual selection
    /// </summary>
    public void ResetSelection()
    {
        _selectedEntry = "";
        _selectedFlverGroupType = GroupSelectionType.None;
        _selectedDummy = -1;
        _selectedMaterial = -1;
        _selectedGXList = -1;
        _selectedNode = -1;
        _selectedMesh = -1;
        _selectedBufferLayout = -1;
        _selectedBaseSkeletonBone = -1;
        _selectedAllSkeletonBone = -1;
        _subSelectedTextureRow = -1;
        _subSelectedGXItemRow = -1;
        _subSelectedFaceSetRow = -1;
        _subSelectedVertexBufferRow = -1;
        _subSelectedBufferLayoutMember = -1;
        _selectedLowCollision = -1;
        _selectedHighCollision = -1;
    }

    /// <summary>
    /// Reset the current multi-selections
    /// </summary>
    public void ResetMultiSelection()
    {
        DummyMultiselect = new Multiselection(MultiSelectKey);
        MaterialMultiselect = new Multiselection(MultiSelectKey);
        GxListMultiselect = new Multiselection(MultiSelectKey);
        NodeMultiselect = new Multiselection(MultiSelectKey);
        MeshMultiselect = new Multiselection(MultiSelectKey);
        BufferLayoutMultiselect = new Multiselection(MultiSelectKey);
        BaseSkeletonMultiselect = new Multiselection(MultiSelectKey);
        AllSkeletonMultiselect = new Multiselection(MultiSelectKey);
    }

    /// <summary>
    /// Set the new header selection
    /// </summary>
    public void SetHeaderSelection()
    {
        _selectedEntry = "Header";
        _selectedFlverGroupType = GroupSelectionType.Header;
    }

    /// <summary>
    /// Check if a dummy poly entry is selected
    /// </summary>
    public bool IsDummySelected(int index)
    {
        if (DummyMultiselect.IsMultiselected(index) || _selectedDummy == index)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Set the new dummy polygon selection
    /// </summary>
    public void SetDummySelection(int index)
    {
        DummyMultiselect.HandleMultiselect(_selectedDummy, index);

        ResetSelection();
        _selectedDummy = index;
        _selectedFlverGroupType = GroupSelectionType.Dummy;

        _trackedDummyPosition = new Vector3();
        ViewportManager.SelectRepresentativeDummy(_selectedDummy, DummyMultiselect);
    }

    /// <summary>
    /// Check if a material entry is selected
    /// </summary>
    public bool IsMaterialSelected(int index)
    {
        if (MaterialMultiselect.IsMultiselected(index) || _selectedMaterial == index)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Set the new material selection
    /// </summary>
    public void SetMaterialSelection(int index, FLVER2.Material curMaterial)
    {
        MaterialMultiselect.HandleMultiselect(_selectedMaterial, index);

        ResetSelection();
        _selectedMaterial = index;
        _selectedFlverGroupType = GroupSelectionType.Material;

        if (curMaterial.Textures.Count > 0)
        {
            _subSelectedTextureRow = 0;
        }
    }

    /// <summary>
    /// Check if a GX list entry is selected
    /// </summary>
    public bool IsGxListSelected(int index)
    {
        if (GxListMultiselect.IsMultiselected(index) || _selectedGXList == index)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Set the new GX list selection
    /// </summary>
    public void SetGxListSelection(int index, FLVER2.GXList curGXList)
    {
        GxListMultiselect.HandleMultiselect(_selectedGXList, index);

        ResetSelection();
        _selectedGXList = index;
        _selectedFlverGroupType = GroupSelectionType.GXList;

        if (curGXList.Count > 0)
        {
            _subSelectedGXItemRow = 0;
        }
    }

    /// <summary>
    /// Check if a Node list entry is selected
    /// </summary>
    public bool IsNodeSelection(int index)
    {
        if (NodeMultiselect.IsMultiselected(index) || _selectedNode == index)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Set the new Node selection
    /// </summary>
    public void SetNodeSelection(int index)
    {
        NodeMultiselect.HandleMultiselect(_selectedNode, index);

        ResetSelection();
        _selectedNode = index;
        _selectedFlverGroupType = GroupSelectionType.Node;

        _trackedNodeTranslation = new Vector3();
        ViewportManager.SelectRepresentativeNode(_selectedNode);
    }

    /// <summary>
    /// Check if a Mesh list entry is selected
    /// </summary>
    public bool IsMeshSelection(int index)
    {
        if (MeshMultiselect.IsMultiselected(index) || _selectedMesh == index)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Set the new Mesh selection
    /// </summary>
    public void SetMeshSelection(int index, FLVER2.Mesh curMesh)
    {
        MeshMultiselect.HandleMultiselect(_selectedMesh, index);

        ResetSelection();
        _selectedMesh = index;
        _selectedFlverGroupType = GroupSelectionType.Mesh;

        if (curMesh.FaceSets.Count > 0)
        {
            _subSelectedFaceSetRow = 0;
        }

        if (curMesh.VertexBuffers.Count > 0)
        {
            _subSelectedVertexBufferRow = 0;
        }

        ViewportManager.SelectRepresentativeMesh(_selectedMesh);
    }

    /// <summary>
    /// Check if a Buffer Layout list entry is selected
    /// </summary>
    public bool IsBufferLayoutSelection(int index)
    {
        if (MeshMultiselect.IsMultiselected(index) || _selectedBufferLayout == index)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Set the new Mesh selection
    /// </summary>
    public void SetBufferLayoutSelection(int index, FLVER2.BufferLayout curLayout)
    {
        BufferLayoutMultiselect.HandleMultiselect(_selectedBufferLayout, index);

        ResetSelection();
        _selectedBufferLayout = index;
        _selectedFlverGroupType = GroupSelectionType.BufferLayout;

        if (curLayout.Count > 0)
        {
            _subSelectedBufferLayoutMember = 0;
        }
    }

    /// <summary>
    /// Check if a Base Skeleton list entry is selected
    /// </summary>
    public bool IsBaseSkeletonSelection(int index)
    {
        if (BaseSkeletonMultiselect.IsMultiselected(index) || _selectedBaseSkeletonBone == index)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Set the new Base Skeleton selection
    /// </summary>
    public void SetBaseSkeletonSelection(int index)
    {
        BaseSkeletonMultiselect.HandleMultiselect(_selectedBaseSkeletonBone, index);

        ResetSelection();
        _selectedBaseSkeletonBone = index;
        _selectedFlverGroupType = GroupSelectionType.BaseSkeleton;
    }

    /// <summary>
    /// Check if a All Skeleton list entry is selected
    /// </summary>
    public bool IsAllSkeletonSelection(int index)
    {
        if (AllSkeletonMultiselect.IsMultiselected(index) || _selectedAllSkeletonBone == index)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Set the new All Skeleton selection
    /// </summary>
    public void SetAllSkeletonSelection(int index)
    {
        AllSkeletonMultiselect.HandleMultiselect(_selectedAllSkeletonBone, index);

        ResetSelection();
        _selectedAllSkeletonBone = index;
        _selectedFlverGroupType = GroupSelectionType.AllSkeleton;
    }

    /// <summary>
    /// Check if a Low Collision list entry is selected
    /// </summary>
    public bool IsLowCollisionSelection(int index)
    {
        if (_selectedLowCollision == index)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Set the new Low Collision selection
    /// </summary>
    public void SetLowCollisionSelection(int index)
    {
        ResetSelection();
        _selectedLowCollision = index;
        _selectedFlverGroupType = GroupSelectionType.CollisionLow;
    }

    /// <summary>
    /// Check if a High Collision list entry is selected
    /// </summary>
    public bool IsHighCollisionSelection(int index)
    {
        if (_selectedHighCollision == index)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Set the new High Collision selection
    /// </summary>
    public void SetHighCollisionSelection(int index)
    {
        ResetSelection();
        _selectedHighCollision = index;
        _selectedFlverGroupType = GroupSelectionType.CollisionHigh;
    }

    public ModelEditorContext CurrentWindowContext = ModelEditorContext.None;

    /// <summary>
    /// Switches the focus context to the passed value.
    /// Use this on all windows (e.g. both Begin and BeginChild)
    /// </summary>
    public void SwitchWindowContext(ModelEditorContext newContext)
    {
        if (ImGui.IsWindowHovered())
        {
            CurrentWindowContext = newContext;
            //TaskLogs.AddLog($"Context: {newContext.GetDisplayName()}");
        }
    }
}
