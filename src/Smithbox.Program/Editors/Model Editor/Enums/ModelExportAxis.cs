using System.ComponentModel.DataAnnotations;

namespace StudioCore.Editors.ModelEditor;

public enum ModelExportAxis
{
    [Display(Name = "MODEL_ModelExport_Axis_PosX")]
    PosX,

    [Display(Name = "MODEL_ModelExport_Axis_NegX")]
    NegX,

    [Display(Name = "MODEL_ModelExport_Axis_PosY")]
    PosY,

    [Display(Name = "MODEL_ModelExport_Axis_NegY")]
    NegY,

    [Display(Name = "MODEL_ModelExport_Axis_PosZ")]
    PosZ,

    [Display(Name = "MODEL_ModelExport_Axis_NegZ")]
    NegZ
}
