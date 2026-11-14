using StudioCore.Application;

namespace StudioCore.Editors.TextEditor;

/// <summary>
/// Builds what the FMG Reloader sends (the editor's FMGs as bytes, keyed by binder entry ID), and works out which of the
/// project's containers the running game actually loaded
/// </summary>
public static class FmgReloadSource
{
    public static string LabelOf(TextContainerWrapper container, TextFmgWrapper fmg) =>
        $"{container.FileEntry.Filename} / {fmg.Name} [{fmg.ID}]";

    /// <summary>
    /// One FMG as the game would load it. Null (and logged) when it can't be written.
    /// </summary>
    public static TextGameLink.SendItem FromEditor(TextContainerWrapper container, TextFmgWrapper fmg)
    {
        if (container?.FileEntry == null || fmg?.File == null)
            return null;

        try
        {
            var entries = fmg.File.Entries;
            var min = entries.Count > 0 ? entries.Min(e => e.ID) : 0;
            var max = entries.Count > 0 ? entries.Max(e => e.ID) : -1;

            return new TextGameLink.SendItem(fmg.ID, LabelOf(container, fmg), fmg.File.Write(), container.FileEntry.Path, min, max);
        }
        catch (Exception e)
        {
            Smithbox.LogError(typeof(FmgReloadSource), LOC.Get("TEXT_FmgReloader_Write_Failed", LabelOf(container, fmg)), e);
            return null;
        }
    }

    /// <summary>
    /// Every FMG of a container
    /// </summary>
    public static List<TextGameLink.SendItem> FromContainer(TextContainerWrapper container)
    {
        if (container?.FmgWrappers == null)
            return new();

        return container.FmgWrappers.Select(f => FromEditor(container, f)).Where(i => i != null).ToList();
    }

    // Resident containers, rebuilt when the game's FMG list, language or bank changes
    private static object ResidentKey;
    private static List<TextContainerWrapper> ResidentCache = new();

    /// <summary>
    /// The containers the game loaded, in its language: for each of the profile's groups, the first candidate whose
    /// FMG IDs are all filled in the game's slot table (DS3: item/menu_dlc2 when both DLCs are owned, else _dlc1).
    /// A project with an FMG the game doesn't have still matches if at least 90% of its IDs are filled.
    /// </summary>
    public static List<TextContainerWrapper> Resident(TextBank bank, TextGameLink link)
    {
        var fmgs = link.Fmgs;

        if (bank?.Containers == null || link.Profile == null || string.IsNullOrEmpty(link.Language) || fmgs.Count == 0)
            return new();

        var key = (bank, fmgs, link.Language);
        if (Equals(key, ResidentKey))
            return ResidentCache;

        var filled = fmgs.Select(f => f.Id).ToHashSet();
        var result = new List<TextContainerWrapper>();

        foreach (var group in link.Profile.Groups)
        {
            TextContainerWrapper best = null;
            var bestShare = 0.0;

            foreach (var name in group)
            {
                var container = bank.Containers.Values.FirstOrDefault(c =>
                    string.Equals(c.FileEntry?.Filename, name, StringComparison.OrdinalIgnoreCase) && link.IsInGameLanguage(c));

                if (container == null)
                    continue;

                // The IDs come from the container itself
                if (container.FmgWrappers == null || container.FmgWrappers.Count == 0)
                    bank.LoadFmgWrappers(container);

                var ids = container.FmgWrappers?.Select(f => f.ID).ToList() ?? new();
                if (ids.Count == 0)
                    continue;

                var share = ids.Count(filled.Contains) / (double)ids.Count;

                if (share >= 1.0)
                {
                    best = container;
                    bestShare = 1.0;
                    break;
                }

                if (share > bestShare)
                {
                    best = container;
                    bestShare = share;
                }
            }

            if (best != null && bestShare >= 0.9)
                result.Add(best);
        }

        ResidentKey = key;
        ResidentCache = result;
        return result;
    }

    /// <summary>
    /// For a candidate container the game didn't load: the one it loaded instead from the same group, if any
    /// </summary>
    public static TextContainerWrapper LoadedInstead(TextBank bank, TextGameLink link, TextContainerWrapper container)
    {
        var group = link.Profile?.Groups.FirstOrDefault(g => g.Contains(container.FileEntry.Filename, StringComparer.OrdinalIgnoreCase));
        if (group == null)
            return null;

        return Resident(bank, link).FirstOrDefault(c => group.Contains(c.FileEntry.Filename, StringComparer.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Modified FMGs in the containers the game loaded (modified since the last save)
    /// </summary>
    public static List<(TextContainerWrapper Container, TextFmgWrapper Fmg)> DirtyFmgs(TextBank bank, TextGameLink link) =>
        Resident(bank, link)
            .Where(c => c.FmgWrappers != null)
            .SelectMany(c => c.FmgWrappers.Where(f => f.IsDirty).Select(f => (c, f)))
            .ToList();

    public static List<TextGameLink.SendItem> Dirty(TextBank bank, TextGameLink link) =>
        DirtyFmgs(bank, link).Select(p => FromEditor(p.Container, p.Fmg)).Where(i => i != null).ToList();

    /// <summary>
    /// The container and FMG for a history entry, found again by path and ID
    /// </summary>
    public static (TextContainerWrapper Container, TextFmgWrapper Fmg) Find(TextBank bank, string containerPath, int id)
    {
        var container = bank?.Containers?.Values.FirstOrDefault(c =>
            string.Equals(c.FileEntry?.Path, containerPath, StringComparison.OrdinalIgnoreCase));

        return (container, container?.FmgWrappers?.FirstOrDefault(f => f.ID == id));
    }
}
