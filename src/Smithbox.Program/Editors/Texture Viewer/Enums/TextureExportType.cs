using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace StudioCore.Editors.TextureViewer;

public enum TextureExportType
{
    [Display(Name = "TEXVIEW_TexExport_Type_DDS")]
    DDS,

    [Display(Name = "TEXVIEW_TexExport_Type_PNG")]
    PNG,

    [Display(Name = "TEXVIEW_TexExport_Type_BMP")]
    BMP,

    [Display(Name = "TEXVIEW_TexExport_Type_TGA")]
    TGA,

    [Display(Name = "TEXVIEW_TexExport_Type_TIFF")]
    TIFF,

    [Display(Name = "TEXVIEW_TexExport_Type_JPEG")]
    JPEG,

    [Display(Name = "TEXVIEW_TexExport_Type_WEBP")]
    WEBP
}
