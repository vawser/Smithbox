using SoulsFormats;
using System;
using System.Linq;

// gpdump param <ER|NR|AC6> <regulation.bin> <def.xml> <paramName> [fieldRegex]
public static class ParamDump
{
    public static void Run(string[] args)
    {
        var bnd = args[1] switch { "NR" => SFUtil.DecryptNightreignRegulation(args[2]), "AC6" => SFUtil.DecryptAC6Regulation(args[2]), _ => SFUtil.DecryptERRegulation(args[2]) };
        var def = PARAMDEF.XmlDeserialize(args[3]);
        var f = bnd.Files.FirstOrDefault(x => System.IO.Path.GetFileName(x.Name).Equals(args[4] + ".param", StringComparison.OrdinalIgnoreCase));
        if (f == null) { foreach (var x in bnd.Files.Where(x => x.Name.Contains(args[4].Substring(0, 6), StringComparison.OrdinalIgnoreCase))) Console.WriteLine(x.Name); return; }
        var p = PARAM.Read(f.Bytes);
        p.ApplyParamdef(def);
        var re = args.Length > 5 ? new System.Text.RegularExpressions.Regex(args[5], System.Text.RegularExpressions.RegexOptions.IgnoreCase) : null;
        foreach (var r in p.Rows)
        {
            var cells = r.Cells.Where(c => re == null || re.IsMatch(c.Def.InternalName)).Select(c => $"{c.Def.InternalName}={c.Value}");
            Console.WriteLine($"{r.ID} {r.Name}: {string.Join(" ", cells)}");
        }
    }
}
