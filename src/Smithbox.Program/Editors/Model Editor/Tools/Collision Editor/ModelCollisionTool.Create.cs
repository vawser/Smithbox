using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.HavokEditor;
using StudioCore.Editors.MapEditor;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
/// The Create tab: builds the model's collision from its own FLVER meshes (the chosen ones, welded and optionally
/// simplified, as the Map Editor's Selected FLVERs shape builds them), in place of the chosen layers' collision. A layer
/// with a file gets the mesh in that file's body; with layer binders, a layer without one gets a new file based on
/// the other layer's (its class set and settings), which the save adds to its binder. One undoable action.
/// </summary>
public partial class ModelCollisionTool
{
    // The FLVER the meshes were read from, and which of them are used
    private FLVER2 CreateFlver;
    private CollisionFlverModel CreateModel;
    private bool[] CreateMeshes = [];

    private CollisionMaterialPicker CreateMaterialPicker;
    private int CreateMaterialId = 1;

    // Vertices closer than this are welded, as the Map Editor's FLVER build does
    private const float WeldGrid = 0.001f;

    // The triangle counts of the last build, for the result
    private int SimplifiedFrom;
    private int SimplifiedTo;

    private void ResetCreate()
    {
        CreateFlver = null;
        CreateModel = null;
        CreateMeshes = [];
        CreateBodyIndex.Clear();
    }

    private void DisplayCreateTab(ModelWrapper wrapper)
    {
        var files = wrapper.CollisionFiles;

        // Source
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MODEL_CollisionTool_Create_Source_Header"), LOC.Get("MODEL_CollisionTool_Create_Source_Header_TT"));

        UpdateCreateSource(wrapper);

        if (CreateModel == null)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MODEL_CollisionTool_Create_No_Flver"));
            return;
        }

        // Meshes
        GUI.Spacer();
        GUI.ConditionalHeader(LOC.Get("MAP_CollisionEditor_Create_Flver_Meshes_Header"),
            LOC.Get("MAP_CollisionEditor_Create_Flver_Meshes_Header_TT"), ref CFG.Current.MapEditor.MapEditor_CollisionEditor_Show_Flver_Meshes);

        if (ImGui.Button($"{LOC.Get("MAP_CollisionEditor_Create_Flver_All")}##modelCollisionCreateAll"))
            Array.Fill(CreateMeshes, true);
        GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Create_Flver_All_TT"));

        ImGui.SameLine();
        if (ImGui.Button($"{LOC.Get("MAP_CollisionEditor_Create_Flver_None")}##modelCollisionCreateNone"))
            Array.Fill(CreateMeshes, false);
        GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Create_Flver_None_TT"));

        if (CFG.Current.MapEditor.MapEditor_CollisionEditor_Show_Flver_Meshes)
        {
            ImGui.BeginChild("modelCollisionCreateMeshList", new Vector2(0, 150 * DPI.UIScale()), ImGuiChildFlags.Borders);

            for (var i = 0; i < CreateMeshes.Length; i++)
            {
                ImGui.Checkbox($"{LOC.Get("MAP_CollisionEditor_Create_Flver_Mesh", i, CreateModel.Meshes[i].Material)}##modelCollisionCreateMesh{i}",
                    ref CreateMeshes[i]);
            }

            ImGui.EndChild();
        }

        // Material
        GUI.Spacer();
        GUI.ConditionalHeader(LOC.Get("MAP_CollisionEditor_Create_Material_Header"), LOC.Get("MAP_CollisionEditor_Create_Material_Header_TT"),
            ref CFG.Current.MapEditor.MapEditor_CollisionEditor_Show_Create_Material);

        if (CFG.Current.MapEditor.MapEditor_CollisionEditor_Show_Create_Material)
        {
            CreateMaterialPicker ??= new CollisionMaterialPicker(Project);
            CreateMaterialPicker.Display(ref CreateMaterialId, null);
        }

        DisplaySimplifyOptions();

        // Layers
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MODEL_CollisionTool_Create_Layers_Header"), LOC.Get("MODEL_CollisionTool_Create_Layers_Header_TT"));

        foreach (var type in files.Layers)
        {
            if (files.UsesLayerBinders)
            {
                ref var chosen = ref type is HavokCollisionType.High
                    ? ref CFG.Current.ModelEditor.ModelEditor_CollisionTool_Create_High
                    : ref CFG.Current.ModelEditor.ModelEditor_CollisionTool_Create_Low;

                ImGui.Checkbox($"{LOC.Get(type.GetDisplayName())}##modelCollisionCreateLayer{type}", ref chosen);
                GUI.Tooltip(LOC.Get("MODEL_CollisionTool_Create_Layer_TT"));
                ImGui.SameLine();
            }

            GUI.WrappedText(DescribeCreateTarget(files, type));

            if (files.Get(type) is { } layerFile)
                DisplayBodyChoice(layerFile, type);
        }

        // Build
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CollisionEditor_Actions_Header"), LOC.Get("MAP_CollisionEditor_Actions_Header_TT"));

        var targets = GetCreateTargets(files);
        var canBuild = targets.Count > 0 && CreateMeshes.Any(m => m);

        GUI.ConditionalMultiButtonInput("modelCollisionCreateActions",
            "modelCollisionCreateBuild", LOC.Get("MODEL_CollisionTool_Create_Build"), LOC.Get("MODEL_CollisionTool_Create_Build_TT"),
            () => BuildFromFlver(wrapper, targets), canBuild);

        if (!canBuild)
            GUI.WrappedText(LOC.Get("MODEL_CollisionTool_Create_Nothing"));

        if (Layers.Values.Any(l => l.IsEdited && targets.Contains(l.File.Type)))
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MODEL_CollisionTool_Create_Drops_Edits"));
    }

    private void DisplaySimplifyOptions()
    {
        GUI.Spacer();
        GUI.ConditionalHeader(LOC.Get("MAP_CollisionEditor_Create_Flver_Simplify_Header"),
            LOC.Get("MAP_CollisionEditor_Create_Flver_Simplify_Header_TT"), ref CFG.Current.MapEditor.MapEditor_CollisionEditor_Show_Flver_Simplify);

        if (!CFG.Current.MapEditor.MapEditor_CollisionEditor_Show_Flver_Simplify)
        {
            if (CFG.Current.MapEditor.MapEditor_CollisionEditor_Flver_Simplify)
                GUI.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, LOC.Get("MAP_CollisionEditor_Create_Flver_Simplify_Applying"));
            return;
        }

        ImGui.Checkbox($"{LOC.Get("MAP_CollisionEditor_Create_Flver_Simplify")}##modelCollisionCreateSimplify",
            ref CFG.Current.MapEditor.MapEditor_CollisionEditor_Flver_Simplify);
        GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Create_Flver_Simplify_TT"));

        ImGui.BeginDisabled(!CFG.Current.MapEditor.MapEditor_CollisionEditor_Flver_Simplify);

        var target = CFG.Current.MapEditor.MapEditor_CollisionEditor_Flver_Simplify_Target;
        if (ImGui.InputFloat($"{LOC.Get("MAP_CollisionEditor_Create_Flver_Simplify_Target")}##modelCollisionCreateSimplifyTarget", ref target, 5f, 10f, "%.0f%%"))
            CFG.Current.MapEditor.MapEditor_CollisionEditor_Flver_Simplify_Target = Math.Clamp(target, 1f, 100f);
        GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Create_Flver_Simplify_Target_TT"));

        var error = CFG.Current.MapEditor.MapEditor_CollisionEditor_Flver_Simplify_Max_Error;
        if (ImGui.InputFloat($"{LOC.Get("MAP_CollisionEditor_Create_Flver_Simplify_Max_Error")}##modelCollisionCreateSimplifyError", ref error, 0.05f, 0.25f, "%.3f m"))
            CFG.Current.MapEditor.MapEditor_CollisionEditor_Flver_Simplify_Max_Error = MathF.Max(error, 0.001f);
        GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Create_Flver_Simplify_Max_Error_TT"));

        ImGui.Checkbox($"{LOC.Get("MAP_CollisionEditor_Create_Flver_Simplify_Lock_Borders")}##modelCollisionCreateSimplifyLock",
            ref CFG.Current.MapEditor.MapEditor_CollisionEditor_Flver_Simplify_Lock_Borders);
        GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Create_Flver_Simplify_Lock_Borders_TT"));

        ImGui.EndDisabled();

        if (CFG.Current.MapEditor.MapEditor_CollisionEditor_Flver_Simplify && SimplifiedFrom > 0)
        {
            GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Create_Flver_Simplify_Result", SimplifiedFrom, SimplifiedTo,
                $"{100.0 * SimplifiedTo / SimplifiedFrom:0.#}"));
        }
    }

    /// <summary>Reads the loaded model's FLVER into triangles when it changes, with every mesh used.</summary>
    private void UpdateCreateSource(ModelWrapper wrapper)
    {
        var flver = wrapper.GetFlverForReading();

        // The meshes as they are now: a mesh added or removed reads them again
        if (flver == CreateFlver && CreateModel != null && CreateModel.Meshes.Count == (flver?.Meshes.Count ?? 0))
            return;

        CreateFlver = flver;
        CreateModel = null;
        CreateMeshes = [];

        if (flver == null)
            return;

        try
        {
            var colorNormalIsNode = !(Project.Descriptor.ProjectType is ProjectType.NR or ProjectType.DS1 or ProjectType.DS1R);
            CreateModel = CollisionFlverReader.FromFlver2(flver, colorNormalIsNode);
            CreateMeshes = Enumerable.Repeat(true, CreateModel.Meshes.Count).ToArray();
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MODEL_CollisionTool_Create_Read_Failed", wrapper.Name), e);
            CreateModel = null;
        }
    }

    /// <summary>The layers Create builds: the chosen ones, or the model's one collision file.</summary>
    private static List<HavokCollisionType> GetCreateTargets(ModelCollisionFiles files)
    {
        if (!files.UsesLayerBinders)
            return files.Get(HavokCollisionType.Low) != null ? [HavokCollisionType.Low] : [];

        var targets = new List<HavokCollisionType>();

        if (CFG.Current.ModelEditor.ModelEditor_CollisionTool_Create_High)
            targets.Add(HavokCollisionType.High);

        if (CFG.Current.ModelEditor.ModelEditor_CollisionTool_Create_Low)
            targets.Add(HavokCollisionType.Low);

        return targets;
    }

    // The body Create gives the mesh to, by layer, for files with several bodies
    private readonly Dictionary<HavokCollisionType, int> CreateBodyIndex = new();

    /// <summary>The body of a layer's file Create builds into: its only body, or the one chosen. Null if it has none.</summary>
    private Havok.Shared.IHavokBody GetCreateBody(ModelCollisionFile file, HavokCollisionType type)
    {
        var bodies = Havok.Shared.CollisionGeometry.Bodies(file.Document.Objects).ToList();

        if (bodies.Count == 0)
            return null;

        return bodies[GetCreateBodyIndex(bodies, type)];
    }

    /// <summary>The chosen body's number, or until one is chosen, the first that can take a mesh.</summary>
    private int GetCreateBodyIndex(List<Havok.Shared.IHavokBody> bodies, HavokCollisionType type)
    {
        if (!CreateBodyIndex.TryGetValue(type, out var index))
            index = Math.Max(0, bodies.FindIndex(b => CollisionEditorShapes.CanReplaceBody(b, out _)));

        return Math.Clamp(index, 0, bodies.Count - 1);
    }

    /// <summary>Vertices in the model's space, carried into a body's space, where its shape is.</summary>
    private static List<Vector3> ToBodySpace(Havok.Shared.IHavokBody body, List<Vector3> vertices)
    {
        var transform = Havok.Shared.CollisionGeometry.BodyTransform(body);

        if (transform.IsIdentity || !Matrix4x4.Invert(transform, out var inverse))
            return vertices;

        return vertices.Select(v => Vector3.Transform(v, inverse)).ToList();
    }

    /// <summary>A body for the body list: its number, name, shape and motion.</summary>
    private static string DescribeBody(int index, Havok.Shared.IHavokBody body)
    {
        var name = CollisionEditorShapes.GetBodyName(body);
        var motion = CollisionEditorShapes.GetBodyMotion(body);

        return LOC.Get("MODEL_CollisionTool_Create_Body", index, string.IsNullOrEmpty(name) ? "-" : name,
            body.Shape?.GetType().Name ?? "-", motion ?? "-");
    }

    /// <summary>For a layer file with several bodies, which one Create gives the mesh to.</summary>
    private void DisplayBodyChoice(ModelCollisionFile file, HavokCollisionType type)
    {
        var bodies = Havok.Shared.CollisionGeometry.Bodies(file.Document.Objects).ToList();

        if (bodies.Count < 2)
            return;

        var index = GetCreateBodyIndex(bodies, type);

        ImGui.SetNextItemWidth(-1);
        if (ImGui.BeginCombo($"##modelCollisionCreateBody{type}", DescribeBody(index, bodies[index])))
        {
            for (var i = 0; i < bodies.Count; i++)
            {
                var replaceable = CollisionEditorShapes.CanReplaceBody(bodies[i], out _);

                ImGui.BeginDisabled(!replaceable);

                if (ImGui.Selectable($"{DescribeBody(i, bodies[i])}##modelCollisionCreateBody{type}{i}", i == index))
                    CreateBodyIndex[type] = i;

                ImGui.EndDisabled();
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("MODEL_CollisionTool_Create_Body_TT"));
    }

    /// <summary>What Create does to a layer: replaces its file's mesh, makes a new file, or can't.</summary>
    private string DescribeCreateTarget(ModelCollisionFiles files, HavokCollisionType type)
    {
        var entry = files.GetEntryName(type);

        if (files.Get(type) is { } file)
        {
            var body = GetCreateBody(file, type);

            if (body == null || !CollisionEditorShapes.CanReplaceBody(body, out var needsTemplate))
                return LOC.Get("MODEL_CollisionTool_Create_Unsupported", entry);

            var several = Havok.Shared.CollisionGeometry.Bodies(file.Document.Objects).Count() > 1;

            // An older game's box or convex body: the mesh is based on another object's, found when building
            return (several, needsTemplate) switch
            {
                (true, false) => LOC.Get("MODEL_CollisionTool_Create_Replaces_Body", entry),
                (true, true) => LOC.Get("MODEL_CollisionTool_Create_Replaces_Body_Templated", entry),
                (false, true) => LOC.Get("MODEL_CollisionTool_Create_Replaces_Templated", entry),
                _ => LOC.Get("MODEL_CollisionTool_Create_Replaces", entry)
            };
        }

        if (!files.UsesLayerBinders)
            return LOC.Get("MODEL_CollisionTool_Create_No_File");

        var other = type is HavokCollisionType.High ? HavokCollisionType.Low : HavokCollisionType.High;

        return files.Get(other) != null
            ? LOC.Get("MODEL_CollisionTool_Create_Makes", entry, files.GetEntryName(other))
            : LOC.Get("MODEL_CollisionTool_Create_No_Template", entry);
    }

    /// <summary>
    /// Builds the chosen meshes into each target layer, everything before anything changes, so a failure leaves the
    /// collision as it was; then gives them to the layers as one action.
    /// </summary>
    private void BuildFromFlver(ModelWrapper wrapper, List<HavokCollisionType> targets)
    {
        if (!TryBuildFlverGeometry(out var vertices, out var indices, out var materials))
        {
            ReportFailure(LOC.Get("MODEL_CollisionTool_Create_Nothing"));
            return;
        }

        var files = wrapper.CollisionFiles;
        var swaps = new List<ModelCollisionAction.ShapeSwap>();
        var newFiles = new List<ModelCollisionFile>();

        foreach (var type in targets)
        {
            var entry = files.GetEntryName(type);

            try
            {
                if (files.Get(type) is { } file)
                {
                    var body = GetCreateBody(file, type);

                    if (body == null || !CollisionEditorShapes.CanReplaceBody(body, out var needsTemplate))
                    {
                        ReportFailure(LOC.Get("MODEL_CollisionTool_Create_Unsupported", entry));
                        return;
                    }

                    // The mesh is in the model's space; a body's shape is in the body's, which it places
                    var local = ToBodySpace(body, vertices);
                    CollisionEditorShapes.BuiltShape built;

                    if (!needsTemplate)
                    {
                        built = CollisionEditorShapes.BuildShape(body.Shape, local, indices, materials);
                    }
                    else
                    {
                        // An older game's box or convex body: another object's mesh holds what a mesh of the game needs
                        var template = ModelCollisionTemplate.FindHkpMesh(Project, out var source);

                        if (template == null)
                        {
                            ReportFailure(LOC.Get("MODEL_CollisionTool_Create_No_Object_Mesh", entry));
                            return;
                        }

                        built = CollisionEditorShapes.BuildShapeFromTemplate(template, local, indices, materials);
                        Smithbox.Log(this, LOC.Get("MODEL_CollisionTool_Create_Template_Used", source, entry));
                    }

                    if (built == null)
                    {
                        ReportFailure(LOC.Get("MAP_CollisionEditor_Create_Build_Failed", entry));
                        return;
                    }

                    swaps.Add(new ModelCollisionAction.ShapeSwap(file, body, body.Shape, built.Shape));
                }
                else if (files.UsesLayerBinders)
                {
                    var other = type is HavokCollisionType.High ? HavokCollisionType.Low : HavokCollisionType.High;
                    var document = files.Get(other) != null ? files.ReadTemplateDocument(other) : null;

                    // The other layer's file, read afresh, with the new mesh in its body, which keeps its placement and
                    // settings (any replaceable shape: a convex one too)
                    var templateBody = document == null ? null : CollisionEditorShapes.GetReplaceableBody(document);
                    var templateBuilt = templateBody == null ? null
                        : CollisionEditorShapes.BuildShape(templateBody.Shape, ToBodySpace(templateBody, vertices), indices, materials);

                    if (templateBuilt == null || !CollisionEditorShapes.SetShape(templateBody, templateBuilt.Shape))
                    {
                        ReportFailure(LOC.Get("MODEL_CollisionTool_Create_No_Template", entry));
                        return;
                    }

                    newFiles.Add(new ModelCollisionFile
                    {
                        Type = type,
                        EntryName = entry,
                        BinderPath = files.GetBinderPath(type),
                        Document = document,
                        IsNew = true,
                        IsModified = true
                    });
                }
            }
            catch (Exception e)
            {
                ReportFailure(LOC.Get("MAP_CollisionEditor_Create_Build_Failed", entry), e);
                return;
            }
        }

        if (swaps.Count == 0 && newFiles.Count == 0)
            return;

        // Pending edits of the rebuilt layers are replaced along with their mesh
        foreach (var type in targets)
        {
            if (Layers.TryGetValue(type, out var layer))
            {
                foreach (var body in layer.Bodies)
                    body.Reset();
            }
        }

        View.ViewportActionManager.ExecuteAction(new ModelCollisionAction(wrapper, swaps, newFiles, OnCollisionChanged,
            LOC.Get("MODEL_CollisionTool_History_Create")));

        ReportSuccess(LOC.Get("MODEL_CollisionTool_Created", indices.Count / 3,
            string.Join(", ", targets.Select(files.GetEntryName))));
    }

    /// <summary>
    /// The triangles of the chosen meshes in the model's space, welded, with each mesh's collision material (its
    /// hkm_Name_Property material if it has one, else the chosen one), then simplified if that is on.
    /// </summary>
    private bool TryBuildFlverGeometry(out List<Vector3> vertices, out List<int> indices, out List<uint> materials)
    {
        var welded = new List<Vector3>();
        vertices = welded;
        indices = new List<int>();
        materials = new List<uint>();

        SimplifiedFrom = 0;
        SimplifiedTo = 0;

        if (CreateModel == null)
            return false;

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

        for (var m = 0; m < CreateModel.Meshes.Count && m < CreateMeshes.Length; m++)
        {
            if (!CreateMeshes[m])
                continue;

            var mesh = CreateModel.Meshes[m];
            var material = HavokCollisionMaterials.TryParse(mesh.Material, out var id) ? id : (uint)Math.Max(0, CreateMaterialId);

            for (var t = 0; t + 2 < mesh.Corners.Count; t += 3)
            {
                var a = Weld(mesh.Corners[t]);
                var b = Weld(mesh.Corners[t + 1]);
                var c = Weld(mesh.Corners[t + 2]);

                if (a == b || b == c || c == a)
                    continue;

                indices.Add(a);
                indices.Add(b);
                indices.Add(c);
                materials.Add(material);
            }
        }

        if (indices.Count == 0)
            return false;

        if (CFG.Current.MapEditor.MapEditor_CollisionEditor_Flver_Simplify)
        {
            var before = indices.Count / 3;

            (vertices, indices, materials) = CollisionMeshSimplifier.Simplify(welded, indices, materials, new CollisionMeshSimplifier.Settings
            {
                TargetRatio = CFG.Current.MapEditor.MapEditor_CollisionEditor_Flver_Simplify_Target / 100f,
                MaxError = CFG.Current.MapEditor.MapEditor_CollisionEditor_Flver_Simplify_Max_Error,
                LockBorders = CFG.Current.MapEditor.MapEditor_CollisionEditor_Flver_Simplify_Lock_Borders
            });

            SimplifiedFrom = before;
            SimplifiedTo = indices.Count / 3;
        }

        return indices.Count > 0;
    }
}
