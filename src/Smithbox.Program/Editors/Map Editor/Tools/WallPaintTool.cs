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
    Remove,
    NoJump,
    AllowJump
}

/// <summary>
/// Paints the WALL and NOJUMPWALL files of Sekiro collisions in the viewport. In the WALL file, each triangle of a
/// low collision under the cursor is marked entirely as a wall or not a wall, or removed from the file, replacing
/// any finer cells it had. In the NOJUMPWALL file, it is added or removed.
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

        GUI.Spacer();
        GUI.SimpleHeader("Actions", "");

        GUI.MultiButtonInput("wallPaintToggle",
            "wallPaintToggleButton", Active ? "Stop Paint" : "Start Paint",
            "While painting, clicking or dragging in the viewport paints triangles instead of selecting.",
            TogglePainting);

        GUI.Spacer();
        GUI.SimpleHeader("wallPaintOptions", "Paint Options", "How painting in the viewport changes the triangles under the cursor.", UI.Current.ImGui_Default_Text_Color);

        if (!CFG.Current.MapEditor_ModelLoad_Collisions || CFG.Current.MapEditor_SkipHavokLoad)
        {
            GUI.WrappedText("Collision isn't loaded, so there is nothing to paint. Enable collision loading and load the map again.");
            GUI.WrappedText("");
        }

        if (ImGui.RadioButton("Wall##wallPaintModeWall", Mode is WallPaintMode.Wall))
            Mode = WallPaintMode.Wall;
        GUI.Tooltip("Mark triangles as walls of the collision they belong to.");

        ImGui.SameLine();
        if (ImGui.RadioButton("Not Wall##wallPaintModeNone", Mode is WallPaintMode.NotWall))
            Mode = WallPaintMode.NotWall;
        GUI.Tooltip("Mark triangles as not being walls. The game never marks a whole triangle like this, only parts of split ones; to stop a whole triangle being a wall, Remove it instead.");

        ImGui.SameLine();
        if (ImGui.RadioButton("Remove##wallPaintModeRemove", Mode is WallPaintMode.Remove))
            Mode = WallPaintMode.Remove;
        GUI.Tooltip("Remove triangles from the WALL file. The game only lists steep triangles in these files.");

        if (ImGui.RadioButton("No Jump##wallPaintModeNoJump", Mode is WallPaintMode.NoJump))
            Mode = WallPaintMode.NoJump;
        GUI.Tooltip("Add triangles to the NOJUMPWALL file, so they can't be jumped off. This is separate from the WALL file, so it doesn't change how they are marked there.");

        ImGui.SameLine();
        if (ImGui.RadioButton("Allow Jump##wallPaintModeAllowJump", Mode is WallPaintMode.AllowJump))
            Mode = WallPaintMode.AllowJump;
        GUI.Tooltip("Remove triangles from the NOJUMPWALL file. A collision left without any is saved without the file, as the game does.");

        DisplayVisibility();

        GUI.Spacer();
        GUI.SimpleHeader("Legend", "");

        if (Hover != null)
        {
            var mapId = Hover.Entity.MapID;
            var modelName = Hover.Entity.GetPartModelName();
            var triangle = View.WallBank.GetTriangle(mapId, modelName, Hover.ShapeKey);
            var noJump = View.WallBank.IsNoJump(mapId, modelName, Hover.ShapeKey);

            GUI.WrappedText($"Collision: {Hover.Entity.Name} ({modelName})");
            GUI.WrappedText($"Shape Key: 0x{Hover.ShapeKey:X8}");
            GUI.WrappedText($"Marked As: {DescribeTriangle(triangle)}");
            GUI.WrappedText($"No Jump: {(noJump ? "Yes" : "No")}");
        }
        else if (IsPainting)
        {
            GUI.WrappedText("Move the cursor over a collision to see how its triangles are marked.");
        }

        ImGui.EndChild();
    }

    private void TogglePainting()
    {
        Active = !Active;

        if (!Active)
        {
            EndStroke();
            SetHover(null);
        }
    }

    /// <summary>
    /// Toggles for what is displayed, and the colors it is displayed in.
    /// </summary>
    private void DisplayVisibility()
    {
        GUI.Spacer();
        GUI.SimpleHeader("wallPaintVisibility", "Visibility", "What is displayed on the collisions while walls are displayed.", UI.Current.ImGui_Default_Text_Color);

        var scene = View.ViewportHandler.ActiveViewport.RenderScene;

        if (!scene.DrawFilter.HasFlag(RenderFilter.Wall))
        {
            GUI.WrappedText("Walls are hidden. They must be displayed to be painted.");

            if (ImGui.Button("Display Walls##wallPaintShow"))
            {
                scene.SetDrawFilter(RenderFilter.Wall, true);
                CFG.Current.MapEditor_BasicFilter_Wall = true;
            }

            GUI.WrappedText("");
        }

        if (ImGui.Checkbox("WALL##wallDisplayCells", ref CFG.Current.MapEditor_Wall_Display_Cells))
        {
            View.WallBank.RebuildAll();
        }
        GUI.Tooltip("Display the cells of the WALL files: which parts of each triangle are walls and which are not.");

        ImGui.SameLine();
        if (ImGui.Checkbox("NOJUMPWALL##wallDisplayNoJump", ref CFG.Current.MapEditor_Wall_Display_NoJump))
        {
            View.WallBank.RebuildAll();
        }
        GUI.Tooltip("Display the triangles of the NOJUMPWALL files, which can't be jumped off.");

        GUI.WrappedText("");

        DisplayLegend();
    }

    /// <summary>
    /// Explains the colors the walls are displayed in.
    /// </summary>
    private static void DisplayLegend()
    {
        GUI.WrappedText("Colors:");

        LegendEntry(CFG.Current.Viewport_Wall_Fill_Color, "Wall",
            "A wall of this collision.");
        LegendEntry(CFG.Current.Viewport_Wall_Other_Collision_Color, "Wall (other collision)",
            "A wall that belongs to a neighbouring collision, usually along the edge where the two meet.");
        LegendEntry(CFG.Current.Viewport_Wall_None_Color, "Not Wall",
            "Part of a triangle that is not a wall.");
        LegendEntry(CFG.Current.Viewport_Wall_NoJump_Color, "No Jump",
            "A triangle in the NOJUMPWALL file, which can't be jumped off. Drawn over its wall cells, so they are tinted by it.");
        LegendEntry(CFG.Current.Viewport_Wall_Hover_Color, "Cursor",
            "The triangle that will be painted.");

        GUI.WrappedText("Collision without any of these colors isn't in the WALL file, so it isn't a wall. The game only lists steep triangles.");
        GUI.WrappedText("");
        GUI.WrappedText("The low collision is coarse, so the game splits some of its triangles into smaller cells, and a single triangle can be partly a wall and partly not. Painting a triangle replaces its cells with one mark.");
        GUI.WrappedText("");
    }

    private static void LegendEntry(Vector3 color, string name, string description)
    {
        var size = ImGui.GetTextLineHeight();

        ImGui.ColorButton($"##wallLegend{name}", new Vector4(color / 255.0f, 1.0f),
            ImGuiColorEditFlags.NoTooltip | ImGuiColorEditFlags.NoPicker, new Vector2(size, size));
        ImGui.SameLine();
        ImGui.Text(name);
        GUI.Tooltip(description);
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

        WallTriangleChange change;

        if (Mode is WallPaintMode.NoJump or WallPaintMode.AllowJump)
        {
            var noJumpBefore = View.WallBank.IsNoJump(mapId, modelName, hit.ShapeKey);
            var noJumpAfter = Mode is WallPaintMode.NoJump;

            if (noJumpBefore == noJumpAfter)
                return;

            change = new WallTriangleChange(entity, mapId, modelName, hit.ShapeKey, WallLayer.NoJump,
                NoJumpBefore: noJumpBefore, NoJumpAfter: noJumpAfter);
        }
        else
        {
            var before = View.WallBank.GetTriangle(mapId, modelName, hit.ShapeKey);
            var after = Mode switch
            {
                WallPaintMode.Wall => WallBank.CreateTriangle(modelName, hit.ShapeKey, WALL.CellType.Wall),
                WallPaintMode.NotWall => WallBank.CreateTriangle(modelName, hit.ShapeKey, WALL.CellType.None),
                _ => null
            };

            if (before == null && after == null)
                return;

            change = new WallTriangleChange(entity, mapId, modelName, hit.ShapeKey, WallLayer.Wall, before, after);
        }

        Stroke[(entity, hit.ShapeKey)] = change;
        StrokeOrder.Add(change);

        // Shown straight away, then recorded as one action when the stroke ends
        WallPaintAction.Apply(View.WallBank, change, true);
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
            var offset = WallBank.GetFrontNormal(a, b, c) * 0.05f;

            var primitive = new DbgPrimWallCells(new[] { (a + offset, b + offset, c + offset) });
            var proxy = MsbEntity.CreateWallProxy(View.ViewportHandler.ActiveViewport.RenderScene, primitive,
                CFG.Current.Viewport_Wall_Hover_Color);

            HoverMesh = new WallMesh(proxy, primitive);
        }

        HoverMesh.Mesh.World = hit.Entity.GetWorldMatrix();
    }
}
