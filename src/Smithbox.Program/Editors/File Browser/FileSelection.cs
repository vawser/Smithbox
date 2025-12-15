using StudioCore.Application;

namespace StudioCore.Editors.FileBrowser;

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
