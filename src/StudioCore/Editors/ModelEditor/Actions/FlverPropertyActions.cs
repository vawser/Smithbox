using SoulsFormats;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.ModelEditor.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static SoulsFormats.FLVER2;

namespace StudioCore.Editors.ModelEditor.Actions;

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
    public override string GetEditMessage()
    {
        return "";
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
    public override string GetEditMessage()
    {
        return "";
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
    public override string GetEditMessage()
    {
        return "";
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
    public override string GetEditMessage()
    {
        return "";
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
    public override string GetEditMessage()
    {
        return "";
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
    public override string GetEditMessage()
    {
        return "";
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
    public override string GetEditMessage()
    {
        return "";
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
    public override string GetEditMessage()
    {
        return "";
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
    public override string GetEditMessage()
    {
        return "";
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
    public override string GetEditMessage()
    {
        return "";
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
    public override string GetEditMessage()
    {
        return "";
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
    public override string GetEditMessage()
    {
        return "";
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
    public override string GetEditMessage()
    {
        return "";
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
    public override string GetEditMessage()
    {
        return "";
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
    public override string GetEditMessage()
    {
        return "";
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
    public override string GetEditMessage()
    {
        return "";
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
    public override string GetEditMessage()
    {
        return "";
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
    public override string GetEditMessage()
    {
        return "";
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
    public override string GetEditMessage()
    {
        return "";
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
    public override string GetEditMessage()
    {
        return "";
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
    public override string GetEditMessage()
    {
        return "";
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
    public override string GetEditMessage()
    {
        return "";
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
    public override string GetEditMessage()
    {
        return "";
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
    public override string GetEditMessage()
    {
        return "";
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
    public override string GetEditMessage()
    {
        return "";
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
    public override string GetEditMessage()
    {
        return "";
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
    public override string GetEditMessage()
    {
        return "";
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
    public override string GetEditMessage()
    {
        return "";
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
    public override string GetEditMessage()
    {
        return "";
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
    public override string GetEditMessage()
    {
        return "";
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
        Entry.Translation = NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Entry.Translation = OldValue;

        return ActionEvent.NoEvent;
    }
    public override string GetEditMessage()
    {
        return "";
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
    public override string GetEditMessage()
    {
        return "";
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
    public override string GetEditMessage()
    {
        return "";
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
    public override string GetEditMessage()
    {
        return "";
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
    public override string GetEditMessage()
    {
        return "";
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
    public override string GetEditMessage()
    {
        return "";
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
    public override string GetEditMessage()
    {
        return "";
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
    public override string GetEditMessage()
    {
        return "";
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
    public override string GetEditMessage()
    {
        return "";
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
    public override string GetEditMessage()
    {
        return "";
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
    public override string GetEditMessage()
    {
        return "";
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
    public override string GetEditMessage()
    {
        return "";
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
    public override string GetEditMessage()
    {
        return "";
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
    public override string GetEditMessage()
    {
        return "";
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
    public override string GetEditMessage()
    {
        return "";
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
    public override string GetEditMessage()
    {
        return "";
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
    public override string GetEditMessage()
    {
        return "";
    }
}

// Buffer Layout -> Layout Member
public class UpdateProperty_FLVERBufferLayout_LayoutMember_Unk00 : ViewportAction
{
    private FLVER.LayoutMember Entry;
    private short OldValue;
    private short NewValue;

    public UpdateProperty_FLVERBufferLayout_LayoutMember_Unk00(FLVER.LayoutMember entry, short oldValue, short newValue)
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
    public override string GetEditMessage()
    {
        return "";
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
    public override string GetEditMessage()
    {
        return "";
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
    public override string GetEditMessage()
    {
        return "";
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
    public override string GetEditMessage()
    {
        return "";
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
    public override string GetEditMessage()
    {
        return "";
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
    public override string GetEditMessage()
    {
        return "";
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
    public override string GetEditMessage()
    {
        return "";
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
    public override string GetEditMessage()
    {
        return "";
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
    public override string GetEditMessage()
    {
        return "";
    }
}

