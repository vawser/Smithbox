using Hexa.NET.ImGui;
using StudioCore.Editors.Viewport;
using System;
using System.Numerics;
using Veldrid;

namespace StudioCore.Editors.ParamEditor;

/// <summary>
///     Draws a simulation as 3D shapes projected into the window's draw list. The param editor has no scene to
///     render into, and this doesn't need one: nothing here outlives the frame, however quickly the trails grow.
///     Shapes are sorted from back to front, as there is no depth buffer.
/// </summary>
public sealed class ProjectileViewport
{
    private const float NearPlane = 0.05f;

    private const float MinFrameRadius = 2.0f;

    private const int GridRadius = 25;
    private const int MaxWallCells = 12;

    private const int SphereRings = 6;
    private const int SphereSegments = 10;

    // Very small bullets still need to be visible
    private const float MinScreenRadius = 2.0f;
    private const float MinWireRadius = 7.0f;

    // Draw lists are limited in how many vertices they can hold, and the window's own contents share it.
    // A simulation can have hundreds of bullets, so the detail goes first when there are a lot of them.
    private const int MaxDetailedSpheres = 48;
    private const int MaxTrailSegments = 4000;

    private static readonly Vector4[] GenerationColors =
    {
        new(1.00f, 0.75f, 0.25f, 1.0f),
        new(0.35f, 0.75f, 1.00f, 1.0f),
        new(0.45f, 1.00f, 0.50f, 1.0f),
        new(1.00f, 0.45f, 0.65f, 1.0f),
        new(0.75f, 0.55f, 1.00f, 1.0f),
        new(1.00f, 0.55f, 0.30f, 1.0f)
    };

    private static readonly Vector4 TargetColor = new(1.00f, 0.25f, 0.25f, 1.0f);
    private static readonly Vector4 ReachColor = new(1.00f, 1.00f, 1.00f, 0.6f);
    private static readonly Vector4 DamageColor = new(1.00f, 0.90f, 0.35f, 1.0f);

    // How long the reach of an attack stays where it landed, and how long the damage it dealt stays up
    private const float ImpactDisplayTime = 0.3f;
    private const float DamageDisplayTime = 1.0f;

    private const float DefaultCorrection = 100.0f;

    private const float DamageRiseSpeed = 30.0f;
    private const int MaxDamageLines = 8;
    private const float OverlayMargin = 8.0f;

    private static readonly Vector3[,] SpherePoints = CreateSpherePoints();

    // Behind the shooter, off to the side and looking down
    private static readonly Quaternion DefaultRotation = Quaternion.CreateFromYawPitchRoll(-0.7f, 0.35f, 0.0f);

    private static readonly Vector3 DefaultFrameCenter = new(0, 1.5f, 5);
    private const float DefaultFrameRadius = 12.0f;

    private readonly List<DrawItem> Items = new();

    // The same camera as the other viewports, so the controls and their settings are the same too.
    // It only uses a viewport to change projection, and this one always stays in perspective.
    private readonly ViewportCamera Camera = new(null, new Rectangle(0, 0, 1, 1));

    private int LastInputFrame = -1;

    private Vector3 Eye;
    private Vector3 Forward;
    private Vector3 Right;
    private Vector3 Up;

    private Vector2 Center;
    private float Focal;

    private enum ItemKind
    {
        Line,
        Quad,
        Sphere,
        Reach
    }

    private struct DrawItem
    {
        public ItemKind Kind;
        public float Depth;
        public Vector4 Color;
        public Vector2 A, B, C, D;
        public Vector3 Position;
        public float Radius;
    }

    public ProjectileViewport()
    {
        Camera.CameraTransform.Rotation = DefaultRotation;

        FrameSphere(DefaultFrameCenter, DefaultFrameRadius);
    }

    /// <summary>
    ///     True while a mouse button is held over the preview, which is when the movement keys are in use.
    /// </summary>
    public bool IsUsingCamera { get; private set; }

    /// <summary>
    ///     True while the cursor is captured to turn the camera.
    /// </summary>
    public bool IsLooking => Camera.RightMousePressed;

    private static float FieldOfView => float.DegreesToRadians(CFG.Current.Viewport_Camera_FOV);

    public static Vector4 GetColor(int generation)
    {
        return GenerationColors[generation % GenerationColors.Length];
    }

    /// <summary>
    ///     Moves the camera back along the way it is facing, until a box is in view.
    /// </summary>
    public void Frame(Vector3 min, Vector3 max)
    {
        FrameSphere((min + max) * 0.5f, MathF.Max(Vector3.Distance(min, max) * 0.5f, MinFrameRadius));
    }

    private void FrameSphere(Vector3 center, float radius)
    {
        Vector3 forward = Vector3.Transform(Vector3.UnitZ, Camera.CameraTransform.Rotation);

        Camera.CameraTransform.Position = center - forward * (radius / MathF.Sin(FieldOfView * 0.5f));
    }

    public void Display(string id, Vector2 size, SimulationResult result, SimulationSettings settings, float time)
    {
        Vector2 min = ImGui.GetCursorScreenPos();
        Vector2 max = min + size;

        ImGui.InvisibleButton(id, size,
            ImGuiButtonFlags.MouseButtonLeft | ImGuiButtonFlags.MouseButtonRight | ImGuiButtonFlags.MouseButtonMiddle);

        HandleInput(min, size);
        UpdateCamera(min, size);

        ImDrawListPtr drawList = ImGui.GetWindowDrawList();

        drawList.PushClipRect(min, max, true);

        Vector3 background = CFG.Current.Viewport_Bg_Color;

        drawList.AddRectFilled(min, max, ImGui.GetColorU32(new Vector4(background, 1.0f)));

        DrawGround(drawList);

        Items.Clear();

        if (settings.SimulateWall)
        {
            AddWall(settings);
        }

        if (settings.SimulateTarget)
        {
            AddTarget(settings);
        }

        if (result != null)
        {
            AddTrails(result, time);
            AddProjectiles(result, time);
            AddImpacts(result, time);
        }

        Items.Sort((a, b) => b.Depth.CompareTo(a.Depth));

        var detailed = Items.Count(item => item.Kind is ItemKind.Sphere) <= MaxDetailedSpheres;

        foreach (DrawItem item in Items)
        {
            DrawItemTo(drawList, item, detailed);
        }

        if (result != null && settings.SimulateTarget)
        {
            DrawDamage(drawList, min, result, settings, time);
        }

        drawList.AddRect(min, max, ImGui.GetColorU32(ImGuiCol.Border));

        drawList.PopClipRect();
    }

    private void HandleInput(Vector2 min, Vector2 size)
    {
        Camera.UpdateBounds(new Rectangle((int)min.X, (int)min.Y, (int)size.X, (int)size.Y));

        // The preview can be on screen twice, but the camera should only move once a frame.
        // Once the cursor is captured it has to keep going until the button is let go of, to give the cursor back.
        var frame = ImGui.GetFrameCount();

        if (frame == LastInputFrame)
        {
            return;
        }

        IsUsingCamera = false;

        if (ImGui.IsItemHovered() || ImGui.IsItemActive() || Camera.RightMousePressed)
        {
            LastInputFrame = frame;

            IsUsingCamera = Camera.UpdateInput(Smithbox.Instance._context.Window, ImGui.GetIO().DeltaTime);
        }
    }

    private void UpdateCamera(Vector2 min, Vector2 size)
    {
        Matrix4x4 view = Camera.CameraTransform.CameraViewMatrixLH;

        Right = new Vector3(view.M11, view.M21, view.M31);
        Up = new Vector3(view.M12, view.M22, view.M32);
        Forward = new Vector3(view.M13, view.M23, view.M33);
        Eye = Camera.CameraTransform.Position;

        Center = min + size * 0.5f;
        Focal = size.Y * 0.5f / MathF.Tan(FieldOfView * 0.5f);
    }

    private float GetDepth(Vector3 position)
    {
        return Vector3.Dot(position - Eye, Forward);
    }

    /// <returns>False if the position is behind the camera.</returns>
    private bool Project(Vector3 position, out Vector2 screen, out float depth)
    {
        Vector3 offset = position - Eye;

        depth = Vector3.Dot(offset, Forward);
        screen = default;

        if (depth < NearPlane)
        {
            return false;
        }

        var scale = Focal / depth;

        screen = Center + new Vector2(Vector3.Dot(offset, Right), -Vector3.Dot(offset, Up)) * scale;

        return true;
    }

    private void DrawGround(ImDrawListPtr drawList)
    {
        var minor = ImGui.GetColorU32(new Vector4(1, 1, 1, 0.08f));
        var major = ImGui.GetColorU32(new Vector4(1, 1, 1, 0.2f));

        for (var i = -GridRadius; i <= GridRadius; i++)
        {
            var color = i % 5 == 0 ? major : minor;

            DrawLine(drawList, new Vector3(i, 0, -GridRadius), new Vector3(i, 0, GridRadius), color);
            DrawLine(drawList, new Vector3(-GridRadius, 0, i), new Vector3(GridRadius, 0, i), color);
        }
    }

    private void DrawLine(ImDrawListPtr drawList, Vector3 from, Vector3 to, uint color)
    {
        if (ClipToCamera(ref from, ref to) && Project(from, out Vector2 a, out _) && Project(to, out Vector2 b, out _))
        {
            drawList.AddLine(a, b, color);
        }
    }

    /// <summary>
    ///     Cuts off the part of a line that is behind the camera.
    /// </summary>
    private bool ClipToCamera(ref Vector3 from, ref Vector3 to)
    {
        var fromDepth = GetDepth(from);
        var toDepth = GetDepth(to);

        if (fromDepth < NearPlane && toDepth < NearPlane)
        {
            return false;
        }

        if (fromDepth < NearPlane)
        {
            from = Vector3.Lerp(from, to, (NearPlane - fromDepth) / (toDepth - fromDepth));
        }
        else if (toDepth < NearPlane)
        {
            to = Vector3.Lerp(to, from, (NearPlane - toDepth) / (fromDepth - toDepth));
        }

        return true;
    }

    private void AddLine(Vector3 from, Vector3 to, Vector4 color)
    {
        if (ClipToCamera(ref from, ref to) && Project(from, out Vector2 a, out var depthA) &&
            Project(to, out Vector2 b, out var depthB))
        {
            Items.Add(new DrawItem { Kind = ItemKind.Line, Depth = (depthA + depthB) * 0.5f, A = a, B = b, Color = color });
        }
    }

    /// <summary>
    ///     The wall is made up of cells, so that bullets can be sorted to either side of it.
    /// </summary>
    private void AddWall(SimulationSettings settings)
    {
        var columns = Math.Clamp((int)MathF.Ceiling(settings.WallWidth), 1, MaxWallCells);
        var rows = Math.Clamp((int)MathF.Ceiling(settings.WallHeight), 1, MaxWallCells);

        Vector3 origin = new(-settings.WallWidth / 2, 0, settings.WallDistance);
        Vector3 across = Vector3.UnitX * (settings.WallWidth / columns);
        Vector3 down = Vector3.UnitY * (settings.WallHeight / rows);

        Vector4 color = new(0.75f, 0.75f, 0.85f, 1.0f);

        for (var x = 0; x < columns; x++)
        {
            for (var y = 0; y < rows; y++)
            {
                Vector3 corner = origin + across * x + down * y;

                if (Project(corner, out Vector2 a, out _) && Project(corner + across, out Vector2 b, out _) &&
                    Project(corner + across + down, out Vector2 c, out _) && Project(corner + down, out Vector2 d, out _))
                {
                    Items.Add(new DrawItem
                    {
                        Kind = ItemKind.Quad,
                        Depth = GetDepth(corner + (across + down) * 0.5f),
                        A = a, B = b, C = c, D = d,
                        Color = color
                    });
                }
            }
        }
    }

    /// <summary>
    ///     The line down to the ground is there to show where the target is, which is hard to tell from one angle.
    /// </summary>
    private void AddTarget(SimulationSettings settings)
    {
        Vector3 position = settings.TargetPosition;

        AddLine(position, new Vector3(position.X, 0, position.Z), TargetColor);

        Items.Add(new DrawItem
        {
            Kind = ItemKind.Sphere,
            Depth = GetDepth(position),
            Position = position,
            Radius = settings.TargetRadius,
            Color = TargetColor
        });
    }

    private void AddTrails(SimulationResult result, float time)
    {
        var total = result.Tracks.Sum(track => track.Samples.Count);
        var stride = Math.Max(1, total / MaxTrailSegments + 1);

        foreach (ProjectileTrack track in result.Tracks)
        {
            if (time < track.SpawnTime)
            {
                continue;
            }

            Vector4 color = GetColor(track.Generation);

            color.W = track.IsActive(time) ? 0.9f : 0.45f;

            Vector3 previous = track.Samples[0].Position;

            for (var i = stride; i < track.Samples.Count && track.Samples[i].Time <= time; i += stride)
            {
                Vector3 position = track.Samples[i].Position;

                AddLine(previous, position, color);

                previous = position;
            }

            AddLine(previous, track.GetPosition(time), color);
        }
    }

    private void AddProjectiles(SimulationResult result, float time)
    {
        foreach (ProjectileTrack track in result.Tracks)
        {
            if (!track.IsActive(time))
            {
                continue;
            }

            Vector3 position = track.GetPosition(time);

            Items.Add(new DrawItem
            {
                Kind = ItemKind.Sphere,
                Depth = GetDepth(position),
                Position = position,
                Radius = track.GetRadius(time),
                Color = GetColor(track.Generation)
            });

            var reach = track.Attack?.Radius ?? 0;

            if (reach > track.GetRadius(time))
            {
                Items.Add(new DrawItem
                {
                    Kind = ItemKind.Reach,
                    Depth = GetDepth(position),
                    Position = position,
                    Radius = reach,
                    Color = ReachColor
                });
            }
        }
    }

    /// <summary>
    ///     Leaves the reach of an attack behind for a moment where a bullet landed.
    /// </summary>
    private void AddImpacts(SimulationResult result, float time)
    {
        foreach (ProjectileTrack track in result.Tracks)
        {
            if (track.End is ProjectileEnd.Expired or ProjectileEnd.TimeLimit || time <= track.EndTime ||
                time > track.EndTime + ImpactDisplayTime)
            {
                continue;
            }

            var radius = MathF.Max(track.Attack?.Radius ?? 0, track.GetRadius(track.EndTime));
            Vector3 position = track.Samples[^1].Position;

            Items.Add(new DrawItem
            {
                Kind = ItemKind.Sphere,
                Depth = GetDepth(position),
                Position = position,
                Radius = radius,
                Color = ReachColor
            });
        }
    }

    private void DrawDamage(ImDrawListPtr drawList, Vector2 min, SimulationResult result, SimulationSettings settings,
        float time)
    {
        var color = ImGui.GetColorU32(DamageColor);

        // Percentages can't be added up, as each one applies to its own attack
        AttackDamage total = default;
        var hits = 0;

        foreach (DamageEvent hit in result.Damage)
        {
            if (hit.Time <= time)
            {
                total += hit.Attack.Flat;
                hits++;
            }
        }

        if (hits > 0)
        {
            drawList.AddText(min + new Vector2(OverlayMargin), color,
                LOC.Get("PARAM_ProjectileSimulator_Damage_Total", hits, FormatDamage(total, default)));
        }

        if (!Project(settings.TargetPosition + Vector3.UnitY * settings.TargetRadius, out Vector2 anchor, out _))
        {
            return;
        }

        var lines = 0;

        for (var i = result.Damage.Count - 1; i >= 0 && lines < MaxDamageLines; i--)
        {
            DamageEvent hit = result.Damage[i];

            if (hit.Time > time || time > hit.Time + DamageDisplayTime)
            {
                continue;
            }

            var height = (time - hit.Time) * DamageRiseSpeed + lines * ImGui.GetTextLineHeight();

            drawList.AddText(anchor - new Vector2(0, height + ImGui.GetTextLineHeight()), color,
                FormatDamage(hit.Attack.Flat, hit.Attack.Correction));

            lines++;
        }
    }

    /// <summary>
    ///     Attack power is shown as it is. Percentages of the attacker's power are only shown when they aren't
    ///     the usual 100, unless they are the only thing that the attack has.
    /// </summary>
    public static string FormatDamage(AttackDamage flat, AttackDamage correction)
    {
        List<string> parts = new();

        AddDamage(parts, "PARAM_ProjectileSimulator_Damage_Physical", flat.Physical, correction.Physical);
        AddDamage(parts, "PARAM_ProjectileSimulator_Damage_Magic", flat.Magic, correction.Magic);
        AddDamage(parts, "PARAM_ProjectileSimulator_Damage_Fire", flat.Fire, correction.Fire);
        AddDamage(parts, "PARAM_ProjectileSimulator_Damage_Lightning", flat.Lightning, correction.Lightning);
        AddDamage(parts, "PARAM_ProjectileSimulator_Damage_Dark", flat.Dark, correction.Dark);
        AddDamage(parts, "PARAM_ProjectileSimulator_Damage_Stamina", flat.Stamina, correction.Stamina);

        if (parts.Count > 0)
        {
            return string.Join("  ", parts);
        }

        return correction.Physical > 0 || correction.Magic > 0
            ? LOC.Get("PARAM_ProjectileSimulator_Damage_Attacker")
            : LOC.Get("PARAM_ProjectileSimulator_Damage_None");
    }

    private static void AddDamage(List<string> parts, string key, float flat, float correction)
    {
        var corrected = correction > 0 && correction != DefaultCorrection;

        if (flat > 0 && corrected)
        {
            parts.Add($"{LOC.Get(key)} {flat:0} ({correction:0}%)");
        }
        else if (flat > 0)
        {
            parts.Add($"{LOC.Get(key)} {flat:0}");
        }
        else if (corrected)
        {
            parts.Add($"{LOC.Get(key)} {correction:0}%");
        }
    }

    private void DrawItemTo(ImDrawListPtr drawList, in DrawItem item, bool detailed)
    {
        switch (item.Kind)
        {
            case ItemKind.Line:
                drawList.AddLine(item.A, item.B, ImGui.GetColorU32(item.Color), 1.5f);
                break;

            case ItemKind.Quad:
                drawList.AddQuadFilled(item.A, item.B, item.C, item.D, ImGui.GetColorU32(WithAlpha(item.Color, 0.25f)));
                drawList.AddQuad(item.A, item.B, item.C, item.D, ImGui.GetColorU32(WithAlpha(item.Color, 0.7f)));
                break;

            case ItemKind.Sphere:
                DrawSphere(drawList, item, detailed);
                break;

            case ItemKind.Reach:
                if (Project(item.Position, out Vector2 center, out var depth))
                {
                    drawList.AddCircle(center, item.Radius * Focal / depth, ImGui.GetColorU32(item.Color));
                }

                break;
        }
    }

    private void DrawSphere(ImDrawListPtr drawList, in DrawItem item, bool detailed)
    {
        if (!Project(item.Position, out Vector2 center, out var depth))
        {
            return;
        }

        var screenRadius = MathF.Max(item.Radius * Focal / depth, MinScreenRadius);
        var fill = ImGui.GetColorU32(WithAlpha(item.Color, 0.35f));
        var wire = ImGui.GetColorU32(WithAlpha(item.Color, 0.9f));

        if (!detailed || screenRadius < MinWireRadius)
        {
            drawList.AddCircleFilled(center, screenRadius, fill);
            drawList.AddCircle(center, screenRadius, wire);
            return;
        }

        for (var ring = 0; ring < SphereRings; ring++)
        {
            for (var segment = 0; segment < SphereSegments; segment++)
            {
                var next = (segment + 1) % SphereSegments;

                Vector3 normalA = SpherePoints[ring, segment];
                Vector3 normalB = SpherePoints[ring, next];
                Vector3 normalC = SpherePoints[ring + 1, next];
                Vector3 normalD = SpherePoints[ring + 1, segment];

                Vector3 middle = Vector3.Normalize(normalA + normalB + normalC + normalD);

                // Only the near side, as otherwise both sides fill the same pixels
                if (Vector3.Dot(middle, item.Position + middle * item.Radius - Eye) >= 0)
                {
                    continue;
                }

                if (Project(item.Position + normalA * item.Radius, out Vector2 a, out _) &&
                    Project(item.Position + normalB * item.Radius, out Vector2 b, out _) &&
                    Project(item.Position + normalC * item.Radius, out Vector2 c, out _) &&
                    Project(item.Position + normalD * item.Radius, out Vector2 d, out _))
                {
                    drawList.AddQuadFilled(a, b, c, d, fill);
                    drawList.AddLine(a, b, wire);
                    drawList.AddLine(b, c, wire);
                }
            }
        }
    }

    private static Vector4 WithAlpha(Vector4 color, float alpha)
    {
        return new Vector4(color.X, color.Y, color.Z, color.W * alpha);
    }

    private static Vector3[,] CreateSpherePoints()
    {
        var points = new Vector3[SphereRings + 1, SphereSegments];

        for (var ring = 0; ring <= SphereRings; ring++)
        {
            var latitude = MathF.PI * ring / SphereRings;

            for (var segment = 0; segment < SphereSegments; segment++)
            {
                var longitude = MathF.Tau * segment / SphereSegments;

                points[ring, segment] = new Vector3(MathF.Sin(latitude) * MathF.Cos(longitude), MathF.Cos(latitude),
                    MathF.Sin(latitude) * MathF.Sin(longitude));
            }
        }

        return points;
    }
}
