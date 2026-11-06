using Hexa.NET.ImGui;
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
/// Reshapes map collision in the viewport. While editing, a cursor sphere sits where the mouse ray meets the nearest
/// displayed collision, and the nearest vertex of the triangle under it can be raised, lowered or dragged. The edits
/// are previewed until they are committed, which rebuilds the compressed meshes of the edited files, puts them in
/// the collision documents and writes the collision binders into the project.
/// </summary>
public class CollisionMakerTool
{
    private readonly MapEditorView View;
    private readonly ProjectEntry Project;

    private bool Active = false;

    // The decoded collision files, keyed by file name (which holds the map ID)
    private readonly Dictionary<string, CollisionMakerFile> Files = new(StringComparer.OrdinalIgnoreCase);

    // Files that were looked at and have nothing that can be edited, so they aren't decoded again
    private readonly HashSet<string> Unsupported = new(StringComparer.OrdinalIgnoreCase);

    // The file names of each map, by the resource name the viewport loads them under
    private readonly Dictionary<string, (int Count, Dictionary<string, string> Names)> ResourceNames = new();

    private Hit Hover;

    // The vertex being dragged, and how the drag follows the mouse
    private Hit Drag;
    private float DragHeight;
    private Vector3 DragGrabOffset;
    private List<(int Vertex, float Weight)> DragFalloff;

    private DebugPrimitiveRenderableProxy CursorMesh;
    private DebugPrimitiveRenderableProxy VertexMesh;

    // The placement orb's sphere, which is already uploaded: a sphere made when it is first needed can be drawn
    // before its geometry is, and its proxy doesn't try again
    private const float SphereRadius = 0.1f;

    // Collision meshes replaced by a commit, freed a few frames later so nothing draws from them when they go
    private const int ReleaseDelayFrames = 10;
    private readonly List<(CollisionSubmesh[] Meshes, int Frame)> PendingRelease = new();
    private int Frame;

    private record Hit(CollisionMakerFile File, CollisionMakerBody Body, int Triangle, int Vertex, Vector3 Point, float Distance);

    public CollisionMakerTool(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
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
        ImGui.BeginChild("collisionMakerSection", ImGuiChildFlags.Borders);

        if (!CanUse())
        {
            GUI.WrappedText(LOC.Get("MAP_CollisionMaker_Unsupported_Game"));
            ImGui.EndChild();
            return;
        }

        GUI.WrappedText(LOC.Get("MAP_CollisionMaker_Description"));

        if (!CFG.Current.MapEditor_ModelLoad_Collisions || CFG.Current.MapEditor_SkipHavokLoad)
        {
            GUI.Spacer();
            GUI.WrappedText(LOC.Get("MAP_CollisionMaker_Collision_Not_Loaded"));
            ImGui.EndChild();
            return;
        }

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CollisionMaker_Actions_Header"), LOC.Get("MAP_CollisionMaker_Actions_Header_TT"));

        GUI.MultiButtonInput("collisionMakerToggle",
            "collisionMakerToggleButton",
            Active ? LOC.Get("MAP_CollisionMaker_Stop") : LOC.Get("MAP_CollisionMaker_Start"),
            Active ? LOC.Get("MAP_CollisionMaker_Stop_TT") : LOC.Get("MAP_CollisionMaker_Start_TT"),
            ToggleEditing);

        GUI.ConditionalMultiButtonInput("collisionMakerActions",
            "collisionMakerCommit", LOC.Get("MAP_CollisionMaker_Commit"), LOC.Get("MAP_CollisionMaker_Commit_TT"),
            CommitChanges, HasEdits,

            "collisionMakerDiscard", LOC.Get("MAP_CollisionMaker_Discard"), LOC.Get("MAP_CollisionMaker_Discard_TT"),
            DiscardChanges, HasEdits);

        // Step
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CollisionMaker_Step_Header"), LOC.Get("MAP_CollisionMaker_Step_Header_TT"));

        ImGui.SetNextItemWidth(-1);
        if (ImGui.InputFloat("##collisionMakerStep", ref CFG.Current.MapEditor_CollisionMaker_Step))
            CFG.Current.MapEditor_CollisionMaker_Step = Math.Max(0.001f, CFG.Current.MapEditor_CollisionMaker_Step);
        GUI.Tooltip(LOC.Get("MAP_CollisionMaker_Step_Input_TT"));

        // Radius
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CollisionMaker_Radius_Header"), LOC.Get("MAP_CollisionMaker_Radius_Header_TT"));

        ImGui.SetNextItemWidth(-1);
        if (ImGui.InputFloat("##collisionMakerRadius", ref CFG.Current.MapEditor_CollisionMaker_Radius))
            CFG.Current.MapEditor_CollisionMaker_Radius = Math.Max(0f, CFG.Current.MapEditor_CollisionMaker_Radius);
        GUI.Tooltip(LOC.Get("MAP_CollisionMaker_Radius_Input_TT"));

        // Cursor Size
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CollisionMaker_Cursor_Size_Header"), LOC.Get("MAP_CollisionMaker_Cursor_Size_Header_TT"));

        ImGui.SetNextItemWidth(-1);
        if (ImGui.InputFloat("##collisionMakerCursorSize", ref CFG.Current.MapEditor_CollisionMaker_Cursor_Size))
            CFG.Current.MapEditor_CollisionMaker_Cursor_Size = Math.Max(0.01f, CFG.Current.MapEditor_CollisionMaker_Cursor_Size);
        GUI.Tooltip(LOC.Get("MAP_CollisionMaker_Cursor_Size_Input_TT"));

        // Controls
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CollisionMaker_Controls_Header"), LOC.Get("MAP_CollisionMaker_Controls_Header_TT"));

        GUI.WrappedText(LOC.Get("MAP_CollisionMaker_Control_Raise", InputManager.GetHint(KeybindID.MapEditor_CollisionMaker_Raise)));
        GUI.WrappedText(LOC.Get("MAP_CollisionMaker_Control_Lower", InputManager.GetHint(KeybindID.MapEditor_CollisionMaker_Lower)));
        GUI.WrappedText(LOC.Get("MAP_CollisionMaker_Control_Drag", InputManager.GetHint(KeybindID.MapEditor_CollisionMaker_Drag)));

        DisplayStatus();

        ImGui.EndChild();
    }

    /// <summary>
    /// What is under the cursor, and which files have edits waiting to be committed.
    /// </summary>
    private void DisplayStatus()
    {
        var scene = View.ViewportHandler.ActiveViewport?.RenderScene;

        // Cursor
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CollisionMaker_Cursor_Header"), LOC.Get("MAP_CollisionMaker_Cursor_Header_TT"));

        if (scene != null && !scene.DrawFilter.HasFlag(RenderFilter.Collision))
        {
            GUI.WrappedText(LOC.Get("MAP_CollisionMaker_Collision_Hidden"));
        }
        else if (!Active)
        {
            GUI.WrappedText(LOC.Get("MAP_CollisionMaker_Cursor_Inactive"));
        }
        else if ((Drag ?? Hover) is { } hit)
        {
            var world = GetBodyWorld(hit.File, hit.Body);
            var position = Vector3.Transform(hit.Body.Vertices[hit.Vertex], world);

            GUI.WrappedText(LOC.Get("MAP_CollisionMaker_Cursor_File", hit.File.FileName, hit.File.Entity.Name));
            GUI.WrappedText(LOC.Get("MAP_CollisionMaker_Cursor_Vertex", hit.Vertex,
                $"{position.X:0.###}, {position.Y:0.###}, {position.Z:0.###}"));
        }
        else
        {
            GUI.WrappedText(LOC.Get("MAP_CollisionMaker_Cursor_None"));
        }

        // Pending Edits
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CollisionMaker_Pending_Header"), LOC.Get("MAP_CollisionMaker_Pending_Header_TT"));

        var edited = Files.Values.Where(f => f.IsEdited).ToList();

        if (edited.Count == 0)
        {
            GUI.WrappedText(LOC.Get("MAP_CollisionMaker_Pending_None"));
            return;
        }

        foreach (var file in edited)
        {
            var moved = file.Bodies.Sum(b => b.Vertices.Where((v, i) => v != b.OriginalVertices[i]).Count());

            GUI.WrappedText(LOC.Get("MAP_CollisionMaker_Pending_File", file.FileName, moved));
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

        Smithbox.Log(this, LOC.Get("MAP_CollisionMaker_Discarded"));
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
            if (!keysFree || !collisionShown || !InputManager.IsDown(KeybindID.MapEditor_CollisionMaker_Drag))
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

            if (keysFree && Hover != null && InputManager.IsPressed(KeybindID.MapEditor_CollisionMaker_Drag))
            {
                BeginDrag(Hover, ray);
            }
        }

        var target = Drag ?? Hover;

        if (keysFree && target != null)
        {
            var step = CFG.Current.MapEditor_CollisionMaker_Step;

            if (InputManager.IsPressedOrRepeated(KeybindID.MapEditor_CollisionMaker_Raise))
                Raise(target, step);

            if (InputManager.IsPressedOrRepeated(KeybindID.MapEditor_CollisionMaker_Lower))
                Raise(target, -step);
        }

        UpdateCursor(scene);
    }

    /// <summary>
    /// Moves the target vertex (and those around it, with a radius) up or down in the world.
    /// </summary>
    private void Raise(Hit target, float amount)
    {
        var falloff = Drag != null ? DragFalloff : target.Body.GetFalloff(target.Vertex, CFG.Current.MapEditor_CollisionMaker_Radius);

        MoveWorld(target, falloff, new Vector3(0, amount, 0));

        // The drag plane follows the vertex, so dragging after raising doesn't pull it back down
        if (Drag != null)
            DragHeight += amount;
    }

    private void BeginDrag(Hit hit, Ray ray)
    {
        var world = GetBodyWorld(hit.File, hit.Body);
        var position = Vector3.Transform(hit.Body.Vertices[hit.Vertex], world);

        Drag = hit;
        DragHeight = position.Y;
        DragFalloff = hit.Body.GetFalloff(hit.Vertex, CFG.Current.MapEditor_CollisionMaker_Radius);

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

        MoveWorld(Drag, DragFalloff, target - current);
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
    private void MoveWorld(Hit target, List<(int Vertex, float Weight)> falloff, Vector3 offset)
    {
        if (!Matrix4x4.Invert(GetBodyWorld(target.File, target.Body), out var inverse))
            return;

        target.Body.Move(falloff, Vector3.TransformNormal(offset, inverse));
    }

    private static Matrix4x4 GetBodyWorld(CollisionMakerFile file, CollisionMakerBody body)
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

    private CollisionMakerFile DecodeFile(MsbEntity entity, string fileName)
    {
        var document = View.HavokCollisionBank.GetMapDocument(fileName);

        if (document == null)
        {
            Unsupported.Add(fileName);
            return null;
        }

        var bodies = new List<CollisionMakerBody>();

        try
        {
            foreach (var body in Havok.Shared.CollisionGeometry.Bodies(document.Objects))
            {
                var decoded = CollisionMakerBody.Decode(body);

                if (decoded != null)
                    bodies.Add(decoded);
            }
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MAP_CollisionMaker_Decode_Failed", fileName), e);
            bodies.Clear();
        }

        if (bodies.Count == 0)
        {
            Unsupported.Add(fileName);
            return null;
        }

        var file = new CollisionMakerFile(entity, entity.MapID, fileName, GetCollisionType(fileName), document, bodies);
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
            var visible = file.Entity.EditorVisible;

            foreach (var body in file.Bodies)
                body.UpdatePreview(scene, world, color, visible);

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
    private void SetOriginalVisible(CollisionMakerFile file, bool visible)
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
            if (CursorMesh != null)
                CursorMesh.Visible = false;

            if (VertexMesh != null)
                VertexMesh.Visible = false;

            return;
        }

        CursorMesh ??= CreateSphereProxy(scene, CFG.Current.MapEditor_CollisionMaker_Cursor_Color);
        VertexMesh ??= CreateSphereProxy(scene, CFG.Current.MapEditor_CollisionMaker_Vertex_Color);

        var size = CFG.Current.MapEditor_CollisionMaker_Cursor_Size / SphereRadius;
        var vertex = Vector3.Transform(target.Body.Vertices[target.Vertex], GetBodyWorld(target.File, target.Body));

        // While dragging, the cursor stays on the vertex it holds
        var cursor = Drag != null ? vertex : target.Point;

        CursorMesh.World = Matrix4x4.CreateScale(size) * Matrix4x4.CreateTranslation(cursor);
        VertexMesh.World = Matrix4x4.CreateScale(size * 0.6f) * Matrix4x4.CreateTranslation(vertex);

        CursorMesh.Visible = true;
        VertexMesh.Visible = true;
    }

    private static DebugPrimitiveRenderableProxy CreateSphereProxy(RenderScene scene, Vector3 color)
    {
        var fill = Color.FromArgb(255, (int)color.X, (int)color.Y, (int)color.Z);

        return new DebugPrimitiveRenderableProxy(scene.OpaqueRenderables, RenderableHelper._placementOrb)
        {
            Pickable = false,
            RenderOverlay = true,
            BaseColor = fill,
            HighlightedColor = fill
        };
    }

    private void DisposeCursor()
    {
        CursorMesh?.Dispose();
        VertexMesh?.Dispose();

        CursorMesh = null;
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

        var swaps = new List<CollisionMakerAction.ShapeSwap>();
        var edited = Files.Values.Where(f => f.IsEdited).ToList();

        foreach (var file in edited)
        {
            var map = file.Entity.Container as MapContainer;

            foreach (var body in file.Bodies.Where(b => b.IsEdited))
            {
                var (vertices, indices, materials) = body.GetBuildInput();

                if (indices.Count == 0)
                {
                    Smithbox.LogError(this, LOC.Get("MAP_CollisionMaker_Commit_No_Triangles", file.FileName));
                    return;
                }

                object shape;

                try
                {
                    shape = CollisionMakerShapes.BuildShape(body.Body.Shape, vertices, indices, materials);
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, LOC.Get("MAP_CollisionMaker_Commit_Build_Failed", file.FileName), e);
                    return;
                }

                if (shape == null)
                {
                    Smithbox.LogError(this, LOC.Get("MAP_CollisionMaker_Commit_Build_Failed", file.FileName));
                    return;
                }

                swaps.Add(new CollisionMakerAction.ShapeSwap(map, file.FileName, body.Body, body.Body.Shape, shape));

                Smithbox.Log(this, LOC.Get("MAP_CollisionMaker_Commit_Rebuilt", file.FileName,
                    body.TriangleCount, indices.Count / 3, vertices.Count, materials.Distinct().Count()));
            }
        }

        if (swaps.Count == 0)
            return;

        View.ViewportActionManager.ExecuteAction(new CollisionMakerAction(swaps, OnShapesChanged));

        // The binders are written now, so the committed files are updated in the project straight away
        var success = true;

        foreach (var (mapId, type) in edited.Select(f => (f.MapId, f.BinderType)).Distinct())
        {
            success &= View.HavokCollisionBank.SaveMapCollision(mapId, type);
        }

        var names = string.Join(", ", edited.Select(f => f.FileName));

        if (success)
            Smithbox.Log(this, LOC.Get("MAP_CollisionMaker_Committed", names));
        else
            Smithbox.LogError(this, LOC.Get("MAP_CollisionMaker_Commit_Save_Failed", names));
    }

    /// <summary>
    /// After a commit, or its undo or redo: the decoded files are out of date, so they are dropped, and the loaded
    /// meshes are rebuilt from the documents.
    /// </summary>
    private void OnShapesChanged(IReadOnlyList<CollisionMakerAction.ShapeSwap> swaps)
    {
        foreach (var fileName in swaps.Select(s => s.FileName).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (Files.TryGetValue(fileName, out var file))
            {
                file.DisposePreview();
                Files.Remove(fileName);

                if (Drag?.File == file)
                    Drag = null;

                if (Hover?.File == file)
                    Hover = null;
            }

            var map = swaps.First(s => string.Equals(s.FileName, fileName, StringComparison.OrdinalIgnoreCase)).Map;
            var mapId = map?.Name ?? file?.MapId;

            RefreshLoadedMesh(mapId, fileName);
        }
    }

    /// <summary>
    /// Rebuilds the drawn mesh of a file from its document, for every part drawn from it, and shows it again.
    /// </summary>
    private void RefreshLoadedMesh(string mapId, string fileName)
    {
        var document = View.HavokCollisionBank.GetMapDocument(fileName);
        var refreshed = new HashSet<HavokCollisionResource>();
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
        }

        // Shown again, following the visibility of their parts
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

        if (failed)
            Smithbox.Log(this, LOC.Get("MAP_CollisionMaker_Refresh_Failed", fileName));
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
