using StudioCore.Core.ProjectNS;

namespace StudioCore.Editors.CutsceneEditorNS;

public class CutsceneFieldInput
{
    public Project Project;
    public CutsceneEditor Editor;

    public CutsceneFieldInput(Project curProject, CutsceneEditor editor)
    {
        Project = curProject;
        Editor = editor;
    }

    public void Draw()
    {

    }
}
