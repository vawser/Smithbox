using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextureViewer.Enums;

public enum TextureViewerContext
{
    [Display(Name = "None")] None,
    [Display(Name = "Files")] BinderList,
    [Display(Name = "TPF List")] TpfList,
    [Display(Name = "Texture List")] TextureList,
    [Display(Name = "Texture Display")] TextureDisplay,
    [Display(Name = "Texture Properties")] TextureProperties,
    [Display(Name = "Tool Window")] ToolWindow
}
