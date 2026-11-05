using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System.Collections;

namespace StudioCore.Editors.HavokEditor;

/// <summary>
/// The behavior edits made of several changes at once (Add Animation, Clone State, Add Transition,
/// Add Variable, Unlink): the Outline's right-click menu, and the dialog for each.
/// Each edit is one undo step (<see cref="HavokCompoundAction"/>).
/// </summary>
public class HavokBehaviorActionsView
{
    private HavokEditorView View;
    private ProjectEntry Project;
    private HavokBehaviorView Owner;

    private enum Dialog { None, AddAnimation, CloneState, AddTransition, AddVariable }

    private Dialog PendingDialog = Dialog.None;
    private Dialog OpenDialog = Dialog.None;

    // Targets
    private object TargetSelector;
    private object TargetState;
    private object TargetStateMachine;

    // Inputs
    private int AnimationCategory = 0;
    private string CloneName = "";
    private bool CloneCopyShared = false;
    private int TransitionFromIndex = -1;   // -1: any state (wildcard)
    private int TransitionEventIndex = -1;
    private bool TransitionNewEvent = false;
    private string TransitionNewEventName = "";
    private int TransitionToIndex = 0;
    private string VariableName = "";
    private int VariableTypeIndex = 0;
    private int VariableInitialValue = 0;
    private float VariableInitialReal = 0f;

    private string LastError = null;

    public HavokBehaviorActionsView(HavokEditorView view, HavokBehaviorView ownerView, ProjectEntry project)
    {
        View = view;
        Project = project;
        Owner = ownerView;
    }

    private HavokBehaviorContext Context => new(View.Selection.SelectedFileWrapper, Owner.Symbols, Project);

    #region Menu
    /// <summary>
    /// The Outline's right-click menu for a node
    /// </summary>
    public void DisplayNodeMenu(object node)
    {
        var typeName = node.GetType().Name;

        if (typeName == "CustomManualSelectorGenerator")
        {
            if (ImGui.Selectable(LOC.Get("HAVOK_BehaviorActions_Add_Animation")))
                RequestAddAnimation(node);
            GUI.Tooltip(LOC.Get("HAVOK_BehaviorActions_Add_Animation_TT"));
        }

        if (typeName is "StateInfo" or "hkbStateMachineStateInfo")
        {
            if (ImGui.Selectable(LOC.Get("HAVOK_BehaviorActions_Clone_State")))
                RequestCloneState(node);
            GUI.Tooltip(LOC.Get("HAVOK_BehaviorActions_Clone_State_TT"));

            if (ImGui.Selectable(LOC.Get("HAVOK_BehaviorActions_Add_Transition_From_State")))
                RequestAddTransition(Owner.Symbols.FindOwningStateMachine(Owner.Symbols.GetParents(node).FirstOrDefault()), node);
            GUI.Tooltip(LOC.Get("HAVOK_BehaviorActions_Add_Transition_TT"));
        }

        if (typeName == "hkbStateMachine")
        {
            if (ImGui.Selectable(LOC.Get("HAVOK_BehaviorActions_Add_Transition")))
                RequestAddTransition(node, null);
            GUI.Tooltip(LOC.Get("HAVOK_BehaviorActions_Add_Transition_TT"));
        }

        if (ImGui.Selectable(LOC.Get("HAVOK_BehaviorActions_Copy_Name")))
            PlatformUtils.Instance.SetClipboardText(HavokBehaviorSymbols.GetValue(node, "m_name") as string ?? "");
    }

    public void RequestAddAnimation(object selector)
    {
        TargetSelector = selector;
        AnimationCategory = 0;

        // Default to the category after the selector's last clip, as the game's selectors list them in order
        if (HavokBehaviorSymbols.GetValue(selector, "m_generators") is IList generators)
        {
            foreach (var generator in generators)
            {
                var name = HavokBehaviorSymbols.GetValue(generator, "m_animationName") as string;

                if (Owner.Animations != null &&
                    HavokAnimationIndex.TryParseAnimationName(name, Owner.Animations.CategorySize, out var fullId))
                {
                    AnimationCategory = Math.Max(AnimationCategory, (int)(fullId / Owner.Animations.CategorySize) + 1);
                }
            }
        }

        Request(Dialog.AddAnimation);
    }

    public void RequestCloneState(object state)
    {
        TargetState = state;
        TargetStateMachine = Owner.Symbols.FindOwningStateMachine(Owner.Symbols.GetParents(state).FirstOrDefault());
        CloneName = (HavokBehaviorSymbols.GetValue(state, "m_name") as string ?? "") + "_Copy";
        CloneCopyShared = false;

        Request(Dialog.CloneState);
    }

    public void RequestAddTransition(object stateMachine, object fromState)
    {
        if (stateMachine == null)
            return;

        TargetStateMachine = stateMachine;

        var states = (HavokBehaviorSymbols.GetValue(stateMachine, "m_states") as IList)?.Cast<object>().ToList() ?? new();

        TransitionFromIndex = fromState == null ? -1 : states.IndexOf(fromState);
        TransitionEventIndex = -1;
        TransitionNewEvent = false;
        TransitionNewEventName = "";
        TransitionToIndex = 0;

        Request(Dialog.AddTransition);
    }

    public void RequestAddVariable()
    {
        VariableName = "";
        VariableTypeIndex = 0;
        VariableInitialValue = 0;
        VariableInitialReal = 0f;

        Request(Dialog.AddVariable);
    }

    private void Request(Dialog dialog)
    {
        PendingDialog = dialog;
        LastError = null;
    }
    #endregion

    #region Dialogs
    /// <summary>
    /// Draws the open dialog. Called once a frame from the behavior view.
    /// </summary>
    public void DisplayDialogs()
    {
        if (PendingDialog != Dialog.None)
        {
            OpenDialog = PendingDialog;
            PendingDialog = Dialog.None;
            ImGui.OpenPopup("###havokBehaviorActionDialog");
        }

        if (OpenDialog == Dialog.None || Owner.Symbols == null)
            return;

        var title = OpenDialog switch
        {
            Dialog.AddAnimation => LOC.Get("HAVOK_BehaviorActions_Add_Animation"),
            Dialog.CloneState => LOC.Get("HAVOK_BehaviorActions_Clone_State"),
            Dialog.AddTransition => LOC.Get("HAVOK_BehaviorActions_Add_Transition"),
            Dialog.AddVariable => LOC.Get("HAVOK_BehaviorActions_Add_Variable"),
            _ => ""
        };

        ImGui.SetNextWindowSize(new System.Numerics.Vector2(520, 0) * DPI.UIScale(), ImGuiCond.Appearing);

        if (!ImGui.BeginPopupModal($"{title}###havokBehaviorActionDialog", ImGuiWindowFlags.AlwaysAutoResize))
        {
            OpenDialog = Dialog.None;
            return;
        }

        var apply = false;

        switch (OpenDialog)
        {
            case Dialog.AddAnimation: apply = DisplayAddAnimation(); break;
            case Dialog.CloneState: apply = DisplayCloneState(); break;
            case Dialog.AddTransition: apply = DisplayAddTransition(); break;
            case Dialog.AddVariable: apply = DisplayAddVariable(); break;
        }

        if (LastError != null)
            ImGui.TextColored(UI.Current.ImGui_Invalid_Text_Color, LastError);

        ImGui.Separator();

        if (ImGui.Button($"{LOC.Get("HAVOK_BehaviorActions_Apply")}##actionApply") || apply)
        {
            if (Apply())
            {
                OpenDialog = Dialog.None;
                ImGui.CloseCurrentPopup();
            }
        }

        ImGui.SameLine();

        if (ImGui.Button($"{LOC.Get("HAVOK_BehaviorActions_Cancel")}##actionCancel") || ImGui.IsKeyPressed(ImGuiKey.Escape))
        {
            OpenDialog = Dialog.None;
            ImGui.CloseCurrentPopup();
        }

        ImGui.EndPopup();
    }

    private bool DisplayAddAnimation()
    {
        var animations = Owner.Animations;
        var animationID = Convert.ToInt64(HavokBehaviorSymbols.GetValue(TargetSelector, "m_animId") ?? 0);
        var size = animations?.CategorySize ?? 1000000;

        ImGui.Text(LOC.Get("HAVOK_BehaviorActions_Selector", HavokBehaviorSymbols.GetOutlineLabel(TargetSelector)));

        ImGui.SetNextItemWidth(120 * DPI.UIScale());
        ImGui.InputInt(LOC.Get("HAVOK_BehaviorActions_Category"), ref AnimationCategory);
        AnimationCategory = Math.Clamp(AnimationCategory, 0, size == 10000 ? 99 : 999);
        GUI.Tooltip(LOC.Get("HAVOK_BehaviorActions_Category_TT"));

        var name = animations?.FormatAnimationName(AnimationCategory, animationID)
            ?? $"a{AnimationCategory:D3}_{animationID:D6}";

        ImGui.Text(LOC.Get("HAVOK_BehaviorActions_New_Clip", name));

        if (animations is { IsReady: true })
        {
            var fullId = (long)AnimationCategory * size + animationID;
            var hasTimeAct = animations.TryGetTimeAct(fullId, out var timeAct);
            var hasFile = animations.HasAnimationFile(name);

            ImGui.TextColored(hasTimeAct && hasFile ? UI.Current.ImGui_Benefit_Text_Color : UI.Current.ImGui_Warning_Text_Color,
                $"{(hasTimeAct ? LOC.Get("HAVOK_AnimationRow_Time_Act", timeAct) : LOC.Get("HAVOK_AnimationRow_No_Time_Act"))}  ·  " +
                $"{(hasFile ? LOC.Get("HAVOK_AnimationRow_File") : LOC.Get("HAVOK_AnimationRow_No_File"))}");
        }

        return false;
    }

    private bool DisplayCloneState()
    {
        ImGui.Text(LOC.Get("HAVOK_BehaviorActions_State", HavokBehaviorSymbols.GetOutlineLabel(TargetState)));

        ImGui.SetNextItemWidth(-1);
        var enter = ImGui.InputText("##cloneName", ref CloneName, 255, ImGuiInputTextFlags.EnterReturnsTrue);

        ImGui.Checkbox(LOC.Get("HAVOK_BehaviorActions_Copy_Shared"), ref CloneCopyShared);
        GUI.Tooltip(LOC.Get("HAVOK_BehaviorActions_Copy_Shared_TT"));

        var nextId = HavokBehaviorOperations.GetFreeStateID(TargetStateMachine);
        ImGui.TextDisabled(LOC.Get("HAVOK_BehaviorActions_New_State_ID", nextId));

        return enter;
    }

    private bool DisplayAddTransition()
    {
        var states = HavokBehaviorSymbols.GetStates(TargetStateMachine);
        var eventNames = Owner.Symbols.EventNames;

        ImGui.Text(LOC.Get("HAVOK_BehaviorActions_State_Machine", HavokBehaviorSymbols.GetOutlineLabel(TargetStateMachine)));

        // From
        var fromPreview = TransitionFromIndex < 0 || TransitionFromIndex >= states.Count
            ? LOC.Get("HAVOK_BehaviorActions_Any_State")
            : $"{states[TransitionFromIndex].Name} [id {states[TransitionFromIndex].StateId}]";

        if (ImGui.BeginCombo(LOC.Get("HAVOK_BehaviorActions_From"), fromPreview, ImGuiComboFlags.HeightLarge))
        {
            if (ImGui.Selectable(LOC.Get("HAVOK_BehaviorActions_Any_State"), TransitionFromIndex < 0))
                TransitionFromIndex = -1;

            for (var i = 0; i < states.Count; i++)
            {
                if (ImGui.Selectable($"{states[i].Name} [id {states[i].StateId}]##from{i}", i == TransitionFromIndex))
                    TransitionFromIndex = i;
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("HAVOK_BehaviorActions_From_TT"));

        // Event
        ImGui.Checkbox(LOC.Get("HAVOK_BehaviorActions_New_Event"), ref TransitionNewEvent);

        if (TransitionNewEvent)
        {
            ImGui.SetNextItemWidth(-1);
            ImGui.InputTextWithHint("##newEventName", LOC.Get("HAVOK_BehaviorActions_New_Event_Hint"), ref TransitionNewEventName, 255);
        }
        else
        {
            var eventPreview = HavokBehaviorSymbols.GetName(eventNames, TransitionEventIndex) ?? LOC.Get("HAVOK_BehaviorActions_Pick_Event");

            if (ImGui.BeginCombo(LOC.Get("HAVOK_BehaviorIndex_Role_Event"), eventPreview, ImGuiComboFlags.HeightLarge))
            {
                DisplayFilteredNames(eventNames, ref TransitionEventIndex);
                ImGui.EndCombo();
            }
        }

        // To
        if (states.Count > 0)
        {
            TransitionToIndex = Math.Clamp(TransitionToIndex, 0, states.Count - 1);

            if (ImGui.BeginCombo(LOC.Get("HAVOK_BehaviorActions_To"), $"{states[TransitionToIndex].Name} [id {states[TransitionToIndex].StateId}]",
                ImGuiComboFlags.HeightLarge))
            {
                for (var i = 0; i < states.Count; i++)
                {
                    if (ImGui.Selectable($"{states[i].Name} [id {states[i].StateId}]##to{i}", i == TransitionToIndex))
                        TransitionToIndex = i;
                }

                ImGui.EndCombo();
            }
        }

        return false;
    }

    private static string NameFilter = "";

    private static void DisplayFilteredNames(IList names, ref int selectedIndex)
    {
        if (ImGui.IsWindowAppearing())
        {
            NameFilter = "";
            ImGui.SetKeyboardFocusHere();
        }

        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##nameFilter", LOC.Get("HAVOK_BehaviorIndex_Search_Hint"), ref NameFilter, 255);

        if (names == null)
            return;

        var filtered = new List<int>();

        for (var i = 0; i < names.Count; i++)
        {
            var name = names[i] as string ?? "";

            if (NameFilter == "" || name.Contains(NameFilter, StringComparison.OrdinalIgnoreCase) || i.ToString() == NameFilter)
                filtered.Add(i);
        }

        var clipper = new ImGuiListClipper();
        clipper.Begin(filtered.Count);

        while (clipper.Step())
        {
            for (var row = clipper.DisplayStart; row < clipper.DisplayEnd; row++)
            {
                var i = filtered[row];

                if (ImGui.Selectable($"{i}: {names[i]}##name{i}", i == selectedIndex))
                    selectedIndex = i;
            }
        }

        clipper.End();
    }

    private bool DisplayAddVariable()
    {
        var types = HavokBehaviorOperations.VariableTypeNames;

        ImGui.SetNextItemWidth(-1);
        var enter = ImGui.InputTextWithHint("##variableName", LOC.Get("HAVOK_BehaviorActions_Variable_Name_Hint"), ref VariableName, 255,
            ImGuiInputTextFlags.EnterReturnsTrue);

        VariableTypeIndex = Math.Clamp(VariableTypeIndex, 0, types.Count - 1);

        if (ImGui.BeginCombo(LOC.Get("HAVOK_BehaviorActions_Variable_Type"), types[VariableTypeIndex]))
        {
            for (var i = 0; i < types.Count; i++)
            {
                if (ImGui.Selectable(types[i], i == VariableTypeIndex))
                    VariableTypeIndex = i;
            }

            ImGui.EndCombo();
        }

        if (types[VariableTypeIndex] == "VARIABLE_TYPE_REAL")
        {
            ImGui.InputFloat(LOC.Get("HAVOK_BehaviorActions_Variable_Initial"), ref VariableInitialReal);
            GUI.Tooltip(LOC.Get("HAVOK_BehaviorActions_Variable_Initial_TT"));
        }
        else if (HavokBehaviorOperations.IsWordVariableType(types[VariableTypeIndex]))
        {
            ImGui.InputInt(LOC.Get("HAVOK_BehaviorActions_Variable_Initial"), ref VariableInitialValue);
            GUI.Tooltip(LOC.Get("HAVOK_BehaviorActions_Variable_Initial_TT"));
        }
        else
        {
            ImGui.TextDisabled(LOC.Get("HAVOK_BehaviorActions_Variable_Not_Word"));
        }

        return enter;
    }
    #endregion

    #region Apply
    private bool Apply()
    {
        var context = Context;
        EditorAction action = null;
        object show = null;
        string error = null;

        switch (OpenDialog)
        {
            case Dialog.AddAnimation:
                action = HavokBehaviorOperations.AddAnimation(context, TargetSelector, AnimationCategory, out show, out error);
                break;

            case Dialog.CloneState:
                action = HavokBehaviorOperations.CloneState(context, TargetStateMachine, TargetState, CloneName.Trim(), CloneCopyShared, out show, out error);
                break;

            case Dialog.AddTransition:
            {
                var states = (HavokBehaviorSymbols.GetValue(TargetStateMachine, "m_states") as IList)?.Cast<object>().ToList() ?? new();
                var fromState = TransitionFromIndex >= 0 && TransitionFromIndex < states.Count ? states[TransitionFromIndex] : null;
                var toStateId = HavokBehaviorSymbols.GetStates(TargetStateMachine).ElementAtOrDefault(TransitionToIndex).StateId;

                action = HavokBehaviorOperations.AddTransition(context, TargetStateMachine, fromState,
                    TransitionNewEvent ? -1 : TransitionEventIndex, TransitionNewEvent ? TransitionNewEventName.Trim() : null,
                    toStateId, out show, out error);
                break;
            }

            case Dialog.AddVariable:
                action = HavokBehaviorOperations.AddVariable(context, VariableName.Trim(),
                    HavokBehaviorOperations.VariableTypeNames[VariableTypeIndex],
                    HavokBehaviorOperations.VariableTypeNames[VariableTypeIndex] == "VARIABLE_TYPE_REAL" ? VariableInitialReal : VariableInitialValue,
                    out var index, out error);
                if (action != null)
                    Owner.VariableView.RequestSelect(index);
                break;
        }

        if (action == null)
        {
            LastError = error;
            return false;
        }

        View.ActionManager.ExecuteAction(action);

        if (show != null)
            Owner.OutlineView.NavigateAfterRebuild(show);

        return true;
    }

    /// <summary>
    /// Removes one reference to an object (the Used By list's Unlink)
    /// </summary>
    public void Unlink(object parent, object child)
    {
        var action = HavokBehaviorOperations.Unlink(Context, parent, child, out var error);

        if (action == null)
        {
            Smithbox.LogError(this, error);
            return;
        }

        View.ActionManager.ExecuteAction(action);
    }
    #endregion
}

/// <summary>
/// What a behavior operation works on
/// </summary>
public record HavokBehaviorContext(HavokFileWrapper File, HavokBehaviorSymbols Symbols, ProjectEntry Project);
