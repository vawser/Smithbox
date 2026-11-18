using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;
using StudioCore.Keybinds;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The Region Icons viewport filter: while it is on, an unselected region is drawn as an icon at its centre instead
/// of its wireframe (see <see cref="Entity.ShowAsIcon"/>), and clicking the icon selects the region as clicking the
/// wireframe would. Once selected, the wireframe is drawn again and the icon is not.
/// </summary>
public class RegionIconTool
{
    private readonly MapEditorView View;
    private readonly ProjectEntry Project;

    private const string Icon = Icons.Bullseye;

    private struct RegionIcon
    {
        public Entity Entity;
        public Vector2 Screen;
        public float Distance;
        public uint Color;
    }

    // The icons on screen, nearest last, so they are drawn over the farther ones and hit tested first
    private readonly List<RegionIcon> IconList = new();
    private int IconFrame = -1;
    private VulkanViewport IconViewport;

    private Entity Hovered;
    private VulkanViewport HoveredViewport;

    // A click that began on an icon: the release selects, if it is still on the same icon
    private Entity Pressed;

    private static float IconSize => 18.0f * DPI.UIScale();
    private static float HitRadius => IconSize * 0.6f;

    public RegionIconTool(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    private static bool IsEnabled => CFG.Current.MapEditor_Region_Icons;

    /// <summary>
    /// Turns the icons on or off: the wireframes of the unselected regions are hidden or shown again.
    /// </summary>
    public void SetEnabled(bool enabled)
    {
        CFG.Current.MapEditor_Region_Icons = enabled;

        foreach (var wrapper in Project.Handler.MapData.PrimaryBank.Maps.Values)
        {
            var map = wrapper.MapContainer;

            if (map == null || map.LoadState is not MapContentLoadState.Loaded)
                continue;

            foreach (var entity in map.Objects)
            {
                if (entity is MsbEntity { Type: MsbEntityType.Region or MsbEntityType.DS2EventLocation })
                {
                    entity.RefreshRenderMeshVisibility();
                }
            }
        }

        Hovered = null;
        Pressed = null;
        IconFrame = -1;

        View.DelayPicking();
    }

    /// <summary>
    /// What a viewport's depth test has said so far: whether each icon tested was in front of the scene.
    /// </summary>
    private class OcclusionState
    {
        public IconOcclusionResult Applied;
        public readonly Dictionary<Entity, bool> Visible = new();
    }

    private readonly Dictionary<VulkanViewport, OcclusionState> Occlusion = new();

    /// <summary>
    /// The icons of the unselected regions visible in the viewport, found once a frame.
    /// </summary>
    private List<RegionIcon> GetIcons(VulkanViewport viewport)
    {
        var frame = ImGui.GetFrameCount();

        if (IconFrame == frame && IconViewport == viewport)
            return IconList;

        IconFrame = frame;
        IconViewport = viewport;
        IconList.Clear();

        var scene = viewport.RenderScene;

        if (!IsEnabled || scene == null || !scene.DrawFilter.HasFlag(RenderFilter.Region))
            return IconList;

        var cfg = CFG.Current;
        var camera = viewport.ViewportCamera.CameraTransform.Position;
        var maxDistance = cfg.MapEditor_Region_Icon_Max_Distance;
        var maxDistanceSquared = maxDistance * maxDistance;
        var color = ImGui.GetColorU32(new Vector4(cfg.MapEditor_Region_Icon_Color / 255f, 1f));

        var occlusion = cfg.MapEditor_Region_Icon_Occlusion ? viewport.ViewPipeline?.IconOcclusion : null;
        var viewProjection = viewport.ViewportCamera.CameraTransform.CameraViewMatrixLH * viewport.ViewportCamera.ProjectionMatrix;
        var zeroToOne = viewport.Device.IsDepthRangeZeroToOne;

        // Each icon on screen is tested against the scene, and shown as the last test that reached it said
        var tested = occlusion != null ? new List<Entity>() : null;
        var points = occlusion != null ? new List<Vector4>() : null;
        var state = occlusion != null ? GetOcclusionState(viewport, occlusion) : null;

        foreach (var wrapper in Project.Handler.MapData.PrimaryBank.Maps.Values)
        {
            var map = wrapper.MapContainer;

            if (map == null || map.LoadState is not MapContentLoadState.Loaded)
                continue;

            // A map is drawn in the scene of the view that loaded it
            if (map.OwnerView != null && map.OwnerView != View)
                continue;

            foreach (var entity in map.Objects)
            {
                if (!entity.ShowAsIcon || entity.IsSelected || !entity.EditorVisible)
                    continue;

                var mesh = entity.RenderSceneMesh;

                // Hidden by the region type filters or the display groups, as the wireframe would be
                if (mesh == null || (mesh.DrawFilter & scene.DrawFilter) == 0 || !mesh.DrawGroups.IsInDisplayGroup(scene.DisplayGroup))
                    continue;

                // The centre of the shape: a box region's origin is at its base
                var centre = Vector3.Transform(mesh.GetLocalBounds().GetCenter(), mesh.World);

                var distanceSquared = Vector3.DistanceSquared(camera, centre);

                if (distanceSquared > maxDistanceSquared)
                    continue;

                var screen = viewport.WorldToScreen(centre, out bool inFront);

                if (!inFront || screen.X < 0 || screen.Y < 0 || screen.X > viewport.Width || screen.Y > viewport.Height)
                    continue;

                if (occlusion != null && tested.Count < IconOcclusionPass.MaxPoints &&
                    GetWindowDepth(viewProjection, zeroToOne, camera, centre, distanceSquared) is float depth)
                {
                    tested.Add(entity);
                    points.Add(new Vector4(screen.X / viewport.Width, screen.Y / viewport.Height, depth, HitRadius * 0.5f));

                    // Not shown until a test has found it in front of the scene, so it doesn't flash up behind a wall
                    if (!state.Visible.TryGetValue(entity, out var visible) || !visible)
                        continue;
                }

                IconList.Add(new RegionIcon
                {
                    Entity = entity,
                    Screen = screen,
                    Distance = distanceSquared,
                    Color = color
                });
            }
        }

        if (occlusion != null)
        {
            occlusion.Request(tested.ToArray(), CollectionsMarshal.AsSpan(points));
        }

        IconList.Sort((a, b) => b.Distance.CompareTo(a.Distance));

        return IconList;
    }

    /// <summary>
    /// The viewport's test results, with the latest one read back applied.
    /// </summary>
    private OcclusionState GetOcclusionState(VulkanViewport viewport, IconOcclusionPass pass)
    {
        if (!Occlusion.TryGetValue(viewport, out var state))
        {
            state = new OcclusionState();
            Occlusion[viewport] = state;
        }

        var result = pass.LatestResult;

        if (result != null && result != state.Applied && result.Tag is Entity[] entities)
        {
            state.Applied = result;

            // Only the icons that were on screen are kept, so the table doesn't grow with each map visited
            state.Visible.Clear();

            for (var i = 0; i < entities.Length && i < result.Visible.Length; i++)
            {
                state.Visible[entities[i]] = result.Visible[i];
            }
        }

        return state;
    }

    /// <summary>
    /// The window depth of a point, as the scene's depth holds it (reversed), pulled a little toward the camera so
    /// that a region at ground level isn't hidden by the ground it sits on.
    /// </summary>
    private static float? GetWindowDepth(Matrix4x4 viewProjection, bool zeroToOne, Vector3 camera, Vector3 point, float distanceSquared)
    {
        var distance = MathF.Sqrt(distanceSquared);

        if (distance > 1e-4f)
        {
            var pull = Math.Clamp(distance * 0.02f, 0.05f, 1.0f);
            point += (camera - point) / distance * MathF.Min(pull, distance * 0.5f);
        }

        var clip = Vector4.Transform(new Vector4(point, 1.0f), viewProjection);

        if (MathF.Abs(clip.W) < 1e-8f)
            return null;

        var z = clip.Z / clip.W;
        var depth = zeroToOne ? z : z * 0.5f + 0.5f;

        return float.IsFinite(depth) ? depth : null;
    }

    /// <summary>
    /// The nearest icon under the mouse, if any.
    /// </summary>
    private Entity GetIconAt(VulkanViewport viewport, Vector2 screen)
    {
        var icons = GetIcons(viewport);
        var radius = HitRadius;

        for (var i = icons.Count - 1; i >= 0; i--)
        {
            if (Vector2.Distance(icons[i].Screen, screen) <= radius)
                return icons[i].Entity;
        }

        return null;
    }

    /// <summary>
    /// Selects a region when its icon is clicked. Called each frame by the active viewport.
    /// Returns true while a click on an icon is held, so the viewport doesn't also pick under it.
    /// </summary>
    public bool HandleViewportInput(VulkanViewport viewport, bool canPick)
    {
        if (!IsEnabled)
        {
            Hovered = null;
            Pressed = null;
            return false;
        }

        var mouse = InputManager.MousePosition;
        var screen = new Vector2(mouse.X - viewport.X, mouse.Y - viewport.Y);

        Hovered = canPick || Pressed != null ? GetIconAt(viewport, screen) : null;
        HoveredViewport = viewport;

        if (Pressed != null)
        {
            if (InputManager.IsMouseReleased(MousebindID.Viewport_Picking_Action))
            {
                if (Hovered == Pressed)
                {
                    Select(viewport, Pressed);
                }

                Pressed = null;
                return true;
            }

            if (!InputManager.IsMouseDown(MousebindID.Viewport_Picking_Action))
            {
                Pressed = null;
            }

            return true;
        }

        if (canPick && Hovered != null && InputManager.IsMousePressed(MousebindID.Viewport_Picking_Action))
        {
            Pressed = Hovered;
            return true;
        }

        return false;
    }

    /// <summary>
    /// The same selection as clicking the region's wireframe: Ctrl toggles it, Shift adds it, otherwise it alone is
    /// selected.
    /// </summary>
    private static void Select(VulkanViewport viewport, Entity entity)
    {
        var selection = viewport.ViewportSelection;

        if (InputManager.HasCtrlDown())
        {
            if (selection.GetSelection().Contains(entity))
                selection.RemoveSelection(entity);
            else
                selection.AddSelection(entity);
        }
        else if (InputManager.HasShiftDown())
        {
            selection.AddSelection(entity);
        }
        else
        {
            selection.ClearSelection();
            selection.AddSelection(entity);
        }
    }

    /// <summary>
    /// The icons, with the hovered one highlighted and named. Called by the viewport's overlay.
    /// </summary>
    public unsafe void DrawOverlay(VulkanViewport viewport)
    {
        if (!IsEnabled || viewport.Owner is not MapUniverse)
            return;

        var icons = GetIcons(viewport);

        if (icons.Count == 0)
            return;

        var drawList = ImGui.GetWindowDrawList();
        var font = ImGui.GetFont();
        var size = IconSize;
        var origin = new Vector2(viewport.X, viewport.Y);
        var shadow = ImGui.GetColorU32(new Vector4(0, 0, 0, 0.9f));
        var highlight = ImGui.GetColorU32(new Vector4(1, 1, 1, 1));
        var hovered = HoveredViewport == viewport ? Hovered : null;

        var half = ImGui.CalcTextSize(Icon) * (size / ImGui.GetFontSize()) / 2;

        foreach (var icon in icons)
        {
            var isHovered = icon.Entity == hovered;
            var at = origin + icon.Screen - half;

            drawList.AddText(font, size, at + Vector2.One, shadow, Icon);
            drawList.AddText(font, size, at, isHovered ? highlight : icon.Color, Icon);
        }

        if (hovered != null)
        {
            foreach (var icon in icons)
            {
                if (icon.Entity != hovered)
                    continue;

                var text = hovered.PrettyName ?? "";
                var textAt = origin + icon.Screen + new Vector2(half.X + 4 * DPI.UIScale(), -ImGui.GetFontSize() / 2);

                drawList.AddText(textAt + Vector2.One, shadow, text);
                drawList.AddText(textAt, highlight, text);
                break;
            }
        }
    }
}
