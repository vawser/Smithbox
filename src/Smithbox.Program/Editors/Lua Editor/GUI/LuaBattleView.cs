using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using StudioCore.Editors.ModelEditor;
using StudioCore.Editors.TimeActEditor;
using System.Numerics;

namespace StudioCore.Editors.LuaEditor;

/// <summary>
/// The battle goal of the selected script (scripts that follow the Common_Battle_Activate template: 401 of ER's
/// 405 battle scripts), as what the AI would do:
/// - Character: the time acts (and model) the acts' animations come from, shown in the Animation viewport;
/// - Situation: what Goal.Activate's conditions read from the game (distance, HP, special effects, …), each its own
///   input, so the situation can be changed on the fly;
/// - Acts: each act's chance of being picked in that situation (its share of the total weight). Clicking an act
///   plays its animations in order on the character.
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

    // The character picked for each binder this session
    private readonly Dictionary<string, string> CharacterByBinder = new();

    // Each act's animations the character has (finding them reads its time acts), per character
    private TimeActContainerWrapper AnimationSource;
    private readonly Dictionary<int, List<long>> AnimationCache = new();

    private int SelectedAct = -1;

    public LuaBattleView(LuaEditorScreen editor)
    {
        Editor = editor;
    }

    private LuaAnimationViewer Viewer => Editor.AnimationViewer;

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
        AnimationCache.Clear();

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

        // The character picked for this binder before
        if (CharacterByBinder.TryGetValue(script.Container.Name, out var name) && Viewer.Character?.Name != name)
        {
            var source = Viewer.Characters.FirstOrDefault(c => c.Name == name);
            if (source != null)
                Viewer.SelectCharacter(source);
        }
    }

    private void Evaluate()
    {
        if (!Dirty || Situation == null)
            return;
        Result = Situation.Evaluate();
        Percent = LuaSituation.Percentages(Result);
        Dirty = false;
    }

    // --- character ---

    private void DrawCharacter(LuaScript script)
    {
        GUI.TitleHeader(LOC.Get("LUA_Battle_Character_Header"), LOC.Get("LUA_Battle_Character_Header_TT"));

        if (!Viewer.IsAvailable)
        {
            ImGui.TextDisabled(LOC.Get("LUA_Viewer_Needs_Time_Act_Editor"));
            return;
        }

        var current = Viewer.Character;
        var preview = current == null ? LOC.Get("LUA_Battle_Character_None")
            : AliasOf(current.Name) is { Length: > 0 } currentAlias ? $"{current.Name}  {currentAlias}" : current.Name;
        ImGui.SetNextItemWidth(-1);
        if (ImGui.BeginCombo("##luaBattleCharacter", preview, ImGuiComboFlags.HeightLarge))
        {
            ImGui.SetNextItemWidth(-1);
            if (ImGui.IsWindowAppearing())
                ImGui.SetKeyboardFocusHere();
            ImGui.InputTextWithHint("##luaBattleCharacterFilter", LOC.Get("LUA_Battle_Character_Filter"), ref CharacterFilter, 64);

            foreach (var source in Viewer.Characters)
            {
                // The chr ID and its alias, as the Time Act Editor's source list shows them; the filter matches either
                var alias = AliasOf(source.Name);
                if (!string.IsNullOrEmpty(CharacterFilter) && !EditorFilters.IsMatch(CharacterFilter, source.Name, false, alias))
                    continue;
                if (ImGui.Selectable($"{source.Name}##luaBattleChr_{source.Name}", source == current))
                {
                    Viewer.SelectCharacter(source);
                    if (Viewer.Character == source)
                        CharacterByBinder[script.Container.Name] = source.Name;
                }
                GUI.DisplayAlias(alias, CFG.Current.Interface_Alias_Wordwrap_Model_Editor);
            }
            ImGui.EndCombo();
        }

        if (Viewer.Status != null)
            GUI.WrappedTextColored(UI.Current.ImGui_Default_Text_Color with { W = 0.7f }, Viewer.Status);
    }

    // Character aliases by chr ID (looked up once each)
    private readonly Dictionary<string, string> Aliases = new();

    private string AliasOf(string chr)
    {
        if (!Aliases.TryGetValue(chr, out var alias))
            Aliases[chr] = alias = ModelEditorUtils.GetAliasForSourceListEntry(Editor.Project, chr, ModelListType.Character) ?? "";
        return alias;
    }

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

        if (Viewer.Character != AnimationSource)
        {
            AnimationSource = Viewer.Character;
            AnimationCache.Clear();
        }

        var flags = ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersInnerH | ImGuiTableFlags.SizingStretchProp;
        if (!ImGui.BeginTable("luaBattleActs", 4, flags))
            return;

        ImGui.TableSetupColumn(LOC.Get("LUA_Battle_Col_Act"), ImGuiTableColumnFlags.WidthFixed, 40 * DPI.UIScale());
        ImGui.TableSetupColumn(LOC.Get("LUA_Battle_Col_Function"), ImGuiTableColumnFlags.WidthStretch, 0.45f);
        ImGui.TableSetupColumn(LOC.Get("LUA_Battle_Col_Chance"), ImGuiTableColumnFlags.WidthStretch, 0.4f);
        ImGui.TableSetupColumn(LOC.Get("LUA_Battle_Col_Animations"), ImGuiTableColumnFlags.WidthStretch, 0.25f);
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
                Viewer.Play(act);
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
            var anims = Animations(act);
            if (Viewer.Character == null)
                ImGui.TextDisabled("-");
            else if (anims.Count == 0)
                ImGui.TextDisabled(LOC.Get("LUA_Battle_No_Animations"));
            else
                ImGui.TextUnformatted(string.Join(", ", anims.Select(id => Viewer.Character.FormatAnimationName(id))));

            if (dim)
                ImGui.PopStyleVar();
        }

        ImGui.EndTable();
    }

    private List<long> Animations(LuaAct act)
    {
        if (AnimationSource == null)
            return new();
        if (!AnimationCache.TryGetValue(act.Id, out var list))
            AnimationCache[act.Id] = list = Viewer.AnimationsOf(act);
        return list;
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
