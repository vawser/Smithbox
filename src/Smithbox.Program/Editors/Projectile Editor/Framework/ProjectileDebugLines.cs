#nullable enable
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.ProjectileEditor;

/// <summary>
/// A set of wireframe lines in a render scene, one line batch per colour, none of them pickable. As in the Cutscene
/// Editor (see CutsceneDebugLines), a new set is uploaded hidden and only replaces the drawn one once it is built, so
/// lines that change every frame don't flicker.
/// </summary>
public sealed class ProjectileDebugLines
{
    private List<(DebugPrimitiveRenderableProxy Proxy, Action Release)> Shown = new();
    private List<(DebugPrimitiveRenderableProxy Proxy, Action Release)>? Pending;
    private int PendingShownFrames;

    private const int OverlapFrames = 2;

    /// <summary>Lines drawn through whatever is in front of them</summary>
    public bool Overlay;

    public bool IsUpdating => Pending != null;

    public void Show(RenderScene scene, Dictionary<Color, List<(Vector3 A, Vector3 B)>> lines)
    {
        if (Pending != null)
            Release(Pending);

        var built = new List<(DebugPrimitiveRenderableProxy, Action)>();

        foreach (var (color, list) in lines)
        {
            if (list.Count == 0)
                continue;

            foreach (var chunk in list.Chunk(DbgPrimLineBatch.MaxLines))
            {
                var primitive = new DbgPrimLineBatch(chunk);
                var proxy = new DebugPrimitiveRenderableProxy(scene.OpaqueRenderables, primitive)
                {
                    Pickable = false,
                    BaseColor = color,
                    HighlightedColor = color,
                    RenderOverlay = Overlay,
                    Visible = false,
                };
                built.Add((proxy, primitive.Release));
            }
        }

        Pending = built;
        PendingShownFrames = 0;
    }

    /// <summary>Called every frame: lets a built set take over from the drawn one.</summary>
    public void Update()
    {
        if (Pending == null)
            return;

        if (PendingShownFrames >= OverlapFrames || Pending.Count == 0)
        {
            Release(Shown);
            Shown = Pending;
            Pending = null;
            PendingShownFrames = 0;
        }
        else if (PendingShownFrames > 0 || Pending.All(p => p.Proxy.IsRenderableConstructed))
        {
            foreach (var (proxy, _) in Pending)
            {
                if (!proxy.Visible)
                    proxy.Visible = true;
            }

            PendingShownFrames++;
        }
    }

    public void Clear()
    {
        Release(Shown);
        if (Pending != null)
            Release(Pending);
        Pending = null;
        PendingShownFrames = 0;
    }

    private static void Release(List<(DebugPrimitiveRenderableProxy Proxy, Action Release)> parts)
    {
        foreach (var (proxy, release) in parts)
        {
            proxy.Dispose();
            release();
        }

        parts.Clear();
    }

    // ---- shapes

    public static Color ToColor(Vector4 color) =>
        Color.FromArgb((int)(Math.Clamp(color.W, 0, 1) * 255), (int)(Math.Clamp(color.X, 0, 1) * 255),
            (int)(Math.Clamp(color.Y, 0, 1) * 255), (int)(Math.Clamp(color.Z, 0, 1) * 255));

    /// <summary>A sphere as three circles, one around each axis</summary>
    public static void AddSphere(List<(Vector3, Vector3)> lines, Vector3 center, float radius, int segments = 16)
    {
        AddCircle(lines, center, Vector3.UnitX, Vector3.UnitY, radius, segments);
        AddCircle(lines, center, Vector3.UnitY, Vector3.UnitZ, radius, segments);
        AddCircle(lines, center, Vector3.UnitZ, Vector3.UnitX, radius, segments);
    }

    /// <summary>A circle in the plane of two axes</summary>
    public static void AddCircle(List<(Vector3, Vector3)> lines, Vector3 center, Vector3 axisA, Vector3 axisB, float radius, int segments)
    {
        var previous = center + axisA * radius;

        for (var i = 1; i <= segments; i++)
        {
            var angle = MathF.Tau * i / segments;
            var next = center + (axisA * MathF.Cos(angle) + axisB * MathF.Sin(angle)) * radius;

            lines.Add((previous, next));
            previous = next;
        }
    }

    /// <summary>A box's twelve edges</summary>
    public static void AddBox(List<(Vector3, Vector3)> lines, Vector3 min, Vector3 max)
    {
        Vector3 c(int x, int y, int z) => new(x == 0 ? min.X : max.X, y == 0 ? min.Y : max.Y, z == 0 ? min.Z : max.Z);

        for (var i = 0; i < 2; i++)
        for (var j = 0; j < 2; j++)
        {
            lines.Add((c(0, i, j), c(1, i, j)));
            lines.Add((c(i, 0, j), c(i, 1, j)));
            lines.Add((c(i, j, 0), c(i, j, 1)));
        }
    }
}
