using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.EmevdEditor.Enums;

public enum EmevdEditorContext
{
    [Display(Name = "None")] None,
    [Display(Name = "File")] File,
    [Display(Name = "Event List")] EventList,
    [Display(Name = "Event Properties")] EventProperties,
    [Display(Name = "Instruction List")] InstructionList,
    [Display(Name = "Instruction Properties")] InstructionProperties,
    [Display(Name = "Tool Window")] ToolWindow
}
