using SoulsFormats;
using System.Numerics;

namespace StudioCore.Editors.MaterialEditor;

/// <summary>
/// One param as the materials of a shader have it: the commonest type, and the commonest value of that type
/// </summary>
public record TemplateParam(string Name, object Type, object Value, int Count);

/// <summary>
/// One sampler (MATBIN) or texture (MTD) as the materials of a shader have it
/// </summary>
/// <remarks>For MTD, <see cref="Texture"/> is the commonest form of the texture (UV number, shader data index, extended
/// path and floats), which a new one copies</remarks>
public record TemplateSampler(string Type, Vector2 Unk14, int Count, MTD.Texture Texture = null);

/// <summary>
/// What the materials of one shader path have, for "Add missing" and for new materials
/// </summary>
public class MaterialTemplate
{
    public string ShaderPath = "";
    public int MaterialCount;
    public List<TemplateParam> Params = new();
    public List<TemplateSampler> Samplers = new();
}

/// <summary>
/// A census of every loaded material by shader path, built once in the background. In vanilla ER/NR/AC6 most shaders'
/// materials share one param set (Documentation/Claude/Material Editor Plan.md, F2), so it stands for what a material
/// of that shader is expected to have. Built from the bank as loaded, so a project's own materials count too.
/// </summary>
public class MaterialTemplates
{
    private readonly MaterialBank Bank;

    private Task BuildTask;
    private Dictionary<(MaterialSourceType, string), MaterialTemplate> Templates;

    public MaterialTemplates(MaterialBank bank)
    {
        Bank = bank;
    }

    /// <summary>
    /// The template for a shader path, or null while the census is being built (it starts on first use)
    /// </summary>
    public MaterialTemplate Get(MaterialSourceType type, string shaderPath)
    {
        if (Templates == null)
        {
            Start();
            return null;
        }

        return Templates.TryGetValue((type, (shaderPath ?? "").ToLowerInvariant()), out var template) ? template : null;
    }

    public bool IsReady => Templates != null;

    private void Start()
    {
        if (BuildTask != null)
            return;

        // Snapshot on the calling (main) thread: the census reads the material objects, which edits change.
        // Values are copied so the templates share no arrays with the materials.
        var snapshot = new List<(MaterialSourceType type, string shader, List<(string name, object type, object value)> parms, List<(string type, Vector2 unk, MTD.Texture texture)> samplers)>();

        foreach (var binder in Bank.MTDs.Values)
        {
            foreach (var file in binder.Entries.Values)
            {
                var m = file.MTD;
                snapshot.Add((MaterialSourceType.MTD, m.ShaderPath,
                    m.Params.Select(p => (p.Name, (object)p.Type, CopyValue(p.Value))).ToList(),
                    m.Textures.Select(t => (t.Type, Vector2.Zero, CopyTexture(t))).ToList()));
            }
        }

        foreach (var binder in Bank.MATBINs.Values)
        {
            foreach (var file in binder.Entries.Values)
            {
                var m = file.MATBIN;
                snapshot.Add((MaterialSourceType.MATBIN, m.ShaderPath,
                    m.Params.Select(p => (p.Name, (object)p.Type, CopyValue(p.Value))).ToList(),
                    m.Samplers.Select(s => (s.Type, s.Unk14, (MTD.Texture)null)).ToList()));
            }
        }

        BuildTask = Task.Run(() =>
        {
            var result = new Dictionary<(MaterialSourceType, string), MaterialTemplate>();

            foreach (var group in snapshot.GroupBy(x => (x.type, (x.shader ?? "").ToLowerInvariant())))
            {
                var materials = group.ToList();

                var template = new MaterialTemplate
                {
                    ShaderPath = materials[0].shader ?? "",
                    MaterialCount = materials.Count
                };

                // Params in the order they first appear, with the commonest type and value
                foreach (var byName in materials.SelectMany(m => m.parms).GroupBy(p => p.name, StringComparer.Ordinal))
                {
                    var commonType = byName.GroupBy(p => p.type).OrderByDescending(g => g.Count()).First();
                    var commonValue = commonType.GroupBy(p => ValueKey(p.value)).OrderByDescending(g => g.Count()).First().First().value;

                    template.Params.Add(new TemplateParam(byName.Key, commonType.Key, commonValue, byName.Count()));
                }

                foreach (var byType in materials.SelectMany(m => m.samplers).GroupBy(s => s.type, StringComparer.Ordinal))
                {
                    var commonUnk = byType.GroupBy(s => s.unk).OrderByDescending(g => g.Count()).First().Key;

                    var commonTexture = byType.Where(s => s.texture != null)
                        .GroupBy(s => TextureKey(s.texture)).OrderByDescending(g => g.Count()).FirstOrDefault()?.First().texture;

                    template.Samplers.Add(new TemplateSampler(byType.Key, commonUnk, byType.Count(), commonTexture));
                }

                result[group.Key] = template;
            }

            Templates = result;
        });
    }

    /// <summary>
    /// The params and samplers every other material of this shader has and this one doesn't. Nothing when the
    /// material is the only one of its shader, or the census isn't ready.
    /// </summary>
    public (List<TemplateParam> parms, List<TemplateSampler> samplers) Missing(MaterialSourceType type, object material)
    {
        var (shader, names, samplerTypes) = material switch
        {
            MTD m => (m.ShaderPath, m.Params.Select(p => p.Name).ToHashSet(), m.Textures.Select(t => t.Type).ToHashSet()),
            MATBIN m => (m.ShaderPath, m.Params.Select(p => p.Name).ToHashSet(), m.Samplers.Select(s => s.Type).ToHashSet()),
            _ => (null, new HashSet<string>(), new HashSet<string>())
        };

        var template = shader == null ? null : Get(type, shader);

        // Counts include this material when it has the entry; "every other" is all of them, less this one if it's counted
        if (template == null || template.MaterialCount < 2)
            return (new(), new());

        var others = template.MaterialCount - 1;

        var parms = template.Params.Where(p => !names.Contains(p.Name) && p.Count >= others).ToList();
        var samplers = template.Samplers.Where(s => !samplerTypes.Contains(s.Type) && s.Count >= others).ToList();

        return (parms, samplers);
    }

    public static object CopyValue(object value) => value is Array a ? a.Clone() : value;

    /// <summary>
    /// A copy of an MTD texture with every field, so a template shares nothing with the material
    /// </summary>
    public static MTD.Texture CopyTexture(MTD.Texture t) => new()
    {
        Type = t.Type,
        Extended = t.Extended,
        UVNumber = t.UVNumber,
        ShaderDataIndex = t.ShaderDataIndex,
        Path = t.Path,
        UnkFloats = new List<float>(t.UnkFloats)
    };

    private static string TextureKey(MTD.Texture t) =>
        $"{t.Extended}|{t.UVNumber}|{t.ShaderDataIndex}|{t.Path}|{string.Join(",", t.UnkFloats)}";

    private static string ValueKey(object value) => value is Array a ? string.Join(",", a.Cast<object>()) : $"{value}";
}
