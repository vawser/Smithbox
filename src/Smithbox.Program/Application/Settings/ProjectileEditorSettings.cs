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
///     The Projectile Editor's settings (<c>CFG.Current.ProjectileEditor</c>). Saved in Configuration.json under the same ProjectileEditor_ keys as
///     before they moved here.
/// </summary>
public sealed class ProjectileEditorSettings : ICfgSection
{
    // Projectile Editor: the simulation
    public float ProjectileEditor_LaunchHeight = 1.5f;
    public float ProjectileEditor_EmitHeight = 1.0f;
    public float ProjectileEditor_FixedEmitRadius = 6.0f;
    public float ProjectileEditor_BehindTargetHeight = 8.0f;
    public float ProjectileEditor_BehindTargetDistance = 2.0f;
    public CastOrientation ProjectileEditor_Orientation = CastOrientation.Forward;
    public bool ProjectileEditor_GroundCollision = true;
    public bool ProjectileEditor_SimulateWall = false;
    public float ProjectileEditor_WallDistance = 10.0f;
    public float ProjectileEditor_WallWidth = 12.0f;
    public float ProjectileEditor_WallHeight = 8.0f;
    public ImpactSurface ProjectileEditor_Surface = ImpactSurface.Map;
    public bool ProjectileEditor_SimulateTarget = false;
    public Vector3 ProjectileEditor_TargetPosition = new Vector3(5.0f, 1.5f, 15.0f);
    public float ProjectileEditor_TargetRadius = 0.5f;
    public TargetMotion ProjectileEditor_TargetMotion = TargetMotion.Still;
    public float ProjectileEditor_TargetSpeed = 5.0f;
    public float ProjectileEditor_TargetRange = 5.0f;
    public bool ProjectileEditor_UseEnemyAttacks = false;
    public int ProjectileEditor_Seed = 1;
    public float ProjectileEditor_EmitterDuration = 0.0f;
    public float ProjectileEditor_DistanceScale = 1.0f;
    public bool ProjectileEditor_DistanceUpEffect = false;

    // Projectile Editor: display
    public bool ProjectileEditor_Display_Fx = true;
    public bool ProjectileEditor_Display_Effects = true;
    public bool ProjectileEditor_Display_Hit_Effects = true;
    public bool ProjectileEditor_Display_Trails = true;
    public bool ProjectileEditor_Display_Reach = true;
    public bool ProjectileEditor_Display_Damage = true;
    public bool ProjectileEditor_Display_Ground_Grid = true;
    public bool ProjectileEditor_Display_Bullets = true;
    public bool ProjectileEditor_Display_Impacts = true;
    public bool ProjectileEditor_Display_Target = true;
    public bool ProjectileEditor_Display_Target_Path = true;
    public bool ProjectileEditor_Display_Wall = true;
    public bool ProjectileEditor_Frame_On_Select = false;
    public bool ProjectileEditor_Loop = true;
    public bool ProjectileEditor_Auto_Play_On_Select = true;
    public float ProjectileEditor_Speed = 1.0f;
    public bool ProjectileEditor_Show_Community_Names = true;
}
