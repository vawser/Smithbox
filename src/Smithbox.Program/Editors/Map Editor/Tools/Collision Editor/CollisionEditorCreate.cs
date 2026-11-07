using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.HavokEditor;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.MapEditor;

public enum CollisionCreateShape
{
    [Display(Name = "MAP_CollisionEditor_Create_Shape_Square")]
    Square,
    [Display(Name = "MAP_CollisionEditor_Create_Shape_Triangle")]
    Triangle,
    [Display(Name = "MAP_CollisionEditor_Create_Shape_Circle")]
    Circle,
    [Display(Name = "MAP_CollisionEditor_Create_Shape_SemiCircle")]
    SemiCircle,
    [Display(Name = "MAP_CollisionEditor_Create_Shape_Selected_Flver")]
    SelectedFlver
}

/// <summary>
/// The Create tab of the Collision Editor: makes a new collision for the active map in one go. Its mesh is a basic
/// shape (as the Havok Editor's generator makes) or the FLVERs of the selected map pieces and assets, combined where
/// they are placed. It goes into new files in the map's h and l
/// binders, and a collision part placing it is added to the map.
/// </summary>
public class CollisionEditorCreate
{
    private readonly MapEditorView View;
    private readonly ProjectEntry Project;
    private readonly CollisionEditorTool Tool;

    private CollisionCreateShape Shape = CollisionCreateShape.Square;
    private CollisionGeneratorTool.CollisionGeneratorFacing Facing = CollisionGeneratorTool.CollisionGeneratorFacing.Up;
    private int MaterialId = 1;

    private readonly CollisionGeneratorTool.SquareParameters SquareInputs = new();
    private readonly CollisionGeneratorTool.TriangeParameters TriangleInputs = new();
    private readonly CollisionGeneratorTool.CircleParameters CircleInputs = new();
    private readonly CollisionGeneratorTool.SemiCircleParameters SemiCircleInputs = new();

    private readonly CollisionMaterialPicker MaterialPicker;

    // The model number typed, and the map it was suggested for, so a new map gets a new suggestion
    private string ModelNumber = "";
    private string ModelNumberMap;

    /// <summary>
    /// A selected map piece or asset whose FLVER goes into the collision: the FLVER, read when the part is selected,
    /// (null if it couldn't be read) and which of its meshes are used.
    /// </summary>
    private sealed class FlverSourceEntry
    {
        public MsbEntity Entity;
        public string Model;
        public FLVER2 Flver;
        public bool[] MeshSelection = [];
    }

    // The selected parts, in map order. The first readable one is the anchor: the new part goes where it is
    private List<FlverSourceEntry> FlverSources = [];

    // FLVERs read for the selected parts, by model, so a model placed by several parts is read once
    private Dictionary<string, FLVER2> FlverCache = new();

    private MeshPreview Preview;
    private string PreviewKey;

    // Whether the inputs make any triangles, as of the last time they changed
    private bool GeometryValid;

    // What an overwrite of a used model number does to each of its files, worked out when it is asked for and shown
    // in the confirm
    private sealed record OverwriteFile(string Type, string FileName, bool Exists);

    private sealed record OverwritePlan(string MapId, string Number, List<OverwriteFile> Files, int PartCount, string Problem);

    private OverwritePlan PendingOverwrite;
    private bool OpenOverwriteConfirm;

    private const float MinShapeSize = 0.01f;
    private const int MinCircleSegments = 3;
    private const int MinSemiCircleSegments = 2;

    // Vertices of different sources closer than this are welded, as they would not be in the source FLVERs
    private const float WeldGrid = 0.001f;

    public CollisionEditorCreate(MapEditorView view, ProjectEntry project, CollisionEditorTool tool)
    {
        View = view;
        Project = project;
        Tool = tool;
        MaterialPicker = new CollisionMaterialPicker(project);
    }

    #region Display

    public void Display()
    {
        GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Create_Description"));

        var map = View.Selection.ResolveTargetMap(MapTargetPicker.ActiveMap);

        // Map
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CollisionEditor_Create_Map_Header"), LOC.Get("MAP_CollisionEditor_Create_Map_Header_TT"));

        if (map == null)
        {
            GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Create_No_Map"));
            return;
        }

        GUI.WrappedText(map.Name);
        GUI.DisplayAlias(AliasHelper.GetMapNameAlias(Project, map.Name));

        if (!View.HavokCollisionBank.HasCollisionBinder(map.Name, "h"))
        {
            GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Create_No_Binder", map.Name));
            return;
        }

        UpdateFlverSource(map);

        // Shape
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CollisionEditor_Create_Shape_Header"), LOC.Get("MAP_CollisionEditor_Create_Shape_Header_TT"));

        ImGui.SetNextItemWidth(-1);
        if (ImGui.BeginCombo("##collisionCreateShape", LOC.Get(Shape.GetDisplayName())))
        {
            foreach (var shape in Enum.GetValues<CollisionCreateShape>())
            {
                // The selected FLVERs are only offered while map pieces or assets of the map are selected
                var usable = shape is not CollisionCreateShape.SelectedFlver || FlverSources.Count > 0;

                ImGui.BeginDisabled(!usable);

                if (ImGui.Selectable($"{LOC.Get(shape.GetDisplayName())}##collisionCreateShape{shape}", shape == Shape))
                {
                    Shape = shape;
                }

                ImGui.EndDisabled();

                if (!usable)
                    GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Create_Flver_Unavailable"));
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Create_Shape_Input_TT"));

        // Facing
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CollisionEditor_Create_Facing_Header"), LOC.Get("MAP_CollisionEditor_Create_Facing_Header_TT"));

        ImGui.SetNextItemWidth(-1);
        if (ImGui.BeginCombo("##collisionCreateFacing", LOC.Get(Facing.GetDisplayName())))
        {
            foreach (var facing in Enum.GetValues<CollisionGeneratorTool.CollisionGeneratorFacing>())
            {
                if (ImGui.Selectable($"{LOC.Get(facing.GetDisplayName())}##collisionCreateFacing{facing}", facing == Facing))
                {
                    Facing = facing;
                }
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Create_Facing_Input_TT"));

        // Collision Material
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CollisionEditor_Create_Material_Header"), LOC.Get("MAP_CollisionEditor_Create_Material_Header_TT"));

        MaterialPicker.Display(ref MaterialId, []);

        DisplayParameters();

        // Model ID
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CollisionEditor_Create_Model_Header"), LOC.Get("MAP_CollisionEditor_Create_Model_Header_TT"));

        if (ModelNumberMap != map.Name)
        {
            ModelNumber = SuggestModelNumber(map);
            ModelNumberMap = map.Name;
        }

        ImGui.SetNextItemWidth(-1);
        // The size is ImGui's buffer size, which includes the null terminator, so 6 digits need 7
        ImGui.InputText("##collisionCreateModel", ref ModelNumber, 7, ImGuiInputTextFlags.CharsDecimal);
        GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Create_Model_Input_TT"));

        var modelProblem = GetModelProblem(ModelNumber);
        var modelUsed = modelProblem == null && GetUsedModelNumbers(map).Contains(int.Parse(ModelNumber));

        if (modelProblem != null)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, modelProblem);
        }
        else if (modelUsed)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MAP_CollisionEditor_Create_Model_Used", ModelNumber));
        }

        DisplayPreview();

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CollisionEditor_Create_Actions_Header"), LOC.Get("MAP_CollisionEditor_Create_Actions_Header_TT"));

        var canCreate = modelProblem == null && GeometryValid;

        // A used number overwrites that collision's mesh, once confirmed
        if (modelUsed)
        {
            GUI.ConditionalMultiButtonInput("collisionCreateActions",
                "collisionOverwriteButton", LOC.Get("MAP_CollisionEditor_Create_Overwrite_Action"), LOC.Get("MAP_CollisionEditor_Create_Overwrite_Action_TT"),
                () => BeginOverwrite(map), canCreate);
        }
        else
        {
            GUI.ConditionalMultiButtonInput("collisionCreateActions",
                "collisionCreateButton", LOC.Get("MAP_CollisionEditor_Create_Action"), LOC.Get("MAP_CollisionEditor_Create_Action_TT"),
                () => Create(map), canCreate);
        }

        DisplayOverwriteConfirm(map);
    }

    private void DisplayParameters()
    {
        // Parameters
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CollisionEditor_Create_Parameters_Header"), LOC.Get("MAP_CollisionEditor_Create_Parameters_Header_TT"));

        switch (Shape)
        {
            case CollisionCreateShape.Square:
            {
                var width = SquareInputs.Width;
                if (ImGui.InputFloat($"{LOC.Get("MAP_CollisionEditor_Create_Width")}##collisionCreateWidth", ref width, 0.5f, 1.0f, "%.2f"))
                    SquareInputs.Width = MathF.Max(width, MinShapeSize);
                GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Create_Width_TT"));

                var length = SquareInputs.Length;
                if (ImGui.InputFloat($"{LOC.Get("MAP_CollisionEditor_Create_Length")}##collisionCreateLength", ref length, 0.5f, 1.0f, "%.2f"))
                    SquareInputs.Length = MathF.Max(length, MinShapeSize);
                GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Create_Length_TT"));
                break;
            }
            case CollisionCreateShape.Triangle:
            {
                var size = TriangleInputs.Size;
                if (ImGui.InputFloat($"{LOC.Get("MAP_CollisionEditor_Create_Size")}##collisionCreateSize", ref size, 0.5f, 1.0f, "%.2f"))
                    TriangleInputs.Size = MathF.Max(size, MinShapeSize);
                GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Create_Size_TT"));
                break;
            }
            case CollisionCreateShape.Circle:
            {
                var radius = CircleInputs.Radius;
                if (ImGui.InputFloat($"{LOC.Get("MAP_CollisionEditor_Create_Radius")}##collisionCreateCircleRadius", ref radius, 0.5f, 1.0f, "%.2f"))
                    CircleInputs.Radius = MathF.Max(radius, MinShapeSize);
                GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Create_Radius_TT"));

                var segments = CircleInputs.Segments;
                if (ImGui.InputInt($"{LOC.Get("MAP_CollisionEditor_Create_Segments")}##collisionCreateCircleSegments", ref segments))
                    CircleInputs.Segments = Math.Max(segments, MinCircleSegments);
                GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Create_Segments_TT"));
                break;
            }
            case CollisionCreateShape.SemiCircle:
            {
                var radius = SemiCircleInputs.Radius;
                if (ImGui.InputFloat($"{LOC.Get("MAP_CollisionEditor_Create_Radius")}##collisionCreateSemiRadius", ref radius, 0.5f, 1.0f, "%.2f"))
                    SemiCircleInputs.Radius = MathF.Max(radius, MinShapeSize);
                GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Create_Radius_TT"));

                var segments = SemiCircleInputs.Segments;
                if (ImGui.InputInt($"{LOC.Get("MAP_CollisionEditor_Create_Segments")}##collisionCreateSemiSegments", ref segments))
                    SemiCircleInputs.Segments = Math.Max(segments, MinSemiCircleSegments);
                GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Create_Segments_TT"));
                break;
            }
            case CollisionCreateShape.SelectedFlver:
                DisplayFlverParameters();
                break;
        }
    }

    private void DisplayFlverParameters()
    {
        if (FlverSources.Count == 0)
        {
            GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Create_Flver_Unavailable"));
            return;
        }

        var anchor = GetAnchor();

        if (anchor == null)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MAP_CollisionEditor_Create_Flver_None_Readable"));
            return;
        }

        GUI.WrappedText(FlverSources.Count == 1
            ? LOC.Get("MAP_CollisionEditor_Create_Flver_Source", anchor.Entity.Name, anchor.Model)
            : LOC.Get("MAP_CollisionEditor_Create_Flver_Sources", FlverSources.Count, anchor.Entity.Name));

        foreach (var source in FlverSources.Where(s => s.Flver == null))
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color,
                LOC.Get("MAP_CollisionEditor_Create_Flver_Read_Failed", source.Entity.Name, source.Model));
        }

        // Meshes
        GUI.Spacer();
        GUI.ConditionalHeader(LOC.Get("MAP_CollisionEditor_Create_Flver_Meshes_Header"),
            LOC.Get("MAP_CollisionEditor_Create_Flver_Meshes_Header_TT"),
            ref CFG.Current.MapEditor_CollisionEditor_Show_Flver_Meshes);

        if (ImGui.Button($"{LOC.Get("MAP_CollisionEditor_Create_Flver_All")}##collisionCreateMeshesAll"))
        {
            foreach (var source in FlverSources)
                Array.Fill(source.MeshSelection, true);
        }
        GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Create_Flver_All_TT"));

        ImGui.SameLine();
        if (ImGui.Button($"{LOC.Get("MAP_CollisionEditor_Create_Flver_None")}##collisionCreateMeshesNone"))
        {
            foreach (var source in FlverSources)
                Array.Fill(source.MeshSelection, false);
        }
        GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Create_Flver_None_TT"));

        if (CFG.Current.MapEditor_CollisionEditor_Show_Flver_Meshes)
        {
            ImGui.BeginChild("collisionCreateMeshList", new Vector2(0, 150 * DPI.UIScale()), ImGuiChildFlags.Borders);

            for (var s = 0; s < FlverSources.Count; s++)
            {
                var source = FlverSources[s];

                if (source.Flver == null)
                    continue;

                // A single source lists its meshes as before; several are each a node, so long lists can be closed
                var open = FlverSources.Count == 1 || ImGui.TreeNodeEx(
                    $"{LOC.Get("MAP_CollisionEditor_Create_Flver_Source", source.Entity.Name, source.Model)}##collisionCreateSource{s}",
                    ImGuiTreeNodeFlags.DefaultOpen);

                if (!open)
                    continue;

                for (var i = 0; i < source.MeshSelection.Length; i++)
                {
                    var mesh = source.Flver.Meshes[i];
                    var material = mesh.MaterialIndex < source.Flver.Materials.Count ? source.Flver.Materials[mesh.MaterialIndex].Name : "";

                    ImGui.Checkbox($"{LOC.Get("MAP_CollisionEditor_Create_Flver_Mesh", i, material)}##collisionCreateMesh{s}_{i}",
                        ref source.MeshSelection[i]);
                }

                if (FlverSources.Count > 1)
                    ImGui.TreePop();
            }

            ImGui.EndChild();
        }
    }

    /// <summary>
    /// A 3D preview of the triangles Create will make, as the Havok Editor's generator shows.
    /// </summary>
    private void DisplayPreview()
    {
        // Preview
        GUI.Spacer();
        GUI.ConditionalHeader(LOC.Get("MAP_CollisionEditor_Create_Preview_Header"),
            LOC.Get("MAP_CollisionEditor_Create_Preview_Header_TT"),
            ref CFG.Current.MapEditor_CollisionEditor_Show_Create_Preview);

        UpdateGeometry();

        if (!MeshPreviewRenderer.IsSupported)
        {
            GUI.WrappedText(LOC.Get("EDITOR_ModelPreview_Unsupported"));
            return;
        }

        if (CFG.Current.MapEditor_CollisionEditor_Show_Create_Preview)
        {
            var size = MathF.Min(ImGui.GetContentRegionAvail().X, MeshPreviewRenderer.Resolution * DPI.UIScale());
            size = MathF.Max(size, ModelPreviewBrowser.MinPreviewSize * DPI.UIScale());

            var start = ImGui.GetCursorScreenPos();
            var end = start + new Vector2(size, size);

            ImGui.Dummy(new Vector2(size, size));

            var drawList = ImGui.GetWindowDrawList();

            if (Preview is { State: MeshPreviewState.Ready })
            {
                drawList.AddImage(new ImTextureID(Preview.TextureHandle), start, end);
            }
            else
            {
                var text = Preview is { State: MeshPreviewState.Pending }
                    ? LOC.Get("EDITOR_ModelPreview_Pending")
                    : LOC.Get("EDITOR_ModelPreview_Unavailable");

                drawList.AddRect(start, end, ImGui.GetColorU32(ImGuiCol.Border));
                drawList.AddText(start + (new Vector2(size, size) - ImGui.CalcTextSize(text)) * 0.5f,
                    ImGui.GetColorU32(ImGuiCol.TextDisabled), text);
            }
        }

        if (Preview != null)
        {
            GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Create_Preview_Triangles", Preview.TriangleCount));
        }
    }

    /// <summary>
    /// Builds the triangles again when an input they depend on has changed: for whether Create can be used, and for
    /// the preview. Building a large FLVER's triangles takes time, so this isn't done every frame.
    /// </summary>
    private void UpdateGeometry()
    {
        var key = GetPreviewKey();

        if (key == PreviewKey)
            return;

        PreviewKey = key;

        Preview?.Dispose();
        Preview = null;

        GeometryValid = TryBuildGeometry(out var vertices, out var indices, out _);

        if (!GeometryValid || !MeshPreviewRenderer.IsSupported)
            return;

        var color = CFG.Current.Viewport_Collision_Color;
        Preview = MeshPreviewRenderer.Request(vertices, indices,
            Color.FromArgb(255, (byte)color.X, (byte)color.Y, (byte)color.Z));
    }

    private string GetPreviewKey()
    {
        var inputs = Shape switch
        {
            CollisionCreateShape.Square => $"{SquareInputs.Width}x{SquareInputs.Length}",
            CollisionCreateShape.Triangle => $"{TriangleInputs.Size}",
            CollisionCreateShape.Circle => $"{CircleInputs.Radius}/{CircleInputs.Segments}",
            CollisionCreateShape.SemiCircle => $"{SemiCircleInputs.Radius}/{SemiCircleInputs.Segments}",
            CollisionCreateShape.SelectedFlver => GetFlverPreviewKey(),
            _ => ""
        };

        return $"{Shape}|{Facing}|{inputs}";
    }

    /// <summary>
    /// The FLVER inputs: each source's meshes and its placement relative to the anchor, so moving the whole
    /// selection together doesn't build the triangles again.
    /// </summary>
    private string GetFlverPreviewKey()
    {
        var anchor = GetAnchor();

        if (anchor == null)
            return "";

        var toAnchor = GetToAnchorMatrix(anchor);
        var key = new System.Text.StringBuilder();

        foreach (var source in FlverSources.Where(s => s.Flver != null))
        {
            key.Append('|').Append(source.Entity.Name).Append('/').Append(source.Model).Append('/')
                .Append(GetSourceMatrix(source, anchor, toAnchor)).Append('/');

            foreach (var selected in source.MeshSelection)
                key.Append(selected ? '1' : '0');
        }

        return key.ToString();
    }

    /// <summary>Lets go of the preview, for when the tool is closed.</summary>
    public void Dispose()
    {
        Preview?.Dispose();
        Preview = null;
        PreviewKey = null;
    }

    #endregion

    #region Source FLVER

    /// <summary>
    /// The selected map pieces and assets of the map, whose FLVERs the Selected FLVERs shape uses. Other selected
    /// objects are left out. A part that stays selected keeps its mesh choices; a newly selected one has its FLVER
    /// read (once per model) with every mesh used.
    /// </summary>
    private void UpdateFlverSource(MapContainer map)
    {
        var selected = new List<(MsbEntity Entity, string Model)>();

        foreach (var entity in View.ViewportSelection.GetFilteredSelection<MsbEntity>())
        {
            if (entity.Container != map
                || !(EntityHelper.IsPartMapPiece(entity) || EntityHelper.IsPartAsset(entity) || EntityHelper.IsPartDummyAsset(entity)))
            {
                continue;
            }

            if (entity.GetPropertyValue("ModelName") is string model && !string.IsNullOrEmpty(model))
                selected.Add((entity, model));
        }

        // Most frames nothing has changed
        if (selected.Count == FlverSources.Count
            && selected.All(s => FlverSources.Any(f => f.Entity == s.Entity && f.Model == s.Model)))
        {
            return;
        }

        var previous = FlverSources;
        var cache = new Dictionary<string, FLVER2>();
        var sources = new List<FlverSourceEntry>();

        foreach (var (entity, model) in selected)
        {
            var kept = previous.FirstOrDefault(f => f.Entity == entity && f.Model == model);

            if (kept != null)
            {
                sources.Add(kept);
                cache.TryAdd(GetFlverCacheKey(entity, model), kept.Flver);
                continue;
            }

            var key = GetFlverCacheKey(entity, model);

            if (!cache.TryGetValue(key, out var flver) && !FlverCache.TryGetValue(key, out flver))
                flver = ReadFlver(entity, model);

            cache[key] = flver;

            var source = new FlverSourceEntry { Entity = entity, Model = model, Flver = flver };

            if (flver != null)
            {
                source.MeshSelection = new bool[flver.Meshes.Count];
                Array.Fill(source.MeshSelection, true);
            }

            sources.Add(source);
        }

        // Map order, so the anchor doesn't depend on the order the parts were selected in
        if (sources.Count > 1)
        {
            var order = new Dictionary<Entity, int>();

            for (var i = 0; i < map.Objects.Count; i++)
                order.TryAdd(map.Objects[i], i);

            sources = sources.OrderBy(s => order.TryGetValue(s.Entity, out var i) ? i : int.MaxValue).ToList();
        }

        FlverSources = sources;
        FlverCache = cache;

        if (FlverSources.Count == 0 && Shape is CollisionCreateShape.SelectedFlver)
            Shape = CollisionCreateShape.Square;
    }

    /// <summary>Map pieces are per map; assets are shared by every map.</summary>
    private static string GetFlverCacheKey(MsbEntity entity, string model)
    {
        return EntityHelper.IsPartMapPiece(entity) ? $"{entity.MapID}/{model}" : model;
    }

    /// <summary>The first source whose FLVER could be read: the new part is placed where it is.</summary>
    private FlverSourceEntry GetAnchor()
    {
        return FlverSources.FirstOrDefault(s => s.Flver != null);
    }

    /// <summary>
    /// Reads the FLVER of a map piece or asset from the binder it is in, as the viewport finds it.
    /// </summary>
    private FLVER2 ReadFlver(MsbEntity entity, string model)
    {
        try
        {
            ResourceDescriptor asset;

            if (EntityHelper.IsPartMapPiece(entity))
            {
                var mapId = PathBuilder.GetAssetMapID(Project, entity.MapID);
                var name = ModelLocator.MapModelNameToAssetName(Project, mapId, model);
                asset = ModelLocator.GetMapModel(Project, mapId, name, name);
            }
            else
            {
                asset = ModelLocator.GetObjModel(Project, model, model);
            }

            var path = asset.GetContainerPath(Project);

            if (string.IsNullOrEmpty(path) || !Project.VFS.FS.FileExists(path))
                return null;

            var data = Project.VFS.FS.ReadFile(path);

            if (data == null)
                return null;

            if (path.Contains("bnd", StringComparison.OrdinalIgnoreCase))
            {
                using var binder = new BND4Reader(data.Value);

                var flvers = binder.Files
                    .Where(f => f.Name != null && f.Name.Contains(".flver", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                // The model's own FLVER first, if the binder holds others
                var file = flvers.FirstOrDefault(f => f.Name.Contains(model.TrimStart('m'), StringComparison.OrdinalIgnoreCase))
                    ?? flvers.FirstOrDefault();

                return file == null ? null : FLVER2.Read(binder.ReadFile(file));
            }

            return FLVER2.Read(data.Value);
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MAP_CollisionEditor_Create_Flver_Read_Failed_Log", model), e);
            return null;
        }
    }

    private static Vector3 GetScale(MsbEntity entity)
    {
        return entity?.GetPropertyValue("Scale") is Vector3 scale ? scale : Vector3.One;
    }

    /// <summary>
    /// From world space into the new part's space: the anchor's placement without its scale, since a collision part
    /// isn't scaled.
    /// </summary>
    private static Matrix4x4 GetToAnchorMatrix(FlverSourceEntry anchor)
    {
        var scale = GetScale(anchor.Entity);

        static float Inverse(float s) => MathF.Abs(s) > 1e-6f ? 1f / s : 1f;

        var unscaled = Matrix4x4.CreateScale(Inverse(scale.X), Inverse(scale.Y), Inverse(scale.Z)) * anchor.Entity.GetWorldMatrix();

        return Matrix4x4.Invert(unscaled, out var inverse) ? inverse : Matrix4x4.Identity;
    }

    /// <summary>
    /// From a source FLVER's space into the new part's space. The anchor's is only its scale, exactly, as a single
    /// FLVER has always been built.
    /// </summary>
    private static Matrix4x4 GetSourceMatrix(FlverSourceEntry source, FlverSourceEntry anchor, Matrix4x4 toAnchor)
    {
        if (source == anchor)
            return Matrix4x4.CreateScale(GetScale(source.Entity));

        return source.Entity.GetWorldMatrix() * toAnchor;
    }

    #endregion

    #region Geometry

    /// <summary>
    /// The triangles of the shape the inputs describe, in the space of the new part, with the material of each.
    /// False when there is nothing to build.
    /// </summary>
    private bool TryBuildGeometry(out List<Vector3> vertices, out List<int> indices, out List<uint> materials)
    {
        vertices = null;
        indices = null;
        materials = null;

        switch (Shape)
        {
            case CollisionCreateShape.Square:
                (vertices, indices) = HavokShapeGenerator.GenerateSquare(SquareInputs.Width, SquareInputs.Length);
                break;
            case CollisionCreateShape.Triangle:
                (vertices, indices) = HavokShapeGenerator.GenerateTriangle(TriangleInputs.Size);
                break;
            case CollisionCreateShape.Circle:
                (vertices, indices) = HavokShapeGenerator.GenerateCircle(CircleInputs.Radius, CircleInputs.Segments);
                break;
            case CollisionCreateShape.SemiCircle:
                (vertices, indices) = HavokShapeGenerator.GenerateSemiCircle(SemiCircleInputs.Radius, SemiCircleInputs.Segments);
                break;
            case CollisionCreateShape.SelectedFlver:
                if (!TryBuildFlverGeometry(out vertices, out indices, out materials))
                    return false;
                break;
        }

        if (vertices == null || indices == null || vertices.Count == 0 || indices.Count == 0)
            return false;

        if (Facing is CollisionGeneratorTool.CollisionGeneratorFacing.Up)
            HavokShapeGenerator.FlipTriangleWinding(indices);

        materials ??= Enumerable.Repeat((uint)MaterialId, indices.Count / 3).ToList();

        return true;
    }

    /// <summary>
    /// The triangles of every source's chosen meshes, moved into the anchor's space (collision isn't scaled, so each
    /// part's scale goes into the vertices) and welded across sources.
    /// </summary>
    private bool TryBuildFlverGeometry(out List<Vector3> vertices, out List<int> indices, out List<uint> materials)
    {
        var welded = new List<Vector3>();
        vertices = welded;
        indices = new List<int>();
        materials = new List<uint>();

        var anchor = GetAnchor();

        if (anchor == null)
            return false;

        var toAnchor = GetToAnchorMatrix(anchor);
        var weld = new Dictionary<(int, int, int), int>();

        int Weld(Vector3 position)
        {
            var key = ((int)MathF.Round(position.X / WeldGrid), (int)MathF.Round(position.Y / WeldGrid), (int)MathF.Round(position.Z / WeldGrid));

            if (!weld.TryGetValue(key, out var index))
            {
                index = welded.Count;
                welded.Add(position);
                weld[key] = index;
            }

            return index;
        }

        foreach (var source in FlverSources)
        {
            if (source.Flver == null)
                continue;

            var selected = Enumerable.Range(0, source.MeshSelection.Length).Where(i => source.MeshSelection[i]).ToList();

            if (selected.Count == 0)
                continue;

            // A mesh whose material is named hkm_Name_Property gets that collision material
            var flver = source.Flver;
            uint MeshMaterial(int meshIndex)
            {
                var name = flver.Materials[flver.Meshes[meshIndex].MaterialIndex].Name;
                return HavokCollisionMaterials.TryParse(name, out var id) ? id : (uint)MaterialId;
            }

            var sourceMaterials = new List<uint>();
            var (sourceVertices, sourceIndices) = HavokShapeGenerator.GenerateFlverCollision(flver, selected, sourceMaterials, MeshMaterial);

            if (sourceIndices.Count == 0)
                continue;

            var matrix = GetSourceMatrix(source, anchor, toAnchor);

            // A mirroring placement turns the faces inside out, so their winding is reversed to keep them facing out
            var mirrored = matrix.GetDeterminant() < 0;

            var remap = new int[sourceVertices.Count];

            for (var v = 0; v < sourceVertices.Count; v++)
                remap[v] = Weld(Vector3.Transform(sourceVertices[v], matrix));

            for (var t = 0; t + 2 < sourceIndices.Count; t += 3)
            {
                var a = remap[sourceIndices[t]];
                var b = remap[sourceIndices[t + 1]];
                var c = remap[sourceIndices[t + 2]];

                // Welding can collapse a small triangle to a line or point
                if (a == b || b == c || c == a)
                    continue;

                if (mirrored)
                    (b, c) = (c, b);

                indices.Add(a);
                indices.Add(b);
                indices.Add(c);
                materials.Add(sourceMaterials[t / 3]);
            }
        }

        return indices.Count > 0;
    }

    #endregion

    #region Model ID

    /// <summary>
    /// The first unused model number after the map's highest below 900000 (the 9xxxxx numbers are kept apart in the
    /// game's maps).
    /// </summary>
    private string SuggestModelNumber(MapContainer map)
    {
        var used = GetUsedModelNumbers(map);
        var highest = used.Where(n => n < 900000).DefaultIfEmpty(0).Max();

        for (var n = highest + 1; n < 1000000; n++)
        {
            if (!used.Contains(n))
                return n.ToString("D6");
        }

        return "";
    }

    /// <summary>The model numbers the map's collision files and collision parts use.</summary>
    private HashSet<int> GetUsedModelNumbers(MapContainer map)
    {
        var used = new HashSet<int>();

        if (View.HavokCollisionBank.MapCollisions.TryGetValue(map.Name, out var names))
        {
            foreach (var name in names)
            {
                var match = Regex.Match(name, @"_(\d{6})\.hkx", RegexOptions.IgnoreCase);

                if (match.Success)
                    used.Add(int.Parse(match.Groups[1].Value));
            }
        }

        foreach (var entity in map.Objects.OfType<MsbEntity>().Where(EntityHelper.IsPartCollision))
        {
            if (entity.GetPropertyValue("ModelName") is string model && model.Length == 7 && int.TryParse(model.Substring(1), out var n))
                used.Add(n);
        }

        return used;
    }

    /// <summary>
    /// Why the model number can't be used, or null if it can. A number the map already uses is fine: it is overwritten.
    /// </summary>
    private static string GetModelProblem(string number)
    {
        if (number.Length != 6 || !number.All(char.IsDigit))
            return LOC.Get("MAP_CollisionEditor_Create_Model_Invalid");

        return null;
    }

    private static string GetFileName(string type, string mapId, string number) => $"{type}{mapId.Substring(1)}_{number}.hkx.dcx";

    #endregion

    #region Create

    /// <summary>
    /// Makes the new collision: its files in the map's h and l binders, written into the project, then a collision part
    /// placing it, added as an action so it can be undone.
    /// </summary>
    private void Create(MapContainer map)
    {
        if (!TryBuildGeometry(out var vertices, out var indices, out var materials))
            return;

        var mapId = map.Name;
        var number = ModelNumber;
        var bank = View.HavokCollisionBank;

        // The part is based on one of the map's own collision parts, so its fields suit the map
        var template = map.Objects.OfType<MsbEntity>().FirstOrDefault(EntityHelper.IsPartCollision);

        if (template == null)
        {
            Smithbox.LogError(this, LOC.Get("MAP_CollisionEditor_Create_No_Template_Part", mapId));
            return;
        }

        // h always, l if the map has a low binder
        var types = new[] { "h", "l" }.Where(t => bank.HasCollisionBinder(mapId, t)).ToList();
        var created = new List<(string Type, string FileName)>();

        foreach (var type in types)
        {
            var document = bank.ReadTemplateDocument(mapId, type, CollisionEditorShapes.IsTemplate);

            if (document == null)
            {
                Smithbox.LogError(this, LOC.Get("MAP_CollisionEditor_Create_No_Template_File", $"{type}{mapId.Substring(1)}"));
                return;
            }

            try
            {
                if (!CollisionEditorShapes.MakeNewCollision(document, number, vertices, indices, materials))
                {
                    Smithbox.LogError(this, LOC.Get("MAP_CollisionEditor_Create_Build_Failed", GetFileName(type, mapId, number)));
                    return;
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("MAP_CollisionEditor_Create_Build_Failed", GetFileName(type, mapId, number)), e);
                return;
            }

            created.Add((type, GetFileName(type, mapId, number)));
            bank.AddMapCollision(mapId, type, GetFileName(type, mapId, number), document);
        }

        var success = true;

        foreach (var (type, _) in created)
            success &= bank.SaveMapCollision(mapId, type);

        if (!success)
        {
            Smithbox.LogError(this, LOC.Get("MAP_CollisionEditor_Create_Save_Failed", number));
            return;
        }

        AddPart(map, template, number);

        // The Edit tab finds the new files by their names, which were listed after it last looked
        Tool.OnCollisionCreated();

        Smithbox.Log(this, LOC.Get("MAP_CollisionEditor_Created", string.Join(", ", created.Select(c => c.FileName)),
            indices.Count / 3));

        ModelNumberMap = null;
    }

    #endregion

    #region Overwrite

    /// <summary>
    /// Works out what overwriting the typed model number does, then opens the confirm. Reading a file's document can
    /// read its whole binder, so it is done here rather than every frame.
    /// </summary>
    private void BeginOverwrite(MapContainer map)
    {
        PendingOverwrite = BuildOverwritePlan(map, ModelNumber);
        OpenOverwriteConfirm = true;
    }

    /// <summary>
    /// The files of a model number in the map's h and l binders, which of them exist (their meshes are replaced) and
    /// which don't (they are made, as Create makes them), and the collision parts already placing the model.
    /// </summary>
    private OverwritePlan BuildOverwritePlan(MapContainer map, string number)
    {
        var mapId = map.Name;
        var bank = View.HavokCollisionBank;
        var files = new List<OverwriteFile>();
        string problem = null;

        foreach (var type in new[] { "h", "l" }.Where(t => bank.HasCollisionBinder(mapId, t)))
        {
            var fileName = GetFileName(type, mapId, number);
            var exists = bank.HasMapCollision(mapId, fileName);

            files.Add(new OverwriteFile(type, fileName, exists));

            if (!exists || problem != null)
                continue;

            if (Tool.HasPendingEdits(fileName))
            {
                problem = LOC.Get("MAP_CollisionEditor_Create_Overwrite_Pending_Edits", fileName);
            }
            else if (bank.GetEditableDocument(fileName) is not { } document || CollisionEditorShapes.GetTemplateBody(document) == null)
            {
                problem = LOC.Get("MAP_CollisionEditor_Create_Overwrite_Unsupported", fileName);
            }
        }

        var modelName = $"h{number}";
        var partCount = map.Objects.OfType<MsbEntity>()
            .Where(EntityHelper.IsPartCollision)
            .Count(e => string.Equals(e.GetPropertyValue("ModelName") as string, modelName, StringComparison.OrdinalIgnoreCase));

        return new OverwritePlan(mapId, number, files, partCount, problem);
    }

    /// <summary>
    /// The overwrite confirm: what happens to each file and to the parts, and why it can't go ahead if it can't.
    /// </summary>
    private void DisplayOverwriteConfirm(MapContainer map)
    {
        var title = $"{LOC.Get("MAP_CollisionEditor_Create_Overwrite_Title")}##collisionCreateOverwriteConfirm";

        if (OpenOverwriteConfirm)
        {
            ImGui.OpenPopup(title);
            OpenOverwriteConfirm = false;
        }

        var plan = PendingOverwrite;
        var open = true;

        ImGui.SetNextWindowSize(new Vector2(480, 0) * DPI.UIScale(), ImGuiCond.Always);

        if (!ImGui.BeginPopupModal(title, ref open, ImGuiWindowFlags.AlwaysAutoResize))
            return;

        // The confirm is for the map and number it was opened for
        var stale = plan == null || plan.MapId != map.Name || plan.Number != ModelNumber;

        if (stale)
        {
            ImGui.CloseCurrentPopup();
            ImGui.EndPopup();
            return;
        }

        GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Create_Overwrite_Text", $"h{plan.Number}"));

        foreach (var file in plan.Files)
        {
            GUI.WrappedText(LOC.Get(file.Exists
                ? "MAP_CollisionEditor_Create_Overwrite_File_Replaced"
                : "MAP_CollisionEditor_Create_Overwrite_File_Created", file.FileName));
        }

        GUI.WrappedText(plan.PartCount > 0
            ? LOC.Get("MAP_CollisionEditor_Create_Overwrite_Parts_Kept", plan.PartCount)
            : LOC.Get("MAP_CollisionEditor_Create_Overwrite_Part_Added"));

        GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Create_Overwrite_Undo"));

        if (plan.Problem != null)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, plan.Problem);
        }

        var confirmed = false;
        var cancelled = false;

        GUI.ConditionalMultiButtonInput("collisionOverwriteConfirmActions",
            "collisionOverwriteConfirm", LOC.Get("MAP_CollisionEditor_Create_Overwrite_Confirm"), LOC.Get("MAP_CollisionEditor_Create_Overwrite_Confirm_TT"),
            () => confirmed = true, plan.Problem == null && GeometryValid,

            "collisionOverwriteCancel", LOC.Get("MAP_CollisionEditor_Create_Overwrite_Cancel"), LOC.Get("MAP_CollisionEditor_Create_Overwrite_Cancel_TT"),
            () => cancelled = true, true);

        if (confirmed || cancelled || !open)
        {
            ImGui.CloseCurrentPopup();
            PendingOverwrite = null;
        }

        ImGui.EndPopup();

        if (confirmed)
            Overwrite(map, plan);
    }

    /// <summary>
    /// Overwrites a used model number: each existing file's body gets the new mesh, built in its own class set, as one
    /// undoable action (the body keeps its name and placement); missing files are made as Create makes them; and a
    /// part is added only if none places the model yet. The binders are written straight away, as a commit writes
    /// them.
    /// </summary>
    private void Overwrite(MapContainer map, OverwritePlan plan)
    {
        if (!TryBuildGeometry(out var vertices, out var indices, out var materials))
            return;

        var mapId = map.Name;
        var number = plan.Number;
        var bank = View.HavokCollisionBank;

        // The part is only needed when nothing places the model yet
        MsbEntity template = null;

        if (plan.PartCount == 0)
        {
            template = map.Objects.OfType<MsbEntity>().FirstOrDefault(EntityHelper.IsPartCollision);

            if (template == null)
            {
                Smithbox.LogError(this, LOC.Get("MAP_CollisionEditor_Create_No_Template_Part", mapId));
                return;
            }
        }

        // Everything is built before anything changes, so a failure leaves the map as it was
        var swaps = new List<CollisionEditorAction.ShapeSwap>();
        var remaps = new List<CollisionEditorAction.WallRemap>();
        var newFiles = new List<(OverwriteFile File, Havok.HavokDocument Document)>();

        foreach (var file in plan.Files)
        {
            if (file.Exists)
            {
                var body = bank.GetEditableDocument(file.FileName) is { } document
                    ? CollisionEditorShapes.GetTemplateBody(document)
                    : null;

                if (body == null)
                {
                    Smithbox.LogError(this, LOC.Get("MAP_CollisionEditor_Create_Overwrite_Unsupported", file.FileName));
                    return;
                }

                CollisionEditorShapes.BuiltShape built;

                try
                {
                    built = CollisionEditorShapes.BuildShape(body.Shape, vertices, indices, materials);
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, LOC.Get("MAP_CollisionEditor_Create_Build_Failed", file.FileName), e);
                    return;
                }

                if (built == null)
                {
                    Smithbox.LogError(this, LOC.Get("MAP_CollisionEditor_Create_Build_Failed", file.FileName));
                    return;
                }

                swaps.Add(new CollisionEditorAction.ShapeSwap(map, file.FileName, body, body.Shape, built.Shape));

                // Sekiro's WALL files mark the low collision's triangles by key, and none of the old triangles are
                // left, so their marks go with them
                if (Project.Descriptor.ProjectType is ProjectType.SDT && file.Type == "l")
                {
                    if (!View.WallBank.EnsureMapLoaded(mapId))
                    {
                        Smithbox.LogError(this, LOC.Get("MAP_CollisionEditor_Create_Overwrite_Walls_Unread", file.FileName));
                        return;
                    }

                    var modelName = $"h{number}";
                    var before = View.WallBank.GetMarks(mapId, modelName);

                    if (before != null && (before.Triangles.Count > 0 || before.NoJumpKeys.Count > 0))
                    {
                        remaps.Add(new CollisionEditorAction.WallRemap(map, mapId, modelName, before,
                            WallBank.RemapMarks(before, new Dictionary<uint, uint>())));
                    }
                }
            }
            else
            {
                var document = bank.ReadTemplateDocument(mapId, file.Type, CollisionEditorShapes.IsTemplate);

                if (document == null)
                {
                    Smithbox.LogError(this, LOC.Get("MAP_CollisionEditor_Create_No_Template_File", $"{file.Type}{mapId.Substring(1)}"));
                    return;
                }

                try
                {
                    if (!CollisionEditorShapes.MakeNewCollision(document, number, vertices, indices, materials))
                    {
                        Smithbox.LogError(this, LOC.Get("MAP_CollisionEditor_Create_Build_Failed", file.FileName));
                        return;
                    }
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, LOC.Get("MAP_CollisionEditor_Create_Build_Failed", file.FileName), e);
                    return;
                }

                newFiles.Add((file, document));
            }
        }

        foreach (var (file, document) in newFiles)
            bank.AddMapCollision(mapId, file.Type, file.FileName, document);

        // The new files' names are worked out before the swapped files' meshes are rebuilt, which looks them up
        Tool.OnCollisionCreated();

        if (swaps.Count > 0)
            Tool.ApplyShapeSwaps(swaps, remaps);

        var success = true;

        foreach (var type in plan.Files.Select(f => f.Type).Distinct())
            success &= bank.SaveMapCollision(mapId, type);

        if (remaps.Count > 0)
            success &= View.WallBank.SaveWalls(mapId);

        if (!success)
        {
            Smithbox.LogError(this, LOC.Get("MAP_CollisionEditor_Create_Save_Failed", number));
            return;
        }

        if (template != null)
            AddPart(map, template, number);

        Smithbox.Log(this, LOC.Get("MAP_CollisionEditor_Overwritten", string.Join(", ", plan.Files.Select(f => f.FileName)),
            indices.Count / 3));

        ModelNumberMap = null;
    }

    #endregion

    #region Part

    /// <summary>
    /// Adds a collision part for the new model, cloned from one of the map's collision parts, at the placement orb
    /// for a basic shape, or where the anchor (the first source part, in map order) is for FLVERs.
    /// </summary>
    private void AddPart(MapContainer map, MsbEntity template, string number)
    {
        var modelName = $"h{number}";
        var part = (MsbEntity)template.Clone();
        var wrapped = part.WrappedObject;

        SetProperty(wrapped, "ModelName", modelName);
        part.ModelName = modelName;

        // A new part has no entity of its own yet (the IDs are uint in ER and NR, int in DS3 and SDT)
        SetProperty(wrapped, "EntityID", 0);

        if (wrapped.GetType().GetProperty("EntityGroupIDs")?.GetValue(wrapped) is Array groups)
            Array.Clear(groups);

        SetProperty(wrapped, "Scale", Vector3.One);

        Entity parent;

        // The FLVERs' triangles are built in the anchor's space, so the part goes where the anchor is
        if (Shape is CollisionCreateShape.SelectedFlver && GetAnchor() is { } anchor)
        {
            parent = anchor.Entity.Parent ?? map.RootObject;

            SetProperty(wrapped, "Position", anchor.Entity.GetPropertyValue("Position"));
            SetProperty(wrapped, "Rotation", anchor.Entity.GetPropertyValue("Rotation"));
        }
        else
        {
            parent = map.RootObject;

            SetProperty(wrapped, "Position", GetPlacementPosition(parent));
            SetProperty(wrapped, "Rotation", Vector3.Zero);
        }

        // Named for its model, with the first free instance number, as the game names parts
        var names = new HashSet<string>(map.Objects.Select(o => o.Name), StringComparer.OrdinalIgnoreCase);
        var instance = 0;

        while (names.Contains($"{modelName}_{instance:D4}"))
            instance++;

        SetProperty(wrapped, "Name", $"{modelName}_{instance:D4}");
        SetProperty(wrapped, "InstanceID", instance);

        // Loaded from the binder by name first, so the part's own load joins this one instead of reading the binder's
        // every collision again
        var assetMapId = PathBuilder.GetAssetMapID(Project, map.Name);
        var assetName = ModelLocator.MapModelNameToAssetName(Project, assetMapId, modelName);

        foreach (var type in View.HavokCollisionBank.GetVisibleCollisionTypes())
        {
            var asset = ModelLocator.GetMapCollisionModel(Project, assetMapId, assetName, false, type);

            if (!asset.IsValid())
                continue;

            ResourceManager.DefaultPool.LoadAsset<HavokCollisionResource>(Project, asset, new LoadOptions
            {
                JobName = LOC.Get("MAP_ResourceManager_Collision_Models"),
                Types = ResourceType.CollisionHKX,
                AccessLevel = AccessLevel.AccessGPUOptimizedOnly,
                OnlyNames = new[] { asset.Name }
            });
        }

        View.ViewportActionManager.ExecuteAction(new EntAddAction(View, map, new List<MsbEntity> { part }, parent));
    }

    /// <summary>
    /// The placement orb's position, made local to the parent chain (which, for tiled maps, holds the map's offset),
    /// as the Create tool places new objects.
    /// </summary>
    private Vector3 GetPlacementPosition(Entity parent)
    {
        if (View.ViewportHandler.ActiveViewport?.Viewport == null)
            return Vector3.Zero;

        var world = View.ViewportWindow.GetPlacementPosition();

        var root = Transform.Default;
        for (var p = parent; p != null; p = p.Parent)
        {
            root += p.GetLocalTransform();
        }

        return Vector3.Transform(world - root.Position, Quaternion.Inverse(root.Rotation));
    }

    private static void SetProperty(object target, string name, object value)
    {
        var property = target.GetType().GetProperty(name);

        if (property == null || !property.CanWrite || value == null)
            return;

        if (property.PropertyType.IsInstanceOfType(value))
        {
            property.SetValue(target, value);
        }
        else if (property.PropertyType.IsPrimitive && value is IConvertible)
        {
            // A number given as one type for a property of another (an entity ID is int in some MSBs, uint in others)
            property.SetValue(target, Convert.ChangeType(value, property.PropertyType));
        }
    }

    #endregion
}
