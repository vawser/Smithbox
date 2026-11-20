#nullable enable
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using System.Numerics;

namespace StudioCore.Editors.LuaEditor;

/// <summary>
/// The First Person Mode tool of the Lua Editor's Tools window, after the Map Editor's: become the target and walk in
/// first person while the battle goal fights you. Sets how fast the player moves, and how each action (attack, guard,
/// estus, roll, jump) reaches the AI: the interrupt it fires, the SpEffects it puts on the target and for how long.
/// Saved in the CFG.
/// </summary>
public class LuaFirstPersonTool
{
    private readonly LuaEditorScreen Editor;

    // The interrupt types offered for an action: those the scripts react to most, then any the goal tests
    private static readonly string[] CommonInterrupts =
    {
        "FindAttack", "Damaged", "TargetIsGuard", "UseItem", "Shoot", "SuccessGuard", "GuardBreak", "MissSwing",
        "ParryTiming", "SuccessParry", "ReboundByOpponentGuard", "TargetOutOfRange",
    };

    private static readonly LuaPlayerAction[] Actions = Enum.GetValues<LuaPlayerAction>();

    // The SpEffect lists being typed in, applied when the field is left (not as each key is typed)
    private readonly Dictionary<LuaPlayerAction, string> SpEffectEdits = new();

    public LuaFirstPersonTool(LuaEditorScreen editor)
    {
        Editor = editor;
    }

    private LuaFirstPerson Mode => Editor.Viewer.FirstPerson;

    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("LUA_Tool_First_Person")}##luaFirstPersonTool"))
            return;

        ImGui.BeginChild("LuaFirstPersonToolSection", ImGuiChildFlags.Borders);
        Display();
        ImGui.EndChild();
    }

    private void Display()
    {
        GUI.WrappedText(LOC.Get("LUA_FirstPerson_Hint"));

        DisplayWalk();
        DisplayActions();
        DisplayMovement();
    }

    /// <summary>Enter and leave, what is happening now, and the keys</summary>
    private void DisplayWalk()
    {
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("LUA_FirstPerson_Walk_Header"), LOC.Get("LUA_FirstPerson_Walk_Header_TT"));

        var release = InputManager.GetHint(KeybindID.MapEditor_FirstPerson_Release);
        if (Mode.IsWalking)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color,
                LOC.Get(Mode.IsCaptured ? "LUA_FirstPerson_State_Walking" : "LUA_FirstPerson_State_Paused", release));
            if (ImGui.Button($"{LOC.Get("LUA_FirstPerson_Exit")}##luaFirstPersonExit", DPI.WholeWidthButton(ImGui.GetContentRegionAvail().X, 24)))
                Mode.Exit();
        }
        else
        {
            var reason = Mode.CannotEnterReason();
            ImGui.BeginDisabled(reason != null);
            if (ImGui.Button($"{LOC.Get("LUA_FirstPerson_Enter")}##luaFirstPersonEnter", DPI.WholeWidthButton(ImGui.GetContentRegionAvail().X, 24)))
                Mode.Enter();
            ImGui.EndDisabled();
            GUI.Tooltip(LOC.Get("LUA_FirstPerson_Enter_TT", release));
            if (reason != null)
                GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, reason);
            else if (Mode.Status != null)
                GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, Mode.Status);
        }

        // The keys
        ImGui.TextDisabled(LOC.Get("LUA_FirstPerson_Keys",
            InputManager.GetHint(KeybindID.MapEditor_FirstPerson_Run), InputManager.GetHint(KeybindID.MapEditor_FirstPerson_Jump),
            InputManager.GetHint(KeybindID.LuaEditor_FirstPerson_Roll), InputManager.GetHint(KeybindID.LuaEditor_FirstPerson_Estus), release));

        // What the AI sees of the player now
        if (Mode.IsWalking)
        {
            var on = Mode.ActiveTargetSpEffects.Where(s => Mode.HasTargetSpEffect(s.Key)).Select(s => $"{s.Key}").ToList();
            ImGui.TextDisabled(LOC.Get(Mode.Guarding ? "LUA_FirstPerson_Guarding" : "LUA_FirstPerson_Not_Guarding"));
            ImGui.TextDisabled(LOC.Get("LUA_FirstPerson_Target_SpEffects", on.Count == 0 ? "-" : string.Join(", ", on)));
        }
    }

    /// <summary>How each action reaches the AI</summary>
    private void DisplayActions()
    {
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("LUA_FirstPerson_Actions_Header"), LOC.Get("LUA_FirstPerson_Actions_Header_TT"));

        var tested = Editor.Viewer.Interrupts.TestedTypes.ToHashSet();
        var flags = ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersInnerH | ImGuiTableFlags.SizingStretchProp;
        if (!ImGui.BeginTable("luaFirstPersonActions", 4, flags))
            return;

        ImGui.TableSetupColumn(LOC.Get("LUA_FirstPerson_Col_Action"), ImGuiTableColumnFlags.WidthStretch, 0.22f);
        ImGui.TableSetupColumn(LOC.Get("LUA_FirstPerson_Col_Interrupt"), ImGuiTableColumnFlags.WidthStretch, 0.32f);
        ImGui.TableSetupColumn(LOC.Get("LUA_FirstPerson_Col_SpEffects"), ImGuiTableColumnFlags.WidthStretch, 0.28f);
        ImGui.TableSetupColumn(LOC.Get("LUA_FirstPerson_Col_Duration"), ImGuiTableColumnFlags.WidthStretch, 0.18f);
        ImGui.TableHeadersRow();

        foreach (var action in Actions)
        {
            ImGui.TableNextRow();

            ImGui.TableSetColumnIndex(0);
            ImGui.AlignTextToFramePadding();
            ImGui.TextUnformatted($"{LuaFirstPerson.ActionName(action)}  ({KeyOf(action)})");
            GUI.Tooltip(LOC.Get($"LUA_FirstPerson_Action_{action}_TT"));

            ImGui.TableSetColumnIndex(1);
            InterruptCombo(action, tested);

            ImGui.TableSetColumnIndex(2);
            SpEffectsInput(action);

            ImGui.TableSetColumnIndex(3);
            ImGui.SetNextItemWidth(-1);
            ImGui.DragFloat($"##luaFpDuration_{action}", ref LuaFirstPerson.DurationOf(action), 0.05f, 0.05f, 60f, "%.2f s");
            GUI.Tooltip(LOC.Get(action == LuaPlayerAction.Guard ? "LUA_FirstPerson_Duration_Guard_TT" : "LUA_FirstPerson_Duration_TT"));
        }
        ImGui.EndTable();

        // An attack that lands: Damaged too, when the character is within reach in front of the player
        var cfg = CFG.Current;
        ImGui.Checkbox($"{LOC.Get("LUA_FirstPerson_Attack_Damaged")}##luaFpDamaged", ref cfg.LuaEditor_FirstPerson_Attack_Damaged);
        GUI.Tooltip(LOC.Get("LUA_FirstPerson_Attack_Damaged_TT"));
        ImGui.BeginDisabled(!cfg.LuaEditor_FirstPerson_Attack_Damaged);
        ImGui.SameLine();
        ImGui.SetNextItemWidth(120 * DPI.UIScale());
        ImGui.DragFloat($"##luaFpReach", ref cfg.LuaEditor_FirstPerson_Attack_Reach, 0.05f, 0.1f, 50f, "%.2f m");
        GUI.Tooltip(LOC.Get("LUA_FirstPerson_Attack_Reach_TT"));
        ImGui.EndDisabled();
    }

    private static string KeyOf(LuaPlayerAction action) => action switch
    {
        LuaPlayerAction.Attack => LOC.Get("LUA_FirstPerson_Left_Click"),
        LuaPlayerAction.Guard => LOC.Get("LUA_FirstPerson_Right_Click"),
        LuaPlayerAction.Estus => InputManager.GetHint(KeybindID.LuaEditor_FirstPerson_Estus),
        LuaPlayerAction.Roll => InputManager.GetHint(KeybindID.LuaEditor_FirstPerson_Roll),
        _ => InputManager.GetHint(KeybindID.MapEditor_FirstPerson_Jump),
    };

    /// <summary>The interrupt an action fires: none, one the scripts react to, or one the goal tests (marked)</summary>
    private static void InterruptCombo(LuaPlayerAction action, HashSet<string> tested)
    {
        ref var current = ref LuaFirstPerson.InterruptOf(action);
        var none = LOC.Get("LUA_FirstPerson_Interrupt_None");
        string Label(string type) => tested.Contains(type) ? $"{type}  {LOC.Get("LUA_FirstPerson_Tested")}" : type;

        ImGui.SetNextItemWidth(-1);
        if (!ImGui.BeginCombo($"##luaFpInterrupt_{action}", string.IsNullOrEmpty(current) ? none : Label(current)))
            return;

        if (ImGui.Selectable($"{none}##luaFpInterruptNone_{action}", string.IsNullOrEmpty(current)))
            current = "";

        foreach (var type in tested.OrderBy(t => t).Concat(CommonInterrupts.Where(t => !tested.Contains(t))))
        {
            if (ImGui.Selectable($"{Label(type)}##luaFpInterrupt_{action}_{type}", current == type))
                current = type;
        }
        ImGui.EndCombo();
    }

    /// <summary>The SpEffect IDs an action puts on the target, as typed: applied when the field is left (Escape undoes)</summary>
    private void SpEffectsInput(LuaPlayerAction action)
    {
        ref var stored = ref LuaFirstPerson.SpEffectsOf(action);
        if (!SpEffectEdits.TryGetValue(action, out var text))
            text = stored;

        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint($"##luaFpSpEffects_{action}", LOC.Get("LUA_FirstPerson_SpEffects_Hint"), ref text, 128);
        var committed = GUI.InputCommitted();
        var active = ImGui.IsItemActive();
        GUI.Tooltip(LOC.Get("LUA_FirstPerson_SpEffects_TT"));

        if (committed)
        {
            // Kept as the IDs read from it, so a typo doesn't stay hidden in the setting
            stored = string.Join(", ", LuaFirstPerson.ParseIds(text));
            SpEffectEdits.Remove(action);
        }
        else if (active)
            SpEffectEdits[action] = text;
        else
            SpEffectEdits.Remove(action);
    }

    /// <summary>How the player moves</summary>
    private static void DisplayMovement()
    {
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("LUA_FirstPerson_Movement_Header"), "");

        var cfg = CFG.Current;
        Drag("LUA_FirstPerson_Walk_Speed", ref cfg.LuaEditor_FirstPerson_Walk_Speed, 0.05f, 0.1f, 30f, "%.2f m/s");
        Drag("LUA_FirstPerson_Run_Speed", ref cfg.LuaEditor_FirstPerson_Run_Speed, 0.05f, 0.1f, 30f, "%.2f m/s");
        Drag("LUA_FirstPerson_Eye_Height", ref cfg.LuaEditor_FirstPerson_Eye_Height, 0.01f, 0.1f, 20f, "%.2f m");
        Drag("LUA_FirstPerson_Jump_Speed", ref cfg.LuaEditor_FirstPerson_Jump_Speed, 0.05f, 0.5f, 30f, "%.2f m/s");
        Drag("LUA_FirstPerson_Roll_Distance", ref cfg.LuaEditor_FirstPerson_Roll_Distance, 0.05f, 0.1f, 20f, "%.2f m");
        Drag("LUA_FirstPerson_Roll_Time", ref cfg.LuaEditor_FirstPerson_Roll_Time, 0.01f, 0.05f, 5f, "%.2f s");
        ImGui.Checkbox($"{LOC.Get("LUA_FirstPerson_Crosshair")}##luaFpCrosshair", ref cfg.LuaEditor_FirstPerson_Crosshair);
    }

    private static void Drag(string key, ref float value, float speed, float min, float max, string format)
    {
        ImGui.SetNextItemWidth(160 * DPI.UIScale());
        ImGui.DragFloat($"{LOC.Get(key)}##{key}", ref value, speed, min, max, format);
    }
}
