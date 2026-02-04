using StudioCore.Application;

namespace StudioCore.Editors.GparamEditor;

public class GparamActionHandler
{
    private GparamEditorView Parent;
    private ProjectEntry Project;

    public GparamActionHandler(GparamEditorView view, ProjectEntry project)
    {
        Parent = view;
        Project = project;
    }

    public void DeleteValueRow()
    {
        if (Parent.Selection.CanAffectSelection())
        {
            var action = new GparamRemoveValueRow(Parent);
            Parent.ActionManager.ExecuteAction(action);
        }
    }

    public void DuplicateValueRow()
    {
        if (Parent.Selection.CanAffectSelection())
        {
            var action = new GparamDuplicateValueRow(Parent);
            Parent.ActionManager.ExecuteAction(action);
        }
    }
}
