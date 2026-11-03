using SoulsFormats;
using System;
using System.IO;
using System.Linq;
if (args[0] == "erbake") ErBake.Run(args[1], args[2]);
else if (args[0] == "erscene") ErScene.Run(args[1], args[2]);
else if (args[0] == "erld") ErLightDiff.Run(args[1]);
else if (args[0] == "ergp") ErGparam.Run(args[1], args[2], args.Length > 3 ? args[3] : null, args.Length > 4 ? args[4] : null);
else if (args[0] == "units") Units.Run(args[1], args.Skip(2).ToArray());
else if (args[0] == "func") Func.Dump(args[1], Convert.ToUInt64(args[2], 16));
else if (args[0] == "callers") Callers.Find(args[1], args[2].Split(",").Select(x => Convert.ToUInt64(x, 16)).ToArray(), int.Parse(args[3]), int.Parse(args[4]), int.Parse(args[5]));
else if (args[0] == "xref") Xref.Find(args[1], args[2].Split(",").Select(x => Convert.ToInt64(x, 16)).ToArray(), int.Parse(args[3]), int.Parse(args[4]));
else if (args[0] == "strings") Strings.Run(args[1], args[2]);
else if (args[0] == "raw") Raw.Run(args[1], args[2], args[3], int.Parse(args[4]));
else if (args[0] == "match") Match.Run(args[1], args[2], args[3]);
else if (args[0] == "msb") Msb.Run(args[1], args[2], args[3]);
else if (args[0] == "shaders") Shaders.Extract(args[1], args[2]);
else if (args[0] == "rt") RoundTrip.Run(args[1], args[2]);
else if (args[0] == "decode") Decode.Run(args[1], args.Skip(2).ToArray());
else if (args[0] == "er") ErSurvey.Run(args[1]);
else if (args[0] == "survey") Survey.Run(args[1], args[2]);
else if (args[0] == "extract")
{
    var bnd = BND4.Read(args[1]);
    Directory.CreateDirectory(args[2]);
    foreach (var f in bnd.Files)
        File.WriteAllBytes(Path.Combine(args[2], Path.GetFileName(f.Name)), f.Bytes.ToArray());
}
else if (args[0] == "tpf")
{
    foreach (var p in args.Skip(1))
    {
        void Dump(TPF t, string label)
        {
            foreach (var tx in t.Textures)
            {
                var h = tx.Header;
                Console.WriteLine($"{label}: {tx.Name} fmt={tx.Format} type={tx.Type} mips={tx.Mipmaps} size={tx.Bytes.Length} hdr={(h == null ? "-" : $"{h.Width}x{h.Height} tc={h.TextureCount}")} dds={DdsInfo(tx.Bytes.ToArray())}");
            }
        }
        if (p.Contains("tpfbnd"))
        {
            var bnd = BND4.Read(p);
            foreach (var f in bnd.Files) Dump(TPF.Read(f.Bytes), Path.GetFileName(p) + "/" + Path.GetFileName(f.Name));
        }
        else Dump(TPF.Read(p), Path.GetFileName(p));
    }
}
static string DdsInfo(byte[] b)
{
    if (b.Length < 128 || BitConverter.ToInt32(b, 0) != 0x20534444) return "nodds";
    int h = BitConverter.ToInt32(b, 12), w = BitConverter.ToInt32(b, 16), d = BitConverter.ToInt32(b, 24);
    string four = System.Text.Encoding.ASCII.GetString(b, 84, 4);
    string dx = four == "DX10" ? $" dxgi={BitConverter.ToInt32(b, 128)} dim={BitConverter.ToInt32(b, 132)} arr={BitConverter.ToInt32(b, 140)}" : "";
    return $"{w}x{h}x{d} {four}{dx}";
}
