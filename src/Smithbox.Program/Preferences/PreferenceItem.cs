using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Preferences;

public class PreferenceItem
{
    public int OrderID { get; set; } = -1;

    public PreferenceCategory Category { get; set; }

    public List<ProjectType> DisplayRestrictions = new(){
        ProjectType.Undefined
    };

    public SectionCategory Section;

    public string Title;
    public string Description;

    public bool Spacer = false;
    public bool InlineName = true;

    public Action PreDraw;
    public Action Draw;
    public Action PostDraw;
}

public enum PreferenceCategory
{
    System,
    Project,
    Interface,
    Viewport,
    MapEditor,
    ModelEditor,
    ParamEditor,
    TextEditor,
    GparamEditor,
    MaterialEditor,
    TextureViewer
}

public enum SectionCategory
{
    [Display(Name = "General")]
    General = 0,

    // System
    [Display(Name = "Developer")]
    Developer = 100,

    [Display(Name = "Loggers")]
    Loggers = 101,

    // Project
    [Display(Name = "Automatic Save")]
    AutomaticSave = 200,

    [Display(Name = "Mod Engine 3")]
    ModEngine3 = 201,

    [Display(Name = "Data Overrides")]
    DataOverride = 202,

    // Interface
    [Display(Name = "Fonts")]
    Fonts = 300,

    [Display(Name = "Additional Font Symbols")]
    AdditionalFontSymbols = 301,

    [Display(Name = "Theme")]
    Theme = 302,

    [Display(Name = "Theme Builder")]
    ThemeBuilder = 303,

    // Viewport
    [Display(Name = "Rendering")]
    Rendering = 400,

    [Display(Name = "Model Rendering")]
    ModelRendering = 401,

    [Display(Name = "Selection")]
    Selection = 402,

    [Display(Name = "Coloring")]
    Coloring = 403,

    [Display(Name = "Filter Presets")]
    FilterPresets = 404,

    // Map Editor
    [Display(Name = "Map List")]
    MapEditor_Map_List = 500,

    [Display(Name = "Map Contents")]
    MapEditor_Map_Contents = 501,

    [Display(Name = "Map Object Properties")]
    MapEditor_Map_Object_Properties = 502,

    [Display(Name = "Additional Property Information")]
    MapEditor_Additional_Property_Information = 503,

    [Display(Name = "Character Substitution")]
    MapEditor_Character_Substitution = 504,

    [Display(Name = "Model Selector")]
    MapEditor_Model_Selector = 505,

    [Display(Name = "Selection Groups")]
    MapEditor_Selection_Groups = 506,

    // Model Editor
    [Display(Name = "Properties")]
    ModelEditor_Properties = 600,

    // Param Editor
    [Display(Name = "Regulation")]
    ParamEditor_Regulation = 700,

    [Display(Name = "Param List")]
    ParamEditor_Param_List = 701,

    [Display(Name = "Row List")]
    ParamEditor_Row_List = 702,

    [Display(Name = "Field List")]
    ParamEditor_Field_List = 703,

    [Display(Name = "Field Input")]
    ParamEditor_Field_Input = 704,

    [Display(Name = "Param Context Menu")]
    ParamEditor_Param_Context_Menu = 705,

    [Display(Name = "Row Context Menu")]
    ParamEditor_Row_Context_Menu = 706,

    [Display(Name = "Field Context Menu")]
    ParamEditor_Field_Context_Menu = 707,

    [Display(Name = "Mass Edit")]
    ParamEditor_Mass_Edit = 708,

    [Display(Name = "Param Reloader")]
    ParamEditor_Param_Reloader = 709,

    // Text Editor
    [Display(Name = "Container List")]
    TextEditor_Container_List = 800,

    [Display(Name = "Text File List")]
    TextEditor_Text_File_List = 801,

    [Display(Name = "Text Entry List")]
    TextEditor_Text_Entry_List = 802,

    [Display(Name = "Text Entires")]
    TextEditor_Text_Entries = 803,

    [Display(Name = "Text Export")]
    TextEditor_Text_Export = 804,

    [Display(Name = "Language Sync")]
    TextEditor_Language_Sync = 805,

    [Display(Name = "Text Clipboard")]
    TextEditor_Text_Clipboard = 806,

    // Gparam Editor

    // Material Editor

    // Texture Viewer
}