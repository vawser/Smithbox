using SoulsFormats;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.ModelEditor.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static SoulsFormats.FLVER2;

namespace StudioCore.Editors.ModelEditor.Actions.BaseSkeleton;

public class ReplaceBaseSkeletonList : ViewportAction
{
    private FLVER2 CurrentFLVER;
    private ModelEditorScreen Screen;
    private List<FLVER2.SkeletonSet.Bone> OldBaseSkeletonBones;
    private List<FLVER2.SkeletonSet.Bone> NewBaseSkeletonBones;

    public ReplaceBaseSkeletonList(ModelEditorScreen screen, List<FLVER2.SkeletonSet.Bone> BaseSkeletonBones)
    {
        Screen = screen;
        CurrentFLVER = screen.ResManager.GetCurrentFLVER();
        OldBaseSkeletonBones = [.. CurrentFLVER.Skeletons.BaseSkeleton];
        NewBaseSkeletonBones = BaseSkeletonBones;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        CurrentFLVER.Skeletons.BaseSkeleton = NewBaseSkeletonBones;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        CurrentFLVER.Skeletons.BaseSkeleton = OldBaseSkeletonBones;

        return ActionEvent.NoEvent;
    }
    public override string GetEditMessage()
    {
        return "";
    }
}
