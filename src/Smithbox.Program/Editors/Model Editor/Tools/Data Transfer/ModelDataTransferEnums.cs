using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace StudioCore.Editors.ModelEditor;

public enum FlverDataImportMode
{
    [Display(Name = "MODEL_DataTransfer_FlverDataMode_Dummy")]
    Dummy,
    [Display(Name = "MODEL_DataTransfer_FlverDataMode_Node")]
    Node
}
public enum FlverDataExportMode
{
    [Display(Name = "MODEL_DataTransfer_FlverDataMode_Dummy")]
    Dummy,
    [Display(Name = "MODEL_DataTransfer_FlverDataMode_Node")]
    Node
}