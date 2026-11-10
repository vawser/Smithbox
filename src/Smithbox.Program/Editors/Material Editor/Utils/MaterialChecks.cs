using SoulsFormats;

namespace StudioCore.Editors.MaterialEditor;

public enum MaterialCheckSeverity
{
    /// <summary>
    /// The file can't be written, or writes something the game can't read
    /// </summary>
    Error,

    /// <summary>
    /// The file writes, but differs from how every vanilla material is made
    /// </summary>
    Warning
}

/// <summary>
/// One problem with a material. <see cref="Code"/> names the localized message (MAT_Check_{Code}), filled with
/// <see cref="Args"/>; <see cref="Target"/> is the param, sampler or texture it's about, for searching to it.
/// </summary>
public record MaterialFinding(MaterialCheckSeverity Severity, string Code, string Target, params object[] Args);

/// <summary>
/// Checks for what makes a material file invalid or unlike vanilla. No editor dependencies, so the survey tool
/// can run it over game data: vanilla files should give no findings.
/// </summary>
public static class MaterialChecks
{
    public static List<MaterialFinding> Check(object material) => material switch
    {
        MTD mtd => Check(mtd),
        MATBIN matbin => Check(matbin),
        _ => new()
    };

    public static List<MaterialFinding> Check(MTD mtd)
    {
        var findings = new List<MaterialFinding>();

        if (string.IsNullOrWhiteSpace(mtd.ShaderPath))
            findings.Add(new(MaterialCheckSeverity.Warning, "Empty_Shader_Path", "ShaderPath"));

        foreach (var p in mtd.Params)
        {
            var name = p.Name ?? "";

            if (p.Type == MTD.ParamType.None)
                findings.Add(new(MaterialCheckSeverity.Error, "Param_Type_None", name, name));
            else if (!ValueFits(p.Value, MtdShape(p.Type)))
                findings.Add(new(MaterialCheckSeverity.Error, "Param_Value_Mismatch", name, name, p.Type, Describe(p.Value)));

            if (string.IsNullOrWhiteSpace(name))
                findings.Add(new(MaterialCheckSeverity.Warning, "Param_Empty_Name", ""));
        }

        foreach (var dupe in Duplicates(mtd.Params.Select(p => p.Name)))
            findings.Add(new(MaterialCheckSeverity.Warning, "Param_Duplicate", dupe, dupe));

        foreach (var t in mtd.Textures.Where(t => string.IsNullOrWhiteSpace(t.Type)))
            findings.Add(new(MaterialCheckSeverity.Warning, "Texture_Empty_Type", ""));

        foreach (var dupe in Duplicates(mtd.Textures.Select(t => t.Type)))
            findings.Add(new(MaterialCheckSeverity.Warning, "Texture_Duplicate", dupe, dupe));

        return findings;
    }

    public static List<MaterialFinding> Check(MATBIN matbin)
    {
        var findings = new List<MaterialFinding>();

        if (string.IsNullOrWhiteSpace(matbin.ShaderPath))
            findings.Add(new(MaterialCheckSeverity.Warning, "Empty_Shader_Path", "ShaderPath"));

        if (string.IsNullOrWhiteSpace(matbin.SourcePath))
            findings.Add(new(MaterialCheckSeverity.Warning, "Empty_Source_Path", "SourcePath"));

        if (matbin.Key != MaterialKeys.Compute(matbin.SourcePath))
            findings.Add(new(MaterialCheckSeverity.Warning, "Stale_Key", "SourcePath", "SourcePath", $"{matbin.Key:X8}", $"{MaterialKeys.Compute(matbin.SourcePath):X8}"));

        foreach (var p in matbin.Params)
        {
            var name = p.Name ?? "";

            if (p.Type == MATBIN.ParamType.None)
                findings.Add(new(MaterialCheckSeverity.Error, "Param_Type_None", name, name));
            else if (!ValueFits(p.Value, MatbinShape(p.Type)))
                findings.Add(new(MaterialCheckSeverity.Error, "Param_Value_Mismatch", name, name, p.Type, Describe(p.Value)));

            if (string.IsNullOrWhiteSpace(name))
                findings.Add(new(MaterialCheckSeverity.Warning, "Param_Empty_Name", ""));

            if (p.Key != MaterialKeys.Compute(p.Name))
                findings.Add(new(MaterialCheckSeverity.Warning, "Stale_Key", name, name, $"{p.Key:X8}", $"{MaterialKeys.Compute(p.Name):X8}"));
        }

        foreach (var dupe in Duplicates(matbin.Params.Select(p => p.Name)))
            findings.Add(new(MaterialCheckSeverity.Warning, "Param_Duplicate", dupe, dupe));

        foreach (var s in matbin.Samplers)
        {
            var type = s.Type ?? "";

            if (string.IsNullOrWhiteSpace(type))
                findings.Add(new(MaterialCheckSeverity.Warning, "Sampler_Empty_Type", ""));

            if (s.Key != MaterialKeys.Compute(s.Type))
                findings.Add(new(MaterialCheckSeverity.Warning, "Stale_Key", type, type, $"{s.Key:X8}", $"{MaterialKeys.Compute(s.Type):X8}"));
        }

        foreach (var dupe in Duplicates(matbin.Samplers.Select(s => s.Type)))
            findings.Add(new(MaterialCheckSeverity.Warning, "Sampler_Duplicate", dupe, dupe));

        return findings;
    }

    // What a param's Value must be for its type, as MTD.Param.Write / MATBIN.Param.WriteData cast it
    private static (Type type, int length) MtdShape(MTD.ParamType type) => type switch
    {
        MTD.ParamType.Bool => (typeof(bool), 0),
        MTD.ParamType.Int => (typeof(int), 0),
        MTD.ParamType.Int2 => (typeof(int[]), 2),
        MTD.ParamType.Float => (typeof(float), 0),
        MTD.ParamType.Float2 => (typeof(float[]), 2),
        MTD.ParamType.Float3 => (typeof(float[]), 3),
        MTD.ParamType.Float4 => (typeof(float[]), 4),
        _ => (null, 0)
    };

    private static (Type type, int length) MatbinShape(MATBIN.ParamType type) => type switch
    {
        MATBIN.ParamType.Bool => (typeof(bool), 0),
        MATBIN.ParamType.Int => (typeof(int), 0),
        MATBIN.ParamType.Int2 => (typeof(int[]), 2),
        MATBIN.ParamType.Float => (typeof(float), 0),
        MATBIN.ParamType.Float2 => (typeof(float[]), 2),
        MATBIN.ParamType.Float3 => (typeof(float[]), 3),
        MATBIN.ParamType.Float4 => (typeof(float[]), 4),
        MATBIN.ParamType.Float5 => (typeof(float[]), 5),
        _ => (null, 0)
    };

    private static bool ValueFits(object value, (Type type, int length) shape)
    {
        if (shape.type == null || value == null)
            return false;

        if (shape.type.IsArray)
            return value.GetType() == shape.type && ((Array)value).Length == shape.length;

        // An int-backed enum unboxes as an int, which is how the writer reads it
        if (shape.type == typeof(int) && value.GetType().IsEnum)
            return Enum.GetUnderlyingType(value.GetType()) == typeof(int);

        return value.GetType() == shape.type;
    }

    private static string Describe(object value) => value switch
    {
        null => "null",
        Array a => $"{value.GetType().GetElementType()?.Name}[{a.Length}]",
        _ => value.GetType().Name
    };

    private static IEnumerable<string> Duplicates(IEnumerable<string> names) =>
        names.Where(n => !string.IsNullOrWhiteSpace(n))
            .GroupBy(n => n, StringComparer.Ordinal)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key);
}
