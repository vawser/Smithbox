using System.ComponentModel.DataAnnotations;

namespace StudioCore.Editors.TextureViewer;

public enum TextureViewCategory
{
    [Display(Name = "TEXVIEW_Category_None")] 
    None,

    [Display(Name = "TEXVIEW_Category_Characters")] 
    Characters,

    [Display(Name = "TEXVIEW_Category_Assets")] 
    Assets,

    [Display(Name = "TEXVIEW_Category_Objects")] 
    Objects,

    [Display(Name = "TEXVIEW_Category_Parts")]
    Parts,

    [Display(Name = "TEXVIEW_Category_Map")] 
    Map,

    [Display(Name = "TEXVIEW_Category_Other")] 
    Other,

    [Display(Name = "TEXVIEW_Category_Menu")] 
    Menu,

    [Display(Name = "TEXVIEW_Category_Particles")] 
    Particles,

    [Display(Name = "TEXVIEW_Category_Adhoc")] 
    Adhoc,

    [Display(Name = "TEXVIEW_Category_Map_Textures")] 
    MapTextures,

    [Display(Name = "TEXVIEW_Category_HD_Icons")] 
    HighDefinitionIcons,

    [Display(Name = "TEXVIEW_Category_Map_Tiles")] 
    MapTiles,

    [Display(Name = "TEXVIEW_Category_TOS")] 
    TOS
}
