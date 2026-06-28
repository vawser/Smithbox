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
    // General
    [Display(Name = "PREF_Category_General")]
    General = 0,

    // System
    [Display(Name = "PREF_Category_Developer")]
    Developer = 100,

    [Display(Name = "PREF_Category_Loggers")]
    Loggers = 101,

    // Project
    [Display(Name = "PREF_Category_Automatic_Save")]
    AutomaticSave = 200,

    [Display(Name = "PREF_Category_ME3")]
    ModEngine3 = 201,

    [Display(Name = "PREF_Category_Data_Overrides")]
    DataOverride = 202,

    // Interface
    [Display(Name = "PREF_Category_UI_Fonts")]
    Fonts = 300,

    [Display(Name = "PREF_Category_UI_Theme")]
    Theme = 302,

    [Display(Name = "PREF_Category_UI_Theme_Builder")]
    ThemeBuilder = 303,

    // Viewport
    [Display(Name = "PREF_Category_Viewport_Rendering")]
    Rendering = 400,

    [Display(Name = "PREF_Category_Viewport_Model_Rendering")]
    ModelRendering = 401,

    [Display(Name = "PREF_Category_Viewport_Selection")]
    Selection = 402,

    [Display(Name = "PREF_Category_Viewport_Coloring")]
    Coloring = 403,

    [Display(Name = "PREF_Category_Viewport_Filter_Presets")]
    FilterPresets = 404,

    // Map Editor
    [Display(Name = "PREF_Category_MapEditor_General")]
    MapEditor_General = 500,

    [Display(Name = "PREF_Category_MapEditor_Map_List")]
    MapEditor_Map_List = 501,

    [Display(Name = "PREF_Category_MapEditor_Map_Contents")]
    MapEditor_Map_Contents = 502,

    [Display(Name = "PREF_Category_MapEditor_Map_Object_Properties")]
    MapEditor_Map_Object_Properties = 503,

    [Display(Name = "PREF_Category_MapEditor_Additional_Property_Information")]
    MapEditor_Additional_Property_Information = 504,

    [Display(Name = "PREF_Category_MapEditor_Character_Substitution")]
    MapEditor_Character_Substitution = 505,

    [Display(Name = "PREF_Category_MapEditor_Model_Selector")]
    MapEditor_Model_Selector = 506,

    [Display(Name = "PREF_Category_MapEditor_Selection_Groups")]
    MapEditor_Selection_Groups = 507,

    // Model Editor
    [Display(Name = "PREF_Category_ModelEditor_Properties")]
    ModelEditor_Properties = 600,

    // Param Editor
    [Display(Name = "PREF_Category_ParamEditor_Regulation")]
    ParamEditor_Regulation = 700,

    [Display(Name = "PREF_Category_ParamEditor_Param_List")]
    ParamEditor_Param_List = 701,

    [Display(Name = "PREF_Category_ParamEditor_Row_List")]
    ParamEditor_Row_List = 702,

    [Display(Name = "PREF_Category_ParamEditor_Field_List")]
    ParamEditor_Field_List = 703,

    [Display(Name = "PREF_Category_ParamEditor_Field_Input")]
    ParamEditor_Field_Input = 704,

    [Display(Name = "PREF_Category_ParamEditor_Param_Context_Menu")]
    ParamEditor_Param_Context_Menu = 705,

    [Display(Name = "PREF_Category_ParamEditor_Row_Context_Menu")]
    ParamEditor_Row_Context_Menu = 706,

    [Display(Name = "PREF_Category_ParamEditor_Field_Context_Menu")]
    ParamEditor_Field_Context_Menu = 707,

    [Display(Name = "PREF_Category_ParamEditor_Mass_Edit")]
    ParamEditor_Mass_Edit = 708,

    [Display(Name = "PREF_Category_ParamEditor_Param_Reloader")]
    ParamEditor_Param_Reloader = 709,

    [Display(Name = "PREF_Category_ParamEditor_Metadata")]
    ParamEditor_Metadata = 710,

    [Display(Name = "PREF_Category_ParamEditor_Field_Layouts")]
    ParamEditor_Field_Layouts = 711,

    // Text Editor
    [Display(Name = "PREF_Category_TextEditor_Container_List")]
    TextEditor_Container_List = 800,

    [Display(Name = "PREF_Category_TextEditor_Text_File_List")]
    TextEditor_Text_File_List = 801,

    [Display(Name = "PREF_Category_TextEditor_Text_Entry_List")]
    TextEditor_Text_Entry_List = 802,

    [Display(Name = "PREF_Category_TextEditor_Text_Entries")]
    TextEditor_Text_Entries = 803,

    [Display(Name = "PREF_Category_TextEditor_Text_Export")]
    TextEditor_Text_Export = 804,

    [Display(Name = "PREF_Category_TextEditor_Language_Sync")]
    TextEditor_Language_Sync = 805,

    [Display(Name = "PREF_Category_TextEditor_Text_Clipboard")]
    TextEditor_Text_Clipboard = 806,

    // Gparam Editor
    [Display(Name = "PREF_Category_GparamEditor_File_List")]
    GparamEditor_File_List = 900,

    [Display(Name = "PREF_Category_GparamEditor_Group_List")]
    GparamEditor_Group_List = 901,

    [Display(Name = "PREF_Category_GparamEditor_Field_List")]
    GparamEditor_Field_List = 902,

    [Display(Name = "PREF_Category_GparamEditor_Value_List")]
    GparamEditor_Value_List = 903,

    [Display(Name = "PREF_Category_GparamEditor_Color_Edit")]
    GparamEditor_Color_Edit = 904,

    // Texture Viewer
    [Display(Name = "PREF_Category_TextureViewer_File_List")]
    TextureViewer_File_List = 1000,

    [Display(Name = "PREF_Category_TextureViewer_Texture_List")]
    TextureViewer_Texture_List = 1001,

    // Material Editor
    [Display(Name = "PREF_Category_MaterialEditor_Properties")]
    MaterialEditor_Properties = 1100,

    // Map Data Editor
    [Display(Name = "PREF_Category_MapDataEditor_General")]
    MapDataEditor_General = 1200,

    // Anim Editor
    [Display(Name = "PREF_Category_AnimEditor_General")]
    AnimEditor_General = 1200,
}