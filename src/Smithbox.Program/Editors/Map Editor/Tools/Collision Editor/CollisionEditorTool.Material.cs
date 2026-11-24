using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.HavokEditor;
using StudioCore.Keybinds;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Veldrid.Utilities;

namespace StudioCore.Editors.MapEditor;

/// <summary>What a click of the Material tab's brush does.</summary>
public enum CollisionMaterialMode
{
    [Display(Name = "MAP_CollisionEditor_Material_Mode_Paint")]
    Paint,
    [Display(Name = "MAP_CollisionEditor_Material_Mode_Pick")]
    Pick,
    [Display(Name = "MAP_CollisionEditor_Material_Mode_Select")]
    Select
}

/// <summary>
/// The Material tab: the collision material (HitMtrlParam row plus property) of each face of the collision, shown as
/// an overlay in a colour per material, and painted with a brush. Holding the picking button gives the chosen
/// material to the face under the cursor, and with a radius, to the faces of the layer around it (optionally only
/// those facing like the hit face); Pick takes the material of the face clicked. Repainted faces are pending edits,
/// as moved vertices are: Commit (in the header) rebuilds the files with their new materials.
/// </summary>
public partial class CollisionEditorTool
{
    private bool MaterialActive = false;

    // The frame the Material tab was last drawn: painting and the overlay are only on while it is
    private int LastMaterialTabFrame = -10;

    private CollisionMaterialMode MaterialMode = CollisionMaterialMode.Paint;

    private Hit MaterialHover;
    private bool MaterialStroking;

    // Where the stroke last painted, so a held button only paints again once the cursor moves to another face
    private (CollisionEditorBody Body, int Triangle, Vector3 Point) LastPaint;

    private CollisionMaterialPicker MaterialNames;

    private readonly CollisionMaterialOverlay MaterialOverlay = new();

    // Materials the legend has hidden from the overlay
    private readonly HashSet<uint> HiddenMaterials = new();

    // The materials of the faces in the overlay, and how many faces have each, for the legend
    private List<(uint Material, int Faces)> MaterialLegend = new();

    // The face under the cursor, drawn in the colour it would be painted
    private (DebugPrimitiveRenderableProxy Proxy, DbgPrimWallCells Primitive) HoverFace;
    private (CollisionEditorBody Body, int Triangle, uint Material, CollisionMaterialMode Mode) HoverFaceKey;

    // How far the hovered face is lifted towards its front: further than the overlay, so it shows above it
    private const float HoverFaceOffset = 0.05f;

    // Time spent decoding files for the overlay each frame, after the first, so showing a large map doesn't stall
    private const double OverlayDecodeBudgetMs = 8;

    private static readonly (KeybindID Key, CollisionMaterialMode Mode)[] MaterialKeys =
    {
        (KeybindID.MapEditor_CollisionEditor_Paint_Material, CollisionMaterialMode.Paint),
        (KeybindID.MapEditor_CollisionEditor_Pick_Material, CollisionMaterialMode.Pick),
        (KeybindID.MapEditor_CollisionEditor_Select_Material, CollisionMaterialMode.Select),
    };

    // The face Select mode clicked, whose material the Selected Face section shows and edits
    private Hit SelectedFace;

    // The ID typed into the Selected Face section, applied when the field is left; taken from the face while it isn't
    // being typed in, so it follows paints and discards
    private int SelectedFaceInput;

    // The selected face, drawn in the selection colour over the overlay
    private (DebugPrimitiveRenderableProxy Proxy, DbgPrimWallCells Primitive) SelectedFaceMesh;
    private (CollisionEditorBody Body, int Triangle) SelectedFaceMeshKey;

    // Further than the hovered face, so the selection shows when the cursor is over it
    private const float SelectedFaceOffset = 0.07f;

    private bool IsMaterialOverlayShown => MaterialOverlay.IsShown;

    private CollisionMaterialPicker GetMaterialNames() => MaterialNames ??= new CollisionMaterialPicker(Project);

    private static uint GetPaintMaterial() => (uint)Math.Max(0, CFG.Current.MapEditor.MapEditor_CollisionEditor_Paint_Material);

    #region Tab

    private void DisplayMaterialTab()
    {
        LastMaterialTabFrame = Frame;

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CollisionEditor_Actions_Header"), LOC.Get("MAP_CollisionEditor_Actions_Header_TT"));

        GUI.MultiButtonInput("collisionEditorMaterialToggle",
            "collisionEditorMaterialToggleButton",
            MaterialActive ? LOC.Get("MAP_CollisionEditor_Material_Stop") : LOC.Get("MAP_CollisionEditor_Material_Start"),
            MaterialActive ? LOC.Get("MAP_CollisionEditor_Material_Stop_TT") : LOC.Get("MAP_CollisionEditor_Material_Start_TT"),
            ToggleMaterialPainting);

        DisplayLayer();
        DisplayMaterialCursor();

        // Controls
        GUI.Spacer();
        GUI.ConditionalHeader(LOC.Get("MAP_CollisionEditor_Controls_Header"), LOC.Get("MAP_CollisionEditor_Controls_Header_TT"),
            ref CFG.Current.MapEditor.MapEditor_CollisionEditor_Show_Controls);

        if (CFG.Current.MapEditor.MapEditor_CollisionEditor_Show_Controls)
        {
            GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Material_Control_Brush", InputManager.GetMouseHint(MousebindID.Viewport_Picking_Action)));
            GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Material_Control_Modes",
                InputManager.GetHint(KeybindID.MapEditor_CollisionEditor_Paint_Material),
                InputManager.GetHint(KeybindID.MapEditor_CollisionEditor_Pick_Material),
                InputManager.GetHint(KeybindID.MapEditor_CollisionEditor_Select_Material)));
        }

        DisplaySelectedFace();
        DisplayPaintMaterial();
        DisplayPaintBrush();
        DisplayMaterialOverlayOptions();
        DisplayPendingList();
    }

    /// <summary>The face under the cursor and its material.</summary>
    private void DisplayMaterialCursor()
    {
        var scene = View.ViewportHandler.ActiveViewport?.RenderScene;

        // Cursor
        GUI.Spacer();
        GUI.ConditionalHeader(LOC.Get("MAP_CollisionEditor_Cursor_Header"), LOC.Get("MAP_CollisionEditor_Material_Cursor_Header_TT"),
            ref CFG.Current.MapEditor.MapEditor_CollisionEditor_Show_Cursor);

        if (!CFG.Current.MapEditor.MapEditor_CollisionEditor_Show_Cursor)
            return;

        if (scene != null && !scene.DrawFilter.HasFlag(RenderFilter.Collision))
        {
            GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Collision_Hidden"));
        }
        else if (!MaterialActive)
        {
            GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Material_Cursor_Inactive"));
        }
        else if (MaterialHover is { } hit)
        {
            var names = GetMaterialNames();
            var material = hit.Body.Materials[hit.Triangle];
            var original = hit.Body.OriginalMaterials[hit.Triangle];

            GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Cursor_File", hit.File.FileName, hit.File.Entity.Name));
            GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Material_Cursor_Face", hit.Triangle, hit.File.Bodies.IndexOf(hit.Body)));

            DisplaySwatch(material, "Cursor");
            ImGui.SameLine();
            GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Material_Cursor_Material", names.Describe(material)));

            if (material != original)
                GUI.WrappedTextColored(UI.Current.ImGui_Invalid_Text_Color,
                    LOC.Get("MAP_CollisionEditor_Material_Cursor_Repainted", names.Describe(original)));
        }
        else
        {
            GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Cursor_None"));
        }
    }

    /// <summary>
    /// The face Select mode clicked: where it is, its material, and its hit material ID, which can be typed in freely.
    /// The ID is applied when the field is left (Enter, Tab or a click elsewhere; Escape reverts it), as a pending
    /// edit like a paint.
    /// </summary>
    private void DisplaySelectedFace()
    {
        ValidateSelectedFace();

        if (MaterialMode is not CollisionMaterialMode.Select && SelectedFace == null)
            return;

        // Selected Face
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CollisionEditor_Material_Selected_Header"), LOC.Get("MAP_CollisionEditor_Material_Selected_Header_TT"));

        if (SelectedFace is not { } face)
        {
            GUI.WrappedText(LOC.Get(MaterialActive ? "MAP_CollisionEditor_Material_Selected_None"
                : "MAP_CollisionEditor_Material_Selected_Inactive"));
            return;
        }

        var names = GetMaterialNames();
        var material = face.Body.Materials[face.Triangle];
        var original = face.Body.OriginalMaterials[face.Triangle];

        GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Cursor_File", face.File.FileName, face.File.Entity.Name));
        GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Material_Cursor_Face", face.Triangle, face.File.Bodies.IndexOf(face.Body)));

        DisplaySwatch(material, "Selected");
        ImGui.SameLine();
        GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Material_Cursor_Material", names.Describe(material)));

        if (material != original)
            GUI.WrappedTextColored(UI.Current.ImGui_Invalid_Text_Color,
                LOC.Get("MAP_CollisionEditor_Material_Cursor_Repainted", names.Describe(original)));

        // Hit Material ID
        GUI.Spacer();
        GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Material_Selected_ID"));

        ImGui.SetNextItemWidth(-1);
        ImGui.InputInt("##collisionEditorSelectedFaceMaterial", ref SelectedFaceInput, 0, 0);

        if (GUI.InputCommitted())
        {
            SelectedFaceInput = Math.Max(0, SelectedFaceInput);
            face.Body.SetMaterial(face.Triangle, (uint)SelectedFaceInput);
        }
        else if (!ImGui.IsItemActive())
        {
            // Follows the face while it isn't being typed in
            SelectedFaceInput = (int)material;
        }
        GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Material_Selected_ID_TT"));

        // Revert, Use for Paint, Clear
        GUI.Spacer();

        if (material != original)
        {
            if (ImGui.Button($"{LOC.Get("MAP_CollisionEditor_Material_Selected_Revert")}##collisionEditorSelectedFaceRevert"))
                face.Body.SetMaterial(face.Triangle, original);
            GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Material_Selected_Revert_TT"));
            ImGui.SameLine();
        }

        if (ImGui.Button($"{LOC.Get("MAP_CollisionEditor_Material_Selected_Use")}##collisionEditorSelectedFaceUse"))
            CFG.Current.MapEditor.MapEditor_CollisionEditor_Paint_Material = (int)material;
        GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Material_Selected_Use_TT"));
        ImGui.SameLine();

        if (ImGui.Button($"{LOC.Get("MAP_CollisionEditor_Material_Selected_Clear")}##collisionEditorSelectedFaceClear"))
            SelectedFace = null;
        GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Material_Selected_Clear_TT"));
    }

    /// <summary>
    /// Drops the selection once its file is no longer the one decoded (a commit, its undo, a reload, or the file let
    /// go of), as its face numbers no longer match.
    /// </summary>
    private void ValidateSelectedFace()
    {
        if (SelectedFace is { } face
            && (!Files.TryGetValue(face.File.FileName, out var file) || file != face.File || face.Triangle >= face.Body.TriangleCount))
        {
            SelectedFace = null;
        }
    }

    /// <summary>The material painted, by surface and property.</summary>
    private void DisplayPaintMaterial()
    {
        GUI.Spacer();
        GUI.ConditionalHeader(LOC.Get("MAP_CollisionEditor_Material_Paint_Header"), LOC.Get("MAP_CollisionEditor_Material_Paint_Header_TT"),
            ref CFG.Current.MapEditor.MapEditor_CollisionEditor_Show_Paint_Material);

        if (!CFG.Current.MapEditor.MapEditor_CollisionEditor_Show_Paint_Material)
            return;

        DisplaySwatch(GetPaintMaterial(), "Paint");
        ImGui.SameLine();
        GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Material_Paint_Colour"));

        GetMaterialNames().Display(ref CFG.Current.MapEditor.MapEditor_CollisionEditor_Paint_Material, null);
    }

    /// <summary>The brush mode, its radius and the facing limit.</summary>
    private void DisplayPaintBrush()
    {
        // Brush
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CollisionEditor_Brush_Header"), LOC.Get("MAP_CollisionEditor_Material_Brush_Header_TT"));

        ImGui.SetNextItemWidth(-1);
        if (ImGui.BeginCombo("##collisionEditorMaterialMode", LOC.Get(MaterialMode.GetDisplayName())))
        {
            foreach (var (key, value) in MaterialKeys)
            {
                var label = $"{LOC.Get(value.GetDisplayName())} ({InputManager.GetHint(key)})";

                if (ImGui.Selectable($"{label}##collisionEditorMaterialMode{value}", value == MaterialMode))
                    MaterialMode = value;
                GUI.Tooltip(LOC.Get($"MAP_CollisionEditor_Material_Mode_{value}_TT"));
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Material_Mode_Input_TT"));

        // Radius
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CollisionEditor_Radius_Header"), LOC.Get("MAP_CollisionEditor_Material_Radius_Header_TT"));

        ImGui.SetNextItemWidth(-1);
        if (ImGui.InputFloat("##collisionEditorPaintRadius", ref CFG.Current.MapEditor.MapEditor_CollisionEditor_Paint_Radius))
            CFG.Current.MapEditor.MapEditor_CollisionEditor_Paint_Radius = Math.Max(0f, CFG.Current.MapEditor.MapEditor_CollisionEditor_Paint_Radius);
        GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Material_Radius_Input_TT"));

        if (CFG.Current.MapEditor.MapEditor_CollisionEditor_Paint_Radius > 0)
        {
            // Facing Limit
            GUI.Spacer();
            GUI.SimpleHeader(LOC.Get("MAP_CollisionEditor_Material_Facing_Header"), LOC.Get("MAP_CollisionEditor_Material_Facing_Header_TT"));

            ImGui.SetNextItemWidth(-1);
            if (ImGui.SliderFloat("##collisionEditorPaintFacing", ref CFG.Current.MapEditor.MapEditor_CollisionEditor_Paint_Facing_Limit, 0f, 180f, "%.0f°"))
                CFG.Current.MapEditor.MapEditor_CollisionEditor_Paint_Facing_Limit = Math.Clamp(CFG.Current.MapEditor.MapEditor_CollisionEditor_Paint_Facing_Limit, 0f, 180f);
            GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Material_Facing_Input_TT"));
        }
    }

    /// <summary>Whether the overlay is shown, its opacity, and the legend of its materials.</summary>
    private void DisplayMaterialOverlayOptions()
    {
        // Overlay
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CollisionEditor_Material_Overlay_Header"), LOC.Get("MAP_CollisionEditor_Material_Overlay_Header_TT"));

        ImGui.Checkbox($"{LOC.Get("MAP_CollisionEditor_Material_Overlay_Toggle")}##collisionEditorMaterialOverlay",
            ref CFG.Current.MapEditor.MapEditor_CollisionEditor_Material_Overlay);
        GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Material_Overlay_Toggle_TT"));

        if (!CFG.Current.MapEditor.MapEditor_CollisionEditor_Material_Overlay)
            return;

        ImGui.SetNextItemWidth(-1);
        ImGui.SliderFloat("##collisionEditorMaterialOverlayAlpha", ref CFG.Current.MapEditor.MapEditor_CollisionEditor_Material_Overlay_Alpha, 5f, 100f, "%.0f%%");
        GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Material_Overlay_Alpha_TT"));

        // Legend
        GUI.Spacer();
        GUI.ConditionalHeader(LOC.Get("MAP_CollisionEditor_Material_Legend_Header"), LOC.Get("MAP_CollisionEditor_Material_Legend_Header_TT"),
            ref CFG.Current.MapEditor.MapEditor_CollisionEditor_Show_Material_Legend);

        if (!CFG.Current.MapEditor.MapEditor_CollisionEditor_Show_Material_Legend)
            return;

        if (!View.HavokCollisionBank.IsCollisionTypeVisible(CFG.Current.MapEditor.MapEditor_CollisionEditor_Layer))
        {
            GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Material_Legend_Layer_Hidden"));
            return;
        }

        if (MaterialLegend.Count == 0)
        {
            GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Material_Legend_None"));
            return;
        }

        if (HiddenMaterials.Count > 0)
        {
            if (ImGui.Button($"{LOC.Get("MAP_CollisionEditor_Material_Legend_Show_All")}##collisionEditorMaterialShowAll"))
                HiddenMaterials.Clear();
            GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Material_Legend_Show_All_TT"));
        }

        var names = GetMaterialNames();
        var paint = GetPaintMaterial();

        foreach (var (material, faces) in MaterialLegend)
        {
            var shown = !HiddenMaterials.Contains(material);

            if (ImGui.Checkbox($"##collisionEditorMaterialLegendShown{material}", ref shown))
            {
                if (shown)
                    HiddenMaterials.Remove(material);
                else
                    HiddenMaterials.Add(material);
            }
            GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Material_Legend_Shown_TT"));

            ImGui.SameLine();
            if (DisplaySwatch(material, "Legend"))
                CFG.Current.MapEditor.MapEditor_CollisionEditor_Paint_Material = (int)material;

            ImGui.SameLine();
            if (ImGui.Selectable($"{LOC.Get("MAP_CollisionEditor_Material_Legend_Entry", names.Describe(material), faces)}##collisionEditorMaterialLegend{material}",
                material == paint))
            {
                CFG.Current.MapEditor.MapEditor_CollisionEditor_Paint_Material = (int)material;
            }
            GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Material_Legend_Entry_TT"));
        }
    }

    /// <summary>A small square in a material's overlay colour. Returns true when clicked.</summary>
    private static bool DisplaySwatch(uint material, string context)
    {
        var color = CollisionMaterialOverlay.GetColor(material);
        var size = ImGui.GetFrameHeight() * 0.8f;

        return ImGui.ColorButton($"##collisionEditorMaterialSwatch{context}{material}", new Vector4(color, 1f),
            ImGuiColorEditFlags.NoTooltip | ImGuiColorEditFlags.NoDragDrop, new Vector2(size, size));
    }

    #endregion

    #region Painting

    private void ToggleMaterialPainting()
    {
        MaterialActive = !MaterialActive;

        if (MaterialActive)
        {
            // The Navigation tool's vertex edit and the Edit tab's brush take the same button
            View.NavigationTool?.StopVertexEdit();

            if (Active)
                ToggleEditing();

            return;
        }

        MaterialStroking = false;
        MaterialHover = null;
        DisposeCursor();
        DisposeHoverFace();

        // The overlay draws from the decoded files, so they are kept while it is shown
        if (!IsMaterialOverlayShown && !Active)
            ReleaseUneditedFiles();
    }

    private void HandleMaterialPainting(RenderScene scene, Ray ray, bool canInteract)
    {
        // Typed text shouldn't change the mode, and the right and middle buttons fly the camera
        var typing = ImGui.GetIO().WantTextInput;
        var flying = InputManager.IsMouseDown(Veldrid.MouseButton.Right) || InputManager.IsMouseDown(Veldrid.MouseButton.Middle);

        var keysFree = canInteract && !typing && !flying && !InputManager.IsMouseDown(Veldrid.MouseButton.Left);
        var brushFree = canInteract && !typing && !flying;

        var collisionShown = scene.DrawFilter.HasFlag(RenderFilter.Collision);

        if (keysFree && !MaterialStroking)
        {
            foreach (var (key, mode) in MaterialKeys)
            {
                if (InputManager.IsPressed(key))
                    MaterialMode = mode;
            }
        }

        if (MaterialStroking && (!brushFree || !collisionShown || !InputManager.IsMouseDown(MousebindID.Viewport_Picking_Action)))
            MaterialStroking = false;

        MaterialHover = canInteract && collisionShown ? RayCast(ray) : null;

        if (MaterialStroking)
        {
            if (MaterialHover is { } hit && HasMovedFromLastPaint(hit))
                PaintAt(hit);
        }
        else if (brushFree && MaterialHover != null && InputManager.IsMousePressed(MousebindID.Viewport_Picking_Action))
        {
            if (MaterialMode is CollisionMaterialMode.Select)
            {
                // The face is shown and edited in the Selected Face section; its ID field follows it
                SelectedFace = MaterialHover;
                SelectedFaceInput = (int)MaterialHover.Body.Materials[MaterialHover.Triangle];
            }
            else if (MaterialMode is CollisionMaterialMode.Pick)
            {
                // A pick is one click, then the brush paints with what was picked
                CFG.Current.MapEditor.MapEditor_CollisionEditor_Paint_Material = (int)MaterialHover.Body.Materials[MaterialHover.Triangle];
                MaterialMode = CollisionMaterialMode.Paint;
            }
            else
            {
                MaterialStroking = true;
                PaintAt(MaterialHover);
            }
        }

        UpdateMaterialCursor(scene);
    }

    /// <summary>
    /// Whether the cursor has moved on since the stroke last painted: onto another face, or (with a radius) by a
    /// quarter of the radius, so a held button over the same spot doesn't search again every frame.
    /// </summary>
    private bool HasMovedFromLastPaint(Hit hit)
    {
        if (hit.Body != LastPaint.Body || hit.Triangle != LastPaint.Triangle)
            return true;

        var radius = CFG.Current.MapEditor.MapEditor_CollisionEditor_Paint_Radius;

        return radius > 0 && Vector3.Distance(hit.Point, LastPaint.Point) > radius * 0.25f;
    }

    /// <summary>
    /// Gives the paint material to the hit face, and with a radius, to the faces of the layer within it that face
    /// within the facing limit of the hit face.
    /// </summary>
    private void PaintAt(Hit hit)
    {
        LastPaint = (hit.Body, hit.Triangle, hit.Point);

        var material = GetPaintMaterial();
        var radius = CFG.Current.MapEditor.MapEditor_CollisionEditor_Paint_Radius;

        hit.Body.SetMaterial(hit.Triangle, material);

        if (radius <= 0)
            return;

        var limit = Math.Clamp(CFG.Current.MapEditor.MapEditor_CollisionEditor_Paint_Facing_Limit, 0f, 180f);
        var checkFacing = limit < 180f;
        var minDot = MathF.Cos(limit * MathF.PI / 180f);
        var reference = GetWorldNormal(hit.File, hit.Body, hit.Triangle);

        foreach (var file in GetBrushFiles(hit, radius))
        {
            foreach (var body in file.Bodies)
            {
                var world = GetBodyWorld(file, body);

                if (!Matrix4x4.Invert(world, out var inverse))
                    continue;

                var scale = GetScale(world);
                var center = Vector3.Transform(hit.Point, inverse);

                foreach (var t in body.GetTrianglesNear(center, radius / scale).ToList())
                {
                    if (checkFacing)
                    {
                        var normal = GetWorldNormal(file, body, t);

                        if (normal == Vector3.Zero || Vector3.Dot(normal, reference) < minDot)
                            continue;
                    }

                    body.SetMaterial(t, material);
                }
            }
        }
    }

    /// <summary>The normal of a triangle in the world, or zero if it has no area.</summary>
    private static Vector3 GetWorldNormal(CollisionEditorFile file, CollisionEditorBody body, int triangle)
    {
        var (a, b, c) = body.GetTriangle(triangle);
        var world = GetBodyWorld(file, body);
        var normal = Vector3.Cross(Vector3.Transform(b, world) - Vector3.Transform(a, world),
            Vector3.Transform(c, world) - Vector3.Transform(a, world));

        return normal.LengthSquared() < 1e-12f ? Vector3.Zero : Vector3.Normalize(normal);
    }

    #endregion

    #region Display

    /// <summary>
    /// The face under the cursor, in the colour it would be painted (white for Pick), and with a radius, the brush
    /// sphere around it.
    /// </summary>
    private void UpdateMaterialCursor(RenderScene scene)
    {
        // The vertex marker is the Edit tab's
        SetVisible(VertexMesh, false);

        var hit = MaterialHover;

        if (hit == null)
        {
            SetVisible(CursorWire, false);
            SetVisible(CursorFill, false);
            DisposeHoverFace();
            return;
        }

        var radius = CFG.Current.MapEditor.MapEditor_CollisionEditor_Paint_Radius;

        if (radius > 0 && MaterialMode is CollisionMaterialMode.Paint)
        {
            var cursorColor = ToColor(CFG.Current.MapEditor.MapEditor_CollisionEditor_Cursor_Color);

            CursorWire ??= RenderableHelper.GetDetectionSphereProxy(scene, cursorColor);
            CursorFill ??= RenderableHelper.GetDetectionSphereFillProxy(scene, cursorColor);

            var fill = ColorHelper.GetTransparencyColor(cursorColor, CFG.Current.MapEditor.MapEditor_CollisionEditor_Cursor_Fill_Alpha);

            if (CursorFill.BaseColor != fill)
            {
                CursorFill.BaseColor = fill;
                CursorFill.HighlightedColor = fill;
            }

            // The detection spheres have a radius of 1
            var cursorWorld = Matrix4x4.CreateScale(radius) * Matrix4x4.CreateTranslation(hit.Point);
            CursorWire.World = cursorWorld;
            CursorFill.World = cursorWorld;

            SetVisible(CursorWire, true);
            SetVisible(CursorFill, true);
        }
        else
        {
            SetVisible(CursorWire, false);
            SetVisible(CursorFill, false);
        }

        // The hovered face, rebuilt when it, its material or the mode changes
        var key = (hit.Body, hit.Triangle, GetPaintMaterial(), MaterialMode);

        if (HoverFace.Proxy == null || !key.Equals(HoverFaceKey))
        {
            DisposeHoverFace();

            var (a, b, c) = hit.Body.GetTriangle(hit.Triangle);
            var normal = Vector3.Cross(b - a, c - a);
            var offset = normal.LengthSquared() < 1e-12f ? Vector3.Zero : Vector3.Normalize(normal) * HoverFaceOffset;

            var color = MaterialMode is CollisionMaterialMode.Paint ? CollisionMaterialOverlay.GetColor(GetPaintMaterial()) : Vector3.One;
            var fill = ColorHelper.GetTransparencyColor(color, 85f);

            var primitive = new DbgPrimWallCells(new[] { (a + offset, b + offset, c + offset) });
            var proxy = new DebugPrimitiveRenderableProxy(scene.OpaqueRenderables, primitive)
            {
                Pickable = false,
                RenderTranslucent = true,
                BaseColor = fill,
                HighlightedColor = fill,
                DrawFilter = RenderFilter.Collision,
            };

            HoverFace = (proxy, primitive);
            HoverFaceKey = key;
        }

        var world = GetBodyWorld(hit.File, hit.Body);

        if (HoverFace.Proxy.World != world)
            HoverFace.Proxy.World = world;
    }

    private void DisposeHoverFace()
    {
        HoverFace.Proxy?.Dispose();
        HoverFace.Primitive?.Release();
        HoverFace = default;
    }

    /// <summary>
    /// Draws the selected face in the selection colour while the Material tab is open, lifted above the overlay and
    /// the hovered face. Without the overlay it still shows, over the collision.
    /// </summary>
    private void UpdateSelectedFaceMesh(RenderScene scene, bool tabOpen)
    {
        ValidateSelectedFace();

        if (!tabOpen || SelectedFace is not { } face)
        {
            DisposeSelectedFaceMesh();
            return;
        }

        var key = (face.Body, face.Triangle);

        if (SelectedFaceMesh.Proxy == null || !key.Equals(SelectedFaceMeshKey))
        {
            DisposeSelectedFaceMesh();

            var (a, b, c) = face.Body.GetTriangle(face.Triangle);
            var normal = Vector3.Cross(b - a, c - a);
            var offset = normal.LengthSquared() < 1e-12f ? Vector3.Zero : Vector3.Normalize(normal) * SelectedFaceOffset;

            var fill = ColorHelper.GetTransparencyColor(ToColor(CFG.Current.MapEditor.MapEditor_CollisionEditor_Vertex_Color), 85f);
            var primitive = new DbgPrimWallCells(new[] { (a + offset, b + offset, c + offset) });
            var proxy = new DebugPrimitiveRenderableProxy(scene.OpaqueRenderables, primitive)
            {
                Pickable = false,
                RenderTranslucent = true,
                BaseColor = fill,
                HighlightedColor = fill,
                DrawFilter = RenderFilter.Collision,
            };

            SelectedFaceMesh = (proxy, primitive);
            SelectedFaceMeshKey = key;
        }

        var world = GetBodyWorld(face.File, face.Body);

        if (SelectedFaceMesh.Proxy.World != world)
            SelectedFaceMesh.Proxy.World = world;

        var visible = face.File.Entity.EditorVisible && View.HavokCollisionBank.IsCollisionTypeVisible(face.File.Type);

        if (SelectedFaceMesh.Proxy.Visible != visible)
            SelectedFaceMesh.Proxy.Visible = visible;
    }

    private void DisposeSelectedFaceMesh()
    {
        SelectedFaceMesh.Proxy?.Dispose();
        SelectedFaceMesh.Primitive?.Release();
        SelectedFaceMesh = default;
    }

    /// <summary>
    /// Shows the material of every face of the chosen layer over the displayed collision, while the Material tab is
    /// open and its overlay is on. The files of the layer are decoded for it, a few each frame.
    /// </summary>
    private void UpdateMaterialOverlay(RenderScene scene)
    {
        UpdateSelectedFaceMesh(scene, Frame - LastMaterialTabFrame <= 1);

        var shown = CFG.Current.MapEditor.MapEditor_CollisionEditor_Material_Overlay && Frame - LastMaterialTabFrame <= 1;

        if (!shown)
        {
            if (MaterialOverlay.IsShown)
            {
                MaterialOverlay.Clear();
                MaterialLegend = new();

                // Decoded for the overlay: dropped now, unless something else still uses them
                if (!Active && !MaterialActive)
                    ReleaseUneditedFiles();
            }

            return;
        }

        var bank = View.HavokCollisionBank;
        var layer = CFG.Current.MapEditor.MapEditor_CollisionEditor_Layer;
        var layerLetter = GetLayerLetter(layer);
        var layerVisible = bank.IsCollisionTypeVisible(layer);

        var wanted = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var files = new List<(CollisionEditorFile File, bool Visible)>();
        var added = new HashSet<CollisionEditorFile>();
        var counts = new Dictionary<uint, int>();

        var start = Stopwatch.GetTimestamp();
        var decodedThisFrame = 0;

        foreach (var entity in GetCollisionParts())
        {
            foreach (var proxy in GetCollisionProxies(entity))
            {
                var fileName = ResolveFileName(entity.MapID, proxy.ResourceHandle?.Name);

                if (fileName == null || Unsupported.Contains(fileName) || char.ToLowerInvariant(fileName[0]) != layerLetter)
                    continue;

                var visible = entity.EditorVisible && layerVisible;

                if (!Files.TryGetValue(fileName, out var file))
                {
                    // Hidden collision isn't decoded just to be drawn hidden
                    if (!visible)
                        continue;

                    wanted.Add(fileName);

                    // One file each frame at least, then more while time is left
                    if (decodedThisFrame > 0 && Stopwatch.GetElapsedTime(start).TotalMilliseconds > OverlayDecodeBudgetMs)
                        continue;

                    file = DecodeFile(entity, fileName);
                    decodedThisFrame++;

                    if (file == null)
                        continue;
                }

                // A file is edited, and drawn, through the part it was first found on, and once if it has several meshes
                if (file.Entity != entity || !added.Add(file))
                    continue;

                wanted.Add(fileName);
                files.Add((file, visible));

                if (!visible)
                    continue;

                foreach (var body in file.Bodies)
                {
                    foreach (var (material, faces) in body.MaterialTriangleCounts)
                        counts[material] = counts.GetValueOrDefault(material) + faces;
                }
            }
        }

        MaterialOverlay.Update(scene, wanted, files, CFG.Current.MapEditor.MapEditor_CollisionEditor_Material_Overlay_Alpha, HiddenMaterials);
        MaterialLegend = counts.OrderBy(e => e.Key).Select(e => (e.Key, e.Value)).ToList();
    }

    #endregion
}
