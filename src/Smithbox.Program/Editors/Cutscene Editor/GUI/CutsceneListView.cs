#nullable enable
using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using StudioCore.Editors.MetadataEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.CutsceneEditor;

/// <summary>
/// The project's cutscenes, with their aliases where the project has them (by the EMEVD cutscene ID: s10_00_0020 is
/// 10000020), and the cuts of the selected one.
/// </summary>
public class CutsceneListView
{
    private readonly CutsceneEditorScreen Editor;

    private string Filter = "";
    public bool ScrollToSelection;

    private static readonly Regex NamePattern = new(@"^s(\d{2})_(\d{2})_(\d{4})$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private Dictionary<string, string>? Aliases;

    public CutsceneListView(CutsceneEditorScreen editor)
    {
        Editor = editor;
    }

    /// <summary>The EMEVD cutscene ID of a cutscene name, or null for a name of another shape.</summary>
    public static int? CutsceneId(string name)
    {
        var m = NamePattern.Match(name);
        if (!m.Success)
            return null;

        return int.Parse(m.Groups[1].Value) * 1000000 + int.Parse(m.Groups[2].Value) * 10000 + int.Parse(m.Groups[3].Value);
    }

    private string? AliasOf(string name)
    {
        if (Aliases == null)
        {
            Aliases = new(StringComparer.OrdinalIgnoreCase);
            if (Editor.Project.Handler.ProjectData?.Aliases.TryGetValue(ProjectAliasType.Cutscenes, out var entries) == true)
            {
                foreach (var entry in entries)
                    Aliases.TryAdd(entry.ID, entry.Name);
            }
        }

        return CutsceneId(name) is { } id && Aliases.TryGetValue(id.ToString(), out var alias) ? alias : null;
    }

    public void Display()
    {
        var files = Editor.Data?.Files;
        if (files == null)
            return;

        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##cutsceneFilter", LOC.Get("CUTSCENE_List_Filter_Hint"), ref Filter, 128);

        var selected = Editor.SelectedFile;

        // The cuts of the selected cutscene, above the list so they stay in view
        if (selected?.Mqb is { } mqb)
        {
            ImGui.SeparatorText(LOC.Get("CUTSCENE_List_Cuts", selected.Name));
            for (var i = 0; i < mqb.Cuts.Count; i++)
            {
                var cut = mqb.Cuts[i];
                var seconds = cut.Duration / (mqb.Framerate > 0 ? mqb.Framerate : 30);
                if (ImGui.Selectable($"{cut.Name}  ({cut.Duration} f, {seconds:0.0} s)##cut{i}", i == Editor.SelectedCut))
                    Editor.SelectCut(i);
            }
        }
        else if (selected?.LoadError is { } error)
        {
            ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, error);
        }

        ImGui.SeparatorText(LOC.Get("CUTSCENE_List_Cutscenes", files.Count));

        if (ImGui.BeginChild("##cutsceneListChild"))
        {
            foreach (var file in files)
            {
                var alias = AliasOf(file.Name);
                if (Filter.Length > 0
                    && !file.Name.Contains(Filter, StringComparison.OrdinalIgnoreCase)
                    && !(alias?.Contains(Filter, StringComparison.OrdinalIgnoreCase) ?? false))
                    continue;

                var isSelected = file == selected;
                if (ImGui.Selectable($"{file.Name}##cutscene_{file.Name}", isSelected))
                    Editor.Select(file, 0);

                if (isSelected && ScrollToSelection)
                {
                    ImGui.SetScrollHereY();
                    ScrollToSelection = false;
                }

                if (alias != null)
                {
                    ImGui.SameLine();
                    ImGui.TextColored(UI.Current.ImGui_AliasName_Text, alias);
                }
            }
        }

        ImGui.EndChild();
    }
}
