using StudioCore.Core;
using StudioCore.Editors.MapEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
