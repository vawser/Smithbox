using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using System.Numerics;

namespace StudioCore.Editors.TextureViewer;

public class TexInternalFileListMenu
{
    public TexView Parent;
    public ProjectEntry Project;

    public TexInternalFileListMenu(TexView view, ProjectEntry project)
    {
        Parent = view;
        Project = project;
    }

    /// <summary>
    /// The main UI for the file container list view
    /// </summary>
    public void Display(float width, float height)
    {
        UIHelper.SimpleHeader("Files", "");

        Parent.Editor.Filters.DisplayTpfFilterSearch();

        ImGui.BeginChild("TpfList", new Vector2(width, height));

        if (Parent.Selection.SelectedBinder != null)
        {
            foreach (var entry in Parent.Selection.SelectedBinder.Files)
            {
                var file = entry.Key;
                var tpfEntry = entry.Value;

                if (Parent.Editor.Filters.IsTpfFilterMatch(file.Name))
                {
                    var displayName = file.Name;

                    var isSelected = false;
                    if(Parent.Selection.SelectedTpfKey == file.Name)
                    {
                        isSelected = true;
                    }

                    // Texture row
                    if (ImGui.Selectable($@"{displayName}", isSelected))
                    {
                        Parent.Selection.SelectTpfFile(entry.Key, entry.Value);
                        Parent.Editor.ViewHandler.ActiveView = Parent;
                    }

                    // Arrow Selection
                    if (ImGui.IsItemHovered() && Parent.Selection.SelectTpf)
                    {
                        Parent.Selection.SelectTpf = false;
                        Parent.Selection.SelectTpfFile(entry.Key, entry.Value);
                    }

                    if (ImGui.IsItemFocused())
                    {
                        if (InputManager.HasArrowSelection())
                        {
                            Parent.Selection.SelectTpf = true;
                        }
                    }
                }
            }
        }

        ImGui.EndChild();
    }
}