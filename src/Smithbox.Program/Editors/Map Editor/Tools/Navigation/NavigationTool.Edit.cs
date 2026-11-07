using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using Veldrid.Utilities;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The Edit tab: moves navmesh vertices in the viewport, as the Collision Editor does for collision. While editing, a
/// cursor sphere sits where the mouse ray meets the drawn navmesh set, and the nearest corner of the face under it can
/// be raised, lowered or dragged, with the vertices around it following within a radius. Vertices of the map's other
/// navmeshes that lie on the moved one follow too, so connected edges stay joined. The edits are drawn in place of the
/// overlay of their navmeshes until they are committed, which swaps the edited files' documents as one undoable action
/// (NavigationVertexEditAction); the map's save writes them. See NavVertexEdit.
/// </summary>
public partial class NavigationTool
{
    private bool VertexEditActive;

    /// <summary>The map the working copies belong to.</summary>
    private MapContainer VertexMap;

    /// <summary>The working copies of the files looked at, by file name.</summary>
    private readonly Dictionary<string, NavVertexFile> VertexFiles = new(StringComparer.OrdinalIgnoreCase);

    private sealed record VertexHit(NavVertexFile File, int Face, int Vertex, Vector3 Point, float Distance);

    /// <summary>The vertices of one file that an edit moves, with how much of the move each takes.</summary>
    private sealed record VertexMove(NavVertexFile File, List<(int Vertex, float Weight)> Falloff);

    private VertexHit VertexHover;
    private VertexHit VertexDrag;
    private float VertexDragHeight;
    private Vector3 VertexDragGrab;
    private List<VertexMove> VertexDragMoves;

    private float VertexStep = 0.1f;
    private float VertexRadius = 0f;
    private bool VertexLinkNeighbours = true;
    private bool VertexLinkOtherSet;
    private float VertexLinkDistance = 0.1f;
    private float VertexCursorSize = 0.15f;
    private float VertexCursorOpacity = 20f;

    /// <summary>Vertices of the edited navmesh this close to the moved one are the same vertex, and move fully.</summary>
    private const float VertexWeld = 0.001f;

    private const float VertexSphereRadius = 0.1f;
    private const float VertexMaxMarkerSize = 0.1f;

    /// <summary>The preview of each edited file, with the revision and overlay build it was drawn from.</summary>
    private readonly Dictionary<string, (NavigationOverlayRenderer Renderer, int Revision, int Build)> VertexPreviews = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Replaced previews, drawn a few frames more alongside the new one, so a drag never leaves a gap.</summary>
    private readonly List<(NavigationOverlayRenderer Renderer, int Frame)> RetiringVertexPreviews = new();
    private const int VertexPreviewOverlapFrames = 2;

    /// <summary>Counts overlay builds, so the previews are drawn again with new layers, opacity or legend.</summary>
    private int OverlayBuildCount;

    /// <summary>The previewed files the overlay was last built without.</summary>
    private HashSet<string> OverlayWithout = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>The Map Editor's own drawings of navmeshes hidden while their previews are shown.</summary>
    private readonly HashSet<Entity> HiddenNavmeshDrawings = new();

    /// <summary>Each navmesh entry's entity in the edited map, and where it is drawn, gathered once a frame.</summary>
    private readonly Dictionary<NVA_ER.Navmesh, (Entity Entity, Matrix4x4 World)> VertexEntries = new(ReferenceEqualityComparer.Instance);

    private (HashSet<(int, int)> InMap, HashSet<(int, int)> CrossMap) VertexEnds;
    private NavigationMap VertexEndsFor;

    private DebugPrimitiveRenderableProxy VertexCursorWire;
    private DebugPrimitiveRenderableProxy VertexCursorFill;
    private DebugPrimitiveRenderableProxy VertexMarker;

    private string VertexMessage;
    private bool VertexMessageIsError;

    private bool HasVertexEdits => VertexFiles.Values.Any(f => f.IsEdited);

    #region Tab

    private void DisplayEditTab()
    {
        GUI.WrappedText(LOC.Get("MAP_Navigation_Edit_Description"));

        if (!ShowOverlay)
            GUI.WrappedText(LOC.Get("MAP_Navigation_Edit_Needs_Overlay"));

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Edit_Actions_Header"), LOC.Get("MAP_Navigation_Edit_Actions_Header_TT"));

        GUI.MultiButtonInput("navigationEditToggle",
            "navigationEditToggleButton",
            VertexEditActive ? LOC.Get("MAP_Navigation_Edit_Stop") : LOC.Get("MAP_Navigation_Edit_Start"),
            VertexEditActive ? LOC.Get("MAP_Navigation_Edit_Stop_TT") : LOC.Get("MAP_Navigation_Edit_Start_TT"),
            ToggleVertexEdit);

        var hasEdits = HasVertexEdits;

        GUI.ConditionalMultiButtonInput("navigationEditActions",
            "navigationEditCommit", LOC.Get("MAP_Navigation_Edit_Commit"), LOC.Get("MAP_Navigation_Edit_Commit_TT"),
            CommitVertexEdits, hasEdits,

            "navigationEditDiscard", LOC.Get("MAP_Navigation_Edit_Discard"), LOC.Get("MAP_Navigation_Edit_Discard_TT"),
            DiscardVertexEdits, hasEdits);

        if (VertexMessage != null)
        {
            if (VertexMessageIsError)
                GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, VertexMessage);
            else
                GUI.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, VertexMessage);
        }

        // Linked Navmeshes
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Edit_Link_Header"), LOC.Get("MAP_Navigation_Edit_Link_Header_TT"));

        ImGui.Checkbox($"{LOC.Get("MAP_Navigation_Edit_Link_Neighbours")}##navigationEditLinkNeighbours", ref VertexLinkNeighbours);
        GUI.Tooltip(LOC.Get("MAP_Navigation_Edit_Link_Neighbours_TT"));

        ImGui.Checkbox($"{LOC.Get("MAP_Navigation_Edit_Link_Other_Set")}##navigationEditLinkOtherSet", ref VertexLinkOtherSet);
        GUI.Tooltip(LOC.Get("MAP_Navigation_Edit_Link_Other_Set_TT"));

        // Link Distance
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Edit_Link_Distance_Header"), LOC.Get("MAP_Navigation_Edit_Link_Distance_Header_TT"));

        ImGui.SetNextItemWidth(-1);
        if (ImGui.InputFloat("##navigationEditLinkDistance", ref VertexLinkDistance))
            VertexLinkDistance = Math.Max(0f, VertexLinkDistance);
        GUI.Tooltip(LOC.Get("MAP_Navigation_Edit_Link_Distance_Input_TT"));

        // Step
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Edit_Step_Header"), LOC.Get("MAP_Navigation_Edit_Step_Header_TT"));

        ImGui.SetNextItemWidth(-1);
        if (ImGui.InputFloat("##navigationEditStep", ref VertexStep))
            VertexStep = Math.Max(0.001f, VertexStep);
        GUI.Tooltip(LOC.Get("MAP_Navigation_Edit_Step_Input_TT"));

        // Radius
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Edit_Radius_Header"), LOC.Get("MAP_Navigation_Edit_Radius_Header_TT"));

        ImGui.SetNextItemWidth(-1);
        if (ImGui.InputFloat("##navigationEditRadius", ref VertexRadius))
            VertexRadius = Math.Max(0f, VertexRadius);
        GUI.Tooltip(LOC.Get("MAP_Navigation_Edit_Radius_Input_TT"));

        // Cursor Size
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Edit_Cursor_Size_Header"), LOC.Get("MAP_Navigation_Edit_Cursor_Size_Header_TT"));

        ImGui.SetNextItemWidth(-1);
        if (ImGui.InputFloat("##navigationEditCursorSize", ref VertexCursorSize))
            VertexCursorSize = Math.Max(0.01f, VertexCursorSize);
        GUI.Tooltip(LOC.Get("MAP_Navigation_Edit_Cursor_Size_Input_TT"));

        // Cursor Opacity
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Edit_Cursor_Opacity_Header"), LOC.Get("MAP_Navigation_Edit_Cursor_Opacity_Header_TT"));

        ImGui.SetNextItemWidth(-1);
        ImGui.SliderFloat("##navigationEditCursorOpacity", ref VertexCursorOpacity, 0f, 100f, "%.0f%%");
        GUI.Tooltip(LOC.Get("MAP_Navigation_Edit_Cursor_Opacity_Input_TT"));

        // Controls
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Edit_Controls_Header"), LOC.Get("MAP_Navigation_Edit_Controls_Header_TT"));

        GUI.WrappedText(LOC.Get("MAP_Navigation_Edit_Control_Raise", InputManager.GetHint(KeybindID.MapEditor_NavigationEditor_Raise)));
        GUI.WrappedText(LOC.Get("MAP_Navigation_Edit_Control_Lower", InputManager.GetHint(KeybindID.MapEditor_NavigationEditor_Lower)));
        GUI.WrappedText(LOC.Get("MAP_Navigation_Edit_Control_Drag", InputManager.GetHint(KeybindID.MapEditor_NavigationEditor_Drag)));

        DisplayVertexStatus();
    }

    /// <summary>What is under the cursor, and which files have edits waiting to be committed.</summary>
    private void DisplayVertexStatus()
    {
        // Cursor
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Edit_Cursor_Header"), LOC.Get("MAP_Navigation_Edit_Cursor_Header_TT"));

        if (!VertexEditActive)
        {
            GUI.WrappedText(LOC.Get("MAP_Navigation_Edit_Cursor_Inactive"));
        }
        else if ((VertexDrag ?? VertexHover) is { } hit)
        {
            var position = hit.File.WorldVertex(hit.Vertex);

            GUI.WrappedText(LOC.Get("MAP_Navigation_Edit_Cursor_File", hit.File.File.Name, (uint)hit.File.Entry.NameID, hit.File.Entry.ModelID));
            GUI.WrappedText(LOC.Get("MAP_Navigation_Edit_Cursor_Vertex", hit.Vertex, hit.Face,
                $"{position.X:0.###}, {position.Y:0.###}, {position.Z:0.###}"));
        }
        else
        {
            GUI.WrappedText(LOC.Get("MAP_Navigation_Edit_Cursor_None"));
        }

        // Pending Edits
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Edit_Pending_Header"), LOC.Get("MAP_Navigation_Edit_Pending_Header_TT"));

        var edited = VertexFiles.Values.Where(f => f.IsEdited).OrderBy(f => f.File.Name).ToList();

        if (edited.Count == 0)
        {
            GUI.WrappedText(LOC.Get("MAP_Navigation_Edit_Pending_None"));
            return;
        }

        if (VertexMap != null && VertexMap != OverlayContainer)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MAP_Navigation_Edit_Pending_Other_Map", VertexMap.Name));

        foreach (var file in edited)
            GUI.WrappedText(LOC.Get("MAP_Navigation_Edit_Pending_File", file.File.Name, file.MovedCount));
    }

    private void ToggleVertexEdit()
    {
        if (VertexEditActive)
        {
            StopVertexEdit();
            return;
        }

        VertexEditActive = true;
        PickMode = false;
        PaintMode = false;
        LinkMode = false;
        SetPicked(null);
    }

    /// <summary>Hides the cursor. Pending edits are kept; the files without edits are read again when next needed.</summary>
    private void StopVertexEdit()
    {
        if (!VertexEditActive)
            return;

        VertexEditActive = false;
        VertexDrag = null;
        VertexHover = null;
        DisposeVertexCursor();

        foreach (var name in VertexFiles.Where(e => !e.Value.IsEdited).Select(e => e.Key).ToList())
            VertexFiles.Remove(name);
    }

    private void DiscardVertexEdits()
    {
        VertexDrag = null;

        foreach (var file in VertexFiles.Values)
            file.Reset();

        VertexMessage = LOC.Get("MAP_Navigation_Edit_Discarded");
        VertexMessageIsError = false;
    }

    /// <summary>
    /// Builds each edited file again with its moved vertices (NavVertexEdit.Build) and puts them in the map's navmesh
    /// files as one undoable action. Nothing is changed if any file can't be built.
    /// </summary>
    private void CommitVertexEdits()
    {
        VertexDrag = null;

        var map = VertexMap;
        var edited = VertexFiles.Values.Where(f => f.IsEdited).ToList();

        if (map == null || edited.Count == 0)
            return;

        var changes = new List<NavFileChange>();
        int moved = 0;

        foreach (var file in edited)
        {
            Havok.HavokDocument document = null;

            try
            {
                document = NavVertexEdit.Build(file.File.Document, file.Vertices);
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("MAP_Navigation_Edit_Commit_Failed", file.File.Name), e);
            }

            if (document == null)
            {
                VertexMessage = LOC.Get("MAP_Navigation_Edit_Commit_Failed", file.File.Name);
                VertexMessageIsError = true;
                return;
            }

            moved += file.MovedCount;
            changes.Add(new NavFileChange(file.File.Name, file.File.Document, document));
        }

        // The Map Editor's drawing of the map is brought up to date after it, as after a model edit
        ModelEditMap = map;
        View.ViewportActionManager.ExecuteAction(new NavigationVertexEditAction(map, Bank, changes, OnModelEdited));

        VertexMessage = LOC.Get("MAP_Navigation_Edit_Committed", changes.Count, moved);
        VertexMessageIsError = false;
        Smithbox.Log(this, VertexMessage);
    }

    #endregion

    #region Viewport

    /// <summary>
    /// Called each frame by HandleViewportInput: handles the cursor and keys while editing, and keeps the previews of
    /// the edited files up to date. Returns true while a vertex is dragged, so the viewport's clicks don't select.
    /// </summary>
    private bool UpdateVertexEdit(Ray ray, bool canInteract)
    {
        DropStaleVertexFiles();
        GatherVertexEntries();

        if (VertexEditActive)
            HandleVertexKeys(ray, canInteract);

        UpdateVertexPreviews();
        UpdateVertexCursor();

        return VertexDrag != null;
    }

    /// <summary>
    /// Drops the working copies whose file has another document (after a commit, undo or redo) or whose map was
    /// unloaded; their edits belonged to the document that went.
    /// </summary>
    private void DropStaleVertexFiles()
    {
        if (VertexMap != null && VertexMap.LoadState != MapContentLoadState.Loaded)
        {
            VertexFiles.Clear();
            HiddenNavmeshDrawings.Clear();
            VertexMap = null;
        }

        List<string> stale = null;

        foreach (var (name, file) in VertexFiles)
        {
            if (!ReferenceEquals(Bank.GetNavmeshDocument(name), file.File.Document))
                (stale ??= new()).Add(name);
        }

        if (stale != null)
        {
            foreach (var name in stale)
                VertexFiles.Remove(name);
        }

        if (VertexDrag != null && !VertexFiles.ContainsValue(VertexDrag.File))
            VertexDrag = null;

        if (VertexHover != null && !VertexFiles.ContainsValue(VertexHover.File))
            VertexHover = null;

        if (VertexFiles.Count == 0 && HiddenNavmeshDrawings.Count == 0)
            VertexMap = null;
    }

    /// <summary>Where each navmesh entry of the edited map (or else the overlay's) is drawn, found once a frame.</summary>
    private void GatherVertexEntries()
    {
        VertexEntries.Clear();

        var map = VertexMap ?? OverlayContainer;

        if (map == null || (!VertexEditActive && VertexFiles.Count == 0 && HiddenNavmeshDrawings.Count == 0))
            return;

        foreach (var entity in map.Objects)
        {
            if (entity.WrappedObject is NVA_ER.Navmesh entry)
                VertexEntries.TryAdd(entry, (entity, entity.GetWorldMatrix()));
        }

        foreach (var file in VertexFiles.Values)
        {
            if (VertexEntries.TryGetValue(file.Entry, out var placed))
                file.SetWorld(placed.World);
        }
    }

    private Matrix4x4 VertexEntryWorld(NVA_ER.Navmesh entry)
        => VertexEntries.TryGetValue(entry, out var placed) ? placed.World : MapWorld(VertexMap ?? OverlayContainer);

    private void HandleVertexKeys(Ray ray, bool canInteract)
    {
        // Edits are made on the overlay's map; edits pending on another map wait for it
        var canEdit = OverlayNavigation != null && OverlayContainer != null && (VertexMap == null || VertexMap == OverlayContainer);

        // Mouse buttons fly the camera with the same keys, and typed text shouldn't edit
        var keysFree = canInteract
            && !ImGui.GetIO().WantTextInput
            && !InputManager.IsMouseDown(Veldrid.MouseButton.Left)
            && !InputManager.IsMouseDown(Veldrid.MouseButton.Right)
            && !InputManager.IsMouseDown(Veldrid.MouseButton.Middle);

        if (VertexDrag != null)
        {
            if (!keysFree || !canEdit || !InputManager.IsDown(KeybindID.MapEditor_NavigationEditor_Drag))
                VertexDrag = null;
            else
                ContinueVertexDrag(ray);
        }

        if (VertexDrag == null)
        {
            VertexHover = canInteract && canEdit ? VertexRayCast(ray) : null;

            if (keysFree && VertexHover != null && InputManager.IsPressed(KeybindID.MapEditor_NavigationEditor_Drag))
                BeginVertexDrag(VertexHover, ray);
        }

        var target = VertexDrag ?? VertexHover;

        if (!keysFree || target == null)
            return;

        if (InputManager.IsPressedOrRepeated(KeybindID.MapEditor_NavigationEditor_Raise))
            RaiseVertex(target, VertexStep);

        if (InputManager.IsPressedOrRepeated(KeybindID.MapEditor_NavigationEditor_Lower))
            RaiseVertex(target, -VertexStep);
    }

    /// <summary>
    /// The working copy of an entry's file on the overlay's map, made the first time it is asked for. Null for an
    /// entry without faces.
    /// </summary>
    private NavVertexFile GetVertexFile(NVA_ER.Navmesh entry)
    {
        var navigation = OverlayNavigation;
        var file = navigation?.FileOf(entry);

        if (file?.Mesh == null || file.FaceCount == 0 || file.Document == null)
            return null;

        if (VertexFiles.TryGetValue(file.Name, out var existing) && ReferenceEquals(existing.File.Document, file.Document))
            return existing;

        // The copies are of one map: those of another map without edits make way
        if (VertexMap != null && VertexMap != OverlayContainer)
        {
            if (HasVertexEdits)
                return null;

            VertexFiles.Clear();
        }

        VertexMap = OverlayContainer;

        var copy = new NavVertexFile(entry, file, VertexEntryWorld(entry));
        VertexFiles[file.Name] = copy;
        return copy;
    }

    /// <summary>Whether an entry is drawn by the overlay, which shows only the selected model with that option.</summary>
    private bool IsEntryDrawn(NVA_ER.Navmesh entry)
        => !(OverlayOnlySelectedModel && SelectedModelID >= 0) || entry.ModelID == SelectedModelID;

    /// <summary>The nearest face of the drawn set under the ray, and the corner of it nearest the hit.</summary>
    private VertexHit VertexRayCast(Ray ray)
    {
        VertexHit best = null;
        var direction = Vector3.Normalize(ray.Direction);

        foreach (var entry in NavigationOverlayBuilder.EntriesOf(OverlayNavigation, OverlaySet))
        {
            if (!IsEntryDrawn(entry))
                continue;

            var file = GetVertexFile(entry);

            if (file == null || !file.RayCast(ray.Origin, direction, out var face, out var distance, out var point))
                continue;

            if (best == null || distance < best.Distance)
                best = new VertexHit(file, face, file.NearestCorner(face, point), point, distance);
        }

        return best;
    }

    /// <summary>
    /// The vertices an edit of the target moves: the target, those of its navmesh within the radius of it, and with
    /// linking, those of the map's other navmeshes on it (within the link distance), with the radius past that.
    /// </summary>
    private List<VertexMove> GetVertexMoves(VertexHit target)
    {
        var point = target.File.WorldVertex(target.Vertex);
        var moves = new List<VertexMove> { new(target.File, target.File.Falloff(point, VertexWeld, VertexRadius)) };

        if ((!VertexLinkNeighbours && !VertexLinkOtherSet) || OverlayNavigation == null)
            return moves;

        foreach (var entry in OverlayNavigation.Nva.Navmeshes)
        {
            bool sameSet = NavigationMap.SetOf(entry) == NavigationMap.SetOf(target.File.Entry);

            if (sameSet ? !VertexLinkNeighbours : !VertexLinkOtherSet)
                continue;

            var file = GetVertexFile(entry);

            if (file == null || file == target.File)
                continue;

            var falloff = file.Falloff(point, VertexLinkDistance, VertexRadius);

            if (falloff.Count > 0)
                moves.Add(new VertexMove(file, falloff));
        }

        return moves;
    }

    private void RaiseVertex(VertexHit target, float amount)
    {
        var moves = VertexDrag != null ? VertexDragMoves : GetVertexMoves(target);

        foreach (var move in moves)
            move.File.MoveWorld(move.Falloff, new Vector3(0, amount, 0));

        // The drag plane follows the vertex, so dragging after raising doesn't pull it back down
        if (VertexDrag != null)
            VertexDragHeight += amount;
    }

    private void BeginVertexDrag(VertexHit hit, Ray ray)
    {
        var position = hit.File.WorldVertex(hit.Vertex);

        VertexDrag = hit;
        VertexDragHeight = position.Y;
        VertexDragMoves = GetVertexMoves(hit);

        // Keeps the vertex where it is relative to the mouse, so it doesn't jump to it
        VertexDragGrab = IntersectVertexHeight(ray, VertexDragHeight, out var point) ? position - point : Vector3.Zero;
        VertexDragGrab.Y = 0;
    }

    /// <summary>Puts the dragged vertex where the mouse ray meets the horizontal plane at its height.</summary>
    private void ContinueVertexDrag(Ray ray)
    {
        if (!IntersectVertexHeight(ray, VertexDragHeight, out var point))
            return;

        var current = VertexDrag.File.WorldVertex(VertexDrag.Vertex);
        var target = new Vector3(point.X + VertexDragGrab.X, VertexDragHeight, point.Z + VertexDragGrab.Z);

        if (Vector3.DistanceSquared(current, target) < 1e-10f)
            return;

        // The followers take the same move as the dragged vertex, so joined edges stay joined
        foreach (var move in VertexDragMoves)
            move.File.MoveWorld(move.Falloff, target - current);
    }

    private static bool IntersectVertexHeight(Ray ray, float height, out Vector3 point)
    {
        point = default;

        if (MathF.Abs(ray.Direction.Y) < 1e-6f)
            return false;

        var t = (height - ray.Origin.Y) / ray.Direction.Y;

        if (t < 0)
            return false;

        point = ray.Origin + ray.Direction * t;
        return true;
    }

    #endregion

    #region Display

    /// <summary>
    /// The edited files whose previews are drawn on a map's overlay: those of the drawn set and models, while the
    /// overlay is shown. The overlay is built without them.
    /// </summary>
    private HashSet<string> VertexPreviewedFiles(MapContainer map)
    {
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (!ShowOverlay || map == null || map != VertexMap)
            return names;

        foreach (var (name, file) in VertexFiles)
        {
            if (file.IsEdited && NavigationMap.SetOf(file.Entry) == OverlaySet && IsEntryDrawn(file.Entry))
                names.Add(name);
        }

        return names;
    }

    /// <summary>
    /// Draws each edited file from its edited vertices, in place of its part of the overlay and of the Map Editor's own
    /// drawing of it. A file moved since its preview was drawn is drawn again, at most once a frame.
    /// </summary>
    private void UpdateVertexPreviews()
    {
        for (int i = RetiringVertexPreviews.Count - 1; i >= 0; i--)
        {
            if (DrawFrame - RetiringVertexPreviews[i].Frame < VertexPreviewOverlapFrames)
                continue;

            RetiringVertexPreviews[i].Renderer.Clear();
            RetiringVertexPreviews.RemoveAt(i);
        }

        var scene = View.ViewportHandler.ActiveViewport?.RenderScene;
        var shown = OverlayNavigation != null && OverlayContainer == VertexMap ? VertexPreviewedFiles(OverlayContainer) : new HashSet<string>();

        // Previews that are no longer wanted
        foreach (var name in VertexPreviews.Keys.Where(n => !shown.Contains(n)).ToList())
        {
            VertexPreviews[name].Renderer.Clear();
            VertexPreviews.Remove(name);
        }

        if (scene != null)
        {
            foreach (var name in shown)
            {
                var file = VertexFiles[name];

                if (VertexPreviews.TryGetValue(name, out var preview) && preview.Revision == file.Revision && preview.Build == OverlayBuildCount)
                    continue;

                if (!ReferenceEquals(VertexEndsFor, OverlayNavigation))
                {
                    VertexEnds = NavigationOverlayBuilder.ConnectionEnds(OverlayNavigation);
                    VertexEndsFor = OverlayNavigation;
                }

                var overlay = NavigationOverlayBuilder.BuildEntry(OverlayNavigation.MapId, OverlaySet, file.Entry, file.PreviewMesh(), file.World, Layers, VertexEnds);
                var renderer = new NavigationOverlayRenderer();
                renderer.Show(scene, overlay, FaceOpacity, OverlayRenderer.FaceLegend);

                if (preview.Renderer != null)
                    RetiringVertexPreviews.Add((preview.Renderer, DrawFrame));

                VertexPreviews[name] = (renderer, file.Revision, OverlayBuildCount);
            }
        }

        // The overlay is drawn again without the previewed files, once the set of them changes
        if (ShowOverlay && OverlayContainer != null && !shown.SetEquals(OverlayWithout))
            OverlayDirty = true;

        UpdateHiddenNavmeshDrawings(shown);
    }

    /// <summary>Hides the Map Editor's own drawing of the previewed files, and shows again those no longer previewed.</summary>
    private void UpdateHiddenNavmeshDrawings(HashSet<string> shown)
    {
        var hide = new HashSet<Entity>();

        if (shown.Count > 0)
        {
            foreach (var (entry, placed) in VertexEntries)
            {
                if (shown.Contains(OverlayNavigation.FileNameOf(entry)))
                    hide.Add(placed.Entity);
            }
        }

        foreach (var entity in HiddenNavmeshDrawings.Where(e => !hide.Contains(e)).ToList())
        {
            if (entity.RenderSceneMesh != null && entity.RenderSceneMesh.Visible != entity.EditorVisible)
                entity.RenderSceneMesh.Visible = entity.EditorVisible;

            HiddenNavmeshDrawings.Remove(entity);
        }

        foreach (var entity in hide)
        {
            if (entity.RenderSceneMesh != null && entity.RenderSceneMesh.Visible)
                entity.RenderSceneMesh.Visible = false;

            HiddenNavmeshDrawings.Add(entity);
        }
    }

    /// <summary>Places the cursor sphere on the hit, and a smaller sphere on the vertex that will be moved.</summary>
    private void UpdateVertexCursor()
    {
        var target = VertexEditActive ? VertexDrag ?? VertexHover : null;

        if (target == null)
        {
            SetVertexProxyVisible(VertexCursorWire, false);
            SetVertexProxyVisible(VertexCursorFill, false);
            SetVertexProxyVisible(VertexMarker, false);
            return;
        }

        var scene = View.ViewportHandler.ActiveViewport?.RenderScene;

        if (scene == null)
            return;

        var cursorColor = ToVertexColor(CFG.Current.MapEditor_CollisionEditor_Cursor_Color);

        // A wireframe over a translucent fill, so the navmesh under a large cursor can still be seen
        VertexCursorWire ??= RenderableHelper.GetDetectionSphereProxy(scene, cursorColor);
        VertexCursorFill ??= RenderableHelper.GetDetectionSphereFillProxy(scene, cursorColor);

        VertexMarker ??= new DebugPrimitiveRenderableProxy(scene.OpaqueRenderables, RenderableHelper._placementOrb)
        {
            Pickable = false,
            RenderOverlay = true,
            BaseColor = ToVertexColor(CFG.Current.MapEditor_CollisionEditor_Vertex_Color),
            HighlightedColor = ToVertexColor(CFG.Current.MapEditor_CollisionEditor_Vertex_Color)
        };

        // Follows the option as it is changed
        var fill = ColorHelper.GetTransparencyColor(cursorColor, VertexCursorOpacity);

        if (VertexCursorFill.BaseColor != fill)
        {
            VertexCursorFill.BaseColor = fill;
            VertexCursorFill.HighlightedColor = fill;
        }

        var vertex = target.File.WorldVertex(target.Vertex);

        // While dragging, the cursor stays on the vertex it holds
        var cursor = VertexDrag != null ? vertex : target.Point;
        var cursorWorld = Matrix4x4.CreateScale(VertexCursorSize) * Matrix4x4.CreateTranslation(cursor);

        // The marker stays small with a large cursor, so it doesn't hide the vertices around it
        var markerSize = MathF.Min(VertexCursorSize * 0.6f, VertexMaxMarkerSize);

        VertexCursorWire.World = cursorWorld;
        VertexCursorFill.World = cursorWorld;
        VertexMarker.World = Matrix4x4.CreateScale(markerSize / VertexSphereRadius) * Matrix4x4.CreateTranslation(vertex);

        SetVertexProxyVisible(VertexCursorWire, true);
        SetVertexProxyVisible(VertexCursorFill, true);
        SetVertexProxyVisible(VertexMarker, true);
    }

    private static Color ToVertexColor(Vector3 color) => Color.FromArgb(255, (int)color.X, (int)color.Y, (int)color.Z);

    private static void SetVertexProxyVisible(DebugPrimitiveRenderableProxy proxy, bool visible)
    {
        if (proxy != null && proxy.Visible != visible)
            proxy.Visible = visible;
    }

    private void DisposeVertexCursor()
    {
        VertexCursorWire?.Dispose();
        VertexCursorFill?.Dispose();
        VertexMarker?.Dispose();

        VertexCursorWire = null;
        VertexCursorFill = null;
        VertexMarker = null;
    }

    #endregion
}
