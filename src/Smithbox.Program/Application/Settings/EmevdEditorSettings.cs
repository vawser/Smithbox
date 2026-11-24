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
///     The EMEVD Editor's settings (<c>CFG.Current.EmevdEditor</c>). Saved in Configuration.json under the same EmevdEditor_ keys as
///     before they moved here.
/// </summary>
public sealed class EmevdEditorSettings : ICfgSection
{
    ///------------------------------------------------------------
    /// EMEVD Editor (event scripts)
    ///------------------------------------------------------------
    public bool EmevdEditor_Show_Arg_Names = true;
    public bool EmevdEditor_Show_Comments = true;
    public bool EmevdEditor_Structured_View = true;
    public bool EmevdEditor_Inline_MustSub_Calls = true;
    public bool EmevdEditor_Allow_Unmodified_Save = false;
    public bool EmevdEditor_Allow_Automatic_Save = true;
    public bool EmevdEditor_File_List_Display_Aliases = true;
    public bool EmevdEditor_File_List_Display_Event_Count = true;
    public bool EmevdEditor_Event_List_Display_Aliases = true;
    public bool EmevdEditor_Event_List_Display_Line_Count = true;
}
