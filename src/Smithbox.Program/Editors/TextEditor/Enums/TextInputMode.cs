using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor;

public enum TextInputMode
{
    [Display(Name = "Simple")] Simple,
    [Display(Name = "Group")] Group,
    [Display(Name = "Programmatic")] Programmatic,
}