using SoulsFormats;
using StudioCore.Editors.MapEditor.Actions.Viewport;

namespace StudioCore.Editors.ModelEditor.Actions.VertexBuffer;

public class RemoveVertexBuffer : ViewportAction
{
    private ModelEditorScreen Screen;
    private ModelSelectionManager Selection;
    private ModelViewportManager ViewportManager;

    private FLVER2 CurrentFLVER;
    private FLVER2.Mesh CurrentMesh;
    private FLVER2.VertexBuffer StoredItem;
    private FLVER2.VertexBuffer OldObject;

    public RemoveVertexBuffer(ModelEditorScreen screen, FLVER2 flver, FLVER2.VertexBuffer curItem)
    {
        Screen = screen;
        Selection = screen.Selection;

        CurrentFLVER = flver;
        CurrentMesh = flver.Meshes[Selection._selectedMesh];

        StoredItem = curItem.Clone();
        OldObject = curItem;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        if (CurrentMesh.VertexBuffers.Count > 1)
            Selection._subSelectedVertexBufferRow = 0;
        else
            Selection._subSelectedVertexBufferRow = -1;

        CurrentMesh.VertexBuffers.Remove(OldObject);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        CurrentMesh.VertexBuffers.Add(StoredItem);

        return ActionEvent.NoEvent;
    }
    public override string GetEditMessage()
    {
        return "";
    }
}
