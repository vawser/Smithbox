using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using System.Numerics;

namespace StudioCore.Editors.SoundEditor;

/// <summary>
/// The sound banks, grouped by name prefix (cs, aeg, s, vc / c, m, rm …). Drawn as one flat, clipped list of group
/// and bank rows, so a thousand banks never become one huge draw list.
/// </summary>
public class SoundContainerList
{
    private readonly SoundEditorView View;

    private string Filter = "";
    private bool ExactFilter = false;

    private readonly HashSet<string> Collapsed = new(StringComparer.OrdinalIgnoreCase);

    // rebuilt when the filter, the collapsed groups or the container list change
    private readonly List<(string Group, SoundContainer Container, int Count)> Rows = new();
    private string RowsFilter;
    private bool RowsExact;
    private int RowsCollapseVersion = -1, CollapseVersion;
    private List<SoundContainer> RowsSource;

    public bool ScrollToSelection;

    public SoundContainerList(SoundEditorView view)
    {
        View = view;
    }

    public void Draw()
    {
        var data = View.Data;
        GUI.TitleHeader(LOC.Get("SOUND_Banks_Header"), LOC.Get(data?.Middleware == SoundMiddleware.Wwise ? "SOUND_Banks_Header_Wwise_TT" : "SOUND_Banks_Header_Fmod_TT"));

        ImGui.BeginChild("soundBankListHeader", EditorFilters.GetHeaderSize(), ImGuiChildFlags.Borders);
        EditorFilters.DisplaySearchbar("soundEditor_BankList", ref Filter, ref ExactFilter);
        ImGui.EndChild();

        ImGui.BeginChild("soundBankList", Vector2.Zero, ImGuiChildFlags.Borders);

        if (data == null || data.Containers.Count == 0)
        {
            ImGui.TextDisabled(LOC.Get("SOUND_Banks_None"));
            ImGui.EndChild();
            return;
        }

        BuildRows(data.Containers);

        var clipper = new ImGuiListClipper();
        clipper.Begin(Rows.Count);
        while (clipper.Step())
        {
            for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var (group, container, count) = Rows[i];
                if (container == null)
                    DrawGroupRow(group, count);
                else
                    DrawContainerRow(container);
            }
        }
        clipper.End();

        if (ScrollToSelection)
        {
            int row = Rows.FindIndex(r => r.Container == View.SelectedContainer);
            if (row >= 0)
                ImGui.SetScrollY(row * ImGui.GetTextLineHeightWithSpacing() - ImGui.GetWindowHeight() / 2);
            ScrollToSelection = false;
        }

        ImGui.EndChild();
    }

    private void DrawGroupRow(string group, int count)
    {
        // a plain selectable, so every row has the same height (the clipper needs that)
        var sign = Collapsed.Contains(group) && string.IsNullOrWhiteSpace(Filter) ? "+" : "-";
        if (ImGui.Selectable($"{sign} {group} ({count})##soundGroup_{group}", false))
            ToggleGroup(group);
    }

    private void ToggleGroup(string group)
    {
        if (!Collapsed.Remove(group))
            Collapsed.Add(group);
        CollapseVersion++;
    }

    private void DrawContainerRow(SoundContainer c)
    {
        var selected = View.SelectedContainer == c;
        var label = string.IsNullOrEmpty(c.Language) ? c.Name : $"{c.Name} [{c.Language}]";

        ImGui.Indent();
        if (ImGui.Selectable($"{label}##soundBank_{c.Path}", selected))
            View.SelectContainer(c);
        GUI.Tooltip(c.Path);

        // File alias: the character / asset / map the bank is for
        var alias = c.FileAlias;
        if (!string.IsNullOrEmpty(alias))
        {
            ImGui.SameLine();
            ImGui.TextColored(UI.Current.ImGui_AliasName_Text, alias);
        }

        if (c.State == SoundLoadState.Failed)
        {
            ImGui.SameLine();
            ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("SOUND_Banks_Failed"));
        }
        ImGui.Unindent();
    }

    private void BuildRows(List<SoundContainer> containers)
    {
        if (RowsSource == containers && RowsFilter == Filter && RowsExact == ExactFilter && RowsCollapseVersion == CollapseVersion)
            return;

        RowsSource = containers;
        RowsFilter = Filter;
        RowsExact = ExactFilter;
        RowsCollapseVersion = CollapseVersion;
        Rows.Clear();

        bool filtering = !string.IsNullOrWhiteSpace(Filter);
        foreach (var group in containers.GroupBy(c => c.Group))
        {
            var matches = filtering
                ? group.Where(c => EditorFilters.IsMatch(Filter, c.Name, ExactFilter, c.FileAlias ?? "") || EditorFilters.IsMatch(Filter, c.Path, ExactFilter)).ToList()
                : group.ToList();
            if (matches.Count == 0)
                continue;

            Rows.Add((group.Key, null, matches.Count));

            // a filter shows its matches even in collapsed groups
            if (Collapsed.Contains(group.Key) && !filtering)
                continue;

            foreach (var c in matches)
                Rows.Add((group.Key, c, 0));
        }
    }
}
