using StudioCore.Configuration;
using StudioCore.Editor.Multiselection;
using StudioCore.TextEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoulsFormats;
using HKLib.hk2018.hkaiCollisionAvoidance;

namespace StudioCore.Editors.TextEditor;

public class TextSelectionManager
{
    private TextEditorScreen Screen;

    public int SelectedContainerKey;
    public TextContainerInfo SelectedContainer;

    public int SelectedFmgKey;
    public FmgInfo SelectedFmgInfo;
    public FMG SelectedFmg;

    public int _selectedFmgEntryIndex;
    public FMG.Entry _selectedFmgEntry;

    public Multiselection FmgEntryMultiselect;

    public bool SelectNextFileContainer;
    public bool SelectNextFmg;
    public bool SelectNextFmgEntry;

    public bool FocusSelection;

    private KeyBind MultiSelectKey = KeyBindings.Current.TEXT_Multiselect;

    public TextSelectionContext CurrentSelectionContext;

    public TextSelectionManager(TextEditorScreen screen)
    {
        Screen = screen;

        FmgEntryMultiselect = new Multiselection(MultiSelectKey);
    }

    public void OnProjectChanged()
    {
        SelectedContainerKey = -1;
        SelectedContainer = null;

        SelectedFmgInfo = null;
        SelectedFmgKey = -1;
        SelectedFmg = null;

        _selectedFmgEntryIndex = -1;
        _selectedFmgEntry = null;

        SelectNextFileContainer = false;
        SelectNextFmg = false;
        SelectNextFmgEntry = false;
        FocusSelection = false;

        FmgEntryMultiselect = new Multiselection(MultiSelectKey);
    }

    /// <summary>
    /// Set current File Container selection
    /// </summary>>
    public void SelectFileContainer(TextContainerInfo info, int index)
    {
        CurrentSelectionContext = TextSelectionContext.File;

        SelectedContainerKey = index;
        SelectedContainer = info;

        _selectedFmgEntryIndex = -1;
        _selectedFmgEntry = null;

        // Refresh the param editor FMG decorators when the file changes.
        Smithbox.EditorHandler.ParamEditor.SetupFmgDecorators();

        // Auto-select first FMG
        AutoSelectFirstValidFmg();
    }

    /// <summary>
    /// Set current FMG selection
    /// </summary>
    public void SelectFmg(FmgInfo fmgInfo, bool changeContext = true)
    {
        if(changeContext)
            CurrentSelectionContext = TextSelectionContext.Fmg;

        SelectedFmgInfo = fmgInfo;
        SelectedFmgKey = fmgInfo.ID;
        SelectedFmg = fmgInfo.File;

        FmgEntryMultiselect = new Multiselection(MultiSelectKey);

        _selectedFmgEntryIndex = -1;
        _selectedFmgEntry = null;

        Screen.DifferenceManager.TrackFmgDifferences();

        // Auto-select first FMG Entry
        AutoSelectFmgEntry();
    }

    /// <summary>
    /// Auto-select first relevant FMG (triggered on File change)
    /// </summary>
    private void AutoSelectFirstValidFmg()
    {
        foreach (var fmgInfo in SelectedContainer.FmgInfos)
        {
            var id = fmgInfo.ID;
            var fmgName = fmgInfo.Name;
            var displayName = TextUtils.GetFmgDisplayName(SelectedContainer, id);

            if (Screen.Filters.IsFmgFilterMatch(fmgName, displayName, id))
            {
                SelectFmg(fmgInfo, false);
                break;
            }
        }
    }

    /// <summary>
    /// Set current FMG Entry selection
    /// </summary>
    public void SelectFmgEntry(int index, FMG.Entry entry, bool changeContext = true)
    {
        if(changeContext)
            CurrentSelectionContext = TextSelectionContext.FmgEntry;

        FmgEntryMultiselect.HandleMultiselect(_selectedFmgEntryIndex, index);

        _selectedFmgEntryIndex = index;
        _selectedFmgEntry = entry;
    }

    /// <summary>
    /// Auto-select first relevant FMG Entry (triggered on FMG change)
    /// </summary>
    public void AutoSelectFmgEntry()
    {
        for (int i = 0; i < SelectedFmg.Entries.Count; i++)
        {
            var entry = SelectedFmg.Entries[i];

            if (Screen.Filters.IsFmgEntryFilterMatch(entry))
            {
                SelectFmgEntry(i, entry, false);
                break;
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