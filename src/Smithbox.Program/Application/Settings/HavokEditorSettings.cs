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
///     The Havok Editor's settings (<c>CFG.Current.HavokEditor</c>). Saved in Configuration.json under the same HavokEditor_ keys as
///     before they moved here.
/// </summary>
public sealed class HavokEditorSettings : ICfgSection
{
    public bool HavokEditor_Allow_Unmodified_Save = false;
    public bool HavokEditor_Allow_Automatic_Save = true;

    public bool HavokEditor_BinderList_Display_Full_Path = false;
    public bool HavokEditor_BinderList_Display_Aliases = true;

    public bool HavokEditor_FileList_Display_Full_Path = false;
    public bool HavokEditor_FileList_Display_Aliases = true;
    public int HavokEditor_FileList_Collision_Type_Filter = 0;

    public bool HavokEditor_EntryList_Display_Aliases = true;

    public bool HavokEditor_Properties_Display_Community_Names = false;
    public bool HavokEditor_Properties_Display_Type_Column = false;
    public bool HavokEditor_Properties_Display_Raw_Data_Fields = true;
    public bool HavokEditor_Properties_Auto_Open_Tree = false;
    public bool HavokEditor_Properties_Display_Property_Bags = false;
}
