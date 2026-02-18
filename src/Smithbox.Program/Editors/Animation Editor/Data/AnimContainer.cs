using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.ModelEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.AnimEditor;

public class AnimContainer : ObjectContainer
{
    public AnimEditorView View;
    public ProjectEntry Project;

    public AnimContainer(AnimEditorView view, ProjectEntry project, string modelName)
    {
        View = view;
        Project = project;
        Name = modelName;
    }

    // TODO: implement if we get to the viewport relevant stuff
}
