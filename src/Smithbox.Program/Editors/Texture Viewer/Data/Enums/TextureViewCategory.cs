using System.ComponentModel.DataAnnotations;

namespace StudioCore.Editors.TextureViewer;

public enum TextureViewCategory
{
    [Display(Name = "None")] None,
    [Display(Name = "Character")] Characters,
    [Display(Name = "Asset")] Assets,
    [Display(Name = "Object")] Objects,
    [Display(Name = "Part")] Parts,
    [Display(Name = "Map")] Map,
    [Display(Name = "Other")] Other,
    [Display(Name = "Menu")] Menu,
    [Display(Name = "Particle")] Particles,
    [Display(Name = "Adhoc")] Adhoc,
    [Display(Name = "Map Textures")] MapTextures,
    [Display(Name = "High Definition Icons")] HighDefinitionIcons,
    [Display(Name = "Map Tiles")] MapTiles,
    [Display(Name = "Terms of Service")] TOS
}
