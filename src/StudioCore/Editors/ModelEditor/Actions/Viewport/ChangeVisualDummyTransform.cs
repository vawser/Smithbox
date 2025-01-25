using SoulsFormats;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.MapEditor.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor.Actions.Viewport;

public class ChangeVisualDummyTransform : ViewportAction
{
    private Entity Node;
    private Vector3 OldPosition;
    private Vector3 NewPosition;
    private FLVER.Dummy Dummy;

    public ChangeVisualDummyTransform(Entity node, Vector3 newPosition)
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