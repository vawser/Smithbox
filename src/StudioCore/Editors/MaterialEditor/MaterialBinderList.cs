using Hexa.NET.ImGui;
using StudioCore.Core;
using StudioCore.Formats.JSON;
using StudioCore.Utilities;
using System.Collections.Generic;

namespace StudioCore.MaterialEditorNS;

/// <summary>
/// The list of binders for the source type.
/// </summary>
public class MaterialBinderList
{
    public MaterialEditorScreen Editor;
    public ProjectEntry Project;

    public MaterialBinderList(MaterialEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Draw()
    {
        // Source Type
        if (MaterialUtils.SupportsMATBIN(Project))
        {
            if (ImGui.BeginCombo("##sourceTypeCombo", Editor.Selection.SourceType.GetDisplayName()))
            {
                if (ImGui.Selectable("MTD"))
                {
                    Editor.Selection.SourceType = SourceType.MTD;
                }

                if (ImGui.Selectable("MATBIN"))
                {
                    Editor.Selection.SourceType = SourceType.MATBIN;
                }
                ImGui.EndCombo();
            }
        }
        else
        {
            Editor.Selection.SourceType = SourceType.MTD;
        }

        Editor.Filters.DisplayBinderFilterSearch();

        ImGui.BeginChild("BinderList");

        if (Editor.Selection.SourceType is SourceType.MTD)
        {
            var wrappers = Project.MaterialData.PrimaryBank.MTDs;

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

        if (MaterialUtils.SupportsMATBIN(Project))
        {
            if (Editor.Selection.SourceType is SourceType.MATBIN)
            {
                var wrappers = Project.MaterialData.PrimaryBank.MATBINs;

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

        ImGui.EndChild();
    }
}
