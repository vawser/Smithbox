using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;
using StudioCore.Keybinds;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using Veldrid.Utilities;

namespace StudioCore.Editors.MapEditor;

/// <summary>A point of the ruler.</summary>
public enum RulerPoint
{
    None,
    Start,
    End
}

/// <summary>
/// Measures the distance between two points in the Map Editor's viewport. The points are placed by clicking on what
/// the viewport draws (the click-picking pass gives the depth of the surface under the cursor), typed in, or taken from
/// the selection, and are moved with the gizmo once picked. Each is drawn as a small wire sphere, with a line between
/// them and the distance in metres beside it. The ruler isn't map data: nothing is saved and moves aren't undoable.
/// </summary>
public class MapRulerTool
{
    private readonly MapEditorView View;
    private readonly ProjectEntry Project;

    public Vector3? Start { get; private set; }
    public Vector3? End { get; private set; }

    /// <summary>The point the next viewport click places, if any.</summary>
    public RulerPoint Placing { get; private set; }

    /// <summary>The point the gizmo moves, if any.</summary>
    public RulerPoint Selected { get; private set; }

    /// <summary>True while placing both points in turn, so placing the start goes on to the end.</summary>
    private bool PlacingBoth;

    // A click waiting for its picking pass: which point it places, where it was and in which viewport
    private bool AwaitingPick;
    private RulerPoint AwaitingPoint;
    private Vector2 AwaitingScreen;
    private VulkanViewport AwaitingViewport;

    // Clicks taken by the ruler are held until the button is released, so the release doesn't select
    private bool SwallowUntilRelease;

    private string Status = "";

    public readonly RulerGizmoTarget GizmoTarget;

    // Drawing: a unit sphere and a unit line, shared by the proxies and placed by their world matrices, so moving a
    // point never rebuilds geometry
    private RenderScene Scene;
    private DbgPrimWireSphere SpherePrim;
    private DbgPrimLineBatch LinePrim;
    private DebugPrimitiveRenderableProxy StartProxy;
    private DebugPrimitiveRenderableProxy EndProxy;
    private DebugPrimitiveRenderableProxy LineProxy;

    public MapRulerTool(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
        GizmoTarget = new RulerGizmoTarget(this);
    }

    public float? Distance => Start is Vector3 a && End is Vector3 b ? Vector3.Distance(a, b) : null;

    public Vector3? GetPoint(RulerPoint point) => point switch
    {
        RulerPoint.Start => Start,
        RulerPoint.End => End,
        _ => null
    };

    public void SetPoint(RulerPoint point, Vector3 position)
    {
        if (point == RulerPoint.Start)
            Start = position;
        else if (point == RulerPoint.End)
            End = position;

        UpdateDrawing();
    }

    private bool IsEnabled => CFG.Current.Interface_MapEditor_Tool_Ruler;

    #region Viewport

    /// <summary>
    /// Places and picks the ruler's points, and keeps its drawing in the viewport's scene. Called each frame by the
    /// active viewport. Returns true while the viewport's clicks are taken, so they don't select.
    /// </summary>
    public bool HandleViewportInput(VulkanViewport viewport, Ray ray, bool canPick)
    {
        if (!IsEnabled)
        {
            Placing = RulerPoint.None;
            Selected = RulerPoint.None;
            AwaitingPick = false;
            ReleaseDrawing();
            return false;
        }

        if (viewport.RenderScene != Scene)
        {
            ReleaseDrawing();
            Scene = viewport.RenderScene;
            UpdateDrawing();
        }

        // A placing click's result, from the viewport it was made in
        if (AwaitingPick)
        {
            var pipeline = AwaitingViewport.ViewPipeline;

            if (pipeline.PickingResultsReady)
            {
                pipeline.PickingResultsReady = false;
                AwaitingPick = false;
                FinishPlacing(AwaitingViewport, pipeline.PickingDepth);
            }
            else if (!pipeline.IsPickingPending)
            {
                // The request was taken by something else
                AwaitingPick = false;
            }
        }

        bool taken = false;

        if (SwallowUntilRelease)
        {
            if (!InputManager.IsMouseDown(MousebindID.Viewport_Picking_Action) || InputManager.IsMouseReleased(MousebindID.Viewport_Picking_Action))
                SwallowUntilRelease = false;

            taken = true;
        }

        bool pressed = canPick && InputManager.IsMousePressed(MousebindID.Viewport_Picking_Action);

        if (Placing != RulerPoint.None)
        {
            if (pressed && !AwaitingPick && !viewport.ViewPipeline.IsPickingPending)
            {
                var mouse = InputManager.MousePosition;

                // The centre of the pixel the picking pass tests
                AwaitingScreen = new Vector2(MathF.Floor(mouse.X - viewport.X) + 0.5f, MathF.Floor(mouse.Y - viewport.Y) + 0.5f);
                AwaitingPoint = Placing;
                AwaitingViewport = viewport;
                AwaitingPick = true;

                viewport.ViewPipeline.PickingResultsReady = false;
                viewport.ViewPipeline.CreateAsyncPickingRequest();

                SwallowUntilRelease = true;
            }

            return true;
        }

        if (AwaitingPick)
            return true;

        if (pressed)
        {
            var hit = HitPoint(ray);

            if (hit != RulerPoint.None)
            {
                SetSelected(hit);
                SwallowUntilRelease = true;
                return true;
            }

            // A click elsewhere lets go of the point, and selects as usual
            if (Selected != RulerPoint.None)
                SetSelected(RulerPoint.None);
        }

        return taken;
    }

    /// <summary>Turns the picked depth into a point under the click.</summary>
    private void FinishPlacing(VulkanViewport viewport, float depth)
    {
        if (depth <= 0 || !Unproject(viewport, AwaitingScreen, depth, out var point))
        {
            Status = LOC.Get("MAP_Ruler_Status_Nothing_Hit");
            return;
        }

        // Diagnostic for verification: remove once placement is confirmed
        Smithbox.Log(typeof(MapRulerTool), $"[Ruler] {AwaitingPoint} placed: screen {AwaitingScreen}, depth {depth:0.########}, point {point}", LogLevel.Debug);

        SetPoint(AwaitingPoint, point);
        Status = "";

        if (AwaitingPoint == RulerPoint.Start && PlacingBoth)
        {
            Placing = RulerPoint.End;
        }
        else
        {
            Placing = RulerPoint.None;
            PlacingBoth = false;
        }
    }

    /// <summary>
    /// The world position at a viewport position (pixels from its corner) and a picking depth, through the inverse of
    /// the view projection the scene was drawn with. Works for every view mode, as nothing is assumed about the
    /// projection.
    /// </summary>
    private static bool Unproject(VulkanViewport viewport, Vector2 screen, float depth, out Vector3 point)
    {
        point = default;

        if (viewport.Width <= 0 || viewport.Height <= 0)
            return false;

        // The same screen to clip mapping as the viewport's rays and WorldToScreen
        float x = 2.0f * screen.X / viewport.Width - 1.0f;
        float y = 1.0f - 2.0f * screen.Y / viewport.Height;
        float z = viewport.Device.IsDepthRangeZeroToOne ? depth : depth * 2.0f - 1.0f;

        var viewProjection = viewport.ViewportCamera.CameraTransform.CameraViewMatrixLH * viewport.ViewportCamera.ProjectionMatrix;

        if (!Matrix4x4.Invert(viewProjection, out var inverse))
            return false;

        var world = Vector4.Transform(new Vector4(x, y, z, 1.0f), inverse);

        if (MathF.Abs(world.W) < 1e-12f)
            return false;

        point = new Vector3(world.X, world.Y, world.Z) / world.W;

        return float.IsFinite(point.X) && float.IsFinite(point.Y) && float.IsFinite(point.Z);
    }

    /// <summary>The nearest point whose sphere the ray passes through.</summary>
    private RulerPoint HitPoint(Ray ray)
    {
        var radius = PointRadius;
        var best = RulerPoint.None;
        var bestDistance = float.MaxValue;

        foreach (var point in new[] { RulerPoint.Start, RulerPoint.End })
        {
            if (GetPoint(point) is not Vector3 position)
                continue;

            if (ViewportUtils.RaySphereIntersection(ref ray, position, radius, out var distance) && distance < bestDistance)
            {
                best = point;
                bestDistance = distance;
            }
        }

        return best;
    }

    private void SetSelected(RulerPoint point)
    {
        Selected = point;
        UpdateDrawing();
    }

    #endregion

    #region Drawing

    private static float PointRadius => Math.Max(CFG.Current.MapEditor_Ruler_Point_Size, 0.01f);

    private static Color LineColor => ToColor(CFG.Current.MapEditor_Ruler_Color);

    private static Color SelectedColor => ToColor(CFG.Current.MapEditor_Ruler_Selected_Color);

    private static Color ToColor(Vector3 c)
        => Color.FromArgb(255, (int)Math.Clamp(c.X, 0, 255), (int)Math.Clamp(c.Y, 0, 255), (int)Math.Clamp(c.Z, 0, 255));

    /// <summary>Brings the spheres and the line up to date with the points, making them if they aren't yet.</summary>
    private void UpdateDrawing()
    {
        if (Scene == null || !IsEnabled)
            return;

        SpherePrim ??= new DbgPrimWireSphere(Transform.Default, 1.0f, Color.White, 8, 12);
        LinePrim ??= new DbgPrimLineBatch(new List<(Vector3, Vector3)> { (Vector3.Zero, Vector3.UnitX) });

        UpdateSphere(ref StartProxy, Start, Selected == RulerPoint.Start);
        UpdateSphere(ref EndProxy, End, Selected == RulerPoint.End);

        LineProxy ??= MakeProxy(LinePrim);

        if (Start is Vector3 a && End is Vector3 b && Vector3.DistanceSquared(a, b) > 1e-10f)
        {
            LineProxy.World = LineWorld(a, b);
            SetColor(LineProxy, LineColor);
            LineProxy.Visible = true;
        }
        else
        {
            LineProxy.Visible = false;
        }
    }

    private void UpdateSphere(ref DebugPrimitiveRenderableProxy proxy, Vector3? position, bool selected)
    {
        proxy ??= MakeProxy(SpherePrim);

        if (position is Vector3 p)
        {
            proxy.World = Matrix4x4.CreateScale(PointRadius) * Matrix4x4.CreateTranslation(p);
            SetColor(proxy, selected ? SelectedColor : LineColor);
            proxy.Visible = true;
        }
        else
        {
            proxy.Visible = false;
        }
    }

    private DebugPrimitiveRenderableProxy MakeProxy(IDbgPrim prim)
    {
        var proxy = new DebugPrimitiveRenderableProxy(Scene.OpaqueRenderables, prim)
        {
            // Clicks go to what is behind, and the ruler shows through the map
            Pickable = false,
            RenderOverlay = true,
            BaseColor = LineColor,
            HighlightedColor = LineColor,
        };

        return proxy;
    }

    private static void SetColor(DebugPrimitiveRenderableProxy proxy, Color color)
    {
        if (proxy.BaseColor != color)
        {
            proxy.BaseColor = color;
            proxy.HighlightedColor = color;
        }
    }

    /// <summary>
    /// The world matrix that takes the unit line from the origin along X onto the line from a to b. The other two
    /// axes only need to be independent of X, so that the matrix can be inverted for culling.
    /// </summary>
    private static Matrix4x4 LineWorld(Vector3 a, Vector3 b)
    {
        var x = b - a;
        var direction = Vector3.Normalize(x);
        var helper = MathF.Abs(direction.Y) < 0.9f ? Vector3.UnitY : Vector3.UnitX;
        var y = Vector3.Normalize(Vector3.Cross(direction, helper));
        var z = Vector3.Cross(direction, y);

        return new Matrix4x4(
            x.X, x.Y, x.Z, 0,
            y.X, y.Y, y.Z, 0,
            z.X, z.Y, z.Z, 0,
            a.X, a.Y, a.Z, 1);
    }

    private void ReleaseDrawing()
    {
        StartProxy?.Dispose();
        EndProxy?.Dispose();
        LineProxy?.Dispose();

        StartProxy = null;
        EndProxy = null;
        LineProxy = null;
        Scene = null;
    }

    /// <summary>
    /// The distance in metres beside the middle of the line. Called by the viewport's overlay.
    /// </summary>
    public void DrawLabels(VulkanViewport viewport)
    {
        if (!IsEnabled || !CFG.Current.MapEditor_Ruler_Show_Label || viewport.Owner is not MapUniverse)
            return;

        if (Start is not Vector3 a || End is not Vector3 b)
            return;

        var screen = viewport.WorldToScreen((a + b) / 2, out bool inFront);

        if (!inFront || screen.X < 0 || screen.Y < 0 || screen.X > viewport.Width || screen.Y > viewport.Height)
            return;

        var drawList = ImGui.GetWindowDrawList();
        var origin = new Vector2(viewport.X, viewport.Y);
        var shadow = ImGui.GetColorU32(new Vector4(0, 0, 0, 1));
        var c = CFG.Current.MapEditor_Ruler_Color;
        var color = ImGui.GetColorU32(new Vector4(c / 255f, 1));
        var text = LOC.Get("MAP_Ruler_Label", $"{Vector3.Distance(a, b):0.##}");

        var at = origin + screen + new Vector2(6, -6) * DPI.UIScale();
        drawList.AddText(at + Vector2.One, shadow, text);
        drawList.AddText(at, color, text);
    }

    #endregion

    #region Tool Window

    public void OnToolWindow()
    {
        ImGui.BeginChild("mapRulerToolSection", ImGuiChildFlags.Borders);

        GUI.WrappedText(LOC.Get("MAP_Ruler_Description"));

        // Measure
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Ruler_Measure_Header"), LOC.Get("MAP_Ruler_Measure_Header_TT"));

        if (Placing != RulerPoint.None)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color,
                LOC.Get(Placing == RulerPoint.Start ? "MAP_Ruler_Status_Placing_Start" : "MAP_Ruler_Status_Placing_End"));
        }
        else if (!string.IsNullOrEmpty(Status))
        {
            GUI.WrappedText(Status);
        }

        GUI.ConditionalMultiButtonInput("mapRulerMeasureActions",
            "mapRulerMeasure", LOC.Get("MAP_Ruler_Measure"), LOC.Get("MAP_Ruler_Measure_TT"),
            () => BeginPlacing(RulerPoint.Start, true), true,

            "mapRulerCancel", LOC.Get("MAP_Ruler_Cancel"), LOC.Get("MAP_Ruler_Cancel_TT"),
            CancelPlacing, Placing != RulerPoint.None,

            "mapRulerClear", LOC.Get("MAP_Ruler_Clear"), LOC.Get("MAP_Ruler_Clear_TT"),
            Clear, Start != null || End != null);

        // Distance
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Ruler_Distance_Header"), LOC.Get("MAP_Ruler_Distance_Header_TT"));

        if (Start is Vector3 a && End is Vector3 b)
        {
            var delta = b - a;

            GUI.WrappedText(LOC.Get("MAP_Ruler_Distance", $"{delta.Length():0.###}"));
            GUI.WrappedText(LOC.Get("MAP_Ruler_Horizontal", $"{new Vector2(delta.X, delta.Z).Length():0.###}"));
            GUI.WrappedText(LOC.Get("MAP_Ruler_Vertical", $"{delta.Y:0.###}"));
            GUI.WrappedText(LOC.Get("MAP_Ruler_Delta", $"{delta.X:0.###}", $"{delta.Y:0.###}", $"{delta.Z:0.###}"));
        }
        else
        {
            GUI.WrappedText(LOC.Get("MAP_Ruler_Distance_None"));
        }

        // Start and End
        DisplayPoint(RulerPoint.Start, "Start");
        DisplayPoint(RulerPoint.End, "End");

        // Display
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Ruler_Display_Header"), LOC.Get("MAP_Ruler_Display_Header_TT"));

        ImGui.SetNextItemWidth(-1);
        if (ImGui.SliderFloat("##mapRulerPointSize", ref CFG.Current.MapEditor_Ruler_Point_Size, 0.02f, 2.0f, "%.2f m"))
            UpdateDrawing();
        GUI.Tooltip(LOC.Get("MAP_Ruler_Point_Size_TT"));

        var color = CFG.Current.MapEditor_Ruler_Color / 255f;
        if (ImGui.ColorEdit3($"{LOC.Get("MAP_Ruler_Color")}##mapRulerColor", ref color))
        {
            CFG.Current.MapEditor_Ruler_Color = color * 255f;
            UpdateDrawing();
        }
        GUI.Tooltip(LOC.Get("MAP_Ruler_Color_TT"));

        var selectedColor = CFG.Current.MapEditor_Ruler_Selected_Color / 255f;
        if (ImGui.ColorEdit3($"{LOC.Get("MAP_Ruler_Selected_Color")}##mapRulerSelectedColor", ref selectedColor))
        {
            CFG.Current.MapEditor_Ruler_Selected_Color = selectedColor * 255f;
            UpdateDrawing();
        }
        GUI.Tooltip(LOC.Get("MAP_Ruler_Selected_Color_TT"));

        ImGui.Checkbox($"{LOC.Get("MAP_Ruler_Show_Label")}##mapRulerShowLabel", ref CFG.Current.MapEditor_Ruler_Show_Label);
        GUI.Tooltip(LOC.Get("MAP_Ruler_Show_Label_TT"));

        ImGui.EndChild();
    }

    private void DisplayPoint(RulerPoint point, string id)
    {
        var name = point == RulerPoint.Start ? "Start" : "End";

        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get($"MAP_Ruler_{name}_Header"), LOC.Get($"MAP_Ruler_{name}_Header_TT"));

        var position = GetPoint(point);

        if (position is Vector3 p)
        {
            ImGui.SetNextItemWidth(-1);
            if (ImGui.DragFloat3($"##mapRuler{id}Position", ref p, 0.05f, 0, 0, "%.3f"))
                SetPoint(point, p);
            GUI.Tooltip(LOC.Get("MAP_Ruler_Position_TT"));
        }
        else
        {
            GUI.WrappedText(LOC.Get("MAP_Ruler_Point_Not_Set"));
        }

        var selection = GetSelectionPosition();

        GUI.ConditionalMultiButtonInput($"mapRuler{id}Actions",
            $"mapRuler{id}Place", LOC.Get("MAP_Ruler_Place"), LOC.Get("MAP_Ruler_Place_TT"),
            () => BeginPlacing(point, false), true,

            $"mapRuler{id}Gizmo", LOC.Get(Selected == point ? "MAP_Ruler_Release_Gizmo" : "MAP_Ruler_Gizmo"), LOC.Get("MAP_Ruler_Gizmo_TT"),
            () => SetSelected(Selected == point ? RulerPoint.None : point), position != null,

            $"mapRuler{id}Selection", LOC.Get("MAP_Ruler_From_Selection"), LOC.Get("MAP_Ruler_From_Selection_TT"),
            () => { if (GetSelectionPosition() is Vector3 s) SetPoint(point, s); }, selection != null);
    }

    /// <summary>The world position of the selection: the average of the selected objects' positions.</summary>
    private Vector3? GetSelectionPosition()
    {
        var selection = View.ViewportSelection.GetFilteredSelection<Entity>(o => o.HasTransform);

        if (selection.Count == 0)
            return null;

        var sum = Vector3.Zero;

        foreach (var entity in selection)
            sum += Vector3.Transform(Vector3.Zero, entity.GetWorldMatrix());

        return sum / selection.Count;
    }

    private void BeginPlacing(RulerPoint point, bool both)
    {
        Placing = point;
        PlacingBoth = both;
        Status = "";
    }

    private void CancelPlacing()
    {
        Placing = RulerPoint.None;
        PlacingBoth = false;
    }

    public void Clear()
    {
        CancelPlacing();
        Start = null;
        End = null;
        Selected = RulerPoint.None;
        Status = "";
        UpdateDrawing();
    }

    #endregion
}

/// <summary>
/// Gizmo target for the ruler's picked point. Only the position moves; rotation and scale are ignored.
/// </summary>
public class RulerGizmoTarget : IGizmoTarget
{
    private readonly MapRulerTool Tool;

    public RulerGizmoTarget(MapRulerTool tool)
    {
        Tool = tool;
    }

    public bool HasTarget => CFG.Current.Interface_MapEditor_Tool_Ruler && Tool.GetPoint(Tool.Selected) != null;

    public bool AllowScale => false;

    public Transform GetTransform()
    {
        return Tool.GetPoint(Tool.Selected) is Vector3 p ? new Transform(p, Quaternion.Identity, Vector3.One) : Transform.Default;
    }

    public void Preview(Transform original, Transform current, GizmoState.GizmosMode mode)
    {
        if (mode == GizmoState.GizmosMode.Translate && Tool.Selected != RulerPoint.None)
            Tool.SetPoint(Tool.Selected, current.Position);
    }

    public void Commit(Transform original, Transform current, GizmoState.GizmosMode mode)
    {
        Preview(original, current, mode);
    }
}
