using Havok.Shared;
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

    /// <summary>The n and o entries a model add made, or null.</summary>
    public (NVA.Navmesh N, NVA.Navmesh O)? Added { get; set; }

    public bool Ok => Error == null;
}

/// <summary>
/// The parts of a map's NVA and NVC files a model edit changes, copied: each navmesh's ModelID, face count, face data
/// index, Unk4C and gate nodes, the node bank faces, and the connectors.
/// </summary>
public sealed class NavModelState
{
    private readonly List<(NVA.Navmesh Entry, int ModelID, int FaceCount, int FaceDataIndex, int Unk4C, List<NVA.GateNode> Gates)> Entries;
    private readonly List<(NVA.NodeBank Bank, List<(int Face, int Collision)> Faces)> Banks;
    private readonly NVA Nva;
    private readonly List<NVA.Navmesh> Order;
    private readonly NavConnectionState Connections;

    public NavModelState(NavigationMap map)
    {
        Nva = map.Nva;
        Order = map.Nva.Navmeshes.ToList();
        Entries = map.Nva.Navmeshes.Select(e => (e, e.ModelID, e.FaceCount, e.FaceDataIndex, e.Unk4C, e.GateNodes.Select(Copy).ToList())).ToList();
        Banks = map.Nva.NodeBanks.Select(b => (b, b.Faces.Select(f => (f.FaceIndex, f.CollisionID)).ToList())).ToList();
        Connections = new NavConnectionState(map.Nva, map.Nvcs);
    }

    private static NVA.GateNode Copy(NVA.GateNode g) => new()
    {
        Position = g.Position,
        ConnectedNavmeshIndex = g.ConnectedNavmeshIndex,
        NodeSubID = g.NodeSubID,
        NeighbourGateNodeCosts = g.NeighbourGateNodeCosts.ToList(),
        Unk14 = g.Unk14,
        Unk18 = g.Unk18,
    };

    /// <summary>Puts the values back as they were when copied, as new copies, so the state can be restored again.</summary>
    public void Restore()
    {
        // The entries themselves, in their order: an added model's entries go, a removed one's come back
        Nva.Navmeshes.Clear();
        Nva.Navmeshes.AddRange(Order);

        foreach (var (entry, modelId, faceCount, faceDataIndex, unk4C, gates) in Entries)
        {
            entry.ModelID = modelId;
            entry.FaceCount = faceCount;
            entry.FaceDataIndex = faceDataIndex;
            entry.Unk4C = unk4C;
            entry.GateNodes = gates.Select(Copy).ToList();
        }

        foreach (var (bank, faces) in Banks)
            bank.Faces = faces.Select(f => new NVA.NodeBankFace { FaceIndex = f.Face, CollisionID = f.Collision }).ToList();

        Connections.Restore();
    }
}

public static class NavModelEdit
{
    /// <summary>
    /// Whether a model has an entry in every set the game has: n and o (ER, NR, DB_NT), or n alone (AC6).
    /// </summary>
    public static bool HasEveryEntry(NavigationMap map, NavmeshModel model)
        => model.NEntry != null && (model.OEntry != null || !map.Profile.HasOSet);

    /// <summary>A model's entries in the sets the game has.</summary>
    public static IEnumerable<NVA.Navmesh> EntriesOf(NavigationMap map, NavmeshModel model)
        => map.Profile.Sets.Select(model.Entry).Where(e => e != null);

    /// <summary>Models that can be removed: with faces, not split into quarters, and with files no other entry uses.</summary>
    public static IEnumerable<NavmeshModel> Removable(NavigationMap map)
        => map.Models.Where(m => !m.IsPlaceholder && m.Quarters.Count == 0 && HasEveryEntry(map, m) && !SharesFiles(map, m));

    /// <summary>
    /// Placeholders a duplicate can fill: no faces in any set, an entry in each, no quarters or gate nodes, and file
    /// names no other entry uses (some maps list a ModelID twice, e.g. 443590 in m60_44_35_00).
    /// </summary>
    public static IEnumerable<NavmeshModel> FreeSlots(NavigationMap map)
        => map.Models.Where(m => m.IsPlaceholder && m.Quarters.Count == 0 && HasEveryEntry(map, m)
            && EntriesOf(map, m).All(e => e.GateNodes.Count == 0) && !SharesFiles(map, m));

    /// <summary>Whether another entry of the map names one of a model's files.</summary>
    private static bool SharesFiles(NavigationMap map, NavmeshModel model)
    {
        var own = EntriesOf(map, model).ToList();
        var names = own.Select(map.FileNameOf).ToHashSet();
        return map.Nva.Navmeshes.Any(e => !own.Any(o => ReferenceEquals(o, e)) && names.Contains(map.FileNameOf(e)));
    }

    /// <summary>
    /// Sets every entry's FaceDataIndex to the running total of the face counts before it, in list order, as the game's
    /// files have it. Left as it is in a game where FaceDataIndex is something else (DB_NT: a value per model).
    /// </summary>
    public static void RecomputeFaceDataIndex(NVA nva, NavGameProfile profile = null)
    {
        if (profile is { FaceDataIndexIsRunningTotal: false })
            return;

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

        if (model.Quarters.Count > 0 || !HasEveryEntry(map, model))
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
        var entries = EntriesOf(map, model).ToList();
        var positions = entries.Select(e => list.IndexOf(e)).ToHashSet();
        var names = entries.Select(e => e.NameID).ToHashSet();

        RemoveGatesTo(list, positions, map.Profile);

        map.Nva.Connectors.RemoveAll(c => names.Contains(c.MainNameID) || names.Contains(c.TargetNameID));

        foreach (var nvc in map.Nvcs)
        {
            bool isA = nvc.MapA == map.MapId, isB = nvc.MapB == map.MapId;
            if (nvc.Nvc.Connectors.RemoveAll(c => (isA && names.Contains(c.MainNameID)) || (isB && names.Contains(c.TargetNameID))) > 0)
                result.Changes.NvcsChanged.Add(nvc);
        }

        foreach (var bank in map.Nva.NodeBanks)
            bank.Faces.RemoveAll(f => names.Contains(f.CollisionID));

        foreach (var entry in entries)
        {
            entry.FaceCount = 0;
            entry.Unk4C = 1;
        }

        RecomputeFaceDataIndex(map.Nva, map.Profile);

        foreach (var file in map.Profile.Sets.Select(model.File))
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

        if (source.Quarters.Count > 0 || map.Profile.Sets.Any(s => source.File(s)?.Document == null))
        {
            result.Error = "MAP_Navigation_Model_Error_Source";
            return result;
        }

        if (!FreeSlots(map).Contains(slot))
        {
            result.Error = "MAP_Navigation_Model_Error_Slot";
            return result;
        }

        foreach (var set in map.Profile.Sets)
        {
            var name = NavigationMap.MeshName(map.NameBase, set, slot.ModelID);
            var before = map.Files.GetValueOrDefault(name)?.Document;
            var copy = TranslatedCopy(source.File(set).Document, offset);

            result.Files.Add(new NavFileChange(name, before, copy));

            var entry = slot.Entry(set);
            entry.FaceCount = copy.ObjectsOf<IHavokNavMeshEdit>().First().m_faces.Count;
            entry.Unk4C = 0;
        }

        RecomputeFaceDataIndex(map.Nva, map.Profile);
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

        if (n == null || (o == null && map.Profile.HasOSet))
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
            foreach (var set in map.Profile.Sets)
            {
                var oldName = NavigationMap.MeshName(map.NameBase, set, target.ModelID);
                if (map.Files.GetValueOrDefault(oldName)?.Document is Havok.HavokDocument old)
                    result.Files.Add(new NavFileChange(oldName, old, null));
            }

            foreach (var entry in EntriesOf(map, target))
                entry.ModelID = id;
        }

        if (replace)
        {
            var names = EntriesOf(map, target).Select(e => e.NameID).ToHashSet();
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

        foreach (var (set, document) in SetDocuments(map, n, o))
        {
            var entry = target.Entry(set);
            var name = NavigationMap.MeshName(map.NameBase, set, entry.ModelID);
            result.Files.Add(new NavFileChange(name, map.Files.GetValueOrDefault(name)?.Document, document));

            entry.FaceCount = document.ObjectsOf<IHavokNavMeshEdit>().First().m_faces.Count;
            entry.Unk4C = entry.FaceCount == 0 ? 1 : 0;
        }

        RecomputeFaceDataIndex(map.Nva, map.Profile);
        return result;
    }

    /// <summary>The n and o documents of an edit, paired with their sets; the o one only in a game with an o set.</summary>
    private static IEnumerable<(NavmeshSet Set, Havok.HavokDocument Document)> SetDocuments(NavigationMap map, Havok.HavokDocument n, Havok.HavokDocument o)
        => map.Profile.Sets.Select(s => (s, s == NavmeshSet.N ? n : o));

    /// <summary>Why a new model can't be added to a map, or null if it can (maps with quarter entries can't).</summary>
    public static string CanAdd(NavigationMap map)
        => map.Nva.Navmeshes.Any(e => NavigationMap.QuarterOf(e, map.Profile) != null) ? "MAP_Navigation_Model_Error_Add_Quarters" : null;

    /// <summary>The lowest ModelID from a value up that no entry of the map has.</summary>
    public static int NextFreeModelId(NavigationMap map, int from)
    {
        var used = map.Nva.Navmeshes.Select(e => e.ModelID).ToHashSet();
        int id = Math.Max(0, from);
        while (used.Contains(id))
            id++;
        return id;
    }

    /// <summary>
    /// Adds a new model with the given n and o files: an n entry at the end of the n half of the NVA's list and an o
    /// entry at the end of the o half, the order the game's files have. The n entry's NameID is the map's prefix and digits
    /// with its position as the index, and the o entry's is that plus 2,000,000,000, so no existing NameID changes.
    /// The o half moves down by one, and gate nodes name navmeshes by position, so those pointing into it are moved too.
    /// The entries' other values are copied from the first n entry and its o entry. Maps with quarter entries are
    /// refused: their NameIDs follow their position, which this would move.
    /// </summary>
    public static NavModelEditResult Add(NavigationMap map, int modelId, Havok.HavokDocument n, Havok.HavokDocument o)
    {
        var result = new NavModelEditResult();
        var list = map.Nva.Navmeshes;

        if (CanAdd(map) is string refused)
        {
            result.Error = refused;
            return result;
        }

        if (n == null || (o == null && map.Profile.HasOSet))
        {
            result.Error = "MAP_Navigation_Generate_Error_File";
            return result;
        }

        if (modelId < 0 || modelId > 999999 || list.Any(e => e.ModelID == modelId))
        {
            result.Error = "MAP_Navigation_Generate_Error_Model_Id";
            return result;
        }

        int nCount = list.Count(e => NavigationMap.SetOf(e) == NavmeshSet.N);
        if (list.Take(nCount).Any(e => NavigationMap.SetOf(e) != NavmeshSet.N))
        {
            result.Error = "MAP_Navigation_Model_Error_Add_Order";
            return result;
        }

        var templateN = list.FirstOrDefault(e => NavigationMap.SetOf(e) == NavmeshSet.N);
        var templateO = templateN == null ? null : list.FirstOrDefault(e => (uint)e.NameID == map.Profile.MirrorOf((uint)templateN.NameID));

        uint nameId = map.ExpectedNameId(nCount);
        if (list.Any(e => (uint)e.NameID == nameId || (map.Profile.HasOSet && (uint)e.NameID == map.Profile.MirrorOf(nameId))))
        {
            result.Error = "MAP_Navigation_Model_Error_Add_NameID";
            return result;
        }

        // The o half moves down by one: every gate pointing at or past the insertion point follows. With no o set the
        // n entry goes at the end, past every gate's navmesh
        foreach (var entry in list)
            foreach (var gate in entry.GateNodes)
                if (gate.ConnectedNavmeshIndex >= nCount)
                    gate.ConnectedNavmeshIndex++;

        var nEntry = NewEntry(templateN, (int)nameId, modelId, n, map.Profile);
        var oEntry = map.Profile.HasOSet ? NewEntry(templateO ?? templateN, (int)map.Profile.MirrorOf(nameId), modelId, o, map.Profile) : null;

        list.Insert(nCount, nEntry);
        if (oEntry != null)
            list.Add(oEntry);

        foreach (var (set, document) in SetDocuments(map, n, o))
        {
            var name = NavigationMap.MeshName(map.NameBase, set, modelId);
            result.Files.Add(new NavFileChange(name, map.Files.GetValueOrDefault(name)?.Document, document));
        }

        RecomputeFaceDataIndex(map.Nva, map.Profile);
        result.Added = (nEntry, oEntry);
        return result;
    }

    private static NVA.Navmesh NewEntry(NVA.Navmesh template, int nameId, int modelId, Havok.HavokDocument document, NavGameProfile profile)
    {
        var entry = new NVA.Navmesh();

        if (template != null)
        {
            entry.Position = template.Position;
            entry.PositionW = template.PositionW;
            entry.Rotation = template.Rotation;
            entry.RotationW = template.RotationW;
            entry.Scale = template.Scale;
            entry.ScaleW = template.ScaleW;
            entry.Unk3C = template.Unk3C;
            entry.Unk44 = template.Unk44;
            // ER and NR: 0xFF01, no quarter. AC6: 1 on every entry with a ModelID of 0 or more (0 on the negative ones).
            // DB_NT's compact entries keep the connected list's offset there, which the writer sets
            entry.Unk50 = profile.HasQuarters ? 0xFF01 : 0x0001;
            entry.Unk54 = template.Unk54;
            entry.Unk58 = template.Unk58;
            entry.Unk5C = template.Unk5C;
        }

        entry.NameID = nameId;
        entry.ModelID = modelId;
        entry.FaceCount = document.ObjectsOf<IHavokNavMeshEdit>().First().m_faces.Count;
        entry.Unk4C = entry.FaceCount == 0 ? 1 : 0;
        entry.GateNodes = new List<NVA.GateNode>();
        entry.ConnectedNavmeshes = new List<int>();
        return entry;
    }

    /// <summary>
    /// Removes the gate nodes that lead to the navmeshes at some positions, and those navmeshes' own. Taking a gate out
    /// of a navmesh's list also takes its column out of that navmesh's gate costs, and moves down the NodeSubID of
    /// gates elsewhere that point at a later gate of the same navmesh.
    /// </summary>
    public static void RemoveGatesTo(List<NVA.Navmesh> list, ISet<int> positions, NavGameProfile profile = null)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (positions.Contains(i))
                continue;

            var gates = list[i].GateNodes;
            for (int g = gates.Count - 1; g >= 0; g--)
                if (positions.Contains(gates[g].ConnectedNavmeshIndex))
                    RemoveGate(list, i, g, profile);
        }

        foreach (var p in positions)
            if (p >= 0 && p < list.Count)
                list[p].GateNodes.Clear();
    }

    /// <summary>
    /// Takes one gate out of a navmesh's list: its column out of the owner's other gates' costs, and the NodeSubID of
    /// gates elsewhere that point at it (made -1) or at a later gate of the owner (moved down).
    /// </summary>
    public static void RemoveGate(List<NVA.Navmesh> list, int owner, int gate, NavGameProfile profile = null)
    {
        var gates = list[owner].GateNodes;
        gates.RemoveAt(gate);
        bool perGate = (profile ?? NavGameProfile.ER).GateCostsPerGate;

        // The costs are by the owner's gate index: padded with -1 to 16 slots (ER, NR), or one per gate (DB_NT)
        foreach (var g in gates)
        {
            var costs = g.NeighbourGateNodeCosts;
            if (gate < costs.Count)
            {
                costs.RemoveAt(gate);
                if (!perGate)
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
        var mesh = copy.ObjectsOf<IHavokNavMeshEdit>().First();

        mesh.m_faces.Clear();
        mesh.m_edges.Clear();
        mesh.m_vertices.Clear();
        mesh.m_faceData.Clear();
        mesh.m_edgeData.Clear();
        mesh.ClearStreamingSets();
        mesh.DropClearanceSeeding();

        if (copy.Root is IHavokRootContainerEdit root)
            root.KeepOnly(v => ReferenceEquals(v, mesh));

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

        foreach (var mesh in copy.ObjectsOf<IHavokNavMeshEdit>())
        {
            for (int i = 0; i < mesh.m_vertices.Count; i++)
                mesh.m_vertices[i] += shift;

            mesh.m_aabb.m_min += shift;
            mesh.m_aabb.m_max += shift;
        }

        // Each face tree's root box (hk2018's mediators, hk2019's cached face iterators): the nodes follow it
        var trees = copy.ObjectsOf<IHavokNavMeshQueryMediator>().Select(m => m.FaceTree)
            .Concat(copy.ObjectsOf<IHavokNavMeshEdit>().Select(m => m.CachedFaceTree))
            .Where(t => t is { HasNodes: true }).Distinct();

        foreach (var tree in trees)
        {
            tree.DomainMin += shift;
            tree.DomainMax += shift;
        }

        foreach (var setups in copy.ObjectsOf<IHavokNavUserEdgeSetups>())
            setups.TranslateWorldSpace(offset);

        return copy;
    }
}
