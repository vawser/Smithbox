using SoulsFormats;
using System.Text;

namespace StudioCore.Editors.MaterialEditor;

/// <summary>
/// Comparing and copying materials and their entries (params, samplers, MTD textures). Field by field, not by bytes,
/// so a material saved without changes compares the same (the writer drops unreferenced data, R3). No editor
/// dependencies, so the survey tool can link it; <see cref="MaterialVanilla"/> and <see cref="MaterialMerge"/> use it.
/// </summary>
public static class MaterialEntries
{
    #region Signatures
    /// <summary>
    /// Every field of a material as text
    /// </summary>
    public static string Signature(object material)
    {
        var sb = new StringBuilder();

        switch (material)
        {
            case MTD m:
                sb.Append(m.ShaderPath).Append('|').Append(m.Description).Append('\n');
                foreach (var p in m.Params) sb.Append(Signature(p)).Append('\n');
                foreach (var t in m.Textures) sb.Append(Signature(t)).Append('\n');
                break;

            case MATBIN m:
                sb.Append(m.ShaderPath).Append('|').Append(m.SourcePath).Append('|').Append(m.Key).Append('\n');
                foreach (var p in m.Params) sb.Append(Signature(p)).Append('\n');
                foreach (var s in m.Samplers) sb.Append(Signature(s)).Append('\n');
                break;
        }

        return sb.ToString();
    }

    public static string Signature(MTD.Param p) => $"P|{p.Name}|{p.Type}|{ValueText(p.Value)}";
    public static string Signature(MATBIN.Param p) => $"P|{p.Name}|{p.Type}|{p.Key}|{ValueText(p.Value)}";
    public static string Signature(MATBIN.Sampler s) => $"S|{s.Type}|{s.Path}|{s.Key}|{s.Unk14}";
    public static string Signature(MTD.Texture t) =>
        $"T|{t.Type}|{t.Extended}|{t.UVNumber}|{t.ShaderDataIndex}|{t.Path}|{string.Join(",", t.UnkFloats)}";

    /// <summary>
    /// The signature of a param, sampler or texture, or null for anything else
    /// </summary>
    public static string EntrySignature(object entry) => entry switch
    {
        MTD.Param p => Signature(p),
        MATBIN.Param p => Signature(p),
        MATBIN.Sampler s => Signature(s),
        MTD.Texture t => Signature(t),
        _ => null
    };

    /// <summary>
    /// The name an entry is matched by: a param's name, a sampler's or texture's type
    /// </summary>
    public static string EntryName(object entry) => entry switch
    {
        MTD.Param p => p.Name,
        MATBIN.Param p => p.Name,
        MATBIN.Sampler s => s.Type,
        MTD.Texture t => t.Type,
        _ => null
    };

    /// <summary>
    /// What an entry holds, short, for showing three sides of a merge: a param's type and value, a sampler's or
    /// texture's path
    /// </summary>
    public static string EntryText(object entry) => entry switch
    {
        MTD.Param p => $"{p.Type} {ValueText(p.Value)}",
        MATBIN.Param p => $"{p.Type} {ValueText(p.Value)}",
        MATBIN.Sampler s => $"{s.Path} ({s.Unk14.X}, {s.Unk14.Y})",
        MTD.Texture t => $"{t.Path} (UV {t.UVNumber}, data {t.ShaderDataIndex})",
        _ => ""
    };

    public static string ValueText(object v) => v is Array a ? string.Join(",", a.Cast<object>()) : $"{v}";
    #endregion

    #region Copies
    /// <summary>
    /// A copy of a material, through its writer and reader, so it shares nothing with the original
    /// </summary>
    public static object Copy(object material) => material switch
    {
        MTD m => MTD.Read(m.Write()),
        MATBIN m => MATBIN.Read(m.Write()),
        _ => null
    };

    /// <summary>
    /// A copy of a param, sampler or texture
    /// </summary>
    public static object CopyEntry(object entry)
    {
        switch (entry)
        {
            case MTD.Param p:
                return new MTD.Param(p.Name, p.Type, CopyValue(p.Value));

            case MATBIN.Param p:
            {
                var copy = new MATBIN.Param { Name = p.Name, Type = p.Type };
                copy.Value = CopyValue(p.Value);
                copy.Key = p.Key;
                return copy;
            }

            case MATBIN.Sampler s:
                return new MATBIN.Sampler { Type = s.Type, Path = s.Path, Key = s.Key, Unk14 = s.Unk14 };

            case MTD.Texture t:
                return new MTD.Texture
                {
                    Type = t.Type,
                    Extended = t.Extended,
                    UVNumber = t.UVNumber,
                    ShaderDataIndex = t.ShaderDataIndex,
                    Path = t.Path,
                    UnkFloats = new List<float>(t.UnkFloats)
                };

            default:
                return null;
        }
    }

    private static object CopyValue(object value) => value is Array a ? a.Clone() : value;
    #endregion
}
