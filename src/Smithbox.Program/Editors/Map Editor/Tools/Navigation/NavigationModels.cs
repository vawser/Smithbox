using Havok.hk2018;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

// Removing a navmesh model and duplicating one (Navigation Editor Plan, Phase 3), without renumbering anything. Kept
// free of the rest of Smithbox; NavSurvey `modeledit` runs it on every map.
//
// What refers to a navmesh entry (NavSurvey modelsurvey):
// - by NameID: NVA and NVC connectors, and node bank faces (n set only);
// - by position in the NVA's list: gate nodes (ConnectedNavmeshIndex), which always point at a connected navmesh, and
//   whose gate there (NodeSubID) points back. Each gate's NeighbourGateNodeCosts is 16 entries: the cost to each of its
//   own navmesh's gates (0 for itself), then -1;
// - the NameID's index part is the n entry's position, so inserting or deleting entries would renumber both.
// The game's files have 4,900 placeholder entries (no faces, no gate nodes, Unk4C 0 or 1, the file empty or absent),
// in pairs across the two sets. So a removed model becomes a placeholder, and a duplicate fills a free placeholder pair:
// no NameID or position changes.

/// <summary>A navmesh file's document before and after a model edit; null when the binder doesn't have the file.</summary>
public sealed record NavFileChange(string Name, Havok.HavokDocument Before, Havok.HavokDocument After);

/// <summary>What a model edit did to the files, or why it was refused.</summary>
public sealed class NavModelEditResult
{
    /// <summary>Null when it worked; else the name of the text that says why not.</summary>
    public string Error { get; set; }

    public List<NavFileChange> Files { get; } = new();

    /// <summary>The NVA and NVC changes, for NavigationBank.MarkEdited.</summary>
    public NavRemapResult Changes { get; } = new() { NvaChanged = true };

    public bool Ok => Error == null;
}

/// <summary>
/// The parts of a map's NVA and NVC files a model edit changes, copied: each navmesh's ModelID, face count, face data
/// index, Unk4C and gate nodes, the node bank faces, and the connectors.
/// </summary>
public sealed class NavModelState
{
    private readonly List<(NVA_ER.Navmesh Entry, int ModelID, int FaceCount, int FaceDataIndex, int Unk4C, List<NVA_ER.GateNode> Gates)> Entries;
    private readonly List<(NVA_ER.NodeBank Bank, List<(int Face, int Collision)> Faces)> Banks;
    private readonly NavConnectionState Connections;

    public NavModelState(NavigationMap map)
    {
        Entries = map.Nva.Navmeshes.Select(e => (e, e.ModelID, e.FaceCount, e.FaceDataIndex, e.Unk4C, e.GateNodes.Select(Copy).ToList())).ToList();
        Banks = map.Nva.NodeBanks.Select(b => (b, b.Faces.Select(f => (f.FaceIndex, f.CollisionID)).ToList())).ToList();
        Connections = new NavConnectionState(map.Nva, map.Nvcs);
    }

    private static NVA_ER.GateNode Copy(NVA_ER.GateNode g) => new()
    {
        Position = g.Position,
        ConnectedNavmeshIndex = g.ConnectedNavmeshIndex,
        NodeSubID = g.NodeSubID,
        NeighbourGateNodeCosts = g.NeighbourGateNodeCosts.ToList(),
        Unk14 = g.Unk14,
    };

    /// <summary>Puts the values back as they were when copied, as new copies, so the state can be restored again.</summary>
    public void Restore()
    {
        foreach (var (entry, modelId, faceCount, faceDataIndex, unk4C, gates) in Entries)
        {
            entry.ModelID = modelId;
            entry.FaceCount = faceCount;
            entry.FaceDataIndex = faceDataIndex;
            entry.Unk4C = unk4C;
            entry.GateNodes = gates.Select(Copy).ToList();
        }

        foreach (var (bank, faces) in Banks)
            bank.Faces = faces.Select(f => new NVA_ER.NodeBankFace { FaceIndex = f.Face, CollisionID = f.Collision }).ToList();

        Connections.Restore();
    }
}

public static class NavModelEdit
{
    /// <summary>Models that can be removed: with faces, not split into quarters, and with files no other entry uses.</summary>
    public static IEnumerable<NavmeshModel> Removable(NavigationMap map)
        => map.Models.Where(m => !m.IsPlaceholder && m.Quarters.Count == 0 && m.NEntry != null && m.OEntry != null && !SharesFiles(map, m));

    /// <summary>
    /// Placeholder pairs a duplicate can fill: no faces in either set, an entry in each, no quarters or gate nodes, and
    /// file names no other entry uses (some maps list a ModelID twice, e.g. 443590 in m60_44_35_00).
    /// </summary>
    public static IEnumerable<NavmeshModel> FreeSlots(NavigationMap map)
        => map.Models.Where(m => m.IsPlaceholder && m.Quarters.Count == 0 && m.NEntry != null && m.OEntry != null
            && m.NEntry.GateNodes.Count == 0 && m.OEntry.GateNodes.Count == 0 && !SharesFiles(map, m));

    /// <summary>Whether another entry of the map names one of a model's files.</summary>
    private static bool SharesFiles(NavigationMap map, NavmeshModel model)
    {
        var names = new HashSet<string> { map.FileNameOf(model.NEntry), map.FileNameOf(model.OEntry) };
        return map.Nva.Navmeshes.Any(e => !ReferenceEquals(e, model.NEntry) && !ReferenceEquals(e, model.OEntry) && names.Contains(map.FileNameOf(e)));
    }

    /// <summary>
    /// Sets every entry's FaceDataIndex to the running total of the face counts before it, in list order, as the game's
    /// files have it.
    /// </summary>
    public static void RecomputeFaceDataIndex(NVA_ER nva)
    {
        int total = 0;
        foreach (var entry in nva.Navmeshes)
        {
            entry.FaceDataIndex = total;
            total += entry.FaceCount;
        }
    }

    /// <summary>
    /// Turns a model into a placeholder, as the game's own: both entries get no faces and Unk4C 1, and lose their gate
    /// nodes, as do the gates of other navmeshes that lead to them. Every connection naming either entry is dropped, in
    /// the NVA and in the NVC files on this map's side, with the node bank faces of its n entry. Its files become empty
    /// navmesh files (only an hkaiNavMesh). Nothing is renumbered.
    /// </summary>
    public static NavModelEditResult Remove(NavigationMap map, NavmeshModel model)
    {
        var result = new NavModelEditResult();

        if (model.Quarters.Count > 0 || model.NEntry == null || model.OEntry == null)
        {
            result.Error = "MAP_Navigation_Model_Error_Quarters";
            return result;
        }

        if (model.IsPlaceholder)
        {
            result.Error = "MAP_Navigation_Model_Error_Placeholder";
            return result;
        }

        var list = map.Nva.Navmeshes;
        var positions = new HashSet<int> { list.IndexOf(model.NEntry), list.IndexOf(model.OEntry) };
        var names = new HashSet<int> { model.NEntry.NameID, model.OEntry.NameID };

        RemoveGatesTo(list, positions);

        map.Nva.Connectors.RemoveAll(c => names.Contains(c.MainNameID) || names.Contains(c.TargetNameID));

        foreach (var nvc in map.Nvcs)
        {
            bool isA = nvc.MapA == map.MapId, isB = nvc.MapB == map.MapId;
            if (nvc.Nvc.Connectors.RemoveAll(c => (isA && names.Contains(c.MainNameID)) || (isB && names.Contains(c.TargetNameID))) > 0)
                result.Changes.NvcsChanged.Add(nvc);
        }

        foreach (var bank in map.Nva.NodeBanks)
            bank.Faces.RemoveAll(f => names.Contains(f.CollisionID));

        foreach (var entry in new[] { model.NEntry, model.OEntry })
        {
            entry.FaceCount = 0;
            entry.Unk4C = 1;
        }

        RecomputeFaceDataIndex(map.Nva);

        foreach (var file in new[] { model.NFile, model.OFile })
            if (file?.Document != null)
                result.Files.Add(new NavFileChange(file.Name, file.Document, EmptyCopy(file.Document)));

        return result;
    }

    /// <summary>
    /// Copies a model into a free placeholder pair, moved by an offset: the slot's n and o files become copies of the
    /// source's, and its entries get their face counts. The slot keeps its ModelID and NameIDs, so nothing is
    /// renumbered. The copy has no connections or gate nodes; re-stitching joins it to its neighbours.
    /// </summary>
    public static NavModelEditResult Duplicate(NavigationMap map, NavmeshModel source, NavmeshModel slot, Vector3 offset)
    {
        var result = new NavModelEditResult();

        if (source.Quarters.Count > 0 || source.NFile?.Document == null || source.OFile?.Document == null)
        {
            result.Error = "MAP_Navigation_Model_Error_Source";
            return result;
        }

        if (!FreeSlots(map).Contains(slot))
        {
            result.Error = "MAP_Navigation_Model_Error_Slot";
            return result;
        }

        foreach (var set in new[] { NavmeshSet.N, NavmeshSet.O })
        {
            var name = NavigationMap.MeshName(map.NameBase, set, slot.ModelID);
            var before = map.Files.GetValueOrDefault(name)?.Document;
            var copy = TranslatedCopy(source.File(set).Document, offset);

            result.Files.Add(new NavFileChange(name, before, copy));

            var entry = slot.Entry(set);
            entry.FaceCount = copy.ObjectsOf<hkaiNavMesh>().First().m_faces.Count;
            entry.Unk4C = 0;
        }

        RecomputeFaceDataIndex(map.Nva);
        return result;
    }

    /// <summary>
    /// Whether a ModelID can be given to a free slot: no other entry of the map has it.
    /// </summary>
    public static bool ModelIdFree(NavigationMap map, NavmeshModel slot, int modelId)
        => !map.Nva.Navmeshes.Any(e => e.ModelID == modelId && !ReferenceEquals(e, slot.NEntry) && !ReferenceEquals(e, slot.OEntry));

    /// <summary>
    /// Puts generated n and o files in a model. Two targets:
    /// - a free placeholder pair, as Duplicate does; with a ModelID, the pair takes it (the collision model's ID, as the
    ///   game's models have), if no other entry has it. NameIDs don't change;
    /// - a model with faces, whose navmesh is replaced: its connections and node bank faces name faces and edges of the
    ///   old mesh, so they are dropped (re-stitch to join it again). Its gate nodes are kept: they name navmeshes, not faces.
    /// </summary>
    public static NavModelEditResult Generate(NavigationMap map, NavmeshModel target, Havok.HavokDocument n, Havok.HavokDocument o, int? modelId = null)
    {
        var result = new NavModelEditResult();
        bool slot = FreeSlots(map).Contains(target);
        bool replace = Removable(map).Contains(target);

        if (n == null || o == null)
        {
            result.Error = "MAP_Navigation_Generate_Error_File";
            return result;
        }

        if (!slot && !replace)
        {
            result.Error = "MAP_Navigation_Model_Error_Slot";
            return result;
        }

        if (slot && modelId is int id && id != target.ModelID)
        {
            if (!ModelIdFree(map, target, id))
            {
                result.Error = "MAP_Navigation_Generate_Error_Model_Id";
                return result;
            }

            // The old files of the placeholder, if it had any, go: the pair is named after its new ModelID
            foreach (var set in new[] { NavmeshSet.N, NavmeshSet.O })
            {
                var oldName = NavigationMap.MeshName(map.NameBase, set, target.ModelID);
                if (map.Files.GetValueOrDefault(oldName)?.Document is Havok.HavokDocument old)
                    result.Files.Add(new NavFileChange(oldName, old, null));
            }

            target.NEntry.ModelID = id;
            target.OEntry.ModelID = id;
        }

        if (replace)
        {
            var names = new HashSet<int> { target.NEntry.NameID, target.OEntry.NameID };
            map.Nva.Connectors.RemoveAll(c => names.Contains(c.MainNameID) || names.Contains(c.TargetNameID));

            foreach (var nvc in map.Nvcs)
            {
                bool isA = nvc.MapA == map.MapId, isB = nvc.MapB == map.MapId;
                if (nvc.Nvc.Connectors.RemoveAll(c => (isA && names.Contains(c.MainNameID)) || (isB && names.Contains(c.TargetNameID))) > 0)
                    result.Changes.NvcsChanged.Add(nvc);
            }

            foreach (var bank in map.Nva.NodeBanks)
                bank.Faces.RemoveAll(f => names.Contains(f.CollisionID));
        }

        foreach (var (set, document) in new[] { (NavmeshSet.N, n), (NavmeshSet.O, o) })
        {
            var entry = target.Entry(set);
            var name = NavigationMap.MeshName(map.NameBase, set, entry.ModelID);
            result.Files.Add(new NavFileChange(name, map.Files.GetValueOrDefault(name)?.Document, document));

            entry.FaceCount = document.ObjectsOf<hkaiNavMesh>().First().m_faces.Count;
            entry.Unk4C = entry.FaceCount == 0 ? 1 : 0;
        }

        RecomputeFaceDataIndex(map.Nva);
        return result;
    }

    /// <summary>
    /// Removes the gate nodes that lead to the navmeshes at some positions, and those navmeshes' own. Taking a gate out
    /// of a navmesh's list also takes its column out of that navmesh's gate costs, and moves down the NodeSubID of
    /// gates elsewhere that point at a later gate of the same navmesh.
    /// </summary>
    public static void RemoveGatesTo(List<NVA_ER.Navmesh> list, ISet<int> positions)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (positions.Contains(i))
                continue;

            var gates = list[i].GateNodes;
            for (int g = gates.Count - 1; g >= 0; g--)
                if (positions.Contains(gates[g].ConnectedNavmeshIndex))
                    RemoveGate(list, i, g);
        }

        foreach (var p in positions)
            if (p >= 0 && p < list.Count)
                list[p].GateNodes.Clear();
    }

    private static void RemoveGate(List<NVA_ER.Navmesh> list, int owner, int gate)
    {
        var gates = list[owner].GateNodes;
        gates.RemoveAt(gate);

        // The costs are by the owner's gate index, padded with -1 to their length
        foreach (var g in gates)
        {
            var costs = g.NeighbourGateNodeCosts;
            if (gate < costs.Count)
            {
                costs.RemoveAt(gate);
                costs.Add(-1);
            }
        }

        foreach (var entry in list)
            foreach (var g in entry.GateNodes)
                if (g.ConnectedNavmeshIndex == owner)
                {
                    if (g.NodeSubID > gate)
                        g.NodeSubID--;
                    else if (g.NodeSubID == gate)
                        g.NodeSubID = -1;
                }
    }

    /// <summary>A copy of a document, independent of it, read back from its written bytes.</summary>
    public static Havok.HavokDocument Clone(Havok.HavokDocument document)
        => Havok.Files.HavokFile.Read(Havok.Files.HavokFile.Write(document), document.Profile);

    /// <summary>
    /// An empty navmesh file in the form the game's most common empty files have: the root holds only the hkaiNavMesh,
    /// with no faces, edges, vertices or clearance seeding. Built from a copy, then read back so its objects are listed.
    /// </summary>
    public static Havok.HavokDocument EmptyCopy(Havok.HavokDocument document)
    {
        var copy = Clone(document);
        var mesh = copy.ObjectsOf<hkaiNavMesh>().First();

        mesh.m_faces.Clear();
        mesh.m_edges.Clear();
        mesh.m_vertices.Clear();
        mesh.m_faceData.Clear();
        mesh.m_edgeData.Clear();
        mesh.m_streamingSets.Clear();
        mesh.m_clearanceCacheSeedingDataSet = null;

        if (copy.Root is hkRootLevelContainer root)
            root.m_namedVariants.RemoveAll(v => !ReferenceEquals(v.m_variant, mesh));

        return Clone(copy);
    }

    /// <summary>
    /// A copy of a navmesh file moved by an offset: its vertices, its bounds, the root box of its query mediator's tree
    /// (whose nodes are stored relative to their parents, so they follow), and the boxes of world-space user edge
    /// setups. The clearance cache holds distances, which a move doesn't change.
    /// </summary>
    public static Havok.HavokDocument TranslatedCopy(Havok.HavokDocument document, Vector3 offset)
    {
        var copy = Clone(document);

        if (offset == Vector3.Zero)
            return copy;

        var shift = new Vector4(offset, 0);

        foreach (var mesh in copy.ObjectsOf<hkaiNavMesh>())
        {
            for (int i = 0; i < mesh.m_vertices.Count; i++)
                mesh.m_vertices[i] += shift;

            mesh.m_aabb.m_min += shift;
            mesh.m_aabb.m_max += shift;
        }

        foreach (var mediator in copy.ObjectsOf<hkaiStaticTreeNavMeshQueryMediator>())
        {
            var domain = mediator.m_tree?.m_treePtr?.m_tree?.m_domain;
            if (domain == null)
                continue;
            domain.m_min += shift;
            domain.m_max += shift;
        }

        foreach (var setups in copy.ObjectsOf<hkaiUserEdgeSetupArray>())
        {
            foreach (var setup in setups.m_edgeSetups)
            {
                if (setup.m_space != hkaiUserEdgeUtils.UserEdgeSetupSpace.USER_EDGE_SPACE_WORLD)
                    continue;

                setup.m_obbA.m_transform = Translate(setup.m_obbA.m_transform, offset);
                setup.m_obbB.m_transform = Translate(setup.m_obbB.m_transform, offset);
            }
        }

        return copy;
    }

    private static Matrix4x4 Translate(Matrix4x4 m, Vector3 offset)
    {
        m.M41 += offset.X;
        m.M42 += offset.Y;
        m.M43 += offset.Z;
        return m;
    }
}
