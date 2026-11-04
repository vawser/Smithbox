using System.Drawing;
using System.Numerics;
using Veldrid.Utilities;

namespace StudioCore.Renderer;

/// <summary>
/// A flat quad from -1 to 1 on the XZ plane, whose lines are drawn by the ViewportGrid shader.
/// Scale it with the world matrix and set the number of squares with <see cref="DebugPrimitiveRenderableProxy.GridCells" />.
/// </summary>
public class DbgPrimGridPlane : DbgPrimSolid
{
    public override BoundingBox Bounds => new(new Vector3(-1, 0, -1), new Vector3(1, 0, 1));

    public override string ShaderName => "ViewportGrid";

    // A grid placed on a floor would flicker with it. Same bias as mesh decals, see MeshRenderableProxy:
    // about a millimetre at ten metres, with the slope keeping it in front at glancing angles.
    public override float DepthBiasConstant => 1024f;
    public override float DepthBiasSlope => 2f;

    public DbgPrimGridPlane()
    {
        AddQuad(
            new Vector3(-1, 0, -1),
            new Vector3(1, 0, -1),
            new Vector3(1, 0, 1),
            new Vector3(-1, 0, 1),
            Color.White);

        SceneRenderer.AddBackgroundUploadTask((d, cl) =>
        {
            UpdatePerFrameResources(d, cl, null);
        });
    }
}
