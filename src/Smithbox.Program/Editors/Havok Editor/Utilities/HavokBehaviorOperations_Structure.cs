using StudioCore.Editors.Common;
using System.Collections;
using System.Numerics;
using System.Reflection;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.HavokEditor;

/// <summary>
/// Duplicate and delete (selectors, state machines, variables) and variable bindings.
/// The rules come from the game files (Tools/BehaviorSurvey editsurvey; see Documentation/Claude/Havok Behavior Structure Edits.md).
/// </summary>
public static partial class HavokBehaviorOperations
{
    #region Helpers
    /// <summary>
    /// Removes the item from the list, finding it when the step runs (so steps on the same list can follow each other)
    /// </summary>
    private static EditorAction RemoveFromList(HavokBehaviorContext context, IList list, object item)
    {
        var index = -1;

        return Step(context,
            () =>
            {
                index = IndexOfReference(list, item);

                if (index >= 0)
                    list.RemoveAt(index);
            },
            () =>
            {
                if (index >= 0)
                    list.Insert(index, item);
            });
    }

    private static int IndexOfReference(IList list, object item)
    {
        for (var i = 0; i < list.Count; i++)
        {
            if (ReferenceEquals(list[i], item))
                return i;
        }

        return -1;
    }

    private static EditorAction SetValue(HavokBehaviorContext context, object obj, FieldInfo field, object value)
    {
        var old = field.GetValue(obj);

        return Step(context, () => field.SetValue(obj, value), () => field.SetValue(obj, old));
    }

    private static int ToInt(object value, int fallback = -1)
    {
        if (value == null)
            return fallback;

        try
        {
            return Convert.ToInt32(value);
        }
        catch
        {
            return fallback;
        }
    }

    private static bool IsStateInfo(object obj) => obj?.GetType().Name is "StateInfo" or "hkbStateMachineStateInfo";
    #endregion

    #region Placement
    public enum PlacementKind
    {
        /// <summary>An entry of a selector's generator list</summary>
        List,
        /// <summary>The generator of a state</summary>
        State,
        /// <summary>Somewhere a duplicate can't go and a delete can't remove it from</summary>
        Unsupported
    }

    /// <summary>
    /// Where a generator is held, for Duplicate and Delete.
    /// Chain runs from the list entry (or the state's generator) down to the object, through the
    /// single-field wrappers in between (a script generator's child).
    /// </summary>
    public record HavokPlacement(PlacementKind Kind, object Holder, object StateMachine, List<object> Chain, string Description);

    /// <summary>
    /// The generator lists an entry can be added to or removed from: the selectors pick entries by index,
    /// so a copy goes at the end and the indices of the others stay the same
    /// </summary>
    private static readonly HashSet<string> GeneratorListOwners = new() { "hkbManualSelectorGenerator", "CustomManualSelectorGenerator" };

    /// <summary>
    /// Wrappers looked through on the way up: they hold one generator and are rarely shared
    /// </summary>
    private static readonly Dictionary<string, string> WrapperFields = new()
    {
        { "hkbScriptGenerator", "m_child" },
        { "hkbModifierGenerator", "m_generator" }
    };

    /// <summary>
    /// Every place the object is held (one per parent)
    /// </summary>
    public static List<HavokPlacement> FindPlacements(HavokBehaviorSymbols symbols, object obj)
    {
        return symbols.GetParents(obj).Select(p => ResolvePlacement(symbols, p, obj)).ToList();
    }

    private static HavokPlacement ResolvePlacement(HavokBehaviorSymbols symbols, object parent, object child)
    {
        var chain = new List<object> { child };

        for (var depth = 0; depth < 8; depth++)
        {
            var parentType = parent.GetType().Name;
            var holding = FindHoldingField(parent, child, out var isList);

            if (holding == null)
                return Unsupported(LOC.Get("HAVOK_BehaviorActions_Placement_Not_Found", HavokBehaviorSymbols.GetLabel(parent)));

            if (isList)
            {
                if (GeneratorListOwners.Contains(parentType) && holding.Name == "m_generators")
                {
                    return new(PlacementKind.List, parent, null, chain,
                        LOC.Get("HAVOK_BehaviorActions_Placement_List", HavokBehaviorSymbols.GetOutlineLabel(parent)));
                }

                return Unsupported(LOC.Get("HAVOK_BehaviorActions_Placement_Unsupported", parentType, holding.Name));
            }

            if (IsStateInfo(parent))
            {
                var stateMachine = symbols.GetParents(parent).FirstOrDefault(p => p.GetType().Name == "hkbStateMachine");

                if (stateMachine == null)
                    return Unsupported(LOC.Get("HAVOK_BehaviorActions_Placement_Not_Found", HavokBehaviorSymbols.GetLabel(parent)));

                return new(PlacementKind.State, parent, stateMachine, chain,
                    LOC.Get("HAVOK_BehaviorActions_Placement_State", HavokBehaviorSymbols.GetOutlineLabel(parent),
                        HavokBehaviorSymbols.GetOutlineLabel(stateMachine)));
            }

            if (WrapperFields.TryGetValue(parentType, out var wrapperField) && holding.Name == wrapperField)
            {
                var wrapperParents = symbols.GetParents(parent);

                if (wrapperParents.Count != 1)
                    return Unsupported(LOC.Get("HAVOK_BehaviorActions_Placement_Shared_Wrapper", HavokBehaviorSymbols.GetLabel(parent), wrapperParents.Count));

                chain.Insert(0, parent);
                child = parent;
                parent = wrapperParents[0];
                continue;
            }

            return Unsupported(LOC.Get("HAVOK_BehaviorActions_Placement_Unsupported", parentType, holding.Name));
        }

        return Unsupported(LOC.Get("HAVOK_BehaviorActions_Placement_Too_Deep"));

        HavokPlacement Unsupported(string reason) => new(PlacementKind.Unsupported, parent, null, chain, reason);
    }

    private static FieldInfo FindHoldingField(object parent, object child, out bool isList)
    {
        isList = false;

        foreach (var field in parent.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public))
        {
            var value = field.GetValue(parent);

            if (ReferenceEquals(value, child))
                return field;

            if (value is IList list && value is not string && IndexOfReference(list, child) >= 0)
            {
                isList = true;
                return field;
            }
        }

        return null;
    }
    #endregion

    #region Duplicate generator
    /// <summary>
    /// Duplicates a selector or state machine where it's held: a copy at the end of the selector list, or a new
    /// state (next free state ID, the original's outgoing transitions) holding a copy.
    /// Children shared with the rest of the file stay shared unless copyShared. The chain down to the object
    /// is always copied. A CMSG given a new animation ID has its clips copied and retargeted to it.
    /// </summary>
    public static EditorAction DuplicateGenerator(HavokBehaviorContext context, object obj, HavokPlacement placement,
        string newName, string newStateName, bool copyShared, long? newAnimId, out object newObj, out string error)
    {
        newObj = null;
        error = null;

        var symbols = context.Symbols;

        if (placement == null || placement.Kind is PlacementKind.Unsupported)
        {
            error = placement?.Description ?? LOC.Get("HAVOK_BehaviorActions_Error_Not_Referenced");
            return null;
        }

        if (string.IsNullOrWhiteSpace(newName))
        {
            error = LOC.Get("HAVOK_BehaviorActions_Error_No_Name");
            return null;
        }

        var isSelector = obj.GetType().Name == "CustomManualSelectorGenerator";
        var oldAnimId = isSelector ? Convert.ToInt64(HavokBehaviorSymbols.GetValue(obj, "m_animId") ?? 0) : 0;
        var retarget = isSelector && newAnimId.HasValue && newAnimId.Value != oldAnimId;

        var alwaysCopy = new HashSet<object>(placement.Chain, ReferenceEqualityComparer.Instance);
        var oldClips = new List<object>();

        if (retarget && GetList(obj, "m_generators") is IList generators)
        {
            foreach (var generator in generators)
            {
                if (generator?.GetType().Name == "hkbClipGenerator")
                {
                    oldClips.Add(generator);
                    alwaysCopy.Add(generator);
                }
            }
        }

        var steps = new List<EditorAction>();
        object root;

        if (placement.Kind is PlacementKind.State)
        {
            var states = GetList(placement.StateMachine, "m_states");

            if (states == null)
            {
                error = LOC.Get("HAVOK_BehaviorActions_Error_No_State_Machine");
                return null;
            }

            if (string.IsNullOrWhiteSpace(newStateName))
            {
                error = LOC.Get("HAVOK_BehaviorActions_Error_No_Name");
                return null;
            }

            if (HavokBehaviorSymbols.GetStates(placement.StateMachine).Any(s => s.Name == newStateName))
            {
                error = LOC.Get("HAVOK_BehaviorActions_Error_State_Exists", newStateName);
                return null;
            }

            root = placement.Holder;
        }
        else
        {
            root = placement.Chain[0];
        }

        var copy = HavokBehaviorCloner.Clone(root, symbols, keepShared: !copyShared, alwaysCopy, out var copies);

        newObj = ReferenceEquals(root, obj) ? copy : copies.GetValueOrDefault(obj);

        if (newObj == null)
        {
            error = LOC.Get("HAVOK_BehaviorActions_Error_Cannot_Create", obj.GetType().Name);
            return null;
        }

        HavokBehaviorSymbols.GetField(newObj.GetType(), "m_name")?.SetValue(newObj, newName);

        if (retarget)
        {
            var animIdField = HavokBehaviorSymbols.GetField(newObj.GetType(), "m_animId");
            animIdField.SetValue(newObj, Convert.ChangeType(newAnimId.Value, animIdField.FieldType));

            var addedNames = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var oldClip in oldClips)
            {
                if (copies.GetValueOrDefault(oldClip) is not { } clipCopy)
                    continue;

                if (!RetargetClip(context, clipCopy, newAnimId.Value, addedNames, steps, out error))
                    return null;
            }
        }

        if (placement.Kind is PlacementKind.State)
        {
            HavokBehaviorSymbols.GetField(copy.GetType(), "m_name").SetValue(copy, newStateName);
            HavokBehaviorSymbols.GetField(copy.GetType(), "m_stateId").SetValue(copy, GetFreeStateID(placement.StateMachine));

            steps.Add(AddToList(context, GetList(placement.StateMachine, "m_states"), copy));
        }
        else
        {
            steps.Add(AddToList(context, GetList(placement.Holder, "m_generators"), copy));
        }

        return Wrap(context, steps);
    }

    private static readonly Regex ClipAnimationRegex = new(@"^a(\d+)_(\d+)$", RegexOptions.IgnoreCase);

    /// <summary>
    /// Points a copied clip at the animation with the new ID (same category): its animation name, its name,
    /// and its internal ID (that of a clip of the same animation, or a new animation name entry)
    /// </summary>
    private static bool RetargetClip(HavokBehaviorContext context, object clip, long newAnimId,
        Dictionary<string, int> addedNames, List<EditorAction> steps, out string error)
    {
        error = null;

        var symbols = context.Symbols;
        var oldAnimation = HavokBehaviorSymbols.GetValue(clip, "m_animationName") as string ?? "";
        var match = ClipAnimationRegex.Match(oldAnimation);

        if (!match.Success)
        {
            error = LOC.Get("HAVOK_BehaviorActions_Error_Clip_Animation_Name", oldAnimation);
            return false;
        }

        var categoryText = match.Groups[1].Value;
        var idDigits = match.Groups[2].Value.Length;
        var newAnimation = $"a{categoryText}_{newAnimId.ToString().PadLeft(idDigits, '0')}";
        var category = long.Parse(categoryText);
        var categorySize = context.Project.Descriptor.ProjectType is ProjectType.DES or ProjectType.DS1 or ProjectType.DS1R ? 10000 : 1000000;

        var oldName = HavokBehaviorSymbols.GetValue(clip, "m_name") as string ?? "";
        var newName = oldName.Contains(oldAnimation, StringComparison.OrdinalIgnoreCase)
            ? Regex.Replace(oldName, Regex.Escape(oldAnimation), newAnimation, RegexOptions.IgnoreCase)
            : newAnimation;

        HavokBehaviorSymbols.GetField(clip.GetType(), "m_animationName").SetValue(clip, newAnimation);
        HavokBehaviorSymbols.GetField(clip.GetType(), "m_name").SetValue(clip, newName);

        var internalIdField = HavokBehaviorSymbols.GetField(clip.GetType(), "m_animationInternalId");
        var sameAnimation = symbols.Clips.FirstOrDefault(c =>
            string.Equals(HavokBehaviorSymbols.GetValue(c, "m_animationName") as string, newAnimation, StringComparison.OrdinalIgnoreCase));

        // HKX2 (DS3): no internal ID; the character file lists the animations the behavior uses
        if (internalIdField == null)
        {
            if (sameAnimation == null)
            {
                error = LOC.Get("HAVOK_BehaviorActions_Error_Character_Bundle", newAnimation);
                return false;
            }

            return true;
        }

        if (sameAnimation != null)
        {
            internalIdField.SetValue(clip, HavokBehaviorSymbols.GetValue(sameAnimation, "m_animationInternalId"));
            return true;
        }

        var animationNames = symbols.AnimationNames;

        if (animationNames == null || animationNames.Count == 0)
        {
            error = LOC.Get("HAVOK_BehaviorActions_Error_No_Animation_Names");
            return false;
        }

        // Another copied clip may already have added this animation
        if (!addedNames.TryGetValue(newAnimation, out var index))
        {
            index = animationNames.Count + addedNames.Count;
            addedNames[newAnimation] = index;
            steps.Add(AddToList(context, animationNames, MakeAnimationPath(animationNames, newAnimation, category, categorySize)));
        }

        internalIdField.SetValue(clip, Convert.ChangeType(index, internalIdField.FieldType));

        return true;
    }
    #endregion

    #region Delete generator
    /// <summary>
    /// What deleting the objects does, one line per place each is held, or null with the reason one can't be deleted
    /// </summary>
    public static List<string> DescribeDeleteGenerators(HavokBehaviorSymbols symbols, IReadOnlyList<object> objects, out string error)
    {
        error = null;

        var lines = new List<string>();
        var states = new HashSet<object>(ReferenceEqualityComparer.Instance);
        var removedFromList = new Dictionary<object, int>(ReferenceEqualityComparer.Instance);

        foreach (var obj in objects)
        {
            var placements = FindPlacements(symbols, obj);

            if (placements.Count == 0)
            {
                error = $"{HavokBehaviorSymbols.GetOutlineLabel(obj)}: {LOC.Get("HAVOK_BehaviorActions_Error_Not_Referenced")}";
                return null;
            }

            var unsupported = placements.FirstOrDefault(p => p.Kind is PlacementKind.Unsupported);

            if (unsupported != null)
            {
                error = $"{HavokBehaviorSymbols.GetOutlineLabel(obj)}: {unsupported.Description}";
                return null;
            }

            foreach (var placement in placements)
            {
                if (placement.Kind is PlacementKind.List)
                {
                    var list = GetList(placement.Holder, "m_generators");
                    var index = IndexOfReference(list, placement.Chain[0]);
                    var later = list.Count - index - 1;

                    removedFromList[placement.Holder] = removedFromList.GetValueOrDefault(placement.Holder) + 1;

                    lines.Add(later > 0
                        ? LOC.Get("HAVOK_BehaviorActions_Delete_From_List_Shift", HavokBehaviorSymbols.GetOutlineLabel(placement.Holder), index, later)
                        : LOC.Get("HAVOK_BehaviorActions_Delete_From_List", HavokBehaviorSymbols.GetOutlineLabel(placement.Holder), index));
                }
                else if (states.Add(placement.Holder))
                {
                    if (!CanDeleteState(placement.StateMachine, placement.Holder, out error))
                        return null;

                    var stateId = ToInt(HavokBehaviorSymbols.GetValue(placement.Holder, "m_stateId"));
                    var transitions = FindTransitionsTo(placement.StateMachine, placement.Holder, stateId).Count;

                    lines.Add(LOC.Get("HAVOK_BehaviorActions_Delete_State", HavokBehaviorSymbols.GetOutlineLabel(placement.Holder),
                        HavokBehaviorSymbols.GetOutlineLabel(placement.StateMachine), transitions));
                }
            }
        }

        // A selector left with nothing to pick from
        foreach (var (holder, count) in removedFromList)
        {
            if (GetList(holder, "m_generators") is IList list && list.Count <= count)
                lines.Add(LOC.Get("HAVOK_BehaviorActions_Delete_Leaves_Empty", HavokBehaviorSymbols.GetOutlineLabel(holder)));
        }

        // Several states of one machine: it must keep one, and its start state
        foreach (var group in states.GroupBy(s => symbols.GetParents(s).FirstOrDefault(p => p.GetType().Name == "hkbStateMachine")))
        {
            if (group.Key != null && GetList(group.Key, "m_states") is IList machineStates && machineStates.Count <= group.Count())
            {
                error = LOC.Get("HAVOK_BehaviorActions_Error_Only_State", HavokBehaviorSymbols.GetOutlineLabel(group.Key));
                return null;
            }
        }

        return lines;
    }

    /// <summary>
    /// Deletes selectors, state machines or clips from every place each is held: its selector list entry, or the
    /// state holding it (and the transitions into that state)
    /// </summary>
    public static EditorAction DeleteGenerators(HavokBehaviorContext context, IReadOnlyList<object> objects, out string error)
    {
        if (DescribeDeleteGenerators(context.Symbols, objects, out error) == null)
            return null;

        var steps = new List<EditorAction>();
        var deletedStates = new HashSet<object>(ReferenceEqualityComparer.Instance);

        foreach (var obj in objects)
        {
            foreach (var placement in FindPlacements(context.Symbols, obj))
            {
                if (placement.Kind is PlacementKind.List)
                {
                    steps.Add(RemoveFromList(context, GetList(placement.Holder, "m_generators"), placement.Chain[0]));
                }
                else if (deletedStates.Add(placement.Holder))
                {
                    steps.AddRange(DeleteStateSteps(context, placement.StateMachine, placement.Holder));
                }
            }
        }

        return Wrap(context, steps);
    }

    private static bool CanDeleteState(object stateMachine, object state, out string error)
    {
        error = null;

        var states = GetList(stateMachine, "m_states");
        var stateId = ToInt(HavokBehaviorSymbols.GetValue(state, "m_stateId"));

        if (states == null || states.Count <= 1)
        {
            error = LOC.Get("HAVOK_BehaviorActions_Error_Only_State", HavokBehaviorSymbols.GetOutlineLabel(state));
            return false;
        }

        if (ToInt(HavokBehaviorSymbols.GetValue(stateMachine, "m_startStateId")) == stateId)
        {
            error = LOC.Get("HAVOK_BehaviorActions_Error_Start_State", HavokBehaviorSymbols.GetOutlineLabel(state),
                HavokBehaviorSymbols.GetOutlineLabel(stateMachine));
            return false;
        }

        return true;
    }

    /// <summary>
    /// The transitions of the machine (from its other states, and its wildcards) that go to the state ID,
    /// with the list each is in
    /// </summary>
    private static List<(IList List, object Transition)> FindTransitionsTo(object stateMachine, object state, int stateId)
    {
        var results = new List<(IList, object)>();

        void Collect(object array)
        {
            if (GetList(array, "m_transitions") is not IList transitions)
                return;

            foreach (var transition in transitions)
            {
                if (transition != null && ToInt(HavokBehaviorSymbols.GetValue(transition, "m_toStateId")) == stateId)
                    results.Add((transitions, transition));
            }
        }

        if (GetList(stateMachine, "m_states") is IList states)
        {
            foreach (var other in states)
            {
                if (other != null && !ReferenceEquals(other, state))
                    Collect(HavokBehaviorSymbols.GetValue(other, "m_transitions"));
            }
        }

        Collect(HavokBehaviorSymbols.GetValue(stateMachine, "m_wildcardTransitions"));

        return results;
    }

    private static List<EditorAction> DeleteStateSteps(HavokBehaviorContext context, object stateMachine, object state)
    {
        var stateId = ToInt(HavokBehaviorSymbols.GetValue(state, "m_stateId"));
        var steps = new List<EditorAction>();

        foreach (var (list, transition) in FindTransitionsTo(stateMachine, state, stateId))
            steps.Add(RemoveFromList(context, list, transition));

        steps.Add(RemoveFromList(context, GetList(stateMachine, "m_states"), state));

        return steps;
    }
    #endregion

    #region Bindings
    private static readonly HashSet<string> UnbindableMembers = new() { "m_userData", "m_variableBindingSet", "m_propertyBag", "m_name" };

    /// <summary>
    /// Whether the object can hold variable bindings (an hkbBindable)
    /// </summary>
    public static bool IsBindable(object obj) => obj != null && HavokBehaviorSymbols.GetField(obj.GetType(), "m_variableBindingSet") != null;

    /// <summary>
    /// The object's own fields a binding can drive: numbers, bools, vectors and quaternions
    /// </summary>
    public static List<FieldInfo> GetBindableMembers(object obj)
    {
        return obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public)
            .Where(f => !UnbindableMembers.Contains(f.Name) && !f.IsInitOnly)
            .Where(f => f.FieldType.IsPrimitive || f.FieldType == typeof(Vector4) || f.FieldType == typeof(Quaternion)
                || f.FieldType == typeof(Vector3) || f.FieldType.IsEnum)
            .OrderBy(f => f.MetadataToken)
            .ToList();
    }

    /// <summary>
    /// The member path a binding uses for a field: its name without m_
    /// </summary>
    public static string MemberPathOf(FieldInfo field) => field.Name.StartsWith("m_") ? field.Name[2..] : field.Name;

    /// <summary>
    /// The variable types that suit a field (a binding to another type may not do what's wanted)
    /// </summary>
    public static bool IsSuitableVariableType(Type fieldType, string variableType)
    {
        if (fieldType == typeof(bool))
            return variableType is "VARIABLE_TYPE_BOOL" or "VARIABLE_TYPE_INT8" or "VARIABLE_TYPE_INT16" or "VARIABLE_TYPE_INT32";

        if (fieldType == typeof(float) || fieldType == typeof(double))
            return variableType is "VARIABLE_TYPE_REAL";

        if (fieldType == typeof(Vector4) || fieldType == typeof(Vector3))
            return variableType is "VARIABLE_TYPE_VECTOR3" or "VARIABLE_TYPE_VECTOR4";

        if (fieldType == typeof(Quaternion))
            return variableType is "VARIABLE_TYPE_QUATERNION";

        return variableType is "VARIABLE_TYPE_BOOL" or "VARIABLE_TYPE_INT8" or "VARIABLE_TYPE_INT16" or "VARIABLE_TYPE_INT32";
    }

    public static string GetVariableTypeName(HavokBehaviorSymbols symbols, int index)
        => HavokBehaviorSymbols.GetValue(symbols.GetVariableInfo(index), "m_type")?.ToString() ?? "";

    /// <summary>
    /// The object's bindings (empty if it has none)
    /// </summary>
    public static List<object> GetOwnBindings(object obj)
    {
        var set = HavokBehaviorSymbols.GetValue(obj, "m_variableBindingSet");

        return HavokBehaviorSymbols.GetBindings(set)?.Cast<object>().Where(b => b != null).ToList() ?? new();
    }

    /// <summary>
    /// Binds a variable to one of the object's members. A member that's already bound has its binding changed.
    /// An object without a binding set gets one. New bindings have bit index -1, as every binding in ER, NR and DS3 has.
    /// </summary>
    public static EditorAction BindVariable(HavokBehaviorContext context, object obj, FieldInfo member, int variableIndex, out string error)
    {
        error = null;

        if (!IsBindable(obj))
        {
            error = LOC.Get("HAVOK_BehaviorActions_Error_Not_Bindable", HavokBehaviorSymbols.GetLabel(obj));
            return null;
        }

        if (HavokBehaviorSymbols.GetName(context.Symbols.VariableNames, variableIndex) == null)
        {
            error = LOC.Get("HAVOK_BehaviorActions_Error_No_Variable");
            return null;
        }

        var path = MemberPathOf(member);
        var steps = new List<EditorAction>();
        var setField = HavokBehaviorSymbols.GetField(obj.GetType(), "m_variableBindingSet");
        var set = setField.GetValue(obj);

        var existing = GetOwnBindings(obj).FirstOrDefault(b =>
            string.Equals(HavokBehaviorSymbols.GetMemberPath(b), path, StringComparison.OrdinalIgnoreCase));

        if (existing != null)
        {
            var typeField = HavokBehaviorSymbols.GetField(existing.GetType(), "m_bindingType");

            steps.Add(SetValue(context, existing, HavokBehaviorSymbols.GetField(existing.GetType(), "m_variableIndex"), variableIndex));
            steps.Add(SetValue(context, existing, typeField, ToFieldValue(typeField.FieldType, 0)));

            return Wrap(context, steps);
        }

        var newSet = set == null;

        if (newSet)
        {
            set = NewObject(setField.FieldType);

            if (set == null)
            {
                error = LOC.Get("HAVOK_BehaviorActions_Error_Cannot_Create", "hkbVariableBindingSet");
                return null;
            }

            HavokBehaviorSymbols.GetField(set.GetType(), "m_indexOfBindingToEnable").SetValue(set, -1);
        }

        var bindings = GetList(set, "m_bindings");
        var binding = NewEntry(set, "m_bindings");

        if (bindings == null || binding == null)
        {
            error = LOC.Get("HAVOK_BehaviorActions_Error_Cannot_Create", "hkbVariableBindingSet.Binding");
            return null;
        }

        var bindingTypeField = HavokBehaviorSymbols.GetField(binding.GetType(), "m_bindingType");

        HavokBehaviorSymbols.GetField(binding.GetType(), "m_memberPath").SetValue(binding, path);
        HavokBehaviorSymbols.GetField(binding.GetType(), "m_variableIndex").SetValue(binding, variableIndex);
        HavokBehaviorSymbols.GetField(binding.GetType(), "m_bitIndex").SetValue(binding, (sbyte)-1);
        bindingTypeField.SetValue(binding, ToFieldValue(bindingTypeField.FieldType, 0));

        if (newSet)
        {
            // Filled before it's linked, so the file never holds an empty set
            bindings.Add(binding);
            steps.Add(SetValue(context, obj, setField, set));
        }
        else
        {
            steps.Add(AddToList(context, bindings, binding));
        }

        return Wrap(context, steps);
    }

    /// <summary>
    /// Removes bindings from the object. With none left, the object's binding set is removed (no vanilla set is empty).
    /// The binding to enable follows the removal.
    /// </summary>
    public static EditorAction RemoveBindings(HavokBehaviorContext context, object obj, IEnumerable<object> remove, out string error)
    {
        error = null;

        var set = HavokBehaviorSymbols.GetValue(obj, "m_variableBindingSet");
        var steps = RemoveBindingSteps(context, obj, set, new HashSet<object>(remove, ReferenceEqualityComparer.Instance));

        if (steps.Count == 0)
        {
            error = LOC.Get("HAVOK_BehaviorActions_Error_Not_Referenced");
            return null;
        }

        return Wrap(context, steps);
    }

    private static List<EditorAction> RemoveBindingSteps(HavokBehaviorContext context, object owner, object set, HashSet<object> remove)
    {
        var steps = new List<EditorAction>();

        if (HavokBehaviorSymbols.GetBindings(set) is not IList bindings)
            return steps;

        var oldBindings = bindings.Cast<object>().ToList();
        var newBindings = oldBindings.Where(b => !remove.Contains(b)).ToList();

        if (newBindings.Count == oldBindings.Count)
            return steps;

        if (newBindings.Count == 0)
        {
            // The set goes from every object that holds it (binding sets aren't shared in the game's files)
            var owners = owner != null ? new List<object> { owner } : context.Symbols.GetParents(set).ToList();

            foreach (var o in owners)
                steps.Add(SetValue(context, o, HavokBehaviorSymbols.GetField(o.GetType(), "m_variableBindingSet"), null));

            return steps;
        }

        steps.Add(Step(context,
            () => { bindings.Clear(); foreach (var b in newBindings) bindings.Add(b); },
            () => { bindings.Clear(); foreach (var b in oldBindings) bindings.Add(b); }));

        var enableField = HavokBehaviorSymbols.GetField(set.GetType(), "m_indexOfBindingToEnable");
        var enable = ToInt(enableField?.GetValue(set));

        if (enableField != null && enable >= 0 && enable < oldBindings.Count)
        {
            var newEnable = remove.Contains(oldBindings[enable]) ? -1 : newBindings.IndexOf(oldBindings[enable]);

            if (newEnable != enable)
                steps.Add(SetValue(context, set, enableField, newEnable));
        }

        return steps;
    }
    #endregion

    #region Duplicate variable
    private static bool IsQuadVariableType(string typeName) => typeName is "VARIABLE_TYPE_VECTOR3" or "VARIABLE_TYPE_VECTOR4" or "VARIABLE_TYPE_QUATERNION";

    /// <summary>
    /// Copies a variable to the end of the list with a new name: its info, bounds and initial value.
    /// A vector, quaternion or pointer variable gets its own copy of its quad or variant value. Bindings aren't copied.
    /// </summary>
    public static EditorAction DuplicateVariable(HavokBehaviorContext context, int index, string name, out int newIndex, out string error)
    {
        newIndex = -1;
        error = null;

        var symbols = context.Symbols;
        var graphData = symbols.GraphData;
        var names = symbols.VariableNames;
        var infos = GetList(graphData, "m_variableInfos");
        var bounds = GetList(graphData, "m_variableBounds");
        var valueSet = HavokBehaviorSymbols.GetValue(graphData, "m_variableInitialValues");
        var words = GetList(valueSet, "m_wordVariableValues");

        if (string.IsNullOrWhiteSpace(name))
        {
            error = LOC.Get("HAVOK_BehaviorActions_Error_No_Name");
            return null;
        }

        if (names == null || infos == null || words == null || index < 0 || index >= names.Count || index >= infos.Count || index >= words.Count)
        {
            error = LOC.Get("HAVOK_BehaviorActions_Error_No_Graph_Data");
            return null;
        }

        if (names.Cast<object>().Any(n => string.Equals(n as string, name, StringComparison.Ordinal)))
        {
            error = LOC.Get("HAVOK_BehaviorActions_Error_Variable_Exists", name);
            return null;
        }

        var typeName = GetVariableTypeName(symbols, index);
        var info = HavokBehaviorCloner.Clone(infos[index], symbols, keepShared: false);
        var word = HavokBehaviorCloner.Clone(words[index], symbols, keepShared: false);
        var wordField = HavokBehaviorSymbols.GetField(word.GetType(), "m_value");

        var steps = new List<EditorAction>();

        if (IsQuadVariableType(typeName) || typeName == "VARIABLE_TYPE_POINTER")
        {
            var listName = typeName == "VARIABLE_TYPE_POINTER" ? "m_variantVariableValues" : "m_quadVariableValues";
            var values = GetList(valueSet, listName);
            var valueIndex = ToInt(wordField.GetValue(word));

            if (values == null || valueIndex < 0 || valueIndex >= values.Count)
            {
                error = LOC.Get("HAVOK_BehaviorActions_Error_No_Graph_Data");
                return null;
            }

            // A quad is a value; a variant is a pointer to an object, which the copy shares
            wordField.SetValue(word, values.Count);
            steps.Add(AddToList(context, values, values[valueIndex]));
        }

        newIndex = names.Count;

        steps.Add(AddToList(context, names, name));
        steps.Add(AddToList(context, infos, info));
        steps.Add(AddToList(context, words, word));

        if (bounds != null && bounds.Count == names.Count && index < bounds.Count)
            steps.Add(AddToList(context, bounds, HavokBehaviorCloner.Clone(bounds[index], symbols, keepShared: false)));

        return Wrap(context, steps);
    }
    #endregion

    #region Delete variable
    /// <summary>
    /// What deleting a variable changes: the bindings to it, and the other fields that name it
    /// </summary>
    public record HavokVariableDeletion(List<(object Bindable, object Binding)> Bindings, List<(object Obj, FieldInfo Field)> Fields);

    /// <summary>
    /// Every VariableIndex-tagged field (other than a binding's index) with its value
    /// </summary>
    private static List<(object Obj, FieldInfo Field, int Value)> FindVariableIndexFields(HavokBehaviorContext context)
    {
        var results = new List<(object, FieldInfo, int)>();

        foreach (var obj in context.Symbols.Objects)
        {
            var type = obj.GetType();

            if (type.Name.Contains("Binding"))
                continue;

            var meta = HavokMetaHelper.GetMeta(context.Project, type);

            if (meta == null)
                continue;

            foreach (var fieldMeta in meta.Fields)
            {
                if (HavokMetaHelper.GetIndexRole(meta, fieldMeta.Field) is not HavokIndexRole.Variable)
                    continue;

                var field = HavokBehaviorSymbols.GetField(type, fieldMeta.Field);

                if (field == null)
                    continue;

                results.Add((obj, field, ToInt(field.GetValue(obj))));
            }
        }

        return results;
    }

    public static HavokVariableDeletion DescribeDeleteVariable(HavokBehaviorContext context, int index)
    {
        var bindings = context.Symbols.FindBindingsTo(index, characterProperty: false);
        var fields = FindVariableIndexFields(context).Where(f => f.Value == index).Select(f => (f.Obj, f.Field)).ToList();

        return new(bindings, fields);
    }

    /// <summary>
    /// Deletes a variable: its name, info, bounds, word value and quad or variant value, and its bindings.
    /// Every variable index above it moves down one, and the quad and variant indices of the other variables follow
    /// their values. A field that named it becomes -1.
    /// </summary>
    public static EditorAction DeleteVariable(HavokBehaviorContext context, int index, out string error)
    {
        error = null;

        var symbols = context.Symbols;
        var graphData = symbols.GraphData;
        var names = symbols.VariableNames;
        var infos = GetList(graphData, "m_variableInfos");
        var bounds = GetList(graphData, "m_variableBounds");
        var valueSet = HavokBehaviorSymbols.GetValue(graphData, "m_variableInitialValues");
        var words = GetList(valueSet, "m_wordVariableValues");

        if (names == null || infos == null || words == null || index < 0 || index >= names.Count || index >= infos.Count || index >= words.Count)
        {
            error = LOC.Get("HAVOK_BehaviorActions_Error_No_Graph_Data");
            return null;
        }

        var steps = new List<EditorAction>();

        // Bindings: remove the ones to it, move the ones above it down
        foreach (var set in symbols.BindingSets)
        {
            if (HavokBehaviorSymbols.GetBindings(set) is not IList bindings)
                continue;

            var remove = new HashSet<object>(ReferenceEqualityComparer.Instance);

            foreach (var binding in bindings)
            {
                if (binding == null || HavokBehaviorSymbols.IsCharacterPropertyBinding(binding))
                    continue;

                var bound = HavokBehaviorSymbols.GetBindingIndex(binding);

                if (bound == index)
                    remove.Add(binding);
                else if (bound > index)
                    steps.Add(SetValue(context, binding, HavokBehaviorSymbols.GetField(binding.GetType(), "m_variableIndex"), bound - 1));
            }

            if (remove.Count > 0)
                steps.AddRange(RemoveBindingSteps(context, null, set, remove));
        }

        // Other index fields (hkbStateMachine.m_syncVariableIndex)
        foreach (var (obj, field, value) in FindVariableIndexFields(context))
        {
            if (value == index)
                steps.Add(SetValue(context, obj, field, Convert.ChangeType(-1, field.FieldType)));
            else if (value > index)
                steps.Add(SetValue(context, obj, field, Convert.ChangeType(value - 1, field.FieldType)));
        }

        // Its quad or variant value, and the other variables' indices into that list
        var typeName = GetVariableTypeName(symbols, index);

        if (IsQuadVariableType(typeName) || typeName == "VARIABLE_TYPE_POINTER")
        {
            var isPointer = typeName == "VARIABLE_TYPE_POINTER";
            var values = GetList(valueSet, isPointer ? "m_variantVariableValues" : "m_quadVariableValues");
            var valueIndex = ToInt(HavokBehaviorSymbols.GetValue(words[index], "m_value"));

            if (values != null && valueIndex >= 0 && valueIndex < values.Count)
            {
                for (var i = 0; i < words.Count; i++)
                {
                    if (i == index)
                        continue;

                    var otherType = GetVariableTypeName(symbols, i);

                    if (isPointer ? otherType != "VARIABLE_TYPE_POINTER" : !IsQuadVariableType(otherType))
                        continue;

                    var otherValue = ToInt(HavokBehaviorSymbols.GetValue(words[i], "m_value"));

                    if (otherValue > valueIndex)
                        steps.Add(SetValue(context, words[i], HavokBehaviorSymbols.GetField(words[i].GetType(), "m_value"), otherValue - 1));
                }

                var removedValue = values[valueIndex];
                var at = valueIndex;
                steps.Add(Step(context, () => values.RemoveAt(at), () => values.Insert(at, removedValue)));
            }
        }

        // The parallel lists
        void RemoveIndex(IList list)
        {
            var item = list[index];
            steps.Add(Step(context, () => list.RemoveAt(index), () => list.Insert(index, item)));
        }

        RemoveIndex(names);
        RemoveIndex(infos);
        RemoveIndex(words);

        if (bounds != null && bounds.Count > index)
            RemoveIndex(bounds);

        return Wrap(context, steps);
    }
    #endregion
}
