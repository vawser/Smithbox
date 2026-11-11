using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Keybinds;
using System.Numerics;

namespace StudioCore.Editors.BehaviorEditor;

/// <summary>
/// P3 structure edits: add/duplicate/remove states, animations, transitions and conditions, move transitions and
/// conditions, and resize state/condition data. States and animations are only appended (never inserted), so adding
/// one renumbers nothing; deleting one renumbers the known references with <see cref="BhvIndexRemap"/> after a
/// confirmation that says what will change. Every edit is one <see cref="BhvStructureChange"/>.
/// </summary>
public class BhvStructureEdits
{
    private readonly BhvEditorView View;

    public BhvStructureEdits(BhvEditorView view)
    {
        View = view;
    }

    private BhvSelection Selection => View.Selection;

    private void Run(BhvFileWrapper file, Action<BHV> apply, Action select)
    {
        var action = new BhvStructureChange(file, apply);

        // Select what the edit made after execute and redo; after undo the selection is just kept in range
        action.OnDone = undo =>
        {
            if (Selection.SelectedFile != file)
                return;

            if (!undo)
                select?.Invoke();

            Selection.Validate();
        };

        // Run at the end of the frame: the menus are drawn inside list loops that index the lists being changed
        Pending.Add(action);
    }

    private readonly List<BhvStructureChange> Pending = new();

    /// <summary>Executes the structure edits requested this frame; called after every window has drawn</summary>
    public void Flush()
    {
        if (Pending.Count == 0)
            return;

        var actions = Pending.ToList();
        Pending.Clear();

        foreach (var action in actions)
            View.Edit(action);
    }

    #region States
    public void AddState(BhvFileWrapper file)
    {
        var at = file.BHV.States.Count;
        Run(file, BhvStructureOps.AddState, () => Selection.SelectState(at, true));
    }

    public void DuplicateState(BhvFileWrapper file, int index)
    {
        var b = file.BHV;
        if (index < 0 || index >= b.States.Count)
            return;

        var at = b.States.Count;
        Run(file, b => BhvStructureOps.DuplicateState(b, index), () => Selection.SelectState(at, true));
    }

    public void DeleteState(BhvFileWrapper file, int index)
    {
        var b = file.BHV;
        if (index < 0 || index >= b.States.Count)
            return;

        var (transitions, preamble) = BhvIndexRemap.CountStateRefs(b, index);
        var shifted = b.States.Count - index - 1;
        var message = LOC.Get("BHV_Delete_State_Message", BhvDisplay.StateLabel(b, index), transitions, preamble, shifted);

        Confirm(message, preamble > 0, () => Run(file, b =>
        {
            var dangling = BhvStructureOps.DeleteState(b, index);
            Smithbox.Log(this, LOC.Get("BHV_Delete_State_Log", index, file.Name, transitions, dangling));
        }, () => Selection.SelectState(Math.Min(index, file.BHV.States.Count - 1))));
    }
    #endregion

    #region Animations
    public void AddAnimation(BhvFileWrapper file)
    {
        var at = file.BHV.Animations.Count;
        Run(file, BhvStructureOps.AddAnimation, () => Selection.SelectAnimation(at, true));
    }

    public void DuplicateAnimation(BhvFileWrapper file, int index)
    {
        var b = file.BHV;
        if (index < 0 || index >= b.Animations.Count)
            return;

        var at = b.Animations.Count;
        Run(file, b => BhvStructureOps.DuplicateAnimation(b, index), () => Selection.SelectAnimation(at, true));
    }

    public void DeleteAnimation(BhvFileWrapper file, int index)
    {
        var b = file.BHV;
        if (index < 0 || index >= b.Animations.Count)
            return;

        var users = BhvIndexRemap.CountAnimationRefs(b, index);
        var shifted = b.Animations.Count - index - 1;
        var message = LOC.Get("BHV_Delete_Animation_Message", BhvDisplay.AnimationLabel(b, index), users, shifted);

        Confirm(message, users > 0, () => Run(file, b => BhvStructureOps.DeleteAnimation(b, index),
            () => Selection.SelectAnimation(Math.Min(index, file.BHV.Animations.Count - 1))));
    }
    #endregion

    #region Transitions
    /// <summary>Appends a transition; with no target it leads to the state itself (skipped until retargeted)</summary>
    public void AddTransition(BhvFileWrapper file, int stateIndex, int target = -1)
    {
        var b = file.BHV;
        if (stateIndex < 0 || stateIndex >= b.States.Count)
            return;

        var at = b.States[stateIndex].Transitions.Count;
        Run(file, b => BhvStructureOps.AddTransition(b, stateIndex, target),
            () => { Selection.SelectState(stateIndex); Selection.SelectTransition(at); });
    }

    public void DuplicateTransition(BhvFileWrapper file, int stateIndex, int index)
    {
        var list = file.BHV.States.ElementAtOrDefault(stateIndex)?.Transitions;
        if (list == null || index < 0 || index >= list.Count)
            return;

        Run(file, b => BhvStructureOps.DuplicateTransition(b, stateIndex, index),
            () => { Selection.SelectState(stateIndex); Selection.SelectTransition(index + 1); });
    }

    public void DeleteTransition(BhvFileWrapper file, int stateIndex, int index)
    {
        var list = file.BHV.States.ElementAtOrDefault(stateIndex)?.Transitions;
        if (list == null || index < 0 || index >= list.Count)
            return;

        Run(file, b => BhvStructureOps.DeleteTransition(b, stateIndex, index),
            () => { Selection.SelectState(stateIndex); Selection.SelectTransition(Math.Min(index, list.Count - 1)); });
    }

    /// <summary>Moves a transition by <paramref name="step"/> in file order (the game tries the last one first)</summary>
    public void MoveTransition(BhvFileWrapper file, int stateIndex, int index, int step)
    {
        var list = file.BHV.States.ElementAtOrDefault(stateIndex)?.Transitions;
        var to = index + step;
        if (list == null || index < 0 || index >= list.Count || to < 0 || to >= list.Count)
            return;

        Run(file, b => BhvStructureOps.MoveTransition(b, stateIndex, index, to),
            () => { Selection.SelectState(stateIndex); Selection.SelectTransition(to); });
    }
    #endregion

    #region Conditions
    public void AddCondition(BhvFileWrapper file, int stateIndex, int transition)
    {
        var t = file.BHV.States.ElementAtOrDefault(stateIndex)?.Transitions.ElementAtOrDefault(transition);
        if (t == null)
            return;

        var at = t.Conditions.Count;
        Run(file, b => BhvStructureOps.AddCondition(t),
            () => { Selection.SelectState(stateIndex); Selection.SelectCondition(transition, at); });
    }

    public void DuplicateCondition(BhvFileWrapper file, int stateIndex, int transition, int index)
    {
        var t = file.BHV.States.ElementAtOrDefault(stateIndex)?.Transitions.ElementAtOrDefault(transition);
        if (t == null || index < 0 || index >= t.Conditions.Count)
            return;

        Run(file, b => BhvStructureOps.DuplicateCondition(t, index),
            () => { Selection.SelectState(stateIndex); Selection.SelectCondition(transition, index + 1); });
    }

    public void DeleteCondition(BhvFileWrapper file, int stateIndex, int transition, int index)
    {
        var t = file.BHV.States.ElementAtOrDefault(stateIndex)?.Transitions.ElementAtOrDefault(transition);
        if (t == null || index < 0 || index >= t.Conditions.Count)
            return;

        Run(file, b => BhvStructureOps.DeleteCondition(t, index), () =>
        {
            Selection.SelectState(stateIndex);
            if (t.Conditions.Count > 0)
                Selection.SelectCondition(transition, Math.Min(index, t.Conditions.Count - 1));
            else
                Selection.SelectTransition(transition);
        });
    }

    /// <summary>Moves a condition by <paramref name="step"/>; conditions run in file order</summary>
    public void MoveCondition(BhvFileWrapper file, int stateIndex, int transition, int index, int step)
    {
        var t = file.BHV.States.ElementAtOrDefault(stateIndex)?.Transitions.ElementAtOrDefault(transition);
        var to = index + step;
        if (t == null || index < 0 || index >= t.Conditions.Count || to < 0 || to >= t.Conditions.Count)
            return;

        Run(file, b => BhvStructureOps.MoveCondition(t, index, to),
            () => { Selection.SelectState(stateIndex); Selection.SelectCondition(transition, to); });
    }
    #endregion

    #region Resize
    /// <summary>The largest size the resize box accepts; vanilla state data and condition data are far smaller</summary>
    public const int MaxBlobLength = 0x1000;

    /// <summary>
    /// Sets a variable-length blob (state data, condition data) to a new length. These blobs carry no size of their
    /// own; the writer recomputes every offset.
    /// </summary>
    public void Resize(BhvFileWrapper file, Func<byte[]> get, Action<byte[]> set, int length)
    {
        var current = get();
        if (length < 0 || length > MaxBlobLength || length == current.Length)
            return;

        Run(file, b => set(BhvStructureOps.Resized(current, length)), null);
    }
    #endregion

    #region Menus
    // Shared by the context menus and the Edit menu

    public void StateItems(BhvFileWrapper file, int index)
    {
        if (ImGui.MenuItem($"{LOC.Get("BHV_Edit_Add_State")}##bhvAddState"))
            AddState(file);
        GUI.Tooltip(LOC.Get("BHV_Edit_Add_State_TT"));

        if (ImGui.MenuItem($"{LOC.Get("BHV_Edit_Duplicate_State")}##bhvDuplicateState", InputManager.GetHint(KeybindID.Duplicate), false, index >= 0))
            DuplicateState(file, index);
        GUI.Tooltip(LOC.Get("BHV_Edit_Duplicate_State_TT"));

        if (ImGui.MenuItem($"{LOC.Get("BHV_Edit_Delete_State")}##bhvDeleteState", InputManager.GetHint(KeybindID.Delete), false, index >= 0))
            DeleteState(file, index);

        if (ImGui.MenuItem($"{LOC.Get("BHV_Edit_Add_Transition")}##bhvAddTransitionFromState", "", false, index >= 0))
            AddTransition(file, index);
        GUI.Tooltip(LOC.Get("BHV_Edit_Add_Transition_TT"));
    }

    public void AnimationItems(BhvFileWrapper file, int index)
    {
        if (ImGui.MenuItem($"{LOC.Get("BHV_Edit_Add_Animation")}##bhvAddAnimation"))
            AddAnimation(file);
        GUI.Tooltip(LOC.Get("BHV_Edit_Add_Animation_TT"));

        if (ImGui.MenuItem($"{LOC.Get("BHV_Edit_Duplicate_Animation")}##bhvDuplicateAnimation", InputManager.GetHint(KeybindID.Duplicate), false, index >= 0))
            DuplicateAnimation(file, index);

        if (ImGui.MenuItem($"{LOC.Get("BHV_Edit_Delete_Animation")}##bhvDeleteAnimation", InputManager.GetHint(KeybindID.Delete), false, index >= 0))
            DeleteAnimation(file, index);
    }

    public void TransitionItems(BhvFileWrapper file, int state, int index)
    {
        var count = file.BHV.States.ElementAtOrDefault(state)?.Transitions.Count ?? 0;
        var valid = index >= 0 && index < count;

        if (ImGui.MenuItem($"{LOC.Get("BHV_Edit_Add_Transition")}##bhvAddTransition", "", false, state >= 0))
            AddTransition(file, state);
        GUI.Tooltip(LOC.Get("BHV_Edit_Add_Transition_TT"));

        if (ImGui.MenuItem($"{LOC.Get("BHV_Edit_Duplicate_Transition")}##bhvDuplicateTransition", InputManager.GetHint(KeybindID.Duplicate), false, valid))
            DuplicateTransition(file, state, index);

        if (ImGui.MenuItem($"{LOC.Get("BHV_Edit_Delete_Transition")}##bhvDeleteTransition", InputManager.GetHint(KeybindID.Delete), false, valid))
            DeleteTransition(file, state, index);

        // The game tries the last transition first, so "sooner" moves toward the end of the list
        if (ImGui.MenuItem($"{LOC.Get("BHV_Edit_Try_Sooner")}##bhvTransitionSooner", "", false, valid && index < count - 1))
            MoveTransition(file, state, index, 1);
        GUI.Tooltip(LOC.Get("BHV_Edit_Try_Order_TT"));

        if (ImGui.MenuItem($"{LOC.Get("BHV_Edit_Try_Later")}##bhvTransitionLater", "", false, valid && index > 0))
            MoveTransition(file, state, index, -1);
        GUI.Tooltip(LOC.Get("BHV_Edit_Try_Order_TT"));

        if (ImGui.MenuItem($"{LOC.Get("BHV_Edit_Add_Condition")}##bhvAddConditionToTransition", "", false, valid))
            AddCondition(file, state, index);
        GUI.Tooltip(LOC.Get("BHV_Edit_Add_Condition_TT"));
    }

    public void ConditionItems(BhvFileWrapper file, int state, int transition, int index)
    {
        var count = file.BHV.States.ElementAtOrDefault(state)?.Transitions.ElementAtOrDefault(transition)?.Conditions.Count ?? 0;
        var valid = index >= 0 && index < count;

        if (ImGui.MenuItem($"{LOC.Get("BHV_Edit_Add_Condition")}##bhvAddCondition"))
            AddCondition(file, state, transition);
        GUI.Tooltip(LOC.Get("BHV_Edit_Add_Condition_TT"));

        if (ImGui.MenuItem($"{LOC.Get("BHV_Edit_Duplicate_Condition")}##bhvDuplicateCondition", InputManager.GetHint(KeybindID.Duplicate), false, valid))
            DuplicateCondition(file, state, transition, index);

        if (ImGui.MenuItem($"{LOC.Get("BHV_Edit_Delete_Condition")}##bhvDeleteCondition", InputManager.GetHint(KeybindID.Delete), false, valid))
            DeleteCondition(file, state, transition, index);

        if (ImGui.MenuItem($"{LOC.Get("BHV_Edit_Run_Sooner")}##bhvConditionSooner", "", false, valid && index > 0))
            MoveCondition(file, state, transition, index, -1);

        if (ImGui.MenuItem($"{LOC.Get("BHV_Edit_Run_Later")}##bhvConditionLater", "", false, valid && index < count - 1))
            MoveCondition(file, state, transition, index, 1);
    }

    /// <summary>The Edit menu's structure items, for whatever is selected</summary>
    public void SelectionItems()
    {
        var file = Selection.SelectedFile;
        if (file == null)
            return;

        switch (Selection.Target)
        {
            case BhvTarget.Condition:
                ConditionItems(file, Selection.StateIndex, Selection.TransitionIndex, Selection.ConditionIndex);
                break;
            case BhvTarget.Transition:
                TransitionItems(file, Selection.StateIndex, Selection.TransitionIndex);
                break;
            case BhvTarget.Animation:
                AnimationItems(file, Selection.AnimationIndex);
                break;
            default:
                StateItems(file, Selection.Target is BhvTarget.State ? Selection.StateIndex : -1);
                break;
        }
    }

    public void DuplicateSelected()
    {
        var file = Selection.SelectedFile;
        if (file == null)
            return;

        switch (Selection.Target)
        {
            case BhvTarget.State: DuplicateState(file, Selection.StateIndex); break;
            case BhvTarget.Transition: DuplicateTransition(file, Selection.StateIndex, Selection.TransitionIndex); break;
            case BhvTarget.Condition: DuplicateCondition(file, Selection.StateIndex, Selection.TransitionIndex, Selection.ConditionIndex); break;
            case BhvTarget.Animation: DuplicateAnimation(file, Selection.AnimationIndex); break;
        }
    }

    public void DeleteSelected()
    {
        var file = Selection.SelectedFile;
        if (file == null)
            return;

        switch (Selection.Target)
        {
            case BhvTarget.State: DeleteState(file, Selection.StateIndex); break;
            case BhvTarget.Transition: DeleteTransition(file, Selection.StateIndex, Selection.TransitionIndex); break;
            case BhvTarget.Condition: DeleteCondition(file, Selection.StateIndex, Selection.TransitionIndex, Selection.ConditionIndex); break;
            case BhvTarget.Animation: DeleteAnimation(file, Selection.AnimationIndex); break;
        }
    }
    #endregion

    #region Confirmation
    private string PendingMessage;
    private bool PendingWarning;
    private Action PendingAction;
    private bool OpenRequested;

    private const string PopupId = "###bhvConfirmDelete";

    private void Confirm(string message, bool warning, Action action)
    {
        PendingMessage = message;
        PendingWarning = warning;
        PendingAction = action;
        OpenRequested = true;
    }

    /// <summary>Drawn once per frame at the editor's top level, so the popup's ID matches where it's opened</summary>
    public void DrawPopups()
    {
        if (OpenRequested)
        {
            ImGui.OpenPopup(PopupId);
            OpenRequested = false;
        }

        ImGui.SetNextWindowSize(new Vector2(520, 0), ImGuiCond.Appearing);

        if (!ImGui.BeginPopupModal($"{LOC.Get("BHV_Delete_Title")}{PopupId}", ImGuiWindowFlags.AlwaysAutoResize))
            return;

        ImGui.PushTextWrapPos(500);
        ImGui.TextWrapped(PendingMessage ?? "");
        GUI.Spacer();
        ImGui.TextColored(PendingWarning ? UI.Current.ImGui_Warning_Text_Color : UI.Current.ImGui_Default_Text_Color,
            LOC.Get("BHV_Delete_Unknown_Refs"));
        ImGui.PopTextWrapPos();

        GUI.Spacer();

        if (ImGui.Button($"{LOC.Get("BHV_Delete_Confirm")}##bhvDeleteConfirm", new Vector2(120, 0)))
        {
            PendingAction?.Invoke();
            PendingAction = null;
            ImGui.CloseCurrentPopup();
        }

        ImGui.SameLine();

        if (ImGui.Button($"{LOC.Get("BHV_Delete_Cancel")}##bhvDeleteCancel", new Vector2(120, 0)) || ImGui.IsKeyPressed(ImGuiKey.Escape))
        {
            PendingAction = null;
            ImGui.CloseCurrentPopup();
        }

        ImGui.EndPopup();
    }
    #endregion
}
