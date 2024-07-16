using DotNext.Collections.Generic;
using StudioCore.Editors.MapEditor;
using StudioCore.Formats.PureFLVER;
using StudioCore.Formats.PureFLVER.FLVER2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

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
    private SoulsFormats.FLVER.Dummy Dummy;

    public DummyPositionChange(Entity node, Vector3 newPosition)
    {
        Node = node;
        Dummy = (SoulsFormats.FLVER.Dummy)node.WrappedObject;
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
    private SoulsFormats.FLVER.Bone Bone;

    public BoneTransformChange(Entity node, Vector3 newPosition, Vector3 newRotation, Vector3 newScale)
    {
        Node = node;
        Bone = (SoulsFormats.FLVER.Bone)node.WrappedObject;
        OldPosition = Bone.Position;
        NewPosition = newPosition;
        OldRotation = Bone.Rotation;
        NewRotation = newRotation;
        OldScale = Bone.Scale;
        NewScale = newScale;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Bone.Position = NewPosition;
        Bone.Rotation = NewRotation;
        Bone.Scale = NewScale;
        Node.UpdateRenderModel();

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Bone.Position = OldPosition;
        Bone.Rotation = OldRotation;
        Bone.Scale = OldScale;
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

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Screen.ModelHierarchy._selectedDummy = PreviousSelectionIndex;
        CurrentFLVER.Dummies.RemoveAt(Index);

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

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Screen.ModelHierarchy._selectedDummy = PreviousSelectionIndex;
        CurrentFLVER.Dummies.RemoveAt(Index);

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

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        CurrentFLVER.Dummies.Insert(Index, RemovedObject);
        Screen.ModelHierarchy._selectedDummy = PreviousSelectionIndex;

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

        foreach (var entry in DupedObjects)
        {
            CurrentFLVER.Dummies.Add(entry);
        }

        Multiselect.StoredIndices = new List<int>();

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        for (int i = 0; i < DupedObjects.Count; i++)
        {
            CurrentFLVER.Dummies.Remove(DupedObjects[i]);
        }

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

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        for(int i = 0; i < StoredIndices.Count; i++)
        {
            CurrentFLVER.Dummies.Insert(StoredIndices[i], StoredObjects[i]);
        }

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

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Screen.ModelHierarchy._selectedNode = PreviousSelectionIndex;
        CurrentFLVER.Nodes.RemoveAt(Index);

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

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Screen.ModelHierarchy._selectedNode = PreviousSelectionIndex;
        CurrentFLVER.Nodes.RemoveAt(Index);

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

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        CurrentFLVER.Nodes.Insert(Index, RemovedObject);
        Screen.ModelHierarchy._selectedNode = PreviousSelectionIndex;

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

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        for (int i = 0; i < DupedObjects.Count; i++)
        {
            CurrentFLVER.Nodes.Remove(DupedObjects[i]);
        }

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

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        for (int i = 0; i < StoredIndices.Count; i++)
        {
            CurrentFLVER.Nodes.Insert(StoredIndices[i], StoredObjects[i]);
        }

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

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Screen.ModelHierarchy._selectedMesh = PreviousSelectionIndex;
        CurrentFLVER.Meshes.RemoveAt(Index);

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

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Screen.ModelHierarchy._selectedMesh = PreviousSelectionIndex;
        CurrentFLVER.Meshes.RemoveAt(Index);

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

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        CurrentFLVER.Meshes.Insert(Index, RemovedObject);
        Screen.ModelHierarchy._selectedMesh = PreviousSelectionIndex;

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

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        for (int i = 0; i < DupedObjects.Count; i++)
        {
            CurrentFLVER.Meshes.Remove(DupedObjects[i]);
        }

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

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        for (int i = 0; i < StoredIndices.Count; i++)
        {
            CurrentFLVER.Meshes.Insert(StoredIndices[i], StoredObjects[i]);
        }

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