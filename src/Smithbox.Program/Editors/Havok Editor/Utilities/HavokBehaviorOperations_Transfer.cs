using StudioCore.Editors.Common;
using System.Collections;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.HavokEditor;

/// <summary>
/// Clip duplicate, and paste (generators within their own file, variables into any behavior file)
/// </summary>
public static partial class HavokBehaviorOperations
{
    #region Sequence
    /// <summary>
    /// Several edits as one undo step, each built after the ones before it have been applied, so each sees their result
    /// (two pasted clips don't take the same new animation name index). Edits that can't be made are skipped and their
    /// reasons returned. Null if none could be made.
    /// </summary>
    public static EditorAction Sequence(HavokBehaviorContext context, IEnumerable<Func<(EditorAction Action, string Error)>> builders,
        out List<string> errors)
    {
        errors = new List<string>();

        var applied = new List<EditorAction>();

        foreach (var build in builders)
        {
            var (action, error) = build();

            if (action == null)
            {
                if (error != null)
                    errors.Add(error);

                continue;
            }

            action.Execute();
            applied.Add(action);
        }

        // Back to the start: the action manager applies them again as one step
        for (var i = applied.Count - 1; i >= 0; i--)
            applied[i].Undo();

        return applied.Count == 0 ? null : Wrap(context, applied);
    }
    #endregion

    #region Clips
    /// <summary>
    /// The category (aCCC) of a clip's animation, or -1
    /// </summary>
    public static long GetClipCategory(object clip)
    {
        var match = ClipAnimationRegex.Match(HavokBehaviorSymbols.GetValue(clip, "m_animationName") as string ?? "");

        return match.Success ? long.Parse(match.Groups[1].Value) : -1;
    }

    /// <summary>
    /// The category after the selector's highest one (its clips are listed in category order in the game's files)
    /// </summary>
    public static long GetFreeCategory(object selector)
    {
        var categories = (GetList(selector, "m_generators")?.Cast<object>() ?? Enumerable.Empty<object>())
            .Where(g => g?.GetType().Name == "hkbClipGenerator")
            .Select(GetClipCategory)
            .Where(c => c >= 0)
            .ToList();

        return categories.Count == 0 ? 0 : categories.Max() + 1;
    }

    /// <summary>
    /// The selectors that hold the clip
    /// </summary>
    public static List<object> GetClipSelectors(HavokBehaviorSymbols symbols, object clip)
    {
        return symbols.GetParents(clip).Where(p => p.GetType().Name == "CustomManualSelectorGenerator").ToList();
    }

    /// <summary>
    /// Copies a clip into a selector for a category: aCCC_{the selector's animation ID}.
    /// Within its own selector this is Duplicate; into another, Paste.
    /// </summary>
    public static EditorAction DuplicateClip(HavokBehaviorContext context, object clip, object selector, long category,
        out object newClip, out string error)
    {
        return AddAnimation(context, selector, category, out newClip, out error, template: clip);
    }
    #endregion

    #region Paste generators
    /// <summary>
    /// A name not used by another object of the same type in the file: the name, else name_Copy, name_Copy2 ...
    /// </summary>
    private static string UniqueObjectName(HavokBehaviorSymbols symbols, Type type, string name)
    {
        var used = new HashSet<string>(symbols.Objects.Where(o => o.GetType() == type)
            .Select(o => HavokBehaviorSymbols.GetValue(o, "m_name") as string ?? ""), StringComparer.Ordinal);

        return UniqueName(used, name);
    }

    private static string UniqueName(HashSet<string> used, string name)
    {
        if (!used.Contains(name))
            return name;

        var candidate = $"{name}_Copy";

        for (var i = 2; used.Contains(candidate); i++)
            candidate = $"{name}_Copy{i}";

        return candidate;
    }

    /// <summary>
    /// Pastes a copy of a selector or state machine where the target is held: at the end of the target's selector list,
    /// or in a new state copied from the target's state, with the copy in the target's place. Children shared with the
    /// rest of the file stay shared.
    /// </summary>
    public static EditorAction PasteGenerator(HavokBehaviorContext context, object source, HavokPlacement placement,
        out object newObj, out string error)
    {
        newObj = null;
        error = null;

        var symbols = context.Symbols;

        if (placement == null || placement.Kind is PlacementKind.Unsupported)
        {
            error = placement?.Description ?? LOC.Get("HAVOK_BehaviorActions_Error_Not_Referenced");
            return null;
        }

        var sourceName = HavokBehaviorSymbols.GetValue(source, "m_name") as string ?? "";
        var copy = HavokBehaviorCloner.Clone(source, symbols, keepShared: true);

        HavokBehaviorSymbols.GetField(copy.GetType(), "m_name")?.SetValue(copy, UniqueObjectName(symbols, copy.GetType(), sourceName));

        newObj = copy;

        if (placement.Kind is PlacementKind.List)
            return Wrap(context, new List<EditorAction> { AddToList(context, GetList(placement.Holder, "m_generators"), copy) });

        // A new state: the target's state copied, with the pasted object where the target was
        var state = placement.Holder;
        var target = placement.Chain[^1];
        var stateCopy = HavokBehaviorCloner.Clone(state, symbols, keepShared: true,
            new HashSet<object>(placement.Chain, ReferenceEqualityComparer.Instance), out var copies);

        var holderCopy = placement.Chain.Count == 1 ? stateCopy : copies.GetValueOrDefault(placement.Chain[^2]);
        var holdingField = holderCopy == null ? null : FindHoldingField(holderCopy, copies.GetValueOrDefault(target), out _);

        if (holdingField == null)
        {
            error = LOC.Get("HAVOK_BehaviorActions_Error_Cannot_Create", state.GetType().Name);
            return null;
        }

        holdingField.SetValue(holderCopy, copy);

        var stateName = UniqueName(HavokBehaviorSymbols.GetStates(placement.StateMachine).Select(s => s.Name).ToHashSet(StringComparer.Ordinal),
            string.IsNullOrEmpty(sourceName) ? HavokBehaviorSymbols.GetValue(state, "m_name") as string ?? "" : sourceName);

        HavokBehaviorSymbols.GetField(stateCopy.GetType(), "m_name").SetValue(stateCopy, stateName);
        HavokBehaviorSymbols.GetField(stateCopy.GetType(), "m_stateId").SetValue(stateCopy, GetFreeStateID(placement.StateMachine));

        return Wrap(context, new List<EditorAction> { AddToList(context, GetList(placement.StateMachine, "m_states"), stateCopy) });
    }
    #endregion

    #region Variables
    /// <summary>
    /// A variable as copied: its data, cloned, so it can be pasted into any behavior file (of the same Havok version)
    /// </summary>
    public record HavokVariableSnapshot(string Name, string TypeName, object Info, object Bound, object Word, object ExtraValue);

    public static HavokVariableSnapshot SnapshotVariable(HavokBehaviorSymbols symbols, int index)
    {
        var name = HavokBehaviorSymbols.GetName(symbols.VariableNames, index);
        var info = symbols.GetVariableInfo(index);
        var word = symbols.GetVariableInitialValue(index);

        if (name == null || info == null || word == null)
            return null;

        var typeName = GetVariableTypeName(symbols, index);
        var valueSet = HavokBehaviorSymbols.GetValue(symbols.GraphData, "m_variableInitialValues");
        object extra = null;

        if (IsQuadVariableType(typeName) || typeName == "VARIABLE_TYPE_POINTER")
        {
            var values = GetList(valueSet, typeName == "VARIABLE_TYPE_POINTER" ? "m_variantVariableValues" : "m_quadVariableValues");
            var valueIndex = ToInt(HavokBehaviorSymbols.GetValue(word, "m_value"));

            if (values != null && valueIndex >= 0 && valueIndex < values.Count)
                extra = values[valueIndex];
        }

        return new(name, typeName,
            HavokBehaviorCloner.Clone(info, symbols, keepShared: false),
            symbols.GetVariableBounds(index) is { } bound ? HavokBehaviorCloner.Clone(bound, symbols, keepShared: false) : null,
            HavokBehaviorCloner.Clone(word, symbols, keepShared: false),
            extra);
    }

    /// <summary>
    /// Adds a copied variable to the file, named as it was (with _Copy if the name is taken)
    /// </summary>
    public static EditorAction PasteVariable(HavokBehaviorContext context, HavokVariableSnapshot snapshot, out int newIndex, out string error)
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

        if (names == null || infos == null || words == null)
        {
            error = LOC.Get("HAVOK_BehaviorActions_Error_No_Graph_Data");
            return null;
        }

        // Another version's classes can't go into this file
        if (infos.GetType().GetGenericArguments().FirstOrDefault() is { } infoType && !infoType.IsInstanceOfType(snapshot.Info))
        {
            error = LOC.Get("HAVOK_BehaviorActions_Error_Paste_Version");
            return null;
        }

        var name = UniqueName(names.Cast<object>().Select(n => n as string ?? "").ToHashSet(StringComparer.Ordinal), snapshot.Name);

        // Fresh copies each paste, so pasting twice doesn't put one object in the file twice
        var info = HavokBehaviorCloner.Clone(snapshot.Info, symbols, keepShared: false);
        var word = HavokBehaviorCloner.Clone(snapshot.Word, symbols, keepShared: false);
        var steps = new List<EditorAction>();

        if (IsQuadVariableType(snapshot.TypeName) || snapshot.TypeName == "VARIABLE_TYPE_POINTER")
        {
            var values = GetList(valueSet, snapshot.TypeName == "VARIABLE_TYPE_POINTER" ? "m_variantVariableValues" : "m_quadVariableValues");

            if (values == null)
            {
                error = LOC.Get("HAVOK_BehaviorActions_Error_No_Graph_Data");
                return null;
            }

            HavokBehaviorSymbols.GetField(word.GetType(), "m_value").SetValue(word, values.Count);
            steps.Add(AddToList(context, values, snapshot.ExtraValue));
        }

        newIndex = names.Count;

        steps.Add(AddToList(context, names, name));
        steps.Add(AddToList(context, infos, info));
        steps.Add(AddToList(context, words, word));

        // Bounds are one per variable or none at all
        if (bounds != null && bounds.Count == names.Count)
        {
            var bound = snapshot.Bound != null
                ? HavokBehaviorCloner.Clone(snapshot.Bound, symbols, keepShared: false)
                : NewEntry(graphData, "m_variableBounds");

            if (bound != null)
                steps.Add(AddToList(context, bounds, bound));
        }

        return Wrap(context, steps);
    }
    #endregion
}
