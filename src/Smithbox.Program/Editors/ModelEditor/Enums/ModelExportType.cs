using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor.Enums;

public enum ModelExportType
{
    [Display(Name = "Collada DAE")] DAE = 0,
    [Display(Name = "Wavefront OBJ")] OBJ = 1
}