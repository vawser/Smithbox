using Andre.IO.VFS;
using SoulsFormats;
using StudioCore.Editors.TimeActEditor;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.HavokEditor;

/// <summary>
/// The animations a character has, for its behavior: the animation IDs its time acts have entries for,
/// and the animation HKX files in its anibnds (the main one and the ones split from it: cXXXX_divNN, c0000_aXX_...).
///
/// Loaded in the background, since the player's anibnds are large, and kept per character for the session.
/// It's read from the files, so unsaved Time Act Editor changes aren't in it until it's reloaded.
///
/// Checked against ER, NR and DS3 (Tools/BehaviorSurvey): the player has one time act per animation category
/// (a000, a020, ...); an enemy has one time act whose animations carry the category in their ID (1012000 = a001_012000).
/// Clips often name animations a character doesn't have. Most of an enemy's clips are in the shared c9997.hkx graph,
/// and about half of those have no file for a given character, so a missing animation is shown, not reported as an error.
/// </summary>
public class HavokAnimationIndex
{
    private static readonly ConditionalWeakTable<ProjectEntry, ConcurrentDictionary<string, HavokAnimationIndex>> Cache = new();

    private static readonly Regex AnimationNameRegex = new(@"^a(\d+)_(\d+)$", RegexOptions.IgnoreCase);

    public string Character { get; }

    public int CategorySize { get; }

    /// <summary>Set once the load has finished (successfully or not)</summary>
    public volatile bool IsReady;

    public string Error { get; private set; }

    // Full animation ID -> the name of the time act holding its entry
    private readonly Dictionary<long, string> TimeActEntries = new();

    // Animation ID within a category -> the categories that have it in a time act
    private readonly Dictionary<long, SortedSet<long>> CategoriesByAnimationID = new();

    // Animation file names (a000_003000), lower case
    private readonly HashSet<string> AnimationFiles = new(StringComparer.OrdinalIgnoreCase);

    public int TimeActEntryCount => TimeActEntries.Count;
    public int AnimationFileCount => AnimationFiles.Count;

    private HavokAnimationIndex(string character, int categorySize)
    {
        Character = character;
        CategorySize = categorySize;
    }

    #region Access
    /// <summary>
    /// The index for a character (the behbnd's name, e.g. c0000), starting its load the first time it's asked for
    /// </summary>
    public static HavokAnimationIndex Get(ProjectEntry project, string character)
    {
        if (project == null || string.IsNullOrEmpty(character))
            return null;

        var indices = Cache.GetValue(project, _ => new ConcurrentDictionary<string, HavokAnimationIndex>(StringComparer.OrdinalIgnoreCase));

        return indices.GetOrAdd(character, c => StartLoad(project, c));
    }

    /// <summary>
    /// Forget a character's index, so the next <see cref="Get"/> reads the files again
    /// </summary>
    public static void Invalidate(ProjectEntry project, string character)
    {
        if (project != null && Cache.TryGetValue(project, out var indices))
            indices.TryRemove(character, out _);
    }

    /// <summary>
    /// The full animation ID of an animation name (a000_003000 = 3000, a001_012000 = 1012000), or false if it isn't one
    /// </summary>
    public static bool TryParseAnimationName(string name, int categorySize, out long fullId)
    {
        fullId = -1;

        var match = AnimationNameRegex.Match(name ?? "");

        if (!match.Success)
            return false;

        fullId = long.Parse(match.Groups[1].Value) * categorySize + long.Parse(match.Groups[2].Value);

        return true;
    }

    public bool TryGetTimeAct(long fullId, out string timeActName)
    {
        if (!IsReady)
        {
            timeActName = null;
            return false;
        }

        return TimeActEntries.TryGetValue(fullId, out timeActName);
    }

    public bool HasAnimationFile(string animationName)
    {
        return IsReady && AnimationFiles.Contains(animationName ?? "");
    }

    /// <summary>
    /// The categories whose time acts have an entry for this animation ID (a CMSG's m_animId), in order
    /// </summary>
    public IReadOnlyCollection<long> GetCategories(long animationID)
    {
        if (IsReady && CategoriesByAnimationID.TryGetValue(animationID, out var categories))
            return categories;

        return Array.Empty<long>();
    }

    /// <summary>
    /// The animation name for a category and ID, in the game's format (a000_003000)
    /// </summary>
    public string FormatAnimationName(long category, long animationID)
    {
        return CategorySize == 10000 ? $"a{category:D2}_{animationID:D4}" : $"a{category:D3}_{animationID:D6}";
    }
    #endregion

    #region Load
    private static HavokAnimationIndex StartLoad(ProjectEntry project, string character)
    {
        // As the Time Act Editor: the older games use 4-digit animation IDs
        var categorySize = project.Descriptor.ProjectType is ProjectType.DES or ProjectType.DS1 or ProjectType.DS1R ? 10000 : 1000000;

        var index = new HavokAnimationIndex(character, categorySize);

        Task.Run(() =>
        {
            try
            {
                index.Load(project);
            }
            catch (Exception e)
            {
                index.Error = e.Message;
                Smithbox.LogError(typeof(HavokAnimationIndex), LOC.Get("HAVOK_AnimationIndex_Load_Failed", character), e);
            }
            finally
            {
                index.IsReady = true;
            }
        });

        return index;
    }

    private void Load(ProjectEntry project)
    {
        var fs = project.VFS.FS;
        var entries = project.Locator.TimeActFiles.Entries
            .Where(e => e.Folder.StartsWith("/chr") &&
                (e.Filename.Equals(Character, StringComparison.OrdinalIgnoreCase) ||
                 e.Filename.StartsWith(Character + "_", StringComparison.OrdinalIgnoreCase)))
            .ToList();

        // Time acts, read as the Time Act Editor reads them
        var source = entries.FirstOrDefault(e => e.Filename.Equals(Character, StringComparison.OrdinalIgnoreCase));

        if (source != null)
        {
            var wrapper = new TimeActContainerWrapper(project, source, null, fs);

            foreach (var timeAct in wrapper.ReadTimeActs())
            {
                timeAct.Load();

                if (timeAct.TimeAct == null)
                    continue;

                foreach (var animation in timeAct.TimeAct.Animations)
                {
                    var fullId = timeAct.GetFullAnimationID(animation, CategorySize);

                    TimeActEntries.TryAdd(fullId, timeAct.Name);

                    var category = fullId / CategorySize;
                    var id = fullId % CategorySize;

                    if (!CategoriesByAnimationID.TryGetValue(id, out var categories))
                    {
                        categories = new SortedSet<long>();
                        CategoriesByAnimationID[id] = categories;
                    }

                    categories.Add(category);
                }
            }
        }

        // Animation files
        foreach (var entry in entries)
        {
            AddAnimationFiles(fs, entry.Path);
        }
    }

    private void AddAnimationFiles(VirtualFileSystem fs, string path)
    {
        var data = fs.ReadFile(path);

        if (data == null)
            return;

        var bytes = data.Value;

        if (DCX.Is(bytes))
            bytes = DCX.Decompress(bytes);

        // Checked by magic, so the (large) player anibnds aren't copied to an array first
        var span = bytes.Span;
        var isBnd4 = span.Length >= 4 && span[0] == 'B' && span[1] == 'N' && span[2] == 'D' && span[3] == '4';
        var isBnd3 = span.Length >= 4 && span[0] == 'B' && span[1] == 'N' && span[2] == 'D' && span[3] == '3';

        IBinder binder = isBnd4 ? BND4.Read(bytes) : isBnd3 ? BND3.Read(bytes.ToArray()) : null;

        if (binder == null)
            return;

        foreach (var file in binder.Files)
        {
            var name = Path.GetFileName(file.Name.Replace('\\', '/')).Split('.')[0];

            if (AnimationNameRegex.IsMatch(name))
                AnimationFiles.Add(name);
        }
    }
    #endregion
}
