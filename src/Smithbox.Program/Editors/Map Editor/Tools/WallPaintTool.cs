using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using StudioCore.Renderer;
using System.Collections.Generic;
using System.Numerics;
using Veldrid.Utilities;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// What a stroke of the Wall Painting tool does to the triangles it passes over.
/// </summary>
public enum WallPaintMode
{
    Wall,
    NotWall,
    Remove
}

/// <summary>
/// Paints the WALL files of Sekiro collisions in the viewport. Each triangle of a low collision under the
/// cursor is marked entirely as a wall or not a wall, or removed from the file, replacing any finer cells it had.
/// A stroke, from pressing the button to releasing it, is undone as one action.
/// </summary>
public class WallPaintTool
{
    private readonly MapEditorView View;
    private readonly ProjectEntry Project;

    private bool Active = false;
    private WallPaintMode Mode = WallPaintMode.Wall;

    // The triangles painted in the current stroke, which become one action when it ends
    private readonly Dictionary<(MsbEntity, uint), WallTriangleChange> Stroke = new();
    private readonly List<WallTriangleChange> StrokeOrder = new();

    private WallHit Hover;
    private WallMesh HoverMesh;

    public WallPaintTool(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    /// <summary>
    /// True while the tool takes the clicks of the viewport, so they don't select anything.
    /// </summary>
    public bool IsPainting => Active && View.WallBank.Visible;

    public void OnToolWindow()
    {
        ImGui.BeginChild("wallPaintSection", ImGuiChildFlags.Borders);

        if (Project.Descriptor.ProjectType is not ProjectType.SDT)
        {
            GUI.WrappedText("This tool is only supported for Sekiro.");
            ImGui.EndChild();
            return;
        }

        GUI.WrappedText("Paint the walls of collisions in the viewport. Each triangle of the low collision under the cursor is marked as a whole, replacing any finer cells it had.");
        GUI.WrappedText("");

        var scene = View.ViewportHandler.ActiveViewport.RenderScene;
        var wallsShown = scene.DrawFilter.HasFlag(RenderFilter.Wall);

        if (!wallsShown)
        {
            GUI.WrappedText("Walls are hidden. They must be displayed to be painted.");

            if (ImGui.Button("Display Walls##wallPaintShow"))
            {
                scene.SetDrawFilter(RenderFilter.Wall, true);
                CFG.Current.MapEditor_BasicFilter_Wall = true;
            }

            GUI.WrappedText("");
        }

        if (!CFG.Current.MapEditor_ModelLoad_Collisions || CFG.Current.MapEditor_SkipHavokLoad)
        {
            GUI.WrappedText("Collision isn't loaded, so there is nothing to paint. Enable collision loading and load the map again.");
            GUI.WrappedText("");
        }

        if (ImGui.Checkbox("Paint##wallPaintActive", ref Active))
        {
            if (!Active)
            {
                EndStroke();
                SetHover(null);
            }
        }
        GUI.Tooltip("While ticked, clicking or dragging in the viewport paints triangles instead of selecting.");

        if (ImGui.RadioButton("Wall##wallPaintModeWall", Mode is WallPaintMode.Wall))
            Mode = WallPaintMode.Wall;
        GUI.Tooltip("Mark triangles as walls of the collision they belong to.");

        ImGui.SameLine();
        if (ImGui.RadioButton("Not Wall##wallPaintModeNone", Mode is WallPaintMode.NotWall))
            Mode = WallPaintMode.NotWall;
        GUI.Tooltip("Mark triangles as not being walls.");

        ImGui.SameLine();
        if (ImGui.RadioButton("Remove##wallPaintModeRemove", Mode is WallPaintMode.Remove))
            Mode = WallPaintMode.Remove;
        GUI.Tooltip("Remove triangles from the WALL file. The game only lists steep triangles in these files.");

        GUI.WrappedText("");

        if (Hover != null)
        {
            var modelName = Hover.Entity.GetPartModelName();
            var triangle = View.WallBank.GetTriangle(Hover.Entity.MapID, modelName, Hover.ShapeKey);

            GUI.WrappedText($"Collision: {Hover.Entity.Name} ({modelName})");
            GUI.WrappedText($"Shape Key: 0x{Hover.ShapeKey:X8}");
            GUI.WrappedText($"Marked As: {DescribeTriangle(triangle)}");
        }
        else if (IsPainting)
        {
            GUI.WrappedText("Move the cursor over a collision to see how its triangles are marked.");
        }

        ImGui.EndChild();
    }

    private static string DescribeTriangle(WALL.Triangle triangle)
    {
        if (triangle == null)
            return "Not in the WALL file";

        if (triangle.Children.Count > 0)
            return "Split into cells";

        return triangle.Type is WALL.CellType.Wall ? $"Wall of collision {triangle.CollisionID}" : "Not a wall";
    }

    /// <summary>
    /// Handles the mouse while the tool is painting. Returns true if the viewport's clicks were taken,
    /// so they shouldn't select anything.
    /// </summary>
    public bool HandleViewportInput(Ray ray, bool canInteract)
    {
        if (!IsPainting)
        {
            EndStroke();
            SetHover(null);
            return false;
        }

        var hit = canInteract ? View.WallBank.RayCast(ray) : null;

        SetHover(hit);

        if (canInteract && hit != null && InputManager.IsMouseDown(MousebindID.Viewport_Picking_Action))
        {
            Paint(hit);
        }

        if (!InputManager.IsMouseDown(MousebindID.Viewport_Picking_Action))
        {
            EndStroke();
        }

        return true;
    }

    private void Paint(WallHit hit)
    {
        var entity = hit.Entity;

        // Each triangle is only painted once a stroke, so dragging back over it doesn't record it again
        if (Stroke.ContainsKey((entity, hit.ShapeKey)))
            return;

        var mapId = entity.MapID;
        var modelName = entity.GetPartModelName();

        if (string.IsNullOrEmpty(modelName) || !View.WallBank.IsMapLoaded(mapId))
            return;

        var before = View.WallBank.GetTriangle(mapId, modelName, hit.ShapeKey);
        var after = Mode switch
        {
            WallPaintMode.Wall => WallBank.CreateTriangle(modelName, hit.ShapeKey, WALL.CellType.Wall),
            WallPaintMode.NotWall => WallBank.CreateTriangle(modelName, hit.ShapeKey, WALL.CellType.None),
            _ => null
        };

        if (before == null && after == null)
            return;

        var change = new WallTriangleChange(entity, mapId, modelName, hit.ShapeKey, before, after);

        Stroke[(entity, hit.ShapeKey)] = change;
        StrokeOrder.Add(change);

        // Shown straight away, then recorded as one action when the stroke ends
        View.WallBank.SetTriangle(mapId, modelName, hit.ShapeKey, after);
        View.WallBank.Rebuild(entity);
    }

    private void EndStroke()
    {
        if (StrokeOrder.Count == 0)
            return;

        var action = new WallPaintAction(View.WallBank, new List<WallTriangleChange>(StrokeOrder));

        Stroke.Clear();
        StrokeOrder.Clear();

        // Already applied, but executing it again marks the map as modified and leaves it on the undo stack
        View.ViewportActionManager.ExecuteAction(action);
    }

    /// <summary>
    /// Outlines the triangle under the cursor.
    /// </summary>
    private void SetHover(WallHit hit)
    {
        var changed = hit?.Entity != Hover?.Entity || hit?.ShapeKey != Hover?.ShapeKey;

        Hover = hit;

        if (HoverMesh != null && (changed || hit == null))
        {
            HoverMesh.Mesh.Dispose();
            HoverMesh.Primitive.Release();
            HoverMesh = null;
        }

        if (hit == null)
            return;

        if (HoverMesh == null)
        {
            if (!View.WallBank.TryGetTriangle(hit.Entity, hit.ShapeKey, out var a, out var b, out var c))
                return;

            // Lifted further than the cells, so it shows above them
            var normal = Vector3.Cross(b - a, c - a);
            var offset = normal.LengthSquared() > 1e-12f ? Vector3.Normalize(normal) * 0.05f : Vector3.Zero;

            var primitive = new DbgPrimWallCells(new[] { (a + offset, b + offset, c + offset) });
            var proxy = MsbEntity.CreateWallProxy(View.ViewportHandler.ActiveViewport.RenderScene, primitive,
                CFG.Current.Viewport_Wall_Hover_Color);

            HoverMesh = new WallMesh(proxy, primitive);
        }

        HoverMesh.Mesh.World = hit.Entity.GetWorldMatrix();
    }
}
