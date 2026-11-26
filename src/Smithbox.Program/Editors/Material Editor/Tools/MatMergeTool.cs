using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System.Collections.Concurrent;
using System.Numerics;

namespace StudioCore.Editors.MaterialEditor;

/// <summary>
/// Material Merge (Claude/Documentation/Materials and Textures.md §2, F11): brings materials from another project, a loose
/// .mtd / .matbin or a Material JSON export into this project. Analyse builds a three-way plan against vanilla
/// (<see cref="MaterialMerge"/>) off the UI thread; the plan is reviewed, with a choice per material and per
/// conflicting entry; Apply turns it into one undo step. Nothing is saved.
/// </summary>
public class MatMergeTool
{
    /// <summary>Saved as an int in <c>MaterialEditor_Merge_Source_Type</c></summary>
    public enum SourceType
    {
        Project,
        MaterialFile,
        JsonExport
    }

    private enum PlanFilter
    {
        All,
        Conflicts,
        Additions,
        Changes,
        Removals,
        Blocked
    }

    public MaterialEditorView View;
    public ProjectEntry Project;

    // Source
    private ProjectEntry TargetProject;
    private string MaterialFilePath = "";
    private string JsonPath = "";

    // Analysis
    private bool IsAnalysing;
    private MaterialMergePlan Plan;
    private readonly ConcurrentQueue<Action> MainThreadQueue = new();

    // Review
    private MaterialMergeItem SelectedItem;
    private PlanFilter Filter = PlanFilter.All;
    private string NameFilter = "";
    private readonly Dictionary<MaterialSourceType, string> TargetBinders = new();
    private string Message = "";

    private bool FocusRequested;

    // Loose materials, or whole binders of them (DS2's binder is a .bnd)
    private static readonly string[] MaterialFilters = ["mtd", "matbin", "mtdbnd", "matbinbnd", "bnd", "dcx"];
    private static readonly string[] JsonFilters = ["json"];

    public MatMergeTool(MaterialEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    private MaterialBank Bank => Project.Handler.MaterialData.PrimaryBank;
    private static MaterialEditorSettings Settings => CFG.Current.MaterialEditor;

    private SourceType CurrentSource => (SourceType)Settings.MaterialEditor_Merge_Source_Type;

    private bool HasSource => CurrentSource switch
    {
        SourceType.Project => TargetProject != null,
        SourceType.MaterialFile => File.Exists(MaterialFilePath),
        _ => File.Exists(JsonPath)
    };

    /// <summary>
    /// Takes finished analyses. Called every frame the tool window is drawn, so a result arrives even if another
    /// tool is selected meanwhile.
    /// </summary>
    public void ProcessPending()
    {
        while (MainThreadQueue.TryDequeue(out var action))
            action();
    }

    /// <summary>
    /// Shows the tool window and selects Material Merge in it (the Tools ▸ Data Transfer menu)
    /// </summary>
    public void RequestFocus()
    {
        CFG.Current.Interface.Interface_MaterialEditor_ToolWindow = true;
        FocusRequested = true;
    }

    public void OnToolWindow()
    {
        if (FocusRequested)
        {
            ToolSections.Focus();
            FocusRequested = false;
        }

        if (!ToolSections.Header($"{LOC.Get("MAT_Merge_Header")}##materialMergeToolHeader"))
            return;

        ImGui.BeginChild("MaterialMergeToolSection", ImGuiChildFlags.Borders);

        Display();

        ImGui.EndChild();
    }

    private void Display()
    {
        GUI.WrappedText(LOC.Get("MAT_Merge_Hint"));

        DisplaySource();
        DisplayOptions();
        DisplayActions();
        DisplayPlan();
    }

    #region Source
    private void DisplaySource()
    {
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAT_Merge_Source_Header"), LOC.Get("MAT_Merge_Source_Header_TT"));

        SourceRadio(SourceType.Project);
        SourceRadio(SourceType.MaterialFile);
        SourceRadio(SourceType.JsonExport);

        switch (CurrentSource)
        {
            case SourceType.Project:
                DisplayProjectList();
                break;

            case SourceType.MaterialFile:
                DisplayPathPicker(MaterialFilePath, "MAT_Merge_Browse_Material", BrowseMaterialFile);
                break;

            case SourceType.JsonExport:
                DisplayPathPicker(JsonPath, "MAT_Merge_Browse_Json", BrowseJson);
                break;
        }
    }

    private void SourceRadio(SourceType type)
    {
        if (ImGui.RadioButton($"{LOC.Get($"MAT_Merge_Source_{type}")}##materialMergeSource{type}", CurrentSource == type) &&
            CurrentSource != type)
        {
            Settings.MaterialEditor_Merge_Source_Type = (int)type;
            ClearPlan();
        }
        GUI.Tooltip(LOC.Get($"MAT_Merge_Source_{type}_TT"));
    }

    private void DisplayProjectList()
    {
        ImGui.BeginChild("materialMergeProjectList", new Vector2(0, 150 * DPI.UIScale()), ImGuiChildFlags.Borders);

        var index = 0;

        foreach (var proj in Smithbox.Orchestrator.Projects)
        {
            if (proj == null || proj == Project || proj.Descriptor.ProjectType != Project.Descriptor.ProjectType)
                continue;

            if (ImGui.Selectable($"{proj.Descriptor.ProjectName}##materialMergeProject{index}", proj == TargetProject) &&
                proj != TargetProject)
            {
                TargetProject = proj;

                // The plan belongs to the project it was built from
                ClearPlan();
            }

            index++;
        }

        if (index == 0)
            GUI.WrappedText(LOC.Get("MAT_Merge_No_Projects"));

        ImGui.EndChild();
    }

    private static void DisplayPathPicker(string path, string browseKey, Action browse)
    {
        if (ImGui.Button($"{LOC.Get(browseKey)}##{browseKey}"))
            browse();
        GUI.Tooltip(LOC.Get($"{browseKey}_TT"));

        ImGui.SameLine();

        // Unformatted: a path can hold '%'
        ImGui.TextUnformatted(string.IsNullOrEmpty(path) ? LOC.Get("MAT_Merge_No_Path") : path);
    }

    private void BrowseMaterialFile()
    {
        if (PlatformUtils.Instance.OpenFileDialog(LOC.Get("MAT_Merge_Browse_Material"), MaterialFilters, out var path) &&
            !string.IsNullOrEmpty(path) && path != MaterialFilePath)
        {
            MaterialFilePath = path;
            ClearPlan();
        }
    }

    private void BrowseJson()
    {
        if (PlatformUtils.Instance.OpenFileDialog(LOC.Get("MAT_Merge_Browse_Json"), JsonFilters, out var path) &&
            !string.IsNullOrEmpty(path) && path != JsonPath)
        {
            JsonPath = path;
            ClearPlan();
        }
    }
    #endregion

    #region Options
    private void DisplayOptions()
    {
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAT_Merge_Options_Header"), LOC.Get("MAT_Merge_Options_Header_TT"));

        // Conflicts: the default for each conflict, changeable per row afterwards
        ImGui.TextUnformatted(LOC.Get("MAT_Merge_Conflict_Mode"));
        GUI.Tooltip(LOC.Get("MAT_Merge_Conflict_Mode_TT"));

        foreach (var mode in Enum.GetValues<MaterialMergeConflictMode>())
        {
            if (ImGui.RadioButton($"{LOC.Get($"MAT_Merge_Conflict_Mode_{mode}")}##materialMergeMode{mode}",
                    Settings.MaterialEditor_Merge_Conflict_Mode == (int)mode) &&
                Settings.MaterialEditor_Merge_Conflict_Mode != (int)mode)
            {
                Settings.MaterialEditor_Merge_Conflict_Mode = (int)mode;
                ClearPlan();
            }
            GUI.Tooltip(LOC.Get($"MAT_Merge_Conflict_Mode_{mode}_TT"));
        }

        var additions = Settings.MaterialEditor_Merge_Include_Additions;
        if (ImGui.Checkbox($"{LOC.Get("MAT_Merge_Include_Additions")}##materialMergeAdditions", ref additions))
        {
            Settings.MaterialEditor_Merge_Include_Additions = additions;
            ClearPlan();
        }
        GUI.Tooltip(LOC.Get("MAT_Merge_Include_Additions_TT"));

        // Only another project holds whole binders, so only it can have deleted something
        if (CurrentSource == SourceType.Project)
        {
            var removals = Settings.MaterialEditor_Merge_Include_Removals;
            if (ImGui.Checkbox($"{LOC.Get("MAT_Merge_Include_Removals")}##materialMergeRemovals", ref removals))
            {
                Settings.MaterialEditor_Merge_Include_Removals = removals;
                ClearPlan();
            }
            GUI.Tooltip(LOC.Get("MAT_Merge_Include_Removals_TT"));
        }
    }

    private MaterialMergeOptions CurrentOptions() => new()
    {
        ConflictMode = (MaterialMergeConflictMode)Settings.MaterialEditor_Merge_Conflict_Mode,
        IncludeAdditions = Settings.MaterialEditor_Merge_Include_Additions,
        IncludeRemovals = CurrentSource == SourceType.Project && Settings.MaterialEditor_Merge_Include_Removals
    };
    #endregion

    #region Actions
    private void DisplayActions()
    {
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAT_Merge_Actions_Header"), LOC.Get("MAT_Merge_Actions_Header_TT"));

        var canApply = !IsAnalysing && Plan != null && Plan.Items.Any(Applies);

        GUI.ConditionalMultiButtonInput("materialMergeActions",
            "materialMergeAnalyse", LOC.Get(IsAnalysing ? "MAT_Merge_Analysing" : "MAT_Merge_Analyse"), LOC.Get("MAT_Merge_Analyse_TT"),
            Analyse, HasSource && !IsAnalysing,

            "materialMergeApply", LOC.Get("MAT_Merge_Apply"), LOC.Get("MAT_Merge_Apply_TT"),
            Apply, canApply,

            "materialMergeClear", LOC.Get("MAT_Merge_Clear"), LOC.Get("MAT_Merge_Clear_TT"),
            ClearPlan, Plan != null && !IsAnalysing);

        if (!string.IsNullOrEmpty(Message))
            GUI.WrappedText(Message);
    }

    /// <summary>
    /// Whether applying the item does anything: a choice other than Skip, and nothing that blocks it
    /// </summary>
    private static bool Applies(MaterialMergeItem item) =>
        item.Choice != MaterialMergeChoice.Skip && !item.HasErrors && (item.Result != null || item.Removes);

    private void ClearPlan()
    {
        Plan = null;
        SelectedItem = null;
        TargetBinders.Clear();
    }
    #endregion

    #region Analysis
    private void Analyse()
    {
        if (!HasSource || IsAnalysing || Bank == null)
            return;

        RunAnalysis();
    }

    private async void RunAnalysis()
    {
        IsAnalysing = true;
        ClearPlan();
        Message = "";

        // Read once, so changing them while the worker runs has no effect on this plan
        var sourceType = CurrentSource;
        var target = TargetProject;
        var materialPath = MaterialFilePath;
        var jsonPath = JsonPath;
        var options = CurrentOptions();

        // My materials as they are now; BuildPlan copies them, and Apply checks they haven't changed since
        var mine = Bank.ToMergeIndex();

        MaterialMergeSource source = null;
        MaterialMergePlan plan = null;
        Exception error = null;

        try
        {
            source = sourceType switch
            {
                SourceType.Project => await MaterialMergeProject.Load(target),
                SourceType.MaterialFile => await Task.Run(() => MaterialMergeSource.FromMaterialFile(materialPath)),
                _ => await Task.Run(() => MaterialMergeSource.FromJsonFile(jsonPath))
            };

            if (source.IsValid)
            {
                await View.Editor.Vanilla.WaitUntilReady();
                var vanilla = View.Editor.Vanilla.ToMergeIndex() ?? new MaterialMergeIndex();

                plan = await Task.Run(() => MaterialMerge.BuildPlan(source, mine, vanilla, options));
            }
        }
        catch (Exception e)
        {
            error = e;
        }

        var sourceName = sourceType switch
        {
            SourceType.Project => target?.Descriptor.ProjectName,
            SourceType.MaterialFile => materialPath,
            _ => jsonPath
        };

        MainThreadQueue.Enqueue(() =>
        {
            IsAnalysing = false;

            if (source is { IsValid: false })
            {
                Message = LOC.Get($"MAT_Merge_Source_{source.Error}", source.ErrorDetail);
                Smithbox.LogError(this, LOC.Get("MAT_Merge_Log_Source_Failed", sourceName, Message));
                return;
            }

            if (plan == null)
            {
                Message = LOC.Get("MAT_Merge_Log_Analysis_Failed", sourceName);
                Smithbox.LogError(this, Message, error);
                return;
            }

            Plan = plan;
            SelectedItem = plan.Items.FirstOrDefault();

            Message = LOC.Get("MAT_Merge_Log_Analysed", plan.SourceName,
                plan.Count(MaterialMergeKind.Add), plan.Count(MaterialMergeKind.Change),
                plan.Count(MaterialMergeKind.Combine), plan.Count(MaterialMergeKind.Conflict),
                plan.Count(MaterialMergeKind.Removed), plan.Unchanged);

            Smithbox.Log(this, Message);

            if (plan.Skipped.Count > 0)
                Smithbox.LogWarning(this, LOC.Get("MAT_Merge_Log_Skipped", plan.Skipped.Count, string.Join(", ", plan.Skipped.Take(20))));
        });
    }
    #endregion

    #region Plan
    private void DisplayPlan()
    {
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAT_Merge_Plan_Header"), LOC.Get("MAT_Merge_Plan_Header_TT"));

        if (Plan == null)
        {
            GUI.WrappedText(LOC.Get(IsAnalysing ? "MAT_Merge_Analysing" : "MAT_Merge_Not_Run"));
            return;
        }

        GUI.WrappedText(LOC.Get("MAT_Merge_Plan_Summary",
            Plan.Count(MaterialMergeKind.Add), Plan.Count(MaterialMergeKind.Change),
            Plan.Count(MaterialMergeKind.Combine), Plan.Count(MaterialMergeKind.Conflict),
            Plan.Count(MaterialMergeKind.Removed), Plan.Unchanged));

        if (Plan.Skipped.Count > 0)
        {
            ImGui.TextDisabled(LOC.Get("MAT_Merge_Plan_Skipped", Plan.Skipped.Count));
            GUI.Tooltip(string.Join("\n", Plan.Skipped.Take(40)));
        }

        if (Plan.Items.Count == 0)
            return;

        DisplayTargetBinders();

        // Filters: the name filter narrows the list as typed, it applies nothing
        ImGui.SetNextItemWidth(160 * DPI.UIScale());
        if (ImGui.BeginCombo("##materialMergeFilter", LOC.Get($"MAT_Merge_Filter_{Filter}")))
        {
            foreach (var filter in Enum.GetValues<PlanFilter>())
            {
                if (ImGui.Selectable(LOC.Get($"MAT_Merge_Filter_{filter}"), filter == Filter))
                    Filter = filter;
            }
            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("MAT_Merge_Filter_TT"));

        ImGui.SameLine();
        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##materialMergeNameFilter", LOC.Get("MAT_Merge_Name_Filter"), ref NameFilter, 255);

        GUI.ConditionalMultiButtonInput("materialMergeBulk",
            "materialMergeAllMine", LOC.Get("MAT_Merge_All_Conflicts_Mine"), LOC.Get("MAT_Merge_All_Conflicts_Mine_TT"),
            () => SetAllConflicts(MaterialEntrySide.Mine), true,

            "materialMergeAllTheirs", LOC.Get("MAT_Merge_All_Conflicts_Theirs"), LOC.Get("MAT_Merge_All_Conflicts_Theirs_TT"),
            () => SetAllConflicts(MaterialEntrySide.Theirs), true);

        DisplayItems();
        DisplayDetail();
    }

    private void DisplayTargetBinders()
    {
        // Where materials that don't say which binder they belong in are added
        foreach (var kind in Plan.Items.Where(i => i.Adds && i.AddBinderPath == null).Select(i => i.MaterialKind).Distinct())
        {
            var binders = BindersOf(kind).ToList();
            var current = TargetBinder(kind);

            ImGui.TextUnformatted(LOC.Get("MAT_Merge_Target_Binder", kind));
            GUI.Tooltip(LOC.Get("MAT_Merge_Target_Binder_TT"));
            ImGui.SameLine();

            ImGui.SetNextItemWidth(-1);
            if (ImGui.BeginCombo($"##materialMergeTarget{kind}", current?.Name ?? LOC.Get("MAT_Merge_No_Binder")))
            {
                foreach (var binder in binders)
                {
                    if (ImGui.Selectable($"{binder.Name}##materialMergeTarget{kind}{binder.FilePath}", binder == current))
                        TargetBinders[kind] = binder.FilePath;
                }
                ImGui.EndCombo();
            }
        }
    }

    private void DisplayItems()
    {
        var items = Plan.Items.Where(Matches).ToList();

        var height = Math.Clamp(items.Count, 3, 12) * ImGui.GetFrameHeightWithSpacing() + 8;

        if (!ImGui.BeginTable("materialMergeItems", 3,
                ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY | ImGuiTableFlags.Resizable,
                new Vector2(0, height)))
            return;

        ImGui.TableSetupScrollFreeze(0, 1);
        ImGui.TableSetupColumn(LOC.Get("MAT_Merge_Column_Kind"), ImGuiTableColumnFlags.WidthFixed, 110 * DPI.UIScale());
        ImGui.TableSetupColumn(LOC.Get("MAT_Merge_Column_Material"), ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableSetupColumn(LOC.Get("MAT_Merge_Column_Choice"), ImGuiTableColumnFlags.WidthFixed, 150 * DPI.UIScale());
        ImGui.TableHeadersRow();

        var clipper = new ImGuiListClipper();
        clipper.Begin(items.Count);

        while (clipper.Step())
        {
            for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var item = items[i];
                ImGui.TableNextRow();

                ImGui.TableNextColumn();
                ImGui.TextUnformatted(LOC.Get($"MAT_Merge_Kind_{item.Kind}"));
                GUI.Tooltip(ReasonText(item));

                ImGui.TableNextColumn();
                var label = item.NameTaken ? $"{item.ShortName}  {LOC.Get("MAT_Merge_Name_Taken_Short")}" : item.ShortName;
                if (item.HasErrors)
                    label = $"{label}  {LOC.Get("MAT_Merge_Blocked_Short")}";

                if (ImGui.Selectable($"{label}##materialMergeItem{i}", item == SelectedItem, ImGuiSelectableFlags.AllowDoubleClick))
                {
                    SelectedItem = item;

                    if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                        JumpTo(item, null);
                }
                GUI.Tooltip(LOC.Get("MAT_Merge_Item_TT", item.Mine?.BinderPath ?? item.AddBinderPath ?? TargetBinder(item.MaterialKind)?.Name ?? ""));

                ImGui.TableNextColumn();
                ChoiceCombo(item, $"materialMergeChoice{i}");
            }
        }

        clipper.End();
        ImGui.EndTable();
    }

    private bool Matches(MaterialMergeItem item)
    {
        if (!string.IsNullOrEmpty(NameFilter) && !item.ShortName.Contains(NameFilter, StringComparison.OrdinalIgnoreCase))
            return false;

        return Filter switch
        {
            PlanFilter.Conflicts => item.Kind is MaterialMergeKind.Conflict || item.NameTaken,
            PlanFilter.Additions => item.Kind is MaterialMergeKind.Add || item.Reason is MaterialConflictReason.DeletedByMe,
            PlanFilter.Changes => item.Kind is MaterialMergeKind.Change or MaterialMergeKind.Combine,
            PlanFilter.Removals => item.Kind is MaterialMergeKind.Removed,
            PlanFilter.Blocked => item.HasErrors,
            _ => true
        };
    }

    private static void ChoiceCombo(MaterialMergeItem item, string id)
    {
        ImGui.SetNextItemWidth(-1);
        if (!ImGui.BeginCombo($"##{id}", ChoiceText(item, item.Choice)))
            return;

        foreach (var choice in item.Choices)
        {
            if (ImGui.Selectable(ChoiceText(item, choice), choice == item.Choice) && choice != item.Choice)
            {
                item.Choice = choice;
                item.Rebuild();
            }
        }

        ImGui.EndCombo();
    }

    /// <summary>
    /// A choice in the item's words: Merge reads as Add, Take Theirs or Delete where that's what it does
    /// </summary>
    private static string ChoiceText(MaterialMergeItem item, MaterialMergeChoice choice) => choice switch
    {
        MaterialMergeChoice.Merge => item.Kind switch
        {
            MaterialMergeKind.Add => LOC.Get("MAT_Merge_Choice_Add"),
            MaterialMergeKind.Change => LOC.Get("MAT_Merge_Choice_Take"),
            MaterialMergeKind.Removed => LOC.Get("MAT_Merge_Choice_Delete"),
            _ => LOC.Get("MAT_Merge_Choice_Merge")
        },
        _ => LOC.Get($"MAT_Merge_Choice_{choice}")
    };

    private static string ReasonText(MaterialMergeItem item) =>
        item.Reason is MaterialConflictReason.None
            ? LOC.Get($"MAT_Merge_Kind_{item.Kind}_TT")
            : LOC.Get($"MAT_Merge_Reason_{item.Reason}");

    private void SetAllConflicts(MaterialEntrySide side)
    {
        foreach (var item in Plan.Items.Where(i => i.Kind is MaterialMergeKind.Conflict))
        {
            if (item.WholeOnly)
            {
                // Theirs, or keep mine by skipping
                var choice = side is MaterialEntrySide.Theirs ? MaterialMergeChoice.Theirs : MaterialMergeChoice.Skip;
                if (item.Choices.Contains(choice))
                    item.Choice = choice;
            }
            else
            {
                if (item.Choices.Contains(MaterialMergeChoice.Merge) && !item.NameTaken)
                    item.Choice = MaterialMergeChoice.Merge;

                foreach (var entry in item.Entries.Where(e => e.Change is MaterialEntryChange.Conflict))
                    entry.Choice = side;
            }

            item.Rebuild();
        }
    }

    /// <summary>
    /// The selected material: why it's in the plan, what checking the result found, and each differing entry
    /// on the three sides, with a choice per entry when its entries merge one by one
    /// </summary>
    private void DisplayDetail()
    {
        var item = SelectedItem;
        if (item == null || !Plan.Items.Contains(item))
            return;

        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAT_Merge_Detail_Header", item.ShortName), LOC.Get("MAT_Merge_Detail_Header_TT"));

        GUI.WrappedText(ReasonText(item));

        if (item.NameTaken && item.Mine != null)
            GUI.WrappedText(LOC.Get("MAT_Merge_Name_Taken", item.ShortName, item.Mine.BinderPath));

        // As Material Check shows them
        foreach (var finding in item.Findings)
            GUI.WrappedTextColored(MaterialEditorChecks.Color(finding), $"{Icons.ExclamationTriangle} {MaterialEditorChecks.Message(finding)}");

        if (item.HasErrors)
            GUI.WrappedText(LOC.Get("MAT_Merge_Blocked"));

        if (item.Entries.Count == 0)
            return;

        // Per-entry choices only matter when the entries are merged one by one
        var perEntry = item.Choice is MaterialMergeChoice.Merge && !item.WholeOnly &&
                       item.Kind is MaterialMergeKind.Combine or MaterialMergeKind.Conflict;

        if (!ImGui.BeginTable("materialMergeEntries", 6,
                ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.Resizable | ImGuiTableFlags.ScrollY,
                new Vector2(0, Math.Clamp(item.Entries.Count + 1, 3, 12) * ImGui.GetFrameHeightWithSpacing() + 8)))
            return;

        ImGui.TableSetupScrollFreeze(0, 1);
        ImGui.TableSetupColumn(LOC.Get("MAT_Merge_Column_Entry"), ImGuiTableColumnFlags.WidthStretch, 1.2f);
        ImGui.TableSetupColumn(LOC.Get("MAT_Merge_Column_Change"), ImGuiTableColumnFlags.WidthFixed, 80 * DPI.UIScale());
        ImGui.TableSetupColumn(LOC.Get("MAT_Merge_Column_Vanilla"), ImGuiTableColumnFlags.WidthStretch, 1f);
        ImGui.TableSetupColumn(LOC.Get("MAT_Merge_Column_Mine"), ImGuiTableColumnFlags.WidthStretch, 1f);
        ImGui.TableSetupColumn(LOC.Get("MAT_Merge_Column_Theirs"), ImGuiTableColumnFlags.WidthStretch, 1f);
        ImGui.TableSetupColumn(LOC.Get("MAT_Merge_Column_Use"), ImGuiTableColumnFlags.WidthFixed, 90 * DPI.UIScale());
        ImGui.TableHeadersRow();

        for (var i = 0; i < item.Entries.Count; i++)
        {
            var entry = item.Entries[i];
            ImGui.TableNextRow();

            ImGui.TableNextColumn();
            var name = entry.List == MaterialMergeEntry.HeaderList ? entry.Name : $"{entry.List} · {entry.Name}";
            if (ImGui.Selectable($"{name}##materialMergeEntry{i}", false, ImGuiSelectableFlags.AllowDoubleClick) &&
                ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
            {
                JumpTo(item, entry.List == MaterialMergeEntry.HeaderList ? null : entry.Name);
            }

            ImGui.TableNextColumn();
            ImGui.TextUnformatted(LOC.Get($"MAT_Merge_Entry_{entry.Change}"));

            ImGui.TableNextColumn();
            SideText(entry.VanillaText);
            ImGui.TableNextColumn();
            SideText(entry.MineText);
            ImGui.TableNextColumn();
            SideText(entry.TheirsText);

            ImGui.TableNextColumn();
            if (perEntry)
            {
                ImGui.SetNextItemWidth(-1);
                if (ImGui.BeginCombo($"##materialMergeEntryChoice{i}", LOC.Get($"MAT_Merge_Side_{entry.Choice}")))
                {
                    foreach (var side in Enum.GetValues<MaterialEntrySide>())
                    {
                        if (ImGui.Selectable(LOC.Get($"MAT_Merge_Side_{side}"), side == entry.Choice) && side != entry.Choice)
                        {
                            entry.Choice = side;
                            item.Rebuild();
                        }
                    }
                    ImGui.EndCombo();
                }
            }
            else
            {
                ImGui.TextDisabled(LOC.Get(item.Choice is MaterialMergeChoice.Skip ? "MAT_Merge_Side_Mine" : "MAT_Merge_Side_Theirs"));
            }
        }

        ImGui.EndTable();
    }

    private static void SideText(string text)
    {
        if (text == null)
            ImGui.TextDisabled(LOC.Get("MAT_Merge_Absent"));
        else
        {
            ImGui.TextUnformatted(text);
            GUI.Tooltip(text);
        }
    }

    private void JumpTo(MaterialMergeItem item, string entryName)
    {
        if (item.Mine == null)
            return;

        var binder = FindBinder(item.MaterialKind, item.Mine.BinderPath);
        var file = binder?.GetFile(item.Mine.Key);
        var entry = binder == null ? null : Bank.FindBinderEntry(binder);

        if (file == null || entry == null)
            return;

        View.Selection.JumpTo(Bank, new MaterialReference(item.MaterialKind, entry, file.Key, file));

        if (!string.IsNullOrEmpty(entryName))
            View.SetPropertyFilter(entryName);
    }
    #endregion

    #region Binders
    private IEnumerable<IMaterialBinder> BindersOf(MaterialSourceType kind) =>
        Bank.BindersInLoadOrder().Where(b => b.SourceType == kind);

    private IMaterialBinder FindBinder(MaterialSourceType kind, string path) =>
        path == null ? null : BindersOf(kind).FirstOrDefault(b => string.Equals(b.FilePath, path, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Where materials of a kind that don't name a binder are added: the one picked, else the selected binder,
    /// else the first in load order (the base binder)
    /// </summary>
    private IMaterialBinder TargetBinder(MaterialSourceType kind)
    {
        if (TargetBinders.TryGetValue(kind, out var path) && FindBinder(kind, path) is { } picked)
            return picked;

        if (View.Selection.SelectedBinder is { } selected && selected.SourceType == kind)
            return selected;

        return BindersOf(kind).FirstOrDefault();
    }
    #endregion

    #region Apply
    /// <summary>
    /// Turns the reviewed plan into one undo step: changed materials swapped for their merged result, new files
    /// inserted at the end of their binder, deleted files removed. Every material is checked again, since the
    /// project may have been edited since the analysis.
    /// </summary>
    private void Apply()
    {
        if (Plan == null)
            return;

        var changes = new List<EditorAction>();
        var inserts = new List<EditorAction>();
        var deletes = new List<EditorAction>();
        var previews = new List<string>();

        var nextIndex = new Dictionary<IMaterialBinder, int>();
        var newNames = new HashSet<(MaterialSourceType, string)>();

        int stale = 0, blocked = 0, noBinder = 0;

        foreach (var item in Plan.Items.Where(i => i.Choice != MaterialMergeChoice.Skip))
        {
            if (item.HasErrors)
            {
                blocked++;
                continue;
            }

            if (item.Removes)
            {
                var (binder, file) = Live(item);
                if (file == null)
                {
                    stale++;
                    continue;
                }

                deletes.Add(new MatDeleteFile(View.Editor, Bank, binder, file));
                continue;
            }

            if (item.Result == null)
                continue;

            if (item.Adds)
            {
                var binder = FindBinder(item.MaterialKind, item.AddBinderPath) ?? TargetBinder(item.MaterialKind);
                var template = binder?.GetFiles().FirstOrDefault();

                if (template == null)
                {
                    noBinder++;
                    continue;
                }

                var name = item.ShortName;

                if (item.Choice is MaterialMergeChoice.AddNumbered)
                {
                    for (var n = 2; Bank.IsShortNameTaken(item.MaterialKind, name) || newNames.Contains((item.MaterialKind, name.ToLowerInvariant())); n++)
                        name = $"{item.ShortName}_{n}";
                }
                else if (Bank.IsShortNameTaken(item.MaterialKind, name) || newNames.Contains((item.MaterialKind, name.ToLowerInvariant())))
                {
                    // Added here since the analysis
                    stale++;
                    continue;
                }

                newNames.Add((item.MaterialKind, name.ToLowerInvariant()));

                // Their folder within the binder when it's the same binder, else the binder's own naming style
                var sameBinder = item.TheirKey != null &&
                                 string.Equals(item.TheirBinderPath, binder.FilePath, StringComparison.OrdinalIgnoreCase);
                var key = MaterialBank.WithShortName(sameBinder ? item.TheirKey : template.Key, name);

                var file = binder.CreateCopy(template, key);
                var material = MaterialVanilla.MaterialOf(file);
                MatDataTransfer.CopyInto(material, MaterialEntries.Copy(item.Result));

                // A MATBIN's source path names it, so it follows a name that had to change
                if (name != item.ShortName && material is MATBIN matbin &&
                    string.Equals(MaterialBank.ShortNameOf(matbin.SourcePath ?? ""), item.ShortName, StringComparison.OrdinalIgnoreCase))
                {
                    matbin.SourcePath = MaterialBank.WithShortName(matbin.SourcePath, name);
                    MaterialKeys.Apply(matbin);
                }

                if (!nextIndex.TryGetValue(binder, out var index))
                    index = binder.FileOrder.Count;
                nextIndex[binder] = index + 1;

                inserts.Add(new MatInsertFile(View.Editor, Bank, binder, file, index));
                continue;
            }

            var (_, target) = Live(item);
            if (target == null)
            {
                stale++;
                continue;
            }

            changes.Add(new MatRevertMaterial(target, MaterialVanilla.MaterialOf(target), MaterialEntries.Copy(item.Result)));
            previews.Add(target.Key);
        }

        // Changes first, then new files at the end of their binders, then deletions
        var actions = changes.Concat(inserts).Concat(deletes).ToList();

        if (actions.Count > 0)
        {
            View.ActionManager.ExecuteAction(new CompoundAction(actions));

            foreach (var key in previews)
                MaterialPreview.Queue(key);
        }

        Message = LOC.Get("MAT_Merge_Applied", changes.Count, inserts.Count, deletes.Count, stale, blocked + noBinder);
        Smithbox.Log(this, Message);

        // The plan was built from the project as it was: analyse again for another round
        ClearPlan();
    }

    /// <summary>
    /// My material file an item merges into, if it's still as analysed
    /// </summary>
    private (IMaterialBinder binder, MaterialFileWrapper file) Live(MaterialMergeItem item)
    {
        if (item.Mine == null)
            return (null, null);

        var binder = FindBinder(item.MaterialKind, item.Mine.BinderPath);
        var file = binder?.GetFile(item.Mine.Key);

        if (file == null || MaterialEntries.Signature(MaterialVanilla.MaterialOf(file)) != item.MineSignature)
            return (binder, null);

        return (binder, file);
    }
    #endregion
}
