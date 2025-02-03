using SoulsFormats;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.ModelEditor.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static SoulsFormats.FLVER2;

namespace StudioCore.Editors.ModelEditor.Actions.Mesh;

public class ReplaceMeshList : ViewportAction
{
    private FLVER2 CurrentFLVER;
    private ModelEditorScreen Screen;
    private List<FLVER2.Mesh> OldMeshes;
    private List<FLVER2.Mesh> NewMeshes;

    public ReplaceMeshList(ModelEditorScreen screen, List<FLVER2.Mesh> meshes)
    {
        Screen = screen;
        CurrentFLVER = screen.ResManager.GetCurrentFLVER();
        OldMeshes = [.. CurrentFLVER.Meshes];
        NewMeshes = meshes;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        CurrentFLVER.Meshes = NewMeshes;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        CurrentFLVER.Meshes = OldMeshes;

        return ActionEvent.NoEvent;
    }
    public override string GetEditMessage()
    {
        return "";
    }
}
