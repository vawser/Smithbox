using StudioCore.Editor;
using StudioCore.Editors.TimeActEditor.Bank;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TimeActEditor.Actions;

/// <summary>
/// Action: TAE duplicate
/// </summary>
public class TimeActDuplicate : EditorAction
{
    private InternalTimeActWrapper TargetTimeAct;
    private List<InternalTimeActWrapper> FileList;
    private int InsertionIndex;

    public TimeActDuplicate(InternalTimeActWrapper newInfo, List<InternalTimeActWrapper> fileList, int index)
    {
        InsertionIndex = index;
        FileList = fileList;
        TargetTimeAct = newInfo;
    }

    public override ActionEvent Execute()
    {
        FileList.Insert(InsertionIndex, TargetTimeAct);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        FileList.RemoveAt(InsertionIndex);

        return ActionEvent.NoEvent;
    }
}