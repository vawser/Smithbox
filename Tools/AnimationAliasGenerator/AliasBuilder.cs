using Andre.Core;
using Andre.IO.VFS;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;

public sealed class AliasEntry
{
    public string ID { get; set; }
    public string Name { get; set; }

    // Left out when empty, as Smithbox defaults it to empty
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string Description { get; set; }

    // Always written, as the alias editor expects a list
    public List<string> Tags { get; set; } = new();
}

/// <summary>
/// Names the animations of each character after the selectors that play them. An animation is often reached
/// from several selectors, so one is picked: a selector whose own animation ID is the clip's comes first, then
/// one that isn't a variant for riding, then the name most selectors share, then the shortest.
/// </summary>
public static class AliasBuilder
{
    private static readonly Regex AnimationName = new(@"^a(\d+)_(\d+)$", RegexOptions.IgnoreCase);

    public sealed class Stats
    {
        public int Binders;
        public int Characters;
        public int Aliases;
        public int Unmatched;
        public int NotInTimeActs;
        public int TimeActAnimations;
        public List<string> NoTimeActs = new();
        public List<string> Errors = new();
    }

    public static List<AliasEntry> Build(VirtualFileSystem fs, Game game, Stats stats, bool includeUnmatched)
    {
        var aliases = new List<AliasEntry>();

        foreach (var path in BehaviorReader.FindBehaviorBinders(fs, game))
        {
            var character = Path.GetFileName(path).Split('.')[0].ToLowerInvariant();

            var result = BehaviorReader.ReadBinder(fs, game, path);

            stats.Binders++;

            foreach (var error in result.Errors)
            {
                stats.Errors.Add($"{character}: {error}");
            }

            if (result.Selectors.Count == 0)
                continue;

            // Only the animations in the character's time acts are listed, and the shared behaviours
            // refer to thousands that a character doesn't have
            var timeActAnimations = TimeActReader.ReadAnimationNames(fs, game, path, character);

            if (timeActAnimations == null)
            {
                stats.NoTimeActs.Add(character);
                continue;
            }

            stats.Characters++;
            stats.TimeActAnimations += timeActAnimations.Count;

            // Every selector that plays each animation
            var byClip = new Dictionary<string, List<SelectorInfo>>(StringComparer.OrdinalIgnoreCase);

            foreach (var selector in result.Selectors)
            {
                foreach (var clip in selector.Clips.Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    if (!byClip.TryGetValue(clip, out var list))
                    {
                        list = new();
                        byClip[clip] = list;
                    }

                    list.Add(selector);
                }
            }

            foreach (var (clip, selectors) in byClip)
            {
                var match = AnimationName.Match(clip);

                if (!match.Success)
                    continue;

                if (!timeActAnimations.Contains(clip))
                {
                    stats.NotInTimeActs++;
                    continue;
                }

                var clipID = long.Parse(match.Groups[2].Value);

                var owners = selectors.Where(e => e.AnimID == clipID).ToList();

                if (owners.Count == 0)
                {
                    stats.Unmatched++;

                    if (!includeUnmatched)
                        continue;

                    owners = selectors;
                }

                // Some selectors are only named after an animation, as in a000_501200, which says nothing new
                var names = owners
                    .Select(e => CleanName(e.SelectorName))
                    .Where(e => e.Length > 0 && !IsAnimationNameOnly(e))
                    .GroupBy(e => e, StringComparer.OrdinalIgnoreCase)
                    .Select(g => (Name: g.Key, Count: g.Count()))
                    .OrderBy(e => IsRideVariant(e.Name) ? 1 : 0)
                    .ThenByDescending(e => e.Count)
                    .ThenBy(e => e.Name.Length)
                    .ThenBy(e => e.Name, StringComparer.Ordinal)
                    .Select(e => e.Name)
                    .ToList();

                if (names.Count == 0)
                    continue;

                var entry = new AliasEntry
                {
                    ID = $"{character}_{clip.ToLowerInvariant()}",
                    Name = names[0]
                };

                // The riding variants repeat the name of the selector they ride along with, so they say nothing more
                var alternates = names.Skip(1).Where(e => !IsRideVariant(e)).ToList();

                if (alternates.Count > 0)
                {
                    var others = alternates.Take(4).ToList();
                    var more = alternates.Count - others.Count;

                    entry.Description = $"Also selected as: {string.Join(", ", others)}" + (more > 0 ? $" (+{more} more)" : "");
                }

                aliases.Add(entry);
            }
        }

        aliases.Sort((a, b) => NaturalCompare(a.ID, b.ID));

        stats.Aliases = aliases.Count;

        return aliases;
    }

    private static bool IsAnimationNameOnly(string name)
    {
        return Regex.IsMatch(name, @"^a \d+ \d+$", RegexOptions.IgnoreCase);
    }

    private static bool IsRideVariant(string name)
    {
        return name.StartsWith("Ride ", StringComparison.OrdinalIgnoreCase) ||
               name.StartsWith("Ridden ", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// AttackRightLight1_CMSG becomes "Attack Right Light 1"
    /// </summary>
    public static string CleanName(string selectorName)
    {
        // The suffix can also sit in the middle, as in SwordArtsOneShot_CMSG_NoMP.
        // Bloodborne spells the class out instead, as in "AtkPowerStart01 CustomManualSelectorGenerator00".
        var name = Regex.Replace(selectorName ?? "", @"(^|[_ ])CustomManualSelectorGenerator\d*(?=[_ ]|$)", "_", RegexOptions.IgnoreCase);
        name = Regex.Replace(name, @"(^|_)CMSG(?=_|$)", "_", RegexOptions.IgnoreCase);

        name = Regex.Replace(name, @"(?<=[a-z])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])|(?<=[A-Za-z])(?=\d)|(?<=\d)(?=[A-Z])", " ");
        name = name.Replace('_', ' ');
        name = Regex.Replace(name, @"\s+", " ").Trim();

        return name;
    }

    public static void Write(List<AliasEntry> aliases, string path, bool indented = true)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = indented,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        Directory.CreateDirectory(Path.GetDirectoryName(path));

        File.WriteAllText(path, JsonSerializer.Serialize(aliases, options));
    }

    private static int NaturalCompare(string a, string b)
    {
        int i = 0, j = 0;

        while (i < a.Length && j < b.Length)
        {
            if (char.IsDigit(a[i]) && char.IsDigit(b[j]))
            {
                int si = i, sj = j;
                while (i < a.Length && char.IsDigit(a[i])) i++;
                while (j < b.Length && char.IsDigit(b[j])) j++;

                var cmp = long.Parse(a[si..i]).CompareTo(long.Parse(b[sj..j]));
                if (cmp != 0) return cmp;
            }
            else
            {
                var cmp = char.ToLowerInvariant(a[i]).CompareTo(char.ToLowerInvariant(b[j]));
                if (cmp != 0) return cmp;
                i++; j++;
            }
        }

        return (a.Length - i).CompareTo(b.Length - j);
    }
}
