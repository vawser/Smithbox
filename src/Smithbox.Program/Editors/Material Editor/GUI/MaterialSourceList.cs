using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Utilities;
using System.Collections.Generic;

namespace StudioCore.Editors.MaterialEditor;

/// <summary>
/// The list of binders for the source type.
/// </summary>
public class MaterialSourceList
{
    public MaterialEditorScreen Editor;
    public ProjectEntry Project;

    public MaterialSourceList(MaterialEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Draw()
    {
        if (Project.MaterialData.PrimaryBank == null)
            return;

        ImGui.BeginTabBar("sourceTabs");


        if (ImGui.BeginTabItem($"MTD"))
        {
            Editor.Selection.SourceType = MaterialSourceType.MTD;

            Editor.Filters.DisplayBinderFilterSearch();

            DisplayMtdList();

            ImGui.EndTabItem();
        }


        if (ImGui.BeginTabItem($"MATBIN"))
        {
            Editor.Selection.SourceType = MaterialSourceType.MATBIN;

            Editor.Filters.DisplayBinderFilterSearch();

            DisplayMatbinList();

            ImGui.EndTabItem();
        }

        ImGui.EndTabBar();
    }

    public void DisplayMtdList()
    {
        ImGui.BeginChild("mtdListSection");

        if (Editor.Selection.SourceType is MaterialSourceType.MTD)
        {
            var wrappers = Project.MaterialData.PrimaryBank.MTDs;

            if (wrappers != null)
            {
                var filteredEntries = new List<FileDictionaryEntry>();
                foreach (var entry in wrappers)
                {
                    if (Editor.Filters.IsBinderFilterMatch(entry.Key.Filename))
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
                        var curWrapper = Project.MaterialData.PrimaryBank.MTDs[key];

                        var displayName = $"{key.Filename}";

                        if (ImGui.Selectable($"{displayName}##mtdEntry_{key.Filename}{i}", key == Editor.Selection.SelectedBinderEntry, ImGuiSelectableFlags.AllowDoubleClick))
                        {
                            Editor.Selection.SelectedBinderEntry = key;
                            Editor.Selection.MTDWrapper = curWrapper;

                            Editor.Selection.SelectedFileKey = "";
                            Editor.Selection.SelectedMTD = null;
                            Editor.Selection.SelectedMATBIN = null;
                        }
                    }
                }

                clipper.End();
            }
        }

        ImGui.EndChild();
    }

    public void DisplayMatbinList()
    {
        ImGui.BeginChild("mtdListSection");

        if (MaterialUtils.SupportsMATBIN(Project))
        {
            if (Editor.Selection.SourceType is MaterialSourceType.MATBIN)
            {
                var wrappers = Project.MaterialData.PrimaryBank.MATBINs;

                if (wrappers != null)
                {
                    var filteredEntries = new List<FileDictionaryEntry>();
                    foreach (var entry in wrappers)
                    {
                        if (Editor.Filters.IsBinderFilterMatch(entry.Key.Filename))
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
                            var curWrapper = Project.MaterialData.PrimaryBank.MATBINs[key];

                            var displayName = $"{key.Filename}";

                            if (ImGui.Selectable($"{displayName}##matbinEntry_{key}", key == Editor.Selection.SelectedBinderEntry, ImGuiSelectableFlags.AllowDoubleClick))
                            {
                                Editor.Selection.SelectedBinderEntry = key;
                                Editor.Selection.MATBINWrapper = curWrapper;

                                Editor.Selection.SelectedFileKey = "";
                                Editor.Selection.SelectedMTD = null;
                                Editor.Selection.SelectedMATBIN = null;
                            }
                        }
                    }

                    clipper.End();
                }
            }
        }

        ImGui.EndChild();
    }
}
