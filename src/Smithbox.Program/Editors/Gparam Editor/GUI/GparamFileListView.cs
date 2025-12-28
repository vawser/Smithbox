using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;

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
        Editor.Selection.SwitchWindowContext(GparamEditorContext.File);

        Editor.Filters.DisplayFileFilterSearch();

        ImGui.BeginChild("GparamFileSection");
        Editor.Selection.SwitchWindowContext(GparamEditorContext.File);

        foreach (var entry in Editor.Project.GparamData.PrimaryBank.Entries)
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
                if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
                {
                    Editor.Selection.SelectGparamFile = true;
                }

                if (CFG.Current.Interface_Display_Alias_for_Gparam)
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
