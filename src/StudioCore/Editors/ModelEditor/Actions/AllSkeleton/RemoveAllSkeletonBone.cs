using SoulsFormats;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.ModelEditor.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static SoulsFormats.FLVER2;

namespace StudioCore.Editors.ModelEditor.Actions.AllSkeleton;

public class RemoveAllSkeletonBone : ViewportAction
{
    private ModelEditorScreen Screen;
    private ModelSelectionManager Selection;
    private ModelViewportManager ViewportManager;

    private FLVER2 CurrentFLVER;

    private FLVER2.SkeletonSet.Bone RemovedObject;
    private int PreviousSelectionIndex;
    private int Index;

    public RemoveAllSkeletonBone(ModelEditorScreen screen, FLVER2 flver, int index)
    {
        Screen = screen;
        Selection = screen.Selection;
        ViewportManager = screen.ViewportManager;

        PreviousSelectionIndex = screen.Selection._selectedAllSkeletonBone;

        CurrentFLVER = flver;
        RemovedObject = CurrentFLVER.Skeletons.AllSkeletons[index].Clone();
        Index = index;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Selection._selectedAllSkeletonBone = -1;
        CurrentFLVER.Skeletons.AllSkeletons.RemoveAt(Index);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        CurrentFLVER.Skeletons.AllSkeletons.Insert(Index, RemovedObject);
        Selection._selectedAllSkeletonBone = PreviousSelectionIndex;

        return ActionEvent.NoEvent;
    }
    public override string GetEditMessage()
    {
        return "";
    }
}