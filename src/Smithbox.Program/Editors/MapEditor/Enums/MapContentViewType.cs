using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Enums;

public enum MapContentViewType
{
    [Display(Name = "Hierarchy")] Hierarchy = 0,
    [Display(Name = "Flat")] Flat = 1,
    [Display(Name = "Object Type")] ObjectType = 2
}
