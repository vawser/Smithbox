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
/// shape (as the Havok Editor's generator makes) or the FLVER of the selected map piece or asset. It goes into new
/// files in the map's h and l binders, and a collision part placing it is added to the map.
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

    // The FLVER of the selected part, read when it is first needed, and which of its meshes go into the collision
    private MsbEntity FlverSource;
    private string FlverSourceModel;
    private FLVER2 Flver;
    private bool[] FlverMeshSelection = [];
    private bool FlverReadFailed;

    private MeshPreview Preview;
    private string PreviewKey;

    // Whether the inputs make any triangles, as of the last time they changed
    private bool GeometryValid;

    private const float MinShapeSize = 0.01f;
    private const int MinCircleSegments = 3;
    private const int MinSemiCircleSegments = 2;

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
                // The selected FLVER is only offered while a map piece or asset of the map is selected
                var usable = shape is not CollisionCreateShape.SelectedFlver || FlverSource != null;

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
        ImGui.InputText("##collisionCreateModel", ref ModelNumber, 6);
        GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Create_Model_Input_TT"));

        var modelProblem = GetModelProblem(map, ModelNumber);

        if (modelProblem != null)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, modelProblem);
        }

        DisplayPreview();

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CollisionEditor_Create_Actions_Header"), LOC.Get("MAP_CollisionEditor_Create_Actions_Header_TT"));

        var canCreate = modelProblem == null && GeometryValid;

        GUI.ConditionalMultiButtonInput("collisionCreateActions",
            "collisionCreateButton", LOC.Get("MAP_CollisionEditor_Create_Action"), LOC.Get("MAP_CollisionEditor_Create_Action_TT"),
            () => Create(map), canCreate);
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
        if (FlverSource == null)
        {
            GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Create_Flver_Unavailable"));
            return;
        }

        GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Create_Flver_Source", FlverSource.Name, FlverSourceModel));

        if (FlverReadFailed || Flver == null)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MAP_CollisionEditor_Create_Flver_Read_Failed"));
            return;
        }

        // Meshes
        GUI.Spacer();
        GUI.ConditionalHeader(LOC.Get("MAP_CollisionEditor_Create_Flver_Meshes_Header"),
            LOC.Get("MAP_CollisionEditor_Create_Flver_Meshes_Header_TT"),
            ref CFG.Current.MapEditor_CollisionEditor_Show_Flver_Meshes);

        if (ImGui.Button($"{LOC.Get("MAP_CollisionEditor_Create_Flver_All")}##collisionCreateMeshesAll"))
            Array.Fill(FlverMeshSelection, true);
        GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Create_Flver_All_TT"));

        ImGui.SameLine();
        if (ImGui.Button($"{LOC.Get("MAP_CollisionEditor_Create_Flver_None")}##collisionCreateMeshesNone"))
            Array.Fill(FlverMeshSelection, false);
        GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Create_Flver_None_TT"));

        if (!CFG.Current.MapEditor_CollisionEditor_Show_Flver_Meshes)
            return;

        ImGui.BeginChild("collisionCreateMeshList", new Vector2(0, 150 * DPI.UIScale()), ImGuiChildFlags.Borders);

        for (var i = 0; i < FlverMeshSelection.Length; i++)
        {
            var mesh = Flver.Meshes[i];
            var material = mesh.MaterialIndex < Flver.Materials.Count ? Flver.Materials[mesh.MaterialIndex].Name : "";

            ImGui.Checkbox($"{LOC.Get("MAP_CollisionEditor_Create_Flver_Mesh", i, material)}##collisionCreateMesh{i}",
                ref FlverMeshSelection[i]);
        }

        ImGui.EndChild();
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
            CollisionCreateShape.SelectedFlver => Flver == null
                ? ""
                : $"{FlverSource?.Name}/{FlverSourceModel}/{GetSourceScale()}/" +
                  string.Concat(FlverMeshSelection.Select(s => s ? '1' : '0')),
            _ => ""
        };

        return $"{Shape}|{Facing}|{inputs}";
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
    /// The single selected map piece or asset of the map, whose FLVER the Selected FLVER shape uses. Its FLVER is read
    /// when the selection changes to it.
    /// </summary>
    private void UpdateFlverSource(MapContainer map)
    {
        var selection = View.ViewportSelection.GetFilteredSelection<MsbEntity>();
        var entity = selection.Count == 1 ? selection.First() : null;

        if (entity != null && (entity.Container != map
            || !(EntityHelper.IsPartMapPiece(entity) || EntityHelper.IsPartAsset(entity) || EntityHelper.IsPartDummyAsset(entity))))
        {
            entity = null;
        }

        var model = entity?.GetPropertyValue("ModelName") as string;

        if (entity == FlverSource && model == FlverSourceModel)
            return;

        FlverSource = entity;
        FlverSourceModel = model;
        Flver = null;
        FlverMeshSelection = [];
        FlverReadFailed = false;

        if (entity == null || string.IsNullOrEmpty(model))
        {
            FlverSource = null;

            if (Shape is CollisionCreateShape.SelectedFlver)
                Shape = CollisionCreateShape.Square;

            return;
        }

        Flver = ReadFlver(entity, model);
        FlverReadFailed = Flver == null;

        if (Flver != null)
        {
            FlverMeshSelection = new bool[Flver.Meshes.Count];
            Array.Fill(FlverMeshSelection, true);
        }
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

    private Vector3 GetSourceScale()
    {
        return FlverSource?.GetPropertyValue("Scale") is Vector3 scale ? scale : Vector3.One;
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
            {
                if (Flver == null || FlverSource == null)
                    return false;

                var selected = Enumerable.Range(0, FlverMeshSelection.Length).Where(i => FlverMeshSelection[i]).ToList();

                if (selected.Count == 0)
                    return false;

                // A mesh whose material is named hkm_Name_Property gets that collision material
                var flver = Flver;
                uint MeshMaterial(int meshIndex)
                {
                    var name = flver.Materials[flver.Meshes[meshIndex].MaterialIndex].Name;
                    return HavokCollisionMaterials.TryParse(name, out var id) ? id : (uint)MaterialId;
                }

                materials = new List<uint>();
                (vertices, indices) = HavokShapeGenerator.GenerateFlverCollision(flver, selected, materials, MeshMaterial);

                // Collision isn't scaled, so the part's scale goes into the vertices
                var scale = GetSourceScale();

                for (var v = 0; v < vertices.Count; v++)
                    vertices[v] *= scale;

                break;
            }
        }

        if (vertices == null || indices == null || vertices.Count == 0 || indices.Count == 0)
            return false;

        if (Facing is CollisionGeneratorTool.CollisionGeneratorFacing.Up)
            HavokShapeGenerator.FlipTriangleWinding(indices);

        materials ??= Enumerable.Repeat((uint)MaterialId, indices.Count / 3).ToList();

        return true;
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

    /// <summary>Why the model number can't be used, or null if it can.</summary>
    private string GetModelProblem(MapContainer map, string number)
    {
        if (number.Length != 6 || !number.All(char.IsDigit))
            return LOC.Get("MAP_CollisionEditor_Create_Model_Invalid");

        if (GetUsedModelNumbers(map).Contains(int.Parse(number)))
            return LOC.Get("MAP_CollisionEditor_Create_Model_Used", number);

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

    /// <summary>
    /// Adds a collision part for the new model, cloned from one of the map's collision parts, at the placement orb
    /// for a basic shape, or where the source part is for its FLVER.
    /// </summary>
    private void AddPart(MapContainer map, MsbEntity template, string number)
    {
        var modelName = $"h{number}";
        var part = (MsbEntity)template.Clone();
        var wrapped = part.WrappedObject;

        SetProperty(wrapped, "ModelName", modelName);
        part.ModelName = modelName;

        // A new part has no entity of its own yet
        SetProperty(wrapped, "EntityID", 0u);

        if (wrapped.GetType().GetProperty("EntityGroupIDs")?.GetValue(wrapped) is uint[] groups)
            Array.Fill(groups, 0u);

        SetProperty(wrapped, "Scale", Vector3.One);

        Entity parent;

        if (Shape is CollisionCreateShape.SelectedFlver && FlverSource != null)
        {
            parent = FlverSource.Parent ?? map.RootObject;

            SetProperty(wrapped, "Position", FlverSource.GetPropertyValue("Position"));
            SetProperty(wrapped, "Rotation", FlverSource.GetPropertyValue("Rotation"));
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
            property.SetValue(target, value);
    }

    #endregion
}
