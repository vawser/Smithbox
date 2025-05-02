using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Enums;

public enum MapContentLoadState
{
    [Display(Name = "Unloaded")] Unloaded = 0,
    [Display(Name = "Loaded")] Loaded = 1,
}
