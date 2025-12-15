using StudioCore.Editors.Common;

namespace StudioCore.Editors.GparamEditor;

public class GparamActionHandler
{
    private GparamEditorScreen Screen;
    private ActionManager EditorActionManager;

    public GparamActionHandler(GparamEditorScreen screen)
    {
        Screen = screen;
        EditorActionManager = screen.EditorActionManager;
    }
    public void DeleteValueRow()
    {
        if (Screen.Selection.CanAffectSelection())
        {
            var action = new GparamRemoveValueRow(Screen);
            EditorActionManager.ExecuteAction(action);
        }
    }
    public void DuplicateValueRow()
    {
        if (Screen.Selection.CanAffectSelection())
        {
            var action = new GparamDuplicateValueRow(Screen);
            EditorActionManager.ExecuteAction(action);
        }
    }
}
