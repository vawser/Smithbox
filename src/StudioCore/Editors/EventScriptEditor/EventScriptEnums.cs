using System.ComponentModel.DataAnnotations;

namespace StudioCore.Editors.EventScriptEditorNS;

public enum EventScriptEditorContext
{
    [Display(Name = "None")] None,
    [Display(Name = "File")] File,
    [Display(Name = "Event List")] EventList,
    [Display(Name = "Event Properties")] EventProperties,
    [Display(Name = "Instruction List")] InstructionList,
    [Display(Name = "Instruction Properties")] InstructionProperties,
    [Display(Name = "Tool Window")] ToolWindow
}
