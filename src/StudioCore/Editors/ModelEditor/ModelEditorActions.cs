using DotNext.Collections.Generic;
using StudioCore.Editors.MapEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using SoulsFormats;
using static SoulsFormats.FLVER2;

namespace StudioCore.Editors.ModelEditor.Actions;

public class GroupAction : ViewportAction
{
    private readonly List<ViewportAction> Actions;

    private Action<bool> PostExecutionAction;

    public GroupAction(List<ViewportAction> actions)
    {
        Actions = actions;
    }

    public bool HasActions => Actions.Any();

    public void SetPostExecutionAction(Action<bool> action)
    {
        PostExecutionAction = action;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        var evt = ActionEvent.NoEvent;
        for (var i = 0; i < Actions.Count; i++)
        {
            ViewportAction act = Actions[i];
            if (act != null)
            {
                evt |= act.Execute();
            }
        }

        if (PostExecutionAction != null)
        {
            PostExecutionAction.Invoke(false);
        }

        return evt;
    }

    public override ActionEvent Undo()
    {
        var evt = ActionEvent.NoEvent;
        for (var i = Actions.Count - 1; i >= 0; i--)
        {
            ViewportAction act = Actions[i];
            if (act != null)
            {
                evt |= act.Undo();
            }
        }

        if (PostExecutionAction != null)
        {
            PostExecutionAction.Invoke(true);
        }

        return evt;
    }
}

// Viewport Representations
public class DummyPositionChange : ViewportAction
{
    private Entity Node;
    private Vector3 OldPosition;
    private Vector3 NewPosition;
    private FLVER.Dummy Dummy;

    public DummyPositionChange(Entity node, Vector3 newPosition)
    {
        Node = node;
        Dummy = (FLVER.Dummy)node.WrappedObject;
        OldPosition = Dummy.Position;
        NewPosition = newPosition;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Dummy.Position = NewPosition;
        Node.UpdateRenderModel();

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Dummy.Position = OldPosition;
        Node.UpdateRenderModel();

        return ActionEvent.NoEvent;
    }
}

public class BoneTransformChange : ViewportAction
{
    private Entity Node;
    private Vector3 OldPosition;
    private Vector3 NewPosition;
    private Vector3 OldRotation;
    private Vector3 NewRotation;
    private Vector3 OldScale;
    private Vector3 NewScale;
    private FLVER.Node BoneNode;

    public BoneTransformChange(Entity node, Vector3 newPosition, Vector3 newRotation, Vector3 newScale)
    {
        Node = node;
        BoneNode = (FLVER.Node)node.WrappedObject;
        OldPosition = BoneNode.Position;
        NewPosition = newPosition;
        OldRotation = BoneNode.Rotation;
        NewRotation = newRotation;
        OldScale = BoneNode.Scale;
        NewScale = newScale;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        BoneNode.Position = NewPosition;
        BoneNode.Rotation = NewRotation;
        BoneNode.Scale = NewScale;
        Node.UpdateRenderModel();

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        BoneNode.Position = OldPosition;
        BoneNode.Rotation = OldRotation;
        BoneNode.Scale = OldScale;
        Node.UpdateRenderModel();

        return ActionEvent.NoEvent;
    }
}

// Bit crude to duplicate these for each type but generalising them seems like it would get messy quick

// Dummies
public class AddDummyEntry : ViewportAction
{
    private FLVER2 CurrentFLVER;
    private ModelEditorScreen Screen;
    private FLVER.Dummy NewObject;
    private int PreviousSelectionIndex;
    private int Index;

    public AddDummyEntry(ModelEditorScreen screen, FLVER2 flver)
    {
        PreviousSelectionIndex = screen.ModelHierarchy._selectedDummy;
        Screen = screen;
        CurrentFLVER = flver;
        NewObject = new FLVER.Dummy();
        Index = flver.Dummies.Count;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        CurrentFLVER.Dummies.Insert(Index, NewObject);
        Screen.ModelHierarchy._selectedDummy = Index;

        Smithbox.EditorHandler.ModelEditor.ViewportHandler.UpdateRepresentativeModel(Index);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Screen.ModelHierarchy._selectedDummy = PreviousSelectionIndex;
        CurrentFLVER.Dummies.RemoveAt(Index);

        Smithbox.EditorHandler.ModelEditor.ViewportHandler.UpdateRepresentativeModel(PreviousSelectionIndex);

        return ActionEvent.NoEvent;
    }
}

public class DuplicateDummyEntry : ViewportAction
{
    private FLVER2 CurrentFLVER;
    private ModelEditorScreen Screen;
    private FLVER.Dummy DupedObject;
    private int PreviousSelectionIndex;
    private int Index;

    public DuplicateDummyEntry(ModelEditorScreen screen, FLVER2 flver, int index)
    {
        PreviousSelectionIndex = screen.ModelHierarchy._selectedDummy;
        Screen = screen;
        CurrentFLVER = flver;
        DupedObject = CurrentFLVER.Dummies[index].Clone();
        Index = flver.Dummies.Count;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        CurrentFLVER.Dummies.Insert(Index, DupedObject);
        Screen.ModelHierarchy._selectedDummy = Index;

        Smithbox.EditorHandler.ModelEditor.ViewportHandler.UpdateRepresentativeModel(Index);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Screen.ModelHierarchy._selectedDummy = PreviousSelectionIndex;
        CurrentFLVER.Dummies.RemoveAt(Index);

        Smithbox.EditorHandler.ModelEditor.ViewportHandler.UpdateRepresentativeModel(PreviousSelectionIndex);

        return ActionEvent.NoEvent;
    }
}

public class RemoveDummyEntry : ViewportAction
{
    private FLVER2 CurrentFLVER;
    private ModelEditorScreen Screen;
    private FLVER.Dummy RemovedObject;
    private int PreviousSelectionIndex;
    private int Index;

    public RemoveDummyEntry(ModelEditorScreen screen, FLVER2 flver, int index)
    {
        PreviousSelectionIndex = screen.ModelHierarchy._selectedDummy;
        Screen = screen;
        CurrentFLVER = flver;
        RemovedObject = CurrentFLVER.Dummies[index].Clone();
        Index = index;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Screen.ModelHierarchy._selectedDummy = -1;
        CurrentFLVER.Dummies.RemoveAt(Index);

        Smithbox.EditorHandler.ModelEditor.ViewportHandler.UpdateRepresentativeModel(-1);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        CurrentFLVER.Dummies.Insert(Index, RemovedObject);
        Screen.ModelHierarchy._selectedDummy = PreviousSelectionIndex;

        Smithbox.EditorHandler.ModelEditor.ViewportHandler.UpdateRepresentativeModel(PreviousSelectionIndex);

        return ActionEvent.NoEvent;
    }
}

public class DuplicateDummyEntryMulti : ViewportAction
{
    private FLVER2 CurrentFLVER;
    private ModelEditorScreen Screen;
    private HierarchyMultiselect Multiselect;
    private List<FLVER.Dummy> DupedObjects;

    public DuplicateDummyEntryMulti(ModelEditorScreen screen, FLVER2 flver, HierarchyMultiselect multiselect)
    {
        Screen = screen;
        CurrentFLVER = flver;
        Multiselect = multiselect;
        DupedObjects = new List<FLVER.Dummy>();
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Screen.ModelHierarchy._selectedDummy = -1;

        foreach (var idx in Multiselect.StoredIndices)
        {
            if (CurrentFLVER.Dummies[idx] != null)
                DupedObjects.Add(CurrentFLVER.Dummies[idx].Clone());
        }

        for (int i = 0; i < DupedObjects.Count; i++)
        {
            CurrentFLVER.Dummies.Add(DupedObjects[i]);
        }

        Multiselect.StoredIndices = new List<int>();

        Smithbox.EditorHandler.ModelEditor.ViewportHandler.UpdateRepresentativeModel(-1);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        for (int i = 0; i < DupedObjects.Count; i++)
        {
            CurrentFLVER.Dummies.Remove(DupedObjects[i]);
        }

        Smithbox.EditorHandler.ModelEditor.ViewportHandler.UpdateRepresentativeModel(-1);

        return ActionEvent.NoEvent;
    }
}

public class RemoveDummyEntryMulti : ViewportAction
{
    private FLVER2 CurrentFLVER;
    private ModelEditorScreen Screen;
    private HierarchyMultiselect Multiselect;
    private List<FLVER.Dummy> RemovedObjects;
    private List<FLVER.Dummy> StoredObjects;
    private List<int> StoredIndices;

    public RemoveDummyEntryMulti(ModelEditorScreen screen, FLVER2 flver, HierarchyMultiselect multiselect)
    {
        Screen = screen;
        CurrentFLVER = flver;
        Multiselect = multiselect;
        RemovedObjects = new List<FLVER.Dummy>();
        StoredIndices = new List<int>();
        StoredObjects = new List<FLVER.Dummy>();
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Screen.ModelHierarchy._selectedDummy = -1;

        foreach (var idx in Multiselect.StoredIndices)
        {
            StoredIndices.Add(idx);

            if (CurrentFLVER.Dummies[idx] != null)
                RemovedObjects.Add(CurrentFLVER.Dummies[idx]);
        }

        foreach (var entry in RemovedObjects)
        {
            StoredObjects.Add(entry.Clone());
            CurrentFLVER.Dummies.Remove(entry);
        }

        Multiselect.StoredIndices = new List<int>();

        Smithbox.EditorHandler.ModelEditor.ViewportHandler.UpdateRepresentativeModel(-1);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        for(int i = 0; i < StoredIndices.Count; i++)
        {
            CurrentFLVER.Dummies.Insert(StoredIndices[i], StoredObjects[i]);
        }

        Smithbox.EditorHandler.ModelEditor.ViewportHandler.UpdateRepresentativeModel(-1);

        return ActionEvent.NoEvent;
    }
}

// Material
public class AddMaterialEntry : ViewportAction
{
    private FLVER2 CurrentFLVER;
    private ModelEditorScreen Screen;
    private FLVER2.Material NewObject;
    private int PreviousSelectionIndex;
    private int Index;

    public AddMaterialEntry(ModelEditorScreen screen, FLVER2 flver)
    {
        PreviousSelectionIndex = screen.ModelHierarchy._selectedMaterial;
        Screen = screen;
        CurrentFLVER = flver;
        NewObject = new FLVER2.Material();
        var emptyMaterialTexture = new FLVER2.Texture();
        NewObject.Textures.Add(emptyMaterialTexture);
        Index = flver.Materials.Count;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        CurrentFLVER.Materials.Insert(Index, NewObject);
        Screen.ModelHierarchy._selectedMaterial = Index;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Screen.ModelHierarchy._selectedMaterial = PreviousSelectionIndex;
        CurrentFLVER.Materials.RemoveAt(Index);

        return ActionEvent.NoEvent;
    }
}

public class DuplicateMaterialEntry : ViewportAction
{
    private FLVER2 CurrentFLVER;
    private ModelEditorScreen Screen;
    private FLVER2.Material DupedObject;
    private int PreviousSelectionIndex;
    private int Index;

    public DuplicateMaterialEntry(ModelEditorScreen screen, FLVER2 flver, int index)
    {
        PreviousSelectionIndex = screen.ModelHierarchy._selectedMaterial;
        Screen = screen;
        CurrentFLVER = flver;
        DupedObject = CurrentFLVER.Materials[index].Clone();
        Index = flver.Materials.Count;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        CurrentFLVER.Materials.Insert(Index, DupedObject);
        Screen.ModelHierarchy._selectedMaterial = Index;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Screen.ModelHierarchy._selectedMaterial = PreviousSelectionIndex;
        CurrentFLVER.Materials.RemoveAt(Index);

        return ActionEvent.NoEvent;
    }
}

public class RemoveMaterialEntry : ViewportAction
{
    private FLVER2 CurrentFLVER;
    private ModelEditorScreen Screen;
    private FLVER2.Material RemovedObject;
    private int PreviousSelectionIndex;
    private int Index;

    public RemoveMaterialEntry(ModelEditorScreen screen, FLVER2 flver, int index)
    {
        PreviousSelectionIndex = screen.ModelHierarchy._selectedMaterial;
        Screen = screen;
        CurrentFLVER = flver;
        RemovedObject = CurrentFLVER.Materials[index].Clone();
        Index = index;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Screen.ModelHierarchy._selectedMaterial = -1;
        CurrentFLVER.Materials.RemoveAt(Index);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        CurrentFLVER.Materials.Insert(Index, RemovedObject);
        Screen.ModelHierarchy._selectedMaterial = PreviousSelectionIndex;

        return ActionEvent.NoEvent;
    }
}

public class DuplicateMaterialEntryMulti : ViewportAction
{
    private FLVER2 CurrentFLVER;
    private ModelEditorScreen Screen;
    private HierarchyMultiselect Multiselect;
    private List<FLVER2.Material> DupedObjects;

    public DuplicateMaterialEntryMulti(ModelEditorScreen screen, FLVER2 flver, HierarchyMultiselect multiselect)
    {
        Screen = screen;
        CurrentFLVER = flver;
        Multiselect = multiselect;
        DupedObjects = new List<FLVER2.Material>();
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Screen.ModelHierarchy._selectedMaterial = -1;

        foreach (var idx in Multiselect.StoredIndices)
        {
            if (CurrentFLVER.Materials[idx] != null)
                DupedObjects.Add(CurrentFLVER.Materials[idx].Clone());
        }

        foreach (var entry in DupedObjects)
        {
            CurrentFLVER.Materials.Add(entry);
        }

        Multiselect.StoredIndices = new List<int>();

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        for (int i = 0; i < DupedObjects.Count; i++)
        {
            CurrentFLVER.Materials.Remove(DupedObjects[i]);
        }

        return ActionEvent.NoEvent;
    }
}

public class RemoveMaterialEntryMulti : ViewportAction
{
    private FLVER2 CurrentFLVER;
    private ModelEditorScreen Screen;
    private HierarchyMultiselect Multiselect;
    private List<FLVER2.Material> RemovedObjects;
    private List<FLVER2.Material> StoredObjects;
    private List<int> StoredIndices;

    public RemoveMaterialEntryMulti(ModelEditorScreen screen, FLVER2 flver, HierarchyMultiselect multiselect)
    {
        Screen = screen;
        CurrentFLVER = flver;
        Multiselect = multiselect;
        RemovedObjects = new List<FLVER2.Material>();
        StoredIndices = new List<int>();
        StoredObjects = new List<FLVER2.Material>();
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Screen.ModelHierarchy._selectedMaterial = -1;

        foreach (var idx in Multiselect.StoredIndices)
        {
            StoredIndices.Add(idx);

            if (CurrentFLVER.Materials[idx] != null)
                RemovedObjects.Add(CurrentFLVER.Materials[idx]);
        }

        foreach (var entry in RemovedObjects)
        {
            StoredObjects.Add(entry.Clone());
            CurrentFLVER.Materials.Remove(entry);
        }

        Multiselect.StoredIndices = new List<int>();

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        for (int i = 0; i < StoredIndices.Count; i++)
        {
            CurrentFLVER.Materials.Insert(StoredIndices[i], StoredObjects[i]);
        }

        return ActionEvent.NoEvent;
    }
}

// GX List
public class AddGXListEntry : ViewportAction
{
    private FLVER2 CurrentFLVER;
    private ModelEditorScreen Screen;
    private FLVER2.GXList NewObject;
    private int PreviousSelectionIndex;
    private int Index;

    public AddGXListEntry(ModelEditorScreen screen, FLVER2 flver)
    {
        PreviousSelectionIndex = screen.ModelHierarchy._selectedGXList;
        Screen = screen;
        CurrentFLVER = flver;
        NewObject = new FLVER2.GXList();
        var emptyGXItem = new FLVER2.GXItem();
        NewObject.Add(emptyGXItem);
        Index = flver.GXLists.Count;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        CurrentFLVER.GXLists.Insert(Index, NewObject);
        Screen.ModelHierarchy._selectedGXList = Index;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Screen.ModelHierarchy._selectedGXList = PreviousSelectionIndex;
        CurrentFLVER.GXLists.RemoveAt(Index);

        return ActionEvent.NoEvent;
    }
}

public class DuplicateGXListEntry : ViewportAction
{
    private FLVER2 CurrentFLVER;
    private ModelEditorScreen Screen;
    private FLVER2.GXList DupedObject;
    private int PreviousSelectionIndex;
    private int Index;

    public DuplicateGXListEntry(ModelEditorScreen screen, FLVER2 flver, int index)
    {
        PreviousSelectionIndex = screen.ModelHierarchy._selectedGXList;
        Screen = screen;
        CurrentFLVER = flver;
        DupedObject = CurrentFLVER.GXLists[index].Clone();
        Index = flver.GXLists.Count;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        CurrentFLVER.GXLists.Insert(Index, DupedObject);
        Screen.ModelHierarchy._selectedGXList = Index;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Screen.ModelHierarchy._selectedGXList = PreviousSelectionIndex;
        CurrentFLVER.GXLists.RemoveAt(Index);

        return ActionEvent.NoEvent;
    }
}

public class RemoveGXListEntry : ViewportAction
{
    private FLVER2 CurrentFLVER;
    private ModelEditorScreen Screen;
    private FLVER2.GXList RemovedObject;
    private int PreviousSelectionIndex;
    private int Index;

    public RemoveGXListEntry(ModelEditorScreen screen, FLVER2 flver, int index)
    {
        PreviousSelectionIndex = screen.ModelHierarchy._selectedGXList;
        Screen = screen;
        CurrentFLVER = flver;
        RemovedObject = CurrentFLVER.GXLists[index].Clone();
        Index = index;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Screen.ModelHierarchy._selectedGXList = -1;
        CurrentFLVER.GXLists.RemoveAt(Index);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        CurrentFLVER.GXLists.Insert(Index, RemovedObject);
        Screen.ModelHierarchy._selectedGXList = PreviousSelectionIndex;

        return ActionEvent.NoEvent;
    }
}

public class DuplicateGXListEntryMulti : ViewportAction
{
    private FLVER2 CurrentFLVER;
    private ModelEditorScreen Screen;
    private HierarchyMultiselect Multiselect;
    private List<FLVER2.GXList> DupedObjects;

    public DuplicateGXListEntryMulti(ModelEditorScreen screen, FLVER2 flver, HierarchyMultiselect multiselect)
    {
        Screen = screen;
        CurrentFLVER = flver;
        Multiselect = multiselect;
        DupedObjects = new List<FLVER2.GXList>();
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Screen.ModelHierarchy._selectedGXList = -1;

        foreach (var idx in Multiselect.StoredIndices)
        {
            if (CurrentFLVER.GXLists[idx] != null)
                DupedObjects.Add(CurrentFLVER.GXLists[idx].Clone());
        }

        foreach (var entry in DupedObjects)
        {
            CurrentFLVER.GXLists.Add(entry);
        }

        Multiselect.StoredIndices = new List<int>();

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        for (int i = 0; i < DupedObjects.Count; i++)
        {
            CurrentFLVER.GXLists.Remove(DupedObjects[i]);
        }

        return ActionEvent.NoEvent;
    }
}

public class RemoveGXListEntryMulti : ViewportAction
{
    private FLVER2 CurrentFLVER;
    private ModelEditorScreen Screen;
    private HierarchyMultiselect Multiselect;
    private List<FLVER2.GXList> RemovedObjects;
    private List<FLVER2.GXList> StoredObjects;
    private List<int> StoredIndices;

    public RemoveGXListEntryMulti(ModelEditorScreen screen, FLVER2 flver, HierarchyMultiselect multiselect)
    {
        Screen = screen;
        CurrentFLVER = flver;
        Multiselect = multiselect;
        RemovedObjects = new List<FLVER2.GXList>();
        StoredIndices = new List<int>();
        StoredObjects = new List<FLVER2.GXList>();
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Screen.ModelHierarchy._selectedGXList = -1;

        foreach (var idx in Multiselect.StoredIndices)
        {
            StoredIndices.Add(idx);

            if (CurrentFLVER.GXLists[idx] != null)
                RemovedObjects.Add(CurrentFLVER.GXLists[idx]);
        }

        foreach (var entry in RemovedObjects)
        {
            StoredObjects.Add(entry.Clone());
            CurrentFLVER.GXLists.Remove(entry);
        }

        Multiselect.StoredIndices = new List<int>();

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        for (int i = 0; i < StoredIndices.Count; i++)
        {
            CurrentFLVER.GXLists.Insert(StoredIndices[i], StoredObjects[i]);
        }

        return ActionEvent.NoEvent;
    }
}

// Node
public class AddNodeEntry : ViewportAction
{
    private FLVER2 CurrentFLVER;
    private ModelEditorScreen Screen;
    private FLVER.Node NewObject;
    private int PreviousSelectionIndex;
    private int Index;

    public AddNodeEntry(ModelEditorScreen screen, FLVER2 flver)
    {
        PreviousSelectionIndex = screen.ModelHierarchy._selectedNode;
        Screen = screen;
        CurrentFLVER = flver;
        NewObject = new FLVER.Node();
        Index = flver.Nodes.Count;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        CurrentFLVER.Nodes.Insert(Index, NewObject);
        Screen.ModelHierarchy._selectedNode = Index;

        Smithbox.EditorHandler.ModelEditor.ViewportHandler.UpdateRepresentativeModel(Index);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Screen.ModelHierarchy._selectedNode = PreviousSelectionIndex;
        CurrentFLVER.Nodes.RemoveAt(Index);

        Smithbox.EditorHandler.ModelEditor.ViewportHandler.UpdateRepresentativeModel(PreviousSelectionIndex);

        return ActionEvent.NoEvent;
    }
}

public class DuplicateNodeEntry : ViewportAction
{
    private FLVER2 CurrentFLVER;
    private ModelEditorScreen Screen;
    private FLVER.Node DupedObject;
    private int PreviousSelectionIndex;
    private int Index;

    public DuplicateNodeEntry(ModelEditorScreen screen, FLVER2 flver, int index)
    {
        PreviousSelectionIndex = screen.ModelHierarchy._selectedNode;
        Screen = screen;
        CurrentFLVER = flver;
        DupedObject = CurrentFLVER.Nodes[index].Clone();
        Index = flver.Nodes.Count;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        CurrentFLVER.Nodes.Insert(Index, DupedObject);
        Screen.ModelHierarchy._selectedNode = Index;

        Smithbox.EditorHandler.ModelEditor.ViewportHandler.UpdateRepresentativeModel(Index);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Screen.ModelHierarchy._selectedNode = PreviousSelectionIndex;
        CurrentFLVER.Nodes.RemoveAt(Index);

        Smithbox.EditorHandler.ModelEditor.ViewportHandler.UpdateRepresentativeModel(PreviousSelectionIndex);

        return ActionEvent.NoEvent;
    }
}

public class RemoveNodeEntry : ViewportAction
{
    private FLVER2 CurrentFLVER;
    private ModelEditorScreen Screen;
    private FLVER.Node RemovedObject;
    private int PreviousSelectionIndex;
    private int Index;

    public RemoveNodeEntry(ModelEditorScreen screen, FLVER2 flver, int index)
    {
        PreviousSelectionIndex = screen.ModelHierarchy._selectedNode;
        Screen = screen;
        CurrentFLVER = flver;
        RemovedObject = CurrentFLVER.Nodes[index].Clone();
        Index = index;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Screen.ModelHierarchy._selectedNode = -1;
        CurrentFLVER.Nodes.RemoveAt(Index);

        Smithbox.EditorHandler.ModelEditor.ViewportHandler.UpdateRepresentativeModel(-1);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        CurrentFLVER.Nodes.Insert(Index, RemovedObject);
        Screen.ModelHierarchy._selectedNode = PreviousSelectionIndex;

        Smithbox.EditorHandler.ModelEditor.ViewportHandler.UpdateRepresentativeModel(PreviousSelectionIndex);

        return ActionEvent.NoEvent;
    }
}

public class DuplicateNodeEntryMulti : ViewportAction
{
    private FLVER2 CurrentFLVER;
    private ModelEditorScreen Screen;
    private HierarchyMultiselect Multiselect;
    private List<FLVER.Node> DupedObjects;

    public DuplicateNodeEntryMulti(ModelEditorScreen screen, FLVER2 flver, HierarchyMultiselect multiselect)
    {
        Screen = screen;
        CurrentFLVER = flver;
        Multiselect = multiselect;
        DupedObjects = new List<FLVER.Node>();
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Screen.ModelHierarchy._selectedNode = -1;

        foreach (var idx in Multiselect.StoredIndices)
        {
            if (CurrentFLVER.Nodes[idx] != null)
                DupedObjects.Add(CurrentFLVER.Nodes[idx].Clone());
        }

        foreach (var entry in DupedObjects)
        {
            CurrentFLVER.Nodes.Add(entry);
        }

        Multiselect.StoredIndices = new List<int>();

        Smithbox.EditorHandler.ModelEditor.ViewportHandler.UpdateRepresentativeModel(-1);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        for (int i = 0; i < DupedObjects.Count; i++)
        {
            CurrentFLVER.Nodes.Remove(DupedObjects[i]);
        }

        Smithbox.EditorHandler.ModelEditor.ViewportHandler.UpdateRepresentativeModel(-1);

        return ActionEvent.NoEvent;
    }
}

public class RemoveNodeEntryMulti : ViewportAction
{
    private FLVER2 CurrentFLVER;
    private ModelEditorScreen Screen;
    private HierarchyMultiselect Multiselect;
    private List<FLVER.Node> RemovedObjects;
    private List<FLVER.Node> StoredObjects;
    private List<int> StoredIndices;

    public RemoveNodeEntryMulti(ModelEditorScreen screen, FLVER2 flver, HierarchyMultiselect multiselect)
    {
        Screen = screen;
        CurrentFLVER = flver;
        Multiselect = multiselect;
        RemovedObjects = new List<FLVER.Node>();
        StoredIndices = new List<int>();
        StoredObjects = new List<FLVER.Node>();
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Screen.ModelHierarchy._selectedNode = -1;

        foreach (var idx in Multiselect.StoredIndices)
        {
            StoredIndices.Add(idx);

            if (CurrentFLVER.Nodes[idx] != null)
                RemovedObjects.Add(CurrentFLVER.Nodes[idx]);
        }

        foreach (var entry in RemovedObjects)
        {
            StoredObjects.Add(entry.Clone());
            CurrentFLVER.Nodes.Remove(entry);
        }

        Multiselect.StoredIndices = new List<int>();

        Smithbox.EditorHandler.ModelEditor.ViewportHandler.UpdateRepresentativeModel(-1);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        for (int i = 0; i < StoredIndices.Count; i++)
        {
            CurrentFLVER.Nodes.Insert(StoredIndices[i], StoredObjects[i]);
        }

        Smithbox.EditorHandler.ModelEditor.ViewportHandler.UpdateRepresentativeModel(-1);

        return ActionEvent.NoEvent;
    }
}

// Mesh
public class AddMeshEntry : ViewportAction
{
    private FLVER2 CurrentFLVER;
    private ModelEditorScreen Screen;
    private FLVER2.Mesh NewObject;
    private int PreviousSelectionIndex;
    private int Index;

    public AddMeshEntry(ModelEditorScreen screen, FLVER2 flver)
    {
        PreviousSelectionIndex = screen.ModelHierarchy._selectedMesh;
        Screen = screen;
        CurrentFLVER = flver;
        NewObject = new FLVER2.Mesh();

        var emptyBoundingBoxes = new FLVER2.Mesh.BoundingBoxes();
        var emptyFaceset = new FLVER2.FaceSet();
        var emptyVertexBuffer = new FLVER2.VertexBuffer(0);
        NewObject.BoundingBox = emptyBoundingBoxes;
        NewObject.FaceSets.Add(emptyFaceset);
        NewObject.VertexBuffers.Add(emptyVertexBuffer);

        Index = flver.Meshes.Count;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        CurrentFLVER.Meshes.Insert(Index, NewObject);
        Screen.ModelHierarchy._selectedMesh = Index;

        Smithbox.EditorHandler.ModelEditor.ViewportHandler.UpdateRepresentativeModel(Index);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Screen.ModelHierarchy._selectedMesh = PreviousSelectionIndex;
        CurrentFLVER.Meshes.RemoveAt(Index);

        Smithbox.EditorHandler.ModelEditor.ViewportHandler.UpdateRepresentativeModel(PreviousSelectionIndex);

        return ActionEvent.NoEvent;
    }
}

public class DuplicateMeshEntry : ViewportAction
{
    private FLVER2 CurrentFLVER;
    private ModelEditorScreen Screen;
    private FLVER2.Mesh DupedObject;
    private int PreviousSelectionIndex;
    private int Index;

    public DuplicateMeshEntry(ModelEditorScreen screen, FLVER2 flver, int index)
    {
        PreviousSelectionIndex = screen.ModelHierarchy._selectedMesh;
        Screen = screen;
        CurrentFLVER = flver;
        DupedObject = CurrentFLVER.Meshes[index].Clone();
        Index = flver.Meshes.Count;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        CurrentFLVER.Meshes.Insert(Index, DupedObject);
        Screen.ModelHierarchy._selectedMesh = Index;

        Smithbox.EditorHandler.ModelEditor.ViewportHandler.UpdateRepresentativeModel(Index);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Screen.ModelHierarchy._selectedMesh = PreviousSelectionIndex;
        CurrentFLVER.Meshes.RemoveAt(Index);

        Smithbox.EditorHandler.ModelEditor.ViewportHandler.UpdateRepresentativeModel(PreviousSelectionIndex);

        return ActionEvent.NoEvent;
    }
}

public class RemoveMeshEntry : ViewportAction
{
    private FLVER2 CurrentFLVER;
    private ModelEditorScreen Screen;
    private FLVER2.Mesh RemovedObject;
    private int PreviousSelectionIndex;
    private int Index;

    public RemoveMeshEntry(ModelEditorScreen screen, FLVER2 flver, int index)
    {
        PreviousSelectionIndex = screen.ModelHierarchy._selectedMesh;
        Screen = screen;
        CurrentFLVER = flver;
        RemovedObject = CurrentFLVER.Meshes[index].Clone();
        Index = index;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Screen.ModelHierarchy._selectedMesh = -1;
        CurrentFLVER.Meshes.RemoveAt(Index);

        Smithbox.EditorHandler.ModelEditor.ViewportHandler.UpdateRepresentativeModel(-1);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        CurrentFLVER.Meshes.Insert(Index, RemovedObject);
        Screen.ModelHierarchy._selectedMesh = PreviousSelectionIndex;

        Smithbox.EditorHandler.ModelEditor.ViewportHandler.UpdateRepresentativeModel(PreviousSelectionIndex);

        return ActionEvent.NoEvent;
    }
}

public class DuplicateMeshEntryMulti : ViewportAction
{
    private FLVER2 CurrentFLVER;
    private ModelEditorScreen Screen;
    private HierarchyMultiselect Multiselect;
    private List<FLVER2.Mesh> DupedObjects;

    public DuplicateMeshEntryMulti(ModelEditorScreen screen, FLVER2 flver, HierarchyMultiselect multiselect)
    {
        Screen = screen;
        CurrentFLVER = flver;
        Multiselect = multiselect;
        DupedObjects = new List<FLVER2.Mesh>();
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Screen.ModelHierarchy._selectedMesh = -1;

        foreach (var idx in Multiselect.StoredIndices)
        {
            if (CurrentFLVER.Meshes[idx] != null)
                DupedObjects.Add(CurrentFLVER.Meshes[idx].Clone());
        }

        foreach (var entry in DupedObjects)
        {
            CurrentFLVER.Meshes.Add(entry);
        }

        Multiselect.StoredIndices = new List<int>();

        Smithbox.EditorHandler.ModelEditor.ViewportHandler.UpdateRepresentativeModel(-1);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        for (int i = 0; i < DupedObjects.Count; i++)
        {
            CurrentFLVER.Meshes.Remove(DupedObjects[i]);
        }

        Smithbox.EditorHandler.ModelEditor.ViewportHandler.UpdateRepresentativeModel(-1);

        return ActionEvent.NoEvent;
    }
}

public class RemoveMeshEntryMulti : ViewportAction
{
    private FLVER2 CurrentFLVER;
    private ModelEditorScreen Screen;
    private HierarchyMultiselect Multiselect;
    private List<FLVER2.Mesh> RemovedObjects;
    private List<FLVER2.Mesh> StoredObjects;
    private List<int> StoredIndices;

    public RemoveMeshEntryMulti(ModelEditorScreen screen, FLVER2 flver, HierarchyMultiselect multiselect)
    {
        Screen = screen;
        CurrentFLVER = flver;
        Multiselect = multiselect;
        RemovedObjects = new List<FLVER2.Mesh>();
        StoredIndices = new List<int>();
        StoredObjects = new List<FLVER2.Mesh>();
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Screen.ModelHierarchy._selectedMesh = -1;

        foreach (var idx in Multiselect.StoredIndices)
        {
            StoredIndices.Add(idx);

            if (CurrentFLVER.Meshes[idx] != null)
                RemovedObjects.Add(CurrentFLVER.Meshes[idx]);
        }

        foreach (var entry in RemovedObjects)
        {
            StoredObjects.Add(entry.Clone());
            CurrentFLVER.Meshes.Remove(entry);
        }

        Multiselect.StoredIndices = new List<int>();

        Smithbox.EditorHandler.ModelEditor.ViewportHandler.UpdateRepresentativeModel(-1);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        for (int i = 0; i < StoredIndices.Count; i++)
        {
            CurrentFLVER.Meshes.Insert(StoredIndices[i], StoredObjects[i]);
        }

        Smithbox.EditorHandler.ModelEditor.ViewportHandler.UpdateRepresentativeModel(-1);

        return ActionEvent.NoEvent;
    }
}

// Buffer Layout
public class AddBufferLayoutEntry : ViewportAction
{
    private FLVER2 CurrentFLVER;
    private ModelEditorScreen Screen;
    private FLVER2.BufferLayout NewObject;
    private int PreviousSelectionIndex;
    private int Index;

    public AddBufferLayoutEntry(ModelEditorScreen screen, FLVER2 flver)
    {
        PreviousSelectionIndex = screen.ModelHierarchy._selectedBufferLayout;
        Screen = screen;
        CurrentFLVER = flver;
        NewObject = new FLVER2.BufferLayout();
        var emptyLayoutMember = new FLVER.LayoutMember();
        NewObject.Add(emptyLayoutMember);
        Index = flver.BufferLayouts.Count;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        CurrentFLVER.BufferLayouts.Insert(Index, NewObject);
        Screen.ModelHierarchy._selectedBufferLayout = Index;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Screen.ModelHierarchy._selectedBufferLayout = PreviousSelectionIndex;
        CurrentFLVER.BufferLayouts.RemoveAt(Index);

        return ActionEvent.NoEvent;
    }
}

public class DuplicateBufferLayoutEntry : ViewportAction
{
    private FLVER2 CurrentFLVER;
    private ModelEditorScreen Screen;
    private FLVER2.BufferLayout DupedObject;
    private int PreviousSelectionIndex;
    private int Index;

    public DuplicateBufferLayoutEntry(ModelEditorScreen screen, FLVER2 flver, int index)
    {
        PreviousSelectionIndex = screen.ModelHierarchy._selectedBufferLayout;
        Screen = screen;
        CurrentFLVER = flver;
        DupedObject = CurrentFLVER.BufferLayouts[index].Clone();
        Index = flver.BufferLayouts.Count;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        CurrentFLVER.BufferLayouts.Insert(Index, DupedObject);
        Screen.ModelHierarchy._selectedBufferLayout = Index;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Screen.ModelHierarchy._selectedBufferLayout = PreviousSelectionIndex;
        CurrentFLVER.BufferLayouts.RemoveAt(Index);

        return ActionEvent.NoEvent;
    }
}

public class RemoveBufferLayoutEntry : ViewportAction
{
    private FLVER2 CurrentFLVER;
    private ModelEditorScreen Screen;
    private FLVER2.BufferLayout RemovedObject;
    private int PreviousSelectionIndex;
    private int Index;

    public RemoveBufferLayoutEntry(ModelEditorScreen screen, FLVER2 flver, int index)
    {
        PreviousSelectionIndex = screen.ModelHierarchy._selectedBufferLayout;
        Screen = screen;
        CurrentFLVER = flver;
        RemovedObject = CurrentFLVER.BufferLayouts[index].Clone();
        Index = index;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Screen.ModelHierarchy._selectedBufferLayout = -1;
        CurrentFLVER.BufferLayouts.RemoveAt(Index);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        CurrentFLVER.BufferLayouts.Insert(Index, RemovedObject);
        Screen.ModelHierarchy._selectedBufferLayout = PreviousSelectionIndex;

        return ActionEvent.NoEvent;
    }
}

public class DuplicateBufferLayoutEntryMulti : ViewportAction
{
    private FLVER2 CurrentFLVER;
    private ModelEditorScreen Screen;
    private HierarchyMultiselect Multiselect;
    private List<FLVER2.BufferLayout> DupedObjects;

    public DuplicateBufferLayoutEntryMulti(ModelEditorScreen screen, FLVER2 flver, HierarchyMultiselect multiselect)
    {
        Screen = screen;
        CurrentFLVER = flver;
        Multiselect = multiselect;
        DupedObjects = new List<FLVER2.BufferLayout>();
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Screen.ModelHierarchy._selectedBufferLayout = -1;

        foreach (var idx in Multiselect.StoredIndices)
        {
            if (CurrentFLVER.BufferLayouts[idx] != null)
                DupedObjects.Add(CurrentFLVER.BufferLayouts[idx].Clone());
        }

        foreach (var entry in DupedObjects)
        {
            CurrentFLVER.BufferLayouts.Add(entry);
        }

        Multiselect.StoredIndices = new List<int>();

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        for (int i = 0; i < DupedObjects.Count; i++)
        {
            CurrentFLVER.BufferLayouts.Remove(DupedObjects[i]);
        }

        return ActionEvent.NoEvent;
    }
}

public class RemoveBufferLayoutEntryMulti : ViewportAction
{
    private FLVER2 CurrentFLVER;
    private ModelEditorScreen Screen;
    private HierarchyMultiselect Multiselect;
    private List<FLVER2.BufferLayout> RemovedObjects;
    private List<FLVER2.BufferLayout> StoredObjects;
    private List<int> StoredIndices;

    public RemoveBufferLayoutEntryMulti(ModelEditorScreen screen, FLVER2 flver, HierarchyMultiselect multiselect)
    {
        Screen = screen;
        CurrentFLVER = flver;
        Multiselect = multiselect;
        RemovedObjects = new List<FLVER2.BufferLayout>();
        StoredIndices = new List<int>();
        StoredObjects = new List<FLVER2.BufferLayout>();
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Screen.ModelHierarchy._selectedBufferLayout = -1;

        foreach (var idx in Multiselect.StoredIndices)
        {
            StoredIndices.Add(idx);

            if (CurrentFLVER.BufferLayouts[idx] != null)
                RemovedObjects.Add(CurrentFLVER.BufferLayouts[idx]);
        }

        foreach (var entry in RemovedObjects)
        {
            StoredObjects.Add(entry.Clone());
            CurrentFLVER.BufferLayouts.Remove(entry);
        }

        Multiselect.StoredIndices = new List<int>();

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        for (int i = 0; i < StoredIndices.Count; i++)
        {
            CurrentFLVER.BufferLayouts.Insert(StoredIndices[i], StoredObjects[i]);
        }

        return ActionEvent.NoEvent;
    }
}

// Base Skeleton Bone
public class AddBaseSkeletonBoneEntry : ViewportAction
{
    private FLVER2 CurrentFLVER;
    private ModelEditorScreen Screen;
    private FLVER2.SkeletonSet.Bone NewObject;
    private int PreviousSelectionIndex;
    private int Index;

    public AddBaseSkeletonBoneEntry(ModelEditorScreen screen, FLVER2 flver)
    {
        PreviousSelectionIndex = screen.ModelHierarchy._selectedBaseSkeletonBone;
        Screen = screen;
        CurrentFLVER = flver;
        NewObject = new FLVER2.SkeletonSet.Bone(-1);
        Index = flver.Skeletons.BaseSkeleton.Count;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        CurrentFLVER.Skeletons.BaseSkeleton.Insert(Index, NewObject);
        Screen.ModelHierarchy._selectedBaseSkeletonBone = Index;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Screen.ModelHierarchy._selectedBaseSkeletonBone = PreviousSelectionIndex;
        CurrentFLVER.Skeletons.BaseSkeleton.RemoveAt(Index);

        return ActionEvent.NoEvent;
    }
}

public class DuplicateBaseSkeletonBoneEntry : ViewportAction
{
    private FLVER2 CurrentFLVER;
    private ModelEditorScreen Screen;
    private FLVER2.SkeletonSet.Bone DupedObject;
    private int PreviousSelectionIndex;
    private int Index;

    public DuplicateBaseSkeletonBoneEntry(ModelEditorScreen screen, FLVER2 flver, int index)
    {
        PreviousSelectionIndex = screen.ModelHierarchy._selectedBaseSkeletonBone;
        Screen = screen;
        CurrentFLVER = flver;
        DupedObject = CurrentFLVER.Skeletons.BaseSkeleton[index].Clone();
        Index = flver.Skeletons.BaseSkeleton.Count;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        CurrentFLVER.Skeletons.BaseSkeleton.Insert(Index, DupedObject);
        Screen.ModelHierarchy._selectedBaseSkeletonBone = Index;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Screen.ModelHierarchy._selectedBaseSkeletonBone = PreviousSelectionIndex;
        CurrentFLVER.Skeletons.BaseSkeleton.RemoveAt(Index);

        return ActionEvent.NoEvent;
    }
}

public class RemoveBaseSkeletonBoneEntry : ViewportAction
{
    private FLVER2 CurrentFLVER;
    private ModelEditorScreen Screen;
    private FLVER2.SkeletonSet.Bone RemovedObject;
    private int PreviousSelectionIndex;
    private int Index;

    public RemoveBaseSkeletonBoneEntry(ModelEditorScreen screen, FLVER2 flver, int index)
    {
        PreviousSelectionIndex = screen.ModelHierarchy._selectedBaseSkeletonBone;
        Screen = screen;
        CurrentFLVER = flver;
        RemovedObject = CurrentFLVER.Skeletons.BaseSkeleton[index].Clone();
        Index = index;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Screen.ModelHierarchy._selectedBaseSkeletonBone = -1;
        CurrentFLVER.Skeletons.BaseSkeleton.RemoveAt(Index);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        CurrentFLVER.Skeletons.BaseSkeleton.Insert(Index, RemovedObject);
        Screen.ModelHierarchy._selectedBaseSkeletonBone = PreviousSelectionIndex;

        return ActionEvent.NoEvent;
    }
}

public class DuplicateBaseSkeletonBoneEntryMulti : ViewportAction
{
    private FLVER2 CurrentFLVER;
    private ModelEditorScreen Screen;
    private HierarchyMultiselect Multiselect;
    private List<FLVER2.SkeletonSet.Bone> DupedObjects;

    public DuplicateBaseSkeletonBoneEntryMulti(ModelEditorScreen screen, FLVER2 flver, HierarchyMultiselect multiselect)
    {
        Screen = screen;
        CurrentFLVER = flver;
        Multiselect = multiselect;
        DupedObjects = new List<FLVER2.SkeletonSet.Bone>();
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Screen.ModelHierarchy._selectedBaseSkeletonBone = -1;

        foreach (var idx in Multiselect.StoredIndices)
        {
            if (CurrentFLVER.Skeletons.BaseSkeleton[idx] != null)
                DupedObjects.Add(CurrentFLVER.Skeletons.BaseSkeleton[idx].Clone());
        }

        foreach (var entry in DupedObjects)
        {
            CurrentFLVER.Skeletons.BaseSkeleton.Add(entry);
        }

        Multiselect.StoredIndices = new List<int>();

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        for (int i = 0; i < DupedObjects.Count; i++)
        {
            CurrentFLVER.Skeletons.BaseSkeleton.Remove(DupedObjects[i]);
        }

        return ActionEvent.NoEvent;
    }
}

public class RemoveBaseSkeletonBoneEntryMulti : ViewportAction
{
    private FLVER2 CurrentFLVER;
    private ModelEditorScreen Screen;
    private HierarchyMultiselect Multiselect;
    private List<FLVER2.SkeletonSet.Bone> RemovedObjects;
    private List<FLVER2.SkeletonSet.Bone> StoredObjects;
    private List<int> StoredIndices;

    public RemoveBaseSkeletonBoneEntryMulti(ModelEditorScreen screen, FLVER2 flver, HierarchyMultiselect multiselect)
    {
        Screen = screen;
        CurrentFLVER = flver;
        Multiselect = multiselect;
        RemovedObjects = new List<FLVER2.SkeletonSet.Bone>();
        StoredIndices = new List<int>();
        StoredObjects = new List<FLVER2.SkeletonSet.Bone>();
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Screen.ModelHierarchy._selectedBaseSkeletonBone = -1;

        foreach (var idx in Multiselect.StoredIndices)
        {
            StoredIndices.Add(idx);

            if (CurrentFLVER.Skeletons.BaseSkeleton[idx] != null)
                RemovedObjects.Add(CurrentFLVER.Skeletons.BaseSkeleton[idx]);
        }

        foreach (var entry in RemovedObjects)
        {
            StoredObjects.Add(entry.Clone());
            CurrentFLVER.Skeletons.BaseSkeleton.Remove(entry);
        }

        Multiselect.StoredIndices = new List<int>();

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        for (int i = 0; i < StoredIndices.Count; i++)
        {
            CurrentFLVER.Skeletons.BaseSkeleton.Insert(StoredIndices[i], StoredObjects[i]);
        }

        return ActionEvent.NoEvent;
    }
}

// All Skeleton Bone
public class AddAllSkeletonBoneEntry : ViewportAction
{
    private FLVER2 CurrentFLVER;
    private ModelEditorScreen Screen;
    private FLVER2.SkeletonSet.Bone NewObject;
    private int PreviousSelectionIndex;
    private int Index;

    public AddAllSkeletonBoneEntry(ModelEditorScreen screen, FLVER2 flver)
    {
        PreviousSelectionIndex = screen.ModelHierarchy._selectedAllSkeletonBone;
        Screen = screen;
        CurrentFLVER = flver;
        NewObject = new FLVER2.SkeletonSet.Bone(-1);
        Index = flver.Skeletons.AllSkeletons.Count;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        CurrentFLVER.Skeletons.AllSkeletons.Insert(Index, NewObject);
        Screen.ModelHierarchy._selectedAllSkeletonBone = Index;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Screen.ModelHierarchy._selectedAllSkeletonBone = PreviousSelectionIndex;
        CurrentFLVER.Skeletons.AllSkeletons.RemoveAt(Index);

        return ActionEvent.NoEvent;
    }
}

public class DuplicateAllSkeletonBoneEntry : ViewportAction
{
    private FLVER2 CurrentFLVER;
    private ModelEditorScreen Screen;
    private FLVER2.SkeletonSet.Bone DupedObject;
    private int PreviousSelectionIndex;
    private int Index;

    public DuplicateAllSkeletonBoneEntry(ModelEditorScreen screen, FLVER2 flver, int index)
    {
        PreviousSelectionIndex = screen.ModelHierarchy._selectedAllSkeletonBone;
        Screen = screen;
        CurrentFLVER = flver;
        DupedObject = CurrentFLVER.Skeletons.AllSkeletons[index].Clone();
        Index = flver.Skeletons.AllSkeletons.Count;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        CurrentFLVER.Skeletons.AllSkeletons.Insert(Index, DupedObject);
        Screen.ModelHierarchy._selectedAllSkeletonBone = Index;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Screen.ModelHierarchy._selectedAllSkeletonBone = PreviousSelectionIndex;
        CurrentFLVER.Skeletons.AllSkeletons.RemoveAt(Index);

        return ActionEvent.NoEvent;
    }
}

public class RemoveAllSkeletonBoneEntry : ViewportAction
{
    private FLVER2 CurrentFLVER;
    private ModelEditorScreen Screen;
    private FLVER2.SkeletonSet.Bone RemovedObject;
    private int PreviousSelectionIndex;
    private int Index;

    public RemoveAllSkeletonBoneEntry(ModelEditorScreen screen, FLVER2 flver, int index)
    {
        PreviousSelectionIndex = screen.ModelHierarchy._selectedAllSkeletonBone;
        Screen = screen;
        CurrentFLVER = flver;
        RemovedObject = CurrentFLVER.Skeletons.AllSkeletons[index].Clone();
        Index = index;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Screen.ModelHierarchy._selectedAllSkeletonBone = -1;
        CurrentFLVER.Skeletons.AllSkeletons.RemoveAt(Index);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        CurrentFLVER.Skeletons.AllSkeletons.Insert(Index, RemovedObject);
        Screen.ModelHierarchy._selectedAllSkeletonBone = PreviousSelectionIndex;

        return ActionEvent.NoEvent;
    }
}

public class DuplicateAllSkeletonBoneEntryMulti : ViewportAction
{
    private FLVER2 CurrentFLVER;
    private ModelEditorScreen Screen;
    private HierarchyMultiselect Multiselect;
    private List<FLVER2.SkeletonSet.Bone> DupedObjects;

    public DuplicateAllSkeletonBoneEntryMulti(ModelEditorScreen screen, FLVER2 flver, HierarchyMultiselect multiselect)
    {
        Screen = screen;
        CurrentFLVER = flver;
        Multiselect = multiselect;
        DupedObjects = new List<FLVER2.SkeletonSet.Bone>();
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Screen.ModelHierarchy._selectedAllSkeletonBone = -1;

        foreach (var idx in Multiselect.StoredIndices)
        {
            if (CurrentFLVER.Skeletons.AllSkeletons[idx] != null)
                DupedObjects.Add(CurrentFLVER.Skeletons.AllSkeletons[idx].Clone());
        }

        foreach (var entry in DupedObjects)
        {
            CurrentFLVER.Skeletons.AllSkeletons.Add(entry);
        }

        Multiselect.StoredIndices = new List<int>();

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        for (int i = 0; i < DupedObjects.Count; i++)
        {
            CurrentFLVER.Skeletons.AllSkeletons.Remove(DupedObjects[i]);
        }

        return ActionEvent.NoEvent;
    }
}

public class RemoveAllSkeletonBoneEntryMulti : ViewportAction
{
    private FLVER2 CurrentFLVER;
    private ModelEditorScreen Screen;
    private HierarchyMultiselect Multiselect;
    private List<FLVER2.SkeletonSet.Bone> RemovedObjects;
    private List<FLVER2.SkeletonSet.Bone> StoredObjects;
    private List<int> StoredIndices;

    public RemoveAllSkeletonBoneEntryMulti(ModelEditorScreen screen, FLVER2 flver, HierarchyMultiselect multiselect)
    {
        Screen = screen;
        CurrentFLVER = flver;
        Multiselect = multiselect;
        RemovedObjects = new List<FLVER2.SkeletonSet.Bone>();
        StoredIndices = new List<int>();
        StoredObjects = new List<FLVER2.SkeletonSet.Bone>();
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Screen.ModelHierarchy._selectedAllSkeletonBone = -1;

        foreach (var idx in Multiselect.StoredIndices)
        {
            StoredIndices.Add(idx);

            if (CurrentFLVER.Skeletons.AllSkeletons[idx] != null)
                RemovedObjects.Add(CurrentFLVER.Skeletons.AllSkeletons[idx]);
        }

        foreach (var entry in RemovedObjects)
        {
            StoredObjects.Add(entry.Clone());
            CurrentFLVER.Skeletons.AllSkeletons.Remove(entry);
        }

        Multiselect.StoredIndices = new List<int>();

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        for (int i = 0; i < StoredIndices.Count; i++)
        {
            CurrentFLVER.Skeletons.AllSkeletons.Insert(StoredIndices[i], StoredObjects[i]);
        }

        return ActionEvent.NoEvent;
    }
}

// Texture
public class AddMaterialTextureEntry : ViewportAction
{
    private FLVER2 CurrentFLVER;
    private ModelEditorScreen Screen;
    private FLVER2.Material CurrentMaterial;
    private FLVER2.Texture NewObject;

    public AddMaterialTextureEntry(ModelEditorScreen screen, FLVER2 flver)
    {
        Screen = screen;
        CurrentFLVER = flver;
        CurrentMaterial = flver.Materials[Screen.ModelHierarchy._selectedMaterial];

        NewObject = new FLVER2.Texture();
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        CurrentMaterial.Textures.Add(NewObject);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        if (CurrentMaterial.Textures.Count > 1)
            Screen.ModelHierarchy._subSelectedTextureRow = 0;
        else
            Screen.ModelHierarchy._subSelectedTextureRow = -1;

        CurrentMaterial.Textures.Remove(NewObject);

        return ActionEvent.NoEvent;
    }
}

public class DuplicateMaterialTextureEntry : ViewportAction
{
    private FLVER2 CurrentFLVER;
    private ModelEditorScreen Screen;
    private FLVER2.Material CurrentMaterial;
    private FLVER2.Texture NewObject;

    public DuplicateMaterialTextureEntry(ModelEditorScreen screen, FLVER2 flver, FLVER2.Texture curTexture)
    {
        Screen = screen;
        CurrentFLVER = flver;
        CurrentMaterial = flver.Materials[Screen.ModelHierarchy._selectedMaterial];

        NewObject = curTexture.Clone();
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        CurrentMaterial.Textures.Add(NewObject);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        if (CurrentMaterial.Textures.Count > 1)
            Screen.ModelHierarchy._subSelectedTextureRow = 0;
        else
            Screen.ModelHierarchy._subSelectedTextureRow = -1;

        CurrentMaterial.Textures.Remove(NewObject);

        return ActionEvent.NoEvent;
    }
}

public class RemoveMaterialTextureEntry : ViewportAction
{
    private FLVER2 CurrentFLVER;
    private ModelEditorScreen Screen;
    private FLVER2.Material CurrentMaterial;
    private FLVER2.Texture StoredTexture;
    private FLVER2.Texture OldObject;

    public RemoveMaterialTextureEntry(ModelEditorScreen screen, FLVER2 flver, FLVER2.Texture curTexture)
    {
        Screen = screen;
        CurrentFLVER = flver;
        CurrentMaterial = flver.Materials[Screen.ModelHierarchy._selectedMaterial];

        StoredTexture = curTexture.Clone();
        OldObject = curTexture;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        if(CurrentMaterial.Textures.Count > 1)
            Screen.ModelHierarchy._subSelectedTextureRow = 0;
        else
            Screen.ModelHierarchy._subSelectedTextureRow = -1;

        CurrentMaterial.Textures.Remove(OldObject);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        CurrentMaterial.Textures.Add(StoredTexture);

        return ActionEvent.NoEvent;
    }
}

// GX Item
public class AddGXItemEntry : ViewportAction
{
    private FLVER2 CurrentFLVER;
    private ModelEditorScreen Screen;
    private FLVER2.GXList CurrentGXList;
    private FLVER2.GXItem NewObject;

    public AddGXItemEntry(ModelEditorScreen screen, FLVER2 flver)
    {
        Screen = screen;
        CurrentFLVER = flver;
        CurrentGXList = flver.GXLists[Screen.ModelHierarchy._selectedGXList];

        NewObject = new FLVER2.GXItem();
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        CurrentGXList.Add(NewObject);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        if (CurrentGXList.Count > 1)
            Screen.ModelHierarchy._subSelectedGXItemRow = 0;
        else
            Screen.ModelHierarchy._subSelectedGXItemRow = -1;

        CurrentGXList.Remove(NewObject);

        return ActionEvent.NoEvent;
    }
}

public class DuplicateGXItemTextureEntry : ViewportAction
{
    private FLVER2 CurrentFLVER;
    private ModelEditorScreen Screen;
    private FLVER2.GXList CurrentGXList;
    private FLVER2.GXItem NewObject;

    public DuplicateGXItemTextureEntry(ModelEditorScreen screen, FLVER2 flver, FLVER2.GXItem curItem)
    {
        Screen = screen;
        CurrentFLVER = flver;
        CurrentGXList = flver.GXLists[Screen.ModelHierarchy._selectedGXList];

        NewObject = curItem.Clone();
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        CurrentGXList.Add(NewObject);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        if (CurrentGXList.Count > 1)
            Screen.ModelHierarchy._subSelectedGXItemRow = 0;
        else
            Screen.ModelHierarchy._subSelectedGXItemRow = -1;

        CurrentGXList.Remove(NewObject);

        return ActionEvent.NoEvent;
    }
}

public class RemoveGXItemEntry : ViewportAction
{
    private FLVER2 CurrentFLVER;
    private ModelEditorScreen Screen;
    private FLVER2.GXList CurrentGXList;
    private FLVER2.GXItem StoredItem;
    private FLVER2.GXItem OldObject;

    public RemoveGXItemEntry(ModelEditorScreen screen, FLVER2 flver, FLVER2.GXItem curItem)
    {
        Screen = screen;
        CurrentFLVER = flver;
        CurrentGXList = flver.GXLists[Screen.ModelHierarchy._selectedGXList];

        StoredItem = curItem.Clone();
        OldObject = curItem;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        if (CurrentGXList.Count > 1)
            Screen.ModelHierarchy._subSelectedGXItemRow = 0;
        else
            Screen.ModelHierarchy._subSelectedGXItemRow = -1;

        CurrentGXList.Remove(OldObject);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        CurrentGXList.Add(StoredItem);

        return ActionEvent.NoEvent;
    }
}

// FaceSet
public class AddFaceSetEntry : ViewportAction
{
    private FLVER2 CurrentFLVER;
    private ModelEditorScreen Screen;
    private FLVER2.Mesh CurrentMesh;
    private FLVER2.FaceSet NewObject;

    public AddFaceSetEntry(ModelEditorScreen screen, FLVER2 flver)
    {
        Screen = screen;
        CurrentFLVER = flver;
        CurrentMesh = flver.Meshes[Screen.ModelHierarchy._selectedMesh];

        NewObject = new FLVER2.FaceSet();
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        CurrentMesh.FaceSets.Add(NewObject);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        if (CurrentMesh.FaceSets.Count > 1)
            Screen.ModelHierarchy._subSelectedFaceSetRow = 0;
        else
            Screen.ModelHierarchy._subSelectedFaceSetRow = -1;

        CurrentMesh.FaceSets.Remove(NewObject);

        return ActionEvent.NoEvent;
    }
}

public class DuplicateFaceSetEntry : ViewportAction
{
    private FLVER2 CurrentFLVER;
    private ModelEditorScreen Screen;
    private FLVER2.Mesh CurrentMesh;
    private FLVER2.FaceSet NewObject;

    public DuplicateFaceSetEntry(ModelEditorScreen screen, FLVER2 flver, FLVER2.FaceSet curItem)
    {
        Screen = screen;
        CurrentFLVER = flver;
        CurrentMesh = flver.Meshes[Screen.ModelHierarchy._selectedMesh];

        NewObject = curItem.Clone();
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        CurrentMesh.FaceSets.Add(NewObject);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        if (CurrentMesh.FaceSets.Count > 1)
            Screen.ModelHierarchy._subSelectedFaceSetRow = 0;
        else
            Screen.ModelHierarchy._subSelectedFaceSetRow = -1;

        CurrentMesh.FaceSets.Remove(NewObject);

        return ActionEvent.NoEvent;
    }
}

public class RemoveFaceSetEntry : ViewportAction
{
    private FLVER2 CurrentFLVER;
    private ModelEditorScreen Screen;
    private FLVER2.Mesh CurrentMesh;
    private FLVER2.FaceSet StoredItem;
    private FLVER2.FaceSet OldObject;

    public RemoveFaceSetEntry(ModelEditorScreen screen, FLVER2 flver, FLVER2.FaceSet curItem)
    {
        Screen = screen;
        CurrentFLVER = flver;
        CurrentMesh = flver.Meshes[Screen.ModelHierarchy._selectedMesh];

        StoredItem = curItem.Clone();
        OldObject = curItem;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        if (CurrentMesh.FaceSets.Count > 1)
            Screen.ModelHierarchy._subSelectedFaceSetRow = 0;
        else
            Screen.ModelHierarchy._subSelectedFaceSetRow = -1;

        CurrentMesh.FaceSets.Remove(OldObject);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        CurrentMesh.FaceSets.Add(StoredItem);

        return ActionEvent.NoEvent;
    }
}

// Vertex Buffer
public class AddVertexBufferEntry : ViewportAction
{
    private FLVER2 CurrentFLVER;
    private ModelEditorScreen Screen;
    private FLVER2.Mesh CurrentMesh;
    private FLVER2.VertexBuffer NewObject;

    public AddVertexBufferEntry(ModelEditorScreen screen, FLVER2 flver)
    {
        Screen = screen;
        CurrentFLVER = flver;
        CurrentMesh = flver.Meshes[Screen.ModelHierarchy._selectedMesh];

        NewObject = new FLVER2.VertexBuffer(0);
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        CurrentMesh.VertexBuffers.Add(NewObject);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        if (CurrentMesh.VertexBuffers.Count > 1)
            Screen.ModelHierarchy._subSelectedVertexBufferRow = 0;
        else
            Screen.ModelHierarchy._subSelectedVertexBufferRow = -1;

        CurrentMesh.VertexBuffers.Remove(NewObject);

        return ActionEvent.NoEvent;
    }
}

public class DuplicateVertexBufferEntry : ViewportAction
{
    private FLVER2 CurrentFLVER;
    private ModelEditorScreen Screen;
    private FLVER2.Mesh CurrentMesh;
    private FLVER2.VertexBuffer NewObject;

    public DuplicateVertexBufferEntry(ModelEditorScreen screen, FLVER2 flver, FLVER2.VertexBuffer curItem)
    {
        Screen = screen;
        CurrentFLVER = flver;
        CurrentMesh = flver.Meshes[Screen.ModelHierarchy._selectedMesh];

        NewObject = curItem.Clone();
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        CurrentMesh.VertexBuffers.Add(NewObject);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        if (CurrentMesh.VertexBuffers.Count > 1)
            Screen.ModelHierarchy._subSelectedVertexBufferRow = 0;
        else
            Screen.ModelHierarchy._subSelectedVertexBufferRow = -1;

        CurrentMesh.VertexBuffers.Remove(NewObject);

        return ActionEvent.NoEvent;
    }
}

public class RemoveVertexBufferEntry : ViewportAction
{
    private FLVER2 CurrentFLVER;
    private ModelEditorScreen Screen;
    private FLVER2.Mesh CurrentMesh;
    private FLVER2.VertexBuffer StoredItem;
    private FLVER2.VertexBuffer OldObject;

    public RemoveVertexBufferEntry(ModelEditorScreen screen, FLVER2 flver, FLVER2.VertexBuffer curItem)
    {
        Screen = screen;
        CurrentFLVER = flver;
        CurrentMesh = flver.Meshes[Screen.ModelHierarchy._selectedMesh];

        StoredItem = curItem.Clone();
        OldObject = curItem;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        if (CurrentMesh.VertexBuffers.Count > 1)
            Screen.ModelHierarchy._subSelectedVertexBufferRow = 0;
        else
            Screen.ModelHierarchy._subSelectedVertexBufferRow = -1;

        CurrentMesh.VertexBuffers.Remove(OldObject);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        CurrentMesh.VertexBuffers.Add(StoredItem);

        return ActionEvent.NoEvent;
    }
}


// Layout Member
public class AddLayoutMemberEntry : ViewportAction
{
    private FLVER2 CurrentFLVER;
    private ModelEditorScreen Screen;
    private FLVER2.BufferLayout CurrentBufferLayout;
    private FLVER.LayoutMember NewObject;

    public AddLayoutMemberEntry(ModelEditorScreen screen, FLVER2 flver)
    {
        Screen = screen;
        CurrentFLVER = flver;
        CurrentBufferLayout = flver.BufferLayouts[Screen.ModelHierarchy._selectedBufferLayout];

        NewObject = new FLVER.LayoutMember();
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        CurrentBufferLayout.Add(NewObject);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        if (CurrentBufferLayout.Count > 1)
            Screen.ModelHierarchy._subSelectedBufferLayoutMember = 0;
        else
            Screen.ModelHierarchy._subSelectedBufferLayoutMember = -1;

        CurrentBufferLayout.Remove(NewObject);

        return ActionEvent.NoEvent;
    }
}

public class DuplicateLayoutMemberEntry : ViewportAction
{
    private FLVER2 CurrentFLVER;
    private ModelEditorScreen Screen;
    private FLVER2.BufferLayout CurrentBufferLayout;
    private FLVER.LayoutMember NewObject;

    public DuplicateLayoutMemberEntry(ModelEditorScreen screen, FLVER2 flver, FLVER.LayoutMember curMember)
    {
        Screen = screen;
        CurrentFLVER = flver;
        CurrentBufferLayout = flver.BufferLayouts[Screen.ModelHierarchy._selectedBufferLayout];

        NewObject = curMember.Clone();
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        CurrentBufferLayout.Add(NewObject);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        if (CurrentBufferLayout.Count > 1)
            Screen.ModelHierarchy._subSelectedBufferLayoutMember = 0;
        else
            Screen.ModelHierarchy._subSelectedBufferLayoutMember = -1;

        CurrentBufferLayout.Remove(NewObject);

        return ActionEvent.NoEvent;
    }
}

public class RemoveLayoutMemberEntry : ViewportAction
{
    private FLVER2 CurrentFLVER;
    private ModelEditorScreen Screen;
    private FLVER2.BufferLayout CurrentBufferLayout;
    private FLVER.LayoutMember StoredMember;
    private FLVER.LayoutMember OldObject;

    public RemoveLayoutMemberEntry(ModelEditorScreen screen, FLVER2 flver, FLVER.LayoutMember curMember)
    {
        Screen = screen;
        CurrentFLVER = flver;
        CurrentBufferLayout = flver.BufferLayouts[Screen.ModelHierarchy._selectedBufferLayout];

        StoredMember = curMember.Clone();
        OldObject = curMember;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        if (CurrentBufferLayout.Count > 1)
            Screen.ModelHierarchy._subSelectedBufferLayoutMember = 0;
        else
            Screen.ModelHierarchy._subSelectedBufferLayoutMember = -1;

        CurrentBufferLayout.Remove(OldObject);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        CurrentBufferLayout.Add(StoredMember);

        return ActionEvent.NoEvent;
    }
}

// Anti-DRY, but too lazy to do it via reflection
public class UpdateProperty_FLVERHeader_BigEndian : ViewportAction
{
    private FLVERHeader Entry;
    private bool OldValue;
    private bool NewValue;

    public UpdateProperty_FLVERHeader_BigEndian(FLVERHeader entry, bool oldValue, bool newValue)
    {
        Entry = entry;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Entry.BigEndian = NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Entry.BigEndian = OldValue;

        return ActionEvent.NoEvent;
    }
}

public class UpdateProperty_FLVERHeader_Version : ViewportAction
{
    private FLVERHeader Entry;
    private int OldValue;
    private int NewValue;

    public UpdateProperty_FLVERHeader_Version(FLVERHeader entry, int oldValue, int newValue)
    {
        Entry = entry;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Entry.Version = NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Entry.Version = OldValue;

        return ActionEvent.NoEvent;
    }
}

public class UpdateProperty_FLVERHeader_BoundingBoxMin : ViewportAction
{
    private FLVERHeader Entry;
    private Vector3 OldValue;
    private Vector3 NewValue;

    public UpdateProperty_FLVERHeader_BoundingBoxMin(FLVERHeader entry, Vector3 oldValue, Vector3 newValue)
    {
        Entry = entry;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Entry.BoundingBoxMin = NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Entry.BoundingBoxMin = OldValue;

        return ActionEvent.NoEvent;
    }
}

public class UpdateProperty_FLVERHeader_BoundingBoxMax : ViewportAction
{
    private FLVERHeader Entry;
    private Vector3 OldValue;
    private Vector3 NewValue;

    public UpdateProperty_FLVERHeader_BoundingBoxMax(FLVERHeader entry, Vector3 oldValue, Vector3 newValue)
    {
        Entry = entry;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Entry.BoundingBoxMax = NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Entry.BoundingBoxMax = OldValue;

        return ActionEvent.NoEvent;
    }
}

public class UpdateProperty_FLVERHeader_Unicode : ViewportAction
{
    private FLVERHeader Entry;
    private bool OldValue;
    private bool NewValue;

    public UpdateProperty_FLVERHeader_Unicode(FLVERHeader entry, bool oldValue, bool newValue)
    {
        Entry = entry;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Entry.Unicode = NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Entry.Unicode = OldValue;

        return ActionEvent.NoEvent;
    }
}

// Dummy
public class UpdateProperty_FLVERDummy_Position : ViewportAction
{
    private FLVER.Dummy Entry;
    private Vector3 OldValue;
    private Vector3 NewValue;

    public UpdateProperty_FLVERDummy_Position(FLVER.Dummy entry, Vector3 oldValue, Vector3 newValue)
    {
        Entry = entry;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Entry.Position = NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Entry.Position = OldValue;

        return ActionEvent.NoEvent;
    }
}

public class UpdateProperty_FLVERDummy_Forward : ViewportAction
{
    private FLVER.Dummy Entry;
    private Vector3 OldValue;
    private Vector3 NewValue;

    public UpdateProperty_FLVERDummy_Forward(FLVER.Dummy entry, Vector3 oldValue, Vector3 newValue)
    {
        Entry = entry;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Entry.Forward = NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Entry.Forward = OldValue;

        return ActionEvent.NoEvent;
    }
}

public class UpdateProperty_FLVERDummy_Upward : ViewportAction
{
    private FLVER.Dummy Entry;
    private Vector3 OldValue;
    private Vector3 NewValue;

    public UpdateProperty_FLVERDummy_Upward(FLVER.Dummy entry, Vector3 oldValue, Vector3 newValue)
    {
        Entry = entry;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Entry.Upward = NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Entry.Upward = OldValue;

        return ActionEvent.NoEvent;
    }
}

public class UpdateProperty_FLVERDummy_ReferenceID : ViewportAction
{
    private FLVER.Dummy Entry;
    private int OldValue;
    private int NewValue;

    public UpdateProperty_FLVERDummy_ReferenceID(FLVER.Dummy entry, int oldValue, int newValue)
    {
        Entry = entry;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        if (NewValue > short.MaxValue)
            NewValue = short.MaxValue;

        Entry.ReferenceID = (short)NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        if (OldValue > short.MaxValue)
            OldValue = short.MaxValue;

        Entry.ReferenceID = (short)OldValue;

        return ActionEvent.NoEvent;
    }
}

public class UpdateProperty_FLVERDummy_ParentBoneIndex : ViewportAction
{
    private FLVER.Dummy Entry;
    private int OldValue;
    private int NewValue;

    public UpdateProperty_FLVERDummy_ParentBoneIndex(FLVER.Dummy entry, int oldValue, int newValue)
    {
        Entry = entry;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        if (NewValue > short.MaxValue)
            NewValue = short.MaxValue;

        Entry.ParentBoneIndex = (short)NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        if (OldValue > short.MaxValue)
            OldValue = short.MaxValue;

        Entry.ParentBoneIndex = (short)OldValue;

        return ActionEvent.NoEvent;
    }
}

public class UpdateProperty_FLVERDummy_AttachBoneIndex : ViewportAction
{
    private FLVER.Dummy Entry;
    private int OldValue;
    private int NewValue;

    public UpdateProperty_FLVERDummy_AttachBoneIndex(FLVER.Dummy entry, int oldValue, int newValue)
    {
        Entry = entry;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        if (NewValue > short.MaxValue)
            NewValue = short.MaxValue;

        Entry.AttachBoneIndex = (short)NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        if (OldValue > short.MaxValue)
            OldValue = short.MaxValue;

        Entry.AttachBoneIndex = (short)OldValue;

        return ActionEvent.NoEvent;
    }
}

public class UpdateProperty_FLVERDummy_Flag1 : ViewportAction
{
    private FLVER.Dummy Entry;
    private bool OldValue;
    private bool NewValue;

    public UpdateProperty_FLVERDummy_Flag1(FLVER.Dummy entry, bool oldValue, bool newValue)
    {
        Entry = entry;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Entry.Flag1 = NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Entry.Flag1 = OldValue;

        return ActionEvent.NoEvent;
    }
}

public class UpdateProperty_FLVERDummy_UseUpwardVector : ViewportAction
{
    private FLVER.Dummy Entry;
    private bool OldValue;
    private bool NewValue;

    public UpdateProperty_FLVERDummy_UseUpwardVector(FLVER.Dummy entry, bool oldValue, bool newValue)
    {
        Entry = entry;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Entry.UseUpwardVector = NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Entry.UseUpwardVector = OldValue;

        return ActionEvent.NoEvent;
    }
}

public class UpdateProperty_FLVERDummy_Unk30 : ViewportAction
{
    private FLVER.Dummy Entry;
    private int OldValue;
    private int NewValue;

    public UpdateProperty_FLVERDummy_Unk30(FLVER.Dummy entry, int oldValue, int newValue)
    {
        Entry = entry;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Entry.Unk30 = NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Entry.Unk30 = OldValue;

        return ActionEvent.NoEvent;
    }
}

public class UpdateProperty_FLVERDummy_Unk34 : ViewportAction
{
    private FLVER.Dummy Entry;
    private int OldValue;
    private int NewValue;

    public UpdateProperty_FLVERDummy_Unk34(FLVER.Dummy entry, int oldValue, int newValue)
    {
        Entry = entry;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Entry.Unk34 = NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Entry.Unk34 = OldValue;

        return ActionEvent.NoEvent;
    }
}

// Material
public class UpdateProperty_FLVERMaterial_Name : ViewportAction
{
    private FLVER2.Material Entry;
    private string OldValue;
    private string NewValue;

    public UpdateProperty_FLVERMaterial_Name(FLVER2.Material entry, string oldValue, string newValue)
    {
        Entry = entry;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Entry.Name = NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Entry.Name = OldValue;

        return ActionEvent.NoEvent;
    }
}
public class UpdateProperty_FLVERMaterial_MTD : ViewportAction
{
    private FLVER2.Material Entry;
    private string OldValue;
    private string NewValue;

    public UpdateProperty_FLVERMaterial_MTD(FLVER2.Material entry, string oldValue, string newValue)
    {
        Entry = entry;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Entry.MTD = NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Entry.MTD = OldValue;

        return ActionEvent.NoEvent;
    }
}
public class UpdateProperty_FLVERMaterial_GXIndex : ViewportAction
{
    private FLVER2.Material Entry;
    private int OldValue;
    private int NewValue;

    public UpdateProperty_FLVERMaterial_GXIndex(FLVER2.Material entry, int oldValue, int newValue)
    {
        Entry = entry;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Entry.GXIndex = NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Entry.GXIndex = OldValue;

        return ActionEvent.NoEvent;
    }
}
public class UpdateProperty_FLVERMaterial_MTDIndex : ViewportAction
{
    private FLVER2.Material Entry;
    private int OldValue;
    private int NewValue;

    public UpdateProperty_FLVERMaterial_MTDIndex(FLVER2.Material entry, int oldValue, int newValue)
    {
        Entry = entry;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Entry.Index = NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Entry.Index = OldValue;

        return ActionEvent.NoEvent;
    }
}

// Material -> Texture
public class UpdateProperty_FLVERMaterial_Texture_Type : ViewportAction
{
    private FLVER2.Texture Entry;
    private string OldValue;
    private string NewValue;

    public UpdateProperty_FLVERMaterial_Texture_Type(FLVER2.Texture entry, string oldValue, string newValue)
    {
        Entry = entry;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Entry.Type = NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Entry.Type = OldValue;

        return ActionEvent.NoEvent;
    }
}
public class UpdateProperty_FLVERMaterial_Texture_Path : ViewportAction
{
    private FLVER2.Texture Entry;
    private string OldValue;
    private string NewValue;

    public UpdateProperty_FLVERMaterial_Texture_Path(FLVER2.Texture entry, string oldValue, string newValue)
    {
        Entry = entry;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Entry.Path = NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Entry.Path = OldValue;

        return ActionEvent.NoEvent;
    }
}
public class UpdateProperty_FLVERMaterial_Texture_Scale : ViewportAction
{
    private FLVER2.Texture Entry;
    private Vector2 OldValue;
    private Vector2 NewValue;

    public UpdateProperty_FLVERMaterial_Texture_Scale(FLVER2.Texture entry, Vector2 oldValue, Vector2 newValue)
    {
        Entry = entry;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Entry.Scale = NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Entry.Scale = OldValue;

        return ActionEvent.NoEvent;
    }
}

// GX List -> GX Item
public class UpdateProperty_FLVERGXList_GXItem_ID : ViewportAction
{
    private FLVER2.GXItem Entry;
    private string OldValue;
    private string NewValue;

    public UpdateProperty_FLVERGXList_GXItem_ID(FLVER2.GXItem entry, string oldValue, string newValue)
    {
        Entry = entry;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Entry.ID = NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Entry.ID = OldValue;

        return ActionEvent.NoEvent;
    }
}
public class UpdateProperty_FLVERGXList_GXItem_Unk04 : ViewportAction
{
    private FLVER2.GXItem Entry;
    private int OldValue;
    private int NewValue;

    public UpdateProperty_FLVERGXList_GXItem_Unk04(FLVER2.GXItem entry, int oldValue, int newValue)
    {
        Entry = entry;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Entry.Unk04 = NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Entry.Unk04 = OldValue;

        return ActionEvent.NoEvent;
    }
}
public class UpdateProperty_FLVERGXList_GXItem_Data : ViewportAction
{
    private FLVER2.GXItem Entry;
    private byte OldValue;
    private int NewValue;
    private int Index;

    public UpdateProperty_FLVERGXList_GXItem_Data(FLVER2.GXItem entry, byte oldValue, int newValue, int index)
    {
        Entry = entry;
        OldValue = oldValue;
        NewValue = newValue;
        Index = index;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        if(NewValue > byte.MaxValue)
            NewValue = byte.MaxValue;

        Entry.Data[Index] = (byte)NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Entry.Data[Index] = OldValue;

        return ActionEvent.NoEvent;
    }
}

// Node
public class UpdateProperty_FLVERNode_Name : ViewportAction
{
    private FLVER.Node Entry;
    private string OldValue;
    private string NewValue;

    public UpdateProperty_FLVERNode_Name(FLVER.Node entry, string oldValue, string newValue)
    {
        Entry = entry;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Entry.Name = NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Entry.Name = OldValue;

        return ActionEvent.NoEvent;
    }
}
public class UpdateProperty_FLVERNode_ParentIndex : ViewportAction
{
    private FLVER.Node Entry;
    private int OldValue;
    private int NewValue;

    public UpdateProperty_FLVERNode_ParentIndex(FLVER.Node entry, int oldValue, int newValue)
    {
        Entry = entry;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        if(NewValue > short.MaxValue)
            NewValue = short.MaxValue;

        Entry.ParentIndex = (short)NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Entry.ParentIndex = (short)OldValue;

        return ActionEvent.NoEvent;
    }
}
public class UpdateProperty_FLVERNode_FirstChildIndex : ViewportAction
{
    private FLVER.Node Entry;
    private int OldValue;
    private int NewValue;

    public UpdateProperty_FLVERNode_FirstChildIndex(FLVER.Node entry, int oldValue, int newValue)
    {
        Entry = entry;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        if (NewValue > short.MaxValue)
            NewValue = short.MaxValue;

        Entry.FirstChildIndex = (short)NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Entry.FirstChildIndex = (short)OldValue;

        return ActionEvent.NoEvent;
    }
}
public class UpdateProperty_FLVERNode_NextSiblingIndex : ViewportAction
{
    private FLVER.Node Entry;
    private int OldValue;
    private int NewValue;

    public UpdateProperty_FLVERNode_NextSiblingIndex(FLVER.Node entry, int oldValue, int newValue)
    {
        Entry = entry;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        if (NewValue > short.MaxValue)
            NewValue = short.MaxValue;

        Entry.NextSiblingIndex = (short)NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Entry.NextSiblingIndex = (short)OldValue;

        return ActionEvent.NoEvent;
    }
}
public class UpdateProperty_FLVERNode_PreviousSiblingIndex : ViewportAction
{
    private FLVER.Node Entry;
    private int OldValue;
    private int NewValue;

    public UpdateProperty_FLVERNode_PreviousSiblingIndex(FLVER.Node entry, int oldValue, int newValue)
    {
        Entry = entry;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        if (NewValue > short.MaxValue)
            NewValue = short.MaxValue;

        Entry.PreviousSiblingIndex = (short)NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Entry.PreviousSiblingIndex = (short)OldValue;

        return ActionEvent.NoEvent;
    }
}
public class UpdateProperty_FLVERNode_Translation : ViewportAction
{
    private FLVER.Node Entry;
    private Vector3 OldValue;
    private Vector3 NewValue;

    public UpdateProperty_FLVERNode_Translation(FLVER.Node entry, Vector3 oldValue, Vector3 newValue)
    {
        Entry = entry;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Entry.Position = NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Entry.Position = OldValue;

        return ActionEvent.NoEvent;
    }
}
public class UpdateProperty_FLVERNode_Rotation : ViewportAction
{
    private FLVER.Node Entry;
    private Vector3 OldValue;
    private Vector3 NewValue;

    public UpdateProperty_FLVERNode_Rotation(FLVER.Node entry, Vector3 oldValue, Vector3 newValue)
    {
        Entry = entry;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Entry.Rotation = NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Entry.Rotation = OldValue;

        return ActionEvent.NoEvent;
    }
}

public class UpdateProperty_FLVERNode_Scale : ViewportAction
{
    private FLVER.Node Entry;
    private Vector3 OldValue;
    private Vector3 NewValue;

    public UpdateProperty_FLVERNode_Scale(FLVER.Node entry, Vector3 oldValue, Vector3 newValue)
    {
        Entry = entry;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Entry.Scale = NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Entry.Scale = OldValue;

        return ActionEvent.NoEvent;
    }
}
public class UpdateProperty_FLVERNode_BoundingBoxMin : ViewportAction
{
    private FLVER.Node Entry;
    private Vector3 OldValue;
    private Vector3 NewValue;

    public UpdateProperty_FLVERNode_BoundingBoxMin(FLVER.Node entry, Vector3 oldValue, Vector3 newValue)
    {
        Entry = entry;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Entry.BoundingBoxMin = NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Entry.BoundingBoxMin = OldValue;

        return ActionEvent.NoEvent;
    }
}
public class UpdateProperty_FLVERNode_BoundingBoxMax : ViewportAction
{
    private FLVER.Node Entry;
    private Vector3 OldValue;
    private Vector3 NewValue;

    public UpdateProperty_FLVERNode_BoundingBoxMax(FLVER.Node entry, Vector3 oldValue, Vector3 newValue)
    {
        Entry = entry;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Entry.BoundingBoxMax = NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Entry.BoundingBoxMax = OldValue;

        return ActionEvent.NoEvent;
    }
}
public class UpdateProperty_FLVERNode_Flags : ViewportAction
{
    private FLVER.Node Entry;
    private int OldValue;
    private int NewValue;

    public UpdateProperty_FLVERNode_Flags(FLVER.Node entry, int oldValue, int newValue)
    {
        Entry = entry;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Entry.Flags = (FLVER.Node.NodeFlags)NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Entry.Flags = (FLVER.Node.NodeFlags)OldValue;

        return ActionEvent.NoEvent;
    }
}

// Mesh
public class UpdateProperty_FLVERMesh_UseBoneWeights : ViewportAction
{
    private FLVER2.Mesh Entry;
    private bool OldValue;
    private bool NewValue;

    public UpdateProperty_FLVERMesh_UseBoneWeights(FLVER2.Mesh entry, bool oldValue, bool newValue)
    {
        Entry = entry;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Entry.UseBoneWeights = NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Entry.UseBoneWeights = OldValue;

        return ActionEvent.NoEvent;
    }
}
public class UpdateProperty_FLVERMesh_MaterialIndex : ViewportAction
{
    private FLVER2.Mesh Entry;
    private int OldValue;
    private int NewValue;

    public UpdateProperty_FLVERMesh_MaterialIndex(FLVER2.Mesh entry, int oldValue, int newValue)
    {
        Entry = entry;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Entry.MaterialIndex = NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Entry.MaterialIndex = OldValue;

        return ActionEvent.NoEvent;
    }
}
public class UpdateProperty_FLVERMesh_NodeIndex : ViewportAction
{
    private FLVER2.Mesh Entry;
    private int OldValue;
    private int NewValue;

    public UpdateProperty_FLVERMesh_NodeIndex(FLVER2.Mesh entry, int oldValue, int newValue)
    {
        Entry = entry;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Entry.NodeIndex = NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Entry.NodeIndex = OldValue;

        return ActionEvent.NoEvent;
    }
}

// Mesh -> Face Set
public class UpdateProperty_FLVERMesh_FaceSet_Flags : ViewportAction
{
    private FLVER2.FaceSet Entry;
    private int OldValue;
    private int NewValue;

    public UpdateProperty_FLVERMesh_FaceSet_Flags(FLVER2.FaceSet entry, int oldValue, int newValue)
    {
        Entry = entry;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Entry.Flags = (FLVER2.FaceSet.FSFlags)NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Entry.Flags = (FLVER2.FaceSet.FSFlags)OldValue;

        return ActionEvent.NoEvent;
    }
}
public class UpdateProperty_FLVERMesh_FaceSet_TriangleStrip : ViewportAction
{
    private FLVER2.FaceSet Entry;
    private bool OldValue;
    private bool NewValue;

    public UpdateProperty_FLVERMesh_FaceSet_TriangleStrip(FLVER2.FaceSet entry, bool oldValue, bool newValue)
    {
        Entry = entry;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Entry.TriangleStrip = NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Entry.TriangleStrip = OldValue;

        return ActionEvent.NoEvent;
    }
}
public class UpdateProperty_FLVERMesh_FaceSet_CullBackfaces : ViewportAction
{
    private FLVER2.FaceSet Entry;
    private bool OldValue;
    private bool NewValue;

    public UpdateProperty_FLVERMesh_FaceSet_CullBackfaces(FLVER2.FaceSet entry, bool oldValue, bool newValue)
    {
        Entry = entry;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Entry.CullBackfaces = NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Entry.CullBackfaces = OldValue;

        return ActionEvent.NoEvent;
    }
}
public class UpdateProperty_FLVERMesh_FaceSet_Unk06 : ViewportAction
{
    private FLVER2.FaceSet Entry;
    private int OldValue;
    private int NewValue;

    public UpdateProperty_FLVERMesh_FaceSet_Unk06(FLVER2.FaceSet entry, int oldValue, int newValue)
    {
        Entry = entry;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        if(NewValue > short.MaxValue)
            NewValue = short.MaxValue;

        Entry.Unk06 = (short)NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        if (OldValue > short.MaxValue)
            OldValue = short.MaxValue;

        Entry.Unk06 = (short)OldValue;

        return ActionEvent.NoEvent;
    }
}

// Mesh -> Vertex Buffer
public class UpdateProperty_FLVERMesh_VertexBuffer_VertexBuffer : ViewportAction
{
    private FLVER2.VertexBuffer Entry;
    private int OldValue;
    private int NewValue;

    public UpdateProperty_FLVERMesh_VertexBuffer_VertexBuffer(FLVER2.VertexBuffer entry, int oldValue, int newValue)
    {
        Entry = entry;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Entry.LayoutIndex = NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Entry.LayoutIndex = OldValue;

        return ActionEvent.NoEvent;
    }
}

// Mesh -> Bounding Boxes
public class UpdateProperty_FLVERMesh_BoundingBoxes_Min : ViewportAction
{
    private FLVER2.Mesh.BoundingBoxes Entry;
    private Vector3 OldValue;
    private Vector3 NewValue;

    public UpdateProperty_FLVERMesh_BoundingBoxes_Min(FLVER2.Mesh.BoundingBoxes entry, Vector3 oldValue, Vector3 newValue)
    {
        Entry = entry;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Entry.Min = NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Entry.Min = OldValue;

        return ActionEvent.NoEvent;
    }
}
public class UpdateProperty_FLVERMesh_BoundingBoxes_Max : ViewportAction
{
    private FLVER2.Mesh.BoundingBoxes Entry;
    private Vector3 OldValue;
    private Vector3 NewValue;

    public UpdateProperty_FLVERMesh_BoundingBoxes_Max(FLVER2.Mesh.BoundingBoxes entry, Vector3 oldValue, Vector3 newValue)
    {
        Entry = entry;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Entry.Max = NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Entry.Max = OldValue;

        return ActionEvent.NoEvent;
    }
}
public class UpdateProperty_FLVERMesh_BoundingBoxes_Unk : ViewportAction
{
    private FLVER2.Mesh.BoundingBoxes Entry;
    private Vector3 OldValue;
    private Vector3 NewValue;

    public UpdateProperty_FLVERMesh_BoundingBoxes_Unk(FLVER2.Mesh.BoundingBoxes entry, Vector3 oldValue, Vector3 newValue)
    {
        Entry = entry;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Entry.Unk = NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Entry.Unk = OldValue;

        return ActionEvent.NoEvent;
    }
}

// Buffer Layout -> Layout Member
public class UpdateProperty_FLVERBufferLayout_LayoutMember_Unk00 : ViewportAction
{
    private FLVER.LayoutMember Entry;
    private int OldValue;
    private int NewValue;

    public UpdateProperty_FLVERBufferLayout_LayoutMember_Unk00(FLVER.LayoutMember entry, int oldValue, int newValue)
    {
        Entry = entry;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Entry.Unk00 = NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Entry.Unk00 = OldValue;

        return ActionEvent.NoEvent;
    }
}
public class UpdateProperty_FLVERBufferLayout_LayoutMember_Type : ViewportAction
{
    private FLVER.LayoutMember Entry;
    private int OldValue;
    private int NewValue;

    public UpdateProperty_FLVERBufferLayout_LayoutMember_Type(FLVER.LayoutMember entry, int oldValue, int newValue)
    {
        Entry = entry;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Entry.Type = (FLVER.LayoutType)NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Entry.Type = (FLVER.LayoutType)OldValue;

        return ActionEvent.NoEvent;
    }
}
public class UpdateProperty_FLVERBufferLayout_LayoutMember_Semantic : ViewportAction
{
    private FLVER.LayoutMember Entry;
    private int OldValue;
    private int NewValue;

    public UpdateProperty_FLVERBufferLayout_LayoutMember_Semantic(FLVER.LayoutMember entry, int oldValue, int newValue)
    {
        Entry = entry;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Entry.Semantic = (FLVER.LayoutSemantic)NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Entry.Semantic = (FLVER.LayoutSemantic)OldValue;

        return ActionEvent.NoEvent;
    }
}
public class UpdateProperty_FLVERBufferLayout_LayoutMember_Index : ViewportAction
{
    private FLVER.LayoutMember Entry;
    private int OldValue;
    private int NewValue;

    public UpdateProperty_FLVERBufferLayout_LayoutMember_Index(FLVER.LayoutMember entry, int oldValue, int newValue)
    {
        Entry = entry;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Entry.Index = NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Entry.Index = OldValue;

        return ActionEvent.NoEvent;
    }
}

// Skeleton Bone
public class UpdateProperty_FLVERSkeleton_Bone_ParentIndex : ViewportAction
{
    private FLVER2.SkeletonSet.Bone Entry;
    private int OldValue;
    private int NewValue;

    public UpdateProperty_FLVERSkeleton_Bone_ParentIndex(FLVER2.SkeletonSet.Bone entry, int oldValue, int newValue)
    {
        Entry = entry;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        if(NewValue > short.MaxValue)
            NewValue = short.MaxValue;

        Entry.ParentIndex = (short)NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        if (OldValue > short.MaxValue)
            OldValue = short.MaxValue;

        Entry.ParentIndex = (short)OldValue;

        return ActionEvent.NoEvent;
    }
}
public class UpdateProperty_FLVERSkeleton_Bone_FirstChildIndex : ViewportAction
{
    private FLVER2.SkeletonSet.Bone Entry;
    private int OldValue;
    private int NewValue;

    public UpdateProperty_FLVERSkeleton_Bone_FirstChildIndex(FLVER2.SkeletonSet.Bone entry, int oldValue, int newValue)
    {
        Entry = entry;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        if (NewValue > short.MaxValue)
            NewValue = short.MaxValue;

        Entry.FirstChildIndex = (short)NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        if (OldValue > short.MaxValue)
            OldValue = short.MaxValue;

        Entry.FirstChildIndex = (short)OldValue;

        return ActionEvent.NoEvent;
    }
}
public class UpdateProperty_FLVERSkeleton_Bone_NextSiblingIndex : ViewportAction
{
    private FLVER2.SkeletonSet.Bone Entry;
    private int OldValue;
    private int NewValue;

    public UpdateProperty_FLVERSkeleton_Bone_NextSiblingIndex(FLVER2.SkeletonSet.Bone entry, int oldValue, int newValue)
    {
        Entry = entry;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        if (NewValue > short.MaxValue)
            NewValue = short.MaxValue;

        Entry.NextSiblingIndex = (short)NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        if (OldValue > short.MaxValue)
            OldValue = short.MaxValue;

        Entry.NextSiblingIndex = (short)OldValue;

        return ActionEvent.NoEvent;
    }
}
public class UpdateProperty_FLVERSkeleton_Bone_PreviousSiblingIndex : ViewportAction
{
    private FLVER2.SkeletonSet.Bone Entry;
    private int OldValue;
    private int NewValue;

    public UpdateProperty_FLVERSkeleton_Bone_PreviousSiblingIndex(FLVER2.SkeletonSet.Bone entry, int oldValue, int newValue)
    {
        Entry = entry;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        if (NewValue > short.MaxValue)
            NewValue = short.MaxValue;

        Entry.PreviousSiblingIndex = (short)NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        if (OldValue > short.MaxValue)
            OldValue = short.MaxValue;

        Entry.PreviousSiblingIndex = (short)OldValue;

        return ActionEvent.NoEvent;
    }
}
public class UpdateProperty_FLVERSkeleton_Bone_NodeIndex : ViewportAction
{
    private FLVER2.SkeletonSet.Bone Entry;
    private int OldValue;
    private int NewValue;

    public UpdateProperty_FLVERSkeleton_Bone_NodeIndex(FLVER2.SkeletonSet.Bone entry, int oldValue, int newValue)
    {
        Entry = entry;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Entry.NodeIndex = NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Entry.NodeIndex = OldValue;

        return ActionEvent.NoEvent;
    }
}