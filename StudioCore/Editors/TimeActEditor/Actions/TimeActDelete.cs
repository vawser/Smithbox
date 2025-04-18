using StudioCore.Editor;
using StudioCore.Editors.TimeActEditor.Bank;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TimeActEditor.Actions;

/// <summary>
/// Action: TAE delete
/// </summary>
public class TimeActDelete : EditorAction
{
    private InternalTimeActWrapper TargetTimeAct;

    public TimeActDelete(InternalTimeActWrapper info)
    {
        TargetTimeAct = info;
    }

    public override ActionEvent Execute()
    {
        TargetTimeAct.MarkForRemoval = true;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        TargetTimeAct.MarkForRemoval = false;

        return ActionEvent.NoEvent;
    }
}