using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public enum ParamFieldMassEditMode
{
    [Display(Name = "None")]
    None,
    [Display(Name = "Auto-fill Menu")]
    AutoFill,
    [Display(Name = "Command Palette Menu")]
    CommandPalette
}