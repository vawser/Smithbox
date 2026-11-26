using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.HavokEditor;
using StudioCore.Editors.MapEditor;
using StudioCore.Keybinds;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Veldrid.Utilities;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
/// The Material tab: the collision material of each face of the layer, as an overlay in a colour per material (the
/// Map Editor's colours, so a material looks the same in both), painted with a brush, picked, or selected face by face
/// and given any hit material ID. As the Map Editor's Material tab, over a single file.
/// </summary>
public partial class ModelCollisionTool
{
    private bool MaterialActive;

    private CollisionMaterialMode MaterialMode = CollisionMaterialMode.Paint;

    private Hit MaterialHover;
    private bool MaterialStroking;
    private (CollisionEditorBody Body, int Triangle, Vector3 Point) LastPaint;

    private CollisionMaterialPicker MaterialNames;

    private readonly CollisionMaterialOverlay MaterialOverlay = new();
    private readonly HashSet<uint> HiddenMaterials = new();
    private List<(uint Material, int Faces)> MaterialLegend = new();

    private (DebugPrimitiveRenderableProxy Proxy, DbgPrimWallCells Primitive) HoverFace;
    private (CollisionEditorBody Body, int Triangle, uint Material, CollisionMaterialMode Mode) HoverFaceKey;

    private Hit SelectedFace;
    private int SelectedFaceInput;
    private (DebugPrimitiveRenderableProxy Proxy, DbgPrimWallCells Primitive) SelectedFaceMesh;
    private (CollisionEditorBody Body, int Triangle) SelectedFaceMeshKey;

    private const float HoverFaceOffset = 0.05f;
    private const float SelectedFaceOffset = 0.07f;

    private static readonly (KeybindID Key, CollisionMaterialMode Mode)[] MaterialKeys =
    {
        (KeybindID.MapEditor_CollisionEditor_Paint_Material, CollisionMaterialMode.Paint),
        (KeybindID.MapEditor_CollisionEditor_Pick_Material, CollisionMaterialMode.Pick),
        (KeybindID.MapEditor_CollisionEditor_Select_Material, CollisionMaterialMode.Select),
    };

    private CollisionMaterialPicker GetMaterialNames() => MaterialNames ??= new CollisionMaterialPicker(Project);

    private static uint GetPaintMaterial() => (uint)Math.Max(0, CFG.Current.MapEditor.MapEditor_CollisionEditor_Paint_Material);

    #region Tab

    private void DisplayMaterialTab()
    {
        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CollisionEditor_Actions_Header"), LOC.Get("MAP_CollisionEditor_Actions_Header_TT"));

        GUI.MultiButtonInput("modelCollisionMaterialToggle", "modelCollisionMaterialToggleButton",
            MaterialActive ? LOC.Get("MAP_CollisionEditor_Material_Stop") : LOC.Get("MAP_CollisionEditor_Material_Start"),
            MaterialActive ? LOC.Get("MAP_CollisionEditor_Material_Stop_TT") : LOC.Get("MAP_CollisionEditor_Material_Start_TT"),
            ToggleMaterialPainting);

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
        DisplayRemap();
        DisplayPendingList();
    }

    private void DisplayMaterialCursor()
    {
        GUI.Spacer();
        GUI.ConditionalHeader(LOC.Get("MAP_CollisionEditor_Cursor_Header"), LOC.Get("MAP_CollisionEditor_Material_Cursor_Header_TT"),
            ref CFG.Current.MapEditor.MapEditor_CollisionEditor_Show_Cursor);

        if (!CFG.Current.MapEditor.MapEditor_CollisionEditor_Show_Cursor)
            return;

        if (!View.RenderScene.DrawFilter.HasFlag(RenderFilter.Collision))
            GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Collision_Hidden"));
        else if (!MaterialActive)
            GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Material_Cursor_Inactive"));
        else if (MaterialHover is { } hit)
            DisplayFace(hit, "Cursor");
        else
            GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Cursor_None"));
    }

    /// <summary>A face's number, body and material, and its material before it was repainted.</summary>
    private void DisplayFace(Hit face, string context)
    {
        var names = GetMaterialNames();
        var material = face.Body.Materials[face.Triangle];
        var original = face.Body.OriginalMaterials[face.Triangle];
        var bodies = GetLayer()?.Bodies;

        GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Material_Cursor_Face", face.Triangle, bodies?.IndexOf(face.Body) ?? 0));

        DisplaySwatch(material, context);
        ImGui.SameLine();
        GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Material_Cursor_Material", names.Describe(material)));

        if (material != original)
            GUI.WrappedTextColored(UI.Current.ImGui_Invalid_Text_Color,
                LOC.Get("MAP_CollisionEditor_Material_Cursor_Repainted", names.Describe(original)));
    }

    /// <summary>The face Select mode clicked, with its hit material ID, applied when the field is left.</summary>
    private void DisplaySelectedFace()
    {
        ValidateSelectedFace();

        if (MaterialMode is not CollisionMaterialMode.Select && SelectedFace == null)
            return;

        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CollisionEditor_Material_Selected_Header"), LOC.Get("MAP_CollisionEditor_Material_Selected_Header_TT"));

        if (SelectedFace is not { } face)
        {
            GUI.WrappedText(LOC.Get(MaterialActive ? "MAP_CollisionEditor_Material_Selected_None"
                : "MAP_CollisionEditor_Material_Selected_Inactive"));
            return;
        }

        var material = face.Body.Materials[face.Triangle];
        var original = face.Body.OriginalMaterials[face.Triangle];

        DisplayFace(face, "Selected");

        // Hit Material ID
        GUI.Spacer();
        GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Material_Selected_ID"));

        ImGui.SetNextItemWidth(-1);
        ImGui.InputInt("##modelCollisionSelectedFaceMaterial", ref SelectedFaceInput, 0, 0);

        if (GUI.InputCommitted())
        {
            SelectedFaceInput = Math.Max(0, SelectedFaceInput);
            face.Body.SetMaterial(face.Triangle, (uint)SelectedFaceInput);
        }
        else if (!ImGui.IsItemActive())
        {
            SelectedFaceInput = (int)material;
        }
        GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Material_Selected_ID_TT"));

        GUI.Spacer();

        if (material != original)
        {
            if (ImGui.Button($"{LOC.Get("MAP_CollisionEditor_Material_Selected_Revert")}##modelCollisionSelectedFaceRevert"))
                face.Body.SetMaterial(face.Triangle, original);
            GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Material_Selected_Revert_TT"));
            ImGui.SameLine();
        }

        if (ImGui.Button($"{LOC.Get("MAP_CollisionEditor_Material_Selected_Use")}##modelCollisionSelectedFaceUse"))
            CFG.Current.MapEditor.MapEditor_CollisionEditor_Paint_Material = (int)material;
        GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Material_Selected_Use_TT"));
        ImGui.SameLine();

        if (ImGui.Button($"{LOC.Get("MAP_CollisionEditor_Material_Selected_Clear")}##modelCollisionSelectedFaceClear"))
            SelectedFace = null;
        GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Material_Selected_Clear_TT"));
    }

    /// <summary>Drops the selection once its body is no longer one of the decoded layer's (a commit, its undo, another layer).</summary>
    private void ValidateSelectedFace()
    {
        if (SelectedFace is { } face && (GetLayer()?.Bodies.Contains(face.Body) != true || face.Triangle >= face.Body.TriangleCount))
            SelectedFace = null;
    }

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

    private void DisplayPaintBrush()
    {
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CollisionEditor_Brush_Header"), LOC.Get("MAP_CollisionEditor_Material_Brush_Header_TT"));

        ImGui.SetNextItemWidth(-1);
        if (ImGui.BeginCombo("##modelCollisionMaterialMode", LOC.Get(MaterialMode.GetDisplayName())))
        {
            foreach (var (key, value) in MaterialKeys)
            {
                var label = $"{LOC.Get(value.GetDisplayName())} ({InputManager.GetHint(key)})";

                if (ImGui.Selectable($"{label}##modelCollisionMaterialMode{value}", value == MaterialMode))
                    MaterialMode = value;
                GUI.Tooltip(LOC.Get($"MAP_CollisionEditor_Material_Mode_{value}_TT"));
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Material_Mode_Input_TT"));

        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CollisionEditor_Radius_Header"), LOC.Get("MAP_CollisionEditor_Material_Radius_Header_TT"));

        ImGui.SetNextItemWidth(-1);
        if (ImGui.InputFloat("##modelCollisionPaintRadius", ref CFG.Current.MapEditor.MapEditor_CollisionEditor_Paint_Radius))
            CFG.Current.MapEditor.MapEditor_CollisionEditor_Paint_Radius = Math.Max(0f, CFG.Current.MapEditor.MapEditor_CollisionEditor_Paint_Radius);
        GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Material_Radius_Input_TT"));

        if (CFG.Current.MapEditor.MapEditor_CollisionEditor_Paint_Radius > 0)
        {
            GUI.Spacer();
            GUI.SimpleHeader(LOC.Get("MAP_CollisionEditor_Material_Facing_Header"), LOC.Get("MAP_CollisionEditor_Material_Facing_Header_TT"));

            ImGui.SetNextItemWidth(-1);
            if (ImGui.SliderFloat("##modelCollisionPaintFacing", ref CFG.Current.MapEditor.MapEditor_CollisionEditor_Paint_Facing_Limit, 0f, 180f, "%.0f°"))
                CFG.Current.MapEditor.MapEditor_CollisionEditor_Paint_Facing_Limit = Math.Clamp(CFG.Current.MapEditor.MapEditor_CollisionEditor_Paint_Facing_Limit, 0f, 180f);
            GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Material_Facing_Input_TT"));
        }
    }

    private void DisplayMaterialOverlayOptions()
    {
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CollisionEditor_Material_Overlay_Header"), LOC.Get("MAP_CollisionEditor_Material_Overlay_Header_TT"));

        ImGui.Checkbox($"{LOC.Get("MAP_CollisionEditor_Material_Overlay_Toggle")}##modelCollisionMaterialOverlay",
            ref CFG.Current.MapEditor.MapEditor_CollisionEditor_Material_Overlay);
        GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Material_Overlay_Toggle_TT"));

        if (!CFG.Current.MapEditor.MapEditor_CollisionEditor_Material_Overlay)
            return;

        ImGui.SetNextItemWidth(-1);
        ImGui.SliderFloat("##modelCollisionMaterialOverlayAlpha", ref CFG.Current.MapEditor.MapEditor_CollisionEditor_Material_Overlay_Alpha, 5f, 100f, "%.0f%%");
        GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Material_Overlay_Alpha_TT"));

        // Legend
        GUI.Spacer();
        GUI.ConditionalHeader(LOC.Get("MAP_CollisionEditor_Material_Legend_Header"), LOC.Get("MAP_CollisionEditor_Material_Legend_Header_TT"),
            ref CFG.Current.MapEditor.MapEditor_CollisionEditor_Show_Material_Legend);

        if (!CFG.Current.MapEditor.MapEditor_CollisionEditor_Show_Material_Legend)
            return;

        if (MaterialLegend.Count == 0)
        {
            GUI.WrappedText(LOC.Get("MODEL_CollisionTool_Legend_None"));
            return;
        }

        if (HiddenMaterials.Count > 0)
        {
            if (ImGui.Button($"{LOC.Get("MAP_CollisionEditor_Material_Legend_Show_All")}##modelCollisionMaterialShowAll"))
                HiddenMaterials.Clear();
            GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Material_Legend_Show_All_TT"));
        }

        var names = GetMaterialNames();
        var paint = GetPaintMaterial();

        foreach (var (material, faces) in MaterialLegend)
        {
            var shown = !HiddenMaterials.Contains(material);

            if (ImGui.Checkbox($"##modelCollisionMaterialLegendShown{material}", ref shown))
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
            if (ImGui.Selectable($"{LOC.Get("MAP_CollisionEditor_Material_Legend_Entry", names.Describe(material), faces)}##modelCollisionMaterialLegend{material}",
                material == paint))
            {
                CFG.Current.MapEditor.MapEditor_CollisionEditor_Paint_Material = (int)material;
            }
            GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Material_Legend_Entry_TT"));
        }
    }

    private static bool DisplaySwatch(uint material, string context)
    {
        var color = CollisionMaterialOverlay.GetColor(material);
        var size = ImGui.GetFrameHeight() * 0.8f;

        return ImGui.ColorButton($"##modelCollisionMaterialSwatch{context}{material}", new Vector4(color, 1f),
            ImGuiColorEditFlags.NoTooltip | ImGuiColorEditFlags.NoDragDrop, new Vector2(size, size));
    }

    #endregion

    #region Painting

    private void ToggleMaterialPainting()
    {
        MaterialActive = !MaterialActive;

        if (MaterialActive)
        {
            if (EditActive)
                ToggleEditing();

            return;
        }

        MaterialStroking = false;
        MaterialHover = null;
        DisposeCursor();
        DisposeHoverFace();
    }

    private void HandleMaterialPainting(RenderScene scene, DecodedLayer layer, Ray ray, bool canInteract)
    {
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

        MaterialHover = canInteract && collisionShown ? RayCast(layer, ray) : null;

        if (MaterialStroking)
        {
            if (MaterialHover is { } hit && HasMovedFromLastPaint(hit))
                PaintAt(layer, hit);
        }
        else if (brushFree && MaterialHover != null && InputManager.IsMousePressed(MousebindID.Viewport_Picking_Action))
        {
            switch (MaterialMode)
            {
                case CollisionMaterialMode.Select:
                    SelectedFace = MaterialHover;
                    SelectedFaceInput = (int)MaterialHover.Body.Materials[MaterialHover.Triangle];
                    break;

                case CollisionMaterialMode.Pick:
                    CFG.Current.MapEditor.MapEditor_CollisionEditor_Paint_Material = (int)MaterialHover.Body.Materials[MaterialHover.Triangle];
                    MaterialMode = CollisionMaterialMode.Paint;
                    break;

                default:
                    MaterialStroking = true;
                    PaintAt(layer, MaterialHover);
                    break;
            }
        }

        UpdateMaterialCursor(scene);
    }

    private bool HasMovedFromLastPaint(Hit hit)
    {
        if (hit.Body != LastPaint.Body || hit.Triangle != LastPaint.Triangle)
            return true;

        var radius = CFG.Current.MapEditor.MapEditor_CollisionEditor_Paint_Radius;

        return radius > 0 && Vector3.Distance(hit.Point, LastPaint.Point) > radius * 0.25f;
    }

    /// <summary>The paint material on the hit face, and with a radius, on the faces within it facing like the hit face.</summary>
    private void PaintAt(DecodedLayer layer, Hit hit)
    {
        LastPaint = (hit.Body, hit.Triangle, hit.Point);

        var material = GetPaintMaterial();
        var radius = CFG.Current.MapEditor.MapEditor_CollisionEditor_Paint_Radius;

        hit.Body.SetMaterial(hit.Triangle, material);

        if (radius <= 0 || layer == null)
            return;

        var limit = Math.Clamp(CFG.Current.MapEditor.MapEditor_CollisionEditor_Paint_Facing_Limit, 0f, 180f);
        var checkFacing = limit < 180f;
        var minDot = MathF.Cos(limit * MathF.PI / 180f);
        var world = GetWorld();
        var reference = GetWorldNormal(hit.Body, hit.Triangle, world);

        foreach (var body in layer.Bodies)
        {
            var bodyWorld = GetBodyWorld(body, world);

            if (!Matrix4x4.Invert(bodyWorld, out var inverse))
                continue;

            var center = Vector3.Transform(hit.Point, inverse);

            foreach (var t in body.GetTrianglesNear(center, radius / GetScale(bodyWorld)).ToList())
            {
                if (checkFacing)
                {
                    var normal = GetWorldNormal(body, t, world);

                    if (normal == Vector3.Zero || Vector3.Dot(normal, reference) < minDot)
                        continue;
                }

                body.SetMaterial(t, material);
            }
        }
    }

    private static Vector3 GetWorldNormal(CollisionEditorBody body, int triangle, Matrix4x4 world)
    {
        var (a, b, c) = body.GetTriangle(triangle);
        var bodyWorld = GetBodyWorld(body, world);
        var normal = Vector3.Cross(Vector3.Transform(b, bodyWorld) - Vector3.Transform(a, bodyWorld),
            Vector3.Transform(c, bodyWorld) - Vector3.Transform(a, bodyWorld));

        return normal.LengthSquared() < 1e-12f ? Vector3.Zero : Vector3.Normalize(normal);
    }

    #endregion

    #region Display

    private void UpdateMaterialCursor(RenderScene scene)
    {
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

        var key = (hit.Body, hit.Triangle, GetPaintMaterial(), MaterialMode);

        if (HoverFace.Proxy == null || !key.Equals(HoverFaceKey))
        {
            DisposeHoverFace();

            var color = MaterialMode is CollisionMaterialMode.Paint ? CollisionMaterialOverlay.GetColor(GetPaintMaterial()) : Vector3.One;
            HoverFace = MakeFaceMesh(scene, hit.Body, hit.Triangle, HoverFaceOffset, ColorHelper.GetTransparencyColor(color, 85f));
            HoverFaceKey = key;
        }

        var world = GetBodyWorld(hit.Body, GetWorld());

        if (HoverFace.Proxy.World != world)
            HoverFace.Proxy.World = world;
    }

    /// <summary>One face, lifted towards its front, in a colour: the hovered or selected face.</summary>
    private static (DebugPrimitiveRenderableProxy, DbgPrimWallCells) MakeFaceMesh(RenderScene scene, CollisionEditorBody body,
        int triangle, float lift, System.Drawing.Color fill)
    {
        var (a, b, c) = body.GetTriangle(triangle);
        var normal = Vector3.Cross(b - a, c - a);
        var offset = normal.LengthSquared() < 1e-12f ? Vector3.Zero : Vector3.Normalize(normal) * lift;

        var primitive = new DbgPrimWallCells(new[] { (a + offset, b + offset, c + offset) });
        var proxy = new DebugPrimitiveRenderableProxy(scene.OpaqueRenderables, primitive)
        {
            Pickable = false,
            RenderTranslucent = true,
            BaseColor = fill,
            HighlightedColor = fill,
            DrawFilter = RenderFilter.Collision,
        };

        return (proxy, primitive);
    }

    private void DisposeHoverFace()
    {
        HoverFace.Proxy?.Dispose();
        HoverFace.Primitive?.Release();
        HoverFace = default;
    }

    private void UpdateSelectedFaceMesh(RenderScene scene, bool shown)
    {
        ValidateSelectedFace();

        if (!shown || SelectedFace is not { } face)
        {
            DisposeSelectedFaceMesh();
            return;
        }

        var key = (face.Body, face.Triangle);

        if (SelectedFaceMesh.Proxy == null || !key.Equals(SelectedFaceMeshKey))
        {
            DisposeSelectedFaceMesh();

            var fill = ColorHelper.GetTransparencyColor(ToColor(CFG.Current.MapEditor.MapEditor_CollisionEditor_Vertex_Color), 85f);
            SelectedFaceMesh = MakeFaceMesh(scene, face.Body, face.Triangle, SelectedFaceOffset, fill);
            SelectedFaceMeshKey = key;
        }

        var world = GetBodyWorld(face.Body, GetWorld());

        if (SelectedFaceMesh.Proxy.World != world)
            SelectedFaceMesh.Proxy.World = world;
    }

    private void DisposeSelectedFaceMesh()
    {
        SelectedFaceMesh.Proxy?.Dispose();
        SelectedFaceMesh.Primitive?.Release();
        SelectedFaceMesh = default;
    }

    /// <summary>The material overlay over the layer, and the legend's counts, while the Material tab is open.</summary>
    private void UpdateMaterialOverlay(RenderScene scene, DecodedLayer layer)
    {
        var tabOpen = OpenTab == ModelCollisionTab.Material;

        UpdateSelectedFaceMesh(scene, tabOpen);

        if (!tabOpen || !CFG.Current.MapEditor.MapEditor_CollisionEditor_Material_Overlay || layer == null)
        {
            if (MaterialOverlay.IsShown)
                MaterialOverlay.Clear();

            MaterialLegend = new();
            return;
        }

        var key = $"{Wrapper.Name}|{layer.File.EntryName}";
        var visible = Wrapper.Container.Collision?.EditorVisible ?? true;

        MaterialOverlay.Update(scene, new HashSet<string>(StringComparer.OrdinalIgnoreCase) { key },
            new List<(string, IReadOnlyList<CollisionEditorBody>, Matrix4x4, bool)> { (key, layer.Bodies, GetWorld(), visible) },
            CFG.Current.MapEditor.MapEditor_CollisionEditor_Material_Overlay_Alpha, HiddenMaterials);

        var counts = new Dictionary<uint, int>();

        foreach (var body in layer.Bodies)
        {
            foreach (var (material, faces) in body.MaterialTriangleCounts)
                counts[material] = counts.GetValueOrDefault(material) + faces;
        }

        MaterialLegend = counts.OrderBy(e => e.Key).Select(e => (e.Key, e.Value)).ToList();
    }

    #endregion
}
