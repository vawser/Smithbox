using SoulsFormats;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.ModelEditor.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static SoulsFormats.FLVER2;

namespace StudioCore.Editors.ModelEditor.Actions.BaseSkeleton;

public class AddBaseSkeletonBone : ViewportAction
{
    private ModelEditorScreen Screen;
    private ModelSelectionManager Selection;
    private ModelViewportManager ViewportManager;

    private FLVER2 CurrentFLVER;
    private FLVER2.SkeletonSet.Bone NewObject;
    private int PreviousSelectionIndex;
    private int Index;

    public AddBaseSkeletonBone(ModelEditorScreen screen, FLVER2 flver)
    {
        Screen = screen;
        Selection = screen.Selection;
        ViewportManager = screen.ViewportManager;

        PreviousSelectionIndex = screen.Selection._selectedBaseSkeletonBone;

        CurrentFLVER = flver;
        NewObject = new FLVER2.SkeletonSet.Bone(-1);
        Index = flver.Skeletons.BaseSkeleton.Count;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        CurrentFLVER.Skeletons.BaseSkeleton.Insert(Index, NewObject);
        Selection._selectedBaseSkeletonBone = Index;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Selection._selectedBaseSkeletonBone = PreviousSelectionIndex;
        CurrentFLVER.Skeletons.BaseSkeleton.RemoveAt(Index);

        return ActionEvent.NoEvent;
    }
    public override string GetEditMessage()
    {
        return "";
    }
}