using StudioCore.Core.ProjectNS;

namespace StudioCore.Editors.CutsceneEditorNS;

public class CutsceneFieldView
{
    public Project Project;
    public CutsceneEditor Editor;

    public CutsceneFieldView(Project curProject, CutsceneEditor editor)
    {
        Project = curProject;
        Editor = editor;
    }

    public void Draw()
    {

    }
}

