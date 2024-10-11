using StudioCore.Configuration;
using StudioCore.Editor.Multiselection;
using StudioCore.TextEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoulsFormats;

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
        SelectedContainerKey = index;
        SelectedContainer = info;

        _selectedFmgEntryIndex = -1;
        _selectedFmgEntry = null;

        // Refresh the param editor FMG decorators when the file changes.
        Smithbox.EditorHandler.ParamEditor.SetupFmgDecorators();

        // Clear the grouping cache when the file changes
        FmgEntryGroupCache.ClearCache();
    }

    /// <summary>
    /// Set current FMG selection
    /// </summary>>
    public void SelectFmg(FmgInfo fmgInfo)
    {
        SelectedFmgInfo = fmgInfo;
        SelectedFmgKey = fmgInfo.ID;
        SelectedFmg = fmgInfo.File;

        FmgEntryMultiselect = new Multiselection(MultiSelectKey);

        _selectedFmgEntryIndex = -1;
        _selectedFmgEntry = null;
    }

    /// <summary>
    /// Set current FMG Entry selection
    /// </summary>>
    public void SelectFmgEntry(int index, FMG.Entry entry)
    {
        FmgEntryMultiselect.HandleMultiselect(_selectedFmgEntryIndex, index);

        _selectedFmgEntryIndex = index;
        _selectedFmgEntry = entry;
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