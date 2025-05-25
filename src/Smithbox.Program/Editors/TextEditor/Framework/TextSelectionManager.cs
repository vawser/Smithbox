using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Formats.JSON;
using StudioCore.TextEditor;

namespace StudioCore.Editors.TextEditor;

public class TextSelectionManager
{
    private TextEditorScreen Editor;

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
    public bool FocusFmgEntrySelection;

    private KeyBind MultiSelectKey = KeyBindings.Current.TEXT_Multiselect;

    public TextEditorContext CurrentWindowContext = TextEditorContext.None;

    public TextSelectionManager(TextEditorScreen screen)
    {
        Editor = screen;

        FmgEntryMultiselect = new TextMultiselection(Editor, MultiSelectKey);
    }

    /// <summary>
    /// Set current File Container selection
    /// </summary>>
    public void SelectFileContainer(FileDictionaryEntry entry, TextContainerWrapper info, int index)
    {
        SelectedFileDictionaryEntry = entry;
        SelectedContainerKey = index;
        SelectedContainerWrapper = info;

        SelectedFmgKey = -1;
        SelectedFmgWrapper = null;

        _selectedFmgEntryIndex = -1;
        _selectedFmgEntry = null;

        var paramEditor = Editor.BaseEditor.ProjectManager.SelectedProject.ParamEditor;

        // Refresh the param editor FMG decorators when the file changes.
        if (paramEditor != null)
        {
            paramEditor.DecoratorHandler.SetupFmgDecorators();
        }

        // Auto-select first FMG
        AutoSelectFirstValidFmg();
    }

    /// <summary>
    /// Set current FMG selection
    /// </summary>
    public void SelectFmg(TextFmgWrapper fmgInfo, bool changeContext = true)
    {
        SelectedFmgWrapper = fmgInfo;
        SelectedFmgKey = fmgInfo.ID;

        FmgEntryMultiselect = new TextMultiselection(Editor, MultiSelectKey);

        _selectedFmgEntryIndex = -1;
        _selectedFmgEntry = null;

        Editor.DifferenceManager.TrackFmgDifferences();

        // Auto-select first FMG Entry
        AutoSelectFmgEntry();
    }

    /// <summary>
    /// Auto-select first relevant FMG (triggered on File change)
    /// </summary>
    private void AutoSelectFirstValidFmg()
    {
        foreach (var fmgInfo in SelectedContainerWrapper.FmgWrappers)
        {
            var id = fmgInfo.ID;
            var fmgName = fmgInfo.Name;
            var displayName = TextUtils.GetFmgDisplayName(Editor.Project, SelectedContainerWrapper, id, fmgName);

            if (Editor.Filters.IsFmgFilterMatch(fmgName, displayName, id))
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
        if (CurrentWindowContext == TextEditorContext.FmgEntry)
        {
            FmgEntryMultiselect.HandleMultiselect(_selectedFmgEntryIndex, index);
        }

        _selectedFmgEntryIndex = index;
        _selectedFmgEntry = entry;
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

                if (Editor.Filters.IsFmgEntryFilterMatch(entry))
                {
                    SelectFmgEntry(i, entry);
                    break;
                }
            }
        }
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

    /// <summary>
    /// Switches the focus context to the passed value.
    /// Use this on all windows (e.g. both Begin and BeginChild)
    /// </summary>
    public void SwitchWindowContext(TextEditorContext newContext)
    {
        if (ImGui.IsWindowHovered())
        {
            CurrentWindowContext = newContext;
            //TaskLogs.AddLog($"Context: {newContext.GetDisplayName()}");
        }
    }
}