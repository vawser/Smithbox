using SoulsFormats;
using System.Collections;
using System.Globalization;

namespace StudioCore.Editors.MaterialEditor;

public enum MaterialComparison
{
    Contains,
    Equals,
    LessThan,
    GreaterThan
}

/// <summary>
/// A material search (Material Editor plan, F8). Every text given must match: the material's short name, its shader
/// path, and then one of its entries (a param by name, a sampler or MTD texture by type) by name, value and path.
/// Text matches by containing the input (case-insensitive), or equalling it when <see cref="Exact"/>. Values compare
/// as numbers for the other comparisons, an array matching when any of its numbers does.
/// </summary>
public record MaterialQuery(string Material, string Shader, string Entry, string Value, string Path,
    MaterialComparison Comparison, bool Exact)
{
    public bool HasMaterial => !string.IsNullOrWhiteSpace(Material);
    public bool HasShader => !string.IsNullOrWhiteSpace(Shader);
    public bool HasEntry => !string.IsNullOrWhiteSpace(Entry);
    public bool HasValue => !string.IsNullOrWhiteSpace(Value);
    public bool HasPath => !string.IsNullOrWhiteSpace(Path);

    /// <summary>Whether entries are matched; if not, each matching material is a result of its own</summary>
    public bool MatchesEntries => HasEntry || HasValue || HasPath;

    public bool HasAny => HasMaterial || HasShader || MatchesEntries;
}

/// <summary>
/// The matching of <see cref="MaterialQuery"/>, and the text forms of values. No editor dependencies, so the
/// survey tool can link it.
/// </summary>
public static class MaterialQueryMatcher
{
    /// <summary>
    /// A material's entries with the list they're in: Params, then Samplers (MATBIN) or Textures (MTD)
    /// </summary>
    public static IEnumerable<(object entry, string list)> Entries(object material) => material switch
    {
        MTD m => m.Params.Select(p => ((object)p, nameof(MTD.Params))).Concat(m.Textures.Select(t => ((object)t, nameof(MTD.Textures)))),
        MATBIN m => m.Params.Select(p => ((object)p, nameof(MATBIN.Params))).Concat(m.Samplers.Select(s => ((object)s, nameof(MATBIN.Samplers)))),
        _ => Enumerable.Empty<(object, string)>()
    };

    public static string ShaderOf(object material) => material switch
    {
        MTD m => m.ShaderPath,
        MATBIN m => m.ShaderPath,
        _ => ""
    };

    public static bool MatchMaterial(MaterialQuery query, string shortName, object material)
    {
        return MatchText(query, query.Material, shortName) && MatchText(query, query.Shader, ShaderOf(material));
    }

    public static bool MatchEntry(MaterialQuery query, object entry)
    {
        (string name, object value, string path) = entry switch
        {
            MTD.Param p => (p.Name, p.Value, (string)null),
            MATBIN.Param p => (p.Name, p.Value, (string)null),
            MATBIN.Sampler s => (s.Type, (object)null, s.Path),
            MTD.Texture t => (t.Type, (object)null, t.Path),
            _ => ((string)null, (object)null, (string)null)
        };

        if (name == null)
            return false;

        if (!MatchText(query, query.Entry, name))
            return false;

        // A value asks for a param, a path for a sampler or texture
        if (query.HasValue && (value == null || !MatchValue(query, value)))
            return false;

        if (query.HasPath && (path == null || !MatchText(query, query.Path, path)))
            return false;

        return true;
    }

    public static bool MatchValue(MaterialQuery query, object value)
    {
        if (query.Comparison is MaterialComparison.Contains)
        {
            // Exact numbers compare as numbers, so 1 finds 1.0
            if (query.Exact && TryParseNumber(query.Value, out var wanted) && Numbers(value) is { Length: 1 } single)
                return single[0] == wanted;

            return MatchText(query, query.Value, ValueText(value));
        }

        if (!TryParseNumber(query.Value, out var target))
            return false;

        var numbers = Numbers(value);

        return query.Comparison switch
        {
            MaterialComparison.Equals => numbers.Any(n => n == target),
            MaterialComparison.LessThan => numbers.Any(n => n < target),
            MaterialComparison.GreaterThan => numbers.Any(n => n > target),
            _ => false
        };
    }

    public static bool MatchText(MaterialQuery query, string input, string text)
    {
        if (string.IsNullOrWhiteSpace(input))
            return true;

        if (string.IsNullOrEmpty(text))
            return false;

        return query.Exact
            ? text.Trim().Equals(input.Trim(), StringComparison.OrdinalIgnoreCase)
            : text.Contains(input.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// A value as text: numbers in invariant form, arrays comma separated. <see cref="MaterialValueEdit"/> reads
    /// this form back.
    /// </summary>
    public static string ValueText(object value) => value switch
    {
        null => "",
        bool b => b ? "true" : "false",
        float f => f.ToString("R", CultureInfo.InvariantCulture),
        IFormattable x when value is not Array => x.ToString(null, CultureInfo.InvariantCulture),
        float[] a => string.Join(", ", a.Select(f => f.ToString("R", CultureInfo.InvariantCulture))),
        int[] a => string.Join(", ", a.Select(i => i.ToString(CultureInfo.InvariantCulture))),
        IEnumerable e => string.Join(", ", e.Cast<object>()),
        _ => $"{value}"
    };

    /// <summary>
    /// The numbers of a value; a bool is 0 or 1
    /// </summary>
    public static double[] Numbers(object value) => value switch
    {
        bool b => [b ? 1 : 0],
        int i => [i],
        float f => [f],
        int[] a => a.Select(x => (double)x).ToArray(),
        float[] a => a.Select(x => (double)x).ToArray(),
        _ when value != null && value.GetType().IsEnum => [Convert.ToInt32(value)],
        _ => []
    };

    public static bool TryParseNumber(string text, out double number) =>
        double.TryParse((text ?? "").Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out number);
}

/// <summary>
/// The mass edit of param values (F8): Set a value in the form <see cref="MaterialQueryMatcher.ValueText"/> writes
/// (one number for every component, or one per component), or Add / Multiply numbers, per component. The result
/// always has the type and length the value had, so the param still writes (MaterialChecks).
/// </summary>
public static class MaterialValueEdit
{
    public enum Operation { Set, Add, Multiply }

    public static bool TryApply(object current, Operation operation, string input, out object result)
    {
        result = null;

        if (current is bool b)
        {
            if (operation != Operation.Set)
                return false;

            if (TryParseBool(input, out var parsed))
            {
                result = parsed;
                return true;
            }

            return false;
        }

        var parts = (input ?? "").Split(new[] { ',', ' ', ';' }, StringSplitOptions.RemoveEmptyEntries);
        var numbers = new double[parts.Length];

        for (var i = 0; i < parts.Length; i++)
        {
            if (!MaterialQueryMatcher.TryParseNumber(parts[i], out numbers[i]))
                return false;
        }

        if (numbers.Length == 0)
            return false;

        var old = MaterialQueryMatcher.Numbers(current);

        // One number for all components, or one each
        if (numbers.Length != 1 && numbers.Length != old.Length)
            return false;

        double Operand(int i) => numbers.Length == 1 ? numbers[0] : numbers[i];

        double Apply(int i) => operation switch
        {
            Operation.Set => Operand(i),
            Operation.Add => old[i] + Operand(i),
            Operation.Multiply => old[i] * Operand(i),
            _ => old[i]
        };

        switch (current)
        {
            case int:
                result = (int)Math.Round(Apply(0));
                return true;

            case float:
                result = (float)Apply(0);
                return true;

            case int[] ints:
                result = Enumerable.Range(0, ints.Length).Select(i => (int)Math.Round(Apply(i))).ToArray();
                return true;

            case float[] floats:
                result = Enumerable.Range(0, floats.Length).Select(i => (float)Apply(i)).ToArray();
                return true;

            default:
                return false;
        }
    }

    private static bool TryParseBool(string text, out bool value)
    {
        text = (text ?? "").Trim();

        if (bool.TryParse(text, out value))
            return true;

        if (text == "1") { value = true; return true; }
        if (text == "0") { value = false; return true; }

        return false;
    }

    /// <summary>
    /// A path with every occurrence of a text replaced, ignoring case; null if the text doesn't occur
    /// </summary>
    public static string ReplaceInPath(string path, string find, string replace)
    {
        if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(find) ||
            path.IndexOf(find, StringComparison.OrdinalIgnoreCase) < 0)
            return null;

        return path.Replace(find, replace ?? "", StringComparison.OrdinalIgnoreCase);
    }
}
