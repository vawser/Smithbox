using Andre.IO.VFS;
using Havok.hk2018;
using SoulsFormats;
using System.Numerics;
using System.Text.RegularExpressions;

// Rebuilds each ER/NR fall protection body's extern mesh from its own triangles with the shared builder and compares
// it with the game's: key bits, simd tree nodes (structure, data, boxes).
//   CollisionSurvey ER <mapId> [max shown] fallrebuild [file filter regex]
static class FallRebuild
{
    public static void Survey(VirtualFileSystem fs, string gameName, string mapId, int maxShown, Regex filter)
    {
        var profile = Havok.HavokProfile.FromGame(gameName);
        var bhdPath = $"/map/{mapId.Substring(0, 3)}/{mapId}/f{mapId.Substring(1)}.hkxbhd";
        var bxf = BXF4.Read(fs.ReadFile(bhdPath).Value.ToArray(), fs.ReadFile(Path.ChangeExtension(bhdPath, ".hkxbdt")).Value.ToArray());
        var compFile = bxf.Files.FirstOrDefault(f => f.Name.Contains(".compendium"));
        Havok.HavokCompendium compendium = null;
        if (compFile != null) { var b = compFile.Bytes.ToArray(); if (DCX.Is(b)) b = DCX.Decompress(b).ToArray(); compendium = Havok.Files.HavokFile.ReadCompendium(b, profile); }

        int bodies = 0, keyBitsBuilder = 0, keyBitsRule = 0, nodeCount = 0, structure = 0, boxes = 0, shown = 0;

        foreach (var file in bxf.Files.Where(f => f.Name.Contains(".hkx") && filter.IsMatch(f.Name)))
        {
            var bytes = file.Bytes.ToArray();
            if (DCX.Is(bytes)) bytes = DCX.Decompress(bytes).ToArray();
            var root = (hkRootLevelContainer)Havok.Files.HavokFile.Read(bytes, profile, compendium).Root;
            var scene = root.m_namedVariants.Select(v => v.m_variant).OfType<hknpPhysicsSceneData>().First();

            foreach (var body in scene.m_systemDatas.SelectMany(s => s.m_bodyCinfos))
            {
                if (body.m_shape is not hknpExternMeshShape game) continue;
                var g = ((hknpDefaultExternMeshShapeGeometry)game.m_geometry).m_geometry;
                var verts = g.m_vertices.Select(v => new Vector3(v.X, v.Y, v.Z)).ToList();
                var idx = g.m_triangles.SelectMany(t => new[] { t.m_a, t.m_b, t.m_c }).ToList();
                if (Environment.GetEnvironmentVariable("FALLDUMP") is { } want && g.m_triangles.Count.ToString() == want && shown++ < maxShown)
                {
                    var nodes = ((hknpExternMeshShapeData)game.m_boundingVolumeData).m_simdTree;
                    Console.WriteLine($"--- {file.Name} tris {g.m_triangles.Count} compact {nodes.m_isCompact}");
                    for (var t = 0; t < g.m_triangles.Count; t++) { var tr = g.m_triangles[t]; Console.WriteLine($"  tri {t}: {verts[tr.m_a]:0.##} {verts[tr.m_b]:0.##} {verts[tr.m_c]:0.##}"); }
                    for (var i = 0; i < nodes.m_nodes.Count; i++) { var n = nodes.m_nodes[i]; Console.WriteLine($"  node {i}: leaf {n.m_isLeaf} active {n.m_isActive} parent {n.m_parent} data {string.Join(",", n.m_data.Select(d => d.ToString("X")))}"); Console.WriteLine($"    lx {n.m_lx:0.##} hx {n.m_hx:0.##}"); Console.WriteLine($"    ly {n.m_ly:0.##} hy {n.m_hy:0.##}"); Console.WriteLine($"    lz {n.m_lz:0.##} hz {n.m_hz:0.##}"); }
                }
                var built = Havok.hk2018.Builders.CollisionShapeBuilder.BuildExternMeshShape(verts, idx);
                bodies++;

                if (built.m_numShapeKeyBits == game.m_numShapeKeyBits) keyBitsBuilder++;
                var bitLength = 32 - System.Numerics.BitOperations.LeadingZeroCount((uint)g.m_triangles.Count);
                if (bitLength + 1 == game.m_numShapeKeyBits) keyBitsRule++;

                var gn = ((hknpExternMeshShapeData)game.m_boundingVolumeData).m_simdTree.m_nodes;
                var bn = ((hknpExternMeshShapeData)built.m_boundingVolumeData).m_simdTree.m_nodes;
                if (gn.Count != bn.Count) { if (shown++ < maxShown) Console.WriteLine($"{file.Name}: nodes {gn.Count} vs built {bn.Count} tris {g.m_triangles.Count}"); continue; }
                nodeCount++;

                var sameStructure = true; var sameBoxes = true;
                for (var i = 0; i < gn.Count; i++)
                {
                    if (gn[i].m_isLeaf != bn[i].m_isLeaf || gn[i].m_isActive != bn[i].m_isActive || gn[i].m_parent != bn[i].m_parent || !gn[i].m_data.SequenceEqual(bn[i].m_data))
                    {
                        if (sameStructure && shown++ < maxShown)
                            Console.WriteLine($"{file.Name} node {i}: leaf {gn[i].m_isLeaf}/{bn[i].m_isLeaf} active {gn[i].m_isActive}/{bn[i].m_isActive} parent {gn[i].m_parent}/{bn[i].m_parent} data {string.Join(",", gn[i].m_data.Select(d => d.ToString("X")))} / {string.Join(",", bn[i].m_data.Select(d => d.ToString("X")))}");
                        sameStructure = false;
                    }
                    var d = new[] { gn[i].m_lx - bn[i].m_lx, gn[i].m_hx - bn[i].m_hx, gn[i].m_ly - bn[i].m_ly, gn[i].m_hy - bn[i].m_hy, gn[i].m_lz - bn[i].m_lz, gn[i].m_hz - bn[i].m_hz };
                    if (d.Any(v => MathF.Abs(v.X) + MathF.Abs(v.Y) + MathF.Abs(v.Z) + MathF.Abs(v.W) > 1e-3f)) sameBoxes = false;
                }
                if (sameStructure) structure++;
                if (sameBoxes) boxes++;
            }
        }

        Console.WriteLine($"bodies {bodies}: key bits builder {keyBitsBuilder}, bitLength+1 rule {keyBitsRule}; node count {nodeCount}, structure {structure}, boxes {boxes}");
    }
}
