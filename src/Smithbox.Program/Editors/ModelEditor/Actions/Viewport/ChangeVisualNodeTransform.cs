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

public class ChangeVisualNodeTransform : ViewportAction
{
    private ModelEditorScreen Editor;

    private Entity Node;
    private Vector3 OldTranslation;
    private Vector3 NewTranslation;
    private Vector3 OldRotation;
    private Vector3 NewRotation;
    private Vector3 OldScale;
    private Vector3 NewScale;
    private FLVER.Node BoneNode;

    public ChangeVisualNodeTransform(ModelEditorScreen editor, Entity node, Vector3 newTranslation, Vector3 newRotation, Vector3 newScale)
    {
        Editor = editor;
        Node = node;
        BoneNode = (FLVER.Node)node.WrappedObject;
        OldTranslation = BoneNode.Translation;
        NewTranslation = newTranslation;
        OldRotation = BoneNode.Rotation;
        NewRotation = newRotation;
        OldScale = BoneNode.Scale;
        NewScale = newScale;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        BoneNode.Translation = NewTranslation;
        BoneNode.Rotation = NewRotation;
        BoneNode.Scale = NewScale;
        Node.UpdateRenderModel(Editor);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        BoneNode.Translation = OldTranslation;
        BoneNode.Rotation = OldRotation;
        BoneNode.Scale = OldScale;
        Node.UpdateRenderModel(Editor);

        return ActionEvent.NoEvent;
    }
    public override string GetEditMessage()
    {
        return "";
    }
}
