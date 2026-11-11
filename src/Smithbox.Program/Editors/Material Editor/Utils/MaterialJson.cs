using SoulsFormats;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StudioCore.Editors.MaterialEditor;

/// <summary>
/// A material as JSON (Material Editor plan, F9), for export, import and the clipboard. Readable and editable by hand:
/// params give their type by name and their value as a number, a list of numbers or a bool. MATBIN keys aren't kept:
/// every one is the Adler-32 of its text (R1), so they're worked out on import. No editor dependencies, so the
/// survey tool can check that every vanilla material comes back the same.
/// </summary>
public class MaterialJson
{
    public const string FormatName = "Smithbox Material";

    public string Format { get; set; } = FormatName;
    public int Version { get; set; } = 1;

    /// <summary>MTD or MATBIN</summary>
    public string Kind { get; set; } = "";

    /// <summary>The material's short name when exported</summary>
    public string Name { get; set; } = "";

    public string ShaderPath { get; set; } = "";

    /// <summary>MATBIN only</summary>
    public string SourcePath { get; set; }

    /// <summary>MTD only</summary>
    public string Description { get; set; }

    public List<ParamJson> Params { get; set; } = new();

    /// <summary>MATBIN only</summary>
    public List<SamplerJson> Samplers { get; set; }

    /// <summary>MTD only</summary>
    public List<TextureJson> Textures { get; set; }

    public class ParamJson
    {
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";

        // One of these, by type
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public bool? Bool { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public int[] Ints { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public float[] Floats { get; set; }
    }

    public class SamplerJson
    {
        public string Type { get; set; } = "";
        public string Path { get; set; } = "";
        public float[] Unk14 { get; set; } = [0, 0];
    }

    public class TextureJson
    {
        public string Type { get; set; } = "";
        public bool Extended { get; set; }
        public int UVNumber { get; set; }
        public int ShaderDataIndex { get; set; }
        public string Path { get; set; } = "";
        public List<float> UnkFloats { get; set; } = new();
    }

    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public string ToText() => JsonSerializer.Serialize(this, Options);

    /// <summary>
    /// The JSON of a material file, or null if the text isn't one
    /// </summary>
    public static MaterialJson FromText(string text)
    {
        try
        {
            var json = JsonSerializer.Deserialize<MaterialJson>(text ?? "", Options);
            return json?.Format == FormatName && json.Kind is "MTD" or "MATBIN" ? json : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    #region From a material
    public static MaterialJson From(string name, object material)
    {
        switch (material)
        {
            case MTD m:
                return new MaterialJson
                {
                    Kind = "MTD",
                    Name = name,
                    ShaderPath = m.ShaderPath,
                    Description = m.Description,
                    Params = m.Params.Select(p => Param(p.Name, p.Type.ToString(), p.Value)).ToList(),
                    Textures = m.Textures.Select(t => new TextureJson
                    {
                        Type = t.Type,
                        Extended = t.Extended,
                        UVNumber = t.UVNumber,
                        ShaderDataIndex = t.ShaderDataIndex,
                        Path = t.Path ?? "",
                        UnkFloats = new List<float>(t.UnkFloats)
                    }).ToList()
                };

            case MATBIN m:
                return new MaterialJson
                {
                    Kind = "MATBIN",
                    Name = name,
                    ShaderPath = m.ShaderPath,
                    SourcePath = m.SourcePath,
                    Params = m.Params.Select(p => Param(p.Name, p.Type.ToString(), p.Value)).ToList(),
                    Samplers = m.Samplers.Select(s => new SamplerJson
                    {
                        Type = s.Type,
                        Path = s.Path ?? "",
                        Unk14 = [s.Unk14.X, s.Unk14.Y]
                    }).ToList()
                };

            default:
                return null;
        }
    }

    private static ParamJson Param(string name, string type, object value)
    {
        var param = new ParamJson { Name = name, Type = type };

        switch (value)
        {
            case bool b: param.Bool = b; break;
            case int i: param.Ints = [i]; break;
            case int[] ints: param.Ints = (int[])ints.Clone(); break;
            case float f: param.Floats = [f]; break;
            case float[] floats: param.Floats = (float[])floats.Clone(); break;
            case Enum e: param.Ints = [Convert.ToInt32(e)]; break;
        }

        return param;
    }
    #endregion

    #region To a material
    /// <summary>
    /// A new material of the JSON's kind. Null with an error when a param's type or value doesn't fit.
    /// </summary>
    public object ToMaterial(out string error)
    {
        error = null;

        if (Kind == "MTD")
        {
            var mtd = new MTD { ShaderPath = ShaderPath ?? "", Description = Description ?? "" };

            foreach (var p in Params)
            {
                if (!Enum.TryParse<MTD.ParamType>(p.Type, true, out var type) || type == MTD.ParamType.None ||
                    !TryValue(p, type.ToString(), out var value))
                {
                    error = p.Name;
                    return null;
                }

                mtd.Params.Add(new MTD.Param(p.Name, type, value));
            }

            foreach (var t in Textures ?? new())
            {
                mtd.Textures.Add(new MTD.Texture
                {
                    Type = t.Type ?? "",
                    Extended = t.Extended,
                    UVNumber = t.UVNumber,
                    ShaderDataIndex = t.ShaderDataIndex,
                    Path = t.Path ?? "",
                    UnkFloats = new List<float>(t.UnkFloats ?? new())
                });
            }

            return mtd;
        }

        if (Kind == "MATBIN")
        {
            var matbin = new MATBIN { ShaderPath = ShaderPath ?? "", SourcePath = SourcePath ?? "" };
            MaterialKeys.Apply(matbin);

            foreach (var p in Params)
            {
                if (!Enum.TryParse<MATBIN.ParamType>(p.Type, true, out var type) || type == MATBIN.ParamType.None ||
                    !TryValue(p, type.ToString(), out var value))
                {
                    error = p.Name;
                    return null;
                }

                var param = new MATBIN.Param { Name = p.Name ?? "", Type = type };
                param.Value = value;
                MaterialKeys.Apply(param);
                matbin.Params.Add(param);
            }

            foreach (var s in Samplers ?? new())
            {
                var unk = s.Unk14 is { Length: >= 2 } u ? new Vector2(u[0], u[1]) : Vector2.Zero;
                var sampler = new MATBIN.Sampler { Type = s.Type ?? "", Path = s.Path ?? "", Unk14 = unk };
                MaterialKeys.Apply(sampler);
                matbin.Samplers.Add(sampler);
            }

            return matbin;
        }

        error = Kind;
        return null;
    }

    /// <summary>
    /// The value a param type needs from the JSON: Bool, Int(s) or Float(s) of the type's length
    /// </summary>
    private static bool TryValue(ParamJson p, string type, out object value)
    {
        value = null;

        int Length(string name) => name switch
        {
            "Int2" or "Float2" => 2,
            "Float3" => 3,
            "Float4" => 4,
            "Float5" => 5,
            _ => 1
        };

        switch (type)
        {
            case "Bool":
                if (p.Bool == null) return false;
                value = p.Bool.Value;
                return true;

            case "Int":
                if (p.Ints is not { Length: 1 }) return false;
                value = p.Ints[0];
                return true;

            case "Int2":
                if (p.Ints?.Length != 2) return false;
                value = (int[])p.Ints.Clone();
                return true;

            case "Float":
                if (p.Floats is not { Length: 1 }) return false;
                value = p.Floats[0];
                return true;

            default:
                if (p.Floats?.Length != Length(type)) return false;
                value = (float[])p.Floats.Clone();
                return true;
        }
    }
    #endregion
}
