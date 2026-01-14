using System.ComponentModel.DataAnnotations;

namespace StudioCore.Editors.MapEditor;

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