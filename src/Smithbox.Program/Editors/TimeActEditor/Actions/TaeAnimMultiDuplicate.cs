using SoulsFormats;
using StudioCore.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TimeActEditor.Actions;

/// <summary>
/// Action: TAE.Animation duplicate (multiple)
/// </summary>
public class TaeAnimMultiDuplicate : EditorAction
{
    private List<TAE.Animation> NewAnims;
    private List<TAE.Animation> AnimationList;
    private List<int> InsertionIndexes;

    public TaeAnimMultiDuplicate(List<TAE.Animation> newAnims, List<TAE.Animation> animList, List<int> indexList)
    {
        InsertionIndexes = indexList;
        NewAnims = newAnims;
        AnimationList = animList;
    }

    public override ActionEvent Execute()
    {
        for (int i = 0; i < InsertionIndexes.Count; i++)
        {
            TAE.Animation curNewAnim = NewAnims[i];
            int curIndex = InsertionIndexes[i];

            AnimationList.Insert(curIndex, curNewAnim);
        }

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        foreach (TAE.Animation entry in NewAnims)
        {
            AnimationList.Remove(entry);
        }

        return ActionEvent.NoEvent;
    }
}