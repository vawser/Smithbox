using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using static StudioCore.Editors.HavokEditor.HavokTemplatePlanner;

namespace StudioCore.Editors.HavokEditor;

/// <summary>
/// Templates (Behavior Editor Tools window): a wizard that applies a gameplay pattern (an Ash of War, a weapon moveset,
/// an idle category) to the shown behavior file as one undo step. Templates are JSON files, built-in and per project
/// (Claude/Documentation/Behavior.md (Havok behavior edits)). Several inputs, so applied with the Apply button only.
/// </summary>
public class HavokBehaviorTemplatesTool
{
    private readonly HavokEditorView View;
    private readonly ProjectEntry Project;

    private HavokBehaviorView Behavior => View.PropertyView.BehaviorView;

    private List<HavokBehaviorTemplate> Templates;
    private List<string> LoadErrors = new();
    private int TemplateIndex = 0;

    // Inputs, reset when another template is picked
    private HavokBehaviorTemplate InputsFor;
    private int Category;
    private int Reference;
    private bool FromReference;
    private bool[] GroupTicks = Array.Empty<bool>();

    private bool ShowRegistered = false;
    private bool ShowNoSelector = true;

    // The plan, rebuilt when the inputs or the file's symbols change
    private string PlanKey;
    private HavokBehaviorSymbols PlanSymbols;
    private List<Row> Rows = new();
    private int CategoryInUse;
    private int ReferenceInUse;

    private string LastResult;

    public HavokBehaviorTemplatesTool(HavokEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    private int CategorySize => Project.Descriptor.ProjectType is ProjectType.DES or ProjectType.DS1 or ProjectType.DS1R ? 10000 : 1000000;

    private int MaxCategory => CategorySize == 10000 ? 99 : 999;

    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("HAVOK_Templates_Header")}##havokBehaviorTemplatesToolHeader"))
            return;

        ImGui.BeginChild("HavokBehaviorTemplatesToolSection", ImGuiChildFlags.Borders);

        Display();

        ImGui.EndChild();
    }

    private void Reload()
    {
        Templates = HavokBehaviorTemplates.Load(Project, out LoadErrors);

        foreach (var error in LoadErrors)
            Smithbox.LogError(this, error);

        TemplateIndex = Math.Clamp(TemplateIndex, 0, Math.Max(0, Templates.Count - 1));
        InputsFor = null;
        PlanKey = null;
    }

    private void Display()
    {
        if (Templates == null)
            Reload();

        GUI.WrappedText(LOC.Get("HAVOK_Templates_Hint"));

        // Template
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("HAVOK_Templates_Template_Header"),
            LOC.Get("HAVOK_Templates_Template_Header_TT", HavokBehaviorTemplates.BuiltInFolder(Project), HavokBehaviorTemplates.ProjectFolder(Project)));

        foreach (var error in LoadErrors)
            GUI.WrappedTextColored(UI.Current.ImGui_Invalid_Text_Color, error);

        if (Templates.Count == 0)
        {
            GUI.WrappedText(LOC.Get("HAVOK_Templates_None"));
            ReloadButton();
            return;
        }

        var template = Templates[TemplateIndex];

        ImGui.SetNextItemWidth(-1);

        if (ImGui.BeginCombo("##havokTemplatePick", template.Name))
        {
            for (var i = 0; i < Templates.Count; i++)
            {
                var label = Templates[i].FromProject ? $"{Templates[i].Name}  {LOC.Get("HAVOK_Templates_Project_Mark")}" : Templates[i].Name;

                if (ImGui.Selectable($"{label}##havokTemplate{i}", i == TemplateIndex))
                {
                    TemplateIndex = i;
                    LastResult = null;
                }

                GUI.Tooltip(Templates[i].Description);
            }

            ImGui.EndCombo();
        }

        template = Templates[TemplateIndex];

        ResetInputsFor(template);

        if (!string.IsNullOrWhiteSpace(template.Description))
            GUI.WrappedText(template.Description);

        ReloadButton();

        if (!Behavior.IsShown)
        {
            GUI.Spacer();
            GUI.WrappedText(LOC.Get("HAVOK_Templates_Needs_File"));
            return;
        }

        var shownFile = Path.GetFileName((View.Selection.FilePath ?? "").Replace('\\', '/'));

        if (!string.IsNullOrEmpty(template.File) && !shownFile.StartsWith(Path.GetFileNameWithoutExtension(template.File), StringComparison.OrdinalIgnoreCase))
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("HAVOK_Templates_Other_File", template.File, shownFile));

        if (template.IsState)
            DisplayState(template);
        else
            DisplayCategory(template);
    }

    private void DisplayCategory(HavokBehaviorTemplate template)
    {
        // Inputs
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("HAVOK_Templates_Inputs_Header"), LOC.Get("HAVOK_Templates_Inputs_Header_TT"));

        DisplayInputs(template);

        UpdatePlan(template);

        // Groups
        if (!FromReference && template.Groups.Count > 0)
        {
            GUI.Spacer();
            GUI.SimpleHeader(LOC.Get("HAVOK_Templates_Groups_Header"), LOC.Get("HAVOK_Templates_Groups_Header_TT"));

            for (var i = 0; i < template.Groups.Count; i++)
            {
                var group = template.Groups[i];

                ImGui.Checkbox($"{group.Name} ({group.Ids.Count})##havokTemplateGroup{i}", ref GroupTicks[i]);

                var ids = string.Join(", ", group.Ids.Take(24)) + (group.Ids.Count > 24 ? ", ..." : "");
                GUI.Tooltip(string.IsNullOrWhiteSpace(group.Description) ? ids : $"{group.Description}\n\n{ids}");
            }
        }

        // Preview
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("HAVOK_Templates_Preview_Header"), LOC.Get("HAVOK_Templates_Preview_Header_TT"));

        DisplayPreview();

        DisplayNotes(template);

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("HAVOK_Templates_Actions_Header"), LOC.Get("HAVOK_Templates_Actions_Header_TT"));

        var adds = CountAdds(Rows);

        GUI.ConditionalMultiButtonInput("havokTemplateActions",
            "havokTemplateApply", LOC.Get("HAVOK_Templates_Apply", adds), LOC.Get("HAVOK_Templates_Apply_TT"),
            () => Apply(template), adds > 0);

        if (LastResult != null)
            GUI.WrappedText(LastResult);
    }

    private void DisplayNotes(HavokBehaviorTemplate template)
    {
        if (template.Notes.Count == 0)
            return;

        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("HAVOK_Templates_Notes_Header"), LOC.Get("HAVOK_Templates_Notes_Header_TT"));

        foreach (var note in template.Notes)
            GUI.WrappedText($"- {Fill(note)}");
    }

    private void ReloadButton()
    {
        if (ImGui.SmallButton($"{LOC.Get("HAVOK_Templates_Reload")}##havokTemplateReload"))
            Reload();

        GUI.Tooltip(LOC.Get("HAVOK_Templates_Reload_TT"));
    }

    private void ResetInputsFor(HavokBehaviorTemplate template)
    {
        if (ReferenceEquals(InputsFor, template))
            return;

        InputsFor = template;
        FirstId = template.Id.Default;
        StateCount = template.Count.Default;
        StateStep = template.Step.Default;
        CandidateIndex = -1;
        StatePlanKey = null;
        StatePlanSymbols = null;
        Category = Math.Clamp(template.Category.Default, 0, MaxCategory);
        Reference = template.Reference.Default;
        FromReference = template.Reference.Required || template.Groups.Count == 0;
        GroupTicks = template.Groups.Select(g => g.Default).ToArray();
        PlanKey = null;
    }

    private void DisplayInputs(HavokBehaviorTemplate template)
    {
        var min = Math.Clamp(template.Category.Min, 0, MaxCategory);
        var max = Math.Clamp(template.Category.Max, min, MaxCategory);

        // New category: only changes the preview, nothing is applied before Apply
        ImGui.SetNextItemWidth(160 * DPI.UIScale());
        if (ImGui.InputInt($"{LOC.Get("HAVOK_Templates_Category")}##havokTemplateCategory", ref Category))
            Category = Math.Clamp(Category, min, max);
        GUI.Tooltip(LOC.Get("HAVOK_Templates_Category_TT", min, max));

        if (CategoryInUse > 0)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("HAVOK_Templates_Category_In_Use", Format(Category), CategoryInUse));

        // Reference category
        ImGui.SetNextItemWidth(160 * DPI.UIScale());
        if (ImGui.InputInt($"{LOC.Get("HAVOK_Templates_Reference")}##havokTemplateReference", ref Reference))
            Reference = Math.Clamp(Reference, template.Reference.Required ? 0 : -1, MaxCategory);
        GUI.Tooltip(LOC.Get(template.Reference.Required ? "HAVOK_Templates_Reference_Required_TT" : "HAVOK_Templates_Reference_TT"));

        if (Reference >= 0 && ReferenceInUse == 0)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("HAVOK_Templates_Reference_Unused", Format(Reference)));
        else if (Reference >= 0 && Reference == Category)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("HAVOK_Templates_Reference_Same"));

        // Source of the IDs: only templates with groups can choose
        if (!template.Reference.Required && template.Groups.Count > 0)
        {
            if (ImGui.RadioButton($"{LOC.Get("HAVOK_Templates_Source_Groups")}##havokTemplateSourceGroups", !FromReference))
                FromReference = false;
            GUI.Tooltip(LOC.Get("HAVOK_Templates_Source_Groups_TT"));

            ImGui.SameLine();

            ImGui.BeginDisabled(Reference < 0);
            if (ImGui.RadioButton($"{LOC.Get("HAVOK_Templates_Source_Reference")}##havokTemplateSourceReference", FromReference))
                FromReference = true;
            ImGui.EndDisabled();

            if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                ImGui.SetTooltip(LOC.Get("HAVOK_Templates_Source_Reference_TT"));

            if (Reference < 0)
                FromReference = false;
        }
    }

    #region State templates
    // Inputs
    private int FirstId;
    private int StateCount;
    private int StateStep;

    /// <summary>The picked template state, -1 for the default</summary>
    private int CandidateIndex = -1;

    // The plan, rebuilt when the inputs or the file's symbols change
    private string StatePlanKey;
    private HavokBehaviorSymbols StatePlanSymbols;
    private object StateMachine;
    private List<HavokStateTemplatePlanner.Candidate> Candidates = new();
    private List<HavokStateTemplatePlanner.Row> StateRows = new();

    private HavokStateTemplatePlanner.Candidate Candidate =>
        CandidateIndex >= 0 && CandidateIndex < Candidates.Count ? Candidates[CandidateIndex] : null;

    private void DisplayState(HavokBehaviorTemplate template)
    {
        UpdateStatePlan(template);

        if (StateMachine == null)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Invalid_Text_Color, LOC.Get("HAVOK_Templates_No_Machine", template.StateMachine));
            return;
        }

        if (Candidates.Count == 0)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Invalid_Text_Color, LOC.Get("HAVOK_Templates_No_Template_State", template.StateMachine, template.TemplateMatch));
            return;
        }

        // Inputs
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("HAVOK_Templates_Inputs_Header"), LOC.Get("HAVOK_Templates_State_Inputs_Header_TT"));

        var width = 160 * DPI.UIScale();

        // Only change the preview: nothing is applied before Apply
        ImGui.SetNextItemWidth(width);
        if (ImGui.InputInt($"{LOC.Get("HAVOK_Templates_First_Id")}##havokTemplateFirstId", ref FirstId))
            FirstId = Math.Clamp(FirstId, template.Id.Min, template.Id.Max);
        GUI.Tooltip(LOC.Get("HAVOK_Templates_First_Id_TT", template.Id.Min, template.Id.Max));

        ImGui.SetNextItemWidth(width);
        if (ImGui.InputInt($"{LOC.Get("HAVOK_Templates_Count")}##havokTemplateCount", ref StateCount))
            StateCount = Math.Clamp(StateCount, template.Count.Min, template.Count.Max);
        GUI.Tooltip(LOC.Get("HAVOK_Templates_Count_TT"));

        if (StateCount > 1)
        {
            ImGui.SetNextItemWidth(width);
            if (ImGui.InputInt($"{LOC.Get("HAVOK_Templates_Step")}##havokTemplateStep", ref StateStep))
                StateStep = Math.Clamp(StateStep, template.Step.Min, template.Step.Max);
            GUI.Tooltip(LOC.Get("HAVOK_Templates_Step_TT"));
        }

        ImGui.SetNextItemWidth(width);
        if (ImGui.InputInt($"{LOC.Get("HAVOK_Templates_Clip_Category")}##havokTemplateClipCategory", ref Category))
            Category = Math.Clamp(Category, Math.Clamp(template.Category.Min, 0, MaxCategory), Math.Clamp(template.Category.Max, 0, MaxCategory));
        GUI.Tooltip(LOC.Get("HAVOK_Templates_Clip_Category_TT"));

        // Template state
        var candidate = Candidate;

        ImGui.SetNextItemWidth(-1);
        if (ImGui.BeginCombo("##havokTemplateState", LOC.Get("HAVOK_Templates_Template_State", candidate.Name), ImGuiComboFlags.HeightLarge))
        {
            for (var i = 0; i < Candidates.Count; i++)
            {
                var c = Candidates[i];
                if (ImGui.Selectable($"{c.Name}  ({string.Join(", ", c.Categories.Select(x => Format((int)x)))})##havokTemplateCandidate{i}", i == CandidateIndex))
                    CandidateIndex = i;
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("HAVOK_Templates_Template_State_TT", string.Join(", ", candidate.Events)));

        if (!candidate.Categories.Contains((long)Category))
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("HAVOK_Templates_Category_Moved", Format(Category), candidate.Name));

        // Preview
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("HAVOK_Templates_Preview_Header"), LOC.Get("HAVOK_Templates_State_Preview_Header_TT"));

        DisplayStatePreview();

        DisplayNotes(template);

        // Script
        var snippet = Snippet(template);

        if (snippet.Length > 0)
        {
            GUI.Spacer();
            GUI.SimpleHeader(LOC.Get("HAVOK_Templates_Snippet_Header"), LOC.Get("HAVOK_Templates_Snippet_Header_TT"));

            if (ImGui.SmallButton($"{LOC.Get("HAVOK_Templates_Copy_Snippet")}##havokTemplateCopySnippet"))
                ImGui.SetClipboardText(snippet);
            GUI.Tooltip(LOC.Get("HAVOK_Templates_Copy_Snippet_TT"));

            var lines = Math.Min(snippet.Count(ch => ch == '\n') + 1, 12);
            ImGui.InputTextMultiline("##havokTemplateSnippet", ref snippet, (uint)snippet.Length + 1,
                new System.Numerics.Vector2(-1, lines * ImGui.GetTextLineHeight() + 8 * DPI.UIScale()), ImGuiInputTextFlags.ReadOnly);
        }

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("HAVOK_Templates_Actions_Header"), LOC.Get("HAVOK_Templates_Actions_Header_TT"));

        var adds = StateRows.Count(r => r.Include && r.Status is HavokStateTemplatePlanner.RowStatus.Add);

        GUI.ConditionalMultiButtonInput("havokTemplateStateActions",
            "havokTemplateStateApply", LOC.Get("HAVOK_Templates_Apply_States", adds), LOC.Get("HAVOK_Templates_Apply_States_TT"),
            () => ApplyState(template), adds > 0);

        if (LastResult != null)
            GUI.WrappedText(LastResult);
    }

    /// <summary>The template's snippet, once per included new state</summary>
    private string Snippet(HavokBehaviorTemplate template)
    {
        if (string.IsNullOrWhiteSpace(template.Snippet))
            return "";

        return string.Join("\n", StateRows
            .Where(r => r.Include && r.Status is HavokStateTemplatePlanner.RowStatus.Add)
            .Select(r => template.Snippet.Replace("{Id}", r.Id.ToString()).Replace("{State}", r.StateName)));
    }

    private void UpdateStatePlan(HavokBehaviorTemplate template)
    {
        var symbols = Behavior.Symbols;

        // The machine and its template states change only with the file
        if (!ReferenceEquals(symbols, StatePlanSymbols))
        {
            var previous = Candidate?.Name;

            StatePlanSymbols = symbols;
            StatePlanKey = null;
            StateMachine = HavokStateTemplatePlanner.FindMachine(symbols, template.StateMachine);
            Candidates = HavokStateTemplatePlanner.FindCandidates(symbols, template, StateMachine);

            // Keep the picked template state across the rebuild an edit makes
            CandidateIndex = previous != null ? Candidates.FindIndex(c => c.Name == previous) : -1;
        }

        if (Candidates.Count == 0)
        {
            StateRows = new();
            return;
        }

        if (CandidateIndex < 0)
            CandidateIndex = HavokStateTemplatePlanner.DefaultCandidate(Candidates, Category);

        var key = $"{TemplateIndex}|{FirstId}|{StateCount}|{StateStep}|{Category}|{CandidateIndex}";

        if (key == StatePlanKey)
            return;

        StatePlanKey = key;
        StateRows = HavokStateTemplatePlanner.Plan(symbols, StateMachine, Candidate, FirstId, StateCount, StateStep, Category, CategorySize);
    }

    private void DisplayStatePreview()
    {
        var add = StateRows.Count(r => r.Status is HavokStateTemplatePlanner.RowStatus.Add);
        var exists = StateRows.Count - add;

        GUI.WrappedText(LOC.Get("HAVOK_Templates_State_Summary", StateRows.Count, add, exists));

        if (StateRows.Count == 0)
            return;

        var scale = DPI.UIScale();
        var flags = ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.Resizable |
            ImGuiTableFlags.ScrollY | ImGuiTableFlags.SizingStretchProp;
        var height = Math.Min(StateRows.Count + 1, 12) * ImGui.GetTextLineHeightWithSpacing() + 8 * scale;

        if (!ImGui.BeginTable("havokTemplateStateTable", 4, flags, new System.Numerics.Vector2(0, height)))
            return;

        ImGui.TableSetupScrollFreeze(0, 1);
        ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed, 24 * scale);
        ImGui.TableSetupColumn(LOC.Get("HAVOK_Templates_Column_State"), ImGuiTableColumnFlags.WidthStretch, 1.2f);
        ImGui.TableSetupColumn(LOC.Get("HAVOK_Templates_Column_Animation"), ImGuiTableColumnFlags.WidthStretch, 1f);
        ImGui.TableSetupColumn(LOC.Get("HAVOK_Templates_Column_Events"), ImGuiTableColumnFlags.WidthStretch, 2f);
        ImGui.TableHeadersRow();

        var animations = Behavior.Animations is { IsReady: true, Error: null } ? Behavior.Animations : null;

        foreach (var row in StateRows)
        {
            ImGui.TableNextRow();

            // Include
            ImGui.TableSetColumnIndex(0);

            if (row.Status is HavokStateTemplatePlanner.RowStatus.Add)
                ImGui.Checkbox($"##havokTemplateStateInclude{row.Id}", ref row.Include);

            // State
            ImGui.TableSetColumnIndex(1);

            if (row.Status is HavokStateTemplatePlanner.RowStatus.Exists)
            {
                ImGui.TextDisabled(row.StateName);
                GUI.Tooltip(LOC.Get("HAVOK_Templates_State_Exists_TT", row.StateName));
            }
            else
            {
                ImGui.Text(row.StateName);

                if (row.IdUsers > 0)
                {
                    ImGui.SameLine();
                    ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, "!");
                    GUI.Tooltip(LOC.Get("HAVOK_Templates_Id_In_Use_TT", row.Id, row.IdUsers));
                }
            }

            // Animation
            ImGui.TableSetColumnIndex(2);
            ImGui.Text(row.AnimationName);

            if (animations != null && row.Status is HavokStateTemplatePlanner.RowStatus.Add && !animations.HasAnimationFile(row.AnimationName))
            {
                ImGui.SameLine();
                ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, "!");
                GUI.Tooltip(LOC.Get("HAVOK_Register_No_Animation_File_TT", row.AnimationName));
            }

            // Events: existing ones are reused
            ImGui.TableSetColumnIndex(3);
            ImGui.TextWrapped(string.Join("  ", row.Events.Select(e => row.ExistingEvents.Contains(e) ? $"={e}" : $"+{e}")));
            GUI.Tooltip(LOC.Get("HAVOK_Templates_Events_TT"));
        }

        ImGui.EndTable();
    }

    private void ApplyState(HavokBehaviorTemplate template)
    {
        var candidate = Candidate;

        if (candidate == null)
            return;

        var snippet = Snippet(template);
        var notes = template.Notes.Select(Fill).ToList();

        var action = HavokStateTemplatePlanner.Apply(Context(), candidate, StateRows, Category, out var added, out var errors);

        foreach (var e in errors.Distinct().Take(10))
            Smithbox.LogError(this, e);

        if (action == null)
        {
            LastResult = errors.FirstOrDefault() ?? LOC.Get("HAVOK_Templates_Nothing");
            return;
        }

        View.ActionManager.ExecuteAction(action);

        LastResult = LOC.Get("HAVOK_Templates_Done_States", template.Name, added, template.StateMachine, errors.Count);
        Smithbox.Log(this, LastResult);

        foreach (var note in notes)
            Smithbox.Log(this, $"{template.Name}: {note}");

        if (snippet.Length > 0)
            Smithbox.Log(this, $"{template.Name}:\n{snippet}");

        StatePlanKey = null;
    }
    #endregion

    private HavokBehaviorContext Context() => new(View.Selection.SelectedFileWrapper, Behavior.Symbols, Project)
    {
        Character = () => HavokBehaviorOperations.FindCharacterAnimations(View.HavokBank, View.Selection.BinderFileEntry,
            View.PropertyCache.GetCachedHavokFields)
    };

    private string Format(int category) => CategorySize == 10000 ? $"a{category:D2}" : $"a{category:D3}";

    /// <summary>A note's {Category}, {Reference} and {Ids} (the new states' IDs), filled in</summary>
    private string Fill(string text) => text
        .Replace("{Category}", Category.ToString())
        .Replace("{Reference}", Reference.ToString())
        .Replace("{Ids}", string.Join(", ", StateRows.Where(r => r.Include).Select(r => r.Id)));

    private void UpdatePlan(HavokBehaviorTemplate template)
    {
        var symbols = Behavior.Symbols;
        var key = $"{TemplateIndex}|{Category}|{Reference}|{FromReference}|{string.Join("", GroupTicks.Select(t => t ? '1' : '0'))}";

        if (key == PlanKey && ReferenceEquals(symbols, PlanSymbols))
            return;

        PlanKey = key;
        PlanSymbols = symbols;

        var inputs = new Inputs
        {
            Category = Category,
            Reference = Reference,
            FromReference = FromReference && Reference >= 0,
        };

        for (var i = 0; i < GroupTicks.Length; i++)
        {
            if (GroupTicks[i])
                inputs.Groups.Add(i);
        }

        Rows = Plan(symbols, template, inputs, CategorySize);
        CategoryInUse = CountCategoryClips(symbols, template, Category);
        ReferenceInUse = Reference >= 0 ? CountCategoryClips(symbols, template, Reference) : 0;
    }

    private void DisplayPreview()
    {
        var add = Rows.Count(r => r.Status is RowStatus.Add);
        var registered = Rows.Count(r => r.Status is RowStatus.Registered);
        var noSelector = Rows.Count(r => r.Status is RowStatus.NoSelector);

        GUI.WrappedText(LOC.Get("HAVOK_Templates_Summary", Rows.Count, add, registered, noSelector, CountAdds(Rows)));

        ImGui.Checkbox($"{LOC.Get("HAVOK_Templates_Show_Registered")}##havokTemplateShowRegistered", ref ShowRegistered);
        ImGui.SameLine();
        ImGui.Checkbox($"{LOC.Get("HAVOK_Templates_Show_No_Selector")}##havokTemplateShowNoSelector", ref ShowNoSelector);
        GUI.Tooltip(LOC.Get("HAVOK_Templates_Show_No_Selector_TT"));

        var visible = Rows.Where(r => r.Status switch
        {
            RowStatus.Registered => ShowRegistered,
            RowStatus.NoSelector => ShowNoSelector,
            _ => true
        }).ToList();

        if (visible.Count == 0)
            return;

        var scale = DPI.UIScale();
        var flags = ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.Resizable |
            ImGuiTableFlags.ScrollY | ImGuiTableFlags.SizingStretchProp;
        var height = Math.Min(visible.Count + 1, 14) * ImGui.GetTextLineHeightWithSpacing() + 8 * scale;

        if (!ImGui.BeginTable("havokTemplateTable", 4, flags, new System.Numerics.Vector2(0, height)))
            return;

        ImGui.TableSetupScrollFreeze(0, 1);
        ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed, 24 * scale);
        ImGui.TableSetupColumn(LOC.Get("HAVOK_Templates_Column_Animation"), ImGuiTableColumnFlags.WidthStretch, 1.2f);
        ImGui.TableSetupColumn(LOC.Get("HAVOK_Templates_Column_Status"), ImGuiTableColumnFlags.WidthStretch, 1f);
        ImGui.TableSetupColumn(LOC.Get("HAVOK_Templates_Column_Selectors"), ImGuiTableColumnFlags.WidthStretch, 3f);
        ImGui.TableHeadersRow();

        var animations = Behavior.Animations is { IsReady: true, Error: null } ? Behavior.Animations : null;

        var clipper = new ImGuiListClipper();
        clipper.Begin(visible.Count);

        while (clipper.Step())
        {
            for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var row = visible[i];

                ImGui.TableNextRow();

                // Include
                ImGui.TableSetColumnIndex(0);

                if (row.Status is RowStatus.Add)
                    ImGui.Checkbox($"##havokTemplateInclude{row.AnimationId}", ref row.Include);

                // Animation
                ImGui.TableSetColumnIndex(1);
                ImGui.Text(row.AnimationName);
                GUI.Tooltip(row.Group);

                if (animations != null && row.Status is RowStatus.Add && !animations.HasAnimationFile(row.AnimationName))
                {
                    ImGui.SameLine();
                    ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, "!");
                    GUI.Tooltip(LOC.Get("HAVOK_Register_No_Animation_File_TT", row.AnimationName));
                }

                // Status
                ImGui.TableSetColumnIndex(2);

                var (statusKey, color) = row.Status switch
                {
                    RowStatus.Add => ("HAVOK_Templates_Status_Add", UI.Current.ImGui_Benefit_Text_Color),
                    RowStatus.Registered => ("HAVOK_Templates_Status_Registered", UI.Current.ImGui_Default_Text_Color),
                    _ => ("HAVOK_Templates_Status_No_Selector", UI.Current.ImGui_Invalid_Text_Color)
                };

                ImGui.TextColored(color, LOC.Get(statusKey));
                GUI.Tooltip(LOC.Get(statusKey + "_TT"));

                // Selectors: + gets the clip, = has it
                ImGui.TableSetColumnIndex(3);

                if (row.Targets.Count == 0)
                {
                    ImGui.TextDisabled("-");
                    continue;
                }

                ImGui.TextWrapped(string.Join("  ", row.Targets.Select(t => $"{(t.HasClip ? "=" : "+")}{Name(t.Selector)}")));

                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.Text(LOC.Get("HAVOK_Templates_Selectors_TT"));
                    ImGui.Separator();

                    foreach (var t in row.Targets)
                    {
                        var what = t.HasClip ? LOC.Get("HAVOK_Register_Target_Has")
                            : t.Template != null ? LOC.Get("HAVOK_Templates_Target_Copy", HavokBehaviorSymbols.GetValue(t.Template, "m_animationName") as string ?? "")
                            : LOC.Get("HAVOK_Register_Target_Add");

                        ImGui.Text($"{Name(t.Selector)} ({t.OffsetType}): {what}");
                    }

                    ImGui.EndTooltip();
                }
            }
        }

        clipper.End();

        ImGui.EndTable();
    }

    private static string Name(object obj) => HavokBehaviorSymbols.GetValue(obj, "m_name") as string ?? "";

    /// <summary>How many clips of a category the template's offset types already have in the file</summary>
    private static int CountCategoryClips(HavokBehaviorSymbols symbols, HavokBehaviorTemplate template, int category)
    {
        if (symbols == null || category < 0)
            return 0;

        var count = 0;

        foreach (var selector in symbols.Selectors)
        {
            var offsetType = HavokBehaviorSymbols.GetValue(selector, "m_offsetType")?.ToString() ?? "";

            if (template.OffsetTypes.Count > 0 && !template.OffsetTypes.Any(t => string.Equals(t, offsetType, StringComparison.OrdinalIgnoreCase)))
                continue;

            if (HavokBehaviorSymbols.GetValue(selector, "m_generators") is not System.Collections.IList generators)
                continue;

            foreach (var generator in generators)
            {
                if (generator?.GetType().Name == "hkbClipGenerator" &&
                    HavokBehaviorOperations.GetClipCategory(generator) == category)
                    count++;
            }
        }

        return count;
    }

    private void Apply(HavokBehaviorTemplate template)
    {
        var action = HavokTemplatePlanner.Apply(Context(), Rows, Category, out var added, out var errors);

        foreach (var e in errors.Distinct().Take(10))
            Smithbox.LogError(this, e);

        if (action == null)
        {
            LastResult = errors.FirstOrDefault() ?? LOC.Get("HAVOK_Templates_Nothing");
            return;
        }

        View.ActionManager.ExecuteAction(action);

        LastResult = LOC.Get("HAVOK_Templates_Done", template.Name, added, Format(Category), errors.Count);
        Smithbox.Log(this, LastResult);

        foreach (var note in template.Notes)
            Smithbox.Log(this, $"{template.Name}: {Fill(note)}");

        // The symbols are rebuilt after the edit, and the plan with them
        PlanKey = null;
    }
}
