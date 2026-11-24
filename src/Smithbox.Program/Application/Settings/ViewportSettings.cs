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
///     The viewport settings (<c>CFG.Current.Viewport</c>): rendering, camera, colors and the rest the viewports and
///     the renderer read. Saved in Configuration.json under the same Viewport_ keys as before they moved here. The
///     per-editor grid settings stay on CFG with their editors' settings.
/// </summary>
public sealed class ViewportSettings : ICfgSection
{
    public RenderFilterPreset Viewport_Filter_Preset_1 { get; set; } = new("Map", RenderFilter.MapPiece | RenderFilter.Object | RenderFilter.Character | RenderFilter.DummyEnemy | RenderFilter.DummyAsset | RenderFilter.Region);
    public RenderFilterPreset Viewport_Filter_Preset_2 { get; set; } = new("Collision", RenderFilter.Collision | RenderFilter.Object | RenderFilter.Character | RenderFilter.DummyEnemy | RenderFilter.DummyAsset | RenderFilter.Region);
    public RenderFilterPreset Viewport_Filter_Preset_3 { get; set; } = new("Collision & Navmesh", RenderFilter.Collision | RenderFilter.Navmesh | RenderFilter.Object | RenderFilter.Character | RenderFilter.DummyEnemy | RenderFilter.DummyAsset | RenderFilter.Region);
    public RenderFilterPreset Viewport_Filter_Preset_4 { get; set; } = new("Lighting (Map)", RenderFilter.MapPiece | RenderFilter.Object | RenderFilter.Character | RenderFilter.DummyEnemy | RenderFilter.DummyAsset | RenderFilter.Light);
    public RenderFilterPreset Viewport_Filter_Preset_5 { get; set; } = new("Lighting (Collision)", RenderFilter.Collision | RenderFilter.Object | RenderFilter.Character | RenderFilter.DummyEnemy | RenderFilter.DummyAsset | RenderFilter.Light);
    public RenderFilterPreset Viewport_Filter_Preset_6 { get; set; } = new("All", RenderFilter.All);

    ///------------------------------------------------------------
    /// Viewport
    ///------------------------------------------------------------
    public float Viewport_Frame_Rate = 60.0f;
    public bool Viewport_Enable_Rendering = true;
    public bool Viewport_Enable_Texturing = true;

    // 0 Editor: fullbright, with the lighting, shadows, GI, fog and screen effects off. 1 Game: those follow the
    // Rendering tool's settings. See RenderMode.
    public int Viewport_Render_Mode = 0;

    // 0 Diffuse: the textured image. 1 Normal: the surface's normals in its place, the Normals surface view.
    public int Viewport_Texturing_Mode = 0;

    // The wireframe view: lines along the triangles of FLVER models, drawn by the shaders' _wire variants (see
    // MeshRenderables.cWireframePipelines). Only: the surfaces are left out, so only the lines show. The color is
    // a display color, 0 to 1; the opacity is how much the lines cover the surfaces when they are shown.
    public bool Viewport_Wireframe_Enable = false;
    public bool Viewport_Wireframe_Only = false;
    public Vector3 Viewport_Wireframe_Line_Color = new Vector3(0.85f, 0.9f, 1.0f);
    public float Viewport_Wireframe_Line_Opacity = 0.75f;
    public float Viewport_Wireframe_Line_Width = 1.0f;

    public bool Viewport_Enable_Material_Animation = true;
    public float Viewport_Wind_Strength = 1.0f;
    public float Viewport_Wind_Direction = 45.0f;
    public bool Viewport_Material_Wind = true;
    public bool Viewport_Material_Vertex_Animation = true;
    public bool Viewport_Material_Sway = true;
    public bool Viewport_Material_Birds = true;
    public bool Viewport_Material_Layer_Blending = true;
    public bool Viewport_Material_Overlays = true;
    public bool Viewport_Material_Magma = true;
    public bool Viewport_Material_Emissive = true;
    public bool Viewport_Material_UV_Scroll = true;
    public bool Viewport_Material_Unlit = true;
    public bool Viewport_Material_Water = true;
    public bool Viewport_Material_Fur = true;
    public bool Viewport_Enable_Culling = true;

    public bool Viewport_Enable_Selection_Tint = true;
    public bool Viewport_Enable_Model_Masks = true;
    public bool Viewport_Enable_LOD_Facesets = false;

    public bool Viewport_Display = true;
    public bool Viewport_Display_Profiling = true;

    public Vector3 Viewport_Frame_Offset = new Vector3();
    public float Viewport_Frame_Distance = 1f;

    public bool Viewport_Render_Gizmos = true;
    public float Viewport_Gizmo_Size_Distance_Scale = 0.04f;

    // Degrees per step while snapping a gizmo rotation (Ctrl held), 0 for none
    public float Viewport_Gizmo_Snap_Rotation_Step = 15f;

    // Viewport Grids (shared by every grid)
    public int Viewport_Grid_Major_Line_Interval = 10;
    public bool Viewport_Grid_Display_Axis_Lines = true;
    public float Viewport_Grid_Opacity = 1.0f;
    public float Viewport_Grid_Fade_Distance = 2000f;
    public float Viewport_Grid_Line_Width = 1f;
    public float Viewport_Grid_Hidden_Opacity = 0f;

    // Camera
    public float Viewport_Camera_FOV { get; set; } = 60.0f;
    public float Viewport_Camera_MoveSpeed_Slow { get; set; } = 1.0f;
    public float Viewport_Camera_MoveSpeed_Normal { get; set; } = 20.0f;
    public float Viewport_Camera_MoveSpeed_Fast { get; set; } = 200.0f;
    public float Viewport_Camera_Sensitivity { get; set; } = 0.0160f;

    public float Viewport_Perspective_Near_Clip { get; set; } = 0.1f;
    public float Viewport_Perspective_Far_Clip { get; set; } = 50000.0f;

    // The clip round the camera, see SceneParam.CameraClip
    public bool Viewport_Camera_Clip_Enabled { get; set; } = false;
    public float Viewport_Camera_Clip_Radius { get; set; } = 5.0f;

    public float Viewport_Orthographic_Near_Clip { get; set; } = -50000.0f;
    public float Viewport_Orthographic_Far_Clip { get; set; } = 50000.0f;

    public float Viewport_DefaultOrthographicSize { get; set; } = 10.0f;
    public float Viewport_DefaultObliqueAngle { get; set; } = 45.0f;
    public float Viewport_DefaultObliqueScaling { get; set; } = 0.5f;
    public float Viewport_MousePan_Sensitivity { get; set; } = 25.0f;
    public bool Viewport_Orthographic_Zoom_To_Cursor { get; set; } = true;
    public bool Viewport_Preset_View_Switch_To_Orthographic { get; set; } = true;
    public bool Viewport_Oblique_Snap_To_Axis { get; set; } = true;
    public float Viewport_Slice_Half_Depth { get; set; } = 2.5f;
    public bool Viewport_DisplayScaleBar { get; set; } = true;

    // Rendering Limits
    public uint Viewport_Limit_Buffer_Flver_Bone = 65536;
    public uint Viewport_Limit_Buffer_Indirect_Draw = 50000;

    // Wireframe Coloring
    public float Viewport_Wireframe_Color_Variance = 0.11f;

    public Vector3 Viewport_Collision_Color = new Vector3(53, 157, 255);
    public Vector3 Viewport_High_Collision_Color = new Vector3(255, 160, 40);
    public Vector3 Viewport_FallProtection_Collision_Color = new Vector3(70, 210, 90);
    public Vector3 Viewport_Wall_Fill_Color = new Vector3(60, 230, 120);
    public Vector3 Viewport_Wall_Other_Collision_Color = new Vector3(190, 90, 255);
    public Vector3 Viewport_Wall_None_Color = new Vector3(255, 60, 60);
    public Vector3 Viewport_Wall_NoJump_Color = new Vector3(255, 170, 30);
    public Vector3 Viewport_Wall_Hover_Color = new Vector3(255, 255, 255);
    public float Viewport_Wall_Alpha = 45.0f;

    // EDGE points (SDT), drawn by type; the EDGE Editor's selected entry is drawn in the selected colour
    public Vector3 Viewport_Edge_Grapple_Color = new Vector3(255, 220, 40);
    public Vector3 Viewport_Edge_Hang_Color = new Vector3(40, 220, 255);
    public Vector3 Viewport_Edge_Hug_Color = new Vector3(255, 110, 200);
    public Vector3 Viewport_Edge_Pull_Color = new Vector3(255, 140, 0);
    public Vector3 Viewport_Edge_Selected_Color = new Vector3(255, 255, 255);
    public Vector3 Viewport_Edge_Hover_Color = new Vector3(120, 255, 120);
    public float Viewport_Edge_Point_Size = 0.15f;
    public float Viewport_Edge_Segment_Radius = 0.05f;
    public Vector3 Viewport_Connect_Collision_Color = new Vector3(146, 57, 158);
    public Vector3 Viewport_Navmesh_Color = new Vector3(157, 53, 255);
    public Vector3 Viewport_Navmesh_Gate_Color = new Vector3(50, 220, 0);
    public float Viewport_Collision_Opacity = 1.0f;

    // The bake of the irradiance volumes to light maps with (AC6, ER, NR), the NN of mAA_BB_CC_DD_envmap_NN
    public int Viewport_GI_Bake = 0;

    // Whether models are lit by the irradiance volumes, and how brightly
    public bool Viewport_GI_Enable = true;
    public bool Viewport_GI_Auto_Exposure = true;
    public float Viewport_GI_Intensity = 1.0f;

    // Blending towards a second bake, as the game does between times of day; its textures load with the map
    public bool Viewport_GI_Bake_Blend = false;
    public int Viewport_GI_Blend_Bake = 1;
    public float Viewport_GI_Blend_Ratio = 0.5f;

    // Whether the environment map's reflections are scaled by the volumes' light, as the game does
    public bool Viewport_GI_Normalize_Reflections = true;

    // Lighting map viewports from the map's GPARAMs (AC6, ER, NR): the file looked in first ("" for mAA_BB_0000),
    // and the time of day on the GPARAMs' TimeOfDay scale, whose first daytime key is 4
    public bool Viewport_Map_Lighting = true;
    public string Viewport_Map_Lighting_File = "";
    public float Viewport_Map_Lighting_Hours = 4.0f;

    // Scales the GPARAM's exposure, for maps where the game's auto exposure would set it differently
    public float Viewport_Map_Lighting_Exposure_Scale = 1.0f;

    // Demon's Souls lights its maps from DrawParam (DrawParamLightingBank): the LightBank and ToneMapBank rows to
    // use, -1 for the one most of the map's pieces use
    public int Viewport_Map_DrawParam_Light_Row = -1;
    public int Viewport_Map_DrawParam_ToneMap_Row = -1;
    public int Viewport_Map_DrawParam_Shadow_Row = -1;
    public int Viewport_Map_DrawParam_ToneCorrect_Row = -1;
    public int Viewport_Map_DrawParam_Fog_Row = -1;

    // Whether each part is lit by the LightBank row its own LightID picks, rather than all by one row
    public bool Viewport_Map_DrawParam_Per_Part = true;

    // Whether Demon's Souls' map materials are lit as the game's HemEnv shaders light them, by the hemisphere and the
    // LightBank row's env cubemaps instead of the sun; off, they take the sun as characters do
    public bool Viewport_Map_DrawParam_Env_Lighting = true;

    // Whether Demon's Souls' Light events light the viewport, at their region (PointLightID) as their PointLightBank
    // row (PointLightBankID) gives it
    public bool Viewport_Map_DrawParam_Point_Lights = true;

    // Whether the map lighting's colour correction (Demon's Souls' ToneCorrectBank) is drawn
    public bool Viewport_Post_Tone_Correction = true;

    // Whether the GPARAM's hemisphere light adds to the irradiance volumes' light, as well as lighting what they
    // don't cover; compared with the game, it does
    public bool Viewport_Map_Lighting_Hemi_Inside_Volumes = true;

    // DS3's lightmaps of its map pieces (the DOL textures of gi_env_mAA), loaded with the map and lighting the pieces
    // in place of the light probes, see DolLightmapBank
    public bool Viewport_Map_Lightmaps = true;

    // The viewport's scale on DS3's lightmaps, on top of the game's own (GparamProfile.LightmapScale), to match the
    // game while the game's scale of them isn't known
    public float Viewport_Map_Lightmap_Intensity = 1.0f;

    // A debug view of the map lighting in the flver shader: 0 off, 1 the source of the light from around (green
    // lightmap, blue light probes or volumes, red hemisphere, yellow a lightmapped material without its lightmap),
    // 2 that light alone, 3 the share of the sun let through
    public int Viewport_Lighting_Debug_View = 0;

    // Whether the viewport's background takes the GPARAM's BG color instead of Viewport_Bg_Color
    public bool Viewport_Map_Lighting_Background = true;

    // Whether characters are lit by the GPARAM LightSet's model set (its sun, fill lights and hemisphere) instead of
    // the scene's; off until compared with the game
    public bool Viewport_Map_Lighting_Model_Lights = false;

    // The weather to light with (WeatherParam's GparamId), 0 for none: the WW of the area's mAA_BB_CCWW files, and the
    // shared m00_00_WWWW_weatheroutdoor/_weatherbase overlays, which the game puts over outdoor areas
    public int Viewport_Map_Lighting_Weather = 0;

    // Whether the chosen weather's shared overlays are read, for an outdoor map; off for an interior
    public bool Viewport_Map_Lighting_Weather_Outdoor = true;

    // Wet surfaces in a weather that wets them, from the GPARAM's Wet group
    public bool Viewport_Post_Wet = true;

    // Whether wet surfaces are kept dry under roofs, by a depth map of the scene seen from above
    public bool Viewport_Post_Wet_Rain_Mask = true;

    // Elden Ring: whether the irradiance volumes' bakes follow the time of day (bake N is timezone N), loading the
    // bakes they need as the time moves, instead of the Global Illumination tab's bake settings
    public bool Viewport_Map_Lighting_Follow_Bakes = true;

    // Elden Ring, while the bakes follow the time of day: whether every bake is loaded with a map, so the time can be
    // changed without waiting, or only those of the time of day the map is loaded at, the others loading as the time
    // reaches them. A map's bakes take hundreds of MB of GPU memory each (m11_00_00_00 has 5, about 700 textures each).
    public bool Viewport_Map_Lighting_Load_All_Bakes = false;

    // Resource loading: how much memory a load may hold in flight and how many containers it reads at once, see
    // LoadMemoryBudget. A LoadMemoryBudget.Profile; the two values below are used by the Custom profile.
    public int Viewport_Load_Memory_Profile = (int)StudioCore.Renderer.LoadMemoryBudget.Profile.Balanced;
    public int Viewport_Load_Memory_Budget_MB = 1024;
    public int Viewport_Load_Container_Readers = 4;

    // The Model and Time Act Editors' viewport environment (AC6, ER, NR): the map whose GPARAM lighting and
    // irradiance bake light them ("" for the project type's default, see ViewportEnvironment), the GPARAM looked in
    // first ("" for the map's own), the volume of the bake (-1 for the one of median light), and the size of the
    // box it is stretched over, which is centred on the origin
    public string Viewport_Environment_Map = "";
    public string Viewport_Environment_Lighting_File = "";
    public int Viewport_Environment_Volume = -1;
    public float Viewport_Environment_Volume_Size = 30.0f;

    // How the viewports' HDR scene is taken to the screen, a ToneMapSetting: from the map's GPARAM Tone Map group,
    // or a fixed curve. The exposure is on top of the GPARAM's, and the white point is for a fixed Reinhard curve.
    public int Viewport_ToneMap = 0;
    public float Viewport_ToneMap_Exposure = 1.0f;
    public float Viewport_ToneMap_White_Point = 1.5f;

    // Whether the viewports' screen effects run (bloom, fog, light shafts... see PostProcessChain), from the map
    // GPARAM's groups. Each effect has its own switch as it is added.
    public bool Viewport_Post_Effects = true;

    // The GPARAM's auto exposure (Tone Map), bloom (Bloom's Glare fields), and FXAA after the tone map (Anti-Alias)
    public bool Viewport_Post_Auto_Exposure = true;
    public bool Viewport_Post_Bloom = true;

    // The viewport's scale on the bloom, over the GPARAM's GlareBloom Alpha, to match a game whose CPU-side bloom
    // scale isn't known (Sekiro)
    public float Viewport_Post_Bloom_Strength = 1.0f;

    // The GPARAM's Glow Filter, round what surfaces give off themselves, and the viewport's scale on it
    public bool Viewport_Post_Glow = true;
    public float Viewport_Post_Glow_Strength = 1.0f;
    public bool Viewport_Post_Fxaa = true;

    // The GPARAM's TAA (Anti-Alias Type 5, as Elden Ring's mapbase has), which replaces FXAA where it runs
    public bool Viewport_Post_Taa = true;

    // The GPARAM's VolumetricFog (Elden Ring's distance and height fog), with the sun's shadows through it and the
    // map's lights in it
    public bool Viewport_Post_Volumetric_Fog = true;

    // The older games' distance fog (Demon's Souls' FogBank), DistanceFogPass
    public bool Viewport_Post_Distance_Fog = true;
    public bool Viewport_Post_Fog_Shadows = true;
    public bool Viewport_Post_Fog_Local_Lights = true;

    // The viewport's scale on the fog's density, over the GPARAM's, to match a game whose CPU-side density scale
    // isn't known (Sekiro's GameScale)
    public float Viewport_Post_Fog_Density_Scale = 1.0f;

    // Whether transparent flvers (water, glass) are fogged at their own depth, as the opaque scene behind them is
    public bool Viewport_Post_Fog_Transparents = true;

    // The GPARAM's LightShaft group: rays from the sun where the sky shows through
    public bool Viewport_Post_Light_Shafts = true;

    // The GPARAM's SSAO, and the viewport's scale on how much it darkens
    public bool Viewport_Post_Ssao = true;
    public float Viewport_Post_Ssao_Strength = 1.0f;

    // The GPARAM's SSR, and the viewport's scale on how much of the environment's reflection it replaces
    public bool Viewport_Post_Ssr = true;
    public float Viewport_Post_Ssr_Strength = 1.0f;

    // The GPARAM's ColorGrading LUT, applied after the tone map
    public bool Viewport_Post_Color_Grading = true;

    // A debug view of the scene's surface attachments in place of the image, see SurfaceView: 0 for none
    public int Viewport_Post_Surface_View = 0;

    // Sun shadows: cascaded shadow maps whose splits, fade and bias come from the map's GPARAM Shadows group, or
    // Elden Ring's mapbase without map lighting. The resolution is that of one cascade; the map holds four.
    public bool Viewport_Shadows = true;
    public int Viewport_Shadow_Resolution = 2048;
    public float Viewport_Shadow_Bias_Scale = 1.0f;
    public float Viewport_Shadow_Strength = 1.0f;

    // Whether MSB parts cast and receive shadows as their IsShadowSrc and IsShadowDest flags say; all do otherwise
    public bool Viewport_Shadows_Use_Msb_Flags = true;

    // The Viewport Rendering tool's Scene Lighting tab: the sun used without map lighting, and the light multipliers.
    // Copied into every viewport's scene params each frame.
    public Vector3 Viewport_Scene_Light_Direction = new Vector3(1.0f, -0.5f, 0.0f);
    public Vector3 Viewport_Scene_Sun_Color = new Vector3(1.0f, 1.0f, 1.0f);
    public float Viewport_Scene_Sun_Intensity = 1.0f;
    public float Viewport_Scene_Direct_Light_Mult = 1.0f;
    public float Viewport_Scene_Indirect_Light_Mult = 1.0f;
    public float Viewport_Scene_Brightness = 1.0f;

    // The light that follows the camera, for lighting up dark interiors
    public bool Viewport_Camera_Light = false;
    public Vector3 Viewport_Camera_Light_Color = new Vector3(1.0f, 1.0f, 1.0f);
    public float Viewport_Camera_Light_Intensity = 8.0f;
    public float Viewport_Camera_Light_Range = 25.0f;

    // Whether the Viewport Rendering tool's tabs show their Status, Resolved Values and Debug sections
    public bool Viewport_Rendering_Show_Status = true;
    public bool Viewport_Rendering_Show_Resolved_Values = false;
    public bool Viewport_Rendering_Show_Debug = false;

    public Vector3 Viewport_Box_Region_Base_Color = Utils.GetDecimalColor(Color.Blue);
    public Vector3 Viewport_Box_Region_Highlight_Color = Utils.GetDecimalColor(Color.DarkViolet);

    public Vector3 Viewport_Cylinder_Region_Base_Color = Utils.GetDecimalColor(Color.Blue);
    public Vector3 Viewport_Cylinder_Region_Highlight_Color = Utils.GetDecimalColor(Color.DarkViolet);

    public Vector3 Viewport_Sphere_Region_Base_Color = Utils.GetDecimalColor(Color.Blue);
    public Vector3 Viewport_Sphere_Region_Highlight_Color = Utils.GetDecimalColor(Color.DarkViolet);

    public Vector3 Viewport_Point_Region_Base_Color = Utils.GetDecimalColor(Color.Yellow);
    public Vector3 Viewport_Point_Region_Highlight_Color = Utils.GetDecimalColor(Color.DarkViolet);

    public float Viewport_Region_Selection_Fill_Alpha = 20.0f;

    public Vector3 Viewport_Dummy_Polygon_Base_Color = Utils.GetDecimalColor(Color.Yellow);
    public Vector3 Viewport_Dummy_Polygon_Highlight_Color = Utils.GetDecimalColor(Color.DarkViolet);
    public float Viewport_Dummy_Polygon_Alpha = 75.0f;

    public Vector3 Viewport_Bone_Marker_Base_Color = Utils.GetDecimalColor(Color.Blue);
    public Vector3 Viewport_Bone_Marker_Highlight_Color = Utils.GetDecimalColor(Color.DarkViolet);
    public float Viewport_Bone_Marker_Alpha = 75.0f;

    public Vector3 Viewport_Character_Marker_Base_Color = Utils.GetDecimalColor(Color.Firebrick);
    public Vector3 Viewport_Character_Marker_Highlight_Color = Utils.GetDecimalColor(Color.Tomato);
    public float Viewport_Character_Marker_Alpha = 75.0f;

    public Vector3 Viewport_Object_Marker_Base_Color = Utils.GetDecimalColor(Color.MediumVioletRed);
    public Vector3 Viewport_Object_Marker_Highlight_Color = Utils.GetDecimalColor(Color.DeepPink);
    public float Viewport_Object_Marker_Alpha = 75.0f;

    public Vector3 Viewport_Player_Marker_Base_Color = Utils.GetDecimalColor(Color.DarkOliveGreen);
    public Vector3 Viewport_Player_Marker_Highlight_Color = Utils.GetDecimalColor(Color.OliveDrab);
    public float Viewport_Player_Marker_Alpha = 75.0f;

    public Vector3 Viewport_Other_Marker_Base_Color = Utils.GetDecimalColor(Color.Wheat);
    public Vector3 Viewport_Other_Marker_Highlight_Color = Utils.GetDecimalColor(Color.AntiqueWhite);
    public float Viewport_Other_Marker_Alpha = 75.0f;

    public Vector3 Viewport_Point_Light_Highlight_Color = Utils.GetDecimalColor(Color.White);

    public Vector3 Viewport_Spot_Light_Highlight_Color = Utils.GetDecimalColor(Color.Violet);

    public Vector3 Viewport_Directional_Light_Highlight_Color = Utils.GetDecimalColor(Color.AliceBlue);

    public float Viewport_Light_Intensity_Scale = 5.0f;

    // The share of the viewport's sun and environment map ambient kept under the BTL lights when Game mode lights a
    // map with them alone (GparamProfile.MapLightsOnly), so their pools of light show
    public float Viewport_Map_Lights_Only_Base_Scale = 0.2f;

    // The Map Editor's BTL lights: those within this many metres of the camera light the viewports, nearest first,
    // up to the count; with the enable distance on, each also only within its own ViewDistance, as in game.
    // Selected lights always do.
    public bool Viewport_BTL_Lights_Near_Camera = true;
    public float Viewport_BTL_Light_Radius = 150.0f;
    public bool Viewport_BTL_Light_Use_Enable_Distance = true;
    public int Viewport_BTL_Light_Max_Count = 256;

    public Vector3 Viewport_Auto_Invade_Marker_Base_Color = Utils.GetDecimalColor(Color.Red);
    public Vector3 Viewport_Auto_Invade_Marker_Highlight_Color = Utils.GetDecimalColor(Color.DarkRed);
    public Vector3 Viewport_MultiPlayer_Wall_Base_Color = Utils.GetDecimalColor(Color.Gold);
    public Vector3 Viewport_MultiPlayer_Wall_Highlight_Color = Utils.GetDecimalColor(Color.DarkGoldenrod);
    public Vector3 Viewport_Map_Limit_Wall_Base_Color = Utils.GetDecimalColor(Color.OrangeRed);
    public Vector3 Viewport_Map_Limit_Wall_Highlight_Color = Utils.GetDecimalColor(Color.DarkOrange);
    public Vector3 Viewport_Fog_Volume_Base_Color = Utils.GetDecimalColor(Color.Gray);
    public Vector3 Viewport_Fog_Volume_Highlight_Color = Utils.GetDecimalColor(Color.LightGray);

    public Vector3 Viewport_Level_Connector_Marker_Base_Color = Utils.GetDecimalColor(Color.Turquoise);
    public Vector3 Viewport_Level_Connector_Marker_Highlight_Color = Utils.GetDecimalColor(Color.DarkTurquoise);

    public Vector3 Viewport_Gizmo_X_Base_Color = new(0.952f, 0.211f, 0.325f);
    public Vector3 Viewport_Gizmo_X_Highlight_Color = new(1.0f, 0.4f, 0.513f);

    public Vector3 Viewport_Gizmo_Y_Base_Color = new(0.525f, 0.784f, 0.082f);
    public Vector3 Viewport_Gizmo_Y_Highlight_Color = new(0.713f, 0.972f, 0.270f);

    public Vector3 Viewport_Gizmo_Z_Base_Color = new(0.219f, 0.564f, 0.929f);
    public Vector3 Viewport_Gizmo_Z_Highlight_Color = new(0.407f, 0.690f, 1.0f);

    public Vector3 Viewport_Bg_Color = new(0.1f, 0.1f, 0.1f);

    public float Viewport_Flat_Model_Brightness = 1.0f;
    public float Viewport_Flat_Model_Saturation = 0.5f;

    public bool Viewport_Enable_Selection_Outline = false;
    public Vector3 Viewport_Selection_Outline_Color = new(1.0f, 0.5f, 0.0f);
    public Vector3 Viewport_Selection_Tint_Color = new(1.0f, 0.5f, 0.0f);
    public Vector3 Viewport_Untextured_Selection_Tint_Color = new(0.85f, 0.0f, 1.0f);
    public float Viewport_Selection_Tint_Strength = 1.0f;
    public bool Viewport_Enable_Selection_Dithering = true;
    public float Viewport_Selection_Dither_Opacity = 0.9f;

    public bool Viewport_DisplayControls = true;
    public bool Viewport_DisplayRotationIncrement = true;
    public bool Viewport_DisplayTranslationIncrement = true;
    public bool Viewport_DisplayActiveMap = true;

    public bool Viewport_Enable_Box_Selection = true;
    public bool Viewport_Enable_Box_Selection_MapPiece = true;
    public bool Viewport_Enable_Box_Selection_Asset = true;
    public bool Viewport_Enable_Box_Selection_Enemy = true;
    public bool Viewport_Enable_Box_Selection_Player = true;
    public bool Viewport_Enable_Box_Selection_Collision = true;
    public bool Viewport_Enable_Box_Selection_Light = true;
    public bool Viewport_Enable_Box_Selection_Region = true;

    // Compass
    public int CompassSize = 120;
    public int CompassOffset = 64;
    public float CompassNorthOffset = -90f;

    // DisplayCompass
    public bool DisplayCompass = true;

    // DummyMeshSize
    public float DummyMeshSize = 0.05f;

    // NodeMeshSize
    public float NodeMeshSize = 0.05f;

    // CurrentHavokCollisionType
    public HavokCollisionType CurrentHavokCollisionType = HavokCollisionType.Low;
}
