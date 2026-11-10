using Andre.Formats;
using StudioCore.Application;
using StudioCore.Editors.ParamEditor;
using StudioCore.Editors.TextureViewer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace StudioCore.Renderer;

/// <summary>
/// Where one icon lives: a sub-texture of a sheet in a menu TPF.
/// </summary>
public class IconSource
{
    /// <summary>The TPF holding the sheet: the TPF beside the layout binder, or null for Smithbox's bundled layouts (then the group's File names it).</summary>
    public FileDictionaryEntry Tpf;

    /// <summary>The layout binder the entry came from ("bundled" for Smithbox's Icon Layouts).</summary>
    public string Binder;

    public string Sheet;

    /// <summary>The layout file the cell came from: a game layout, "project layout X", "bundled layout X" or "generated grid".</summary>
    public string Layout;

    public SubTexture SubTexture;

    /// <summary>Later cells with the same prefix and id, which this one hides (first wins).</summary>
    public List<IconSource> Shadowed = new();

    /// <summary>The TPF the sheet was found in when the icon was shown (set by IconManager).</summary>
    public string ShownFrom;
}

/// <summary>
/// The icon lookup for one project (Icon Preview mod support, P1). Built from every layout binder the Texture Viewer's
/// bank has parsed, so the icons of the project's own sblytbnd (a mod's new sheets included) are found without
/// listing their sheets anywhere:
///  - each sblytbnd is paired with the TPF in its own folder (/menu/hi/01_common.sblytbnd ↔ /menu/hi/01_common.tpf);
///    a low binder is skipped when it has a hi twin, as its sheets have other sizes and coordinates;
///  - per Icon Configuration group, a (prefix, id) map over all binders. InternalFiles only orders the sheets
///    (listed first) unless the group is restricted to them: always for the bundled layouts (DS3, BB, where several
///    sheets reuse ids), or when the group sets RestrictToInternalFiles;
///  - the first entry for a (prefix, id) wins: the group's own File binder first, then the listed sheets, then the
///    layout order. Later ones are counted as conflicts.
/// See Documentation/Claude/Icon Preview Mod Support.md.
/// </summary>
public class IconIndex
{
    public ProjectEntry Project;

    /// <summary>The bank's layout containers this index was built from; a new dictionary means the bank was set up again.</summary>
    public object Source;

    /// <summary>How many containers the bank had; it fills them in the background, so a different count means rebuild.</summary>
    public int SourceCount;

    private class BinderEntry
    {
        public FileDictionaryEntry Entry;
        public ShoeboxLayoutContainer Container;
        public FileDictionaryEntry Tpf;
        public bool Bundled;

        /// <summary>Grids generated for sheets no layout covers (bundled layouts only, see AddGeneratedSheets).</summary>
        public List<TextureAtlas> Generated = new();
    }

    /// <summary>
    /// The MENU_Icon_XXYYY grid of the games with bundled layouts, for sheets that have none (a mod's new sheet).
    /// DS3: 160 px cells, 12 x 12, from XX * 1000 + YYY * 144. BB: 80 px cells, 12 x 12, from XX * 1000 + (YYY - 1) * 144
    /// (BB's 3xxx/4xxx gem sheets number on without a gap instead; they all have layouts). Checked with IconLayoutSurvey.
    /// </summary>
    private record GridRule(int Cell, int Cols, int Rows, Func<int, int, int> Start);

    private GridRule GetGridRule()
    {
        return Project.Descriptor.ProjectType switch
        {
            ProjectType.DS3 => new GridRule(160, 12, 12, (xx, yyy) => xx * 1000 + yyy * 144),
            ProjectType.BB => new GridRule(80, 12, 12, (xx, yyy) => xx * 1000 + Math.Max(0, yyy - 1) * 144),
            _ => null,
        };
    }

    private static readonly Regex GridSheetName = new(@"^MENU_Icon_(\d\d)(\d\d\d)$", RegexOptions.CultureInvariant);

    private readonly HashSet<string> ScannedTpfs = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Width and height of a TPF texture: from its header (consoles) or its DDS header (PC).</summary>
    public static (int Width, int Height) TextureSize(SoulsFormats.TPF.Texture texture)
    {
        if (texture.Header != null && texture.Header.Width > 0)
            return (texture.Header.Width, texture.Header.Height);

        var bytes = texture.Bytes;
        if (bytes != null && bytes.Length >= 20 && bytes[0] == 'D' && bytes[1] == 'D' && bytes[2] == 'S')
            return (BitConverter.ToInt32(bytes, 16), BitConverter.ToInt32(bytes, 12));

        return (0, 0);
    }

    /// <summary>
    /// For the bundled layouts: the group's TPF, if its sheets have not been checked for missing layouts yet.
    /// </summary>
    public bool NeedsSheetScan(IconConfigurationEntry group, out FileDictionaryEntry tpf)
    {
        tpf = null;

        if (GetGridRule() == null || !Binders.Any(b => b.Bundled))
            return false;

        tpf = GroupTpf(group);
        return tpf != null && !ScannedTpfs.Contains(tpf.Path);
    }

    /// <summary>
    /// Generates a grid for every MENU_Icon_XXYYY sheet of the TPF that no layout covers, rows and columns cut to the
    /// sheet's real size. A generated sheet joins the groups that list a sheet of the same XX block (else Item).
    /// Returns how many sheets were added; the group maps are rebuilt when any were.
    /// </summary>
    public int AddGeneratedSheets(FileDictionaryEntry tpfEntry, SoulsFormats.TPF tpf, IEnumerable<IconConfigurationEntry> groups)
    {
        ScannedTpfs.Add(tpfEntry.Path);

        var rule = GetGridRule();
        var binder = Binders.FirstOrDefault(b => b.Bundled);
        if (rule == null || binder == null || tpf == null)
            return 0;

        var known = binder.Container.Layouts.Values
            .SelectMany(l => l.TextureAtlases)
            .Concat(binder.Generated)
            .Select(a => Path.GetFileNameWithoutExtension(a.ImagePath))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var groupList = groups.ToList();
        int added = 0;

        foreach (var texture in tpf.Textures)
        {
            var match = GridSheetName.Match(texture.Name);
            if (!match.Success || known.Contains(texture.Name))
                continue;

            var (width, height) = TextureSize(texture);
            var cols = Math.Min(rule.Cols, width / rule.Cell);
            var rows = Math.Min(rule.Rows, height / rule.Cell);
            if (cols <= 0 || rows <= 0)
                continue;

            var block = match.Groups[1].Value;
            var id = rule.Start(int.Parse(block), int.Parse(match.Groups[2].Value));

            var subs = new List<SubTexture>();
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < rule.Cols; col++)
                {
                    // Keep the numbering of a full 12-column row even if the sheet is narrower
                    if (col < cols)
                        subs.Add(new SubTexture($"ICON_{id:D5}.png", col * rule.Cell, row * rule.Cell, rule.Cell, rule.Cell));
                    id++;
                }
            }

            var joined = groupList
                .Where(g => g.InternalFiles != null && g.InternalFiles.Any(f => f.StartsWith($"MENU_Icon_{block}", StringComparison.OrdinalIgnoreCase)))
                .Select(g => g.Name)
                .ToList();
            if (joined.Count == 0 && groupList.Any(g => g.Name == "Item"))
                joined.Add("Item");

            binder.Generated.Add(new TextureAtlas($"{texture.Name}.tif", string.Join(",", joined), subs));
            added++;
        }

        if (added > 0)
            GroupMaps.Clear();

        return added;
    }

    private readonly List<BinderEntry> Binders = new();

    private readonly Dictionary<string, Dictionary<(string Prefix, int Id), IconSource>> GroupMaps = new();

    public bool IsEmpty => Binders.Count == 0;

    public IconIndex(ProjectEntry project)
    {
        Project = project;

        var bank = project?.Handler?.TextureData?.PrimaryBank;
        var containers = bank?.ShoeboxEntries;
        Source = containers;

        if (containers == null)
            return;

        var all = containers.ToList();
        SourceCount = all.Count;

        foreach (var (entry, container) in all)
        {
            var bundled = string.IsNullOrEmpty(entry.Path);

            if (!bundled && IsLow(entry) && all.Any(other => IsHiTwin(other.Key, entry)))
                continue;

            var tpf = bundled ? null : FindPairedTpf(entry);

            Binders.Add(new BinderEntry { Entry = entry, Container = container, Tpf = tpf, Bundled = bundled });
        }
    }

    private static bool IsLow(FileDictionaryEntry e)
    {
        return e.Folder.EndsWith("/low", StringComparison.OrdinalIgnoreCase)
            || e.Filename.EndsWith("_l", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsHiTwin(FileDictionaryEntry hi, FileDictionaryEntry low)
    {
        if (string.IsNullOrEmpty(hi.Path))
            return false;

        if (low.Folder.EndsWith("/low", StringComparison.OrdinalIgnoreCase)
            && hi.Folder.Equals(low.Folder[..^4] + "/hi", StringComparison.OrdinalIgnoreCase)
            && hi.Filename.Equals(low.Filename, StringComparison.OrdinalIgnoreCase))
            return true;

        if (low.Filename.EndsWith("_l", StringComparison.OrdinalIgnoreCase)
            && hi.Folder.Equals(low.Folder, StringComparison.OrdinalIgnoreCase)
            && hi.Filename.Equals(low.Filename[..^2] + "_h", StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }

    private FileDictionaryEntry FindPairedTpf(FileDictionaryEntry binder)
    {
        return Project.Locator.TextureFiles.Entries?.FirstOrDefault(t =>
            t.Extension == "tpf"
            && t.Folder.Equals(binder.Folder, StringComparison.OrdinalIgnoreCase)
            && t.Filename.Equals(binder.Filename, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>The TPF a group's File names, for the bundled layouts (and as a fallback when a sheet is not in its paired TPF).</summary>
    public FileDictionaryEntry GroupTpf(IconConfigurationEntry group)
    {
        var entries = Project.Locator.TextureFiles.Entries;
        if (entries == null || string.IsNullOrEmpty(group.File))
            return null;

        var named = entries.Where(t => t.Extension == "tpf" && t.Filename.Equals(group.File, StringComparison.OrdinalIgnoreCase)).ToList();

        // Prefer the hi copy where there are two
        return named.FirstOrDefault(t => !IsLow(t) && t.Folder.StartsWith("/menu", StringComparison.OrdinalIgnoreCase))
            ?? named.FirstOrDefault(t => t.Folder.StartsWith("/menu", StringComparison.OrdinalIgnoreCase))
            ?? named.FirstOrDefault();
    }

    /// <summary>
    /// The sub-texture prefixes a group's field value is looked up with, in order. AC6's weapon and armor groups
    /// have several: weapons try WP_A_, WP_R_ then WP_L_, armor takes its prefix from the row's equip-slot flags
    /// (as the old SubTextureHelper did, now in .archive/Icon Preview P1).
    /// </summary>
    public static List<string> GetPrefixes(IconConfigurationEntry group, Param.Row context)
    {
        if (group.SubTexturePrefix == "AC6-Weapon")
            return new List<string> { "WP_A_", "WP_R_", "WP_L_" };

        if (group.SubTexturePrefix == "AC6-Armor")
        {
            var prefix = "";

            if (context["headEquip"]?.Value?.ToString() == "1") prefix = "HD_M_";
            if (context["bodyEquip"]?.Value?.ToString() == "1") prefix = "BD_M_";
            if (context["armEquip"]?.Value?.ToString() == "1") prefix = "AM_M_";
            if (context["legEquip"]?.Value?.ToString() == "1") prefix = "LG_M_";

            return new List<string> { prefix };
        }

        return new List<string> { group.SubTexturePrefix ?? "" };
    }

    private static IEnumerable<string> AllPrefixes(IconConfigurationEntry group)
    {
        return group.SubTexturePrefix switch
        {
            "AC6-Weapon" => new[] { "WP_A_", "WP_R_", "WP_L_" },
            "AC6-Armor" => new[] { "HD_M_", "BD_M_", "AM_M_", "LG_M_" },
            _ => new[] { group.SubTexturePrefix ?? "" },
        };
    }

    /// <summary>
    /// The layout a cell came from, for the icon tooltip. Game layout names are build paths
    /// (N:\GR\data\Menu\...\SB_Icon_00.layout), so only the file name is kept.
    /// </summary>
    private static string DescribeLayout(BinderEntry binder, string layoutKey)
    {
        if (layoutKey.StartsWith("project:", StringComparison.Ordinal))
            return $"project layout {layoutKey["project:".Length..]}";

        if (binder.Bundled)
            return $"bundled layout {layoutKey}";

        return Path.GetFileName(layoutKey.Replace('\\', '/'));
    }

    /// <summary>
    /// A layout atlas can join groups through its type attribute (a comma-separated list of group names), as DS2's
    /// layouts already do. This is how a project-local layout adds a new sheet to a DS3/BB group.
    /// </summary>
    private static bool TypeNamesGroup(TextureAtlas atlas, IconConfigurationEntry group)
    {
        if (string.IsNullOrEmpty(atlas.Type) || string.IsNullOrEmpty(group.Name))
            return false;

        return atlas.Type.Split(',').Any(t => t.Trim().Equals(group.Name, StringComparison.OrdinalIgnoreCase));
    }

    public IconSource Find(IconConfigurationEntry group, List<string> prefixes, int id)
    {
        var map = GetGroupMap(group);

        foreach (var prefix in prefixes)
        {
            if (map.TryGetValue((prefix, id), out var source))
                return source;
        }

        return null;
    }

    private Dictionary<(string Prefix, int Id), IconSource> GetGroupMap(IconConfigurationEntry group)
    {
        if (GroupMaps.TryGetValue(group.Name, out var existing))
            return existing;

        var map = new Dictionary<(string, int), IconSource>();
        GroupMaps[group.Name] = map;

        var listed = group.InternalFiles ?? new List<string>();
        var listedSet = new HashSet<string>(listed, StringComparer.OrdinalIgnoreCase);

        // The old SubTextureHelper regex ({prefix}([0-9]+)), anchored at both ends: at the start so MENU_MAP_ does not also take
        // KG_MENU_MAP_, at the end so the games' placeholders (MENU_ItemIcon_00000d, ArenaRank_00000d) and named
        // variants (MENU_MAP_01_Bonfire) are not taken as icons. A second number is allowed, as NR's DLC map icons
        // are named MENU_MAP_42043_073 (the first number is the icon id).
        var regexes = AllPrefixes(group).Distinct()
            .Select(p => (Prefix: p, Regex: new Regex($"^{Regex.Escape(p)}([0-9]+)(?:_[0-9]+)?$", RegexOptions.CultureInvariant)))
            .ToList();

        // The group's own File binder first, then the rest in bank order
        var binders = Binders
            .OrderBy(b => b.Entry.Filename.Equals(group.File, StringComparison.OrdinalIgnoreCase) ? 0 : 1)
            .ToList();

        foreach (var binder in binders)
        {
            var restrict = group.RestrictToInternalFiles ?? binder.Bundled;

            var atlases = binder.Container.Layouts
                .SelectMany(kv => kv.Value.TextureAtlases.Select(atlas => (Atlas: atlas, Layout: DescribeLayout(binder, kv.Key))))
                .Concat(binder.Generated.Select(atlas => (Atlas: atlas, Layout: "generated grid")))
                .Select((a, order) => (a.Atlas, a.Layout, Sheet: Path.GetFileNameWithoutExtension(a.Atlas.ImagePath), Order: order))
                .Where(a => !restrict || listedSet.Contains(a.Sheet) || TypeNamesGroup(a.Atlas, group))
                // InternalFiles sheets in their order, then sheets whose type names the group, then the rest
                .OrderBy(a => listedSet.Contains(a.Sheet)
                    ? listed.FindIndex(f => f.Equals(a.Sheet, StringComparison.OrdinalIgnoreCase))
                    : TypeNamesGroup(a.Atlas, group) ? int.MaxValue - 1 : int.MaxValue)
                .ThenBy(a => a.Order);

            foreach (var (atlas, layout, sheet, _) in atlases)
            {
                foreach (var sub in atlas.SubTextures)
                {
                    var name = sub.Name.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ? sub.Name[..^4] : sub.Name;

                    foreach (var (prefix, regex) in regexes)
                    {
                        var match = regex.Match(name);
                        if (!match.Success || !int.TryParse(match.Groups[1].Value, out var id))
                            continue;

                        var source = new IconSource
                        {
                            Tpf = binder.Tpf,
                            Binder = binder.Bundled ? "bundled" : binder.Entry.Path,
                            Sheet = sheet,
                            Layout = layout,
                            SubTexture = sub,
                        };

                        // First wins; later cells are kept for the icon tooltip
                        if (!map.TryAdd((prefix, id), source))
                            map[(prefix, id)].Shadowed.Add(source);
                    }
                }
            }
        }

        return map;
    }
}
