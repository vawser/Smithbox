using SoulsFormats;

// roundtrip: every ER .nvc / .onav read and written through SoulsFormats, compared byte for byte
// check: NVC fields against the two maps' NVAs (map UIDs, navmesh NameIDs, face and edge ranges)
static class RoundTrip
{
    public static void Run(IEnumerable<(string Name, byte[] Data)> files)
    {
        int ok = 0, bad = 0, failed = 0;
        foreach (var (name, data) in files)
        {
            try
            {
                byte[] written = name.EndsWith(".nvc") ? NVC.Read(data).Write() : ONAV.Read(data).Write();
                if (written.AsSpan().SequenceEqual(data)) ok++;
                else
                {
                    bad++;
                    int diff = 0; while (diff < Math.Min(written.Length, data.Length) && written[diff] == data[diff]) diff++;
                    if (bad <= 10) Console.WriteLine($"{name}: differs at 0x{diff:X} (len {data.Length:X} -> {written.Length:X})");
                }
            }
            catch (Exception e)
            {
                failed++;
                if (failed <= 10) Console.WriteLine($"{name}: {e.GetType().Name} {e.Message}");
            }
        }
        Console.WriteLine($"identical {ok}, different {bad}, failed {failed}");
    }

    public static void Check(Andre.IO.VFS.VirtualFileSystem fs, List<string> paths)
    {
        var nvas = new Dictionary<string, NVA_ER>();
        NVA_ER Nva(string map)
        {
            if (nvas.TryGetValue(map, out var n)) return n;
            var p = $"/map/{map.Substring(0, 3)}/{map}/{map}.nva.dcx";
            var f = fs.ReadFile(p);
            return nvas[map] = f == null ? null : NVA_ER.Read(DCX.Decompress(f.Value.ToArray(), out DCX.Type _));
        }
        foreach (var p in paths.Where(p => p.EndsWith(".nvc.dcx")))
        {
            var raw = fs.ReadFile(p); if (raw == null) continue;
            var nvc = NVC.Read(DCX.Decompress(raw.Value.ToArray(), out DCX.Type _));
            var n = Path.GetFileName(p).Replace(".nvc.dcx", "");
            string a = n.Substring(0, 12), b = n.Substring(13);
            var na = Nva(a); var nb = Nva(b);
            if (na == null || nb == null) { Raw.Count("nva missing", na == null ? a : b); continue; }
            Raw.Count("MapUID == A Entry11.Unk00", (int)nvc.MapUID == na.Entries11[0].Unk00);
            Raw.Count("OppositeMapUID == B Entry11.Unk00", (int)nvc.OppositeMapUID == nb.Entries11[0].Unk00);
            foreach (var c in nvc.Connectors)
            {
                var ma = na.Navmeshes.FirstOrDefault(x => x.NameID == c.MainNameID);
                var mb = nb.Navmeshes.FirstOrDefault(x => x.NameID == c.TargetNameID);
                Raw.Count("MainNameID in A navmeshes", ma != null);
                Raw.Count("TargetNameID in B navmeshes", mb != null);
                foreach (var k in c.NavMeshConnections)
                {
                    if (ma != null) Raw.Count("Face < A FaceCount", k.FaceIndex < ma.FaceCount);
                    if (mb != null) Raw.Count("OppositeFace < B FaceCount", k.OppositeFaceIndex < mb.FaceCount);
                    Raw.Count("Edge >= 3*Face", k.EdgeIndex >= 3 * k.FaceIndex && k.OppositeEdgeIndex >= 3 * k.OppositeFaceIndex);
                }
            }
        }
        Raw.Print();
    }
}
