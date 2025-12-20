using StudioCore.Application;

namespace StudioCore.Editors.GparamEditor;

public class GparamActionHandler
{
    private GparamEditorScreen Editor;
    private ProjectEntry Project;

    public GparamActionHandler(GparamEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void DeleteValueRow()
    {
        if (Editor.Selection.CanAffectSelection())
        {
            var action = new GparamRemoveValueRow(Editor);
            Editor.EditorActionManager.ExecuteAction(action);
        }
    }

    public void DuplicateValueRow()
    {
        if (Editor.Selection.CanAffectSelection())
        {
            var action = new GparamDuplicateValueRow(Editor);
            Editor.EditorActionManager.ExecuteAction(action);
        }
    }
}
