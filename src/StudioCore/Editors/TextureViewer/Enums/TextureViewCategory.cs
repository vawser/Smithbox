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
    [Display(Name = "Menu")] Menu,
    [Display(Name = "Asset")] Asset,
    [Display(Name = "Character")] Character,
    [Display(Name = "Object")] Object,
    [Display(Name = "Other")] Other,
    [Display(Name = "Part")] Part,
    [Display(Name = "Particle")] Particle,
    [Display(Name = "HD Icons")] HDIcons,
    [Display(Name = "Map Tiles")] MapTiles,
}
