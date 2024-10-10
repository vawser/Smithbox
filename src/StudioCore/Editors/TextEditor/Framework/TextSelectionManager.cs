using StudioCore.Configuration;
using StudioCore.Editor;
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

    public bool SelectNextFileContainer;
    public bool SelectNextFmg;
    public bool SelectNextFmgEntry;

    public TextSelectionManager(TextEditorScreen screen)
    {
        Screen = screen;
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
    }

    /// <summary>
    /// Set current File Container selection
    /// </summary>>
    public void SelectFileContainer(TextContainerInfo info, int index)
    {
        SelectedContainerKey = index;
        SelectedContainer = info;

        // Refresh the param editor FMG decorators when the file changes.
        Smithbox.EditorHandler.ParamEditor.ResetFMGDecorators();
    }

    /// <summary>
    /// Set current FMG selection
    /// </summary>>
    public void SelectFmg(FmgInfo fmgInfo)
    {
        SelectedFmgInfo = fmgInfo;
        SelectedFmgKey = fmgInfo.ID;
        SelectedFmg = fmgInfo.File;
    }

    /// <summary>
    /// Set current FMG Entry selection
    /// </summary>>
    public void SelectFmgEntry(int index, FMG.Entry entry)
    {
        _selectedFmgEntryIndex = index;
        _selectedFmgEntry = entry;
    }
}