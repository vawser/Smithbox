using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System.Collections.Generic;

namespace StudioCore.Editors.MaterialEditor;

/// <summary>
/// The list of binders for the source type.
/// </summary>
public class MaterialContainerList
{
    public MaterialEditorView Parent;
    public ProjectEntry Project;

    public MaterialContainerList(MaterialEditorView view, ProjectEntry project)
    {
        Parent = view;
        Project = project;
    }

    public void Draw(float width, float height)
    {
        UIHelper.SimpleHeader("Containers", "");
        
        ImGui.BeginChild("ContainerList", new System.Numerics.Vector2(width, height), ImGuiChildFlags.Borders);

        ImGui.BeginTabBar("sourceTabs");

        if (ImGui.BeginTabItem($"MTD"))
        {
            Parent.Selection.SourceType = MaterialSourceType.MTD;

            Parent.Filters.DisplayBinderFilterSearch();

            DisplayMtdList();

            ImGui.EndTabItem();
        }

        if (MaterialUtils.SupportsMATBIN(Project))
        {
            if (ImGui.BeginTabItem($"MATBIN"))
            {
                Parent.Selection.SourceType = MaterialSourceType.MATBIN;

                Parent.Filters.DisplayBinderFilterSearch();

                DisplayMatbinList();

                ImGui.EndTabItem();
            }
        }

        ImGui.EndTabBar();

        ImGui.EndChild();
    }

    public void DisplayMtdList()
    {
        ImGui.BeginChild("mtdListSection");

        if (Parent.Selection.SourceType is MaterialSourceType.MTD)
        {
            var wrappers = Project.Handler.MaterialData.PrimaryBank.MTDs;

            if (wrappers != null)
            {
                var filteredEntries = new List<FileDictionaryEntry>();
                foreach (var entry in wrappers)
                {
                    if (Parent.Filters.IsBinderFilterMatch(entry.Key.Filename))
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
                        var curWrapper = Project.Handler.MaterialData.PrimaryBank.MTDs[key];

                        var displayName = $"{key.Filename}";

                        if (ImGui.Selectable($"{displayName}##mtdEntry_{key.Filename}{i}", key == Parent.Selection.SelectedBinderEntry, ImGuiSelectableFlags.AllowDoubleClick))
                        {
                            Parent.Selection.SelectedBinderEntry = key;
                            Parent.Selection.MTDWrapper = curWrapper;

                            Parent.Selection.SelectedFileKey = "";
                            Parent.Selection.SelectedMTD = null;
                            Parent.Selection.SelectedMATBIN = null;
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
            if (Parent.Selection.SourceType is MaterialSourceType.MATBIN)
            {
                var wrappers = Project.Handler.MaterialData.PrimaryBank.MATBINs;

                if (wrappers != null)
                {
                    var filteredEntries = new List<FileDictionaryEntry>();
                    foreach (var entry in wrappers)
                    {
                        if (Parent.Filters.IsBinderFilterMatch(entry.Key.Filename))
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
                            var curWrapper = Project.Handler.MaterialData.PrimaryBank.MATBINs[key];

                            var displayName = $"{key.Filename}";

                            if (ImGui.Selectable($"{displayName}##matbinEntry_{key}", key == Parent.Selection.SelectedBinderEntry, ImGuiSelectableFlags.AllowDoubleClick))
                            {
                                Parent.Selection.SelectedBinderEntry = key;
                                Parent.Selection.MATBINWrapper = curWrapper;

                                Parent.Selection.SelectedFileKey = "";
                                Parent.Selection.SelectedMTD = null;
                                Parent.Selection.SelectedMATBIN = null;
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
