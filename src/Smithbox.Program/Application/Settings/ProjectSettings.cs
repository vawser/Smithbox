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
///     The project settings (<c>CFG.Current.Project</c>): how projects are loaded, saved and backed up, and automatic saving. Saved in Configuration.json under the same keys as before they moved here.
/// </summary>
public sealed class ProjectSettings : ICfgSection
{

    // Project_
    ///------------------------------------------------------------
    /// Project
    ///------------------------------------------------------------
    // Preferences
    public bool Project_Enable_Auto_Load = true;
    public bool Project_Enable_Automatic_Auto_Load_Assignment = true;

    public bool Project_Enable_Backup_Saves = true;
    public bool Project_Enable_Comprehensive_Backups = false;

    public bool Project_Enable_Automatic_Save = true;
    public float Project_Automatic_Save_Interval = 300;

    public string Project_Default_Mod_Directory = "";
    public string Project_Default_Data_Directory = "";
    public string Project_ME3_Profile_Directory = "";

    public bool Project_Scan_Directory_For_Additions = true;
    public bool Project_Scan_Directory_Strict_Mode = false;
    public bool Project_VFS_Prefer_Loose_Files = false;

    // Options
    public string Project_Alias_Export_Delimiter = ";";
    public bool Project_Alias_Editor_Export_Ignore_Empty = false;

    public bool Project_Alias_Editor_Use_Base_Source = true;
    public bool Project_Alias_Editor_Use_Project_Source = true;
    public bool Project_Alias_Editor_Save_Applies_To_Base = false;
    public bool Project_Alias_Editor_Add_Insert_At_Top = false;

    public bool Project_Enum_Editor_Use_Base_Source = true;
    public bool Project_Enum_Editor_Use_Project_Source = true;
    public bool Project_Enum_Editor_Save_Applies_To_Base = false;
    public bool Project_Enum_Editor_Add_Insert_At_Top = false;

    // Saving
    public bool Project_Automatic_Save_Include_Map_Editor = true;

    // Saving
    public bool Project_Automatic_Save_Include_Param_Editor = true;

    // Saving
    public bool Project_Automatic_Save_Include_Text_Editor = true;

    // Saving
    public bool Project_Automatic_Save_Include_Gparam_Editor = false;

    // Saving
    public bool Project_Automatic_Save_Include_Material_Editor = false;

    // AutomaticSave_
    public bool AutomaticSave_ModelEditor = true;

    public bool AutomaticSave_TimeActEditor = true;

    public bool AutomaticSave_AnimlEditor = true;
}
