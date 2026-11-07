using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The Models tab's Generate group (Navigation Editor Plan, Phase 4; Navmesh Generation Plan): generates a model's n and
/// o navmeshes from collision models of the map, in place of the selected model's or into a free placeholder, as one
/// undo. The generation runs in the background; the edit is made when it is done. See NavigationGenerator.
/// </summary>
public partial class NavigationTool
{
    /// <summary>
    /// A selected part to generate from: a map collision part (its high collision file) or an asset (its own collision
    /// binder), placed where the part is.
    /// </summary>
    private sealed record GenerateSource(MsbEntity Part, string MapId, bool IsAsset, string Model, string Label);

    /// <summary>
    /// The parts tagged as blockers, by map and part name. Kept for the session; every tagged part of a loaded map is a
    /// blocker in each generation, selected or not.
    /// </summary>
    private readonly HashSet<(string MapId, string Part)> BlockerTags = new();
    private bool ShowBlockers = true;

    /// <summary>Asset collision read so far, by asset model, as triangles in the asset's own space (null: none found).</summary>
    private readonly Dictionary<string, List<Vector3>> AssetCollisionCache = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Where a model edit puts its new n and o files.</summary>
    private enum ModelTarget { Replace, Slot, New }

    private ModelTarget GenerateTarget = ModelTarget.New;

    /// <summary>The ModelID a generated new model gets; -1 until it is set from the chosen collision.</summary>
    private int GenerateNewModelId = -1;
    private bool GenerateNameAfterCollision = true;
    private bool ShowGenerateCollisions = true;
    private NavGenParameters GenerateN = NavGenParameters.DefaultN;
    private NavGenParameters GenerateO = NavGenParameters.DefaultO;

    private bool Generating;
    private readonly ConcurrentQueue<Action> GenerateResults = new();

    private void DisplayGenerate(MapContainer map, NavigationMap navigation, NavmeshModel selected, HashSet<NavmeshModel> removable, List<NavmeshModel> slots)
    {
        // Generate
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Generate_Header"), LOC.Get("MAP_Navigation_Generate_Header_TT"));

        var sources = SelectedSources();
        var walkable = sources.Where(s => !IsBlocker(s)).ToList();
        int preferredId = walkable.Where(s => !s.IsAsset).Select(s => CollisionModelId(s.Model)).FirstOrDefault(-1);

        // Collision: the selected parts. The count stays outside the list toggle
        GUI.ConditionalHeader(LOC.Get("MAP_Navigation_Generate_Collision_Header"), LOC.Get("MAP_Navigation_Generate_Collision_Header_TT"), ref ShowGenerateCollisions);
        GUI.WrappedText(LOC.Get("MAP_Navigation_Generate_Collision_Count", sources.Count(s => !s.IsAsset), sources.Count(s => s.IsAsset)));

        if (ShowGenerateCollisions && sources.Count > 0)
        {
            ImGui.BeginChild("##navigationGenerateSources", new Vector2(0, Math.Min(140, 20 + 18 * sources.Count) * DPI.UIScale()), ImGuiChildFlags.Borders);
            foreach (var source in sources)
            {
                bool blocker = IsBlocker(source);
                if (ImGui.Checkbox($"{LOC.Get("MAP_Navigation_Generate_Blocker")}##navigationGenerateBlocker_{source.MapId}_{source.Part.Name}", ref blocker))
                {
                    if (blocker) BlockerTags.Add((source.MapId, source.Part.Name));
                    else BlockerTags.Remove((source.MapId, source.Part.Name));
                    BlockerOverlayDirty = true;
                }
                GUI.Tooltip(LOC.Get("MAP_Navigation_Generate_Blocker_TT"));
                ImGui.SameLine();
                ImGui.Text(source.Label);
            }
            ImGui.EndChild();
        }

        DisplayBlockers();

        // Target
        if (!DisplayModelTarget("navigationGenerate", map, navigation, selected, removable, slots, ref GenerateTarget, ref GenerateNewModelId, preferredId))
            return;

        if (GenerateTarget == ModelTarget.Slot)
        {
            ImGui.Checkbox($"{LOC.Get("MAP_Navigation_Generate_Name_After")}##navigationGenerateNameAfter", ref GenerateNameAfterCollision);
            GUI.Tooltip(LOC.Get("MAP_Navigation_Generate_Name_After_TT"));
        }

        // Settings of each set
        GenerateN = DisplayGenerateParameters("n", LOC.Get("MAP_Navigation_Generate_Set_N"), GenerateN, NavGenParameters.DefaultN);
        GenerateO = DisplayGenerateParameters("o", LOC.Get("MAP_Navigation_Generate_Set_O"), GenerateO, NavGenParameters.DefaultO);

        if (Generating)
        {
            GUI.WrappedText(LOC.Get("MAP_Navigation_Generate_Running"));
            return;
        }

        if (walkable.Count == 0)
        {
            GUI.WrappedText(LOC.Get("MAP_Navigation_Generate_Pick_Collision"));
            return;
        }

        if (ImGui.Button($"{LOC.Get("MAP_Navigation_Generate")}##navigationGenerate"))
            StartGenerate(map, navigation, walkable, TaggedBlockers(), preferredId, GenerateTarget switch
            {
                ModelTarget.Slot => slots.FirstOrDefault(s => s.ModelID == SlotModelID),
                ModelTarget.Replace => selected,
                _ => null,
            }, GenerateTarget, GenerateNewModelId);
        GUI.Tooltip(LOC.Get("MAP_Navigation_Generate_TT"));
    }

    private NavGenParameters DisplayGenerateParameters(string id, string title, NavGenParameters p, NavGenParameters defaults)
    {
        GUI.Spacer();
        GUI.SimpleHeader(title, LOC.Get("MAP_Navigation_Generate_Set_TT"));

        float cell = p.CellSize, radius = p.AgentRadius, height = p.AgentHeight, climb = p.MaxClimb, slope = p.MaxSlope, edge = p.MaxEdgeLength;

        ImGui.SetNextItemWidth(-1);
        ImGui.SliderFloat($"##navigationGenerateCell_{id}", ref cell, 0.1f, 1f, $"{LOC.Get("MAP_Navigation_Generate_Cell")} %.2f m");
        GUI.Tooltip(LOC.Get("MAP_Navigation_Generate_Cell_TT"));
        ImGui.SetNextItemWidth(-1);
        ImGui.SliderFloat($"##navigationGenerateRadius_{id}", ref radius, 0f, 2f, $"{LOC.Get("MAP_Navigation_Generate_Radius")} %.2f m");
        GUI.Tooltip(LOC.Get("MAP_Navigation_Generate_Radius_TT"));
        ImGui.SetNextItemWidth(-1);
        ImGui.SliderFloat($"##navigationGenerateHeight_{id}", ref height, 0.5f, 8f, $"{LOC.Get("MAP_Navigation_Generate_Height")} %.2f m");
        GUI.Tooltip(LOC.Get("MAP_Navigation_Generate_Height_TT"));
        ImGui.SetNextItemWidth(-1);
        ImGui.SliderFloat($"##navigationGenerateClimb_{id}", ref climb, 0f, 3f, $"{LOC.Get("MAP_Navigation_Generate_Climb")} %.2f m");
        GUI.Tooltip(LOC.Get("MAP_Navigation_Generate_Climb_TT"));
        ImGui.SetNextItemWidth(-1);
        ImGui.SliderFloat($"##navigationGenerateSlope_{id}", ref slope, 5f, 89f, $"{LOC.Get("MAP_Navigation_Generate_Slope")} %.0f°");
        GUI.Tooltip(LOC.Get("MAP_Navigation_Generate_Slope_TT"));
        ImGui.SetNextItemWidth(-1);
        ImGui.SliderFloat($"##navigationGenerateEdge_{id}", ref edge, 0f, 30f, $"{LOC.Get("MAP_Navigation_Generate_Edge")} %.1f m");
        GUI.Tooltip(LOC.Get("MAP_Navigation_Generate_Edge_TT"));

        p = p with { CellSize = cell, AgentRadius = radius, AgentHeight = height, MaxClimb = climb, MaxSlope = slope, MaxEdgeLength = edge };

        if (ImGui.SmallButton($"{LOC.Get("MAP_Navigation_Generate_Reset")}##navigationGenerateReset_{id}"))
            p = defaults;
        GUI.Tooltip(LOC.Get("MAP_Navigation_Generate_Reset_TT"));

        return p;
    }

    /// <summary>Makes the edits of finished generations, on the thread that draws. Called every frame by the viewport.</summary>
    private void DrainGenerateResults()
    {
        while (GenerateResults.TryDequeue(out var apply))
            apply();
    }

    /// <summary>
    /// The parts selected in the viewport to generate from: map collision parts and assets, in any loaded map. Dummy
    /// assets have no collision in game, so they aren't used.
    /// </summary>
    private List<GenerateSource> SelectedSources()
    {
        var sources = new List<GenerateSource>();

        foreach (var part in View.ViewportSelection.GetFilteredSelection<MsbEntity>())
            if (SourceOf(part) is GenerateSource source)
                sources.Add(source);

        return sources;
    }

    /// <summary>The parts tagged as blockers: their count, each with Untag, and Clear. Shown whatever is selected.</summary>
    private void DisplayBlockers()
    {
        // Blockers: the count and Clear stay outside the list toggle
        GUI.Spacer();
        GUI.ConditionalHeader(LOC.Get("MAP_Navigation_Generate_Blockers_Header"), LOC.Get("MAP_Navigation_Generate_Blockers_Header_TT"), ref ShowBlockers);
        GUI.WrappedText(LOC.Get("MAP_Navigation_Generate_Blockers_Count", BlockerTags.Count));

        if (BlockerTags.Count == 0)
            return;

        if (ImGui.SmallButton($"{LOC.Get("MAP_Navigation_Generate_Blockers_Clear")}##navigationBlockersClear"))
        {
            BlockerTags.Clear();
            BlockerOverlayDirty = true;
            return;
        }
        GUI.Tooltip(LOC.Get("MAP_Navigation_Generate_Blockers_Clear_TT"));

        if (!ShowBlockers)
            return;

        foreach (var tag in BlockerTags.OrderBy(t => t.MapId).ThenBy(t => t.Part).ToList())
        {
            if (ImGui.SmallButton($"{LOC.Get("MAP_Navigation_Generate_Blockers_Untag")}##navigationBlockerUntag_{tag.MapId}_{tag.Part}"))
            {
                BlockerTags.Remove(tag);
                BlockerOverlayDirty = true;
            }
            ImGui.SameLine();
            ImGui.Text($"{tag.Part} ({tag.MapId})");
        }
    }

    /// <summary>The tagged parts' transforms when the red boxes were last drawn, to see when one moves.</summary>
    private int BlockerTransformsSignature;

    /// <summary>Called every frame: every 15th, redraws the red boxes if a tagged part has moved.</summary>
    private void CheckBlockerMoves()
    {
        if (!ShowOverlay || BlockerTags.Count == 0 || DrawFrame % 15 != 0)
            return;

        var hash = new HashCode();
        foreach (var source in TaggedBlockers())
            hash.Add(source.Part.GetWorldMatrix());

        int signature = hash.ToHashCode();
        if (signature != BlockerTransformsSignature)
        {
            BlockerTransformsSignature = signature;
            BlockerOverlayDirty = true;
        }
    }

    /// <summary>
    /// Draws the box of every tagged blocker in the loaded maps as a red fill while the overlay is shown, so it is
    /// plain what generation will go around. Each triangle is pushed a few centimetres out along its normal, so the
    /// fill shows over the collision drawn in the same place.
    /// </summary>
    private void UpdateBlockerOverlay()
    {
        BlockerOverlayDirty = false;
        BlockerRenderer.Clear();

        var scene = View.ViewportHandler.ActiveViewport?.RenderScene;
        if (!ShowOverlay || scene == null || BlockerTags.Count == 0)
            return;

        var overlay = new NavOverlay { MapId = OverlayMap, Set = OverlaySet };
        var batch = overlay.Batch(NavOverlayLayer.Blockers);

        foreach (var source in TaggedBlockers())
        {
            // In world space, a little larger, so the fill shows over the part
            var corners = BlockerCorners(source, Matrix4x4.Identity, 0.03f);
            if (corners == null)
                continue;

            var triangles = BoxTriangles(corners);
            for (int i = 0; i + 2 < triangles.Count; i += 3)
                batch.Triangles.Add((triangles[i], triangles[i + 1], triangles[i + 2]));
        }

        BlockerRenderer.Show(scene, overlay, FaceOpacity);
    }

    /// <summary>
    /// The eight corners of a blocker's box: the bounds of its model, as the viewport's selection box shows them,
    /// placed by the part's world transform and then the given one. Corner i has the high X, Y, Z side for bits 1, 2, 4.
    /// When the model isn't loaded, the bounds of its collision. Grown by a margin in each direction. Null if neither.
    /// </summary>
    private List<Vector3> BlockerCorners(GenerateSource source, Matrix4x4 then, float margin)
    {
        Vector3 lo, hi;
        var bounds = source.Part.RenderSceneMesh?.GetLocalBounds();

        if (bounds is Veldrid.Utilities.BoundingBox box && box.Max.X > box.Min.X && box.Max.Z > box.Min.Z)
        {
            lo = box.Min;
            hi = box.Max;
        }
        else if (SourceTriangles(source) is { Count: > 0 } local)
        {
            lo = local.Aggregate(new Vector3(float.MaxValue), Vector3.Min);
            hi = local.Aggregate(new Vector3(float.MinValue), Vector3.Max);
        }
        else
        {
            return null;
        }

        var world = source.Part.GetWorldMatrix() * then;
        var corners = new List<Vector3>(8);
        for (int i = 0; i < 8; i++)
            corners.Add(Vector3.Transform(new Vector3((i & 1) != 0 ? hi.X : lo.X, (i & 2) != 0 ? hi.Y : lo.Y, (i & 4) != 0 ? hi.Z : lo.Z), world));

        if (margin > 0)
        {
            var centre = corners.Aggregate(Vector3.Zero, (s, c) => s + c) / 8;
            for (int i = 0; i < 8; i++)
            {
                var away = corners[i] - centre;
                if (away.LengthSquared() > 1e-12f)
                    corners[i] += Vector3.Normalize(away) * margin;
            }
        }

        return corners;
    }

    /// <summary>The twelve triangles (three points each) of a box from its eight corners, numbered as BlockerCorners does.</summary>
    private static List<Vector3> BoxTriangles(List<Vector3> c)
    {
        var quads = new[] { (0, 1, 3, 2), (4, 6, 7, 5), (0, 4, 5, 1), (2, 3, 7, 6), (0, 2, 6, 4), (1, 5, 7, 3) };
        var triangles = new List<Vector3>(36);
        foreach (var (a, b, d, e) in quads)
        {
            triangles.Add(c[a]); triangles.Add(c[b]); triangles.Add(c[d]);
            triangles.Add(c[a]); triangles.Add(c[d]); triangles.Add(c[e]);
        }
        return triangles;
    }

    /// <summary>A part as a source of collision: a map collision part or an asset, else null.</summary>
    private static GenerateSource SourceOf(MsbEntity part)
    {
        var model = part.GetProperty("ModelName")?.GetValue(part.WrappedObject) as string;
        var mapId = part.MapID;
        if (string.IsNullOrEmpty(model) || string.IsNullOrEmpty(mapId))
            return null;

        if (EntityHelper.IsPartCollision(part))
            return new GenerateSource(part, mapId, false, model, LOC.Get("MAP_Navigation_Generate_Source_Collision", part.Name, model, mapId));
        if (EntityHelper.IsPartAsset(part))
            return new GenerateSource(part, mapId, true, model, LOC.Get("MAP_Navigation_Generate_Source_Asset", part.Name, model, mapId));
        return null;
    }

    private bool IsBlocker(GenerateSource source) => BlockerTags.Contains((source.MapId, source.Part.Name));

    /// <summary>The tagged blockers in the loaded maps, as sources.</summary>
    private List<GenerateSource> TaggedBlockers()
    {
        var result = new List<GenerateSource>();
        foreach (var loaded in LoadedMaps())
            foreach (var entity in loaded.Objects)
                if (entity is MsbEntity part && BlockerTags.Contains((part.MapID, part.Name)) && SourceOf(part) is GenerateSource source)
                    result.Add(source);
        return result;
    }

    /// <summary>
    /// The triangles of a source in its own space: a collision part's high collision file of its map (h + map digits +
    /// the model's digits), or an asset's own collision (high, else low), read from its binder. Null if there is none.
    /// </summary>
    private List<Vector3> SourceTriangles(GenerateSource source)
    {
        if (!source.IsAsset)
        {
            var fileName = $"h{source.MapId.Substring(1)}_{source.Model.Substring(1)}.hkx.dcx";
            var document = View.HavokCollisionBank.GetMapDocument(fileName);
            return document == null ? null : Havok.Shared.CollisionGeometry.Submeshes(document.Objects).SelectMany(t => t).ToList();
        }

        if (AssetCollisionCache.TryGetValue(source.Model, out var cached))
            return cached;

        List<Vector3> triangles = null;
        try
        {
            var descriptor = Renderer.ModelLocator.GetMapObjCollision(Project, source.Model, HavokCollisionType.High);
            var path = descriptor.GetContainerPath(Project);

            if (path != null && Project.VFS.FS.FileExists(path))
            {
                var binder = SoulsFormats.BND4.Read(Project.VFS.FS.ReadFile(path).Value);
                var file = binder.Files.FirstOrDefault(f => string.Equals(
                    System.IO.Path.GetFileName(f.Name).Split('.')[0], descriptor.AssetName, StringComparison.OrdinalIgnoreCase));

                if (file != null)
                {
                    var bytes = file.Bytes;
                    var data = SoulsFormats.DCX.Is(bytes) ? SoulsFormats.DCX.Decompress(bytes) : bytes;
                    var profile = Havok.HavokProfile.FromGame(Project.Descriptor.ProjectType.ToString());
                    var document = Havok.Files.HavokFile.Read(data.ToArray(), profile);
                    triangles = Havok.Shared.CollisionGeometry.Submeshes(document.Objects).SelectMany(t => t).ToList();
                }
            }
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MAP_Navigation_Generate_Asset_Failed", source.Model), e);
        }

        AssetCollisionCache[source.Model] = triangles;
        return triangles;
    }

    /// <summary>The model number of a collision file or part model name, e.g. 1000 for h10_01_00_00_001000.hkx.dcx or h001000, or -1.</summary>
    private static int CollisionModelId(string name)
    {
        // The digits after the last underscore of a file name, or after the type letter of a part model name
        var stem = name.Split('.')[0];
        var digits = stem.Contains('_') ? stem.Substring(stem.LastIndexOf('_') + 1) : stem.TrimStart('h', 'l', 'f', 'H', 'L', 'F');
        return int.TryParse(digits, out var id) ? id : -1;
    }

    /// <summary>
    /// Reads the selected parts' collision here (the collision bank isn't safe to read from another thread), placed
    /// where each part is, then generates both sets and builds their files in the background, and makes the edit when
    /// that is done.
    /// </summary>
    private void StartGenerate(MapContainer map, NavigationMap navigation, List<GenerateSource> sources, List<GenerateSource> blockerSources, int preferredId, NavmeshModel target, ModelTarget kind, int newModelId)
    {
        ModelMessage = null;
        bool intoSlot = kind == ModelTarget.Slot;

        if (target == null && kind != ModelTarget.New)
            return;

        var template = NavGenFile.FindTemplate(navigation);
        if (template == null)
        {
            ModelMessage = LOC.Get("MAP_Navigation_Generate_Error_Template");
            ModelMessageIsError = true;
            return;
        }

        // Each part's world transform, taken back out of this map's placement: navmesh vertices are relative to it
        if (!Matrix4x4.Invert(MapWorld(map), out var toNavmeshSpace))
            toNavmeshSpace = Matrix4x4.Identity;

        var triangles = new List<Vector3>();
        var missing = new List<string>();
        foreach (var source in sources)
        {
            var local = SourceTriangles(source);
            if (local == null || local.Count == 0)
            {
                missing.Add(source.Model);
                continue;
            }

            var placement = source.Part.GetWorldMatrix() * toNavmeshSpace;
            foreach (var point in local)
                triangles.Add(Vector3.Transform(point, placement));
        }

        // Blockers, one list a part, placed the same way
        var blockers = new List<NavBlocker>();
        foreach (var source in blockerSources)
        {
            var corners = BlockerCorners(source, toNavmeshSpace, 0);
            if (corners == null)
            {
                missing.Add(source.Model);
                continue;
            }

            blockers.Add(NavBlocker.FromCorners(corners));
        }

        if (missing.Count > 0)
            Smithbox.Log(this, LOC.Get("MAP_Navigation_Generate_Missing", string.Join(", ", missing)));

        int? modelId = null;
        if (intoSlot && GenerateNameAfterCollision && preferredId >= 0)
            modelId = preferredId;

        var pn = GenerateN;
        var po = GenerateO;
        // The per-face values of the generated meshes come from the target's meshes, or any model's for a new one
        var templateModel = target ?? navigation.Models.FirstOrDefault(m => m.NFile?.FaceCount > 0 && m.OFile?.FaceCount > 0);
        var nTemplate = templateModel?.NFile?.Mesh;
        var oTemplate = templateModel?.OFile?.Mesh;
        Generating = true;

        Task.Run(() =>
        {
            NavGenResult n = null, o = null;
            Havok.HavokDocument nDoc = null, oDoc = null;
            string error = null;

            try
            {
                n = NavigationGenerator.Generate(triangles, pn, nTemplate, blockers);
                o = NavigationGenerator.Generate(triangles, po, oTemplate, blockers);
                error = n.Error ?? o.Error;

                if (error == null)
                {
                    nDoc = NavGenFile.Build(template, n.Mesh);
                    oDoc = NavGenFile.Build(template, o.Mesh);
                    if (nDoc == null || oDoc == null)
                        error = "MAP_Navigation_Generate_Error_Template";
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("MAP_Navigation_Generate_Failed"), e);
                error = "MAP_Navigation_Generate_Error_Recast";
            }

            GenerateResults.Enqueue(() =>
            {
                Generating = false;

                if (error != null)
                {
                    ModelMessage = LOC.Get(error);
                    ModelMessageIsError = true;
                    return;
                }

                // The map may have changed while the generation ran; the edit is made against it as it is now
                var current = Bank.GetMap(map);
                int faces = n.Mesh.m_faces.Count, oFaces = o.Mesh.m_faces.Count;

                if (kind == ModelTarget.New)
                {
                    if (current != null)
                        EditModel(map, current, () => NavModelEdit.Add(current, newModelId, nDoc, oDoc),
                            LOC.Get("MAP_Navigation_Generate_Done", faces, oFaces, newModelId, triangles.Count / 3));
                    return;
                }

                var currentTarget = current?.Models.FirstOrDefault(m => ReferenceEquals(m.NEntry, target.NEntry));
                if (currentTarget == null)
                {
                    ModelMessage = LOC.Get("MAP_Navigation_Model_Error_Slot");
                    ModelMessageIsError = true;
                    return;
                }

                EditModel(map, current, () => NavModelEdit.Generate(current, currentTarget, nDoc, oDoc, modelId),
                    LOC.Get("MAP_Navigation_Generate_Done", faces, oFaces, modelId ?? currentTarget.ModelID, triangles.Count / 3));
            });
        });
    }
}
