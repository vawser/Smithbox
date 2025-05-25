using SoulsFormats;
using StudioCore.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TimeActEditor.Actions;

/// <summary>
/// Action: TAE.Animation delete
/// </summary>
public class TaeAnimDelete : EditorAction
{
    private TAE.Animation StoredAnim;
    private List<TAE.Animation> AnimationList;
    private int RemovalIndex;

    public TaeAnimDelete(TAE.Animation storedAnim, List<TAE.Animation> animList, int index)
    {
        StoredAnim = storedAnim;
        AnimationList = animList;
        RemovalIndex = index;
    }

    public override ActionEvent Execute()
    {
        AnimationList.RemoveAt(RemovalIndex);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        AnimationList.Insert(RemovalIndex, StoredAnim);

        return ActionEvent.NoEvent;
    }
}