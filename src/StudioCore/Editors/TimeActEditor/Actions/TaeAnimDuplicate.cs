using SoulsFormats;
using StudioCore.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TimeActEditor.Actions;

/// <summary>
/// Action: TAE.Animation duplicate
/// </summary>
public class TaeAnimDuplicate : EditorAction
{
    private TAE.Animation NewAnim;
    private List<TAE.Animation> AnimationList;
    private int InsertionIndex;

    public TaeAnimDuplicate(TAE.Animation newAnimation, List<TAE.Animation> animList, int index)
    {
        InsertionIndex = index;
        NewAnim = newAnimation;
        AnimationList = animList;
    }

    public override ActionEvent Execute()
    {
        AnimationList.Insert(InsertionIndex, NewAnim);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        AnimationList.RemoveAt(InsertionIndex);

        return ActionEvent.NoEvent;
    }
}