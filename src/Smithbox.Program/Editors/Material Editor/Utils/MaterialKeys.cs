using SoulsFormats;
using System.Text;

namespace StudioCore.Editors.MaterialEditor;

/// <summary>
/// MATBIN keys. Every one in ER, NR, AC6 and DB_NT is the Adler-32 of a UTF-16LE string, no terminator:
/// MATBIN.Key of SourcePath, Param.Key of Name, Sampler.Key of Type.
/// See Documentation/Claude/Material Editor Plan.md, R1. No editor dependencies, so tools can link it.
/// </summary>
public static class MaterialKeys
{
    /// <summary>
    /// The key a MATBIN, param or sampler should have for this text
    /// </summary>
    public static uint Compute(string text)
    {
        uint a = 1, b = 0;

        foreach (var x in Encoding.Unicode.GetBytes(text ?? ""))
        {
            a = (a + x) % 65521;
            b = (b + a) % 65521;
        }

        return (b << 16) | a;
    }

    /// <summary>
    /// The name of the text property a MATBIN object's Key is made from, or null if it has no derived key
    /// </summary>
    public static string SourceProperty(object obj) => obj switch
    {
        MATBIN => nameof(MATBIN.SourcePath),
        MATBIN.Param => nameof(MATBIN.Param.Name),
        MATBIN.Sampler => nameof(MATBIN.Sampler.Type),
        _ => null
    };

    /// <summary>
    /// The key this object should have, or null if it has no derived key
    /// </summary>
    public static uint? Expected(object obj) => obj switch
    {
        MATBIN m => Compute(m.SourcePath),
        MATBIN.Param p => Compute(p.Name),
        MATBIN.Sampler s => Compute(s.Type),
        _ => null
    };

    /// <summary>
    /// Sets the key of a MATBIN, param or sampler from its text. Does nothing for anything else.
    /// </summary>
    public static void Apply(object obj)
    {
        switch (obj)
        {
            case MATBIN m: m.Key = Compute(m.SourcePath); break;
            case MATBIN.Param p: p.Key = Compute(p.Name); break;
            case MATBIN.Sampler s: s.Key = Compute(s.Type); break;
        }
    }
}
