using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using Veldrid.Utilities;

namespace StudioCore.Renderer;

/// <summary>
/// A batch of triangles in a single color, used to display the cells of WALL files.
/// Indices are 16 bit, so a batch holds at most <see cref="MaxTriangles"/> triangles.
/// </summary>
public class DbgPrimWallCells : DbgPrimSolid
{
    public const int MaxTriangles = 10000;

    private readonly BoundingBox _bounds;

    // Set once the batch is no longer displayed, so a pending upload doesn't allocate a buffer nothing frees
    private volatile bool _released;

    public override BoundingBox Bounds => _bounds;

    public override BoundingBox FramingBounds => _bounds;

    public DbgPrimWallCells(IReadOnlyList<(Vector3 A, Vector3 B, Vector3 C)> triangles)
    {
        var min = new Vector3(float.MaxValue);
        var max = new Vector3(float.MinValue);

        ReserveGeometry(triangles.Count * 3, triangles.Count * 3);

        foreach (var (a, b, c) in triangles)
        {
            // The color comes from the proxy's material, so the vertex color isn't used
            AddTri(a, b, c, Color.White);

            min = Vector3.Min(min, Vector3.Min(a, Vector3.Min(b, c)));
            max = Vector3.Max(max, Vector3.Max(a, Vector3.Max(b, c)));
        }

        _bounds = new BoundingBox(min, max);

        SceneRenderer.AddBackgroundUploadTask((d, cl) =>
        {
            if (!_released)
            {
                UpdatePerFrameResources(d, cl, null);
            }
        });
    }

    /// <summary>
    /// Frees the geometry of the batch. Done once the proxy displaying it has been disposed.
    /// </summary>
    public void Release()
    {
        _released = true;
        DestroyDeviceObjects();
    }
}
