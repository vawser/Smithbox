namespace StudioCore.Editors.BehaviorEditor;

/// <summary>What the Properties window shows</summary>
public enum BhvTarget { None, File, State, Transition, Condition, Animation, StructC, StructD }

/// <summary>
/// The selected behbnd, .bhv file and record within it
/// </summary>
public class BhvSelection
{
    public BhvEditorView View;

    public BhvBinderWrapper SelectedBinder { get; private set; }
    public BhvFileWrapper SelectedFile { get; private set; }

    public int StateIndex { get; private set; } = -1;
    public int TransitionIndex { get; private set; } = -1;
    public int ConditionIndex { get; private set; } = -1;
    public int AnimationIndex { get; private set; } = -1;
    public int StructIndex { get; private set; } = -1;

    public BhvTarget Target { get; private set; } = BhvTarget.None;

    /// <summary>Set when a jump selects a state, so the state list scrolls to it once</summary>
    public bool ScrollToState;
    public bool ScrollToAnimation;

    public BhvSelection(BhvEditorView view)
    {
        View = view;
    }

    public void SelectFile(BhvFileWrapper file)
    {
        if (file == SelectedFile)
            return;

        SelectedFile = file;
        SelectedBinder = file?.Binder;

        StateIndex = -1;
        TransitionIndex = -1;
        ConditionIndex = -1;
        AnimationIndex = -1;
        StructIndex = -1;

        Target = file != null ? BhvTarget.File : BhvTarget.None;
    }

    public void ShowFile()
    {
        if (SelectedFile != null)
            Target = BhvTarget.File;
    }

    public void SelectState(int index, bool scroll = false)
    {
        if (SelectedFile == null || index < 0 || index >= SelectedFile.BHV.States.Count)
            return;

        if (index != StateIndex)
        {
            TransitionIndex = -1;
            ConditionIndex = -1;
        }

        StateIndex = index;
        Target = BhvTarget.State;
        ScrollToState = scroll;
    }

    public void SelectTransition(int index)
    {
        var state = SelectedState;
        if (state == null || index < 0 || index >= state.Transitions.Count)
            return;

        TransitionIndex = index;
        ConditionIndex = -1;
        Target = BhvTarget.Transition;
    }

    public void SelectCondition(int transition, int condition)
    {
        var state = SelectedState;
        if (state == null || transition < 0 || transition >= state.Transitions.Count)
            return;

        if (condition < 0 || condition >= state.Transitions[transition].Conditions.Count)
            return;

        TransitionIndex = transition;
        ConditionIndex = condition;
        Target = BhvTarget.Condition;
    }

    public void SelectAnimation(int index, bool scroll = false)
    {
        if (SelectedFile == null || index < 0 || index >= SelectedFile.BHV.Animations.Count)
            return;

        AnimationIndex = index;
        Target = BhvTarget.Animation;
        ScrollToAnimation = scroll;
    }

    public void SelectStructC(int index)
    {
        if (SelectedFile == null || index < 0 || index >= SelectedFile.BHV.StructCs.Count)
            return;

        StructIndex = index;
        Target = BhvTarget.StructC;
    }

    public void SelectStructD(int index)
    {
        if (SelectedFile == null || index < 0 || index >= SelectedFile.BHV.StructDs.Count)
            return;

        StructIndex = index;
        Target = BhvTarget.StructD;
    }

    public SoulsFormats.BHV.State SelectedState =>
        SelectedFile != null && StateIndex >= 0 && StateIndex < SelectedFile.BHV.States.Count
            ? SelectedFile.BHV.States[StateIndex] : null;

    public SoulsFormats.BHV.Transition SelectedTransition
    {
        get
        {
            var state = SelectedState;
            return state != null && TransitionIndex >= 0 && TransitionIndex < state.Transitions.Count
                ? state.Transitions[TransitionIndex] : null;
        }
    }

    public SoulsFormats.BHV.Condition SelectedCondition
    {
        get
        {
            var transition = SelectedTransition;
            return transition != null && ConditionIndex >= 0 && ConditionIndex < transition.Conditions.Count
                ? transition.Conditions[ConditionIndex] : null;
        }
    }
}
