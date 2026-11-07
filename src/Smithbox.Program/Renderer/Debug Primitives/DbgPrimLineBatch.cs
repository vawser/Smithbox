using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using Veldrid.Utilities;

namespace StudioCore.Renderer;

/// <summary>
/// A batch of lines in a single color, given in world space. Indices are 16 bit, so a batch holds at most
/// <see cref="MaxLines"/> lines. Used by the Map Editor's navigation overlay.
/// </summary>
public class DbgPrimLineBatch : DbgPrimWire
{
    public const int MaxLines = 16000;

    private readonly BoundingBox _bounds;

    // Set once the batch is no longer displayed, so a pending upload doesn't allocate a buffer nothing frees
    private volatile bool _released;

    public override BoundingBox Bounds => _bounds;

    public override BoundingBox FramingBounds => _bounds;

    public DbgPrimLineBatch(IReadOnlyList<(Vector3 A, Vector3 B)> lines)
    {
        var min = new Vector3(float.MaxValue);
        var max = new Vector3(float.MinValue);

        ReserveGeometry(lines.Count * 2, lines.Count * 2);

        foreach (var (a, b) in lines)
        {
            // The color comes from the proxy's material, so the vertex color isn't used
            AddLine(a, b, Color.White);

            min = Vector3.Min(min, Vector3.Min(a, b));
            max = Vector3.Max(max, Vector3.Max(a, b));
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
