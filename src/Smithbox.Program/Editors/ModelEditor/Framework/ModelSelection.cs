using SoulsFormats;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Formats.JSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor;

public class ModelSelection
{
    private ModelEditorScreen Editor;
    private ProjectEntry Project;

    public ModelContainerWrapper SelectedModelContainerWrapper;
    public ModelWrapper SelectedModelWrapper;

    public ModelSelection(ModelEditorScreen baseEditor, ProjectEntry project)
    {
        Editor = baseEditor;
        Project = project;
    }

    public bool IsAnyModelLoaded()
    {
        if(SelectedModelWrapper != null)
        {
            return true;
        }

        return false;
    }
}
