using StudioCore.Application;
using System;
using System.Linq;

namespace StudioCore.Editors.FileBrowser;

/// <summary>
/// Commands for the File Browser:
///   file/select/&lt;path&gt;  selects and reveals a file, e.g. "file/select/chr/c2010.chrbnd.dcx"
///                          (a folder path selects and reveals the folder)
/// </summary>
public class FileCommandQueue
{
    public FileBrowserScreen Editor;
    public ProjectEntry Project;

    public bool DoFocus = false;

    // A select that arrived before the file tree was built
    private string PendingSelect = null;

    public FileCommandQueue(FileBrowserScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Parse(string[] commands)
    {
        if (commands is { Length: >= 2 } && commands[0] == "select")
        {
            // The path's own slashes split it into parts; put them back together
            PendingSelect = "/" + string.Join("/", commands.Skip(1).Where(p => p != ""));
        }

        if (PendingSelect != null && Editor.Tree.Root != null)
        {
            var path = PendingSelect;
            PendingSelect = null;

            HandleSelect(path);
        }
    }

    private void HandleSelect(string path)
    {
        var view = Editor.ViewHandler.ActiveView;
        if (view == null)
            return;

        var file = Editor.Tree.FindFile(path);
        if (file != null)
        {
            view.Selection.SelectAndReveal(file);
            return;
        }

        var folder = Editor.Tree.FindFolder(path.TrimEnd('/'));
        if (folder != null)
        {
            view.Selection.SelectedVfsFolder = folder;
            view.Selection.Reveal(folder.Path);
            return;
        }

        Smithbox.Log(this, LOC.Get("FILE_Command_Select_Not_Found", path));
    }
}
