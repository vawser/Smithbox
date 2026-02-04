using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.FileBrowser;

public class FileShortcuts
{
    public FileBrowserScreen Editor;
    public ProjectEntry Project;

    public FileShortcuts(FileBrowserScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Monitor()
    {
        var activeView = Editor.ViewHandler.ActiveView;

        if (!FocusManager.IsInFileBrowser())
            return;

        if (InputManager.IsPressed(KeybindID.Toggle_Tools_Menu))
        {
            CFG.Current.Interface_FileBrowser_ToolView = !CFG.Current.Interface_FileBrowser_ToolView;
        }
    }
}

