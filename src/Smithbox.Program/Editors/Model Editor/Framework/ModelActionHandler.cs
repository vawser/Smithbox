using StudioCore.Application;

namespace StudioCore.Editors.ModelEditor;

public class ModelActionHandler
{
    private ModelEditorScreen Editor;
    private ProjectEntry Project;

    public ModelActionHandler(ModelEditorScreen baseEditor, ProjectEntry project)
    {
        Editor = baseEditor;
        Project = project;
    }
}
