using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextureViewer.Enums;

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
