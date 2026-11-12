using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
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

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The Edit tab: moves navmesh vertices in the viewport with a brush, as the Collision Editor does for collision. While
/// editing, a brush sphere sits where the mouse ray meets the drawn navmesh set, and holding the picking button raises,
/// lowers, drags or smooths the vertices within its radius (Raise, Lower and Smooth again at each interval while held).
/// Vertices of the map's other navmeshes under the brush follow too, so connected edges stay joined. The edits are drawn in place of the
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

    // The hit a brush stroke started on, while the button is held; null between strokes
    private VertexHit VertexStroke;
    private CollisionBrushMode VertexStrokeMode;

    // When Raise, Lower or Smooth next applies, as a Stopwatch timestamp
    private long VertexNextApply;

    // A Drag stroke: the vertices it took, the horizontal plane they slide on, where the mouse last met it, and where
    // the brush is drawn
    private List<VertexMove> VertexDragMoves;
    private float VertexDragHeight;
    private Vector3 VertexDragLast;
    private Vector3 VertexDragAnchor;

    private CollisionBrushMode VertexBrushMode = CollisionBrushMode.Raise;
    private float VertexStep = 0.1f;
    private float VertexRadius = 2f;
    private float VertexInterval = 0.1f;
    private float VertexSmoothStrength = 0.25f;
    private bool VertexLinkNeighbours = true;
    private bool VertexLinkOtherSet;
    private float VertexLinkDistance = 0.1f;
    private float VertexCursorOpacity = 20f;

    private bool ShowVertexControls = true;
    private bool ShowVertexCursor = true;

    private bool IsVertexDragStroke => VertexStroke != null && VertexStrokeMode is CollisionBrushMode.Drag;

    private static readonly (KeybindID Key, CollisionBrushMode Mode)[] VertexBrushKeys =
    {
        (KeybindID.MapEditor_NavigationEditor_Raise, CollisionBrushMode.Raise),
        (KeybindID.MapEditor_NavigationEditor_Lower, CollisionBrushMode.Lower),
        (KeybindID.MapEditor_NavigationEditor_Drag, CollisionBrushMode.Drag),
        (KeybindID.MapEditor_NavigationEditor_Smooth, CollisionBrushMode.Smooth),
    };

    private const float VertexSphereRadius = 0.1f;
    private const float VertexMaxMarkerSize = 0.1f;

    /// <summary>The preview of each edited file, with the revision and overlay build it was drawn from.</summary>
    private readonly Dictionary<string, (NavigationOverlayRenderer Renderer, int Revision, int Build)> VertexPreviews = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// The previewed files whose preview has been drawn: only these are left out of the overlay and the Map Editor's
    /// drawing, so a file is never missing while its preview uploads.
    /// </summary>
    private HashSet<string> DrawnVertexPreviews = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Counts overlay builds, so the previews are drawn again with new layers, opacity or legend.</summary>
    private int OverlayBuildCount;

    /// <summary>The previewed files the overlay was last built without.</summary>
    private HashSet<string> OverlayWithout = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>The Map Editor's own drawings of navmeshes hidden while their previews are shown.</summary>
    private readonly HashSet<Entity> HiddenNavmeshDrawings = new();

    /// <summary>Each navmesh entry's entity in the edited map, and where it is drawn, gathered once a frame.</summary>
    private readonly Dictionary<NVA.Navmesh, (Entity Entity, Matrix4x4 World)> VertexEntries = new(ReferenceEqualityComparer.Instance);

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

        // No other set to follow in a game with the n set only (AC6)
        if (Bank.Profile.HasOSet)
        {
            ImGui.Checkbox($"{LOC.Get("MAP_Navigation_Edit_Link_Other_Set")}##navigationEditLinkOtherSet", ref VertexLinkOtherSet);
            GUI.Tooltip(LOC.Get("MAP_Navigation_Edit_Link_Other_Set_TT"));
        }
        else
        {
            VertexLinkOtherSet = false;
        }

        // Link Distance
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Edit_Link_Distance_Header"), LOC.Get("MAP_Navigation_Edit_Link_Distance_Header_TT"));

        ImGui.SetNextItemWidth(-1);
        if (ImGui.InputFloat("##navigationEditLinkDistance", ref VertexLinkDistance))
            VertexLinkDistance = Math.Max(0f, VertexLinkDistance);
        GUI.Tooltip(LOC.Get("MAP_Navigation_Edit_Link_Distance_Input_TT"));

        DisplayVertexBrushOptions();

        // Cursor Opacity
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Edit_Cursor_Opacity_Header"), LOC.Get("MAP_Navigation_Edit_Cursor_Opacity_Header_TT"));

        ImGui.SetNextItemWidth(-1);
        ImGui.SliderFloat("##navigationEditCursorOpacity", ref VertexCursorOpacity, 0f, 100f, "%.0f%%");
        GUI.Tooltip(LOC.Get("MAP_Navigation_Edit_Cursor_Opacity_Input_TT"));

        // Controls
        GUI.Spacer();
        GUI.ConditionalHeader(LOC.Get("MAP_Navigation_Edit_Controls_Header"), LOC.Get("MAP_Navigation_Edit_Controls_Header_TT"),
            ref ShowVertexControls);

        if (ShowVertexControls)
        {
            GUI.WrappedText(LOC.Get("MAP_Navigation_Edit_Control_Brush", InputManager.GetMouseHint(MousebindID.Viewport_Picking_Action)));
            GUI.WrappedText(LOC.Get("MAP_Navigation_Edit_Control_Modes",
                InputManager.GetHint(KeybindID.MapEditor_NavigationEditor_Raise),
                InputManager.GetHint(KeybindID.MapEditor_NavigationEditor_Lower),
                InputManager.GetHint(KeybindID.MapEditor_NavigationEditor_Drag),
                InputManager.GetHint(KeybindID.MapEditor_NavigationEditor_Smooth)));
        }

        DisplayVertexStatus();
    }

    /// <summary>The brush mode, its radius, and the options of the chosen mode.</summary>
    private void DisplayVertexBrushOptions()
    {
        var mode = VertexBrushMode;

        // Brush
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Edit_Brush_Header"), LOC.Get("MAP_Navigation_Edit_Brush_Header_TT"));

        ImGui.SetNextItemWidth(-1);
        if (ImGui.BeginCombo("##navigationEditBrushMode", LOC.Get(mode.GetDisplayName())))
        {
            foreach (var (key, value) in VertexBrushKeys)
            {
                var label = $"{LOC.Get(value.GetDisplayName())} ({InputManager.GetHint(key)})";

                if (ImGui.Selectable($"{label}##navigationEditBrushMode{value}", value == mode))
                    VertexBrushMode = value;
                GUI.Tooltip(LOC.Get($"MAP_Navigation_Edit_Brush_Mode_{value}_TT"));
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("MAP_Navigation_Edit_Brush_Mode_Input_TT"));

        mode = VertexBrushMode;
        GUI.WrappedText(LOC.Get($"MAP_Navigation_Edit_Brush_Mode_{mode}_TT"));

        // Radius
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Edit_Radius_Header"), LOC.Get("MAP_Navigation_Edit_Radius_Header_TT"));

        ImGui.SetNextItemWidth(-1);
        if (ImGui.InputFloat("##navigationEditRadius", ref VertexRadius))
            VertexRadius = Math.Max(0.01f, VertexRadius);
        GUI.Tooltip(LOC.Get("MAP_Navigation_Edit_Radius_Input_TT"));

        if (mode is CollisionBrushMode.Raise or CollisionBrushMode.Lower)
        {
            // Step
            GUI.Spacer();
            GUI.SimpleHeader(LOC.Get("MAP_Navigation_Edit_Step_Header"), LOC.Get("MAP_Navigation_Edit_Step_Header_TT"));

            ImGui.SetNextItemWidth(-1);
            if (ImGui.InputFloat("##navigationEditStep", ref VertexStep))
                VertexStep = Math.Max(0.001f, VertexStep);
            GUI.Tooltip(LOC.Get("MAP_Navigation_Edit_Step_Input_TT"));
        }

        if (mode is CollisionBrushMode.Smooth)
        {
            // Strength
            GUI.Spacer();
            GUI.SimpleHeader(LOC.Get("MAP_Navigation_Edit_Smooth_Strength_Header"), LOC.Get("MAP_Navigation_Edit_Smooth_Strength_Header_TT"));

            var percent = VertexSmoothStrength * 100f;

            ImGui.SetNextItemWidth(-1);
            if (ImGui.SliderFloat("##navigationEditSmoothStrength", ref percent, 1f, 100f, "%.0f%%"))
                VertexSmoothStrength = Math.Clamp(percent / 100f, 0.01f, 1f);
            GUI.Tooltip(LOC.Get("MAP_Navigation_Edit_Smooth_Strength_Input_TT"));
        }

        if (mode is not CollisionBrushMode.Drag)
        {
            // Interval
            GUI.Spacer();
            GUI.SimpleHeader(LOC.Get("MAP_Navigation_Edit_Brush_Interval_Header"), LOC.Get("MAP_Navigation_Edit_Brush_Interval_Header_TT"));

            ImGui.SetNextItemWidth(-1);
            if (ImGui.InputFloat("##navigationEditBrushInterval", ref VertexInterval))
                VertexInterval = Math.Max(0.01f, VertexInterval);
            GUI.Tooltip(LOC.Get("MAP_Navigation_Edit_Brush_Interval_Input_TT"));
        }
    }

    /// <summary>What is under the cursor, and which files have edits waiting to be committed.</summary>
    private void DisplayVertexStatus()
    {
        // Cursor
        GUI.Spacer();
        GUI.ConditionalHeader(LOC.Get("MAP_Navigation_Edit_Cursor_Header"), LOC.Get("MAP_Navigation_Edit_Cursor_Header_TT"),
            ref ShowVertexCursor);

        // Hidden, the section shows nothing; Pending Edits below is still shown
        if (!ShowVertexCursor)
        {
        }
        else if (!VertexEditActive)
        {
            GUI.WrappedText(LOC.Get("MAP_Navigation_Edit_Cursor_Inactive"));
        }
        else if (((IsVertexDragStroke ? VertexStroke : null) ?? VertexHover) is { } hit)
        {
            var position = IsVertexDragStroke ? VertexDragAnchor : hit.Point;
            var moves = IsVertexDragStroke && VertexDragMoves != null ? VertexDragMoves : GetCursorVertexMoves(hit);

            GUI.WrappedText(LOC.Get("MAP_Navigation_Edit_Cursor_File", hit.File.File.Name, (uint)hit.File.Entry.NameID, hit.File.Entry.ModelID));
            GUI.WrappedText(LOC.Get("MAP_Navigation_Edit_Cursor_Point",
                $"{position.X:0.###}, {position.Y:0.###}, {position.Z:0.###}"));
            GUI.WrappedText(LOC.Get("MAP_Navigation_Edit_Cursor_Moves",
                moves.Sum(m => m.Falloff.Count), moves.Count, $"{VertexRadius:0.###}"));
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

        // The Collision Editor's cursor takes the same keys, so one press would move both meshes
        View.CollisionEditorTool?.StopEditing();

        PickMode = false;
        PaintMode = false;
        LinkMode = false;
        StopGateMode();
        SetPicked(null);
    }

    /// <summary>
    /// Hides the cursor. Pending edits are kept; the files without edits are read again when next needed. Also called
    /// by the Collision Editor when its cursor starts, as both take the same keys.
    /// </summary>
    public void StopVertexEdit()
    {
        if (!VertexEditActive)
            return;

        VertexEditActive = false;
        EndVertexStroke();
        VertexHover = null;
        DisposeVertexCursor();

        foreach (var name in VertexFiles.Where(e => !e.Value.IsEdited).Select(e => e.Key).ToList())
            VertexFiles.Remove(name);
    }

    private void DiscardVertexEdits()
    {
        EndVertexStroke();

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
        EndVertexStroke();

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
                document = NavVertexEdit.Build(file.File.Document, file.Vertices, out var clearance);

                if (document != null)
                    Smithbox.Log(this, LOC.Get("MAP_Navigation_Edit_Clearance", file.File.Name, clearance.Caches, clearance.Vertices, clearance.Edges, clearance.Pairs, clearance.Skipped));
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
    /// the edited files up to date. Returns true while editing, as the picking button brushes, so it doesn't select.
    /// </summary>
    private bool UpdateVertexEdit(Ray ray, bool canInteract)
    {
        DropStaleVertexFiles();
        GatherVertexEntries();

        if (VertexEditActive)
            HandleVertexBrush(ray, canInteract);

        UpdateVertexPreviews();
        UpdateVertexCursor();

        return VertexEditActive;
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
            if (!ReferenceEquals(Bank.GetNavmeshDocument(VertexMap, name), file.File.Document))
                (stale ??= new()).Add(name);
        }

        if (stale != null)
        {
            foreach (var name in stale)
                VertexFiles.Remove(name);
        }

        // A stroke's vertices may be in a dropped file
        if (VertexStroke != null && (stale != null || !VertexFiles.ContainsValue(VertexStroke.File)))
            EndVertexStroke();

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
            if (entity.WrappedObject is NVA.Navmesh entry)
                VertexEntries.TryAdd(entry, (entity, entity.GetWorldMatrix()));
        }

        foreach (var file in VertexFiles.Values)
        {
            if (VertexEntries.TryGetValue(file.Entry, out var placed))
                file.SetWorld(placed.World);
        }
    }

    private Matrix4x4 VertexEntryWorld(NVA.Navmesh entry)
        => VertexEntries.TryGetValue(entry, out var placed) ? placed.World : MapWorld(VertexMap ?? OverlayContainer);

    private void HandleVertexBrush(Ray ray, bool canInteract)
    {
        // Edits are made on the overlay's map; edits pending on another map wait for it
        var canEdit = OverlayNavigation != null && OverlayContainer != null && (VertexMap == null || VertexMap == OverlayContainer);

        // Typed text shouldn't change the brush, and the right and middle buttons fly the camera
        var typing = ImGui.GetIO().WantTextInput;
        var flying = InputManager.IsMouseDown(Veldrid.MouseButton.Right) || InputManager.IsMouseDown(Veldrid.MouseButton.Middle);

        // The mode keys are WASD's neighbours, which fly the camera while a button is held
        var keysFree = canInteract && !typing && !flying && !InputManager.IsMouseDown(Veldrid.MouseButton.Left);
        var brushFree = canInteract && !typing && !flying;

        if (keysFree && VertexStroke == null)
        {
            foreach (var (key, mode) in VertexBrushKeys)
            {
                if (InputManager.IsPressed(key))
                    VertexBrushMode = mode;
            }
        }

        if (VertexStroke != null && (!brushFree || !canEdit || !InputManager.IsMouseDown(MousebindID.Viewport_Picking_Action)))
            EndVertexStroke();

        // A drag holds the vertices it took; the other brushes follow the mouse over the navmesh
        if (!IsVertexDragStroke)
            VertexHover = canInteract && canEdit ? VertexRayCast(ray) : null;

        if (VertexStroke != null)
            ContinueVertexStroke(ray);
        else if (brushFree && VertexHover != null && InputManager.IsMousePressed(MousebindID.Viewport_Picking_Action))
            BeginVertexStroke(VertexHover);
    }

    /// <summary>
    /// The working copy of an entry's file on the overlay's map, made the first time it is asked for. Null for an
    /// entry without faces.
    /// </summary>
    private NavVertexFile GetVertexFile(NVA.Navmesh entry)
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
    private bool IsEntryDrawn(NVA.Navmesh entry)
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
    /// The vertices a brush at a hit moves: those of the hit navmesh within the radius of the hit point; with
    /// neighbours linked, those of the map's other navmeshes of the same set within it, by the same falloff, so joined
    /// edges stay joined; and with the other set linked, its vertices, fully within the link distance and fading out
    /// over the radius past it. If no vertex of the hit navmesh is within the radius, the face's nearest corner moves
    /// alone, so a small brush still moves something.
    /// </summary>
    private List<VertexMove> GetVertexMoves(VertexHit target)
    {
        var point = target.Point;
        var radius = Math.Max(0.01f, VertexRadius);
        var moves = new List<VertexMove>();

        var own = target.File.Falloff(point, 0f, radius);

        if (own.Count == 0)
            own.Add((target.Vertex, 1f));

        moves.Add(new VertexMove(target.File, own));

        if ((!VertexLinkNeighbours && !VertexLinkOtherSet) || OverlayNavigation == null)
            return moves;

        foreach (var entry in OverlayNavigation.Nva.Navmeshes)
        {
            bool sameSet = NavigationMap.SetOf(entry, Bank.Profile) == NavigationMap.SetOf(target.File.Entry, Bank.Profile);

            if (sameSet ? !VertexLinkNeighbours : !VertexLinkOtherSet)
                continue;

            // The overlay only draws the selected model with that option, and the brush only reaches what is drawn
            if (sameSet && !IsEntryDrawn(entry))
                continue;

            var file = GetVertexFile(entry);

            if (file == null || file == target.File)
                continue;

            var falloff = file.Falloff(point, sameSet ? 0f : VertexLinkDistance, radius);

            if (falloff.Count > 0)
                moves.Add(new VertexMove(file, falloff));
        }

        return moves;
    }

    // What a stroke at the cursor moves, worked out again only when the cursor or an option it depends on changes
    private (VertexHit Hit, float Radius, float LinkDistance, bool Neighbours, bool OtherSet) CursorVertexMovesKey;
    private List<VertexMove> CursorVertexMoves;

    private List<VertexMove> GetCursorVertexMoves(VertexHit hit)
    {
        var key = (hit, VertexRadius, VertexLinkDistance, VertexLinkNeighbours, VertexLinkOtherSet);

        if (CursorVertexMoves == null || !key.Equals(CursorVertexMovesKey))
        {
            CursorVertexMovesKey = key;
            CursorVertexMoves = GetVertexMoves(hit);
        }

        return CursorVertexMoves;
    }

    private void BeginVertexStroke(VertexHit hit)
    {
        VertexStroke = hit;
        VertexStrokeMode = VertexBrushMode;

        if (VertexStrokeMode is CollisionBrushMode.Drag)
        {
            // The vertices are taken once, so dragging over other ground doesn't pick it up too
            VertexDragMoves = GetVertexMoves(hit);
            VertexDragHeight = hit.Point.Y;
            VertexDragLast = hit.Point;
            VertexDragAnchor = hit.Point;
            return;
        }

        // Applied on the press, so a click is one step
        ApplyVertexBrush(hit);
        VertexNextApply = Stopwatch.GetTimestamp() + VertexIntervalTicks();
    }

    private void ContinueVertexStroke(Ray ray)
    {
        if (VertexStrokeMode is CollisionBrushMode.Drag)
        {
            if (!IntersectVertexHeight(ray, VertexDragHeight, out var point))
                return;

            var offset = point - VertexDragLast;
            offset.Y = 0;

            if (offset.LengthSquared() < 1e-10f)
                return;

            foreach (var move in VertexDragMoves)
                move.File.MoveWorld(move.Falloff, offset);

            VertexDragLast = point;
            VertexDragAnchor += offset;
            return;
        }

        var now = Stopwatch.GetTimestamp();

        if (now < VertexNextApply)
            return;

        // Off the navmesh, nothing is brushed until the brush is back on it; the stroke goes on
        if (VertexHover != null)
            ApplyVertexBrush(VertexHover);

        // From now rather than from the last time, so a slow frame doesn't apply several steps at once
        VertexNextApply = now + VertexIntervalTicks();
    }

    private void EndVertexStroke()
    {
        VertexStroke = null;
        VertexDragMoves = null;
        CursorVertexMoves = null;
    }

    private long VertexIntervalTicks() => (long)(Math.Max(0.01f, VertexInterval) * Stopwatch.Frequency);

    /// <summary>One application of the Raise, Lower or Smooth brush at a hit.</summary>
    private void ApplyVertexBrush(VertexHit hit)
    {
        var moves = GetVertexMoves(hit);

        switch (VertexStrokeMode)
        {
            case CollisionBrushMode.Raise:
                foreach (var move in moves)
                    move.File.MoveWorld(move.Falloff, new Vector3(0, VertexStep, 0));
                break;

            case CollisionBrushMode.Lower:
                foreach (var move in moves)
                    move.File.MoveWorld(move.Falloff, new Vector3(0, -VertexStep, 0));
                break;

            case CollisionBrushMode.Smooth:
                // Each set towards a plane of its own, so a linked set keeps its distance from the edited one
                foreach (var set in moves.GroupBy(m => NavigationMap.SetOf(m.File.Entry, Bank.Profile)))
                    SmoothVerticesTowardsPlane(set.ToList());
                break;
        }

        // The vertices under the brush have moved, so what it reaches may have changed
        CursorVertexMoves = null;
    }

    /// <summary>
    /// Pulls the brushed vertices towards the plane that best fits them (weighted by the falloff), along its normal, by
    /// the strength's share of their distance to it, scaled by their weight (as the Collision Editor's Smooth brush).
    /// </summary>
    private void SmoothVerticesTowardsPlane(List<VertexMove> moves)
    {
        var points = new List<(Vector3 Position, float Weight)>();

        foreach (var move in moves)
        {
            foreach (var (v, weight) in move.Falloff)
                points.Add((move.File.WorldVertex(v), weight));
        }

        if (!CollisionEditorTool.FitPlane(points, out var center, out var normal))
            return;

        var strength = Math.Clamp(VertexSmoothStrength, 0f, 1f);

        foreach (var move in moves)
        {
            var offsets = new List<(int, Vector3)>(move.Falloff.Count);

            foreach (var (v, weight) in move.Falloff)
            {
                var distance = Vector3.Dot(move.File.WorldVertex(v) - center, normal);

                if (MathF.Abs(distance) < 1e-6f)
                    continue;

                offsets.Add((v, -normal * (distance * weight * strength)));
            }

            move.File.OffsetWorld(offsets);
        }
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
            if (file.IsEdited && NavigationMap.SetOf(file.Entry, Bank.Profile) == OverlaySet && IsEntryDrawn(file.Entry))
                names.Add(name);
        }

        return names;
    }

    /// <summary>
    /// Draws each edited file from its edited vertices, in place of its part of the overlay and of the Map Editor's own
    /// drawing of it. A file moved since its preview was drawn is drawn again, at most once a frame. As in the
    /// Collision Editor, nothing is ever missing while a drawing uploads: a redrawn preview replaces the old one only
    /// once it has been drawn (see <see cref="NavigationOverlayRenderer.Update" />), a file moved again meanwhile waits
    /// for it and is then drawn with the latest vertices, a file is only left out of the overlay once its preview has
    /// been drawn, and a preview no longer wanted stays until the overlay drawn with its file again has taken over.
    /// </summary>
    private void UpdateVertexPreviews()
    {
        var scene = View.ViewportHandler.ActiveViewport?.RenderScene;
        var overlayOn = ShowOverlay && OverlayNavigation != null && OverlayContainer == VertexMap;
        var wanted = overlayOn ? VertexPreviewedFiles(OverlayContainer) : new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var (renderer, _, _) in VertexPreviews.Values)
            renderer.Update();

        // Previews that are no longer wanted go once the overlay drawn with their file again has been drawn, or at once
        // when the overlay itself went
        foreach (var name in VertexPreviews.Keys.Where(n => !wanted.Contains(n)).ToList())
        {
            if (overlayOn && (OverlayWithout.Contains(name) || OverlayDirty || OverlayRenderer.IsUpdating))
                continue;

            VertexPreviews[name].Renderer.Clear();
            VertexPreviews.Remove(name);
        }

        if (scene != null)
        {
            foreach (var name in wanted)
            {
                var file = VertexFiles[name];
                var exists = VertexPreviews.TryGetValue(name, out var preview);

                if (exists && preview.Revision == file.Revision && preview.Build == OverlayBuildCount)
                    continue;

                // Still uploading the last drawing: drawn again with the latest vertices once that one is shown
                if (exists && preview.Renderer.IsUpdating)
                    continue;

                if (!ReferenceEquals(VertexEndsFor, OverlayNavigation))
                {
                    VertexEnds = NavigationOverlayBuilder.ConnectionEnds(OverlayNavigation);
                    VertexEndsFor = OverlayNavigation;
                }

                var overlay = NavigationOverlayBuilder.BuildEntry(OverlayNavigation.MapId, OverlaySet, file.Entry, file.PreviewMesh(), file.World, Layers, VertexEnds);
                var renderer = preview.Renderer ?? new NavigationOverlayRenderer();
                renderer.Show(scene, overlay, FaceOpacity, OverlayRenderer.FaceLegend);

                VertexPreviews[name] = (renderer, file.Revision, OverlayBuildCount);
            }
        }

        // Left out of the overlay and the Map Editor's drawing: the wanted previews once drawn
        DrawnVertexPreviews = new HashSet<string>(wanted.Where(n => VertexPreviews.TryGetValue(n, out var p) && p.Renderer.HasDrawn),
            StringComparer.OrdinalIgnoreCase);

        // The overlay is drawn again without the drawn previews' files, once the set of them changes
        if (ShowOverlay && OverlayContainer != null && !DrawnVertexPreviews.SetEquals(OverlayWithout))
            OverlayDirty = true;

        // The Map Editor's drawing stays hidden while any preview of the file is drawn
        UpdateHiddenNavmeshDrawings(new HashSet<string>(VertexPreviews.Where(p => p.Value.Renderer.HasDrawn).Select(p => p.Key),
            StringComparer.OrdinalIgnoreCase));
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

    /// <summary>
    /// Places the brush sphere on the hit at the brush's radius, and a smaller sphere on the face's nearest corner.
    /// </summary>
    private void UpdateVertexCursor()
    {
        var target = VertexEditActive ? (IsVertexDragStroke ? VertexStroke : null) ?? VertexHover : null;

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

        // Where it is now: a drag carries it
        var vertex = target.File.WorldVertex(target.Vertex);

        // The detection spheres have a radius of 1; while dragging, the brush stays with the vertices it holds
        var size = Math.Max(0.01f, VertexRadius);
        var cursor = IsVertexDragStroke ? VertexDragAnchor : target.Point;
        var cursorWorld = Matrix4x4.CreateScale(size) * Matrix4x4.CreateTranslation(cursor);

        // The marker stays small with a large brush, so it doesn't hide the vertices around it
        var markerSize = MathF.Min(size * 0.6f, VertexMaxMarkerSize);

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
