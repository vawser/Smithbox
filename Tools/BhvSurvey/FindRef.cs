using SoulsFormats;
// findref <file> <from>:<to> ...  -> where the destination state index appears (s16) in the source state's body/data
public static class FindRef
{
    public static void Run(string path, IEnumerable<string> pairs)
    {
        var bhv = BHV.Read(File.ReadAllBytes(path));
        foreach (var p in pairs)
        {
            var (a, b) = (int.Parse(p.Split(':')[0]), int.Parse(p.Split(':')[1]));
            var s = bhv.States[a];
            var body = Enumerable.Range(0, s.Body.Length / 2).Where(i => BitConverter.ToInt16(s.Body, i * 2) == b).Select(i => $"body+{i * 2:X}");
            var data = Enumerable.Range(0, s.Data.Length / 2).Where(i => BitConverter.ToInt16(s.Data, i * 2) == b).Select(i => $"data+{i * 2:X}");
            Console.WriteLine($"{Path.GetFileName(path)} {a}->{b} (kind {s.Kind}, data len {s.Data.Length:X}): {string.Join(" ", body.Concat(data))}");
        }
    }
}
