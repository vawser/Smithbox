using StudioCore.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// Handles property edits for collection fields
/// </summary>
public class CollectionPropertyHandler
{
    private ActionManager EditorActionManager;
    private TimeActEditorScreen Screen;
    private TimeActDecorator Decorator;

    public CollectionPropertyHandler(ActionManager editorActionManager, TimeActEditorScreen screen, TimeActDecorator decorator)
    {
        EditorActionManager = editorActionManager;
        Screen = screen;
        Decorator = decorator;
    }

    public void DuplicateTimeAct()
    {

    }
    public void DeleteTimeAct()
    {

    }

    public void DuplicateAnimation()
    {

    }
    public void DeleteAnimation()
    {

    }

    public void DuplicateEvent()
    {

    }

    public void DeleteEvent()
    {

    }
    public void OrderAnimation()
    {

    }

    public void OrderEvent()
    {

    }

    public enum OrderType
    {
        Up,
        Down,
        Top,
        Bottom,
        Sort
    }
}
