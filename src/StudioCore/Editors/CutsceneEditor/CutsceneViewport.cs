using StudioCore.Core.ProjectNS;

namespace StudioCore.Editors.CutsceneEditorNS;

public class CutsceneViewport
{
    public Project Project;
    public CutsceneEditor Editor;

    public CutsceneViewport(Project curProject, CutsceneEditor editor)
    {
        Project = curProject;
        Editor = editor;
    }

    public void Draw(float dt)
    {

    }
}

