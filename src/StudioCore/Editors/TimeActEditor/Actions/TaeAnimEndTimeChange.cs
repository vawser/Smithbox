using SoulsFormats;
using StudioCore.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TimeActEditor.Actions;

/// <summary>
/// Action: TAE.Animation.EndTime property change
/// </summary>
public class TaeAnimEndTimeChange : EditorAction
{
    private TimeActEditorScreen Editor;

    private TAE.Animation Animation;
    private object OldValue;
    private object NewValue;
    private TransientAnimHeader OldTempHeader;

    public TaeAnimEndTimeChange(TimeActEditorScreen editor, TAE.Animation entry, object oldValue, object newValue, TransientAnimHeader tempHeader)
    {
        Editor = editor;
        Animation = entry;
        OldValue = oldValue;
        NewValue = newValue;
        OldTempHeader = tempHeader;
    }

    public override ActionEvent Execute()
    {
        Animation.MiniHeader = (TAE.Animation.AnimMiniHeader)NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Animation.MiniHeader = (TAE.Animation.AnimMiniHeader)OldValue;
        Editor.Selection.CurrentTemporaryAnimHeader = OldTempHeader;

        return ActionEvent.NoEvent;
    }
}