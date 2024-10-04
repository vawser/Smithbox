using StudioCore.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StudioCore.Editors.TimeActEditor.TimeActViewSelection;

namespace StudioCore.Editors.TimeActEditor.Actions;

public class TimeActTools
{
    private TimeActEditorScreen Screen;

    public TimeActTools(TimeActEditorScreen screen)
    {
        Screen = screen;
    }

    public void DetermineCreateTarget()
    {
        var handler = Screen.ActionHandler;
        var context = Screen.Selection.CurrentSelectionContext;

        switch (context)
        {
            case SelectionContext.File: break;
            case SelectionContext.TimeAct:
                break;
            case SelectionContext.Animation:
                break;
            case SelectionContext.Event:
                Screen.ActionHandler.CreateEvent();
                break;
            case SelectionContext.Property: break;
        }
    }

    public void DetermineDuplicateTarget()
    {
        var handler = Screen.ActionHandler;
        var context = Screen.Selection.CurrentSelectionContext;

        switch(context)
        {
            case SelectionContext.File: break;
            case SelectionContext.TimeAct:
                Screen.ActionHandler.DuplicateTimeAct();
                break;
            case SelectionContext.Animation:
                Screen.ActionHandler.DuplicateAnimation(); 
                break;
            case SelectionContext.Event:
                Screen.ActionHandler.DuplicateEvent(); 
                break;
            case SelectionContext.Property: break;
        }
    }
    public void DetermineDeleteTarget()
    {
        var handler = Screen.ActionHandler;
        var context = Screen.Selection.CurrentSelectionContext;

        switch (context)
        {
            case SelectionContext.File: break;
            case SelectionContext.TimeAct:
                Screen.ActionHandler.DeleteTimeAct();
                break;
            case SelectionContext.Animation:
                Screen.ActionHandler.DeleteAnimation(); 
                break;
            case SelectionContext.Event:
                Screen.ActionHandler.DeleteEvent(); 
                break;
            case SelectionContext.Property: break;
        }
    }
}
