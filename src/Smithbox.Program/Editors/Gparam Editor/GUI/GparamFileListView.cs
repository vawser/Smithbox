using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;

namespace StudioCore.Editors.GparamEditor;

public class GparamFileListView
{
    private GparamEditorScreen Editor;
    private ProjectEntry Project;

    public GparamFileListView(GparamEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    /// <summary>
    /// The main UI for this view
    /// </summary>
    public void Display()
    {
        ImGui.Begin("Files##GparamFileList");
        FocusManager.SetFocus(EditorFocusContext.GparamEditor_FileList);

        Editor.Filters.DisplayFileFilterSearch();

        ImGui.BeginChild("GparamFileSection");

        foreach (var entry in Editor.Project.Handler.GparamData.PrimaryBank.Entries)
        {
            var alias = AliasHelper.GetGparamAliasName(Editor.Project, entry.Key.Filename);

            if (Editor.Filters.IsFileFilterMatch(entry.Key.Filename, alias))
            {
                ImGui.BeginGroup();

                // File row
                if (ImGui.Selectable($@" {entry.Key.Filename}", entry.Key.Filename == Editor.Selection._selectedGparamKey))
                {
                    Editor.Selection.SetFileSelection(entry.Key);
                }

                // Arrow Selection
                if (ImGui.IsItemHovered() && Editor.Selection.SelectGparamFile)
                {
                    Editor.Selection.SelectGparamFile = false;

                    Editor.Selection.SetFileSelection(entry.Key);
                }

                if (ImGui.IsItemFocused())
                {
                    if (InputManager.HasArrowSelection())
                    {
                        Editor.Selection.SelectGparamFile = true;
                    }
                }

                if (CFG.Current.GparamEditor_File_List_Display_Aliases)
                {
                    UIHelper.DisplayAlias(alias);
                }

                ImGui.EndGroup();
            }

            Editor.ContextMenu.FileContextMenu(entry.Key);
        }

        ImGui.EndChild();

        ImGui.End();
    }
}
