using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MaterialEditorNS;

public enum MaterialEditorContext
{
    [Display(Name = "None")] None,
    [Display(Name = "Source List")] SourceList,
    [Display(Name = "File List")] FileList,
    [Display(Name = "Internal File List")] InternalFileList,
    [Display(Name = "Data View")] DataView,
    [Display(Name = "Field View")] FieldView,
    [Display(Name = "Tool View")] ToolView
}

public enum MaterialSourceType
{
    [Display(Name = "Material")]
    MTD,
    [Display(Name = "Material Bin")]
    MATBIN
}
