using SoulsFormats;

// dcxpairs mode: every DES file that exists both as X and X.dcx, decompressed and compared byte for byte.
// Usage: DesModelSurvey dcxpairs <USRDIR>
static class DcxPairs
{
    public static void Run(string root)
    {
        int same = 0, differ = 0, failed = 0;
        var dcxTypes = new Dictionary<string, int>();
        var differing = new List<string>();

        foreach (var dcxPath in Directory.EnumerateFiles(root, "*.dcx", SearchOption.AllDirectories))
        {
            var rawPath = dcxPath[..^4];
            if (!File.Exists(rawPath)) continue;

            try
            {
                var raw = File.ReadAllBytes(rawPath);
                var unpacked = DCX.Decompress(File.ReadAllBytes(dcxPath), out DCX.Type type);
                dcxTypes[type.ToString()] = dcxTypes.GetValueOrDefault(type.ToString()) + 1;

                if (unpacked.AsSpan().SequenceEqual(raw)) same++;
                else { differ++; differing.Add($"{Path.GetRelativePath(root, rawPath)} raw {raw.Length} dcx-unpacked {unpacked.Length}"); }
            }
            catch (Exception e) { failed++; Console.WriteLine($"fail {dcxPath}: {e.Message}"); }
        }

        Console.WriteLine($"pairs identical: {same}, different: {differ}, failed: {failed}");
        Console.WriteLine("dcx types: " + string.Join(", ", dcxTypes.Select(x => $"{x.Key} {x.Value}")));
        foreach (var d in differing.Take(40)) Console.WriteLine($"  differs: {d}");
    }
}
