using System.ComponentModel.DataAnnotations;

namespace StudioCore.Editors.MapEditor;

public enum SelectionConditionLogic
{
    [Display(Name = "All must match")]
    AND = 0,
    [Display(Name = "One must match")]
    OR = 1
}

