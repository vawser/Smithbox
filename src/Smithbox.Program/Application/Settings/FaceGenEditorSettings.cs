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
///     The FaceGen Editor's settings (<c>CFG.Current.FaceGenEditor</c>). Saved in Configuration.json under the same FaceGenEditor_ keys as
///     before they moved here.
/// </summary>
public sealed class FaceGenEditorSettings : ICfgSection
{
    public bool FaceGenEditor_Show_Internal_Names = false;

    /// <summary>The body the face is shown on: 0 as the first character wearing it, 1 male, 2 female</summary>
    public int FaceGenEditor_Body = 0;

    /// <summary>Whether the viewport shapes the face parts by the face's sliders (off: the parts' base shapes)</summary>
    public bool FaceGenEditor_Apply_Shape = true;
}
