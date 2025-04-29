using StudioCore.Core.ProjectNS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.EzStateEditorNS;

public class EzStateToolView
{
    public Project Project;
    public EzStateEditor Editor;

    public EzStateToolView(Project curProject, EzStateEditor editor)
    {
        Project = curProject;
        Editor = editor;
    }

    public void Draw()
    {

    }
}