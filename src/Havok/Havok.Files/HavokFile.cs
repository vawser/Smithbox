using Havok.Formats;
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
                HavokCompendium? used = compendium is { } c && c.Sdk == profile.Sdk ? c : null;
                if (used != null) serializer.LoadCompendium(used);
                IReadOnlyList<object> objects = serializer.ReadAllObjects(data);
                if (objects.Count == 0) throw new InvalidDataException("The tagfile has no objects.");
                return new HavokDocument(AsHavokObject(objects[0]), profile, info, used) { Objects = objects };
            }
            default:
                throw new NotSupportedException($"{info} can't be read as a document.");
        }
    }

    private static IHavokObject AsHavokObject(object root) =>
        root as IHavokObject ?? throw new InvalidDataException($"The root ({root.GetType().Name}) isn't a Havok object.");
}
