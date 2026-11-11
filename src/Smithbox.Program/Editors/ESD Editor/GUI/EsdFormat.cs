using SoulsFormats;
using System.Globalization;
using System.Numerics;

namespace StudioCore.Editors.EsdEditor;

/// <summary>
/// Names, text and colours shared by the ESD Editor's windows
/// </summary>
public static class EsdFormat
{
    /// <summary>A command's name: the catalogue's, else bank:id</summary>
    public static string CommandName(EsdKindCatalog cat, ESD.CommandCall c)
    {
        return EsdLayout.KindOf(c) switch
        {
            EsdCommandKind.CallMachine => LOC.Get("ESD_Command_Call", EsdLayout.MachineLabel(c.CommandID)),
            EsdCommandKind.Return => LOC.Get("ESD_Command_Return"),
            _ => cat?.Command(c.CommandBank, c.CommandID)?.Name ?? $"{c.CommandBank}:{c.CommandID}"
        };
    }

    public static string FunctionName(EsdKindCatalog cat, long id)
    {
        return cat?.Function(id)?.Name ?? $"f{id}";
    }

    /// <summary>A machine's label: its id (or x&lt;n&gt; for a sub-machine), with DS2's .edd name when there is one</summary>
    public static string MachineLabel(EsdFileWrapper file, long id)
    {
        var label = EsdLayout.MachineLabel(id);
        return file != null && file.MachineNames.TryGetValue(id, out var name) ? $"{label} {name}" : label;
    }

    /// <summary>
    /// An expression as text, with the catalogue's function names and enum value names for literal enum args.
    /// </summary>
    public static string Text(EsdKindCatalog cat, EsdExprNode n)
    {
        if (n == null)
            return "?";

        switch (n.Kind)
        {
            case EsdExprKind.Call:
                {
                    var id = n.FunctionId;
                    var def = id != null ? cat?.Function(id.Value) : null;
                    var name = id != null ? FunctionName(cat, id.Value) : $"call[{Text(cat, n.Children[0])}]";
                    var args = new List<string>();
                    for (int k = 1; k < n.Children.Count; k++)
                        args.Add(ArgText(cat, def, k - 1, n.Children[k]));
                    return $"{name}({string.Join(", ", args)})";
                }
            case EsdExprKind.Binary:
                {
                    string Side(EsdExprNode c) { var t = Text(cat, c); return EsdExpr.Strip(c).Kind == EsdExprKind.Binary ? $"({t})" : t; }
                    return $"{Side(n.Children[0])} {EsdExpr.BinaryName(n.Op)} {Side(n.Children[1])}";
                }
            case EsdExprKind.Unary when n.Op is 0xA6 or 0xB7:
                return Text(cat, n.Children[0]);
            case EsdExprKind.Unary when n.Op is >= 0xA7 and <= 0xAE:
                return $"(r{n.Op - 0xA7} = {Text(cat, n.Children[0])})";
            case EsdExprKind.Unary when n.Op == 0xB8:
                return $"arg{Text(cat, n.Children[0])}";
            default:
                return EsdExpr.Text(n);
        }
    }

    /// <summary>An arg of a call: its enum value's name when the arg is an enum and the value a literal</summary>
    public static string ArgText(EsdKindCatalog cat, EsdCallDef def, int index, EsdExprNode arg)
    {
        var argDef = def != null && index < def.Args.Count ? def.Args[index] : null;
        if (argDef?.Enum != null && EsdExpr.Constant(arg) is long v && cat?.FindEnum(argDef.Enum)?.ValueName(v) is { } name)
            return name;
        return Text(cat, arg);
    }

    /// <summary>A whole bytecode blob as text, or its hex when it doesn't decode</summary>
    public static string ExprText(EsdKindCatalog cat, byte[] code)
    {
        var node = EsdExpr.Decode(code);
        return node != null ? Text(cat, node) : $"<{Hex(code)}>";
    }

    public static string Hex(byte[] data)
    {
        if (data == null || data.Length == 0)
            return "";
        return Convert.ToHexString(data);
    }

    public static string ListName(EsdCommandList list) => list switch
    {
        EsdCommandList.Entry => LOC.Get("ESD_Section_Entry"),
        EsdCommandList.While => LOC.Get("ESD_Section_While"),
        EsdCommandList.Exit => LOC.Get("ESD_Section_Exit"),
        _ => LOC.Get("ESD_Section_Pass")
    };

    public static string Number(double v) => v.ToString("0.###", CultureInfo.InvariantCulture);

    // Block colours: muted Scratch-like hues that keep white text readable (as the EMEVD Editor's)
    public static readonly Vector4 StateColor = new(0.55f, 0.20f, 0.30f, 1f);
    public static readonly Vector4 StartStateColor = new(0.62f, 0.30f, 0.18f, 1f);
    public static readonly Vector4 SectionColor = new(0.36f, 0.36f, 0.40f, 1f);
    public static readonly Vector4 ConditionColor = new(0.20f, 0.42f, 0.70f, 1f);
    public static readonly Vector4 AlwaysColor = new(0.30f, 0.40f, 0.52f, 1f);
    public static readonly Vector4 CommandColor = new(0.22f, 0.55f, 0.36f, 1f);
    public static readonly Vector4 UnknownCommandColor = new(0.30f, 0.30f, 0.30f, 1f);
    public static readonly Vector4 CallColor = new(0.62f, 0.26f, 0.55f, 1f);
    public static readonly Vector4 ReturnColor = new(0.62f, 0.22f, 0.22f, 1f);

    // Reporter chips inside expressions
    public static readonly Vector4 FunctionChipColor = new(0.16f, 0.52f, 0.56f, 1f);
    public static readonly Vector4 UnknownFunctionChipColor = new(0.34f, 0.40f, 0.42f, 1f);
    public static readonly Vector4 OperatorChipColor = new(0.36f, 0.56f, 0.22f, 1f);
    public static readonly Vector4 LiteralChipColor = new(0.92f, 0.92f, 0.92f, 1f);
    public static readonly Vector4 EnumChipColor = new(0.85f, 0.80f, 0.60f, 1f);
    public static readonly Vector4 ArgChipColor = new(0.95f, 0.75f, 0.25f, 1f);
    public static readonly Vector4 RegisterChipColor = new(0.85f, 0.55f, 0.25f, 1f);
    public static readonly Vector4 StatusChipColor = new(0.55f, 0.55f, 0.58f, 1f);
    public static readonly Vector4 TargetChipColor = new(0.95f, 0.85f, 0.85f, 1f);
    public static readonly Vector4 SharedChipColor = new(0.80f, 0.70f, 0.95f, 1f);

    public static Vector4 Darken(Vector4 c, float f) => new(c.X * f, c.Y * f, c.Z * f, c.W);
}
