using SoulsFormats;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.ModelEditor.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static SoulsFormats.FLVER2;

namespace StudioCore.Editors.ModelEditor.Actions.AllSkeleton;

public class ReplaceAllSkeletonList : ViewportAction
{
    private FLVER2 CurrentFLVER;
    private ModelEditorScreen Screen;
    private List<FLVER2.SkeletonSet.Bone> OldAllSkeletonBones;
    private List<FLVER2.SkeletonSet.Bone> NewAllSkeletonBones;

    public ReplaceAllSkeletonList(ModelEditorScreen screen, List<FLVER2.SkeletonSet.Bone> AllSkeletonBones)
    {
        Screen = screen;
        CurrentFLVER = screen.ResManager.GetCurrentFLVER();
        OldAllSkeletonBones = [.. CurrentFLVER.Skeletons.AllSkeletons];
        NewAllSkeletonBones = AllSkeletonBones;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        CurrentFLVER.Skeletons.AllSkeletons = NewAllSkeletonBones;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        CurrentFLVER.Skeletons.AllSkeletons = OldAllSkeletonBones;

        return ActionEvent.NoEvent;
    }
    public override string GetEditMessage()
    {
        return "";
    }
}
