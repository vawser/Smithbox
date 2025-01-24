using SoulsFormats;
using StudioCore.Editors.MapEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor.Actions.Viewport;

public class ChangeVisualNodeTransform : ViewportAction
{
    private Entity Node;
    private Vector3 OldPosition;
    private Vector3 NewPosition;
    private Vector3 OldRotation;
    private Vector3 NewRotation;
    private Vector3 OldScale;
    private Vector3 NewScale;
    private FLVER.Node BoneNode;

    public ChangeVisualNodeTransform(Entity node, Vector3 newPosition, Vector3 newRotation, Vector3 newScale)
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
