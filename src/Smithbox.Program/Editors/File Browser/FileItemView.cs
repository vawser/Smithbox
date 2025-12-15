using Hexa.NET.ImGui;
using StudioCore.Application;


namespace StudioCore.Editors.FileBrowser;

public class FileItemView
{
    public FileBrowserScreen Editor;
    public ProjectEntry Project;
    public FileItemView(FileBrowserScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Display()
    {
        ImGui.Begin($"Item Viewer##ItemViewer");

        DisplayItemViewer();

        ImGui.End();
    }

    private void DisplayItemViewer()
    {
        if (Editor.Selection.SelectedEntry == null)
        {
            ImGui.Text("Nothing selected");
        }
        else
        {
            if (Editor.Selection.SelectedEntry.CanView)
            {
                if (!Editor.Selection.SelectedEntry.IsInitialized && !Editor.Selection.SelectedEntry.IsLoading)
                {
                    Editor.Selection.SelectedEntry.LoadAsync(Editor.Selection.SelectedEntryID, Editor.Selection.SelectedEntry.Name, Project);
                }

                if (Editor.Selection.SelectedEntry.IsInitialized)
                {
                    Editor.Selection.SelectedEntry.OnGui();
                }
                else
                {
                    ImGui.Text("Loading...");
                }
            }
            else
            {
                ImGui.Text($"Selected: {Editor.Selection.SelectedEntry.Name}");
                ImGui.Text("This file has no Item Viewer.");
            }
        }
    }
}
