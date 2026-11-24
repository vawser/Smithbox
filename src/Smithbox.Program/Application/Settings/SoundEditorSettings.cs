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
///     The Sound Editor's settings (<c>CFG.Current.SoundEditor</c>). Saved in Configuration.json under the same SoundEditor_ keys as
///     before they moved here.
/// </summary>
public sealed class SoundEditorSettings : ICfgSection
{
    ///------------------------------------------------------------
    /// Sound Editor (FMOD / Wwise)
    ///------------------------------------------------------------
    public float SoundEditor_Volume = 0.8f;
    public bool SoundEditor_Loop = false;
    public bool SoundEditor_Play_On_Select = false;
    public string SoundEditor_Export_Folder = "";
    public string SoundEditor_Import_Folder = "";
}
