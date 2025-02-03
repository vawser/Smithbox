using SoulsFormats;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.ModelEditor.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static SoulsFormats.FLVER2;

namespace StudioCore.Editors.ModelEditor.Actions.Mesh;

public class AppendMeshList : ViewportAction
{
    private FLVER2 CurrentFLVER;
    private ModelEditorScreen Screen;
    private List<FLVER2.Mesh> OldMeshs;
    private List<FLVER2.Mesh> NewMeshs;

    public AppendMeshList(ModelEditorScreen screen, List<FLVER2.Mesh> meshes)
    {
        Screen = screen;
        CurrentFLVER = screen.ResManager.GetCurrentFLVER();
        OldMeshs = [.. CurrentFLVER.Meshes];
        NewMeshs = meshes;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        foreach (var entry in NewMeshs)
        {
            CurrentFLVER.Meshes.Add(entry);
        }

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        CurrentFLVER.Meshes = OldMeshs;

        return ActionEvent.NoEvent;
    }
    public override string GetEditMessage()
    {
        return "";
    }
}