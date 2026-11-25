using Hexa.NET.ImGui;
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

/// <summary>
/// The Subtract tab: a box placed in the map (on clicked collision, at the selection, or by its fields and the gizmo)
/// that cuts what it covers out of the collision of the ticked layers. Triangles inside it are removed, and those
/// crossing it are cut along its faces, so the hole is box-shaped (see CollisionBoxSubtractor). Subtract rebuilds the
/// cut bodies as one undoable action and writes their binders into the project, as the Create tab's Overwrite does.
/// </summary>
public partial class CollisionEditorTool
{
    // The frame the Subtract tab was last drawn: the box is only drawn, placed and moved while it is
    private int LastSubtractTabFrame = -10;

    private bool IsSubtractShown => Frame - LastSubtractTabFrame <= 1;

    // The box: its centre, rotation and size in the world. The rotation is shown as angles in degrees, which are
    // worked out again only when the gizmo turns it, so typed angles aren't rewritten as they are typed.
    private bool SubtractHasBox;
    private Vector3 SubtractPosition;
    private Quaternion SubtractRotation = Quaternion.Identity;
    private Vector3 SubtractAngles;
    private Vector3 SubtractSize = new(4f, 4f, 4f);

    private const float MinSubtractSize = 0.01f;

    // A click on collision places the box there
    private bool SubtractPlacing;

    // Clicks taken by placing are held until the button is released, so the release doesn't select
    private bool SubtractSwallowUntilRelease;

    // Whether the gizmo moves the box (on while the tab is open, until stopped)
    private bool SubtractGizmo = true;

    private readonly HashSet<HavokCollisionType> SubtractLayers = new() { HavokCollisionType.High, HavokCollisionType.Low };

    private RenderScene SubtractScene;
    private DebugPrimitiveRenderableProxy SubtractWire;
    private DebugPrimitiveRenderableProxy SubtractFill;

    private static readonly Color SubtractBoxColor = Color.OrangeRed;

    private CollisionBoxGizmoTarget SubtractGizmoTargetInstance;

    /// <summary>The gizmo target of the Subtract tab's box.</summary>
    public CollisionBoxGizmoTarget SubtractGizmoTarget => SubtractGizmoTargetInstance ??= new CollisionBoxGizmoTarget(this);

    /// <summary>A body the box reaches, and how many of its triangles it would remove whole or cut.</summary>
    private sealed record SubtractTarget(CollisionEditorFile File, CollisionEditorBody Body, Matrix4x4 ToBox, int Removed, int Cut);

    // What the box reaches, worked out again when the box, the layers or the decoded files change (at most every few
    // frames, as a gizmo drag changes the box every frame)
    private (Vector3 Position, Quaternion Rotation, Vector3 Size, int Layers, int Files, int Version) SubtractPreviewKey;
    private List<SubtractTarget> SubtractPreview;
    private List<string> SubtractBlocked;
    private int SubtractPreviewFrame = -100;
    private int SubtractVersion;

    private const int SubtractPreviewInterval = 6;

    #region Gizmo

    /// <summary>Whether the gizmo moves the box: the tab is open, there is a box, and the gizmo hasn't been stopped.</summary>
    public bool HasSubtractGizmo => IsSubtractShown && SubtractHasBox && SubtractGizmo;

    public Transform GetSubtractTransform()
    {
        return new Transform(SubtractPosition, SubtractRotation, SubtractSize);
    }

    /// <summary>The box as the gizmo moves, turns or sizes it.</summary>
    public void SetSubtractTransform(Transform transform, GizmoState.GizmosMode mode)
    {
        switch (mode)
        {
            case GizmoState.GizmosMode.Translate:
                SubtractPosition = transform.Position;
                break;
            case GizmoState.GizmosMode.Rotate:
                SubtractRotation = Quaternion.Normalize(transform.Rotation);
                SubtractAngles = ToAngles(SubtractRotation);
                break;
            case GizmoState.GizmosMode.Scale:
                SubtractSize = Vector3.Max(transform.Scale, new Vector3(MinSubtractSize));
                break;
        }
    }

    #endregion

    #region Tab

    private void DisplaySubtractTab()
    {
        LastSubtractTabFrame = Frame;

        DisplaySubtractBox();
        DisplaySubtractLayers();
        DisplaySubtractPreview();
        DisplaySubtractActions();
    }

    private void DisplaySubtractBox()
    {
        // Box
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CollisionEditor_Subtract_Box_Header"), LOC.Get("MAP_CollisionEditor_Subtract_Box_Header_TT"));

        GUI.ConditionalMultiButtonInput("collisionEditorSubtractPlace",
            "collisionEditorSubtractPlaceClick",
            SubtractPlacing ? LOC.Get("MAP_CollisionEditor_Subtract_Place_Stop") : LOC.Get("MAP_CollisionEditor_Subtract_Place"),
            LOC.Get("MAP_CollisionEditor_Subtract_Place_TT"),
            () => SubtractPlacing = !SubtractPlacing, true,

            "collisionEditorSubtractPlaceSelection", LOC.Get("MAP_CollisionEditor_Subtract_Place_Selection"),
            LOC.Get("MAP_CollisionEditor_Subtract_Place_Selection_TT"),
            PlaceSubtractAtSelection, GetSelectionPosition() != null,

            "collisionEditorSubtractClear", LOC.Get("MAP_CollisionEditor_Subtract_Clear"), LOC.Get("MAP_CollisionEditor_Subtract_Clear_TT"),
            ClearSubtractBox, SubtractHasBox);

        if (SubtractPlacing)
            GUI.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color,
                LOC.Get("MAP_CollisionEditor_Subtract_Placing", InputManager.GetMouseHint(MousebindID.Viewport_Picking_Action)));

        if (!SubtractHasBox)
        {
            GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Subtract_No_Box"));
            return;
        }

        ImGui.Checkbox($"{LOC.Get("MAP_CollisionEditor_Subtract_Gizmo")}##collisionEditorSubtractGizmo", ref SubtractGizmo);
        GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Subtract_Gizmo_TT"));

        // Position
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CollisionEditor_Subtract_Position_Header"), LOC.Get("MAP_CollisionEditor_Subtract_Position_Header_TT"));

        ImGui.SetNextItemWidth(-1);
        ImGui.DragFloat3("##collisionEditorSubtractPosition", ref SubtractPosition, 0.05f, 0, 0, "%.3f");
        GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Subtract_Position_Input_TT"));

        // Rotation
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CollisionEditor_Subtract_Rotation_Header"), LOC.Get("MAP_CollisionEditor_Subtract_Rotation_Header_TT"));

        ImGui.SetNextItemWidth(-1);
        if (ImGui.DragFloat3("##collisionEditorSubtractRotation", ref SubtractAngles, 0.5f, 0, 0, "%.2f"))
            SubtractRotation = FromAngles(SubtractAngles);
        GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Subtract_Rotation_Input_TT"));

        // Size
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CollisionEditor_Subtract_Size_Header"), LOC.Get("MAP_CollisionEditor_Subtract_Size_Header_TT"));

        ImGui.SetNextItemWidth(-1);
        if (ImGui.DragFloat3("##collisionEditorSubtractSize", ref SubtractSize, 0.05f, MinSubtractSize, 10000f, "%.3f"))
            SubtractSize = Vector3.Max(SubtractSize, new Vector3(MinSubtractSize));
        GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Subtract_Size_Input_TT"));
    }

    private void DisplaySubtractLayers()
    {
        // Layers
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CollisionEditor_Subtract_Layers_Header"), LOC.Get("MAP_CollisionEditor_Subtract_Layers_Header_TT"));

        foreach (var type in View.HavokCollisionBank.GetSupportedCollisionTypes())
        {
            var ticked = SubtractLayers.Contains(type);

            if (ImGui.Checkbox($"{LOC.Get(type.GetDisplayName())}##collisionEditorSubtractLayer{type}", ref ticked))
            {
                if (ticked)
                    SubtractLayers.Add(type);
                else
                    SubtractLayers.Remove(type);
            }
            GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Subtract_Layer_TT"));
        }
    }

    /// <summary>What Subtract would do: the files the box reaches and how many triangles it removes and cuts in each.</summary>
    private void DisplaySubtractPreview()
    {
        // Affected Collision
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CollisionEditor_Subtract_Preview_Header"), LOC.Get("MAP_CollisionEditor_Subtract_Preview_Header_TT"));

        if (!SubtractHasBox)
        {
            GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Subtract_No_Box"));
            return;
        }

        if (SubtractLayers.Count == 0)
        {
            GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Subtract_No_Layers"));
            return;
        }

        UpdateSubtractPreview();

        if (SubtractPreview == null || SubtractPreview.Count == 0)
        {
            GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Subtract_Preview_None"));
            return;
        }

        foreach (var group in SubtractPreview.GroupBy(t => t.File))
        {
            GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Subtract_Preview_File", group.Key.FileName,
                group.Sum(t => t.Removed), group.Sum(t => t.Cut)));
        }

        foreach (var name in SubtractBlocked)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MAP_CollisionEditor_Subtract_Blocked", name));
        }
    }

    private void DisplaySubtractActions()
    {
        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CollisionEditor_Subtract_Actions_Header"), LOC.Get("MAP_CollisionEditor_Subtract_Actions_Header_TT"));

        var canApply = SubtractHasBox && SubtractLayers.Count > 0 && SubtractPreview is { Count: > 0 } && SubtractBlocked is { Count: 0 };

        GUI.ConditionalMultiButtonInput("collisionEditorSubtractActions",
            "collisionEditorSubtractApply", LOC.Get("MAP_CollisionEditor_Subtract_Apply"), LOC.Get("MAP_CollisionEditor_Subtract_Apply_TT"),
            ApplySubtract, canApply);

        DisplayLastResult(CollisionEditorSource.Subtract);
    }

    #endregion

    #region Placing

    private Vector3? GetSelectionPosition()
    {
        var entity = View.ViewportSelection.GetSelection().OfType<MsbEntity>().FirstOrDefault();
        return entity?.GetWorldMatrix().Translation;
    }

    private void PlaceSubtractAtSelection()
    {
        if (GetSelectionPosition() is not { } position)
            return;

        SubtractPosition = position;
        SubtractHasBox = true;
        SubtractPlacing = false;
        SubtractGizmo = true;
    }

    private void ClearSubtractBox()
    {
        SubtractHasBox = false;
        SubtractPlacing = false;
        SubtractPreview = null;
        SubtractBlocked = null;
    }

    /// <summary>
    /// Places the box where a click meets the collision of the ticked layers, and draws it. Called each frame by the
    /// viewport input. Returns true while the viewport's clicks are taken.
    /// </summary>
    private bool HandleSubtractInput(RenderScene scene, Ray ray, bool canInteract)
    {
        if (!IsSubtractShown)
        {
            SubtractPlacing = false;
            SubtractSwallowUntilRelease = false;
            HideSubtractBox();
            return false;
        }

        var taken = false;

        if (SubtractSwallowUntilRelease)
        {
            if (InputManager.IsMouseDown(MousebindID.Viewport_Picking_Action))
                taken = true;
            else
                SubtractSwallowUntilRelease = false;
        }

        if (SubtractPlacing)
        {
            taken = true;

            if (canInteract && !ImGui.GetIO().WantTextInput && InputManager.IsMousePressed(MousebindID.Viewport_Picking_Action))
            {
                var letters = SubtractLayers.Select(GetLayerLetter).ToHashSet();
                var hit = scene.DrawFilter.HasFlag(RenderFilter.Collision) ? RayCast(ray, letters) : null;

                if (hit != null)
                {
                    SubtractPosition = hit.Point;
                    SubtractHasBox = true;
                    SubtractPlacing = false;
                    SubtractGizmo = true;
                }

                SubtractSwallowUntilRelease = true;
            }
        }

        UpdateSubtractBox(scene);

        return taken;
    }

    #endregion

    #region Box Display

    private Matrix4x4 GetSubtractBoxWorld()
    {
        return Matrix4x4.CreateScale(SubtractSize) * Matrix4x4.CreateFromQuaternion(SubtractRotation)
            * Matrix4x4.CreateTranslation(SubtractPosition);
    }

    private void UpdateSubtractBox(RenderScene scene)
    {
        if (scene != SubtractScene)
        {
            DisposeSubtractBox();
            SubtractScene = scene;
        }

        if (!SubtractHasBox)
        {
            HideSubtractBox();
            return;
        }

        SubtractWire ??= RenderableHelper.GetUnitBoxWireProxy(scene, SubtractBoxColor);
        SubtractFill ??= RenderableHelper.GetUnitBoxFillProxy(scene, SubtractBoxColor);

        var world = GetSubtractBoxWorld();

        if (SubtractWire.World != world)
            SubtractWire.World = world;

        if (SubtractFill.World != world)
            SubtractFill.World = world;

        SetVisible(SubtractWire, true);
        SetVisible(SubtractFill, true);
    }

    private void HideSubtractBox()
    {
        SetVisible(SubtractWire, false);
        SetVisible(SubtractFill, false);
    }

    private void DisposeSubtractBox()
    {
        SubtractWire?.Dispose();
        SubtractFill?.Dispose();

        SubtractWire = null;
        SubtractFill = null;
    }

    #endregion

    #region Preview

    private void UpdateSubtractPreview()
    {
        var layers = SubtractLayers.Aggregate(0, (mask, type) => mask | 1 << (int)type);
        var key = (SubtractPosition, SubtractRotation, SubtractSize, layers, Files.Count, SubtractVersion);

        if (SubtractPreview != null && key.Equals(SubtractPreviewKey))
            return;

        // A gizmo drag changes the box every frame: the search over the bodies is done every few frames
        if (SubtractPreview != null && Frame - SubtractPreviewFrame < SubtractPreviewInterval)
            return;

        SubtractPreviewKey = key;
        SubtractPreviewFrame = Frame;
        SubtractPreview = FindSubtractTargets(out SubtractBlocked);
    }

    /// <summary>
    /// The bodies of the ticked layers the box reaches, in the visible collision parts of the loaded maps, with how
    /// many triangles it would remove and cut in each. Files with uncommitted Edit or Material tab changes are listed
    /// in blocked, as Subtract rebuilds from the decoded collision and would drop those changes or commit them unseen.
    /// </summary>
    private List<SubtractTarget> FindSubtractTargets(out List<string> blocked)
    {
        var targets = new List<SubtractTarget>();
        blocked = new List<string>();

        if (!SubtractHasBox || SubtractLayers.Count == 0)
            return targets;

        var boxWorld = GetSubtractBoxWorld();

        if (!Matrix4x4.Invert(boxWorld, out var worldToBox))
            return targets;

        var boxBounds = TransformBounds(new BoundingBox(new Vector3(-0.5f), new Vector3(0.5f)), boxWorld);
        var letters = SubtractLayers.Select(GetLayerLetter).ToHashSet();
        var found = new List<CollisionEditorFile>();

        foreach (var entity in GetCollisionParts())
        {
            if (!entity.EditorVisible)
                continue;

            foreach (var proxy in GetCollisionProxies(entity))
            {
                var fileName = ResolveFileName(entity.MapID, proxy.ResourceHandle?.Name);

                if (fileName == null || Unsupported.Contains(fileName) || !letters.Contains(char.ToLowerInvariant(fileName[0])))
                    continue;

                if (!proxy.GetBounds().Intersects(boxBounds))
                    continue;

                var file = Files.TryGetValue(fileName, out var decoded) ? decoded : DecodeFile(entity, fileName);

                // A file is edited through the part it was first found on
                if (file == null || file.Entity != entity || found.Contains(file))
                    continue;

                found.Add(file);
            }
        }

        // The other layer of a part may not be drawn, so it has no mesh to be found by: it is found by its name
        foreach (var file in found.ToList())
        {
            var linkedName = GetLinkedFileName(file.FileName);

            if (linkedName == null || Unsupported.Contains(linkedName) || !letters.Contains(char.ToLowerInvariant(linkedName[0])))
                continue;

            var linked = Files.TryGetValue(linkedName, out var decoded) ? decoded : DecodeFile(file.Entity, linkedName);

            if (linked != null && linked.Entity == file.Entity && !found.Contains(linked))
                found.Add(linked);
        }

        foreach (var file in found)
        {
            var hits = false;

            foreach (var body in file.Bodies)
            {
                var bodyWorld = GetBodyWorld(file, body);

                if (!TransformBounds(body.Bounds, bodyWorld).Intersects(boxBounds))
                    continue;

                var toBox = bodyWorld * worldToBox;
                var (removed, cut) = CollisionBoxSubtractor.Count(body.Vertices, body.Indices, toBox);

                if (removed + cut == 0)
                    continue;

                targets.Add(new SubtractTarget(file, body, toBox, removed, cut));
                hits = true;
            }

            if (hits && file.IsEdited)
                blocked.Add(file.FileName);
        }

        return targets;
    }

    /// <summary>The world box around a box moved by a matrix.</summary>
    private static BoundingBox TransformBounds(BoundingBox bounds, Matrix4x4 matrix)
    {
        var min = new Vector3(float.MaxValue);
        var max = new Vector3(float.MinValue);

        for (var i = 0; i < 8; i++)
        {
            var corner = new Vector3(
                (i & 1) == 0 ? bounds.Min.X : bounds.Max.X,
                (i & 2) == 0 ? bounds.Min.Y : bounds.Max.Y,
                (i & 4) == 0 ? bounds.Min.Z : bounds.Max.Z);

            var moved = Vector3.Transform(corner, matrix);
            min = Vector3.Min(min, moved);
            max = Vector3.Max(max, moved);
        }

        return new BoundingBox(min, max);
    }

    #endregion

    #region Apply

    /// <summary>
    /// Cuts the box out of every body it reaches, rebuilds them as one undoable action, and writes their binders (and
    /// Sekiro's walls that follow the low collision) into the project.
    /// </summary>
    private void ApplySubtract()
    {
        var targets = FindSubtractTargets(out var blocked);

        if (blocked.Count > 0)
        {
            ReportFailure(GetType(), CollisionEditorSource.Subtract, LOC.Get("MAP_CollisionEditor_Subtract_Failed_Pending", string.Join(", ", blocked)));
            return;
        }

        if (targets.Count == 0)
        {
            ReportFailure(GetType(), CollisionEditorSource.Subtract, LOC.Get("MAP_CollisionEditor_Subtract_Preview_None"));
            return;
        }

        var swaps = new List<CollisionEditorAction.ShapeSwap>();
        var remaps = new List<CollisionEditorAction.WallRemap>();
        var files = targets.Select(t => t.File).Distinct().ToList();
        var totalRemoved = 0;
        var totalCut = 0;

        foreach (var file in files)
        {
            var map = file.Entity.Container as MapContainer;

            // A rebuild renumbers the triangles, and Sekiro's WALL files mark the low collision's triangles by number
            var remapWalls = HasWallMarks(file);
            var newKeys = new Dictionary<uint, uint>();

            if (remapWalls && !View.WallBank.EnsureMapLoaded(file.MapId))
            {
                ReportFailure(GetType(), CollisionEditorSource.Subtract, LOC.Get("MAP_CollisionEditor_Commit_Walls_Unread", file.FileName));
                return;
            }

            foreach (var target in targets.Where(t => t.File == file))
            {
                var body = target.Body;
                CollisionBoxSubtractor.Result result;

                try
                {
                    result = CollisionBoxSubtractor.Subtract(body.Vertices, body.Indices, body.Materials, body.Keys, target.ToBox);
                }
                catch (Exception e)
                {
                    ReportFailure(GetType(), CollisionEditorSource.Subtract, LOC.Get("MAP_CollisionEditor_Subtract_Failed_Cut", file.FileName), e);
                    return;
                }

                if (!result.Changed)
                    continue;

                if (result.Indices.Count == 0)
                {
                    ReportFailure(GetType(), CollisionEditorSource.Subtract, LOC.Get("MAP_CollisionEditor_Subtract_Failed_Empty", file.FileName));
                    return;
                }

                CollisionEditorShapes.BuiltShape built;

                try
                {
                    // Built afresh, as the cut adds triangles and vertices: the older games' meshes are regrouped into
                    // a storage per material, as a commit of repainted triangles does
                    built = CollisionEditorShapes.BuildShape(body.Body.Shape, result.Vertices, result.Indices, result.Materials);
                }
                catch (Exception e)
                {
                    ReportFailure(GetType(), CollisionEditorSource.Subtract, LOC.Get("MAP_CollisionEditor_Commit_Build_Failed", file.FileName), e);
                    return;
                }

                if (built == null)
                {
                    ReportFailure(GetType(), CollisionEditorSource.Subtract, LOC.Get("MAP_CollisionEditor_Commit_Build_Failed", file.FileName));
                    return;
                }

                // Each kept triangle's key as decoded, to a key it was built with (a cut triangle's marks go to one
                // of its parts; a removed triangle's marks go with it)
                for (var t = 0; t < result.Keys.Count; t++)
                    newKeys.TryAdd(result.Keys[t], built.StoredKeys[t]);

                swaps.Add(new CollisionEditorAction.ShapeSwap(map, file.FileName, body.Body, body.Body.Shape, built.Shape));

                totalRemoved += result.Removed;
                totalCut += result.Cut;

                Smithbox.Log(this, LOC.Get("MAP_CollisionEditor_Subtract_Rebuilt", file.FileName,
                    result.Removed, result.Cut, body.TriangleCount, result.Indices.Count / 3));
            }

            if (remapWalls && swaps.Any(s => s.FileName == file.FileName))
            {
                var modelName = GetWallModelName(file);
                var before = View.WallBank.GetMarks(file.MapId, modelName);

                if (before != null && (before.Triangles.Count > 0 || before.NoJumpKeys.Count > 0))
                {
                    var after = WallBank.RemapMarks(before, newKeys);
                    remaps.Add(new CollisionEditorAction.WallRemap(map, file.MapId, modelName, before, after));
                }
            }
        }

        if (swaps.Count == 0)
        {
            ReportFailure(GetType(), CollisionEditorSource.Subtract, LOC.Get("MAP_CollisionEditor_Subtract_Preview_None"));
            return;
        }

        var written = files.Where(f => swaps.Any(s => s.FileName == f.FileName)).ToList();

        ApplyShapeSwaps(swaps, remaps);
        SubtractVersion++;

        var success = true;

        foreach (var (mapId, type) in written.Select(f => (f.MapId, f.BinderType)).Distinct())
            success &= View.HavokCollisionBank.SaveMapCollision(mapId, type);

        foreach (var mapId in remaps.Select(r => r.MapId).Distinct())
            success &= View.WallBank.SaveWalls(mapId);

        var names = string.Join(", ", written.Select(f => f.FileName));

        if (success)
            ReportSuccess(GetType(), CollisionEditorSource.Subtract, true, LOC.Get("MAP_CollisionEditor_Subtract_Applied", totalRemoved, totalCut, names));
        else
            ReportFailure(GetType(), CollisionEditorSource.Subtract, LOC.Get("MAP_CollisionEditor_Commit_Save_Failed", names));
    }

    #endregion

    #region Angles

    // The box's rotation as angles in degrees about X, Y and Z, as System.Numerics' yaw (Y), pitch (X) and roll (Z)
    private static Quaternion FromAngles(Vector3 degrees)
    {
        var radians = degrees * (MathF.PI / 180f);
        return Quaternion.CreateFromYawPitchRoll(radians.Y, radians.X, radians.Z);
    }

    private static Vector3 ToAngles(Quaternion rotation)
    {
        var m = Matrix4x4.CreateFromQuaternion(rotation);
        var pitch = MathF.Asin(Math.Clamp(-m.M32, -1f, 1f));
        float yaw;
        float roll;

        if (MathF.Abs(MathF.Cos(pitch)) > 1e-4f)
        {
            yaw = MathF.Atan2(m.M31, m.M33);
            roll = MathF.Atan2(m.M12, m.M22);
        }
        else
        {
            // Pitched straight up or down: the yaw and roll turn about the same axis, so it is all yaw
            yaw = MathF.Atan2(-m.M13, m.M11);
            roll = 0f;
        }

        return new Vector3(pitch, yaw, roll) * (180f / MathF.PI);
    }

    #endregion
}

/// <summary>
/// Gizmo target for the Subtract tab's box: it moves, turns and sizes the box (the gizmo's scale is the box's size).
/// </summary>
public class CollisionBoxGizmoTarget : IGizmoTarget
{
    private readonly CollisionEditorTool Owner;

    public CollisionBoxGizmoTarget(CollisionEditorTool owner)
    {
        Owner = owner;
    }

    public bool HasTarget => Owner.HasSubtractGizmo;

    public bool AllowScale => true;

    public Transform GetTransform()
    {
        return HasTarget ? Owner.GetSubtractTransform() : Transform.Default;
    }

    public void Preview(Transform original, Transform current, GizmoState.GizmosMode mode)
    {
        if (HasTarget)
            Owner.SetSubtractTransform(current, mode);
    }

    public void Commit(Transform original, Transform current, GizmoState.GizmosMode mode)
    {
        Preview(original, current, mode);
    }
}
