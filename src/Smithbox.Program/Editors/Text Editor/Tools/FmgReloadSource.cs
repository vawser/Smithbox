using StudioCore.Application;

namespace StudioCore.Editors.TextEditor;

/// <summary>
/// Builds what the FMG Reloader sends: the editor's FMGs as bytes, keyed by binder entry ID
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
            return new TextGameLink.SendItem(fmg.ID, LabelOf(container, fmg), fmg.File.Write(), container.FileEntry.Path);
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

    /// <summary>
    /// The containers the game reads, in its language
    /// </summary>
    public static IEnumerable<TextContainerWrapper> Resident(TextBank bank, string gameLanguage)
    {
        if (bank?.Containers == null)
            return Enumerable.Empty<TextContainerWrapper>();

        return bank.Containers.Values.Where(c => TextGameLink.IsResident(c, gameLanguage));
    }

    /// <summary>
    /// Modified FMGs in the containers the game reads (modified since the last save)
    /// </summary>
    public static List<(TextContainerWrapper Container, TextFmgWrapper Fmg)> DirtyFmgs(TextBank bank, string gameLanguage) =>
        Resident(bank, gameLanguage)
            .Where(c => c.FmgWrappers != null)
            .SelectMany(c => c.FmgWrappers.Where(f => f.IsDirty).Select(f => (c, f)))
            .ToList();

    public static List<TextGameLink.SendItem> Dirty(TextBank bank, string gameLanguage) =>
        DirtyFmgs(bank, gameLanguage).Select(p => FromEditor(p.Container, p.Fmg)).Where(i => i != null).ToList();

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
