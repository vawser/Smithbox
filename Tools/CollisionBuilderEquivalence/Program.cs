global using Havok.Builders; // the archived copies name the algorithm classes unqualified
using System.Numerics;
using StudioCore.Editors.HavokEditor;

// Phase 6.4 check: builds the same meshes with the archived per-SDK generators (HavokShapeGenerator_HKX3/_2019,
// HavokCompressedMeshGenerator_HKX3/_2019) and with the shared Havok.hk2018/hk2019 CollisionShapeBuilder, and compares
// the object graphs field by field (floats by bits).
//   CollisionBuilderEquivalence [meshes = 200]

int count = args.Length > 0 ? int.Parse(args[0]) : 200;
var random = new Random(6_4);
int same = 0, different = 0;
var failures = new List<string>();

for (int m = 0; m < count; m++)
{
    var (vertices, indices, materials) = Mesh(random, m);

    void Check(string what, object oldShape, object newShape)
    {
        string difference = GraphCompare.Compare(oldShape, newShape);
        if (difference == null) same++;
        else { different++; if (failures.Count < 20) failures.Add($"mesh {m} ({indices.Count / 3} tris) {what}: {difference}"); }
    }

    Check("2018 extern", HavokShapeGenerator_HKX3.BuildExternMeshShape(vertices, indices),
        Havok.hk2018.Builders.CollisionShapeBuilder.BuildExternMeshShape(vertices, indices));
    Check("2019 extern", HavokShapeGenerator_HKX3_2019.BuildExternMeshShape(vertices, indices),
        Havok.hk2019.Builders.CollisionShapeBuilder.BuildExternMeshShape(vertices, indices));
    Check("2018 compressed", HavokCompressedMeshGenerator_HKX3.BuildCompressedMeshShape(vertices, indices, materials, 7),
        Havok.hk2018.Builders.CollisionShapeBuilder.BuildCompressedMeshShape(vertices, indices, materials, 7));
    Check("2019 compressed", HavokCompressedMeshGenerator_HKX3_2019.BuildCompressedMeshShape(vertices, indices, materials, 7),
        Havok.hk2019.Builders.CollisionShapeBuilder.BuildCompressedMeshShape(vertices, indices, materials, 7));
}

Console.WriteLine($"{count} meshes, {same + different} shapes compared: {same} equal, {different} different");
foreach (var f in failures) Console.WriteLine("  " + f);
return different == 0 ? 0 : 1;

// A deformed grid of a few to several thousand triangles, with a material per triangle on every other mesh (so both
// the per-triangle and the default material paths run), and some duplicated vertices
static (List<Vector3>, List<int>, List<uint>) Mesh(Random random, int index)
{
    int w = 1 + random.Next(index % 7 == 0 ? 60 : 12), h = 1 + random.Next(index % 5 == 0 ? 50 : 10);
    float scale = (float)(0.1 + random.NextDouble() * 200);
    var offset = new Vector3((float)random.NextDouble() * 1000 - 500, (float)random.NextDouble() * 100, (float)random.NextDouble() * 1000 - 500);

    var vertices = new List<Vector3>();
    for (int y = 0; y <= h; y++)
        for (int x = 0; x <= w; x++)
            vertices.Add(offset + new Vector3(x * scale, (float)(random.NextDouble() * scale * 0.3), y * scale));

    var indices = new List<int>();
    for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            int a = y * (w + 1) + x, b = a + 1, c = a + w + 1, d = c + 1;
            indices.AddRange([a, c, b, b, c, d]);
        }

    List<uint> materials = index % 2 == 0
        ? Enumerable.Range(0, indices.Count / 3).Select(_ => (uint)random.Next(4)).ToList()
        : null;

    return (vertices, indices, materials);
}

namespace StudioCore.Editors.HavokEditor
{
    // Stand-ins for the Smithbox types the archived copies' replace-action helpers name; not used here
    public class HavokFileWrapper;

    public class HavokDelegateAction(HavokFileWrapper wrapper, Action apply, Action revert);
}
