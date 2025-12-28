using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor;

public enum QueryMapListType
{
    [Display(Name = "Local")]
    Local,
    [Display(Name = "Global")]
    Global
}
