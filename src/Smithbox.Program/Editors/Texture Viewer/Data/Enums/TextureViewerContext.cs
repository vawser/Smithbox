using System.ComponentModel.DataAnnotations;

namespace StudioCore.Editors.TextureViewer;

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
