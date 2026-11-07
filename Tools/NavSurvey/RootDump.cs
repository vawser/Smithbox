using Andre.IO.VFS;
using Havok.hk2018;
using SoulsFormats;

// rootdump <map> : the root variants of a map's navmesh files, empty and full, and the binder's file entries
static class RootDump
{
    public static void Run(VirtualFileSystem fs, string mapId)
    {
        var profile = Havok.HavokProfile.FromGame("ER");
        var bnd = BND4.Read(fs.ReadFile(StudioCore.Editors.MapEditor.NavigationReader.NvmPath(mapId)).Value.ToArray());
        Console.WriteLine($"binder: format {bnd.Format}, unicode {bnd.Unicode}, ext {bnd.Extended}, version {bnd.Version}, {bnd.Files.Count} files");
        var shapes = new Dictionary<string, int>();
        foreach (var f in bnd.Files)
        {
            var bytes = f.Bytes.ToArray();
            string dcx = "none";
            if (DCX.Is(bytes)) { bytes = DCX.Decompress(bytes, out DCX.Type t).ToArray(); dcx = t.ToString(); }
            var doc = Havok.Files.HavokFile.Read(bytes, profile);
            var mesh = doc.ObjectsOf<hkaiNavMesh>().First();
            var root = (hkRootLevelContainer)doc.Root;
            var variants = string.Join(", ", root.m_namedVariants.Select(v => $"{v.m_name}:{v.m_className}"));
            var key = $"{(mesh.m_faces.Count == 0 ? "empty" : "full")} dcx {dcx} flags {f.Flags} | {variants} | seeding {(mesh.m_clearanceCacheSeedingDataSet != null ? "set" : "none")}";
            if (shapes.TryAdd(key, 1)) Console.WriteLine($"{f.ID,4} {f.Name}\n     {key}");
            else shapes[key]++;
        }
        foreach (var (k, v) in shapes) Console.WriteLine($"{v,5} x {k}");
    }
}
