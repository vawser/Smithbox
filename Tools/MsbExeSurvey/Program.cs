// MsbExeSurvey: finds how eldenring.exe reads its MSB data.
//   classes <exe> <filter>        RTTI classes whose name contains filter, with vtables
//   vtable  <exe> <class>         the virtual functions of a class
//   strings <exe> <text>          strings containing text, with the code that references them
//   xref    <exe> <va>            code that references an address
//   dis     <exe> <va> [count]    disassembles a function
using Iced.Intel;
using System.Globalization;

var mode = args[0];
Exe.Load(args[1]);
Exe.LoadRtti();

ulong Hex(string s) => ulong.Parse(s.Replace("0x", ""), NumberStyles.HexNumber);

switch (mode)
{
    case "classes":
        foreach (var (vt, cls) in Exe.VtableClass.Where(k => k.Value.Contains(args[2], StringComparison.OrdinalIgnoreCase)).OrderBy(k => k.Value))
            Console.WriteLine($"{vt:X}  {cls}");
        break;

    case "vtable":
        foreach (var (vt, cls) in Exe.VtableClass.Where(k => k.Value == args[2]))
        {
            Console.WriteLine($"{vt:X}  {cls}");
            for (var i = 0; i < 200; i++)
            {
                var fn = Exe.ReadU64(vt + (ulong)i * 8);
                if (!Exe.IsCode(fn) || (i > 0 && Exe.VtableClass.ContainsKey(vt + (ulong)i * 8))) break;
                Console.WriteLine($"   #{i,-3} {fn:X}  shared by {Exe.VirtualOf[fn].Count}");
            }
        }
        break;

    case "strings":
        {
            var strs = Exe.FindStrings(args[2]);
            var set = strs.ToDictionary(s => s.VA, s => s.Text);
            var refs = new Dictionary<ulong, List<ulong>>();
            foreach (var ins in Exe.Sweep())
            {
                var t = Exe.RefTarget(ins);
                if (t != 0 && set.ContainsKey(t))
                {
                    if (!refs.TryGetValue(t, out var l)) refs[t] = l = new();
                    l.Add(ins.IP);
                }
            }
            // pointers to the strings in data (string tables)
            foreach (var s in Exe.Sections.Where(s => s.Name == ".rdata" || s.Name == ".data"))
                for (long fo = s.Raw; fo + 8 <= s.Raw + s.RawSize; fo += 8)
                {
                    var p = BitConverter.ToUInt64(Exe.Image, (int)fo);
                    if (set.ContainsKey(p))
                    {
                        if (!refs.TryGetValue(p, out var l)) refs[p] = l = new();
                        l.Add(Exe.VaOfFile(fo) | 0x8000000000000000);
                    }
                }
            foreach (var (va, text) in strs)
                Console.WriteLine($"{va:X}  \"{text}\"  refs: {string.Join(" ", (refs.GetValueOrDefault(va) ?? new()).Select(r => (r & 0x8000000000000000) != 0 ? $"ptr@{r & 0x7FFFFFFFFFFFFFFF:X}" : $"{r:X}"))}");
        }
        break;

    case "xref":
        {
            var targets = args.Skip(2).Select(Hex).ToHashSet();
            foreach (var ins in Exe.Sweep())
                if (targets.Contains(Exe.RefTarget(ins)))
                    Console.WriteLine(Exe.Format(ins));
            foreach (var s in Exe.Sections.Where(s => s.Name == ".rdata" || s.Name == ".data"))
                for (long fo = s.Raw; fo + 8 <= s.Raw + s.RawSize; fo += 8)
                    if (targets.Contains(BitConverter.ToUInt64(Exe.Image, (int)fo)))
                        Console.WriteLine($"ptr@{Exe.VaOfFile(fo):X}");
        }
        break;

    case "dis":
        foreach (var ins in Exe.Function(Hex(args[2]), args.Length > 3 ? int.Parse(args[3]) : 4000))
            Console.WriteLine(Exe.Format(ins));
        break;

    default:
        Extra.Run(mode, args);
        break;
}
