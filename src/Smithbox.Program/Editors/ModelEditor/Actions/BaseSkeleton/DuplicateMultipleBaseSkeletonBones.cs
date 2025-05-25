using SoulsFormats;
using StudioCore.Editor.Multiselection;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static SoulsFormats.FLVER2;

namespace StudioCore.Editors.ModelEditor.Actions.BaseSkeleton;

public class DuplicateMultipleBaseSkeletonBones : ViewportAction
{
    private ModelEditorScreen Screen;
    private ModelSelectionManager Selection;
    private ModelViewportManager ViewportManager;

    private FLVER2 CurrentFLVER;
    private Multiselection Multiselect;
    private List<FLVER2.SkeletonSet.Bone> DupedObjects;

    public DuplicateMultipleBaseSkeletonBones(ModelEditorScreen screen, FLVER2 flver, Multiselection multiselect)
    {
        Screen = screen;
        Selection = screen.Selection;
        ViewportManager = screen.ViewportManager;

        CurrentFLVER = flver;
        Multiselect = multiselect;
        DupedObjects = new List<FLVER2.SkeletonSet.Bone>();
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Selection._selectedBaseSkeletonBone = -1;

        foreach (var idx in Multiselect.StoredIndices)
        {
            if (CurrentFLVER.Skeletons.BaseSkeleton[idx] != null)
                DupedObjects.Add(CurrentFLVER.Skeletons.BaseSkeleton[idx].Clone());
        }

        foreach (var entry in DupedObjects)
        {
            CurrentFLVER.Skeletons.BaseSkeleton.Add(entry);
        }

        Multiselect.StoredIndices = new List<int>();

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        for (int i = 0; i < DupedObjects.Count; i++)
        {
            CurrentFLVER.Skeletons.BaseSkeleton.Remove(DupedObjects[i]);
        }

        return ActionEvent.NoEvent;
    }
    public override string GetEditMessage()
    {
        return "";
    }
}