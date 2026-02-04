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
    public bool FocusFmgEntrySelection;

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

        // Load in the FMG if this is the first time this container has been selected
        if (container.FmgWrappers == null || container.FmgWrappers.Count == 0)
        {
            var textData = Project.Handler.TextData;

            // Primary
            textData.PrimaryBank.LoadFmgWrappers(container);

            var vanillaContainer = textData.VanillaBank.Containers.FirstOrDefault(e => e.Value.FileEntry.Filename == container.FileEntry.Filename);

            if (vanillaContainer.Value != null)
            {
                textData.VanillaBank.LoadFmgWrappers(vanillaContainer.Value);
            }
        }
        else
        {
            var paramEditor = Smithbox.Orchestrator.SelectedProject.Handler.ParamEditor;

            // Refresh the param editor FMG decorators when the file changes.
            if (paramEditor != null)
            {
                var activeView = paramEditor.ViewHandler.ActiveView;
                activeView.RowDecorators.SetupFmgDecorators();
            }

            // Auto-select first FMG
            AutoSelectFirstValidFmg();
        }
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
        foreach (var fmgInfo in SelectedContainerWrapper.FmgWrappers)
        {
            var id = fmgInfo.ID;
            var fmgName = fmgInfo.Name;
            var displayName = TextUtils.GetFmgDisplayName(Project, SelectedContainerWrapper, id, fmgName);

            if (Parent.Filters.IsFmgFilterMatch(fmgName, displayName, id))
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
        if (FocusManager.IsFocus(EditorFocusContext.TextEditor_EntryList))
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

                if (Parent.Filters.IsFmgEntryFilterMatch(entry))
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

}