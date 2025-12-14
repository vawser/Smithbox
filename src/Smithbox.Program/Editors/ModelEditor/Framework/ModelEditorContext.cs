using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor;

public enum ModelEditorContext
{
    [Display(Name = "None")] None,
    [Display(Name = "Model Source List")] ModelSourceList,
    [Display(Name = "Model Selection List")] ModelSelectList,
    [Display(Name = "Model Contents")] ModelContents,
    [Display(Name = "Model Viewport")] ModelViewport,
    [Display(Name = "Model Properties Header")] ModelPropertiesHeader,
    [Display(Name = "Model Properties")] ModelProperties,
    [Display(Name = "Tool Window")] ToolWindow
}
