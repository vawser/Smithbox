using StudioCore.Core.ProjectNS;
using StudioCore.Editors.EventScriptEditorNS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.EzStateEditorNS;

public class EzStateDecorator
{
    public Project Project;
    public EzStateEditor Editor;

    public EzStateDecorator(Project curProject, EzStateEditor editor)
    {
        Project = curProject;
        Editor = editor;
    }
}
