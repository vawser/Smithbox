using SoulsFormats;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.ModelEditor.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static SoulsFormats.FLVER2;

namespace StudioCore.Editors.ModelEditor.Actions.VertexBuffer;

public class AddVertexBuffer : ViewportAction
{
    private ModelEditorScreen Screen;
    private ModelSelectionManager Selection;
    private ModelViewportManager ViewportManager;

    private FLVER2 CurrentFLVER;
    private FLVER2.Mesh CurrentMesh;
    private FLVER2.VertexBuffer NewObject;

    public AddVertexBuffer(ModelEditorScreen screen, FLVER2 flver)
    {
        Screen = screen;
        Selection = screen.Selection;
        ViewportManager = screen.ViewportManager;

        CurrentFLVER = flver;
        CurrentMesh = flver.Meshes[Selection._selectedMesh];

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
            Selection._subSelectedVertexBufferRow = 0;
        else
            Selection._subSelectedVertexBufferRow = -1;

        CurrentMesh.VertexBuffers.Remove(NewObject);

        return ActionEvent.NoEvent;
    }
    public override string GetEditMessage()
    {
        return "";
    }
}