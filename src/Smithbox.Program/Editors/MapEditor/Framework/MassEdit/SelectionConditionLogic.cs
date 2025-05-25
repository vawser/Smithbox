using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Framework.MassEdit;

public enum SelectionConditionLogic
{
    [Display(Name = "All must match")]
    AND = 0,
    [Display(Name = "One must match")]
    OR = 1
}

