using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using System.Numerics;

namespace StudioCore.Editors.TextureViewer;

public class TexInternalFileList
{
    public TexEditorView Parent;
    public ProjectEntry Project;

    private string TpfFileListFilter = "";
    private bool ExactTpfFileListFilter = false;

    public TexInternalFileList(TexEditorView view, ProjectEntry project)
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

        EditorFilters.DisplayFramedListFilter("textureViewer_TpfList", ref TpfFileListFilter, ref ExactTpfFileListFilter);

        ImGui.BeginChild("TpfList", new Vector2(0, 0), ImGuiChildFlags.Borders);

        if (Parent.Selection.SelectedBinder != null)
        {
            int index = 0;

            foreach (var entry in Parent.Selection.SelectedBinder.Files)
            {
                var file = entry.Key;
                var tpfEntry = entry.Value;

                var isMatch = EditorFilters.IsMatch(
                    TpfFileListFilter, file.Name, ExactTpfFileListFilter, "", true);

                if (isMatch)
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
                        Parent.Selection.AutoSelectTexture = true;
                    }

                    // Arrow Selection
                    if (ImGui.IsItemHovered() && Parent.Selection.SelectTpf)
                    {
                        Parent.Selection.SelectTpf = false;
                        Parent.Selection.SelectTpfFile(entry.Key, entry.Value);
                        Parent.Selection.AutoSelectTexture = true;
                    }

                    if (ImGui.IsItemFocused())
                    {
                        if (InputManager.HasArrowSelection())
                        {
                            Parent.Selection.SelectTpf = true;
                        }
                    }

                    if (index == 0 && Parent.Selection.AutoSelectTpf)
                    {
                        Parent.Selection.AutoSelectTpf = false;
                        Parent.Selection.SelectTpfFile(entry.Key, entry.Value);
                        Parent.Editor.ViewHandler.ActiveView = Parent;
                        Parent.Selection.AutoSelectTexture = true;
                    }
                }

                index++;
            }
        }

        ImGui.EndChild();
    }
}