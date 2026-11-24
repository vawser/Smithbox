using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.MapEditor;
using StudioCore.Keybinds;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using Veldrid.Utilities;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
/// The Edit tab: the Collision Editor's vertex brush over the model's collision. A sphere sits where the mouse ray
/// meets the layer; holding the picking button raises, lowers, drags or smooths the vertices within it. Its options
/// are the Map Editor's, so the brush feels the same in both editors.
/// </summary>
public partial class ModelCollisionTool
{
    private bool EditActive;

    private Hit Hover;

    // The hit a stroke started on, while the button is held
    private Hit Stroke;
    private CollisionBrushMode StrokeMode;
    private long NextStrokeApply;

    // A Drag stroke: the vertices it took, the horizontal plane they slide on, where the mouse last met it, and where
    // the cursor is drawn
    private List<(CollisionEditorBody Body, List<(int Vertex, float Weight)> Falloff)> DragMoves;
    private float DragHeight;
    private Vector3 DragLast;
    private Vector3 DragAnchor;

    // The brush sphere and the vertex marker, as the Map Editor's are drawn
    private DebugPrimitiveRenderableProxy CursorWire;
    private DebugPrimitiveRenderableProxy CursorFill;
    private DebugPrimitiveRenderableProxy VertexMesh;

    private const float SphereRadius = 0.1f;
    private const float MaxVertexMarkerSize = 0.1f;

    private static readonly (KeybindID Key, CollisionBrushMode Mode)[] BrushKeys =
    {
        (KeybindID.MapEditor_CollisionEditor_Raise, CollisionBrushMode.Raise),
        (KeybindID.MapEditor_CollisionEditor_Lower, CollisionBrushMode.Lower),
        (KeybindID.MapEditor_CollisionEditor_Drag, CollisionBrushMode.Drag),
        (KeybindID.MapEditor_CollisionEditor_Smooth, CollisionBrushMode.Smooth),
    };

    private bool IsDragStroke => Stroke != null && StrokeMode is CollisionBrushMode.Drag;

    #region Tab

    private void DisplayEditTab()
    {
        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CollisionEditor_Actions_Header"), LOC.Get("MAP_CollisionEditor_Actions_Header_TT"));

        GUI.MultiButtonInput("modelCollisionEditToggle", "modelCollisionEditToggleButton",
            EditActive ? LOC.Get("MAP_CollisionEditor_Stop") : LOC.Get("MAP_CollisionEditor_Start"),
            EditActive ? LOC.Get("MAP_CollisionEditor_Stop_TT") : LOC.Get("MAP_CollisionEditor_Start_TT"),
            ToggleEditing);

        // Cursor
        GUI.Spacer();
        GUI.ConditionalHeader(LOC.Get("MAP_CollisionEditor_Cursor_Header"), LOC.Get("MAP_CollisionEditor_Cursor_Header_TT"),
            ref CFG.Current.MapEditor.MapEditor_CollisionEditor_Show_Cursor);

        if (CFG.Current.MapEditor.MapEditor_CollisionEditor_Show_Cursor)
        {
            if (!View.RenderScene.DrawFilter.HasFlag(RenderFilter.Collision))
                GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Collision_Hidden"));
            else if (!EditActive)
                GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Cursor_Inactive"));
            else if (((IsDragStroke ? Stroke : null) ?? Hover) is { } hit)
            {
                var position = IsDragStroke ? DragAnchor : hit.Point;
                var moves = IsDragStroke && DragMoves != null ? DragMoves : GetMoves(hit);

                GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Cursor_Point", $"{position.X:0.###}, {position.Y:0.###}, {position.Z:0.###}"));
                GUI.WrappedText(LOC.Get("MODEL_CollisionTool_Cursor_Moves", moves.Sum(m => m.Falloff.Count),
                    $"{CFG.Current.MapEditor.MapEditor_CollisionEditor_Brush_Radius:0.###}"));
            }
            else
                GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Cursor_None"));
        }

        // Controls
        GUI.Spacer();
        GUI.ConditionalHeader(LOC.Get("MAP_CollisionEditor_Controls_Header"), LOC.Get("MAP_CollisionEditor_Controls_Header_TT"),
            ref CFG.Current.MapEditor.MapEditor_CollisionEditor_Show_Controls);

        if (CFG.Current.MapEditor.MapEditor_CollisionEditor_Show_Controls)
        {
            GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Control_Brush", InputManager.GetMouseHint(MousebindID.Viewport_Picking_Action)));
            GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Control_Modes",
                InputManager.GetHint(KeybindID.MapEditor_CollisionEditor_Raise),
                InputManager.GetHint(KeybindID.MapEditor_CollisionEditor_Lower),
                InputManager.GetHint(KeybindID.MapEditor_CollisionEditor_Drag),
                InputManager.GetHint(KeybindID.MapEditor_CollisionEditor_Smooth)));
        }

        DisplayBrushOptions();
        DisplayPendingList();
    }

    /// <summary>The brush mode, its radius, and the options of the chosen mode: the Map Editor's.</summary>
    private void DisplayBrushOptions()
    {
        var mode = CFG.Current.MapEditor.MapEditor_CollisionEditor_Brush_Mode;

        // Brush
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CollisionEditor_Brush_Header"), LOC.Get("MAP_CollisionEditor_Brush_Header_TT"));

        ImGui.SetNextItemWidth(GUI.HelpButtonItemWidth());
        if (ImGui.BeginCombo("##modelCollisionBrushMode", LOC.Get(mode.GetDisplayName())))
        {
            foreach (var (key, value) in BrushKeys)
            {
                var label = $"{LOC.Get(value.GetDisplayName())} ({InputManager.GetHint(key)})";

                if (ImGui.Selectable($"{label}##modelCollisionBrushMode{value}", value == mode))
                    CFG.Current.MapEditor.MapEditor_CollisionEditor_Brush_Mode = value;
                GUI.Tooltip(LOC.Get($"MAP_CollisionEditor_Brush_Mode_{value}_TT"));
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Brush_Mode_Input_TT"));

        mode = CFG.Current.MapEditor.MapEditor_CollisionEditor_Brush_Mode;
        GUI.HelpButton("modelCollisionBrushModeHelp", LOC.Get(mode.GetDisplayName()), LOC.Get($"MAP_CollisionEditor_Brush_Mode_{mode}_TT"));

        // Radius
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CollisionEditor_Radius_Header"), LOC.Get("MAP_CollisionEditor_Radius_Header_TT"));

        ImGui.SetNextItemWidth(-1);
        if (ImGui.InputFloat("##modelCollisionBrushRadius", ref CFG.Current.MapEditor.MapEditor_CollisionEditor_Brush_Radius))
            CFG.Current.MapEditor.MapEditor_CollisionEditor_Brush_Radius = Math.Max(0.01f, CFG.Current.MapEditor.MapEditor_CollisionEditor_Brush_Radius);
        GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Radius_Input_TT"));

        if (mode is CollisionBrushMode.Raise or CollisionBrushMode.Lower)
        {
            GUI.Spacer();
            GUI.SimpleHeader(LOC.Get("MAP_CollisionEditor_Step_Header"), LOC.Get("MAP_CollisionEditor_Step_Header_TT"));

            ImGui.SetNextItemWidth(-1);
            if (ImGui.InputFloat("##modelCollisionStep", ref CFG.Current.MapEditor.MapEditor_CollisionEditor_Step))
                CFG.Current.MapEditor.MapEditor_CollisionEditor_Step = Math.Max(0.001f, CFG.Current.MapEditor.MapEditor_CollisionEditor_Step);
            GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Step_Input_TT"));
        }

        if (mode is CollisionBrushMode.Smooth)
        {
            GUI.Spacer();
            GUI.SimpleHeader(LOC.Get("MAP_CollisionEditor_Smooth_Strength_Header"), LOC.Get("MAP_CollisionEditor_Smooth_Strength_Header_TT"));

            var percent = CFG.Current.MapEditor.MapEditor_CollisionEditor_Smooth_Strength * 100f;

            ImGui.SetNextItemWidth(-1);
            if (ImGui.SliderFloat("##modelCollisionSmoothStrength", ref percent, 1f, 100f, "%.0f%%"))
                CFG.Current.MapEditor.MapEditor_CollisionEditor_Smooth_Strength = Math.Clamp(percent / 100f, 0.01f, 1f);
            GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Smooth_Strength_Input_TT"));
        }

        if (mode is not CollisionBrushMode.Drag)
        {
            GUI.Spacer();
            GUI.SimpleHeader(LOC.Get("MAP_CollisionEditor_Brush_Interval_Header"), LOC.Get("MAP_CollisionEditor_Brush_Interval_Header_TT"));

            ImGui.SetNextItemWidth(-1);
            if (ImGui.InputFloat("##modelCollisionBrushInterval", ref CFG.Current.MapEditor.MapEditor_CollisionEditor_Brush_Interval))
                CFG.Current.MapEditor.MapEditor_CollisionEditor_Brush_Interval = Math.Max(0.01f, CFG.Current.MapEditor.MapEditor_CollisionEditor_Brush_Interval);
            GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Brush_Interval_Input_TT"));
        }
    }

    #endregion

    #region Brush

    private void ToggleEditing()
    {
        EditActive = !EditActive;

        if (EditActive)
        {
            if (MaterialActive)
                ToggleMaterialPainting();

            return;
        }

        EndStroke();
        Hover = null;
        DisposeCursor();
    }

    private void HandleEditing(RenderScene scene, DecodedLayer layer, Ray ray, bool canInteract)
    {
        // Typed text shouldn't change the brush, and the right and middle buttons fly the camera
        var typing = ImGui.GetIO().WantTextInput;
        var flying = InputManager.IsMouseDown(Veldrid.MouseButton.Right) || InputManager.IsMouseDown(Veldrid.MouseButton.Middle);

        var keysFree = canInteract && !typing && !flying && !InputManager.IsMouseDown(Veldrid.MouseButton.Left);
        var brushFree = canInteract && !typing && !flying;
        var collisionShown = scene.DrawFilter.HasFlag(RenderFilter.Collision);

        if (keysFree && Stroke == null)
        {
            foreach (var (key, mode) in BrushKeys)
            {
                if (InputManager.IsPressed(key))
                    CFG.Current.MapEditor.MapEditor_CollisionEditor_Brush_Mode = mode;
            }
        }

        if (Stroke != null && (!brushFree || !collisionShown || !InputManager.IsMouseDown(MousebindID.Viewport_Picking_Action)))
            EndStroke();

        if (!IsDragStroke)
            Hover = canInteract && collisionShown ? RayCast(layer, ray) : null;

        if (Stroke != null)
        {
            ContinueStroke(ray);
        }
        else if (brushFree && Hover != null && InputManager.IsMousePressed(MousebindID.Viewport_Picking_Action))
        {
            BeginStroke(Hover);
        }

        UpdateCursor(scene);
    }

    private void BeginStroke(Hit hit)
    {
        Stroke = hit;
        StrokeMode = CFG.Current.MapEditor.MapEditor_CollisionEditor_Brush_Mode;

        if (StrokeMode is CollisionBrushMode.Drag)
        {
            // The vertices are taken once, so dragging over other ground doesn't pick it up too
            DragMoves = GetMoves(hit);
            DragHeight = hit.Point.Y;
            DragLast = hit.Point;
            DragAnchor = hit.Point;
            return;
        }

        ApplyBrush(hit);
        NextStrokeApply = Stopwatch.GetTimestamp() + IntervalTicks();
    }

    private void ContinueStroke(Ray ray)
    {
        if (StrokeMode is CollisionBrushMode.Drag)
        {
            if (MathF.Abs(ray.Direction.Y) < 1e-6f)
                return;

            var t = (DragHeight - ray.Origin.Y) / ray.Direction.Y;

            if (t < 0)
                return;

            var offset = ray.Origin + ray.Direction * t - DragLast;
            offset.Y = 0;

            if (offset.LengthSquared() < 1e-10f)
                return;

            foreach (var move in DragMoves)
                MoveWorld(move.Body, move.Falloff, offset);

            DragLast += offset;
            DragAnchor += offset;
            return;
        }

        var now = Stopwatch.GetTimestamp();

        if (now < NextStrokeApply)
            return;

        if (Hover != null)
            ApplyBrush(Hover);

        NextStrokeApply = now + IntervalTicks();
    }

    private void EndStroke()
    {
        Stroke = null;
        DragMoves = null;
    }

    private static long IntervalTicks()
    {
        return (long)(Math.Max(0.01f, CFG.Current.MapEditor.MapEditor_CollisionEditor_Brush_Interval) * Stopwatch.Frequency);
    }

    private void ApplyBrush(Hit hit)
    {
        var moves = GetMoves(hit);
        var step = CFG.Current.MapEditor.MapEditor_CollisionEditor_Step;

        switch (StrokeMode)
        {
            case CollisionBrushMode.Raise:
                foreach (var move in moves)
                    MoveWorld(move.Body, move.Falloff, new Vector3(0, step, 0));
                break;

            case CollisionBrushMode.Lower:
                foreach (var move in moves)
                    MoveWorld(move.Body, move.Falloff, new Vector3(0, -step, 0));
                break;

            case CollisionBrushMode.Smooth:
                SmoothTowardsPlane(moves);
                break;
        }
    }

    /// <summary>
    /// The vertices a brush at a hit moves: those within the radius in every body of the layer, or the hit triangle's
    /// nearest corner alone if none is, so a small brush still moves something.
    /// </summary>
    private List<(CollisionEditorBody Body, List<(int Vertex, float Weight)> Falloff)> GetMoves(Hit hit)
    {
        var radius = Math.Max(0.01f, CFG.Current.MapEditor.MapEditor_CollisionEditor_Brush_Radius);
        var moves = new List<(CollisionEditorBody, List<(int, float)>)>();
        var layer = GetLayer();

        if (layer == null)
            return moves;

        var world = GetWorld();

        foreach (var body in layer.Bodies)
        {
            var bodyWorld = GetBodyWorld(body, world);

            if (!Matrix4x4.Invert(bodyWorld, out var inverse))
                continue;

            var scale = GetScale(bodyWorld);
            var falloff = body.GetBrushFalloff(Vector3.Transform(hit.Point, inverse), radius / scale);

            if (falloff.Count == 0 && body == hit.Body)
                falloff.Add((hit.Vertex, 1f));

            if (falloff.Count > 0)
                moves.Add((body, falloff));
        }

        return moves;
    }

    private void MoveWorld(CollisionEditorBody body, List<(int Vertex, float Weight)> falloff, Vector3 offset)
    {
        if (!Matrix4x4.Invert(GetBodyWorld(body, GetWorld()), out var inverse))
            return;

        body.Move(falloff, Vector3.TransformNormal(offset, inverse));
    }

    /// <summary>Pulls the brushed vertices towards the plane that best fits them, as the Map Editor's Smooth does.</summary>
    private void SmoothTowardsPlane(List<(CollisionEditorBody Body, List<(int Vertex, float Weight)> Falloff)> moves)
    {
        var world = GetWorld();
        var points = new List<(Vector3 Position, float Weight)>();

        foreach (var (body, falloff) in moves)
        {
            var bodyWorld = GetBodyWorld(body, world);

            foreach (var (v, weight) in falloff)
                points.Add((Vector3.Transform(body.Vertices[v], bodyWorld), weight));
        }

        if (!CollisionEditorTool.FitPlane(points, out var center, out var normal))
            return;

        var strength = Math.Clamp(CFG.Current.MapEditor.MapEditor_CollisionEditor_Smooth_Strength, 0f, 1f);

        foreach (var (body, falloff) in moves)
        {
            var bodyWorld = GetBodyWorld(body, world);

            if (!Matrix4x4.Invert(bodyWorld, out var inverse))
                continue;

            var offsets = new List<(int, Vector3)>(falloff.Count);

            foreach (var (v, weight) in falloff)
            {
                var distance = Vector3.Dot(Vector3.Transform(body.Vertices[v], bodyWorld) - center, normal);

                if (MathF.Abs(distance) < 1e-6f)
                    continue;

                offsets.Add((v, Vector3.TransformNormal(-normal * (distance * weight * strength), inverse)));
            }

            body.Offset(offsets);
        }
    }

    private static float GetScale(Matrix4x4 world)
    {
        var scale = Vector3.TransformNormal(Vector3.UnitX, world).Length();
        return scale > 1e-6f ? scale : 1f;
    }

    #endregion

    #region Cursor

    /// <summary>The brush sphere on the hit, at the brush's radius, and a marker on the hit triangle's nearest corner.</summary>
    private void UpdateCursor(RenderScene scene)
    {
        var target = (IsDragStroke ? Stroke : null) ?? Hover;

        if (target == null)
        {
            SetVisible(CursorWire, false);
            SetVisible(CursorFill, false);
            SetVisible(VertexMesh, false);
            return;
        }

        var cursorColor = ToColor(CFG.Current.MapEditor.MapEditor_CollisionEditor_Cursor_Color);

        CursorWire ??= RenderableHelper.GetDetectionSphereProxy(scene, cursorColor);
        CursorFill ??= RenderableHelper.GetDetectionSphereFillProxy(scene, cursorColor);

        var fill = ColorHelper.GetTransparencyColor(cursorColor, CFG.Current.MapEditor.MapEditor_CollisionEditor_Cursor_Fill_Alpha);

        if (CursorFill.BaseColor != fill)
        {
            CursorFill.BaseColor = fill;
            CursorFill.HighlightedColor = fill;
        }

        VertexMesh ??= new DebugPrimitiveRenderableProxy(scene.OpaqueRenderables, RenderableHelper._placementOrb)
        {
            Pickable = false,
            RenderOverlay = true,
            BaseColor = ToColor(CFG.Current.MapEditor.MapEditor_CollisionEditor_Vertex_Color),
            HighlightedColor = ToColor(CFG.Current.MapEditor.MapEditor_CollisionEditor_Vertex_Color)
        };

        var size = Math.Max(0.01f, CFG.Current.MapEditor.MapEditor_CollisionEditor_Brush_Radius);
        var cursorWorld = Matrix4x4.CreateScale(size) * Matrix4x4.CreateTranslation(IsDragStroke ? DragAnchor : target.Point);

        var vertex = Vector3.Transform(target.Body.Vertices[target.Vertex], GetBodyWorld(target.Body, GetWorld()));
        var markerSize = MathF.Min(size * 0.6f, MaxVertexMarkerSize);

        CursorWire.World = cursorWorld;
        CursorFill.World = cursorWorld;
        VertexMesh.World = Matrix4x4.CreateScale(markerSize / SphereRadius) * Matrix4x4.CreateTranslation(vertex);

        SetVisible(CursorWire, true);
        SetVisible(CursorFill, true);
        SetVisible(VertexMesh, true);
    }

    private static Color ToColor(Vector3 color) => Color.FromArgb(255, (int)color.X, (int)color.Y, (int)color.Z);

    private static void SetVisible(DebugPrimitiveRenderableProxy proxy, bool visible)
    {
        if (proxy != null && proxy.Visible != visible)
            proxy.Visible = visible;
    }

    private void DisposeCursor()
    {
        CursorWire?.Dispose();
        CursorFill?.Dispose();
        VertexMesh?.Dispose();

        CursorWire = null;
        CursorFill = null;
        VertexMesh = null;
    }

    #endregion
}
