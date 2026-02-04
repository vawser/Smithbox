using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;

namespace StudioCore.Editors.GparamEditor;

public class GparamFileList
{
    private GparamEditorView Parent;
    private ProjectEntry Project;

    public GparamFileList(GparamEditorView view, ProjectEntry project)
    {
        Parent = view;
        Project = project;
    }

    /// <summary>
    /// The main UI for this view
    /// </summary>
    public void Display()
    {
        UIHelper.SimpleHeader("Files", "");

        Parent.Filters.DisplayFileFilterSearch();

        ImGui.BeginChild("GparamFileSection");

        foreach (var entry in Parent.Project.Handler.GparamData.PrimaryBank.Entries)
        {
            var alias = AliasHelper.GetGparamAliasName(Parent.Project, entry.Key.Filename);

            if (Parent.Filters.IsFileFilterMatch(entry.Key.Filename, alias))
            {
                ImGui.BeginGroup();

                // File row
                if (ImGui.Selectable($@" {entry.Key.Filename}", entry.Key.Filename == Parent.Selection._selectedGparamKey))
                {
                    Parent.Selection.SetFileSelection(entry.Key);
                }

                // Arrow Selection
                if (ImGui.IsItemHovered() && Parent.Selection.SelectGparamFile)
                {
                    Parent.Selection.SelectGparamFile = false;

                    Parent.Selection.SetFileSelection(entry.Key);
                }

                if (ImGui.IsItemFocused())
                {
                    if (InputManager.HasArrowSelection())
                    {
                        Parent.Selection.SelectGparamFile = true;
                    }
                }

                if (CFG.Current.GparamEditor_File_List_Display_Aliases)
                {
                    UIHelper.DisplayAlias(alias);
                }

                ImGui.EndGroup();
            }

            Parent.ContextMenu.FileContextMenu(entry.Key);
        }

        ImGui.EndChild();
    }
}
