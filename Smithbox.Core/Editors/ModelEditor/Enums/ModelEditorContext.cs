using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor.Enums;

public enum ModelEditorContext
{
    [Display(Name = "None")] None,
    [Display(Name = "File")] File,
    [Display(Name = "Model Hierarchy")] ModelHierarchy,
    [Display(Name = "Model Viewport")] ModelViewport,
    [Display(Name = "Model Properties")] ModelProperties,
    [Display(Name = "Tool Window")] ToolWindow
}
