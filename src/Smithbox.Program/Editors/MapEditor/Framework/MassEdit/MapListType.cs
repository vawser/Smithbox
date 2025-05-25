using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Framework.MassEdit;

public enum MapListType
{
    [Display(Name = "Local")]
    Local,
    [Display(Name = "Global")]
    Global
}
