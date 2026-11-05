using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System.Linq;

namespace StudioCore.Editors.TextEditor;

public class TextViewSelection
{
    private TextEditorView Parent;
    private ProjectEntry Project;

    public FileDictionaryEntry SelectedFileDictionaryEntry;
    public int SelectedContainerKey;
    public TextContainerWrapper SelectedContainerWrapper;

    public int SelectedFmgKey;
    public TextFmgWrapper SelectedFmgWrapper;

    public int _selectedFmgEntryIndex;
    public FMG.Entry _selectedFmgEntry;

    public TextMultiselection FmgEntryMultiselect;

    public bool SelectNextFileContainer;
    public bool SelectNextFmg;
    public bool SelectNextFmgEntry;

    public bool FocusFileSelection;
    public bool FocusFmgSelection;

    public TextViewSelection(TextEditorView view, ProjectEntry project)
    {
        Parent = view;
        Project = project;

        FmgEntryMultiselect = new TextMultiselection(view);
    }

    /// <summary>
    /// Set current File Container selection
    /// </summary>>
    public void SelectFileContainer(FileDictionaryEntry entry, TextContainerWrapper container, int index)
    {
        SelectedFileDictionaryEntry = entry;
        SelectedContainerKey = index;
        SelectedContainerWrapper = container;

        SelectedFmgKey = -1;
        SelectedFmgWrapper = null;

        _selectedFmgEntryIndex = -1;
        _selectedFmgEntry = null;

        var textData = Project.Handler.TextData;

        // Load in the FMG if this is the first time this container has been selected
        if (container.FmgWrappers == null || container.FmgWrappers.Count == 0)
        {
            textData.PrimaryBank.LoadFmgWrappers(container);
        }

        // Load the matching vanilla container for the modified/unique colours.
        // Match the language (and DS2 sub-category) too: several languages share the
        // same filename, and matching by filename alone often loaded another language.
        var vanillaContainer = textData.VanillaBank.Containers
            .FirstOrDefault(e =>
                e.Value.FileEntry.Filename == container.FileEntry.Filename &&
                e.Value.ContainerDisplayCategory == container.ContainerDisplayCategory &&
                e.Value.ContainerDisplaySubCategory == container.ContainerDisplaySubCategory);

        if (vanillaContainer.Value != null &&
            (vanillaContainer.Value.FmgWrappers == null || vanillaContainer.Value.FmgWrappers.Count == 0))
        {
            textData.VanillaBank.LoadFmgWrappers(vanillaContainer.Value);
        }

        // Refresh the param editor FMG decorators when the file changes.
        Project.Handler.ParamEditor?.ViewHandler?.ActiveView?.RefreshDecorators();

        // Auto-select first FMG. This used to be skipped on a container's first selection,
        // leaving no FMG selected (and undo/redo then crashed in TrackFmgDifferences).
        AutoSelectFirstValidFmg();
    }

    /// <summary>
    /// Set current FMG selection
    /// </summary>
    public void SelectFmg(TextFmgWrapper fmgInfo, bool changeContext = true)
    {
        SelectedFmgWrapper = fmgInfo;
        SelectedFmgKey = fmgInfo.ID;

        FmgEntryMultiselect = new TextMultiselection(Parent);

        _selectedFmgEntryIndex = -1;
        _selectedFmgEntry = null;

        Parent.DifferenceManager.TrackFmgDifferences();

        // Auto-select first FMG Entry
        AutoSelectFmgEntry();
    }

    /// <summary>
    /// Auto-select first relevant FMG (triggered on File change)
    /// </summary>
    private void AutoSelectFirstValidFmg()
    {
        if (SelectedContainerWrapper?.FmgWrappers == null)
            return;

        foreach (var fmgInfo in SelectedContainerWrapper.FmgWrappers)
        {
            var id = fmgInfo.ID;
            var fmgName = fmgInfo.Name;
            var displayName = TextUtils.GetFmgDisplayName(Project, SelectedContainerWrapper, id, fmgName);

            var isMatch = EditorFilters.IsMatch(
                Parent.FileList.FmgListFilter, fmgName, Parent.FileList.ExactFmgListFilter, 
                displayName, false, false, id.ToString());

            if (isMatch)
            {
                SelectFmg(fmgInfo);
                break;
            }
        }
    }

    /// <summary>
    /// Set current FMG Entry selection
    /// </summary>
    public void SelectFmgEntry(int index, FMG.Entry entry)
    {
        FmgEntryMultiselect.HandleMultiselect(_selectedFmgEntryIndex, index);

        // Ctrl+click removed this row from the multi-selection: move the primary
        // selection to a row that is still selected, so the row actually deselects.
        if (!FmgEntryMultiselect.IsMultiselected(index) && FmgEntryMultiselect.HasValidMultiselection())
        {
            var remaining = FmgEntryMultiselect.StoredEntries.First();
            index = remaining.Key;
            entry = remaining.Value;
        }

        Parent.TextEntryCreator.UpdateParameters(entry);

        _selectedFmgEntryIndex = index;
        _selectedFmgEntry = entry;
    }

    /// <summary>
    /// Drop or re-index selections whose entries moved or left the FMG.
    /// A deleted entry used to stay selected and editable in the Contents panel.
    /// </summary>
    public void RevalidateEntrySelection()
    {
        var fmg = SelectedFmgWrapper?.File;

        FmgEntryMultiselect.Revalidate(fmg);

        if (_selectedFmgEntry == null)
            return;

        if (fmg == null)
        {
            _selectedFmgEntryIndex = -1;
            _selectedFmgEntry = null;
            return;
        }

        if (_selectedFmgEntryIndex >= 0 && _selectedFmgEntryIndex < fmg.Entries.Count &&
            fmg.Entries[_selectedFmgEntryIndex] == _selectedFmgEntry)
            return;

        _selectedFmgEntryIndex = fmg.Entries.IndexOf(_selectedFmgEntry);

        if (_selectedFmgEntryIndex == -1)
        {
            _selectedFmgEntry = null;
        }
    }

    /// <summary>
    /// After a delete: select the entry now at the given index (the row below the
    /// deleted ones), or the last entry, so the Contents panel doesn't show a deleted entry.
    /// </summary>
    public void SelectEntryNearIndex(int index)
    {
        FmgEntryMultiselect.StoredEntries.Clear();
        _selectedFmgEntryIndex = -1;
        _selectedFmgEntry = null;

        var fmg = SelectedFmgWrapper?.File;
        if (fmg == null || fmg.Entries.Count == 0)
            return;

        // Prefer the first visible entry at or after the index, then before it
        for (int i = System.Math.Max(index, 0); i < fmg.Entries.Count; i++)
        {
            if (Parent.TextEntryList.IsEntryVisible(fmg.Entries[i]))
            {
                SetSingleSelection(i, fmg.Entries[i]);
                return;
            }
        }

        for (int i = System.Math.Min(index, fmg.Entries.Count) - 1; i >= 0; i--)
        {
            if (Parent.TextEntryList.IsEntryVisible(fmg.Entries[i]))
            {
                SetSingleSelection(i, fmg.Entries[i]);
                return;
            }
        }
    }

    /// <summary>
    /// Select the container, FMG and entry an undone/redone action changed, and scroll
    /// to them. Leaves the selection alone when the entry is already selected, so undoing
    /// an edit to the current entry (or within a multi-selection) doesn't reset anything.
    /// </summary>
    public void RevealTarget(ITextActionTarget target)
    {
        if (target?.TargetContainer == null || target.TargetFmg == null)
            return;

        var container = target.TargetContainer;

        if (SelectedContainerWrapper != container)
        {
            SelectFileContainer(container.FileEntry, container, -1);
            FocusFileSelection = true;
        }

        var fmgWrapper = container.FmgWrappers?.FirstOrDefault(w => w.File == target.TargetFmg);

        if (fmgWrapper == null)
            return;

        if (SelectedFmgWrapper != fmgWrapper)
        {
            SelectFmg(fmgWrapper);
            FocusFmgSelection = true;
        }

        var entry = target.TargetEntry;
        if (entry == null)
            return;

        var index = target.TargetFmg.Entries.IndexOf(entry);

        // Not in the FMG any more (e.g. a delete was redone): the FMG is shown, nothing to select
        if (index == -1)
            return;

        if (_selectedFmgEntry == entry || FmgEntryMultiselect.IsMultiselected(index))
            return;

        SetSingleSelection(index, entry);
        Parent.TextEntryList.FocusSelection = true;
    }

    private void SetSingleSelection(int index, FMG.Entry entry)
    {
        FmgEntryMultiselect.StoredEntries.Clear();
        FmgEntryMultiselect.StoredEntries.Add(index, entry);

        _selectedFmgEntryIndex = index;
        _selectedFmgEntry = entry;

        Parent.TextEntryCreator.UpdateParameters(entry);
    }

    /// <summary>
    /// Auto-select first relevant FMG Entry (triggered on FMG change)
    /// </summary>
    public void AutoSelectFmgEntry()
    {
        if (SelectedFmgWrapper != null && SelectedFmgWrapper.File != null)
        {
            for (int i = 0; i < SelectedFmgWrapper.File.Entries.Count; i++)
            {
                var entry = SelectedFmgWrapper.File.Entries[i];

                if (Parent.TextEntryList.IsEntryVisible(entry))
                {
                    SetSingleSelection(i, entry);
                    break;
                }
            }
        }
    }

    public bool IsFmgSelected()
    {
        return SelectedFmgWrapper != null;
    }

    /// <summary>
    /// Check if a dummy poly entry is selected
    /// </summary>
    public bool IsFmgEntrySelected(int index)
    {
        if (FmgEntryMultiselect.IsMultiselected(index) || _selectedFmgEntryIndex == index)
        {
            return true;
        }

        return false;
    }

}