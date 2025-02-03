using SoulsFormats;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.ModelEditor.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static SoulsFormats.FLVER2;

namespace StudioCore.Editors.ModelEditor.Actions.BaseSkeleton;

public class RemoveBaseSkeletonBone : ViewportAction
{
    private ModelEditorScreen Screen;
    private ModelSelectionManager Selection;
    private ModelViewportManager ViewportManager;

    private FLVER2 CurrentFLVER;

    private FLVER2.SkeletonSet.Bone RemovedObject;
    private int PreviousSelectionIndex;
    private int Index;

    public RemoveBaseSkeletonBone(ModelEditorScreen screen, FLVER2 flver, int index)
    {
        Screen = screen;
        Selection = screen.Selection;
        ViewportManager = screen.ViewportManager;

        PreviousSelectionIndex = screen.Selection._selectedBaseSkeletonBone;

        CurrentFLVER = flver;
        RemovedObject = CurrentFLVER.Skeletons.BaseSkeleton[index].Clone();
        Index = index;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Selection._selectedBaseSkeletonBone = -1;
        CurrentFLVER.Skeletons.BaseSkeleton.RemoveAt(Index);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        CurrentFLVER.Skeletons.BaseSkeleton.Insert(Index, RemovedObject);
        Selection._selectedBaseSkeletonBone = PreviousSelectionIndex;

        return ActionEvent.NoEvent;
    }
    public override string GetEditMessage()
    {
        return "";
    }
}