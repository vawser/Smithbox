#nullable enable
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;
using Rectangle = Veldrid.Rectangle;

namespace StudioCore.Editors.ProjectileEditor;

/// <summary>
/// The Projectile Editor's viewport: the simulation drawn as lines in its render scene, with the bullets' effects on
/// them (<see cref="ProjectileEffects"/>). Bullets fly along +Z from above the origin, with Y up, as the simulation
/// flies them.
/// <list type="bullet">
/// <item>The scenery (the ground's grid, the wall and the target's path) is built when the settings change.</item>
/// <item>The bullets (their trails, their hit spheres and reaches, the target, the impacts) are built again as the time
/// moves, at most every <see cref="BuildInterval"/>.</item>
/// <item>The damage dealt to the target is drawn over the viewport by <see cref="DrawOverlay"/>.</item>
/// </list>
/// </summary>
public class ProjectileViewportWindow
{
    public ProjectileEditorView View;
    public ProjectEntry Project;

    public IViewport? Viewport;
    public Rectangle Rect;

    public bool ViewportUsingKeyboard;

    private const int GridRadius = 50;
    private const int MaxWallCells = 12;
    private const int TargetPathSegments = 48;

    // How long the reach of an attack stays where it landed, and how long the damage it dealt stays up
    private const float ImpactDisplayTime = 0.3f;
    private const float DamageDisplayTime = 1.0f;
    private const float DamageRiseSpeed = 30.0f;
    private const int MaxDamageLines = 8;

    // Very small bullets still need to be visible
    private const float MinSphereRadius = 0.05f;

    // A simulation can have hundreds of bullets, so the detail goes first when there are a lot of them
    private const int MaxDetailedSpheres = 64;
    private const int MaxTrailSegments = 20000;

    private static readonly TimeSpan BuildInterval = TimeSpan.FromMilliseconds(33);

    // Behind the shooter, off to the side and looking down
    private static readonly Quaternion DefaultRotation = Quaternion.CreateFromYawPitchRoll(-0.7f, 0.35f, 0.0f);
    private static readonly Vector3 DefaultFrameCenter = new(0, 1.5f, 5);
    private const float DefaultFrameRadius = 12.0f;
    private const float MinFrameRadius = 2.0f;

    private readonly ProjectileDebugLines Scenery = new();
    private readonly ProjectileDebugLines Bullets = new();

    private (SimulationSettings? Settings, bool Grid) SceneryFor;
    private (int Version, float Time, SimulationSettings? Settings, bool Trails, bool Reach) BulletsFor;
    private DateTime LastBuild;

    public ProjectileViewportWindow(ProjectileEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        var window = Smithbox.Instance._context.Window;
        Rect = window.Bounds;

        // Without a renderer the viewport is still shown, as a disabled placeholder
        if (Smithbox.Instance.HasRenderer)
            Viewport = new VulkanViewport(view.Universe, "ProjectileViewport", Rect.Width, Rect.Height, view.RenderScene);
        else
            Viewport = new NullViewport(view.Universe, "ProjectileViewport", Rect.Width, Rect.Height, view.RenderScene);

        if (Viewport is VulkanViewport vk)
        {
            vk.ViewportCamera.CameraTransform.Rotation = DefaultRotation;
            FrameSphere(DefaultFrameCenter, DefaultFrameRadius);
        }
    }

    /// <summary>The window's id, for the editor's default layout</summary>
    public const string WindowId = "###ProjectileViewport";

    /// <summary>Where the viewport camera is and which way it looks</summary>
    public (Vector3 Eye, Vector3 Forward) CameraEyeAndForward()
    {
        if (Viewport is not VulkanViewport vk)
            return (Vector3.Zero, Vector3.UnitZ);

        var transform = vk.ViewportCamera.CameraTransform;
        return (transform.Position, Vector3.Transform(Vector3.UnitZ, transform.RotationMatrix));
    }

    public void Display(uint dockspaceId)
    {
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_ProjectileEditorView);

        if (Viewport is VulkanViewport vulkanViewport)
            vulkanViewport.Display();
        else if (Viewport is NullViewport nullViewport)
            nullViewport.Display();
    }

    // ---- camera

    /// <summary>Moves the camera back along the way it is facing, until a box is in view</summary>
    public void Frame(Vector3 min, Vector3 max)
    {
        FrameSphere((min + max) * 0.5f, MathF.Max(Vector3.Distance(min, max) * 0.5f, MinFrameRadius));
    }

    /// <summary>Frames the simulation, or where it would be when there is none</summary>
    public void FrameSimulation()
    {
        if (View.Simulator.Result is { } result)
            Frame(result.BoundsMin, result.BoundsMax);
        else
            FrameSphere(DefaultFrameCenter, DefaultFrameRadius);
    }

    private void FrameSphere(Vector3 center, float radius)
    {
        if (Viewport is not VulkanViewport vk)
            return;

        var camera = vk.ViewportCamera;
        var forward = Vector3.Transform(Vector3.UnitZ, camera.CameraTransform.Rotation);
        var fov = float.DegreesToRadians(camera.FovOverrideDegrees ?? CFG.Current.Viewport_Camera_FOV);

        camera.CameraTransform.Position = center - forward * (radius / MathF.Sin(fov * 0.5f));
    }

    // ---- update

    public void Update(float deltatime)
    {
        if (Viewport != null)
            ViewportUsingKeyboard = Viewport.Update(Smithbox.Instance._context.Window, deltatime);

        if (View.RenderScene != null)
        {
            UpdateScenery();
            UpdateBullets();
        }

        Scenery.Update();
        Bullets.Update();
    }

    private void UpdateScenery()
    {
        if (Scenery.IsUpdating)
            return;

        var settings = View.Simulator.Settings;
        var key = (settings, CFG.Current.ProjectileEditor_Display_Ground_Grid);

        if (key == SceneryFor)
            return;

        SceneryFor = key;

        var lines = new Dictionary<Color, List<(Vector3, Vector3)>>();

        if (CFG.Current.ProjectileEditor_Display_Ground_Grid)
        {
            var minor = Lines(lines, Color.FromArgb(255, 70, 70, 70));
            var major = Lines(lines, Color.FromArgb(255, 120, 120, 120));
            var axis = Lines(lines, Color.FromArgb(255, 160, 160, 200));

            for (var i = -GridRadius; i <= GridRadius; i++)
            {
                var list = i == 0 ? axis : i % 5 == 0 ? major : minor;

                list.Add((new Vector3(i, 0, -GridRadius), new Vector3(i, 0, GridRadius)));
                list.Add((new Vector3(-GridRadius, 0, i), new Vector3(GridRadius, 0, i)));
            }
        }

        if (settings.SimulateWall)
            AddWall(Lines(lines, ProjectileDebugLines.ToColor(ProjectileFormat.WallColor)), settings);

        if (settings.SimulateTarget && settings.IsTargetMoving)
            AddTargetPath(Lines(lines, ProjectileDebugLines.ToColor(Dim(ProjectileFormat.TargetColor, 0.5f))), settings);

        Scenery.Show(View.RenderScene!, lines);
    }

    private void UpdateBullets()
    {
        if (Bullets.IsUpdating)
            return;

        var simulator = View.Simulator;
        var settings = simulator.Settings;
        var key = (simulator.Version, simulator.Time, settings, CFG.Current.ProjectileEditor_Display_Trails,
            CFG.Current.ProjectileEditor_Display_Reach);

        if (key == BulletsFor)
            return;

        // Another simulation shows at once; a moving time at most every BuildInterval
        if (simulator.Version == BulletsFor.Version && DateTime.UtcNow - LastBuild < BuildInterval)
            return;

        BulletsFor = key;
        LastBuild = DateTime.UtcNow;

        var lines = new Dictionary<Color, List<(Vector3, Vector3)>>();
        var time = simulator.Time;

        if (settings.SimulateTarget)
        {
            var target = Lines(lines, ProjectileDebugLines.ToColor(ProjectileFormat.TargetColor));
            var position = settings.GetTargetPosition(time);

            // The line down to the ground is there to show where the target is, which is hard to tell from one angle
            target.Add((position, new Vector3(position.X, 0, position.Z)));
            ProjectileDebugLines.AddSphere(target, position, settings.TargetRadius, 24);
        }

        if (simulator.Result is { } result)
        {
            if (CFG.Current.ProjectileEditor_Display_Trails)
                AddTrails(lines, result, time);

            AddProjectiles(lines, result, time);
            AddImpacts(lines, result, time);
        }

        Bullets.Show(View.RenderScene!, lines);
    }

    private static List<(Vector3, Vector3)> Lines(Dictionary<Color, List<(Vector3, Vector3)>> lines, Color color)
    {
        if (!lines.TryGetValue(color, out var list))
        {
            list = new List<(Vector3, Vector3)>();
            lines[color] = list;
        }

        return list;
    }

    private static Vector4 Dim(Vector4 color, float amount) =>
        new(color.X * amount, color.Y * amount, color.Z * amount, color.W);

    /// <summary>The wall, as the outlines of its cells</summary>
    private static void AddWall(List<(Vector3, Vector3)> lines, SimulationSettings settings)
    {
        var columns = Math.Clamp((int)MathF.Ceiling(settings.WallWidth), 1, MaxWallCells);
        var rows = Math.Clamp((int)MathF.Ceiling(settings.WallHeight), 1, MaxWallCells);

        var left = -settings.WallWidth / 2;
        var z = settings.WallDistance;

        for (var x = 0; x <= columns; x++)
        {
            var px = left + settings.WallWidth * x / columns;
            lines.Add((new Vector3(px, 0, z), new Vector3(px, settings.WallHeight, z)));
        }

        for (var y = 0; y <= rows; y++)
        {
            var py = settings.WallHeight * y / rows;
            lines.Add((new Vector3(left, py, z), new Vector3(-left, py, z)));
        }
    }

    private static void AddTargetPath(List<(Vector3, Vector3)> lines, SimulationSettings settings)
    {
        var previous = settings.GetTargetPosition(0);

        for (var i = 1; i <= TargetPathSegments; i++)
        {
            var position = settings.GetTargetPosition(settings.TargetPeriod * i / TargetPathSegments);

            lines.Add((previous, position));
            previous = position;
        }
    }

    private static void AddTrails(Dictionary<Color, List<(Vector3, Vector3)>> lines, SimulationResult result, float time)
    {
        var total = 0;
        foreach (var track in result.Tracks)
            total += track.Samples.Count;

        var stride = Math.Max(1, total / MaxTrailSegments + 1);

        foreach (var track in result.Tracks)
        {
            if (time < track.SpawnTime || track.Samples.Count == 0)
                continue;

            // Bullets that have ended leave a darker trail
            var color = ProjectileFormat.GetColor(track.Generation);
            var list = Lines(lines, ProjectileDebugLines.ToColor(track.IsActive(time) ? color : Dim(color, 0.5f)));

            var previous = track.Samples[0].Position;

            for (var i = stride; i < track.Samples.Count && track.Samples[i].Time <= time; i += stride)
            {
                var position = track.Samples[i].Position;

                list.Add((previous, position));
                previous = position;
            }

            list.Add((previous, track.GetPosition(Math.Min(time, track.EndTime))));
        }
    }

    private static void AddProjectiles(Dictionary<Color, List<(Vector3, Vector3)>> lines, SimulationResult result, float time)
    {
        var active = 0;
        foreach (var track in result.Tracks)
        {
            if (track.IsActive(time))
                active++;
        }

        var segments = active <= MaxDetailedSpheres ? 16 : 6;
        var reach = Lines(lines, ProjectileDebugLines.ToColor(Dim(ProjectileFormat.ReachColor, 0.8f)));

        foreach (var track in result.Tracks)
        {
            if (!track.IsActive(time))
                continue;

            var position = track.GetPosition(time);
            var radius = MathF.Max(track.GetRadius(time), MinSphereRadius);

            ProjectileDebugLines.AddSphere(Lines(lines, ProjectileDebugLines.ToColor(ProjectileFormat.GetColor(track.Generation))),
                position, radius, segments);

            var attackReach = track.Attack?.Radius ?? 0;

            if (CFG.Current.ProjectileEditor_Display_Reach && attackReach > radius)
            {
                ProjectileDebugLines.AddCircle(reach, position, Vector3.UnitX, Vector3.UnitZ, attackReach, segments * 2);
            }
        }
    }

    /// <summary>Leaves the reach of an attack behind for a moment where a bullet landed</summary>
    private static void AddImpacts(Dictionary<Color, List<(Vector3, Vector3)>> lines, SimulationResult result, float time)
    {
        var list = Lines(lines, ProjectileDebugLines.ToColor(ProjectileFormat.ReachColor));

        foreach (var track in result.Tracks)
        {
            if (track.End is ProjectileEnd.Expired or ProjectileEnd.TimeLimit || time <= track.EndTime ||
                time > track.EndTime + ImpactDisplayTime || track.Samples.Count == 0)
            {
                continue;
            }

            var radius = MathF.Max(track.Attack?.Radius ?? 0, track.GetRadius(track.EndTime));

            ProjectileDebugLines.AddSphere(list, track.Samples[^1].Position, MathF.Max(radius, MinSphereRadius), 16);
        }
    }

    // ---- overlay

    /// <summary>
    /// The damage dealt to the target so far, at the top of the viewport, and each hit rising off the target as it
    /// lands. Called by the viewport's overlay, inside its window.
    /// </summary>
    public void DrawOverlay(VulkanViewport parent)
    {
        var simulator = View.Simulator;
        var settings = simulator.Settings;

        if (simulator.Result is not { } result || !settings.SimulateTarget || !CFG.Current.ProjectileEditor_Display_Damage)
            return;

        var time = simulator.Time;
        var projectType = Project.Descriptor.ProjectType;
        var color = ImGui.GetColorU32(ProjectileFormat.DamageColor);
        var shadow = ImGui.GetColorU32(new Vector4(0, 0, 0, 1));

        // Percentages can't be added up, as each one applies to its own attack
        AttackDamage total = default;
        var hits = 0;

        foreach (var hit in result.Damage)
        {
            if (hit.Time <= time)
            {
                total += hit.Attack.Flat;
                hits++;
            }
        }

        if (hits > 0)
        {
            ImGui.TextColored(ProjectileFormat.DamageColor,
                LOC.Get("PROJECTILE_Damage_Total", hits, ProjectileFormat.FormatDamage(total, default, projectType)));
        }

        var anchor = parent.WorldToScreen(settings.GetTargetPosition(time) + Vector3.UnitY * settings.TargetRadius, out var inFront);

        if (!inFront)
            return;

        var drawList = ImGui.GetWindowDrawList();
        var origin = new Vector2(parent.X, parent.Y) + anchor;
        var lineHeight = ImGui.GetTextLineHeight();
        var lines = 0;

        for (var i = result.Damage.Count - 1; i >= 0 && lines < MaxDamageLines; i--)
        {
            var hit = result.Damage[i];

            if (hit.Time > time || time > hit.Time + DamageDisplayTime)
                continue;

            var height = (time - hit.Time) * DamageRiseSpeed + lines * lineHeight;
            var position = origin - new Vector2(0, height + lineHeight);
            var text = ProjectileFormat.FormatDamage(hit.Attack.Flat, hit.Attack.Correction, projectType);

            drawList.AddText(position + Vector2.One, shadow, text);
            drawList.AddText(position, color, text);

            lines++;
        }
    }

    // ---- frame

    public void EditorResized(Sdl2Window window, GraphicsDevice device)
    {
        Rect = window.Bounds;
    }

    public void Draw(GraphicsDevice device, CommandList cl)
    {
        Viewport?.Draw(device, cl);
    }

    public bool InputCaptured()
    {
        return Viewport != null && Viewport.IsViewportSelected;
    }

    /// <summary>Drops the lines (the scene is being disposed or shown again)</summary>
    public void ClearLines()
    {
        Scenery.Clear();
        Bullets.Clear();
        SceneryFor = default;
        BulletsFor = default;
    }

    public void Dispose()
    {
        ClearLines();
        (Viewport as VulkanViewport)?.Dispose();
    }
}
