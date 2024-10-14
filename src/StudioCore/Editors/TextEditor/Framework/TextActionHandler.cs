using StudioCore.TextEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor;

public class TextActionHandler
{
    private TextEditorScreen Screen;

    public TextActionHandler(TextEditorScreen screen)
    {
        Screen = screen;
    }

    public void OnProjectChanged()
    {

    }

    /// <summary>
    /// Duplicate current selection of FMG.Entries
    /// </summary>
    public void DuplicateEntries()
    {

    }

    /// <summary>
    /// Delete current selection of FMG.Entries
    /// </summary>
    public void DeleteEntries()
    {

    }
}