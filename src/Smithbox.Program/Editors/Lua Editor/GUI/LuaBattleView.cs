using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using StudioCore.Editors.ModelEditor;
using System.Numerics;

namespace StudioCore.Editors.LuaEditor;

/// <summary>
/// The battle goal of the selected script (scripts that follow the Common_Battle_Activate template: 401 of ER's
/// 405 battle scripts), as what the AI would do:
/// - Character: the character the goal runs on, and the target it fights (another character, placed by distance and
///   bearing), shown in the Viewport window;
/// - Situation: what Goal.Activate's conditions read from the game (distance, HP, special effects, …), each its own
///   input, so the situation can be changed on the fly;
/// - Acts: each act's chance of being picked in that situation (its share of the total weight), and what it does.
///   Clicking an act plays it: its function runs with the situation, and each sub-goal that runs becomes a step
///   (an attack's animation, walking or running to the target, turning, strafing, backing off, waiting), then the
///   character goes back to its idle.
/// </summary>
public class LuaBattleView
{
    private readonly LuaEditorScreen Editor;

    // Read from the shown revision of the script
    private LuaScript CachedScript;
    private int CachedRevision = -1;
    private bool CachedWithConstants;
    private LuaBattleGoal Goal;
    private string Why;
    private LuaSituation Situation;
    private LuaSituationResult Result;
    private SortedDictionary<int, double> Percent = new();
    private bool Dirty = true;

    private string InputFilter = "";
    private bool HideZero;
    private string CharacterFilter = "";
    private string TargetFilter = "";

    // The character (and target) picked for each binder this session
    private readonly Dictionary<string, (string Self, string Target)> CharactersByBinder = new();

    // Each act's plan at the current distance, for the act list (re-planned when the situation or distance changes)
    private readonly Dictionary<int, List<LuaActStep>> Plans = new();
    private double PlannedDistance = double.NaN;

    private int SelectedAct = -1;

    public LuaBattleView(LuaEditorScreen editor)
    {
        Editor = editor;
    }

    private LuaViewer Viewer => Editor.Viewer;

    public void Draw()
    {
        GUI.TitleHeader(LOC.Get("LUA_Battle_Header"), LOC.Get("LUA_Battle_Header_TT"));

        var script = Editor.SelectedScript;
        if (script?.Text == null)
        {
            ImGui.TextDisabled(LOC.Get("LUA_Script_None_Selected"));
            return;
        }

        // aicommon's constants let comparisons with named values (ROLE_TYPE_…) work by value
        Editor.Definitions.EnsureIndex();
        var constants = Editor.Definitions.Constants;

        if (CachedScript != script || CachedRevision != script.Revision || CachedWithConstants != (constants != null))
            Rebuild(script, constants);

        if (Goal == null)
        {
            ImGui.TextDisabled(LOC.Get(Why ?? "LUA_Battle_Why_Invalid"));
            return;
        }

        ImGui.Text(Goal.GoalName ?? script.EntryName);
        ImGui.SameLine();
        if (ImGui.SmallButton($"Activate##luaBattleActivate"))
            Editor.ShowLine(script, Goal.ActivateLine);
        GUI.Tooltip(LOC.Get("LUA_Outline_Line", Goal.ActivateLine));

        ImGui.BeginChild("luaBattleBody");
        DrawCharacter(script);
        DrawSituation(script);
        DrawActs(script);
        ImGui.EndChild();
    }

    private void Rebuild(LuaScript script, IReadOnlyDictionary<string, double> constants)
    {
        // Keep the user's situation across edits of the same script (inputs match by their source text)
        var previous = CachedScript == script ? Situation?.Inputs.ToDictionary(i => i.Key) : null;

        CachedScript = script;
        CachedRevision = script.Revision;
        CachedWithConstants = constants != null;
        Goal = LuaBattleGoal.Read(script.Text, script.Parse, out Why);
        Situation = Goal != null ? new LuaSituation(Goal, script.Text, script.Parse.Tokens, constants) : null;
        Plans.Clear();

        if (Situation != null && previous != null)
        {
            foreach (var input in Situation.Inputs)
            {
                if (previous.TryGetValue(input.Key, out var old) && old.Kind == input.Kind)
                {
                    input.Boolean = old.Boolean;
                    input.Number = old.Number;
                    if (old.Choice == null || input.Choices.Contains(old.Choice) || old.Choice == LuaSituation.OtherChoice)
                        input.Choice = old.Choice;
                }
            }
        }
        Dirty = true;

        // The characters picked for this binder before
        if (CharactersByBinder.TryGetValue(script.Container.Name, out var picked))
        {
            Viewer.SetSelf(picked.Self);
            Viewer.SetTarget(picked.Target);
        }
    }

    private void Evaluate()
    {
        if (!Dirty || Situation == null)
            return;
        Result = Situation.Evaluate();
        Percent = LuaSituation.Percentages(Result);
        Plans.Clear();
        Dirty = false;
    }

    // --- character and target ---

    private void DrawCharacter(LuaScript script)
    {
        GUI.TitleHeader(LOC.Get("LUA_Battle_Character_Header"), LOC.Get("LUA_Battle_Character_Header_TT"));

        if (ImGui.BeginTable("luaBattleChars", 2, ImGuiTableFlags.SizingStretchProp))
        {
            ImGui.TableSetupColumn("label", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("value", ImGuiTableColumnFlags.WidthStretch);

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            ImGui.AlignTextToFramePadding();
            ImGui.Text(LOC.Get("LUA_Battle_Character_Self"));
            ImGui.TableSetColumnIndex(1);
            if (CharacterCombo("luaBattleSelf", Viewer.Self?.Name, ref CharacterFilter, false, out var self))
            {
                Viewer.SetSelf(self);
                Remember(script);
            }

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            ImGui.AlignTextToFramePadding();
            ImGui.Text(LOC.Get("LUA_Battle_Character_Target"));
            GUI.Tooltip(LOC.Get("LUA_Battle_Character_Target_TT"));
            ImGui.TableSetColumnIndex(1);
            if (CharacterCombo("luaBattleTarget", Viewer.Target?.Name, ref TargetFilter, true, out var target))
            {
                Viewer.SetTarget(target);
                Remember(script);
            }

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            ImGui.AlignTextToFramePadding();
            ImGui.Text(LOC.Get("LUA_Battle_Target_Distance"));
            ImGui.TableSetColumnIndex(1);
            ImGui.SetNextItemWidth(-1);
            if (ImGui.DragFloat("##luaTargetDistance", ref Viewer.TargetDistance, 0.05f, 0, 100, "%.2f m"))
                Viewer.PlaceTarget();

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            ImGui.AlignTextToFramePadding();
            ImGui.Text(LOC.Get("LUA_Battle_Target_Bearing"));
            GUI.Tooltip(LOC.Get("LUA_Battle_Target_Bearing_TT"));
            ImGui.TableSetColumnIndex(1);
            ImGui.SetNextItemWidth(-1);
            if (ImGui.DragFloat("##luaTargetBearing", ref Viewer.TargetBearing, 0.5f, -180, 180, "%.0f deg"))
                Viewer.PlaceTarget();

            ImGui.EndTable();
        }

        if (ImGui.Button($"{LOC.Get("LUA_Battle_Reset_Positions")}##luaResetPositions"))
            Viewer.ResetPositions();
        GUI.Tooltip(LOC.Get("LUA_Battle_Reset_Positions_TT"));
        ImGui.SameLine();
        if (ImGui.Button($"{LOC.Get("LUA_Battle_Stop")}##luaStop"))
            Viewer.Player.Idle();

        // What is playing, and what couldn't be shown
        if (Viewer.Player.Current is { } step)
            ImGui.TextDisabled(LOC.Get("LUA_Viewer_Playing_Step", StepText(step), step.Line));
        foreach (var skipped in Viewer.Player.Skipped.Take(3))
            ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, skipped);
        if (Viewer.Status != null)
            GUI.WrappedTextColored(UI.Current.ImGui_Default_Text_Color with { W = 0.7f }, Viewer.Status);
    }

    private void Remember(LuaScript script) =>
        CharactersByBinder[script.Container.Name] = (Viewer.Self?.Name, Viewer.Target?.Name);

    /// <summary>A filterable dropdown of chr IDs with their aliases; true with the pick (null for none) when changed</summary>
    private bool CharacterCombo(string id, string current, ref string filter, bool allowNone, out string picked)
    {
        picked = current;
        var none = LOC.Get(allowNone ? "LUA_Battle_Target_None" : "LUA_Battle_Character_None");
        var preview = current == null ? none
            : AliasOf(current) is { Length: > 0 } currentAlias ? $"{current}  {currentAlias}" : current;
        var changed = false;

        ImGui.SetNextItemWidth(-1);
        if (ImGui.BeginCombo($"##{id}", preview, ImGuiComboFlags.HeightLarge))
        {
            ImGui.SetNextItemWidth(-1);
            if (ImGui.IsWindowAppearing())
                ImGui.SetKeyboardFocusHere();
            ImGui.InputTextWithHint($"##{id}_filter", LOC.Get("LUA_Battle_Character_Filter"), ref filter, 64);

            if (allowNone && ImGui.Selectable($"{none}##{id}_none", current == null))
            {
                picked = null;
                changed = true;
            }

            foreach (var chr in Viewer.Characters)
            {
                // The chr ID and its alias, as the other editors' source lists show them; the filter matches either
                var alias = AliasOf(chr);
                if (!string.IsNullOrEmpty(filter) && !EditorFilters.IsMatch(filter, chr, false, alias))
                    continue;
                if (ImGui.Selectable($"{chr}##{id}_{chr}", chr == current))
                {
                    picked = chr;
                    changed = true;
                }
                GUI.DisplayAlias(alias, CFG.Current.Interface_Alias_Wordwrap_Model_Editor);
            }
            ImGui.EndCombo();
        }
        return changed && picked != current;
    }

    // Character aliases by chr ID (looked up once each)
    private readonly Dictionary<string, string> Aliases = new();

    private string AliasOf(string chr)
    {
        if (!Aliases.TryGetValue(chr, out var alias))
            Aliases[chr] = alias = ModelEditorUtils.GetAliasForSourceListEntry(Editor.Project, chr, ModelListType.Character) ?? "";
        return alias;
    }

    /// <summary>A step as a short line: what it does and with what</summary>
    private static string StepText(LuaActStep s) => s.Kind switch
    {
        LuaStepKind.Clip => LOC.Get("LUA_Step_Clip", $"a{s.Animation / 1_000_000:D3}_{s.Animation % 1_000_000:D6}"),
        LuaStepKind.Approach => LOC.Get(s.Walk ? "LUA_Step_Walk_To" : "LUA_Step_Run_To", LuaSituation.Format(s.Distance)),
        LuaStepKind.Leave => LOC.Get("LUA_Step_Leave", LuaSituation.Format(s.Distance)),
        LuaStepKind.Sideway => LOC.Get(s.Right ? "LUA_Step_Strafe_Right" : "LUA_Step_Strafe_Left"),
        LuaStepKind.Turn => LOC.Get("LUA_Step_Turn", LuaSituation.Format(s.Angle)),
        LuaStepKind.Wait => LOC.Get("LUA_Step_Wait", LuaSituation.Format(s.Life)),
        _ => s.Goal,
    };

    // --- situation ---

    private void DrawSituation(LuaScript script)
    {
        GUI.TitleHeader(LOC.Get("LUA_Battle_Situation_Header"), LOC.Get("LUA_Battle_Situation_Header_TT"));

        if (Situation.Inputs.Count == 0)
        {
            ImGui.TextDisabled(LOC.Get("LUA_Situation_No_Inputs"));
        }
        else
        {
            ImGui.SetNextItemWidth(-ImGui.CalcTextSize(LOC.Get("LUA_Situation_Reset")).X - 24 * DPI.UIScale());
            ImGui.InputTextWithHint("##luaSituationFilter", LOC.Get("LUA_Situation_Filter"), ref InputFilter, 64);
            ImGui.SameLine();
            if (ImGui.Button($"{LOC.Get("LUA_Situation_Reset")}##luaSituationReset"))
            {
                CachedRevision = -1;
                CachedScript = null;
            }
            GUI.Tooltip(LOC.Get("LUA_Situation_Reset_TT"));

            var height = Math.Min(Situation.Inputs.Count, 12) * ImGui.GetFrameHeightWithSpacing() + 8 * DPI.UIScale();
            ImGui.BeginChild("luaSituationInputs", new Vector2(0, height), ImGuiChildFlags.Borders | ImGuiChildFlags.ResizeY);
            if (ImGui.BeginTable("luaSituationTable", 2, ImGuiTableFlags.SizingStretchProp))
            {
                ImGui.TableSetupColumn("name", ImGuiTableColumnFlags.WidthStretch, 0.6f);
                ImGui.TableSetupColumn("value", ImGuiTableColumnFlags.WidthStretch, 0.4f);
                for (int i = 0; i < Situation.Inputs.Count; i++)
                {
                    var input = Situation.Inputs[i];
                    if (!string.IsNullOrEmpty(InputFilter) && !EditorFilters.IsMatch(InputFilter, input.Label, false, input.Detail))
                        continue;
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.AlignTextToFramePadding();
                    ImGui.TextUnformatted(input.Label);
                    if (ImGui.IsItemHovered())
                        GUI.Tooltip(LOC.Get("LUA_Situation_Input_TT", input.Detail, input.Line));
                    if (ImGui.IsItemClicked())
                        Editor.ShowLine(script, input.Line);

                    ImGui.TableSetColumnIndex(1);
                    ImGui.SetNextItemWidth(-1);
                    if (InputControl(input, i))
                        Dirty = true;
                }
                ImGui.EndTable();
            }
            ImGui.EndChild();
        }

        Evaluate();
        if (Result != null)
        {
            if (Result.LastWeightLine > 0)
            {
                ImGui.TextDisabled(LOC.Get("LUA_Situation_Set_By"));
                ImGui.SameLine();
                if (ImGui.SmallButton($"{LOC.Get("LUA_Outline_Line", Result.LastWeightLine)}##luaSituationLine"))
                    Editor.ShowLine(script, Result.LastWeightLine);
            }
            foreach (var (line, message) in Result.Notes.Take(3))
            {
                ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, message);
                if (ImGui.IsItemClicked())
                    Editor.ShowLine(script, line);
            }
        }
    }

    /// <summary>The input's control; true when its value changed</summary>
    private static bool InputControl(LuaSituationInput input, int index)
    {
        switch (input.Kind)
        {
            case LuaInputKind.Boolean:
                return ImGui.Checkbox($"##luaIn_{index}", ref input.Boolean);

            case LuaInputKind.Number:
                var n = (float)input.Number;
                if (ImGui.DragFloat($"##luaIn_{index}", ref n, 0.1f, 0, 0, "%.2f"))
                {
                    input.Number = n;
                    return true;
                }
                return false;

            default:
                var shown = string.IsNullOrEmpty(input.Choice) ? LOC.Get("LUA_Situation_Other") : input.Choice;
                var changed = false;
                if (ImGui.BeginCombo($"##luaIn_{index}", shown))
                {
                    foreach (var c in input.Choices.Append(LuaSituation.OtherChoice))
                    {
                        var label = c == LuaSituation.OtherChoice ? LOC.Get("LUA_Situation_Other") : c;
                        if (ImGui.Selectable($"{label}##luaInChoice_{index}_{c}", c == (input.Choice ?? "")))
                        {
                            input.Choice = c;
                            changed = true;
                        }
                    }
                    ImGui.EndCombo();
                }
                return changed;
        }
    }

    // --- acts ---

    private void DrawActs(LuaScript script)
    {
        GUI.TitleHeader(LOC.Get("LUA_Battle_Acts_Header"), LOC.Get("LUA_Battle_Acts_Header_TT"));
        ImGui.Checkbox($"{LOC.Get("LUA_Battle_Hide_Zero")}##luaBattleHideZero", ref HideZero);

        // What each act does depends on the distance to the target: plan again when it has moved
        var distance = Viewer.CurrentDistance();
        if (Math.Abs(distance - PlannedDistance) > 0.25)
        {
            Plans.Clear();
            PlannedDistance = distance;
        }

        var flags = ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersInnerH | ImGuiTableFlags.SizingStretchProp;
        if (!ImGui.BeginTable("luaBattleActs", 4, flags))
            return;

        ImGui.TableSetupColumn(LOC.Get("LUA_Battle_Col_Act"), ImGuiTableColumnFlags.WidthFixed, 40 * DPI.UIScale());
        ImGui.TableSetupColumn(LOC.Get("LUA_Battle_Col_Function"), ImGuiTableColumnFlags.WidthStretch, 0.2f);
        ImGui.TableSetupColumn(LOC.Get("LUA_Battle_Col_Chance"), ImGuiTableColumnFlags.WidthStretch, 0.25f);
        ImGui.TableSetupColumn(LOC.Get("LUA_Battle_Col_Does"), ImGuiTableColumnFlags.WidthStretch, 0.55f);
        ImGui.TableHeadersRow();

        foreach (var act in Goal.Acts.Values)
        {
            var percent = Percent.GetValueOrDefault(act.Id);
            if (HideZero && percent <= 0)
                continue;

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            var dim = percent <= 0;
            if (dim)
                ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.55f);

            if (ImGui.Selectable($"{act.Id:D2}##luaAct_{act.Id}", SelectedAct == act.Id, ImGuiSelectableFlags.SpanAllColumns))
            {
                SelectedAct = act.Id;
                Play(act);
            }
            if (ImGui.IsItemHovered())
                ActTooltip(act);
            if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left) && act.FunctionLine > 0)
                Editor.ShowLine(script, act.FunctionLine);

            ImGui.TableSetColumnIndex(1);
            ImGui.TextUnformatted(ShortFunction(act.Function));

            ImGui.TableSetColumnIndex(2);
            ImGui.ProgressBar((float)(percent / 100), new Vector2(-1, 0), $"{percent:0.#} %");

            ImGui.TableSetColumnIndex(3);
            var plan = PlanOf(act);
            if (plan.Count == 0)
                ImGui.TextDisabled(LOC.Get("LUA_Battle_No_Steps"));
            else
            {
                var text = string.Join(" → ", plan.Select(StepText));
                ImGui.TextUnformatted(text.Length > 80 ? text[..77] + "..." : text);
                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    foreach (var s in plan)
                        ImGui.Text(LOC.Get("LUA_Battle_Step_Line", StepText(s), s.Goal, s.Line));
                    ImGui.EndTooltip();
                }
            }

            if (dim)
                ImGui.PopStyleVar();
        }

        ImGui.EndTable();
    }

    /// <summary>The act's steps at the current distance (its function run with the situation)</summary>
    private List<LuaActStep> PlanOf(LuaAct act)
    {
        if (!Plans.TryGetValue(act.Id, out var plan))
            Plans[act.Id] = plan = Situation.PlanAct(act, PlannedDistance);
        return plan;
    }

    /// <summary>Plays the act on the character from where it stands</summary>
    private void Play(LuaAct act)
    {
        if (Viewer.Self == null)
        {
            Viewer.Status = LOC.Get("LUA_Viewer_No_Character");
            return;
        }
        var plan = Situation.PlanAct(act, Viewer.CurrentDistance());
        Viewer.Status = plan.Count == 0 ? LOC.Get("LUA_Viewer_Act_Nothing", act.Id) : null;
        Viewer.Player.Play(plan);
    }

    /// <summary>The act function's name without the goal's prefix (UncoverTombNobleManSword430000_Act01 → Act01)</summary>
    private static string ShortFunction(string name)
    {
        if (name == null)
            return "?";
        var i = name.LastIndexOf('_');
        return i > 0 && i < name.Length - 1 ? name[(i + 1)..] : name;
    }

    private void ActTooltip(LuaAct act)
    {
        ImGui.BeginTooltip();
        ImGui.Text(LOC.Get("LUA_Battle_Act", act.Id, act.Function ?? "?"));
        ImGui.TextDisabled(LOC.Get("LUA_Battle_Weight", LuaSituation.Format(Result?.Weights.GetValueOrDefault(act.Id) ?? 0)));
        foreach (var s in act.SubGoals.Take(8))
            ImGui.TextDisabled(s.Goal);
        foreach (var (anim, seconds, _) in act.Cooldowns)
            ImGui.TextDisabled(LOC.Get("LUA_Battle_Cooldown", anim?.Text ?? "?", seconds?.Text ?? "?"));
        ImGui.Separator();
        ImGui.TextDisabled(LOC.Get("LUA_Battle_Act_Click_TT"));
        ImGui.EndTooltip();
    }
}
