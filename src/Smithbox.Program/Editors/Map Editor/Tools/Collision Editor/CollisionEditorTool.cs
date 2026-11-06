using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
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
/// Reshapes map collision in the viewport. While editing, a cursor sphere sits where the mouse ray meets the nearest
/// displayed collision, and the nearest vertex of the triangle under it can be raised, lowered or dragged. The edits
/// are previewed until they are committed, which rebuilds the compressed meshes of the edited files, puts them in
/// the collision documents and writes the collision binders into the project.
/// </summary>
public class CollisionEditorTool
{
    private readonly MapEditorView View;
    private readonly ProjectEntry Project;

    private bool Active = false;

    // The decoded collision files, keyed by file name (which holds the map ID)
    private readonly Dictionary<string, CollisionEditorFile> Files = new(StringComparer.OrdinalIgnoreCase);

    // Files that were looked at and have nothing that can be edited, so they aren't decoded again
    private readonly HashSet<string> Unsupported = new(StringComparer.OrdinalIgnoreCase);

    // The file names of each map, by the resource name the viewport loads them under
    private readonly Dictionary<string, (int Count, Dictionary<string, string> Names)> ResourceNames = new();

    private Hit Hover;

    // The vertex being dragged, and how the drag follows the mouse
    private Hit Drag;
    private float DragHeight;
    private Vector3 DragGrabOffset;
    private List<MoveSet> DragMoves;

    // The cursor is drawn with the detection spheres (radius 1) and the vertex marker with the placement orb's sphere
    // (radius 0.1): these are uploaded at startup, while a sphere made when it is first needed can be drawn before
    // its geometry is, and its proxy doesn't try again
    private DebugPrimitiveRenderableProxy CursorWire;
    private DebugPrimitiveRenderableProxy CursorFill;
    private DebugPrimitiveRenderableProxy VertexMesh;

    private const float SphereRadius = 0.1f;
    private const float MaxVertexMarkerSize = 0.1f;

    // Collision meshes replaced by a commit, freed a few frames later so nothing draws from them when they go
    private const int ReleaseDelayFrames = 10;
    private readonly List<(CollisionSubmesh[] Meshes, int Frame)> PendingRelease = new();
    private int Frame;

    // Previews of committed files, drawn until the rebuilt loaded meshes are, so the collision never disappears
    private sealed class RetiringPreview(CollisionEditorFile file, List<MeshRenderableProxy> proxies, int startFrame)
    {
        public CollisionEditorFile File { get; } = file;
        public List<MeshRenderableProxy> Proxies { get; } = proxies;
        public int StartFrame { get; } = startFrame;
        public int ShownFrames { get; set; }
    }

    private readonly List<RetiringPreview> Retiring = new();

    // How long a committed preview waits for the rebuilt mesh before handing over anyway
    private const int RetireTimeoutFrames = 300;

    private record Hit(CollisionEditorFile File, CollisionEditorBody Body, int Triangle, int Vertex, Vector3 Point, float Distance);

    /// <summary>The vertices of one body that an edit moves, with how much of the move each takes.</summary>
    private record MoveSet(CollisionEditorFile File, CollisionEditorBody Body, List<(int Vertex, float Weight)> Falloff);

    private readonly CollisionEditorCreate Create;

    public CollisionEditorTool(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
        Create = new CollisionEditorCreate(view, project, this);
    }

    /// <summary>
    /// After the Create tab adds collision files: the names the viewport loads them under are worked out again, and
    /// files that were found to have nothing to edit are looked at again.
    /// </summary>
    public void OnCollisionCreated()
    {
        ResourceNames.Clear();
        Unsupported.Clear();
    }

    /// <summary>
    /// The games whose map collision the Map Editor holds as documents it can write back, with FromSoftware compressed
    /// meshes the shared builder makes.
    /// </summary>
    public bool CanUse()
    {
        return Project.Descriptor.ProjectType is ProjectType.ER or ProjectType.NR or ProjectType.AC6;
    }

    /// <summary>True while a vertex is dragged, so the viewport's clicks don't select anything.</summary>
    public bool IsDragging => Drag != null;

    private bool HasEdits => Files.Values.Any(f => f.IsEdited);

    #region Tool Window

    public void OnToolWindow()
    {
        ImGui.BeginChild("collisionEditorSection", ImGuiChildFlags.Borders);

        if (!CanUse())
        {
            GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Unsupported_Game"));
            ImGui.EndChild();
            return;
        }

        if (!CFG.Current.MapEditor_ModelLoad_Collisions || CFG.Current.MapEditor_SkipHavokLoad)
        {
            GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Collision_Not_Loaded"));
            ImGui.EndChild();
            return;
        }

        if (ImGui.BeginTabBar("collisionEditorTabs"))
        {
            if (ImGui.BeginTabItem($"{LOC.Get("MAP_CollisionEditor_Tab_Edit")}##collisionEditorTabEdit"))
            {
                DisplayEditTab();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem($"{LOC.Get("MAP_CollisionEditor_Tab_Create")}##collisionEditorTabCreate"))
            {
                Create.Display();
                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }

        ImGui.EndChild();
    }

    /// <summary>
    /// Reshaping existing collision: the cursor, its keys and options, and committing the edits.
    /// </summary>
    private void DisplayEditTab()
    {
        GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Description"));

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CollisionEditor_Actions_Header"), LOC.Get("MAP_CollisionEditor_Actions_Header_TT"));

        GUI.MultiButtonInput("collisionEditorToggle",
            "collisionEditorToggleButton",
            Active ? LOC.Get("MAP_CollisionEditor_Stop") : LOC.Get("MAP_CollisionEditor_Start"),
            Active ? LOC.Get("MAP_CollisionEditor_Stop_TT") : LOC.Get("MAP_CollisionEditor_Start_TT"),
            ToggleEditing);

        GUI.ConditionalMultiButtonInput("collisionEditorActions",
            "collisionEditorCommit", LOC.Get("MAP_CollisionEditor_Commit"), LOC.Get("MAP_CollisionEditor_Commit_TT"),
            CommitChanges, HasEdits,

            "collisionEditorDiscard", LOC.Get("MAP_CollisionEditor_Discard"), LOC.Get("MAP_CollisionEditor_Discard_TT"),
            DiscardChanges, HasEdits);

        DisplayLayerOptions();

        // Step
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CollisionEditor_Step_Header"), LOC.Get("MAP_CollisionEditor_Step_Header_TT"));

        ImGui.SetNextItemWidth(-1);
        if (ImGui.InputFloat("##collisionEditorStep", ref CFG.Current.MapEditor_CollisionEditor_Step))
            CFG.Current.MapEditor_CollisionEditor_Step = Math.Max(0.001f, CFG.Current.MapEditor_CollisionEditor_Step);
        GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Step_Input_TT"));

        // Radius
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CollisionEditor_Radius_Header"), LOC.Get("MAP_CollisionEditor_Radius_Header_TT"));

        ImGui.SetNextItemWidth(-1);
        if (ImGui.InputFloat("##collisionEditorRadius", ref CFG.Current.MapEditor_CollisionEditor_Radius))
            CFG.Current.MapEditor_CollisionEditor_Radius = Math.Max(0f, CFG.Current.MapEditor_CollisionEditor_Radius);
        GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Radius_Input_TT"));

        // Cursor Size
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CollisionEditor_Cursor_Size_Header"), LOC.Get("MAP_CollisionEditor_Cursor_Size_Header_TT"));

        ImGui.SetNextItemWidth(-1);
        if (ImGui.InputFloat("##collisionEditorCursorSize", ref CFG.Current.MapEditor_CollisionEditor_Cursor_Size))
            CFG.Current.MapEditor_CollisionEditor_Cursor_Size = Math.Max(0.01f, CFG.Current.MapEditor_CollisionEditor_Cursor_Size);
        GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Cursor_Size_Input_TT"));

        // Cursor Opacity
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CollisionEditor_Cursor_Opacity_Header"), LOC.Get("MAP_CollisionEditor_Cursor_Opacity_Header_TT"));

        ImGui.SetNextItemWidth(-1);
        ImGui.SliderFloat("##collisionEditorCursorOpacity", ref CFG.Current.MapEditor_CollisionEditor_Cursor_Fill_Alpha, 0f, 100f, "%.0f%%");
        GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Cursor_Opacity_Input_TT"));

        // Controls
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CollisionEditor_Controls_Header"), LOC.Get("MAP_CollisionEditor_Controls_Header_TT"));

        GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Control_Raise", InputManager.GetHint(KeybindID.MapEditor_CollisionEditor_Raise)));
        GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Control_Lower", InputManager.GetHint(KeybindID.MapEditor_CollisionEditor_Lower)));
        GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Control_Drag", InputManager.GetHint(KeybindID.MapEditor_CollisionEditor_Drag)));

        DisplayStatus();
    }

    /// <summary>
    /// Which collision layer the cursor edits, and whether the other layer follows.
    /// </summary>
    private void DisplayLayerOptions()
    {
        var bank = View.HavokCollisionBank;
        var layer = CFG.Current.MapEditor_CollisionEditor_Layer;

        // Layer
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CollisionEditor_Layer_Header"), LOC.Get("MAP_CollisionEditor_Layer_Header_TT"));

        ImGui.SetNextItemWidth(-1);
        if (ImGui.BeginCombo("##collisionEditorLayer", LOC.Get(layer.GetDisplayName())))
        {
            foreach (var type in bank.GetSupportedCollisionTypes())
            {
                if (ImGui.Selectable($"{LOC.Get(type.GetDisplayName())}##collisionEditorLayer{type}", type == layer))
                {
                    CFG.Current.MapEditor_CollisionEditor_Layer = type;

                    // The cursor and drag belong to the layer they were on
                    Drag = null;
                    Hover = null;
                }
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Layer_Input_TT"));

        if (!bank.IsCollisionTypeVisible(layer))
        {
            GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Layer_Hidden", LOC.Get(layer.GetDisplayName())));

            if (ImGui.Button($"{LOC.Get("MAP_CollisionEditor_Layer_Display", LOC.Get(layer.GetDisplayName()))}##collisionEditorLayerDisplay"))
            {
                bank.ToggleCollisionType(layer);
            }
            GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Layer_Display_TT"));
        }

        // Linked Layers
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CollisionEditor_Link_Header"), LOC.Get("MAP_CollisionEditor_Link_Header_TT"));

        ImGui.Checkbox($"{LOC.Get("MAP_CollisionEditor_Link_Toggle")}##collisionEditorLink", ref CFG.Current.MapEditor_CollisionEditor_Link_Layers);
        GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Link_Toggle_TT"));

        if (GetLinkedLayer(layer) is not { } linked)
        {
            GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Link_None", LOC.Get(layer.GetDisplayName())));
            return;
        }

        if (!CFG.Current.MapEditor_CollisionEditor_Link_Layers)
            return;

        GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Link_Follows", LOC.Get(linked.GetDisplayName()), LOC.Get(layer.GetDisplayName())));

        // Link Distance
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CollisionEditor_Link_Distance_Header"), LOC.Get("MAP_CollisionEditor_Link_Distance_Header_TT"));

        ImGui.SetNextItemWidth(-1);
        if (ImGui.InputFloat("##collisionEditorLinkDistance", ref CFG.Current.MapEditor_CollisionEditor_Link_Distance))
            CFG.Current.MapEditor_CollisionEditor_Link_Distance = Math.Max(0f, CFG.Current.MapEditor_CollisionEditor_Link_Distance);
        GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Link_Distance_Input_TT"));
    }

    /// <summary>
    /// The layer that follows edits to a layer: high and low are the same ground at two levels of detail. Fall
    /// protection is a barrier of its own, so it isn't linked.
    /// </summary>
    private static HavokCollisionType? GetLinkedLayer(HavokCollisionType layer)
    {
        return layer switch
        {
            HavokCollisionType.High => HavokCollisionType.Low,
            HavokCollisionType.Low => HavokCollisionType.High,
            _ => null
        };
    }

    private static char GetLayerLetter(HavokCollisionType layer)
    {
        return layer switch
        {
            HavokCollisionType.High => 'h',
            HavokCollisionType.FallProtection => 'f',
            _ => 'l'
        };
    }

    /// <summary>
    /// What is under the cursor, and which files have edits waiting to be committed.
    /// </summary>
    private void DisplayStatus()
    {
        var scene = View.ViewportHandler.ActiveViewport?.RenderScene;

        // Cursor
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CollisionEditor_Cursor_Header"), LOC.Get("MAP_CollisionEditor_Cursor_Header_TT"));

        if (scene != null && !scene.DrawFilter.HasFlag(RenderFilter.Collision))
        {
            GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Collision_Hidden"));
        }
        else if (!Active)
        {
            GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Cursor_Inactive"));
        }
        else if ((Drag ?? Hover) is { } hit)
        {
            var world = GetBodyWorld(hit.File, hit.Body);
            var position = Vector3.Transform(hit.Body.Vertices[hit.Vertex], world);

            GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Cursor_File", hit.File.FileName, hit.File.Entity.Name));
            GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Cursor_Vertex", hit.Vertex,
                $"{position.X:0.###}, {position.Y:0.###}, {position.Z:0.###}"));

            if (CFG.Current.MapEditor_CollisionEditor_Link_Layers && GetLinkedLayer(hit.File.Type) != null)
            {
                var linkedName = GetLinkedFileName(hit.File.FileName);
                var linked = linkedName != null && Files.ContainsKey(linkedName) ? linkedName : null;

                GUI.WrappedText(linked != null
                    ? LOC.Get("MAP_CollisionEditor_Cursor_Linked", linked)
                    : LOC.Get("MAP_CollisionEditor_Cursor_Linked_None"));
            }
        }
        else
        {
            GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Cursor_None"));
        }

        // Pending Edits
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CollisionEditor_Pending_Header"), LOC.Get("MAP_CollisionEditor_Pending_Header_TT"));

        var edited = Files.Values.Where(f => f.IsEdited).ToList();

        if (edited.Count == 0)
        {
            GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Pending_None"));
            return;
        }

        foreach (var file in edited)
        {
            var moved = file.Bodies.Sum(b => b.Vertices.Where((v, i) => v != b.OriginalVertices[i]).Count());

            GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Pending_File", file.FileName, moved));
        }
    }

    private void ToggleEditing()
    {
        Active = !Active;

        if (!Active)
        {
            Drag = null;
            Hover = null;
            DisposeCursor();

            // Files without edits are decoded again the next time they are needed
            foreach (var key in Files.Where(e => !e.Value.IsEdited).Select(e => e.Key).ToList())
            {
                Files[key].DisposePreview();
                Files.Remove(key);
            }
        }
    }

    private void DiscardChanges()
    {
        Drag = null;

        foreach (var file in Files.Values)
        {
            foreach (var body in file.Bodies)
                body.Reset();

            file.DisposePreview();
            SetOriginalVisible(file, true);
        }

        Smithbox.Log(this, LOC.Get("MAP_CollisionEditor_Discarded"));
    }

    #endregion

    #region Viewport

    /// <summary>
    /// Handles the cursor and keys while editing, and keeps the previews of the edited files up to date. Called each
    /// frame by the active viewport. Returns true while the viewport's clicks are taken, so they don't select.
    /// </summary>
    public bool HandleViewportInput(Ray ray, bool canInteract)
    {
        Frame++;
        ReleaseReplacedMeshes();

        if (!CanUse())
            return false;

        var scene = View.ViewportHandler.ActiveViewport?.RenderScene;

        if (scene == null)
            return false;

        DropUnloadedFiles();

        if (Active)
        {
            HandleEditing(scene, ray, canInteract);
        }

        UpdatePreviews(scene);
        UpdateRetiringPreviews();

        return IsDragging;
    }

    private void HandleEditing(RenderScene scene, Ray ray, bool canInteract)
    {
        // Mouse buttons fly the camera with the same keys, and typed text shouldn't edit
        var keysFree = canInteract
            && !ImGui.GetIO().WantTextInput
            && !InputManager.IsMouseDown(Veldrid.MouseButton.Left)
            && !InputManager.IsMouseDown(Veldrid.MouseButton.Right)
            && !InputManager.IsMouseDown(Veldrid.MouseButton.Middle);

        var collisionShown = scene.DrawFilter.HasFlag(RenderFilter.Collision);

        if (Drag != null)
        {
            if (!keysFree || !collisionShown || !InputManager.IsDown(KeybindID.MapEditor_CollisionEditor_Drag))
            {
                Drag = null;
            }
            else
            {
                ContinueDrag(ray);
            }
        }

        if (Drag == null)
        {
            Hover = canInteract && collisionShown ? RayCast(ray) : null;

            // Decoded ahead of the first edit, so the status can say what follows
            if (Hover != null)
                GetLinkedFile(Hover.File);

            if (keysFree && Hover != null && InputManager.IsPressed(KeybindID.MapEditor_CollisionEditor_Drag))
            {
                BeginDrag(Hover, ray);
            }
        }

        var target = Drag ?? Hover;

        if (keysFree && target != null)
        {
            var step = CFG.Current.MapEditor_CollisionEditor_Step;

            if (InputManager.IsPressedOrRepeated(KeybindID.MapEditor_CollisionEditor_Raise))
                Raise(target, step);

            if (InputManager.IsPressedOrRepeated(KeybindID.MapEditor_CollisionEditor_Lower))
                Raise(target, -step);
        }

        UpdateCursor(scene);
    }

    /// <summary>
    /// Moves the target vertex (and those around it, with a radius) up or down in the world.
    /// </summary>
    private void Raise(Hit target, float amount)
    {
        var moves = Drag != null ? DragMoves : GetMoves(target);

        foreach (var move in moves)
            MoveWorld(move, new Vector3(0, amount, 0));

        // The drag plane follows the vertex, so dragging after raising doesn't pull it back down
        if (Drag != null)
            DragHeight += amount;
    }

    /// <summary>
    /// The vertices an edit of the target moves: the target and those within the radius of it, then, with linked
    /// layers, the vertices of the other layer of the same part near the target.
    /// </summary>
    private List<MoveSet> GetMoves(Hit target)
    {
        var radius = CFG.Current.MapEditor_CollisionEditor_Radius;
        var moves = new List<MoveSet> { new(target.File, target.Body, target.Body.GetFalloff(target.Vertex, radius)) };

        var linked = GetLinkedFile(target.File);

        if (linked == null)
            return moves;

        var point = Vector3.Transform(target.Body.Vertices[target.Vertex], GetBodyWorld(target.File, target.Body));

        foreach (var body in linked.Bodies)
        {
            if (!Matrix4x4.Invert(GetBodyWorld(linked, body), out var inverse))
                continue;

            var falloff = body.GetLinkedFalloff(Vector3.Transform(point, inverse),
                CFG.Current.MapEditor_CollisionEditor_Link_Distance, radius);

            if (falloff.Count > 0)
                moves.Add(new MoveSet(linked, body, falloff));
        }

        return moves;
    }

    /// <summary>
    /// The other layer of the same part's collision (high for low, low for high), decoded if it hasn't been.
    /// Null with linking off, for fall protection, or if the part has no such file that can be edited.
    /// </summary>
    private CollisionEditorFile GetLinkedFile(CollisionEditorFile file)
    {
        if (!CFG.Current.MapEditor_CollisionEditor_Link_Layers || GetLinkedLayer(file.Type) == null)
            return null;

        var linkedName = GetLinkedFileName(file.FileName);

        if (linkedName == null || Unsupported.Contains(linkedName))
            return null;

        if (!Files.TryGetValue(linkedName, out var linked))
            linked = DecodeFile(file.Entity, linkedName);

        // A file is edited through the part it was first found on
        return linked?.Entity == file.Entity ? linked : null;
    }

    /// <summary>
    /// The file of the other layer: h10_00_00_00_002400.hkx.dcx for l10_00_00_00_002400.hkx.dcx, and back. Null if
    /// the map doesn't have it.
    /// </summary>
    private string GetLinkedFileName(string fileName)
    {
        var letter = char.ToLowerInvariant(fileName[0]) switch
        {
            'h' => 'l',
            'l' => 'h',
            _ => '\0'
        };

        if (letter == '\0')
            return null;

        var candidate = letter + fileName.Substring(1);

        foreach (var names in View.HavokCollisionBank.MapCollisions.Values)
        {
            var match = names.FirstOrDefault(n => string.Equals(n, candidate, StringComparison.OrdinalIgnoreCase));

            if (match != null)
                return match;
        }

        return null;
    }

    private void BeginDrag(Hit hit, Ray ray)
    {
        var world = GetBodyWorld(hit.File, hit.Body);
        var position = Vector3.Transform(hit.Body.Vertices[hit.Vertex], world);

        Drag = hit;
        DragHeight = position.Y;
        DragMoves = GetMoves(hit);

        // Keeps the vertex where it is relative to the mouse, so it doesn't jump to it
        DragGrabOffset = IntersectHeight(ray, DragHeight, out var point) ? position - point : Vector3.Zero;
        DragGrabOffset.Y = 0;
    }

    /// <summary>
    /// Puts the dragged vertex where the mouse ray meets the horizontal plane at its height.
    /// </summary>
    private void ContinueDrag(Ray ray)
    {
        if (!IntersectHeight(ray, DragHeight, out var point))
            return;

        var world = GetBodyWorld(Drag.File, Drag.Body);
        var current = Vector3.Transform(Drag.Body.Vertices[Drag.Vertex], world);
        var target = new Vector3(point.X + DragGrabOffset.X, DragHeight, point.Z + DragGrabOffset.Z);

        if (Vector3.DistanceSquared(current, target) < 1e-10f)
            return;

        // The linked layer takes the same move as the dragged vertex, so it stays under it
        foreach (var move in DragMoves)
            MoveWorld(move, target - current);
    }

    private static bool IntersectHeight(Ray ray, float height, out Vector3 point)
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

    /// <summary>Moves vertices by an offset in the world, which is turned into the space of their shape.</summary>
    private static void MoveWorld(MoveSet move, Vector3 offset)
    {
        if (!Matrix4x4.Invert(GetBodyWorld(move.File, move.Body), out var inverse))
            return;

        move.Body.Move(move.Falloff, Vector3.TransformNormal(offset, inverse));
    }

    private static Matrix4x4 GetBodyWorld(CollisionEditorFile file, CollisionEditorBody body)
    {
        return body.BodyTransform * file.Entity.GetWorldMatrix();
    }

    /// <summary>
    /// Finds the nearest displayed collision triangle under the ray, out of the collision parts of the loaded maps,
    /// and the corner of it nearest the hit.
    /// </summary>
    private Hit RayCast(Ray ray)
    {
        Hit nearest = null;
        var layerLetter = GetLayerLetter(CFG.Current.MapEditor_CollisionEditor_Layer);

        foreach (var entity in GetCollisionParts())
        {
            if (!entity.EditorVisible)
                continue;

            var entityWorld = entity.GetWorldMatrix();

            foreach (var proxy in GetCollisionProxies(entity))
            {
                var fileName = ResolveFileName(entity.MapID, proxy.ResourceHandle?.Name);

                if (fileName == null || Unsupported.Contains(fileName))
                    continue;

                // Only the edited layer is hit: the layers lie on top of each other, so the nearest would flicker
                if (char.ToLowerInvariant(fileName[0]) != layerLetter)
                    continue;

                if (!Files.TryGetValue(fileName, out var file))
                {
                    // Only decoded once the ray reaches the drawn mesh
                    if (!ray.Intersects(proxy.GetBounds()))
                        continue;

                    file = DecodeFile(entity, fileName);

                    if (file == null)
                        continue;
                }

                // A file is edited through the part it was first found on
                if (file.Entity != entity)
                    continue;

                foreach (var body in file.Bodies)
                {
                    if (!Matrix4x4.Invert(body.BodyTransform * entityWorld, out var inverse))
                        continue;

                    var origin = Vector3.Transform(ray.Origin, inverse);
                    var direction = Vector3.TransformNormal(ray.Direction, inverse);
                    var scale = direction.Length();

                    if (scale < 1e-8f)
                        continue;

                    if (!body.RayCast(new Ray(origin, direction / scale), out var triangle, out var distance))
                        continue;

                    var localPoint = origin + direction / scale * distance;

                    // Back into world units, so parts with a different scale compare correctly
                    distance /= scale;

                    if (nearest == null || distance < nearest.Distance)
                    {
                        var point = Vector3.Transform(localPoint, body.BodyTransform * entityWorld);

                        nearest = new Hit(file, body, triangle, body.NearestCorner(triangle, localPoint), point, distance);
                    }
                }
            }
        }

        return nearest;
    }

    private CollisionEditorFile DecodeFile(MsbEntity entity, string fileName)
    {
        var document = View.HavokCollisionBank.GetMapDocument(fileName);

        if (document == null)
        {
            Unsupported.Add(fileName);
            return null;
        }

        var bodies = new List<CollisionEditorBody>();

        try
        {
            foreach (var body in Havok.Shared.CollisionGeometry.Bodies(document.Objects))
            {
                var decoded = CollisionEditorBody.Decode(body);

                if (decoded != null)
                    bodies.Add(decoded);
            }
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MAP_CollisionEditor_Decode_Failed", fileName), e);
            bodies.Clear();
        }

        if (bodies.Count == 0)
        {
            Unsupported.Add(fileName);
            return null;
        }

        var file = new CollisionEditorFile(entity, entity.MapID, fileName, GetCollisionType(fileName), document, bodies);
        Files[fileName] = file;

        return file;
    }

    private static HavokCollisionType GetCollisionType(string fileName)
    {
        return char.ToLowerInvariant(fileName[0]) switch
        {
            'h' => HavokCollisionType.High,
            'f' => HavokCollisionType.FallProtection,
            _ => HavokCollisionType.Low
        };
    }

    /// <summary>
    /// The name of the collision file a viewport mesh was loaded from, from the bank's list of the map's files.
    /// </summary>
    private string ResolveFileName(string mapId, string resourceName)
    {
        if (string.IsNullOrEmpty(mapId) || string.IsNullOrEmpty(resourceName))
            return null;

        if (!View.HavokCollisionBank.MapCollisions.TryGetValue(mapId, out var fileNames))
            return null;

        if (!ResourceNames.TryGetValue(mapId, out var cached) || cached.Count != fileNames.Count)
        {
            var names = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var name in fileNames)
                names.TryAdd(ResourceName.Normalize(name), name);

            cached = (fileNames.Count, names);
            ResourceNames[mapId] = cached;
        }

        return cached.Names.GetValueOrDefault(resourceName);
    }

    private IEnumerable<MsbEntity> GetCollisionParts()
    {
        foreach (var entry in Project.Handler.MapData.PrimaryBank.Maps)
        {
            if (entry.Value.MapContainer == null)
                continue;

            foreach (var ent in entry.Value.MapContainer.Objects)
            {
                if (ent is MsbEntity msbEnt && EntityHelper.IsPartCollision(ent))
                    yield return msbEnt;
            }
        }
    }

    /// <summary>The meshes of a collision part: one per displayed collision type.</summary>
    private static IEnumerable<MeshRenderableProxy> GetCollisionProxies(MsbEntity entity)
    {
        if (entity.RenderSceneMesh is MeshRenderableProxy mesh)
            yield return mesh;

        if (entity.CollisionLayerMeshes == null)
            yield break;

        foreach (var layer in entity.CollisionLayerMeshes)
        {
            if (layer is MeshRenderableProxy layerMesh)
                yield return layerMesh;
        }
    }

    /// <summary>The meshes of every collision part drawn from a file.</summary>
    private IEnumerable<MeshRenderableProxy> GetFileProxies(string mapId, string fileName)
    {
        foreach (var entity in GetCollisionParts())
        {
            if (entity.MapID != mapId)
                continue;

            foreach (var proxy in GetCollisionProxies(entity))
            {
                if (string.Equals(ResolveFileName(mapId, proxy.ResourceHandle?.Name), fileName, StringComparison.OrdinalIgnoreCase))
                    yield return proxy;
            }
        }
    }

    /// <summary>
    /// Drops the files of maps that were unloaded (or read again), whose documents are no longer the bank's.
    /// </summary>
    private void DropUnloadedFiles()
    {
        // Committed previews of unloaded maps have nothing left to hand over to
        for (var i = Retiring.Count - 1; i >= 0; i--)
        {
            var file = Retiring[i].File;

            if (!View.HavokCollisionBank.MapDocuments.TryGetValue(file.FileName, out var document) || document != file.Document)
            {
                file.DisposePreview();
                Retiring.RemoveAt(i);
            }
        }

        List<string> dropped = null;

        foreach (var (key, file) in Files)
        {
            if (!View.HavokCollisionBank.MapDocuments.TryGetValue(file.FileName, out var document) || document != file.Document)
            {
                (dropped ??= new()).Add(key);
            }
        }

        if (dropped == null)
            return;

        foreach (var key in dropped)
        {
            Files[key].DisposePreview();
            Files.Remove(key);
        }

        if (Drag != null && !Files.ContainsValue(Drag.File))
            Drag = null;

        if (Hover != null && !Files.ContainsValue(Hover.File))
            Hover = null;

        Unsupported.Clear();
        ResourceNames.Clear();
    }

    #endregion

    #region Display

    /// <summary>
    /// Draws the edited files from their edited vertices, in place of their loaded meshes.
    /// </summary>
    private void UpdatePreviews(RenderScene scene)
    {
        foreach (var file in Files.Values)
        {
            if (!file.IsEdited)
                continue;

            var world = file.Entity.GetWorldMatrix();
            var color = GetPreviewColor(file.Type);
            // A linked layer is edited while it isn't displayed, so its preview follows its display too
            var visible = file.Entity.EditorVisible && View.HavokCollisionBank.IsCollisionTypeVisible(file.Type);

            var ready = true;

            foreach (var body in file.Bodies)
                ready &= body.UpdatePreview(scene, world, color, visible);

            // The loaded mesh stays drawn until the preview is, so the collision never disappears while editing
            if (ready)
                SetOriginalVisible(file, false);
        }
    }

    private static Color GetPreviewColor(HavokCollisionType type)
    {
        var color = type switch
        {
            HavokCollisionType.High => CFG.Current.Viewport_High_Collision_Color,
            HavokCollisionType.FallProtection => CFG.Current.Viewport_FallProtection_Collision_Color,
            _ => CFG.Current.Viewport_Collision_Color
        };

        return Color.FromArgb(255, (int)color.X, (int)color.Y, (int)color.Z);
    }

    /// <summary>
    /// Shows or hides the loaded meshes of a file. Shown again, they follow their part's visibility.
    /// </summary>
    private void SetOriginalVisible(CollisionEditorFile file, bool visible)
    {
        foreach (var proxy in GetFileProxies(file.MapId, file.FileName))
        {
            var target = visible && file.Entity.EditorVisible;

            if (proxy.Visible != target)
                proxy.Visible = target;
        }
    }

    /// <summary>
    /// Places the cursor sphere on the hit, and a smaller sphere on the vertex that will be moved.
    /// </summary>
    private void UpdateCursor(RenderScene scene)
    {
        var target = Drag ?? Hover;

        if (target == null)
        {
            SetVisible(CursorWire, false);
            SetVisible(CursorFill, false);
            SetVisible(VertexMesh, false);

            return;
        }

        var cursorColor = ToColor(CFG.Current.MapEditor_CollisionEditor_Cursor_Color);

        // A wireframe over a translucent fill, so the collision under a large cursor can still be seen
        CursorWire ??= RenderableHelper.GetDetectionSphereProxy(scene, cursorColor);
        CursorFill ??= RenderableHelper.GetDetectionSphereFillProxy(scene, cursorColor);

        VertexMesh ??= new DebugPrimitiveRenderableProxy(scene.OpaqueRenderables, RenderableHelper._placementOrb)
        {
            Pickable = false,
            RenderOverlay = true,
            BaseColor = ToColor(CFG.Current.MapEditor_CollisionEditor_Vertex_Color),
            HighlightedColor = ToColor(CFG.Current.MapEditor_CollisionEditor_Vertex_Color)
        };

        // Follows the option as it is changed
        var fill = ColorHelper.GetTransparencyColor(cursorColor, CFG.Current.MapEditor_CollisionEditor_Cursor_Fill_Alpha);

        if (CursorFill.BaseColor != fill)
        {
            CursorFill.BaseColor = fill;
            CursorFill.HighlightedColor = fill;
        }

        var size = CFG.Current.MapEditor_CollisionEditor_Cursor_Size;
        var vertex = Vector3.Transform(target.Body.Vertices[target.Vertex], GetBodyWorld(target.File, target.Body));

        // While dragging, the cursor stays on the vertex it holds
        var cursor = Drag != null ? vertex : target.Point;
        var cursorWorld = Matrix4x4.CreateScale(size) * Matrix4x4.CreateTranslation(cursor);

        // The marker stays small with a large cursor, so it doesn't hide the vertices around it
        var markerSize = MathF.Min(size * 0.6f, MaxVertexMarkerSize);

        CursorWire.World = cursorWorld;
        CursorFill.World = cursorWorld;
        VertexMesh.World = Matrix4x4.CreateScale(markerSize / SphereRadius) * Matrix4x4.CreateTranslation(vertex);

        SetVisible(CursorWire, true);
        SetVisible(CursorFill, true);
        SetVisible(VertexMesh, true);
    }

    private static Color ToColor(Vector3 color)
    {
        return Color.FromArgb(255, (int)color.X, (int)color.Y, (int)color.Z);
    }

    private static void SetVisible(DebugPrimitiveRenderableProxy proxy, bool visible)
    {
        if (proxy != null && proxy.Visible != visible)
            proxy.Visible = visible;
    }

    private void DisposeCursor()
    {
        CursorWire?.Dispose();
        CursorFill?.Dispose();
        VertexMesh?.Dispose();

        CursorWire = null;
        CursorFill = null;
        VertexMesh = null;
    }

    #endregion

    #region Commit

    /// <summary>
    /// Rebuilds the compressed meshes of the edited files from their edited vertices, gives them to their bodies as
    /// one action, then writes the collision binders they are in into the project.
    /// </summary>
    private void CommitChanges()
    {
        Drag = null;

        var swaps = new List<CollisionEditorAction.ShapeSwap>();
        var edited = Files.Values.Where(f => f.IsEdited).ToList();

        foreach (var file in edited)
        {
            var map = file.Entity.Container as MapContainer;

            foreach (var body in file.Bodies.Where(b => b.IsEdited))
            {
                var (vertices, indices, materials) = body.GetBuildInput();

                if (indices.Count == 0)
                {
                    Smithbox.LogError(this, LOC.Get("MAP_CollisionEditor_Commit_No_Triangles", file.FileName));
                    return;
                }

                object shape;

                try
                {
                    shape = CollisionEditorShapes.BuildShape(body.Body.Shape, vertices, indices, materials);
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, LOC.Get("MAP_CollisionEditor_Commit_Build_Failed", file.FileName), e);
                    return;
                }

                if (shape == null)
                {
                    Smithbox.LogError(this, LOC.Get("MAP_CollisionEditor_Commit_Build_Failed", file.FileName));
                    return;
                }

                swaps.Add(new CollisionEditorAction.ShapeSwap(map, file.FileName, body.Body, body.Body.Shape, shape));

                Smithbox.Log(this, LOC.Get("MAP_CollisionEditor_Commit_Rebuilt", file.FileName,
                    body.TriangleCount, indices.Count / 3, vertices.Count, materials.Distinct().Count()));
            }
        }

        if (swaps.Count == 0)
            return;

        View.ViewportActionManager.ExecuteAction(new CollisionEditorAction(swaps, OnShapesChanged));

        // The binders are written now, so the committed files are updated in the project straight away
        var success = true;

        foreach (var (mapId, type) in edited.Select(f => (f.MapId, f.BinderType)).Distinct())
        {
            success &= View.HavokCollisionBank.SaveMapCollision(mapId, type);
        }

        var names = string.Join(", ", edited.Select(f => f.FileName));

        if (success)
            Smithbox.Log(this, LOC.Get("MAP_CollisionEditor_Committed", names));
        else
            Smithbox.LogError(this, LOC.Get("MAP_CollisionEditor_Commit_Save_Failed", names));
    }

    /// <summary>
    /// After a commit, or its undo or redo: the decoded files are out of date, so they are dropped, and the loaded
    /// meshes are rebuilt from the documents.
    /// </summary>
    private void OnShapesChanged(IReadOnlyList<CollisionEditorAction.ShapeSwap> swaps)
    {
        foreach (var fileName in swaps.Select(s => s.FileName).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var map = swaps.First(s => string.Equals(s.FileName, fileName, StringComparison.OrdinalIgnoreCase)).Map;

            Files.TryGetValue(fileName, out var file);
            var mapId = map?.Name ?? file?.MapId;

            if (file != null)
            {
                Files.Remove(fileName);

                if (Drag?.File == file)
                    Drag = null;

                if (Hover?.File == file)
                    Hover = null;
            }

            // A previewed file keeps its preview drawn, and its loaded mesh hidden, until the rebuilt loaded mesh
            // has been uploaded, so the collision doesn't disappear while it is
            var previewed = file != null && file.Bodies.Any(b => b.PreviewMeshes.Any());
            var proxies = RefreshLoadedMesh(mapId, fileName, showNow: !previewed);

            if (!previewed)
            {
                file?.DisposePreview();
            }
            else if (proxies == null)
            {
                // Not rebuilt, so there is nothing to wait for
                file.DisposePreview();
                SetOriginalVisible(file, true);
            }
            else
            {
                Retiring.Add(new RetiringPreview(file, proxies, Frame));
            }
        }
    }

    /// <summary>
    /// Hands the collision back from the previews of committed files to their rebuilt loaded meshes: once those are
    /// built (or after a timeout), they are shown alongside the preview for a few frames, then the preview goes.
    /// </summary>
    private void UpdateRetiringPreviews()
    {
        for (var i = Retiring.Count - 1; i >= 0; i--)
        {
            var retiring = Retiring[i];

            if (retiring.ShownFrames == 0)
            {
                var built = retiring.Proxies.All(p => p.AreRenderablesConstructed());

                if (!built && Frame - retiring.StartFrame < RetireTimeoutFrames)
                    continue;

                SetOriginalVisible(retiring.File, true);
            }

            retiring.ShownFrames++;

            if (retiring.ShownFrames > CollisionEditorBody.OverlapFrames)
            {
                retiring.File.DisposePreview();
                Retiring.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Rebuilds the drawn mesh of a file from its document, for every part drawn from it. With showNow, they are shown
    /// again straight away; otherwise the caller shows them once they are built. Returns the meshes rebuilt, or null
    /// if any couldn't be.
    /// </summary>
    private List<MeshRenderableProxy> RefreshLoadedMesh(string mapId, string fileName, bool showNow)
    {
        var document = View.HavokCollisionBank.GetMapDocument(fileName);
        var refreshed = new HashSet<HavokCollisionResource>();
        var proxies = new List<MeshRenderableProxy>();
        var failed = false;

        foreach (var proxy in GetFileProxies(mapId, fileName).ToList())
        {
            var resource = (proxy.ResourceHandle as ResourceHandle<HavokCollisionResource>)?.Get();

            if (resource == null || document == null)
            {
                failed = true;
                continue;
            }

            if (refreshed.Add(resource))
            {
                var old = HavokCollisionMeshBuilder.RefreshCollisionMesh(resource, document);

                if (old == null)
                {
                    failed = true;
                    continue;
                }

                PendingRelease.Add((old, Frame));
            }

            if (proxy._meshProvider is CollisionMeshProvider provider)
                provider._bounds = resource.Bounds;

            foreach (var submesh in proxy.Submeshes)
                submesh.ScheduleRenderableConstruction();

            proxies.Add(proxy);
        }

        // Shown again, following the visibility of their parts. A failed rebuild has nothing to wait for.
        if (showNow || failed)
        {
            foreach (var entity in GetCollisionParts().Where(e => e.MapID == mapId))
            {
                foreach (var proxy in GetCollisionProxies(entity))
                {
                    if (string.Equals(ResolveFileName(mapId, proxy.ResourceHandle?.Name), fileName, StringComparison.OrdinalIgnoreCase)
                        && proxy.Visible != entity.EditorVisible)
                    {
                        proxy.Visible = entity.EditorVisible;
                    }
                }
            }
        }

        if (failed)
        {
            Smithbox.Log(this, LOC.Get("MAP_CollisionEditor_Refresh_Failed", fileName));
            return null;
        }

        return proxies;
    }

    private void ReleaseReplacedMeshes()
    {
        for (var i = PendingRelease.Count - 1; i >= 0; i--)
        {
            if (Frame - PendingRelease[i].Frame < ReleaseDelayFrames)
                continue;

            foreach (var mesh in PendingRelease[i].Meshes)
                mesh.GeomBuffer?.Dispose();

            PendingRelease.RemoveAt(i);
        }
    }

    #endregion
}
