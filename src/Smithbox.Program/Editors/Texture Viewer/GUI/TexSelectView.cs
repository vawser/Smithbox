using Hexa.NET.ImGui;
using StudioCore.Application;

namespace StudioCore.Editors.TextureViewer;

public class TexSelectView
{
    public TextureViewerScreen Editor;
    public ProjectEntry Project;

    public TexSelectView(TextureViewerScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    /// <summary>
    /// The main UI for the file container list view
    /// </summary>
    public void Display()
    {
        ImGui.Begin("TPFs##TextureTpfList");
        Editor.Selection.SwitchWindowContext(TextureViewerContext.TpfList);

        Editor.Filters.DisplayTpfFilterSearch();

        ImGui.BeginChild("TpfList");
        Editor.Selection.SwitchWindowContext(TextureViewerContext.TpfList);

        if (Editor.Selection.SelectedBinder != null)
        {
            foreach (var entry in Editor.Selection.SelectedBinder.Files)
            {
                var file = entry.Key;
                var tpfEntry = entry.Value;

                if (Editor.Filters.IsTpfFilterMatch(file.Name))
                {
                    var displayName = file.Name;

                    var isSelected = false;
                    if(Editor.Selection.SelectedTpfKey == file.Name)
                    {
                        isSelected = true;
                    }

                    // Texture row
                    if (ImGui.Selectable($@"{displayName}", isSelected))
                    {
                        Editor.Selection.SelectTpfFile(entry.Key, entry.Value);
                    }

                    // Arrow Selection
                    if (ImGui.IsItemHovered() && Editor.Selection.SelectTpf)
                    {
                        Editor.Selection.SelectTpf = false;
                        Editor.Selection.SelectTpfFile(entry.Key, entry.Value);
                    }
                    if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
                    {
                        Editor.Selection.SelectTpf = true;
                    }
                }
            }
        }

        ImGui.EndChild();

        ImGui.End();
    }
}