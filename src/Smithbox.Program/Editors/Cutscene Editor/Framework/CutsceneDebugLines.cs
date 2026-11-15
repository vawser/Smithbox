#nullable enable
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.CutsceneEditor;

public enum CutsceneLineLayer
{
    /// <summary>The path the camera takes through the cut</summary>
    CameraPath,

    /// <summary>The camera's frustum at the current frame</summary>
    Camera,

    /// <summary>Actors without a model, and locators (dNNNN), as small axes</summary>
    Markers,

    /// <summary>The cut's placement (offset.sib)</summary>
    Root
}

/// <summary>
/// The Cutscene Editor's wireframes, one line batch per layer, none of them pickable. As in the FX Editor (see
/// FxDebugRenderer), a new set is uploaded hidden and only replaces the drawn one once it is built, so a changing
/// frame doesn't flicker.
/// </summary>
public sealed class CutsceneDebugLines
{
    private List<(DebugPrimitiveRenderableProxy Proxy, Action Release)> Shown = new();
    private List<(DebugPrimitiveRenderableProxy Proxy, Action Release)>? Pending;
    private int PendingShownFrames;

    private const int OverlapFrames = 2;

    public bool IsUpdating => Pending != null;

    public static Color LayerColor(CutsceneLineLayer layer) => layer switch
    {
        CutsceneLineLayer.CameraPath => Color.FromArgb(255, 255, 220, 60),
        CutsceneLineLayer.Camera => Color.FromArgb(255, 255, 140, 40),
        CutsceneLineLayer.Markers => Color.FromArgb(255, 80, 200, 255),
        CutsceneLineLayer.Root => Color.FromArgb(255, 255, 255, 255),
        _ => Color.White
    };

    public void Show(RenderScene scene, Dictionary<CutsceneLineLayer, List<(Vector3 A, Vector3 B)>> lines)
    {
        if (Pending != null)
            Release(Pending);

        var built = new List<(DebugPrimitiveRenderableProxy, Action)>();

        foreach (var (layer, list) in lines)
        {
            if (list.Count == 0)
                continue;

            var color = LayerColor(layer);

            foreach (var chunk in list.Chunk(DbgPrimLineBatch.MaxLines))
            {
                var primitive = new DbgPrimLineBatch(chunk);
                var proxy = new DebugPrimitiveRenderableProxy(scene.OpaqueRenderables, primitive)
                {
                    Pickable = false,
                    BaseColor = color,
                    HighlightedColor = color,
                    // The camera shows through the actors in front of it
                    RenderOverlay = layer is CutsceneLineLayer.Camera,
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

    public static void AddAxes(List<(Vector3, Vector3)> lines, Matrix4x4 world, float size)
    {
        var o = world.Translation;
        lines.Add((o, Vector3.Transform(new Vector3(size, 0, 0), world)));
        lines.Add((o, Vector3.Transform(new Vector3(0, size, 0), world)));
        lines.Add((o, Vector3.Transform(new Vector3(0, 0, size), world)));
    }

    /// <summary>A camera frustum, from the eye along <paramref name="forward" /> out to <paramref name="length" />.</summary>
    public static void AddFrustum(List<(Vector3, Vector3)> lines, Vector3 eye, Vector3 forward, Vector3 up,
        float verticalFovRadians, float aspect, float length)
    {
        forward = Vector3.Normalize(forward);
        var right = Vector3.Normalize(Vector3.Cross(up, forward));
        up = Vector3.Normalize(Vector3.Cross(forward, right));

        var h = MathF.Tan(verticalFovRadians / 2) * length;
        var w = h * aspect;
        var center = eye + forward * length;

        Vector3[] corners =
        {
            center + up * h - right * w,
            center + up * h + right * w,
            center - up * h + right * w,
            center - up * h - right * w
        };

        for (var i = 0; i < 4; i++)
        {
            lines.Add((eye, corners[i]));
            lines.Add((corners[i], corners[(i + 1) % 4]));
        }

        // Which way is up
        lines.Add((center + up * h, center + up * h * 1.4f));
    }
}
