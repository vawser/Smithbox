using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Memory;
using System.Numerics;

namespace StudioCore.Editors.FxEditor;

/// <summary>
/// FX Reloader (DS3, SDT, ER, AC6, NR): reloads the selected effect in the running game from the editor's copy, lists the effects the game
/// has loaded with how many of each are playing (a click selects it here), and keeps a history of sends.
/// See Claude/Documentation/FX Reloader Analysis.md (F1).
/// </summary>
public class FxReloaderTool
{
    private readonly FxEditorView Editor;

    private FxReloader Reloader => Editor.Editor.Reloader;
    private GameLink Link => Reloader.Link;
    private FxBank Bank => Editor.Data.Bank;

    private const float MinRefreshSeconds = 0.5f, MaxRefreshSeconds = 30f;

    /// <summary>How often the game is read while the tool is shown (the setting, kept in range)</summary>
    private static float RefreshSeconds =>
        Math.Clamp(CFG.Current.FxEditor.FxEditor_InGame_RefreshSeconds, MinRefreshSeconds, MaxRefreshSeconds);

    private bool ShowHistory = true;
    private string Filter = "";

    // Rows are rebuilt only when what they come from changes
    private object RowsKey;
    private List<FxReloader.LoadedEffect> Rows = new();

    public FxReloaderTool(FxEditorView editor)
    {
        Editor = editor;
    }

    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("FX_Reloader_Header")}##fxReloaderToolHeader"))
            return;

        ImGui.BeginChild("FxReloaderToolSection", ImGuiChildFlags.Borders);
        Display();
        ImGui.EndChild();
    }

    private void Display()
    {
        // Games with FX Reloader offsets (Assets/FX Reloader)
        if (!Reloader.IsSupported)
        {
            GUI.WrappedText(LOC.Get("FX_Reloader_Game_Not_Supported"));
            return;
        }

        GUI.WrappedText(LOC.Get("FX_Reloader_Hint"));

        // Polls only while the tool is shown
        if (CFG.Current.FxEditor.FxEditor_InGame_AutoRefresh &&
            (Reloader.LastPoll == null || (DateTime.Now - Reloader.LastPoll.Value).TotalSeconds >= RefreshSeconds))
        {
            Reloader.RequestPoll();
        }

        DisplayConnection();
        DisplayReload();
        DisplayWorld();
        DisplayHistory();
        DisplayRecent();
        DisplayLoaded();
    }

    // Game
    private void DisplayConnection()
    {
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("FX_Reloader_Game_Header"), LOC.Get("FX_Reloader_Game_Header_TT"));

        var message = Reloader.StateMessage;
        if (string.IsNullOrEmpty(message))
            message = LOC.Get("GAMELINK_State_Waiting");

        if (Reloader.State is GameLink.LinkState.Failed)
            GUI.WrappedTextColored(UI.Current.ImGui_Invalid_Text_Color, message);
        else
            GUI.WrappedText(message);

        ImGui.Checkbox($"{LOC.Get("FX_Reloader_Auto_Refresh")}##fxReloaderAutoRefresh", ref CFG.Current.FxEditor.FxEditor_InGame_AutoRefresh);
        GUI.Tooltip(LOC.Get("FX_Reloader_Auto_Refresh_TT", RefreshSeconds));

        // A slider, so a value applies as it's dragged; Ctrl+click types one, clamped to the range
        ImGui.SameLine();
        var seconds = RefreshSeconds;
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
        if (ImGui.SliderFloat("##fxReloaderRefreshSeconds", ref seconds, MinRefreshSeconds, MaxRefreshSeconds, LOC.Get("FX_Reloader_Refresh_Seconds_Format"), ImGuiSliderFlags.AlwaysClamp | ImGuiSliderFlags.Logarithmic))
            CFG.Current.FxEditor.FxEditor_InGame_RefreshSeconds = MathF.Round(Math.Clamp(seconds, MinRefreshSeconds, MaxRefreshSeconds), 1);
        GUI.Tooltip(LOC.Get("FX_Reloader_Refresh_Seconds_TT", MinRefreshSeconds, MaxRefreshSeconds, Reloader.LastPollMs));

        GUI.MultiButtonInput("fxReloaderRefreshActions",
            "fxReloaderRefresh", LOC.Get("FX_Reloader_Refresh"), LOC.Get("FX_Reloader_Refresh_TT"),
            Reloader.RequestPoll,
            "fxReloaderDetach", LOC.Get("FX_Reloader_Detach"), LOC.Get("FX_Reloader_Detach_TT"),
            () =>
            {
                CFG.Current.FxEditor.FxEditor_InGame_AutoRefresh = false;
                Reloader.Detach();
            });
    }

    private FxReloader.AfterReload After => (FxReloader.AfterReload)CFG.Current.FxEditor.FxEditor_InGame_AfterReload;

    // Reload
    private void DisplayReload()
    {
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("FX_Reloader_Reload_Header"), LOC.Get("FX_Reloader_Reload_Header_TT"));

        // Afterwards: only the actions this game offers. The setting is shared by every game, so a choice this game
        // doesn't have is shown as "Nothing" without being changed, and the reloader skips it.
        CFG.Current.FxEditor.FxEditor_InGame_AfterReload = Math.Clamp(CFG.Current.FxEditor.FxEditor_InGame_AfterReload, 0, 2);

        if (Link.OffersWorldActions)
        {
            var choices = new List<(FxReloader.AfterReload Value, string Label)> { (FxReloader.AfterReload.Nothing, LOC.Get("FX_Reloader_After_Nothing")) };

            if (Link.OffersWorldReload)
                choices.Add((FxReloader.AfterReload.ReloadWorld, LOC.Get(Link.WorldReloadSavesFirst ? "FX_Reloader_After_Reload_World" : "FX_Reloader_After_Load_Save")));
            if (Link.OffersQuitOut)
                choices.Add((FxReloader.AfterReload.QuitOut, LOC.Get("FX_Reloader_After_Quit_Out")));

            var labels = choices.Select(c => c.Label).ToArray();
            var index = Math.Max(0, choices.FindIndex(c => (int)c.Value == CFG.Current.FxEditor.FxEditor_InGame_AfterReload));

            ImGui.SetNextItemWidth(-1);
            if (ImGui.Combo("##fxReloaderAfterReload", ref index, labels, labels.Length))
                CFG.Current.FxEditor.FxEditor_InGame_AfterReload = (int)choices[index].Value;
            GUI.Tooltip(LOC.Get("FX_Reloader_After_TT"));
        }

        // Selected
        var entry = Editor.Selection.Entry;

        if (entry == null)
        {
            GUI.WrappedText(LOC.Get("FX_Reloader_No_Selection"));
        }
        else
        {
            GUI.WrappedText(LOC.Get("FX_Reloader_Selected", NameOf(entry.Id)));

            if (Reloader.IsReady)
            {
                var loaded = Reloader.Loaded.FirstOrDefault(e => e.Id == entry.Id);
                GUI.WrappedText(loaded == null
                    ? LOC.Get("FX_Reloader_Selected_Not_Loaded")
                    : LOC.Get("FX_Reloader_Selected_Loaded", Math.Max(0, loaded.Playing)));
            }

            if (Reloader.IsReloading)
            {
                GUI.WrappedText(LOC.Get("FX_Reloader_Reload_Running"));
            }
            else if (entry.Fxr == null)
            {
                GUI.WrappedText(LOC.Get("FX_Reloader_Selected_Not_Fxr"));
            }
            else if (entry.Vanilla is FxVanillaState.Modified)
            {
                GUI.MultiButtonInput("fxReloaderReloadActions",
                    "fxReloaderReload", LOC.Get("FX_Reloader_Reload"), LOC.Get("FX_Reloader_Reload_TT"),
                    () => SendEditor(entry, FxReloader.SendSource.Selected),
                    "fxReloaderSendVanilla", LOC.Get("FX_Reloader_Send_Vanilla"), LOC.Get("FX_Reloader_Send_Vanilla_TT"),
                    () => SendVanilla(entry));
            }
            else
            {
                GUI.MultiButtonInput("fxReloaderReloadActions",
                    "fxReloaderReload", LOC.Get("FX_Reloader_Reload"), LOC.Get("FX_Reloader_Reload_TT"),
                    () => SendEditor(entry, FxReloader.SendSource.Selected));
            }
        }

        // Modified
        var dirty = DirtyEntries().Count();
        GUI.WrappedText(LOC.Get("FX_Reloader_Modified_Count", dirty));

        if (dirty > 0 && !Reloader.IsReloading)
        {
            GUI.MultiButtonInput("fxReloaderModifiedActions",
                "fxReloaderReloadModified", LOC.Get("FX_Reloader_Modified_Reload"), LOC.Get("FX_Reloader_Modified_Reload_TT"),
                SendModified);
        }

        // Restart: weapon and SpEffect effects that are playing, so they show a reload
        if (Reloader.Restarter.IsAvailable)
        {
            ImGui.Checkbox($"{LOC.Get("FX_Reloader_Restart_After")}##fxReloaderRestartAfter", ref CFG.Current.FxEditor.FxEditor_InGame_RestartAfterReload);
            GUI.Tooltip(LOC.Get("FX_Reloader_Restart_After_TT"));

            if (entry != null && !Reloader.IsReloading)
            {
                GUI.MultiButtonInput("fxReloaderRestartActions",
                    "fxReloaderRestart", LOC.Get("FX_Reloader_Restart"), LOC.Get("FX_Reloader_Restart_TT"),
                    () => Reloader.RequestRestart(entry.Id));
            }
        }
        else
        {
            GUI.WrappedText(LOC.Get("FX_Reloader_Restart_Unavailable"));
        }

        // Last result, and how the latest reloaded effect's instances move from the old version to the new
        var last = Reloader.LastReload;
        if (last != null)
        {
            var text = $"{last.At:T}  {last.Message}";

            if (last.Outcome is FxReloader.ReloadOutcome.Reloaded)
                GUI.WrappedText(text);
            else
                GUI.WrappedTextColored(UI.Current.ImGui_Invalid_Text_Color, text);
        }

        var watch = Reloader.Watch;
        if (watch != null)
        {
            if (watch.State is FxReloader.WatchState.InUse)
            {
                GUI.WrappedText(LOC.Get("FX_Reloader_Watch", NameOf(watch.Id), Math.Max(0, watch.Playing)));
                GUI.Tooltip(LOC.Get("FX_Reloader_Watch_TT", watch.PlayingAtSend));
            }

            if (watch.State is not FxReloader.WatchState.InUse)
                GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get($"FX_Reloader_Watch_{watch.State}", NameOf(watch.Id)));
        }

        GUI.WrappedText(LOC.Get("FX_Reloader_Reload_Note"));
    }

    /// <summary>The selected effect as the editor holds it, unsaved edits included</summary>
    private void SendEditor(FxEntry entry, FxReloader.SendSource source)
    {
        // A value still being typed is committed first, so the send holds what the Inspector shows
        Editor.FlushEdits();

        if (entry.Fxr == null)
            return;

        Reloader.RequestReloadMany(new[] { new FxReloader.SendItem(entry.Id, entry.Fxr.Write()) }, After, source);
    }

    /// <summary>The game's own copy of the selected effect</summary>
    private void SendVanilla(FxEntry entry)
    {
        var vanilla = Bank.ReadVanilla(entry);
        if (vanilla == null)
        {
            Smithbox.LogWarning(this, LOC.Get("FX_Reloader_Send_Vanilla_None", NameOf(entry.Id)));
            return;
        }

        Reloader.RequestReloadMany(new[] { new FxReloader.SendItem(entry.Id, vanilla.Write()) }, After, FxReloader.SendSource.Vanilla);
    }

    private IEnumerable<FxEntry> DirtyEntries() => Bank?.AllEntries.Where(e => e.IsLoaded && e.IsDirty && e.Fxr != null) ?? Enumerable.Empty<FxEntry>();

    private void SendModified()
    {
        Editor.FlushEdits();

        var items = DirtyEntries().Select(e => new FxReloader.SendItem(e.Id, e.Fxr.Write())).ToList();
        if (items.Count > 0)
            Reloader.RequestReloadMany(items, After, FxReloader.SendSource.Modified);
    }

    // World
    private void DisplayWorld()
    {
        if (!Link.OffersWorldActions)
            return;

        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("GAMELINK_World_Header"), LOC.Get("FX_Reloader_World_Header_TT"));

        // ER and NR save first; AC6 loads the last save as it is
        var savesFirst = Link.WorldReloadSavesFirst;
        var reloadTip = LOC.Get(savesFirst ? "GAMELINK_World_Reload_TT" : "GAMELINK_World_Reload_No_Save_TT");

        if (Link.OffersWorldReload && !savesFirst)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("GAMELINK_World_No_Save_Warning"));

        if (Link.IsReloadingWorld)
        {
            GUI.WrappedText(LOC.Get("GAMELINK_World_Running"));
        }
        else if (Link.IsQuittingOut)
        {
            GUI.WrappedText(LOC.Get("GAMELINK_QuitOut_Running"));
        }
        else if (Link.OffersWorldReload && Link.OffersQuitOut)
        {
            GUI.MultiButtonInput("fxReloaderWorldActions",
                "fxReloaderReloadWorld", LOC.Get("GAMELINK_World_Reload"), reloadTip,
                Link.RequestReloadWorld,
                "fxReloaderQuitOut", LOC.Get("GAMELINK_QuitOut"), LOC.Get("GAMELINK_QuitOut_TT"),
                Link.RequestQuitOut);
        }
        else if (Link.OffersWorldReload)
        {
            GUI.MultiButtonInput("fxReloaderWorldActions",
                "fxReloaderReloadWorld", LOC.Get("GAMELINK_World_Reload"), reloadTip,
                Link.RequestReloadWorld);
        }
        else
        {
            GUI.MultiButtonInput("fxReloaderWorldActions",
                "fxReloaderQuitOut", LOC.Get("GAMELINK_QuitOut"), LOC.Get("GAMELINK_QuitOut_TT"),
                Link.RequestQuitOut);
        }

        var world = Link.LastWorldReload;
        if (world != null)
            GUI.WrappedText(world);

        var quit = Link.LastQuitOut;
        if (quit != null)
            GUI.WrappedText(quit);
    }

    // History
    private void DisplayHistory()
    {
        GUI.Spacer();
        GUI.ConditionalHeader(LOC.Get("FX_Reloader_History_Header"), LOC.Get("FX_Reloader_History_Header_TT"), ref ShowHistory);

        var history = Reloader.History;

        if (history.Count == 0)
        {
            GUI.WrappedText(LOC.Get("FX_Reloader_History_Empty"));
            return;
        }

        if (!ShowHistory)
            return;

        var flags = ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.SizingStretchProp;
        var height = Math.Min(history.Count, 8) * ImGui.GetFrameHeightWithSpacing() + ImGui.GetFrameHeightWithSpacing();

        if (ImGui.BeginTable("fxReloaderHistory", 4, flags | ImGuiTableFlags.ScrollY, new Vector2(-1, height)))
        {
            ImGui.TableSetupScrollFreeze(0, 1);
            ImGui.TableSetupColumn(LOC.Get("FX_Reloader_Column_Effect"), ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn(LOC.Get("FX_Reloader_History_Column_Sent"), ImGuiTableColumnFlags.WidthFixed, 110 * DPI.UIScale());
            ImGui.TableSetupColumn("##send", ImGuiTableColumnFlags.WidthFixed, 45 * DPI.UIScale());
            ImGui.TableSetupColumn("##vanilla", ImGuiTableColumnFlags.WidthFixed, 60 * DPI.UIScale());
            ImGui.TableHeadersRow();

            foreach (var item in history)
            {
                Bank.Entries.TryGetValue(item.Id, out var entry);

                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                var failed = item.Outcome is not FxReloader.ReloadOutcome.Reloaded;
                if (failed)
                    ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Invalid_Text_Color);

                if (ImGui.Selectable($"{NameOf(item.Id)}##fxReloaderHistory_{item.Id}") && entry != null)
                    Editor.JumpTo(item.Id);

                if (failed)
                    ImGui.PopStyleColor();

                var source = LOC.Get($"FX_Reloader_Source_{item.Source}");
                GUI.Tooltip(LOC.Get("FX_Reloader_History_Row_TT", source, item.Outcome));

                ImGui.TableSetColumnIndex(1);
                ImGui.TextUnformatted($"{item.At:T} {source}");

                // Sending needs the effect loaded in the editor
                if (entry?.Fxr == null || Reloader.IsReloading)
                    continue;

                ImGui.TableSetColumnIndex(2);
                if (ImGui.SmallButton($"{LOC.Get("FX_Reloader_History_Send")}##fxReloaderHistorySend_{item.Id}"))
                    SendEditor(entry, FxReloader.SendSource.History);
                GUI.Tooltip(LOC.Get("FX_Reloader_History_Send_TT"));

                if (entry.Vanilla is not FxVanillaState.Modified)
                    continue;

                ImGui.TableSetColumnIndex(3);
                if (ImGui.SmallButton($"{LOC.Get("FX_Reloader_History_Vanilla")}##fxReloaderHistoryVanilla_{item.Id}"))
                    SendVanilla(entry);
                GUI.Tooltip(LOC.Get("FX_Reloader_Send_Vanilla_TT"));
            }

            ImGui.EndTable();
        }

        GUI.MultiButtonInput("fxReloaderHistoryActions",
            "fxReloaderHistoryClear", LOC.Get("FX_Reloader_History_Clear"), LOC.Get("FX_Reloader_History_Clear_TT"),
            Reloader.ClearHistory);
    }

    // Recently detected: effects that loaded or started playing since connecting, newest first
    private const int RecentLimit = 200;

    private object RecentKey;
    private List<FxReloader.LoadedEffect> RecentRows = new();

    private void DisplayRecent()
    {
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("FX_Reloader_Recent_Header"), LOC.Get("FX_Reloader_Recent_Header_TT", RefreshSeconds));

        if (!Reloader.IsReady)
        {
            GUI.WrappedText(LOC.Get("FX_Reloader_Loaded_Not_Attached"));
            return;
        }

        var loaded = Reloader.Loaded;
        if (!ReferenceEquals(loaded, RecentKey))
        {
            RecentKey = loaded;
            RecentRows = loaded.Where(e => !e.Initial)
                .OrderByDescending(e => e.DetectedAt).ThenBy(e => e.Id)
                .Take(RecentLimit).ToList();
        }

        if (RecentRows.Count == 0)
        {
            GUI.WrappedText(LOC.Get("FX_Reloader_Recent_Empty"));
            return;
        }

        var flags = ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.ScrollY | ImGuiTableFlags.SizingStretchProp;
        var height = (Math.Min(RecentRows.Count, 10) + 1) * ImGui.GetFrameHeightWithSpacing();

        if (!ImGui.BeginTable("fxReloaderRecentTable", 3, flags, new Vector2(-1, height)))
            return;

        ImGui.TableSetupScrollFreeze(0, 1);
        ImGui.TableSetupColumn(LOC.Get("FX_Reloader_Column_Effect"), ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableSetupColumn(LOC.Get("FX_Reloader_Column_Detected"), ImGuiTableColumnFlags.WidthFixed, 110 * DPI.UIScale());
        ImGui.TableSetupColumn(LOC.Get("FX_Reloader_Column_Playing"), ImGuiTableColumnFlags.WidthFixed, 60 * DPI.UIScale());
        ImGui.TableHeadersRow();

        var now = DateTime.Now;
        var selected = Editor.Selection.Entry?.Id;

        foreach (var row in RecentRows)
        {
            var inProject = Bank.Entries.ContainsKey(row.Id);

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            if (!inProject)
                ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Invalid_Text_Color);

            if (ImGui.Selectable($"{NameOf(row.Id)}##fxReloaderRecent_{row.Id}", row.Id == selected, ImGuiSelectableFlags.SpanAllColumns) && inProject)
                Editor.JumpTo(row.Id);

            if (!inProject)
            {
                ImGui.PopStyleColor();
                GUI.Tooltip(LOC.Get("FX_Reloader_Loaded_Not_In_Project_TT"));
            }

            ImGui.TableSetColumnIndex(1);
            ImGui.TextUnformatted(LOC.Get($"FX_Reloader_Recent_{row.Reason}", Ago(now - row.DetectedAt)));

            ImGui.TableSetColumnIndex(2);
            if (row.Playing > 0)
                ImGui.TextUnformatted($"{row.Playing}");
        }

        ImGui.EndTable();
    }

    private static string Ago(TimeSpan span) => span.TotalSeconds < 60 ? $"{(int)span.TotalSeconds} s" : $"{(int)span.TotalMinutes} min";

    // Loaded
    private void DisplayLoaded()
    {
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("FX_Reloader_Loaded_Header"), LOC.Get("FX_Reloader_Loaded_Header_TT"));

        if (!Reloader.IsReady)
        {
            GUI.WrappedText(LOC.Get("FX_Reloader_Loaded_Not_Attached"));
            return;
        }

        var loaded = Reloader.Loaded;
        GUI.WrappedText(LOC.Get("FX_Reloader_Loaded_Count", loaded.Count, loaded.Count(e => e.Playing > 0)));

        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X * 0.6f);
        ImGui.InputTextWithHint("##fxReloaderFilter", LOC.Get("FX_Reloader_Filter_Hint"), ref Filter, 128);
        GUI.Tooltip(LOC.Get("FX_Reloader_Filter_TT"));

        ImGui.SameLine();
        ImGui.Checkbox($"{LOC.Get("FX_Reloader_Playing_First")}##fxReloaderPlayingFirst", ref CFG.Current.FxEditor.FxEditor_InGame_PlayingFirst);
        GUI.Tooltip(LOC.Get("FX_Reloader_Playing_First_TT"));

        var rows = BuildRows(loaded);
        var selected = Editor.Selection.Entry?.Id;

        var flags = ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.ScrollY | ImGuiTableFlags.SizingStretchProp;

        if (!ImGui.BeginTable("fxReloaderLoadedTable", 2, flags, new Vector2(-1, -1)))
            return;

        ImGui.TableSetupScrollFreeze(0, 1);
        ImGui.TableSetupColumn(LOC.Get("FX_Reloader_Column_Effect"), ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableSetupColumn(LOC.Get("FX_Reloader_Column_Playing"), ImGuiTableColumnFlags.WidthFixed, 60 * DPI.UIScale());
        ImGui.TableHeadersRow();

        var clipper = new ImGuiListClipper();
        clipper.Begin(rows.Count);

        while (clipper.Step())
        {
            for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var row = rows[i];
                var inProject = Bank.Entries.ContainsKey(row.Id);

                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                if (!inProject)
                    ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Invalid_Text_Color);

                if (ImGui.Selectable($"{NameOf(row.Id)}##fxReloaderLoaded_{row.Id}", row.Id == selected, ImGuiSelectableFlags.SpanAllColumns) && inProject)
                    Editor.JumpTo(row.Id);

                if (!inProject)
                {
                    ImGui.PopStyleColor();
                    GUI.Tooltip(LOC.Get("FX_Reloader_Loaded_Not_In_Project_TT"));
                }

                ImGui.TableSetColumnIndex(1);
                if (row.Playing > 0)
                    ImGui.TextUnformatted($"{row.Playing}");
            }
        }

        clipper.End();
        ImGui.EndTable();
    }

    private List<FxReloader.LoadedEffect> BuildRows(IReadOnlyList<FxReloader.LoadedEffect> loaded)
    {
        var filter = Filter.Trim();
        var key = (loaded, filter, CFG.Current.FxEditor.FxEditor_InGame_PlayingFirst, Bank);

        if (Equals(key, RowsKey))
            return Rows;

        IEnumerable<FxReloader.LoadedEffect> rows = loaded;

        if (filter.Length > 0)
            rows = rows.Where(e => NameOf(e.Id).Contains(filter, StringComparison.OrdinalIgnoreCase));

        if (CFG.Current.FxEditor.FxEditor_InGame_PlayingFirst)
            rows = rows.OrderByDescending(e => e.Playing > 0).ThenBy(e => e.Id);

        RowsKey = key;
        Rows = rows.ToList();
        return Rows;
    }

    /// <summary>The effect's id with its alias, as the Effect List shows it</summary>
    private string NameOf(int id)
    {
        var alias = Bank != null && Bank.Entries.TryGetValue(id, out var entry) ? entry.Alias : "";
        return string.IsNullOrEmpty(alias) ? $"{id}" : $"{id}  {alias}";
    }
}
