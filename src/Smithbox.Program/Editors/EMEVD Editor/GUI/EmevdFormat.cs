using SoulsFormats;
using System.Globalization;
using System.Numerics;

namespace StudioCore.Editors.EmevdEditor;

/// <summary>
/// Text and colours shared by the EMEVD Editor's windows
/// </summary>
public static class EmevdFormat
{
    /// <summary>An arg's value: the enum value's name when it has one, else the number</summary>
    public static string Value(EmevdCatalog catalog, EmevdArg arg)
    {
        if (arg.IsFloat)
            return arg.Float.ToString("0.###", CultureInfo.InvariantCulture);

        var e = catalog?.FindEnum(arg.Def.EnumKey);
        var name = e?.ValueName(arg.Int);

        return string.IsNullOrEmpty(name) ? arg.Int.ToString(CultureInfo.InvariantCulture) : name;
    }

    /// <summary>The number itself, for the Properties window beside the enum name</summary>
    public static string RawValue(EmevdArg arg)
    {
        return arg.IsFloat
            ? arg.Float.ToString("R", CultureInfo.InvariantCulture)
            : arg.Int.ToString(CultureInfo.InvariantCulture);
    }

    public static string ArgName(EmevdArg arg)
    {
        return arg.Repeat > 0 ? $"{arg.Def.DisplayName} {arg.Repeat + 1}" : arg.Def.DisplayName;
    }

    public static string Hex(byte[] data)
    {
        if (data == null || data.Length == 0)
            return "";

        var parts = new List<string>();
        for (var i = 0; i < data.Length; i += 4)
            parts.Add(Convert.ToHexString(data, i, Math.Min(4, data.Length - i)));

        return string.Join(' ', parts);
    }

    public static string RestBehavior(EMEVD.Event.RestBehaviorType rest) => rest switch
    {
        EMEVD.Event.RestBehaviorType.Default => LOC.Get("EMEVD_Rest_Default"),
        EMEVD.Event.RestBehaviorType.Restart => LOC.Get("EMEVD_Rest_Restart"),
        EMEVD.Event.RestBehaviorType.End => LOC.Get("EMEVD_Rest_End"),
        _ => rest.ToString()
    };

    public static string EventLabel(EMEVD.Event ev)
    {
        return string.IsNullOrEmpty(ev.Name) ? ev.ID.ToString(CultureInfo.InvariantCulture) : $"{ev.ID} {ev.Name}";
    }

    // Block colours: muted Scratch-like hues that keep white text readable
    private static readonly Vector4 ConditionColor = new(0.20f, 0.42f, 0.70f, 1f);
    private static readonly Vector4 WaitColor = new(0.72f, 0.52f, 0.12f, 1f);
    private static readonly Vector4 FlowColor = new(0.70f, 0.40f, 0.10f, 1f);
    private static readonly Vector4 EndColor = new(0.62f, 0.22f, 0.22f, 1f);
    private static readonly Vector4 LabelColor = new(0.45f, 0.45f, 0.45f, 1f);
    private static readonly Vector4 CallColor = new(0.62f, 0.26f, 0.55f, 1f);
    private static readonly Vector4 UnknownColor = new(0.30f, 0.30f, 0.30f, 1f);

    // Command banks (2000+) cycle through these
    private static readonly Vector4[] CommandColors =
    {
        new(0.22f, 0.55f, 0.36f, 1f),
        new(0.35f, 0.38f, 0.72f, 1f),
        new(0.16f, 0.52f, 0.56f, 1f),
        new(0.52f, 0.48f, 0.18f, 1f),
        new(0.48f, 0.30f, 0.62f, 1f),
        new(0.58f, 0.36f, 0.26f, 1f),
        new(0.26f, 0.46f, 0.60f, 1f),
        new(0.40f, 0.52f, 0.22f, 1f),
    };

    public static Vector4 BlockColor(EmevdLine line) => line.Kind switch
    {
        EmevdBlockKind.Condition => ConditionColor,
        EmevdBlockKind.Wait => WaitColor,
        EmevdBlockKind.Skip or EmevdBlockKind.Goto or EmevdBlockKind.ControlFlow => FlowColor,
        EmevdBlockKind.End => EndColor,
        EmevdBlockKind.Label => LabelColor,
        EmevdBlockKind.Call => CallColor,
        EmevdBlockKind.Command => CommandColors[Math.Abs(line.Instr.Bank - 2000) % CommandColors.Length],
        _ => UnknownColor
    };

    public static readonly Vector4 HatColor = new(0.55f, 0.20f, 0.30f, 1f);
    public static readonly Vector4 SpanColor = new(0.85f, 0.65f, 0.30f, 1f);
    public static readonly Vector4 GotoSpanColor = new(0.55f, 0.75f, 0.95f, 1f);
    public static readonly Vector4 BrokenSpanColor = new(0.95f, 0.30f, 0.30f, 1f);
    public static readonly Vector4 ParamChipColor = new(0.95f, 0.75f, 0.25f, 1f);

    public static Vector4 Darken(Vector4 c, float f) => new(c.X * f, c.Y * f, c.Z * f, c.W);
}
