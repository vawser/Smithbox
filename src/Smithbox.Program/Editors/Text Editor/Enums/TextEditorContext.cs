using System.ComponentModel.DataAnnotations;

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
