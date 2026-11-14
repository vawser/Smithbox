using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MetadataEditor;
using System.Numerics;

namespace StudioCore.Editors.MaterialEditor;

/// <summary>
/// Material Reloader (ER): lists the running game's loaded models and the materials they use, refreshed every few
/// seconds, newest first if asked, with a click selecting the material here; reloads the selected material in the
/// game from the editor's copy, then optionally reloads the world. See Claude/Documentation/Material Reloader UX Analysis.md.
/// </summary>
public class MatInGameTool
{
    public MaterialEditorView View;
    public ProjectEntry Project;

    // Shared by every view, and by Save for "send on save"
    private MatGameLink Link => View.Editor.GameLink;

    private bool ShowHistory = true;

    private const double RefreshSeconds = 3;

    private string Filter = "";

    public MatInGameTool(MaterialEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    private MaterialBank Bank => Project.Handler.MaterialData.PrimaryBank;

    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("MAT_InGameTool_Header")}##materialInGameToolHeader"))
            return;

        ImGui.BeginChild("MaterialInGameToolSection", ImGuiChildFlags.Borders);

        Display();

        ImGui.EndChild();
    }

    private void Display()
    {
        if (Project.Descriptor.ProjectType is not ProjectType.ER)
        {
            GUI.WrappedText(LOC.Get("MAT_InGameTool_Game_Not_Supported"));
            return;
        }

        GUI.WrappedText(LOC.Get("MAT_InGameTool_Hint"));

        // Send Vanilla needs the game's own materials; they load once, in the background
        View.Editor.Vanilla.EnsureLoading();

        // Polls only while the tool is shown
        if (CFG.Current.MaterialEditor_InGame_AutoRefresh &&
            (Link.LastPoll == null || (DateTime.Now - Link.LastPoll.Value).TotalSeconds >= RefreshSeconds))
        {
            Link.RequestPoll();
        }

        DisplayConnection();
        DisplayReload();
        DisplayBrowse();
    }

    // Game
    private void DisplayConnection()
    {
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAT_InGameTool_Game_Header"), LOC.Get("MAT_InGameTool_Game_Header_TT"));

        var message = Link.StateMessage;
        if (string.IsNullOrEmpty(message))
            message = LOC.Get("MAT_InGameTool_State_Waiting");

        if (Link.State is MatGameLink.LinkState.Failed)
            GUI.WrappedTextColored(UI.Current.ImGui_Invalid_Text_Color, message);
        else
            GUI.WrappedText(message);

        ImGui.Checkbox($"{LOC.Get("MAT_InGameTool_Auto_Refresh")}##materialInGameAutoRefresh", ref CFG.Current.MaterialEditor_InGame_AutoRefresh);
        GUI.Tooltip(LOC.Get("MAT_InGameTool_Auto_Refresh_TT", RefreshSeconds));

        GUI.MultiButtonInput("materialInGameRefreshActions",
            "materialInGameRefresh", LOC.Get("MAT_InGameTool_Refresh"), LOC.Get("MAT_InGameTool_Refresh_TT"),
            Link.RequestPoll,
            "materialInGameDetach", LOC.Get("MAT_InGameTool_Detach"), LOC.Get("MAT_InGameTool_Detach_TT"),
            () =>
            {
                CFG.Current.MaterialEditor_InGame_AutoRefresh = false;
                Link.Detach();
            });
    }

    // Reload
    private void DisplayReload()
    {
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAT_InGameTool_Reload_Header"), LOC.Get("MAT_InGameTool_Reload_Header_TT"));

        // Afterwards: applies to every send below, and to send on save
        var afterLabels = new[]
        {
            LOC.Get("MAT_InGameTool_After_Nothing"),
            LOC.Get("MAT_InGameTool_After_Reload_World"),
            LOC.Get("MAT_InGameTool_After_Quit_Out")
        };
        CFG.Current.MaterialEditor_InGame_AfterReload = Math.Clamp(CFG.Current.MaterialEditor_InGame_AfterReload, 0, afterLabels.Length - 1);

        ImGui.SetNextItemWidth(-1);
        ImGui.Combo("##materialInGameAfterReload", ref CFG.Current.MaterialEditor_InGame_AfterReload, afterLabels, afterLabels.Length);
        GUI.Tooltip(LOC.Get("MAT_InGameTool_After_TT"));

        // Selected
        var file = View.Selection.SelectedFileWrapper as MATBINFileWrapper;

        if (file == null)
        {
            GUI.WrappedText(LOC.Get("MAT_InGameTool_Reload_No_Selection"));
        }
        else
        {
            var gameName = MatGameLink.GameNameOf(file.Key);
            GUI.WrappedText(LOC.Get("MAT_InGameTool_Reload_Selected", gameName));

            if (Link.State is MatGameLink.LinkState.Attached)
                DisplaySelectedInGame(gameName);

            if (Link.IsReloading)
            {
                GUI.WrappedText(LOC.Get("MAT_InGameTool_Reload_Running"));
            }
            else
            {
                GUI.MultiButtonInput("materialInGameReloadActions",
                    "materialInGameReload", LOC.Get("MAT_InGameTool_Reload"), LOC.Get("MAT_InGameTool_Reload_TT"),
                    () => Send(MatReloadSource.FromEditor(Bank, View.Selection.SelectedBinder, file, View.Editor), MatGameLink.SendSource.Selected),
                    "materialInGameSendVanilla", LOC.Get("MAT_InGameTool_Send_Vanilla"), LOC.Get("MAT_InGameTool_Send_Vanilla_TT"),
                    () => Send(MatReloadSource.FromVanilla(View.Selection.SelectedBinder, file, View.Editor), MatGameLink.SendSource.Vanilla));
            }
        }

        // Modified
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAT_InGameTool_Modified_Header"), LOC.Get("MAT_InGameTool_Modified_Header_TT"));

        var dirty = MatReloadSource.CountDirty(Bank);
        GUI.WrappedText(LOC.Get("MAT_InGameTool_Modified_Count", dirty));

        if (dirty > 0 && !Link.IsReloading)
        {
            GUI.MultiButtonInput("materialInGameModifiedActions",
                "materialInGameReloadModified", LOC.Get("MAT_InGameTool_Modified_Reload"), LOC.Get("MAT_InGameTool_Modified_Reload_TT"),
                () =>
                {
                    var items = MatReloadSource.Dirty(Bank, View.Editor);
                    if (items.Count > 0)
                        Link.RequestReloadMany(items, (MatGameLink.AfterReload)CFG.Current.MaterialEditor_InGame_AfterReload, MatGameLink.SendSource.Modified);
                });
        }

        ImGui.Checkbox($"{LOC.Get("MAT_InGameTool_Send_On_Save")}##materialInGameSendOnSave", ref CFG.Current.MaterialEditor_InGame_SendOnSave);
        GUI.Tooltip(LOC.Get("MAT_InGameTool_Send_On_Save_TT"));

        var last = Link.LastReload;
        if (last != null)
        {
            var text = $"{last.At:T}  {last.Message}";

            if (last.Outcome is MatGameLink.ReloadOutcome.Reloaded)
                GUI.WrappedText(text);
            else
                GUI.WrappedTextColored(UI.Current.ImGui_Invalid_Text_Color, text);
        }

        GUI.WrappedText(LOC.Get("MAT_InGameTool_Reload_Note"));

        // World
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAT_InGameTool_World_Header"), LOC.Get("MAT_InGameTool_World_Header_TT"));

        if (Link.IsReloadingWorld)
        {
            GUI.WrappedText(LOC.Get("MAT_InGameTool_World_Running"));
        }
        else if (Link.IsQuittingOut)
        {
            GUI.WrappedText(LOC.Get("MAT_InGameTool_QuitOut_Running"));
        }
        else
        {
            GUI.MultiButtonInput("materialInGameWorldActions",
                "materialInGameReloadWorld", LOC.Get("MAT_InGameTool_World_Reload"), LOC.Get("MAT_InGameTool_World_Reload_TT"),
                Link.RequestReloadWorld,
                "materialInGameQuitOut", LOC.Get("MAT_InGameTool_QuitOut"), LOC.Get("MAT_InGameTool_QuitOut_TT"),
                Link.RequestQuitOut);
        }

        var world = Link.LastWorldReload;
        if (world != null)
            GUI.WrappedText(world);

        var quit = Link.LastQuitOut;
        if (quit != null)
            GUI.WrappedText(quit);

        DisplayHistory();
    }

    /// <summary>
    /// Sends one prepared material with the chosen "after" action; a null item was refused and already logged
    /// </summary>
    private void Send(MatGameLink.SendItem item, MatGameLink.SendSource source)
    {
        if (item != null)
            Link.RequestReload(item.Name, item.Bytes, (MatGameLink.AfterReload)CFG.Current.MaterialEditor_InGame_AfterReload, source);
    }

    // History
    private void DisplayHistory()
    {
        GUI.Spacer();
        GUI.ConditionalHeader(LOC.Get("MAT_InGameTool_History_Header"), LOC.Get("MAT_InGameTool_History_Header_TT"), ref ShowHistory);

        var history = Link.History;

        if (history.Count == 0)
        {
            GUI.WrappedText(LOC.Get("MAT_InGameTool_History_Empty"));
            return;
        }

        if (!ShowHistory)
            return;

        var flags = ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.SizingStretchProp;
        var height = Math.Min(history.Count, 8) * ImGui.GetFrameHeightWithSpacing() + ImGui.GetFrameHeightWithSpacing();

        if (ImGui.BeginTable("materialInGameHistory", 4, flags | ImGuiTableFlags.ScrollY, new Vector2(-1, height)))
        {
            ImGui.TableSetupScrollFreeze(0, 1);
            ImGui.TableSetupColumn(LOC.Get("MAT_InGameTool_Column_Name"), ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn(LOC.Get("MAT_InGameTool_History_Column_Sent"), ImGuiTableColumnFlags.WidthFixed, 110);
            ImGui.TableSetupColumn("##send", ImGuiTableColumnFlags.WidthFixed, 45);
            ImGui.TableSetupColumn("##vanilla", ImGuiTableColumnFlags.WidthFixed, 60);
            ImGui.TableHeadersRow();

            foreach (var entry in history)
            {
                var (binder, file) = MatReloadSource.Find(Bank, entry.Name);

                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                var failed = entry.Outcome is not MatGameLink.ReloadOutcome.Reloaded;
                if (failed)
                    ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Invalid_Text_Color);

                // Selects the material, like the Browse rows
                if (ImGui.Selectable($"{entry.Name}##materialInGameHistory_{entry.Name}") && file != null)
                {
                    var reference = Bank.FindReference(MaterialSourceType.MATBIN, entry.Name);
                    if (reference != null)
                        View.Selection.JumpTo(Bank, reference);
                }

                if (failed)
                    ImGui.PopStyleColor();

                GUI.Tooltip(LOC.Get("MAT_InGameTool_History_Row_TT", LOC.Get($"MAT_InGameTool_Source_{entry.Source}"), entry.Outcome));

                ImGui.TableSetColumnIndex(1);
                ImGui.TextUnformatted($"{entry.At:T} {LOC.Get($"MAT_InGameTool_Source_{entry.Source}")}");

                if (file == null || Link.IsReloading)
                    continue;

                ImGui.TableSetColumnIndex(2);
                if (ImGui.SmallButton($"{LOC.Get("MAT_InGameTool_History_Send")}##materialInGameHistorySend_{entry.Name}"))
                    Send(MatReloadSource.FromEditor(Bank, binder, file, View.Editor), MatGameLink.SendSource.History);
                GUI.Tooltip(LOC.Get("MAT_InGameTool_History_Send_TT"));

                ImGui.TableSetColumnIndex(3);
                if (ImGui.SmallButton($"{LOC.Get("MAT_InGameTool_History_Vanilla")}##materialInGameHistoryVanilla_{entry.Name}"))
                    Send(MatReloadSource.FromVanilla(binder, file, View.Editor), MatGameLink.SendSource.Vanilla);
                GUI.Tooltip(LOC.Get("MAT_InGameTool_Send_Vanilla_TT"));
            }

            ImGui.EndTable();
        }

        GUI.MultiButtonInput("materialInGameHistoryActions",
            "materialInGameHistoryClear", LOC.Get("MAT_InGameTool_History_Clear"), LOC.Get("MAT_InGameTool_History_Clear_TT"),
            Link.ClearHistory);
    }

    #region Browse

    private enum KindFilter { All, Character, Asset, MapPiece, Part }

    private const double RecentSeconds = 120;

    private MaterialUsage Usage => View.Editor.Usage;

    private KindFilter Kind = KindFilter.All;
    private readonly HashSet<string> ExpandedModels = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>One row of either view: a model, or a material (under a model, or on its own)</summary>
    private record Row(string Model, string Material, string Label, string KindLabel, string Count,
        DateTime FirstSeen, bool Initial, bool Expandable, bool Expanded, MaterialReference Reference);

    // Rows are rebuilt only when what they come from changes
    private object RowsKey;
    private List<Row> CachedRows = new();
    private int ExpandedVersion;

    // Browse
    private void DisplayBrowse()
    {
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAT_InGameTool_Browse_Header"), LOC.Get("MAT_InGameTool_Browse_Header_TT"));

        if (Link.State is not MatGameLink.LinkState.Attached)
        {
            GUI.WrappedText(LOC.Get("MAT_InGameTool_List_Not_Attached"));
            return;
        }

        GUI.WrappedText(LOC.Get("MAT_InGameTool_Browse_Count", Link.Models.Count, Link.InUse.Count, Link.TotalMaterials));

        // Filters
        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##materialInGameFilter", LOC.Get("MAT_InGameTool_Filter_Hint"), ref Filter, 128);
        GUI.Tooltip(LOC.Get("MAT_InGameTool_Filter_TT"));

        var kindLabels = new[]
        {
            LOC.Get("MAT_InGameTool_Kind_All"), LOC.Get("MAT_InGameTool_Kind_Character"), LOC.Get("MAT_InGameTool_Kind_Asset"),
            LOC.Get("MAT_InGameTool_Kind_MapPiece"), LOC.Get("MAT_InGameTool_Kind_Part")
        };
        var kind = (int)Kind;
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X * 0.5f);
        if (ImGui.Combo("##materialInGameKind", ref kind, kindLabels, kindLabels.Length))
            Kind = (KindFilter)kind;
        GUI.Tooltip(LOC.Get("MAT_InGameTool_Kind_TT"));

        ImGui.SameLine();
        ImGui.Checkbox($"{LOC.Get("MAT_InGameTool_Sort_Recent")}##materialInGameSortRecent", ref CFG.Current.MaterialEditor_InGame_SortRecent);
        GUI.Tooltip(LOC.Get("MAT_InGameTool_Sort_Recent_TT", RecentSeconds));

        if (ImGui.BeginTabBar("materialInGameBrowseTabs"))
        {
            if (ImGui.BeginTabItem($"{LOC.Get("MAT_InGameTool_Tab_Models")}##materialInGameTabModels"))
            {
                DisplayModelsTab();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem($"{LOC.Get("MAT_InGameTool_Tab_Materials")}##materialInGameTabMaterials"))
            {
                DisplayRows(BuildRows(false), "materialInGameMaterialTable");
                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }
    }

    private void DisplayModelsTab()
    {
        if (!Link.CanListModels)
        {
            GUI.WrappedText(LOC.Get("MAT_InGameTool_Models_Unavailable"));
            return;
        }

        // Each model's materials come from the Used By scan
        if (Usage.IsScanning)
        {
            var fraction = Usage.Total == 0 ? 0 : (float)Usage.Done / Usage.Total;
            ImGui.ProgressBar(fraction, new Vector2(-1, 0), $"{Usage.Done} / {Usage.Total}");
        }
        else if (!Usage.HasScan)
        {
            GUI.WrappedText(LOC.Get("MAT_InGameTool_Models_Needs_Scan"));
            GUI.MultiButtonInput("materialInGameScanActions",
                "materialInGameScan", LOC.Get("MAT_UsedBy_Scan"), LOC.Get("MAT_UsedBy_Scan_TT"),
                Usage.StartScan);
        }

        DisplayRows(BuildRows(true), "materialInGameModelTable");
    }

    private void DisplayRows(List<Row> rows, string tableId)
    {
        var selectedName = View.Selection.SelectedFileWrapper is MATBINFileWrapper selected
            ? MatGameLink.GameNameOf(selected.Key)
            : null;

        var flags = ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.ScrollY | ImGuiTableFlags.SizingStretchProp;

        if (!ImGui.BeginTable(tableId, 4, flags, new Vector2(-1, -1)))
            return;

        ImGui.TableSetupScrollFreeze(0, 1);
        ImGui.TableSetupColumn(LOC.Get("MAT_InGameTool_Column_Name"), ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableSetupColumn(LOC.Get("MAT_InGameTool_Column_Kind"), ImGuiTableColumnFlags.WidthFixed, 80);
        ImGui.TableSetupColumn(LOC.Get("MAT_InGameTool_Column_Users"), ImGuiTableColumnFlags.WidthFixed, 50);
        ImGui.TableSetupColumn(LOC.Get("MAT_InGameTool_Column_Loaded"), ImGuiTableColumnFlags.WidthFixed, 70);
        ImGui.TableHeadersRow();

        var now = DateTime.Now;
        var clipper = new ImGuiListClipper();
        clipper.Begin(rows.Count);

        while (clipper.Step())
        {
            for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var row = rows[i];
                var isMaterial = row.Material != null;
                var isSelected = isMaterial && string.Equals(row.Material, selectedName, StringComparison.OrdinalIgnoreCase);
                var recent = !row.Initial && (now - row.FirstSeen).TotalSeconds < RecentSeconds;

                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                var missing = isMaterial && row.Reference == null;
                if (missing)
                    ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Invalid_Text_Color);

                // IDs from names, so rows keep their state when the list changes between polls
                var id = $"##{tableId}_{row.Model}_{row.Material}";
                if (ImGui.Selectable($"{row.Label}{id}", isSelected, ImGuiSelectableFlags.SpanAllColumns))
                {
                    if (isMaterial && row.Reference != null)
                    {
                        View.Selection.JumpTo(Bank, row.Reference);
                    }
                    else if (!isMaterial && row.Expandable)
                    {
                        if (!ExpandedModels.Remove(row.Model))
                            ExpandedModels.Add(row.Model);
                        ExpandedVersion++;
                    }
                }

                if (missing)
                    ImGui.PopStyleColor();

                if (isMaterial)
                {
                    GUI.Tooltip(row.Reference != null
                        ? LOC.Get("MAT_InGameTool_Row_TT", row.Reference.BinderEntry.Filename)
                        : LOC.Get("MAT_InGameTool_Row_Not_In_Project_TT"));
                }
                else if (!row.Expandable)
                {
                    GUI.Tooltip(Usage.HasScan ? LOC.Get("MAT_InGameTool_Model_No_Materials_TT") : LOC.Get("MAT_InGameTool_Models_Needs_Scan"));
                }

                ImGui.TableSetColumnIndex(1);
                ImGui.TextUnformatted(row.KindLabel);

                ImGui.TableSetColumnIndex(2);
                ImGui.TextUnformatted(row.Count);

                ImGui.TableSetColumnIndex(3);
                if (recent)
                    ImGui.TextColored(UI.Current.ImGui_Benefit_Text_Color, LOC.Get("MAT_InGameTool_Loaded_Ago", Ago(now - row.FirstSeen)));
            }
        }

        clipper.End();
        ImGui.EndTable();
    }

    private static string Ago(TimeSpan span) => span.TotalSeconds < 60 ? $"{(int)span.TotalSeconds} s" : $"{(int)span.TotalMinutes} min";

    /// <summary>
    /// The rows of the Models view (models, with their in-use materials under the expanded ones) or of the Materials view
    /// </summary>
    private List<Row> BuildRows(bool models)
    {
        var inUse = Link.InUse;
        var loaded = Link.Models;
        var filter = Filter.Trim();

        var key = (models, inUse, loaded, filter, Kind, CFG.Current.MaterialEditor_InGame_SortRecent, ExpandedVersion,
            Usage.ScannedAt, Bank);
        if (Equals(key, RowsKey))
            return CachedRows;

        var rows = new List<Row>();
        var byName = inUse.ToDictionary(m => m.Name, StringComparer.OrdinalIgnoreCase);

        if (models)
        {
            var shown = new List<(MatGameLink.InGameModel Model, string Kind, string Label, List<MatGameLink.InGameMaterial> Materials, bool MatchedByMaterial)>();

            foreach (var model in loaded)
            {
                var entry = Usage.MaterialsOfModel(model.Name);
                var kind = entry?.Kind ?? KindFromName(model.Name);

                if (!KindMatches(kind))
                    continue;

                var alias = AliasOf(kind, model.Name);
                var label = string.IsNullOrEmpty(alias) ? model.Name : $"{model.Name}  {alias}";
                var materials = entry?.Materials.Where(byName.ContainsKey).Select(n => byName[n]).ToList() ?? new();

                var matchedByMaterial = false;
                if (filter.Length > 0 && !label.Contains(filter, StringComparison.OrdinalIgnoreCase))
                {
                    materials = materials.Where(m => m.Name.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToList();
                    if (materials.Count == 0)
                        continue;
                    matchedByMaterial = true;
                }

                shown.Add((model, kind, label, materials, matchedByMaterial));
            }

            shown.Sort((a, b) => CompareRecent(a.Model.FirstSeen, a.Model.Initial, b.Model.FirstSeen, b.Model.Initial, a.Model.Name, b.Model.Name));

            foreach (var (model, kind, label, materials, matchedByMaterial) in shown)
            {
                var expandable = materials.Count > 0;
                var expanded = expandable && (matchedByMaterial || ExpandedModels.Contains(model.Name));
                var arrow = !expandable ? "   " : expanded ? "v  " : ">  ";

                rows.Add(new Row(model.Name, null, arrow + label, KindLabel(kind), expandable ? $"{materials.Count}" : "",
                    model.FirstSeen, model.Initial, expandable, expanded, null));

                if (!expanded)
                    continue;

                foreach (var material in materials)
                {
                    rows.Add(new Row(model.Name, material.Name, $"      {material.Name}", "", $"{material.Users}",
                        material.FirstSeen, material.Initial, false, false, Bank?.FindReference(MaterialSourceType.MATBIN, material.Name)));
                }
            }
        }
        else
        {
            var shown = new List<(MatGameLink.InGameMaterial Material, string Kinds)>();

            foreach (var material in inUse)
            {
                var kinds = Usage.HasScan ? Usage.KindsOf(material.Name).ToList() : new List<string>();

                if (Kind != KindFilter.All && !kinds.Any(KindMatches))
                    continue;

                if (filter.Length > 0 && !material.Name.Contains(filter, StringComparison.OrdinalIgnoreCase))
                    continue;

                shown.Add((material, string.Join(", ", kinds.Select(KindLabel))));
            }

            shown.Sort((a, b) => CompareRecent(a.Material.FirstSeen, a.Material.Initial, b.Material.FirstSeen, b.Material.Initial, a.Material.Name, b.Material.Name));

            foreach (var (material, kinds) in shown)
            {
                rows.Add(new Row(null, material.Name, material.Name, kinds, $"{material.Users}",
                    material.FirstSeen, material.Initial, false, false, Bank?.FindReference(MaterialSourceType.MATBIN, material.Name)));
            }
        }

        RowsKey = key;
        CachedRows = rows;
        return rows;
    }

    /// <summary>Newest first when sorting by recently loaded (the ones loaded at connection last), else by name</summary>
    private static int CompareRecent(DateTime aTime, bool aInitial, DateTime bTime, bool bInitial, string aName, string bName)
    {
        if (CFG.Current.MaterialEditor_InGame_SortRecent)
        {
            if (aInitial != bInitial)
                return aInitial ? 1 : -1;

            var byTime = bTime.CompareTo(aTime);
            if (byTime != 0)
                return byTime;
        }

        return string.Compare(aName, bName, StringComparison.OrdinalIgnoreCase);
    }

    private bool KindMatches(string kind) => Kind switch
    {
        KindFilter.Character => kind == "Character",
        KindFilter.Asset => kind == "Asset",
        KindFilter.MapPiece => kind == "MapPiece",
        KindFilter.Part => kind == "Part",
        _ => true
    };

    private static string KindLabel(string kind) => kind switch
    {
        "Character" => LOC.Get("MAT_InGameTool_Kind_Character"),
        "Asset" => LOC.Get("MAT_InGameTool_Kind_Asset"),
        "MapPiece" => LOC.Get("MAT_InGameTool_Kind_MapPiece"),
        "Part" => LOC.Get("MAT_InGameTool_Kind_Part"),
        _ => ""
    };

    /// <summary>The kind of a model the Used By scan doesn't know, from its name: c2010, AEG099_001, m60_..., am_m_1620</summary>
    private static string KindFromName(string model)
    {
        var name = model.ToLowerInvariant();

        if (name.Length == 5 && name[0] == 'c' && name.Skip(1).All(char.IsDigit))
            return "Character";
        if (name.StartsWith("aeg"))
            return "Asset";
        if (name.Length > 3 && name[0] == 'm' && char.IsDigit(name[1]) && char.IsDigit(name[2]) && name[3] == '_')
            return "MapPiece";
        if (name.Length > 3 && name[2] == '_' && name[..2] is "am" or "bd" or "hd" or "lg" or "wp" or "fc" or "fg" or "hr")
            return "Part";

        return "";
    }

    // Alias names by kind, built once
    private Dictionary<string, Dictionary<string, string>> Aliases;

    private string AliasOf(string kind, string model)
    {
        if (Aliases == null)
        {
            Aliases = new();
            var data = Project.Handler.ProjectData.Aliases;

            foreach (var (k, type) in new[] { ("Character", ProjectAliasType.Characters), ("Asset", ProjectAliasType.Assets),
                ("MapPiece", ProjectAliasType.MapPieces), ("Part", ProjectAliasType.Parts) })
            {
                var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                if (data.TryGetValue(type, out var entries))
                {
                    foreach (var entry in entries)
                    {
                        if (entry?.ID != null && !string.IsNullOrWhiteSpace(entry.Name))
                            map.TryAdd(entry.ID, entry.Name);
                    }
                }
                Aliases[k] = map;
            }
        }

        return Aliases.TryGetValue(kind, out var names) && names.TryGetValue(model, out var name) ? name : "";
    }

    /// <summary>
    /// For the Reload section: the selected material's users in game and the loaded models that use it
    /// </summary>
    private void DisplaySelectedInGame(string gameName)
    {
        var material = Link.InUse.FirstOrDefault(m => string.Equals(m.Name, gameName, StringComparison.OrdinalIgnoreCase));

        if (material == null)
        {
            GUI.WrappedText(LOC.Get("MAT_InGameTool_Selected_Not_In_Use"));
            return;
        }

        var loaded = Link.Models.Select(m => m.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var models = Usage.HasScan
            ? Usage.UsesOf(gameName).Select(u => MatGameLink.GameNameOf(u.Flver)).Where(loaded.Contains)
                .Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(n => n, StringComparer.OrdinalIgnoreCase).ToList()
            : new List<string>();

        if (models.Count == 0)
        {
            GUI.WrappedText(LOC.Get("MAT_InGameTool_Selected_In_Use", material.Users));
            return;
        }

        var names = models.Take(5).Select(m =>
        {
            var alias = AliasOf(Usage.MaterialsOfModel(m)?.Kind ?? KindFromName(m), m);
            return string.IsNullOrEmpty(alias) ? m : $"{m} ({alias})";
        });

        GUI.WrappedText(LOC.Get("MAT_InGameTool_Selected_In_Use_By", material.Users,
            string.Join(", ", names) + (models.Count > 5 ? ", ..." : "")));
    }

    #endregion
}

