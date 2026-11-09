using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;
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
/// Moves the selection along a world axis until its bounding box meets the nearest drawn FLVER mesh in that
/// direction. The surface is found on the GPU by <see cref="SnapDepthPass" />: the scene's depth, drawn along the
/// direction over the selection's footprint without the selection, looked at from the plane through the centre of
/// each box, so a selection already sunk into the ground is lifted out onto it.
/// While the tool's section is open, a wire arrow from the face of each box to where it lands previews the snap,
/// following the selection a few frames behind. See Documentation/Claude/Snap To Mesh GPU.md.
/// </summary>
public class SnapToMeshAction
{
    public MapEditorView View;
    public ProjectEntry Project;

    public SnapToMeshAction(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    // Matches CFG.MapEditor_SnapToMesh_Direction
    private static readonly (string Name, Vector3 Direction)[] Directions =
    {
        ("Down (-Y)", -Vector3.UnitY),
        ("Up (+Y)", Vector3.UnitY),
        ("-X", -Vector3.UnitX),
        ("+X", Vector3.UnitX),
        ("-Z", -Vector3.UnitZ),
        ("+Z", Vector3.UnitZ),
    };

    private static Vector3 CurrentDirection =>
        Directions[Math.Clamp(CFG.Current.MapEditor_SnapToMesh_Direction, 0, Directions.Length - 1)].Direction;

    #region Interface

    /// <summary>
    /// Context Menu
    /// </summary>
    public void OnContext(Entity ent)
    {
        if (ent.WrappedObject is IMsbPart or IMsbRegion)
        {
            if (ImGui.Selectable(LOC.Get("MAP_SnapToMesh_Apply")))
            {
                ApplySnap();
            }
            GUI.Tooltip(LOC.Get("MAP_SnapToMesh_Apply_TT"));
        }
    }

    /// <summary>
    /// Edit Menu
    /// </summary>
    public void OnMenu()
    {
        if (ImGui.MenuItem(LOC.Get("MAP_SnapToMesh_Apply")))
        {
            ApplySnap();
        }
        GUI.Tooltip(LOC.Get("MAP_SnapToMesh_Apply_TT"));
    }

    /// <summary>
    /// Tool Window
    /// </summary>
    public void OnToolWindow()
    {
        // The preview is drawn while this section is shown
        ShownFrame = ImGui.GetFrameCount();

        GUI.WrappedText(LOC.Get("MAP_SnapToMesh_Hint"));

        if (!string.IsNullOrEmpty(PreviewStatus))
        {
            GUI.WrappedText(PreviewStatus);
        }

        // Direction
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_SnapToMesh_Direction_Header"), LOC.Get("MAP_SnapToMesh_Direction_Header_TT"));

        var directionIndex = Math.Clamp(CFG.Current.MapEditor_SnapToMesh_Direction, 0, Directions.Length - 1);

        ImGui.SetNextItemWidth(-1);
        if (ImGui.BeginCombo("##snapDirection", Directions[directionIndex].Name))
        {
            for (var i = 0; i < Directions.Length; i++)
            {
                if (ImGui.Selectable(Directions[i].Name, i == directionIndex))
                {
                    CFG.Current.MapEditor_SnapToMesh_Direction = i;
                }
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("MAP_SnapToMesh_Direction_TT"));

        // Offset
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_SnapToMesh_Offset_Header"), LOC.Get("MAP_SnapToMesh_Offset_Header_TT"));

        ImGui.SetNextItemWidth(-1);
        ImGui.InputFloat("##snapOffset", ref CFG.Current.MapEditor_SnapToMesh_Offset, 0.01f, 0.1f, "%.3f");
        GUI.Tooltip(LOC.Get("MAP_SnapToMesh_Offset_TT"));

        // Max Distance
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_SnapToMesh_MaxDistance_Header"), LOC.Get("MAP_SnapToMesh_MaxDistance_Header_TT"));

        ImGui.SetNextItemWidth(-1);
        if (ImGui.InputFloat("##snapMaxDistance", ref CFG.Current.MapEditor_SnapToMesh_MaxDistance, 1.0f, 10.0f, "%.1f"))
        {
            if (CFG.Current.MapEditor_SnapToMesh_MaxDistance < 0.01f)
                CFG.Current.MapEditor_SnapToMesh_MaxDistance = 0.01f;
        }
        GUI.Tooltip(LOC.Get("MAP_SnapToMesh_MaxDistance_TT"));

        // Options
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_SnapToMesh_Options_Header"), LOC.Get("MAP_SnapToMesh_Options_Header_TT"));

        ImGui.Checkbox($"{LOC.Get("MAP_SnapToMesh_Individually")}##snapIndividually", ref CFG.Current.MapEditor_SnapToMesh_Individually);
        GUI.Tooltip(LOC.Get("MAP_SnapToMesh_Individually_TT"));

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_SnapToMesh_Actions_Header"), "");

        GUI.MultiButtonInput("snapToMeshActions",
            "applySnap", LOC.Get("MAP_SnapToMesh_Apply"), LOC.Get("MAP_SnapToMesh_Apply_TT"), ApplySnap);
    }

    #endregion

    #region State

    /// <summary>What a request was made from: the groups, and the hashes that say whether it still applies.</summary>
    private sealed class RequestState
    {
        public SnapDepthPass Pass;

        /// <summary>The selection and settings, without positions: whether the groups are the same groups</summary>
        public int GroupKey;

        /// <summary>The selection and settings with positions: whether nothing has moved since</summary>
        public int Signature;

        public List<List<Entity>> Groups;
    }

    // The last frame the tool's section was drawn, so the preview shows only while the tool is open
    private int ShownFrame = -1;
    private bool PreviewWasShown;

    // The last request made, when, and the newest result taken from the pass
    private int RequestedSignature;
    private int RequestFrame = -1000;
    private SnapDepthResult CurrentResult;

    /// <summary>Frames after which an idle preview asks again, for what has changed around the selection</summary>
    private const int RefreshFrames = 30;

    // A snap waiting for a result for the selection as it is, and since when
    private bool PendingApply;
    private int PendingApplyFrame;

    /// <summary>Frames a snap waits for its result before giving up</summary>
    private const int ApplyTimeoutFrames = 120;

    private string PreviewStatus = "";

    /// <summary>
    /// Asks the GPU for the snap while the tool is open or a snap is waiting, applies a waiting snap once its result
    /// arrives, and keeps the preview arrows up to date. Called each frame, after the Tools window.
    /// </summary>
    public void Update()
    {
        var frame = ImGui.GetFrameCount();

        // Drawn this frame or the last, as the Tools window may be drawn before or after this
        var shown = ShownFrame >= frame - 1 && CFG.Current.Interface_MapEditor_Tool_SnapToMesh;

        if (!shown && PreviewWasShown)
        {
            ReleasePreview();
            PreviewStatus = "";
        }

        PreviewWasShown = shown;

        if (!shown && !PendingApply)
            return;

        var pass = GetPass();
        var sels = GetSelection();

        if (pass == null)
        {
            PreviewStatus = LOC.Get("MAP_SnapToMesh_Unavailable");
            ShowArrows(new List<(Vector3, Vector3)>());
            FailPendingApply("Snap to Mesh needs a viewport drawn with shadows enabled.");
            return;
        }

        if (sels.Count == 0)
        {
            PreviewStatus = "";
            ShowArrows(new List<(Vector3, Vector3)>());
            FailPendingApply("No object selected.");
            return;
        }

        var groups = GetGroups(sels);
        var groupKey = GetGroupKey(sels);
        var signature = GetSignature(sels, groupKey);

        // Take the newest result of this viewport's pass
        if (pass.LatestResult != null && pass.LatestResult != CurrentResult &&
            pass.LatestResult.Request.Tag is RequestState state && state.Pass == pass)
        {
            CurrentResult = pass.LatestResult;
        }

        var current = CurrentResult?.Request.Tag as RequestState;
        var currentMatches = current != null && current.Pass == pass && current.GroupKey == groupKey;

        // A waiting snap applies once there is a result for the selection exactly as it is
        if (PendingApply)
        {
            if (currentMatches && current.Signature == signature)
            {
                PendingApply = false;
                Apply(groups, CurrentResult);
            }
            else if (frame - PendingApplyFrame > ApplyTimeoutFrames)
            {
                FailPendingApply("Snap to Mesh timed out waiting for the viewport. Make sure a map viewport is shown.");
            }
        }

        // Ask again when the selection or settings have changed, or now and then for the scene around it
        if (pass.IsIdle && (signature != RequestedSignature || frame - RequestFrame >= RefreshFrames))
        {
            Request(pass, sels, groups, groupKey, signature);
            RequestedSignature = signature;
            RequestFrame = frame;
        }

        if (shown)
        {
            UpdatePreview(groups, currentMatches ? CurrentResult : null);
        }
    }

    private void FailPendingApply(string message)
    {
        if (!PendingApply)
            return;

        PendingApply = false;
        Smithbox.LogError<SnapToMeshAction>(message);
    }

    /// <summary>The depth pass of the viewport the snap is found in: the active one</summary>
    private SnapDepthPass GetPass()
    {
        return (View.ViewportHandler.ActiveViewport?.Viewport as VulkanViewport)?.ViewPipeline?.SnapDepth;
    }

    private List<Entity> GetSelection()
    {
        if (!View.ViewportSelection.IsSelection())
            return new List<Entity>();

        return View.ViewportSelection.GetFilteredSelection<Entity>(o => o.HasTransform).ToList();
    }

    private static List<List<Entity>> GetGroups(List<Entity> sels)
    {
        var groups = CFG.Current.MapEditor_SnapToMesh_Individually
            ? sels.Select(s => new List<Entity> { s }).ToList()
            : new List<List<Entity>> { sels };

        return groups.Take(SnapDepthPass.MaxGroups).ToList();
    }

    /// <summary>A hash of the selection and the settings a request depends on, leaving out where things are.</summary>
    private static int GetGroupKey(List<Entity> sels)
    {
        var hash = new HashCode();

        hash.Add(CFG.Current.MapEditor_SnapToMesh_Direction);
        hash.Add(CFG.Current.MapEditor_SnapToMesh_MaxDistance);
        hash.Add(CFG.Current.MapEditor_SnapToMesh_Individually);
        hash.Add(sels.Count);

        foreach (var sel in sels)
            hash.Add(sel);

        return hash.ToHashCode();
    }

    /// <summary>The group key with where each selected object is.</summary>
    private static int GetSignature(List<Entity> sels, int groupKey)
    {
        var hash = new HashCode();
        hash.Add(groupKey);

        foreach (var sel in sels)
            hash.Add(sel.GetWorldMatrix());

        // Never 0, which marks no request
        var value = hash.ToHashCode();
        return value == 0 ? 1 : value;
    }

    private static void Request(SnapDepthPass pass, List<Entity> sels, List<List<Entity>> groups, int groupKey, int signature)
    {
        var excluded = new HashSet<ISelectable>();

        foreach (var sel in sels)
            AddWithChildren(sel, excluded);

        pass.Request(new SnapDepthRequest
        {
            Direction = CurrentDirection,
            MaxDistance = CFG.Current.MapEditor_SnapToMesh_MaxDistance,
            Groups = groups.Select(GetGroupBounds).ToList(),
            Excluded = excluded,
            Tag = new RequestState { Pass = pass, GroupKey = groupKey, Signature = signature, Groups = groups }
        });
    }

    private static void AddWithChildren(Entity ent, HashSet<ISelectable> set)
    {
        if (!set.Add(ent))
            return;

        foreach (var child in ent.Children)
            AddWithChildren(child, set);
    }

    /// <summary>
    /// The world offset that puts a group's leading face on the surface a result found for it, with the configured
    /// offset, or null if there was none. Measured from where the group is now, so a result a few frames old still
    /// lands on the right surface.
    /// </summary>
    private static Vector3? GetDelta(BoundingBox bounds, SnapDepthResult result, int group, out Vector3 faceCenter)
    {
        var direction = result.Request.Direction;
        faceCenter = bounds.GetCenter() + direction * SnapDepthPass.FaceDistance(bounds, direction);

        if (group >= result.SurfacePlanes.Length || result.SurfacePlanes[group] is not float plane)
            return null;

        var move = plane - Vector3.Dot(direction, faceCenter) + CFG.Current.MapEditor_SnapToMesh_Offset;
        return direction * move;
    }

    #endregion

    #region Effect

    /// <summary>
    /// Effect: snaps with a result for the selection as it is, asking for one if there isn't one yet.
    /// </summary>
    public void ApplySnap()
    {
        if (!View.ViewportSelection.IsSelection() || GetSelection().Count == 0)
        {
            Smithbox.LogError<SnapToMeshAction>("No object with a transform is selected.");
            return;
        }

        if (GetPass() == null)
        {
            Smithbox.LogError<SnapToMeshAction>("Snap to Mesh needs a viewport drawn with shadows enabled.");
            return;
        }

        // Update applies it as soon as a result for the selection as it is arrives, which may be this frame
        PendingApply = true;
        PendingApplyFrame = ImGui.GetFrameCount();
    }

    private void Apply(List<List<Entity>> groups, SnapDepthResult result)
    {
        var actions = new List<ViewportAction>();
        var snapped = 0;
        var total = groups.Sum(g => g.Count);

        for (var g = 0; g < groups.Count; g++)
        {
            if (GetDelta(GetGroupBounds(groups[g]), result, g, out _) is not Vector3 delta)
                continue;

            foreach (var sel in groups[g])
                actions.Add(BuildMoveAction(sel, delta));

            snapped += groups[g].Count;
        }

        if (actions.Count == 0)
        {
            Smithbox.LogError<SnapToMeshAction>(
                $"No mesh found within {CFG.Current.MapEditor_SnapToMesh_MaxDistance} units in the snap direction.");
            View.DelayPicking();
            return;
        }

        View.ViewportActionManager.ExecuteAction(new ViewportCompoundAction(actions));

        if (snapped < total)
        {
            Smithbox.Log<SnapToMeshAction>($"Snapped {snapped} of {total} objects; the rest had no mesh in range.");
        }

        View.DelayPicking();
    }

    /// <summary>
    /// The world bounds of the group's meshes. An entity without a mesh counts as the point of its position.
    /// </summary>
    private static BoundingBox GetGroupBounds(List<Entity> group)
    {
        BoundingBox? combined = null;

        foreach (var ent in group)
        {
            var bounds = GetEntityBounds(ent);
            combined = combined == null ? bounds : BoundingBox.Combine(combined.Value, bounds);
        }

        return combined ?? new BoundingBox(Vector3.Zero, Vector3.Zero);
    }

    private static BoundingBox GetEntityBounds(Entity ent)
    {
        if (ent.RenderSceneMesh != null)
        {
            var bounds = ent.RenderSceneMesh.GetBounds();

            if (IsValid(bounds))
                return bounds;
        }

        var position = ent.GetWorldMatrix().Translation;
        return new BoundingBox(position, position);
    }

    private static bool IsValid(BoundingBox bounds)
    {
        return float.IsFinite(bounds.Min.X) && float.IsFinite(bounds.Min.Y) && float.IsFinite(bounds.Min.Z)
               && float.IsFinite(bounds.Max.X) && float.IsFinite(bounds.Max.Y) && float.IsFinite(bounds.Max.Z)
               && bounds.Min.X <= bounds.Max.X && bounds.Min.Y <= bounds.Max.Y && bounds.Min.Z <= bounds.Max.Z
               && bounds.Max != bounds.Min;
    }

    /// <summary>
    /// An action that moves the entity by a world offset, keeping its rotation and scale.
    /// </summary>
    private static ViewportAction BuildMoveAction(Entity ent, Vector3 worldDelta)
    {
        var localT = ent.GetLocalTransform();
        var rootT = ent.GetRootTransform();

        // The offset in the space of the entity's parent
        var localDelta = Vector3.Transform(worldDelta, Quaternion.Inverse(rootT.Rotation));

        Transform newT = new(localT.Position + localDelta, localT.EulerRotation);

        return ent.GetUpdateTransformAction(newT);
    }

    #endregion

    #region Preview

    private static readonly Color PreviewColor = Color.FromArgb(255, 255, 196, 0);

    // A unit line along X, and an arrowhead with its tip at the origin pointing along X, shared by every arrow
    private RenderScene PreviewScene;
    private DbgPrimLineBatch ShaftPrim;
    private DbgPrimLineBatch HeadPrim;
    private readonly List<(DebugPrimitiveRenderableProxy Shaft, DebugPrimitiveRenderableProxy Head)> Arrows = new();

    // The arrows last drawn, so the proxies are only moved when they change
    private int ArrowHash;

    /// <summary>The arrows and status for the current result, measured from where the groups are now.</summary>
    private void UpdatePreview(List<List<Entity>> groups, SnapDepthResult result)
    {
        if (result == null)
        {
            PreviewStatus = LOC.Get("MAP_SnapToMesh_Preview_Waiting");
            ShowArrows(new List<(Vector3, Vector3)>());
            return;
        }

        var moves = new List<(Vector3, Vector3)>();
        var moved = 0;
        var total = 0;

        for (var g = 0; g < groups.Count; g++)
        {
            total += groups[g].Count;

            if (GetDelta(GetGroupBounds(groups[g]), result, g, out var faceCenter) is not Vector3 delta)
                continue;

            moves.Add((faceCenter, faceCenter + delta));
            moved += groups[g].Count;
        }

        PreviewStatus = moved == 0
            ? LOC.Get("MAP_SnapToMesh_Preview_NoHit")
            : LOC.Get("MAP_SnapToMesh_Preview_Status", moved, total);

        ShowArrows(moves);
    }

    /// <summary>Draws one arrow per move, from the face of the bounding box to where it lands.</summary>
    private void ShowArrows(List<(Vector3 From, Vector3 To)> moves)
    {
        var hash = new HashCode();
        hash.Add(moves.Count);

        foreach (var (from, to) in moves)
        {
            hash.Add(from);
            hash.Add(to);
        }

        var arrowHash = hash.ToHashCode();

        var scene = View.ViewportHandler.RenderScene;

        if (scene != PreviewScene)
        {
            ReleasePreview();
            PreviewScene = scene;
        }
        else if (arrowHash == ArrowHash)
        {
            return;
        }

        ArrowHash = arrowHash;

        if (PreviewScene == null)
            return;

        ShaftPrim ??= new DbgPrimLineBatch(new List<(Vector3, Vector3)> { (Vector3.Zero, Vector3.UnitX) });
        HeadPrim ??= new DbgPrimLineBatch(BuildHeadLines());

        for (var i = 0; i < moves.Count; i++)
        {
            if (i >= Arrows.Count)
                Arrows.Add((MakeProxy(ShaftPrim), MakeProxy(HeadPrim)));

            var (shaft, head) = Arrows[i];
            var (from, to) = moves[i];
            var length = Vector3.Distance(from, to);

            // A move too short to draw a line along: show only the head, at the landing point
            shaft.Visible = length > 1e-3f;

            if (shaft.Visible)
                shaft.World = LineWorld(from, to);

            var pointing = length > 1e-3f ? Vector3.Normalize(to - from) : CurrentDirection;
            var headSize = Math.Clamp(length * 0.25f, 0.1f, 1.0f);

            head.World = Matrix4x4.CreateScale(headSize) * LineWorld(Vector3.Zero, pointing) * Matrix4x4.CreateTranslation(to);
            head.Visible = true;
        }

        for (var i = moves.Count; i < Arrows.Count; i++)
        {
            Arrows[i].Shaft.Visible = false;
            Arrows[i].Head.Visible = false;
        }
    }

    /// <summary>Four lines back from the tip at the origin, and the square that joins their ends.</summary>
    private static List<(Vector3, Vector3)> BuildHeadLines()
    {
        const float width = 0.35f;

        var corners = new[]
        {
            new Vector3(-1, width, 0),
            new Vector3(-1, 0, width),
            new Vector3(-1, -width, 0),
            new Vector3(-1, 0, -width),
        };

        var lines = new List<(Vector3, Vector3)>();

        for (var i = 0; i < corners.Length; i++)
        {
            lines.Add((Vector3.Zero, corners[i]));
            lines.Add((corners[i], corners[(i + 1) % corners.Length]));
        }

        return lines;
    }

    private DebugPrimitiveRenderableProxy MakeProxy(IDbgPrim prim)
    {
        return new DebugPrimitiveRenderableProxy(PreviewScene.OpaqueRenderables, prim)
        {
            // Clicks go to what is behind, and the arrow shows through the map
            Pickable = false,
            RenderOverlay = true,
            BaseColor = PreviewColor,
            HighlightedColor = PreviewColor,
        };
    }

    /// <summary>
    /// The world matrix that takes the unit line from the origin along X onto the line from a to b. The other two
    /// axes are unit length and independent of X, so the matrix can be inverted for culling.
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

    private void ReleasePreview()
    {
        foreach (var (shaft, head) in Arrows)
        {
            shaft.Dispose();
            head.Dispose();
        }

        Arrows.Clear();
        PreviewScene = null;
        ArrowHash = 0;
    }

    #endregion
}
