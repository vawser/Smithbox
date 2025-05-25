using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Interface;
using StudioCore.Utilities;
using System.Linq;

namespace StudioCore.EzStateEditorNS;

/// <summary>
/// Handles the file selection, viewing and editing.
/// </summary>
public class EsdFileView
{
    public EsdEditorScreen Editor;
    public ProjectEntry Project;

    public EsdFileView(EsdEditorScreen editor, ProjectEntry project)
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
        ImGui.Begin("Files##TalkFileList");
        Editor.Selection.SwitchWindowContext(EsdEditorContext.File);

        Editor.Filters.DisplayFileFilterSearch();

        ImGui.BeginChild("FileListSection");
        Editor.Selection.SwitchWindowContext(EsdEditorContext.File);

        // Talk
        if (ImGui.CollapsingHeader("Talk", ImGuiTreeNodeFlags.DefaultOpen))
        {
            int index = 0;

            foreach (var entry in Project.EsdData.PrimaryBank.Scripts)
            {
                if (entry.Key.Extension == "talkesdbnd")
                {
                    var displayName = $"{entry.Key.Filename}";
                    var aliasName = AliasUtils.GetMapNameAlias(Project, displayName);

                    if (Editor.Filters.IsFileFilterMatch(displayName, aliasName))
                    {
                        // File row
                        if (ImGui.Selectable($@" {displayName}##fileEntry{index}", entry.Key == Editor.Selection.SelectedFileEntry))
                        {
                            Editor.Selection.ResetScript();
                            Editor.Selection.ResetStateGroup();
                            Editor.Selection.ResetStateGroupNode();

                            Editor.Selection.SetFile(entry.Key);
                        }

                        // Arrow Selection
                        if (ImGui.IsItemHovered() && Editor.Selection.SelectNextFile)
                        {
                            Editor.Selection.SelectNextFile = false;
                            Editor.Selection.SetFile(entry.Key);
                        }
                        if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
                        {
                            Editor.Selection.SelectNextFile = true;
                        }

                        // Only apply to selection
                        if (entry.Key == Editor.Selection.SelectedFileEntry)
                        {
                            Editor.ContextMenu.FileContextMenu(entry.Key);
                        }

                        UIHelper.DisplayAlias(aliasName);
                    }
                }

                index++;
            }
        }

        // Loose
        if (Project.EsdData.EsdFiles.Entries.Any(e => e.Extension == "esd"))
        {
            if (ImGui.CollapsingHeader("Loose", ImGuiTreeNodeFlags.DefaultOpen))
            {
                int index = 0;

                foreach (var entry in Project.EsdData.PrimaryBank.Scripts)
                {
                    if (entry.Key.Extension == "esd")
                    {
                        var displayName = $"{entry.Key.Filename}";
                        var aliasName = AliasUtils.GetMapNameAlias(Project, displayName);

                        if (Editor.Filters.IsFileFilterMatch(displayName, aliasName))
                        {
                            // File row
                            if (ImGui.Selectable($@" {displayName}##looseFileEntry{index}", entry.Key == Editor.Selection.SelectedFileEntry))
                            {
                                Editor.Selection.ResetScript();
                                Editor.Selection.ResetStateGroup();
                                Editor.Selection.ResetStateGroupNode();

                                Editor.Selection.SetFile(entry.Key);
                            }

                            // Arrow Selection
                            if (ImGui.IsItemHovered() && Editor.Selection.SelectNextFile)
                            {
                                Editor.Selection.SelectNextFile = false;
                                Editor.Selection.SetFile(entry.Key);
                            }
                            if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
                            {
                                Editor.Selection.SelectNextFile = true;
                            }

                            // Only apply to selection
                            if (entry.Key == Editor.Selection.SelectedFileEntry)
                            {
                                Editor.ContextMenu.FileContextMenu(entry.Key);
                            }

                            UIHelper.DisplayAlias(aliasName);
                        }
                    }

                    index++;
                }
            }
        }

        ImGui.EndChild();

        ImGui.End();
    }
}
