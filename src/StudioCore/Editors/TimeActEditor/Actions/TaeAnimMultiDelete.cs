using SoulsFormats;
using StudioCore.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TimeActEditor.Actions;

/// <summary>
/// Action: TAE.Animation delete (multiple)
/// </summary>
public class TaeAnimMultiDelete : EditorAction
{
    private List<TAE.Animation> StoredAnims;
    private List<TAE.Animation> AnimationList;
    private List<int> RemovedIndices;

    public TaeAnimMultiDelete(List<TAE.Animation> storedAnims, List<TAE.Animation> animList, List<int> indices)
    {
        StoredAnims = storedAnims;
        AnimationList = animList;
        RemovedIndices = indices;
    }

    public override ActionEvent Execute()
    {
        for (int i = RemovedIndices.Count - 1; i >= 0; i--)
        {
            int curIndex = RemovedIndices[i];
            AnimationList.RemoveAt(curIndex);
        }

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        for (int i = 0; i < RemovedIndices.Count; i++)
        {
            TAE.Animation storedAnim = StoredAnims[i];
            int curIndex = RemovedIndices[i];

            AnimationList.Insert(curIndex, storedAnim);
        }

        return ActionEvent.NoEvent;
    }
}