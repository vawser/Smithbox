using Andre.Core;
using Andre.IO.VFS;
using SoulsFormats;
using System.Collections;
using System.Reflection;

/// <summary>
/// A CustomManualSelectorGenerator and the animations of the clips it selects between
/// </summary>
public sealed record SelectorInfo(string SelectorName, int AnimID, List<string> Clips);

public sealed class BinderResult
{
    public List<SelectorInfo> Selectors = new();
    public List<string> Errors = new();

    /// <summary>
    /// The behaviour files that were read, by file name, such as c0000 or c9997
    /// </summary>
    public List<string> BehaviorFiles = new();
}

public static class BehaviorReader
{
    /// <summary>
    /// The binders holding the behaviours of each character. Bloodborne keeps them in the anibnd,
    /// and the later games in a behbnd of their own.
    /// </summary>
    public static List<string> FindBehaviorBinders(VirtualFileSystem fs, Game game)
    {
        var pattern = game == Game.BB
            ? @"(?i)(^|/)c\d{4}\.anibnd(\.dcx)?$"
            : @"(?i)\.behbnd(\.dcx)?$";

        return fs.GetFileNamesMatching("/chr", pattern)
            .Select(p => p.Replace('\\', '/'))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static BinderResult ReadBinder(VirtualFileSystem fs, Game game, string path)
    {
        var result = new BinderResult();

        Memory<byte>? data;

        try
        {
            data = fs.ReadFile(path);
        }
        catch (Exception e)
        {
            result.Errors.Add($"read: {e.Message}");
            return result;
        }

        if (data == null)
        {
            result.Errors.Add("not found");
            return result;
        }

        BND4 bnd;

        try
        {
            var raw = data.Value.ToArray();

            bnd = BND4.Read(DCX.Is(raw) ? DCX.Decompress(raw).ToArray() : raw);
        }
        catch (Exception e)
        {
            result.Errors.Add($"bnd: {e.Message}");
            return result;
        }

        byte[] compendium = bnd.Files
            .FirstOrDefault(f => f.Name.Contains(".compendium", StringComparison.OrdinalIgnoreCase)) is { } c
            ? (c.Name.EndsWith(".dcx", StringComparison.OrdinalIgnoreCase) ? DCX.Decompress(c.Bytes).ToArray() : c.Bytes.ToArray())
            : null;

        List<TagfileReader.TagType> compendiumTypes = null;

        if (compendium != null && UsesTagfiles(game))
        {
            try
            {
                compendiumTypes = TagfileReader.ReadCompendiumTypes(compendium);
            }
            catch (Exception e)
            {
                result.Errors.Add($"compendium: {e.Message}");
            }
        }

        foreach (var file in bnd.Files)
        {
            var name = "/" + file.Name.Replace('\\', '/');

            if (!name.Contains("/Behaviors/", StringComparison.OrdinalIgnoreCase) ||
                !name.Contains(".hkx", StringComparison.OrdinalIgnoreCase))
                continue;

            var bytes = name.EndsWith(".dcx", StringComparison.OrdinalIgnoreCase)
                ? DCX.Decompress(file.Bytes).ToArray()
                : file.Bytes.ToArray();

            result.BehaviorFiles.Add(Path.GetFileName(name).Split('.')[0]);

            try
            {
                // Sekiro still has a few packfiles among its tagfiles
                var isPackfile = bytes.Length >= 4 && BitConverter.ToUInt32(bytes, 0) == 0x57E0E057;

                if (UsesTagfiles(game) && !isPackfile)
                {
                    CollectSelectors(new TagfileReader(bytes, compendiumTypes), result.Selectors);
                }
                else
                {
                    var rootObject = ReadRoot(game, bytes, compendium);

                    CollectSelectors(rootObject, result.Selectors);
                }
            }
            catch (Exception e)
            {
                result.Errors.Add($"{Path.GetFileName(name)}: {e.GetType().Name}: {e.Message}");
            }
        }

        return result;
    }

    /// <summary>
    /// A behaviour file read through the unified Havok library: its generated classes, which the walker reads by
    /// field name (HKX2 and HKLib until Phase 7)
    /// </summary>
    private static object ReadRoot(Game game, byte[] bytes, byte[] compendium)
    {
        var profile = Havok.HavokProfile.FromGame(game.ToString())
            ?? throw new NotSupportedException($"{game} has no Havok profile");
        var unified = compendium != null ? Havok.Files.HavokFile.ReadCompendium(compendium, profile) : null;

        return Havok.Files.HavokFile.Read(bytes, profile, unified).Root;
    }

    /// <summary>
    /// DS3 and BB behaviours are packfiles, read through the unified library. The later games use tagfiles, which are read
    /// by their own type descriptions, as the class libraries miss Sekiro's version and Nightreign's selector layout.
    /// </summary>
    public static bool UsesTagfiles(Game game) => game is not (Game.DS3 or Game.BB or Game.DS1 or Game.DS1R);

    private static void CollectSelectors(TagfileReader reader, List<SelectorInfo> selectors)
    {
        foreach (var (type, offset) in reader.FindObjects("CustomManualSelectorGenerator"))
        {
            var name = reader.ReadString(type, offset, "name") ?? "";
            var animId = reader.ReadInt32(type, offset, "animId") ?? -1;
            var clips = new List<string>();

            foreach (var (generatorType, generatorOffset) in reader.ReadObjectArray(type, offset, "generators"))
            {
                if (!generatorType.IsOrInherits("hkbClipGenerator"))
                    continue;

                var animationName = reader.ReadString(generatorType, generatorOffset, "animationName");

                if (!string.IsNullOrEmpty(animationName))
                    clips.Add(animationName);
            }

            selectors.Add(new SelectorInfo(name, animId, clips));
        }
    }

    /// <summary>
    /// Walks the object graph for selectors. Every SDK's generated classes share their field names,
    /// so the walk goes by reflection rather than by type.
    /// </summary>
    private static void CollectSelectors(object rootObject, List<SelectorInfo> selectors)
    {
        var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);
        var stack = new Stack<object>();
        stack.Push(rootObject);

        while (stack.Count > 0)
        {
            var obj = stack.Pop();

            if (obj == null || !visited.Add(obj))
                continue;

            var type = obj.GetType();

            if (type.Name == "CustomManualSelectorGenerator")
            {
                var name = GetField(obj, "m_name") as string ?? "";
                var animId = GetField(obj, "m_animId") is int id ? id : -1;
                var clips = new List<string>();

                if (GetField(obj, "m_generators") is IEnumerable generators)
                {
                    foreach (var generator in generators)
                    {
                        if (generator?.GetType().Name == "hkbClipGenerator" &&
                            GetField(generator, "m_animationName") is string animationName &&
                            animationName.Length > 0)
                        {
                            clips.Add(animationName);
                        }
                    }
                }

                selectors.Add(new SelectorInfo(name, animId, clips));
            }

            foreach (var field in GetFields(type))
            {
                var value = field.GetValue(obj);

                if (value == null)
                    continue;

                if (value is string || value.GetType().IsPrimitive || value.GetType().IsEnum)
                    continue;

                if (value is IEnumerable enumerable)
                {
                    foreach (var item in enumerable)
                    {
                        if (item != null && IsWalkable(item.GetType()))
                            stack.Push(item);
                    }
                }
                else if (IsWalkable(value.GetType()))
                {
                    stack.Push(value);
                }
            }
        }
    }

    private static bool IsWalkable(Type type)
    {
        return type.IsClass && type != typeof(string) &&
               type.Namespace?.StartsWith("Havok.hk") == true;
    }

    private static readonly Dictionary<Type, FieldInfo[]> FieldCache = new();

    private static FieldInfo[] GetFields(Type type)
    {
        if (!FieldCache.TryGetValue(type, out var fields))
        {
            fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance)
                .Where(f => !f.FieldType.IsPrimitive && f.FieldType != typeof(string) && !f.FieldType.IsEnum)
                .ToArray();

            FieldCache[type] = fields;
        }

        return fields;
    }

    private static object GetField(object obj, string name)
    {
        return obj.GetType().GetField(name, BindingFlags.Public | BindingFlags.Instance)?.GetValue(obj);
    }
}
