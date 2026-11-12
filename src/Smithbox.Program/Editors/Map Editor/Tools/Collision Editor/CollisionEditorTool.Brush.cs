using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Keybinds;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Veldrid.Utilities;

namespace StudioCore.Editors.MapEditor;

/// <summary>What the Edit tab's brush does to the vertices under it while the mouse button is held.</summary>
public enum CollisionBrushMode
{
    [Display(Name = "MAP_CollisionEditor_Brush_Mode_Raise")]
    Raise,
    [Display(Name = "MAP_CollisionEditor_Brush_Mode_Lower")]
    Lower,
    [Display(Name = "MAP_CollisionEditor_Brush_Mode_Drag")]
    Drag,
    [Display(Name = "MAP_CollisionEditor_Brush_Mode_Smooth")]
    Smooth
}

/// <summary>
/// The Edit tab's brush: a sphere on the collision under the mouse, which moves every vertex within its radius (more
/// at its centre, fading out to its edge) while the picking button is held. Raise, Lower and Smooth apply again at a
/// fixed interval for as long as the button is held, at wherever the brush is; Drag takes the vertices under the brush
/// when the button goes down and slides them with the mouse at their height.
/// </summary>
public partial class CollisionEditorTool
{
    // The hit a stroke started on, while the button is held; null between strokes
    private Hit Stroke;
    private CollisionBrushMode StrokeMode;

    // When Raise, Lower or Smooth next applies, as a Stopwatch timestamp
    private long NextStrokeApply;

    // A Drag stroke: the vertices it took, the horizontal plane they slide on, where the mouse last met it, and where
    // the cursor is drawn
    private List<MoveSet> DragMoves;
    private float DragHeight;
    private Vector3 DragLast;
    private Vector3 DragAnchor;

    /// <summary>True while a stroke is held, so the viewport's clicks don't select anything.</summary>
    public bool IsStroking => Stroke != null;

    private static readonly (KeybindID Key, CollisionBrushMode Mode)[] BrushKeys =
    {
        (KeybindID.MapEditor_CollisionEditor_Raise, CollisionBrushMode.Raise),
        (KeybindID.MapEditor_CollisionEditor_Lower, CollisionBrushMode.Lower),
        (KeybindID.MapEditor_CollisionEditor_Drag, CollisionBrushMode.Drag),
        (KeybindID.MapEditor_CollisionEditor_Smooth, CollisionBrushMode.Smooth),
    };

    #region Options

    /// <summary>The brush mode, its radius, and the options of the chosen mode.</summary>
    private void DisplayBrushOptions()
    {
        var mode = CFG.Current.MapEditor_CollisionEditor_Brush_Mode;

        // Brush
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CollisionEditor_Brush_Header"), LOC.Get("MAP_CollisionEditor_Brush_Header_TT"));

        ImGui.SetNextItemWidth(-1);
        if (ImGui.BeginCombo("##collisionEditorBrushMode", LOC.Get(mode.GetDisplayName())))
        {
            foreach (var (key, value) in BrushKeys)
            {
                var label = $"{LOC.Get(value.GetDisplayName())} ({InputManager.GetHint(key)})";

                if (ImGui.Selectable($"{label}##collisionEditorBrushMode{value}", value == mode))
                    CFG.Current.MapEditor_CollisionEditor_Brush_Mode = value;
                GUI.Tooltip(LOC.Get($"MAP_CollisionEditor_Brush_Mode_{value}_TT"));
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Brush_Mode_Input_TT"));

        GUI.WrappedText(LOC.Get($"MAP_CollisionEditor_Brush_Mode_{CFG.Current.MapEditor_CollisionEditor_Brush_Mode}_TT"));

        mode = CFG.Current.MapEditor_CollisionEditor_Brush_Mode;

        // Radius
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CollisionEditor_Radius_Header"), LOC.Get("MAP_CollisionEditor_Radius_Header_TT"));

        ImGui.SetNextItemWidth(-1);
        if (ImGui.InputFloat("##collisionEditorBrushRadius", ref CFG.Current.MapEditor_CollisionEditor_Brush_Radius))
            CFG.Current.MapEditor_CollisionEditor_Brush_Radius = Math.Max(0.01f, CFG.Current.MapEditor_CollisionEditor_Brush_Radius);
        GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Radius_Input_TT"));

        if (mode is CollisionBrushMode.Raise or CollisionBrushMode.Lower)
        {
            // Step
            GUI.Spacer();
            GUI.SimpleHeader(LOC.Get("MAP_CollisionEditor_Step_Header"), LOC.Get("MAP_CollisionEditor_Step_Header_TT"));

            ImGui.SetNextItemWidth(-1);
            if (ImGui.InputFloat("##collisionEditorStep", ref CFG.Current.MapEditor_CollisionEditor_Step))
                CFG.Current.MapEditor_CollisionEditor_Step = Math.Max(0.001f, CFG.Current.MapEditor_CollisionEditor_Step);
            GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Step_Input_TT"));
        }

        if (mode is CollisionBrushMode.Smooth)
        {
            // Strength
            GUI.Spacer();
            GUI.SimpleHeader(LOC.Get("MAP_CollisionEditor_Smooth_Strength_Header"), LOC.Get("MAP_CollisionEditor_Smooth_Strength_Header_TT"));

            var percent = CFG.Current.MapEditor_CollisionEditor_Smooth_Strength * 100f;

            ImGui.SetNextItemWidth(-1);
            if (ImGui.SliderFloat("##collisionEditorSmoothStrength", ref percent, 1f, 100f, "%.0f%%"))
                CFG.Current.MapEditor_CollisionEditor_Smooth_Strength = Math.Clamp(percent / 100f, 0.01f, 1f);
            GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Smooth_Strength_Input_TT"));
        }

        if (mode is not CollisionBrushMode.Drag)
        {
            // Interval
            GUI.Spacer();
            GUI.SimpleHeader(LOC.Get("MAP_CollisionEditor_Brush_Interval_Header"), LOC.Get("MAP_CollisionEditor_Brush_Interval_Header_TT"));

            ImGui.SetNextItemWidth(-1);
            if (ImGui.InputFloat("##collisionEditorBrushInterval", ref CFG.Current.MapEditor_CollisionEditor_Brush_Interval))
                CFG.Current.MapEditor_CollisionEditor_Brush_Interval = Math.Max(0.01f, CFG.Current.MapEditor_CollisionEditor_Brush_Interval);
            GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Brush_Interval_Input_TT"));
        }
    }

    /// <summary>The keys that pick the brush mode, between strokes.</summary>
    private void HandleBrushKeys()
    {
        foreach (var (key, mode) in BrushKeys)
        {
            if (InputManager.IsPressed(key))
                CFG.Current.MapEditor_CollisionEditor_Brush_Mode = mode;
        }
    }

    #endregion

    #region Strokes

    private void BeginStroke(Hit hit)
    {
        Stroke = hit;
        StrokeMode = CFG.Current.MapEditor_CollisionEditor_Brush_Mode;

        if (StrokeMode is CollisionBrushMode.Drag)
        {
            // The vertices are taken once, so dragging over other ground doesn't pick it up too
            DragMoves = GetMoves(hit);
            DragHeight = hit.Point.Y;
            DragLast = hit.Point;
            DragAnchor = hit.Point;
            return;
        }

        // Applied on the press, so a click is one step
        ApplyBrush(hit);
        NextStrokeApply = Stopwatch.GetTimestamp() + IntervalTicks();
    }

    private void ContinueStroke(Ray ray)
    {
        if (StrokeMode is CollisionBrushMode.Drag)
        {
            if (!IntersectHeight(ray, DragHeight, out var point))
                return;

            var offset = point - DragLast;
            offset.Y = 0;

            if (offset.LengthSquared() < 1e-10f)
                return;

            foreach (var move in DragMoves)
                MoveWorld(move, offset);

            DragLast = point;
            DragAnchor += offset;
            return;
        }

        var now = Stopwatch.GetTimestamp();

        if (now < NextStrokeApply)
            return;

        // Off the collision, nothing is brushed until the brush is back on it; the stroke goes on
        if (Hover != null)
            ApplyBrush(Hover);

        // From now rather than from the last time, so a slow frame doesn't apply several steps at once
        NextStrokeApply = now + IntervalTicks();
    }

    private void EndStroke()
    {
        Stroke = null;
        DragMoves = null;
        CursorMoves = null;
    }

    private static long IntervalTicks()
    {
        return (long)(Math.Max(0.01f, CFG.Current.MapEditor_CollisionEditor_Brush_Interval) * Stopwatch.Frequency);
    }

    /// <summary>One application of the Raise, Lower or Smooth brush at a hit.</summary>
    private void ApplyBrush(Hit hit)
    {
        var moves = GetMoves(hit);
        var step = CFG.Current.MapEditor_CollisionEditor_Step;

        switch (StrokeMode)
        {
            case CollisionBrushMode.Raise:
                foreach (var move in moves)
                    MoveWorld(move, new Vector3(0, step, 0));
                break;

            case CollisionBrushMode.Lower:
                foreach (var move in moves)
                    MoveWorld(move, new Vector3(0, -step, 0));
                break;

            case CollisionBrushMode.Smooth:
                // Each layer towards a plane of its own, so a linked layer keeps its distance from the edited one
                foreach (var layer in moves.GroupBy(m => m.File.Type))
                    SmoothTowardsPlane(layer.ToList());
                break;
        }

        // The vertices under the brush have moved, so what it reaches may have changed
        CursorMoves = null;
    }

    /// <summary>
    /// Pulls the brushed vertices towards the plane that best fits them (weighted by the falloff), along its normal:
    /// bumps flatten while slopes, ramps and walls keep their angle. Each application moves a vertex the strength's
    /// share of its distance to the plane, scaled by its weight.
    /// </summary>
    private static void SmoothTowardsPlane(List<MoveSet> moves)
    {
        var points = new List<(Vector3 Position, float Weight)>();

        foreach (var move in moves)
        {
            var world = GetBodyWorld(move.File, move.Body);

            foreach (var (v, weight) in move.Falloff)
                points.Add((Vector3.Transform(move.Body.Vertices[v], world), weight));
        }

        if (!FitPlane(points, out var center, out var normal))
            return;

        var strength = Math.Clamp(CFG.Current.MapEditor_CollisionEditor_Smooth_Strength, 0f, 1f);

        foreach (var move in moves)
        {
            var world = GetBodyWorld(move.File, move.Body);

            if (!Matrix4x4.Invert(world, out var inverse))
                continue;

            var offsets = new List<(int, Vector3)>(move.Falloff.Count);

            foreach (var (v, weight) in move.Falloff)
            {
                var distance = Vector3.Dot(Vector3.Transform(move.Body.Vertices[v], world) - center, normal);

                if (MathF.Abs(distance) < 1e-6f)
                    continue;

                offsets.Add((v, Vector3.TransformNormal(-normal * (distance * weight * strength), inverse)));
            }

            move.Body.Offset(offsets);
        }
    }

    /// <summary>
    /// The weighted least-squares plane through points: their weighted centre, and the normal of least spread, from
    /// the axis whose 2x2 minor of the covariance is largest. False for fewer than three points, or points on a line,
    /// which have no plane of their own.
    /// </summary>
    internal static bool FitPlane(List<(Vector3 Position, float Weight)> points, out Vector3 center, out Vector3 normal)
    {
        center = default;
        normal = Vector3.UnitY;

        if (points.Count < 3)
            return false;

        double total = 0, cx = 0, cy = 0, cz = 0;

        foreach (var (p, w) in points)
        {
            total += w;
            cx += p.X * w;
            cy += p.Y * w;
            cz += p.Z * w;
        }

        if (total <= 1e-9)
            return false;

        cx /= total;
        cy /= total;
        cz /= total;

        double xx = 0, xy = 0, xz = 0, yy = 0, yz = 0, zz = 0;

        foreach (var (p, w) in points)
        {
            var x = p.X - cx;
            var y = p.Y - cy;
            var z = p.Z - cz;

            xx += x * x * w;
            xy += x * y * w;
            xz += x * z * w;
            yy += y * y * w;
            yz += y * z * w;
            zz += z * z * w;
        }

        var detX = yy * zz - yz * yz;
        var detY = xx * zz - xz * xz;
        var detZ = xx * yy - xy * xy;
        var max = Math.Max(detX, Math.Max(detY, detZ));
        var trace = xx + yy + zz;

        // Points on a line (or one point) leave every minor near zero
        if (max <= 1e-12 * trace * trace || max <= 0)
            return false;

        double nx, ny, nz;

        if (max == detX)
        {
            nx = detX;
            ny = xz * yz - xy * zz;
            nz = xy * yz - xz * yy;
        }
        else if (max == detY)
        {
            nx = xz * yz - xy * zz;
            ny = detY;
            nz = xy * xz - yz * xx;
        }
        else
        {
            nx = xy * yz - xz * yy;
            ny = xy * xz - yz * xx;
            nz = detZ;
        }

        var length = Math.Sqrt(nx * nx + ny * ny + nz * nz);

        if (length < 1e-12)
            return false;

        center = new Vector3((float)cx, (float)cy, (float)cz);
        normal = new Vector3((float)(nx / length), (float)(ny / length), (float)(nz / length));

        return true;
    }

    #endregion

    #region Reach

    /// <summary>
    /// The vertices a brush at a hit moves: those within the radius of the hit point, in every body of the edited layer
    /// it reaches (so a stroke over a seam between two parts moves both sides), then, with linked layers, the vertices
    /// of the other layer of each of those parts near the point. If no vertex is within the radius, the corner of the
    /// hit triangle nearest the point moves alone, so a small brush still moves something.
    /// </summary>
    private List<MoveSet> GetMoves(Hit hit)
    {
        var radius = Math.Max(0.01f, CFG.Current.MapEditor_CollisionEditor_Brush_Radius);
        var moves = new List<MoveSet>();
        var brushed = new List<CollisionEditorFile>();

        foreach (var file in GetBrushFiles(hit, radius))
        {
            var reached = false;

            foreach (var body in file.Bodies)
            {
                var world = GetBodyWorld(file, body);

                if (!Matrix4x4.Invert(world, out var inverse))
                    continue;

                var scale = GetScale(world);
                var falloff = body.GetBrushFalloff(Vector3.Transform(hit.Point, inverse), radius / scale);

                if (falloff.Count == 0 && file == hit.File && body == hit.Body)
                    falloff.Add((hit.Vertex, 1f));

                if (falloff.Count == 0)
                    continue;

                moves.Add(new MoveSet(file, body, falloff));
                reached = true;
            }

            if (reached)
                brushed.Add(file);
        }

        foreach (var file in brushed)
        {
            var linked = GetLinkedFile(file);

            if (linked == null)
                continue;

            foreach (var body in linked.Bodies)
            {
                var world = GetBodyWorld(linked, body);

                if (!Matrix4x4.Invert(world, out var inverse))
                    continue;

                var scale = GetScale(world);
                var falloff = body.GetLinkedFalloff(Vector3.Transform(hit.Point, inverse),
                    CFG.Current.MapEditor_CollisionEditor_Link_Distance / scale, radius / scale);

                if (falloff.Count > 0)
                    moves.Add(new MoveSet(linked, body, falloff));
            }
        }

        return moves;
    }

    /// <summary>
    /// The files of the edited layer the brush may reach: the hit's, then those of the other visible collision parts
    /// whose drawn mesh is within the radius of the hit point, decoded if they haven't been.
    /// </summary>
    private IEnumerable<CollisionEditorFile> GetBrushFiles(Hit hit, float radius)
    {
        yield return hit.File;

        var layerLetter = GetLayerLetter(hit.File.Type);
        var reach = new Vector3(radius);
        var box = new BoundingBox(hit.Point - reach, hit.Point + reach);

        foreach (var entity in GetCollisionParts())
        {
            if (!entity.EditorVisible)
                continue;

            foreach (var proxy in GetCollisionProxies(entity))
            {
                var fileName = ResolveFileName(entity.MapID, proxy.ResourceHandle?.Name);

                if (fileName == null || Unsupported.Contains(fileName) || char.ToLowerInvariant(fileName[0]) != layerLetter)
                    continue;

                if (string.Equals(fileName, hit.File.FileName, StringComparison.OrdinalIgnoreCase))
                    continue;

                // An edited file's drawn mesh is hidden behind its preview, and may no longer be where it was
                if (!Files.TryGetValue(fileName, out var file))
                {
                    if (!proxy.GetBounds().Intersects(box))
                        continue;

                    file = DecodeFile(entity, fileName);

                    if (file == null)
                        continue;
                }

                // A file is edited through the part it was first found on
                if (file.Entity == entity)
                    yield return file;
            }
        }
    }

    /// <summary>How much a placement scales distances, from its X axis (parts are scaled evenly).</summary>
    private static float GetScale(Matrix4x4 world)
    {
        var scale = Vector3.TransformNormal(Vector3.UnitX, world).Length();
        return scale > 1e-6f ? scale : 1f;
    }

    #endregion
}
