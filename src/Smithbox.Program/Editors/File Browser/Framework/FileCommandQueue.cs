using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.FileBrowser;

public class FileCommandQueue
{
    public FileBrowserScreen Editor;
    public ProjectEntry Project;

    public bool DoFocus = false;

    public FileCommandQueue(FileBrowserScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }
    public void Parse(string[] commands)
    {
        var activeView = Editor.ViewHandler.ActiveView;

    }
}
