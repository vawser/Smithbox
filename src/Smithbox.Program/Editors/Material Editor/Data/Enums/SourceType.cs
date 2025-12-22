using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MaterialEditor;

public enum MaterialSourceType
{
    [Display(Name = "MTD")]
    MTD,
    [Display(Name = "MATBIN")]
    MATBIN
}
