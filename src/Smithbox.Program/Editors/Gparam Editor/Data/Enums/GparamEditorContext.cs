using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.GparamEditor;

public enum GparamEditorContext
{
    [Display(Name = "None")] None,
    [Display(Name = "File")] File,
    [Display(Name = "Group")] Group,
    [Display(Name = "Field")] Field,
    [Display(Name = "Field Value")] FieldValue,
    [Display(Name = "Tool Window")] ToolWindow
}