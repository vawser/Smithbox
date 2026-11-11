using SoulsFormats;
// animrefs <file> <state> ...  -> the state's animation record(s) as s16s
public static class AnimRefs
{
    public static void Run(string path, IEnumerable<string> states)
    {
        var bhv = BHV.Read(File.ReadAllBytes(path));
        foreach (var st in states.Select(int.Parse))
        {
            var s = bhv.States[st];
            foreach (int off in new[] { 0x0, 0x52, 0x54 })
            {
                int ai = BitConverter.ToInt16(s.Body, off);
                if (ai < 0 || ai >= bhv.Animations.Count) { Console.WriteLine($"{Path.GetFileName(path)} s{st} body+{off:X} anim index {ai}"); continue; }
                var a = bhv.Animations[ai];
                var shorts = Enumerable.Range(0, a.Length / 2).Select(i => BitConverter.ToInt16(a, i * 2)).ToArray();
                Console.WriteLine($"{Path.GetFileName(path)} s{st} body+{off:X} anim[{ai}] id={BitConverter.ToInt32(a, 0)}  +44..+53 s16: {string.Join(",", shorts.Skip(0x22).Take(8))}  +04 s16: {shorts[2]},{shorts[3]}  +1C: {BitConverter.ToInt32(a, 0x1C)}");
            }
        }
    }
}
