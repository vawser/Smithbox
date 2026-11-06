using Havok.hk2016;

// Dumps a Sekiro SIMD tree, to work out its node encoding
static class SdtSimd
{
    public static void Dump(fsnpCustomParamCompressedMeshShape cms)
    {
        var data = (hknpCompressedMeshShapeData)cms.m_data;
        var nodes = data.m_simdTree.m_nodes;
        var tree = data.m_meshTree;
        int shift = 32 - tree.m_bitsPerKey;
        var keys = cms.m_triangleIndexToShapeKey.Select(k => k >> shift).ToHashSet();
        Console.WriteLine($"  simd nodes {nodes.Count}, triangles {cms.m_triangleIndexToShapeKey.Count}, keys bits {tree.m_bitsPerKey}");
        for (int i = 0; i < Math.Min(nodes.Count, 14); i++)
        {
            var n = nodes[i];
            Console.WriteLine($"  [{i}] {string.Join(" ", n.m_data.Select(d => $"{d:X}"))} | x {n.m_lx.X:G4}..{n.m_hx.X:G4} {n.m_lx.Y:G4}..{n.m_hx.Y:G4} {n.m_lx.Z:G4}..{n.m_hx.Z:G4} {n.m_lx.W:G4}..{n.m_hx.W:G4}");
        }
        // Odd lane data: is it a leaf? what does it hold?
        int odd = 0, even = 0, oddIsKey = 0, oddShiftIsKey = 0, empty = 0, evenInRange = 0;
        var leafValues = new List<uint>();
        foreach (var n in nodes.Skip(1))
            for (int l = 0; l < 4; l++)
            {
                var d = n.m_data[l];
                if (Lane(n.m_lx, l) > Lane(n.m_hx, l)) { empty++; continue; }
                if ((d & 1) == 1) { odd++; leafValues.Add(d); if (keys.Contains(d >> 1)) oddShiftIsKey++; if (keys.Contains(d)) oddIsKey++; }
                else { even++; if ((d >> 1) < nodes.Count) evenInRange++; }
            }
        Console.WriteLine($"  lanes: empty {empty} odd {odd} (d>>1 is key {oddShiftIsKey}, d is key {oddIsKey}) even {even} (d>>1 < nodes {evenInRange})");
        Console.WriteLine($"  odd samples: {string.Join(",", leafValues.Take(16).Select(v => $"{v:X}"))}");
        Console.WriteLine($"  max leaf d>>1 {(leafValues.Count == 0 ? 0 : leafValues.Max() >> 1):X}  maxKey {tree.m_maxKeyValue:X}  numPrims {tree.m_primitives.Count}");
    }

    static float Lane(System.Numerics.Vector4 v, int l) => l switch { 0 => v.X, 1 => v.Y, 2 => v.Z, _ => v.W };
}
