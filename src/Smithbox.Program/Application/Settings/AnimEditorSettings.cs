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
///     The Animation Editor's settings (<c>CFG.Current.AnimEditor</c>). Saved in Configuration.json under the same AnimEditor_ keys as
///     before they moved here.
/// </summary>
public sealed class AnimEditorSettings : ICfgSection
{
    public bool AnimEditor_AutomaticSave_IncludeTAE = true;
    public bool AnimEditor_ManualSave_IncludeTAE = true;
    public bool AnimEditor_AutomaticSave_IncludeBEH = true;
    public bool AnimEditor_ManualSave_IncludeBEH = true;

    // Anim Editor: Primary Grid
    public bool AnimEditor_DisplayPrimaryGrid = false;
    public bool AnimEditor_RegeneratePrimaryGrid = false;
    public int AnimEditor_PrimaryGrid_Size = 1000;
    public float AnimEditor_PrimaryGrid_SectionSize = 10;

    public Vector3 AnimEditor_PrimaryGrid_Color = new Vector3(0.5f, 0.5f, 0.5f);

    public float AnimEditor_PrimaryGrid_Position_X = 0;
    public float AnimEditor_PrimaryGrid_Position_Y = 0;
    public float AnimEditor_PrimaryGrid_Position_Z = 0;

    public float AnimEditor_PrimaryGrid_Rotation_X = 0;
    public float AnimEditor_PrimaryGrid_Rotation_Y = 0;
    public float AnimEditor_PrimaryGrid_Rotation_Z = 0;

    // Anim Editor: Secondary Grid
    public bool AnimEditor_DisplaySecondaryGrid = false;
    public bool AnimEditor_RegenerateSecondaryGrid = false;
    public int AnimEditor_SecondaryGrid_Size = 1000;
    public float AnimEditor_SecondaryGrid_SectionSize = 10;

    public Vector3 AnimEditor_SecondaryGrid_Color = new Vector3(0.5f, 0.5f, 0.5f);

    public float AnimEditor_SecondaryGrid_Position_X = 0;
    public float AnimEditor_SecondaryGrid_Position_Y = 0;
    public float AnimEditor_SecondaryGrid_Position_Z = 0;

    public float AnimEditor_SecondaryGrid_Rotation_X = 90;
    public float AnimEditor_SecondaryGrid_Rotation_Y = 0;
    public float AnimEditor_SecondaryGrid_Rotation_Z = 0;

    // Anim Editor: Tertiary Grid
    public bool AnimEditor_DisplayTertiaryGrid = false;
    public bool AnimEditor_RegenerateTertiaryGrid = false;
    public int AnimEditor_TertiaryGrid_Size = 1000;
    public float AnimEditor_TertiaryGrid_SectionSize = 10;

    public Vector3 AnimEditor_TertiaryGrid_Color = new Vector3(0.5f, 0.5f, 0.5f);

    public float AnimEditor_TertiaryGrid_Position_X = 0;
    public float AnimEditor_TertiaryGrid_Position_Y = 0;
    public float AnimEditor_TertiaryGrid_Position_Z = 0;

    public float AnimEditor_TertiaryGrid_Rotation_X = 0;
    public float AnimEditor_TertiaryGrid_Rotation_Y = 0;
    public float AnimEditor_TertiaryGrid_Rotation_Z = 90;
}
