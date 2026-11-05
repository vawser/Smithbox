using StudioCore.Editors.Common;
using System.Collections;
using System.Numerics;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.HavokEditor;

/// <summary>
/// Behavior edits made of several changes, each returned as one undoable action
/// (or null, with the reason it can't be done).
///
/// New objects copy what the game's own objects have (Tools/BehaviorSurvey, ER/NR/DS3; see the plan doc):
/// a new clip copies a clip of the same selector, a new transition copies one from the same list,
/// and the lists kept alongside the name lists grow with them.
/// </summary>
public static class HavokBehaviorOperations
{
    #region Steps
    private static EditorAction Step(HavokBehaviorContext context, Action apply, Action revert)
        => new HavokDelegateAction(context.File, apply, revert);

    private static EditorAction AddToList(HavokBehaviorContext context, IList list, object value)
        => Step(context, () => list.Add(value), () => list.RemoveAt(list.Count - 1));

    private static EditorAction SetField(HavokBehaviorContext context, object obj, string fieldName, object value)
    {
        var field = HavokBehaviorSymbols.GetField(obj.GetType(), fieldName);
        var old = field.GetValue(obj);

        return Step(context, () => field.SetValue(obj, value), () => field.SetValue(obj, old));
    }

    private static IList GetList(object obj, string fieldName) => HavokBehaviorSymbols.GetValue(obj, fieldName) as IList;

    /// <summary>
    /// A new entry of a list's element type, or null if it can't be made
    /// </summary>
    private static object NewEntry(object owner, string listName)
    {
        var field = HavokBehaviorSymbols.GetField(owner.GetType(), listName);

        if (field == null || !field.FieldType.IsGenericType)
            return null;

        return NewObject(field.FieldType.GetGenericArguments()[0]);
    }

    private static object NewFieldValue(object owner, string fieldName)
    {
        var field = HavokBehaviorSymbols.GetField(owner.GetType(), fieldName);

        return field == null ? null : NewObject(field.FieldType);
    }

    /// <summary>
    /// A new object with its embedded members and lists created. The HKX2 classes only create those when a
    /// file is read (hkbVariableInfo.m_role, a TransitionInfoArray's list), and a null one can't be saved.
    /// Pointers (hkReferencedObject types) are left null, as optional.
    /// </summary>
    private static object NewObject(Type type)
    {
        var obj = PropFinderUtil.CreateDefaultListElement(type);

        if (obj != null && !type.IsValueType && type != typeof(string))
            FillEmbedded(obj, 0);

        return obj;
    }

    private static void FillEmbedded(object obj, int depth)
    {
        if (depth > 8)
            return;

        foreach (var field in obj.GetType().GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public))
        {
            var type = field.FieldType;

            if (type.IsValueType || type == typeof(string) || field.GetValue(obj) != null)
                continue;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                field.SetValue(obj, Activator.CreateInstance(type));
                continue;
            }

            if (type.IsAbstract || type.IsInterface || IsReferencedObject(type) || type.GetConstructor(Type.EmptyTypes) == null)
                continue;

            var member = Activator.CreateInstance(type)!;
            field.SetValue(obj, member);
            FillEmbedded(member, depth + 1);
        }
    }

    private static bool IsReferencedObject(Type type)
    {
        for (var t = type; t != null; t = t.BaseType)
        {
            if (t.Name == "hkReferencedObject")
                return true;
        }

        return false;
    }

    /// <summary>
    /// A number as a field's type: an enum (hk2018) or a plain integer (HKX2)
    /// </summary>
    private static object ToFieldValue(Type fieldType, long value)
        => fieldType.IsEnum ? Enum.ToObject(fieldType, value) : Convert.ChangeType(value, fieldType);

    private static HavokCompoundAction Wrap(HavokBehaviorContext context, List<EditorAction> steps)
        => new(context.File, steps);
    #endregion

    #region Events
    /// <summary>
    /// The steps that add an event: its name, and its hkbEventInfo (flags 0, as every event in ER, NR and DS3 has)
    /// </summary>
    public static List<EditorAction> AddEventSteps(HavokBehaviorContext context, string name, out int index, out string error)
    {
        index = -1;
        error = null;

        var symbols = context.Symbols;
        var names = symbols.EventNames;
        var infos = GetList(symbols.GraphData, "m_eventInfos");

        if (string.IsNullOrWhiteSpace(name))
        {
            error = LOC.Get("HAVOK_BehaviorActions_Error_No_Name");
            return null;
        }

        if (names == null || infos == null)
        {
            error = LOC.Get("HAVOK_BehaviorActions_Error_No_Graph_Data");
            return null;
        }

        if (names.Cast<object>().Any(n => string.Equals(n as string, name, StringComparison.Ordinal)))
        {
            error = LOC.Get("HAVOK_BehaviorActions_Error_Event_Exists", name);
            return null;
        }

        var info = NewEntry(symbols.GraphData, "m_eventInfos");

        if (info == null)
        {
            error = LOC.Get("HAVOK_BehaviorActions_Error_Cannot_Create", "hkbEventInfo");
            return null;
        }

        index = names.Count;

        return new List<EditorAction>
        {
            AddToList(context, names, name),
            AddToList(context, infos, info)
        };
    }
    #endregion

    #region Variables
    public static readonly List<string> VariableTypeNames = new()
    {
        "VARIABLE_TYPE_BOOL", "VARIABLE_TYPE_INT8", "VARIABLE_TYPE_INT16", "VARIABLE_TYPE_INT32", "VARIABLE_TYPE_REAL",
        "VARIABLE_TYPE_POINTER", "VARIABLE_TYPE_VECTOR3", "VARIABLE_TYPE_VECTOR4", "VARIABLE_TYPE_QUATERNION"
    };

    /// <summary>
    /// Whether a variable of the type keeps its value in the initial word values
    /// (the others keep an index into the quad or variant values there)
    /// </summary>
    public static bool IsWordVariableType(string typeName)
        => typeName is "VARIABLE_TYPE_BOOL" or "VARIABLE_TYPE_INT8" or "VARIABLE_TYPE_INT16" or "VARIABLE_TYPE_INT32" or "VARIABLE_TYPE_REAL";

    /// <summary>
    /// Adds a variable: its name, hkbVariableInfo, bounds (if the file has bounds) and initial value.
    /// A vector or quaternion variable's word value is the index of its value in the quad values, and a pointer
    /// variable's the index of its value in the variant values, so those lists grow too.
    /// </summary>
    public static EditorAction AddVariable(HavokBehaviorContext context, string name, string typeName, double initialValue,
        out int index, out string error)
    {
        index = -1;
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

        if (names == null || infos == null || words == null)
        {
            error = LOC.Get("HAVOK_BehaviorActions_Error_No_Graph_Data");
            return null;
        }

        if (names.Cast<object>().Any(n => string.Equals(n as string, name, StringComparison.Ordinal)))
        {
            error = LOC.Get("HAVOK_BehaviorActions_Error_Variable_Exists", name);
            return null;
        }

        var info = NewEntry(graphData, "m_variableInfos");
        var word = NewEntry(valueSet, "m_wordVariableValues");

        if (info == null || word == null)
        {
            error = LOC.Get("HAVOK_BehaviorActions_Error_Cannot_Create", "hkbVariableInfo");
            return null;
        }

        var typeField = HavokBehaviorSymbols.GetField(info.GetType(), "m_type");
        typeField.SetValue(info, Enum.Parse(typeField.FieldType, typeName));

        var steps = new List<EditorAction>();

        // The word value: the value itself, or an index into the quad or variant values
        var wordValue = (int)initialValue;

        if (typeName is "VARIABLE_TYPE_VECTOR3" or "VARIABLE_TYPE_VECTOR4" or "VARIABLE_TYPE_QUATERNION")
        {
            var quads = GetList(valueSet, "m_quadVariableValues");

            if (quads == null)
            {
                error = LOC.Get("HAVOK_BehaviorActions_Error_No_Graph_Data");
                return null;
            }

            wordValue = quads.Count;
            steps.Add(AddToList(context, quads, typeName == "VARIABLE_TYPE_QUATERNION" ? new Vector4(0, 0, 0, 1) : Vector4.Zero));
        }
        else if (typeName == "VARIABLE_TYPE_POINTER")
        {
            var variants = GetList(valueSet, "m_variantVariableValues");

            if (variants == null)
            {
                error = LOC.Get("HAVOK_BehaviorActions_Error_No_Graph_Data");
                return null;
            }

            wordValue = variants.Count;
            steps.Add(AddToList(context, variants, null));
        }
        else if (typeName == "VARIABLE_TYPE_REAL")
        {
            // A real is stored as the bits of the float (checked against ER: 1.0 is 1065353216)
            wordValue = BitConverter.SingleToInt32Bits((float)initialValue);
        }

        HavokBehaviorSymbols.GetField(word.GetType(), "m_value").SetValue(word, wordValue);

        index = names.Count;

        steps.Add(AddToList(context, names, name));
        steps.Add(AddToList(context, infos, info));
        steps.Add(AddToList(context, words, word));

        // Bounds are either one per variable or none at all (113 ER files have none)
        if (bounds != null && bounds.Count == names.Count)
        {
            var bound = NewEntry(graphData, "m_variableBounds");

            if (bound != null)
                steps.Add(AddToList(context, bounds, bound));
        }

        return Wrap(context, steps);
    }
    #endregion

    #region Animations
    private static readonly Regex AnimationNameRegex = new(@"a\d+_\d+", RegexOptions.IgnoreCase);
    private static readonly Regex CategoryFolderRegex = new(@"([\\/])a\d+([\\/])", RegexOptions.IgnoreCase);

    /// <summary>
    /// Adds a clip for a category to a CustomManualSelectorGenerator: aCCC_{its m_animId}.
    /// The clip copies an existing clip of the selector (or of the file), keeping what those share.
    /// Its internal ID is the one other clips of the same animation use, or a new animation name entry
    /// (hk2018: m_animationNames holds the animation's path, one entry per animation).
    /// </summary>
    public static EditorAction AddAnimation(HavokBehaviorContext context, object selector, long category,
        out object newClip, out string error)
    {
        newClip = null;
        error = null;

        var symbols = context.Symbols;
        var generators = GetList(selector, "m_generators");
        var animationID = Convert.ToInt64(HavokBehaviorSymbols.GetValue(selector, "m_animId") ?? 0);
        var categorySize = context.Project.Descriptor.ProjectType is ProjectType.DES or ProjectType.DS1 or ProjectType.DS1R ? 10000 : 1000000;
        var animationName = categorySize == 10000 ? $"a{category:D2}_{animationID:D4}" : $"a{category:D3}_{animationID:D6}";

        if (generators == null)
        {
            error = LOC.Get("HAVOK_BehaviorActions_Error_No_Generators");
            return null;
        }

        var clips = generators.Cast<object>().Where(g => g?.GetType().Name == "hkbClipGenerator").ToList();

        if (clips.Any(c => string.Equals(HavokBehaviorSymbols.GetValue(c, "m_animationName") as string, animationName, StringComparison.OrdinalIgnoreCase)))
        {
            error = LOC.Get("HAVOK_BehaviorActions_Error_Clip_Exists", animationName);
            return null;
        }

        // Copy a clip of this selector, or any clip of the file
        var template = clips.FirstOrDefault() ?? symbols.Clips.FirstOrDefault();

        if (template == null)
        {
            error = LOC.Get("HAVOK_BehaviorActions_Error_No_Template_Clip");
            return null;
        }

        var clip = HavokBehaviorCloner.Clone(template, symbols, keepShared: true);
        var templateName = HavokBehaviorSymbols.GetValue(template, "m_name") as string ?? "";
        var templateAnimation = HavokBehaviorSymbols.GetValue(template, "m_animationName") as string ?? "";

        // Named as the template is: its animation name in its name is replaced by the new one
        var clipName = !string.IsNullOrEmpty(templateAnimation) && templateName.Contains(templateAnimation, StringComparison.OrdinalIgnoreCase)
            ? Regex.Replace(templateName, Regex.Escape(templateAnimation), animationName, RegexOptions.IgnoreCase)
            : animationName;

        HavokBehaviorSymbols.GetField(clip.GetType(), "m_name").SetValue(clip, clipName);
        HavokBehaviorSymbols.GetField(clip.GetType(), "m_animationName").SetValue(clip, animationName);

        var steps = new List<EditorAction>();

        // Internal ID (hk2018): the animation's index in m_animationNames, which has one entry per distinct clip
        // animation in every ER and NR file. HKX2 (DS3) clips have no internal ID; instead the character file
        // (Characters/cXXXX.hkx, m_animationBundleNameData) lists each animation the behavior uses, by name.
        var internalIdField = HavokBehaviorSymbols.GetField(clip.GetType(), "m_animationInternalId");
        var sameAnimation = symbols.Clips.FirstOrDefault(c =>
            string.Equals(HavokBehaviorSymbols.GetValue(c, "m_animationName") as string, animationName, StringComparison.OrdinalIgnoreCase));
        var animationNames = symbols.AnimationNames;

        if (internalIdField == null)
        {
            if (sameAnimation == null)
            {
                error = LOC.Get("HAVOK_BehaviorActions_Error_Character_Bundle", animationName);
                return null;
            }
        }
        else if (sameAnimation != null)
        {
            internalIdField.SetValue(clip, HavokBehaviorSymbols.GetValue(sameAnimation, "m_animationInternalId"));
        }
        else if (animationNames != null && animationNames.Count > 0)
        {
            var path = MakeAnimationPath(animationNames, animationName, category, categorySize);

            internalIdField.SetValue(clip, Convert.ChangeType(animationNames.Count, internalIdField.FieldType));
            steps.Add(AddToList(context, animationNames, path));
        }
        else
        {
            error = LOC.Get("HAVOK_BehaviorActions_Error_No_Animation_Names");
            return null;
        }

        steps.Add(AddToList(context, generators, clip));

        newClip = clip;

        return Wrap(context, steps);
    }

    /// <summary>
    /// The path of a new animation name entry, made from an existing one:
    /// ..\..\..\..\Model\chr\c0000\hkx\a000\a000_003000.hkx with the animation and its category folder replaced
    /// </summary>
    private static string MakeAnimationPath(IList animationNames, string animationName, long category, int categorySize)
    {
        var template = animationNames[animationNames.Count - 1] as string ?? "";
        var path = AnimationNameRegex.Replace(template, animationName);
        var folder = categorySize == 10000 ? $"a{category:D2}" : $"a{category:D3}";

        return CategoryFolderRegex.Replace(path, m => $"{m.Groups[1].Value}{folder}{m.Groups[2].Value}");
    }
    #endregion

    #region States
    /// <summary>
    /// The state ID after the highest one of the state machine
    /// </summary>
    public static int GetFreeStateID(object stateMachine)
    {
        var states = HavokBehaviorSymbols.GetStates(stateMachine);

        return states.Count == 0 ? 0 : states.Max(s => s.StateId) + 1;
    }

    /// <summary>
    /// Copies a state into the same state machine, with a new name and the next free state ID.
    /// Generators also used outside the state stay shared unless copyShared; its outgoing transitions are copied
    /// (their effects stay shared), transitions into it aren't.
    /// </summary>
    public static EditorAction CloneState(HavokBehaviorContext context, object stateMachine, object state, string name,
        bool copyShared, out object newState, out string error)
    {
        newState = null;
        error = null;

        var states = GetList(stateMachine, "m_states");

        if (states == null)
        {
            error = LOC.Get("HAVOK_BehaviorActions_Error_No_State_Machine");
            return null;
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            error = LOC.Get("HAVOK_BehaviorActions_Error_No_Name");
            return null;
        }

        if (HavokBehaviorSymbols.GetStates(stateMachine).Any(s => s.Name == name))
        {
            error = LOC.Get("HAVOK_BehaviorActions_Error_State_Exists", name);
            return null;
        }

        var copy = HavokBehaviorCloner.Clone(state, context.Symbols, keepShared: !copyShared);

        HavokBehaviorSymbols.GetField(copy.GetType(), "m_name").SetValue(copy, name);
        HavokBehaviorSymbols.GetField(copy.GetType(), "m_stateId").SetValue(copy, GetFreeStateID(stateMachine));

        newState = copy;

        // Added last with the highest ID (the game's files don't always keep IDs in order or without gaps)
        return Wrap(context, new List<EditorAction> { AddToList(context, states, copy) });
    }
    #endregion

    #region Transitions
    /// <summary>
    /// Adds a transition on an event to a state, from a state or from any state of the machine (a wildcard).
    /// With a new event name, the event is added first. The transition copies one from the same list
    /// (or from the machine's wildcard transitions), keeping its effect shared.
    /// </summary>
    public static EditorAction AddTransition(HavokBehaviorContext context, object stateMachine, object fromState, int eventIndex,
        string newEventName, int toStateId, out object show, out string error)
    {
        show = null;
        error = null;

        var steps = new List<EditorAction>();

        if (newEventName != null)
        {
            var eventSteps = AddEventSteps(context, newEventName, out eventIndex, out error);

            if (eventSteps == null)
                return null;

            steps.AddRange(eventSteps);
        }
        else if (HavokBehaviorSymbols.GetName(context.Symbols.EventNames, eventIndex) == null)
        {
            error = LOC.Get("HAVOK_BehaviorActions_Error_No_Event");
            return null;
        }

        var owner = fromState ?? stateMachine;
        var arrayField = fromState == null ? "m_wildcardTransitions" : "m_transitions";
        var array = HavokBehaviorSymbols.GetValue(owner, arrayField);

        if (array == null)
        {
            array = NewFieldValue(owner, arrayField);

            if (array == null)
            {
                error = LOC.Get("HAVOK_BehaviorActions_Error_Cannot_Create", "TransitionInfoArray");
                return null;
            }

            steps.Add(SetField(context, owner, arrayField, array));
        }

        var transitions = GetList(array, "m_transitions");

        // Copy a transition from the same list, else from the machine's wildcards
        var template = transitions?.Cast<object>().LastOrDefault(t => t != null)
            ?? GetList(HavokBehaviorSymbols.GetValue(stateMachine, "m_wildcardTransitions"), "m_transitions")?.Cast<object>().LastOrDefault(t => t != null);

        var transition = template != null
            ? HavokBehaviorCloner.Clone(template, context.Symbols, keepShared: true)
            : NewEntry(array, "m_transitions");

        if (transitions == null || transition == null)
        {
            error = LOC.Get("HAVOK_BehaviorActions_Error_Cannot_Create", "TransitionInfo");
            return null;
        }

        HavokBehaviorSymbols.GetField(transition.GetType(), "m_eventId").SetValue(transition, eventIndex);
        HavokBehaviorSymbols.GetField(transition.GetType(), "m_toStateId").SetValue(transition, toStateId);

        // Every transition from a state has flags 0 in ER, NR and DS3. A wildcard copies its flags from another
        // wildcard, or takes the usual ones (ALLOW_SELF_TRANSITION_BY_TRANSITION_FROM_ANY_STATE | IS_GLOBAL_WILDCARD | IS_LOCAL_WILDCARD)
        var flagsField = HavokBehaviorSymbols.GetField(transition.GetType(), "m_flags");

        if (fromState != null)
            flagsField.SetValue(transition, ToFieldValue(flagsField.FieldType, 0));
        else if (template == null)
            flagsField.SetValue(transition, ToFieldValue(flagsField.FieldType, 512 | 1024 | 2048));

        steps.Add(AddToList(context, transitions, transition));

        // The outline shows the state (or machine) the transition belongs to
        show = owner;

        return Wrap(context, steps);
    }
    #endregion

    #region Unlink
    /// <summary>
    /// Removes the parent's reference to the child: its entry in a list, or the field, set to null
    /// </summary>
    public static EditorAction Unlink(HavokBehaviorContext context, object parent, object child, out string error)
    {
        error = null;

        foreach (var field in parent.GetType().GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public))
        {
            var value = field.GetValue(parent);

            if (ReferenceEquals(value, child))
                return Wrap(context, new List<EditorAction> { SetField(context, parent, field.Name, null) });

            if (value is IList list && value is not string)
            {
                var index = -1;

                for (var i = 0; i < list.Count; i++)
                {
                    if (ReferenceEquals(list[i], child))
                    {
                        index = i;
                        break;
                    }
                }

                if (index >= 0)
                {
                    var i2 = index;
                    return Wrap(context, new List<EditorAction>
                    {
                        Step(context, () => list.RemoveAt(i2), () => list.Insert(i2, child))
                    });
                }
            }
        }

        error = LOC.Get("HAVOK_BehaviorActions_Error_Not_Referenced");
        return null;
    }
    #endregion
}
