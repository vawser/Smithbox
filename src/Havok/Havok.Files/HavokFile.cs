using Havok.Formats;
using Havok.Reflection;
using Havok.Formats.Packfile;
using Havok.Formats.Tagfile;

namespace Havok.Files;

/// <summary>
/// One entry point for reading Havok files of every supported game and SDK (plan §2.3). The file decides the format
/// and SDK; the game's profile picks the registry overlay (NR, DB_NT, BB, DeS 5.1…).
/// </summary>
public static class HavokFile
{
    /// <summary>The container, SDK, pointer size and byte order of a file, from its header.</summary>
    public static HavokFileInfo Sniff(ReadOnlySpan<byte> data) => HavokFileInfo.Read(data);

    /// <summary>
    /// The profile a file of a game is read with: the game's own, unless the file is of another SDK (ER and AC6 keep
    /// a few Sekiro-era 2016 files), in which case the profile of that SDK's own game.
    /// </summary>
    public static HavokProfile ProfileFor(HavokProfile game, HavokFileInfo file)
    {
        if (file.Sdk == HavokSdk.Hk5x) return HavokProfile.For(game.Game, file.SdkVersion) is { Sdk: HavokSdk.Hk5x } des ? des : HavokProfile.DES;
        if (file.Sdk == game.Sdk || file.Sdk == HavokSdk.Unknown) return game;
        return file.Sdk switch
        {
            HavokSdk.Hk2010 => HavokProfile.DS1,
            HavokSdk.Hk2014 => HavokProfile.DS3,
            HavokSdk.Hk2015 => HavokProfile.DS1R,
            HavokSdk.Hk2016 => HavokProfile.SDT,
            HavokSdk.Hk2018 => HavokProfile.ER,
            HavokSdk.Hk2019 => HavokProfile.AC6,
            _ => throw new NotSupportedException($"{file} files aren't supported.")
        };
    }

    /// <summary>The generated class set of a profile's SDK.</summary>
    public static IHavokClassSet ClassSetFor(HavokProfile profile) => profile.Sdk switch
    {
        HavokSdk.Hk5x => hk55.ClassSet.For(profile),
        HavokSdk.Hk2010 => hk2010.ClassSet.For(profile),
        HavokSdk.Hk2014 => hk2014.ClassSet.For(profile),
        HavokSdk.Hk2015 => hk2015.ClassSet.For(profile),
        HavokSdk.Hk2016 => hk2016.ClassSet.For(profile),
        HavokSdk.Hk2018 => hk2018.ClassSet.For(profile),
        HavokSdk.Hk2019 => hk2019.ClassSet.For(profile),
        _ => throw new NotSupportedException($"{profile} has no class set.")
    };

    /// <summary>The tagfile dialect of an SDK.</summary>
    public static TagfileDialect DialectFor(HavokSdk sdk) => sdk switch
    {
        HavokSdk.Hk2015 => TagfileDialect.Hk2015,
        HavokSdk.Hk2016 => TagfileDialect.Hk2016,
        HavokSdk.Hk2018 => TagfileDialect.Hk2018,
        HavokSdk.Hk2019 => TagfileDialect.Hk2019,
        _ => throw new NotSupportedException($"{sdk} has no tagfile format.")
    };

    /// <summary>
    /// Reads a type compendium (TCM0), for the tagfiles of a binder that refer to it. Read it with the profile of the
    /// files that use it: its types come from that class set's registry.
    /// </summary>
    public static HavokCompendium ReadCompendium(byte[] data, HavokProfile game)
    {
        HavokFileInfo info = Sniff(data);
        HavokProfile profile = ProfileFor(game, info);
        return new HavokTagfileSerializer(ClassSetFor(profile), DialectFor(profile.Sdk)).ReadCompendium(data);
    }

    /// <summary>
    /// Reads a packfile or tagfile. <paramref name="compendium" /> is needed for tagfiles whose types live in their
    /// binder's compendium; it's ignored for files of another SDK.
    /// </summary>
    public static HavokDocument Read(byte[] data, HavokProfile game, HavokCompendium? compendium = null)
    {
        HavokFileInfo info = Sniff(data);
        HavokProfile profile = ProfileFor(game, info);
        IHavokClassSet classSet = ClassSetFor(profile);

        switch (info.Container)
        {
            case HavokContainer.Packfile:
            {
                var serializer = new HavokPackfileSerializer(classSet);
                object root = serializer.Read(data);
                return new HavokDocument(AsHavokObject(root), profile, info)
                {
                    Objects = serializer.LastObjects.OrderBy(o => o.Key).Select(o => o.Value).ToList()
                };
            }
            case HavokContainer.Tagfile:
            {
                var serializer = new HavokTagfileSerializer(classSet, DialectFor(profile.Sdk));
                // Only a file that refers to a compendium keeps it: writes then refer to it again, and a file holding its own
                // types keeps its TYPE section
                HavokCompendium? used = info.UsesCompendium && compendium is { } c && c.Sdk == profile.Sdk ? c : null;
                if (used != null) serializer.LoadCompendium(used);
                IReadOnlyList<object> objects = serializer.ReadAllObjects(data);
                if (objects.Count == 0) throw new InvalidDataException("The tagfile has no objects.");
                return new HavokDocument(AsHavokObject(objects[0]), profile, info, used) { Objects = objects };
            }
            default:
                throw new NotSupportedException($"{info} can't be read as a document.");
        }
    }

    /// <summary>
    /// Writes a document back in the form it was read: its container, SDK, profile and compendium. The engines keep
    /// what they read of a root (packfile header and <c>__types__</c>, tagfile TYPE section) in weak tables keyed by
    /// the root object, so an edited root is written as faithfully as an unchanged one.
    /// </summary>
    public static byte[] Write(HavokDocument document)
    {
        IHavokClassSet classSet = ClassSetFor(document.Profile);

        switch (document.Container)
        {
            case HavokContainer.Packfile:
                return new HavokPackfileSerializer(classSet).Write(document.Root);
            case HavokContainer.Tagfile:
            {
                var serializer = new HavokTagfileSerializer(classSet, DialectFor(document.Profile.Sdk));
                if (document.Compendium != null) serializer.LoadCompendium(document.Compendium);
                return serializer.Write(document.Root);
            }
            default:
                throw new NotSupportedException($"{document.Source} can't be written.");
        }
    }

    /// <summary>
    /// Whether the profile's registry gives a class a field, counting its parents. The generated classes of an SDK
    /// hold every game's fields (ER, NR and DB_NT share hk2018), so a field another game added isn't in this game's
    /// files. Classes the registry doesn't know keep every field.
    /// </summary>
    /// <param name="fieldName">The C# field name (<c>m_additiveType</c>) or the Havok one (<c>additiveType</c>).</param>
    public static bool HasField(HavokProfile profile, Type type, string fieldName)
    {
        HavokTypeRegistry registry = ClassSetFor(profile).Registry;
        string name = fieldName.StartsWith("m_", StringComparison.Ordinal) ? fieldName[2..] : fieldName;

        HavokType? havokType = registry.GetType(type);

        if (havokType == null)
            return true;

        for (HavokType? t = havokType; t != null; t = t.Parent)
        {
            if (t.Fields.Any(f => f.Name == name))
                return true;
        }

        // A field of a C# base class the registry doesn't know (e.g. one the generator added) is kept
        Type? declaring = type.GetField(fieldName)?.DeclaringType;
        return declaring != null && declaring != type && registry.GetType(declaring) == null;
    }

    private static IHavokObject AsHavokObject(object root) =>
        root as IHavokObject ?? throw new InvalidDataException($"The root ({root.GetType().Name}) isn't a Havok object.");
}
