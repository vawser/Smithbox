using StudioCore.Editors.Common;
using StudioCore.Editors.GparamEditor;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.ModelEditor;
using StudioCore.Editors.ParamEditor;
using StudioCore.Editors.ProjectileEditor;
using StudioCore.Editors.TextEditor;
using StudioCore.Editors.TextureViewer;
using StudioCore.Editors.TimeActEditor;
using StudioCore.Editors.Viewport;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System.Drawing;
using System.Numerics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using static StudioCore.Editors.MapEditor.DuplicateAction;

namespace StudioCore.Application;

/// <summary>
///     The interface settings (<c>CFG.Current.Interface</c>): UI scale, fonts, the editor rail and the other settings shared by every editor's UI. Saved in
///     Configuration.json under the same Interface_ keys as before they moved here.
/// </summary>
public sealed class InterfaceSettings : ICfgSection
{
    ///------------------------------------------------------------
    /// Interface
    ///------------------------------------------------------------
    public float Interface_UI_Scale = 1.0f;
    public bool Interface_Scale_by_DPI = true;
    public float Interface_Font_Size = 14.0f;
    public bool Interface_Allow_Window_Movement = true;
    public bool Interface_Editor_Rail_Text_Labels = false;

    // Size of the editor rail's icon buttons and their icons, as a multiple of the default
    public float Interface_Editor_Rail_Icon_Scale = 1.0f;

    // The editor rail's button order as editor ids, set by ctrl+dragging the buttons. Empty is the registry's
    // order, grouped by category.
    public List<string> Interface_Editor_Rail_Order = new List<string>();

    // Editor rail button colors (RGB, 0 to 1) by editor id. An editor missing here uses the theme's text color.
    public Dictionary<string, Vector3> Interface_Editor_Rail_Colors = new Dictionary<string, Vector3>();
    public bool Interface_Tool_Window_Dropdown_Navigation = false;

    // The Project Editor list's groups whose projects are hidden: "open", or "game_" + the project type
    public List<string> Interface_Project_List_Collapsed_Groups = new List<string>();

    public bool Interface_Alias_Wordwrap_General = true;
    public bool Interface_Alias_Wordwrap_Map_Editor = true;
    public bool Interface_Alias_Wordwrap_Model_Editor = true;
    public bool Interface_Alias_Wordwrap_Animation_Editor = true;

    public string Interface_Selected_Theme = "";

    // Editor Presentation
    public bool Interface_Display_Title_Headers = false;

    // Every editor colours and marks items that differ from, or aren't in, the game's data (VanillaMarkers)
    public bool Interface_Vanilla_Highlight = true;
    public bool Interface_Display_Searchbar_Minimize = true;
    public bool Interface_Searchbar_Fixed_Width = true;

    public EditorDisplayType Interface_MapEditor_EditorDisplayType = EditorDisplayType.Windows;
    public EditorDisplayType Interface_ModelEditor_EditorDisplayType = EditorDisplayType.Windows;

    public int Interface_Model_Preview_Size = 150;
    public bool Interface_Asset_Browser_Use_Previews = true;

    // Windows
    public bool Interface_MapEditor_Viewport_Grid = true;
    public bool Interface_MapEditor_MapList = true;
    public bool Interface_MapEditor_MapContents = true;
    public bool Interface_MapEditor_Properties = true;
    public bool Interface_MapEditor_RenderGroups = true;
    public bool Interface_MapEditor_AssetBrowser = true;
    public bool Interface_MapEditor_ToolWindow = true;
    public bool Interface_MapEditor_MapInformation = false;

    // Tools
    public bool Interface_MapEditor_Tool_TreasureMaker = false;
    public bool Interface_MapEditor_Tool_WorldMapLayoutGenerator = false;
    public bool Interface_MapEditor_Tool_AssetBrowser = true;

    // Windows
    public bool Interface_ModelEditor_Viewport_Grid = true;
    public bool Interface_ModelEditor_ResourceList = true;
    public bool Interface_ModelEditor_ToolWindow = true;
    public bool Interface_ModelEditor_ScreenshotMode = false;

    // Tools
    public bool Interface_ModelEditor_Tool_CreateAction = true;
    public bool Interface_ModelEditor_Tool_ModelInsight = true;

    public bool Interface_ModelEditor_Tool_Data_Transfer = true;
    public bool Interface_ModelEditor_Tool_ViewportConfig = true;

    ///------------------------------------------------------------
    /// Time Act Editor
    ///------------------------------------------------------------
    public bool Interface_TimeActEditor_ToolWindow = true;

    // 
    public bool Interface_ParamEditor_Table = true;
    public bool Interface_ParamEditor_ToolWindow = true;

    public bool Interface_ProjectileEditor_List = true;
    public bool Interface_ProjectileEditor_Properties = true;
    public bool Interface_ProjectileEditor_ToolWindow = true;
    public bool Interface_ProjectileEditor_Tool_Playback = true;
    public bool Interface_ProjectileEditor_Tool_SimulationFilters = true;

    // FaceGen Editor
    public bool Interface_FaceGenEditor_List = true;
    public bool Interface_FaceGenEditor_Properties = true;
    public bool Interface_FaceGenEditor_References = true;
    public bool Interface_FaceGenEditor_ToolWindow = true;
    public bool Interface_FaceGenEditor_Tool_ViewportFilters = true;
    public bool Interface_FaceGenEditor_Tool_CharacterPreview = true;
    public bool Interface_FaceGenEditor_Tool_AnimationBrowser = true;
    public bool Interface_FaceGenEditor_Tool_Randomise = true;
    public bool Interface_FaceGenEditor_Tool_FaceCheck = true;
    public bool Interface_FaceGenEditor_Tool_FaceLibrary = true;

    // Animation Editor
    public bool Interface_AnimationEditor_Characters = true;
    public bool Interface_AnimationEditor_Clips = true;
    public bool Interface_AnimationEditor_Clip = true;
    public bool Interface_AnimationEditor_Bones = true;
    public bool Interface_AnimationEditor_Timeline = true;

    // The skeleton drawn over the character in the viewport
    public bool Interface_AnimationEditor_Skeleton = true;

    // A small sphere at each bone of the skeleton overlay
    public bool Interface_AnimationEditor_Skeleton_Joints = true;

    // The joint spheres' size, times their default (1 % of the skeleton's height)
    public float Interface_AnimationEditor_Joint_Size = 1f;

    // How opaque the character's mesh is drawn (below 1 the bones show through it)
    public float Interface_AnimationEditor_Mesh_Opacity = 1f;

    // Windows
    public bool Interface_TextEditor_ToolWindow = true;

    // Windows
    public bool Interface_GparamEditor_FileList = true;
    public bool Interface_GparamEditor_GroupList = true;
    public bool Interface_GparamEditor_FieldList = true;
    public bool Interface_GparamEditor_FieldValues = true;

    // Tools
    public bool Interface_GparamEditor_ToolWindow = true;

    // Windows
    public bool Interface_TextureViewer_ToolWindow = true;
    public bool Interface_TextureViewer_Properties = true;
    public bool Interface_TextureViewer_Layout = true;

    // Windows
    public bool Interface_MaterialEditor_ToolWindow = true;

    ///------------------------------------------------------------
    /// File Browser
    ///------------------------------------------------------------
    public bool Interface_FileBrowser_FileList = true;
    public bool Interface_FileBrowser_ItemViewer = true;
    public bool Interface_FileBrowser_ToolView = true;

    public bool Interface_BehaviorEditor_ToolWindow = true;
    public bool Interface_BehaviorEditor_Tool_Search = true;
    public bool Interface_BehaviorEditor_Tool_Problems = true;
    public bool Interface_BehaviorEditor_Tool_Changes = true;
    public bool Interface_BehaviorEditor_Tool_Merge = true;
    public bool Interface_BehaviorEditor_Tool_Bookmarks = true;
    public bool Interface_BehaviorEditor_Tool_Templates = true;

    // Havok behaviors: the viewport that plays the behavior selection's clips on a character
    public bool Interface_BehaviorEditor_Viewport = true;
    public bool Interface_BehaviorEditor_Tool_Character_Selector = true;
    public bool Interface_BehaviorEditor_Tool_Character_Preview = true;

    public bool Interface_SoundEditor_ToolWindow = true;

    public bool Interface_EmevdEditor_ToolWindow = true;

    public bool Interface_EsdEditor_ToolWindow = true;

    public bool Interface_LuaEditor_ToolWindow = true;

    // Tools window, and its tools
    public bool Interface_CutsceneEditor_ToolWindow = true;

    // Tools window
    public bool Interface_FxEditor_ToolWindow = true;
    public bool Interface_FxEditor_CurveEditor = true;

    public bool Interface_HavokEditor_ToolWindow = true;

    // English_
    public string English_Font = Path.Join("Assets","Fonts","RobotoMono-Light.ttf");

    // Additional_
    public string Additional_Font_1 = Path.Join("Assets","Fonts","NotoSansCJKtc-Light.otf");
    public string Additional_Font_2 = "";
    public string Additional_Font_3 = "";
    public string Additional_Font_4 = "";
    public string Additional_Font_5 = "";
}
