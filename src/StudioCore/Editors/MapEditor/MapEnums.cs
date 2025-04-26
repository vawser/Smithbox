using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditorNS;

public enum MapContentLoadState
{
    [Display(Name = "Unloaded")] Unloaded = 0,
    [Display(Name = "Loaded")] Loaded = 1,
}

public enum MapContentViewType
{
    [Display(Name = "Hierarchy")] Hierarchy = 0,
    [Display(Name = "Flat")] Flat = 1,
    [Display(Name = "Object Type")] ObjectType = 2
}
public enum MapDuplicateToMapType
{
    ToolWindow,
    Menubar,
    Popup
}
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

public enum MsbEntityType
{
    MapRoot,
    Editor,
    Part,
    Region,
    Event,
    Models,
    Routes,
    Layers,
    Light,
    DS2Generator,
    DS2GeneratorRegist,
    DS2Event,
    DS2EventLocation,
    DS2ObjectInstance
}