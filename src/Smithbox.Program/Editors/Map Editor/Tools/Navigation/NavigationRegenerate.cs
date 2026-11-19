using Havok.Shared;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// What a whole-map regeneration is built from: the walkable geometry with, per triangle, the collision model it came
/// from and its collision material, the points paths start from, and the options. Navigation Full Regeneration
/// Analysis.md, steps 1-5.
/// </summary>
public sealed class NavRegenInput
{
    /// <summary>Triangles in map space, three points each: the map's low collision and the placed assets' low collision.</summary>
    public List<Vector3> Triangles { get; } = new();

    /// <summary>Per triangle: the ModelID of the collision model it came from (l…_001000 → 1000), or -1 for an asset.</summary>
    public List<int> Sources { get; } = new();

    /// <summary>Per triangle: its collision material (a HitMtrlParam row plus a property in the hundreds), or uint.MaxValue.</summary>
    public List<uint> Materials { get; } = new();

    /// <summary>Points in map space to keep the navmesh reachable from: enemies, players and the like.</summary>
    public List<Vector3> Seeds { get; } = new();

    public NavGenParameters N { get; set; } = NavGenParameters.DefaultN with { Links = true };

    public NavGenParameters O { get; set; } = NavGenParameters.DefaultO;

    /// <summary>Also seed from the faces of the map's navmesh as it is now, so a vanilla map keeps what its navmesh covers.</summary>
    public bool SeedFromCurrent { get; set; } = true;

    /// <summary>Give each new face the face data of the current face under its centre, where there is one.</summary>
    public bool KeepFaceData { get; set; } = true;

    /// <summary>Derive face data from the collision material under each face: Lava 0x1000 (surface 7), 0x20000 (surfaces 43 and 46).</summary>
    public bool DeriveFaceData { get; set; } = true;

    /// <summary>
    /// Put each new face in the model of the current face under it, where there is one, so a vanilla map keeps its own split
    /// into models; elsewhere the collision model under the face decides.
    /// </summary>
    public bool KeepModels { get; set; } = true;

    /// <summary>
    /// Count the generated jump and fall links when finding what the seeds reach. Off by default: two-way jumps reach up onto
    /// ledges and roofs the game's navmesh leaves out (Roundtable Hold doubled its area); the links are still made.
    /// </summary>
    public bool ReachOverLinks { get; set; }

    /// <summary>
    /// Carry over the current navmesh's jump and fall links whose two ends land on kept new faces, with their edge data
    /// and direction; generated links within 2 m of a kept one at both ends are dropped as the same link. The generated
    /// links find only part of the game's, so a vanilla map keeps its own this way.
    /// </summary>
    public bool KeepLinks { get; set; } = true;

    /// <summary>Join the new navmeshes to the loaded neighbours through their NVC files (open edges only).</summary>
    public bool StitchNeighbours { get; set; } = true;

    /// <summary>
    /// Pull the new border back onto the edges the current navmesh joins to other maps (NavGenParameters.Borders), as
    /// Recast keeps it the agent radius and more in from where the collision ends, too far for the re-stitch.
    /// </summary>
    public bool SnapBorders { get; set; } = true;

    public void Add(IReadOnlyList<Vector3> triangles, int source, IReadOnlyList<uint> materials = null)
    {
        for (int t = 0; t + 2 < triangles.Count; t += 3)
        {
            Triangles.Add(triangles[t]);
            Triangles.Add(triangles[t + 1]);
            Triangles.Add(triangles[t + 2]);
            Sources.Add(source);
            Materials.Add(materials != null && t / 3 < materials.Count ? materials[t / 3] : uint.MaxValue);
        }
    }
}

/// <summary>What a regeneration made of one set.</summary>
public sealed class NavRegenSetReport
{
    public int Generated { get; set; }
    public int Kept { get; set; }
    public int SeedsPlaced { get; set; }
    public int Seeds { get; set; }
    public int Models { get; set; }
    public int UnderAssetsOnly { get; set; }
    public int Links { get; set; }
    public int LinksAcrossModels { get; set; }
    public int LandingFacesMoved { get; set; }
    public int LinksKept { get; set; }
    public int LinksGenerated { get; set; }
    public List<string> LinkErrors { get; } = new();
    public int LinksNoFace { get; set; }
    public int LinksSameFace { get; set; }
    public int LinksPruned { get; set; }
    public int CutEdges { get; set; }
    public int FaceDataKept { get; set; }
    public int FaceDataDerived { get; set; }

    /// <summary>The edges joined to other maps before, used as border lines, and the new vertices moved onto them.</summary>
    public int Borders { get; set; }
    public int BorderVertices { get; set; }
}

/// <summary>The outcome of a regeneration: the file changes, the NVA and NVC changes, and what was made.</summary>
public sealed class NavRegenResult
{
    /// <summary>Null when it worked; else the name of the text that says why not.</summary>
    public string Error { get; set; }

    public List<NavFileChange> Files { get; } = new();

    public NavRemapResult Changes { get; } = new() { NvaChanged = true };

    public Dictionary<NavmeshSet, NavRegenSetReport> Sets { get; } = new();

    /// <summary>Each set's whole generated mesh before pruning and the split, with whether each face was kept: for checks.</summary>
    public Dictionary<NavmeshSet, (IHavokNavMeshEdit Mesh, HashSet<int> Kept)> Whole { get; } = new();

    public int Replaced { get; set; }
    public int Added { get; set; }
    public int Removed { get; set; }
    public List<int> Skipped { get; } = new();
    public int Connections { get; set; }
    public int Gates { get; set; }
    public int NodeBankFacesKept { get; set; }
    public int NodeBankFacesDropped { get; set; }
    public int CrossMapConnections { get; set; }

    /// <summary>Maps whose connections with this one were removed and not made again, as they weren't loaded.</summary>
    public List<string> NeighboursNotLoaded { get; } = new();

    /// <summary>The map as it is after the regeneration (the NVA and NVC objects are the input map's, edited in place).</summary>
    public NavigationMap Map { get; set; }

    public bool Ok => Error == null;
}

/// <summary>
/// A whole-map regeneration of the navigation data (Navigation Full Regeneration Analysis.md, steps 1-5):
/// 1. one generation of each set over all the walkable geometry;
/// 2. pruned to the faces reachable, over edges and jump and fall links, from the seeds;
/// 3. each face labelled with the collision model under it (through assets to the collision below them), and the mesh split
///    into one navmesh per collision model, the cut edges kept as exact NVA connections;
/// 4. face data derived from the collision material and/or taken from the current face under each new face;
/// 5. the NVA rebuilt through NavModelEdit (models replaced, added, made placeholders), the in-map connections from the
///    split, the gates rebuilt, the node bank faces moved to the new face at their old place, and the NVC files re-stitched.
/// The NVA and NVC objects of the map are edited in place; the caller takes a NavModelState before and after for undo,
/// and puts the result's files in the navmesh bank. Maps whose models are split into quarter files are refused.
/// </summary>
public static class NavRegenerate
{
    /// <summary>A face that a split cut off from its neighbour: (set, model, face, edge) on both sides, in the models' numbering.</summary>
    internal readonly record struct Cut(int ModelA, int FaceA, int EdgeA, int ModelB, int FaceB, int EdgeB);

    /// <summary>What Prepare made, for Apply: each set's meshes by model and the cut edges, and the file template.</summary>
    public sealed class Prepared
    {
        internal NavRegenResult Result { get; init; }
        internal Dictionary<NavmeshSet, Dictionary<int, IHavokNavMeshEdit>> Meshes { get; init; }
        internal Dictionary<NavmeshSet, List<Cut>> Cuts { get; init; }
        internal Havok.HavokDocument Template { get; init; }

        /// <summary>Null when it worked; else the name of the text that says why not.</summary>
        public string Error => Result.Error;

        public IReadOnlyDictionary<NavmeshSet, NavRegenSetReport> Sets => Result.Sets;
    }

    /// <summary>Prepare, then Apply.</summary>
    public static NavRegenResult Run(NavigationMap map, NavRegenInput input, Func<string, NavigationMap> neighbour = null)
    {
        var prepared = Prepare(map, input);
        return prepared.Error != null ? prepared.Result : Apply(map, prepared, input, neighbour);
    }

    /// <summary>
    /// Steps 1-4: generates, prunes, labels, sets face data, splits and links each set. Only reads the map, so it can run
    /// in the background; the map's data must not be edited meanwhile (Apply checks nothing).
    /// </summary>
    public static Prepared Prepare(NavigationMap map, NavRegenInput input)
    {
        var result = new NavRegenResult();
        var profile = map.Profile;
        var meshes = new Dictionary<NavmeshSet, Dictionary<int, IHavokNavMeshEdit>>();
        var cuts = new Dictionary<NavmeshSet, List<Cut>>();
        Havok.HavokDocument template = null;
        Prepared Done() => new() { Result = result, Meshes = meshes, Cuts = cuts, Template = template };

        if (map.Models.Any(m => m.Quarters.Count > 0))
        {
            result.Error = "MAP_Navigation_Regenerate_Error_Quarters";
            return Done();
        }

        template = NavGenFile.FindTemplate(map);
        var templateMesh = template?.ObjectsOf<IHavokNavMeshEdit>().FirstOrDefault();
        if (template == null || templateMesh == null)
        {
            result.Error = "MAP_Navigation_Regenerate_Error_Template";
            return Done();
        }

        if (input.Triangles.Count < 3)
        {
            result.Error = "MAP_Navigation_Regenerate_Error_Input";
            return Done();
        }

        var geometry = new TriangleGrid(input.Triangles, 4f);

        // 1-4, per set: the models' meshes and the cut edges between them
        foreach (var set in profile.Sets)
        {
            var p = set == NavmeshSet.N ? input.N : input.O;
            var report = new NavRegenSetReport();
            result.Sets[set] = report;

            // The edges joined to other maps now: the new mesh's border is pulled back onto them, so the re-stitch finds them
            var borders = input.SnapBorders ? CurrentBorders(map, set) : new List<(Vector3, Vector3)>();
            report.Borders = borders.Count;

            var generated = NavigationGenerator.Generate(input.Triangles, p with { Links = false, Borders = borders }, templateMesh);
            if (!generated.Ok)
            {
                result.Error = generated.Error;
                return Done();
            }

            var whole = generated.Mesh;
            report.Generated = whole.m_faces.Count;
            report.BorderVertices = generated.BorderVertices;

            // Links on the whole mesh, for reachability, and later kept within each model
            var links = p.Links ? NavLinkGenerator.Find(whole, input.Triangles, p).Links : new List<NavUserEdgeLink>();

            // Seeds: the given points and, for a vanilla map, the current navmesh's faces
            var current = CurrentFaces(map, set);
            var seeds = input.Seeds.ToList();
            if (input.SeedFromCurrent)
                for (int i = 0; i < current.Count; i += 10)
                    seeds.Add(current[i].Centre + new Vector3(0, 0.3f, 0));
            report.Seeds = seeds.Count;

            var wholeFaces = new FaceGrid();
            for (int f = 0; f < whole.m_faces.Count; f++)
                wholeFaces.Add(NavigationOverlayBuilder.FaceCorners(whole, f), f);

            var reach = Reachable(whole, input.ReachOverLinks ? links : new List<NavUserEdgeLink>(), seeds, wholeFaces, out int placed);
            report.SeedsPlaced = placed;
            report.Kept = reach.Count;
            result.Whole[set] = (whole, reach);

            // Labels: the current model, or the collision model, under each kept face
            var currentGrid = new FaceGrid();
            for (int i = 0; i < current.Count; i++)
                currentGrid.Add(current[i].Corners, i);

            var label = Label(whole, reach, geometry, input,
                input.KeepModels ? (Func<Vector3, int?>)(c => currentGrid.At(c, 1f) is int old ? current[old].ModelID : null) : null, out int assetsOnly);
            report.UnderAssetsOnly = assetsOnly;

            for (int f = 0; f < whole.m_faces.Count; f++)
            {
                if (label[f] < 0)
                    continue;

                var centre = NavigationOverlayBuilder.FaceCentre(whole, f);
                uint data = 0;

                if (input.DeriveFaceData && geometry.Below(centre, 1.5f) is int t && input.Materials[t] != uint.MaxValue)
                {
                    uint surface = input.Materials[t] % 100;
                    if (surface == 7) data |= 0x1000;
                    if (surface is 43 or 46) data |= 0x20000;
                    if (data != 0) report.FaceDataDerived++;
                }

                if (input.KeepFaceData && currentGrid.At(centre, 1f) is int old)
                {
                    data = current[old].Data;
                    report.FaceDataKept++;
                }

                whole.m_faceData[f * whole.m_faceDataStriding] = (int)data;
            }

            // The current links that land on kept faces at both ends; generated ones that repeat them go
            report.LinksGenerated = links.Count;
            if (input.KeepLinks)
            {
                var keptLinks = new List<NavUserEdgeLink>();
                foreach (var (from, to, both, data) in CurrentLinks(map, set))
                {
                    var ff = NearestFace(wholeFaces, whole, from);
                    var tf = NearestFace(wholeFaces, whole, to);
                    if (ff is not int f0 || tf is not int t0) { report.LinksNoFace++; continue; }
                    if (f0 == t0) { report.LinksSameFace++; continue; }
                    if (label[f0] < 0 || label[t0] < 0) { report.LinksPruned++; continue; }
                    keptLinks.Add(new NavUserEdgeLink(f0, IntoFace(whole, f0, from), t0, IntoFace(whole, t0, to), both, data, NavUserEdges.DefaultClearance));
                }

                bool Same(NavUserEdgeLink a, NavUserEdgeLink b)
                    => (Vector3.Distance(a.FromPoint, b.FromPoint) < 2 && Vector3.Distance(a.ToPoint, b.ToPoint) < 2)
                    || (a.Bidirectional && b.Bidirectional && Vector3.Distance(a.FromPoint, b.ToPoint) < 2 && Vector3.Distance(a.ToPoint, b.FromPoint) < 2);

                // Kept links as the game has them (several may join the same two new faces, at different places); a generated
                // link only where no kept one is the same link or joins the same two faces
                var keptPairs = keptLinks.Select(l => (Math.Min(l.FromFace, l.ToFace), Math.Max(l.FromFace, l.ToFace))).ToHashSet();
                var merged = new List<NavUserEdgeLink>(keptLinks);
                merged.AddRange(links.Where(g => !keptPairs.Contains((Math.Min(g.FromFace, g.ToFace), Math.Max(g.FromFace, g.ToFace))) && !keptLinks.Any(k => Same(k, g))));

                report.LinksKept = merged.Count(l => keptLinks.Contains(l));
                links = merged;
            }

            // A link between two models: the game's links stay within their navmesh, so the face it lands on (each end, for a
            // jump) goes to the model it starts from. The split joins that face to its old neighbours with exact connections.
            // A face is moved once; a later link across models on it is dropped
            var moved = new HashSet<int>();
            foreach (var l in links)
            {
                if (label[l.FromFace] < 0 || label[l.ToFace] < 0 || label[l.FromFace] == label[l.ToFace] || moved.Contains(l.FromFace) || !moved.Add(l.ToFace))
                    continue;
                label[l.ToFace] = label[l.FromFace];
                report.LandingFacesMoved++;
            }

            // Split, then each model's own links
            var split = Split(whole, label, out var setCuts);
            cuts[set] = setCuts;
            report.CutEdges = setCuts.Count;
            report.Models = split.Count;

            foreach (var (model, part) in split)
            {
                var own = links.Where(l => label[l.FromFace] == model && label[l.ToFace] == model)
                    .Select(l => l with { FromFace = part.NewFace[l.FromFace], ToFace = part.NewFace[l.ToFace] }).ToList();
                report.LinksAcrossModels += links.Count(l => label[l.FromFace] == model && label[l.ToFace] >= 0 && label[l.ToFace] != model);
                if (own.Count == 0)
                    continue;
                var added = NavUserEdges.AddAll(part.Mesh, own);
                if (added.Ok)
                    report.Links += own.Count;
                else
                    report.LinkErrors.Add($"{model}: {added.Error} ({own.Count} links)");
            }

            meshes[set] = split.ToDictionary(x => x.Key, x => x.Value.Mesh);
        }

        return Done();
    }

    /// <summary>
    /// Step 5: rebuilds the map's NVA from a Prepare of it, and re-stitches its NVC files with the loaded neighbours. Edits
    /// the map's NVA and NVC objects in place; the result lists the file changes.
    /// </summary>
    public static NavRegenResult Apply(NavigationMap map, Prepared prepared, NavRegenInput input, Func<string, NavigationMap> neighbour = null)
    {
        var result = prepared.Result;
        var profile = map.Profile;
        var meshes = prepared.Meshes;
        var cuts = prepared.Cuts;
        var template = prepared.Template;

        // 5. The NVA. The node bank faces' places first, as the old meshes go
        var bankPlaces = map.Nva.NodeBanks.Select(b => (Bank: b, Places: b.Faces.Select(face =>
        {
            var entry = map.FindNavmesh(face.CollisionID);
            var mesh = entry == null ? null : map.FileOf(entry)?.Mesh;
            return mesh == null || face.FaceIndex < 0 || face.FaceIndex >= mesh.m_faces.Count ? (Vector3?)null
                : Vector3.Transform(NavigationOverlayBuilder.FaceCentre(mesh, face.FaceIndex), NavigationMap.EntryToMap(entry));
        }).ToList())).ToList();

        // Every connection and gate of this map's navmeshes goes: the split gives the in-map ones again, exactly
        var names = map.Nva.Navmeshes.Select(e => e.NameID).ToHashSet();
        map.Nva.Connectors.Clear();
        foreach (var nvc in map.Nvcs)
        {
            bool isA = nvc.MapA == map.MapId, isB = nvc.MapB == map.MapId;
            if (nvc.Nvc.Connectors.RemoveAll(c => (isA && names.Contains(c.MainNameID)) || (isB && names.Contains(c.TargetNameID))) > 0)
            {
                result.Changes.NvcsChanged.Add(nvc);

                // The re-stitch below joins only loaded neighbours: this one's connections are gone until it is stitched
                var other = nvc.Opposite(map.MapId);
                if (other != map.MapId && (!input.StitchNeighbours || neighbour?.Invoke(other)?.Nva == null) && !result.NeighboursNotLoaded.Contains(other))
                    result.NeighboursNotLoaded.Add(other);
            }
        }
        foreach (var entry in map.Nva.Navmeshes)
            entry.GateNodes.Clear();

        var ids = meshes.Values.SelectMany(m => m.Keys).Distinct().OrderBy(id => id).ToList();
        var files = new Dictionary<string, Havok.HavokDocument>();
        var before = new Dictionary<string, Havok.HavokDocument>();

        void Collect(NavModelEditResult edit)
        {
            foreach (var change in edit.Files)
            {
                before.TryAdd(change.Name, change.Before);
                files[change.Name] = change.After;
            }
            foreach (var nvc in edit.Changes.NvcsChanged)
                result.Changes.NvcsChanged.Add(nvc);
        }

        Havok.HavokDocument Document(NavmeshSet set, int id)
            => meshes.TryGetValue(set, out var bySet) && bySet.TryGetValue(id, out var mesh)
                ? NavGenFile.Build(template, mesh) : NavModelEdit.EmptyCopy(template);

        var removable = NavModelEdit.Removable(map).ToHashSet();
        var slots = NavModelEdit.FreeSlots(map).ToHashSet();

        foreach (var model in map.Models.ToList())
        {
            bool usable = removable.Contains(model) || slots.Contains(model);

            if (ids.Contains(model.ModelID))
            {
                if (!usable)
                {
                    result.Skipped.Add(model.ModelID);
                    continue;
                }

                var edit = NavModelEdit.Generate(map, model, Document(NavmeshSet.N, model.ModelID), profile.HasOSet ? Document(NavmeshSet.O, model.ModelID) : null);
                if (!edit.Ok)
                {
                    result.Skipped.Add(model.ModelID);
                    continue;
                }
                Collect(edit);
                result.Replaced++;
            }
            else if (removable.Contains(model))
            {
                var edit = NavModelEdit.Remove(map, model);
                if (edit.Ok)
                {
                    Collect(edit);
                    result.Removed++;
                }
            }
        }

        var existing = map.Models.Select(m => m.ModelID).ToHashSet();
        foreach (var id in ids.Where(id => !existing.Contains(id)))
        {
            var edit = NavModelEdit.Add(map, id, Document(NavmeshSet.N, id), profile.HasOSet ? Document(NavmeshSet.O, id) : null);
            if (!edit.Ok)
            {
                result.Skipped.Add(id);
                continue;
            }
            Collect(edit);
            result.Added++;
        }

        foreach (var (name, after) in files)
            result.Files.Add(new NavFileChange(name, before[name], after));

        // The map with the new files
        var mapFiles = map.Files.ToDictionary(x => x.Key, x => x.Value);
        foreach (var (name, after) in files)
        {
            if (after == null) mapFiles.Remove(name);
            else mapFiles[name] = NavigationReader.FromDocument(name, after);
        }
        var regenerated = new NavigationMap(map.MapId, map.Nva, mapFiles, map.Nvcs, profile);
        result.Map = regenerated;

        // In-map connections from the cuts, both directions
        var plan = new NavStitchPlan { MapId = map.MapId };
        foreach (var (set, list) in cuts)
        {
            var byModel = regenerated.Models.Where(m => m.Entry(set) != null).GroupBy(m => m.ModelID).ToDictionary(g => g.Key, g => g.First().Entry(set));
            foreach (var cut in list)
            {
                if (!byModel.TryGetValue(cut.ModelA, out var a) || !byModel.TryGetValue(cut.ModelB, out var b))
                    continue;
                plan.Added.Add(new NavStitchPair(map.MapId, a.NameID, cut.FaceA, cut.EdgeA, map.MapId, b.NameID, cut.FaceB, cut.EdgeB));
            }
        }
        result.Connections = NavStitchPlanner.Apply(regenerated, plan, false, true).Changed;

        // Gates between the connected navmeshes
        var owners = regenerated.Nva.Navmeshes.Where(e => e.FaceCount > 0).ToList();
        result.Gates = NavGates.Rebuild(regenerated, owners, NavigationMap.EntryToMap).Added;

        // Node bank faces at their old places
        var newFaces = new FaceGrid();
        var newEntries = new List<(NVA.Navmesh Entry, int Face)>();
        foreach (var entry in NavigationOverlayBuilder.EntriesOf(regenerated, NavmeshSet.N))
        {
            var mesh = regenerated.FileOf(entry)?.Mesh;
            if (mesh == null) continue;
            var toMap = NavigationMap.EntryToMap(entry);
            for (int f = 0; f < mesh.m_faces.Count; f++)
            {
                newFaces.Add(NavigationOverlayBuilder.FaceCorners(mesh, f).Select(c => Vector3.Transform(c, toMap)).ToList(), newEntries.Count);
                newEntries.Add((entry, f));
            }
        }

        foreach (var (bank, places) in bankPlaces)
        {
            var faces = new List<NVA.NodeBankFace>();
            foreach (var place in places)
            {
                if (place is Vector3 at && newFaces.At(at, 1f) is int found)
                {
                    var (entry, face) = newEntries[found];
                    if (!faces.Any(x => x.CollisionID == entry.NameID && x.FaceIndex == face))
                        faces.Add(new NVA.NodeBankFace { FaceIndex = face, CollisionID = entry.NameID });
                    result.NodeBankFacesKept++;
                }
                else
                {
                    result.NodeBankFacesDropped++;
                }
            }
            bank.Faces = faces;
        }

        // Cross-map connections through the NVC files, with the loaded neighbours
        if (input.StitchNeighbours && neighbour != null)
        {
            foreach (var set in profile.Sets)
            {
                var stitch = NavStitchPlanner.Plan(regenerated, set, neighbour, new NavStitchParameters(), null, true);
                var crossOnly = new NavStitchPlan { MapId = map.MapId, Set = set };
                crossOnly.Added.AddRange(stitch.Added.Where(x => x.MapA != x.MapB));
                var applied = NavStitchPlanner.Apply(regenerated, crossOnly, false, true);
                result.CrossMapConnections += applied.Changed;
                foreach (var nvc in applied.NvcsChanged)
                    result.Changes.NvcsChanged.Add(nvc);
            }
        }

        NavModelEdit.RecomputeFaceDataIndex(map.Nva, profile);
        return result;
    }

    /// <summary>
    /// The current links of a set in map space: each user edge pair once (a two-way jump once, a one-way fall from its
    /// open end), as the middles of its two edges, with its edge data. Links naming another section are left out.
    /// </summary>
    private static List<(Vector3 From, Vector3 To, bool Bidirectional, int EdgeData)> CurrentLinks(NavigationMap map, NavmeshSet set)
    {
        var list = new List<(Vector3, Vector3, bool, int)>();
        foreach (var entry in NavigationOverlayBuilder.EntriesOf(map, set))
        {
            var mesh = map.FileOf(entry)?.Mesh;
            if (mesh == null)
                continue;

            var toMap = NavigationMap.EntryToMap(entry);
            Vector3 Middle(int k)
            {
                var a = mesh.m_vertices[mesh.m_edges[k].m_a];
                var b = mesh.m_vertices[mesh.m_edges[k].m_b];
                return Vector3.Transform(new Vector3((a.X + b.X) / 2, (a.Y + b.Y) / 2, (a.Z + b.Z) / 2), toMap);
            }

            int original = mesh.m_faces.Sum(f => f.m_numEdges);
            for (int k = original; k < mesh.m_edges.Count; k++)
            {
                var edge = mesh.m_edges[k];
                if ((edge.m_flags & HavokNavEdgeFlags.EDGE_BLOCKED) != 0 || edge.m_oppositeEdge == 0xFFFFFFFF || (edge.m_oppositeEdge >> 22) != 0
                    || edge.m_oppositeEdge >= mesh.m_edges.Count)
                    continue;

                int back = (int)edge.m_oppositeEdge;
                bool oneWay = (mesh.m_edges[back].m_flags & HavokNavEdgeFlags.EDGE_BLOCKED) != 0;
                if (!oneWay && back < k)
                    continue;

                int data = mesh.m_edgeDataStriding > 0 && k * mesh.m_edgeDataStriding < mesh.m_edgeData.Count ? mesh.m_edgeData[k * mesh.m_edgeDataStriding] : 0;
                list.Add((Middle(k), Middle(back), !oneWay, data));
            }
        }
        return list;
    }

    /// <summary>
    /// The edges of a set's navmeshes that the map's NVC files join to another map, in map space, each once: where the
    /// tile's navmesh meets its neighbours'. The map's edges are an NVC connection's main side when it is the file's first
    /// map, its opposite side when it is the second.
    /// </summary>
    private static List<(Vector3 A, Vector3 B)> CurrentBorders(NavigationMap map, NavmeshSet set)
    {
        var byName = NavigationOverlayBuilder.EntriesOf(map, set).GroupBy(e => e.NameID).ToDictionary(g => g.Key, g => g.First());
        var seen = new HashSet<(int, int)>();
        var list = new List<(Vector3, Vector3)>();

        void Add(int nameId, int edgeIndex)
        {
            if (!seen.Add((nameId, edgeIndex)) || !byName.TryGetValue(nameId, out var entry) || map.FileOf(entry)?.Mesh is not IHavokNavMeshEdit mesh
                || edgeIndex < 0 || edgeIndex >= mesh.m_edges.Count)
                return;

            var toMap = NavigationMap.EntryToMap(entry);
            var edge = mesh.m_edges[edgeIndex];
            if (edge.m_a < 0 || edge.m_a >= mesh.m_vertices.Count || edge.m_b < 0 || edge.m_b >= mesh.m_vertices.Count)
                return;
            var a = mesh.m_vertices[edge.m_a];
            var b = mesh.m_vertices[edge.m_b];
            list.Add((Vector3.Transform(new Vector3(a.X, a.Y, a.Z), toMap), Vector3.Transform(new Vector3(b.X, b.Y, b.Z), toMap)));
        }

        foreach (var nvc in map.Nvcs)
        {
            bool isA = nvc.MapA == map.MapId, isB = nvc.MapB == map.MapId;
            if (isA == isB)
                continue;

            foreach (var connector in nvc.Nvc.Connectors)
                foreach (var c in connector.NavMeshConnections)
                {
                    if (isA) Add(connector.MainNameID, c.EdgeIndex);
                    else Add(connector.TargetNameID, c.OppositeEdgeIndex);
                }
        }

        return list;
    }

    /// <summary>
    /// The face a link end lands on: the face under it, else (the generated mesh keeps back from a ledge, where links
    /// start) the nearest face within 1.5 m across and 1.5 m in height.
    /// </summary>
    private static int? NearestFace(FaceGrid faces, IHavokNavMeshEdit mesh, Vector3 point)
        => faces.Under(point + new Vector3(0, 0.3f, 0), 1.5f, 0.5f) ?? faces.Nearest(point, 1.5f, 1.5f);

    /// <summary>A link end moved into its face: unchanged when it is inside, else 0.3 m in from the nearest point of the face's outline.</summary>
    private static Vector3 IntoFace(IHavokNavMeshEdit mesh, int face, Vector3 point)
    {
        var corners = NavigationOverlayBuilder.FaceCorners(mesh, face);
        if (InsideXZ(corners, point))
            return point;

        var centre = corners.Aggregate(Vector3.Zero, (a, b) => a + b) / corners.Count;
        var best = corners[0];
        float bestD = float.MaxValue;
        for (int i = 0; i < corners.Count; i++)
        {
            var a = corners[i];
            var b = corners[(i + 1) % corners.Count];
            var ab = b - a;
            float t = Math.Clamp(Vector3.Dot(point - a, ab) / Math.Max(1e-9f, ab.LengthSquared()), 0, 1);
            var q = a + ab * t;
            float d = Vector3.DistanceSquared(q, point);
            if (d < bestD) { bestD = d; best = q; }
        }

        var inward = centre - best;
        float length = inward.Length();
        return length < 1e-4f ? centre : best + inward * MathF.Min(0.3f, length * 0.5f) / length;
    }

    /// <summary>The current faces of a set in map space, with their face data and their model.</summary>
    private static List<(List<Vector3> Corners, Vector3 Centre, uint Data, int ModelID)> CurrentFaces(NavigationMap map, NavmeshSet set)
    {
        var list = new List<(List<Vector3>, Vector3, uint, int)>();
        foreach (var entry in NavigationOverlayBuilder.EntriesOf(map, set))
        {
            var mesh = map.FileOf(entry)?.Mesh;
            if (mesh == null || mesh.m_faces.Count == 0) continue;
            var toMap = NavigationMap.EntryToMap(entry);
            for (int f = 0; f < mesh.m_faces.Count; f++)
            {
                var corners = NavigationOverlayBuilder.FaceCorners(mesh, f).Select(c => Vector3.Transform(c, toMap)).ToList();
                var centre = corners.Aggregate(Vector3.Zero, (a, b) => a + b) / corners.Count;
                uint data = mesh.m_faceDataStriding > 0 && f * mesh.m_faceDataStriding < mesh.m_faceData.Count ? (uint)mesh.m_faceData[f * mesh.m_faceDataStriding] : 0;
                list.Add((corners, centre, data, entry.ModelID));
            }
        }
        return list;
    }

    /// <summary>The faces reachable from the seeds over edges with a face on the other side and over the links.</summary>
    private static HashSet<int> Reachable(IHavokNavMeshEdit mesh, List<NavUserEdgeLink> links, List<Vector3> seeds, FaceGrid faces, out int placed)
    {
        var linked = new Dictionary<int, List<int>>();
        foreach (var l in links)
        {
            (linked.TryGetValue(l.FromFace, out var a) ? a : linked[l.FromFace] = new()).Add(l.ToFace);
            if (l.Bidirectional)
                (linked.TryGetValue(l.ToFace, out var b) ? b : linked[l.ToFace] = new()).Add(l.FromFace);
        }

        var seen = new HashSet<int>();
        var open = new Queue<int>();
        placed = 0;
        foreach (var s in seeds)
        {
            if (faces.Under(s, 2f, 0.5f) is not int f) continue;
            placed++;
            if (seen.Add(f)) open.Enqueue(f);
        }

        while (open.Count > 0)
        {
            int f = open.Dequeue();
            var face = mesh.m_faces[f];
            for (int e = face.m_startEdgeIndex; e < face.m_startEdgeIndex + face.m_numEdges; e++)
            {
                var edge = mesh.m_edges[e];
                if (edge.m_oppositeFace == 0xFFFFFFFF) continue;
                int o = (int)edge.m_oppositeFace;
                if (o >= 0 && o < mesh.m_faces.Count && seen.Add(o)) open.Enqueue(o);
            }
            if (linked.TryGetValue(f, out var targets))
                foreach (var o in targets)
                    if (seen.Add(o)) open.Enqueue(o);
        }
        return seen;
    }

    /// <summary>
    /// The model of each kept face (-1 for a face not kept): the current model under its centre, when given; else the
    /// source of the highest triangle under it; under an asset, the first map collision further down. Faces with none
    /// (over assets alone) take the label of a labelled face they are joined to, else the nearest labelled face's, else
    /// (no face labelled at all: maps whose map collision is only stubs) the nearest map collision triangle's model.
    /// </summary>
    private static int[] Label(IHavokNavMeshEdit mesh, HashSet<int> keep, TriangleGrid geometry, NavRegenInput input,
        Func<Vector3, int?> currentModel, out int assetsOnly)
    {
        var label = Enumerable.Repeat(-1, mesh.m_faces.Count).ToArray();
        var pending = new List<int>();

        foreach (var f in keep)
        {
            var centre = NavigationOverlayBuilder.FaceCentre(mesh, f);
            if (currentModel?.Invoke(centre) is int kept)
            {
                label[f] = kept;
                continue;
            }

            int source = -1;
            if (geometry.Below(centre, 3f) is int t)
            {
                source = input.Sources[t];
                if (source < 0 && geometry.BelowMatching(centre, 30f, i => input.Sources[i] >= 0) is int under)
                    source = input.Sources[under];
            }

            if (source >= 0) label[f] = source;
            else pending.Add(f);
        }

        assetsOnly = pending.Count;

        // Joined faces first, as far as they reach
        var open = new Queue<int>(keep.Where(f => label[f] >= 0));
        while (open.Count > 0)
        {
            int f = open.Dequeue();
            var face = mesh.m_faces[f];
            for (int e = face.m_startEdgeIndex; e < face.m_startEdgeIndex + face.m_numEdges; e++)
            {
                uint o = mesh.m_edges[e].m_oppositeFace;
                if (o == 0xFFFFFFFF || o >= mesh.m_faces.Count || label[o] >= 0 || !keep.Contains((int)o)) continue;
                label[o] = label[f];
                open.Enqueue((int)o);
            }
        }

        // Islands with no labelled face: the nearest labelled face's; with none labelled at all, the nearest map collision's
        var labelled = keep.Where(f => label[f] >= 0).Select(f => (Centre: NavigationOverlayBuilder.FaceCentre(mesh, f), Label: label[f])).ToList();
        if (labelled.Count == 0)
        {
            var parts = new List<(Vector3 Centre, int Label)>();
            for (int t = 0; t < input.Sources.Count; t += 16)
                if (input.Sources[t] >= 0)
                    parts.Add(((input.Triangles[t * 3] + input.Triangles[t * 3 + 1] + input.Triangles[t * 3 + 2]) / 3, input.Sources[t]));
            labelled = parts;
        }

        foreach (var f in pending.Where(f => label[f] < 0))
        {
            if (labelled.Count == 0) break;
            var centre = NavigationOverlayBuilder.FaceCentre(mesh, f);
            label[f] = labelled.MinBy(x => Vector3.DistanceSquared(x.Centre, centre)).Label;
        }

        return label;
    }

    private sealed record SplitPart(IHavokNavMeshEdit Mesh, Dictionary<int, int> NewFace);

    /// <summary>
    /// The mesh split into one mesh per label (faces labelled -1 are left out), each with its faces in their order, their
    /// edges consecutive, its own vertices, face and edge data. An edge whose face on the other side has another label
    /// becomes a boundary edge in both meshes, listed as a cut in both directions.
    /// </summary>
    private static Dictionary<int, SplitPart> Split(IHavokNavMeshEdit whole, int[] label, out List<Cut> cuts)
    {
        var parts = new Dictionary<int, SplitPart>();
        var newFace = new int[whole.m_faces.Count];
        var newEdge = new int[whole.m_edges.Count];
        var byLabel = new Dictionary<int, List<int>>();

        for (int f = 0; f < whole.m_faces.Count; f++)
            if (label[f] >= 0)
                (byLabel.TryGetValue(label[f], out var l) ? l : byLabel[label[f]] = new()).Add(f);

        // Numbering first, so edges across faces can be named
        foreach (var (_, faces) in byLabel)
        {
            int edges = 0;
            for (int i = 0; i < faces.Count; i++)
            {
                newFace[faces[i]] = i;
                var face = whole.m_faces[faces[i]];
                for (int k = 0; k < face.m_numEdges; k++)
                    newEdge[face.m_startEdgeIndex + k] = edges++;
            }
        }

        cuts = new List<Cut>();
        float wMin = whole.m_aabb.m_min.W, wMax = whole.m_aabb.m_max.W;

        foreach (var (model, faces) in byLabel)
        {
            var mesh = whole.CreateEmpty();
            mesh.m_faceDataStriding = 1;
            mesh.m_edgeDataStriding = 1;
            mesh.m_flags = whole.m_flags;
            mesh.m_erosionRadius = whole.m_erosionRadius;
            mesh.m_userData = whole.m_userData;

            var vertex = new Dictionary<int, int>();
            int Vertex(int v)
            {
                if (vertex.TryGetValue(v, out var n)) return n;
                vertex[v] = mesh.m_vertices.Count;
                mesh.m_vertices.Add(whole.m_vertices[v]);
                return mesh.m_vertices.Count - 1;
            }

            var map = new Dictionary<int, int>();
            foreach (var f in faces)
            {
                var face = whole.m_faces[f];
                map[f] = newFace[f];
                mesh.m_faces.Add(mesh.NewFace(mesh.m_edges.Count, face.m_numEdges, -1, 0, face.m_clusterIndex, face.m_padding));
                mesh.m_faceData.Add(whole.m_faceData[f * whole.m_faceDataStriding]);

                for (int e = face.m_startEdgeIndex; e < face.m_startEdgeIndex + face.m_numEdges; e++)
                {
                    var edge = whole.m_edges[e];
                    uint oppFace = 0xFFFFFFFF, oppEdge = 0xFFFFFFFF;
                    if (edge.m_oppositeFace != 0xFFFFFFFF && edge.m_oppositeFace < whole.m_faces.Count)
                    {
                        int o = (int)edge.m_oppositeFace;
                        if (label[o] == model)
                        {
                            oppFace = (uint)newFace[o];
                            oppEdge = (uint)newEdge[edge.m_oppositeEdge];
                        }
                        else if (label[o] >= 0)
                        {
                            cuts.Add(new Cut(model, newFace[f], newEdge[e], label[o], newFace[o], newEdge[edge.m_oppositeEdge]));
                        }
                    }

                    mesh.m_edges.Add(mesh.NewEdge(Vertex(edge.m_a), Vertex(edge.m_b), oppEdge, oppFace, edge.m_flags, edge.m_userEdgeCost));
                    mesh.m_edgeData.Add(whole.m_edgeDataStriding > 0 && e * whole.m_edgeDataStriding < whole.m_edgeData.Count ? whole.m_edgeData[e * whole.m_edgeDataStriding] : 0);
                }
            }

            var min = new Vector3(float.MaxValue);
            var max = new Vector3(float.MinValue);
            foreach (var v in mesh.m_vertices)
            {
                min = Vector3.Min(min, new Vector3(v.X, v.Y, v.Z));
                max = Vector3.Max(max, new Vector3(v.X, v.Y, v.Z));
            }
            mesh.m_aabb.Set(new Vector4(min, wMin), new Vector4(max, wMax));

            parts[model] = new SplitPart(mesh, map);
        }

        return parts;
    }

    /// <summary>Triangles on an XZ grid, for the triangle straight below a point.</summary>
    private sealed class TriangleGrid
    {
        private readonly List<Vector3> T;
        private readonly float Cell;
        private readonly Dictionary<(int, int), List<int>> Cells = new();

        public TriangleGrid(List<Vector3> triangles, float cell)
        {
            T = triangles;
            Cell = cell;
            for (int i = 0; i + 2 < T.Count; i += 3)
            {
                float x0 = MathF.Min(T[i].X, MathF.Min(T[i + 1].X, T[i + 2].X)), x1 = MathF.Max(T[i].X, MathF.Max(T[i + 1].X, T[i + 2].X));
                float z0 = MathF.Min(T[i].Z, MathF.Min(T[i + 1].Z, T[i + 2].Z)), z1 = MathF.Max(T[i].Z, MathF.Max(T[i + 1].Z, T[i + 2].Z));
                for (int x = (int)MathF.Floor(x0 / cell); x <= (int)MathF.Floor(x1 / cell); x++)
                    for (int z = (int)MathF.Floor(z0 / cell); z <= (int)MathF.Floor(z1 / cell); z++)
                    {
                        if (!Cells.TryGetValue((x, z), out var l)) Cells[(x, z)] = l = new();
                        l.Add(i / 3);
                    }
            }
        }

        /// <summary>The triangle (by index) highest below the point, at most a range down (and 0.5 m up), or null.</summary>
        public int? Below(Vector3 p, float range) => BelowMatching(p, range, _ => true);

        public int? BelowMatching(Vector3 p, float range, Func<int, bool> match)
        {
            if (!Cells.TryGetValue(((int)MathF.Floor(p.X / Cell), (int)MathF.Floor(p.Z / Cell)), out var list)) return null;
            int? best = null;
            float bestDrop = float.MaxValue;
            foreach (var t in list)
            {
                if (!match(t) || HeightAt(T[t * 3], T[t * 3 + 1], T[t * 3 + 2], p.X, p.Z) is not float y) continue;
                float drop = p.Y - y;
                if (drop > -0.5f && drop < range && drop < bestDrop)
                {
                    bestDrop = drop;
                    best = t;
                }
            }
            return best;
        }
    }

    /// <summary>Faces (as corner lists in map space) on an XZ grid, for the face at or below a point.</summary>
    private sealed class FaceGrid
    {
        private const float Cell = 4f;
        private readonly List<(List<Vector3> Corners, int Id)> Faces = new();
        private readonly Dictionary<(int, int), List<int>> Cells = new();

        public void Add(List<Vector3> corners, int id)
        {
            int index = Faces.Count;
            Faces.Add((corners, id));
            float x0 = corners.Min(c => c.X), x1 = corners.Max(c => c.X), z0 = corners.Min(c => c.Z), z1 = corners.Max(c => c.Z);
            for (int x = (int)MathF.Floor(x0 / Cell); x <= (int)MathF.Floor(x1 / Cell); x++)
                for (int z = (int)MathF.Floor(z0 / Cell); z <= (int)MathF.Floor(z1 / Cell); z++)
                {
                    if (!Cells.TryGetValue((x, z), out var l)) Cells[(x, z)] = l = new();
                    l.Add(index);
                }
        }

        /// <summary>The face nearest the point across (0 inside it) within a range, its height within a limit, or null.</summary>
        public int? Nearest(Vector3 p, float range, float height)
        {
            int? best = null;
            float bestD = range;
            for (int x = (int)MathF.Floor((p.X - range) / Cell); x <= (int)MathF.Floor((p.X + range) / Cell); x++)
                for (int z = (int)MathF.Floor((p.Z - range) / Cell); z <= (int)MathF.Floor((p.Z + range) / Cell); z++)
                {
                    if (!Cells.TryGetValue((x, z), out var list)) continue;
                    foreach (var i in list)
                    {
                        var c = Faces[i].Corners;
                        if (MathF.Abs(c.Average(q => q.Y) - p.Y) > height) continue;
                        float d = InsideXZ(c, p) ? 0 : DistanceXZ(c, p);
                        if (d < bestD) { bestD = d; best = Faces[i].Id; }
                    }
                }
            return best;
        }

        private static float DistanceXZ(List<Vector3> c, Vector3 p)
        {
            float best = float.MaxValue;
            var q = new Vector2(p.X, p.Z);
            for (int i = 0; i < c.Count; i++)
            {
                var a = new Vector2(c[i].X, c[i].Z);
                var b = new Vector2(c[(i + 1) % c.Count].X, c[(i + 1) % c.Count].Z);
                var ab = b - a;
                float t = Math.Clamp(Vector2.Dot(q - a, ab) / Math.Max(1e-9f, ab.LengthSquared()), 0, 1);
                best = MathF.Min(best, Vector2.Distance(q, a + ab * t));
            }
            return best;
        }

        /// <summary>The face whose plane is within a height of the point, under or over it, or null.</summary>
        public int? At(Vector3 p, float height) => Under(p, height, height);

        /// <summary>The face under the point (within below) or just over it (within above), nearest in height, or null.</summary>
        public int? Under(Vector3 p, float below, float above)
        {
            if (!Cells.TryGetValue(((int)MathF.Floor(p.X / Cell), (int)MathF.Floor(p.Z / Cell)), out var list)) return null;
            int? best = null;
            float bestD = float.MaxValue;
            foreach (var i in list)
            {
                var c = Faces[i].Corners;
                if (!InsideXZ(c, p)) continue;
                float y = HeightOnFace(c, p);
                float d = p.Y - y;
                if (d < -above || d > below || MathF.Abs(d) >= bestD) continue;
                bestD = MathF.Abs(d);
                best = Faces[i].Id;
            }
            return best;
        }

        private static float HeightOnFace(List<Vector3> c, Vector3 p)
        {
            for (int i = 1; i + 1 < c.Count; i++)
                if (HeightAt(c[0], c[i], c[i + 1], p.X, p.Z) is float y)
                    return y;
            return c.Average(q => q.Y);
        }
    }

    private static bool InsideXZ(List<Vector3> c, Vector3 p)
    {
        bool inside = false;
        for (int i = 0, j = c.Count - 1; i < c.Count; j = i++)
            if ((c[i].Z > p.Z) != (c[j].Z > p.Z) && p.X < (c[j].X - c[i].X) * (p.Z - c[i].Z) / (c[j].Z - c[i].Z) + c[i].X)
                inside = !inside;
        return inside;
    }

    private static float? HeightAt(Vector3 a, Vector3 b, Vector3 c, float x, float z)
    {
        float d = (b.Z - c.Z) * (a.X - c.X) + (c.X - b.X) * (a.Z - c.Z);
        if (MathF.Abs(d) < 1e-9f) return null;
        float u = ((b.Z - c.Z) * (x - c.X) + (c.X - b.X) * (z - c.Z)) / d;
        float v = ((c.Z - a.Z) * (x - c.X) + (a.X - c.X) * (z - c.Z)) / d;
        float w = 1 - u - v;
        if (u < -1e-4f || v < -1e-4f || w < -1e-4f) return null;
        return u * a.Y + v * b.Y + w * c.Y;
    }
}
