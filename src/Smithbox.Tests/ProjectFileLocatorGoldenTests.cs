using StudioCore.Application;
using System.Reflection;
using System.Text;

namespace SmithboxTests;

/// <summary>
///     Runs ProjectFileLocator.CompileDictionaries over every game and a file list with an entry for each of its
///     checks, and compares which lists each file lands in with Golden/ProjectFileLocator.golden.txt.
/// </summary>
public class ProjectFileLocatorGoldenTests
{
    // (archive, folder, file name, extension). "sd" in the archive name marks the low-detail copies most lists skip.
    private static readonly (string Archive, string Folder, string Name, string Ext)[] Files =
    [
        ("data0", "/map/mapstudio", "m10_00_00_00", "msb"),
        ("data0", "/map/mapstudio/autoroute", "m10_00_00_00", "msb"),
        ("sd", "/map/mapstudio", "m10_00_00_00", "msb"),
        ("data0", "/asset/aep/mapstudio", "aep000_00", "msb"),

        ("data0", "/chr", "c1000", "chrbnd"),
        ("sd", "/chr", "c1000", "chrbnd"),
        ("data0", "/model/chr", "c1000", "bnd"),

        ("data0", "/obj", "o000100", "objbnd"),
        ("data0", "/model/obj", "o000100", "bnd"),
        ("data0", "/asset/aeg/aeg099", "aeg099_001", "geombnd"),
        ("sd", "/asset/aeg/aeg099", "aeg099_001", "geombnd"),
        ("data0", "/asset/aeg/aeg099", "aeg099_001", "geomhkxbnd"),

        ("data0", "/parts", "hd_m_1000", "partsbnd"),
        ("sd", "/parts", "hd_m_1000", "partsbnd"),
        ("data0", "/model/parts/head", "hd_m_1000", "bnd"),

        ("data0", "/map/m10_00_00_00", "h0000b0", "hkx"),
        ("data0", "/map/m10_00_00_00", "h10_00_00_00", "hkxbhd"),
        ("sd", "/map/m10_00_00_00", "h10_00_00_00", "hkxbhd"),
        ("data0", "/model/map", "h10_00_00_00", "hkxbhd"),

        ("data0", "/map/m10_00_00_00", "m10_00_00_00", "nvmhktbnd"),
        ("data0", "/map/m10_00_00_00", "m10_00_00_00", "nvmbnd"),
        ("data0", "/map/m10_00_00_00", "m10_00_00_00", "nva"),
        ("data0", "/map/m10_00_00_00", "m10_00_00_00", "nvva"),

        ("data0", "/map/m10_00_00_00", "m0000b0", "flver"),
        ("data0", "/map/m10_00_00_00", "m10_00_00_00_000000", "mapbnd"),
        ("sd", "/map/m10_00_00_00", "m10_00_00_00_000000", "mapbnd"),
        ("data0", "/model/map", "m10_00_00_00", "mapbhd"),

        ("data0", "/map/m10_00_00_00", "m10_00_00_00_0000", "btl"),
        ("data0", "/map/m10_00_00_00", "m10_00_00_00", "msld"),
        ("data0", "/map/m10_00_00_00", "m10_00_00_00", "mpw"),
        ("data0", "/map/m10_00_00_00", "m10_00_00_00", "fvb"),
        ("data0", "/map/m10_00_00_00", "m10_00_00_00", "ivinfobnd"),
        ("data0", "/map/m10_00_00_00", "m10_00_00_00_envmap_00_high_00", "tpfbnd"),
        ("data0", "/map/m10_00_00_00", "m10_00_00_00_envmap_00_high_w", "tpf"),
        ("data0", "/map/breakobj", "m10_00_00_00", "breakobj"),
        ("data0", "/other", "autoinvadepoint", "aipbnd"),
        ("data0", "/map/m10_00_00_00", "m10_00_00_00", "btab"),
        ("data0", "/map/m10_00_00_00", "m10_00_00_00", "btpb"),
        ("data0", "/model/map", "g10_00_00_00", "gibhd"),
        ("data0", "/map/entryfilelist", "e0000", "entryfilelist"),
        ("data0", "/map", "worldloadlistlist", "worldloadlistlist"),
        ("data0", "/other", "ingamestay", "loadlist"),
        ("data0", "/parts", "partstokenidlist", "ptidlbnd"),
        ("data0", "/map/aithreadinfo", "m10_00_00_00", "aith"),

        ("data0", "/param/drawparam", "m10_00_00_00", "gparam"),
        ("data0", "/param/drawparam", "m10_00_00_00", "gparambnd"),
        ("data0", "/filter", "m10_00_00_00", "fltparam"),
        ("data0", "/sfx", "frpg_sfxbnd_c1000", "ffxbnd"),
        ("data0", "/msg/engus", "item_dlc01", "msgbnd"),
        ("data0", "/msg/engus", "item", "msgbnd"),
        ("data0", "/menu/text", "bonfirename", "fmg"),
        ("data0", "/mtd", "allmaterial", "mtdbnd"),
        ("data0", "/material", "allmaterialbnd", "bnd"),
        ("data0", "/material", "allmaterial", "matbinbnd"),

        ("data0", "/menu", "01_common", "tpf"),
        ("data0", "/chr", "c1000", "texbnd"),
        ("data0", "/other", "systex", "commonbnd"),
        ("data0", "/map/m10_00_00_00", "m10_00_00_00_0000", "tpfbhd"),
        ("data0", "/menu", "shoebox", "sblytbnd"),
        ("data0", "/menu", "title", "gfx"),
        ("data0", "/font", "font", "gfx"),

        ("data0", "/chr", "c1000", "anibnd"),
        ("data0", "/chr", "c1000", "behbnd"),
        ("data0", "/remo", "scn100000", "remobnd"),
        ("data0", "/cutscene", "s10_00_0000", "cutscenebnd"),
        ("data0", "/chr", "c1000", "rumblebnd"),
        ("data0", "/action/script", "c0000", "hks"),
    ];

    // The lists the locator compiles, by field name (the full file dictionaries it reads from are left out)
    private static readonly FieldInfo[] Lists = typeof(ProjectFileLocator)
        .GetFields(BindingFlags.Public | BindingFlags.Instance)
        .Where(f => f.FieldType == typeof(FileDictionary)
                    && f.Name is not nameof(ProjectFileLocator.FileDictionary)
                    && f.Name is not nameof(ProjectFileLocator.UnvalidatedFileDictionary))
        .OrderBy(f => f.Name, StringComparer.Ordinal)
        .ToArray();

    private static string Render()
    {
        var text = new StringBuilder();

        foreach (var type in Enum.GetValues<ProjectType>())
        {
            if (type is ProjectType.Undefined)
                continue;

            text.Append("## ").Append(type).Append('\n');

            var entries = Files
                .Select(f => new FileDictionaryEntry
                {
                    Archive = f.Archive,
                    Folder = f.Folder,
                    Filename = f.Name,
                    Extension = f.Ext,
                    Path = $"{f.Folder}/{f.Name}.{f.Ext}",
                    Validated = true
                })
                .ToList();

            var locator = new ProjectFileLocator(TestProjects.Of(type));
            locator.FileDictionary = new FileDictionary { Entries = entries.ToHashSet() };
            locator.CompileDictionaries();

            foreach (var entry in entries)
            {
                var lists = Lists
                    .Where(f => ((FileDictionary)f.GetValue(locator)!).Entries.Contains(entry))
                    .Select(f => f.Name);

                text.Append($"{entry.Archive}:{entry.Path} => {string.Join(", ", lists)}").Append('\n');
            }

            // The Text Editor lists ER's text files in a set order
            var textOrder = locator.TextFiles.Entries.Select(e => e.Filename);
            text.Append("text order: ").Append(string.Join(", ", textOrder)).Append('\n');
        }

        return text.ToString();
    }

    [Fact]
    public void ProjectFileLocatorGolden_MatchesTheRecordedLists()
    {
        Golden.Verify("ProjectFileLocator", Render());
    }
}
