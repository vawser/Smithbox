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
    private ModelEditorScreen Editor;

    private Entity Node;
    private Vector3 OldPosition;
    private Vector3 NewPosition;
    private FLVER.Dummy Dummy;

    public ChangeVisualDummyTransform(ModelEditorScreen editor, Entity node, Vector3 newPosition)
    {
        Editor = editor;
        Node = node;
        Dummy = (FLVER.Dummy)node.WrappedObject;
        OldPosition = Dummy.Position;
        NewPosition = newPosition;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Dummy.Position = NewPosition;
        Node.UpdateRenderModel(Editor);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Dummy.Position = OldPosition;
        Node.UpdateRenderModel(Editor);

        return ActionEvent.NoEvent;
    }
    public override string GetEditMessage()
    {
        return "";
    }
}