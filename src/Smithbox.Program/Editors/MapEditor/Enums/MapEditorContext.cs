using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Enums;

public enum MapEditorContext
{
    [Display(Name = "None")] None,
    [Display(Name = "Map List")] MapIdList,
    [Display(Name = "Map Contents")] MapContents,
    [Display(Name = "Map Viewport")] MapViewport,
    [Display(Name = "Map Object Properties")] MapObjectProperties,
    [Display(Name = "Tool Window")] ToolWindow,
    [Display(Name = "Render Groups")] RenderGroups,
    [Display(Name = "Asset Browser")] AssetBrowser,
    [Display(Name = "World Map")] WorldMap,
    [Display(Name = "World Map Properties")] WorldMapProperties
}
