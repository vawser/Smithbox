using SoulsFormats;
using StudioCore.Editors.EsdEditor;
using StudioCore.Editors.TalkEditor;
using StudioCore.TalkEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.EsdEditor;

/// <summary>
/// Handles the context menus used by the view classes.
/// </summary>
public class EsdContextMenu
{
    private EsdEditorScreen Screen;
    private EsdPropertyDecorator Decorator;
    private EsdSelectionManager Selection;

    public EsdContextMenu(EsdEditorScreen screen)
    {
        Screen = screen;
        Decorator = screen.Decorator;
        Selection = screen.Selection;
    }

    public void FileContextMenu(EsdBank.EsdScriptInfo info)
    {

    }

    public void ScriptContextMenu(ESD entry)
    {

    }

    public void StateGroupContextMenu(KeyValuePair<long, Dictionary<long, ESD.State>> entry)
    {

    }

    public void StateNodeContextMenu(ESD.State entry)
    {

    }
}
