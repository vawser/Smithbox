using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor;

public enum TextEditorContext
{
    [Display(Name = "None")] None,
    [Display(Name = "File")] File,
    [Display(Name = "FMG")] Fmg,
    [Display(Name = "FMG.Entry")] FmgEntry,
    [Display(Name = "FMG.Entry Contents")] FmgEntryContents,
    [Display(Name = "Tool Window")] ToolWindow
}
