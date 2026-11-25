using Andre.IO.VFS;
using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System.Collections.Concurrent;
using System.Numerics;
using System.Text.Json;

namespace StudioCore.Editors.TextEditor;

/// <summary>
/// Where Text Merge reads the other side from. Saved as an int in <c>TextEditor_Merge_Source_Type</c>.
/// </summary>
public enum TextMergeSourceType
{
    Project = 0,
    ModFolder = 1,
    JsonExport = 2
}

/// <summary>
/// The default for entries that both projects changed (or both added) with different text.
/// Saved as an int in <c>TextEditor_Merge_Conflict_Mode</c>.
/// </summary>
public enum TextMergeConflictMode
{
    KeepMine = 0,
    TakeTheirs = 1,
    AskEach = 2
}

/// <summary>
/// Merges another project's text into the current project.
///
/// Three-way merge against vanilla (V = vanilla, M = mine, O = other project):
/// - O equals M, or O equals V: nothing to do (keeps my edits).
/// - Only they changed it (M equals V): update to theirs.
/// - Only they have it (not in M; not in V): add it.
/// - Both changed it differently, both added the ID, or I deleted it and they changed it: conflict.
/// - They deleted a vanilla entry I still have: listed as "removed by them", off by default.
///
/// Analyse builds the plan on a worker thread (read-only). The plan is shown for review (per FMG and
/// per entry, with a choice per conflict), and Apply turns it into one <see cref="FmgGroupedAction"/>
/// on the UI thread, so the merge is a single undo step.
/// </summary>
public class TextMerge
{
    public TextEditorView View;
    public ProjectEntry Project;

    private ProjectEntry targetProject = null;
    private string sourceFolderPath = "";
    private string sourceJsonPath = "";
    private bool isAnalysing = false;

    // The bank read from a mod folder; replaced (and disposed) by the next folder analysis
    private TextBank folderBank = null;

    private MergePlan currentPlan = null;
    private MergeFmgGroup selectedGroup = null;
    private MergeFilter currentFilter = MergeFilter.All;

    // Results of the worker are applied while drawing, on the UI thread
    private readonly ConcurrentQueue<Action> MainThreadQueue = new();

    public TextMerge(TextEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    /// <summary>
    /// Take finished analyses. Called every frame by the view, so a result arrives even if the
    /// Text Merge section was closed while it ran.
    /// </summary>
    public void ProcessPending()
    {
        while (MainThreadQueue.TryDequeue(out var action))
        {
            action();
        }
    }

    #region Display

    public void Display()
    {
        ImGui.BeginChild("TextMergeSection", ImGuiChildFlags.Borders);

        GUI.WrappedText(LOC.Get("TEXT_TextMerge_Hint"));

        DisplaySourceList();
        DisplayOptions();
        DisplayActions();
        DisplayFillOffer();

        if (currentPlan != null)
        {
            DisplayPlan(currentPlan);
        }

        ImGui.EndChild();
    }

    private void DisplaySourceList()
    {
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("TEXT_TextMerge_Header_Target_Project"),
            LOC.Get("TEXT_TextMerge_Header_Target_Project_TT"));

        SourceTypeRadio(TextMergeSourceType.Project, "TEXT_TextMerge_Source_Project");
        ImGui.SameLine();
        SourceTypeRadio(TextMergeSourceType.ModFolder, "TEXT_TextMerge_Source_Folder");
        ImGui.SameLine();
        SourceTypeRadio(TextMergeSourceType.JsonExport, "TEXT_TextMerge_Source_Json");

        switch (CurrentSourceType)
        {
            case TextMergeSourceType.ModFolder:
                DisplayPathPicker(sourceFolderPath, "TEXT_TextMerge_Browse_Folder", BrowseFolder);
                break;

            case TextMergeSourceType.JsonExport:
                DisplayPathPicker(sourceJsonPath, "TEXT_TextMerge_Browse_Json", BrowseJson);
                break;

            default:
                DisplayProjectList();
                break;
        }
    }

    private static TextMergeSourceType CurrentSourceType =>
        (TextMergeSourceType)CFG.Current.TextEditor.TextEditor_Merge_Source_Type;

    private void SourceTypeRadio(TextMergeSourceType type, string locKey)
    {
        if (ImGui.RadioButton($"{LOC.Get(locKey)}##mergeSourceType{(int)type}", CurrentSourceType == type))
        {
            if (CurrentSourceType != type)
            {
                ClearPlan();
            }

            CFG.Current.TextEditor.TextEditor_Merge_Source_Type = (int)type;
        }
        GUI.Tooltip(LOC.Get($"{locKey}_TT"));
    }

    private void DisplayPathPicker(string path, string browseLocKey, Action browse)
    {
        if (ImGui.Button($"{LOC.Get(browseLocKey)}##mergeBrowse"))
        {
            browse();
        }
        GUI.Tooltip(LOC.Get($"{browseLocKey}_TT"));

        ImGui.SameLine();

        // Unformatted: a path can hold '%'
        ImGui.TextUnformatted(string.IsNullOrEmpty(path) ? LOC.Get("TEXT_TextMerge_No_Path") : path);
    }

    private void BrowseFolder()
    {
        if (PlatformUtils.Instance.OpenFolderDialog(LOC.Get("TEXT_TextMerge_Browse_Folder"), out var path) &&
            !string.IsNullOrEmpty(path) && path != sourceFolderPath)
        {
            sourceFolderPath = path;
            ClearPlan();
        }
    }

    private void BrowseJson()
    {
        if (PlatformUtils.Instance.OpenFileDialog(LOC.Get("TEXT_TextMerge_Browse_Json"), ["json"], out var path) &&
            !string.IsNullOrEmpty(path) && path != sourceJsonPath)
        {
            sourceJsonPath = path;
            ClearPlan();
        }
    }

    private void DisplayProjectList()
    {
        ImGui.BeginChild("mergeProjectListSection", new Vector2(0, 150), ImGuiChildFlags.Borders);
        int index = 0;

        foreach (var proj in Smithbox.Orchestrator.Projects)
        {
            if (proj == null)
                continue;

            if (proj.Descriptor.ProjectType != View.Project.Descriptor.ProjectType)
                continue;

            if (proj == Smithbox.Orchestrator.SelectedProject)
                continue;

            var isSelected = targetProject != null &&
                targetProject.Descriptor.ProjectName == proj.Descriptor.ProjectName;

            if (ImGui.Selectable($"{proj.Descriptor.ProjectName}##targetProject{index}", isSelected))
            {
                if (targetProject != proj)
                {
                    // The plan belongs to the project it was built from
                    ClearPlan();
                }

                targetProject = proj;
            }

            index++;
        }

        if (index == 0)
        {
            GUI.WrappedText(LOC.Get("TEXT_TextMerge_No_Projects"));
        }

        ImGui.EndChild();
    }

    private void DisplayOptions()
    {
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("TEXT_TextMerge_Header_Options"),
            LOC.Get("TEXT_TextMerge_Header_Options_TT"));

        DisplayLanguages();

        ImGui.Text(LOC.Get("TEXT_TextMerge_Conflict_Label"));
        GUI.Tooltip(LOC.Get("TEXT_TextMerge_Conflict_Label_TT"));

        ConflictModeRadio(TextMergeConflictMode.KeepMine, "TEXT_TextMerge_Conflict_Keep_Mine");
        ImGui.SameLine();
        ConflictModeRadio(TextMergeConflictMode.TakeTheirs, "TEXT_TextMerge_Conflict_Take_Theirs");
        ImGui.SameLine();
        ConflictModeRadio(TextMergeConflictMode.AskEach, "TEXT_TextMerge_Conflict_Ask_Each");
    }

    /// <summary>
    /// One checkbox per language this project has. Saved in <c>TextEditor_Merge_Languages</c>
    /// (comma-separated); none ticked means the primary language. Not used for a JSON export, which
    /// names its own container.
    /// </summary>
    private void DisplayLanguages()
    {
        ImGui.Text(LOC.Get("TEXT_TextMerge_Languages_Label"));
        GUI.Tooltip(LOC.Get("TEXT_TextMerge_Languages_Label_TT"));

        if (CurrentSourceType == TextMergeSourceType.JsonExport)
        {
            GUI.WrappedText(LOC.Get("TEXT_TextMerge_Languages_Json"));
            return;
        }

        var selected = GetMergeLanguages();
        var primary = CFG.Current.TextEditor.TextEditor_Primary_Category;
        var changed = false;

        foreach (var language in GetProjectLanguages())
        {
            var isTicked = selected.Contains(language);
            var name = LOC.Get(language.GetDisplayName());

            if (language == primary)
                name = LOC.Get("TEXT_TextMerge_Language_Primary", name);

            if (ImGui.Checkbox($"{name}##mergeLanguage{(int)language}", ref isTicked))
            {
                if (isTicked) selected.Add(language); else selected.Remove(language);
                changed = true;
            }
        }

        if (ImGui.Button($"{LOC.Get("TEXT_TextMerge_Languages_All")}##mergeLanguagesAll"))
        {
            selected = GetProjectLanguages().ToHashSet();
            changed = true;
        }
        GUI.Tooltip(LOC.Get("TEXT_TextMerge_Languages_All_TT"));

        ImGui.SameLine();

        if (ImGui.Button($"{LOC.Get("TEXT_TextMerge_Languages_Primary")}##mergeLanguagesPrimary"))
        {
            selected = new HashSet<TextContainerCategory> { primary };
            changed = true;
        }
        GUI.Tooltip(LOC.Get("TEXT_TextMerge_Languages_Primary_TT"));

        if (changed)
        {
            CFG.Current.TextEditor.TextEditor_Merge_Languages = string.Join(",", selected.Select(e => e.ToString()));

            // The plan covers the languages chosen when it was built
            ClearPlan();
        }
    }

    /// <summary>
    /// Languages of this project's containers, in enum order.
    /// </summary>
    private List<TextContainerCategory> GetProjectLanguages()
    {
        var bank = View.Project.Handler.TextData.PrimaryBank;

        if (bank?.Containers == null)
            return new();

        return bank.Containers.Values
            .Select(e => e.ContainerDisplayCategory)
            .Distinct()
            .OrderBy(e => e)
            .ToList();
    }

    /// <summary>
    /// The saved language choice; the primary language when none is saved (or none is valid).
    /// </summary>
    private static HashSet<TextContainerCategory> GetMergeLanguages()
    {
        var result = new HashSet<TextContainerCategory>();
        var saved = CFG.Current.TextEditor.TextEditor_Merge_Languages ?? "";

        foreach (var part in saved.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (Enum.TryParse<TextContainerCategory>(part, out var language))
                result.Add(language);
        }

        if (result.Count == 0)
            result.Add(CFG.Current.TextEditor.TextEditor_Primary_Category);

        return result;
    }

    private void ConflictModeRadio(TextMergeConflictMode mode, string locKey)
    {
        var settings = CFG.Current.TextEditor;

        if (ImGui.RadioButton($"{LOC.Get(locKey)}##conflictMode{(int)mode}",
            settings.TextEditor_Merge_Conflict_Mode == (int)mode))
        {
            settings.TextEditor_Merge_Conflict_Mode = (int)mode;
        }
        GUI.Tooltip(LOC.Get($"{locKey}_TT"));
    }

    private void DisplayActions()
    {
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("TEXT_TextMerge_Header_Actions"),
            LOC.Get("TEXT_TextMerge_Header_Actions_TT"));

        if (isAnalysing)
        {
            GUI.WrappedText(LOC.Get("TEXT_TextMerge_Analysing"));
            return;
        }

        var canApply = currentPlan != null && currentPlan.CanApply;

        GUI.ConditionalMultiButtonInput("mergeActions",
            "analyseMerge",
            LOC.Get("TEXT_TextMerge_Action_Analyse"),
            LOC.Get("TEXT_TextMerge_Action_Analyse_TT"),
            Analyse, HasSource,

            "applyMerge",
            LOC.Get("TEXT_TextMerge_Action_Apply"),
            LOC.Get("TEXT_TextMerge_Action_Apply_TT"),
            ApplyCurrentPlan, canApply,

            "clearMerge",
            LOC.Get("TEXT_TextMerge_Action_Clear"),
            LOC.Get("TEXT_TextMerge_Action_Clear_TT"),
            ClearPlan, currentPlan != null);

        if (currentPlan != null && currentPlan.UnresolvedCount > 0)
        {
            GUI.WrappedText(LOC.Get("TEXT_TextMerge_Unresolved_Hint", currentPlan.UnresolvedCount));
        }
    }

    private void DisplayPlan(MergePlan plan)
    {
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("TEXT_TextMerge_Header_Plan"),
            LOC.Get("TEXT_TextMerge_Header_Plan_TT"));

        GUI.WrappedText(LOC.Get("TEXT_TextMerge_Plan_Summary", plan.SourceName,
            plan.Count(MergeItemKind.Add), plan.Count(MergeItemKind.Change),
            plan.Count(MergeItemKind.Conflict), plan.Count(MergeItemKind.Removed),
            plan.UnchangedByThem));

        if (plan.FmgsWithoutVanilla > 0)
        {
            GUI.WrappedText(LOC.Get("TEXT_TextMerge_Plan_No_Vanilla", plan.FmgsWithoutVanilla));
        }

        if (plan.Unmatched.Count > 0)
        {
            GUI.WrappedText(LOC.Get("TEXT_TextMerge_Plan_Unmatched", plan.Unmatched.Count));

            // At most 30 lines in the tooltip; the log has the full list
            var shown = string.Join("\n", plan.Unmatched.Take(30));
            if (plan.Unmatched.Count > 30)
                shown += $"\n{LOC.Get("TEXT_TextMerge_Plan_Unmatched_More", plan.Unmatched.Count - 30)}";

            GUI.Tooltip(LOC.Get("TEXT_TextMerge_Plan_Unmatched_TT", shown));
        }

        if (plan.Groups.Count == 0)
        {
            GUI.WrappedText(LOC.Get("TEXT_TextMerge_Plan_Empty"));
            return;
        }

        GUI.MultiButtonInput("mergeConflictBulk",
            "keepAllMine",
            LOC.Get("TEXT_TextMerge_Action_Keep_All_Mine"),
            LOC.Get("TEXT_TextMerge_Action_Keep_All_Mine_TT"),
            () => SetAllConflicts(plan, MergeChoice.Mine),

            "takeAllTheirs",
            LOC.Get("TEXT_TextMerge_Action_Take_All_Theirs"),
            LOC.Get("TEXT_TextMerge_Action_Take_All_Theirs_TT"),
            () => SetAllConflicts(plan, MergeChoice.Theirs));

        // Filter
        ImGui.SetNextItemWidth(200);
        if (ImGui.BeginCombo("##mergeFilter", FilterName(currentFilter)))
        {
            foreach (var filter in Enum.GetValues<MergeFilter>())
            {
                if (ImGui.Selectable(FilterName(filter), filter == currentFilter))
                {
                    currentFilter = filter;
                }
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("TEXT_TextMerge_Filter_TT"));

        DisplayGroupList(plan);
        DisplayGroupRows();
    }

    private void DisplayGroupList(MergePlan plan)
    {
        ImGui.BeginChild("mergeGroupList", new Vector2(0, 160), ImGuiChildFlags.Borders);

        for (int i = 0; i < plan.Groups.Count; i++)
        {
            var group = plan.Groups[i];

            if (!group.Items.Any(e => MatchesFilter(e, currentFilter)))
                continue;

            var included = group.Included;
            if (ImGui.Checkbox($"##mergeGroupInclude{i}", ref included))
            {
                group.Included = included;
            }
            GUI.Tooltip(LOC.Get("TEXT_TextMerge_Group_Include_TT"));

            ImGui.SameLine();

            var label = LOC.Get("TEXT_TextMerge_Group_Label", GetGroupName(group),
                group.Count(MergeItemKind.Add), group.Count(MergeItemKind.Change),
                group.Count(MergeItemKind.Conflict), group.Count(MergeItemKind.Removed));

            if (ImGui.Selectable($"{label}##mergeGroup{i}", selectedGroup == group))
            {
                selectedGroup = group;
            }
        }

        ImGui.EndChild();
    }

    private void DisplayGroupRows()
    {
        ImGui.BeginChild("mergeGroupRows", new Vector2(0, 0), ImGuiChildFlags.Borders);

        if (selectedGroup == null)
        {
            GUI.WrappedText(LOC.Get("TEXT_TextMerge_Select_Group"));
            ImGui.EndChild();
            return;
        }

        var rows = selectedGroup.Items.Where(e => MatchesFilter(e, currentFilter)).ToList();

        var tblFlags = ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersInnerV |
            ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.ScrollY;

        if (ImGui.BeginTable("mergeRowsTable", 4, tblFlags))
        {
            ImGui.TableSetupScrollFreeze(0, 1);
            ImGui.TableSetupColumn(LOC.Get("TEXT_TextMerge_Column_Use"), ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn(LOC.Get("TEXT_TextMerge_Column_ID"), ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn(LOC.Get("TEXT_TextMerge_Column_Kind"), ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn(LOC.Get("TEXT_TextMerge_Column_Text"), ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableHeadersRow();

            // Clipped: one FMG can have thousands of added entries
            var clipper = new ImGuiListClipper();
            clipper.Begin(rows.Count);

            while (clipper.Step())
            {
                for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                {
                    DisplayRow(rows[i], i);
                }
            }

            clipper.End();

            ImGui.EndTable();
        }

        ImGui.EndChild();
    }

    private void DisplayRow(MergeItem item, int i)
    {
        ImGui.TableNextRow();

        // Use
        ImGui.TableSetColumnIndex(0);

        if (item.Kind == MergeItemKind.Conflict)
        {
            if (ImGui.RadioButton($"{LOC.Get("TEXT_TextMerge_Choice_Mine")}##mergeMine{i}", item.Choice == MergeChoice.Mine))
            {
                item.Choice = MergeChoice.Mine;
            }
            GUI.Tooltip(LOC.Get("TEXT_TextMerge_Choice_Mine_TT"));

            ImGui.SameLine();

            if (ImGui.RadioButton($"{LOC.Get("TEXT_TextMerge_Choice_Theirs")}##mergeTheirs{i}", item.Choice == MergeChoice.Theirs))
            {
                item.Choice = MergeChoice.Theirs;
            }
            GUI.Tooltip(LOC.Get("TEXT_TextMerge_Choice_Theirs_TT"));
        }
        else
        {
            var included = item.Included;
            if (ImGui.Checkbox($"##mergeInclude{i}", ref included))
            {
                item.Included = included;
            }
            GUI.Tooltip(LOC.Get("TEXT_TextMerge_Row_Include_TT"));
        }

        // ID: click to show the entry in the editor
        ImGui.TableSetColumnIndex(1);

        if (ImGui.Selectable($"{item.ID}##mergeId{i}"))
        {
            JumpToEntry(item);
        }
        GUI.Tooltip(LOC.Get("TEXT_TextMerge_Row_Jump_TT"));

        // Kind
        ImGui.TableSetColumnIndex(2);
        ImGui.Text(KindName(item.Kind));

        // Text: theirs (mine for an entry they removed), with all three versions in the tooltip
        ImGui.TableSetColumnIndex(3);

        var shownText = item.Kind == MergeItemKind.Removed ? item.MyEntry?.Text : item.TheirEntry?.Text;
        // Unformatted: FMG text can hold '%' (e.g. ER's %null%)
        ImGui.TextUnformatted(FirstLine(shownText));

        GUI.Tooltip(LOC.Get("TEXT_TextMerge_Row_Versions_TT",
            ShowText(item.VanillaText, item.HasVanilla),
            ShowText(item.MyEntry?.Text, item.MyEntry != null),
            ShowText(item.TheirEntry?.Text, item.TheirEntry != null)));
    }

    private string GetGroupName(MergeFmgGroup group)
    {
        var containerName = group.Container.FileEntry.Filename;
        if (CFG.Current.TextEditor.TextEditor_Container_List_Display_Community_Names)
        {
            containerName = group.Container.GetContainerDisplayName();
        }

        var fmgName = group.Wrapper.Name;
        if (CFG.Current.TextEditor.TextEditor_Text_File_List_Display_Community_Names)
        {
            fmgName = TextUtils.GetFmgDisplayName(View.Project, group.Container, group.Wrapper.ID, group.Wrapper.Name);
        }

        return $"{group.Container.ContainerDisplayCategory} - {containerName} - {fmgName}";
    }

    private void JumpToEntry(MergeItem item)
    {
        var group = item.Group;
        var category = group.Container.ContainerDisplayCategory.ToString();

        EditorCommandQueue.AddCommand(
            $"text/select/{category}/{group.Container.FileEntry.Filename}/{group.Wrapper.Name}/{item.ID}");
    }

    private static string FirstLine(string text)
    {
        if (text == null)
            return "<null>";

        var newline = text.IndexOf('\n');
        return newline < 0 ? text : $"{text[..newline]} <...>";
    }

    private static string ShowText(string text, bool exists)
    {
        if (!exists)
            return LOC.Get("TEXT_TextMerge_Row_Missing");

        return text ?? "<null>";
    }

    private static string KindName(MergeItemKind kind) => kind switch
    {
        MergeItemKind.Add => LOC.Get("TEXT_TextMerge_Kind_Add"),
        MergeItemKind.Change => LOC.Get("TEXT_TextMerge_Kind_Change"),
        MergeItemKind.Conflict => LOC.Get("TEXT_TextMerge_Kind_Conflict"),
        MergeItemKind.Removed => LOC.Get("TEXT_TextMerge_Kind_Removed"),
        _ => ""
    };

    private static string FilterName(MergeFilter filter) => filter switch
    {
        MergeFilter.All => LOC.Get("TEXT_TextMerge_Filter_All"),
        MergeFilter.Added => LOC.Get("TEXT_TextMerge_Kind_Add"),
        MergeFilter.Updated => LOC.Get("TEXT_TextMerge_Kind_Change"),
        MergeFilter.Conflicts => LOC.Get("TEXT_TextMerge_Kind_Conflict"),
        MergeFilter.UnresolvedConflicts => LOC.Get("TEXT_TextMerge_Filter_Unresolved"),
        MergeFilter.Removed => LOC.Get("TEXT_TextMerge_Kind_Removed"),
        _ => ""
    };

    private static bool MatchesFilter(MergeItem item, MergeFilter filter) => filter switch
    {
        MergeFilter.Added => item.Kind == MergeItemKind.Add,
        MergeFilter.Updated => item.Kind == MergeItemKind.Change,
        MergeFilter.Conflicts => item.Kind == MergeItemKind.Conflict,
        MergeFilter.UnresolvedConflicts => item.Kind == MergeItemKind.Conflict && item.Choice == MergeChoice.Unresolved,
        MergeFilter.Removed => item.Kind == MergeItemKind.Removed,
        _ => true
    };

    private static void SetAllConflicts(MergePlan plan, MergeChoice choice)
    {
        foreach (var item in plan.Groups.SelectMany(e => e.Items))
        {
            if (item.Kind == MergeItemKind.Conflict)
                item.Choice = choice;
        }
    }

    private void ClearPlan()
    {
        currentPlan = null;
        selectedGroup = null;
    }

    #endregion

    #region Analyse

    private bool HasSource => CurrentSourceType switch
    {
        TextMergeSourceType.ModFolder => Directory.Exists(sourceFolderPath),
        TextMergeSourceType.JsonExport => File.Exists(sourceJsonPath),
        _ => targetProject != null
    };

    // Identifies the chosen source, so a plan finished after the user picked another one is dropped
    private string CurrentSourceKey => CurrentSourceType switch
    {
        TextMergeSourceType.ModFolder => $"folder|{sourceFolderPath}",
        TextMergeSourceType.JsonExport => $"json|{sourceJsonPath}",
        _ => $"project|{targetProject?.Descriptor.ProjectName}"
    };

    public void Analyse()
    {
        if (!HasSource)
        {
            Smithbox.LogError<TextMerge>(LOC.Get("TEXT_TextMerge_Log_No_Target_Project"));
            return;
        }

        if (isAnalysing)
        {
            Smithbox.LogError<TextMerge>(LOC.Get("TEXT_TextMerge_Log_Merge_In_Progress"));
            return;
        }

        RunAnalysis(View);
    }

    private async void RunAnalysis(TextEditorView view)
    {
        isAnalysing = true;
        ClearPlan();
        pendingFill = null;

        // Options are read once, so changing them while the worker runs has no effect on this plan
        var sourceType = CurrentSourceType;
        var sourceKey = CurrentSourceKey;
        var project = targetProject;
        var folderPath = sourceFolderPath;
        var jsonPath = sourceJsonPath;
        var mode = (TextMergeConflictMode)CFG.Current.TextEditor.TextEditor_Merge_Conflict_Mode;
        var languages = GetMergeLanguages();

        // A JSON export without its source container is matched against the selected container
        var selectedContainer = view.Selection.SelectedContainerWrapper;

        MergeSource source = null;
        MergePlan plan = null;
        Exception error = null;

        try
        {
            switch (sourceType)
            {
                case TextMergeSourceType.ModFolder:
                    var bank = await Task.Run(() => CreateFolderBank(view, folderPath));
                    if (bank != null)
                    {
                        folderBank?.Dispose();
                        folderBank = bank;
                        source = new BankMergeSource(Path.GetFileName(folderPath.TrimEnd('\\', '/')), bank);
                    }
                    break;

                case TextMergeSourceType.JsonExport:
                    source = await Task.Run(() => JsonMergeSource.Load(jsonPath, selectedContainer));
                    break;

                default:
                    await view.Project.Handler.TextData.LoadAuxBank(project, true);
                    if (view.Project.Handler.TextData.AuxBanks.TryGetValue(project.Descriptor.ProjectName, out var auxBank))
                    {
                        source = new BankMergeSource(project.Descriptor.ProjectName, auxBank);
                    }
                    break;
            }

            if (source != null)
            {
                plan = await Task.Run(() => BuildPlan(view, source, languages, mode));
            }
        }
        catch (Exception e)
        {
            error = e;
        }

        var sourceName = sourceType switch
        {
            TextMergeSourceType.ModFolder => folderPath,
            TextMergeSourceType.JsonExport => jsonPath,
            _ => project?.Descriptor.ProjectName
        };

        MainThreadQueue.Enqueue(() =>
        {
            isAnalysing = false;

            if (plan == null)
            {
                Smithbox.LogError<TextMerge>(LOC.Get("TEXT_TextMerge_Log_Merged_Text_FAIL", sourceName), error);
                return;
            }

            // Another source was picked while this one was analysed
            if (CurrentSourceKey != sourceKey)
                return;

            currentPlan = plan;
            selectedGroup = plan.Groups.FirstOrDefault();

            Smithbox.Log(typeof(TextMerge), LOC.Get("TEXT_TextMerge_Log_Analysed",
                plan.SourceName,
                plan.Count(MergeItemKind.Add), plan.Count(MergeItemKind.Change),
                plan.Count(MergeItemKind.Conflict), plan.Count(MergeItemKind.Removed)));

            if (plan.Unmatched.Count > 0)
            {
                Smithbox.LogWarning<TextMerge>(LOC.Get("TEXT_TextMerge_Log_Unmatched",
                    plan.Unmatched.Count, string.Join(", ", plan.Unmatched)));
            }
        });
    }

    /// <summary>
    /// Read a mod folder (as the game's folder layout: msg/engus/item.msgbnd.dcx etc.) into a bank.
    /// The chosen folder may be the mod root or a folder inside it (msg, or a language folder):
    /// the first of the folder, its parent and its grandparent that holds any of the game's text
    /// files is used. Only text files present in the folder are read.
    /// </summary>
    private static TextBank CreateFolderBank(TextEditorView view, string folderPath)
    {
        var gameTextFiles = view.Project.Locator?.TextFiles?.Entries;

        if (gameTextFiles == null || gameTextFiles.Count == 0)
            return null;

        var candidate = new DirectoryInfo(folderPath);

        for (int depth = 0; depth < 3 && candidate != null; depth++, candidate = candidate.Parent)
        {
            var fs = new RealVirtualFileSystem(candidate.FullName, true);

            var present = new FileDictionary();
            foreach (var entry in gameTextFiles)
            {
                if (fs.FileExists(entry.Path))
                    present.Entries.Add(entry);
            }

            if (present.Entries.Count == 0)
                continue;

            if (depth > 0)
            {
                Smithbox.Log(typeof(TextMerge), LOC.Get("TEXT_TextMerge_Log_Folder_Root", candidate.FullName));
            }

            var bank = new TextBank(candidate.Name, view.Project, fs, present);
            bank.Setup().GetAwaiter().GetResult();

            return bank;
        }

        Smithbox.LogError<TextMerge>(LOC.Get("TEXT_TextMerge_Log_Folder_No_Text", folderPath));
        return null;
    }

    #region Sources

    /// <summary>
    /// Where the other side of the merge comes from. Asked for the FMG that matches each of this
    /// project's FMGs (on the worker thread).
    /// </summary>
    private abstract class MergeSource
    {
        public string Name;

        /// <summary>
        /// Whether entries missing from the source mean "deleted by them". False for a JSON export,
        /// which can hold only some entries of an FMG.
        /// </summary>
        public bool ReportsRemovals = true;

        /// <summary>
        /// Whether the Primary Language Only option applies. A JSON export names its own container.
        /// </summary>
        public bool UsesLanguageOption = true;

        /// <summary>
        /// Cheap check, before this project's container is loaded, that the source can have it.
        /// </summary>
        public abstract bool MayMatch(TextContainerWrapper myContainer);

        public abstract FMG FindFmg(TextContainerWrapper myContainer, TextFmgWrapper myWrapper);

        /// <summary>
        /// Files and FMGs the source has that match nothing in this project (after the plan is built).
        /// </summary>
        public abstract List<string> CollectUnmatched(TextBank mine, Func<TextContainerWrapper, bool> languageOk);
    }

    /// <summary>
    /// Another project, or a mod folder, read into a <see cref="TextBank"/>.
    /// </summary>
    private sealed class BankMergeSource : MergeSource
    {
        private readonly TextBank Bank;

        public BankMergeSource(string name, TextBank bank)
        {
            Name = name;
            Bank = bank;
        }

        public override bool MayMatch(TextContainerWrapper myContainer) =>
            FindMatchingContainer(Bank, myContainer) != null;

        public override FMG FindFmg(TextContainerWrapper myContainer, TextFmgWrapper myWrapper)
        {
            var container = FindMatchingContainer(Bank, myContainer);

            if (container == null)
                return null;

            // Load from the source's own bank (this used the primary bank, which read this
            // project's files, so merging a non-primary language merged the project with itself)
            if (container.FmgWrappers == null || container.FmgWrappers.Count == 0)
            {
                Bank.LoadFmgWrappers(container);
            }

            return container.FmgWrappers?.FirstOrDefault(e => e.ID == myWrapper.ID)?.File;
        }

        public override List<string> CollectUnmatched(TextBank mine, Func<TextContainerWrapper, bool> languageOk)
        {
            var result = new List<string>();

            var theirs = Bank.Containers.Values
                .Where(languageOk)
                .OrderBy(e => e.ContainerDisplayCategory)
                .ThenBy(e => e.FileEntry.Filename);

            foreach (var their in theirs)
            {
                var myContainer = FindMatchingContainer(mine, their);

                if (myContainer == null)
                {
                    result.Add($"{their.ContainerDisplayCategory} - {their.FileEntry.Filename}");
                    continue;
                }

                // Only containers the plan read (both sides loaded) can be compared per FMG
                if (their.FmgWrappers == null || myContainer.FmgWrappers == null || myContainer.FmgWrappers.Count == 0)
                    continue;

                foreach (var wrapper in their.FmgWrappers)
                {
                    if (!myContainer.FmgWrappers.Any(e => e.ID == wrapper.ID))
                    {
                        result.Add($"{their.ContainerDisplayCategory} - {their.FileEntry.Filename} - {wrapper.Name} ({wrapper.ID})");
                    }
                }
            }

            return result;
        }
    }

    /// <summary>
    /// A text export (Data ▸ Export, one container). Matched by the container and language the
    /// export records, or the selected container for older exports that don't record it; FMGs
    /// match by binder ID and name, as in the importer.
    /// </summary>
    private sealed class JsonMergeSource : MergeSource
    {
        private StoredFmgContainer Stored;
        private TextContainerWrapper FallbackContainer;

        public static JsonMergeSource Load(string path, TextContainerWrapper selectedContainer)
        {
            StoredFmgContainer stored;

            using (var stream = File.OpenRead(path))
            {
                stored = JsonSerializer.Deserialize(stream,
                    StoredContainerWrapperSerializationContext.Default.StoredFmgContainer);
            }

            if (stored?.FmgWrappers == null)
            {
                Smithbox.LogError<TextMerge>(LOC.Get("TEXT_TextMerge_Log_Json_Invalid", path));
                return null;
            }

            var hasSource = !string.IsNullOrEmpty(stored.SourceContainer);

            if (!hasSource)
            {
                if (selectedContainer == null)
                {
                    Smithbox.LogError<TextMerge>(LOC.Get("TEXT_TextMerge_Log_Json_No_Container", path));
                    return null;
                }

                Smithbox.LogWarning<TextMerge>(LOC.Get("TEXT_TextMerge_Log_Json_Selected_Container",
                    selectedContainer.FileEntry.Filename));
            }

            return new JsonMergeSource
            {
                Name = Path.GetFileName(path),
                Stored = stored,
                FallbackContainer = hasSource ? null : selectedContainer,
                ReportsRemovals = false,
                UsesLanguageOption = false
            };
        }

        public override bool MayMatch(TextContainerWrapper myContainer)
        {
            if (FallbackContainer != null)
                return myContainer == FallbackContainer;

            if (myContainer.FileEntry.Filename != Stored.SourceContainer)
                return false;

            // Exports with a container but no language match any language
            return string.IsNullOrEmpty(Stored.SourceCategory) ||
                myContainer.ContainerDisplayCategory.ToString() == Stored.SourceCategory;
        }

        public override FMG FindFmg(TextContainerWrapper myContainer, TextFmgWrapper myWrapper)
        {
            if (!MayMatch(myContainer))
                return null;

            var stored = Stored.FmgWrappers.FirstOrDefault(e =>
                e?.Fmg?.Entries != null &&
                e.ID == myWrapper.ID &&
                (string.IsNullOrEmpty(e.Name) || e.Name == myWrapper.Name));

            if (stored != null)
                Matched.Add(stored);

            return stored?.Fmg;
        }

        private readonly HashSet<StoredFmgWrapper> Matched = new();

        public override List<string> CollectUnmatched(TextBank mine, Func<TextContainerWrapper, bool> languageOk)
        {
            var container = FallbackContainer?.FileEntry.Filename ?? Stored.SourceContainer;

            return Stored.FmgWrappers
                .Where(e => e?.Fmg?.Entries != null && !Matched.Contains(e))
                .Select(e => $"{container} - {e.Name} ({e.ID})")
                .ToList();
        }
    }

    #endregion

    private enum MergeItemKind
    {
        Add,
        Change,
        Conflict,
        Removed
    }

    private enum MergeChoice
    {
        Unresolved,
        Mine,
        Theirs
    }

    private enum MergeFilter
    {
        All,
        Added,
        Updated,
        Conflicts,
        UnresolvedConflicts,
        Removed
    }

    private sealed class MergeItem
    {
        public MergeItemKind Kind;
        public MergeFmgGroup Group;
        public FMG.Entry MyEntry;      // null for Add (and for a conflict on an entry I deleted)
        public FMG.Entry TheirEntry;   // null for Removed
        public string VanillaText;
        public bool HasVanilla;
        public int ID;

        // Add / Change / Removed
        public bool Included = true;

        // Conflict
        public MergeChoice Choice = MergeChoice.Unresolved;

        public bool IsUnresolved => Kind == MergeItemKind.Conflict && Choice == MergeChoice.Unresolved;
    }

    private sealed class MergeFmgGroup
    {
        public TextContainerWrapper Container;
        public TextFmgWrapper Wrapper;
        public List<MergeItem> Items = new();
        public bool Included = true;

        public FMG MyFmg => Wrapper.File;

        public int Count(MergeItemKind kind) => Items.Count(e => e.Kind == kind);
    }

    private sealed class MergePlan
    {
        public string SourceName;
        public List<MergeFmgGroup> Groups = new();
        public int UnchangedByThem;    // they left it vanilla (or matched mine): my text kept
        public int FmgsWithoutVanilla;
        public List<string> Unmatched = new(); // files / FMGs only the source has (not merged)

        public int Count(MergeItemKind kind) => Groups.Sum(e => e.Count(kind));

        // Only conflicts that would be applied block Apply
        public int UnresolvedCount => Groups
            .Where(e => e.Included)
            .Sum(e => e.Items.Count(i => i.IsUnresolved));

        public bool CanApply => Groups.Count > 0 && UnresolvedCount == 0;
    }

    /// <summary>
    /// Compare each FMG of this project with the same FMG in the other project and in vanilla.
    /// Reads only; nothing is changed here.
    /// </summary>
    private MergePlan BuildPlan(TextEditorView view, MergeSource source,
        HashSet<TextContainerCategory> languages, TextMergeConflictMode mode)
    {
        bool LanguageOk(TextContainerWrapper container) =>
            !source.UsesLanguageOption || languages.Contains(container.ContainerDisplayCategory);

        var textData = view.Project.Handler.TextData;

        var plan = new MergePlan { SourceName = source.Name };

        var defaultChoice = mode switch
        {
            TextMergeConflictMode.KeepMine => MergeChoice.Mine,
            TextMergeConflictMode.TakeTheirs => MergeChoice.Theirs,
            _ => MergeChoice.Unresolved
        };

        // Same order as the container list, so the plan reads like the editor
        var containers = textData.PrimaryBank.Containers.Values
            .OrderBy(e => e.ContainerDisplayCategory)
            .ThenBy(e => e.FileEntry.Filename)
            .ToList();

        foreach (var currentContainer in containers)
        {
            if (!LanguageOk(currentContainer))
                continue;

            // Checked before loading, so containers the source doesn't have aren't read
            if (!source.MayMatch(currentContainer))
                continue;

            if (currentContainer.FmgWrappers == null || currentContainer.FmgWrappers.Count == 0)
            {
                textData.PrimaryBank.LoadFmgWrappers(currentContainer);
            }

            var vanillaContainer = FindMatchingContainer(textData.VanillaBank, currentContainer);

            if (vanillaContainer != null &&
                (vanillaContainer.FmgWrappers == null || vanillaContainer.FmgWrappers.Count == 0))
            {
                textData.VanillaBank.LoadFmgWrappers(vanillaContainer);
            }

            if (currentContainer.FmgWrappers == null)
                continue;

            foreach (var curWrapper in currentContainer.FmgWrappers)
            {
                if (curWrapper.File == null)
                    continue;

                var theirFmg = source.FindFmg(currentContainer, curWrapper);

                if (theirFmg?.Entries == null)
                    continue;

                var vanWrapper = vanillaContainer?.FmgWrappers?.FirstOrDefault(e => e.ID == curWrapper.ID);

                if (vanWrapper?.File == null)
                    plan.FmgsWithoutVanilla++;

                var group = new MergeFmgGroup { Container = currentContainer, Wrapper = curWrapper };

                PlanFmg(plan, group, theirFmg, vanWrapper?.File, defaultChoice, source.ReportsRemovals);

                if (group.Items.Count > 0)
                    plan.Groups.Add(group);
            }
        }

        // Files and FMGs only the source has can't be merged: saving writes the FMGs already in each
        // binder, and the games load FMGs by fixed IDs. They are listed so nothing is dropped silently.
        plan.Unmatched = source.CollectUnmatched(textData.PrimaryBank, LanguageOk);

        return plan;
    }

    private static TextContainerWrapper FindMatchingContainer(TextBank bank, TextContainerWrapper container)
    {
        if (bank?.Containers == null)
            return null;

        // Sub-category separates DS2's loose FMGs of the same name; it is the same for all other games
        return bank.Containers.Values.FirstOrDefault(e =>
            e.FileEntry.Filename == container.FileEntry.Filename &&
            e.ContainerDisplayCategory == container.ContainerDisplayCategory &&
            e.ContainerDisplaySubCategory == container.ContainerDisplaySubCategory);
    }

    private static void PlanFmg(MergePlan plan, MergeFmgGroup group, FMG theirs, FMG vanilla,
        MergeChoice defaultChoice, bool reportRemovals)
    {
        // First entry wins for duplicate IDs, on every side
        var mineById = FirstById(group.MyFmg);
        var theirsById = FirstById(theirs);
        var vanillaById = vanilla != null ? FirstById(vanilla) : new Dictionary<int, FMG.Entry>();

        foreach (var (id, theirEntry) in theirsById)
        {
            var theirText = Norm(theirEntry.Text);
            var hasVanilla = vanillaById.TryGetValue(id, out var vanillaEntry);
            var vanillaText = hasVanilla ? Norm(vanillaEntry.Text) : null;

            MergeItem NewItem(MergeItemKind kind, FMG.Entry myEntry) => new MergeItem
            {
                Kind = kind,
                Group = group,
                MyEntry = myEntry,
                TheirEntry = theirEntry,
                VanillaText = vanillaEntry?.Text,
                HasVanilla = hasVanilla,
                ID = id,
                Choice = kind == MergeItemKind.Conflict ? defaultChoice : MergeChoice.Unresolved
            };

            if (!mineById.TryGetValue(id, out var myEntry))
            {
                // I deleted a vanilla entry that they left alone: keep my deletion
                if (hasVanilla && theirText == vanillaText)
                {
                    plan.UnchangedByThem++;
                    continue;
                }

                // I deleted it and they changed it: both changed it
                group.Items.Add(NewItem(hasVanilla ? MergeItemKind.Conflict : MergeItemKind.Add, null));
                continue;
            }

            var myText = Norm(myEntry.Text);

            if (myText == theirText)
                continue;

            // They didn't touch it: keep mine (this used to revert my edits to vanilla)
            if (hasVanilla && theirText == vanillaText)
            {
                plan.UnchangedByThem++;
                continue;
            }

            // Only they changed it, or both changed it (or both added the ID)
            var itemKind = hasVanilla && myText == vanillaText ? MergeItemKind.Change : MergeItemKind.Conflict;
            group.Items.Add(NewItem(itemKind, myEntry));
        }

        // They deleted a vanilla entry that I still have. Listed for information, off by default:
        // a mod rarely means to delete vanilla text, and removing it can break references.
        if (vanilla != null && reportRemovals)
        {
            foreach (var (id, vanillaEntry) in vanillaById)
            {
                if (theirsById.ContainsKey(id) || !mineById.TryGetValue(id, out var myEntry))
                    continue;

                group.Items.Add(new MergeItem
                {
                    Kind = MergeItemKind.Removed,
                    Group = group,
                    MyEntry = myEntry,
                    VanillaText = vanillaEntry.Text,
                    HasVanilla = true,
                    ID = id,
                    Included = false
                });
            }
        }

        group.Items.Sort((a, b) => a.ID.CompareTo(b.ID));
    }

    private static Dictionary<int, FMG.Entry> FirstById(FMG fmg)
    {
        var result = new Dictionary<int, FMG.Entry>();

        foreach (var entry in fmg.Entries)
        {
            result.TryAdd(entry.ID, entry);
        }

        return result;
    }

    // Null text is an empty entry in the game, so treat it as equal to ""
    private static string Norm(string text) => text ?? "";

    #endregion

    #region Apply

    private void ApplyCurrentPlan()
    {
        if (currentPlan == null || !currentPlan.CanApply)
            return;

        ApplyPlan(View, currentPlan);
        ClearPlan();
    }

    /// <summary>
    /// Turn the reviewed plan into one grouped action (UI thread). Entries are checked again here,
    /// since the user may have edited the project after the analysis.
    /// </summary>
    private void ApplyPlan(TextEditorView view, MergePlan plan)
    {
        var actions = new List<EditorAction>();
        var touchedFmgs = new HashSet<FMG>();
        var addedEntries = new List<AddedEntry>();

        int added = 0;
        int changed = 0;
        int removed = 0;
        int conflictsTaken = 0;
        int conflictsKept = 0;
        int stale = 0;

        foreach (var group in plan.Groups)
        {
            if (!group.Included)
                continue;

            var fmg = group.MyFmg;

            foreach (var item in group.Items)
            {
                var isConflict = item.Kind == MergeItemKind.Conflict;

                if (isConflict)
                {
                    if (item.Choice != MergeChoice.Theirs)
                    {
                        conflictsKept++;
                        continue;
                    }
                }
                else if (!item.Included)
                {
                    continue;
                }

                if (item.Kind == MergeItemKind.Removed)
                {
                    if (!fmg.Entries.Contains(item.MyEntry))
                    {
                        stale++;
                        continue;
                    }

                    actions.Add(new DeleteFmgEntry(view, group.Container, fmg, item.MyEntry));
                    removed++;
                }
                else if (item.MyEntry == null)
                {
                    // Added since the analysis
                    if (fmg.Entries.Any(e => e.ID == item.ID))
                    {
                        stale++;
                        continue;
                    }

                    var newEntry = item.TheirEntry.Clone();
                    newEntry.Parent = fmg;

                    actions.Add(new AddFmgEntry(view, group.Container, newEntry, newEntry, item.ID));
                    addedEntries.Add(new AddedEntry(group.Container, group.Wrapper.ID, fmg, newEntry));

                    if (isConflict) conflictsTaken++; else added++;
                }
                else
                {
                    // Deleted, or already the same, since the analysis
                    if (!fmg.Entries.Contains(item.MyEntry) ||
                        Norm(item.MyEntry.Text) == Norm(item.TheirEntry.Text))
                    {
                        stale++;
                        continue;
                    }

                    actions.Add(new ChangeFmgEntryText(view, group.Container, item.MyEntry, item.TheirEntry.Text));

                    if (isConflict) conflictsTaken++; else changed++;
                }

                touchedFmgs.Add(fmg);
            }
        }

        if (actions.Count > 0)
        {
            view.ActionManager.ExecuteAction(new FmgGroupedAction(actions));
        }

        Smithbox.Log(typeof(TextMerge),
            LOC.Get("TEXT_TextMerge_Log_Merged_Text_PASS", plan.SourceName));

        Smithbox.Log(typeof(TextMerge),
            LOC.Get("TEXT_TextMerge_Log_Summary", added, changed, conflictsTaken, conflictsKept,
                removed, plan.UnchangedByThem, touchedFmgs.Count));

        if (stale > 0)
        {
            Smithbox.LogWarning(typeof(TextMerge), LOC.Get("TEXT_TextMerge_Log_Stale", stale));
        }

        // Offer to give the other languages the new entries (most mods ship one language)
        var otherLanguageCount = CountOtherLanguageGaps(addedEntries);
        pendingFill = otherLanguageCount > 0 ? addedEntries : null;
        pendingFillCount = otherLanguageCount;
    }

    #endregion

    #region Fill Other Languages

    private sealed record AddedEntry(TextContainerWrapper Container, int FmgId, FMG Fmg, FMG.Entry Entry);

    // Entries the last Apply added, while the offer to add them to the other languages is shown
    private List<AddedEntry> pendingFill = null;
    private int pendingFillCount = 0;

    private void DisplayFillOffer()
    {
        if (pendingFill == null)
            return;

        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("TEXT_TextMerge_Header_Fill"),
            LOC.Get("TEXT_TextMerge_Header_Fill_TT"));

        GUI.WrappedText(LOC.Get("TEXT_TextMerge_Fill_Hint", pendingFill.Count, pendingFillCount));

        GUI.MultiButtonInput("mergeFillActions",
            "fillOtherLanguages",
            LOC.Get("TEXT_TextMerge_Action_Fill"),
            LOC.Get("TEXT_TextMerge_Action_Fill_TT"),
            FillOtherLanguages,

            "dismissFill",
            LOC.Get("TEXT_TextMerge_Action_Fill_Dismiss"),
            LOC.Get("TEXT_TextMerge_Action_Fill_Dismiss_TT"),
            () => pendingFill = null);
    }

    /// <summary>
    /// The same container in this project's other languages (same filename and sub-category).
    /// </summary>
    private IEnumerable<TextContainerWrapper> GetOtherLanguageContainers(TextContainerWrapper container)
    {
        return View.Project.Handler.TextData.PrimaryBank.Containers.Values.Where(e =>
            e != container &&
            e.ContainerDisplayCategory != container.ContainerDisplayCategory &&
            e.FileEntry.Filename == container.FileEntry.Filename &&
            e.ContainerDisplaySubCategory == container.ContainerDisplaySubCategory);
    }

    /// <summary>
    /// Visit each (added entry, other-language FMG that lacks its ID). Loads the other languages'
    /// FMGs as needed. Entries that were undone since the merge are skipped.
    /// </summary>
    private void ForEachOtherLanguageGap(List<AddedEntry> entries, Action<AddedEntry, TextContainerWrapper, FMG> visit)
    {
        var bank = View.Project.Handler.TextData.PrimaryBank;
        var idsByFmg = new Dictionary<FMG, HashSet<int>>();

        foreach (var added in entries)
        {
            if (!added.Fmg.Entries.Contains(added.Entry))
                continue;

            foreach (var other in GetOtherLanguageContainers(added.Container))
            {
                if (other.FmgWrappers == null || other.FmgWrappers.Count == 0)
                {
                    bank.LoadFmgWrappers(other);
                }

                var otherFmg = other.FmgWrappers?.FirstOrDefault(e => e.ID == added.FmgId)?.File;

                if (otherFmg == null)
                    continue;

                if (!idsByFmg.TryGetValue(otherFmg, out var ids))
                {
                    ids = otherFmg.Entries.Select(e => e.ID).ToHashSet();
                    idsByFmg[otherFmg] = ids;
                }

                // Added to ids so the same ID isn't added twice (duplicate IDs in the merge)
                if (ids.Add(added.Entry.ID))
                {
                    visit(added, other, otherFmg);
                }
            }
        }
    }

    private int CountOtherLanguageGaps(List<AddedEntry> entries)
    {
        if (entries.Count == 0)
            return 0;

        var count = 0;
        ForEachOtherLanguageGap(entries, (_, _, _) => count++);
        return count;
    }

    /// <summary>
    /// Add each merged entry to the same FMG in every other language that lacks it, with the
    /// merged text (and the Language Sync prefix when that option is on). One undo step.
    /// Only additions: updated entries keep their existing translations.
    /// </summary>
    private void FillOtherLanguages()
    {
        if (pendingFill == null)
            return;

        var actions = new List<EditorAction>();
        var settings = CFG.Current.TextEditor;

        ForEachOtherLanguageGap(pendingFill, (added, otherContainer, otherFmg) =>
        {
            var text = added.Entry.Text;

            if (settings.TextEditor_Language_Sync_Apply_Prefix && !string.IsNullOrEmpty(text))
            {
                text = $"{settings.TextEditor_Language_Sync_Prefix}{text}";
            }

            var newEntry = new FMG.Entry(otherFmg, added.Entry.ID, text);
            actions.Add(new AddFmgEntry(View, otherContainer, newEntry, newEntry, added.Entry.ID));
        });

        if (actions.Count > 0)
        {
            View.ActionManager.ExecuteAction(new FmgGroupedAction(actions));
        }

        Smithbox.Log(typeof(TextMerge), LOC.Get("TEXT_TextMerge_Log_Filled", actions.Count));

        pendingFill = null;
    }

    #endregion
}
