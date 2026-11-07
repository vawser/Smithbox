using Andre.IO.VFS;
using Havok.hk2018;
using SoulsFormats;

// cachedump <map> <n> : the clearance cache blocks of faces with and without user edges, to see their layout
static class CacheDump
{
    public static void Run(VirtualFileSystem fs, string mapId, int max)
    {
        var profile = Havok.HavokProfile.FromGame("ER");
        var bnd = fs.ReadFile(StudioCore.Editors.MapEditor.NavigationReader.NvmPath(mapId));
        int shown = 0, plain = 0;
        foreach (var f in BND4.Read(bnd.Value.ToArray()).Files)
        {
            var bytes = f.Bytes.ToArray();
            if (DCX.Is(bytes)) bytes = DCX.Decompress(bytes).ToArray();
            var doc = Havok.Files.HavokFile.Read(bytes, profile);
            var mesh = doc.ObjectsOf<hkaiNavMesh>().First();
            var cache = doc.ObjectsOf<hkaiNavMeshClearanceCache>().FirstOrDefault();
            if (cache == null || mesh.m_faces.Count == 0) continue;
            Console.WriteLine($"{f.Name}: faces {mesh.m_faces.Count} pairs {cache.m_edgePairClearances.Count} last offset {cache.m_faceOffsets[^1]} uncalc {cache.m_uncalculatedFacesLowerBound} ceiling {cache.m_clearanceCeiling}");
            for (int fi = 0; fi + 1 < mesh.m_faces.Count && shown < max; fi++)
            {
                var face = mesh.m_faces[fi];
                var linked = new List<string>();
                for (int e = face.m_startEdgeIndex; e < face.m_startEdgeIndex + face.m_numEdges; e++)
                    linked.Add(mesh.m_edges[e].m_oppositeFace != 0xFFFFFFFF ? "o" : "-");
                for (int e = face.m_startUserEdgeIndex; e < face.m_startUserEdgeIndex + face.m_numUserEdges; e++)
                    linked.Add(mesh.m_edges[e].m_oppositeFace != 0xFFFFFFFF ? "U" : "u");
                if (face.m_numUserEdges == 0 && plain++ % 50 != 0) continue;
                int o0 = (int)cache.m_faceOffsets[fi], o1 = (int)cache.m_faceOffsets[fi + 1];
                var mcp = string.Join(" ", Enumerable.Range(face.m_startEdgeIndex, face.m_numEdges).Concat(Enumerable.Range(Math.Max(0, face.m_startUserEdgeIndex), face.m_numUserEdges)).Select(e => $"{cache.m_mcpData[e].m_interpolant:X2}/{cache.m_mcpData[e].m_clearance:X2}"));
                var vc = string.Join(" ", Enumerable.Range(face.m_startEdgeIndex, face.m_numEdges).Select(e => cache.m_vertexClearances[mesh.m_edges[e].m_a].ToString("X2")));
                Console.WriteLine($"  face {fi} [{string.Join("", linked)}] {BitConverter.ToString(cache.m_edgePairClearances.Skip(o0).Take(o1 - o0).ToArray())} | mcp {mcp} | vc {vc}");
                shown++;
            }
            if (shown >= max) break;
        }
    }
}
