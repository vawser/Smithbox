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
///     The ESD Editor's settings (<c>CFG.Current.EsdEditor</c>). Saved in Configuration.json under the same EsdEditor_ keys as
///     before they moved here.
/// </summary>
public sealed class EsdEditorSettings : ICfgSection
{
    ///------------------------------------------------------------
    /// ESD Editor (EzState scripts)
    ///------------------------------------------------------------
    public bool EsdEditor_Code_View = true;
    public bool EsdEditor_Show_Comments = true;
    public bool EsdEditor_File_List_Display_Counts = true;
    public bool EsdEditor_Machine_List_Display_State_Count = true;
    public bool EsdEditor_Machine_List_Display_Call_Count = true;
    public bool EsdEditor_File_List_Display_Aliases = true;
    public bool EsdEditor_Machine_List_Display_Aliases = true;
    public bool EsdEditor_Properties_Display_Bytecode = true;
}
