using StudioCore.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StudioCore.Editors.TimeActEditor.TimeActSelectionHandler;

namespace StudioCore.Editors.TimeActEditor.Actions;

public class ActionHandler
{
    private TimeActEditorScreen Screen;
    private ActionManager EditorActionManager;

    public ActionHandler(TimeActEditorScreen screen, ActionManager manager)
    {
        Screen = screen;
        EditorActionManager = manager;
    }

    public void DetermineCreateTarget()
    {
        var handler = Screen.CollectionPropertyHandler;
        var context = Screen.SelectionHandler.CurrentSelectionContext;

        switch (context)
        {
            case SelectionContext.File: break;
            case SelectionContext.TimeAct:
                break;
            case SelectionContext.Animation:
                break;
            case SelectionContext.Event:
                Screen.CollectionPropertyHandler.CreateEvent();
                break;
            case SelectionContext.Property: break;
        }
    }

    public void DetermineDuplicateTarget()
    {
        var handler = Screen.CollectionPropertyHandler;
        var context = Screen.SelectionHandler.CurrentSelectionContext;

        switch(context)
        {
            case SelectionContext.File: break;
            case SelectionContext.TimeAct:
                Screen.CollectionPropertyHandler.DuplicateTimeAct();
                break;
            case SelectionContext.Animation:
                Screen.CollectionPropertyHandler.DuplicateAnimation(); 
                break;
            case SelectionContext.Event:
                Screen.CollectionPropertyHandler.DuplicateEvent(); 
                break;
            case SelectionContext.Property: break;
        }
    }
    public void DetermineDeleteTarget()
    {
        var handler = Screen.CollectionPropertyHandler;
        var context = Screen.SelectionHandler.CurrentSelectionContext;

        switch (context)
        {
            case SelectionContext.File: break;
            case SelectionContext.TimeAct:
                Screen.CollectionPropertyHandler.DeleteTimeAct();
                break;
            case SelectionContext.Animation:
                Screen.CollectionPropertyHandler.DeleteAnimation(); 
                break;
            case SelectionContext.Event:
                Screen.CollectionPropertyHandler.DeleteEvent(); 
                break;
            case SelectionContext.Property: break;
        }
    }
}
