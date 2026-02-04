using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.MaterialEditor;

/// <summary>
/// The list of each discrete material entry (.MTD or .MATBIN)
/// </summary>
public class MaterialFileList
{
    public MaterialEditorView Parent;
    public ProjectEntry Project;

    public MaterialFileList(MaterialEditorView view, ProjectEntry project)
    {
        Parent = view;
        Project = project;
    }
    public void Draw(float width, float height)
    {
        UIHelper.SimpleHeader("Files", "");

        Parent.Filters.DisplayFileFilterSearch();

        ImGui.BeginChild("FileList", new System.Numerics.Vector2(width, height), ImGuiChildFlags.Borders);

        // MTD
        if (Parent.Selection.SourceType is MaterialSourceType.MTD && Parent.Selection.MTDWrapper != null)
        {
            var files = Parent.Selection.MTDWrapper.Entries;

            var filteredEntries = new List<string>();
            foreach (var entry in files)
            {
                if (Parent.Filters.IsFileFilterMatch(entry.Key))
                {
                    filteredEntries.Add(entry.Key);
                }
            }

            var clipper = new ImGuiListClipper();
            clipper.Begin(filteredEntries.Count);

            while (clipper.Step())
            {
                for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                {
                    var key = filteredEntries[i];
                    var curFile = Parent.Selection.MTDWrapper.Entries[key];

                    var displayName = GetPrettyName($"{key}");

                    if (ImGui.Selectable($"{displayName}##mtdFileEntry_{key}{i}", key == Parent.Selection.SelectedFileKey, ImGuiSelectableFlags.AllowDoubleClick))
                    {
                        Parent.Selection.SelectedFileKey = key;
                        Parent.Selection.SelectedMTD = curFile;
                    }

                    // Arrow Selection
                    if (ImGui.IsItemHovered() && Parent.Selection.SelectFileListEntry)
                    {
                        Parent.Selection.SelectFileListEntry = false;

                        Parent.Selection.SelectedFileKey = key;
                        Parent.Selection.SelectedMTD = curFile;
                    }

                    if (ImGui.IsItemFocused())
                    {
                        if (InputManager.HasArrowSelection())
                        {
                            Parent.Selection.SelectFileListEntry = true;
                        }
                    }
                }
            }

            clipper.End();
        }

        // MATBIN
        if (Parent.Selection.SourceType is MaterialSourceType.MATBIN && MaterialUtils.SupportsMATBIN(Project))
        {
            if (Parent.Selection.MATBINWrapper != null)
            {
                var files = Parent.Selection.MATBINWrapper.Entries;

                var filteredEntries = new List<string>();
                foreach (var entry in files)
                {
                    if (Parent.Filters.IsFileFilterMatch(entry.Key))
                    {
                        filteredEntries.Add(entry.Key);
                    }
                }

                var clipper = new ImGuiListClipper();
                clipper.Begin(filteredEntries.Count);

                while (clipper.Step())
                {
                    for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                    {
                        var key = filteredEntries[i];
                        var curFile = Parent.Selection.MATBINWrapper.Entries[key];

                        var displayName = GetPrettyName($"{key}");

                        if (ImGui.Selectable($"{displayName}##matbinFileEntry_{key}", key == Parent.Selection.SelectedFileKey, ImGuiSelectableFlags.AllowDoubleClick))
                        {
                            Parent.Selection.SelectedFileKey = key;
                            Parent.Selection.SelectedMATBIN = curFile;
                        }

                        // Arrow Selection
                        if (ImGui.IsItemHovered() && Parent.Selection.SelectFileListEntry)
                        {
                            Parent.Selection.SelectFileListEntry = false;

                            Parent.Selection.SelectedFileKey = key;
                            Parent.Selection.SelectedMATBIN = curFile;
                        }

                        if (ImGui.IsItemFocused())
                        {
                            if (InputManager.HasArrowSelection())
                            {
                                Parent.Selection.SelectFileListEntry = true;
                            }
                        }
                    }
                }

                clipper.End();
            }
        }

        ImGui.EndChild();
    }

    public string GetPrettyName(string path)
    {
        var newName = path;

        if (Project.Handler.MaterialData.MaterialDisplayConfiguration != null && Project.Handler.MaterialData.MaterialDisplayConfiguration.FileListConfigurations != null)
        {
            var curConfig = Project.Handler.MaterialData.MaterialDisplayConfiguration.FileListConfigurations
                .Where(e => e.SourceType == $"{Parent.Selection.SourceType}")
                .Where(e => e.Binder == Parent.Selection.SelectedBinderEntry.Filename)
                .FirstOrDefault();

            if (curConfig != null)
            {
                newName = path.Replace(curConfig.CommonPath, "");
            }
        }

        return newName;
    }
}
