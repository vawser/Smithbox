using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System.Numerics;

namespace StudioCore.Editors.TextEditor;

/// <summary>
/// FMG Reloader (ER): replaces FMGs in the running game with the editor's copies, so text edits can be tested without
/// a restart. Lists the FMGs the game holds; a click selects that FMG here. See
/// Claude/Documentation/FMG Live Reload Analysis.md.
/// </summary>
public class FmgReloaderTool
{
    public TextEditorView View;
    public ProjectEntry Project;

    // Shared by every view, and by Save for "send on save"
    private TextGameLink Link => View.Editor.GameLink;

    private TextBank Bank => Project.Handler.TextData?.PrimaryBank;

    private const double RefreshSeconds = 3;

    private bool ShowHistory = true;
    private bool ShowGameFmgs = true;
    private string Filter = "";

    // The language whose resident containers have had their FMGs loaded for the names in the list
    private string LoadedNamesFor;

    public FmgReloaderTool(TextEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("TEXT_FmgReloader_Header")}##fmgReloaderToolHeader"))
            return;

        ImGui.BeginChild("FmgReloaderToolSection", ImGuiChildFlags.Borders);

        Display();

        ImGui.EndChild();
    }

    private void Display()
    {
        if (Project.Descriptor.ProjectType is not ProjectType.ER)
        {
            GUI.WrappedText(LOC.Get("TEXT_FmgReloader_Game_Not_Supported"));
            return;
        }

        GUI.WrappedText(LOC.Get("TEXT_FmgReloader_Hint"));

        // Polls only while the tool is shown
        if (CFG.Current.TextEditor_FmgReloader_AutoRefresh &&
            (Link.LastPoll == null || (DateTime.Now - Link.LastPoll.Value).TotalSeconds >= RefreshSeconds))
        {
            Link.RequestPoll();
        }

        DisplayConnection();
        DisplayReload();
        DisplayHistory();
        DisplayGameFmgs();
    }

    // Game
    private void DisplayConnection()
    {
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("TEXT_FmgReloader_Game_Header"), LOC.Get("TEXT_FmgReloader_Game_Header_TT"));

        var message = Link.StateMessage;
        if (string.IsNullOrEmpty(message))
            message = LOC.Get("TEXT_FmgReloader_State_Waiting");

        if (Link.State is TextGameLink.LinkState.Failed)
            GUI.WrappedTextColored(UI.Current.ImGui_Invalid_Text_Color, message);
        else
            GUI.WrappedText(message);

        if (Link.State is TextGameLink.LinkState.Attached && !string.IsNullOrEmpty(Link.Language))
        {
            GUI.WrappedText(LOC.Get("TEXT_FmgReloader_Language", Link.Language));

            if (Link.KeptBytes > 0)
                GUI.WrappedText(LOC.Get("TEXT_FmgReloader_Kept", $"{Link.KeptBytes / 1024.0:0.#}"));
        }

        ImGui.Checkbox($"{LOC.Get("TEXT_FmgReloader_Auto_Refresh")}##fmgReloaderAutoRefresh", ref CFG.Current.TextEditor_FmgReloader_AutoRefresh);
        GUI.Tooltip(LOC.Get("TEXT_FmgReloader_Auto_Refresh_TT", RefreshSeconds));

        GUI.MultiButtonInput("fmgReloaderRefreshActions",
            "fmgReloaderRefresh", LOC.Get("TEXT_FmgReloader_Refresh"), LOC.Get("TEXT_FmgReloader_Refresh_TT"),
            Link.RequestPoll,
            "fmgReloaderDetach", LOC.Get("TEXT_FmgReloader_Detach"), LOC.Get("TEXT_FmgReloader_Detach_TT"),
            () =>
            {
                CFG.Current.TextEditor_FmgReloader_AutoRefresh = false;
                Link.Detach();
            });
    }

    // Reload
    private void DisplayReload()
    {
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("TEXT_FmgReloader_Reload_Header"), LOC.Get("TEXT_FmgReloader_Reload_Header_TT"));

        var container = View.Selection.SelectedContainerWrapper;
        var fmg = View.Selection.SelectedFmgWrapper;

        if (container?.FileEntry == null || fmg == null)
        {
            GUI.WrappedText(LOC.Get("TEXT_FmgReloader_No_Selection"));
        }
        else
        {
            GUI.WrappedText(LOC.Get("TEXT_FmgReloader_Selected", FmgReloadSource.LabelOf(container, fmg)));

            var problem = SelectionProblem(container, fmg);

            if (problem != null)
            {
                GUI.WrappedTextColored(UI.Current.ImGui_Invalid_Text_Color, problem);
            }
            else if (Link.IsReloading)
            {
                GUI.WrappedText(LOC.Get("TEXT_FmgReloader_Reload_Running"));
            }
            else
            {
                GUI.MultiButtonInput("fmgReloaderReloadActions",
                    "fmgReloaderReloadFmg", LOC.Get("TEXT_FmgReloader_Reload_Fmg"), LOC.Get("TEXT_FmgReloader_Reload_Fmg_TT"),
                    () => Send(new[] { FmgReloadSource.FromEditor(container, fmg) }, TextGameLink.SendSource.Selected),
                    "fmgReloaderReloadContainer", LOC.Get("TEXT_FmgReloader_Reload_Container"), LOC.Get("TEXT_FmgReloader_Reload_Container_TT"),
                    () => Send(FmgReloadSource.FromContainer(container).Where(i => InGame(i.Id)).ToList(), TextGameLink.SendSource.Container));
            }
        }

        // Modified
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("TEXT_FmgReloader_Modified_Header"), LOC.Get("TEXT_FmgReloader_Modified_Header_TT"));

        if (string.IsNullOrEmpty(Link.Language))
        {
            GUI.WrappedText(LOC.Get("TEXT_FmgReloader_List_Not_Attached"));
        }
        else
        {
            var dirty = FmgReloadSource.DirtyFmgs(Bank, Link.Language);
            GUI.WrappedText(LOC.Get("TEXT_FmgReloader_Modified_Count", dirty.Count, Link.Language));

            if (dirty.Count > 0 && !Link.IsReloading)
            {
                GUI.MultiButtonInput("fmgReloaderModifiedActions",
                    "fmgReloaderReloadModified", LOC.Get("TEXT_FmgReloader_Modified_Reload"), LOC.Get("TEXT_FmgReloader_Modified_Reload_TT"),
                    () => Send(FmgReloadSource.Dirty(Bank, Link.Language), TextGameLink.SendSource.Modified));
            }
        }

        ImGui.Checkbox($"{LOC.Get("TEXT_FmgReloader_Send_On_Save")}##fmgReloaderSendOnSave", ref CFG.Current.TextEditor_FmgReloader_SendOnSave);
        GUI.Tooltip(LOC.Get("TEXT_FmgReloader_Send_On_Save_TT"));

        var last = Link.LastReload;
        if (last != null)
        {
            var text = $"{last.At:T}  {last.Message}";

            if (last.Outcome is TextGameLink.ReloadOutcome.Reloaded)
                GUI.WrappedText(text);
            else
                GUI.WrappedTextColored(UI.Current.ImGui_Invalid_Text_Color, text);
        }

        GUI.WrappedText(LOC.Get("TEXT_FmgReloader_Reload_Note"));
    }

    /// <summary>
    /// Why the selected FMG can't go to the game, or null when it can
    /// </summary>
    private string SelectionProblem(TextContainerWrapper container, TextFmgWrapper fmg)
    {
        if (!TextGameLink.IsResidentName(container))
            return LOC.Get("TEXT_FmgReloader_Problem_Not_Resident", container.FileEntry.Filename, string.Join(", ", TextGameLink.ResidentContainers));

        if (Link.State is not TextGameLink.LinkState.Attached || string.IsNullOrEmpty(Link.Language))
            return LOC.Get("TEXT_FmgReloader_List_Not_Attached");

        var language = TextGameLink.LanguageOf(container);
        if (!string.Equals(language, Link.Language, StringComparison.OrdinalIgnoreCase))
            return LOC.Get("TEXT_FmgReloader_Problem_Language", language, Link.Language);

        if (Link.Fmgs.Count == 0)
            return LOC.Get("TEXT_FmgReloader_State_Not_Loaded");

        if (!InGame(fmg.ID))
            return LOC.Get("TEXT_FmgReloader_Problem_No_Slot", fmg.ID);

        return null;
    }

    private bool InGame(int id) => Link.Fmgs.Any(f => f.Id == id);

    private void Send(IReadOnlyList<TextGameLink.SendItem> items, TextGameLink.SendSource source)
    {
        var list = items.Where(i => i != null).ToList();
        if (list.Count > 0)
            Link.RequestReloadMany(list, source);
    }

    // History
    private void DisplayHistory()
    {
        GUI.Spacer();
        GUI.ConditionalHeader(LOC.Get("TEXT_FmgReloader_History_Header"), LOC.Get("TEXT_FmgReloader_History_Header_TT"), ref ShowHistory);

        var history = Link.History;

        if (history.Count == 0)
        {
            GUI.WrappedText(LOC.Get("TEXT_FmgReloader_History_Empty"));
            return;
        }

        if (ShowHistory)
        {
            var flags = ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.SizingStretchProp | ImGuiTableFlags.ScrollY;
            var height = Math.Min(history.Count, 8) * ImGui.GetFrameHeightWithSpacing() + ImGui.GetFrameHeightWithSpacing();

            if (ImGui.BeginTable("fmgReloaderHistory", 3, flags, new Vector2(-1, height)))
            {
                ImGui.TableSetupScrollFreeze(0, 1);
                ImGui.TableSetupColumn(LOC.Get("TEXT_FmgReloader_Column_Fmg"), ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableSetupColumn(LOC.Get("TEXT_FmgReloader_History_Column_Sent"), ImGuiTableColumnFlags.WidthFixed, 120);
                ImGui.TableSetupColumn("##send", ImGuiTableColumnFlags.WidthFixed, 45);
                ImGui.TableHeadersRow();

                foreach (var entry in history)
                {
                    var (container, fmg) = FmgReloadSource.Find(Bank, entry.ContainerPath, entry.Id);
                    var source = LOC.Get($"TEXT_FmgReloader_Source_{entry.Source}");

                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    var failed = entry.Outcome is not TextGameLink.ReloadOutcome.Reloaded;
                    if (failed)
                        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Invalid_Text_Color);

                    if (ImGui.Selectable($"{entry.Label}##fmgReloaderHistory_{entry.ContainerPath}_{entry.Id}") && fmg != null)
                        SelectInEditor(container, fmg);

                    if (failed)
                        ImGui.PopStyleColor();

                    GUI.Tooltip(LOC.Get("TEXT_FmgReloader_History_Row_TT", source, entry.Outcome));

                    ImGui.TableSetColumnIndex(1);
                    ImGui.TextUnformatted($"{entry.At:T} {source}");

                    if (fmg == null || Link.IsReloading)
                        continue;

                    ImGui.TableSetColumnIndex(2);
                    if (ImGui.SmallButton($"{LOC.Get("TEXT_FmgReloader_History_Send")}##fmgReloaderHistorySend_{entry.ContainerPath}_{entry.Id}"))
                        Send(new[] { FmgReloadSource.FromEditor(container, fmg) }, TextGameLink.SendSource.History);
                    GUI.Tooltip(LOC.Get("TEXT_FmgReloader_History_Send_TT"));
                }

                ImGui.EndTable();
            }
        }

        GUI.MultiButtonInput("fmgReloaderHistoryActions",
            "fmgReloaderHistoryClear", LOC.Get("TEXT_FmgReloader_History_Clear"), LOC.Get("TEXT_FmgReloader_History_Clear_TT"),
            Link.ClearHistory);
    }

    // Game FMGs
    private void DisplayGameFmgs()
    {
        GUI.Spacer();
        GUI.ConditionalHeader(LOC.Get("TEXT_FmgReloader_Game_Fmgs_Header"), LOC.Get("TEXT_FmgReloader_Game_Fmgs_Header_TT"), ref ShowGameFmgs);

        if (Link.State is not TextGameLink.LinkState.Attached || string.IsNullOrEmpty(Link.Language))
        {
            GUI.WrappedText(LOC.Get("TEXT_FmgReloader_List_Not_Attached"));
            return;
        }

        var fmgs = Link.Fmgs;
        GUI.WrappedText(LOC.Get("TEXT_FmgReloader_Game_Fmgs_Count", fmgs.Count, fmgs.Count(f => f.Replaced)));

        if (!ShowGameFmgs || fmgs.Count == 0)
            return;

        var byId = EditorFmgsById();

        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##fmgReloaderFilter", LOC.Get("TEXT_FmgReloader_Filter_Hint"), ref Filter, 128);
        GUI.Tooltip(LOC.Get("TEXT_FmgReloader_Filter_TT"));

        var filter = Filter.Trim();
        var selected = View.Selection.SelectedFmgWrapper;

        var flags = ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.ScrollY | ImGuiTableFlags.SizingStretchProp;

        if (!ImGui.BeginTable("fmgReloaderGameFmgs", 4, flags, new Vector2(-1, -1)))
            return;

        ImGui.TableSetupScrollFreeze(0, 1);
        ImGui.TableSetupColumn(LOC.Get("TEXT_FmgReloader_Column_Id"), ImGuiTableColumnFlags.WidthFixed, 40);
        ImGui.TableSetupColumn(LOC.Get("TEXT_FmgReloader_Column_Fmg"), ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableSetupColumn(LOC.Get("TEXT_FmgReloader_Column_Strings"), ImGuiTableColumnFlags.WidthFixed, 60);
        ImGui.TableSetupColumn(LOC.Get("TEXT_FmgReloader_Column_State"), ImGuiTableColumnFlags.WidthFixed, 60);
        ImGui.TableHeadersRow();

        foreach (var game in fmgs)
        {
            byId.TryGetValue(game.Id, out var editor);
            var name = editor.Fmg != null ? TextUtils.GetFmgDisplayName(Project, editor.Container, editor.Fmg.ID, editor.Fmg.Name) : "";
            var label = editor.Fmg != null ? $"{editor.Container.FileEntry.Filename} / {name}" : LOC.Get("TEXT_FmgReloader_Not_In_Project");

            if (filter.Length > 0 && !label.Contains(filter, StringComparison.OrdinalIgnoreCase) && !$"{game.Id}".Contains(filter))
                continue;

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            ImGui.TextUnformatted($"{game.Id}");

            ImGui.TableSetColumnIndex(1);
            var isSelected = editor.Fmg != null && editor.Fmg == selected;
            if (ImGui.Selectable($"{label}##fmgReloaderGameFmg_{game.Id}", isSelected, ImGuiSelectableFlags.SpanAllColumns) && editor.Fmg != null)
                SelectInEditor(editor.Container, editor.Fmg);

            if (editor.Fmg != null && editor.Fmg.IsDirty)
                GUI.Tooltip(LOC.Get("TEXT_FmgReloader_Row_Modified_TT"));

            ImGui.TableSetColumnIndex(2);
            ImGui.TextUnformatted($"{game.StringCount}");

            ImGui.TableSetColumnIndex(3);
            if (game.Replaced)
                ImGui.TextColored(UI.Current.ImGui_Benefit_Text_Color, LOC.Get("TEXT_FmgReloader_State_Replaced"));
            else if (editor.Fmg != null && editor.Fmg.IsDirty)
                ImGui.TextColored(UI.Current.ImGui_Invalid_Text_Color, LOC.Get("TEXT_FmgReloader_State_Modified"));
        }

        ImGui.EndTable();
    }

    /// <summary>
    /// The editor's FMG for each binder entry ID, from the containers the game reads. Their FMGs are loaded once per
    /// language so the list can name them.
    /// </summary>
    private Dictionary<int, (TextContainerWrapper Container, TextFmgWrapper Fmg)> EditorFmgsById()
    {
        var map = new Dictionary<int, (TextContainerWrapper, TextFmgWrapper)>();
        var resident = FmgReloadSource.Resident(Bank, Link.Language).ToList();

        if (LoadedNamesFor != Link.Language)
        {
            foreach (var container in resident.Where(c => c.FmgWrappers == null || c.FmgWrappers.Count == 0))
                Bank.LoadFmgWrappers(container);

            LoadedNamesFor = Link.Language;
        }

        foreach (var container in resident)
        {
            if (container.FmgWrappers == null)
                continue;

            foreach (var fmg in container.FmgWrappers)
                map.TryAdd(fmg.ID, (container, fmg));
        }

        return map;
    }

    private void SelectInEditor(TextContainerWrapper container, TextFmgWrapper fmg)
    {
        if (View.Selection.SelectedContainerWrapper != container)
            View.Selection.SelectFileContainer(container.FileEntry, container, -1);

        View.Selection.SelectFmg(fmg);
    }
}
