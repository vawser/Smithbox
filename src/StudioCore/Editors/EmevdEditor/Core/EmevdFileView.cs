using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Interface;
using StudioCore.Utilities;

namespace StudioCore.EventScriptEditorNS;

/// <summary>
/// Handles the file selection, viewing and editing.
/// </summary>
public class EmevdFileView
{
    public EmevdEditorScreen Editor;
    public ProjectEntry Project;

    public EmevdFileView(EmevdEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    /// <summary>
    /// The main UI for the file view
    /// </summary>
    public void Display()
    {
        // File List
        ImGui.Begin("Files##EventScriptFileList");
        Editor.Selection.SwitchWindowContext(EmevdEditorContext.File);

        Editor.Filters.DisplayFileFilterSearch();

        ImGui.BeginChild("FileListSection");
        Editor.Selection.SwitchWindowContext(EmevdEditorContext.File);

        foreach (var entry in Project.EmevdData.PrimaryBank.Scripts)
        {
            var fileEntry = entry.Key;

            var displayName = $"{fileEntry.Filename}";
            var aliasName = AliasUtils.GetMapNameAlias(Editor.Project, fileEntry.Filename);

            if (Editor.Filters.IsFileFilterMatch(displayName, aliasName))
            {
                // Script row
                if (ImGui.Selectable(displayName, fileEntry == Editor.Selection.SelectedFileEntry))
                {
                    Editor.Selection.SelectFile(fileEntry);
                }

                // Arrow Selection
                if (ImGui.IsItemHovered() && Editor.Selection.SelectNextScript)
                {
                    Editor.Selection.SelectNextScript = false;
                    Editor.Selection.SelectFile(fileEntry);
                }
                if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
                {
                    Editor.Selection.SelectNextScript = true;
                }

                // Only apply to selection
                if (fileEntry == Editor.Selection.SelectedFileEntry)
                {
                    Editor.ContextMenu.FileContextMenu();
                }

                UIHelper.DisplayAlias(aliasName);
            }
        }

        ImGui.EndChild();

        ImGui.End();
    }
}
