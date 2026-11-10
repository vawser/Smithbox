using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.HavokEditor;
using StudioCore.Editors.Viewport;
using StudioCore.Keybinds;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using Veldrid.Utilities;

namespace StudioCore.Editors.MapEditor;

/// <summary>How the User-Defined shape turns its placed points into triangles.</summary>
public enum CollisionUserDefinedMode
{
    [Display(Name = "MAP_CollisionEditor_User_Mode_Line")]
    Line,
    [Display(Name = "MAP_CollisionEditor_User_Mode_Plane")]
    Plane,
    [Display(Name = "MAP_CollisionEditor_User_Mode_Box")]
    Box
}

/// <summary>How the User-Defined shape's points are placed.</summary>
public enum CollisionUserDefinedPlacement
{
    [Display(Name = "MAP_CollisionEditor_User_Placement_Raycast")]
    Raycast,
    [Display(Name = "MAP_CollisionEditor_User_Placement_Gizmo")]
    Gizmo
}

/// <summary>
/// The User-Defined shape of the Create tab: points placed in the viewport (by clicking on what it draws, as the
/// Ruler places its points), each drawn as a wire sphere with a line to the next, and moved with the gizmo once
/// picked. They make:
/// <list type="bullet">
/// <item>Line: a vertical wall along them, from each point up by the height.</item>
/// <item>Plane: the outline they make, filled, at each point's own height.</item>
/// <item>Box: that outline extruded into a closed solid, floor at the lowest point, roof the height above the highest.</item>
/// </list>
/// The points are in world space; the new part goes on the first, so the triangles are relative to it.
/// </summary>
public class CollisionEditorUserDefined
{
    private readonly MapEditorView View;
    private readonly CollisionEditorTool Tool;

    /// <summary>The placed points, in the world, in the order they join.</summary>
    public List<Vector3> Points { get; } = new();

    /// <summary>The point the gizmo moves, or -1.</summary>
    public int Selected { get; private set; } = -1;

    /// <summary>True while each viewport click adds a point.</summary>
    public bool Placing { get; private set; }

    // Changes whenever a point does, so the Create tab knows to build the triangles again
    public int Version { get; private set; }

    // A placing click waiting for its picking pass: where it was and in which viewport
    private bool AwaitingPick;
    private Vector2 AwaitingScreen;
    private VulkanViewport AwaitingViewport;

    // Clicks taken by the tool are held until the button is released, so the release doesn't select
    private bool SwallowUntilRelease;

    private string Status = "";

    // The frame the Create tab last showed this shape, so the points are only drawn and used while it does
    private int LastShownFrame = -10;

    public readonly CollisionPointGizmoTarget GizmoTarget;

    // Drawing: a unit sphere and a unit line, shared by the proxies and placed by their world matrices, so moving a
    // point never rebuilds geometry
    private RenderScene Scene;
    private DbgPrimWireSphere SpherePrim;
    private DbgPrimLineBatch LinePrim;
    private readonly List<DebugPrimitiveRenderableProxy> SphereProxies = new();
    private readonly List<DebugPrimitiveRenderableProxy> LineProxies = new();
    private bool DrawingDirty = true;

    // The triangles drawn in the viewport, as of the last time they changed
    private MeshRenderableProxy PreviewMesh;
    private string PreviewMeshKey;

    // Points closer than this (across the ground) are the same point
    private const float MinSpacing = 0.001f;
    private const float MinHeight = 0.01f;

    public CollisionEditorUserDefined(MapEditorView view, CollisionEditorTool tool)
    {
        View = view;
        Tool = tool;
        GizmoTarget = new CollisionPointGizmoTarget(this);
    }

    /// <summary>Whether the Create tab showed this shape last frame (the tab is drawn after the viewport).</summary>
    public bool IsShown => Tool.CurrentFrame - LastShownFrame <= 1;

    /// <summary>Called by the Create tab each frame it shows this shape.</summary>
    public void MarkShown()
    {
        LastShownFrame = Tool.CurrentFrame;
    }

    public void SetPoint(int index, Vector3 position)
    {
        if (index < 0 || index >= Points.Count || Points[index] == position)
            return;

        Points[index] = position;
        Changed();
    }

    private void Changed()
    {
        Version++;
        DrawingDirty = true;
    }

    #region Viewport

    /// <summary>
    /// Places and picks the points, and keeps their drawing and the preview in the viewport's scene. Called each frame
    /// by the tool. Returns true while the viewport's clicks are taken, so they don't select.
    /// </summary>
    public bool HandleViewportInput(VulkanViewport viewport, Ray ray, bool canPick, Func<string> previewKey,
        Func<(List<Vector3> Vertices, List<int> Indices)?> buildPreview)
    {
        if (!IsShown)
        {
            Placing = false;
            AwaitingPick = false;
            SwallowUntilRelease = false;
            Selected = -1;
            HideDrawing();
            return false;
        }

        if (viewport.RenderScene != Scene)
        {
            ReleaseDrawing();
            Scene = viewport.RenderScene;
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

        var taken = false;

        if (SwallowUntilRelease)
        {
            if (!InputManager.IsMouseDown(MousebindID.Viewport_Picking_Action) || InputManager.IsMouseReleased(MousebindID.Viewport_Picking_Action))
                SwallowUntilRelease = false;

            taken = true;
        }

        var pressed = canPick && InputManager.IsMousePressed(MousebindID.Viewport_Picking_Action);

        if (Placing)
        {
            if (pressed && !AwaitingPick && !viewport.ViewPipeline.IsPickingPending)
            {
                var mouse = InputManager.MousePosition;

                // The centre of the pixel the picking pass tests
                AwaitingScreen = new Vector2(MathF.Floor(mouse.X - viewport.X) + 0.5f, MathF.Floor(mouse.Y - viewport.Y) + 0.5f);
                AwaitingViewport = viewport;
                AwaitingPick = true;

                viewport.ViewPipeline.PickingResultsReady = false;
                viewport.ViewPipeline.CreateAsyncPickingRequest();

                SwallowUntilRelease = true;
            }

            taken = true;
        }
        else if (AwaitingPick)
        {
            taken = true;
        }
        else if (pressed)
        {
            var hit = HitPoint(ray);

            if (hit >= 0)
            {
                SetSelected(hit);
                SwallowUntilRelease = true;
                taken = true;
            }
            else if (Selected >= 0)
            {
                // A click elsewhere lets go of the point, and selects as usual
                SetSelected(-1);
            }
        }

        UpdateDrawing();
        UpdatePreviewMesh(previewKey, buildPreview);

        return taken;
    }

    /// <summary>Turns the picked depth into a point under the click, added after the last.</summary>
    private void FinishPlacing(VulkanViewport viewport, float depth)
    {
        if (depth <= 0 || !Unproject(viewport, AwaitingScreen, depth, out var point))
        {
            Status = LOC.Get("MAP_CollisionEditor_User_Nothing_Hit");
            return;
        }

        // Diagnostic for verification: remove once placement is confirmed
        Smithbox.Log(this, $"[Collision User-Defined] point {Points.Count} placed: screen {AwaitingScreen}, depth {depth:0.########}, point {point}",
            Microsoft.Extensions.Logging.LogLevel.Debug);

        Points.Add(point);
        Status = "";
        Changed();
    }

    /// <summary>
    /// The world position at a viewport position (pixels from its corner) and a picking depth, through the inverse of
    /// the view projection the scene was drawn with, as the Ruler finds its points.
    /// </summary>
    private static bool Unproject(VulkanViewport viewport, Vector2 screen, float depth, out Vector3 point)
    {
        point = default;

        if (viewport.Width <= 0 || viewport.Height <= 0)
            return false;

        var x = 2.0f * screen.X / viewport.Width - 1.0f;
        var y = 1.0f - 2.0f * screen.Y / viewport.Height;
        var z = viewport.Device.IsDepthRangeZeroToOne ? depth : depth * 2.0f - 1.0f;

        var viewProjection = viewport.ViewportCamera.CameraTransform.CameraViewMatrixLH * viewport.ViewportCamera.ProjectionMatrix;

        if (!Matrix4x4.Invert(viewProjection, out var inverse))
            return false;

        var world = Vector4.Transform(new Vector4(x, y, z, 1.0f), inverse);

        if (MathF.Abs(world.W) < 1e-12f)
            return false;

        point = new Vector3(world.X, world.Y, world.Z) / world.W;

        return float.IsFinite(point.X) && float.IsFinite(point.Y) && float.IsFinite(point.Z);
    }

    /// <summary>The nearest point whose sphere the ray passes through, or -1.</summary>
    private int HitPoint(Ray ray)
    {
        var radius = PointRadius;
        var best = -1;
        var bestDistance = float.MaxValue;

        for (var i = 0; i < Points.Count; i++)
        {
            if (ViewportUtils.RaySphereIntersection(ref ray, Points[i], radius, out var distance) && distance < bestDistance)
            {
                best = i;
                bestDistance = distance;
            }
        }

        return best;
    }

    private void SetSelected(int index)
    {
        Selected = index;
        DrawingDirty = true;
    }

    #endregion

    #region Drawing

    private static float PointRadius => Math.Max(CFG.Current.MapEditor_CollisionEditor_User_Point_Size, 0.01f);

    private static Color PointColor => ToColor(CFG.Current.MapEditor_CollisionEditor_Cursor_Color);

    private static Color SelectedColor => ToColor(CFG.Current.MapEditor_CollisionEditor_Vertex_Color);

    private static Color ToColor(Vector3 c)
        => Color.FromArgb(255, (int)Math.Clamp(c.X, 0, 255), (int)Math.Clamp(c.Y, 0, 255), (int)Math.Clamp(c.Z, 0, 255));

    /// <summary>
    /// A sphere on each point and a line from each to the next (and back to the first, when the shape is closed).
    /// </summary>
    private void UpdateDrawing()
    {
        if (Scene == null || !DrawingDirty)
            return;

        DrawingDirty = false;

        SpherePrim ??= new DbgPrimWireSphere(Transform.Default, 1.0f, Color.White, 8, 12);
        LinePrim ??= new DbgPrimLineBatch(new List<(Vector3, Vector3)> { (Vector3.Zero, Vector3.UnitX) });

        while (SphereProxies.Count < Points.Count)
            SphereProxies.Add(MakeProxy(SpherePrim));

        for (var i = 0; i < SphereProxies.Count; i++)
        {
            var proxy = SphereProxies[i];

            if (i < Points.Count)
            {
                proxy.World = Matrix4x4.CreateScale(PointRadius) * Matrix4x4.CreateTranslation(Points[i]);
                SetColor(proxy, i == Selected ? SelectedColor : PointColor);
                SetVisible(proxy, true);
            }
            else
            {
                SetVisible(proxy, false);
            }
        }

        var segments = GetSegments();

        while (LineProxies.Count < segments.Count)
            LineProxies.Add(MakeProxy(LinePrim));

        for (var i = 0; i < LineProxies.Count; i++)
        {
            var proxy = LineProxies[i];

            if (i < segments.Count)
            {
                proxy.World = LineWorld(segments[i].A, segments[i].B);
                SetColor(proxy, PointColor);
                SetVisible(proxy, true);
            }
            else
            {
                SetVisible(proxy, false);
            }
        }
    }

    /// <summary>The lines drawn between the points: closed for a plane, a box, or a closed line.</summary>
    private List<(Vector3 A, Vector3 B)> GetSegments()
    {
        var segments = new List<(Vector3, Vector3)>();

        for (var i = 0; i + 1 < Points.Count; i++)
        {
            if (Vector3.DistanceSquared(Points[i], Points[i + 1]) > 1e-10f)
                segments.Add((Points[i], Points[i + 1]));
        }

        var closed = CFG.Current.MapEditor_CollisionEditor_User_Mode is not CollisionUserDefinedMode.Line
            || CFG.Current.MapEditor_CollisionEditor_User_Closed;

        if (closed && Points.Count > 2 && Vector3.DistanceSquared(Points[^1], Points[0]) > 1e-10f)
            segments.Add((Points[^1], Points[0]));

        return segments;
    }

    /// <summary>
    /// Draws the triangles the points make, in the world, so they can be seen against the map. Hidden while placing,
    /// as the clicks would land on them.
    /// </summary>
    private void UpdatePreviewMesh(Func<string> previewKey, Func<(List<Vector3> Vertices, List<int> Indices)?> buildPreview)
    {
        var shown = !Placing && !AwaitingPick && CFG.Current.MapEditor_CollisionEditor_User_Show_Preview;

        if (!shown)
        {
            if (PreviewMesh != null && PreviewMesh.Visible)
                PreviewMesh.Visible = false;

            return;
        }

        var key = previewKey();

        if (PreviewMesh == null || PreviewMeshKey != key)
        {
            DisposePreviewMesh();
            PreviewMeshKey = key;

            if (buildPreview() is not { } geometry)
                return;

            var color = CFG.Current.Viewport_Collision_Color;
            var provider = RawMeshProvider.FromTriangles(geometry.Vertices, geometry.Indices,
                Color.FromArgb(255, (int)color.X, (int)color.Y, (int)color.Z));

            PreviewMesh = MeshRenderableProxy.FromProvider(Scene, provider);
            PreviewMesh.DrawFilter = RenderFilter.Collision;
            PreviewMesh.World = Matrix4x4.Identity;
        }

        if (!PreviewMesh.Visible)
            PreviewMesh.Visible = true;
    }

    private DebugPrimitiveRenderableProxy MakeProxy(IDbgPrim prim)
    {
        return new DebugPrimitiveRenderableProxy(Scene.OpaqueRenderables, prim)
        {
            // Clicks go to what is behind, and the points show through the map
            Pickable = false,
            RenderOverlay = true,
            BaseColor = PointColor,
            HighlightedColor = PointColor,
        };
    }

    private static void SetColor(DebugPrimitiveRenderableProxy proxy, Color color)
    {
        if (proxy.BaseColor != color)
        {
            proxy.BaseColor = color;
            proxy.HighlightedColor = color;
        }
    }

    private static void SetVisible(RenderableProxy proxy, bool visible)
    {
        if (proxy != null && proxy.Visible != visible)
            proxy.Visible = visible;
    }

    /// <summary>
    /// The world matrix that takes the unit line from the origin along X onto the line from a to b (as the Ruler's).
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

    private void HideDrawing()
    {
        foreach (var proxy in SphereProxies)
            SetVisible(proxy, false);

        foreach (var proxy in LineProxies)
            SetVisible(proxy, false);

        if (PreviewMesh != null && PreviewMesh.Visible)
            PreviewMesh.Visible = false;

        // Shown again as they are
        DrawingDirty = true;
    }

    private void DisposePreviewMesh()
    {
        PreviewMesh?.Dispose();
        PreviewMesh = null;
        PreviewMeshKey = null;
    }

    private void ReleaseDrawing()
    {
        foreach (var proxy in SphereProxies)
            proxy.Dispose();

        foreach (var proxy in LineProxies)
            proxy.Dispose();

        SphereProxies.Clear();
        LineProxies.Clear();
        DisposePreviewMesh();

        Scene = null;
        DrawingDirty = true;
    }

    #endregion

    #region Tool Window

    /// <summary>
    /// The User-Defined shape options: the mode and its height, loop and flip. Under the Create tab's Parameters
    /// header, so they hide with it.
    /// </summary>
    public void DisplayShapeOptions()
    {
        var mode = CFG.Current.MapEditor_CollisionEditor_User_Mode;

        // Mode
        ImGui.SetNextItemWidth(-1);
        if (ImGui.BeginCombo("##collisionUserMode", LOC.Get(mode.GetDisplayName())))
        {
            foreach (var value in Enum.GetValues<CollisionUserDefinedMode>())
            {
                if (ImGui.Selectable($"{LOC.Get(value.GetDisplayName())}##collisionUserMode{value}", value == mode))
                {
                    CFG.Current.MapEditor_CollisionEditor_User_Mode = value;
                    DrawingDirty = true;
                }
                GUI.Tooltip(LOC.Get($"MAP_CollisionEditor_User_Mode_{value}_TT"));
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("MAP_CollisionEditor_User_Mode_Input_TT"));

        mode = CFG.Current.MapEditor_CollisionEditor_User_Mode;

        if (mode is CollisionUserDefinedMode.Line or CollisionUserDefinedMode.Box)
        {
            var height = CFG.Current.MapEditor_CollisionEditor_User_Height;
            if (ImGui.InputFloat($"{LOC.Get("MAP_CollisionEditor_User_Height")}##collisionUserHeight", ref height, 0.5f, 1.0f, "%.2f"))
                CFG.Current.MapEditor_CollisionEditor_User_Height = MathF.Max(height, MinHeight);
            GUI.Tooltip(LOC.Get(mode is CollisionUserDefinedMode.Line
                ? "MAP_CollisionEditor_User_Height_Line_TT"
                : "MAP_CollisionEditor_User_Height_Box_TT"));
        }

        if (mode is CollisionUserDefinedMode.Line)
        {
            if (ImGui.Checkbox($"{LOC.Get("MAP_CollisionEditor_User_Closed")}##collisionUserClosed", ref CFG.Current.MapEditor_CollisionEditor_User_Closed))
                DrawingDirty = true;
            GUI.Tooltip(LOC.Get("MAP_CollisionEditor_User_Closed_TT"));

            ImGui.Checkbox($"{LOC.Get("MAP_CollisionEditor_User_Flip")}##collisionUserFlip", ref CFG.Current.MapEditor_CollisionEditor_User_Flip);
            GUI.Tooltip(LOC.Get("MAP_CollisionEditor_User_Flip_TT"));
        }
    }

    /// <summary>
    /// The User-Defined points: how they are placed, the placing buttons, the count and why the points make no shape,
    /// the point list and display. Always shown, as it holds the actions and warnings.
    /// </summary>
    public void DisplayPoints(string problem)
    {
        var mode = CFG.Current.MapEditor_CollisionEditor_User_Mode;

        // Points
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CollisionEditor_User_Points_Header"), LOC.Get("MAP_CollisionEditor_User_Points_Header_TT"));

        // Placement
        var placement = CFG.Current.MapEditor_CollisionEditor_User_Placement;

        ImGui.SetNextItemWidth(-1);
        if (ImGui.BeginCombo("##collisionUserPlacement", LOC.Get(placement.GetDisplayName())))
        {
            foreach (var value in Enum.GetValues<CollisionUserDefinedPlacement>())
            {
                if (ImGui.Selectable($"{LOC.Get(value.GetDisplayName())}##collisionUserPlacement{value}", value == placement))
                {
                    CFG.Current.MapEditor_CollisionEditor_User_Placement = value;

                    // Clicks only place points with a raycast
                    if (value is not CollisionUserDefinedPlacement.Raycast)
                        Placing = false;

                    Status = "";
                }
                GUI.Tooltip(LOC.Get($"MAP_CollisionEditor_User_Placement_{value}_TT"));
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("MAP_CollisionEditor_User_Placement_Input_TT"));

        placement = CFG.Current.MapEditor_CollisionEditor_User_Placement;

        if (placement is CollisionUserDefinedPlacement.Gizmo)
        {
            ImGui.SetNextItemWidth(-1);
            if (ImGui.InputFloat("##collisionUserSpawnSpacing", ref CFG.Current.MapEditor_CollisionEditor_User_Spawn_Spacing, 0.5f, 1.0f, "%.2f m"))
                CFG.Current.MapEditor_CollisionEditor_User_Spawn_Spacing = MathF.Max(CFG.Current.MapEditor_CollisionEditor_User_Spawn_Spacing, 0f);
            GUI.Tooltip(LOC.Get("MAP_CollisionEditor_User_Spawn_Spacing_TT"));
        }

        if (Placing)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, LOC.Get("MAP_CollisionEditor_User_Placing"));
        }
        else if (!string.IsNullOrEmpty(Status))
        {
            GUI.WrappedText(Status);
        }

        if (placement is CollisionUserDefinedPlacement.Gizmo)
        {
            GUI.ConditionalMultiButtonInput("collisionUserPointActions",
                "collisionUserAdd", LOC.Get("MAP_CollisionEditor_User_Add_Point"), LOC.Get("MAP_CollisionEditor_User_Add_Point_TT"),
                AddGizmoPoint, true,

                "collisionUserRemoveLast", LOC.Get("MAP_CollisionEditor_User_Remove_Last"), LOC.Get("MAP_CollisionEditor_User_Remove_Last_TT"),
                () => RemovePoint(Points.Count - 1), Points.Count > 0,

                "collisionUserClear", LOC.Get("MAP_CollisionEditor_User_Clear"), LOC.Get("MAP_CollisionEditor_User_Clear_TT"),
                Clear, Points.Count > 0);
        }
        else
        {
            GUI.ConditionalMultiButtonInput("collisionUserPointActions",
                "collisionUserPlace", LOC.Get(Placing ? "MAP_CollisionEditor_User_Stop_Placing" : "MAP_CollisionEditor_User_Place"),
                LOC.Get(Placing ? "MAP_CollisionEditor_User_Stop_Placing_TT" : "MAP_CollisionEditor_User_Place_TT"),
                TogglePlacing, true,

                "collisionUserRemoveLast", LOC.Get("MAP_CollisionEditor_User_Remove_Last"), LOC.Get("MAP_CollisionEditor_User_Remove_Last_TT"),
                () => RemovePoint(Points.Count - 1), Points.Count > 0,

                "collisionUserClear", LOC.Get("MAP_CollisionEditor_User_Clear"), LOC.Get("MAP_CollisionEditor_User_Clear_TT"),
                Clear, Points.Count > 0);
        }

        GUI.WrappedText(LOC.Get("MAP_CollisionEditor_User_Point_Count", Points.Count, GetMinimumPoints(mode)));

        if (problem != null && Points.Count > 0)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, problem);

        // Point List
        GUI.Spacer();
        GUI.ConditionalHeader(LOC.Get("MAP_CollisionEditor_User_Point_List_Header"),
            LOC.Get("MAP_CollisionEditor_User_Point_List_Header_TT"),
            ref CFG.Current.MapEditor_CollisionEditor_User_Show_Points);

        if (CFG.Current.MapEditor_CollisionEditor_User_Show_Points && Points.Count > 0)
        {
            ImGui.BeginChild("collisionUserPointList", new Vector2(0, 150 * DPI.UIScale()), ImGuiChildFlags.Borders);

            var remove = -1;

            for (var i = 0; i < Points.Count; i++)
            {
                var selected = i == Selected;

                if (ImGui.Button($"{(selected ? Icons.Crosshairs : Icons.ArrowsAlt)}##collisionUserPointGizmo{i}"))
                    SetSelected(selected ? -1 : i);
                GUI.Tooltip(LOC.Get(selected ? "MAP_CollisionEditor_User_Point_Release_TT" : "MAP_CollisionEditor_User_Point_Gizmo_TT"));

                ImGui.SameLine();
                if (ImGui.Button($"{Icons.Times}##collisionUserPointRemove{i}"))
                    remove = i;
                GUI.Tooltip(LOC.Get("MAP_CollisionEditor_User_Point_Remove_TT"));

                ImGui.SameLine();
                var p = Points[i];
                ImGui.SetNextItemWidth(-1);
                if (ImGui.DragFloat3($"##collisionUserPoint{i}", ref p, 0.05f, 0, 0, "%.3f"))
                    SetPoint(i, p);
                GUI.Tooltip(LOC.Get("MAP_CollisionEditor_User_Point_Position_TT", i + 1));
            }

            ImGui.EndChild();

            if (remove >= 0)
                RemovePoint(remove);
        }

        // Point Display
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CollisionEditor_User_Display_Header"), LOC.Get("MAP_CollisionEditor_User_Display_Header_TT"));

        ImGui.SetNextItemWidth(-1);
        if (ImGui.SliderFloat("##collisionUserPointSize", ref CFG.Current.MapEditor_CollisionEditor_User_Point_Size, 0.02f, 2.0f, "%.2f m"))
            DrawingDirty = true;
        GUI.Tooltip(LOC.Get("MAP_CollisionEditor_User_Point_Size_TT"));

        ImGui.Checkbox($"{LOC.Get("MAP_CollisionEditor_User_Show_Preview")}##collisionUserShowPreview", ref CFG.Current.MapEditor_CollisionEditor_User_Show_Preview);
        GUI.Tooltip(LOC.Get("MAP_CollisionEditor_User_Show_Preview_TT"));
    }

    /// <summary>
    /// Stops placing points and lets go of the picked point, so clicks select and the gizmo moves the selection again.
    /// The points are kept.
    /// </summary>
    public void StopModes()
    {
        Placing = false;
        Status = "";
        SetSelected(-1);
    }

    private void TogglePlacing()
    {
        Placing = !Placing;
        Status = "";

        if (Placing)
            SetSelected(-1);
    }

    /// <summary>
    /// Spawns a point to be moved with the gizmo, and picks it: after the picked point (or the last), the spacing along
    /// world X from it, or at the placement orb if there are none yet.
    /// </summary>
    private void AddGizmoPoint()
    {
        Vector3 position;
        int index;

        if (Points.Count == 0)
        {
            position = View.ViewportHandler.ActiveViewport?.Viewport != null
                ? View.ViewportWindow.GetPlacementPosition()
                : Vector3.Zero;
            index = 0;
        }
        else
        {
            var after = Selected >= 0 && Selected < Points.Count ? Selected : Points.Count - 1;

            position = Points[after] + new Vector3(CFG.Current.MapEditor_CollisionEditor_User_Spawn_Spacing, 0, 0);
            index = after + 1;
        }

        Points.Insert(index, position);
        Status = "";
        Changed();

        SetSelected(index);
    }

    private void RemovePoint(int index)
    {
        if (index < 0 || index >= Points.Count)
            return;

        Points.RemoveAt(index);

        if (Selected == index)
            Selected = -1;
        else if (Selected > index)
            Selected--;

        Changed();
    }

    public void Clear()
    {
        Points.Clear();
        Selected = -1;
        Placing = false;
        Status = "";
        Changed();
    }

    private static int GetMinimumPoints(CollisionUserDefinedMode mode) => mode switch
    {
        CollisionUserDefinedMode.Line => 2,
        CollisionUserDefinedMode.Box => 2,
        _ => 3
    };

    /// <summary>The inputs the triangles depend on, so they are only built again when one changes.</summary>
    public string GetKey(bool facingUp)
    {
        var key = new StringBuilder();
        var cfg = CFG.Current;

        key.Append(cfg.MapEditor_CollisionEditor_User_Mode).Append('/')
            .Append(cfg.MapEditor_CollisionEditor_User_Height.ToString(CultureInfo.InvariantCulture)).Append('/')
            .Append(cfg.MapEditor_CollisionEditor_User_Closed).Append('/')
            .Append(cfg.MapEditor_CollisionEditor_User_Flip).Append('/')
            .Append(facingUp).Append('/')
            .Append(Version);

        return key.ToString();
    }

    #endregion

    #region Geometry

    /// <summary>
    /// The triangles the points make, in the world. Returns why there are none, or null if there are. Faces point the
    /// way the Create tab's facing means: up (or out of a box) with facingUp, the other way without. A line's wall
    /// faces left of its direction, seen from above, unless flipped.
    /// </summary>
    public string BuildGeometry(bool facingUp, out List<Vector3> vertices, out List<int> indices)
    {
        vertices = new List<Vector3>();
        indices = new List<int>();

        var mode = CFG.Current.MapEditor_CollisionEditor_User_Mode;
        var height = MathF.Max(CFG.Current.MapEditor_CollisionEditor_User_Height, MinHeight);

        if (Points.Count < GetMinimumPoints(mode))
            return LOC.Get("MAP_CollisionEditor_User_Too_Few_Points", GetMinimumPoints(mode));

        return mode switch
        {
            CollisionUserDefinedMode.Line => BuildLine(height, vertices, indices),
            CollisionUserDefinedMode.Plane => BuildPlane(facingUp, vertices, indices),
            _ => BuildBox(height, facingUp, vertices, indices)
        };
    }

    /// <summary>
    /// A vertical wall along the points: a bottom vertex on each point and a top vertex the height above it, two
    /// triangles between each pair. Segments without length across the ground are skipped, as they'd make no wall.
    /// </summary>
    private string BuildLine(float height, List<Vector3> vertices, List<int> indices)
    {
        var points = WithoutRepeats(Points, closed: false);
        var closed = CFG.Current.MapEditor_CollisionEditor_User_Closed && points.Count > 2;

        if (closed && HorizontalDistance(points[^1], points[0]) < MinSpacing)
            points.RemoveAt(points.Count - 1);

        if (points.Count < 2)
            return LOC.Get("MAP_CollisionEditor_User_Too_Few_Points", 2);

        var up = new Vector3(0, height, 0);

        foreach (var point in points)
        {
            vertices.Add(point);
            vertices.Add(point + up);
        }

        var flip = CFG.Current.MapEditor_CollisionEditor_User_Flip;
        var count = closed ? points.Count : points.Count - 1;

        for (var i = 0; i < count; i++)
        {
            var j = (i + 1) % points.Count;

            if (HorizontalDistance(points[i], points[j]) < MinSpacing)
                continue;

            AddQuad(indices, i * 2, j * 2, j * 2 + 1, i * 2 + 1, flip);
        }

        return indices.Count == 0 ? LOC.Get("MAP_CollisionEditor_User_No_Wall") : null;
    }

    /// <summary>The outline of the points filled, each corner at its point's own height.</summary>
    private string BuildPlane(bool facingUp, List<Vector3> vertices, List<int> indices)
    {
        var points = WithoutRepeats(Points, closed: true);

        if (points.Count < 3)
            return LOC.Get("MAP_CollisionEditor_User_Too_Few_Points", 3);

        var triangles = Triangulate(points);

        if (triangles == null)
            return LOC.Get("MAP_CollisionEditor_User_Outline_Invalid");

        vertices.AddRange(points);

        for (var t = 0; t < triangles.Count; t += 3)
            AddFacing(vertices, indices, triangles[t], triangles[t + 1], triangles[t + 2], facingUp);

        return null;
    }

    /// <summary>
    /// The outline extruded into a closed solid: the floor flat at the lowest point, the roof flat the height above
    /// the highest, and a wall on each edge. Two points make a rectangle along the world axes, with them as opposite
    /// corners.
    /// </summary>
    private string BuildBox(float height, bool facingUp, List<Vector3> vertices, List<int> indices)
    {
        var points = WithoutRepeats(Points, closed: true);

        if (points.Count == 2)
        {
            var a = points[0];
            var b = points[1];

            if (MathF.Abs(a.X - b.X) < MinSpacing || MathF.Abs(a.Z - b.Z) < MinSpacing)
                return LOC.Get("MAP_CollisionEditor_User_Box_Flat");

            points = [a, new Vector3(b.X, a.Y, a.Z), b, new Vector3(a.X, b.Y, b.Z)];
        }

        if (points.Count < 3)
            return LOC.Get("MAP_CollisionEditor_User_Too_Few_Points", 2);

        var floor = Points.Min(p => p.Y);
        var roof = Points.Max(p => p.Y) + height;

        // Walls face out when the outline goes clockwise seen from above (see AddQuad)
        if (SignedArea(points) > 0)
            points.Reverse();

        var triangles = Triangulate(points);

        if (triangles == null)
            return LOC.Get("MAP_CollisionEditor_User_Outline_Invalid");

        var n = points.Count;

        foreach (var point in points)
            vertices.Add(new Vector3(point.X, floor, point.Z));

        foreach (var point in points)
            vertices.Add(new Vector3(point.X, roof, point.Z));

        // Floor down, roof up
        for (var t = 0; t < triangles.Count; t += 3)
        {
            AddFacing(vertices, indices, triangles[t], triangles[t + 1], triangles[t + 2], !facingUp);
            AddFacing(vertices, indices, triangles[t] + n, triangles[t + 1] + n, triangles[t + 2] + n, facingUp);
        }

        // Walls out
        for (var i = 0; i < n; i++)
        {
            var j = (i + 1) % n;
            AddQuad(indices, i, j, j + n, i + n, !facingUp);
        }

        return null;
    }

    /// <summary>
    /// Two triangles on a wall quad from bottom a, bottom b, top b, top a. Unflipped, its face points left of a to b
    /// seen from above (the cross product of its edges is (-dz, 0, dx) for a to b of (dx, dz)).
    /// </summary>
    private static void AddQuad(List<int> indices, int a, int b, int topB, int topA, bool flip)
    {
        if (flip)
        {
            indices.AddRange([a, topB, b]);
            indices.AddRange([a, topA, topB]);
        }
        else
        {
            indices.AddRange([a, b, topB]);
            indices.AddRange([a, topB, topA]);
        }
    }

    /// <summary>
    /// Adds a triangle wound so its face (the cross product of its edges) points up, or down without up.
    /// </summary>
    private static void AddFacing(List<Vector3> vertices, List<int> indices, int a, int b, int c, bool up)
    {
        var normal = Vector3.Cross(vertices[b] - vertices[a], vertices[c] - vertices[a]);

        if (normal.Y > 0 != up)
            (b, c) = (c, b);

        indices.AddRange([a, b, c]);
    }

    /// <summary>The points with repeats next to each other dropped (and the last, if it is the first, when closed).</summary>
    private static List<Vector3> WithoutRepeats(List<Vector3> source, bool closed)
    {
        var points = new List<Vector3>();

        foreach (var point in source)
        {
            if (points.Count == 0 || HorizontalDistance(points[^1], point) >= MinSpacing)
                points.Add(point);
        }

        if (closed && points.Count > 2 && HorizontalDistance(points[^1], points[0]) < MinSpacing)
            points.RemoveAt(points.Count - 1);

        return points;
    }

    private static float HorizontalDistance(Vector3 a, Vector3 b) => new Vector2(a.X - b.X, a.Z - b.Z).Length();

    /// <summary>Twice the outline's area across the ground (X, Z): positive when it goes anticlockwise.</summary>
    private static float SignedArea(List<Vector3> points)
    {
        var area = 0f;

        for (var i = 0; i < points.Count; i++)
        {
            var a = points[i];
            var b = points[(i + 1) % points.Count];
            area += a.X * b.Z - b.X * a.Z;
        }

        return area;
    }

    /// <summary>
    /// Splits an outline into triangles across the ground by ear clipping: three indices per triangle, or null if the
    /// outline crosses itself or has no area. Corners in a straight line are dropped without a triangle.
    /// </summary>
    private static List<int> Triangulate(List<Vector3> points)
    {
        var area = SignedArea(points);

        if (MathF.Abs(area) < 1e-8f)
            return null;

        // Worked on anticlockwise, so an ear is a corner turning left
        var remaining = Enumerable.Range(0, points.Count).ToList();

        if (area < 0)
            remaining.Reverse();

        static Vector2 Flat(Vector3 v) => new(v.X, v.Z);

        static float Cross(Vector2 o, Vector2 a, Vector2 b) => (a.X - o.X) * (b.Y - o.Y) - (a.Y - o.Y) * (b.X - o.X);

        var triangles = new List<int>();
        var guard = remaining.Count * remaining.Count + 8;

        while (remaining.Count > 3 && guard-- > 0)
        {
            var clipped = false;

            for (var i = 0; i < remaining.Count; i++)
            {
                var prev = remaining[(i + remaining.Count - 1) % remaining.Count];
                var curr = remaining[i];
                var next = remaining[(i + 1) % remaining.Count];

                var a = Flat(points[prev]);
                var b = Flat(points[curr]);
                var c = Flat(points[next]);
                var turn = Cross(a, b, c);

                // A straight corner adds nothing
                if (MathF.Abs(turn) < 1e-9f)
                {
                    remaining.RemoveAt(i);
                    clipped = true;
                    break;
                }

                if (turn < 0)
                    continue;

                // An ear holds no other corner
                var empty = true;

                foreach (var other in remaining)
                {
                    if (other == prev || other == curr || other == next)
                        continue;

                    var p = Flat(points[other]);

                    if (Cross(a, b, p) >= 0 && Cross(b, c, p) >= 0 && Cross(c, a, p) >= 0)
                    {
                        empty = false;
                        break;
                    }
                }

                if (!empty)
                    continue;

                triangles.AddRange([prev, curr, next]);
                remaining.RemoveAt(i);
                clipped = true;
                break;
            }

            // No ear: the outline crosses itself
            if (!clipped)
                return null;
        }

        if (remaining.Count == 3)
        {
            var turn = Cross(Flat(points[remaining[0]]), Flat(points[remaining[1]]), Flat(points[remaining[2]]));

            if (MathF.Abs(turn) >= 1e-9f)
                triangles.AddRange(remaining);
        }

        return triangles.Count > 0 ? triangles : null;
    }

    #endregion
}

/// <summary>
/// Gizmo target for the User-Defined shape's picked point. Only the position moves; rotation and scale are ignored.
/// </summary>
public class CollisionPointGizmoTarget : IGizmoTarget
{
    private readonly CollisionEditorUserDefined Owner;

    public CollisionPointGizmoTarget(CollisionEditorUserDefined owner)
    {
        Owner = owner;
    }

    public bool HasTarget => Owner.IsShown && Owner.Selected >= 0 && Owner.Selected < Owner.Points.Count;

    public bool AllowScale => false;

    public Transform GetTransform()
    {
        return HasTarget ? new Transform(Owner.Points[Owner.Selected], Quaternion.Identity, Vector3.One) : Transform.Default;
    }

    public void Preview(Transform original, Transform current, GizmoState.GizmosMode mode)
    {
        if (mode == GizmoState.GizmosMode.Translate && HasTarget)
            Owner.SetPoint(Owner.Selected, current.Position);
    }

    public void Commit(Transform original, Transform current, GizmoState.GizmosMode mode)
    {
        Preview(original, current, mode);
    }
}
