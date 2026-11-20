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
    private string CachedTable;

    // The act table picked (null: the goal's first; see LuaBattleGoal.Tables): kept across scripts, the first where a
    // script hasn't that one
    private string ActTable;
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

    // The NpcParam row (or character, without one) and target picked for each script this session
    private readonly Dictionary<string, (int? Npc, int? Think, string Self, string Target)> CharactersByScript = new();
    private string ThinkFilter = "";
    private string NpcFilter = "";

    // Each act's plan at the current distance, for the act list (re-planned when the situation or distance changes)
    private readonly Dictionary<int, List<LuaActStep>> Plans = new();
    private double PlannedDistance = double.NaN;

    private int SelectedAct = -1;

    public LuaBattleView(LuaEditorScreen editor)
    {
        Editor = editor;
    }

    private LuaViewer Viewer => Editor.Viewer;

    // Which sections show (each header's eye toggle)
    private bool ShowCharacter = true;
    private bool ShowTimeAct = true;
    private bool ShowSituation = true;
    private bool ShowActs = true;
    private bool ShowInterrupts = true;

    /// <summary>The Battle Simulation tool of the Tools window</summary>
    public void OnToolWindow()
    {
        if (ToolSections.Header($"{LOC.Get("LUA_Tool_Battle_Simulation")}##luaBattleSimulationTool"))
        {
            ImGui.BeginChild("LuaBattleSimulationToolSection", ImGuiChildFlags.Borders);
            Draw();
            ImGui.EndChild();
        }
    }

    /// <summary>
    /// Reads the selected script's battle goal again when it has changed (another script, an edit), so the character,
    /// the situation and the Interrupt follow it whether or not the tool is showing. Called every frame.
    /// </summary>
    public LuaScript Sync()
    {
        var script = Editor.SelectedScript;
        if (script?.Text == null)
            return null;

        // aicommon's constants let comparisons with named values (ROLE_TYPE_…) work by value
        Editor.Definitions.EnsureIndex();
        var constants = Editor.Definitions.Constants;

        if (CachedScript != script || CachedRevision != script.Revision || CachedWithConstants != (constants != null) ||
            CachedTable != ActTable)
            Rebuild(script, constants);
        return script;
    }

    private void Draw()
    {
        GUI.TitleHeader(LOC.Get("LUA_Battle_Header"), LOC.Get("LUA_Battle_Header_TT"));

        var script = Sync();
        if (script == null)
        {
            ImGui.TextDisabled(LOC.Get("LUA_Script_None_Selected"));
            return;
        }

        if (Goal == null)
        {
            ImGui.TextDisabled(LOC.Get(Why ?? "LUA_Battle_Why_Invalid"));
            return;
        }

        ImGui.Text(Goal.GoalName ?? script.EntryName);
        ImGui.SameLine();
        var entry = TableName(Goal.Table);
        if (ImGui.SmallButton($"{entry}##luaBattleActivate"))
            Editor.ShowLine(script, Goal.ActivateLine);
        GUI.Tooltip(LOC.Get("LUA_Outline_Line", Goal.ActivateLine));
        DrawTableSwitch();

        // A goal without an act table: how its acts are made
        if (Goal.Kind != LuaGoalKind.Table)
        {
            ImGui.SameLine();
            var odds = Goal.Kind == LuaGoalKind.Odds;
            ImGui.TextDisabled(LOC.Get(odds ? "LUA_Battle_Kind_Odds" : "LUA_Battle_Kind_Direct"));
            GUI.Tooltip(LOC.Get(odds ? "LUA_Battle_Kind_Odds_TT" : "LUA_Battle_Kind_Direct_TT"));
        }

        DrawCharacter(script);
        DrawTimeAct();
        DrawSituation(script);
        // The acts' chances follow the situation even while its section is hidden
        Evaluate();
        DrawActs(script);
        DrawInterrupts(script);
    }

    /// <summary>
    /// The goal's act tables, when it has more than one: Battle (Activate's own), Kengeki (SDT's deflect exchanges), and
    /// a function Activate calls that picks acts itself (BB). The acts, weights, situation and plans below are the
    /// picked table's.
    /// </summary>
    private void DrawTableSwitch()
    {
        if (Goal.Tables.Count < 2)
            return;

        foreach (var table in Goal.Tables)
        {
            ImGui.SameLine();
            var label = table switch
            {
                LuaBattleGoal.MainTable => LOC.Get("LUA_Battle_Table_Battle"),
                LuaBattleGoal.KengekiEntry => LOC.Get("LUA_Battle_Table_Kengeki"),
                _ => TableName(table),
            };
            if (ImGui.RadioButton($"{label}##luaBattleTable_{table}", Goal.Table == table))
                ActTable = table;
            GUI.Tooltip(table switch
            {
                LuaBattleGoal.MainTable => LOC.Get("LUA_Battle_Table_Battle_TT"),
                LuaBattleGoal.KengekiEntry => LOC.Get("LUA_Battle_Table_Kengeki_TT"),
                _ => LOC.Get("LUA_Battle_Table_Called_TT", TableName(table)),
            });
        }
    }

    /// <summary>An act table's function as the script names it (Activate, Kengeki_Activate, Avater_B_BoarKing701000Battle)</summary>
    private static string TableName(string table) =>
        table == LuaBattleGoal.MainTable ? "Activate"
        : table.StartsWith(LuaBattleGoal.MethodPrefix, StringComparison.Ordinal) ? table[LuaBattleGoal.MethodPrefix.Length..] : table;

    private void Rebuild(LuaScript script, IReadOnlyDictionary<string, double> constants)
    {
        // The simulation runs on the situation being replaced
        Viewer.Simulation.Stop();

        // Keep the user's situation across edits of the same script (inputs match by their source text)
        var previous = CachedScript == script ? Situation?.Inputs.ToDictionary(i => i.Key) : null;

        CachedScript = script;
        CachedRevision = script.Revision;
        CachedWithConstants = constants != null;
        CachedTable = ActTable;
        Goal = LuaBattleGoal.Read(script.Text, script.Parse, out Why, ActTable);
        if (Goal == null && ActTable != null)
            Goal = LuaBattleGoal.Read(script.Text, script.Parse, out Why);
        Situation = Goal != null ? new LuaSituation(Goal, script.Text, script.Parse.Tokens, constants, Editor.Definitions.Functions) : null;
        // Acts are planned from where the characters stand (distance and facing)
        if (Situation != null)
        {
            Situation.Scene = Viewer;
            // The character's rows answer what the goal asks about itself
            Situation.Npc = () => Viewer.Self != null ? Viewer : null;
        }
        // The goal's Interrupt runs with this situation
        Viewer.Interrupts.Bind(Goal, Situation);
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

        // The characters picked for this script before, else the battle goal's own
        if (CharactersByScript.TryGetValue(script.Key, out var picked))
        {
            if (picked.Npc is { } npcId && LuaNpc.GetParam(Editor.Project)?[npcId] is { } row)
                Viewer.SetSelfNpc(row, script.GoalId);
            else
                Viewer.SetSelf(picked.Self);
            Viewer.SetTarget(picked.Target);
            Viewer.SetThinkRow(picked.Think is { } thinkId ? LuaNpc.GetThinkParam(Editor.Project)?[thinkId] : null);
        }
        else if (previous == null && Goal != null)
        {
            // Another script (not an edit of this one): its goal's own character, and its own think row
            PickForGoal(script);
            Viewer.SetThinkRow(null);
        }
    }

    /// <summary>
    /// Runs the battle goal as its own character: the NpcParam row of an NpcThinkParam row that uses the goal, else the
    /// character's first NpcParam row, else the character alone (from the script's goal ID)
    /// </summary>
    private void PickForGoal(LuaScript script)
    {
        var chr = script.CharacterId;
        var rows = LuaNpc.RowsForGoal(Editor.Project, script.GoalId, chr, Viewer.CharacterSet);
        if (rows.Count > 0)
            Viewer.SetSelfNpc(rows[0], script.GoalId);
        else if (chr != null && Viewer.CharacterSet.Contains(chr))
            Viewer.SetSelf(chr);
    }

    private LuaNpc EvaluatedNpc;
    private int EvaluatedSpEffects = -1;
    private int EvaluatedThink = -1;
    private (bool Walking, int Version) EvaluatedPlayer = (false, -1);
    private (int Mode, int? Role) EvaluatedRole = (-1, null);

    private void Evaluate()
    {
        // While the simulation runs, the distance and cooldowns come from the scene, so the chances follow them
        if (Viewer.Simulation.Running && Situation != null)
        {
            Viewer.Simulation.ApplyLive(Situation);
            Dirty = true;
        }
        // Another NPC row answers the goal's questions about itself differently (and plans other distances)
        if (!ReferenceEquals(EvaluatedNpc, Viewer.SelfNpc))
        {
            EvaluatedNpc = Viewer.SelfNpc;
            Dirty = true;
        }
        // Another NpcThinkParam row answers GetExcelParam differently
        if (EvaluatedThink != Viewer.ThinkVersion)
        {
            EvaluatedThink = Viewer.ThinkVersion;
            Dirty = true;
        }
        // The player's guard or a SpEffect their action put on the target (First Person Mode) changes what
        // IsTargetGuard and HasSpecialEffectId(TARGET_ENE_0, …) answer; entering or leaving it frees or takes them
        // The team role (a rotating one changes on its own) changes which branch of Activate runs
        var role = (CFG.Current.LuaEditor_Team_Role_Mode, Viewer.TeamRoleNow);
        if (EvaluatedRole != role)
        {
            EvaluatedRole = role;
            Dirty = true;
        }
        var playerState = (Viewer.FirstPerson.IsWalking, Viewer.FirstPerson.Version);
        if (EvaluatedPlayer != playerState)
        {
            EvaluatedPlayer = playerState;
            Dirty = true;
        }
        // A SpEffect the TAE put on (or that ran out) changes what HasSpecialEffectId(TARGET_SELF, …) answers
        if (EvaluatedSpEffects != Viewer.TimeAct.SpEffectVersion)
        {
            EvaluatedSpEffects = Viewer.TimeAct.SpEffectVersion;
            Dirty = true;
        }
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
        GUI.ConditionalHeader(LOC.Get("LUA_Battle_Character_Header"), LOC.Get("LUA_Battle_Character_Header_TT"), ref ShowCharacter);
        if (!ShowCharacter)
            return;

        if (ImGui.BeginTable("luaBattleChars", 2, ImGuiTableFlags.SizingStretchProp))
        {
            ImGui.TableSetupColumn("label", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("value", ImGuiTableColumnFlags.WidthStretch);

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            ImGui.AlignTextToFramePadding();
            // The character runs as an NpcParam row (its animation offsets, its behaviors); a project without NpcParam
            // picks the model alone
            var npcParam = LuaNpc.GetParam(Editor.Project);
            ImGui.Text(LOC.Get(npcParam != null ? "LUA_Battle_Character_Npc" : "LUA_Battle_Character_Self"));
            if (npcParam != null)
                GUI.Tooltip(LOC.Get("LUA_Battle_Character_Npc_TT"));
            ImGui.TableSetColumnIndex(1);
            if (npcParam != null)
            {
                if (NpcCombo(npcParam, script.CharacterId, out var row))
                {
                    Viewer.SetSelfNpc(row, script.GoalId);
                    Remember(script);
                }
            }
            else if (CharacterCombo("luaBattleSelf", Viewer.Self?.Name, ref CharacterFilter, false, out var self))
            {
                Viewer.SetSelf(self);
                Remember(script);
            }

            // The NpcThinkParam row GetExcelParam reads: any row, as the script picked is the battle goal
            if (LuaNpc.GetThinkParam(Editor.Project) is { } thinkParam)
            {
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                ImGui.AlignTextToFramePadding();
                ImGui.Text(LOC.Get("LUA_Battle_Character_Think"));
                GUI.Tooltip(LOC.Get("LUA_Battle_Character_Think_TT"));
                ImGui.TableSetColumnIndex(1);
                if (ThinkCombo(thinkParam, script.GoalId, script.CharacterId, out var think))
                {
                    Viewer.SetThinkRow(think);
                    Remember(script);
                }
            }

            // The team role GetTeamOrder(ORDER_TYPE_Role) gives: whether the goal attacks, or surrounds or watches
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            ImGui.AlignTextToFramePadding();
            ImGui.Text(LOC.Get("LUA_Battle_Team_Role"));
            GUI.Tooltip(LOC.Get("LUA_Battle_Team_Role_TT"));
            ImGui.TableSetColumnIndex(1);
            TeamRoleControls();

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

        // What the NpcParam row changes: the model it is, and the offsets its clips play with
        if (Viewer.SelfNpc is { } npc)
        {
            var alias = AliasOf(npc.Character);
            ImGui.TextDisabled(LOC.Get("LUA_Npc_Model", npc.Character, alias));

            // What the row swaps in
            if (npc.Source is { } swap)
            {
                var parts = new List<string>();
                if (swap.Model != npc.Character)
                    parts.Add(LOC.Get("LUA_Npc_Swap_Model", swap.Model, AliasOf(swap.Model)));
                if (swap.Animations != swap.Model)
                    parts.Add(LOC.Get("LUA_Npc_Swap_Animations", swap.Animations));
                if (swap.Textures != swap.Model)
                    parts.Add(LOC.Get("LUA_Npc_Swap_Textures", swap.Textures));
                ImGui.TextDisabled(LOC.Get("LUA_Npc_Swaps", string.Join(", ", parts)));
                GUI.Tooltip(LOC.Get("LUA_Npc_Swaps_TT"));
            }

            // Its model masks: the groups shown (a mesh without a group always is)
            if (npc.Masks is { } masks)
            {
                var shown = Enumerable.Range(0, masks.Length).Where(i => masks[i] == 1).ToList();
                ImGui.TextDisabled(LOC.Get("LUA_Npc_Masks", shown.Count == 0 ? "-" : string.Join(", ", shown)));
                GUI.Tooltip(LOC.Get("LUA_Npc_Masks_TT"));
            }
            if (npc.AnimationOffsets.Count == 0)
            {
                ImGui.TextDisabled(LOC.Get("LUA_Npc_No_Offsets"));
            }
            else
            {
                ImGui.TextDisabled(LOC.Get("LUA_Npc_Offsets",
                    string.Join(", ", npc.AnimationOffsets.Select(o => $"+{o.Offset} ({o.Source})"))));
                GUI.Tooltip(LOC.Get("LUA_Npc_Offsets_TT"));
            }
            if (npc.BehaviorVariation is { } variation)
                ImGui.TextDisabled(LOC.Get("LUA_Npc_Variation", variation));

            // What answers the goal's questions about itself
            ImGui.TextDisabled(npc.TurnVelocity is { } turnSpeed
                ? LOC.Get("LUA_Npc_Turn", LuaSituation.Format(turnSpeed))
                : LOC.Get("LUA_Npc_No_Turn"));
            GUI.Tooltip(LOC.Get("LUA_Npc_Turn_TT"));
            if (npc.MapHitRadius is { } radius)
            {
                ImGui.TextDisabled(LOC.Get("LUA_Npc_Hit_Radius", LuaSituation.Format(radius)));
                GUI.Tooltip(LOC.Get("LUA_Npc_Hit_Radius_TT"));
            }
            ImGui.TextDisabled(LOC.Get("LUA_Npc_Resident", npc.ResidentSpEffects.Count));
            if (ImGui.IsItemHovered() && npc.ResidentSpEffects.Count > 0)
            {
                ImGui.BeginTooltip();
                ImGui.Text(LOC.Get("LUA_Npc_Resident_TT"));
                foreach (var (id, slot) in npc.ResidentSpEffects)
                {
                    var name = CharacterComposition.GetRow(Editor.Project, "SpEffectParam", id)?.Name;
                    ImGui.TextDisabled(string.IsNullOrEmpty(name) ? $"{slot}: {id}" : $"{slot}: {id} {name}");
                }
                ImGui.EndTooltip();
            }
        }

        if (ImGui.Button($"{LOC.Get("LUA_Battle_Reset_Positions")}##luaResetPositions"))
            Viewer.ResetPositions();
        GUI.Tooltip(LOC.Get("LUA_Battle_Reset_Positions_TT"));
        if (Viewer.Target != null)
        {
            ImGui.SameLine();
            ImGui.TextDisabled(LOC.Get("LUA_Battle_Gizmo_Hint"));
        }

        // What is playing (always the one line, so it doesn't come and go between steps), and what couldn't be shown
        if (Viewer.Player.Current is { } step)
        {
            ImGui.TextDisabled(LOC.Get("LUA_Viewer_Playing_Step", StepText(step), step.Line));
            if (Viewer.Player.Turning is { } turning)
            {
                ImGui.SameLine();
                ImGui.TextDisabled(LOC.Get("LUA_Viewer_Turning", LuaSituation.Format(turning.Speed), turning.Source));
            }
        }
        else
        {
            ImGui.TextDisabled(LOC.Get("LUA_Viewer_Playing_Step_None"));
        }
        foreach (var skipped in Viewer.Player.Skipped.Take(3))
            ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, skipped);
        if (Viewer.Status != null)
            GUI.WrappedTextColored(UI.Current.ImGui_Default_Text_Color with { W = 0.7f }, Viewer.Status);
    }

    // --- the playing clip's TAE ---

    private bool ShowTimeActEvents = true;

    /// <summary>
    /// The playing clip's TAE: its events (fired or not yet), what's running in the viewport, and a log of what the
    /// events did (effects spawned, bullets fired, or why not)
    /// </summary>
    private void DrawTimeAct()
    {
        GUI.ConditionalHeader(LOC.Get("LUA_TAE_Header"), LOC.Get("LUA_TAE_Header_TT"), ref ShowTimeAct);
        if (!ShowTimeAct)
            return;
        var timeAct = Viewer.TimeAct;

        // (What the viewport shows of the effects and bullets is the Viewport Filters tool's)
        ImGui.Checkbox($"{LOC.Get("LUA_TAE_Show_Bullets")}##luaTaeBullets", ref CFG.Current.LuaEditor_TimeAct_Bullets);
        GUI.Tooltip(LOC.Get("LUA_TAE_Show_Bullets_TT"));
        ImGui.SameLine();
        ImGui.Checkbox($"{LOC.Get("LUA_TAE_Log_All")}##luaTaeLogAll", ref CFG.Current.LuaEditor_TimeAct_Log_All_Events);
        GUI.Tooltip(LOC.Get("LUA_TAE_Log_All_TT"));

        if (!timeAct.TimeActsAvailable)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("LUA_TAE_Why_No_Editor"));
            return;
        }
        if (!timeAct.EffectsAvailable && CFG.Current.LuaEditor_TimeAct_Effects)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("LUA_TAE_No_Fx_Editor"));
        else if (timeAct.PreloadStatus is { } loading)
            ImGui.TextDisabled(loading);

        // The play: which clip, from which TAE, how far in
        var play = timeAct.Current;
        if (play == null)
        {
            ImGui.TextDisabled(LOC.Get("LUA_TAE_Nothing_Playing"));
        }
        else
        {
            var clip = LuaTimeAct.FormatAnimation(play.AnimationId);
            var source = play.Source != null && play.Entry != null ? $"{play.Source.Name} {play.Entry.Name}" : "-";
            ImGui.Text(LOC.Get("LUA_TAE_Playing", clip, source, $"{play.LastTime:0.00}", $"{play.Duration:0.00}",
                play.Fired.Count, play.Events.Count));
            if (play.ImportedFrom is { } imported)
            {
                ImGui.SameLine();
                ImGui.TextDisabled(LOC.Get("LUA_TAE_Imported", LuaTimeAct.FormatAnimation(imported)));
            }
            if (play.Why != null)
                GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, play.Why);
        }

        ImGui.TextDisabled(LOC.Get("LUA_TAE_Counts", timeAct.RunningEffects, timeAct.FlyingBullets));
        if (timeAct.MissingEffects > 0)
        {
            ImGui.SameLine();
            ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("LUA_TAE_Counts_Missing", timeAct.MissingEffects));
        }
        if (timeAct.LoadingEffects > 0)
        {
            ImGui.SameLine();
            ImGui.TextDisabled(LOC.Get("LUA_TAE_Counts_Loading", timeAct.LoadingEffects));
        }

        // The play's events, in order: done, running, or still to come
        if (play is { Events.Count: > 0 })
        {
            ImGui.Checkbox($"{LOC.Get("LUA_TAE_Show_Events")}##luaTaeShowEvents", ref ShowTimeActEvents);
            if (ShowTimeActEvents)
                DrawTimeActEvents(play);
        }

        // The SpEffects the TAE has put on the character, which the goal's HasSpecialEffectId(TARGET_SELF, …) sees
        if (timeAct.ActiveSpEffects.Count > 0)
        {
            var now = timeAct.SceneTime;
            var text = string.Join(", ", timeAct.ActiveSpEffects.Select(s =>
                float.IsPositiveInfinity(s.Until) ? $"{s.Id}" : $"{s.Id} ({Math.Max(s.Until - now, 0):0.0} s)"));
            ImGui.TextColored(UI.Current.ImGui_TimeAct_InfoText_4_Color, LOC.Get("LUA_TAE_SpEffects_On", text));
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text(LOC.Get("LUA_TAE_SpEffects_On_TT"));
                foreach (var s in timeAct.ActiveSpEffects)
                    ImGui.TextDisabled(string.IsNullOrEmpty(s.Name) ? $"{s.Id}  ({s.Event})" : $"{s.Id} {s.Name}  ({s.Event})");
                ImGui.EndTooltip();
            }
        }

        DrawTimeActLog(timeAct);
    }

    private static void DrawTimeActEvents(LuaTimeActPlay play)
    {
        var rows = Math.Min(play.Events.Count, 8);
        var height = (rows + 1) * ImGui.GetTextLineHeightWithSpacing() + 12 * DPI.UIScale();
        ImGui.BeginChild("luaTaeEvents", new Vector2(0, height), ImGuiChildFlags.Borders | ImGuiChildFlags.ResizeY);
        var flags = ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingStretchProp | ImGuiTableFlags.ScrollY;
        if (ImGui.BeginTable("luaTaeEventsTable", 3, flags))
        {
            ImGui.TableSetupScrollFreeze(0, 1);
            ImGui.TableSetupColumn(LOC.Get("LUA_TAE_Col_Time"), ImGuiTableColumnFlags.WidthFixed, 90 * DPI.UIScale());
            ImGui.TableSetupColumn(LOC.Get("LUA_TAE_Col_Event"), ImGuiTableColumnFlags.WidthStretch, 0.55f);
            ImGui.TableSetupColumn(LOC.Get("LUA_TAE_Col_State"), ImGuiTableColumnFlags.WidthStretch, 0.45f);
            ImGui.TableHeadersRow();

            var now = play.LastTime;
            foreach (var ev in play.Events)
            {
                var fired = play.Fired.Contains(ev);
                var infinite = ev.EndTime >= SoulsFormats.TAE.Event.EldenRingInfiniteLengthEventPlaceholder;
                var running = fired && (infinite || now < ev.EndTime);

                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                var end = infinite ? "∞" : $"{ev.EndTime:0.00}";
                if (!fired)
                    ImGui.TextDisabled($"{ev.StartTime:0.00}–{end}");
                else
                    ImGui.Text($"{ev.StartTime:0.00}–{end}");

                ImGui.TableSetColumnIndex(1);
                var name = ev.TypeName ?? LOC.Get("LUA_TAE_Event_Type", ev.Type);
                if (play.EffectEvents.TryGetValue(ev, out var effect))
                    name = $"{name}  ({LOC.Get("LUA_TAE_Effect_Short", effect.EffectId)})";
                if (!fired)
                    ImGui.TextDisabled(name);
                else
                    ImGui.TextUnformatted(name);

                ImGui.TableSetColumnIndex(2);
                if (running)
                    ImGui.TextColored(UI.Current.ImGui_Benefit_Text_Color, LOC.Get("LUA_TAE_State_Running"));
                else if (fired)
                    ImGui.TextUnformatted(LOC.Get("LUA_TAE_State_Done"));
                else
                    ImGui.TextDisabled(LOC.Get("LUA_TAE_State_Pending"));
            }
            ImGui.EndTable();
        }
        ImGui.EndChild();
    }

    private int ShownLogVersion = -1;

    /// <summary>What the events did, newest first</summary>
    private void DrawTimeActLog(LuaTimeAct timeAct)
    {
        ImGui.AlignTextToFramePadding();
        ImGui.Text(LOC.Get("LUA_TAE_Log"));
        GUI.Tooltip(LOC.Get("LUA_TAE_Log_TT"));
        ImGui.SameLine();
        if (ImGui.SmallButton($"{LOC.Get("LUA_TAE_Log_Clear")}##luaTaeLogClear"))
            timeAct.ClearLog();

        var height = 7 * ImGui.GetTextLineHeightWithSpacing() + 8 * DPI.UIScale();
        ImGui.BeginChild("luaTaeLog", new Vector2(0, height), ImGuiChildFlags.Borders | ImGuiChildFlags.ResizeY);
        if (timeAct.Log.Count == 0)
            ImGui.TextDisabled(LOC.Get("LUA_TAE_Log_Empty"));

        // Newest at the top: a new line scrolls back to it
        if (ShownLogVersion != timeAct.LogVersion)
        {
            ShownLogVersion = timeAct.LogVersion;
            ImGui.SetScrollY(0);
        }

        for (var i = timeAct.Log.Count - 1; i >= 0; i--)
        {
            var entry = timeAct.Log[i];
            var color = entry.Kind switch
            {
                LuaTimeActLogKind.Effect => UI.Current.ImGui_Benefit_Text_Color,
                LuaTimeActLogKind.Bullet => UI.Current.ImGui_TimeAct_InfoText_3_Color,
                LuaTimeActLogKind.SpEffect => UI.Current.ImGui_TimeAct_InfoText_4_Color,
                LuaTimeActLogKind.Problem => UI.Current.ImGui_Warning_Text_Color,
                _ => UI.Current.ImGui_Default_Text_Color with { W = 0.7f },
            };
            var line = LOC.Get("LUA_TAE_Log_Line", $"{entry.SceneTime:0.00}", LuaTimeAct.FormatAnimation(entry.Animation),
                $"{entry.ClipTime:0.00}", entry.Event, entry.Detail);
            ImGui.PushStyleColor(ImGuiCol.Text, color);
            ImGui.TextWrapped(line);
            ImGui.PopStyleColor();
        }
        ImGui.EndChild();
    }

    private static readonly string[] TeamRoleModes = { "LUA_Battle_Team_Role_Attacker", "LUA_Battle_Team_Role_Rotating", "LUA_Battle_Team_Role_Manual" };

    /// <summary>The Team role setting: the mode, and for a rotating role its two times and the role now</summary>
    private void TeamRoleControls()
    {
        var cfg = CFG.Current;
        var mode = Math.Clamp(cfg.LuaEditor_Team_Role_Mode, 0, TeamRoleModes.Length - 1);

        ImGui.SetNextItemWidth(-1);
        if (ImGui.BeginCombo("##luaTeamRole", LOC.Get(TeamRoleModes[mode])))
        {
            for (var i = 0; i < TeamRoleModes.Length; i++)
            {
                if (ImGui.Selectable($"{LOC.Get(TeamRoleModes[i])}##luaTeamRole_{i}", i == mode))
                    cfg.LuaEditor_Team_Role_Mode = i;
                GUI.Tooltip(LOC.Get($"{TeamRoleModes[i]}_TT"));
            }
            ImGui.EndCombo();
        }

        if (cfg.LuaEditor_Team_Role_Mode != 1)
            return;

        // Rotating: how long each turn lasts, and where it is now
        var half = (ImGui.GetContentRegionAvail().X - ImGui.GetStyle().ItemSpacing.X) / 2;
        ImGui.SetNextItemWidth(half);
        ImGui.DragFloat("##luaTeamAttackTime", ref cfg.LuaEditor_Team_Attack_Time, 0.1f, 0.5f, 120f, LOC.Get("LUA_Battle_Team_Attack_Time"));
        GUI.Tooltip(LOC.Get("LUA_Battle_Team_Attack_Time_TT"));
        ImGui.SameLine();
        ImGui.SetNextItemWidth(half);
        ImGui.DragFloat("##luaTeamSurroundTime", ref cfg.LuaEditor_Team_Surround_Time, 0.1f, 0f, 120f, LOC.Get("LUA_Battle_Team_Surround_Time"));
        GUI.Tooltip(LOC.Get("LUA_Battle_Team_Surround_Time_TT"));

        var now = Viewer.TeamRoleNow == LuaViewer.RoleAttack ? LOC.Get("LUA_Battle_Team_Now_Attacking") : LOC.Get("LUA_Battle_Team_Now_Surrounding");
        ImGui.TextDisabled(Viewer.TeamRoleChangesIn is { } left
            ? LOC.Get("LUA_Battle_Team_Now", now, $"{left:0.0}")
            : now);
    }

    private void Remember(LuaScript script) =>
        CharactersByScript[script.Key] = (Viewer.SelfNpc?.Row.ID, Viewer.PickedThinkRow?.ID, Viewer.Self?.Name, Viewer.Target?.Name);

    // The NpcParam rows that name a character, for the combo: built once per param, filtered once per filter and limit
    private (Andre.Formats.Param Param, List<(Andre.Formats.Param.Row Row, string Chr, string Text)> Rows)? NpcRows;
    private (string Filter, string Limit, List<(Andre.Formats.Param.Row Row, string Chr, string Text)> Rows)? NpcShown;

    // The NpcThinkParam rows for the combo (ID, name, and the character the ID names): the same
    private (Andre.Formats.Param Param, List<(Andre.Formats.Param.Row Row, string Chr, string Text)> Rows)? ThinkRows;
    private (string Filter, string Limit, List<(Andre.Formats.Param.Row Row, string Chr, string Text)> Rows)? ThinkShown;

    private static string RowLabel(Andre.Formats.Param.Row row) =>
        string.IsNullOrEmpty(row.Name) ? $"{row.ID}" : $"{row.ID} {row.Name}";

    /// <summary>
    /// The "Only cXXXX" option of the NPC and Think lists (one setting for both): with it on, they list only the rows of
    /// the character the battle script's filename names. Gives that character while it is on, else null. Nothing shows
    /// for a script whose name names no character.
    /// </summary>
    private static string LimitOption(string scriptCharacter, string id)
    {
        if (CharacterFamily(scriptCharacter) is not { } family)
            return null;
        ImGui.Checkbox($"{LOC.Get("LUA_Battle_Limit_To_Character", $"{family}x")}##{id}", ref CFG.Current.LuaEditor_Limit_To_Script_Character);
        GUI.Tooltip(LOC.Get("LUA_Battle_Limit_To_Character_TT", scriptCharacter, $"{family}0", $"{family}9"));
        return CFG.Current.LuaEditor_Limit_To_Script_Character ? family : null;
    }

    /// <summary>
    /// A character's family: its ID without the last digit (c4300 → c430), which its model variants share (c4301 to
    /// c4309 are variants of c4300). Null for a name that isn't a chr ID.
    /// </summary>
    private static string CharacterFamily(string chr) =>
        chr is { Length: 5 } && (chr[0] is 'c' or 'C') ? chr[..4].ToLowerInvariant() : null;

    /// <summary>The rows the filter and the character limit (a family, such as c430) keep</summary>
    private static List<(Andre.Formats.Param.Row Row, string Chr, string Text)> Filtered(
        List<(Andre.Formats.Param.Row Row, string Chr, string Text)> rows, string filter, string limit)
    {
        IEnumerable<(Andre.Formats.Param.Row Row, string Chr, string Text)> kept = rows;
        if (limit != null)
            kept = kept.Where(x => CharacterFamily(x.Chr) == limit);
        if (!string.IsNullOrEmpty(filter))
            kept = kept.Where(x => EditorFilters.IsMatch(filter, x.Text, false));
        return kept.ToList();
    }

    /// <summary>
    /// A filterable dropdown of every NpcThinkParam row (its battleGoalID is ignored: the script picked is the battle
    /// goal simulated), with Automatic first: the NPC's own row. Rows that name this goal are marked. True with the pick
    /// (null for Automatic) when changed.
    /// </summary>
    private bool ThinkCombo(Andre.Formats.Param param, int? goalId, string scriptCharacter, out Andre.Formats.Param.Row picked)
    {
        picked = Viewer.PickedThinkRow;
        var current = Viewer.PickedThinkRow;
        var automatic = Viewer.SelfNpc?.ThinkRow;
        var preview = current != null
            ? RowLabel(current)
            : automatic != null ? LOC.Get("LUA_Battle_Think_Automatic_Row", RowLabel(automatic)) : LOC.Get("LUA_Battle_Think_Automatic");
        var changed = false;

        ImGui.SetNextItemWidth(-1);
        if (!ImGui.BeginCombo("##luaBattleThink", preview, ImGuiComboFlags.HeightLarge))
            return false;

        if (ThinkRows?.Param != param)
        {
            // NpcThinkParam rows are numbered as NpcParam rows are (43000000 → c4300)
            ThinkRows = (param, param.Rows.Where(r => r.ID >= 0)
                .Select(r => (r, LuaNpc.CharacterOf(r.ID, Viewer.CharacterSet), RowLabel(r))).ToList());
            ThinkShown = null;
        }

        ImGui.SetNextItemWidth(-1);
        if (ImGui.IsWindowAppearing())
            ImGui.SetKeyboardFocusHere();
        ImGui.InputTextWithHint("##luaBattleThink_filter", LOC.Get("LUA_Battle_Think_Filter"), ref ThinkFilter, 64);
        var limit = LimitOption(scriptCharacter, "luaBattleThink_limit");

        if (ImGui.Selectable($"{LOC.Get("LUA_Battle_Think_Automatic")}##luaThink_auto", current == null))
        {
            picked = null;
            changed = true;
        }
        GUI.Tooltip(LOC.Get("LUA_Battle_Think_Automatic_TT"));

        if (ThinkShown?.Filter != ThinkFilter || ThinkShown?.Limit != limit)
            ThinkShown = (ThinkFilter, limit, Filtered(ThinkRows.Value.Rows, ThinkFilter, limit));

        var rows = ThinkShown.Value.Rows;
        var clipper = new ImGuiListClipper();
        clipper.Begin(rows.Count);
        while (clipper.Step())
        {
            for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var (row, _, text) = rows[i];
                if (ImGui.Selectable($"{text}##luaThink_{row.ID}", current == row))
                {
                    picked = row;
                    changed = true;
                }
                // Rows that run this battle goal (the others name another, or none)
                if (goalId is { } goal && LuaNpc.ReadField(row, "battleGoalID") is { } rowGoal && (int)rowGoal == goal)
                {
                    ImGui.SameLine();
                    ImGui.TextDisabled(LOC.Get("LUA_Battle_Think_Uses_Goal"));
                }
            }
        }
        clipper.End();
        ImGui.EndCombo();
        return changed && picked != current;
    }

    /// <summary>A filterable dropdown of NpcParam rows (ID, name, model and its alias); true with the pick when changed</summary>
    private bool NpcCombo(Andre.Formats.Param param, string scriptCharacter, out Andre.Formats.Param.Row picked)
    {
        picked = Viewer.SelfNpc?.Row;
        var current = Viewer.SelfNpc;
        var preview = current == null
            ? Viewer.Self != null ? LOC.Get("LUA_Battle_Npc_Model_Only", Viewer.Self.Name) : LOC.Get("LUA_Battle_Character_None")
            : $"{current.Label}  ({current.Character})";
        var changed = false;

        ImGui.SetNextItemWidth(-1);
        if (!ImGui.BeginCombo("##luaBattleNpc", preview, ImGuiComboFlags.HeightLarge))
            return false;

        if (NpcRows?.Param != param)
        {
            NpcRows = (param, param.Rows
                .Where(r => r.ID > 0)
                .Select(r => (Row: r, Chr: LuaNpc.CharacterOf(r.ID, Viewer.CharacterSet)))
                .Where(x => x.Chr != null)
                .Select(x => (x.Row, x.Chr!, $"{x.Row.ID} {x.Row.Name} {x.Chr} {AliasOf(x.Chr!)}"))
                .ToList());
            NpcShown = null;
        }

        ImGui.SetNextItemWidth(-1);
        if (ImGui.IsWindowAppearing())
            ImGui.SetKeyboardFocusHere();
        ImGui.InputTextWithHint("##luaBattleNpc_filter", LOC.Get("LUA_Battle_Npc_Filter"), ref NpcFilter, 64);
        var limit = LimitOption(scriptCharacter, "luaBattleNpc_limit");

        if (NpcShown?.Filter != NpcFilter || NpcShown?.Limit != limit)
            NpcShown = (NpcFilter, limit, Filtered(NpcRows.Value.Rows, NpcFilter, limit));

        var rows = NpcShown.Value.Rows;
        var clipper = new ImGuiListClipper();
        clipper.Begin(rows.Count);
        while (clipper.Step())
        {
            for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var (row, chr, _) = rows[i];
                var label = string.IsNullOrEmpty(row.Name) ? $"{row.ID}" : $"{row.ID} {row.Name}";
                if (ImGui.Selectable($"{label}##luaNpc_{row.ID}", current?.Row == row))
                {
                    picked = row;
                    changed = true;
                }
                ImGui.SameLine();
                ImGui.TextDisabled(chr);
                GUI.DisplayAlias(AliasOf(chr), CFG.Current.Interface_Alias_Wordwrap_Model_Editor);
            }
        }
        clipper.End();
        ImGui.EndCombo();
        return changed && picked != current?.Row;
    }

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
        GUI.ConditionalHeader(LOC.Get("LUA_Battle_Situation_Header"), LOC.Get("LUA_Battle_Situation_Header_TT"), ref ShowSituation);
        if (!ShowSituation)
            return;

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
                    // The NPC's rows answer it (its radius, a resident SpEffect, its think param), else the scene while
                    // the simulation runs, else the user
                    var fromNpc = input.FromNpc;
                    var live = fromNpc || input.IsLive && Viewer.Simulation.Running;
                    var tag = fromNpc ? LOC.Get("LUA_Situation_From_Npc") : LOC.Get("LUA_Situation_Live");
                    ImGui.TextUnformatted(live ? $"{input.Label}  {tag}" : input.Label);
                    if (ImGui.IsItemHovered())
                        GUI.Tooltip(LOC.Get("LUA_Situation_Input_TT", input.Detail, input.Line) +
                            (fromNpc ? "\n" + LOC.Get("LUA_Situation_From_Npc_TT")
                                : input.IsLive ? "\n" + LOC.Get("LUA_Situation_Live_TT") : ""));
                    if (ImGui.IsItemClicked())
                        Editor.ShowLine(script, input.Line);

                    // A live input follows the scene while the simulation runs
                    ImGui.TableSetColumnIndex(1);
                    ImGui.SetNextItemWidth(-1);
                    if (live)
                        ImGui.BeginDisabled();
                    if (InputControl(input, i))
                        Dirty = true;
                    if (live)
                        ImGui.EndDisabled();
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
        GUI.ConditionalHeader(LOC.Get("LUA_Battle_Acts_Header"), LOC.Get("LUA_Battle_Acts_Header_TT"), ref ShowActs);
        if (!ShowActs)
            return;

        // Play: run the goal as the game does, act after act; Stop: back to idle
        var simulation = Viewer.Simulation;
        var running = simulation.Running;
        if (ImGui.Button(running ? $"{Icons.Stop} {LOC.Get("LUA_Simulation_Stop")}##luaSimToggle" : $"{Icons.Play} {LOC.Get("LUA_Simulation_Play")}##luaSimToggle"))
        {
            if (running)
                simulation.Stop();
            else if (Viewer.Self == null)
                Viewer.Status = LOC.Get("LUA_Viewer_No_Character");
            else
                simulation.Start(Goal, Situation);
        }
        GUI.Tooltip(LOC.Get("LUA_Simulation_Toggle_TT"));
        ImGui.SameLine();
        ImGui.Checkbox($"{LOC.Get("LUA_Battle_Hide_Zero")}##luaBattleHideZero", ref HideZero);

        // Acts start once every effect the character can trigger is ready, so none stalls mid-act
        if (Viewer.TimeAct.PreloadStatus is { } loading)
        {
            var waiting = running || Viewer.WaitingForEffects;
            GUI.WrappedTextColored(waiting ? UI.Current.ImGui_Warning_Text_Color : UI.Current.ImGui_Default_Text_Color with { W = 0.7f },
                waiting ? LOC.Get("LUA_TAE_Preload_Waiting", loading) : loading);
            GUI.Tooltip(LOC.Get("LUA_TAE_Preload_TT"));
        }

        if (running)
        {
            // Always the one line, so it doesn't come and go between acts
            if (simulation.Current is { } now)
                ImGui.Text(LOC.Get("LUA_Simulation_Now", now.Act, $"{now.Percent:0.#}"));
            else
                ImGui.TextDisabled(LOC.Get("LUA_Simulation_Now_None"));
            if (simulation.History.Count > 1)
            {
                var recent = simulation.History.TakeLast(10).Reverse().Skip(1).Select(h => $"{h.Act:D2}");
                ImGui.TextDisabled(LOC.Get("LUA_Simulation_History", string.Join(" ← ", recent)));
            }
        }

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

            var highlighted = simulation.Running ? simulation.Current?.Act == act.Id : SelectedAct == act.Id;
            if (ImGui.Selectable($"{act.Id:D2}##luaAct_{act.Id}", highlighted, ImGuiSelectableFlags.SpanAllColumns))
            {
                // One act on its own: the simulation stops
                simulation.Stop();
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

    // --- interrupts ---

    /// <summary>
    /// The goal's Interrupt: the interrupts it reacts to, each fired by a click (the SpEffects it observes too), and
    /// what it did with the last ones fired (by hand, or by a SpEffect the TAE put on)
    /// </summary>
    private void DrawInterrupts(LuaScript script)
    {
        GUI.ConditionalHeader(LOC.Get("LUA_Interrupt_Header"), LOC.Get("LUA_Interrupt_Header_TT"), ref ShowInterrupts);
        if (!ShowInterrupts)
            return;

        if (Goal.Interrupt == null)
        {
            ImGui.TextDisabled(LOC.Get("LUA_Interrupt_None"));
            return;
        }

        if (ImGui.SmallButton($"{LOC.Get("LUA_Outline_Line", Goal.InterruptLine)}##luaInterruptLine"))
            Editor.ShowLine(script, Goal.InterruptLine);
        GUI.Tooltip(LOC.Get("LUA_Interrupt_Line_TT"));

        var interrupts = Viewer.Interrupts;

        // The interrupt types it tests, but a SpEffect's (fired from the list below, with its ID)
        var types = Goal.InterruptTypes.Where(t => t.Type != LuaInterrupts.ActivateSpecialEffect).ToList();
        if (types.Count > 0)
        {
            ImGui.AlignTextToFramePadding();
            ImGui.Text(LOC.Get("LUA_Interrupt_Fire"));
            foreach (var (type, line) in types)
            {
                ImGui.SameLine();
                if (ImGui.SmallButton($"{type}##luaInterrupt_{type}"))
                    interrupts.Fire(type, null);
                GUI.Tooltip(LOC.Get("LUA_Interrupt_Fire_TT", type, line));
            }
        }

        // SpEffects: those it observes and those it tests, with who they are observed on
        var spEffects = Goal.ObservedSpEffects.Select(o => (o.Id, Target: o.Target, o.Line))
            .Concat(Goal.InterruptSpEffects.Where(t => !Goal.ObservedSpEffects.Any(o => o.Id == t.Id)).Select(t => (t.Id, Target: (string)null, t.Line)))
            .ToList();
        if (spEffects.Count > 0)
        {
            var rows = Math.Min(spEffects.Count, 6);
            var height = (rows + 1) * ImGui.GetFrameHeightWithSpacing() + 8 * DPI.UIScale();
            ImGui.BeginChild("luaInterruptSpEffects", new Vector2(0, height), ImGuiChildFlags.Borders | ImGuiChildFlags.ResizeY);
            if (ImGui.BeginTable("luaInterruptSpEffectsTable", 4, ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingStretchProp | ImGuiTableFlags.ScrollY))
            {
                ImGui.TableSetupScrollFreeze(0, 1);
                ImGui.TableSetupColumn(LOC.Get("LUA_Interrupt_Col_SpEffect"), ImGuiTableColumnFlags.WidthStretch, 0.5f);
                ImGui.TableSetupColumn(LOC.Get("LUA_Interrupt_Col_On"), ImGuiTableColumnFlags.WidthStretch, 0.2f);
                ImGui.TableSetupColumn(LOC.Get("LUA_Interrupt_Col_Reacts"), ImGuiTableColumnFlags.WidthStretch, 0.15f);
                ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableHeadersRow();

                foreach (var (id, target, line) in spEffects)
                {
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    var name = CharacterComposition.GetRow(Editor.Project, "SpEffectParam", id)?.Name;
                    ImGui.TextUnformatted(string.IsNullOrEmpty(name) ? $"{id}" : $"{id} {name}");
                    if (ImGui.IsItemClicked())
                        Editor.ShowLine(script, line);
                    GUI.Tooltip(LOC.Get("LUA_Outline_Line", line));

                    ImGui.TableSetColumnIndex(1);
                    ImGui.TextDisabled(target switch
                    {
                        "TARGET_SELF" => LOC.Get("LUA_Interrupt_On_Self"),
                        "TARGET_ENE_0" => LOC.Get("LUA_Interrupt_On_Target"),
                        null => LOC.Get("LUA_Interrupt_Not_Observed"),
                        _ => target,
                    });

                    ImGui.TableSetColumnIndex(2);
                    var reacts = Goal.InterruptSpEffects.Any(t => t.Id == id);
                    ImGui.TextDisabled(reacts ? LOC.Get("LUA_Interrupt_Yes") : "-");

                    ImGui.TableSetColumnIndex(3);
                    if (ImGui.SmallButton($"{LOC.Get("LUA_Interrupt_Fire_Button")}##luaInterruptSp_{id}_{target}"))
                        interrupts.Fire(LuaInterrupts.ActivateSpecialEffect, id);
                    GUI.Tooltip(LOC.Get("LUA_Interrupt_Fire_SpEffect_TT", id));
                }
                ImGui.EndTable();
            }
            ImGui.EndChild();
        }

        // What it did with the last interrupts, newest first
        if (interrupts.History.Count == 0)
        {
            ImGui.TextDisabled(LOC.Get("LUA_Interrupt_None_Fired"));
            return;
        }
        ImGui.Text(LOC.Get("LUA_Interrupt_Fired"));
        ImGui.SameLine();
        if (ImGui.SmallButton($"{LOC.Get("LUA_TAE_Log_Clear")}##luaInterruptClear"))
            interrupts.Clear();
        foreach (var firing in interrupts.History.AsEnumerable().Reverse().Take(6))
        {
            var r = firing.Result;
            var what = r.SpEffect is { } sp ? $"{r.Type} {sp}" : r.Type;
            var how = firing.Via;
            string outcome;
            if (!r.Handled)
                outcome = LOC.Get("LUA_Interrupt_Not_Taken");
            else
            {
                var calls = r.Calls.Where(c => c.Call is not ("ClearSubGoal" or "Replaning")).Select(c => c.Call).ToList();
                outcome = r.Steps.Count > 0
                    ? LOC.Get("LUA_Interrupt_Taken_Steps", string.Join(" → ", r.Steps.Select(StepText)))
                    : LOC.Get("LUA_Interrupt_Taken_Stop");
                if (calls.Count > 0)
                    outcome += "  " + LOC.Get("LUA_Interrupt_Calls", string.Join(", ", calls));
                if (r.Replans)
                    outcome += "  " + LOC.Get("LUA_Interrupt_Replans");
            }
            var color = r.Handled ? UI.Current.ImGui_Benefit_Text_Color : UI.Current.ImGui_Default_Text_Color with { W = 0.7f };
            ImGui.PushStyleColor(ImGuiCol.Text, color);
            ImGui.TextWrapped(LOC.Get("LUA_Interrupt_Line", $"{firing.SceneTime:0.00}", what, how, outcome));
            ImGui.PopStyleColor();
            if (ImGui.IsItemHovered() && r.Notes.Count > 0)
            {
                ImGui.BeginTooltip();
                foreach (var (line, message) in r.Notes.Take(6))
                    ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, $"{LOC.Get("LUA_Outline_Line", line)}: {message}");
                ImGui.EndTooltip();
            }
        }
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
        // Played once the effects the character can trigger are ready (planned then, from where they stand then)
        var situation = Situation;
        Viewer.RunWhenReady(() =>
        {
            var plan = situation.PlanAct(act, Viewer.CurrentDistance());
            Viewer.Status = plan.Count == 0 ? LOC.Get("LUA_Viewer_Act_Nothing", act.Id) : null;
            Viewer.Player.Play(plan);
        });
    }

    /// <summary>The act function's name without the goal's prefix (UncoverTombNobleManSword430000_Act01 → Act01)</summary>
    private static string ShortFunction(string name)
    {
        if (name == null)
            return "?";
        // Name_Act01 → Act01; a goal's method, Goal.Act01 → Act01 (Goal.ActAfter_AdjustSpace → AdjustSpace)
        var i = Math.Max(name.LastIndexOf('_'), name.LastIndexOf('.'));
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
