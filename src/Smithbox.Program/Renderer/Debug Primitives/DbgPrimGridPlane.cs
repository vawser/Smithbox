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
