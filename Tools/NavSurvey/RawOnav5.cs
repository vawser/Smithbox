// Checks the decoded ONAV layout: objects packed as body, records, record entries, links; then the 0x230 array
static class RawOnav5
{
    public static void Survey(string dir)
    {
        foreach (var path in Directory.GetFiles(dir, "*.onav"))
        {
            var b = File.ReadAllBytes(path);
            var name = Path.GetFileName(path);
            int pos = 0x74;
            int n = BitConverter.ToUInt16(b, 0x34), na = BitConverter.ToUInt16(b, 0x36);
            int arrC = (int)Raw.L(b, 0x20);
            bool ok = true;
            var sites = new List<int> { 0x10, 0x18, 0x20 };
            for (int i = 0; i < n; i++)
            {
                int obj = (int)Raw.L(b, arrC + i * 8);
                if (obj != pos) { ok = false; Console.WriteLine($"{name} obj {i} at {obj:X} expected {pos:X}"); break; }
                int recs = b[obj + 2], links = BitConverter.ToUInt16(b, obj + 0x28);
                Raw.Count("obj id==i", BitConverter.ToUInt16(b, obj) == i);
                Raw.Count("obj b3 rotmode", b[obj + 3]);
                Raw.Count("obj +2A u16", BitConverter.ToUInt16(b, obj + 0x2A));
                Raw.Count("obj +2C,+2D,+2E,+2F", $"{b[obj + 0x2C]},{b[obj + 0x2D]},{b[obj + 0x2E]},{b[obj + 0x2F]}");
                for (int o = 0x30; o < 0x40; o += 4) Raw.Count($"obj +{o:X}", Raw.I(b, obj + o));
                Raw.Count("records", recs); Raw.Count("links", links);
                int p = obj + 0x248 + recs * 0x38;
                sites.Add(obj + 0x240);
                for (int r = 0; r < recs; r++)
                {
                    int rec = obj + 0x248 + r * 0x38;
                    sites.Add(rec + 0x30);
                    int ec = b[rec + 0x2A];
                    if (Raw.L(b, rec + 0x30) != p) { ok = false; Console.WriteLine($"{name} obj {i} rec {r} ptr {Raw.L(b, rec + 0x30):X} expected {p:X}"); }
                    Raw.Count("rec b0 layer", b[rec]); Raw.Count("rec b1 flag", b[rec + 1]); Raw.Count("rec b2,b3", $"{b[rec + 2]},{b[rec + 3]}");
                    Raw.Count("rec +10", Raw.I(b, rec + 0x10)); Raw.Count("rec +14", Raw.I(b, rec + 0x14)); Raw.Count("rec +18", Raw.I(b, rec + 0x18));
                    Raw.Count("rec +20", Raw.I(b, rec + 0x20)); Raw.Count("rec +24 nonzero", Raw.I(b, rec + 0x24) != 0);
                    Raw.Count("rec +28,+29,+2B", $"{b[rec + 0x28]:X},{b[rec + 0x29]},{b[rec + 0x2B]}"); Raw.Count("rec +2C", Raw.I(b, rec + 0x2C));
                    Raw.Count("rec entries", ec);
                    for (int e = 0; e < ec; e++)
                    {
                        int en = p + e * 16;
                        Raw.Count("ent b0", $"{b[en]:X2}"); Raw.Count("ent +4", Raw.I(b, en + 4)); Raw.Count("ent +8", Raw.I(b, en + 8)); Raw.Count("ent +C", Raw.I(b, en + 12));
                        Raw.Count("ent b1..3", $"{b[en + 1]},{b[en + 2]},{b[en + 3]}");
                    }
                    p += ec * 16;
                }
                if (Raw.L(b, obj + 0x240) != p) { ok = false; Console.WriteLine($"{name} obj {i} link ptr {Raw.L(b, obj + 0x240):X} expected {p:X}"); }
                for (int l = 0; l < links; l++)
                {
                    long v = Raw.L(b, p + l * 8);
                    Raw.Count("link target<n", (v & 0xFFFF) < n);
                    Raw.Count("link b2", $"{(v >> 16) & 0xFF:X2}"); Raw.Count("link b3..7", $"{v >> 24:X}");
                }
                pos = p + links * 8;
            }
            if (!ok) continue;
            Raw.Count("A at pos", Raw.L(b, 0x10) == pos);
            pos += na * 0x230;
            for (int a = 0; a < na; a++)
            {
                int el = (int)Raw.L(b, 0x10) + a * 0x230;
                Raw.Count("A b2,b3", $"{b[el + 2]},{b[el + 3]}"); Raw.Count("A +28", Raw.I(b, el + 0x28)); Raw.Count("A +2C", Raw.I(b, el + 0x2C));
                Raw.Count("A id", BitConverter.ToUInt16(b, el));
            }
            Raw.Count("B at pos", Raw.L(b, 0x18) == pos);
            Raw.Count("C at pos", arrC == pos);
            pos += n * 8;
            Raw.Count("table at pos", Raw.L(b, 0x28) == pos);
            int relocCount = Raw.I(b, 0xC);
            var table = Enumerable.Range(0, relocCount).Select(i => (int)Raw.L(b, pos + i * 8)).ToList();
            Raw.Count("table == expected", table.SequenceEqual(sites));
            Raw.Count("ends", pos + relocCount * 8 == b.Length);
            Raw.Count("version", $"{b[4]}.{b[5]}");
            Raw.Count("+08", Raw.I(b, 8));
            Raw.Count("grid product", Raw.I(b, 0x68) * Raw.I(b, 0x6C) * Raw.I(b, 0x70));
        }
        Raw.Print(16);
    }
}
