using SoulsFormats;
using StudioCore.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TimeActEditor.Actions;

/// <summary>
/// Action: TAE.Animation.ID property change
/// </summary>
public class TaeAnimIdChange : EditorAction
{
    private TAE.Animation Animation;
    private object OldValue;
    private object NewValue;

    public TaeAnimIdChange(TAE.Animation entry, object oldValue, object newValue)
    {
        Animation = entry;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public override ActionEvent Execute()
    {
        Animation.ID = (long)NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Animation.ID = (long)OldValue;

        return ActionEvent.NoEvent;
    }
}
