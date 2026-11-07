using Andre.IO.VFS;
using Havok.hk2018;
using SoulsFormats;
using System.Text.RegularExpressions;

// havokedit [regex] [max] : face data, edge data and edge flag edits to a navmesh survive HavokFile.Write and a re-read,
// and nothing else in the mesh changes. This is how the Map Editor saves the Navigation tool's edits.
static class HavokEdit
{
    public static void Run(VirtualFileSystem fs, string dict, string[] args)
    {
        var re = new Regex(args.Length > 1 ? args[1] : ".");
        int max = args.Length > 2 ? int.Parse(args[2]) : 10;
        var maps = Regex.Matches(dict, @"""/map/m\d\d/(m\d\d_\d\d_\d\d_\d\d)/\1\.nva\.dcx""").Select(m => m.Groups[1].Value)
            .Distinct().Where(m => re.IsMatch(m)).Take(max).ToList();
        var profile = Havok.HavokProfile.FromGame("ER");
        int files = 0, ok = 0, unchangedOk = 0;

        foreach (var mapId in maps)
        {
            var bnd = fs.ReadFile(StudioCore.Editors.MapEditor.NavigationReader.NvmPath(mapId));
            if (bnd == null) continue;

            foreach (var f in BND4.Read(bnd.Value.ToArray()).Files)
            {
                var bytes = f.Bytes.ToArray();
                if (DCX.Is(bytes)) bytes = DCX.Decompress(bytes).ToArray();

                // Unedited: written and read back, the mesh is the same
                var plain = Havok.Files.HavokFile.Read(bytes, profile);
                var plainMesh = plain.ObjectsOf<hkaiNavMesh>().First();
                var plainBack = Havok.Files.HavokFile.Read(Havok.Files.HavokFile.Write(plain), profile).ObjectsOf<hkaiNavMesh>().First();
                if (Same(plainMesh, plainBack)) unchangedOk++;

                var doc = Havok.Files.HavokFile.Read(bytes, profile);
                var mesh = doc.ObjectsOf<hkaiNavMesh>().First();
                if (mesh.m_faces.Count == 0 || mesh.m_faceDataStriding != 1 || mesh.m_edgeDataStriding != 1) continue;
                files++;

                int face = mesh.m_faces.Count / 2;
                int userEdge = mesh.m_faces.Select(x => x.m_numUserEdges > 0 ? x.m_startUserEdgeIndex : -1).FirstOrDefault(x => x >= 0, -1);
                int edge = userEdge >= 0 ? userEdge : mesh.m_faces[face].m_startEdgeIndex;

                var expected = Clone(mesh);
                mesh.m_faceData[face] ^= 0x00100000;
                mesh.m_edgeData[edge] ^= 0x0700;
                mesh.m_edges[edge].m_flags ^= hkaiNavMesh.EdgeFlagBits.EDGE_BLOCKED;
                expected.FaceData[face] ^= 0x00100000;
                expected.EdgeData[edge] ^= 0x0700;
                expected.Flags[edge] ^= (int)hkaiNavMesh.EdgeFlagBits.EDGE_BLOCKED;

                var back = Havok.Files.HavokFile.Read(Havok.Files.HavokFile.Write(doc), profile).ObjectsOf<hkaiNavMesh>().First();
                var got = Clone(back);

                bool same = got.FaceData.SequenceEqual(expected.FaceData) && got.EdgeData.SequenceEqual(expected.EdgeData)
                    && got.Flags.SequenceEqual(expected.Flags) && Same(mesh, back);
                if (same) ok++;
                else Console.WriteLine($"{mapId} {f.Name}: the edit didn't survive the write");
            }
        }

        Console.WriteLine($"unedited round trip: {unchangedOk} files keep their mesh");
        Console.WriteLine($"edited round trip: {ok} of {files} files keep exactly the edit");
    }

    record Snapshot(int[] FaceData, int[] EdgeData, int[] Flags);

    static Snapshot Clone(hkaiNavMesh m) => new(m.m_faceData.ToArray(), m.m_edgeData.ToArray(), m.m_edges.Select(e => (int)e.m_flags).ToArray());

    static bool Same(hkaiNavMesh a, hkaiNavMesh b)
        => a.m_faces.Count == b.m_faces.Count && a.m_edges.Count == b.m_edges.Count && a.m_vertices.SequenceEqual(b.m_vertices)
        && a.m_faceData.SequenceEqual(b.m_faceData) && a.m_edgeData.SequenceEqual(b.m_edgeData)
        && a.m_edges.Select(e => (e.m_a, e.m_b, e.m_oppositeEdge, e.m_oppositeFace, e.m_flags)).SequenceEqual(b.m_edges.Select(e => (e.m_a, e.m_b, e.m_oppositeEdge, e.m_oppositeFace, e.m_flags)))
        && a.m_faces.Select(f => (f.m_startEdgeIndex, f.m_numEdges, f.m_startUserEdgeIndex, f.m_numUserEdges)).SequenceEqual(b.m_faces.Select(f => (f.m_startEdgeIndex, f.m_numEdges, f.m_startUserEdgeIndex, f.m_numUserEdges)));
}
