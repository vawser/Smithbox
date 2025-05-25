using SoulsFormats;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.ModelEditor.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static SoulsFormats.FLVER2;

namespace StudioCore.Editors.ModelEditor.Actions.AllSkeleton;

public class DuplicateAllSkeletonBone : ViewportAction
{
    private ModelEditorScreen Editor;
    private ModelSelectionManager Selection;
    private ModelViewportManager ViewportManager;

    private FLVER2 CurrentFLVER;

    private FLVER2.SkeletonSet.Bone DupedObject;
    private int PreviousSelectionIndex;
    private int Index;

    public DuplicateAllSkeletonBone(ModelEditorScreen screen, FLVER2 flver, int index)
    {
        Editor = screen;
        Selection = screen.Selection;
        ViewportManager = screen.ViewportManager;

        PreviousSelectionIndex = screen.Selection._selectedAllSkeletonBone;

        CurrentFLVER = flver;
        DupedObject = CurrentFLVER.Skeletons.AllSkeletons[index].Clone();
        Index = flver.Skeletons.AllSkeletons.Count;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        CurrentFLVER.Skeletons.AllSkeletons.Insert(Index, DupedObject);
        Selection._selectedAllSkeletonBone = Index;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Selection._selectedAllSkeletonBone = PreviousSelectionIndex;
        CurrentFLVER.Skeletons.AllSkeletons.RemoveAt(Index);

        return ActionEvent.NoEvent;
    }
    public override string GetEditMessage()
    {
        return "";
    }
}
