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
///     The Material Editor's settings (<c>CFG.Current.MaterialEditor</c>). Saved in Configuration.json under the same MaterialEditor_ keys as
///     before they moved here.
/// </summary>
public sealed class MaterialEditorSettings : ICfgSection
{
    ///------------------------------------------------------------
    /// Material Editor
    ///------------------------------------------------------------
    // General
    public bool MaterialEditor_FileList_DisplayFullPath = false;

    public bool MaterialEditor_Properties_Display_Community_Names = false;
    public bool MaterialEditor_Properties_Display_Type_Column = false;

    // Tools
    public bool MaterialEditor_Live_Preview = true;
    public bool MaterialEditor_FileList_Modified_Only = false;
    public bool MaterialEditor_InGame_AutoRefresh = true;
    public bool MaterialEditor_InGame_SortRecent = true;
    public bool MaterialEditor_InGame_SendOnSave = false;
    public int MaterialEditor_InGame_AfterReload = 1; // MatReloader.AfterReload: 0 nothing, 1 reload world, 2 quit out

    public bool MaterialEditor_Textures_Display_Preview = true;

    public bool MaterialEditor_Allow_Unmodified_Save = false;
    public bool MaterialEditor_Allow_Automatic_Save = true;
}
