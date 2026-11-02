using System.Text;

namespace HKLib.ClassGeneration;

/// <summary>
/// Writes the declarations in the same layout as the 2018 generated code.
/// </summary>
internal static class Emitter
{
    private const string Header = "// Automatically Generated";

    #region Classes

    public static void WriteClasses(IEnumerable<Decl> decls, string outputDirectory)
    {
        foreach (Decl decl in decls.Where(x => x.Outer is null))
        {
            StringBuilder sb = new();
            sb.AppendLine(Header);
            sb.AppendLine();

            List<string> usings = Collect(decl, x => x.Usings).Distinct().OrderBy(x => x).ToList();
            foreach (string ns in usings) sb.AppendLine($"using {ns};");
            if (usings.Count > 0) sb.AppendLine();

            sb.AppendLine(decl.Namespace.Length == 0 ? "namespace HKLib.hk2019;" : $"namespace HKLib.hk2019.{decl.Namespace};");
            sb.AppendLine();
            WriteDecl(sb, decl, "");

            string directory = Path.Join(outputDirectory, decl.Namespace.Replace('.', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(directory);
            File.WriteAllText(Path.Join(directory, decl.Name + ".cs"), sb.ToString());
        }
    }

    private static void WriteDecl(StringBuilder sb, Decl decl, string indent)
    {
        string inner = indent + "    ";
        switch (decl.Kind)
        {
            case DeclKind.Enum:
                if (decl.IsFlags) sb.AppendLine($"{indent}[Flags]");
                sb.AppendLine($"{indent}public enum {decl.Name} : {decl.EnumUnderlyingType}");
                sb.AppendLine($"{indent}{{");
                sb.AppendLine(string.Join(",\n",
                    decl.EnumValues.Select(x => $"{inner}{x.Name} = {FormatEnumValue(x.Value, decl.EnumUnderlyingType)}")));
                sb.AppendLine($"{indent}}}");
                return;
            case DeclKind.Interface:
                sb.AppendLine($"{indent}public interface {decl.Name}{decl.GenericSuffix} : {string.Join(", ", decl.BaseTypes.Select(Relative))}");
                break;
            default:
                string partial = decl.Partial ? "partial " : "";
                sb.AppendLine($"{indent}public {partial}class {decl.Name}{decl.GenericSuffix} : {string.Join(", ", decl.BaseTypes.Select(Relative))}");
                break;
        }

        sb.AppendLine($"{indent}{{");

        foreach (FieldDecl field in decl.Fields)
        {
            sb.AppendLine($"{inner}{GetFieldDeclaration(field)}");
            sb.AppendLine();
        }

        if (decl.Nested.Count > 0)
        {
            sb.AppendLine();
            foreach (Decl nested in decl.Nested)
            {
                WriteDecl(sb, nested, inner);
                sb.AppendLine();
            }
        }

        sb.AppendLine($"{indent}}}");
    }

    private static string GetFieldDeclaration(FieldDecl field)
    {
        string type = field.Type.Write(false);
        return field.Type.Shape switch
        {
            Shape.Init => $"public {type} {field.Name} = new();",
            Shape.Fixed => $"public readonly {type} {field.Name} = new {TypeInfo.Materialize(field.Type.Element!, false)}[{string.Join(", ", field.Type.Dimensions)}];",
            _ => $"public {type} {field.Name};"
        };
    }

    private static string FormatEnumValue(long value, string underlyingType)
    {
        // flags with the high bit set are stored as unsigned values
        return underlyingType switch
        {
            "int" when value > int.MaxValue => unchecked((int)value).ToString(),
            "sbyte" when value > sbyte.MaxValue => unchecked((sbyte)value).ToString(),
            "short" when value > short.MaxValue => unchecked((short)value).ToString(),
            _ => value.ToString()
        };
    }

    #endregion

    #region Reflection data

    public static void WriteData(IEnumerable<Decl> decls, string outputDirectory)
    {
        Directory.CreateDirectory(outputDirectory);
        foreach (Decl decl in decls.Where(x => x.Kind == DeclKind.Class))
        {
            List<FieldDecl> fields = GetAllFields(decl, new Dictionary<string, TypeInfo>());

            string instanceType = "HKLib.hk2019." + decl.RelativeName + decl.GenericSuffix;
            string dataName = decl.DataName + decl.GenericSuffix;

            StringBuilder sb = new();
            sb.AppendLine(Header);
            sb.AppendLine();
            sb.AppendLine("using System.Diagnostics.CodeAnalysis;");
            sb.AppendLine();
            sb.AppendLine("namespace HKLib.Reflection.hk2019;");
            sb.AppendLine();
            sb.AppendLine($"internal class {dataName} : HavokData<{instanceType}> ");
            sb.AppendLine("{");

            foreach (FieldDecl field in fields.Where(x => x.Type.Shape == Shape.Fixed))
            {
                sb.AppendLine($"    private static readonly System.Reflection.FieldInfo _{BareName(field)}Info = typeof({instanceType}).GetField(\"{field.Name}\")!;");
            }

            sb.AppendLine($"    public {decl.DataName}(HavokType type, {instanceType} instance) : base(type, instance) {{}}");
            sb.AppendLine();

            sb.AppendLine("    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)");
            sb.AppendLine("    {");
            sb.AppendLine("        value = default;");
            sb.AppendLine("        switch (fieldName)");
            sb.AppendLine("        {");
            foreach (FieldDecl field in fields) WriteGetCase(sb, field);
            sb.AppendLine("            default:");
            sb.AppendLine("            return false;");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine();

            sb.AppendLine("    public override bool TrySetField<TSet>(string fieldName, TSet value)");
            sb.AppendLine("    {");
            sb.AppendLine("        switch (fieldName)");
            sb.AppendLine("        {");
            foreach (FieldDecl field in fields) WriteSetCase(sb, field);
            sb.AppendLine("            default:");
            sb.AppendLine("            return false;");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine("}");

            File.WriteAllText(Path.Join(outputDirectory, decl.DataName + ".cs"), sb.ToString());
        }
    }

    /// <summary>
    /// Fields of the declaration and its base classes, base class fields first. Generic parameters of base classes are
    /// replaced with the arguments the derived class passes to them.
    /// </summary>
    private static List<FieldDecl> GetAllFields(Decl decl, IReadOnlyDictionary<string, TypeInfo> arguments)
    {
        List<FieldDecl> fields = new();
        if (decl.BaseDecl is { Kind: DeclKind.Class } baseDecl)
        {
            Dictionary<string, TypeInfo> baseArguments = new();
            for (int i = 0; i < baseDecl.GenericParameters.Count && i < decl.BaseArguments.Count; i++)
            {
                baseArguments[baseDecl.GenericParameters[i]] = decl.BaseArguments[i].Substitute(arguments);
            }

            fields.AddRange(GetAllFields(baseDecl, baseArguments));
        }

        foreach (FieldDecl field in decl.Fields)
        {
            // a derived field hides a base field of the same name
            fields.RemoveAll(x => x.Name == field.Name);
            fields.Add(new FieldDecl(field.Name, field.Type.Substitute(arguments)));
        }

        return fields;
    }

    private static void WriteGetCase(StringBuilder sb, FieldDecl field)
    {
        WriteCaseLabels(sb, field);
        sb.AppendLine("            {");
        switch (field.Type.Shape)
        {
            case Shape.Nullable:
            case Shape.Generic:
                sb.AppendLine($"                if (instance.{field.Name} is null)");
                sb.AppendLine("                {");
                sb.AppendLine("                    return true;");
                sb.AppendLine("                }");
                sb.AppendLine($"                if (instance.{field.Name} is TGet castValue)");
                sb.AppendLine("                {");
                sb.AppendLine("                    value = castValue;");
                sb.AppendLine("                    return true;");
                sb.AppendLine("                }");
                sb.AppendLine("                return false;");
                break;
            case Shape.Enum:
                string storage = field.Type.Storage!;
                sb.AppendLine($"                if (instance.{field.Name} is TGet castValue)");
                sb.AppendLine("                {");
                sb.AppendLine("                    value = castValue;");
                sb.AppendLine("                    return true;");
                sb.AppendLine("                }");
                sb.AppendLine($"                if (({storage})instance.{field.Name} is TGet {storage}Value)");
                sb.AppendLine("                {");
                sb.AppendLine($"                    value = {storage}Value;");
                sb.AppendLine("                    return true;");
                sb.AppendLine("                }");
                sb.AppendLine("                return false;");
                break;
            default:
                sb.AppendLine($"                if (instance.{field.Name} is not TGet castValue) return false;");
                sb.AppendLine("                value = castValue;");
                sb.AppendLine("                return true;");
                break;
        }

        sb.AppendLine("            }");
    }

    private static void WriteSetCase(StringBuilder sb, FieldDecl field)
    {
        string type = field.Type.Write(true);
        WriteCaseLabels(sb, field);
        sb.AppendLine("            {");
        switch (field.Type.Shape)
        {
            case Shape.Nullable:
            case Shape.Generic:
                sb.AppendLine("                if (value is null)");
                sb.AppendLine("                {");
                sb.AppendLine($"                    instance.{field.Name} = default;");
                sb.AppendLine("                    return true;");
                sb.AppendLine("                }");
                sb.AppendLine($"                if (value is {type.TrimEnd('?')} castValue)");
                sb.AppendLine("                {");
                sb.AppendLine($"                    instance.{field.Name} = castValue;");
                sb.AppendLine("                    return true;");
                sb.AppendLine("                }");
                sb.AppendLine("                return false;");
                break;
            case Shape.Enum:
                string storage = field.Type.Storage!;
                sb.AppendLine($"                if (value is {type} castValue)");
                sb.AppendLine("                {");
                sb.AppendLine($"                    instance.{field.Name} = castValue;");
                sb.AppendLine("                    return true;");
                sb.AppendLine("                }");
                sb.AppendLine($"                if (value is {storage} {storage}Value)");
                sb.AppendLine("                {");
                sb.AppendLine($"                    instance.{field.Name} = ({type}){storage}Value;");
                sb.AppendLine("                    return true;");
                sb.AppendLine("                }");
                sb.AppendLine("                return false;");
                break;
            case Shape.Fixed:
                int[] dimensions = field.Type.Dimensions;
                string lengthCheck = dimensions.Length == 1
                    ? $"castValue.Length != {dimensions[0]}"
                    : string.Join(" || ", dimensions.Select((x, i) => $"castValue.GetLength({i}) != {x}"));
                sb.AppendLine($"                if (value is not {type} castValue || {lengthCheck}) return false;");
                sb.AppendLine("                try");
                sb.AppendLine("                {");
                sb.AppendLine($"                    _{BareName(field)}Info.SetValue(instance, value);");
                sb.AppendLine("                    return true;");
                sb.AppendLine("                }");
                sb.AppendLine("                catch");
                sb.AppendLine("                {");
                sb.AppendLine("                    return false;");
                sb.AppendLine("                }");
                break;
            default:
                sb.AppendLine($"                if (value is not {type} castValue) return false;");
                sb.AppendLine($"                instance.{field.Name} = castValue;");
                sb.AppendLine("                return true;");
                break;
        }

        sb.AppendLine("            }");
    }

    private static void WriteCaseLabels(StringBuilder sb, FieldDecl field)
    {
        sb.AppendLine($"            case \"{field.Name}\":");
        sb.AppendLine($"            case \"{BareName(field)}\":");
    }

    private static string BareName(FieldDecl field) => field.Name[2..];

    #endregion

    #region Identity type map

    public static void WriteIdentityTypeMap(IEnumerable<(string Identity, string Type)> entries, string path)
    {
        StringBuilder sb = new();
        sb.AppendLine(Header);
        sb.AppendLine();
        sb.AppendLine("namespace HKLib.hk2019;");
        sb.AppendLine();
        sb.AppendLine("public static class IdentityTypeMap");
        sb.AppendLine("{");
        sb.AppendLine("    public static Dictionary<string, Type> Map { get; } = new()");
        sb.AppendLine("    {");
        foreach ((string identity, string type) in entries)
        {
            string escaped = identity.Replace("\\", "\\\\").Replace("\"", "\\\"");
            sb.AppendLine($"        {{ \"{escaped}\", typeof({TypeInfo.Materialize(type, false)}) }},");
        }

        sb.AppendLine("    };");
        sb.AppendLine("}");

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, sb.ToString());
    }

    #endregion

    private static string Relative(string text) => TypeInfo.Materialize(text, false);

    private static IEnumerable<string> Collect(Decl decl, Func<Decl, IEnumerable<string>> selector) =>
        selector(decl).Concat(decl.Nested.SelectMany(x => Collect(x, selector)));
}
