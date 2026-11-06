using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
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

    private enum Dialog
    {
        None, AddAnimation, CloneState, AddTransition, AddVariable,
        BindVariable, DuplicateGenerator, DeleteGenerator, DuplicateVariable, DeleteVariable, DuplicateClip
    }

    // Duplicate clip / Delete (several)
    private List<object> TargetObjects = new();
    private List<object> DuplicateClipSelectors = new();
    private int DuplicateClipSelectorIndex = 0;
    private int DuplicateClipCategory = 0;

    /// <summary>
    /// Where an edit was asked for: the Outline shows its result, a list selects it
    /// </summary>
    public enum Source { Outline, List }

    private Dialog PendingDialog = Dialog.None;
    private Dialog OpenDialog = Dialog.None;
    private Source RequestSource = Source.Outline;

    // Bind Variable
    private object TargetObject;
    private List<System.Reflection.FieldInfo> BindMembers = new();
    private int BindMemberIndex = 0;
    private int BindVariableIndex = -1;

    // Duplicate / Delete generator
    private List<HavokBehaviorOperations.HavokPlacement> DuplicatePlacements = new();
    private string DuplicateUnsupported = null;
    private int DuplicatePlacementIndex = 0;
    private string DuplicateName = "";
    private string DuplicateStateName = "";
    private bool DuplicateCopyShared = false;
    private int DuplicateAnimId = 0;
    private List<string> DeleteLines = null;
    private string DeleteBlocked = null;

    // Duplicate / Delete variable
    private int TargetVariable = -1;
    private string DuplicateVariableName = "";
    private HavokBehaviorOperations.HavokVariableDeletion VariableDeletion;

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

        DisplayGeneratorItems(node, Source.Outline);
        DisplayBindingItems(node, Source.Outline);

        if (ImGui.Selectable(LOC.Get("HAVOK_BehaviorActions_Copy_Name")))
            PlatformUtils.Instance.SetClipboardText(HavokBehaviorSymbols.GetValue(node, "m_name") as string ?? "");
    }

    /// <summary>
    /// Whether Duplicate, Delete, Copy and Paste work on the object: a selector, state machine or clip
    /// </summary>
    public static bool IsTransferable(object obj) => obj?.GetType().Name is "CustomManualSelectorGenerator" or "hkbStateMachine" or "hkbClipGenerator";

    /// <summary>
    /// Duplicate, Delete, Copy and Paste, for a selector, state machine or clip.
    /// selection is everything selected in the list (Delete and Copy act on all of it), or null for just obj.
    /// </summary>
    public void DisplayGeneratorItems(object obj, Source source, IReadOnlyList<object> selection = null)
    {
        if (!IsTransferable(obj))
            return;

        var typeName = obj.GetType().Name;
        var targets = selection != null && selection.Contains(obj) ? selection.Where(IsTransferable).ToList() : new List<object> { obj };

        var (duplicateKey, deleteKey) = typeName switch
        {
            "CustomManualSelectorGenerator" => ("HAVOK_BehaviorActions_Duplicate_Selector", "HAVOK_BehaviorActions_Delete_Selector"),
            "hkbStateMachine" => ("HAVOK_BehaviorActions_Duplicate_State_Machine", "HAVOK_BehaviorActions_Delete_State_Machine"),
            _ => ("HAVOK_BehaviorActions_Duplicate_Clip", "HAVOK_BehaviorActions_Delete_Clip")
        };

        if (ImGui.MenuItem(LOC.Get(duplicateKey), InputManager.GetHint(KeybindID.Duplicate)))
            RequestDuplicate(obj, source);
        GUI.Tooltip(LOC.Get(typeName == "hkbClipGenerator" ? "HAVOK_BehaviorActions_Duplicate_Clip_TT" : "HAVOK_BehaviorActions_Duplicate_Generator_TT"));

        if (ImGui.MenuItem(targets.Count > 1 ? LOC.Get("HAVOK_BehaviorActions_Delete_Selected", targets.Count) : LOC.Get(deleteKey),
            InputManager.GetHint(KeybindID.Delete)))
            RequestDeleteGenerators(targets, source);
        GUI.Tooltip(LOC.Get(typeName == "hkbClipGenerator" ? "HAVOK_BehaviorActions_Delete_Clip_TT" : "HAVOK_BehaviorActions_Delete_Generator_TT"));

        if (ImGui.MenuItem(targets.Count > 1 ? LOC.Get("HAVOK_BehaviorActions_Copy_Selected", targets.Count) : LOC.Get("HAVOK_BehaviorActions_Copy"),
            InputManager.GetHint(KeybindID.Copy)))
            CopyGenerators(targets);
        GUI.Tooltip(LOC.Get("HAVOK_BehaviorActions_Copy_TT"));

        DisplayPasteItem(obj, source);
    }

    private void DisplayPasteItem(object target, Source source)
    {
        var label = Clipboard.IsEmpty
            ? LOC.Get("HAVOK_BehaviorActions_Paste")
            : LOC.Get("HAVOK_BehaviorActions_Paste_Count", Clipboard.Count);

        if (ImGui.MenuItem(label, InputManager.GetHint(KeybindID.Paste), false, !Clipboard.IsEmpty))
            Paste(target, source);
        GUI.Tooltip(LOC.Get("HAVOK_BehaviorActions_Paste_TT"));
    }

    /// <summary>
    /// Bind Variable, and Remove Binding (one entry per binding), for an object that can hold bindings
    /// </summary>
    public void DisplayBindingItems(object obj, Source source)
    {
        if (!HavokBehaviorOperations.IsBindable(obj) || Owner.Symbols == null)
            return;

        if (ImGui.Selectable(LOC.Get("HAVOK_BehaviorActions_Bind_Variable")))
            RequestBindVariable(obj, source);
        GUI.Tooltip(LOC.Get("HAVOK_BehaviorActions_Bind_Variable_TT"));

        var bindings = HavokBehaviorOperations.GetOwnBindings(obj);

        if (ImGui.BeginMenu(LOC.Get("HAVOK_BehaviorActions_Remove_Binding"), bindings.Count > 0))
        {
            for (var i = 0; i < bindings.Count; i++)
            {
                if (ImGui.Selectable($"{DescribeBinding(bindings[i])}##removeBinding{i}"))
                    RemoveBinding(obj, bindings[i]);
            }

            ImGui.EndMenu();
        }
        GUI.Tooltip(LOC.Get("HAVOK_BehaviorActions_Remove_Binding_TT"));
    }

    /// <summary>
    /// Duplicate and Delete, for a variable
    /// </summary>
    public void DisplayVariableItems(int index)
    {
        if (ImGui.MenuItem(LOC.Get("HAVOK_BehaviorActions_Duplicate_Variable"), InputManager.GetHint(KeybindID.Duplicate)))
            RequestDuplicateVariable(index);
        GUI.Tooltip(LOC.Get("HAVOK_BehaviorActions_Duplicate_Variable_TT"));

        if (ImGui.MenuItem(LOC.Get("HAVOK_BehaviorActions_Delete_Variable"), InputManager.GetHint(KeybindID.Delete)))
            RequestDeleteVariable(index);
        GUI.Tooltip(LOC.Get("HAVOK_BehaviorActions_Delete_Variable_TT"));

        if (ImGui.MenuItem(LOC.Get("HAVOK_BehaviorActions_Copy"), InputManager.GetHint(KeybindID.Copy)))
            CopyVariable(index);
        GUI.Tooltip(LOC.Get("HAVOK_BehaviorActions_Copy_Variable_TT"));

        DisplayPasteItem(null, Source.List);
    }

    #region Clipboard
    /// <summary>
    /// What Copy took: generators (pasted only into the file they came from, since their indices name that file's
    /// variables, events and animations), or variable snapshots (pasted into any behavior file).
    /// Shared by every view, so a copy in one can be pasted in another.
    /// </summary>
    private class TransferClipboard
    {
        public HavokFileWrapper File;
        public List<object> Generators = new();
        public List<HavokBehaviorOperations.HavokVariableSnapshot> Variables = new();

        public int Count => Generators.Count + Variables.Count;
        public bool IsEmpty => Count == 0;
    }

    private static readonly TransferClipboard Clipboard = new();

    public void CopyGenerators(IReadOnlyList<object> objects)
    {
        var generators = objects.Where(IsTransferable).ToList();

        if (generators.Count == 0)
            return;

        Clipboard.File = View.Selection.SelectedFileWrapper;
        Clipboard.Generators = generators;
        Clipboard.Variables = new();

        Smithbox.Log(this, LOC.Get("HAVOK_BehaviorActions_Copied", generators.Count));
    }

    public void CopyVariable(int index)
    {
        var snapshot = Owner.Symbols == null ? null : HavokBehaviorOperations.SnapshotVariable(Owner.Symbols, index);

        if (snapshot == null)
            return;

        Clipboard.File = View.Selection.SelectedFileWrapper;
        Clipboard.Generators = new();
        Clipboard.Variables = new() { snapshot };

        Smithbox.Log(this, LOC.Get("HAVOK_BehaviorActions_Copied", 1));
    }

    /// <summary>
    /// Pastes the clipboard. Variables go at the end of the variable list. Clips go into the target selector (or the
    /// selector of the target clip) for their category, or the next free one if it's taken. Selectors and state
    /// machines go where the target selector or state machine is held, as a duplicate would.
    /// </summary>
    public void Paste(object target, Source source)
    {
        if (Clipboard.IsEmpty || Owner.Symbols == null)
            return;

        var context = Context;
        var builders = new List<Func<(EditorAction, string)>>();
        var created = new List<object>();
        var pastedVariable = -1;

        foreach (var snapshot in Clipboard.Variables)
        {
            builders.Add(() =>
            {
                var action = HavokBehaviorOperations.PasteVariable(context, snapshot, out var index, out var error);

                if (action != null)
                    pastedVariable = index;

                return (action, error);
            });
        }

        if (Clipboard.Generators.Count > 0)
        {
            if (!ReferenceEquals(Clipboard.File, View.Selection.SelectedFileWrapper))
            {
                Smithbox.LogError(this, LOC.Get("HAVOK_BehaviorActions_Error_Paste_Other_File"));
                return;
            }

            foreach (var source0 in Clipboard.Generators)
                builders.Add(() => BuildPaste(context, source0, target, created));
        }

        var action = HavokBehaviorOperations.Sequence(context, builders, out var errors);

        foreach (var error in errors)
            Smithbox.LogError(this, error);

        if (action == null)
            return;

        View.ActionManager.ExecuteAction(action);

        if (pastedVariable >= 0)
            Owner.VariableView.RequestSelect(pastedVariable);

        RequestSource = source;
        ShowResult(created.FirstOrDefault());
    }

    private (EditorAction, string) BuildPaste(HavokBehaviorContext context, object source, object target, List<object> created)
    {
        var symbols = context.Symbols;
        var label = HavokBehaviorSymbols.GetOutlineLabel(source);

        if (source.GetType().Name == "hkbClipGenerator")
        {
            var selector = target?.GetType().Name switch
            {
                "CustomManualSelectorGenerator" => target,
                "hkbClipGenerator" => HavokBehaviorOperations.GetClipSelectors(symbols, target).FirstOrDefault(),
                _ => null
            };

            if (selector == null)
                return (null, LOC.Get("HAVOK_BehaviorActions_Error_Paste_Clip_Target", label));

            var category = HavokBehaviorOperations.GetClipCategory(source);
            var generators = HavokBehaviorSymbols.GetValue(selector, "m_generators") as IList;

            // Taken (pasting into its own selector): the next free category, as Duplicate does
            if (category < 0 || generators?.Cast<object>().Any(g => g != null && HavokBehaviorOperations.GetClipCategory(g) == category) == true)
                category = HavokBehaviorOperations.GetFreeCategory(selector);

            var action = HavokBehaviorOperations.DuplicateClip(context, source, selector, category, out var newClip, out var error);

            if (action != null)
                created.Add(newClip);

            return (action, error == null ? null : $"{label}: {error}");
        }

        // A selector or state machine: where the target (of either kind) is held
        if (target == null || target.GetType().Name is not ("CustomManualSelectorGenerator" or "hkbStateMachine"))
            return (null, LOC.Get("HAVOK_BehaviorActions_Error_Paste_Generator_Target", label));

        var placement = HavokBehaviorOperations.FindPlacements(symbols, target)
            .FirstOrDefault(p => p.Kind is not HavokBehaviorOperations.PlacementKind.Unsupported);

        if (placement == null)
            return (null, LOC.Get("HAVOK_BehaviorActions_Cannot_Duplicate", HavokBehaviorSymbols.GetOutlineLabel(target)));

        var pasteAction = HavokBehaviorOperations.PasteGenerator(context, source, placement, out var newObj, out var pasteError);

        if (pasteAction != null)
            created.Add(newObj);

        return (pasteAction, pasteError == null ? null : $"{label}: {pasteError}");
    }
    #endregion

    #region Shortcuts
    /// <summary>
    /// Duplicate, Delete, Copy and Paste on the Contents window's selection (called while it has focus)
    /// </summary>
    public void Shortcuts()
    {
        if (Owner.Symbols == null || OpenDialog != Dialog.None || PendingDialog != Dialog.None)
            return;

        var duplicate = InputManager.IsPressed(KeybindID.Duplicate);
        var delete = InputManager.IsPressed(KeybindID.Delete);
        var copy = InputManager.IsPressed(KeybindID.Copy);
        var paste = InputManager.IsPressed(KeybindID.Paste);

        if (!duplicate && !delete && !copy && !paste)
            return;

        if (Owner.ContentsMode is HavokBehaviorContentsMode.Variables)
        {
            var index = Owner.VariableView.SelectedIndex;
            var hasVariable = HavokBehaviorSymbols.GetName(Owner.Symbols.VariableNames, index) != null;

            if (duplicate && hasVariable) RequestDuplicateVariable(index);
            if (delete && hasVariable) RequestDeleteVariable(index);
            if (copy && hasVariable) CopyVariable(index);
            if (paste) Paste(null, Source.List);

            return;
        }

        var (selection, source) = GetContentsSelection();
        var first = selection.FirstOrDefault();

        if (duplicate && IsTransferable(first)) RequestDuplicate(first, source);
        if (delete && selection.Any(IsTransferable)) RequestDeleteGenerators(selection.Where(IsTransferable).ToList(), source);
        if (copy) CopyGenerators(selection);
        if (paste) Paste(first, source);
    }

    /// <summary>
    /// What the Contents window has selected, in the mode it's showing
    /// </summary>
    private (List<object> Selection, Source Source) GetContentsSelection()
    {
        return Owner.ContentsMode switch
        {
            HavokBehaviorContentsMode.Outline => (Owner.OutlineView.Selected == null ? new() : new() { Owner.OutlineView.Selected }, Source.Outline),
            HavokBehaviorContentsMode.Animations => (Owner.AnimationTableView.Selected == null ? new() : new() { Owner.AnimationTableView.Selected }, Source.Outline),
            HavokBehaviorContentsMode.Selectors => (Owner.AnimationSelectorView.SelectedAnimSelectors.Cast<object>().ToList(), Source.List),
            HavokBehaviorContentsMode.StateMachines => (Owner.StateMachineView.SelectedStateMachines.Cast<object>().ToList(), Source.List),
            HavokBehaviorContentsMode.Clips => (Owner.AnimationClipView.SelectedAnimationClips.Cast<object>().ToList(), Source.List),
            _ => (new(), Source.List)
        };
    }
    #endregion

    /// <summary>
    /// "memberPath ← variable name" (or the character property it reads)
    /// </summary>
    private string DescribeBinding(object binding)
    {
        var symbols = Owner.Symbols;
        var isCharacterProperty = HavokBehaviorSymbols.IsCharacterPropertyBinding(binding);
        var index = HavokBehaviorSymbols.GetBindingIndex(binding);
        var name = HavokBehaviorSymbols.GetName(isCharacterProperty ? symbols.CharacterPropertyNames : symbols.VariableNames, index) ?? $"#{index}";

        return isCharacterProperty
            ? LOC.Get("HAVOK_BehaviorActions_Binding_Label_Character_Property", HavokBehaviorSymbols.GetMemberPath(binding), name)
            : LOC.Get("HAVOK_BehaviorActions_Binding_Label", HavokBehaviorSymbols.GetMemberPath(binding), name);
    }

    public void RequestBindVariable(object obj, Source source)
    {
        TargetObject = obj;
        RequestSource = source;
        BindMembers = HavokBehaviorOperations.GetBindableMembers(obj);
        BindVariableIndex = -1;

        // Start on the member games bind most (a clip's startTime), else the first
        BindMemberIndex = Math.Max(0, BindMembers.FindIndex(f => f.Name is "m_startTime" or "m_enableTae"));

        Request(Dialog.BindVariable);
    }

    public void RequestDuplicateGenerator(object obj, Source source)
    {
        TargetObject = obj;
        RequestSource = source;

        var placements = HavokBehaviorOperations.FindPlacements(Owner.Symbols, obj);

        DuplicatePlacements = placements.Where(p => p.Kind is not HavokBehaviorOperations.PlacementKind.Unsupported).ToList();
        DuplicateUnsupported = placements.Count == 0
            ? LOC.Get("HAVOK_BehaviorActions_Error_Not_Referenced")
            : placements.FirstOrDefault(p => p.Kind is HavokBehaviorOperations.PlacementKind.Unsupported)?.Description;
        DuplicatePlacementIndex = 0;
        DuplicateName = (HavokBehaviorSymbols.GetValue(obj, "m_name") as string ?? "") + "_Copy";
        DuplicateCopyShared = false;
        DuplicateAnimId = Convert.ToInt32(HavokBehaviorSymbols.GetValue(obj, "m_animId") ?? 0);

        var state = DuplicatePlacements.FirstOrDefault(p => p.Kind is HavokBehaviorOperations.PlacementKind.State)?.Holder;
        DuplicateStateName = (HavokBehaviorSymbols.GetValue(state, "m_name") as string ?? "") + "_Copy";

        Request(Dialog.DuplicateGenerator);
    }

    /// <summary>
    /// Duplicate for a selector or state machine (its own dialog) or a clip (a copy for a new category)
    /// </summary>
    public void RequestDuplicate(object obj, Source source)
    {
        if (obj?.GetType().Name == "hkbClipGenerator")
            RequestDuplicateClip(obj, source);
        else if (IsTransferable(obj))
            RequestDuplicateGenerator(obj, source);
    }

    public void RequestDuplicateClip(object clip, Source source)
    {
        TargetObject = clip;
        RequestSource = source;
        DuplicateClipSelectors = HavokBehaviorOperations.GetClipSelectors(Owner.Symbols, clip);
        DuplicateClipSelectorIndex = 0;
        DuplicateClipCategory = DuplicateClipSelectors.Count > 0 ? (int)HavokBehaviorOperations.GetFreeCategory(DuplicateClipSelectors[0]) : 0;

        Request(Dialog.DuplicateClip);
    }

    public void RequestDeleteGenerators(IReadOnlyList<object> objects, Source source)
    {
        if (objects.Count == 0)
            return;

        TargetObjects = objects.ToList();
        TargetObject = objects[0];
        RequestSource = source;
        DeleteLines = HavokBehaviorOperations.DescribeDeleteGenerators(Owner.Symbols, TargetObjects, out DeleteBlocked);

        Request(Dialog.DeleteGenerator);
    }

    public void RequestDuplicateVariable(int index)
    {
        TargetVariable = index;
        RequestSource = Source.List;
        DuplicateVariableName = (HavokBehaviorSymbols.GetName(Owner.Symbols.VariableNames, index) ?? "") + "_Copy";

        Request(Dialog.DuplicateVariable);
    }

    public void RequestDeleteVariable(int index)
    {
        TargetVariable = index;
        RequestSource = Source.List;
        VariableDeletion = HavokBehaviorOperations.DescribeDeleteVariable(Context, index);

        Request(Dialog.DeleteVariable);
    }

    /// <summary>
    /// Removes one binding straight away (undoable)
    /// </summary>
    private void RemoveBinding(object obj, object binding)
    {
        var action = HavokBehaviorOperations.RemoveBindings(Context, obj, new[] { binding }, out var error);

        if (action == null)
        {
            Smithbox.LogError(this, error);
            return;
        }

        View.ActionManager.ExecuteAction(action);
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

    public void RequestAddTransition(object stateMachine, object fromState) => RequestAddTransition(stateMachine, fromState, null);

    /// <summary>
    /// Opens Add Transition for a machine, from a state (or any state, if null), to a state (or the first, if null)
    /// </summary>
    public void RequestAddTransition(object stateMachine, object fromState, object toState)
    {
        if (stateMachine == null)
            return;

        TargetStateMachine = stateMachine;

        var states = (HavokBehaviorSymbols.GetValue(stateMachine, "m_states") as IList)?.Cast<object>().ToList() ?? new();

        TransitionFromIndex = fromState == null ? -1 : states.IndexOf(fromState);
        TransitionEventIndex = -1;
        TransitionNewEvent = false;
        TransitionNewEventName = "";
        TransitionToIndex = toState == null ? 0 : Math.Max(0, states.IndexOf(toState));

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
            Dialog.BindVariable => LOC.Get("HAVOK_BehaviorActions_Bind_Variable"),
            Dialog.DuplicateGenerator => LOC.Get(IsSelectorTarget ? "HAVOK_BehaviorActions_Duplicate_Selector" : "HAVOK_BehaviorActions_Duplicate_State_Machine"),
            Dialog.DeleteGenerator => TargetObjects.Count > 1
                ? LOC.Get("HAVOK_BehaviorActions_Delete_Selected", TargetObjects.Count)
                : TargetObject?.GetType().Name switch
                {
                    "CustomManualSelectorGenerator" => LOC.Get("HAVOK_BehaviorActions_Delete_Selector"),
                    "hkbClipGenerator" => LOC.Get("HAVOK_BehaviorActions_Delete_Clip"),
                    _ => LOC.Get("HAVOK_BehaviorActions_Delete_State_Machine")
                },
            Dialog.DuplicateClip => LOC.Get("HAVOK_BehaviorActions_Duplicate_Clip"),
            Dialog.DuplicateVariable => LOC.Get("HAVOK_BehaviorActions_Duplicate_Variable"),
            Dialog.DeleteVariable => LOC.Get("HAVOK_BehaviorActions_Delete_Variable"),
            _ => ""
        };

        ImGui.SetNextWindowSize(new System.Numerics.Vector2(520, 0) * DPI.UIScale(), ImGuiCond.Appearing);

        if (!ImGui.BeginPopupModal($"{title}###havokBehaviorActionDialog", ImGuiWindowFlags.AlwaysAutoResize))
        {
            OpenDialog = Dialog.None;
            return;
        }

        var apply = false;
        var canApply = true;

        switch (OpenDialog)
        {
            case Dialog.AddAnimation: apply = DisplayAddAnimation(); break;
            case Dialog.CloneState: apply = DisplayCloneState(); break;
            case Dialog.AddTransition: apply = DisplayAddTransition(); break;
            case Dialog.AddVariable: apply = DisplayAddVariable(); break;
            case Dialog.BindVariable: apply = DisplayBindVariable(out canApply); break;
            case Dialog.DuplicateGenerator: apply = DisplayDuplicateGenerator(out canApply); break;
            case Dialog.DeleteGenerator: DisplayDeleteGenerator(out canApply); break;
            case Dialog.DuplicateVariable: apply = DisplayDuplicateVariable(); break;
            case Dialog.DeleteVariable: DisplayDeleteVariable(); break;
            case Dialog.DuplicateClip: DisplayDuplicateClip(out canApply); break;
        }

        if (LastError != null)
            ImGui.TextColored(UI.Current.ImGui_Invalid_Text_Color, LastError);

        ImGui.Separator();

        ImGui.BeginDisabled(!canApply);

        if (ImGui.Button($"{LOC.Get("HAVOK_BehaviorActions_Apply")}##actionApply") || (apply && canApply))
        {
            if (Apply())
            {
                OpenDialog = Dialog.None;
                ImGui.CloseCurrentPopup();
            }
        }

        ImGui.EndDisabled();

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

    private bool IsSelectorTarget => TargetObject?.GetType().Name == "CustomManualSelectorGenerator";

    private bool DisplayBindVariable(out bool canApply)
    {
        canApply = false;

        var symbols = Owner.Symbols;

        ImGui.Text(LOC.Get("HAVOK_BehaviorActions_Object", HavokBehaviorSymbols.GetLabel(TargetObject)));

        if (BindMembers.Count == 0)
        {
            ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("HAVOK_BehaviorActions_No_Bindable_Members"));
            return false;
        }

        BindMemberIndex = Math.Clamp(BindMemberIndex, 0, BindMembers.Count - 1);

        var bindings = HavokBehaviorOperations.GetOwnBindings(TargetObject);

        string MemberLabel(System.Reflection.FieldInfo field)
        {
            var path = HavokBehaviorOperations.MemberPathOf(field);
            var bound = bindings.FirstOrDefault(b => string.Equals(HavokBehaviorSymbols.GetMemberPath(b), path, StringComparison.OrdinalIgnoreCase));

            return bound == null ? $"{path} ({field.FieldType.Name})" : $"{path} ({field.FieldType.Name})  {DescribeBinding(bound)}";
        }

        // Member
        if (ImGui.BeginCombo(LOC.Get("HAVOK_BehaviorActions_Member"), MemberLabel(BindMembers[BindMemberIndex]), ImGuiComboFlags.HeightLarge))
        {
            for (var i = 0; i < BindMembers.Count; i++)
            {
                if (ImGui.Selectable($"{MemberLabel(BindMembers[i])}##member{i}", i == BindMemberIndex))
                    BindMemberIndex = i;
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("HAVOK_BehaviorActions_Member_TT"));

        // Variable
        var variablePreview = HavokBehaviorSymbols.GetName(symbols.VariableNames, BindVariableIndex) ?? LOC.Get("HAVOK_BehaviorActions_Pick_Variable");

        if (ImGui.BeginCombo(LOC.Get("HAVOK_BehaviorIndex_Role_Variable"), variablePreview, ImGuiComboFlags.HeightLarge))
        {
            DisplayFilteredNames(symbols.VariableNames, ref BindVariableIndex);
            ImGui.EndCombo();
        }

        var member = BindMembers[BindMemberIndex];

        if (HavokBehaviorSymbols.GetName(symbols.VariableNames, BindVariableIndex) != null)
        {
            var variableType = HavokBehaviorOperations.GetVariableTypeName(symbols, BindVariableIndex);

            ImGui.TextDisabled(variableType);

            if (!HavokBehaviorOperations.IsSuitableVariableType(member.FieldType, variableType))
                ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("HAVOK_BehaviorActions_Variable_Type_Mismatch", variableType, member.FieldType.Name));

            canApply = true;
        }

        if (bindings.Any(b => string.Equals(HavokBehaviorSymbols.GetMemberPath(b), HavokBehaviorOperations.MemberPathOf(member), StringComparison.OrdinalIgnoreCase)))
            ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("HAVOK_BehaviorActions_Rebind"));

        return false;
    }

    private bool DisplayDuplicateGenerator(out bool canApply)
    {
        canApply = DuplicatePlacements.Count > 0;

        ImGui.Text(LOC.Get("HAVOK_BehaviorActions_Object", HavokBehaviorSymbols.GetOutlineLabel(TargetObject)));

        if (DuplicatePlacements.Count == 0)
        {
            ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("HAVOK_BehaviorActions_Cannot_Duplicate", DuplicateUnsupported ?? ""));
            return false;
        }

        DuplicatePlacementIndex = Math.Clamp(DuplicatePlacementIndex, 0, DuplicatePlacements.Count - 1);
        var placement = DuplicatePlacements[DuplicatePlacementIndex];

        // Where (a shared object can be duplicated at any place that holds it)
        if (DuplicatePlacements.Count > 1)
        {
            if (ImGui.BeginCombo(LOC.Get("HAVOK_BehaviorActions_Placement"), placement.Description))
            {
                for (var i = 0; i < DuplicatePlacements.Count; i++)
                {
                    if (ImGui.Selectable($"{DuplicatePlacements[i].Description}##placement{i}", i == DuplicatePlacementIndex))
                    {
                        DuplicatePlacementIndex = i;
                        DuplicateStateName = (HavokBehaviorSymbols.GetValue(DuplicatePlacements[i].Holder, "m_name") as string ?? "") + "_Copy";
                    }
                }

                ImGui.EndCombo();
            }
        }
        else
        {
            ImGui.TextDisabled(placement.Description);
        }

        if (DuplicateUnsupported != null)
            ImGui.TextDisabled(LOC.Get("HAVOK_BehaviorActions_Some_Placements_Unsupported", DuplicateUnsupported));

        ImGui.SetNextItemWidth(-1);
        var enter = ImGui.InputTextWithHint("##duplicateName", LOC.Get("HAVOK_BehaviorActions_Name_Hint"), ref DuplicateName, 255,
            ImGuiInputTextFlags.EnterReturnsTrue);

        if (IsSelectorTarget)
        {
            ImGui.SetNextItemWidth(160 * DPI.UIScale());
            ImGui.InputInt(LOC.Get("HAVOK_BehaviorActions_Animation_ID"), ref DuplicateAnimId);
            DuplicateAnimId = Math.Max(0, DuplicateAnimId);
            GUI.Tooltip(LOC.Get("HAVOK_BehaviorActions_Animation_ID_TT"));

            var oldAnimId = Convert.ToInt64(HavokBehaviorSymbols.GetValue(TargetObject, "m_animId") ?? 0);

            if (DuplicateAnimId != oldAnimId)
                ImGui.TextDisabled(LOC.Get("HAVOK_BehaviorActions_Retarget_Clips", DuplicateAnimId));
        }

        if (placement.Kind is HavokBehaviorOperations.PlacementKind.State)
        {
            ImGui.Text(LOC.Get("HAVOK_BehaviorActions_New_State_Name"));
            ImGui.SetNextItemWidth(-1);
            enter |= ImGui.InputText("##duplicateStateName", ref DuplicateStateName, 255, ImGuiInputTextFlags.EnterReturnsTrue);

            ImGui.TextDisabled(LOC.Get("HAVOK_BehaviorActions_New_State_ID", HavokBehaviorOperations.GetFreeStateID(placement.StateMachine)));
        }
        else
        {
            ImGui.TextDisabled(LOC.Get("HAVOK_BehaviorActions_Appended_To_List"));
        }

        ImGui.Checkbox(LOC.Get("HAVOK_BehaviorActions_Copy_Shared"), ref DuplicateCopyShared);
        GUI.Tooltip(LOC.Get("HAVOK_BehaviorActions_Copy_Shared_TT"));

        return enter;
    }

    private void DisplayDuplicateClip(out bool canApply)
    {
        canApply = DuplicateClipSelectors.Count > 0;

        ImGui.Text(LOC.Get("HAVOK_BehaviorActions_Object", HavokBehaviorSymbols.GetOutlineLabel(TargetObject)));

        if (DuplicateClipSelectors.Count == 0)
        {
            ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("HAVOK_BehaviorActions_Cannot_Duplicate", LOC.Get("HAVOK_BehaviorActions_Clip_No_Selector")));
            return;
        }

        DuplicateClipSelectorIndex = Math.Clamp(DuplicateClipSelectorIndex, 0, DuplicateClipSelectors.Count - 1);
        var selector = DuplicateClipSelectors[DuplicateClipSelectorIndex];

        // A shared clip can be duplicated into any selector that holds it
        if (DuplicateClipSelectors.Count > 1)
        {
            if (ImGui.BeginCombo(LOC.Get("HAVOK_BehaviorActions_Selector_Combo"), HavokBehaviorSymbols.GetOutlineLabel(selector)))
            {
                for (var i = 0; i < DuplicateClipSelectors.Count; i++)
                {
                    if (ImGui.Selectable($"{HavokBehaviorSymbols.GetOutlineLabel(DuplicateClipSelectors[i])}##clipSelector{i}", i == DuplicateClipSelectorIndex))
                    {
                        DuplicateClipSelectorIndex = i;
                        DuplicateClipCategory = (int)HavokBehaviorOperations.GetFreeCategory(DuplicateClipSelectors[i]);
                    }
                }

                ImGui.EndCombo();
            }
        }
        else
        {
            ImGui.Text(LOC.Get("HAVOK_BehaviorActions_Selector", HavokBehaviorSymbols.GetOutlineLabel(selector)));
        }

        var size = Owner.Animations?.CategorySize ?? 1000000;

        ImGui.SetNextItemWidth(120 * DPI.UIScale());
        ImGui.InputInt(LOC.Get("HAVOK_BehaviorActions_Category"), ref DuplicateClipCategory);
        DuplicateClipCategory = Math.Clamp(DuplicateClipCategory, 0, size == 10000 ? 99 : 999);
        GUI.Tooltip(LOC.Get("HAVOK_BehaviorActions_Duplicate_Clip_Category_TT"));

        DisplayNewClipStatus(selector, DuplicateClipCategory);
    }

    /// <summary>
    /// The clip name a category gives in a selector, and whether the character has its time act and animation file
    /// </summary>
    private void DisplayNewClipStatus(object selector, int category)
    {
        var animations = Owner.Animations;
        var animationID = Convert.ToInt64(HavokBehaviorSymbols.GetValue(selector, "m_animId") ?? 0);
        var size = animations?.CategorySize ?? 1000000;

        var name = animations?.FormatAnimationName(category, animationID)
            ?? $"a{category:D3}_{animationID:D6}";

        ImGui.Text(LOC.Get("HAVOK_BehaviorActions_New_Clip", name));

        if (animations is { IsReady: true })
        {
            var fullId = (long)category * size + animationID;
            var hasTimeAct = animations.TryGetTimeAct(fullId, out var timeAct);
            var hasFile = animations.HasAnimationFile(name);

            ImGui.TextColored(hasTimeAct && hasFile ? UI.Current.ImGui_Benefit_Text_Color : UI.Current.ImGui_Warning_Text_Color,
                $"{(hasTimeAct ? LOC.Get("HAVOK_AnimationRow_Time_Act", timeAct) : LOC.Get("HAVOK_AnimationRow_No_Time_Act"))}  ·  " +
                $"{(hasFile ? LOC.Get("HAVOK_AnimationRow_File") : LOC.Get("HAVOK_AnimationRow_No_File"))}");
        }
    }

    private void DisplayDeleteGenerator(out bool canApply)
    {
        canApply = DeleteLines != null;

        if (TargetObjects.Count > 1)
        {
            ImGui.Text(LOC.Get("HAVOK_BehaviorActions_Objects", TargetObjects.Count));

            foreach (var obj in TargetObjects.Take(8))
                ImGui.TextDisabled($"    {HavokBehaviorSymbols.GetOutlineLabel(obj)}");

            if (TargetObjects.Count > 8)
                ImGui.TextDisabled($"    {LOC.Get("HAVOK_BehaviorActions_And_More", TargetObjects.Count - 8)}");
        }
        else
        {
            ImGui.Text(LOC.Get("HAVOK_BehaviorActions_Object", HavokBehaviorSymbols.GetOutlineLabel(TargetObject)));
        }

        if (DeleteLines == null)
        {
            ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("HAVOK_BehaviorActions_Cannot_Delete", DeleteBlocked ?? ""));
            return;
        }

        foreach (var line in DeleteLines)
            ImGui.BulletText(line);

        ImGui.TextDisabled(LOC.Get("HAVOK_BehaviorActions_Delete_Undo_Note"));
    }

    private bool DisplayDuplicateVariable()
    {
        var symbols = Owner.Symbols;

        ImGui.Text(LOC.Get("HAVOK_BehaviorActions_Variable",
            HavokBehaviorSymbols.GetName(symbols.VariableNames, TargetVariable) ?? "", TargetVariable,
            HavokBehaviorOperations.GetVariableTypeName(symbols, TargetVariable)));

        ImGui.SetNextItemWidth(-1);
        var enter = ImGui.InputTextWithHint("##duplicateVariableName", LOC.Get("HAVOK_BehaviorActions_Variable_Name_Hint"),
            ref DuplicateVariableName, 255, ImGuiInputTextFlags.EnterReturnsTrue);

        ImGui.TextDisabled(LOC.Get("HAVOK_BehaviorActions_Duplicate_Variable_Note", symbols.VariableNames?.Count ?? 0));

        return enter;
    }

    private void DisplayDeleteVariable()
    {
        var symbols = Owner.Symbols;
        var deletion = VariableDeletion;

        ImGui.Text(LOC.Get("HAVOK_BehaviorActions_Variable",
            HavokBehaviorSymbols.GetName(symbols.VariableNames, TargetVariable) ?? "", TargetVariable,
            HavokBehaviorOperations.GetVariableTypeName(symbols, TargetVariable)));

        ImGui.BulletText(LOC.Get("HAVOK_BehaviorActions_Delete_Variable_Bindings", deletion.Bindings.Count));

        // The first few, so it's clear what stops being driven
        foreach (var (bindable, binding) in deletion.Bindings.Take(8))
            ImGui.TextDisabled($"    {HavokBehaviorSymbols.GetLabel(bindable)}  .{HavokBehaviorSymbols.GetMemberPath(binding)}");

        if (deletion.Bindings.Count > 8)
            ImGui.TextDisabled($"    {LOC.Get("HAVOK_BehaviorActions_And_More", deletion.Bindings.Count - 8)}");

        if (deletion.Fields.Count > 0)
            ImGui.BulletText(LOC.Get("HAVOK_BehaviorActions_Delete_Variable_Fields", deletion.Fields.Count));

        var after = (symbols.VariableNames?.Count ?? 0) - TargetVariable - 1;

        if (after > 0)
            ImGui.BulletText(LOC.Get("HAVOK_BehaviorActions_Delete_Variable_Shift", after));

        ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("HAVOK_BehaviorActions_Delete_Variable_Warning"));
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

            case Dialog.BindVariable:
                action = HavokBehaviorOperations.BindVariable(context, TargetObject, BindMembers[BindMemberIndex], BindVariableIndex, out error);
                break;

            case Dialog.DuplicateGenerator:
            {
                var placement = DuplicatePlacements.ElementAtOrDefault(DuplicatePlacementIndex);

                action = HavokBehaviorOperations.DuplicateGenerator(context, TargetObject, placement, DuplicateName.Trim(),
                    DuplicateStateName.Trim(), DuplicateCopyShared, IsSelectorTarget ? DuplicateAnimId : null, out var newObj, out error);

                if (action != null)
                    ShowResult(newObj);
                break;
            }

            case Dialog.DeleteGenerator:
                action = HavokBehaviorOperations.DeleteGenerators(context, TargetObjects, out error);
                break;

            case Dialog.DuplicateClip:
            {
                var selector = DuplicateClipSelectors.ElementAtOrDefault(DuplicateClipSelectorIndex);

                if (selector == null)
                    break;

                action = HavokBehaviorOperations.DuplicateClip(context, TargetObject, selector, DuplicateClipCategory, out var newClip, out error);

                if (action != null)
                    ShowResult(newClip);
                break;
            }

            case Dialog.DuplicateVariable:
                action = HavokBehaviorOperations.DuplicateVariable(context, TargetVariable, DuplicateVariableName.Trim(), out var newIndex, out error);

                if (action != null)
                    Owner.VariableView.RequestSelect(newIndex);
                break;

            case Dialog.DeleteVariable:
                action = HavokBehaviorOperations.DeleteVariable(context, TargetVariable, out error);

                if (action != null)
                    Owner.VariableView.ResetSelection();
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
    /// Shows a new object where the edit was asked for: the Outline goes to it, a list selects it
    /// </summary>
    private void ShowResult(object obj)
    {
        if (obj == null)
            return;

        if (RequestSource is Source.Outline)
        {
            Owner.OutlineView.NavigateAfterRebuild(obj);
            return;
        }

        switch (obj)
        {
            case Havok.Shared.ICustomManualSelectorGenerator selector:
                Owner.AnimationSelectorView.SelectedAnimSelectors.Clear();
                Owner.AnimationSelectorView.SelectedAnimSelectors.Add(selector);
                break;

            case Havok.Shared.IStateMachine stateMachine:
                Owner.StateMachineView.SelectedStateMachines.Clear();
                Owner.StateMachineView.SelectedStateMachines.Add(stateMachine);
                break;

            case Havok.Shared.IClipGenerator clip:
                Owner.AnimationClipView.SelectedAnimationClips.Clear();
                Owner.AnimationClipView.SelectedAnimationClips.Add(clip);
                break;
        }
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
