using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using System.Collections;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.HavokEditor;

/// <summary>
/// Search (Behavior Editor Tools window): events, variables, animations, states or any named object, in the shown
/// file, its binder, the files the project has changed, or every behavior file (HavokBehaviorScan). Clicking a result
/// opens its file and shows it. See Claude/Documentation/Behavior.md (Havok behavior tools) and UI.md (Tools window conventions).
/// </summary>
public class HavokBehaviorSearchTool
{
    private readonly HavokEditorView View;
    private readonly ProjectEntry Project;

    private HavokBehaviorView Behavior => View.PropertyView.BehaviorView;

    private enum SearchMode { Event, Variable, Animation, State, Name }

    private SearchMode Mode = SearchMode.Event;
    private HavokBehaviorScope Scope = HavokBehaviorScope.Binder;
    private string Text = "";
    private bool Exact = false;

    private readonly HavokBehaviorScan Scan = new();

    // aCCC_NNNNNN: an animation name, whose N is a selector's m_animId
    private static readonly Regex AnimationName = new(@"^a\d{3}_(\d{6})$", RegexOptions.IgnoreCase);

    public HavokBehaviorSearchTool(HavokEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("HAVOK_SearchTool_Header")}##havokBehaviorSearchToolHeader"))
            return;

        ImGui.BeginChild("HavokBehaviorSearchToolSection", ImGuiChildFlags.Borders);

        Display();

        ImGui.EndChild();
    }

    private void Display()
    {
        GUI.WrappedText(LOC.Get("HAVOK_SearchTool_Hint"));

        // Search For
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("HAVOK_SearchTool_Search_Header"), LOC.Get("HAVOK_SearchTool_Search_Header_TT"));

        ImGui.SetNextItemWidth(-1);

        if (ImGui.BeginCombo("##havokSearchMode", LOC.Get($"HAVOK_SearchView_Mode_{Mode}")))
        {
            foreach (var mode in Enum.GetValues<SearchMode>())
            {
                if (ImGui.Selectable(LOC.Get($"HAVOK_SearchView_Mode_{mode}"), Mode == mode))
                    Mode = mode;

                GUI.Tooltip(LOC.Get($"HAVOK_SearchView_Mode_{mode}_TT"));
            }

            ImGui.EndCombo();
        }

        GUI.Tooltip(LOC.Get("HAVOK_SearchView_Mode_TT"));

        // Run with the Search button: an All search reads every behavior file, so not on leaving the field
        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##havokSearchText", LOC.Get($"HAVOK_SearchView_Hint_{Mode}"), ref Text, 255);

        ImGui.Checkbox($"{LOC.Get("HAVOK_SearchView_Exact")}##havokSearchExact", ref Exact);
        GUI.Tooltip(LOC.Get("HAVOK_SearchView_Exact_TT"));

        // Scope
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("HAVOK_Tool_Scope_Header"), LOC.Get("HAVOK_Tool_Scope_Header_TT"));

        var canRun = HavokBehaviorScan.DrawScope("havokSearchScope", ref Scope, Behavior.IsShown);

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("HAVOK_Tool_Actions_Header"), LOC.Get("HAVOK_Tool_Actions_Header_TT"));

        GUI.ConditionalMultiButtonInput("havokSearchActions",
            "havokSearchRun", LOC.Get("HAVOK_SearchView_Search"), LOC.Get("HAVOK_SearchView_Search_TT"),
            Run, canRun && !Scan.IsRunning && Text.Trim().Length > 0);

        // Results
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("HAVOK_Tool_Results_Header"), LOC.Get("HAVOK_SearchTool_Results_Header_TT"));

        if (!Scan.HasRun && !Scan.IsRunning)
        {
            GUI.WrappedText(LOC.Get("HAVOK_SearchTool_Not_Run"));
            return;
        }

        var results = Scan.Snapshot();

        ImGui.TextDisabled(LOC.Get("HAVOK_SearchView_Count", results.Count));

        Scan.DrawStatus("havokSearchScan");

        HavokBehaviorScan.DrawResults("havokSearchScan", results, Behavior, Scope != HavokBehaviorScope.File);
    }

    private void Run()
    {
        var text = Text.Trim();
        var exact = Exact;
        var mode = Mode;

        bool Match(string value) => value != null && (exact
            ? string.Equals(value, text, StringComparison.OrdinalIgnoreCase)
            : value.Contains(text, StringComparison.OrdinalIgnoreCase));

        HavokScanWork work = mode switch
        {
            SearchMode.Event => (symbols, project, sink) => SearchEvents(symbols, project, sink, Match),
            SearchMode.Variable => (symbols, project, sink) => SearchVariables(symbols, sink, Match),
            SearchMode.Animation => (symbols, project, sink) => SearchAnimations(symbols, sink, Match, text),
            SearchMode.State => (symbols, project, sink) => SearchStates(symbols, sink, Match),
            _ => (symbols, project, sink) => SearchNames(symbols, sink, Match)
        };

        Scan.Start(View, Project, Scope, work);
    }

    #region Modes
    private static void SearchEvents(HavokBehaviorSymbols symbols, ProjectEntry project, HavokScanSink sink, Func<string, bool> match)
    {
        var names = symbols.EventNames;

        if (names == null)
            return;

        List<HavokEventUse>[] uses = null;

        for (var i = 0; i < names.Count && !sink.IsFull; i++)
        {
            var name = names[i] as string ?? "";

            if (!match(name))
                continue;

            // Built once per file, the first time an event matches
            uses ??= symbols.BuildEventUses(project);

            var eventUses = i < uses.Length ? uses[i] : new();
            var first = eventUses.FirstOrDefault(u => u.IsTransition) ?? eventUses.FirstOrDefault();
            var where = first == null ? LOC.Get("HAVOK_SearchView_Unused") : HavokBehaviorSymbols.DescribeEventUse(first);

            sink.Add(LOC.Get("HAVOK_SearchView_Result_Event", name, i, where, eventUses.Count), null, HavokScanJump.Event, i);
        }
    }

    private static void SearchVariables(HavokBehaviorSymbols symbols, HavokScanSink sink, Func<string, bool> match)
    {
        var names = symbols.VariableNames;

        if (names == null)
            return;

        for (var i = 0; i < names.Count && !sink.IsFull; i++)
        {
            var name = names[i] as string ?? "";

            if (!match(name))
                continue;

            sink.Add(LOC.Get("HAVOK_SearchView_Result_Variable", name, i,
                HavokBehaviorOperations.GetVariableTypeName(symbols, i), symbols.FindBindingsTo(i, characterProperty: false).Count),
                null, HavokScanJump.Variable, i);
        }
    }

    /// <summary>
    /// Clips by animation name; selectors by m_animId when the text is a number or an animation name (aCCC_NNNNNN)
    /// </summary>
    private static void SearchAnimations(HavokBehaviorSymbols symbols, HavokScanSink sink, Func<string, bool> match, string text)
    {
        foreach (var clip in symbols.Clips)
        {
            if (sink.IsFull)
                return;

            var animation = HavokBehaviorSymbols.GetValue(clip, "m_animationName") as string;

            if (match(animation))
                sink.Add(LOC.Get("HAVOK_SearchView_Result_Clip", HavokBehaviorSymbols.GetValue(clip, "m_name") as string ?? "", animation), clip);
        }

        long animId;
        var animationName = AnimationName.Match(text);

        if (animationName.Success)
            animId = long.Parse(animationName.Groups[1].Value);
        else if (!long.TryParse(text, out animId))
            return;

        foreach (var selector in symbols.Selectors)
        {
            if (sink.IsFull)
                return;

            var value = HavokBehaviorSymbols.GetValue(selector, "m_animId");

            if (value == null || Convert.ToInt64(value) != animId)
                continue;

            var machine = symbols.FindOwningStateMachine(selector);

            sink.Add(LOC.Get("HAVOK_SearchView_Result_Selector", HavokBehaviorSymbols.GetValue(selector, "m_name") as string ?? "", animId,
                machine == null ? "-" : HavokBehaviorSymbols.GetValue(machine, "m_name") as string ?? ""), selector);
        }
    }

    /// <summary>
    /// States (StateInfo) by name
    /// </summary>
    private static void SearchStates(HavokBehaviorSymbols symbols, HavokScanSink sink, Func<string, bool> match)
    {
        foreach (var machine in symbols.Objects.Where(o => o.GetType().Name == "hkbStateMachine"))
        {
            if (HavokBehaviorSymbols.GetValue(machine, "m_states") is not IList states)
                continue;

            foreach (var state in states)
            {
                if (sink.IsFull)
                    return;

                var name = HavokBehaviorSymbols.GetValue(state, "m_name") as string;

                if (!match(name))
                    continue;

                sink.Add(LOC.Get("HAVOK_SearchView_Result_State", HavokBehaviorSymbols.GetValue(machine, "m_name") as string ?? "", name,
                    HavokBehaviorSymbols.GetValue(state, "m_stateId") ?? -1), state);
            }
        }
    }

    /// <summary>
    /// Any object by its m_name
    /// </summary>
    private static void SearchNames(HavokBehaviorSymbols symbols, HavokScanSink sink, Func<string, bool> match)
    {
        foreach (var obj in symbols.Objects)
        {
            if (sink.IsFull)
                return;

            if (HavokBehaviorSymbols.GetValue(obj, "m_name") is not string name || !match(name))
                continue;

            sink.Add(LOC.Get("HAVOK_SearchView_Result_Name", obj.GetType().Name, name), obj);
        }
    }
    #endregion
}
