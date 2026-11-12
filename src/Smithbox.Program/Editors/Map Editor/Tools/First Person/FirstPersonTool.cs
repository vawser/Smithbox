using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;
using StudioCore.Keybinds;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using Veldrid.Utilities;

namespace StudioCore.Editors.MapEditor;

/// <summary>How the spawn point stands.</summary>
public enum FirstPersonSpawnState
{
    None,

    /// <summary>On walkable ground, with room for the player.</summary>
    Ready,

    /// <summary>On a surface steeper than the walkable slope.</summary>
    NotWalkable,

    /// <summary>The player's capsule overlaps the collision there (a low ceiling, a wall).</summary>
    Blocked,

    /// <summary>No collision below the point.</summary>
    NoGround
}

/// <summary>
/// The First Person tool of the Map Editor: walks the loaded maps at head height, colliding with their collision.
/// This part picks where the player starts: a click in the viewport casts a ray against the tool's own collision
/// world (<see cref="FirstPersonCollisionWorld"/>), so the spawn is on what the player stands on, whether or not
/// collision is displayed. The spawn is drawn as a sphere with the player's capsule above it.
/// </summary>
public partial class FirstPersonTool
{
    private readonly MapEditorView View;
    private readonly ProjectEntry Project;

    private FirstPersonCollisionWorld World => View.FirstPersonCollision;

    /// <summary>The player's feet at the spawn, if one is set.</summary>
    public Vector3? Spawn { get; private set; }

    public FirstPersonSpawnState SpawnState { get; private set; }

    /// <summary>True while the next viewport click places the spawn.</summary>
    public bool Picking { get; private set; }

    // The surface under the cursor while picking
    private FirstPersonHit? Hover;
    private FirstPersonSpawnState HoverState;

    // Clicks taken by the tool are held until the button is released, so the release doesn't select
    private bool SwallowUntilRelease;

    // The collision set the spawn was checked against, so a rebuild checks it again
    private FirstPersonCollisionSet CheckedSet;

    private string Status = "";

    // Drawing: a unit sphere and the capsule's lines, placed by their world matrices
    private RenderScene Scene;
    private DbgPrimWireSphere SpherePrim;
    private DbgPrimLineBatch CapsulePrim;
    private (float Radius, float Height) CapsulePrimSize;
    private DebugPrimitiveRenderableProxy SpawnSphere;
    private DebugPrimitiveRenderableProxy SpawnCapsule;
    private DebugPrimitiveRenderableProxy HoverSphere;
    private DebugPrimitiveRenderableProxy HoverCapsule;

    public FirstPersonTool(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public bool CanUse() => World.CanUse();

    private bool IsEnabled => CFG.Current.Interface_MapEditor_Tool_FirstPerson && CanUse();

    public static FirstPersonCapsule PlayerCapsule => new(CFG.Current.MapEditor_FirstPerson_Radius, CFG.Current.MapEditor_FirstPerson_Height);

    /// <summary>How the player moves, from the settings.</summary>
    public static FirstPersonMoveSettings MoveSettings
    {
        get
        {
            var settings = FirstPersonMoveSettings.Default;
            settings.WalkSpeed = MathF.Max(0.1f, CFG.Current.MapEditor_FirstPerson_Walk_Speed);
            settings.RunSpeed = MathF.Max(settings.WalkSpeed, CFG.Current.MapEditor_FirstPerson_Run_Speed);
            settings.Gravity = MathF.Max(0.1f, CFG.Current.MapEditor_FirstPerson_Gravity);
            settings.JumpHeight = MathF.Max(0f, CFG.Current.MapEditor_FirstPerson_Jump_Height);
            settings.StepHeight = Math.Clamp(CFG.Current.MapEditor_FirstPerson_Step_Height, 0f, CFG.Current.MapEditor_FirstPerson_Height * 0.5f);
            settings.MaxSlopeCos = MathF.Cos(Math.Clamp(CFG.Current.MapEditor_FirstPerson_Max_Slope, 0f, 89f) * MathF.PI / 180f);
            return settings;
        }
    }

    /// <summary>How far into the collision the capsule may reach and still count as clear: float error, skin.</summary>
    private const float Allowance = 0.02f;

    /// <summary>The spawn is this far above the ground, so the capsule starts just clear of it.</summary>
    private const float Skin = 0.01f;

    #region Viewport

    /// <summary>
    /// Picks the spawn and keeps its drawing in the viewport's scene. Called each frame by the active viewport.
    /// Returns true while the viewport's clicks are taken, so they don't select.
    /// </summary>
    public bool HandleViewportInput(VulkanViewport viewport, Ray ray, bool canPick)
    {
        if (!IsEnabled)
        {
            Picking = false;
            Hover = null;
            ReleaseDrawing();
            return false;
        }

        if (viewport.RenderScene != Scene)
        {
            ReleaseDrawing();
            Scene = viewport.RenderScene;
        }

        // A rebuild may have moved the ground from under the spawn
        if (Spawn is Vector3 spawn && CheckedSet != World.Current && !World.IsBuilding)
        {
            SpawnState = Evaluate(ref spawn, out _);
            Spawn = spawn;
            CheckedSet = World.Current;
        }

        var taken = false;

        if (SwallowUntilRelease)
        {
            if (!InputManager.IsMouseDown(MousebindID.Viewport_Picking_Action) || InputManager.IsMouseReleased(MousebindID.Viewport_Picking_Action))
                SwallowUntilRelease = false;

            taken = true;
        }

        if (Picking)
        {
            UpdateHover(ray, canPick);

            if (canPick && InputManager.IsMousePressed(MousebindID.Viewport_Picking_Action))
            {
                SwallowUntilRelease = true;

                if (World.IsBuilding)
                {
                    Status = LOC.Get("MAP_FirstPerson_Status_Wait_Build");
                }
                else if (Hover is FirstPersonHit hit)
                {
                    SetSpawn(hit.Point);
                    Picking = false;
                    Hover = null;
                }
                else
                {
                    Status = LOC.Get("MAP_FirstPerson_Status_Nothing_Hit");
                }
            }

            UpdateDrawing();
            return true;
        }

        Hover = null;
        UpdateDrawing();

        return taken;
    }

    /// <summary>The surface under the cursor, and how a spawn there would stand.</summary>
    private void UpdateHover(Ray ray, bool canPick)
    {
        Hover = null;

        if (!canPick || World.IsBuilding)
            return;

        if (World.Current.RayCast(ray.Origin, ray.Direction, 100000, out var hit))
        {
            var feet = hit.Point + new Vector3(0, Skin, 0);
            HoverState = Evaluate(ref feet, out _, hit);
            Hover = hit with { Point = feet };
        }
    }

    /// <summary>
    /// How a spawn at the feet stands: dropped onto the ground below it, pushed out of what it overlaps, then checked
    /// for slope and room. The feet are moved to where the player would stand.
    /// </summary>
    private FirstPersonSpawnState Evaluate(ref Vector3 feet, out FirstPersonHit ground, FirstPersonHit? knownGround = null)
    {
        var set = World.Current;
        var capsule = PlayerCapsule;

        // The ground: the given hit, else the first surface below the point (from a little above it, so a point on
        // the ground finds that ground)
        if (knownGround is FirstPersonHit known)
        {
            ground = known;
        }
        else if (set.RayCast(feet + new Vector3(0, 0.5f, 0), -Vector3.UnitY, 1000, out var below))
        {
            ground = below;
            feet = below.Point + new Vector3(0, Skin, 0);
        }
        else
        {
            ground = default;
            return FirstPersonSpawnState.NoGround;
        }

        var maxSlope = MathF.Cos(Math.Clamp(CFG.Current.MapEditor_FirstPerson_Max_Slope, 0f, 89f) * MathF.PI / 180f);

        if (ground.Normal.Y < maxSlope)
            return FirstPersonSpawnState.NotWalkable;

        // The capsule's bottom sphere touches sloped ground beside the point; push it clear, then check it stayed on
        // the spot (a push of more than the radius means it was put somewhere else entirely)
        var start = feet;

        if (!capsule.Depenetrate(set, ref feet, Allowance) || Vector3.Distance(start, feet) > capsule.Radius)
        {
            feet = start;
            return FirstPersonSpawnState.Blocked;
        }

        return FirstPersonSpawnState.Ready;
    }

    private void SetSpawn(Vector3 feet)
    {
        SpawnState = Evaluate(ref feet, out var ground);
        Spawn = feet;
        CheckedSet = World.Current;
        Status = "";

        // Diagnostic for verification: remove once spawn picking is confirmed in the app
        Smithbox.Log(typeof(FirstPersonTool), $"[First Person] Spawn {SpawnState} at {feet}: {ground.Mesh?.Kind} collision of {ground.Mesh?.PartName} ({ground.Mesh?.Source}), normal {ground.Normal}", LogLevel.Debug);
    }

    #endregion

    #region Drawing

    private static Color ToColor(Vector3 c)
        => Color.FromArgb(255, (int)Math.Clamp(c.X, 0, 255), (int)Math.Clamp(c.Y, 0, 255), (int)Math.Clamp(c.Z, 0, 255));

    private static Color StateColor(FirstPersonSpawnState state)
        => ToColor(state == FirstPersonSpawnState.Ready ? CFG.Current.MapEditor_FirstPerson_Spawn_Color : CFG.Current.MapEditor_FirstPerson_Blocked_Color);

    private void UpdateDrawing()
    {
        if (Scene == null)
            return;

        var capsule = PlayerCapsule;

        SpherePrim ??= new DbgPrimWireSphere(Transform.Default, 1.0f, Color.White, 8, 12);

        if (CapsulePrim == null || CapsulePrimSize != (capsule.Radius, capsule.Height))
        {
            // The proxies hold the old lines, so they are made again with the new ones
            SpawnCapsule?.Dispose();
            HoverCapsule?.Dispose();
            SpawnCapsule = null;
            HoverCapsule = null;

            CapsulePrim = new DbgPrimLineBatch(CapsuleLines(capsule));
            CapsulePrimSize = (capsule.Radius, capsule.Height);
        }

        // Hidden while walking: the capsule would be drawn around the camera
        var spawn = IsWalking ? null : Spawn;
        Place(ref SpawnSphere, SpherePrim, spawn, SpawnState, true);
        Place(ref SpawnCapsule, CapsulePrim, spawn, SpawnState, false);

        var hover = Hover?.Point;
        Place(ref HoverSphere, SpherePrim, hover, HoverState, true);
        Place(ref HoverCapsule, CapsulePrim, hover, HoverState, false);
    }

    private void Place(ref DebugPrimitiveRenderableProxy proxy, IDbgPrim prim, Vector3? position, FirstPersonSpawnState state, bool sphere)
    {
        proxy ??= MakeProxy(prim);

        if (position is Vector3 p)
        {
            proxy.World = sphere
                ? Matrix4x4.CreateScale(0.15f) * Matrix4x4.CreateTranslation(p)
                : Matrix4x4.CreateTranslation(p);

            var color = StateColor(state);

            if (proxy.BaseColor != color)
            {
                proxy.BaseColor = color;
                proxy.HighlightedColor = color;
            }

            proxy.Visible = true;
        }
        else
        {
            proxy.Visible = false;
        }
    }

    private DebugPrimitiveRenderableProxy MakeProxy(IDbgPrim prim)
    {
        var color = ToColor(CFG.Current.MapEditor_FirstPerson_Spawn_Color);

        return new DebugPrimitiveRenderableProxy(Scene.OpaqueRenderables, prim)
        {
            // Clicks go to what is behind, and the spawn shows through the map
            Pickable = false,
            RenderOverlay = true,
            BaseColor = color,
            HighlightedColor = color,
        };
    }

    /// <summary>The capsule as lines, feet at the origin: rings at the spheres' centres and the middle, side lines and arcs.</summary>
    private static List<(Vector3, Vector3)> CapsuleLines(FirstPersonCapsule capsule)
    {
        const int segments = 16;
        var lines = new List<(Vector3, Vector3)>();
        var r = capsule.Radius;
        var bottom = r;
        var top = capsule.Height - r;

        void Ring(float y)
        {
            for (var i = 0; i < segments; i++)
            {
                var a0 = i * MathF.Tau / segments;
                var a1 = (i + 1) * MathF.Tau / segments;
                lines.Add((new Vector3(MathF.Cos(a0) * r, y, MathF.Sin(a0) * r), new Vector3(MathF.Cos(a1) * r, y, MathF.Sin(a1) * r)));
            }
        }

        Ring(bottom);
        Ring(top);

        // Side lines, and a half circle over each end in two planes
        for (var k = 0; k < 4; k++)
        {
            var angle = k * MathF.PI / 2;
            var side = new Vector3(MathF.Cos(angle), 0, MathF.Sin(angle)) * r;
            lines.Add((side + new Vector3(0, bottom, 0), side + new Vector3(0, top, 0)));
        }

        foreach (var plane in new[] { Vector3.UnitX, Vector3.UnitZ })
        {
            for (var i = 0; i < segments / 2; i++)
            {
                var a0 = i * MathF.PI / (segments / 2);
                var a1 = (i + 1) * MathF.PI / (segments / 2);

                Vector3 Arc(float a, float centre, float sign) => plane * (MathF.Cos(a) * r) + new Vector3(0, centre + sign * MathF.Sin(a) * r, 0);

                lines.Add((Arc(a0, top, 1), Arc(a1, top, 1)));
                lines.Add((Arc(a0, bottom, -1), Arc(a1, bottom, -1)));
            }
        }

        // The eye, as a short cross
        var eye = CFG.Current.MapEditor_FirstPerson_Eye_Height;
        lines.Add((new Vector3(-r * 0.5f, eye, 0), new Vector3(r * 0.5f, eye, 0)));
        lines.Add((new Vector3(0, eye, -r * 0.5f), new Vector3(0, eye, r * 0.5f)));

        return lines;
    }

    private void ReleaseDrawing()
    {
        SpawnSphere?.Dispose();
        SpawnCapsule?.Dispose();
        HoverSphere?.Dispose();
        HoverCapsule?.Dispose();

        SpawnSphere = null;
        SpawnCapsule = null;
        HoverSphere = null;
        HoverCapsule = null;
        Scene = null;
    }

    #endregion

    #region Tool Window

    public void OnToolWindow()
    {
        ImGui.BeginChild("mapFirstPersonToolSection", ImGuiChildFlags.Borders);

        GUI.WrappedText(LOC.Get("MAP_FirstPerson_Description"));

        if (!CanUse())
        {
            GUI.WrappedText(LOC.Get("MAP_FirstPerson_Unsupported"));
            ImGui.EndChild();
            return;
        }

        if (!CFG.Current.MapEditor_ModelLoad_Collisions)
        {
            GUI.WrappedText(LOC.Get("MAP_FirstPerson_Collision_Loading_Off"));
            ImGui.EndChild();
            return;
        }

        if (!Project.Handler.MapData.PrimaryBank.Maps.Values.Any(m => m.MapContainer != null))
        {
            GUI.WrappedText(LOC.Get("MAP_FirstPerson_No_Map"));
            ImGui.EndChild();
            return;
        }

        DisplaySpawn();
        DisplayWalk();
        DisplayCollision();
        DisplayPlayer();
        DisplayWalkDisplay();

        ImGui.EndChild();
    }

    private void DisplaySpawn()
    {
        // Spawn
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_FirstPerson_Spawn_Header"), LOC.Get("MAP_FirstPerson_Spawn_Header_TT"));

        if (Picking)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, LOC.Get(World.IsBuilding
                ? "MAP_FirstPerson_Status_Picking_Wait"
                : "MAP_FirstPerson_Status_Picking"));
        }

        if (!string.IsNullOrEmpty(Status))
            GUI.WrappedText(Status);

        if (Spawn is Vector3 spawn)
        {
            GUI.WrappedText(LOC.Get($"MAP_FirstPerson_Spawn_State_{SpawnState}"));

            ImGui.SetNextItemWidth(-1);
            if (ImGui.DragFloat3("##mapFirstPersonSpawnPosition", ref spawn, 0.05f, 0, 0, "%.3f"))
            {
                // Typed in: checked where it is, without dropping it onto the ground
                Spawn = spawn;
                SpawnState = CheckInPlace(spawn);
                CheckedSet = World.Current;
            }
            GUI.Tooltip(LOC.Get("MAP_FirstPerson_Spawn_Position_TT"));
        }
        else
        {
            GUI.WrappedText(LOC.Get("MAP_FirstPerson_Spawn_None"));
        }

        var camera = View.ViewportHandler.ActiveViewport?.Viewport?.ViewportCamera;
        var selection = GetSelectionPosition();

        GUI.ConditionalMultiButtonInput("mapFirstPersonSpawnActions",
            "mapFirstPersonPick", LOC.Get(Picking ? "MAP_FirstPerson_Pick_Cancel" : "MAP_FirstPerson_Pick"),
            LOC.Get(Picking ? "MAP_FirstPerson_Pick_Cancel_TT" : "MAP_FirstPerson_Pick_TT"),
            TogglePicking, !IsWalking,

            "mapFirstPersonFromCamera", LOC.Get("MAP_FirstPerson_From_Camera"), LOC.Get("MAP_FirstPerson_From_Camera_TT"),
            () => DropFrom(camera.CameraTransform.Position), camera != null && !World.IsBuilding,

            "mapFirstPersonFromSelection", LOC.Get("MAP_FirstPerson_From_Selection"), LOC.Get("MAP_FirstPerson_From_Selection_TT"),
            () => DropFrom(selection.Value + new Vector3(0, 0.5f, 0)), selection != null && !World.IsBuilding,

            "mapFirstPersonClearSpawn", LOC.Get("MAP_FirstPerson_Clear_Spawn"), LOC.Get("MAP_FirstPerson_Clear_Spawn_TT"),
            ClearSpawn, Spawn != null);
    }

    private void DisplayCollision()
    {
        // Collision
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_FirstPerson_Collision_Header"), LOC.Get("MAP_FirstPerson_Collision_Header_TT"));

        var status = World.Status;

        if (World.IsBuilding)
        {
            var fraction = status.Total > 0 ? (float)status.Done / status.Total : 0f;
            ImGui.ProgressBar(fraction, new Vector2(-1, 0), LOC.Get("MAP_FirstPerson_Building", status.Done, status.Total));
        }
        else if (status.Failed)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Invalid_Text_Color, LOC.Get("MAP_FirstPerson_Build_Failed"));
        }
        else if (World.Current.Meshes.Count > 0 || status.Maps > 0)
        {
            GUI.WrappedText(LOC.Get("MAP_FirstPerson_Built_Map", status.Maps, status.MapParts, $"{status.MapTriangles:N0}"));

            if (status.MapPartsFiltered > 0)
                GUI.WrappedText(LOC.Get("MAP_FirstPerson_Built_Map_Filtered", status.MapPartsFiltered, World.PlayerHitFilterName));

            if (status.MapPartsFallback > 0 || status.MapPartsMissing > 0)
                GUI.WrappedText(LOC.Get("MAP_FirstPerson_Built_Map_Other", status.MapPartsFallback, status.MapPartsMissing));

            if (CFG.Current.MapEditor_FirstPerson_Assets)
                GUI.WrappedText(LOC.Get("MAP_FirstPerson_Built_Assets", status.AssetPlacements, status.AssetModels, $"{status.AssetTriangles:N0}"));

            GUI.WrappedText(LOC.Get("MAP_FirstPerson_Built_Cost", $"{status.MemoryBytes / (1024.0 * 1024.0):0.0}", $"{status.Milliseconds / 1000.0:0.0}"));

            if (status.Errors.Count > 0)
                GUI.WrappedText(LOC.Get("MAP_FirstPerson_Built_Errors", status.Errors.Count));

            if (World.IsOutOfDate)
                GUI.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, LOC.Get("MAP_FirstPerson_Out_Of_Date"));
        }
        else
        {
            GUI.WrappedText(LOC.Get("MAP_FirstPerson_Not_Built"));
        }

        GUI.ConditionalMultiButtonInput("mapFirstPersonCollisionActions",
            "mapFirstPersonBuild", LOC.Get("MAP_FirstPerson_Build"), LOC.Get("MAP_FirstPerson_Build_TT"),
            () => World.RequestBuild(force: true), !World.IsBuilding);

        // Layer
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_FirstPerson_Layer_Header"), LOC.Get("MAP_FirstPerson_Layer_Header_TT"));

        var layer = CFG.Current.MapEditor_FirstPerson_Layer is HavokCollisionType.High ? HavokCollisionType.High : HavokCollisionType.Low;

        ImGui.SetNextItemWidth(-1);
        if (ImGui.BeginCombo("##mapFirstPersonLayer", LOC.Get(layer.GetDisplayName())))
        {
            foreach (var type in new[] { HavokCollisionType.Low, HavokCollisionType.High })
            {
                if (ImGui.Selectable($"{LOC.Get(type.GetDisplayName())}##mapFirstPersonLayer{type}", type == layer))
                    CFG.Current.MapEditor_FirstPerson_Layer = type;
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("MAP_FirstPerson_Layer_TT"));

        ImGui.Checkbox($"{LOC.Get("MAP_FirstPerson_Fall_Protection")}##mapFirstPersonFall", ref CFG.Current.MapEditor_FirstPerson_Fall_Protection);
        GUI.Tooltip(LOC.Get("MAP_FirstPerson_Fall_Protection_TT"));

        ImGui.Checkbox($"{LOC.Get("MAP_FirstPerson_Assets")}##mapFirstPersonAssets", ref CFG.Current.MapEditor_FirstPerson_Assets);
        GUI.Tooltip(LOC.Get("MAP_FirstPerson_Assets_TT"));

        if (CFG.Current.MapEditor_FirstPerson_Assets)
        {
            ImGui.Checkbox($"{LOC.Get("MAP_FirstPerson_Dummy_Assets")}##mapFirstPersonDummies", ref CFG.Current.MapEditor_FirstPerson_Dummy_Assets);
            GUI.Tooltip(LOC.Get("MAP_FirstPerson_Dummy_Assets_TT"));
        }
    }

    private void DisplayPlayer()
    {
        // Player
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_FirstPerson_Player_Header"), LOC.Get("MAP_FirstPerson_Player_Header_TT"));

        var changed = false;

        ImGui.SetNextItemWidth(-1);
        changed |= ImGui.SliderFloat("##mapFirstPersonEyeHeight", ref CFG.Current.MapEditor_FirstPerson_Eye_Height, 0.5f, 3.0f, LOC.Get("MAP_FirstPerson_Eye_Height_Format"));
        GUI.Tooltip(LOC.Get("MAP_FirstPerson_Eye_Height_TT"));

        ImGui.SetNextItemWidth(-1);
        changed |= ImGui.SliderFloat("##mapFirstPersonHeight", ref CFG.Current.MapEditor_FirstPerson_Height, 0.5f, 3.5f, LOC.Get("MAP_FirstPerson_Height_Format"));
        GUI.Tooltip(LOC.Get("MAP_FirstPerson_Height_TT"));

        ImGui.SetNextItemWidth(-1);
        changed |= ImGui.SliderFloat("##mapFirstPersonRadius", ref CFG.Current.MapEditor_FirstPerson_Radius, 0.1f, 1.0f, LOC.Get("MAP_FirstPerson_Radius_Format"));
        GUI.Tooltip(LOC.Get("MAP_FirstPerson_Radius_TT"));

        ImGui.SetNextItemWidth(-1);
        changed |= ImGui.SliderFloat("##mapFirstPersonSlope", ref CFG.Current.MapEditor_FirstPerson_Max_Slope, 10f, 80f, LOC.Get("MAP_FirstPerson_Max_Slope_Format"));
        GUI.Tooltip(LOC.Get("MAP_FirstPerson_Max_Slope_TT"));

        // The head stays inside the capsule
        CFG.Current.MapEditor_FirstPerson_Height = MathF.Max(CFG.Current.MapEditor_FirstPerson_Height, CFG.Current.MapEditor_FirstPerson_Eye_Height + 0.1f);

        // Movement
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_FirstPerson_Movement_Header"), LOC.Get("MAP_FirstPerson_Movement_Header_TT"));

        ImGui.SetNextItemWidth(-1);
        ImGui.SliderFloat("##mapFirstPersonWalkSpeed", ref CFG.Current.MapEditor_FirstPerson_Walk_Speed, 0.5f, 15f, LOC.Get("MAP_FirstPerson_Walk_Speed_Format"));
        GUI.Tooltip(LOC.Get("MAP_FirstPerson_Walk_Speed_TT"));

        ImGui.SetNextItemWidth(-1);
        ImGui.SliderFloat("##mapFirstPersonRunSpeed", ref CFG.Current.MapEditor_FirstPerson_Run_Speed, 0.5f, 30f, LOC.Get("MAP_FirstPerson_Run_Speed_Format"));
        GUI.Tooltip(LOC.Get("MAP_FirstPerson_Run_Speed_TT"));

        ImGui.SetNextItemWidth(-1);
        ImGui.SliderFloat("##mapFirstPersonJumpHeight", ref CFG.Current.MapEditor_FirstPerson_Jump_Height, 0f, 5f, LOC.Get("MAP_FirstPerson_Jump_Height_Format"));
        GUI.Tooltip(LOC.Get("MAP_FirstPerson_Jump_Height_TT"));

        ImGui.SetNextItemWidth(-1);
        ImGui.SliderFloat("##mapFirstPersonGravity", ref CFG.Current.MapEditor_FirstPerson_Gravity, 1f, 60f, LOC.Get("MAP_FirstPerson_Gravity_Format"));
        GUI.Tooltip(LOC.Get("MAP_FirstPerson_Gravity_TT"));

        ImGui.SetNextItemWidth(-1);
        ImGui.SliderFloat("##mapFirstPersonStepHeight", ref CFG.Current.MapEditor_FirstPerson_Step_Height, 0f, 1f, LOC.Get("MAP_FirstPerson_Step_Height_Format"));
        GUI.Tooltip(LOC.Get("MAP_FirstPerson_Step_Height_TT"));

        if (ImGui.Button($"{LOC.Get("MAP_FirstPerson_Player_Reset")}##mapFirstPersonPlayerReset", new Vector2(-1, 0)))
        {
            CFG.Current.MapEditor_FirstPerson_Eye_Height = 1.5f;
            CFG.Current.MapEditor_FirstPerson_Height = 1.7f;
            CFG.Current.MapEditor_FirstPerson_Radius = 0.35f;
            CFG.Current.MapEditor_FirstPerson_Max_Slope = 50f;
            CFG.Current.MapEditor_FirstPerson_Walk_Speed = 4f;
            CFG.Current.MapEditor_FirstPerson_Run_Speed = 7f;
            CFG.Current.MapEditor_FirstPerson_Jump_Height = 1f;
            CFG.Current.MapEditor_FirstPerson_Gravity = 20f;
            CFG.Current.MapEditor_FirstPerson_Step_Height = 0.45f;
            changed = true;
        }
        GUI.Tooltip(LOC.Get("MAP_FirstPerson_Player_Reset_TT"));

        // A bigger or smaller player may no longer fit where it stands
        if (changed && Spawn is Vector3 spawn)
        {
            SpawnState = CheckInPlace(spawn);
        }
    }

    #endregion

    #region Spawn

    private void TogglePicking()
    {
        Picking = !Picking;
        Hover = null;
        Status = "";

        // The ray needs the collision; it is built now if it isn't, or is out of date
        if (Picking && !World.RequestBuild())
        {
            Picking = false;
            Status = LOC.Get("MAP_FirstPerson_No_Map");
        }
    }

    /// <summary>Drops the spawn onto the ground below a point (the camera, the selection).</summary>
    private void DropFrom(Vector3 point)
    {
        if (World.IsOutOfDate && World.RequestBuild())
        {
            // Checked again once the build is in
            Spawn = point;
            SpawnState = FirstPersonSpawnState.None;
            CheckedSet = null;
            return;
        }

        if (World.Current.RayCast(point, -Vector3.UnitY, 5000, out var hit))
        {
            SetSpawn(hit.Point + new Vector3(0, Skin, 0));
        }
        else
        {
            Status = LOC.Get("MAP_FirstPerson_Status_Nothing_Below");
        }
    }

    /// <summary>How a typed-in spawn stands, where it is: on ground just below it, with room.</summary>
    private FirstPersonSpawnState CheckInPlace(Vector3 feet)
    {
        var set = World.Current;

        if (!set.RayCast(feet + new Vector3(0, 0.5f, 0), -Vector3.UnitY, 0.5f + PlayerCapsule.Radius, out var ground))
            return FirstPersonSpawnState.NoGround;

        var maxSlope = MathF.Cos(Math.Clamp(CFG.Current.MapEditor_FirstPerson_Max_Slope, 0f, 89f) * MathF.PI / 180f);

        if (ground.Normal.Y < maxSlope)
            return FirstPersonSpawnState.NotWalkable;

        return PlayerCapsule.Overlaps(set, feet, Allowance, out _) ? FirstPersonSpawnState.Blocked : FirstPersonSpawnState.Ready;
    }

    private void ClearSpawn()
    {
        Spawn = null;
        SpawnState = FirstPersonSpawnState.None;
        CheckedSet = null;
        Status = "";
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

    #endregion
}
