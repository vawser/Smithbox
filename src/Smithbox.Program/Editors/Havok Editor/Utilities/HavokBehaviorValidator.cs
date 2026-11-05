using System.Collections;

namespace StudioCore.Editors.HavokEditor;

/// <summary>
/// A problem the validator found: the object it's on, and what's wrong
/// </summary>
public record HavokBehaviorProblem(object Target, string Message);

/// <summary>
/// Checks a behavior graph for the mistakes that make the game crash or silently do nothing.
///
/// Only checks that every ER, NR and DS3 file passes are included (Tools/BehaviorSurvey), so a vanilla file
/// reports nothing and anything reported comes from an edit. Missing animations and time act entries aren't
/// checked: shared graphs (c9997) name animations most characters don't have, so those would be noise.
/// The Animations tab shows them instead.
/// </summary>
public static class HavokBehaviorValidator
{
    /// <summary>
    /// Members the game binds that aren't serialised fields (seen on hkbStateMachine in ER, NR and DS3)
    /// </summary>
    private static readonly HashSet<string> RuntimeMembers = new(StringComparer.OrdinalIgnoreCase) { "isActive", "currentStateId" };

    public static List<HavokBehaviorProblem> Run(ProjectEntry project, HavokBehaviorSymbols symbols)
    {
        var problems = new List<HavokBehaviorProblem>();

        if (symbols == null)
            return problems;

        CheckIndexFields(project, symbols, problems);
        CheckParallelArrays(symbols, problems);
        CheckStateIDs(symbols, problems);
        CheckClipInternalIDs(symbols, problems);
        CheckBindingPaths(symbols, problems);

        return problems;
    }

    /// <summary>
    /// Every field tagged EventID, VariableIndex or StateID: its index must name something
    /// </summary>
    private static void CheckIndexFields(ProjectEntry project, HavokBehaviorSymbols symbols, List<HavokBehaviorProblem> problems)
    {
        foreach (var obj in symbols.Objects)
        {
            var type = obj.GetType();
            var meta = HavokMetaHelper.GetMeta(project, type);

            if (meta == null)
                continue;

            foreach (var fieldMeta in meta.Fields)
            {
                var role = HavokMetaHelper.GetIndexRole(meta, fieldMeta.Field);

                if (role is HavokIndexRole.None)
                    continue;

                var value = HavokBehaviorSymbols.GetValue(obj, fieldMeta.Field);

                if (value == null)
                    continue;

                var index = Convert.ToInt32(value);

                if (role is HavokIndexRole.Variable && HavokBehaviorSymbols.IsCharacterPropertyBinding(obj))
                    role = HavokIndexRole.CharacterProperty;

                bool found;

                if (role is HavokIndexRole.State)
                {
                    var stateMachine = symbols.FindOwningStateMachine(obj);
                    found = stateMachine != null && HavokBehaviorSymbols.GetStateName(stateMachine, index) != null;
                }
                else
                {
                    found = index == -1 || HavokBehaviorSymbols.GetName(symbols.GetNames(role), index) != null;
                }

                if (!found)
                {
                    problems.Add(new(obj, LOC.Get("HAVOK_Validator_Index_Missing",
                        fieldMeta.Field, LOC.Get($"HAVOK_BehaviorIndex_Role_{GetRoleKey(role)}"), index)));
                }
            }
        }
    }

    private static string GetRoleKey(HavokIndexRole role) => role switch
    {
        HavokIndexRole.CharacterProperty => "Character_Property",
        _ => role.ToString()
    };

    /// <summary>
    /// The lists that run alongside the name lists must be as long as them
    /// (variable bounds may be empty: 113 ER files have none)
    /// </summary>
    private static void CheckParallelArrays(HavokBehaviorSymbols symbols, List<HavokBehaviorProblem> problems)
    {
        var graphData = symbols.GraphData;

        if (graphData == null)
            return;

        void Compare(IList names, object listOwner, string listName, string namesName, bool mayBeEmpty)
        {
            if (names == null || HavokBehaviorSymbols.GetValue(listOwner, listName) is not IList list)
                return;

            if (list.Count == names.Count || (mayBeEmpty && list.Count == 0))
                return;

            problems.Add(new(listOwner, LOC.Get("HAVOK_Validator_Length_Mismatch", listName, list.Count, namesName, names.Count)));
        }

        Compare(symbols.EventNames, graphData, "m_eventInfos", "m_eventNames", false);
        Compare(symbols.VariableNames, graphData, "m_variableInfos", "m_variableNames", false);
        Compare(symbols.VariableNames, graphData, "m_variableBounds", "m_variableNames", true);
        Compare(symbols.CharacterPropertyNames, graphData, "m_characterPropertyInfos", "m_characterPropertyNames", false);

        var initialValues = HavokBehaviorSymbols.GetValue(graphData, "m_variableInitialValues");

        if (initialValues != null)
            Compare(symbols.VariableNames, initialValues, "m_wordVariableValues", "m_variableNames", false);
    }

    /// <summary>
    /// A state machine's state IDs must be unique, since transitions find their target by ID
    /// </summary>
    private static void CheckStateIDs(HavokBehaviorSymbols symbols, List<HavokBehaviorProblem> problems)
    {
        foreach (var obj in symbols.Objects)
        {
            if (obj.GetType().Name != "hkbStateMachine")
                continue;

            var duplicates = HavokBehaviorSymbols.GetStates(obj)
                .GroupBy(s => s.StateId)
                .Where(g => g.Count() > 1);

            foreach (var group in duplicates)
            {
                problems.Add(new(obj, LOC.Get("HAVOK_Validator_Duplicate_State_ID", group.Key,
                    string.Join(", ", group.Select(s => s.Name)))));
            }
        }
    }

    /// <summary>
    /// A clip's m_animationInternalId indexes the animation names, whose file name matches the clip's
    /// m_animationName in every ER and NR clip (hk2018 only; HKX2 has no animation names)
    /// </summary>
    private static void CheckClipInternalIDs(HavokBehaviorSymbols symbols, List<HavokBehaviorProblem> problems)
    {
        var animationNames = symbols.AnimationNames;

        if (animationNames == null || animationNames.Count == 0)
            return;

        foreach (var clip in symbols.Clips)
        {
            var internalId = Convert.ToInt32(HavokBehaviorSymbols.GetValue(clip, "m_animationInternalId") ?? -1);
            var animationName = HavokBehaviorSymbols.GetValue(clip, "m_animationName") as string ?? "";
            var listed = HavokBehaviorSymbols.GetName(animationNames, internalId);

            if (listed == null)
            {
                problems.Add(new(clip, LOC.Get("HAVOK_Validator_Internal_ID_Missing", internalId, animationNames.Count)));
                continue;
            }

            var listedFile = Path.GetFileNameWithoutExtension(listed.Replace('\\', '/'));

            if (!string.Equals(listedFile, animationName, StringComparison.OrdinalIgnoreCase))
            {
                problems.Add(new(clip, LOC.Get("HAVOK_Validator_Internal_ID_Mismatch", internalId, listedFile, animationName)));
            }
        }
    }

    /// <summary>
    /// A binding's member path must lead to a field of the object it binds
    /// </summary>
    private static void CheckBindingPaths(HavokBehaviorSymbols symbols, List<HavokBehaviorProblem> problems)
    {
        foreach (var bindingSet in symbols.BindingSets)
        {
            if (HavokBehaviorSymbols.GetBindings(bindingSet) is not IList bindings)
                continue;

            foreach (var bindable in symbols.GetParents(bindingSet))
            {
                foreach (var binding in bindings)
                {
                    if (binding == null)
                        continue;

                    var path = HavokBehaviorSymbols.GetMemberPath(binding);

                    if (RuntimeMembers.Contains(path))
                        continue;

                    if (!HavokBehaviorSymbols.TryResolveMemberPath(bindable, path, out var reason))
                    {
                        problems.Add(new(bindable, LOC.Get("HAVOK_Validator_Binding_Path", path, reason)));
                    }
                }
            }
        }
    }
}
