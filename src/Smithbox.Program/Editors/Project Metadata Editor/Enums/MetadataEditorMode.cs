using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace StudioCore.Editors.MetadataEditor;

public enum MetadataEditorMode
{
    [Display(Name = "META_Project_Mode")]
    Project,
    [Display(Name = "META_Param_Editor_Mode")]
    ParamEditor
}
