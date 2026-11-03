using System.Drawing;
using System.Numerics;

namespace StudioCore.Renderer;

/// <summary>
/// The unit box a fog volume is drawn on, centered on its origin. FogVolume.frag marches through the box from the
/// far side of each pixel, so both sides are rasterized and the shader keeps only the far one.
/// </summary>
public class DbgPrimFogBox : DbgPrimSolidBox
{
    public override string ShaderName => "FogVolume";

    public DbgPrimFogBox() : base(Transform.Default, new Vector3(-0.5f), new Vector3(0.5f), Color.White)
    {
    }
}
