using Hexa.NET.ImGui;
using Org.BouncyCastle.Crypto;
using StudioCore.Core;
using StudioCore.MaterialEditorNS;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.MaterialEditorNS;

/// <summary>
/// The list of each discrete material entry (.MTD or .MATBIN)
/// </summary>
public class MaterialFileList
{
    public MaterialEditorScreen Editor;
    public ProjectEntry Project;

    public MaterialFileList(MaterialEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }
    public void Draw()
    {
        Editor.Filters.DisplayFileFilterSearch();

        ImGui.BeginChild("FileList");

        // MTD
        if (Editor.Selection.SourceType is SourceType.MTD && Editor.Selection.MTDWrapper != null)
        {
            var files = Editor.Selection.MTDWrapper.Entries;

            var filteredEntries = new List<string>();
            foreach (var entry in files)
            {
                if (Editor.Filters.IsFileFilterMatch(entry.Key))
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
                    var curFile = Editor.Selection.MTDWrapper.Entries[key];

                    var displayName = GetPrettyName($"{key}");

                    if (ImGui.Selectable($"{displayName}##mtdFileEntry_{key}{i}", key == Editor.Selection.SelectedFileKey, ImGuiSelectableFlags.AllowDoubleClick))
                    {
                        Editor.Selection.SelectedFileKey = key;
                        Editor.Selection.SelectedMTD = curFile;

                        Editor.Selection.SelectedTextureIndex = -1;
                    }
                }
            }

            clipper.End();
        }

        // MATBIN
        if (Editor.Selection.SourceType is SourceType.MATBIN && MaterialUtils.SupportsMATBIN(Project))
        {
            if (Editor.Selection.MATBINWrapper != null)
            {
                var files = Editor.Selection.MATBINWrapper.Entries;

                var filteredEntries = new List<string>();
                foreach (var entry in files)
                {
                    if (Editor.Filters.IsFileFilterMatch(entry.Key))
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
                        var curFile = Editor.Selection.MATBINWrapper.Entries[key];

                        var displayName = GetPrettyName($"{key}");

                        if (ImGui.Selectable($"{displayName}##matbinFileEntry_{key}", key == Editor.Selection.SelectedFileKey, ImGuiSelectableFlags.AllowDoubleClick))
                        {
                            Editor.Selection.SelectedFileKey = key;
                            Editor.Selection.SelectedMATBIN = curFile;

                            Editor.Selection.SelectedTextureIndex = -1;
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

        if(Project.MaterialDisplayConfiguration != null && Project.MaterialDisplayConfiguration.FileListConfigurations != null)
        {
            var curConfig = Project.MaterialDisplayConfiguration.FileListConfigurations
                .Where(e => e.SourceType == $"{Editor.Selection.SourceType}")
                .Where(e => e.Binder == Editor.Selection.SelectedBinderEntry.Filename)
                .FirstOrDefault();

            if (curConfig != null)
            {
                newName = path.Replace(curConfig.CommonPath, "");
            }
        }

        return newName;
    }
}
