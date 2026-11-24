using StudioCore.Utilities;
using System.Drawing;
using System.Numerics;

namespace StudioCore.Application;

/// <summary>
///     The window and renderer settings (<c>CFG.Current.Graphics</c>). Saved in Configuration.json under the same
///     GFX_ keys as before they moved here.
/// </summary>
public sealed class GraphicsSettings : ICfgSection
{
    // The main window's placement, kept from the last session
    public int GFX_Display_Width { get; set; } = 1920;
    public int GFX_Display_Height { get; set; } = 1057;
    public int GFX_Display_X { get; set; } = 0;
    public int GFX_Display_Y { get; set; } = 23;

    public float GFX_Renderable_Default_Wireframe_Alpha = 100.0f;

    public Vector3 GFX_Renderable_LightProbeSphere_BaseColor = Utils.GetDecimalColor(Color.Yellow);
    public Vector3 GFX_Renderable_LightProbeSphere_HighlightColor = Utils.GetDecimalColor(Color.YellowGreen);
}
