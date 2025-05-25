using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TimeActEditor.Enums;

public enum FileContainerType
{
    [Display(Name = "None")] None,
    [Display(Name = "Character")] Character,
    [Display(Name = "Object")] Object
}
