using System.ComponentModel.DataAnnotations;

namespace StudioCore.Editors.MapEditor;

// Editor
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

/// <summary>
/// Enum for Entity Type within the MSB.
/// </summary>
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
    DS2ObjectInstance,
    AutoInvadePoint,
    Navmesh
}

// Grid
public enum TargetGrid
{
    [Display(Name = "Primary")]
    Primary = 0,
    [Display(Name = "Secondary")]
    Secondary = 1,
    [Display(Name = "Tertiary")]
    Tertiary = 2
}

public enum RootAxis
{
    [Display(Name = "X")]
    X = 0,
    [Display(Name = "Y")]
    Y = 1,
    [Display(Name = "Z")]
    Z = 2
}


// Editor Visibility action
public enum EditorVisibilityType
{
    Selected,
    All
}

public enum EditorVisibilityState
{
    Flip,
    Enable,
    Disable
}

// Game Visibility action
public enum GameVisibilityType
{
    DummyObject,
    GameEditionDisable
}
public enum GameVisibilityState
{
    Enable,
    Disable
}

// Duplicate to map action
public enum MapDuplicateToMapType
{
    ToolWindow,
    Menubar,
    Popup
}

// Model Selector tool
public enum FileSelectionType
{
    [Display(Name = "None")] None,
    [Display(Name = "Character")] Character,
    [Display(Name = "Enemy")] Enemy,
    [Display(Name = "Asset")] Asset,
    [Display(Name = "Part")] Part,
    [Display(Name = "Map Piece")] MapPiece,
    [Display(Name = "Loose")] Loose
}
