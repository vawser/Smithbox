using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using System.Collections;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.HavokEditor;

/// <summary>
/// New Action: from a state's (or its selector's) right-click menu, a dialog that adds a new state playing a new
/// animation, copied from that state, with an event and a wildcard transition into it, as one undo step. Applied with
/// the dialog's Apply button only, as it has several inputs. See Claude/Documentation/Behavior.md (Havok behavior edits).
/// </summary>
public partial class HavokBehaviorActionsView
{
    private object NewActionState;
    private string NewActionStateName = "";
    private string NewActionSelectorName = "";
    private int NewActionAnimId = 0;
    private bool NewActionWildcard = true;
    private bool NewActionNewEvent = true;
    private string NewActionEventName = "";
    private int NewActionEventIndex = -1;

    private static readonly Regex NewActionClipAnimation = new(@"^a(\d+)_(\d+)$", RegexOptions.IgnoreCase);

    /// <summary>
    /// The menu item for a state, or a selector held by a state (disabled, with the reason, when it can't be the
    /// template)
    /// </summary>
    public void DisplayNewActionItem(object obj, Source source)
    {
        var symbols = Owner.Symbols;

        if (symbols == null)
            return;

        var typeName = obj?.GetType().Name;

        if (typeName is not ("StateInfo" or "hkbStateMachineStateInfo" or "CustomManualSelectorGenerator"))
            return;

        var state = HavokBehaviorOperations.FindTemplateState(symbols, obj);
        string reason = null;

        if (state == null)
            reason = LOC.Get("HAVOK_NewAction_Error_No_State_Selected");
        else if (HavokBehaviorOperations.FindStateSelector(symbols, state, out _, out var error) == null)
            reason = error;

        if (ImGui.MenuItem(LOC.Get("HAVOK_NewAction_Menu"), "", false, reason == null))
            RequestNewAction(state, source);

        // Shown on the disabled item too, so the reason can be read
        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
            ImGui.SetTooltip(reason ?? LOC.Get("HAVOK_NewAction_Menu_TT"));
    }

    public void RequestNewAction(object state, Source source)
    {
        var selector = HavokBehaviorOperations.FindStateSelector(Owner.Symbols, state, out _, out _);

        if (selector == null)
            return;

        NewActionState = state;
        RequestSource = source;

        NewActionStateName = Name(state) + "_New";
        NewActionSelectorName = Name(selector) + "_New";
        NewActionAnimId = Convert.ToInt32(HavokBehaviorSymbols.GetValue(selector, "m_animId") ?? 0);
        NewActionWildcard = true;
        NewActionNewEvent = true;
        NewActionEventName = "W_" + NewActionStateName;
        NewActionEventIndex = -1;

        Request(Dialog.NewAction);
    }

    private static string Name(object obj) => HavokBehaviorSymbols.GetValue(obj, "m_name") as string ?? "";

    private void DisplayNewAction(out bool canApply)
    {
        canApply = false;

        var symbols = Owner.Symbols;
        var selector = HavokBehaviorOperations.FindStateSelector(symbols, NewActionState, out var placement, out var templateError);

        if (selector == null)
        {
            ImGui.TextColored(UI.Current.ImGui_Invalid_Text_Color, templateError);
            return;
        }

        var machine = placement.StateMachine;

        // Template
        GUI.SimpleHeader(LOC.Get("HAVOK_NewAction_Template_Header"), LOC.Get("HAVOK_NewAction_Template_Header_TT"));

        ImGui.Text(LOC.Get("HAVOK_NewAction_Template_State", Name(NewActionState), HavokBehaviorSymbols.GetValue(NewActionState, "m_stateId") ?? -1));
        ImGui.Text(LOC.Get("HAVOK_NewAction_Template_Machine", Name(machine)));
        ImGui.Text(LOC.Get("HAVOK_NewAction_Template_Selector", Name(selector), HavokBehaviorSymbols.GetValue(selector, "m_animId") ?? -1));

        // New Action
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("HAVOK_NewAction_New_Header"), LOC.Get("HAVOK_NewAction_New_Header_TT"));

        // The names are applied with the dialog's Apply button
        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##newActionStateName", LOC.Get("HAVOK_NewAction_State_Name_Hint"), ref NewActionStateName, 255);
        GUI.Tooltip(LOC.Get("HAVOK_NewAction_State_Name_TT", HavokBehaviorOperations.GetFreeStateID(machine)));

        ImGui.TextDisabled(LOC.Get("HAVOK_BehaviorActions_New_State_ID", HavokBehaviorOperations.GetFreeStateID(machine)));
        DisplayStateIdHint(machine);

        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##newActionSelectorName", LOC.Get("HAVOK_NewAction_Selector_Name_Hint"), ref NewActionSelectorName, 255);
        GUI.Tooltip(LOC.Get("HAVOK_NewAction_Selector_Name_TT"));

        ImGui.SetNextItemWidth(-1);
        ImGui.InputInt("##newActionAnimId", ref NewActionAnimId);
        GUI.Tooltip(LOC.Get("HAVOK_NewAction_Anim_Id_TT"));

        var users = HavokBehaviorOperations.CountSelectorsWithAnimId(symbols, NewActionAnimId);

        if (NewActionAnimId == Convert.ToInt64(HavokBehaviorSymbols.GetValue(selector, "m_animId") ?? -1))
            ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("HAVOK_NewAction_Same_Anim_Id"));
        else if (users > 0)
            ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("HAVOK_NewAction_Anim_Id_Used", users));

        DisplayNewActionAnimations(selector);

        // Transition
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("HAVOK_NewAction_Transition_Header"), LOC.Get("HAVOK_NewAction_Transition_Header_TT"));

        ImGui.Checkbox($"{LOC.Get("HAVOK_NewAction_Add_Wildcard")}##newActionWildcard", ref NewActionWildcard);
        GUI.Tooltip(LOC.Get("HAVOK_NewAction_Add_Wildcard_TT"));

        // Event
        if (NewActionWildcard)
        {
            GUI.Spacer();
            GUI.SimpleHeader(LOC.Get("HAVOK_NewAction_Event_Header"), LOC.Get("HAVOK_NewAction_Event_Header_TT"));

            if (ImGui.RadioButton($"{LOC.Get("HAVOK_NewAction_Event_New")}##newActionEventNew", NewActionNewEvent))
                NewActionNewEvent = true;
            ImGui.SameLine();
            if (ImGui.RadioButton($"{LOC.Get("HAVOK_NewAction_Event_Existing")}##newActionEventExisting", !NewActionNewEvent))
                NewActionNewEvent = false;

            if (NewActionNewEvent)
            {
                ImGui.SetNextItemWidth(-1);
                ImGui.InputTextWithHint("##newActionEventName", LOC.Get("HAVOK_BehaviorActions_New_Event_Hint"), ref NewActionEventName, 255);
            }
            else
            {
                var preview = HavokBehaviorSymbols.GetName(symbols.EventNames, NewActionEventIndex) ?? LOC.Get("HAVOK_BehaviorActions_Pick_Event");

                if (ImGui.BeginCombo(LOC.Get("HAVOK_BehaviorIndex_Role_Event"), preview, ImGuiComboFlags.HeightLarge))
                {
                    DisplayFilteredNames(symbols.EventNames, ref NewActionEventIndex);
                    ImGui.EndCombo();
                }
            }

            ImGui.TextDisabled(LOC.Get("HAVOK_NewAction_Event_Script_Note"));
        }

        canApply = NewActionStateName.Trim().Length > 0 && NewActionSelectorName.Trim().Length > 0 &&
            (!NewActionWildcard || (NewActionNewEvent ? NewActionEventName.Trim().Length > 0 : NewActionEventIndex >= 0));
    }

    /// <summary>
    /// One line per category of the template's clips: whether the new animation has a time act entry and a file
    /// </summary>
    private void DisplayNewActionAnimations(object selector)
    {
        var index = Owner.Animations;

        if (HavokBehaviorSymbols.GetValue(selector, "m_generators") is not IList generators)
            return;

        foreach (var clip in generators)
        {
            if (clip?.GetType().Name != "hkbClipGenerator")
                continue;

            var match = NewActionClipAnimation.Match(HavokBehaviorSymbols.GetValue(clip, "m_animationName") as string ?? "");

            if (!match.Success)
                continue;

            var animation = $"a{match.Groups[1].Value}_{NewActionAnimId.ToString().PadLeft(match.Groups[2].Value.Length, '0')}";

            if (index == null || !index.IsReady)
            {
                ImGui.TextDisabled($"  {animation}");
                continue;
            }

            var timeAct = HavokAnimationIndex.TryParseAnimationName(animation, index.CategorySize, out var fullId) &&
                index.TryGetTimeAct(fullId, out var timeActName)
                    ? LOC.Get("HAVOK_AnimationRow_Time_Act", timeActName)
                    : LOC.Get("HAVOK_AnimationRow_No_Time_Act");

            var file = index.HasAnimationFile(animation) ? LOC.Get("HAVOK_AnimationRow_File") : LOC.Get("HAVOK_AnimationRow_No_File");

            ImGui.TextDisabled($"  {animation}:  {timeAct} · {file}");
        }
    }

    private EditorAction ApplyNewAction(HavokBehaviorContext context, out object show, out string error)
    {
        var action = HavokBehaviorOperations.NewAction(context, NewActionState, NewActionStateName, NewActionSelectorName, NewActionAnimId,
            NewActionWildcard, NewActionWildcard && NewActionNewEvent ? NewActionEventName : null, NewActionEventIndex,
            out show, out error);

        if (action != null && NewActionWildcard)
        {
            var eventName = NewActionNewEvent ? NewActionEventName.Trim() : HavokBehaviorSymbols.GetName(context.Symbols.EventNames, NewActionEventIndex);
            Smithbox.Log(this, LOC.Get("HAVOK_NewAction_Created", eventName));
        }

        return action;
    }
}
