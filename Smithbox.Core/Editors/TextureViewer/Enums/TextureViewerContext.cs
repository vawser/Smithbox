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
    [Display(Name = "File")] FileList,
    [Display(Name = "Texture List")] TextureList,
    [Display(Name = "Texture Viewport")] TextureViewport,
    [Display(Name = "Texture Properties")] TextureProperties,
    [Display(Name = "Tool Window")] ToolWindow
}
