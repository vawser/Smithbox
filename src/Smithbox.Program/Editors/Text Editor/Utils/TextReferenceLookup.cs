using SoulsFormats;
using StudioCore.Utilities;
using StudioCore.Application;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.TextEditor;

/// <summary>
/// Finds the same entry in the reference source for the Contents panel's reference column:
/// the vanilla version of the selected container, or the project's container of another
/// language. Matched by container filename (and DS2 sub-category), FMG binder ID and entry ID.
/// </summary>
public class TextReferenceLookup
{
    private TextEditorView View;
    private ProjectEntry Project;

    public TextReferenceLookup(TextEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    // Rebuilt when the text data changes (edits, loads), so lookups stay cheap per frame
    private int _dataVersion = -1;
    private readonly Dictionary<(TextContainerWrapper, bool, TextContainerCategory), TextContainerWrapper> _containers = new();
    private readonly Dictionary<FMG, Dictionary<int, FMG.Entry>> _entryIndex = new(ReferenceEqualityComparer.Instance);

    // Containers whose load was tried, so a missing or broken file isn't reloaded every frame
    private readonly HashSet<TextContainerWrapper> _loadAttempted = new(ReferenceEqualityComparer.Instance);

    /// <summary>
    /// Label for the column header, e.g. "Vanilla" or "French"
    /// </summary>
    public string SourceLabel()
    {
        if (CFG.Current.TextEditor_Contents_Reference_Use_Vanilla)
            return LOC.Get("TEXT_Reference_Source_Vanilla");

        return LOC.Get(CFG.Current.TextEditor_Contents_Reference_Category.GetDisplayName());
    }

    /// <summary>
    /// The reference text for an entry of the selected container. False when the reference
    /// source has no such container, FMG or entry (or is the selected container itself).
    /// </summary>
    public bool TryGetReference(FMG.Entry entry, out string text)
    {
        text = null;

        var selected = View.Selection.SelectedContainerWrapper;

        if (entry?.Parent == null || selected?.FmgWrappers == null)
            return false;

        var version = TextFinder.CurrentDataVersion;
        if (version != _dataVersion)
        {
            _containers.Clear();
            _entryIndex.Clear();
            _dataVersion = version;
        }

        var useVanilla = CFG.Current.TextEditor_Contents_Reference_Use_Vanilla;
        var category = CFG.Current.TextEditor_Contents_Reference_Category;

        var reference = GetReferenceContainer(selected, useVanilla, category);

        if (reference == null || reference == selected)
            return false;

        var fmgId = selected.FmgWrappers.FirstOrDefault(w => w.File == entry.Parent)?.ID;

        if (fmgId == null)
            return false;

        var referenceFmg = reference.FmgWrappers?.FirstOrDefault(w => w.ID == fmgId.Value)?.File;

        if (referenceFmg == null)
            return false;

        if (!_entryIndex.TryGetValue(referenceFmg, out var index))
        {
            index = new Dictionary<int, FMG.Entry>(referenceFmg.Entries.Count);

            foreach (var referenceEntry in referenceFmg.Entries)
            {
                index.TryAdd(referenceEntry.ID, referenceEntry);
            }

            _entryIndex[referenceFmg] = index;
        }

        if (!index.TryGetValue(entry.ID, out var found))
            return false;

        text = found.Text;
        return true;
    }

    private TextContainerWrapper GetReferenceContainer(TextContainerWrapper selected, bool useVanilla, TextContainerCategory category)
    {
        var key = (selected, useVanilla, category);

        if (_containers.TryGetValue(key, out var cached))
            return cached;

        var textData = Project.Handler.TextData;
        var bank = useVanilla ? textData.VanillaBank : textData.PrimaryBank;

        // Vanilla: the same language as the selected container
        var targetCategory = useVanilla ? selected.ContainerDisplayCategory : category;
        var isDS2 = Project.Descriptor.ProjectType is ProjectType.DS2 or ProjectType.DS2S;

        var found = bank?.Containers.Values.FirstOrDefault(c =>
            c.FileEntry.Filename == selected.FileEntry.Filename &&
            c.ContainerDisplayCategory == targetCategory &&
            (!isDS2 || c.ContainerDisplaySubCategory == selected.ContainerDisplaySubCategory));

        // Load it the first time it's needed (other languages usually are by now, in the background)
        if (found != null && (found.FmgWrappers == null || found.FmgWrappers.Count == 0) && _loadAttempted.Add(found))
        {
            bank.LoadFmgWrappers(found);
        }

        // The load bumped the data version; the next call rebuilds the caches
        _containers[key] = found;

        return found;
    }
}
