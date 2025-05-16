using StudioCore.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.FileBrowserNS;

public class FileSelection
{
    public FileBrowserScreen Editor;
    public ProjectEntry Project;

    public FsEntry SelectedEntry = null;
    public string SelectedEntryID = "";

    public FileSelection(FileBrowserScreen baseEditor, ProjectEntry project)
    {
        Editor = baseEditor;
        Project = project;
    }

    public void SelectFile(string id, FsEntry selectedEntry)
    {
        SelectedEntryID = id;
        SelectedEntry = selectedEntry;
    }
}
