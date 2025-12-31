using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MaterialEditor;

public enum MaterialEditorContext
{
    [Display(Name = "None")] None,
    [Display(Name = "Source List")] SourceList,
    [Display(Name = "File List")] FileList,
    [Display(Name = "Properties")] Properties,
    [Display(Name = "Tool Window")] ToolWindow
}
